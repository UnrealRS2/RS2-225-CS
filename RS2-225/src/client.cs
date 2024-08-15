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
        Console.WriteLine("loadArchive: " + name);
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
        
        Console.WriteLine("RS2 cache loader - release #" + signlink.clientversion);
        signlink.startpriv(IPAddress.Loopback);
        try
        {
            var config = loadArchive("config", "config", 15);
            FloType.unpack(config);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        var finish = DateTime.Now;
        var time = finish - startup;
        Console.WriteLine("Loaded cache data in " + time.Milliseconds + "ms");
        if (args.Length > 0) {
            nodeId = int.Parse(args[0]);
        }
        
        if (args.Length > 1) {
            portOffset = int.Parse(args[1]);
        }
        
        //TODO: finish
        /*if (args.Length > 2 && args[2].Equals("lowmem")) {
            setLowMemory();
        } else {
            setHighMemory();
        }*/
        
        members = args.Length <= 3 || !args[3].Equals("free");
        

    }
}