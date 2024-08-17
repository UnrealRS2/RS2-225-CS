using System.Net;
using RS2_225.jagex2.config;
using RS2_225.jagex2.graphics;
using RS2_225.jagex2.io;
using RS2_225.java;
using RS2_225.sign;
using Math = RS2_225.java.Math;

namespace RS2_225;

public class client
{
    public static int nodeId = 10;
    public static int portOffset = 0;
    public static bool members = true;
    private static readonly CRC32 crc32 = new();
    private static Jagfile? archiveTitle;
    public static PixFont fontPlain11;

    public static PixFont fontPlain12;

    public static PixFont fontBold12;

    public static PixFont fontQuill8;

    public static ManualResetEvent ioThreadLoaded = new(false);

    private static PixMap imageTitle2;

    private static PixMap imageTitle3;

    private static PixMap imageTitle4;

    private static PixMap imageTitle0;

    private static PixMap imageTitle1;

    private static PixMap imageTitle5;

    private static PixMap imageTitle6;

    private static PixMap imageTitle7;

    private static PixMap imageTitle8;

    private static Pix8 imageTitlebox;

    private static Pix8 imageTitlebutton;

    private static Pix8[] imageRunes;

    private static int[] flameGradient;

    private static int[] flameGradient0;

    private static int[] flameGradient1;

    private static int[] flameGradient2;

    private static int[] flameBuffer0;

    private static int[] flameBuffer1;

    private static int[] flameBuffer3;

    private static int[] flameBuffer2;

    private static bool flameActive;

    private static bool flamesThread;

    private static PixMap areaSidebar;

    private static PixMap areaMapback;

    private static PixMap areaViewport;

    private static PixMap areaChatback;

    private static PixMap areaBackbase1;

    private static PixMap areaBackbase2;

    private static PixMap areaBackhmid1;


    protected void load()
    {
    }

    public static Jagfile? loadArchive(string name /*, int crc*/, string displayName, int displayProgress)
    {
        var retry = 5;
        var data = signlink.cacheload(name);
        if (data != null)
        {
            crc32.reset();
            crc32.update(data);
            var value = crc32.getValue();
            /*if (value != crc) {
                data = null;
            }*/
        }

        if (data != null) return new Jagfile(data);

        Console.WriteLine("ERROR: Failed to load: " + name);
        return null;
    }

