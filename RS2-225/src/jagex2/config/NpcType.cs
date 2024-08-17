using RS2_225.jagex2.datastruct;
using RS2_225.jagex2.graphics;
using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class NpcType
{
    private static int count;

    private static int[] offsets;

    private static Packet dat;

    private static NpcType[] cache;

    private static int cachePos;

    public static LruCache modelCache = new(30);

    public static NpcType?[]? instances;

    private bool animHasAlpha;

    public string desc;

    private int[] heads;

    public long index = -1L;

    public bool minimap = true;

    private int[] models;

    public string name;

    public string[] op;

    public int readyanim = -1;

    private int[] recol_d;

    private int[] recol_s;

    private int resizeh = 128;

    private int resizev = 128;

    private int resizex = -1;

    private int resizey = -1;

    private int resizez = -1;

    public byte size = 1;

    public int vislevel = -1;

    public int walkanim = -1;

    public int walkanim_b = -1;

    public int walkanim_l = -1;

    public int walkanim_r = -1;

    public static void unpack(Jagfile config)
    {
        dat = new Packet(config.read("npc.dat", null));
        var idx = new Packet(config.read("npc.idx", null));

        count = idx.g2();
        offsets = new int[count];

        var offset = 2;
        for (var id = 0; id < count; id++)
        {
            offsets[id] = offset;
            offset += idx.g2();
        }

        cache = new NpcType[20];
        for (var id = 0; id < 20; id++) cache[id] = new NpcType();

        instances = new NpcType[count];
        for (var id = 0; id < count; id++) instances[id] = get(id);
        Console.WriteLine("Decoded " + count + " NpcType configs");
    }

    public static void unload()
    {
        modelCache = null;
        offsets = null;
        cache = null;
        dat = null;
    }

    public static NpcType get(int id)
    {
        for (var i = 0; i < 20; i++)
            if (cache[i].index == id)
                return cache[i];

        cachePos = (cachePos + 1) % 20;
        var npc = cache[cachePos] = new NpcType();
        dat.pos = offsets[id];
        npc.index = id;
        npc.decode(dat);
        return npc;
    }

    public void decode(Packet dat)
    {
        while (true)
        {
            var code = dat.g1();
            if (code == 0) return;

            if (code == 1)
            {
                var count = dat.g1();
                models = new int[count];

                for (var i = 0; i < count; i++) models[i] = dat.g2();
            }
            else if (code == 2)
            {
                name = dat.gjstr();
            }
            else if (code == 3)
            {
                desc = dat.gjstr();
            }
            else if (code == 12)
            {
                size = byte.Parse($"{dat.g1b()}");
            }
            else if (code == 13)
            {
                readyanim = dat.g2();
            }
            else if (code == 14)
            {
                walkanim = dat.g2();
            }
            else if (code == 16)
            {
                animHasAlpha = true;
            }
            else if (code == 17)
            {
                walkanim = dat.g2();
                walkanim_b = dat.g2();
                walkanim_r = dat.g2();
                walkanim_l = dat.g2();
            }
            else if (code >= 30 && code < 40)
            {
                if (op == null) op = new string[5];

                op[code - 30] = dat.gjstr();
                if (op[code - 30].Equals("hidden", StringComparison.CurrentCultureIgnoreCase)) op[code - 30] = null;
            }
            else if (code == 40)
            {
                var count = dat.g1();
                recol_s = new int[count];
                recol_d = new int[count];

                for (var i = 0; i < count; i++)
                {
                    recol_s[i] = dat.g2();
                    recol_d[i] = dat.g2();
                }
            }
            else if (code == 60)
            {
                var count = dat.g1();
                heads = new int[count];

                for (var i = 0; i < count; i++) heads[i] = dat.g2();
            }
            else if (code == 90)
            {
                // unused
                resizex = dat.g2();
            }
            else if (code == 91)
            {
                // unused
                resizey = dat.g2();
            }
            else if (code == 92)
            {
                // unused
                resizez = dat.g2();
            }
            else if (code == 93)
            {
                minimap = false;
            }
            else if (code == 95)
            {
                vislevel = dat.g2();
            }
            else if (code == 97)
            {
                resizeh = dat.g2();
            }
            else if (code == 98)
            {
                resizev = dat.g2();
            }
            else
            {
                Console.WriteLine("Error unrecognised npc config code: " + code);
            }
        }
    }

    public Model getSequencedModel(int primaryTransformId, int secondaryTransformId, int[] seqMask)
    {
        Model tmp = null;
        var model = (Model)modelCache.get(index);

        if (model == null)
        {
            var models = new Model[this.models.Length];
            for (var i = 0; i < this.models.Length; i++) models[i] = new Model(this.models[i]);

            if (models.Length == 1)
                model = models[0];
            else
                model = new Model(models, models.Length);

            if (recol_s != null)
                for (var i = 0; i < recol_s.Length; i++)
                    model.recolor(recol_s[i], recol_d[i]);

            model.createLabelReferences();
            model.calculateNormals(64, 850, -30, -50, -30, true);
            modelCache.put(index, model);
        }

        tmp = new Model(model, !animHasAlpha);

        if (primaryTransformId != -1 && secondaryTransformId != -1)
            tmp.applyTransforms(primaryTransformId, secondaryTransformId, seqMask);
        else if (primaryTransformId != -1) tmp.applyTransform(primaryTransformId);

        if (resizeh != 128 || resizev != 128) tmp.scale(resizeh, resizev, resizeh);

        tmp.calculateBoundsCylinder();
        tmp.labelFaces = null;
        tmp.labelVertices = null;

        if (size == 1) tmp.pickable = true;

        return tmp;
    }

    public Model getHeadModel()
    {
        if (heads == null) return null;

        var models = new Model[heads.Length];
        for (var i = 0; i < heads.Length; i++) models[i] = new Model(heads[i]);

        Model model;
        if (models.Length == 1)
            model = models[0];
        else
            model = new Model(models, models.Length);

        if (recol_s != null)
            for (var i = 0; i < recol_s.Length; i++)
                model.recolor(recol_s[i], recol_d[i]);

        return model;
    }
}