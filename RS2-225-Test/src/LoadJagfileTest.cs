using System.Net;
using NUnit.Framework;
using RS2_225;
using RS2_225.sign;

namespace RS2_225_Test;

[TestFixture]
public class LoadJagfileTest
{
    [Test]
    public void LoadJagfile()
    {
        var ioThread = signlink.startpriv(IPAddress.Loopback);
        client.ioThreadLoaded.WaitOne();
        var title = client.loadArchive("title", "title screen", 10);
        Assert.That(title!.buffer, Has.Length.GreaterThan(0));
        signlink.threadliveid = null;
    }
}