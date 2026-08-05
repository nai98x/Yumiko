using SkiaSharp;

namespace Yumiko.Application.Helpers;

public static class ImageHelper
{
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear, SKMipmapMode.None);

    /// <summary>
    /// Places two images side by side (each one taking the left/right half) on an x*y canvas.
    /// </summary>
    public static byte[] MergeImage(byte[] bytes1, byte[] bytes2, int x, int y)
    {
        using SKImage img1 = SKImage.FromEncodedData(bytes1);
        using SKImage img2 = SKImage.FromEncodedData(bytes2);

        using SKSurface surface = SKSurface.Create(new SKImageInfo(x, y));
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        canvas.DrawImage(img1, new SKRect(0, 0, x / 2f, y), Sampling);
        canvas.DrawImage(img2, new SKRect(x / 2f, 0, x, y), Sampling);

        return Encode(surface);
    }

    /// <summary>
    /// Overlays <paramref name="image2"/> on top of <paramref name="image1"/>, both filling the x*y canvas.
    /// </summary>
    public static byte[] OverlapImage(byte[] image1, byte[] image2, int x, int y)
    {
        using SKImage img1 = SKImage.FromEncodedData(image1);
        using SKImage img2 = SKImage.FromEncodedData(image2);

        using SKSurface surface = SKSurface.Create(new SKImageInfo(x, y));
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        SKRect dest = new(0, 0, x, y);
        canvas.DrawImage(img1, dest, Sampling);
        canvas.DrawImage(img2, dest, Sampling);

        return Encode(surface);
    }

    /// <summary>
    /// Draws <paramref name="image"/> (resized to 500x500) inside <paramref name="templateImage"/> at
    /// position (x, y). Uses DstAtop composition so the image only shows through the transparent gaps of the
    /// template, keeping it behind the frame.
    /// </summary>
    public static byte[] DrawIntoImage(byte[] templateImage, byte[] image, int x, int y)
    {
        using SKImage template = SKImage.FromEncodedData(templateImage);
        using SKImage img = SKImage.FromEncodedData(image);

        using SKSurface surface = SKSurface.Create(new SKImageInfo(template.Width, template.Height));
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        canvas.DrawImage(template, 0, 0, Sampling);

        using SKPaint paint = new() { BlendMode = SKBlendMode.DstATop };
        canvas.DrawImage(img, new SKRect(x, y, x + 500, y + 500), Sampling, paint);

        return Encode(surface);
    }

    /// <summary>Checks that the bytes are a decodable image (the declared content-type is not enough).</summary>
    public static bool IsValidImage(byte[] bytes)
    {
        if (bytes.Length == 0) return false;
        using SKData data = SKData.CreateCopy(bytes);
        using SKCodec? codec = SKCodec.Create(data);
        return codec is not null;
    }

    private static byte[] Encode(SKSurface surface)
    {
        using SKImage snapshot = surface.Snapshot();
        using SKData data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
