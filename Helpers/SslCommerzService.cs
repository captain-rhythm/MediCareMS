using Microsoft.Extensions.Options;

namespace MediCareMS.Helpers;

public interface ISslCommerzService
{
    Task<SslCommerzInitResponse> InitiatePaymentAsync(SslCommerzInitRequest request);
    Task<bool> ValidateTransactionAsync(string valId, decimal amount, string currency);
}

public class SslCommerzInitRequest
{
    public string TransactionId     { get; set; } = string.Empty;
    public decimal Amount           { get; set; }
    public string Currency          { get; set; } = "BDT";
    public string CustomerName      { get; set; } = string.Empty;
    public string CustomerEmail     { get; set; } = string.Empty;
    public string CustomerPhone     { get; set; } = string.Empty;
    public string CustomerAddress   { get; set; } = "N/A";
    public string CustomerCity      { get; set; } = "Dhaka";
    public string CustomerCountry   { get; set; } = "Bangladesh";
    public string ProductName       { get; set; } = string.Empty;
    public string ProductCategory   { get; set; } = "Medical";
    public string SuccessUrl        { get; set; } = string.Empty;
    public string FailUrl           { get; set; } = string.Empty;
    public string CancelUrl         { get; set; } = string.Empty;
    public string IpnUrl            { get; set; } = string.Empty;   // ← server-to-server IPN
}

public class SslCommerzInitResponse
{
    public bool IsSuccess       { get; set; }
    public string? GatewayUrl  { get; set; }
    public string? SessionKey  { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SslCommerzService : ISslCommerzService
{
    private readonly SslCommerzOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SslCommerzService> _logger;

    public SslCommerzService(IOptions<SslCommerzOptions> options, IHttpClientFactory httpClientFactory, ILogger<SslCommerzService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SslCommerzInitResponse> InitiatePaymentAsync(SslCommerzInitRequest request)
    {
        try
        {
            _logger.LogInformation("SSLCommerz: Calling init API for txnId={TxnId}", request.TransactionId);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var postData = new Dictionary<string, string>
            {
                ["store_id"]       = _options.StoreId,
                ["store_passwd"]   = _options.StorePassword,
                ["total_amount"]   = request.Amount.ToString("F2"),
                ["currency"]       = request.Currency,
                ["tran_id"]        = request.TransactionId,
                ["success_url"]    = request.SuccessUrl,
                ["fail_url"]       = request.FailUrl,
                ["cancel_url"]     = request.CancelUrl,
                ["ipn_url"]        = request.IpnUrl,           // ← dedicated IPN endpoint
                ["cus_name"]       = request.CustomerName,
                ["cus_email"]      = request.CustomerEmail,
                ["cus_phone"]      = request.CustomerPhone,
                ["cus_add1"]       = request.CustomerAddress,
                ["cus_city"]       = request.CustomerCity,
                ["cus_country"]    = request.CustomerCountry,
                ["product_name"]   = request.ProductName,
                ["product_category"] = request.ProductCategory,
                ["product_profile"]  = "general",
                ["shipping_method"]  = "NO",
                ["num_of_item"]      = "1",
                ["product_amount"]   = request.Amount.ToString("F2"),
                ["vat"]              = "0",
                ["discount_amount"]  = "0",
                ["convenience_fee"]  = "0",
            };

            var response = await client.PostAsync(_options.InitUrl, new FormUrlEncodedContent(postData));
            var body     = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("SSLCommerz: Init response body={Body}", body);

            var result = System.Text.Json.JsonDocument.Parse(body).RootElement;
            var status = result.GetProperty("status").GetString();

            if (status == "SUCCESS")
            {
                return new SslCommerzInitResponse
                {
                    IsSuccess  = true,
                    GatewayUrl = result.GetProperty("GatewayPageURL").GetString(),
                    SessionKey = result.GetProperty("sessionkey").GetString()
                };
            }

            var reason = result.TryGetProperty("failedreason", out var fr) ? fr.GetString() : "Unknown error from SSLCommerz";
            _logger.LogError("SSLCommerz: Init FAILED status={Status} reason={Reason}", status, reason);

            return new SslCommerzInitResponse { IsSuccess = false, ErrorMessage = reason };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSLCommerz: Init exception for txnId={TxnId}", request.TransactionId);
            return new SslCommerzInitResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> ValidateTransactionAsync(string valId, decimal amount, string currency)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var url = $"{_options.ValidationUrl}?val_id={valId}&store_id={_options.StoreId}&store_passwd={_options.StorePassword}&format=json";
            var responseBody = await client.GetStringAsync(url);

            _logger.LogDebug("SSLCommerz: Validation response body={Body}", responseBody);

            var result = System.Text.Json.JsonDocument.Parse(responseBody).RootElement;
            var status = result.GetProperty("status").GetString();

            _logger.LogInformation("SSLCommerz: Validation valId={ValId} status={Status}", valId, status);
            return status is "VALID" or "VALIDATED";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSLCommerz: Validation exception for valId={ValId}", valId);
            return false;
        }
    }
}
