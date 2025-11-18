using UnityEngine;

namespace UnitSystem
{
    public enum DamagePhase { PreHit, Mitigation, PostHit }

    public interface IHitModifier
    {
        public string Name { get; }
        public string Tag { get; }
        public int Priority { get; }
        public DamagePhase Phase { get; }

        public Hit Apply(Hit hit);
    }

    public sealed class RageBuff : IHitModifier
    {
        public string Name => "Rage";
        public string Tag => "Buff:Rage";
        public int Priority { get; } = 100;
        public DamagePhase Phase { get; } = DamagePhase.PreHit;

        private float _multiplier;

        public RageBuff(float multiplier)
        {
            _multiplier = multiplier;
        }

        public Hit Apply(Hit hit)
        {
            hit.finalDamage *= _multiplier;
            return hit;
        }
    }
}