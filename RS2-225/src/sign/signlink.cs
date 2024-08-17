using System.Net;
using Microsoft.Win32.SafeHandles;
using Exception = System.Exception;
using Math = RS2_225.java.Math;

namespace RS2_225.sign;

public class signlink
{
    public static int clientversion = 225;
    public static int uid;
    public static int? threadliveid;
    public static bool active;
    private static int socketreq;
    private static Action? threadreq;
    private static string? dnsreq;
    private static string? loadreq;
    private static string? savereq;
    private static string? urlreq;
    private static IPAddress socketip;
    private static byte[]? loadbuf;

    public static string findcachedir()
    {
        var paths = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\rs2-225-cs\"
        };

        for (var i = 0; i < paths.Length; i++)
            try
            {
                var dir = paths[i];
                SafeFileHandle? cache = null;
                if (dir.Length > 0)
                    try
                    {
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Console.WriteLine("Unable to find or write to cache directory: " + dir);
                        continue;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Unauthorized access to cache directory: " + dir);
                        continue;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                Console.Error.WriteLine("Cache: " + dir);
                return dir;
            }
            catch (Exception _ex)
            {
            }

        return null;
    }

    public static int getuid(string? cacheDir)
    {
        if (cacheDir == null) return 0;

        var uidFilePath = cacheDir + "uid.dat";

        try
        {
            var uidFile = File.Open(uidFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            if (uidFile.Length < 4L)
            {
                var stream = new BinaryWriter(uidFile);
                var newUid = (int)(Math.random() * 9.9999999E7D);
                Console.WriteLine("Generated UID: " + newUid);
                stream.Write(newUid);
                stream.Close();
                stream.Dispose();
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        try
        {
            var uidFile = File.Open(uidFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            if (uidFile.Length == 4L)
            {
                var stream = new BinaryReader(uidFile);
                var uid = stream.ReadInt32();
                Console.WriteLine("UID: " + uid);
                stream.Close();
                stream.Dispose();
                return uid + 1;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return 0;
        }

        return 0;
    }
    
    static void Run()
    {

        //active = true;
        var cacheDir = findcachedir();
        uid = getuid(cacheDir);

        client.ioThreadLoaded.Set();

        var threadId = threadliveid;
        while (threadId == threadliveid)
            if (loadreq != null)
            {
                Console.WriteLine("Load jagfile: " + loadreq);
                loadbuf = null;
                try
                {
                    var file = File.Open(cacheDir + loadreq, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    var buffer = new byte[file.Length];
                    var result = file.Read(buffer, 0, buffer.Length);
                    if (result == file.Length)
                        loadbuf = buffer;
                    file.Dispose();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }

                loadreq = null;
            }

        Console.WriteLine("IO Thread stopped.");
    }

    public static Thread startpriv(IPAddress address)
    {
        threadliveid = (int)(Math.random() * 9.9999999E7D);
        if (active)
        {
            try
            {
                Thread.Sleep(500);
            }
            catch (Exception ignored)
            {
            }

            active = false;
        }

        socketreq = 0;
        threadreq = null;
        dnsreq = null;
        loadreq = null;
        savereq = null;
        urlreq = null;
        socketip = address;

        var thread = new Thread(Run)
        {
            IsBackground = false
        };
        thread.Start();
        return thread;
    }

    public static byte[]? cacheload(string name)
    {
        loadreq = name;
        while (loadreq != null)
            try
            {
                Thread.Sleep(1);
            }
            catch (Exception ignored)
            {
            }

        return loadbuf;
    }

    public static long gethash(string str)
    {
        var trimmed = str.Trim();
        var hash = 0L;

        for (var i = 0; i < trimmed.Length && i < 12; i++)
        {
            var c = trimmed.ToCharArray()[i];
            hash *= 37L;

            if (c >= 'A' && c <= 'Z')
                hash += c + 1 - 65;
            else if (c >= 'a' && c <= 'z')
                hash += c + 1 - 97;
            else if (c >= '0' && c <= '9') hash += c + 27 - 48;
        }

        return hash;
    }
}