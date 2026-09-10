using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using uFlr.Enums;
using uFlr.Services;

namespace uFlr.Controllers;

[ApiVersion(Constants.Swagger.Version)]
[ApiExplorerSettings(GroupName = Constants.Swagger.GroupName)]
public class CloudflareApiController : CloudflareApiControllerBase
{
    private readonly ICloudflareService _cloudflareService;

    public CloudflareApiController(ICloudflareService cloudflareService) => _cloudflareService = cloudflareService;

    [HttpGet("status")]
    [ProducesResponseType<AssetStatus>(StatusCodes.Status200OK)]
    public async Task<AssetStatus> GetStatus(string assetId)
    {
        var asset = await _cloudflareService.GetAsset(assetId);
        if (asset == null)
        {
            return AssetStatus.NotFound;
        }

        return asset.Status?.ToLowerInvariant() switch
        {
            "created" or "pre-queued" or "upload-pending" or "uploaded" or "queued" or
            "downloading" or "downloaded" or "processing" or "processed" or
            "stream-ready" or "repackaging" => AssetStatus.Preparing,
            "errored" or "error" => AssetStatus.Errored,
            "ready" => AssetStatus.Ready,
            _ => AssetStatus.Unknown,
        };
    }
}
