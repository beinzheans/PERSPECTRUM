using System.Collections.Generic;

/// <summary>
/// Implementation of a stack with enforced capacity using FIFO replacement (ie removing the bottom of the stack) when the stack is full
/// </summary>
/// <typeparam name="T"></typeparam>
public class LimitedStack<T>
{
    private LinkedList<T> linkedList = new();
    public readonly int Capacity;

    public LimitedStack(int capacity)
    {
        if (capacity == 0)
        {
            return;
        }

        Capacity = capacity;
    }

    public void Push(T value)
    {
        if (linkedList.Count >= Capacity)
        {
            linkedList.RemoveFirst();
        }

        linkedList.AddLast(value);
    }

    public bool TryPop(out T value)
    {
        if (linkedList.Count <= 0)
        {
            value = default;
            return false;
        }

        value = linkedList.Last.Value;
        linkedList.RemoveLast();
        return true;
    }

    public bool TryPeek(out T value)
    {
        if (linkedList.Count <= 0)
        {
            value = default;
            return false;
        }

        value = linkedList.Last.Value;
        return true;
    }

    public void Clear()
    {
        linkedList.Clear();
    }
    public int Count => linkedList.Count;
}
