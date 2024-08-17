using RS2_225.jagex2.io;

namespace RS2_225.jagex2.graphics;

public class Pix8
{
    private int cropH;

    public int cropW;

    public int cropX;

    public int cropY;

    public int height;

    public int[] palette;

    public byte[] pixels;

    public int width;

    public Pix8(Jagfile jag, string name, int index)
    {
        var dat = new Packet(jag.read(name + ".dat", null));
        var idx = new Packet(jag.read("index.dat", null));
        idx.pos = dat.g2();

        cropW = idx.g2();
        cropH = idx.g2();

        var paletteCount = idx.g1();
        palette = new int[paletteCount];
        for (var i = 0; i < paletteCount - 1; i++) palette[i + 1] = idx.g3();

        for (var i = 0; i < index; i++)
        {
            idx.pos += 2;
            dat.pos += idx.g2() * idx.g2();
            idx.pos++;
        }

        cropX = idx.g1();
        cropY = idx.g1();
        width = idx.g2();
        height = idx.g2();

        var pixelOrder = idx.g1();
        var len = width * height;
        pixels = new byte[len];

        if (pixelOrder == 0)
            for (var i = 0; i < len; i++)
                pixels[i] = byte.Parse($"{dat.g1b()}");
        else if (pixelOrder == 1)
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                pixels[x + y * width] = byte.Parse($"{dat.g1b()}");
    }

    public void shrink()
    {
        cropW /= 2;
        cropH /= 2;

        var pixels = new byte[cropW * cropH];
        var off = 0;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            pixels[((x + cropX) >> 1) + ((y + cropY) >> 1) * cropW] = this.pixels[off++];

        this.pixels = pixels;
        width = cropW;
        height = cropH;
        cropX = 0;
        cropY = 0;
    }

    public void crop()
    {
        if (width == cropW && height == cropH) return;

        var pixels = new byte[cropW * cropH];
        var off = 0;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            pixels[x + cropX + (y + cropY) * cropW] = this.pixels[off++];

        this.pixels = pixels;
        width = cropW;
        height = cropH;
        cropX = 0;
        cropY = 0;
    }

    public void flipHorizontally()
    {
        var pixels = new byte[width * height];
        var off = 0;
        for (var y = 0; y < height; y++)
        for (var x = width - 1; x >= 0; x--)
            pixels[off++] = this.pixels[x + y * width];

        this.pixels = pixels;
        cropX = cropW - width - cropX;
    }

    public void flipVertically()
    {
        var pixels = new byte[width * height];
        var off = 0;
        for (var y = height - 1; y >= 0; y--)
        for (var x = 0; x < width; x++)
            pixels[off++] = this.pixels[x + y * width];

        this.pixels = pixels;
        cropY = cropH - height - cropY;
    }

    public void translate(int r, int g, int b)
    {
        for (var i = 0; i < palette.Length; i++)
        {
            var red = (palette[i] >> 16) & 0xFF;
            red += r;
            if (red < 0)
                red = 0;
            else if (red > 255) red = 255;

            var green = (palette[i] >> 8) & 0xFF;
            green += g;
            if (green < 0)
                green = 0;
            else if (green > 255) green = 255;

            var blue = palette[i] & 0xFF;
            blue += b;
            if (blue < 0)
                blue = 0;
            else if (blue > 255) blue = 255;

            palette[i] = (red << 16) + (green << 8) + blue;
        }
    }

    public void draw(int x, int y)
    {
        x += cropX;
        y += cropY;

        var dstOff = x + y * Pix2D.width2d;
        var srcOff = 0;
        var h = height;
        var w = width;
        var dstStep = Pix2D.width2d - w;
        var srcStep = 0;

        if (y < Pix2D.boundTop)
        {
            var cutoff = Pix2D.boundTop - y;
            h -= cutoff;
            y = Pix2D.boundTop;
            srcOff += cutoff * w;
            dstOff += cutoff * Pix2D.width2d;
        }

        if (y + h > Pix2D.boundBottom) h -= y + h - Pix2D.boundBottom;

        if (x < Pix2D.boundLeft)
        {
            var cutoff = Pix2D.boundLeft - x;
            w -= cutoff;
            x = Pix2D.boundLeft;
            srcOff += cutoff;
            dstOff += cutoff;
            srcStep += cutoff;
            dstStep += cutoff;
        }

        if (x + w > Pix2D.boundRight)
        {
            var cutoff = x + w - Pix2D.boundRight;
            w -= cutoff;
            srcStep += cutoff;
            dstStep += cutoff;
        }

        if (w > 0 && h > 0) copyPixels(w, h, pixels, srcOff, srcStep, Pix2D.data, dstOff, dstStep, palette);
    }

