using RS2_225.jagex2.io;

namespace RS2_225.jagex2.graphics;

public class PixFont : Pix2D
{
    private static readonly int[] CHAR_LOOKUP = new int[256];

    private readonly int[] charAdvance = new int[95];

    private readonly byte[][] charMask = new byte[94][];

    private readonly int[] charMaskHeight = new int[94];

    private readonly int[] charMaskWidth = new int[94];

    private readonly int[] charOffsetX = new int[94];

    private readonly int[] charOffsetY = new int[94];

    private readonly int[] drawWidth = new int[256];

    public int height;

    private Random random = new();

    static PixFont()
    {
        var charset =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!\"£$%^&*()-_=+[{]};:'@#~,<.>/?\\| ";

        for (var i = 0; i < 256; i++)
        {
            var c = -1;
            if (i < charset.Length)
                c = charset.IndexOf((char)i);
            if (c == -1) c = 74;
            if (i < charset.Length)
                CHAR_LOOKUP[i] = c;
        }
    }

    public PixFont(Jagfile title, string font)
    {
        var dat = new Packet(title.read(font + ".dat", null));
        var idx = new Packet(title.read("index.dat", null));
        idx.pos = dat.g2() + 4;

        var off = idx.g1();
        if (off > 0) idx.pos += (off - 1) * 3;

        for (var i = 0; i < 94; i++)
        {
            charOffsetX[i] = idx.g1();
            charOffsetY[i] = idx.g1();

            var w = charMaskWidth[i] = idx.g2();
            var h = charMaskHeight[i] = idx.g2();

            var type = idx.g1();
            var len = w * h;

            charMask[i] = new byte[len];

            if (type == 0)
                for (var j = 0; j < len; j++)
                    charMask[i][j] = byte.Parse($"{dat.g1b()}");
            else if (type == 1)
                for (var x = 0; x < w; x++)
                for (var y = 0; y < h; y++)
                    charMask[i][x + y * w] = byte.Parse($"{dat.g1b()}");

            if (h > height) height = h;

            charOffsetX[i] = 1;
            charAdvance[i] = w + 2;

            var space = 0;
            for (var j = h / 7; j < h; j++) space += charMask[i][j * w];

            if (space <= h / 7)
            {
                charAdvance[i]--;
                charOffsetX[i] = 0;
            }

            space = 0;
            for (var j = h / 7; j < h; j++) space += charMask[i][w + j * w - 1];

            if (space <= h / 7) charAdvance[i]--;
        }

        charAdvance[94] = charAdvance[8];
        for (var c = 0; c < 256; c++) drawWidth[c] = charAdvance[CHAR_LOOKUP[c]];
    }

    public void drawStringCenter(int x, int y, string str, int rgb)
    {
        drawString(x - stringWidth(str) / 2, y, str, rgb);
    }

    public void drawStringTaggableCenter(string str, int x, int y, int color, bool shadowed)
    {
        drawStringTaggable(x - stringWidth(str) / 2, y, str, color, shadowed);
    }

    public int stringWidth(string str)
    {
        if (str == null) return 0;

        var size = 0;
        for (var c = 0; c < str.Length; c++)
            if (str.ToCharArray()[c] == '@' && c + 4 < str.Length && str.ToCharArray()[c + 4] == '@')
                c += 4;
            else
                size += drawWidth[str.ToCharArray()[c]];

        return size;
    }

    public void drawString(int x, int y, string str, int rgb)
    {
        if (str == null) return;

        var offY = y - height;

        for (var i = 0; i < str.Length; i++)
        {
            var c = CHAR_LOOKUP[str.ToCharArray()[i]];
            if (c != 94)
                drawChar(charMask[c], x + charOffsetX[c], offY + charOffsetY[c], charMaskWidth[c], charMaskHeight[c],
                    rgb);

            x += charAdvance[c];
        }
    }

    public void drawStringRight(int x, int y, string str, int rgb, bool shadowed)
    {
        if (shadowed) drawString(x + 1 - stringWidth(str), y + 1, str, 0x000000);

        drawString(x - stringWidth(str), y, str, rgb);
    }

