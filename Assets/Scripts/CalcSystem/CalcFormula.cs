using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace CalcSystem
{
    public class Formula
    {
        public string id;
        public double value;
        public Operator op;
        public int order = 0;

        public Formula(string id, double value, Operator op)
        {
            this.id = id;
            this.value = value;
            this.op = op;
        }

        public override string ToString()
        {
            return $"{id} {value} {op}";
        }
    }
}