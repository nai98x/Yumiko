using SkiaSharp;
using Yumiko.Application.Helpers;

namespace Yumiko.Application.Tests.Helpers;

public class ImageHelperTests
{
    private static byte[] SolidPng(int width, int height, SKColor color)
    {
        using SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
        surface.Canvas.Clear(color);
        using SKImage snapshot = surface.Snapshot();
        using SKData data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static SKBitmap Decode(byte[] bytes) => SKBitmap.Decode(bytes);

    [Fact]
    public void MergeImage_PutsEachImageOnItsOwnHalf()
    {
        byte[] red = SolidPng(100, 100, SKColors.Red);
        byte[] blue = SolidPng(100, 100, SKColors.Blue);

        byte[] result = ImageHelper.MergeImage(red, blue, 500, 375);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(500, bmp.Width);
        Assert.Equal(375, bmp.Height);
        Assert.Equal(SKColors.Red, bmp.GetPixel(100, 187));
        Assert.Equal(SKColors.Blue, bmp.GetPixel(400, 187));
    }

    [Fact]
    public void OverlapImage_PutsTheSecondImageOnTop()
    {
        byte[] background = SolidPng(50, 50, SKColors.Red);
        byte[] onTop = SolidPng(50, 50, SKColors.Blue);

        byte[] result = ImageHelper.OverlapImage(background, onTop, 200, 200);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(200, bmp.Width);
        Assert.Equal(200, bmp.Height);
        Assert.Equal(SKColors.Blue, bmp.GetPixel(100, 100));
    }

    [Fact]
    public void OverlapImage_TransparentFrameShowsTheBackground()
    {
        byte[] background = SolidPng(100, 100, SKColors.Red);
        byte[] transparentFrame = SolidPng(100, 100, SKColors.Transparent);

        byte[] result = ImageHelper.OverlapImage(background, transparentFrame, 100, 100);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(SKColors.Red, bmp.GetPixel(50, 50));
    }

    [Fact]
    public void IsValidImage_TellsImagesFromGarbage()
    {
        Assert.True(ImageHelper.IsValidImage(SolidPng(10, 10, SKColors.Green)));
        Assert.False(ImageHelper.IsValidImage([]));
        Assert.False(ImageHelper.IsValidImage([1, 2, 3, 4, 5]));
    }

    [Fact]
    public void DrawIntoImage_KeepsTheTemplateSize()
    {
        byte[] template = SolidPng(300, 200, SKColors.Transparent);
        byte[] image = SolidPng(50, 50, SKColors.Red);

        byte[] result = ImageHelper.DrawIntoImage(template, image, 0, 0);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(300, bmp.Width);
        Assert.Equal(200, bmp.Height);
    }

    [Fact]
    public void DrawIntoImage_OpaqueTemplateCoversTheImage()
    {
        // DstATop: donde la plantilla es opaca gana la plantilla; la imagen solo asoma por sus huecos.
        byte[] template = SolidPng(600, 600, SKColors.White);
        byte[] image = SolidPng(50, 50, SKColors.Red);

        byte[] result = ImageHelper.DrawIntoImage(template, image, 50, 50);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(SKColors.White, bmp.GetPixel(300, 300));
    }

    [Fact]
    public void DrawIntoImage_ImageShowsThroughTransparentHoles()
    {
        byte[] template = SolidPng(200, 200, SKColors.Transparent);
        byte[] image = SolidPng(50, 50, SKColors.Red);

        byte[] result = ImageHelper.DrawIntoImage(template, image, 0, 0);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(SKColors.Red, bmp.GetPixel(100, 100));
    }

    [Fact]
    public void DrawIntoImage_HonoursTheRequestedOffset()
    {
        // La imagen se escala a 500x500 desde el offset: con offset (300, 300) sobre un lienzo de 400,
        // el cuadrante superior izquierdo queda fuera de su rectángulo y no se dibuja nada ahí.
        byte[] template = SolidPng(400, 400, SKColors.Transparent);
        byte[] image = SolidPng(50, 50, SKColors.Red);

        byte[] result = ImageHelper.DrawIntoImage(template, image, 300, 300);

        using SKBitmap bmp = Decode(result);
        Assert.Equal(0, bmp.GetPixel(100, 100).Alpha);
        Assert.Equal(SKColors.Red, bmp.GetPixel(350, 350));
    }
}
