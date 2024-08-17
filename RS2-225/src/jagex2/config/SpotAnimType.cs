using RS2_225.jagex2.datastruct;
using RS2_225.jagex2.graphics;
using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class SpotAnimType
{
    private static int count;

    public static SpotAnimType[] instances;

    public static LruCache modelCache = new(30);

    public int ambient;

    private int anim = -1;

    public bool animHasAlpha;

    public int contrast;

    public int index;

    private int model;

    public int orientation;

    private readonly int[] recol_d = new int[6];

    private readonly int[] recol_s = new int[6];

    public int resizeh = 128;

    public int resizev = 128;

    public SeqType seq;

    public static void unpack(Jagfile config)
    {
        var dat = new Packet(config.read("spotanim.dat", null));
        count = dat.g2();

        if (instances == null) instances = new SpotAnimType[count];

        for (var id = 0; id < count; id++)
        {
            if (instances[id] == null) instances[id] = new SpotAnimType();

            instances[id].index = id;
            instances[id].decode(dat);
        }

        Console.WriteLine("Decoded " + count + " SpotAnimType configs");
    }

    public void decode(Packet dat)
    {
        while (true)
        {
            var code = dat.g1();
            if (code == 0) break;

            if (code == 1)
            {
                model = dat.g2();
            }
            else if (code == 2)
            {
                anim = dat.g2();

                if (SeqType.instances != null) seq = SeqType.instances[anim]!;
            }
            else if (code == 3)
            {
                animHasAlpha = true;
            }
            else if (code == 4)
            {
                resizeh = dat.g2();
            }
            else if (code == 5)
            {
                resizev = dat.g2();
            }
            else if (code == 6)
            {
                orientation = dat.g2();
            }
            else if (code == 7)
            {
                ambient = dat.g1();
            }
            else if (code == 8)
            {
                contrast = dat.g1();
            }
            else if (code >= 40 && code < 50)
            {
                recol_s[code - 40] = dat.g2();
            }
            else if (code >= 50 && code < 60)
            {
                recol_d[code - 50] = dat.g2();
            }
            else
            {
                Console.WriteLine("Error unrecognised spotanim config code: " + code);
            }
        }
    }

    public Model getModel()
    {
        var model = (Model)modelCache.get(index);
        if (model != null) return model;

        model = new Model(this.model);
        for (var i = 0; i < 6; i++)
            if (recol_s[0] != 0)
                model.recolor(recol_s[i], recol_d[i]);

        modelCache.put(index, model);
        return model;
    }
}