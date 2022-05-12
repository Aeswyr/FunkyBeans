using System.Collections.Generic;
using System;

public class PriorityQueue<T> {
    private List<KeyValuePair<int, T>> elements = new List<KeyValuePair<int, T>>();

    public void Put(T element, int priority) {
        if (Empty()) {
            elements.Add(new KeyValuePair<int, T>(priority, element));
            return;
        }

        // binary search for insertion index
        int left = 0, right = elements.Count;
        int index = 0;
        while (left < right) {
            index = (left + right) / 2;
            if (priority < elements[index].Key)
                right = index;
            else if (priority > elements[index].Key)
                left  = index + 1;
            else
                break;
            
        }

        elements.Insert(index, new KeyValuePair<int, T>(priority, element));       
    }

    public T Pop() {
        var value = elements[0].Value;
        elements.RemoveAt(0);
        return value;
    }

    public void Clear() {
        elements.Clear();
    }

    public bool Empty() {
        return elements.Count == 0;
    }
}