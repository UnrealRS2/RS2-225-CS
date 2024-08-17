﻿using RS2_225.jagex2.io;

namespace RS2_225.jagex2.graphics;

public class Pix3D : Pix2D
{
    public static bool lowDetail = true;

    public static bool hclip;

    private static bool opaque;

    public static bool jagged = true;

    public static int trans;

    public static int centerW3D;

    public static int centerH3D;

    public static int[] divTable = new int[512];

    public static int[] divTable2 = new int[2048];

    public static int[] sinTable = new int[2048];

    public static int[] cosTable = new int[2048];

    public static int[] lineOffset;

    private static int textureCount;

    public static Pix8[] textures = new Pix8[50];

    private static bool[] textureTranslucent = new bool[50];

    private static int[] averageTextureRGB = new int[50];

    private static int poolSize;

    private static int[][] texelPool;

    private static int[][] activeTexels = new int[50][];

    public static int[] textureCycle = new int[50];

    public static int cycle;

    public static int[] colourTable = new int[65536];

    private static int[][] texturePalette = new int[50][];

    static Pix3D()
    {
        for (var i = 1; i < 512; i++) divTable[i] = 32768 / i;

        for (var i = 1; i < 2048; i++) divTable2[i] = 65536 / i;

        for (var i = 0; i < 2048; i++)
        {
            sinTable[i] = (int)(Math.Sin(i * 0.0030679615D) * 65536.0D);
            cosTable[i] = (int)(Math.Cos(i * 0.0030679615D) * 65536.0D);
        }
    }

    public static void unload()
    {
        divTable = null;
        divTable2 = null; // original typo: divTable = null; (yes twice)
        sinTable = null;
        cosTable = null;
        lineOffset = null;
        textures = null;
        textureTranslucent = null;
        averageTextureRGB = null;
        texelPool = null;
        activeTexels = null;
        textureCycle = null;
        colourTable = null;
        texturePalette = null;
    }

    public static void init2D()
    {
        lineOffset = new int[height2d];
        for (var y = 0; y < height2d; y++) lineOffset[y] = width2d * y;
        centerW3D = width2d / 2;
        centerH3D = height2d / 2;
    }

    public static void init3D(int width, int height)
    {
        lineOffset = new int[height];
        for (var y = 0; y < height; y++) lineOffset[y] = width * y;
        centerW3D = width / 2;
        centerH3D = height / 2;
    }

    public static void clearTexels()
    {
        texelPool = null;
        for (var i = 0; i < 50; i++) activeTexels[i] = null;
    }

    public static void initPool(int size)
    {
        if (texelPool != null) return;
        poolSize = size;
        if (lowDetail)
        {
            texelPool = new int[poolSize][];
            for (var i = 0; i < poolSize; i++) texelPool[i] = new int[16384];
        }
        else
        {
            texelPool = new int[poolSize][];
            for (var i = 0; i < poolSize; i++) texelPool[i] = new int[65536];
        }

        for (var i = 0; i < 50; i++) activeTexels[i] = null;
    }

    public static void unpackTextures(Jagfile jag)
    {
        textureCount = 0;
        for (var id = 0; id < 50; id++)
            try
            {
                textures[id] = new Pix8(jag, $"{id}", 0);
                if (lowDetail && textures[id].cropW == 128)
                    textures[id].shrink();
                else
                    textures[id].crop();
                textureCount++;
            }
            catch (Exception ex)
            {
            }

        Console.WriteLine("Unpacked " + textureCount + " textures");
    }

    public static int getAverageTextureRGB(int id)
    {
        if (averageTextureRGB[id] != 0) return averageTextureRGB[id];

        var r = 0;
        var g = 0;
        var b = 0;
        var length = texturePalette[id].Length;
        for (var i = 0; i < length; i++)
        {
            r += (texturePalette[id][i] >> 16) & 0xFF;
            g += (texturePalette[id][i] >> 8) & 0xFF;
            b += texturePalette[id][i] & 0xFF;
        }

        var rgb = ((r / length) << 16) + ((g / length) << 8) + b / length;
        rgb = setGamma(rgb, 1.4D);
        if (rgb == 0) rgb = 1;
        averageTextureRGB[id] = rgb;
        return rgb;
    }

    public static void pushTexture(int id)
    {
        if (activeTexels[id] != null)
        {
            texelPool[poolSize++] = activeTexels[id];
            activeTexels[id] = null;
        }
    }

    private static int[] getTexels(int id)
    {
        textureCycle[id] = cycle++;
        if (activeTexels[id] != null) return activeTexels[id];

        int[] texels;
        if (poolSize > 0)
        {
            texels = texelPool[--poolSize];
            texelPool[poolSize] = null;
        }
        else
        {
            var cycle = 0;
            var selected = -1;
            for (var t = 0; t < textureCount; t++)
                if (activeTexels[t] != null && (textureCycle[t] < cycle || selected == -1))
                {
                    cycle = textureCycle[t];
                    selected = t;
                }

            texels = activeTexels[selected];
            activeTexels[selected] = null;
        }

        activeTexels[id] = texels;
        var texture = textures[id];
        var palette = texturePalette[id];

        if (lowDetail)
        {
            textureTranslucent[id] = false;
            for (var i = 0; i < 4096; i++)
            {
                var rgb = texels[i] = palette[texture.pixels[i]] & 0xF8F8FF;
                if (rgb == 0) textureTranslucent[id] = true;
                texels[i + 4096] = (rgb - (rgb >>> 3)) & 0xF8F8FF;
                texels[i + 8192] = (rgb - (rgb >>> 2)) & 0xF8F8FF;
                texels[i + 12288] = (rgb - (rgb >>> 2) - (rgb >>> 3)) & 0xF8F8FF;
            }
        }
        else
        {
            if (texture.width == 64)
                for (var y = 0; y < 128; y++)
                for (var x = 0; x < 128; x++)
                    texels[x + (y << 7)] = palette[texture.pixels[(x >> 1) + ((y >> 1) << 6)]];
            else
                for (var i = 0; i < 16384; i++)
                    texels[i] = palette[texture.pixels[i]];

            textureTranslucent[id] = false;
            for (var i = 0; i < 0x4000; i++)
            {
                texels[i] &= 0xF8F8FF;

                var rgb = texels[i];
                if (rgb == 0) textureTranslucent[id] = true;

                texels[i + 0x4000] = (rgb - (rgb >>> 3)) & 0xF8F8FF;
                texels[i + 0x8000] = (rgb - (rgb >>> 2)) & 0xF8F8FF;
                texels[i + 0xc000] = (rgb - (rgb >>> 2) - (rgb >>> 3)) & 0xF8F8FF;
            }
        }

        return texels;
    }