    private void copyPixels(int w, int h, byte[] src, int srcOff, int srcStep, int[] dst, int dstOff, int dstStep,
        int[] palette)
    {
        var qw = -(w >> 2);
        w = -(w & 0x3);

        for (var y = -h; y < 0; y++)
        {
            for (var x = qw; x < 0; x++)
            {
                var palIndex = src[srcOff++];
                if (palIndex == 0)
                    dstOff++;
                else
                    dst[dstOff++] = palette[palIndex & 0xFF];

                palIndex = src[srcOff++];
                if (palIndex == 0)
                    dstOff++;
                else
                    dst[dstOff++] = palette[palIndex & 0xFF];

                palIndex = src[srcOff++];
                if (palIndex == 0)
                    dstOff++;
                else
                    dst[dstOff++] = palette[palIndex & 0xFF];

                palIndex = src[srcOff++];
                if (palIndex == 0)
                    dstOff++;
                else
                    dst[dstOff++] = palette[palIndex & 0xFF];
            }

            for (var x = w; x < 0; x++)
            {
                var palIndex = src[srcOff++];
                if (palIndex == 0)
                    dstOff++;
                else
                    dst[dstOff++] = palette[palIndex & 0xFF];
            }

            dstOff += dstStep;
            srcOff += srcStep;
        }
    }

    public void clip(int arg0, int arg1, int arg2, int arg3)
    {
        try
        {
            var local2 = width;
            var local5 = height;
            var local7 = 0;
            var local9 = 0;
            var local15 = (local2 << 16) / arg2;
            var local21 = (local5 << 16) / arg3;
            var local24 = cropW;
            var local27 = cropH;
            var local33 = (local24 << 16) / arg2;
            var local39 = (local27 << 16) / arg3;
            arg0 += (cropX * arg2 + local24 - 1) / local24;
            arg1 += (cropY * arg3 + local27 - 1) / local27;
            if (cropX * arg2 % local24 != 0) local7 = ((local24 - cropX * arg2 % local24) << 16) / arg2;
            if (cropY * arg3 % local27 != 0) local9 = ((local27 - cropY * arg3 % local27) << 16) / arg3;
            arg2 = arg2 * (width - (local7 >> 16)) / local24;
            arg3 = arg3 * (height - (local9 >> 16)) / local27;
            var local133 = arg0 + arg1 * Pix2D.width2d;
            var local137 = Pix2D.width2d - arg2;
            int local144;
            if (arg1 < Pix2D.boundTop)
            {
                local144 = Pix2D.boundTop - arg1;
                arg3 -= local144;
                arg1 = 0;
                local133 += local144 * Pix2D.width2d;
                local9 += local39 * local144;
            }

            if (arg1 + arg3 > Pix2D.boundBottom) arg3 -= arg1 + arg3 - Pix2D.boundBottom;
            if (arg0 < Pix2D.boundLeft)
            {
                local144 = Pix2D.boundLeft - arg0;
                arg2 -= local144;
                arg0 = 0;
                local133 += local144;
                local7 += local33 * local144;
                local137 += local144;
            }

            if (arg0 + arg2 > Pix2D.boundRight)
            {
                local144 = arg0 + arg2 - Pix2D.boundRight;
                arg2 -= local144;
                local137 += local144;
            }

            plot_scale(Pix2D.data, pixels, palette, local7, local9, local133, local137, arg2, arg3, local33, local39,
                local2);
        }
        catch (Exception local239)
        {
            Console.WriteLine("error in sprite clipping routine");
        }
    }

    private void plot_scale(int[] arg0, byte[] arg1, int[] arg2, int arg3, int arg4, int arg5, int arg6, int arg7,
        int arg8, int arg9, int arg10, int arg11)
    {
        try
        {
            var local3 = arg3;
            for (var local6 = -arg8; local6 < 0; local6++)
            {
                var local14 = (arg4 >> 16) * arg11;
                for (var local17 = -arg7; local17 < 0; local17++)
                {
                    var local27 = arg1[(arg3 >> 16) + local14];
                    if (local27 == 0)
                        arg5++;
                    else
                        arg0[arg5++] = arg2[local27 & 0xFF];
                    arg3 += arg9;
                }

                arg4 += arg10;
                arg3 = local3;
                arg5 += arg6;
            }
        }
        catch (Exception local63)
        {
            Console.WriteLine("error in plot_scale");
        }
    }
}