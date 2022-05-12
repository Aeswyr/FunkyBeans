using System.Collections.Generic;
using System;
using UnityEngine;

public class PriorityQueue<T> {
    private List<KeyValuePair<int, T>> elements = new List<KeyValuePair<int, T>>();

    public void Put(T element, int priority) {
        if (Empty()) {
            elements.Add(new KeyValuePair<int, T>(priority, element));
            return;
        }

        // binary search for insertion index
        int left = 0, right = elements.Count;
        int index =(left + right) / 2;
        while (left < right) {
            if (priority < elements[index].Key)
                right = index;
            else if (priority > elements[index].Key)
                left  = index + 1;
            else
                break;
            index = (left + right) / 2;
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

    public void PrintCosts() {
        string output = "";
        for (int i = 0; i < elements.Count; i++)
            output += $"{elements[i].Key} ";
        Debug.Log(output);
    }
}