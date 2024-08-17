using RS2_225.jagex2.io;

namespace RS2_225.java;

public class CRC32
{
    private static readonly uint[] Crc32Table;
    private uint _crc;

    static CRC32()
    {
        Crc32Table = new uint[256];

        for (uint i = 0; i < 256; i++)
        {
            var crc = i;
            for (uint j = 8; j > 0; j--)
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ Packet.CRC32_POLYNOMIAL;
                else
                    crc >>= 1;
            Crc32Table[i] = crc;
        }
    }

    public CRC32()
    {
        reset();
    }

    public void reset()
    {
        _crc = 0xffffffff;
    }

    public void update(byte[] buffer)
    {
        foreach (var b in buffer)
        {
            var tableIndex = (byte)((_crc & 0xff) ^ b);
            _crc = (_crc >> 8) ^ Crc32Table[tableIndex];
        }
    }

    public uint getValue()
    {
        return _crc ^ 0xffffffff;
    }
}