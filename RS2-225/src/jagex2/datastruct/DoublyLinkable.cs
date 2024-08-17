namespace RS2_225.jagex2.datastruct;

public class DoublyLinkable : Linkable
{
    public DoublyLinkable? next2;

    public DoublyLinkable? prev2;

    public void uncache()
    {
        if (prev2 != null)
        {
            prev2.next2 = next2;
            next2!.prev2 = prev2;
            next2 = null;
            prev2 = null;
        }
    }
}