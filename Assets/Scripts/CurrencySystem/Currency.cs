using System;
using UnityEngine;

namespace CurrencySystem
{
    [CreateAssetMenu(menuName = "Currency", fileName = "Currency_")]
    [Serializable]
    public class Currency : ScriptableObject
    {
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private Sprite image;
        [SerializeField] private bool isPermanent = false; // 영구 재화 여부

        private CurrencyState currencyState;

        public string Title => title;
        public string Description => description;
        public Sprite SpriteImage => image;
        public bool IsPermanent => isPermanent;
        public long Amount => currencyState.amount;
        public long TotalAmount => currencyState.totalAmount;
        public CurrencyState CurrencyState => currencyState;

        public event Action<long> onAmountChanged;
        public event Action<long> onTotalAmountChanged;

        public void Earn(long earn)
        {
            currencyState.amount += earn;
            currencyState.totalAmount += earn;

            onAmountChanged?.Invoke(currencyState.amount);
            onTotalAmountChanged?.Invoke(currencyState.totalAmount);
        }

        public bool Use(long useAmount)
        {
            if (useAmount > currencyState.amount) return false;

            currencyState.amount -= useAmount;
            onAmountChanged?.Invoke(currencyState.amount);

            return true;
        }

        public void ResetCurrency(bool removeAction = false)
        {
            if (removeAction)
            {
                onAmountChanged = null;
                onTotalAmountChanged = null;
            }
            
            SetDefaultValue(0, 0);
        }

        public void SetDefaultValue(CurrencyState currencyState)
        {
            this.currencyState = currencyState;
        }

        public void SetDefaultValue(long amount, long totalAmount)
        {
            if (amount < 0) amount = 0;
            if (totalAmount < 0) totalAmount = 0;

            this.currencyState.amount = amount;
            this.currencyState.totalAmount = totalAmount;
        }
    }

    public static class CurrencyHelper
    {
        public static Currency Clone(this Currency source, long initialQuantity = 0, long initialTotal = 0)
        {
            Currency runtime = ScriptableObject.Instantiate(source);

#if UNITY_EDITOR
            // 에셋/씬에 저장 방지 (에러 발생 경우 있어서 추가 지우지 말 것)
            runtime.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
#endif

            runtime.SetDefaultValue(initialQuantity, initialTotal);

            return runtime;
        }
    }
}