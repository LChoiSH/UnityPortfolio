using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CurrencySystem;

public class CurrencyCanvas : MonoBehaviour
{
    public CurrencyItem currencyItem;
    public RectTransform wrap;

    void Start()
    {
        Clear();

        foreach (Currency currency in CurrencyManager.Instance.Currencies)
        {
            CurrencyItem madeItem = Instantiate(currencyItem, wrap);
            madeItem.SetCurrency(currency);
        }
    }
    
    private void Clear()
    {
        foreach(Transform child in wrap)
        {
            Destroy(child.gameObject);
        }
    }
}
