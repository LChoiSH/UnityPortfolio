
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UnitSystem
{
    public struct Hit
    {
        public Attacker attacker;
        public Defender defender;
        public float baseDamage;
        public float finalDamage;

        public List<Action> postCallbacks;

        public static Hit Create(Attacker attacker, Defender defender, float baseDamage)
        {
            return new Hit
            {
                attacker = attacker,
                defender = defender,
                baseDamage = baseDamage,
                finalDamage = baseDamage,
                postCallbacks = new List<Action>()
            };
        }
    }
}