using RS2_225.jagex2.graphics;
using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class SeqType
{
    private static int count;
    public static SeqType?[]? instances;
    private int[] delay;
    private int frameCount;
    private int[] frames;
    private int[] iframes;
    private int lefthand = -1;
    private int priority = 5;
    private int replaycount = 99;
    private int replayoff = -1;
    private int righthand = -1;
    private bool stretches;
    private int[] walkmerge;

    public static void unpack(Jagfile config)
    {
        var dat = new Packet(config.read("seq.dat", null));
        count = dat.g2();

        if (instances == null) instances = new SeqType[count];

        for (var id = 0; id < count; id++)
        {
            if (instances[id] == null) instances[id] = new SeqType();

            instances[id].decode(dat);
        }

        Console.WriteLine("Decoded " + instances.Length + " SeqType configs");
    }

    public void decode(Packet dat)
    {
        while (true)
        {
            var code = dat.g1();
            if (code == 0) break;

            if (code == 1)
            {
                frameCount = dat.g1();
                frames = new int[frameCount];
                iframes = new int[frameCount];
                delay = new int[frameCount];

                for (var i = 0; i < frameCount; i++)
                {
                    frames[i] = dat.g2();

                    iframes[i] = dat.g2();
                    if (iframes[i] == 65535) iframes[i] = -1;

                    delay[i] = dat.g2();
                    if (delay[i] == 0) delay[i] = AnimFrame.instances[frames[i]].delay;

                    if (delay[i] == 0) delay[i] = 1;
                }
            }
            else if (code == 2)
            {
                replayoff = dat.g2();
            }
            else if (code == 3)
            {
                var count = dat.g1();
                walkmerge = new int[count + 1];

                for (var i = 0; i < count; i++) walkmerge[i] = dat.g1();

                walkmerge[count] = 9999999;
            }
            else if (code == 4)
            {
                stretches = true;
            }
            else if (code == 5)
            {
                priority = dat.g1();
            }
            else if (code == 6)
            {
                // later RS (think RS3) this becomes mainhand
                righthand = dat.g2();
            }
            else if (code == 7)
            {
                // later RS (think RS3) this becomes offhand
                lefthand = dat.g2();
            }
            else if (code == 8)
            {
                replaycount = dat.g1();
            }
            else
            {
                Console.WriteLine("Error unrecognised seq config code: " + code);
            }
        }

        if (frameCount == 0)
        {
            frameCount = 1;

            frames = new int[1];
            frames[0] = -1;

            iframes = new int[1];
            iframes[0] = -1;

            delay = new int[1];
            delay[0] = -1;
        }
    }
}