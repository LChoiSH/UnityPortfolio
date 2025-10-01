using UnityEngine;
using System.Numerics;
using System.Collections.Generic;

namespace CurrencySystem
{
    public class CurrencyData
    {
        public Dictionary<string, CurrencyState> currencyStates;

        public CurrencyData()
        {
            currencyStates = new Dictionary<string, CurrencyState>();
        }
    }
}