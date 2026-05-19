namespace MediCareMS.Helpers.QRCode;

/// <summary>
/// Service for generating QR codes from text data.
/// </summary>
public interface IQRCodeService
{
    /// <summary>
    /// Generates a QR code as a Base64-encoded PNG image.
    /// </summary>
    /// <param name="data">The data to encode in the QR code</param>
    /// <returns>Base64-encoded PNG image string</returns>
    string GenerateQRCodeBase64(string data);

    /// <summary>
    /// Generates a QR code as a PNG byte array.
    /// </summary>
    /// <param name="data">The data to encode in the QR code</param>
    /// <returns>PNG image as byte array</returns>
    byte[] GenerateQRCodePNG(string data);

    /// <summary>
    /// Generates a QR code as an SVG string (scalable vector graphics).
    /// </summary>
    /// <param name="data">The data to encode in the QR code</param>
    /// <returns>SVG markup as string</returns>
    string GenerateQRCodeSVG(string data);
}
