namespace PolygonDownloadAmazonS3FlatFile
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string localDownloadPath = @"D:\data\downloads";

            await DownloadManager.DownloadRecentFiles(localDownloadPath);
        }
    }
}