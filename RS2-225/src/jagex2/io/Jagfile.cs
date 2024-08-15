namespace RS2_225.jagex2.io;

public class Jagfile
{
    private byte[] buffer;
    private int fileCount;
    private int[] fileHash;
    private int[] fileUnpackedSize;
    private int[] filePackedSize;
    private int[] fileOffset;
    private bool unpacked;

    public Jagfile(byte[] src)
    {
        load(src);
    }

    private void load(byte[] src)
    {
        var data = new Packet(src);
        var unpackedSize = data.g3();
        var packedSize = data.g3();
        if (packedSize == unpackedSize)
        {
            buffer = src;
            unpacked = false;
        }
        else
        {
            var temp = new byte[unpackedSize];
            BZip2.read(temp, unpackedSize, src, packedSize, 6);
            buffer = temp;

            data = new Packet(buffer);
            unpacked = true;
        }
         
        fileCount = data.g2();
        fileHash = new int[fileCount];
        fileUnpackedSize = new int[fileCount];
        filePackedSize = new int[fileCount];
        fileOffset = new int[fileCount];

        var pos = data.pos + fileCount * 10;
        for (var i = 0; i < fileCount; i++)
        {
            fileHash[i] = data.g4();
            fileUnpackedSize[i] = data.g3();
            filePackedSize[i] = data.g3();
            fileOffset[i] = pos;
            pos += filePackedSize[i];
        }
    }
    
    public byte[]? read(String name, byte[]? dst) {
        var hash = 0;
        var upper = name.ToUpper();
        for (var i = 0; i < upper.Length; i++) {
            hash = hash * 61 + upper.ToCharArray()[i] - 32;
        }

        for (var i = 0; i < fileCount; i++) {
            if (fileHash[i] == hash) {
                if (dst == null) {
                    dst = new byte[fileUnpackedSize[i]];
                }

                if (unpacked) {
                    if (fileUnpackedSize[i] >= 0) {
                        Array.Copy(buffer, fileOffset[i], dst, 0, fileUnpackedSize[i]);
                    }
                } else {
                    BZip2.read(dst, fileUnpackedSize[i], buffer, filePackedSize[i], fileOffset[i]);
                }

                return dst;
            }
        }
        return null;
    }
}