    public void drawCenteredWave(int x, int y, string str, int rgb, int phase)
    {
        if (str == null) return;

        x -= stringWidth(str) / 2;
        var offY = y - height;

        for (var i = 0; i < str.Length; i++)
        {
            var c = CHAR_LOOKUP[str.ToCharArray()[i]];

            if (c != 94)
                drawChar(charMask[c], x + charOffsetX[c],
                    offY + charOffsetY[c] + (int)(Math.Sin(i / 2.0D + phase / 5.0D) * 5.0D), charMaskWidth[c],
                    charMaskHeight[c], rgb);

            x += charAdvance[c];
        }
    }

    public void drawStringTaggable(int x, int y, string str, int rgb, bool shadowed)
    {
        if (str == null) return;

        var offY = y - height;

        for (var i = 0; i < str.Length; i++)
            if (str.ToCharArray()[i] == '@' && i + 4 < str.Length && str.ToCharArray()[i + 4] == '@')
            {
                rgb = evaluateTag(str.Substring(i + 1, i + 4));
                i += 4;
            }
            else
            {
                var c = CHAR_LOOKUP[str.ToCharArray()[i]];
                if (c != 94)
                {
                    if (shadowed)
                        drawChar(charMask[c], x + charOffsetX[c] + 1, offY + charOffsetY[c] + 1, charMaskWidth[c],
                            charMaskHeight[c], 0);

                    drawChar(charMask[c], x + charOffsetX[c], offY + charOffsetY[c], charMaskWidth[c],
                        charMaskHeight[c], rgb);
                }

                x += charAdvance[c];
            }
    }

    public void drawStringTooltip(int x, int y, string str, int color, bool shadowed, int seed)
    {
        if (str == null) return;

        random = new Random(seed);

        var rand = (random.Next() & 0x1F) + 192;
        var offY = y - height;
        for (var i = 0; i < str.Length; i++)
            if (str.ToCharArray()[i] == '@' && i + 4 < str.Length && str.ToCharArray()[i + 4] == '@')
            {
                color = evaluateTag(str.Substring(i + 1, i + 4));
                i += 4;
            }
            else
            {
                var c = CHAR_LOOKUP[str.ToCharArray()[i]];
                if (c != 94)
                {
                    if (shadowed)
                        drawCharAlpha(x + charOffsetX[c] + 1, offY + charOffsetY[c] + 1, charMaskWidth[c],
                            charMaskHeight[c], 0, 192, charMask[c]);

                    drawCharAlpha(x + charOffsetX[c], offY + charOffsetY[c], charMaskWidth[c], charMaskHeight[c], color,
                        rand, charMask[c]);
                }

                x += charAdvance[c];
                if ((random.Next() & 0x3) == 0) x++;
            }
    }

    private int evaluateTag(string tag)
    {
        if (tag.Equals("red"))
            return 0xff0000;
        if (tag.Equals("gre"))
            return 0xff00;
        if (tag.Equals("blu"))
            return 0xff;
        if (tag.Equals("yel"))
            return 0xffff00;
        if (tag.Equals("cya"))
            return 0xffff;
        if (tag.Equals("mag"))
            return 0xff00ff;
        if (tag.Equals("whi"))
            return 0xffffff;
        if (tag.Equals("bla"))
            return 0;
        if (tag.Equals("lre"))
            return 0xff9040;
        if (tag.Equals("dre"))
            return 0x800000;
        if (tag.Equals("dbl"))
            return 0x80;
        if (tag.Equals("or1"))
            return 0xffb000;
        if (tag.Equals("or2"))
            return 0xff7000;
        if (tag.Equals("or3"))
            return 0xff3000;
        if (tag.Equals("gr1"))
            return 0xc0ff00;
        if (tag.Equals("gr2"))
            return 0x80ff00;
        if (tag.Equals("gr3"))
            return 0x40ff00;
        return 0;
    }

