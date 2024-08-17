namespace RS2_225.jagex2.datastruct;

public class DoublyLinkList
{
    private readonly DoublyLinkable head = new();

    public DoublyLinkList()
    {
        head.next2 = head;
        head.prev2 = head;
    }

    public void push(DoublyLinkable node)
    {
        if (node.prev2 != null) node.uncache();

        node.prev2 = head.prev2;
        node.next2 = head;
        node.prev2.next2 = node;
        node.next2.prev2 = node;
    }

    public DoublyLinkable? pop()
    {
        var node = head.next2;
        if (node == head) return null;

        node.uncache();
        return node;
    }
}