    public static void setBrightness(double brightness)
    {
        var randomized = brightness + java.Math.random() * 0.03D - 0.015D;

        var offset = 0;
        for (var y = 0; y < 512; y++)
        {
            var hue = y / 8 / 64.0D + 0.0078125D;
            var saturation = (y & 0x7) / 8.0D + 0.0625D;

            for (var x = 0; x < 128; x++)
            {
                var lightness = x / 128.0D;
                var r = lightness;
                var g = lightness;
                var b = lightness;

                if (saturation != 0.0D)
                {
                    double q;
                    if (lightness < 0.5D)
                        q = lightness * (saturation + 1.0D);
                    else
                        q = lightness + saturation - lightness * saturation;

                    var p = lightness * 2.0D - q;

                    var t = hue + 0.3333333333333333D;
                    if (t > 1.0D) t--;

                    var d11 = hue - 0.3333333333333333D;
                    if (d11 < 0.0D) d11++;

                    if (t * 6.0D < 1.0D)
                        r = p + (q - p) * 6.0D * t;
                    else if (t * 2.0D < 1.0D)
                        r = q;
                    else if (t * 3.0D < 2.0D)
                        r = p + (q - p) * (0.6666666666666666D - t) * 6.0D;
                    else
                        r = p;

                    if (hue * 6.0D < 1.0D)
                        g = p + (q - p) * 6.0D * hue;
                    else if (hue * 2.0D < 1.0D)
                        g = q;
                    else if (hue * 3.0D < 2.0D)
                        g = p + (q - p) * (0.6666666666666666D - hue) * 6.0D;
                    else
                        g = p;

                    if (d11 * 6.0D < 1.0D)
                        b = p + (q - p) * 6.0D * d11;
                    else if (d11 * 2.0D < 1.0D)
                        b = q;
                    else if (d11 * 3.0D < 2.0D)
                        b = p + (q - p) * (0.6666666666666666D - d11) * 6.0D;
                    else
                        b = p;
                }

                var intR = (int)(r * 256.0D);
                var intG = (int)(g * 256.0D);
                var intB = (int)(b * 256.0D);
                var rgb = (intR << 16) + (intG << 8) + intB;
                var rgbAdjusted = setGamma(rgb, randomized);
                colourTable[offset++] = rgbAdjusted;
            }
        }

        for (var id = 0; id < 50; id++)
            if (textures[id] != null)
            {
                var palette = textures[id].palette;
                texturePalette[id] = new int[palette.Length];

                for (var i = 0; i < palette.Length; i++) texturePalette[id][i] = setGamma(palette[i], randomized);
            }

        for (var id = 0; id < 50; id++) pushTexture(id);
    }

    private static int setGamma(int rgb, double gamma)
    {
        var r = ((rgb >> 16) & 0xFF) / 256.0D;
        var g = ((rgb >> 8) & 0xFF) / 256.0D;
        var b = (rgb & 0xFF) / 256.0D;

        var powR = Math.Pow(r, gamma);
        var powG = Math.Pow(g, gamma);
        var powB = Math.Pow(b, gamma);

        var intR = (int)(powR * 256.0D);
        var intG = (int)(powG * 256.0D);
        var intB = (int)(powB * 256.0D);
        return (intR << 16) + (intG << 8) + intB;
    }