    public static void Main(string[] args)
    {
        var startup = DateTime.Now;

        Console.WriteLine("RS2 Cache - release #" + signlink.clientversion);
        Console.WriteLine();
        //Start IO Thread and await
        signlink.startpriv(IPAddress.Loopback);
        ioThreadLoaded.WaitOne();

        try
        {
            Console.WriteLine();
            Console.WriteLine("---Loading Title Screen---");
            archiveTitle = loadArchive("title", "title screen", 10);
            Console.WriteLine();
            fontPlain11 = new PixFont(archiveTitle, "p11");
            fontPlain12 = new PixFont(archiveTitle, "p12");
            fontBold12 = new PixFont(archiveTitle, "b12");
            fontQuill8 = new PixFont(archiveTitle, "q8");
            loadTitle();
            Console.WriteLine("---Loading JagFiles---");
            var config = loadArchive("config", "config", 15)!;
            var inter = loadArchive("interface", "interface", 20)!;
            var media = loadArchive("media", "2d graphics", 30)!;
            var models = loadArchive("models", "3d graphics", 40)!;
            var textures = loadArchive("textures", "textures", 60)!;
            var wordenc = loadArchive("wordenc", "chat system", 65)!;
            var sounds = loadArchive("sounds", "sound effects", 70)!;
            Console.WriteLine();

            Console.WriteLine("---Unpacking Textures---");
            Pix3D.unpackTextures(textures);
            Pix3D.setBrightness(0.8D);
            Pix3D.initPool(20);
            Console.WriteLine();

            Console.WriteLine("---Unpacking Models---");
            Model.unpack(models);
            AnimBase.unpack(models);
            AnimFrame.unpack(models);
            Console.WriteLine();

            Console.WriteLine("---Unpacking Configs---");
            SeqType.unpack(config);
            LocType.unpack(config);
            FloType.unpack(config);
            ObjType.unpack(config);
            NpcType.unpack(config);
            IdkType.unpack(config);
            VarpType.unpack(config);
            Console.WriteLine();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        var finish = DateTime.Now;
        var time = finish - startup;
        Console.WriteLine("Loaded cache data in " + time.Milliseconds + "ms");

        //Will kill the io thread
        //signlink.threadliveid = null;
    }

    private static void loadTitleBackground()
    {
        /*var data = archiveTitle!.read("title.dat", null)!;
        Pix32 title = new Pix32(data);
        //imageTitle0.bind();
        title.blitOpaque(0, 0);

        //imageTitle1.bind();
        title.blitOpaque(-661, 0);

        //imageTitle2.bind();
        title.blitOpaque(-128, 0);

        //imageTitle3.bind();
        title.blitOpaque(-214, -386);

        //imageTitle4.bind();
        title.blitOpaque(-214, -186);

        //imageTitle5.bind();
        title.blitOpaque(0, -265);

        //imageTitle6.bind();
        title.blitOpaque(-574, -265);

        //imageTitle7.bind();
        title.blitOpaque(-128, -186);

        //imageTitle8.bind();
        title.blitOpaque(-574, -186);

        int[] mirror = new int[title.width];
        for ( int y = 0; y < title.height; y++) {
            for ( int x = 0; x < title.width; x++) {
                mirror[x] = title.pixels[title.width + title.width * y - x - 1];
            }

            if (title.width >= 0) {
                Array.Copy(mirror, 0, title.pixels, title.width * y, title.width);
            }
        }

        //imageTitle0.bind();
        title.blitOpaque(394, 0);

        //imageTitle1.bind();
        title.blitOpaque(-267, 0);

        //imageTitle2.bind();
        title.blitOpaque(266, 0);

        //imageTitle3.bind();
        title.blitOpaque(180, -386);

        //imageTitle4.bind();
        title.blitOpaque(180, -186);

        //imageTitle5.bind();
        title.blitOpaque(394, -265);

        //imageTitle6.bind();
        title.blitOpaque(-180, -265);

        //imageTitle7.bind();
        title.blitOpaque(212, -186);

        //imageTitle8.bind();
        title.blitOpaque(-180, -186);

        title = new Pix32(archiveTitle, "logo", 0);
        //imageTitle2.bind();
        title.draw(789 / 2 - title.width / 2 - 128, 18);
        title = null;

        // some null objects may force garbage collection threshold
        //Object dummy1 = null;
        //Object dummy2 = null;
        //System.gc();*/
    }

    private static void loadTitle()
    {
        if (imageTitle2 != null) return;

        //base.drawArea = null;
        areaChatback = null;
        areaMapback = null;
        areaSidebar = null;
        areaViewport = null;
        areaBackbase1 = null;
        areaBackbase2 = null;
        areaBackhmid1 = null;

        imageTitle0 = new PixMap(128, 265);
        Pix2D.clear();

        imageTitle1 = new PixMap(128, 265);
        Pix2D.clear();

        imageTitle2 = new PixMap(533, 186);
        Pix2D.clear();

        imageTitle3 = new PixMap(360, 146);
        Pix2D.clear();

        imageTitle4 = new PixMap(360, 200);
        Pix2D.clear();

        imageTitle5 = new PixMap(214, 267);
        Pix2D.clear();

        imageTitle6 = new PixMap(215, 267);
        Pix2D.clear();

        imageTitle7 = new PixMap(86, 79);
        Pix2D.clear();

        imageTitle8 = new PixMap(87, 79);
        Pix2D.clear();

        if (archiveTitle != null)
        {
            loadTitleBackground();
            loadTitleImages();
        }

        //redrawBackground = true;
    }

    private static void loadTitleImages()
    {
        imageTitlebox = new Pix8(archiveTitle, "titlebox", 0);
        imageTitlebutton = new Pix8(archiveTitle, "titlebutton", 0);
        imageRunes = new Pix8[12];
        for (var i = 0; i < 12; i++) imageRunes[i] = new Pix8(archiveTitle, "runes", i);
        /*imageFlamesLeft = new Pix32(128, 265);
        imageFlamesRight = new Pix32(128, 265);
        Array.Copy(imageTitle0.pixels, 0, imageFlamesLeft.pixels, 0, 33920);
        Array.Copy(imageTitle1.pixels, 0, imageFlamesRight.pixels, 0, 33920);*/
        flameGradient0 = new int[256];
        for (var i = 0; i < 64; i++) flameGradient0[i] = i * 262144;
        for (var i = 0; i < 64; i++) flameGradient0[i + 64] = i * 1024 + 16711680;
        for (var i = 0; i < 64; i++) flameGradient0[i + 128] = i * 4 + 16776960;
        for (var i = 0; i < 64; i++) flameGradient0[i + 192] = 16777215;
        flameGradient1 = new int[256];
        for (var i = 0; i < 64; i++) flameGradient1[i] = i * 1024;
        for (var i = 0; i < 64; i++) flameGradient1[i + 64] = i * 4 + 65280;
        for (var i = 0; i < 64; i++) flameGradient1[i + 128] = i * 262144 + 65535;
        for (var i = 0; i < 64; i++) flameGradient1[i + 192] = 16777215;
        flameGradient2 = new int[256];
        for (var i = 0; i < 64; i++) flameGradient2[i] = i * 4;
        for (var i = 0; i < 64; i++) flameGradient2[i + 64] = i * 262144 + 255;
        for (var i = 0; i < 64; i++) flameGradient2[i + 128] = i * 1024 + 16711935;
        for (var i = 0; i < 64; i++) flameGradient2[i + 192] = 16777215;
        flameGradient = new int[256];
        flameBuffer0 = new int[32768];
        flameBuffer1 = new int[32768];
        updateFlameBuffer(null);
        flameBuffer3 = new int[32768];
        flameBuffer2 = new int[32768];
        if (!flameActive)
        {
            flamesThread = true;
            flameActive = true;
            //startThread(this, 2);
        }
    }

    private static void updateFlameBuffer(Pix8 image)
    {
        short flameHeight = 256;
        for (var i = 0; i < flameBuffer0.Length; i++) flameBuffer0[i] = 0;

        for (var i = 0; i < 5000; i++)
        {
            var rand = (int)(Math.random() * 128.0D * flameHeight);
            flameBuffer0[rand] = (int)(Math.random() * 256.0D);
        }

        for (var i = 0; i < 20; i++)
        {
            for (var y = 1; y < flameHeight - 1; y++)
            for (var x = 1; x < 127; x++)
            {
                var index = x + (y << 7);
                flameBuffer1[index] = (flameBuffer0[index - 1] + flameBuffer0[index + 1] + flameBuffer0[index - 128] +
                                       flameBuffer0[index + 128]) / 4;
            }

            var last = flameBuffer0;
            flameBuffer0 = flameBuffer1;
            flameBuffer1 = last;
        }

        if (image != null)
        {
            var off = 0;

            for (var y = 0; y < image.height; y++)
            for (var x = 0; x < image.width; x++)
                if (image.pixels[off++] != 0)
                {
                    var x0 = x + image.cropX + 16;
                    var y0 = y + image.cropY + 16;
                    var index = x0 + (y0 << 7);
                    flameBuffer0[index] = 0;
                }
        }
    }
}