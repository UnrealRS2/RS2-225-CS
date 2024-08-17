using System.Net;
using NUnit.Framework;
using RS2_225;
using RS2_225.jagex2.config;
using RS2_225.jagex2.graphics;
using RS2_225.sign;

namespace RS2_225_Test;

[TestFixture]
public class DecodeSeqTypesTest
{
    [Test]
    public void DecodeSeqTypes()
    {
        var ioThread = signlink.startpriv(IPAddress.Loopback);
        client.ioThreadLoaded.WaitOne();
        var config = client.loadArchive("config", "config", 15)!;
        var models = client.loadArchive("models", "3d graphics", 40)!;
        AnimBase.unpack(models);
        AnimFrame.unpack(models);
        SeqType.unpack(config);
        Assert.That(SeqType.instances!, Has.Length.EqualTo(1027));
        signlink.threadliveid = null;
    }
}