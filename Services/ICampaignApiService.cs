namespace CouponsFrontend.Services;

public interface ICampaignApiService
{
    Task<GenerateResponse> GenerateCouponsAsync(string campaignId, int quantity);
    Task<List<CampaignInfo>> GetCampaignsAsync();
    Task<CampaignStats> GetCampaignStatsAsync(string campaignId);
}

public record CampaignInfo(
    string CampaignId,
    string Name,
    bool IsActive,
    DateTime StartDate,
    DateTime EndDate,
    decimal? DiscountPercentage,
    decimal? DiscountAmount
);

public record GenerateResponse(
    bool Success,
    int Generated,
    string Message,
    string? RequestId = null,
    string? CampaignId = null,
    string? Status = null,
    DateTime? EstimatedCompletionTime = null
);

public record CampaignStats(
    string CampaignId,
    int TotalGenerated,
    int TotalUsed,
    int TotalAvailable
);