    public static void gouraudTriangle(int xA, int xB, int xC, int yA, int yB, int yC, int colorA, int colorB,
        int colorC)
    {
        var dxAB = xB - xA;
        var dyAB = yB - yA;
        var dxAC = xC - xA;
        var dyAC = yC - yA;

        var xStepAB = 0;
        var colorStepAB = 0;
        if (yB != yA)
        {
            xStepAB = (dxAB << 16) / dyAB;
            colorStepAB = ((colorB - colorA) << 15) / dyAB;
        }

        var xStepBC = 0;
        var colorStepBC = 0;
        if (yC != yB)
        {
            xStepBC = ((xC - xB) << 16) / (yC - yB);
            colorStepBC = ((colorC - colorB) << 15) / (yC - yB);
        }

        var xStepAC = 0;
        var colorStepAC = 0;
        if (yC != yA)
        {
            xStepAC = ((xA - xC) << 16) / (yA - yC);
            colorStepAC = ((colorA - colorC) << 15) / (yA - yC);
        }

        // this won't change any rendering, saves not wasting time "drawing" an invalid triangle
        var triangleArea = dxAB * dyAC - dyAB * dxAC;
        if (triangleArea == 0) return;

        if (yA <= yB && yA <= yC)
        {
            if (yA < boundBottom)
            {
                if (yB > boundBottom) yB = boundBottom;

                if (yC > boundBottom) yC = boundBottom;

                if (yB < yC)
                {
                    xC = xA <<= 16;
                    colorC = colorA <<= 15;
                    if (yA < 0)
                    {
                        xC -= xStepAC * yA;
                        xA -= xStepAB * yA;
                        colorC -= colorStepAC * yA;
                        colorA -= colorStepAB * yA;
                        yA = 0;
                    }

                    xB <<= 16;
                    colorB <<= 15;
                    if (yB < 0)
                    {
                        xB -= xStepBC * yB;
                        colorB -= colorStepBC * yB;
                        yB = 0;
                    }

                    if ((yA != yB && xStepAC < xStepAB) || (yA == yB && xStepAC > xStepBC))
                    {
                        yC -= yB;
                        yB -= yA;
                        yA = lineOffset[yA];

                        while (--yB >= 0)
                        {
                            gouraudRaster(xC >> 16, xA >> 16, colorC >> 7, colorA >> 7, data, yA, 0);
                            xC += xStepAC;
                            xA += xStepAB;
                            colorC += colorStepAC;
                            colorA += colorStepAB;
                            yA += width2d;
                        }

                        while (--yC >= 0)
                        {
                            gouraudRaster(xC >> 16, xB >> 16, colorC >> 7, colorB >> 7, data, yA, 0);
                            xC += xStepAC;
                            xB += xStepBC;
                            colorC += colorStepAC;
                            colorB += colorStepBC;
                            yA += width2d;
                        }
                    }
                    else
                    {
                        yC -= yB;
                        yB -= yA;
                        yA = lineOffset[yA];

                        while (--yB >= 0)
                        {
                            gouraudRaster(xA >> 16, xC >> 16, colorA >> 7, colorC >> 7, data, yA, 0);
                            xC += xStepAC;
                            xA += xStepAB;
                            colorC += colorStepAC;
                            colorA += colorStepAB;
                            yA += width2d;
                        }

                        while (--yC >= 0)
                        {
                            gouraudRaster(xB >> 16, xC >> 16, colorB >> 7, colorC >> 7, data, yA, 0);
                            xC += xStepAC;
                            xB += xStepBC;
                            colorC += colorStepAC;
                            colorB += colorStepBC;
                            yA += width2d;
                        }
                    }
                }
                else
                {
                    xB = xA <<= 16;
                    colorB = colorA <<= 15;
                    if (yA < 0)
                    {
                        xB -= xStepAC * yA;
                        xA -= xStepAB * yA;
                        colorB -= colorStepAC * yA;
                        colorA -= colorStepAB * yA;
                        yA = 0;
                    }

                    xC <<= 16;
                    colorC <<= 15;
                    if (yC < 0)
                    {
                        xC -= xStepBC * yC;
                        colorC -= colorStepBC * yC;
                        yC = 0;
                    }

                    if ((yA != yC && xStepAC < xStepAB) || (yA == yC && xStepBC > xStepAB))
                    {
                        yB -= yC;
                        yC -= yA;
                        yA = lineOffset[yA];

                        while (--yC >= 0)
                        {
                            gouraudRaster(xB >> 16, xA >> 16, colorB >> 7, colorA >> 7, data, yA, 0);
                            xB += xStepAC;
                            xA += xStepAB;
                            colorB += colorStepAC;
                            colorA += colorStepAB;
                            yA += width2d;
                        }

                        while (--yB >= 0)
                        {
                            gouraudRaster(xC >> 16, xA >> 16, colorC >> 7, colorA >> 7, data, yA, 0);
                            xC += xStepBC;
                            xA += xStepAB;
                            colorC += colorStepBC;
                            colorA += colorStepAB;
                            yA += width2d;
                        }
                    }
                    else
                    {
                        yB -= yC;
                        yC -= yA;
                        yA = lineOffset[yA];

                        while (--yC >= 0)
                        {
                            gouraudRaster(xA >> 16, xB >> 16, colorA >> 7, colorB >> 7, data, yA, 0);
                            xB += xStepAC;
                            xA += xStepAB;
                            colorB += colorStepAC;
                            colorA += colorStepAB;
                            yA += width2d;
                        }

                        while (--yB >= 0)
                        {
                            gouraudRaster(xA >> 16, xC >> 16, colorA >> 7, colorC >> 7, data, yA, 0);
                            xC += xStepBC;
                            xA += xStepAB;
                            colorC += colorStepBC;
                            colorA += colorStepAB;
                            yA += width2d;
                        }
                    }
                }
            }
        }
        else if (yB <= yC)
        {
            if (yB < boundBottom)
            {
                if (yC > boundBottom) yC = boundBottom;

                if (yA > boundBottom) yA = boundBottom;

                if (yC < yA)
                {
                    xA = xB <<= 16;
                    colorA = colorB <<= 15;
                    if (yB < 0)
                    {
                        xA -= xStepAB * yB;
                        xB -= xStepBC * yB;
                        colorA -= colorStepAB * yB;
                        colorB -= colorStepBC * yB;
                        yB = 0;
                    }

                    xC <<= 16;
                    colorC <<= 15;
                    if (yC < 0)
                    {
                        xC -= xStepAC * yC;
                        colorC -= colorStepAC * yC;
                        yC = 0;
                    }

                    if ((yB != yC && xStepAB < xStepBC) || (yB == yC && xStepAB > xStepAC))
                    {
                        yA -= yC;
                        yC -= yB;
                        yB = lineOffset[yB];

                        while (--yC >= 0)
                        {
                            gouraudRaster(xA >> 16, xB >> 16, colorA >> 7, colorB >> 7, data, yB, 0);
                            xA += xStepAB;
                            xB += xStepBC;
                            colorA += colorStepAB;
                            colorB += colorStepBC;
                            yB += width2d;
                        }

                        while (--yA >= 0)
                        {
                            gouraudRaster(xA >> 16, xC >> 16, colorA >> 7, colorC >> 7, data, yB, 0);
                            xA += xStepAB;
                            xC += xStepAC;
                            colorA += colorStepAB;
                            colorC += colorStepAC;
                            yB += width2d;
                        }
                    }
                    else
                    {
                        yA -= yC;
                        yC -= yB;
                        yB = lineOffset[yB];

                        while (--yC >= 0)
                        {
                            gouraudRaster(xB >> 16, xA >> 16, colorB >> 7, colorA >> 7, data, yB, 0);
                            xA += xStepAB;
                            xB += xStepBC;
                            colorA += colorStepAB;
                            colorB += colorStepBC;
                            yB += width2d;
                        }

                        while (--yA >= 0)
                        {
                            gouraudRaster(xC >> 16, xA >> 16, colorC >> 7, colorA >> 7, data, yB, 0);
                            xA += xStepAB;
                            xC += xStepAC;
                            colorA += colorStepAB;
                            colorC += colorStepAC;
                            yB += width2d;
                        }
                    }
                }
                else
                {
                    xC = xB <<= 16;
                    colorC = colorB <<= 15;
                    if (yB < 0)
                    {
                        xC -= xStepAB * yB;
                        xB -= xStepBC * yB;
                        colorC -= colorStepAB * yB;
                        colorB -= colorStepBC * yB;
                        yB = 0;
                    }

                    xA <<= 16;
                    colorA <<= 15;
                    if (yA < 0)
                    {
                        xA -= xStepAC * yA;
                        colorA -= colorStepAC * yA;
                        yA = 0;
                    }

                    if (xStepAB < xStepBC)
                    {
                        yC -= yA;
                        yA -= yB;
                        yB = lineOffset[yB];

                        while (--yA >= 0)
                        {
                            gouraudRaster(xC >> 16, xB >> 16, colorC >> 7, colorB >> 7, data, yB, 0);
                            xC += xStepAB;
                            xB += xStepBC;
                            colorC += colorStepAB;
                            colorB += colorStepBC;
                            yB += width2d;
                        }

                        while (--yC >= 0)
                        {
                            gouraudRaster(xA >> 16, xB >> 16, colorA >> 7, colorB >> 7, data, yB, 0);
                            xA += xStepAC;
                            xB += xStepBC;
                            colorA += colorStepAC;
                            colorB += colorStepBC;
                            yB += width2d;
                        }
                    }
                    else
                    {
                        yC -= yA;
                        yA -= yB;
                        yB = lineOffset[yB];

                        while (--yA >= 0)
                        {
                            gouraudRaster(xB >> 16, xC >> 16, colorB >> 7, colorC >> 7, data, yB, 0);
                            xC += xStepAB;
                            xB += xStepBC;
                            colorC += colorStepAB;
                            colorB += colorStepBC;
                            yB += width2d;
                        }

                        while (--yC >= 0)
                        {
                            gouraudRaster(xB >> 16, xA >> 16, colorB >> 7, colorA >> 7, data, yB, 0);
                            xA += xStepAC;
                            xB += xStepBC;
                            colorA += colorStepAC;
                            colorB += colorStepBC;
                            yB += width2d;
                        }
                    }
                }
            }
        }
        else if (yC < boundBottom)
        {
            if (yA > boundBottom) yA = boundBottom;

            if (yB > boundBottom) yB = boundBottom;

            if (yA < yB)
            {
                xB = xC <<= 16;
                colorB = colorC <<= 15;
                if (yC < 0)
                {
                    xB -= xStepBC * yC;
                    xC -= xStepAC * yC;
                    colorB -= colorStepBC * yC;
                    colorC -= colorStepAC * yC;
                    yC = 0;
                }

                xA <<= 16;
                colorA <<= 15;
                if (yA < 0)
                {
                    xA -= xStepAB * yA;
                    colorA -= colorStepAB * yA;
                    yA = 0;
                }

                if (xStepBC < xStepAC)
                {
                    yB -= yA;
                    yA -= yC;
                    yC = lineOffset[yC];

                    while (--yA >= 0)
                    {
                        gouraudRaster(xB >> 16, xC >> 16, colorB >> 7, colorC >> 7, data, yC, 0);
                        xB += xStepBC;
                        xC += xStepAC;
                        colorB += colorStepBC;
                        colorC += colorStepAC;
                        yC += width2d;
                    }

                    while (--yB >= 0)
                    {
                        gouraudRaster(xB >> 16, xA >> 16, colorB >> 7, colorA >> 7, data, yC, 0);
                        xB += xStepBC;
                        xA += xStepAB;
                        colorB += colorStepBC;
                        colorA += colorStepAB;
                        yC += width2d;
                    }
                }
                else
                {
                    yB -= yA;
                    yA -= yC;
                    yC = lineOffset[yC];

                    while (--yA >= 0)
                    {
                        gouraudRaster(xC >> 16, xB >> 16, colorC >> 7, colorB >> 7, data, yC, 0);
                        xB += xStepBC;
                        xC += xStepAC;
                        colorB += colorStepBC;
                        colorC += colorStepAC;
                        yC += width2d;
                    }

                    while (--yB >= 0)
                    {
                        gouraudRaster(xA >> 16, xB >> 16, colorA >> 7, colorB >> 7, data, yC, 0);
                        xB += xStepBC;
                        xA += xStepAB;
                        colorB += colorStepBC;
                        colorA += colorStepAB;
                        yC += width2d;
                    }
                }
            }
            else
            {
                xA = xC <<= 16;
                colorA = colorC <<= 15;
                if (yC < 0)
                {
                    xA -= xStepBC * yC;
                    xC -= xStepAC * yC;
                    colorA -= colorStepBC * yC;
                    colorC -= colorStepAC * yC;
                    yC = 0;
                }

                xB <<= 16;
                colorB <<= 15;
                if (yB < 0)
                {
                    xB -= xStepAB * yB;
                    colorB -= colorStepAB * yB;
                    yB = 0;
                }

                if (xStepBC < xStepAC)
                {
                    yA -= yB;
                    yB -= yC;
                    yC = lineOffset[yC];

                    while (--yB >= 0)
                    {
                        gouraudRaster(xA >> 16, xC >> 16, colorA >> 7, colorC >> 7, data, yC, 0);
                        xA += xStepBC;
                        xC += xStepAC;
                        colorA += colorStepBC;
                        colorC += colorStepAC;
                        yC += width2d;
                    }

                    while (--yA >= 0)
                    {
                        gouraudRaster(xB >> 16, xC >> 16, colorB >> 7, colorC >> 7, data, yC, 0);
                        xB += xStepAB;
                        xC += xStepAC;
                        colorB += colorStepAB;
                        colorC += colorStepAC;
                        yC += width2d;
                    }
                }
                else
                {
                    yA -= yB;
                    yB -= yC;
                    yC = lineOffset[yC];

                    while (--yB >= 0)
                    {
                        gouraudRaster(xC >> 16, xA >> 16, colorC >> 7, colorA >> 7, data, yC, 0);
                        xA += xStepBC;
                        xC += xStepAC;
                        colorA += colorStepBC;
                        colorC += colorStepAC;
                        yC += width2d;
                    }

                    while (--yA >= 0)
                    {
                        gouraudRaster(xC >> 16, xB >> 16, colorC >> 7, colorB >> 7, data, yC, 0);
                        xB += xStepAB;
                        xC += xStepAC;
                        colorB += colorStepAB;
                        colorC += colorStepAC;
                        yC += width2d;
                    }
                }
            }
        }
    }