    private void drawChar(byte[] data, int x, int y, int w, int h, int rgb)
    {
        var dstOff = x + y * width2d;
        var dstStep = width2d - w;

        var srcStep = 0;
        var srcOff = 0;

        if (y < boundTop)
        {
            var cutoff = boundTop - y;
            h -= cutoff;
            y = boundTop;
            srcOff += cutoff * w;
            dstOff += cutoff * width2d;
        }

        if (y + h >= boundBottom) h -= y + h + 1 - boundBottom;

        if (x < boundLeft)
        {
            var cutoff = boundLeft - x;
            w -= cutoff;
            x = boundLeft;
            srcOff += cutoff;
            dstOff += cutoff;
            srcStep += cutoff;
            dstStep += cutoff;
        }

        if (x + w >= boundRight)
        {
            var cutoff = x + w + 1 - boundRight;
            w -= cutoff;
            srcStep += cutoff;
            dstStep += cutoff;
        }

        if (w > 0 && h > 0) drawMask(w, h, data, srcOff, srcStep, Pix2D.data, dstOff, dstStep, rgb);
    }

    private void drawMask(int w, int h, byte[] src, int srcOff, int srcStep, int[] dst, int dstOff, int dstStep,
        int rgb)
    {
        var hw = -(w >> 2);
        w = -(w & 0x3);

        for (var y = -h; y < 0; y++)
        {
            for (var x = hw; x < 0; x++)
            {
                if (src[srcOff++] == 0)
                    dstOff++;
                else
                    dst[dstOff++] = rgb;

                if (src[srcOff++] == 0)
                    dstOff++;
                else
                    dst[dstOff++] = rgb;

                if (src[srcOff++] == 0)
                    dstOff++;
                else
                    dst[dstOff++] = rgb;

                if (src[srcOff++] == 0)
                    dstOff++;
                else
                    dst[dstOff++] = rgb;
            }

            for (var x = w; x < 0; x++)
                if (src[srcOff++] == 0)
                    dstOff++;
                else
                    dst[dstOff++] = rgb;

            dstOff += dstStep;
            srcOff += srcStep;
        }
    }

    private void drawCharAlpha(int x, int y, int w, int h, int rgb, int alpha, byte[] mask)
    {
        var dstOff = x + y * width2d;
        var dstStep = width2d - w;

        var srcStep = 0;
        var srcOff = 0;

        if (y < boundTop)
        {
            var cutoff = boundTop - y;
            h -= cutoff;
            y = boundTop;
            srcOff += cutoff * w;
            dstOff += cutoff * width2d;
        }

        if (y + h >= boundBottom) h -= y + h + 1 - boundBottom;

        if (x < boundLeft)
        {
            var cutoff = boundLeft - x;
            w -= cutoff;
            x = boundLeft;
            srcOff += cutoff;
            dstOff += cutoff;
            srcStep += cutoff;
            dstStep += cutoff;
        }

        if (x + w >= boundRight)
        {
            var cutoff = x + w + 1 - boundRight;
            w -= cutoff;
            srcStep += cutoff;
            dstStep += cutoff;
        }

        if (w > 0 && h > 0) drawMaskAlpha(w, h, data, dstOff, dstStep, mask, srcOff, srcStep, rgb, alpha);
    }

    private void drawMaskAlpha(int w, int h, int[] dst, int dstOff, int dstStep, byte[] mask, int maskOff, int maskStep,
        int color, int alpha)
    {
        var rgb = ((int)(((color & 0xFF00FF) * alpha) & 0xFF00FF00) + (((color & 0xFF00) * alpha) & 0xFF0000)) >> 8;
        var invAlpha = 256 - alpha;

        for (var y = -h; y < 0; y++)
        {
            for (var x = -w; x < 0; x++)
                if (mask[maskOff++] == 0)
                {
                    dstOff++;
                }
                else
                {
                    var dstRgb = dst[dstOff];
                    dst[dstOff++] = (int)(((((dstRgb & 0xFF00FF) * invAlpha) & 0xFF00FF00) +
                                           (((dstRgb & 0xFF00) * invAlpha) & 0xFF0000)) >> 8) + rgb;
                }

            dstOff += dstStep;
            maskOff += maskStep;
        }
    }
}