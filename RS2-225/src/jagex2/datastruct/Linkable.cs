using System.Numerics;

namespace RS2_225.jagex2.datastruct;

public class Linkable
{
    public BigInteger key;
    public Linkable? next;
    public Linkable? prev;

    public Linkable()
    {
        key = new BigInteger(0);
        next = this;
        prev = this;
    }

    public void unlink()
    {
        if (next == null || prev == null) return;
        prev.next = next;
        next.prev = prev;
        next = null;
        prev = null;
    }
}