    private static void gouraudRaster(int x0, int x1, int color0, int color1, int[] dst, int offset, int length)
    {
        int rgb;

        if (jagged)
        {
            int colorStep;

            if (hclip)
            {
                if (x1 - x0 > 3)
                    colorStep = (color1 - color0) / (x1 - x0);
                else
                    colorStep = 0;

                if (x1 > safeWidth) x1 = safeWidth;

                if (x0 < 0)
                {
                    color0 -= x0 * colorStep;
                    x0 = 0;
                }

                if (x0 >= x1) return;

                offset += x0;
                length = (x1 - x0) >> 2;
                colorStep <<= 2;
            }
            else if (x0 < x1)
            {
                offset += x0;
                length = (x1 - x0) >> 2;

                if (length > 0)
                    colorStep = ((color1 - color0) * divTable[length]) >> 15;
                else
                    colorStep = 0;
            }
            else
            {
                return;
            }

            if (trans == 0)
            {
                while (--length >= 0)
                {
                    rgb = colourTable[color0 >> 8];
                    color0 += colorStep;

                    dst[offset++] = rgb;
                    dst[offset++] = rgb;
                    dst[offset++] = rgb;
                    dst[offset++] = rgb;
                }

                length = (x1 - x0) & 0x3;
                if (length > 0)
                {
                    rgb = colourTable[color0 >> 8];

                    while (--length >= 0) dst[offset++] = rgb;
                }
            }
            else
            {
                var alpha = trans;
                var invAlpha = 256 - trans;

                while (--length >= 0)
                {
                    rgb = colourTable[color0 >> 8];
                    color0 += colorStep;

                    rgb = ((((rgb & 0xFF00FF) * invAlpha) >> 8) & 0xFF00FF) +
                          ((((rgb & 0xFF00) * invAlpha) >> 8) & 0xFF00);
                    dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                    ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                    dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                    ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                    dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                    ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                    dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                    ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                }

                length = (x1 - x0) & 0x3;
                if (length > 0)
                {
                    rgb = colourTable[color0 >> 8];
                    rgb = ((((rgb & 0xFF00FF) * invAlpha) >> 8) & 0xFF00FF) +
                          ((((rgb & 0xFF00) * invAlpha) >> 8) & 0xFF00);

                    while (--length >= 0)
                        dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                        ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                }
            }
        }
        else if (x0 < x1)
        {
            var colorStep = (color1 - color0) / (x1 - x0);

            if (hclip)
            {
                if (x1 > safeWidth) x1 = safeWidth;

                if (x0 < 0)
                {
                    color0 -= x0 * colorStep;
                    x0 = 0;
                }

                if (x0 >= x1) return;
            }

            offset += x0;
            length = x1 - x0;

            if (trans == 0)
            {
                while (--length >= 0)
                {
                    dst[offset++] = colourTable[color0 >> 8];
                    color0 += colorStep;
                }
            }
            else
            {
                var alpha = trans;
                var invAlpha = 256 - trans;

                while (--length >= 0)
                {
                    rgb = colourTable[color0 >> 8];
                    color0 += colorStep;

                    rgb = ((((rgb & 0xFF00FF) * invAlpha) >> 8) & 0xFF00FF) +
                          ((((rgb & 0xFF00) * invAlpha) >> 8) & 0xFF00);
                    dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                    ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                }
            }
        }
    }

