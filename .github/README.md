# uFlr

[![Downloads](https://img.shields.io/nuget/dt/Umbraco.Community.uFlr?color=cc9900)](https://www.nuget.org/packages/Umbraco.Community.uFlr/)
[![NuGet](https://img.shields.io/nuget/vpre/Umbraco.Community.uFlr?color=0273B3)](https://www.nuget.org/packages/Umbraco.Community.uFlr)
[![GitHub license](https://img.shields.io/github/license/skttl/umbraco-uflr?color=8AB803)](../LICENSE)

uFlr synchronizes video files from Umbraco with Cloudflare Stream. When an editor saves a video, uFlr uploads it to Cloudflare Stream and stores the Stream video ID and HLS playback URL on the Umbraco item.

## Requirements

- Umbraco CMS 18 or newer
- A Cloudflare account with Stream enabled
- A Cloudflare API token with Stream read and write permissions
- The Cloudflare account ID
- The Stream customer code for HLS playback
- A local Umbraco video upload property

## Installation

~~~sh
dotnet add package Umbraco.Community.uFlr
~~~

## Cloudflare Stream configuration

Add the following section to appsettings.json:

~~~json
{
  "Umbraco": {
    "Cloudflare": {
      "ApiBasePath": "https://api.cloudflare.com",
      "AccountId": "YOUR_CLOUDFLARE_ACCOUNT_ID",
      "ApiToken": "YOUR_CLOUDFLARE_API_TOKEN",
      "CustomerCode": "YOUR_CUSTOMER_CODE",
      "TusChunkSizeBytes": 5242880
    }
  }
}
~~~

AccountId is the Cloudflare account identifier. ApiToken must have Stream read and write permissions. CustomerCode is used to construct the HLS manifest URL returned by the API.

The same settings can be supplied with environment variables:

~~~text
Umbraco__Cloudflare__AccountId=YOUR_CLOUDFLARE_ACCOUNT_ID
Umbraco__Cloudflare__ApiToken=YOUR_CLOUDFLARE_API_TOKEN
Umbraco__Cloudflare__CustomerCode=YOUR_CUSTOMER_CODE
Umbraco__Cloudflare__TusChunkSizeBytes=5242880
~~~

Create the token in Cloudflare's API Tokens area. Never include the token in screenshots or source control.

<!-- Screenshot needed: Cloudflare API token creation showing Stream read/write permissions. Do not include the actual token. -->

![Cloudflare API token](../docs/cloudflare_api_token.png)

Find the account ID in the Cloudflare dashboard. The customer code is available from Stream playback URLs or the Stream account settings.

<!-- Screenshot needed: Cloudflare dashboard showing the account ID and Stream customer code. -->

![Cloudflare account and customer code](../docs/cloudflare_account_customer.png)

See Cloudflare's [Stream API documentation](https://developers.cloudflare.com/api/resources/stream/) and [upload documentation](https://developers.cloudflare.com/stream/uploading-videos/).

## Add the property editor

uFlr adds a Cloudflare Stream Sync property editor to the Umbraco backoffice.

1. Open Settings > Data Types.
2. Create a data type using the Cloudflare Stream Sync property editor.
3. In Upload Property Alias, enter the alias of the Upload property containing the video file.
4. Add the new data type to the same media, content, or member type as the Upload property.
5. Save the data type and the content type.

<!-- Screenshot needed: Umbraco Data Type editor showing Cloudflare Stream Sync and Upload Property Alias. -->

![Cloudflare Stream Sync data type settings](../docs/umbraco_cloudflare_sync_data_type.png)

## Upload and synchronization

When an editor saves an item, uFlr checks whether the configured upload property changed. If it did, uFlr:

1. Deletes the previously linked Stream video, if one exists.
2. Uses a multipart upload for files up to 200 MB.
3. Uses resumable TUS upload with chunked PATCH requests for larger files.
4. Reads the Cloudflare Stream status and playback information.
5. Stores the Stream video ID and HLS manifest URL in the sync property.

Cloudflare processes uploaded videos asynchronously. The backoffice editor polls Stream and shows Preparing, Ready, or Error status.

For a video that existed before the sync property was added, save the item again to trigger synchronization.

<!-- Screenshot needed: Umbraco media or content item showing a Cloudflare Stream video synchronized successfully. -->

![Synchronized Cloudflare Stream video in Umbraco](../docs/umbraco_cloudflare_media.png)

## Stored value

The sync property stores a JSON value converted to uFlr.Models.CloudflareValue:

~~~json
{
  "Src": "/media/example/video.mp4",
  "CloudflareAssetId": "ea95132c15732412d22c1476fa83f27a",
  "PlaybackUrl": "https://customer-xxxxxxxx.cloudflarestream.com/ea95132c15732412d22c1476fa83f27a/manifest/video.m3u8"
}
~~~

CloudflareAssetId is the Stream video UID. PlaybackUrl is the HLS manifest URL returned by Cloudflare.

## Render a video

Use an HLS-capable player:

~~~cshtml
@if (Model.CloudflareVideo?.PlaybackUrl is { } url)
{
    <video controls style="width: 100%;" src="@url"></video>
}
~~~

Native HLS support varies by browser. For broader browser support, use an HLS player such as hls.js. Cloudflare's hosted player can also be used with the video UID.

See Cloudflare's [HLS playback documentation](https://developers.cloudflare.com/stream/viewing-videos/using-own-player/).

## Troubleshooting

### Nothing is uploaded

- Check AccountId and ApiToken.
- Confirm that the token has Stream read and write permissions.
- Confirm that the Cloudflare Stream Sync property and upload property are on the same content type.
- Confirm that Upload Property Alias exactly matches the upload property's alias.
- Check the Umbraco logs for the Cloudflare request error.

### The status stays at Preparing

Cloudflare processes the uploaded video asynchronously. Check the video in Stream and inspect its status and error reason.

### The HLS URL does not work

- Confirm that CustomerCode is correct.
- Confirm that the video has reached Ready status.
- Check Stream allowed origins and signed URL settings.
- Use an HLS-capable player.

## Contributing

The repository includes a test site for local development. Keep Cloudflare credentials in local secrets or appsettings.Development.json, never in committed source.

