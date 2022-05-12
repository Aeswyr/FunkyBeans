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
        //int index = elements.Count / 2;
        //while (index != elements.Count
        //  && !(priority <= elements[index].Key && (index == 0 || priority >= elements[index - 1].Key))) {
        //    if (priority < elements[index].Key)
        //        index /= 2;
        //    else if (priority > elements[index].Key)
        //        index += (elements.Count - index) / 2 + 1;
        //}

        int index;
        for (index = 0; index < elements.Count; index++)
            if (priority <= elements[index].Key && (index == 0 || priority >= elements[index - 1].Key))
                break;

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