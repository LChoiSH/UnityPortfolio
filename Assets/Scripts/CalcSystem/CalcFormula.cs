using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace CalcSystem
{
    /// <summary>
    /// Immutable value object representing a calculation formula.
    /// Uses readonly struct for value semantics and guaranteed immutability.
    /// </summary>
    public readonly struct CalcFormula
    {
        public string Id { get; }
        public double Value { get; }
        public CalcOperator Op { get; }

        public CalcFormula(string id, double value, CalcOperator op)
        {
            Id = id;
            Value = value;
            Op = op;
        }

        public override string ToString()
        {
            return $"{Id} {Value} {Op}";
        }
    }
}