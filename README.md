# Download a Flat File from Polygon.io's Amazon S3 bucket

This example looks at a destination folder and will only download the latest
files that are not already in that folder. I haven't tested it with an empty
folder. Use this when you need to have a chron / scheduled job that will download
the latest data on a nightly basis.

Obviously, requires a subscription. You probably should read their documentation.

**IMPORTANT! THIS EXAMPLE REQUIRES SOME CONFIGURATION BEFORE IT WILL WORK!**

This example uses an environment variable file to hold secrets so it doesn't 
hardcode them in your code. That's insecure. Never do that.

To get this to work:

Add an `.env` file as a child to the Project (not solution).

The contents must be in the form:

```
AWS_ACCESS_KEY_ID=xxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
AWS_SECRET_ACCESS_KEY=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
S3_SERVICE_URL=https://files.polygon.io
S3_BUCKET_NAME=flatfiles
S3_PREFIX=us_stocks_sip/day_aggs_v1/
```

You can get your subscription-specific information from: 
[https://polygon.io/dashboard/flat-files](https://polygon.io/dashboard/flat-files)

As far as the `S3_PREFIX` setting goes, you'll modify this so you get the type of data you want. 
Go [here](https://polygon.io/flat-files), click on one of the types of "Flat Files" you want
from the TOC on the left, in the "File Browser", click a Year and the a Month, and when you see
a day, click the `</> Code` button, click the `Boto3` tab, find the `object_key` and copy the
first two segments of that (ex. `us_stocks_sip/day_aggs_v1/`).

AND MOST IMPORTANTLY, select the file, then in the Properties window, set the `Copy to output directory` 
to `Copy if newer`.