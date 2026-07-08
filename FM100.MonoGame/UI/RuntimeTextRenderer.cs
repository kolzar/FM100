using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using XColor = Microsoft.Xna.Framework.Color;
using XFontStyle = System.Drawing.FontStyle;
using XPoint = Microsoft.Xna.Framework.Point;
using XRectangle = Microsoft.Xna.Framework.Rectangle;
using XVector2 = Microsoft.Xna.Framework.Vector2;

namespace FM100.MonoGame.UI;

internal sealed class RuntimeTextRenderer : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<string, Texture2D> _cache = new(StringComparer.Ordinal);

    public RuntimeTextRenderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public void DrawText(
        SpriteBatch spriteBatch,
        string text,
        XVector2 position,
        XColor color,
        int size = 18,
        bool bold = false)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var texture = GetTexture(text, size, bold);
        spriteBatch.Draw(texture, position, color);
    }

    public void DrawMultilineText(
        SpriteBatch spriteBatch,
        IEnumerable<string> lines,
        XVector2 position,
        XColor color,
        int size = 18,
        int lineSpacing = 8)
    {
        var y = position.Y;
        foreach (var line in lines)
        {
            DrawText(spriteBatch, line, new XVector2(position.X, y), color, size);
            y += MeasureText(line, size).Y + lineSpacing;
        }
    }

    public XPoint MeasureText(string text, int size = 18, bool bold = false)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new XPoint(0, size);
        }

        var texture = GetTexture(text, size, bold);
        return new XPoint(texture.Width, texture.Height);
    }

    public void Dispose()
    {
        foreach (var texture in _cache.Values)
        {
            texture.Dispose();
        }

        _cache.Clear();
    }

    private Texture2D GetTexture(string text, int size, bool bold)
    {
        var key = $"{size}:{bold}:{text}";
        if (_cache.TryGetValue(key, out var texture))
        {
            return texture;
        }

        texture = CreateTexture(text, size, bold);
        _cache[key] = texture;
        return texture;
    }

    private Texture2D CreateTexture(string text, int size, bool bold)
    {
        using var scratchBitmap = new Bitmap(8, 8);
        using var scratchGraphics = Graphics.FromImage(scratchBitmap);
        using var font = new Font("Segoe UI", size, bold ? XFontStyle.Bold : XFontStyle.Regular, GraphicsUnit.Pixel);
        var measured = scratchGraphics.MeasureString(text, font, 5000, StringFormat.GenericTypographic);
        var width = Math.Max(1, (int)Math.Ceiling(measured.Width) + 4);
        var height = Math.Max(1, (int)Math.Ceiling(measured.Height) + 4);

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        graphics.DrawString(text, font, Brushes.White, 0f, 0f, StringFormat.GenericTypographic);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        return Texture2D.FromStream(_graphicsDevice, stream);
    }
}
