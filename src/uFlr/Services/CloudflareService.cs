using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using uFlr.Configuration;

namespace uFlr.Services;

public sealed class CloudflareService : ICloudflareService
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CloudflareSettings _settings;

    public CloudflareService(IOptionsMonitor<CloudflareSettings> options, IHttpClientFactory httpClientFactory)
    {
        _settings = options.CurrentValue;
        _httpClientFactory = httpClientFactory;
        _client = httpClientFactory.CreateClient(nameof(CloudflareService));
        _client.BaseAddress = new Uri(_settings.ApiBasePath.TrimEnd('/') + "/");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<CloudflareAsset?> GetAsset(string assetId, CancellationToken cancellationToken = default)
    {
        using var response = await _client.GetAsync($"v1/video/assets/{assetId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CloudflareAsset>(cancellationToken);
    }

    public async Task<CloudflareAsset> CreateAsset(byte[] bytes, string? title = null, string? creatorId = null, string? externalId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_settings.ApiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(_settings.SourceId);
        var request = new CreateUploadRequest
        {
            SourceId = _settings.SourceId, Format = _settings.Format, Resolution = _settings.Resolution,
            Title = title, Description = creatorId,
            Metadata = string.IsNullOrWhiteSpace(externalId) ? null : new Dictionary<string, string> { ["external_id"] = externalId },
            KeepOriginal = _settings.KeepOriginal,
        };
        using var createResponse = await _client.PostAsJsonAsync("v1/video/assets/upload", request, cancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var asset = await createResponse.Content.ReadFromJsonAsync<CloudflareAsset>(cancellationToken)
            ?? throw new InvalidOperationException("Cloudflare returned an empty asset response.");
        ArgumentException.ThrowIfNullOrWhiteSpace(asset.UploadUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(asset.AssetId);
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using var uploadResponse = await _httpClientFactory.CreateClient("CloudflareUpload").PutAsync(asset.UploadUrl, content, cancellationToken);
        uploadResponse.EnsureSuccessStatusCode();
        return await GetAsset(asset.AssetId, cancellationToken)
            ?? throw new InvalidOperationException($"Cloudflare asset '{asset.AssetId}' could not be read after upload.");
    }

    public async Task DeleteAsset(string assetId, CancellationToken cancellationToken = default)
    {
        using var response = await _client.DeleteAsync($"v1/video/assets/{assetId}", cancellationToken);
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound) response.EnsureSuccessStatusCode();
    }

    private sealed class CreateUploadRequest
    {
        [JsonPropertyName("source_id")] public string? SourceId { get; init; }
        [JsonPropertyName("format")] public string? Format { get; init; }
        [JsonPropertyName("resolution")] public string[]? Resolution { get; init; }
        [JsonPropertyName("title")] public string? Title { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("metadata")] public Dictionary<string, string>? Metadata { get; init; }
        [JsonPropertyName("keep_original")] public bool KeepOriginal { get; init; }
    }
}
