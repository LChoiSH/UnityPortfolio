using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;
using CurrencySystem;

public class CurrencyManager : MonoBehaviour, HaveSave, HaveLoad
{
    public static CurrencyManager Instance;
    private static readonly string PATH = "/currencyData.json";
    public bool IsLoaded { get; private set; } = false;

    // [SerializeField] private Currency[] currencies;
    [SerializeField] private CurrencyDatabaseSO currencyDatabase;

    private CurrencyData currencyData;
    private Dictionary<string, Currency> currencyDic = new Dictionary<string, Currency>();

    public Sprite GetCurrencyImage(string currencyName) => currencyDic[currencyName].Icon;
    public Currency FindCurrencyByTitle(string title) => currencyDic.ContainsKey(title) ? currencyDic[title] : null;
    public IReadOnlyList<Currency> Currencies => currencyDatabase.Items; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Load();
    }

    public void Save()
    {
        foreach (string currencyTitle in currencyDic.Keys)
        {
            currencyData.currencyStates[currencyTitle] = currencyDic[currencyTitle].CurrencyState;
        }

        DataManager.SaveToFile<CurrencyData>(PATH, currencyData);
        Debug.Log("currency save");
    }

    public void Load()
    {
        currencyData = DataManager.LoadFromFile<CurrencyData>(PATH);

        if (currencyData == null)
        {
            currencyData = new CurrencyData();
        }

        foreach (Currency currency in currencyDatabase.Items)
        {
            Currency madeCurrency = currency;

            if (!currencyData.currencyStates.ContainsKey(currency.Title) || !currency.IsPermanent)
            {
                currencyData.currencyStates[currency.Title] = new CurrencyState();
            }

            madeCurrency.SetDefaultValue(currencyData.currencyStates[currency.Title]);
            if (currency.IsPermanent) madeCurrency.onAmountChanged += (noUse) => Save();

            currencyDic[currency.Title] = madeCurrency;
        }

        IsLoaded = true;
        Debug.Log("Currency Data Loaded");
    }

    public long GetCurrencyAmount(string title) => currencyDic[title].Amount;

    public long GetCurrencyAmount(Currency currency) => currencyDic[currency.Title].Amount;

    public long GetCurrencyTotalAmount(string title) => currencyDic[title].TotalAmount;

    public long GetCurrencyTotalAmount(Currency currency) => currencyDic[currency.Title].TotalAmount;

    public bool UseCurrency(string title, long useAmount) => currencyDic[title].Use(useAmount);

    public bool UseCurrency(Currency currency, long useAmount) => UseCurrency(currency.Title, useAmount);

    public void EarnCurrency(string title, long amount)
    {
        if (amount < 0) return;

        currencyDic[title].Earn(amount);
    }
    public void EarnCurrency(Currency currency, long amount) => EarnCurrency(currency.Title, amount);

    public void AddActionCurrency(string title, Action<long> action, bool isTotal = false)
    {
        if (isTotal)
        {
            currencyDic[title].onTotalAmountChanged += action;
        }
        else
        {
            currencyDic[title].onAmountChanged += action;
        }
    }

    public void AddActionCurrency(Currency currency, Action<long> action, bool isTotal = false) => AddActionCurrency(currency.Title, action, isTotal);

    public void RemoveActionCurrency(string title, Action<long> action, bool isTotal = false)
    {
        if (isTotal)
        {
            currencyDic[title].onTotalAmountChanged -= action;
        }
        else
        {
            currencyDic[title].onAmountChanged -= action;
        }
    }

    public void RemoveActionCurrency(Currency currency, Action<long> action, bool isTotal = false) => RemoveActionCurrency(currency.Title, action, isTotal);

    public void ResetCurrency(Currency currency)
    {
        currency.ResetCurrency();
    }

    public void ResetCurrency(string currencyName) => currencyDic[currencyName].ResetCurrency();

    [Button, EditorButton]
    public void ResetCurrencies()
    {
        foreach (Currency currency in currencyDic.Values)
        {
            currency.ResetCurrency();
        }

        Save();
    }    

#if UNITY_EDITOR
    [Button]
    public void TestEarnCoin(Currency currency, int quantity)
    {
        EarnCurrency(currency, quantity);
    }
#endif
}

