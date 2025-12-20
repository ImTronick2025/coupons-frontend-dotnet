namespace CouponsFrontend.Services;

public class CouponApiService : ICouponApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string ApiVersion = "1.0";
    private readonly string _basePath;

    public CouponApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        var baseUrl = _configuration["ApiSettings:RedeemServiceUrl"] ?? "http://localhost:5210";
        _httpClient.BaseAddress = new Uri(baseUrl);
        
        // En desarrollo con localhost, no usa /api/v1
        // En producción con APIM, sí usa /api/v1
        _basePath = baseUrl.Contains("localhost") ? "" : "/api/v1";
        
        // Agregar headers comunes según OpenAPI spec
        _httpClient.DefaultRequestHeaders.Add("x-api-version", ApiVersion);
    }

    public async Task<RedeemResponse> RedeemCouponAsync(string couponCode, string userId)
    {
        try
        {
            // Endpoint: POST /api/redeem
            var request = new { couponCode, userId };
            var response = await _httpClient.PostAsJsonAsync($"{_basePath}/api/redeem", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiRedeemResponse>();
                if (result != null)
                {
                    return new RedeemResponse(
                        result.Success,
                        result.Message,
                        result.Discount,
                        result.CouponCode,
                        result.CampaignId,
                        result.RedeemedAt
                    );
                }
            }
            
            // Manejar errores según spec
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new RedeemResponse(false, error?.Message ?? $"Error: {response.StatusCode}", null);
        }
        catch (HttpRequestException ex)
        {
            return new RedeemResponse(false, $"Error de conexión: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            return new RedeemResponse(false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<CouponInfo> GetCouponInfoAsync(string couponCode)
    {
        try
        {
            // Endpoint: GET /api/coupon/{code}
            var response = await _httpClient.GetAsync($"{_basePath}/api/coupon/{couponCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiCouponInfo>();
                if (result != null)
                {
                    return new CouponInfo(
                        result.Code,
                        result.Status,
                        result.ExpiresAt,
                        result.Discount,
                        result.Valid,
                        result.Redeemed,
                        result.CampaignId,
                        result.AssignedTo,
                        result.RedeemedAt
                    );
                }
            }
            
            return new CouponInfo(couponCode, "not_found", null, null);
        }
        catch
        {
            return new CouponInfo(couponCode, "error", null, null);
        }
    }

    // DTOs internos para deserialización de la API (match OpenAPI spec)
    private record ApiRedeemResponse(
        bool Success,
        string Message,
        decimal? Discount,
        string? CouponCode,
        string? CampaignId,
        DateTime? RedeemedAt
    );

    private record ApiCouponInfo(
        string Code,
        string Status,
        bool Valid,
        bool Redeemed,
        decimal? Discount,
        DateTime? ExpiresAt,
        string? CampaignId,
        string? AssignedTo,
        DateTime? RedeemedAt
    );

    private record ApiGenerateResponse(
        bool Success,
        int Generated,
        string Message,
        string? RequestId,
        string? CampaignId,
        string? Status,
        DateTime? EstimatedCompletionTime
    );

    private record ApiErrorResponse(
        string Error,
        string Message,
        DateTime Timestamp
    );
}
