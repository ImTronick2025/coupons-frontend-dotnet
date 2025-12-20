using System.Net.Http.Json;

namespace CouponsFrontend.Services;

public class CampaignApiService : ICampaignApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CampaignApiService> _logger;
    private readonly string _basePath;

    public CampaignApiService(HttpClient httpClient, IConfiguration configuration, ILogger<CampaignApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Read base URL from configuration
        var baseUrl = configuration["ApiSettings:CampaignServiceUrl"] ?? "http://localhost:5277";
        _basePath = $"{baseUrl}/api/campaigns";
        
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<GenerateResponse> GenerateCouponsAsync(string campaignId, int quantity)
    {
        try
        {
            _logger.LogInformation("Generating {Quantity} coupons for campaign {CampaignId}", quantity, campaignId);
            
            // Extract prefix from campaignId (e.g., "CAMPAIGN-NY2026" -> "NY26")
            var prefix = ExtractPrefix(campaignId);
            
            var request = new { Amount = quantity, Prefix = prefix };
            var response = await _httpClient.PostAsJsonAsync($"{_basePath}/{campaignId}/generate", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiGenerateResponse>();
                if (result != null)
                {
                    _logger.LogInformation("Generation request created: {RequestId}", result.RequestId);
                    return new GenerateResponse(
                        result.Success,
                        result.Generated,
                        result.Message,
                        result.RequestId,
                        result.CampaignId,
                        result.Status,
                        result.EstimatedCompletionTime
                    );
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Campaign generation failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            
            return new GenerateResponse(false, 0, $"Error al generar cupones: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error generating coupons");
            return new GenerateResponse(false, 0, $"Error de conexión con CampaignService: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating coupons");
            return new GenerateResponse(false, 0, $"Error inesperado: {ex.Message}");
        }
    }

    public async Task<List<CampaignInfo>> GetCampaignsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching active campaigns");
            
            var response = await _httpClient.GetAsync(_basePath);
            
            if (response.IsSuccessStatusCode)
            {
                var campaigns = await response.Content.ReadFromJsonAsync<List<ApiCampaignResponse>>();
                if (campaigns != null)
                {
                    return campaigns
                        .Where(c => c.IsActive)
                        .Select(c => new CampaignInfo(
                            c.CampaignId,
                            c.Name,
                            c.IsActive,
                            c.StartDate,
                            c.EndDate,
                            c.DiscountPercentage,
                            c.DiscountAmount
                        ))
                        .ToList();
                }
            }
            
            _logger.LogWarning("Failed to fetch campaigns: {StatusCode}", response.StatusCode);
            return new List<CampaignInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns");
            return new List<CampaignInfo>();
        }
    }

    private string ExtractPrefix(string campaignId)
    {
        // Extract meaningful prefix from campaign ID
        // Example: "CAMPAIGN-NY2026" -> "NY26"
        var parts = campaignId.Split('-');
        if (parts.Length > 1)
        {
            var lastPart = parts[^1]; // Get last part (e.g., "NY2026")
            // Take first 2 letters + last 2 digits
            if (lastPart.Length >= 4)
            {
                return lastPart.Substring(0, 2) + lastPart.Substring(lastPart.Length - 2);
            }
            return lastPart.Substring(0, Math.Min(4, lastPart.Length));
        }
        // Fallback: use first 4 chars of campaign ID
        return campaignId.Substring(0, Math.Min(4, campaignId.Length)).ToUpper();
    }

    public async Task<CampaignStats> GetCampaignStatsAsync(string campaignId)
    {
        try
        {
            _logger.LogInformation("Fetching stats for campaign {CampaignId}", campaignId);
            
            var response = await _httpClient.GetAsync($"{_basePath}/{campaignId}/stats");
            
            if (response.IsSuccessStatusCode)
            {
                var stats = await response.Content.ReadFromJsonAsync<ApiCampaignStatsResponse>();
                if (stats != null)
                {
                    return new CampaignStats(
                        stats.CampaignId,
                        stats.TotalGenerated,
                        stats.TotalUsed,
                        stats.TotalAvailable
                    );
                }
            }
            
            _logger.LogWarning("Failed to fetch campaign stats: {StatusCode}", response.StatusCode);
            return new CampaignStats(campaignId, 0, 0, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign stats");
            return new CampaignStats(campaignId, 0, 0, 0);
        }
    }

    // API Response DTOs
    private class ApiGenerateResponse
    {
        public bool Success { get; set; }
        public int Generated { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RequestId { get; set; }
        public string? CampaignId { get; set; }
        public string? Status { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
    }

    private class ApiCampaignResponse
    {
        public string CampaignId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
    }

    private class ApiCampaignStatsResponse
    {
        public string CampaignId { get; set; } = string.Empty;
        public int TotalGenerated { get; set; }
        public int TotalUsed { get; set; }
        public int TotalAvailable { get; set; }
    }
}
