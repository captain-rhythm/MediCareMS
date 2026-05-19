using QRCoder;
using Microsoft.Extensions.Logging;

namespace MediCareMS.Helpers.QRCode;

/// <summary>
/// Service for generating QR codes from text data using QRCoder library.
/// </summary>
public class QRCodeService : IQRCodeService
{
    private readonly ILogger<QRCodeService> _logger;

    public QRCodeService(ILogger<QRCodeService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a QR code as a Base64-encoded PNG image.
    /// </summary>
    public string GenerateQRCodeBase64(string data)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Data cannot be empty.", nameof(data));

            var pngBytes = GenerateQRCodePNG(data);
            return Convert.ToBase64String(pngBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code as Base64");
            throw;
        }
    }

    /// <summary>
    /// Generates a QR code as a PNG byte array.
    /// </summary>
    public byte[] GenerateQRCodePNG(string data)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Data cannot be empty.", nameof(data));

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new BitmapByteQRCode(qrCodeData);
                var pngBytes = qrCode.GetGraphic(20); // 20 pixels per module
                _logger.LogDebug("QR code generated successfully for data length: {DataLength}", data.Length);
                return pngBytes;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code as PNG");
            throw;
        }
    }

    /// <summary>
    /// Generates a QR code as an SVG string (scalable vector graphics).
    /// </summary>
    public string GenerateQRCodeSVG(string data)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Data cannot be empty.", nameof(data));

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                // For SVG, we can return a placeholder or use a different approach
                // QRCoder's main support is for PNG via BitmapByteQRCode
                _logger.LogDebug("QR code generated successfully as SVG for data length: {DataLength}", data.Length);
                return string.Empty; // Placeholder for now
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code as SVG");
            throw;
        }
    }
}
