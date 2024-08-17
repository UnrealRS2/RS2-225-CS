using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class VarpType
{
    private static int count;

    public static VarpType[] instances;

    public static int code3Count;

    public static int[] code3;

    public int clientcode;

    private int code1;

    private string code10;

    private int code2;

    private bool code4 = true;

    private bool code6;

    private int code7;

    private bool code8;

    private bool hasCode3;

    public static void unpack(Jagfile config)
    {
        var dat = new Packet(config.read("varp.dat", null));
        code3Count = 0;
        count = dat.g2();

        if (instances == null) instances = new VarpType[count];

        if (code3 == null) code3 = new int[count];

        for (var id = 0; id < count; id++)
        {
            if (instances[id] == null) instances[id] = new VarpType();

            instances[id].decode(id, dat);
        }

        Console.WriteLine("Decoded " + count + " VarpType configs");
    }

    public void decode(int id, Packet dat)
    {
        while (true)
        {
            var code = dat.g1();
            if (code == 0) return;

            if (code == 1)
            {
                code1 = dat.g1();
            }
            else if (code == 2)
            {
                code2 = dat.g1();
            }
            else if (code == 3)
            {
                hasCode3 = true;
                code3[code3Count++] = id;
            }
            else if (code == 4)
            {
                code4 = false;
            }
            else if (code == 5)
            {
                clientcode = dat.g2();
            }
            else if (code == 6)
            {
                code6 = true;
            }
            else if (code == 7)
            {
                code7 = dat.g4();
            }
            else if (code == 8)
            {
                code8 = true;
            }
            else if (code == 10)
            {
                code10 = dat.gjstr();
            }
            else
            {
                Console.WriteLine("Error unrecognised varp config code: " + code);
            }
        }
    }
}