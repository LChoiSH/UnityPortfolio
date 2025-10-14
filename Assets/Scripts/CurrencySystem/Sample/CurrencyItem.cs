using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CurrencySystem;

public class CurrencyItem : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI amountText;
    public Button earnButton;
    public Button useButton;
    public TMP_InputField inputField;

    private Currency currency;
    private int inputNum;

    public void SetCurrency(Currency currency)
    {
        if (currency == null || this.currency != null) return;

        this.currency = currency;

        image.sprite = currency.Icon;
        currency.onAmountChanged += OnCurrencyChanged;
        OnCurrencyChanged(currency.Amount);

        inputField.onEndEdit.AddListener(OnEndEdit);
        earnButton.onClick.AddListener(() => Earn(inputNum));
        useButton.onClick.AddListener(() => Use(inputNum));
    }

    void OnDestroy()
    {
        if(currency != null)
        {
            currency.onAmountChanged -= OnCurrencyChanged;
        }
    }

    private void OnCurrencyChanged(long amount)
    {
        amountText.text = currency.IsPermanent ? amount.ToString() : $"{amount}\n temporary";            
    }

    private void Earn(int earnNum = 10)
    {
        CurrencyManager.Instance.EarnCurrency(currency, earnNum);
    }

    private void Use(int useNum = 5)
    {
        CurrencyManager.Instance.UseCurrency(currency, useNum);
    }

    private void OnEndEdit(string raw)
    {
        int.TryParse(raw, out inputNum);
    }
}
