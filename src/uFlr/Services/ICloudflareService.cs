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
    public string? AssetId { get; init; }
    public string? Status { get; init; }
    public CloudflareOutput? Output { get; init; }
}

public sealed class CloudflareOutput
{
    public string? StatusUrl { get; init; }
    public string? PlaybackUrl { get; init; }
}

internal sealed class CloudflareResponse<T>
{
    [JsonPropertyName("success")] public bool Success { get; init; }
    [JsonPropertyName("result")] public T? Result { get; init; }
}

internal sealed class CloudflareVideo
{
    [JsonPropertyName("uid")] public string? Uid { get; init; }
    [JsonPropertyName("readyToStream")] public bool ReadyToStream { get; init; }
    [JsonPropertyName("status")] public CloudflareVideoStatus? Status { get; init; }
    [JsonPropertyName("playback")] public CloudflarePlayback? Playback { get; init; }
}

internal sealed class CloudflareVideoStatus
{
    [JsonPropertyName("state")] public string? State { get; init; }
    [JsonPropertyName("errorReasonText")] public string? ErrorReasonText { get; init; }
}

internal sealed class CloudflarePlayback
{
    [JsonPropertyName("hls")] public string? Hls { get; init; }
}
