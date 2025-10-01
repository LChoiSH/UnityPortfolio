using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CurrencySystem;

public class CurrencyAutoFill : MonoBehaviour
{
    const string key = "LastSavedTime";

    [SerializeField] private Currency currency;
    [SerializeField] private float fillTime;
    [SerializeField] private int max;

    [Header("UI")]
    public TextMeshProUGUI remainTimeText;
    public TextMeshProUGUI maxText;

    private Coroutine fillCoroutine;
    private string Key => $"{key}_{currency.Title}";

    private void Start()
    {
        if(maxText != null) maxText.text = $"/{max}";

        OnAmountChanged(CurrencyManager.Instance.GetCurrencyAmount(currency));
        CurrencyManager.Instance.AddActionCurrency(currency, OnAmountChanged);

        if(!PlayerPrefs.HasKey(Key))
        {
            CurrencyManager.Instance.EarnCurrency(currency, max);

            return;
        }
        else
        {
            double diffTime = GetSecondsSinceLastSave();
            int fillAmount = (int)(diffTime / fillTime);

            RequestKey(fillAmount);
            float remainTime = fillTime - (float)diffTime % fillTime;

            if(remainTime > 0) fillCoroutine = StartCoroutine(CheckTime(remainTime));
        }
    }

    private void OnApplicationQuit()
    {
        SaveTime();
    }

    public void OnDestroy()
    {
        SaveTime();
        CurrencyManager.Instance.RemoveActionCurrency(currency, OnAmountChanged);
    }

    public void RequestKey(int amount)
    {
        int current = (int)CurrencyManager.Instance.GetCurrencyAmount(currency);
        if (max <= current) return;
        if (max < current + amount) amount = max - current;

        CurrencyManager.Instance.EarnCurrency(currency, amount);
    }

    private IEnumerator CheckTime(float remainTime = -1)
    {
        if (remainTime == -1) remainTime = fillTime;

        while(true)
        {
            remainTime -= 1;

            if (remainTime <= 0 && CurrencyManager.Instance.GetCurrencyAmount(currency) < max)
            {
                RequestKey(1);
                remainTime = fillTime;
            }

            int min = (int)remainTime / 60;
            remainTimeText.text = $"{min}:{(remainTime % 60).ToString("F0")}";

            yield return new WaitForSeconds(1);
        }
    }

    private void SaveTime()
    {
        DateTime now = DateTime.UtcNow;
        PlayerPrefs.SetString(Key, now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public void OnAmountChanged(long amount)
    {
        if(amount >= max)
        {
            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
            }

            remainTimeText.gameObject.SetActive(false);
        }
        else
        {
            if(fillCoroutine == null)
            {
                remainTimeText.gameObject.SetActive(true);

                StartCoroutine(CheckTime());
            }
        }
    }

    public double GetSecondsSinceLastSave()
    {
        if (!PlayerPrefs.HasKey(Key)) return -1;

        long binary = Convert.ToInt64(PlayerPrefs.GetString(Key));
        DateTime last = DateTime.FromBinary(binary);

        return (DateTime.UtcNow - last).TotalSeconds;
    }

#if UNITY_EDITOR
    [VInspector.Button]
    public void ResetTime() => PlayerPrefs.DeleteKey(Key);
#endif
}
