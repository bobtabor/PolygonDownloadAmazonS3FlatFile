using Amazon.S3.Model;
using Amazon.S3;
using System.Globalization;
using Amazon.Runtime;

namespace PolygonDownloadAmazonS3FlatFile
{
    public class DownloadManager
    {
        public static async Task DownloadRecentFiles(string localDownloadPath)
        {
            // Load environment variables
            DotNetEnv.Env.Load();

            // Get credentials from environment variables
            var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")!;
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")!;
            var serviceUrl = Environment.GetEnvironmentVariable("S3_SERVICE_URL")!;
            var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")!;
            var prefix = Environment.GetEnvironmentVariable("S3_PREFIX")!;

            // Create an AmazonS3Config object
            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            // Create an AmazonS3Client
            var s3Client = new AmazonS3Client(
                new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
                config
            );

            // Get the most recent local file date
            DateTime mostRecentLocalDate = GetMostRecentLocalFileDate(localDownloadPath);

            Console.WriteLine($"Most recent local file date: {mostRecentLocalDate:yyyy-MM-dd}");

            // Download newer files
            await DownloadNewerFilesAsync(s3Client, bucketName, prefix, mostRecentLocalDate, localDownloadPath);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static DateTime GetMostRecentLocalFileDate(string localDownloadPath)
        {
            return Directory.GetFiles(localDownloadPath, "*.csv.gz")
                .Select(f => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f)))
                .Where(f => DateTime.TryParseExact(f, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                .Select(f => DateTime.ParseExact(f, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                .OrderByDescending(d => d)
                .FirstOrDefault(default(DateTime));
        }

        private static async Task DownloadNewerFilesAsync(IAmazonS3 s3Client, string bucketName, string prefix,
            DateTime lastDownloadedDate, string localDownloadPath)
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = prefix
                };

                var fileCount = 0;
                do
                {
                    Console.WriteLine($"Requesting objects with prefix: {prefix}");
                    var response = await s3Client.ListObjectsV2Async(request);
                    Console.WriteLine($"Received {response.S3Objects.Count} objects in this response");

                    foreach (var entry in response.S3Objects)
                    {
                        Console.WriteLine($"Processing object: {entry.Key}");
                        // Parse the date from the object key
                        var dateString = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry.Key.Split('/').Last()));
                        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var fileDate))
                        {
                            Console.WriteLine(
                                $"Parsed date: {fileDate:yyyy-MM-dd}, Last downloaded date: {lastDownloadedDate:yyyy-MM-dd}");
                            if (fileDate > lastDownloadedDate)
                            {
                                Console.WriteLine($"Downloading newer file: {entry.Key}");
                                await DownloadFileAsync(s3Client, bucketName, entry.Key, localDownloadPath);
                                fileCount++;
                            }
                            else
                            {
                                Console.WriteLine($"Skipping file: {entry.Key} (not newer than last downloaded)");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse date from: {dateString}");
                        }
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (!string.IsNullOrEmpty(request.ContinuationToken));

                Console.WriteLine($"Total files downloaded: {fileCount}");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when listing objects");
            }
        }

        private static async Task DownloadFileAsync(IAmazonS3 s3Client, string bucketName, string objectKey,
            string localDownloadPath)
        {
            try
            {
                var localFileName = Path.GetFileName(objectKey);
                var localFilePath = Path.Combine(localDownloadPath, localFileName);

                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey
                };

                using var response = await s3Client.GetObjectAsync(request);
                await response.WriteResponseStreamToFileAsync(localFilePath, false, default);
                Console.WriteLine($"Successfully downloaded {objectKey} to {localFilePath}");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when downloading an object");
            }
        }

    }
}
