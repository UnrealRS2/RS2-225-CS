using System.Data;
using RS2_225.jagex2.datastruct;

namespace RS2_225.jagex2.io;

public class Packet : Hashable
{
    private static uint CRC32_POLYNOMIAL = 0xedb88320;
    
    private static int[] crctable = new int[256];
    private static ulong[] bitmask = new ulong[33];
    
    private static LinkList cacheMin = new();
    private static LinkList cacheMid = new();
    private static LinkList cacheMax = new();
    
    private static int cacheMinCount = 0;
    private static int cacheMidCount = 0;
    private static int cacheMaxCount = 0;

    static Packet()
    {
        for (var i = 0; i < 32; i++)
        {
            bitmask[i] = (uint)(1 << i) - 1;
        }
        bitmask[32] = 0xffffffff;

        for (var i = 0; i < 256; i++)
        {
            var remainder = i;
            for (var j = 0; j < 8; j++)
            {
                if ((remainder & 1) == 1) {
                    remainder = (int)((remainder >>> 1) ^ CRC32_POLYNOMIAL);
                } else {
                    remainder >>>= 1;
                }
            }
            crctable[i] = remainder;
        }
    }

    static long crc32(sbyte[] src)
    {
        var crc = src.Aggregate<sbyte, long>(0xffffffff, (current, t) => (current >>> 8) ^ crctable[(current ^ t) & 0xff]);
        return ~crc;
    }
    
    private sbyte[] data;

    private int pos;
    private int bitPo = 0;
    private Isaac? random = null;

    public Packet(sbyte[] src)
    {
        data = src;
        pos = 0;
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
                0 => new Packet(new sbyte[100]),
                1 => new Packet(new sbyte[5000]),
                _ => new Packet(new sbyte[30000])
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
        data[pos++] = (sbyte) (opcode + random.nextInt());
    }

    public void p1(int value)
    {
        data[pos++] = (sbyte) value;
    }
    
    public void p2(int value)
    {
        data[pos++] = (sbyte) (value >> 8);
        data[pos++] = (sbyte) value;
    }
    
    public void ip2(int value)
    {
        data[pos++] = (sbyte) value;
        data[pos++] = (sbyte) (value >> 8);
    }
    
    public void p3(int value)
    {
        data[pos++] = (sbyte) (value >> 16);
        data[pos++] = (sbyte) (value >> 8);
        data[pos++] = (sbyte) value;
    }
    
    public void p4(int value)
    {
        data[pos++] = (sbyte) (value >> 24);
        data[pos++] = (sbyte) (value >> 16);
        data[pos++] = (sbyte) (value >> 8);
        data[pos++] = (sbyte) value;
    }
    
    public void ip4(int value)
    {
        data[pos++] = (sbyte) value;
        data[pos++] = (sbyte) (value >> 8);
        data[pos++] = (sbyte) (value >> 16);
        data[pos++] = (sbyte) (value >> 24);
    }
    
    public void p8(long value)
    {
        data[pos++] = (sbyte) (value >> 56);
        data[pos++] = (sbyte) (value >> 48);
        data[pos++] = (sbyte) (value >> 40);
        data[pos++] = (sbyte) (value >> 32);
        data[pos++] = (sbyte) (value >> 24);
        data[pos++] = (sbyte) (value >> 16);
        data[pos++] = (sbyte) (value >> 8);
        data[pos++] = (sbyte) value;
    }
}