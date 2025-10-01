using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GachaPair<T>
{
    public T item;
    [Min(0f)]
    public float weight;

    public GachaPair(T item, float weight)
    {
        this.item = item;
        this.weight = Mathf.Max(0, weight);
    }

    public GachaPair(T item)
    {
        this.item = item;
        this.weight = 1;
    }
}
