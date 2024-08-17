using RS2_225.jagex2.io;

namespace RS2_225.jagex2.graphics;

public class AnimFrame
{
    public static AnimFrame[] instances;
    public AnimBase _base;
    public int delay;
    public int[] groups;
    public int length;
    public int[] x;
    public int[] y;
    public int[] z;

    public static void unpack(Jagfile models)
    {
        var head = new Packet(models.read("frame_head.dat", null));
        var tran1 = new Packet(models.read("frame_tran1.dat", null));
        var tran2 = new Packet(models.read("frame_tran2.dat", null));
        var del = new Packet(models.read("frame_del.dat", null));

        var total = head.g2();
        var count = head.g2();
        instances = new AnimFrame[count + 1];

        var groups = new int[500];
        var x = new int[500];
        var y = new int[500];
        var z = new int[500];

        for (var i = 0; i < total; i++)
        {
            var id = head.g2();
            var frame = instances[id] = new AnimFrame();
            frame.delay = del.g1();

            var baseId = head.g2();
            var _base = AnimBase.instances[baseId];
            frame._base = _base;

            var groupCount = head.g1();
            var lastGroup = -1;
            var current = 0;

            for (var j = 0; j < groupCount; j++)
            {
                var flags = tran1.g1();

                if (flags > 0)
                {
                    if (_base.types[j] != AnimBase.OP_BASE)
                        for (var group = j - 1; group > lastGroup; group--)
                            if (_base.types[group] == AnimBase.OP_BASE)
                            {
                                groups[current] = group;
                                x[current] = 0;
                                y[current] = 0;
                                z[current] = 0;
                                current++;
                                break;
                            }

                    groups[current] = j;

                    var defaultValue = 0;
                    if (_base.types[groups[current]] == AnimBase.OP_SCALE) defaultValue = 128;

                    if ((flags & 0x1) == 0)
                        x[current] = defaultValue;
                    else
                        x[current] = tran2.gsmart();

                    if ((flags & 0x2) == 0)
                        y[current] = defaultValue;
                    else
                        y[current] = tran2.gsmart();

                    if ((flags & 0x4) == 0)
                        z[current] = defaultValue;
                    else
                        z[current] = tran2.gsmart();

                    lastGroup = j;
                    current++;
                }
            }

            frame.length = current;
            frame.groups = new int[current];
            frame.x = new int[current];
            frame.y = new int[current];
            frame.z = new int[current];

            for (var j = 0; j < current; j++)
            {
                frame.groups[j] = groups[j];
                frame.x[j] = x[j];
                frame.y[j] = y[j];
                frame.z[j] = z[j];
            }
        }

        Console.WriteLine("Unpacked " + instances.Length + " AnimFrames");
    }
}