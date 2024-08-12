namespace RS2_225.jagex2.datastruct;

public class LinkList
{
    private Linkable sentinel;
    private Linkable? cursor = null;

    public LinkList()
    {
        var head = new Linkable();
        head.next = head;
        head.prev = head;
        sentinel = head;
    }

    public void addTail(Linkable node)
    {
        if (node.prev != null)
        {
            node.unlink();
        }
        node.prev = sentinel.prev;
        node.next = sentinel;
        if (node.prev != null)
        {
            node.prev.next = node;
        }
        node.next.prev = node;
    }

    public void addHead(Linkable node)
    {
        if (node.prev != null)
        {
            node.unlink();
        }
        node.prev = sentinel;
        node.next = sentinel.next;
        node.prev.next = node;
        if (node.next != null)
        {
            node.next.prev = node;
        }
    }

    public Linkable? removeHead()
    {
        var node = sentinel.next;
        if (node == sentinel)
            return null;
        node?.unlink();
        return node;
    }

    public Linkable? head()
    {
        var node = sentinel.next;
        if (node == sentinel)
        {
            cursor = null;
            return null;
        }
        cursor = node?.next;
        return node;
    }

    public Linkable? tail()
    {
        var node = sentinel.prev;
        if (node == sentinel)
        {
            cursor = null;
            return null;
        }
        cursor = node?.prev;
        return node;
    }

    public Linkable? next()
    {
        var node = cursor;
        if (node == sentinel)
        {
            cursor = null;
            return null;
        }
        cursor = node?.next;
        return node;
    }
    
    public Linkable? prev()
    {
        var node = cursor;
        if (node == sentinel)
        {
            cursor = null;
            return null;
        }
        cursor = node?.prev;
        return node;
    }

    public void clear()
    {
        while (true)
        {
            var node = sentinel.next;
            if (node == sentinel)
                return;
            node?.unlink();
        }
    }
}