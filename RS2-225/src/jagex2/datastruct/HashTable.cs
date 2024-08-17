using System.Numerics;

namespace RS2_225.jagex2.datastruct;

public class HashTable
{
    private readonly int bucketCount;
    private readonly Linkable[] buckets;

    public HashTable(int size)
    {
        buckets = new Linkable[size];
        bucketCount = size;
        for (var i = 0; i < size; i++) buckets[i] = new Linkable();
    }

    public Linkable? get(BigInteger key)
    {
        var start = buckets[(int)(key & new BigInteger(bucketCount - 1))];
        var next = start.next;
        if (next == null)
            return null;
        for (var node = start.next; node != start; node = node.next)
        {
            if (node == null)
                continue;
            if (node.key == key) return node;
        }

        return null;
    }

    public void put(BigInteger key, Linkable value)
    {
        if (value.prev != null)
            value.unlink();

        var sentinel = buckets[(int)(key & new BigInteger(bucketCount - 1))];
        value.prev = sentinel.prev;
        value.next = sentinel;

        if (value.prev != null)
            value.prev.next = value;
        value.next.prev = value;
        value.key = key;
    }
}