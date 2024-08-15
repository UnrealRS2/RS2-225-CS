using System.Net;
using RS2_225.jagex2.config;
using RS2_225.jagex2.io;
using RS2_225.java;
using RS2_225.sign;

namespace RS2_225;

public class client
{
    public static int nodeId = 10;
    public static int portOffset = 0;
    public static bool members = true;
    private static CRC32 crc32 = new();
    
    protected void load()
    {
        
    }

    private static Jagfile? loadArchive(string name/*, int crc*/, string displayName, int displayProgress)
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
        
        if (data != null) {
            return new Jagfile(data);
        }
        
        Console.WriteLine("ERROR: Failed to load: " + name);
        return null;
    }
    
    public static void Main(string[] args)
    {
        var startup = DateTime.Now;
        
        Console.WriteLine("RS2 Cache - release #" + signlink.clientversion);
        signlink.startpriv(IPAddress.Loopback);
        try
        {
            var config = loadArchive("config", "config", 15)!;
            var inter = loadArchive("interface", "interface", 20)!;
            var media = loadArchive("media",  "2d graphics", 30)!;
            var models = loadArchive("models", "3d graphics", 40)!;
            var textures = loadArchive("textures", "textures", 60)!;
            var wordenc = loadArchive("wordenc", "chat system", 65)!;
            var sounds = loadArchive("sounds", "sound effects", 70)!;
            
            FloType.unpack(config);
            ObjType.unpack(config);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        var finish = DateTime.Now;
        var time = finish - startup;
        Console.WriteLine("Loaded cache data in " + time.Milliseconds + "ms");
    }
}