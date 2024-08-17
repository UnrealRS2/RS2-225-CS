using RS2_225.jagex2.datastruct;
using RS2_225.jagex2.graphics;
using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class LocType
{
    // shapes
    public static int WALL_STRAIGHT = 0;
    public static int WALL_DIAGONALCORNER = 1;
    public static int WALL_L = 2;
    public static int WALL_SQUARECORNER = 3;
    public static int WALL_DIAGONAL = 9;
    public static int WALLDECOR_STRAIGHT_NOOFFSET = 4;
    public static int WALLDECOR_STRAIGHT_OFFSET = 5;
    public static int WALLDECOR_DIAGONAL_OFFSET = 6;
    public static int WALLDECOR_DIAGONAL_NOOFFSET = 7;
    public static int WALLDECOR_DIAGONAL_BOTH = 8;
    public static int CENTREPIECE_STRAIGHT = 10;
    public static int CENTREPIECE_DIAGONAL = 11;
    public static int GROUNDDECOR = 22;
    public static int ROOF_STRAIGHT = 12;
    public static int ROOF_DIAGONAL_WITH_ROOFEDGE = 13;
    public static int ROOF_DIAGONAL = 14;
    public static int ROOF_L_CONCAVE = 15;
    public static int ROOF_L_CONVEX = 16;
    public static int ROOF_FLAT = 17;
    public static int ROOFEDGE_STRAIGHT = 18;
    public static int ROOFEDGE_DIAGONALCORNER = 19;
    public static int ROOFEDGE_L = 20;
    public static int ROOFEDGE_SQUARECORNER = 21;

    public static bool _reset;

    private static int count;

    private static int[] offsets;

    private static Packet dat;

    private static LocType[] cache;

    private static int cachePos;

    public static LruCache modelCacheStatic = new(500);

    public static LruCache modelCacheDynamic = new(30);

    public static LocType?[]? instances;

    public bool active;

    private byte ambient;

    public int anim;

    private bool animHasAlpha;

    public bool blockrange;

    public bool blockwalk;

    private byte contrast;

    public string desc;

    public int forceapproach;

    public bool forcedecor;

    private bool hillskew;

    public int index = -1;

    public int length;

    public int mapfunction;

    public int mapscene;

    private bool mirror;

    private int[] models;

    public string name;

    public bool occlude;

    private int offsetx;

    private int offsety;

    private int offsetz;

    public string[]? op;

    private int[] recol_d;

    private int[] recol_s;

    private int resizex;

    private int resizey;

    private int resizez;

    public bool shadow;

    private int[]? shapes;

    private bool sharelight;

    public int wallwidth;

    public int width;

    public static void unpack(Jagfile config)
    {
        dat = new Packet(config.read("loc.dat", null));
        var idx = new Packet(config.read("loc.idx", null));

        count = idx.g2();
        offsets = new int[count];

        var offset = 2;
        for (var id = 0; id < count; id++)
        {
            offsets[id] = offset;
            offset += idx.g2();
        }

        cache = new LocType[10];
        for (var id = 0; id < 10; id++) cache[id] = new LocType();

        if (instances == null)
        {
            instances = new LocType[count];
            for (var id = 0; id < count; id++) instances[id] = get(id);
        }

        Console.WriteLine("Unpacked " + count + " LocType configs");
    }

    public static void unload()
    {
        modelCacheStatic = null;
        modelCacheDynamic = null;
        offsets = null;
        cache = null;
        dat = null;
    }

    public static LocType get(int id)
    {
        for (var i = 0; i < 10; i++)
            if (cache[i].index == id)
                return cache[i];

        cachePos = (cachePos + 1) % 10;
        var loc = cache[cachePos];
        dat.pos = offsets[id];
        loc.index = id;
        loc.reset();
        loc.decode(dat);
        return loc;
    }

    public void reset()
    {
        models = null;
        shapes = null;
        name = null;
        desc = null;
        recol_s = null;
        recol_d = null;
        width = 1;
        length = 1;
        blockwalk = true;
        blockrange = true;
        active = false;
        hillskew = false;
        sharelight = false;
        occlude = false;
        anim = -1;
        wallwidth = 16;
        ambient = 0;
        contrast = 0;
        op = null;
        animHasAlpha = false;
        mapfunction = -1;
        mapscene = -1;
        mirror = false;
        shadow = true;
        resizex = 128;
        resizey = 128;
        resizez = 128;
        forceapproach = 0;
        offsetx = 0;
        offsety = 0;
        offsetz = 0;
        forcedecor = false;
    }

    public void decode(Packet dat)
    {
        var active = -1;

        while (true)
        {
            var code = dat.g1();
            if (code == 0) break;

            if (code == 1)
            {
                var count = dat.g1();
                shapes = new int[count];
                models = new int[count];

                for (var i = 0; i < count; i++)
                {
                    models[i] = dat.g2();
                    shapes[i] = dat.g1();
                }
            }
            else if (code == 2)
            {
                name = dat.gjstr();
            }
            else if (code == 3)
            {
                desc = dat.gjstr();
            }
            else if (code == 14)
            {
                width = dat.g1();
            }
            else if (code == 15)
            {
                length = dat.g1();
            }
            else if (code == 17)
            {
                blockwalk = false;
            }
            else if (code == 18)
            {
                blockrange = false;
            }
            else if (code == 19)
            {
                active = dat.g1();

                if (active == 1) this.active = true;
            }
            else if (code == 21)
            {
                hillskew = true;
            }
            else if (code == 22)
            {
                sharelight = true;
            }
            else if (code == 23)
            {
                occlude = true;
            }
            else if (code == 24)
            {
                anim = dat.g2();
                if (anim == 65535) anim = -1;
            }
            else if (code == 25)
            {
                animHasAlpha = true;
            }
            else if (code == 28)
            {
                wallwidth = dat.g1();
            }
            else if (code == 29)
            {
                ambient = byte.Parse($"{dat.g1b()}");
            }
            else if (code == 39)
            {
                contrast = byte.Parse($"{dat.g1b()}");
            }
            else if (code >= 30 && code < 39)
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
                mapfunction = dat.g2();
            }
            else if (code == 62)
            {
                mirror = true;
            }
            else if (code == 64)
            {
                shadow = false;
            }
            else if (code == 65)
            {
                resizex = dat.g2();
            }
            else if (code == 66)
            {
                resizey = dat.g2();
            }
            else if (code == 67)
            {
                resizez = dat.g2();
            }
            else if (code == 68)
            {
                mapscene = dat.g2();
            }
            else if (code == 69)
            {
                forceapproach = dat.g1();
            }
            else if (code == 70)
            {
                offsetx = dat.g2b();
            }
            else if (code == 71)
            {
                offsety = dat.g2b();
            }
            else if (code == 72)
            {
                offsetz = dat.g2b();
            }
            else if (code == 73)
            {
                forcedecor = true;
            }
            else
            {
                Console.WriteLine("Error unrecognised loc config code: " + code);
            }
        }

        if (shapes == null) shapes = new int[0];

        if (active == -1)
        {
            this.active = shapes.Length > 0 && shapes[0] == 10;

            if (op != null) this.active = true;
        }
    }

    public Model getModel(int shape, int rotation, int heightmapSW, int heightmapSE, int heightmapNE, int heightmapNW,
        int transformId)
    {
        var shapeIndex = -1;
        for (var i = 0; i < shapes.Length; i++)
            if (shapes[i] == shape)
            {
                shapeIndex = i;
                break;
            }

        if (shapeIndex == -1) return null;

        var bitset = ((long)index << 6) + ((long)shapeIndex << 3) + rotation + ((long)(transformId + 1) << 32);
        if (_reset) bitset = 0L;

        var cached = (Model)modelCacheDynamic.get(bitset);
        if (cached != null)
        {
            if (_reset) return cached;

            if (hillskew || sharelight) cached = new Model(cached, hillskew, sharelight);

            if (hillskew)
            {
                var groundY = (heightmapSW + heightmapSE + heightmapNE + heightmapNW) / 4;

                for (var i = 0; i < cached.vertexCount; i++)
                {
                    var x = cached.vertexX[i];
                    var z = cached.vertexZ[i];

                    var heightS = heightmapSW + (heightmapSE - heightmapSW) * (x + 64) / 128;
                    var heightN = heightmapNW + (heightmapNE - heightmapNW) * (x + 64) / 128;
                    var y = heightS + (heightN - heightS) * (z + 64) / 128;

                    cached.vertexY[i] += y - groundY;
                }

                cached.calculateBoundsY();
            }

            return cached;
        }

        if (shapeIndex >= models.Length) return null;

        var modelId = models[shapeIndex];
        if (modelId == -1) return null;

        var flipped = mirror ^ (rotation > 3);
        if (flipped) modelId += 65536;

        var model = (Model)modelCacheStatic.get(modelId);
        if (model == null)
        {
            model = new Model(modelId & 0xFFFF);
            if (flipped) model.rotateY180();
            modelCacheStatic.put(modelId, model);
        }

        var scaled = resizex != 128 || resizey != 128 || resizez != 128;
        var translated = offsetx != 0 || offsety != 0 || offsetz != 0;

        var modified = new Model(model, recol_s == null, !animHasAlpha,
            rotation == 0 && transformId == -1 && !scaled && !translated);
        if (transformId != -1)
        {
            modified.createLabelReferences();
            modified.applyTransform(transformId);
            modified.labelFaces = null;
            modified.labelVertices = null;
        }

        while (rotation-- > 0) modified.rotateY90();

        if (recol_s != null)
            for (var i = 0; i < recol_s.Length; i++)
                modified.recolor(recol_s[i], recol_d[i]);

        if (scaled) modified.scale(resizex, resizey, resizez);

        if (translated) modified.translate(offsety, offsetx, offsetz);

        modified.calculateNormals(ambient + 64, contrast * 5 + 768, -50, -10, -50, !sharelight);

        if (blockwalk) modified.objRaise = modified.maxY;

        modelCacheDynamic.put(bitset, modified);

        if (hillskew || sharelight) modified = new Model(modified, hillskew, sharelight);

        if (hillskew)
        {
            var groundY = (heightmapSW + heightmapSE + heightmapNE + heightmapNW) / 4;

            for (var i = 0; i < modified.vertexCount; i++)
            {
                var x = modified.vertexX[i];
                var z = modified.vertexZ[i];

                var heightS = heightmapSW + (heightmapSE - heightmapSW) * (x + 64) / 128;
                var heightN = heightmapNW + (heightmapNE - heightmapNW) * (x + 64) / 128;
                var y = heightS + (heightN - heightS) * (z + 64) / 128;

                modified.vertexY[i] += y - groundY;
            }

            modified.calculateBoundsY();
        }

        return modified;
    }
}