using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeSystem
{
    public enum RogueConstrictType { Currency, Exp, Unit }

    [System.Serializable]
    public class RogueConstrict
    {
        [SerializeField] private RogueConstrictType type;
        [SerializeField] private string name;
        [SerializeField] private int needAmount;

        public int NeedAmount => needAmount;
        public RogueConstrictType Type => type;

        public RogueConstrict(RogueConstrictType rewardType, string name, int needAmount)
        {
            this.type = rewardType;
            this.name = name;
            this.needAmount = needAmount;
        }

        public RogueConstrict Clone()
        {
            return new RogueConstrict(type, name, needAmount);
        }

        public bool IsUsable()
        {
            switch (type)
            {
                case RogueConstrictType.Currency:
                    if (CurrencyManager.Instance == null) return false;
                    if (CurrencyManager.Instance.GetCurrencyAmount(name) < needAmount) return false;
                    return true;
            }

            return false;
        }

        public void UseConstrict()
        {
            switch (type)
            {
                case RogueConstrictType.Currency:
                    CurrencyManager.Instance.UseCurrency(name, needAmount);
                    break;
            }
        }
    }
}
