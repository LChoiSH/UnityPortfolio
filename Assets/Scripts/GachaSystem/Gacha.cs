using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class Gacha<T>
{
    [SerializeField]
    private List<GachaPair<T>> gachaPairs = new List<GachaPair<T>>();
    private HashSet<T> gachaItems = new HashSet<T>();
    private float _totalWeight;
    private bool _totalWeightDirty = true;

    public float TotalWeight
    {
        get
        {
            if (_totalWeightDirty)
            {
                float sum = 0f;
                for (int i = 0; i < gachaPairs.Count; i++)
                {
                    float w = gachaPairs[i].weight;
                    if (w > 0f) sum += w;
                }
                _totalWeight = sum;
                _totalWeightDirty = false;
            }
            return _totalWeight;
        }
    }

    public IReadOnlyList<GachaPair<T>> GachaItems => gachaPairs;
    public IEnumerable<T> AllItems => gachaPairs.Select(x => x.item).Distinct();
    public int Count => gachaPairs.Count;

    public void Clear()
    {
        gachaPairs.Clear();
        gachaItems.Clear();

        _totalWeight = 0f;
        _totalWeightDirty = false;
    }

    public void AddGacha(T item, float weight = 1f) => AddGacha(new GachaPair<T>(item, weight));

    public void AddGacha(GachaPair<T> gachaPair)
    {
        int index = gachaPairs.FindIndex(p => EqualityComparer<T>.Default.Equals(p.item, gachaPair.item));

        if (index >= 0)
        {
            gachaPairs[index] = new GachaPair<T>(gachaPair.item, gachaPairs[index].weight + gachaPair.weight);
        }
        else
        {
            gachaItems.Add(gachaPair.item);
            gachaPairs.Add(gachaPair);
        }

        _totalWeightDirty = true;
    }

    public void RemoveItem(T item)
    {
        int index = gachaPairs.FindIndex(p => EqualityComparer<T>.Default.Equals(p.item, item));

        if (index >= 0)
        {
            gachaPairs.RemoveAt(index);
            _totalWeightDirty = true;
        }
    }

    public T GetRandom()
    {
        float randomValue = UnityEngine.Random.Range(0, TotalWeight);

        foreach (var pair in gachaPairs)
        {
            randomValue -= pair.weight;
            if (randomValue <= 0) return pair.item;
        }

        // 이론상 도달불가
        // return default(T);
        Debug.LogWarning("Gacha selection failed. Returning default.");
        return gachaPairs.LastOrDefault().item;
    }

    public Dictionary<T, float> GetRates()
    {
        var result = new Dictionary<T, float>();
        if (TotalWeight <= 0) return result;

        foreach (var pair in gachaPairs)
        {
            result[pair.item] = (float)pair.weight / TotalWeight;
        }

        return result;
    }

    public List<T> GetAllItems()
    {
        return gachaPairs.Select(p => p.item).ToList();
    }

    public List<T> GetRandomMultiple(int count, bool allowDuplicate = false)
    {
        List<T> results = new List<T>();
        var tempGachaItems = new List<GachaPair<T>>(gachaPairs);

        int maxCount = Mathf.Min(count, tempGachaItems.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if(allowDuplicate)
            {
                 results.Add(GetRandom());
            }
            else
            {
                float totalWeight = tempGachaItems.Sum(x => x.weight);
                float randomValue = UnityEngine.Random.Range(0, totalWeight);

                for (int j = 0; j < tempGachaItems.Count; j++)
                {
                    randomValue -= tempGachaItems[j].weight;
                    if (randomValue <= 0)
                    {
                        results.Add(tempGachaItems[j].item);
                        tempGachaItems.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        return results;
    }
}
