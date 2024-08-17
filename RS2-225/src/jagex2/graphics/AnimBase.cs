using RS2_225.jagex2.io;

namespace RS2_225.jagex2.graphics;

public class AnimBase
{
    public static int OP_BASE = 0;
    public static int OP_TRANSLATE = 1;
    public static int OP_ROTATE = 2;
    public static int OP_SCALE = 3;
    public static int OP_ALPHA = 5;

    public static AnimBase?[]? instances;
    public int[][] labels;
    private int length;
    public int[] types;

    public static void unpack(Jagfile models)
    {
        var head = new Packet(models.read("base_head.dat", null));
        var type = new Packet(models.read("base_type.dat", null));
        var label = new Packet(models.read("base_label.dat", null));

        var total = head.g2();
        var count = head.g2();
        instances = new AnimBase[count + 1];

        for (var i = 0; i < total; i++)
        {
            var id = head.g2();
            var length = head.g1();

            var types = new int[length];
            var labels = new int[length][];

            for (var g = 0; g < length; g++)
            {
                types[g] = type.g1();

                var labelCount = label.g1();
                labels[g] = new int[labelCount];

                for (var l = 0; l < labelCount; l++) labels[g][l] = label.g1();
            }

            instances[id] = new AnimBase
            {
                length = length,
                types = types,
                labels = labels
            };
        }

        Console.WriteLine("Unpacked " + instances.Length + " AnimBases");
    }
}