namespace uFlr.Configuration;

public class CloudflareSettings
{
    public string ApiBasePath { get; set; } = "https://api.cloudflare.com";
    public string? AccountId { get; set; }
    public string? ApiToken { get; set; }
    public string? CustomerCode { get; set; }
    public int TusChunkSizeBytes { get; set; } = 5_242_880;
}
