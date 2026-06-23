namespace MediCareMS.Helpers;

public class SslCommerzOptions
{
    public string StoreId { get; set; } = string.Empty;
    public string StorePassword { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.sslcommerz.com";
    public string InitUrl => $"{BaseUrl}/gwprocess/v4/api.php";
    public string ValidationUrl => $"{BaseUrl}/validator/api/validationserverAPI.php";
    public bool IsSandbox { get; set; } = true;
    public bool UseMockPayment { get; set; } = false;
    public string AppBaseUrl { get; set; } = "http://localhost:5002";
}