    public static void flatTriangle(int xA, int xB, int xC, int yA, int yB, int yC, int color)
    {
        var dxAB = xB - xA;
        var dyAB = yB - yA;
        var dxAC = xC - xA;
        var dyAC = yC - yA;

        var xStepAB = 0;
        if (yB != yA) xStepAB = (dxAB << 16) / dyAB;

        var xStepBC = 0;
        if (yC != yB) xStepBC = ((xC - xB) << 16) / (yC - yB);

        var xStepAC = 0;
        if (yC != yA) xStepAC = ((xA - xC) << 16) / (yA - yC);

        // this won't change any rendering, saves not wasting time "drawing" an invalid triangle
        var triangleArea = dxAB * dyAC - dyAB * dxAC;
        if (triangleArea == 0) return;

        if (yA <= yB && yA <= yC)
        {
            if (yA < boundBottom)
            {
                if (yB > boundBottom) yB = boundBottom;

                if (yC > boundBottom) yC = boundBottom;

                if (yB < yC)
                {
                    xC = xA <<= 16;
                    if (yA < 0)
                    {
                        xC -= xStepAC * yA;
                        xA -= xStepAB * yA;
                        yA = 0;
                    }

                    xB <<= 16;
                    if (yB < 0)
                    {
                        xB -= xStepBC * yB;
                        yB = 0;
                    }

                    if ((yA != yB && xStepAC < xStepAB) || (yA == yB && xStepAC > xStepBC))
                    {
                        yC -= yB;
                        yB -= yA;
                        yA = lineOffset[yA];

                        while (--yB >= 0)
                        {
                            flatRaster(xC >> 16, xA >> 16, data, yA, color);
                            xC += xStepAC;
                            xA += xStepAB;
                            yA += width2d;
                        }

                        while (--yC >= 0.0F)
                        {
                            flatRaster(xC >> 16, xB >> 16, data, yA, color);
                            xC += xStepAC;
                            xB += xStepBC;
                            yA += width2d;
                        }
                    }
                    else
                    {
                        yC -= yB;
                        yB -= yA;
                        yA = lineOffset[yA];

                        while (--yB >= 0)
                        {
                            flatRaster(xA >> 16, xC >> 16, data, yA, color);
                            xC += xStepAC;
                            xA += xStepAB;
                            yA += width2d;
                        }

                        while (--yC >= 0)
                        {
                            flatRaster(xB >> 16, xC >> 16, data, yA, color);
                            xC += xStepAC;
                            xB += xStepBC;
                            yA += width2d;
                        }
                    }
                }
                else
                {
                    xB = xA <<= 16;
                    if (yA < 0)
                    {
                        xB -= xStepAC * yA;
                        xA -= xStepAB * yA;
                        yA = 0;
                    }

                    xC <<= 16;
                    if (yC < 0)
                    {
                        xC -= xStepBC * yC;
                        yC = 0;
                    }

                    if ((yA != yC && xStepAC < xStepAB) || (yA == yC && xStepBC > xStepAB))
                    {
                        yB -= yC;
                        yC -= yA;
                        yA = lineOffset[yA];

                        while (--yC >= 0)
                        {
                            flatRaster(xB >> 16, xA >> 16, data, yA, color);
                            xB += xStepAC;
                            xA += xStepAB;
                            yA += width2d;
                        }

                        while (--yB >= 0)
                        {
                            flatRaster(xC >> 16, xA >> 16, data, yA, color);
                            xC += xStepBC;
                            xA += xStepAB;
                            yA += width2d;
                        }
                    }
                    else
                    {
                        yB -= yC;
                        yC -= yA;
                        yA = lineOffset[yA];

                        while (--yC >= 0)
                        {
                            flatRaster(xA >> 16, xB >> 16, data, yA, color);
                            xB += xStepAC;
                            xA += xStepAB;
                            yA += width2d;
                        }

                        while (--yB >= 0)
                        {
                            flatRaster(xA >> 16, xC >> 16, data, yA, color);
                            xC += xStepBC;
                            xA += xStepAB;
                            yA += width2d;
                        }
                    }
                }
            }
        }
        else if (yB <= yC)
        {
            if (yB < boundBottom)
            {
                if (yC > boundBottom) yC = boundBottom;

                if (yA > boundBottom) yA = boundBottom;

                if (yC < yA)
                {
                    xA = xB <<= 16;
                    if (yB < 0)
                    {
                        xA -= xStepAB * yB;
                        xB -= xStepBC * yB;
                        yB = 0;
                    }

                    xC <<= 16;
                    if (yC < 0)
                    {
                        xC -= xStepAC * yC;
                        yC = 0;
                    }

                    if ((yB != yC && xStepAB < xStepBC) || (yB == yC && xStepAB > xStepAC))
                    {
                        yA -= yC;
                        yC -= yB;
                        yB = lineOffset[yB];

                        while (--yC >= 0)
                        {
                            flatRaster(xA >> 16, xB >> 16, data, yB, color);
                            xA += xStepAB;
                            xB += xStepBC;
                            yB += width2d;
                        }

                        while (--yA >= 0)
                        {
                            flatRaster(xA >> 16, xC >> 16, data, yB, color);
                            xA += xStepAB;
                            xC += xStepAC;
                            yB += width2d;
                        }
                    }
                    else
                    {
                        yA -= yC;
                        yC -= yB;
                        yB = lineOffset[yB];

                        while (--yC >= 0)
                        {
                            flatRaster(xB >> 16, xA >> 16, data, yB, color);
                            xA += xStepAB;
                            xB += xStepBC;
                            yB += width2d;
                        }

                        while (--yA >= 0)
                        {
                            flatRaster(xC >> 16, xA >> 16, data, yB, color);
                            xA += xStepAB;
                            xC += xStepAC;
                            yB += width2d;
                        }
                    }
                }
                else
                {
                    xC = xB <<= 16;
                    if (yB < 0)
                    {
                        xC -= xStepAB * yB;
                        xB -= xStepBC * yB;
                        yB = 0;
                    }

                    xA <<= 16;
                    if (yA < 0)
                    {
                        xA -= xStepAC * yA;
                        yA = 0;
                    }

                    if (xStepAB < xStepBC)
                    {
                        yC -= yA;
                        yA -= yB;
                        yB = lineOffset[yB];

                        while (--yA >= 0)
                        {
                            flatRaster(xC >> 16, xB >> 16, data, yB, color);
                            xC += xStepAB;
                            xB += xStepBC;
                            yB += width2d;
                        }

                        while (--yC >= 0)
                        {
                            flatRaster(xA >> 16, xB >> 16, data, yB, color);
                            xA += xStepAC;
                            xB += xStepBC;
                            yB += width2d;
                        }
                    }
                    else
                    {
                        yC -= yA;
                        yA -= yB;
                        yB = lineOffset[yB];

                        while (--yA >= 0)
                        {
                            flatRaster(xB >> 16, xC >> 16, data, yB, color);
                            xC += xStepAB;
                            xB += xStepBC;
                            yB += width2d;
                        }

                        while (--yC >= 0)
                        {
                            flatRaster(xB >> 16, xA >> 16, data, yB, color);
                            xA += xStepAC;
                            xB += xStepBC;
                            yB += width2d;
                        }
                    }
                }
            }
        }
        else if (yC < boundBottom)
        {
            if (yA > boundBottom) yA = boundBottom;

            if (yB > boundBottom) yB = boundBottom;

            if (yA < yB)
            {
                xB = xC <<= 16;
                if (yC < 0)
                {
                    xB -= xStepBC * yC;
                    xC -= xStepAC * yC;
                    yC = 0;
                }

                xA <<= 16;
                if (yA < 0)
                {
                    xA -= xStepAB * yA;
                    yA = 0;
                }

                if (xStepBC < xStepAC)
                {
                    yB -= yA;
                    yA -= yC;
                    yC = lineOffset[yC];

                    while (--yA >= 0)
                    {
                        flatRaster(xB >> 16, xC >> 16, data, yC, color);
                        xB += xStepBC;
                        xC += xStepAC;
                        yC += width2d;
                    }

                    while (--yB >= 0)
                    {
                        flatRaster(xB >> 16, xA >> 16, data, yC, color);
                        xB += xStepBC;
                        xA += xStepAB;
                        yC += width2d;
                    }
                }
                else
                {
                    yB -= yA;
                    yA -= yC;
                    yC = lineOffset[yC];

                    while (--yA >= 0)
                    {
                        flatRaster(xC >> 16, xB >> 16, data, yC, color);
                        xB += xStepBC;
                        xC += xStepAC;
                        yC += width2d;
                    }

                    while (--yB >= 0)
                    {
                        flatRaster(xA >> 16, xB >> 16, data, yC, color);
                        xB += xStepBC;
                        xA += xStepAB;
                        yC += width2d;
                    }
                }
            }
            else
            {
                xA = xC <<= 16;
                if (yC < 0)
                {
                    xA -= xStepBC * yC;
                    xC -= xStepAC * yC;
                    yC = 0;
                }

                xB <<= 16;
                if (yB < 0)
                {
                    xB -= xStepAB * yB;
                    yB = 0;
                }

                if (xStepBC < xStepAC)
                {
                    yA -= yB;
                    yB -= yC;
                    yC = lineOffset[yC];

                    while (--yB >= 0)
                    {
                        flatRaster(xA >> 16, xC >> 16, data, yC, color);
                        xA += xStepBC;
                        xC += xStepAC;
                        yC += width2d;
                    }

                    while (--yA >= 0)
                    {
                        flatRaster(xB >> 16, xC >> 16, data, yC, color);
                        xB += xStepAB;
                        xC += xStepAC;
                        yC += width2d;
                    }
                }
                else
                {
                    yA -= yB;
                    yB -= yC;
                    yC = lineOffset[yC];

                    while (--yB >= 0)
                    {
                        flatRaster(xC >> 16, xA >> 16, data, yC, color);
                        xA += xStepBC;
                        xC += xStepAC;
                        yC += width2d;
                    }

                    while (--yA >= 0)
                    {
                        flatRaster(xC >> 16, xB >> 16, data, yC, color);
                        xB += xStepAB;
                        xC += xStepAC;
                        yC += width2d;
                    }
                }
            }
        }
    }

    private static void flatRaster(int x0, int x1, int[] dst, int offset, int rgb)
    {
        if (hclip)
        {
            if (x1 > safeWidth) x1 = safeWidth;

            if (x0 < 0) x0 = 0;
        }

        if (x0 >= x1) return;

        offset += x0;
        var length = (x1 - x0) >> 2;

        if (trans == 0)
        {
            while (--length >= 0)
            {
                dst[offset++] = rgb;
                dst[offset++] = rgb;
                dst[offset++] = rgb;
                dst[offset++] = rgb;
            }

            length = (x1 - x0) & 0x3;
            while (--length >= 0) dst[offset++] = rgb;
        }
        else
        {
            var alpha = trans;
            var invAlpha = 256 - trans;
            rgb = ((((rgb & 0xFF00FF) * invAlpha) >> 8) & 0xFF00FF) + ((((rgb & 0xFF00) * invAlpha) >> 8) & 0xFF00);

            while (--length >= 0)
            {
                dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
                dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
            }

            length = (x1 - x0) & 0x3;
            while (--length >= 0)
                dst[offset++] = rgb + ((((dst[offset] & 0xFF00FF) * alpha) >> 8) & 0xFF00FF) +
                                ((((dst[offset] & 0xFF00) * alpha) >> 8) & 0xFF00);
        }
    }

