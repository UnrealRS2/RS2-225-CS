namespace RS2_225.jagex2.datastruct;

public class LruCache
{
    private int available;

    private readonly int capacity;

    private readonly HashTable hashtable = new(1024);

    private readonly DoublyLinkList history = new();

    public LruCache(int size)
    {
        capacity = size;
        available = size;
    }

    public DoublyLinkable? get(long key)
    {
        var node = (DoublyLinkable?)hashtable.get(key);
        if (node != null) history.push(node);

        return node;
    }

    public void put(long key, DoublyLinkable value)
    {
        if (available == 0)
        {
            var node = history.pop();
            node!.unlink();
            node.uncache();
        }
        else
        {
            available--;
        }

        hashtable.put(key, value);
        history.push(value);
    }

    public void clear()
    {
        while (true)
        {
            var node = history.pop();
            if (node == null)
            {
                available = capacity;
                return;
            }

            node.unlink();
            node.uncache();
        }
    }
}