using System.Text.Json.Serialization;

namespace uFlr.Services;

public interface ICloudflareService
{
    Task<CloudflareAsset?> GetAsset(string assetId, CancellationToken cancellationToken = default);
    Task<CloudflareAsset> CreateAsset(byte[] bytes, string? title = null, string? creatorId = null, string? externalId = null, CancellationToken cancellationToken = default);
    Task DeleteAsset(string assetId, CancellationToken cancellationToken = default);
}

public sealed class CloudflareAsset
{
    [JsonPropertyName("asset_id")]
    public string? AssetId { get; set; }
    public string? Status { get; set; }
    public CloudflareOutput? Output { get; set; }
    [JsonPropertyName("upload_url")]
    public string? UploadUrl { get; set; }
}

public sealed class CloudflareOutput
{
    [JsonPropertyName("status_url")] public string? StatusUrl { get; set; }
    [JsonPropertyName("playback_url")] public string? PlaybackUrl { get; set; }
}