    public static void textureTriangle(int xA, int xB, int xC, int yA, int yB, int yC, int shadeA, int shadeB,
        int shadeC, int originX, int originY, int originZ, int txB, int txC, int tyB, int tyC, int tzB, int tzC,
        int texture)
    {
        var texels = getTexels(texture);
        opaque = !textureTranslucent[texture];

        var verticalX = originX - txB;
        var verticalY = originY - tyB;
        var verticalZ = originZ - tzB;

        var horizontalX = txC - originX;
        var horizontalY = tyC - originY;
        var horizontalZ = tzC - originZ;

        var u = (horizontalX * originY - horizontalY * originX) << 14;
        var uStride = (horizontalY * originZ - horizontalZ * originY) << 8;
        var uStepVertical = (horizontalZ * originX - horizontalX * originZ) << 5;

        var v = (verticalX * originY - verticalY * originX) << 14;
        var vStride = (verticalY * originZ - verticalZ * originY) << 8;
        var vStepVertical = (verticalZ * originX - verticalX * originZ) << 5;

        var w = (verticalY * horizontalX - verticalX * horizontalY) << 14;
        var wStride = (verticalZ * horizontalY - verticalY * horizontalZ) << 8;
        var wStepVertical = (verticalX * horizontalZ - verticalZ * horizontalX) << 5;

        var dxAB = xB - xA;
        var dyAB = yB - yA;
        var dxAC = xC - xA;
        var dyAC = yC - yA;

        var xStepAB = 0;
        var shadeStepAB = 0;
        if (yB != yA)
        {
            xStepAB = (dxAB << 16) / dyAB;
            shadeStepAB = ((shadeB - shadeA) << 16) / dyAB;
        }

        var xStepBC = 0;
        var shadeStepBC = 0;
        if (yC != yB)
        {
            xStepBC = ((xC - xB) << 16) / (yC - yB);
            shadeStepBC = ((shadeC - shadeB) << 16) / (yC - yB);
        }

        var xStepAC = 0;
        var shadeStepAC = 0;
        if (yC != yA)
        {
            xStepAC = ((xA - xC) << 16) / (yA - yC);
            shadeStepAC = ((shadeA - shadeC) << 16) / (yA - yC);
        }

        // this won't change any rendering, saves not wasting time "drawing" an invalid triangle
        var triangleArea = dxAB * dyAC - dyAB * dxAC;
        if (triangleArea == 0) return;

        if (yA <= yB && yA <= yC)
        {
            if (yA < boundBottom)
            {
                if (yB > boundBottom) yB = boundBottom;

                if (yC > boundBottom) yC = boundBottom;

                if (yB < yC)
                {
                    xC = xA <<= 16;
                    shadeC = shadeA <<= 16;
                    if (yA < 0)
                    {
                        xC -= xStepAC * yA;
                        xA -= xStepAB * yA;
                        shadeC -= shadeStepAC * yA;
                        shadeA -= shadeStepAB * yA;
                        yA = 0;
                    }

                    xB <<= 16;
                    shadeB <<= 16;
                    if (yB < 0)
                    {
                        xB -= xStepBC * yB;
                        shadeB -= shadeStepBC * yB;
                        yB = 0;
                    }

                    var dy = yA - centerH3D;
                    u += uStepVertical * dy;
                    v += vStepVertical * dy;
                    w += wStepVertical * dy;

                    if ((yA != yB && xStepAC < xStepAB) || (yA == yB && xStepAC > xStepBC))
                    {
                        yC -= yB;
                        yB -= yA;
                        yA = lineOffset[yA];

                        while (--yB >= 0)
                        {
                            textureRaster(xC >> 16, xA >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeC >> 8, shadeA >> 8);
                            xC += xStepAC;
                            xA += xStepAB;
                            shadeC += shadeStepAC;
                            shadeA += shadeStepAB;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yC >= 0)
                        {
                            textureRaster(xC >> 16, xB >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeC >> 8, shadeB >> 8);
                            xC += xStepAC;
                            xB += xStepBC;
                            shadeC += shadeStepAC;
                            shadeB += shadeStepBC;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                    else
                    {
                        yC -= yB;
                        yB -= yA;
                        yA = lineOffset[yA];

                        while (--yB >= 0)
                        {
                            textureRaster(xA >> 16, xC >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeA >> 8, shadeC >> 8);
                            xC += xStepAC;
                            xA += xStepAB;
                            shadeC += shadeStepAC;
                            shadeA += shadeStepAB;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yC >= 0)
                        {
                            textureRaster(xB >> 16, xC >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeB >> 8, shadeC >> 8);
                            xC += xStepAC;
                            xB += xStepBC;
                            shadeC += shadeStepAC;
                            shadeB += shadeStepBC;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                }
                else
                {
                    xB = xA <<= 16;
                    shadeB = shadeA <<= 16;
                    if (yA < 0)
                    {
                        xB -= xStepAC * yA;
                        xA -= xStepAB * yA;
                        shadeB -= shadeStepAC * yA;
                        shadeA -= shadeStepAB * yA;
                        yA = 0;
                    }

                    xC <<= 16;
                    shadeC <<= 16;
                    if (yC < 0)
                    {
                        xC -= xStepBC * yC;
                        shadeC -= shadeStepBC * yC;
                        yC = 0;
                    }

                    var dy = yA - centerH3D;
                    u += uStepVertical * dy;
                    v += vStepVertical * dy;
                    w += wStepVertical * dy;

                    if ((yA == yC || xStepAC >= xStepAB) && (yA != yC || xStepBC <= xStepAB))
                    {
                        yB -= yC;
                        yC -= yA;
                        yA = lineOffset[yA];

                        while (--yC >= 0)
                        {
                            textureRaster(xA >> 16, xB >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeA >> 8, shadeB >> 8);
                            xB += xStepAC;
                            xA += xStepAB;
                            shadeB += shadeStepAC;
                            shadeA += shadeStepAB;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yB >= 0)
                        {
                            textureRaster(xA >> 16, xC >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeA >> 8, shadeC >> 8);
                            xC += xStepBC;
                            xA += xStepAB;
                            shadeC += shadeStepBC;
                            shadeA += shadeStepAB;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                    else
                    {
                        yB -= yC;
                        yC -= yA;
                        yA = lineOffset[yA];

                        while (--yC >= 0)
                        {
                            textureRaster(xB >> 16, xA >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeB >> 8, shadeA >> 8);
                            xB += xStepAC;
                            xA += xStepAB;
                            shadeB += shadeStepAC;
                            shadeA += shadeStepAB;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yB >= 0)
                        {
                            textureRaster(xC >> 16, xA >> 16, data, yA, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeC >> 8, shadeA >> 8);
                            xC += xStepBC;
                            xA += xStepAB;
                            shadeC += shadeStepBC;
                            shadeA += shadeStepAB;
                            yA += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                }
            }
        }
        else if (yB <= yC)
        {
            if (yB < boundBottom)
            {
                if (yC > boundBottom) yC = boundBottom;

                if (yA > boundBottom) yA = boundBottom;

                if (yC < yA)
                {
                    xA = xB <<= 16;
                    shadeA = shadeB <<= 16;
                    if (yB < 0)
                    {
                        xA -= xStepAB * yB;
                        xB -= xStepBC * yB;
                        shadeA -= shadeStepAB * yB;
                        shadeB -= shadeStepBC * yB;
                        yB = 0;
                    }

                    xC <<= 16;
                    shadeC <<= 16;
                    if (yC < 0)
                    {
                        xC -= xStepAC * yC;
                        shadeC -= shadeStepAC * yC;
                        yC = 0;
                    }

                    var dy = yB - centerH3D;
                    u += uStepVertical * dy;
                    v += vStepVertical * dy;
                    w += wStepVertical * dy;

                    if ((yB != yC && xStepAB < xStepBC) || (yB == yC && xStepAB > xStepAC))
                    {
                        yA -= yC;
                        yC -= yB;
                        yB = lineOffset[yB];

                        while (--yC >= 0)
                        {
                            textureRaster(xA >> 16, xB >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeA >> 8, shadeB >> 8);
                            xA += xStepAB;
                            xB += xStepBC;
                            shadeA += shadeStepAB;
                            shadeB += shadeStepBC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yA >= 0)
                        {
                            textureRaster(xA >> 16, xC >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeA >> 8, shadeC >> 8);
                            xA += xStepAB;
                            xC += xStepAC;
                            shadeA += shadeStepAB;
                            shadeC += shadeStepAC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                    else
                    {
                        yA -= yC;
                        yC -= yB;
                        yB = lineOffset[yB];

                        while (--yC >= 0)
                        {
                            textureRaster(xB >> 16, xA >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeB >> 8, shadeA >> 8);
                            xA += xStepAB;
                            xB += xStepBC;
                            shadeA += shadeStepAB;
                            shadeB += shadeStepBC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yA >= 0)
                        {
                            textureRaster(xC >> 16, xA >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeC >> 8, shadeA >> 8);
                            xA += xStepAB;
                            xC += xStepAC;
                            shadeA += shadeStepAB;
                            shadeC += shadeStepAC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                }
                else
                {
                    xC = xB <<= 16;
                    shadeC = shadeB <<= 16;
                    if (yB < 0)
                    {
                        xC -= xStepAB * yB;
                        xB -= xStepBC * yB;
                        shadeC -= shadeStepAB * yB;
                        shadeB -= shadeStepBC * yB;
                        yB = 0;
                    }

                    xA <<= 16;
                    shadeA <<= 16;
                    if (yA < 0)
                    {
                        xA -= xStepAC * yA;
                        shadeA -= shadeStepAC * yA;
                        yA = 0;
                    }

                    var dy = yB - centerH3D;
                    u += uStepVertical * dy;
                    v += vStepVertical * dy;
                    w += wStepVertical * dy;

                    if (xStepAB < xStepBC)
                    {
                        yC -= yA;
                        yA -= yB;
                        yB = lineOffset[yB];

                        while (--yA >= 0)
                        {
                            textureRaster(xC >> 16, xB >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeC >> 8, shadeB >> 8);
                            xC += xStepAB;
                            xB += xStepBC;
                            shadeC += shadeStepAB;
                            shadeB += shadeStepBC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yC >= 0)
                        {
                            textureRaster(xA >> 16, xB >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeA >> 8, shadeB >> 8);
                            xA += xStepAC;
                            xB += xStepBC;
                            shadeA += shadeStepAC;
                            shadeB += shadeStepBC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                    else
                    {
                        yC -= yA;
                        yA -= yB;
                        yB = lineOffset[yB];

                        while (--yA >= 0)
                        {
                            textureRaster(xB >> 16, xC >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeB >> 8, shadeC >> 8);
                            xC += xStepAB;
                            xB += xStepBC;
                            shadeC += shadeStepAB;
                            shadeB += shadeStepBC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }

                        while (--yC >= 0)
                        {
                            textureRaster(xB >> 16, xA >> 16, data, yB, texels, 0, 0, u, v, w, uStride, vStride,
                                wStride, shadeB >> 8, shadeA >> 8);
                            xA += xStepAC;
                            xB += xStepBC;
                            shadeA += shadeStepAC;
                            shadeB += shadeStepBC;
                            yB += width2d;
                            u += uStepVertical;
                            v += vStepVertical;
                            w += wStepVertical;
                        }
                    }
                }
            }
        }
        else if (yC < boundBottom)
        {
            if (yA > boundBottom) yA = boundBottom;

            if (yB > boundBottom) yB = boundBottom;

            if (yA < yB)
            {
                xB = xC <<= 16;
                shadeB = shadeC <<= 16;
                if (yC < 0)
                {
                    xB -= xStepBC * yC;
                    xC -= xStepAC * yC;
                    shadeB -= shadeStepBC * yC;
                    shadeC -= shadeStepAC * yC;
                    yC = 0;
                }

                xA <<= 16;
                shadeA <<= 16;
                if (yA < 0)
                {
                    xA -= xStepAB * yA;
                    shadeA -= shadeStepAB * yA;
                    yA = 0;
                }

                var dy = yC - centerH3D;
                u += uStepVertical * dy;
                v += vStepVertical * dy;
                w += wStepVertical * dy;

                if (xStepBC < xStepAC)
                {
                    yB -= yA;
                    yA -= yC;
                    yC = lineOffset[yC];

                    while (--yA >= 0)
                    {
                        textureRaster(xB >> 16, xC >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeB >> 8, shadeC >> 8);
                        xB += xStepBC;
                        xC += xStepAC;
                        shadeB += shadeStepBC;
                        shadeC += shadeStepAC;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }

                    while (--yB >= 0)
                    {
                        textureRaster(xB >> 16, xA >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeB >> 8, shadeA >> 8);
                        xB += xStepBC;
                        xA += xStepAB;
                        shadeB += shadeStepBC;
                        shadeA += shadeStepAB;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }
                }
                else
                {
                    yB -= yA;
                    yA -= yC;
                    yC = lineOffset[yC];

                    while (--yA >= 0)
                    {
                        textureRaster(xC >> 16, xB >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeC >> 8, shadeB >> 8);
                        xB += xStepBC;
                        xC += xStepAC;
                        shadeB += shadeStepBC;
                        shadeC += shadeStepAC;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }

                    while (--yB >= 0)
                    {
                        textureRaster(xA >> 16, xB >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeA >> 8, shadeB >> 8);
                        xB += xStepBC;
                        xA += xStepAB;
                        shadeB += shadeStepBC;
                        shadeA += shadeStepAB;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }
                }
            }
            else
            {
                xA = xC <<= 16;
                shadeA = shadeC <<= 16;
                if (yC < 0)
                {
                    xA -= xStepBC * yC;
                    xC -= xStepAC * yC;
                    shadeA -= shadeStepBC * yC;
                    shadeC -= shadeStepAC * yC;
                    yC = 0;
                }

                xB <<= 16;
                shadeB <<= 16;
                if (yB < 0)
                {
                    xB -= xStepAB * yB;
                    shadeB -= shadeStepAB * yB;
                    yB = 0;
                }

                var dy = yC - centerH3D;
                u += uStepVertical * dy;
                v += vStepVertical * dy;
                w += wStepVertical * dy;

                if (xStepBC < xStepAC)
                {
                    yA -= yB;
                    yB -= yC;
                    yC = lineOffset[yC];

                    while (--yB >= 0)
                    {
                        textureRaster(xA >> 16, xC >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeA >> 8, shadeC >> 8);
                        xA += xStepBC;
                        xC += xStepAC;
                        shadeA += shadeStepBC;
                        shadeC += shadeStepAC;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }

                    while (--yA >= 0)
                    {
                        textureRaster(xB >> 16, xC >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeB >> 8, shadeC >> 8);
                        xB += xStepAB;
                        xC += xStepAC;
                        shadeB += shadeStepAB;
                        shadeC += shadeStepAC;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }
                }
                else
                {
                    yA -= yB;
                    yB -= yC;
                    yC = lineOffset[yC];

                    while (--yB >= 0)
                    {
                        textureRaster(xC >> 16, xA >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeC >> 8, shadeA >> 8);
                        xA += xStepBC;
                        xC += xStepAC;
                        shadeA += shadeStepBC;
                        shadeC += shadeStepAC;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }

                    while (--yA >= 0)
                    {
                        textureRaster(xC >> 16, xB >> 16, data, yC, texels, 0, 0, u, v, w, uStride, vStride, wStride,
                            shadeC >> 8, shadeB >> 8);
                        xB += xStepAB;
                        xC += xStepAC;
                        shadeB += shadeStepAB;
                        shadeC += shadeStepAC;
                        yC += width2d;
                        u += uStepVertical;
                        v += vStepVertical;
                        w += wStepVertical;
                    }
                }
            }
        }
    }

    private static void textureRaster(int xA, int xB, int[] dst, int offset, int[] texels, int curU, int curV, int u,
        int v, int w, int uStride, int vStride, int wStride, int shadeA, int shadeB)
    {
        if (xA >= xB) return;

        int shadeStrides;
        int strides;
        if (hclip)
        {
            shadeStrides = (shadeB - shadeA) / (xB - xA);

            if (xB > safeWidth) xB = safeWidth;

            if (xA < 0)
            {
                shadeA -= xA * shadeStrides;
                xA = 0;
            }

            if (xA >= xB) return;

            strides = (xB - xA) >> 3;
            shadeStrides <<= 12;
            shadeA <<= 9;
        }
        else
        {
            if (xB - xA > 7)
            {
                strides = (xB - xA) >> 3;
                shadeStrides = ((shadeB - shadeA) * divTable[strides]) >> 6;
            }
            else
            {
                strides = 0;
                shadeStrides = 0;
            }

            shadeA <<= 9;
        }

        offset += xA;

        if (lowDetail)
        {
            var nextU = 0;
            var nextV = 0;
            var dx = xA - centerW3D;

            u = u + (uStride >> 3) * dx;
            v = v + (vStride >> 3) * dx;
            w = w + (wStride >> 3) * dx;

            var curW = w >> 12;
            if (curW != 0)
            {
                curU = u / curW;
                curV = v / curW;
                if (curU < 0)
                    curU = 0;
                else if (curU > 0xfc0) curU = 0xfc0;
            }

            u = u + uStride;
            v = v + vStride;
            w = w + wStride;

            curW = w >> 12;
            if (curW != 0)
            {
                nextU = u / curW;
                nextV = v / curW;
                if (nextU < 0x7)
                    nextU = 0x7;
                else if (nextU > 0xfc0) nextU = 0xfc0;
            }

            var stepU = (nextU - curU) >> 3;
            var stepV = (nextV - curV) >> 3;
            curU += (shadeA >> 3) & 0xC0000;
            var shadeShift = shadeA >> 23;

            if (opaque)
            {
                while (strides-- > 0)
                {
                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU = nextU;
                    curV = nextV;

                    u += uStride;
                    v += vStride;
                    w += wStride;

                    curW = w >> 12;
                    if (curW != 0)
                    {
                        nextU = u / curW;
                        nextV = v / curW;
                        if (nextU < 0x7)
                            nextU = 0x7;
                        else if (nextU > 0xfc0) nextU = 0xfc0;
                    }

                    stepU = (nextU - curU) >> 3;
                    stepV = (nextV - curV) >> 3;
                    shadeA += shadeStrides;
                    curU += (shadeA >> 3) & 0xC0000;
                    shadeShift = shadeA >> 23;
                }

                strides = (xB - xA) & 0x7;
                while (strides-- > 0)
                {
                    dst[offset++] = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;
                }
            }
            else
            {
                while (strides-- > 0)
                {
                    int rgb;
                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU = nextU;
                    curV = nextV;

                    u += uStride;
                    v += vStride;
                    w += wStride;

                    curW = w >> 12;
                    if (curW != 0)
                    {
                        nextU = u / curW;
                        nextV = v / curW;
                        if (nextU < 7)
                            nextU = 7;
                        else if (nextU > 0xfc0) nextU = 0xfc0;
                    }

                    stepU = (nextU - curU) >> 3;
                    stepV = (nextV - curV) >> 3;
                    shadeA += shadeStrides;
                    curU += (shadeA >> 3) & 0xC0000;
                    shadeShift = shadeA >> 23;
                }

                strides = (xB - xA) & 0x7;
                while (strides-- > 0)
                {
                    int rgb;
                    if ((rgb = texels[(curV & 0xFC0) + (curU >> 6)] >>> shadeShift) != 0) dst[offset] = rgb;

                    offset++;
                    curU += stepU;
                    curV += stepV;
                }
            }
        }
        else
        {
            var nextU = 0;
            var nextV = 0;
            var dx = xA - centerW3D;

            u = u + (uStride >> 3) * dx;
            v = v + (vStride >> 3) * dx;
            w = w + (wStride >> 3) * dx;

            var curW = w >> 14;
            if (curW != 0)
            {
                curU = u / curW;
                curV = v / curW;
                if (curU < 0)
                    curU = 0;
                else if (curU > 0x3f80) curU = 0x3f80;
            }

            u = u + uStride;
            v = v + vStride;
            w = w + wStride;

            curW = w >> 14;
            if (curW != 0)
            {
                nextU = u / curW;
                nextV = v / curW;
                if (nextU < 0x7)
                    nextU = 0x7;
                else if (nextU > 0x3f80) nextU = 0x3f80;
            }

            var stepU = (nextU - curU) >> 3;
            var stepV = (nextV - curV) >> 3;
            curU += shadeA & 0x600000;
            var shadeShift = shadeA >> 23;

            if (opaque)
            {
                while (strides-- > 0)
                {
                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;

                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU = nextU;
                    curV = nextV;

                    u += uStride;
                    v += vStride;
                    w += wStride;

                    curW = w >> 14;
                    if (curW != 0)
                    {
                        nextU = u / curW;
                        nextV = v / curW;
                        if (nextU < 0x7)
                            nextU = 0x7;
                        else if (nextU > 0x3f80) nextU = 0x3f80;
                    }

                    stepU = (nextU - curU) >> 3;
                    stepV = (nextV - curV) >> 3;
                    shadeA += shadeStrides;
                    curU += shadeA & 0x600000;
                    shadeShift = shadeA >> 23;
                }

                strides = (xB - xA) & 0x7;
                while (strides-- > 0)
                {
                    dst[offset++] = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift;
                    curU += stepU;
                    curV += stepV;
                }
            }
            else
            {
                while (strides-- > 0)
                {
                    int rgb;
                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU += stepU;
                    curV += stepV;

                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;
                    offset++;
                    curU = nextU;
                    curV = nextV;

                    u += uStride;
                    v += vStride;
                    w += wStride;

                    curW = w >> 14;
                    if (curW != 0)
                    {
                        nextU = u / curW;
                        nextV = v / curW;
                        if (nextU < 0x7)
                            nextU = 0x7;
                        else if (nextU > 0x3f80) nextU = 0x3f80;
                    }

                    stepU = (nextU - curU) >> 3;
                    stepV = (nextV - curV) >> 3;
                    shadeA += shadeStrides;
                    curU += shadeA & 0x600000;
                    shadeShift = shadeA >> 23;
                }

                strides = (xB - xA) & 0x7;
                while (strides-- > 0)
                {
                    int rgb;
                    if ((rgb = texels[(curV & 0x3F80) + (curU >> 7)] >>> shadeShift) != 0) dst[offset] = rgb;

                    offset++;
                    curU += stepU;
                    curV += stepV;
                }
            }
        }
    }
}