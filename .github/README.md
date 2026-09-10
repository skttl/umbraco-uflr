# uFlr

uFlr synchronizes Umbraco video files with Cloudflare Stream.

## Installation

`sh
dotnet add package Umbraco.Community.uFlr
`

Add a Cloudflare Stream Sync property to the same type as the source video property and set Upload Property Alias to the source property alias.

## Configuration

Create a Cloudflare API token with Stream read/write permissions. CustomerCode is used to build the HLS manifest URL.

`json
{
  "Umbraco": {
    "Cloudflare": {
      "AccountId": "YOUR_CLOUDFLARE_ACCOUNT_ID",
      "ApiToken": "YOUR_CLOUDFLARE_API_TOKEN",
      "CustomerCode": "YOUR_CUSTOMER_CODE"
    }
  }
}
`

Never expose or commit the API token. It is sent as a Bearer token.

## API behavior

Files up to 200 MB use the multipart Stream upload endpoint. Larger files use a resumable TUS upload with chunked PATCH requests. Status is read with the Stream video endpoint and deletion uses the Stream delete endpoint.

The stored playback value is the HLS manifest URL. CloudflareAssetId is the Stream video UID.

## References

- [Cloudflare basic video upload](https://developers.cloudflare.com/stream/uploading-videos/upload-video-file/)
- [Cloudflare resumable uploads](https://developers.cloudflare.com/stream/uploading-videos/resumable-uploads/)
- [Cloudflare Stream API](https://developers.cloudflare.com/api/resources/stream/)
- [Cloudflare HLS playback](https://developers.cloudflare.com/stream/viewing-videos/using-own-player/)

