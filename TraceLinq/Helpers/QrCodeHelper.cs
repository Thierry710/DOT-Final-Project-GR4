using QRCoder;

namespace TraceLinq.Helpers;

// Makes QR code PNG images with QRCoder's PngByteQRCode, which writes PNG bytes
// directly (no System.Drawing), so it works the same on Windows, Linux and macOS.
public static class QrCodeHelper
{
    // pixelsPerModule = size of one QR "square" in pixels; 10 gives roughly a 330px image.
    public static byte[] ToPng(string text, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        // ECC level Q: still scans with up to ~25% of the code damaged (good for printed labels).
        using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }
}
