namespace CouponsFrontend.Services;

public interface ICouponApiService
{
    Task<RedeemResponse> RedeemCouponAsync(string couponCode, string userId);
    Task<CouponInfo> GetCouponInfoAsync(string couponCode);
}

// Modelos alineados con OpenAPI spec
public record RedeemResponse(
    bool Success, 
    string Message, 
    decimal? Discount,
    string? CouponCode = null,
    string? CampaignId = null,
    DateTime? RedeemedAt = null
);

public record CouponInfo(
    string Code, 
    string Status, 
    DateTime? ExpiresAt, 
    decimal? Discount,
    bool Valid = false,
    bool Redeemed = false,
    string? CampaignId = null,
    string? AssignedTo = null,
    DateTime? RedeemedAt = null
);