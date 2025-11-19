using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace CalcSystem
{
    public class CalcValue
    {
        /// <summary>
        /// Tolerance for floating-point comparison (0.01% precision).
        /// Used to determine if values are effectively equal.
        /// </summary>
        private const double Epsilon = 0.0001d;

        private Dictionary<string, CalcFormula> formulaDic = new();
        private bool _dirty = false;
        private double _cachedValue = 0;

        // cached
        private double initialValue = 0;
        private double initialPercent = 0;
        private double addPercent = 0;
        private double addValue = 0;
        private double multipleValue = 1;

        /// <summary>
        /// Gets the calculated value.
        /// Returns the raw calculated result without any clamping.
        /// If you need to clamp the value (e.g., prevent negative values),
        /// do so externally: Math.Max(0, calcValue.Value)
        /// </summary>
        public double Value
        {
            get
            {
                if (_dirty) Calculate();
                return _cachedValue;
            }
        }

        /// <summary>
        /// Gets a readonly view of all formulas in this CalcValue.
        /// Useful for UI display, debugging, or iterating over all applied effects.
        /// </summary>
        public IReadOnlyDictionary<string, CalcFormula> Formulas => formulaDic;

        /// <summary>
        /// Creates an empty CalcValue with no formulas.
        /// Initial value will be 0 until formulas are added.
        /// </summary>
        public CalcValue()
        {
            // Start empty - formulas can be added as needed
        }

        /// <summary>
        /// Creates a CalcValue with an initial base value.
        /// </summary>
        /// <param name="initialValue">The base value to start with</param>
        public CalcValue(double initialValue)
        {
            // Only add if non-zero (skip neutral element)
            if (System.Math.Abs(initialValue) > Epsilon)
            {
                AddFormula(new CalcFormula("__initial__", initialValue, CalcOperator.AddInitial));
            }
        }

        /// <summary>
        /// Calculates the final value by rebuilding intermediate caches and computing the result.
        /// Formula: ((initialValue * (1 + initialPercent%)) + addValue) * (1 + addPercent%) * multipleValue
        /// This method is called lazily when Value is accessed and _dirty is true.
        /// </summary>
        private void Calculate()
        {
            // Reset all caches to neutral values
            initialValue = 0.0d;
            initialPercent = 0.0d;
            addPercent = 0.0d;
            addValue = 0.0d;
            multipleValue = 1.0d;

            // Rebuild caches from dictionary
            foreach (CalcFormula formula in formulaDic.Values)
            {
                switch (formula.Op)
                {
                    case CalcOperator.AddInitial:
                        initialValue += formula.Value;
                        break;
                    case CalcOperator.AddInitialByPercent:
                        initialPercent += formula.Value;
                        break;
                    case CalcOperator.AddPercent:
                        addPercent += formula.Value;
                        break;
                    case CalcOperator.Add:
                        addValue += formula.Value;
                        break;
                    case CalcOperator.Multiple:
                        multipleValue *= formula.Value;
                        break;
                }
            }

            // Calculate final value
            double calcedInitial = initialValue * (1.0d + initialPercent * 0.01d) + addValue;
            _cachedValue = calcedInitial * (1.0d + addPercent * 0.01d) * multipleValue;

            _dirty = false;
        }

        public void AddFormula(string id, double value, CalcOperator op)
        {
            AddFormula(new CalcFormula(id, value, op));
        }

        public void AddFormula(CalcFormula formula)
        {
            // Warn if overwriting existing formula
            if (formulaDic.ContainsKey(formula.Id))
            {
                var oldFormula = formulaDic[formula.Id];
                UnityEngine.Debug.LogWarning(
                    $"[CalcValue] Overwriting formula '{formula.Id}'. " +
                    $"Old: {oldFormula.Value} ({oldFormula.Op}) → " +
                    $"New: {formula.Value} ({formula.Op})");

                RemoveFormula(formula.Id);
            }

            // Skip neutral elements (0 for add operations, 1 for multiply)
            if (IsNeutralElement(formula.Op, formula.Value)) return;

            formulaDic[formula.Id] = formula;
            _dirty = true;
        }

        public void RemoveFormula(string id)
        {
            if (!formulaDic.ContainsKey(id)) return;

            formulaDic.Remove(id);
            _dirty = true;
        }

        /// <summary>
        /// Checks if a formula with the specified ID exists.
        /// </summary>
        /// <param name="id">The formula ID to check</param>
        /// <returns>True if a formula with the given ID exists, false otherwise</returns>
        public bool HasFormula(string id)
        {
            return formulaDic.ContainsKey(id);
        }

        /// <summary>
        /// Attempts to get a formula by its ID.
        /// </summary>
        /// <param name="id">The formula ID to retrieve</param>
        /// <param name="formula">The formula if found, default value otherwise</param>
        /// <returns>True if the formula was found, false otherwise</returns>
        public bool TryGetFormula(string id, out CalcFormula formula)
        {
            return formulaDic.TryGetValue(id, out formula);
        }

        /// <summary>
        /// Returns detailed debug information including all formulas and cached values.
        /// Useful for debugging calculation issues and understanding the calculation flow.
        /// </summary>
        /// <returns>A formatted string with all debug information</returns>
        public string GetDebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CalcValue Debug Info ===");
            sb.AppendLine($"Final Value: {Value}");
            sb.AppendLine($"Formula Count: {Formulas.Count}");
            sb.AppendLine();

            sb.AppendLine("Formulas (by operator):");
            foreach (var formula in Formulas.Values.OrderBy(f => f.Op))
            {
                sb.AppendLine($"  [{formula.Op}] {formula.Id}: {formula.Value}");
            }

            sb.AppendLine();
            sb.AppendLine("Cached Intermediate Values:");
            sb.AppendLine($"  initialValue: {initialValue}");
            sb.AppendLine($"  initialPercent: {initialPercent}%");
            sb.AppendLine($"  addValue: {addValue}");
            sb.AppendLine($"  addPercent: {addPercent}%");
            sb.AppendLine($"  multipleValue: {multipleValue}x");

            return sb.ToString();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Logs detailed debug information to the Unity console.
        /// Editor-only method for debugging. Calls are automatically removed in release builds.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogDebugInfo()
        {
            UnityEngine.Debug.Log(GetDebugInfo());
        }
#endif

        /// <summary>
        /// Determines if a value is a neutral element for the given operator.
        /// Uses epsilon comparison to handle floating-point precision issues.
        /// </summary>
        /// <param name="op">The operator to check against</param>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is neutral (has no effect when applied)</returns>
        private bool IsNeutralElement(CalcOperator op, double value)
        {
            if (op == CalcOperator.Multiple)
            {
                // 1.0 is neutral for multiplication (x * 1 = x)
                return System.Math.Abs(value - 1.0d) < Epsilon;
            }
            else
            {
                // 0.0 is neutral for addition (x + 0 = x)
                return System.Math.Abs(value) < Epsilon;
            }
        }

    }
}