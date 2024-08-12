using NUnit.Framework;
using RS2_225.jagex2.datastruct;

namespace RS2_225_Test;

[TestFixture]
public class HashableTest
{
    [Test]
    public void Uncache_RemovesLinks_Correctly()
    {
        var a = new Hashable();
        var b = new Hashable();
        var c = new Hashable();

        a.nextHashable = b;
        b.prevHashable = a;
        
        b.nextHashable = c;
        c.prevHashable = b;

        a.uncache();

        Assert.Multiple(() =>
        {
            Assert.That(a.nextHashable, Is.Null, "a.nextHashable should be null.");
            Assert.That(a.prevHashable, Is.Null, "a.prevHashable should be null.");
            Assert.That(c, Is.EqualTo(b.nextHashable), "b.nextHashable should point to c.");
            Assert.That(b, Is.EqualTo(c.prevHashable), "c.prevHashable should point to b.");
        });
    }
}