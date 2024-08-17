using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

/**
 * [instances] was added to fully cache objtype configs on load
 */
public class ObjType
{
    private static int count;
    private static int[] offsets;
    private static Packet dat;
    private static ObjType[] cache;
    private static int cachePos;
    private static ObjType[] instances;
    private int certlink;
    private int certtemplate;
    private bool? code10;
    private bool code9;
    private int cost;
    private int[]? countco;
    private int[]? countobj;
    private string? desc;
    private int index = -1;
    private string[]? iop;
    private int manhead;
    private int manhead2;
    private int manwear;
    private int manwear2;
    private int manwear3;
    private byte manwearOffsetY;
    private bool members;
    private int model;
    private string? name;
    private string[]? op;
    private int[]? recol_d;
    private int[]? recol_s;
    private bool stackable;
    private int womanhead;
    private int womanhead2;
    private int womanwear;
    private int womanwear2;
    private int womanwear3;
    private byte womanwearOffsetY;
    private int xan2d;
    private int xof2d;
    private int yan2d;
    private int yof2d;
    private int zan2d;
    private int zoom2d;

    public static void unpack(Jagfile config)
    {
        dat = new Packet(config.read("obj.dat", null));
        var idx = new Packet(config.read("obj.idx", null));

        count = idx.g2();
        offsets = new int[count];

        var offset = 2;
        for (var id = 0; id < count; id++)
        {
            offsets[id] = offset;
            offset += idx.g2();
        }

        cache = new ObjType[10];
        for (var id = 0; id < 10; id++) cache[id] = new ObjType();

        instances = new ObjType[count];
        for (var id = 0; id < count; id++) instances[id] = get(id);
        Console.WriteLine("Decoded " + count + " ObjType configs");
    }

    public static ObjType get(int id)
    {
        for (var i = 0; i < 10; i++)
            if (cache[i].index == id)
                return cache[i];

        cachePos = (cachePos + 1) % 10;
        var obj = cache[cachePos];
        dat.pos = offsets[id];
        obj.index = id;
        obj.reset();
        obj.decode(dat);

        if (obj.certtemplate != -1) obj.toCertificate();

        if (!client.members && obj.members)
        {
            obj.name = "Members Object";
            obj.desc = "Login to a members' server to use this object.";
            obj.op = null;
            obj.iop = null;
        }

        return obj;
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
                name = dat.gjstr();
            }
            else if (code == 3)
            {
                desc = dat.gjstr();
            }
            else if (code == 4)
            {
                zoom2d = dat.g2();
            }
            else if (code == 5)
            {
                xan2d = dat.g2();
            }
            else if (code == 6)
            {
                yan2d = dat.g2();
            }
            else if (code == 7)
            {
                xof2d = dat.g2();
                if (xof2d > 32767) xof2d -= 65536;
            }
            else if (code == 8)
            {
                yof2d = dat.g2();
                if (yof2d > 32767) yof2d -= 65536;
            }
            else if (code == 9)
            {
                // animHasAlpha from code10?
                code9 = true;
            }
            else if (code == 10)
            {
                // seq?
                code10 = dat.g2() == 1;
            }
            else if (code == 11)
            {
                stackable = true;
            }
            else if (code == 12)
            {
                cost = dat.g4();
            }
            else if (code == 16)
            {
                members = true;
            }
            else if (code == 23)
            {
                manwear = dat.g2();
                manwearOffsetY = dat.g1b() == 1 ? (byte)1 : (byte)0;
                ;
            }
            else if (code == 24)
            {
                manwear2 = dat.g2();
            }
            else if (code == 25)
            {
                womanwear = dat.g2();
                womanwearOffsetY = dat.g1b() == 1 ? (byte)1 : (byte)0;
                ;
            }
            else if (code == 26)
            {
                womanwear2 = dat.g2();
            }
            else if (code >= 30 && code < 35)
            {
                if (op == null) op = new string[5];

                op[code - 30] = dat.gjstr();
                if (op[code - 30].Equals("hidden", StringComparison.CurrentCultureIgnoreCase)) op[code - 30] = null;
            }
            else if (code >= 35 && code < 40)
            {
                if (iop == null) iop = new string[5];

                iop[code - 35] = dat.gjstr();
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
            else if (code == 78)
            {
                manwear3 = dat.g2();
            }
            else if (code == 79)
            {
                womanwear3 = dat.g2();
            }
            else if (code == 90)
            {
                manhead = dat.g2();
            }
            else if (code == 91)
            {
                womanhead = dat.g2();
            }
            else if (code == 92)
            {
                manhead2 = dat.g2();
            }
            else if (code == 93)
            {
                womanhead2 = dat.g2();
            }
            else if (code == 95)
            {
                zan2d = dat.g2();
            }
            else if (code == 97)
            {
                certlink = dat.g2();
            }
            else if (code == 98)
            {
                certtemplate = dat.g2();
            }
            else if (code >= 100 && code < 110)
            {
                if (countobj == null)
                {
                    countobj = new int[10];
                    countco = new int[10];
                }

                countobj[code - 100] = dat.g2();
                countco[code - 100] = dat.g2();
            }
            else
            {
                Console.WriteLine("Error unrecognised obj config code: " + code);
            }
        }
    }

    private void reset()
    {
        model = 0;
        name = null;
        desc = null;
        recol_s = null;
        recol_d = null;
        zoom2d = 2000;
        xan2d = 0;
        yan2d = 0;
        zan2d = 0;
        xof2d = 0;
        yof2d = 0;
        code9 = false;
        code10 = null;
        stackable = false;
        cost = 1;
        members = false;
        op = null;
        iop = null;
        manwear = -1;
        manwear2 = -1;
        manwearOffsetY = 0;
        womanwear = -1;
        womanwear2 = -1;
        womanwearOffsetY = 0;
        manwear3 = -1;
        womanwear3 = -1;
        manhead = -1;
        manhead2 = -1;
        womanhead = -1;
        womanhead2 = -1;
        countobj = null;
        countco = null;
        certlink = -1;
        certtemplate = -1;
    }

    public void toCertificate()
    {
        var template = get(certtemplate);
        model = template.model;
        zoom2d = template.zoom2d;
        xan2d = template.xan2d;
        yan2d = template.yan2d;
        zan2d = template.zan2d;
        xof2d = template.xof2d;
        yof2d = template.yof2d;
        recol_s = template.recol_s;
        recol_d = template.recol_d;

        var link = get(certlink);
        name = link.name;
        members = link.members;
        cost = link.cost;

        var article = "a";
        var c = link.name.ToCharArray()[0];
        if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U') article = "an";
        desc = "Swap this note at any bank for " + article + " " + link.name + ".";

        stackable = true;
    }
}