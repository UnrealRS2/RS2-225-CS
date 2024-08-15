namespace RS2_225.jagex2.io;

public class Isaac
{
    private int count = 0;
    private uint[] rsl = new uint[256];
    private uint[] mem = new uint[256];
    private uint a = 0;
    private uint b = 0;
    private uint c = 0;

    public Isaac(uint[] seed)
    {
        for (var i = 0; i < seed.Length; i++)
        {
            rsl[i] = seed[i];
        }
        init();
    }

    public int nextInt()
    {
        if (count-- == 0) {
            isaac();
            count = 255;
        }
        return (int) rsl[count];
    }

    private void init()
    {
        var a = 0x9e3779b9;
        var b = 0x9e3779b9;
        var c = 0x9e3779b9;
        var d = 0x9e3779b9;
        var e = 0x9e3779b9;
        var f = 0x9e3779b9;
        var g = 0x9e3779b9;
        var h = 0x9e3779b9;

        for (var i = 0; i < 4; i++)
        {
            a ^= b << 11;
            d += a;
            b += c;
            b ^= c >>> 2;
            e += b;
            c += d;
            c ^= d << 8;
            f += c;
            d += e;
            d ^= e >>> 16;
            g += d;
            e += f;
            e ^= f << 10;
            h += e;
            f += g;
            f ^= g >>> 4;
            a += f;
            g += h;
            g ^= h << 8;
            b += g;
            h += a;
            h ^= a >>> 9;
            c += h;
            a += b;
        }

        for (var i = 0; i < 256; i++)
        {
            a += rsl[i];
            b += rsl[i + 1];
            c += rsl[i + 2];
            d += rsl[i + 3];
            e += rsl[i + 4];
            f += rsl[i + 5];
            g += rsl[i + 6];
            h += rsl[i + 7];

            a ^= b << 11;
            d += a;
            b += c;
            b ^= c >>> 2;
            e += b;
            c += d;
            c ^= d << 8;
            f += c;
            d += e;
            d ^= e >>> 16;
            g += d;
            e += f;
            e ^= f << 10;
            h += e;
            f += g;
            f ^= g >>> 4;
            a += f;
            g += h;
            g ^= h << 8;
            b += g;
            h += a;
            h ^= a >>> 9;
            c += h;
            a += b;

            mem[i] = a;
            mem[i + 1] = b;
            mem[i + 2] = c;
            mem[i + 3] = d;
            mem[i + 4] = e;
            mem[i + 5] = f;
            mem[i + 6] = g;
            mem[i + 7] = h;
        }

        for (var i = 0; i < 256; i++)
        {
            a += mem[i];
            b += mem[i + 1];
            c += mem[i + 2];
            d += mem[i + 3];
            e += mem[i + 4];
            f += mem[i + 5];
            g += mem[i + 6];
            h += mem[i + 7];

            a ^= b << 11;
            d += a;
            b += c;
            b ^= c >>> 2;
            e += b;
            c += d;
            c ^= d << 8;
            f += c;
            d += e;
            d ^= e >>> 16;
            g += d;
            e += f;
            e ^= f << 10;
            h += e;
            f += g;
            f ^= g >>> 4;
            a += f;
            g += h;
            g ^= h << 8;
            b += g;
            h += a;
            h ^= a >>> 9;
            c += h;
            a += b;

            mem[i] = a;
            mem[i + 1] = b;
            mem[i + 2] = c;
            mem[i + 3] = d;
            mem[i + 4] = e;
            mem[i + 5] = f;
            mem[i + 6] = g;
            mem[i + 7] = h;
        }
        
        isaac();
        count = 256;
    }

    private void isaac()
    {
        c++;
        b += c;
        for (var i = 0; i < 256; i++)
        {
            var x = this.mem[i];

            var mem = i & 3;
            if (mem == 0) {
                a ^= a << 13;
            } else if (mem == 1) {
                a ^= a >>> 6;
            } else if (mem == 2) {
                a ^= a << 2;
            } else if (mem == 3) {
                a ^= a >>> 16;
            }

            a += this.mem[(i + 128) & 0xff];

            uint y;
            this.mem[i] = y = this.mem[(x >>> 2) & 0xff] + a + b;
            rsl[i] = b = this.mem[((y >>> 8) >>> 2) & 0xff] + x;
        }
    }
}