using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using uFlr.Configuration;

namespace uFlr.Services;

public sealed class CloudflareService : ICloudflareService
{
    private readonly HttpClient _client;
    private readonly CloudflareSettings _settings;

    public CloudflareService(IOptionsMonitor<CloudflareSettings> options, IHttpClientFactory httpClientFactory)
    {
        _settings = options.CurrentValue;
        _client = httpClientFactory.CreateClient(nameof(CloudflareService));
        _client.BaseAddress = new Uri(_settings.ApiBasePath.TrimEnd('/') + "/");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiToken);
    }

    public async Task<CloudflareAsset?> GetAsset(string assetId, CancellationToken cancellationToken = default)
    {
        ValidateSettings();
        using var response = await _client.GetAsync($"client/v4/accounts/{_settings.AccountId}/stream/{Uri.EscapeDataString(assetId)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<CloudflareResponse<CloudflareVideo>>(cancellationToken)
            ?? throw new InvalidOperationException("Cloudflare returned an empty video response.");
        return envelope.Result is null ? null : Map(envelope.Result);
    }

    public async Task<CloudflareAsset> CreateAsset(byte[] bytes, string? title = null, string? creatorId = null, string? externalId = null, CancellationToken cancellationToken = default)
    {
        ValidateSettings();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytes.Length);
        var id = bytes.Length > 200 * 1024 * 1024
            ? await UploadTusAsync(bytes, title, creatorId, cancellationToken)
            : await UploadBasicAsync(bytes, creatorId, cancellationToken);

        return await GetAsset(id, cancellationToken)
            ?? throw new InvalidOperationException($"Cloudflare video '{id}' could not be read after upload.");
    }

    public async Task DeleteAsset(string assetId, CancellationToken cancellationToken = default)
    {
        ValidateSettings();
        using var response = await _client.DeleteAsync($"client/v4/accounts/{_settings.AccountId}/stream/{Uri.EscapeDataString(assetId)}", cancellationToken);
        if (response.StatusCode != HttpStatusCode.NotFound) response.EnsureSuccessStatusCode();
    }

    private async Task<string> UploadBasicAsync(byte[] bytes, string? creatorId, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "file", "umbraco-video.mp4");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"client/v4/accounts/{_settings.AccountId}/stream")
        {
            Content = form
        };
        if (!string.IsNullOrWhiteSpace(creatorId)) request.Headers.Add("Upload-Creator", creatorId);
        using var response = await _client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<CloudflareResponse<CloudflareVideo>>(cancellationToken)
            ?? throw new InvalidOperationException("Cloudflare returned an empty upload response.");
        return envelope.Result?.Uid
            ?? throw new InvalidOperationException("Cloudflare did not return a video UID.");
    }

    private async Task<string> UploadTusAsync(byte[] bytes, string? title, string? creatorId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"client/v4/accounts/{_settings.AccountId}/stream");
        request.Headers.Add("Tus-Resumable", "1.0.0");
        request.Headers.Add("Upload-Length", bytes.Length.ToString());
        var metadata = new List<string>();
        if (!string.IsNullOrWhiteSpace(title)) metadata.Add($"name {Convert.ToBase64String(Encoding.UTF8.GetBytes(title))}");
        if (!string.IsNullOrWhiteSpace(creatorId)) metadata.Add($"creator {Convert.ToBase64String(Encoding.UTF8.GetBytes(creatorId))}");
        if (metadata.Count > 0) request.Headers.Add("Upload-Metadata", string.Join(',', metadata));
        using var response = await _client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var uploadUrl = response.Headers.Location?.ToString()
            ?? throw new InvalidOperationException("Cloudflare did not return a TUS upload location.");
        var assetId = response.Headers.TryGetValues("stream-media-id", out var values)
            ? values.SingleOrDefault()
            : null;
        ArgumentException.ThrowIfNullOrWhiteSpace(assetId);

        var offset = 0;
        var chunkSize = Math.Max(5_242_880, _settings.TusChunkSizeBytes);
        while (offset < bytes.Length)
        {
            var count = Math.Min(chunkSize, bytes.Length - offset);
            using var chunk = new ByteArrayContent(bytes, offset, count);
            chunk.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
            using var patch = new HttpRequestMessage(HttpMethod.Patch, uploadUrl) { Content = chunk };
            patch.Headers.Add("Tus-Resumable", "1.0.0");
            patch.Headers.Add("Upload-Offset", offset.ToString());
            using var patchResponse = await _client.SendAsync(patch, cancellationToken);
            patchResponse.EnsureSuccessStatusCode();
            offset = patchResponse.Headers.TryGetValues("Upload-Offset", out var offsets)
                && int.TryParse(offsets.SingleOrDefault(), out var serverOffset)
                ? serverOffset
                : offset + count;
        }
        return assetId;
    }

    private void ValidateSettings()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_settings.AccountId);
        ArgumentException.ThrowIfNullOrWhiteSpace(_settings.ApiToken);
    }

    private CloudflareAsset Map(CloudflareVideo video)
        => new()
        {
            AssetId = video.Uid,
            Status = video.Status?.ErrorReasonText is not null && video.Status.ErrorReasonText.Length > 0
                ? "error"
                : video.ReadyToStream || video.Status?.State == "ready" ? "ready" : video.Status?.State,
            Output = new CloudflareOutput
            {
                StatusUrl = video.Uid is null ? null : $"client/v4/accounts/{_settings.AccountId}/stream/{video.Uid}",
                PlaybackUrl = video.Playback?.Hls ?? BuildPlaybackUrl(video.Uid)
            }
        };

    private string? BuildPlaybackUrl(string? uid)
        => string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(_settings.CustomerCode)
            ? null
            : $"https://customer-{_settings.CustomerCode}.cloudflarestream.com/{uid}/manifest/video.m3u8";
}
