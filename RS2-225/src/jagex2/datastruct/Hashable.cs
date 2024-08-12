namespace RS2_225.jagex2.datastruct;

public class Hashable : Linkable
{
    public Hashable? nextHashable;
    public Hashable? prevHashable;

    public Hashable()
    {
        nextHashable = this;
        prevHashable = this;
    }

    public void uncache()
    {
        if (nextHashable == null || prevHashable == null) return;
        prevHashable.nextHashable = nextHashable;
        nextHashable.prevHashable = prevHashable;
        nextHashable = null;
        prevHashable = null;
    }
}