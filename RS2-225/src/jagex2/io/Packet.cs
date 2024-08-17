using RS2_225.jagex2.datastruct;

namespace RS2_225.jagex2.io;

public class Packet : Hashable
{
    public static uint CRC32_POLYNOMIAL = 0xedb88320;

    private static readonly int[] crctable = new int[256];
    private static readonly ulong[] bitmask = new ulong[33];

    private static readonly LinkList cacheMin = new();
    private static readonly LinkList cacheMid = new();
    private static readonly LinkList cacheMax = new();

    private static int cacheMinCount;
    private static int cacheMidCount;
    private static int cacheMaxCount;
    private int bitPo = 0;

    private readonly byte[] data;

    public int pos;
    private readonly Isaac? random = null;

    static Packet()
    {
        for (var i = 0; i < 32; i++) bitmask[i] = (uint)(1 << i) - 1;
        bitmask[32] = 0xffffffff;

        for (var i = 0; i < 256; i++)
        {
            var remainder = i;
            for (var j = 0; j < 8; j++)
                if ((remainder & 1) == 1)
                    remainder = (int)((remainder >>> 1) ^ CRC32_POLYNOMIAL);
                else
                    remainder >>>= 1;
            crctable[i] = remainder;
        }
    }

    public Packet(byte[]? src)
    {
        data = src;
        pos = 0;
    }

    private static long crc32(sbyte[] src)
    {
        var crc = src.Aggregate<sbyte, long>(0xffffffff,
            (current, t) => (current >>> 8) ^ crctable[(current ^ t) & 0xff]);
        return ~crc;
    }

    public int length()
    {
        return data.Length;
    }

    public int available()
    {
        return length() - pos;
    }

    public static Packet alloc(int type)
    {
        Packet? cached = null;
        switch (type)
        {
            case 0 when cacheMinCount > 0:
                cacheMinCount--;
                cached = cacheMin.removeHead() as Packet;
                break;
            case 1 when cacheMidCount > 0:
                cacheMidCount--;
                cached = cacheMid.removeHead() as Packet;
                break;
            case 2 when cacheMaxCount > 0:
                cacheMaxCount--;
                cached = cacheMax.removeHead() as Packet;
                break;
        }

        if (cached == null)
            return type switch
            {
                0 => new Packet(new byte[100]),
                1 => new Packet(new byte[5000]),
                _ => new Packet(new byte[30000])
            };

        cached.pos = 0;
        return cached;
    }

    public void release()
    {
        pos = 0;
        switch (data.Length)
        {
            case 100 when cacheMinCount < 1000:
                cacheMin.addTail(this);
                cacheMinCount++;
                break;
            case 5000 when cacheMidCount < 250:
                cacheMid.addTail(this);
                cacheMidCount++;
                break;
            case 30000 when cacheMaxCount < 50:
                cacheMax.addTail(this);
                cacheMaxCount++;
                break;
        }
    }

    public void p1isaac(int opcode)
    {
        data[pos++] = (byte)(opcode + random.nextInt());
    }

    public void p1(int value)
    {
        data[pos++] = (byte)value;
    }

    public void p2(int value)
    {
        data[pos++] = (byte)(value >> 8);
        data[pos++] = (byte)value;
    }

    public void ip2(int value)
    {
        data[pos++] = (byte)value;
        data[pos++] = (byte)(value >> 8);
    }

    public void p3(int value)
    {
        data[pos++] = (byte)(value >> 16);
        data[pos++] = (byte)(value >> 8);
        data[pos++] = (byte)value;
    }

    public void p4(int value)
    {
        data[pos++] = (byte)(value >> 24);
        data[pos++] = (byte)(value >> 16);
        data[pos++] = (byte)(value >> 8);
        data[pos++] = (byte)value;
    }

    public void ip4(int value)
    {
        data[pos++] = (byte)value;
        data[pos++] = (byte)(value >> 8);
        data[pos++] = (byte)(value >> 16);
        data[pos++] = (byte)(value >> 24);
    }

    public void p8(long value)
    {
        data[pos++] = (byte)(value >> 56);
        data[pos++] = (byte)(value >> 48);
        data[pos++] = (byte)(value >> 40);
        data[pos++] = (byte)(value >> 32);
        data[pos++] = (byte)(value >> 24);
        data[pos++] = (byte)(value >> 16);
        data[pos++] = (byte)(value >> 8);
        data[pos++] = (byte)value;
    }

    public int g1()
    {
        return data[pos++] & 0xFF;
    }

    public int g1b()
    {
        return data[pos++];
    }

    public int g2()
    {
        pos += 2;
        return ((data[pos - 2] & 0xFF) << 8) + (data[pos - 1] & 0xFF);
    }

    public int g2b()
    {
        pos += 2;
        var value = ((data[pos - 2] & 0xFF) << 8) + (data[pos - 1] & 0xFF);
        if (value > 32767) value -= 65536;
        return value;
    }

    public int g3()
    {
        pos += 3;
        return ((data[pos - 3] & 0xFF) << 16) + ((data[pos - 2] & 0xFF) << 8) + (data[pos - 1] & 0xFF);
    }

    public int g4()
    {
        pos += 4;
        return ((data[pos - 4] & 0xFF) << 24) + ((data[pos - 3] & 0xFF) << 16) + ((data[pos - 2] & 0xFF) << 8) +
               (data[pos - 1] & 0xFF);
    }

    public long g8()
    {
        var high = g4() & 0xFFFFFFFFL;
        var low = g4() & 0xFFFFFFFFL;
        return (high << 32) + low;
    }

    public string gjstr()
    {
        var start = pos;
        while (data[pos++] != 10)
        {
        }

        var charBufferLen = pos - start - 1;
        var charBuffer = new char[charBufferLen];
        for (var i = 0; i < charBufferLen; i++) charBuffer[i] = (char)data[start + i];
        return new string(charBuffer, 0, charBuffer.Length);
    }

    public int gsmart()
    {
        var value = data[pos] & 0xFF;
        return value < 128 ? g1() - 64 : g2() - 49152;
    }
}