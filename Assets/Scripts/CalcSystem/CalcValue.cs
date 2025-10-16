using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

namespace CalcSystem
{
    public class CalcValue
    {
        private Dictionary<string, Formula> formulaDic = new();
        private bool _dirty = false;
        private double _cachedValue = 0;

        // cached
        private double initialValue = 0;
        private double initialPercent = 0;
        private double addPercent = 0;
        private double addValue = 0;
        private double multipleValue = 1;

        public bool allowMinusValue = false;

        public double Value
        {
            get
            {
                if (_dirty) CalcReturn();
                return !allowMinusValue && _cachedValue < 0 ? 0 : _cachedValue;
            }
        }

        public CalcValue()
        {
            AddFormula(new Formula("default", 0, Operator.AddInitial));
        }

        public CalcValue(double initialValue)
        {
            AddFormula(new Formula("default", initialValue, Operator.AddInitial));
        }

        public void CalcReturn()
        {
            double calcedInitial = initialValue * (1d + initialPercent * 0.01d) + addValue;

            _cachedValue = calcedInitial * (1d + addPercent * 0.01d) * multipleValue;

            _dirty = false;
        }

        public void AddFormula(string id, double value, Operator op)
        {
            AddFormula(new Formula(id, value, op));
        }

        public void AddFormula(Formula formula)
        {
            if (formulaDic.ContainsKey(formula.id)) RemoveFormula(formula.id);
            if (formula.value == 0) return;

            formulaDic[formula.id] = formula;

            switch (formula.op)
            {
                case Operator.AddInitial:
                    initialValue += formula.value;
                    break;
                case Operator.AddInitialByPercent:
                    initialPercent += formula.value;
                    break;
                case Operator.AddPercent:
                    addPercent += formula.value;
                    break;
                case Operator.Add:
                    addValue += formula.value;
                    break;
                case Operator.Multiple:
                    multipleValue *= formula.value;
                    break;
            }

            _dirty = true;
        }

        public void RemoveFormula(string id)
        {
            if (!formulaDic.ContainsKey(id)) return;
            Formula formula = formulaDic[id];

            switch (formula.op)
            {
                case Operator.AddInitial:
                    initialValue -= formula.value;
                    break;
                case Operator.AddInitialByPercent:
                    initialPercent -= formula.value;
                    break;
                case Operator.AddPercent:
                    addPercent -= formula.value;
                    break;
                case Operator.Add:
                    addValue -= formula.value;
                    break;
                case Operator.Multiple:
                    multipleValue /= formula.value;
                    break;
            }

            formulaDic.Remove(id);

            _dirty = true;
        }
    }
}