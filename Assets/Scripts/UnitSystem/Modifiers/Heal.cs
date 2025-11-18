using System;
using System.Collections.Generic;

namespace UnitSystem
{
    public struct Heal
    {
        public Unit healer;
        public Defender target;
        public float baseAmount;
        public float finalAmount;

        public List<Action> postCallbacks;

        public static Heal Create(Unit healer, Defender target, float baseAmount)
        {
            return new Heal
            {
                healer = healer,
                target = target,
                baseAmount = baseAmount,
                finalAmount = baseAmount,
                postCallbacks = new List<Action>()
            };
        }
    }
}
