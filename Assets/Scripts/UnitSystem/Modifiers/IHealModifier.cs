namespace UnitSystem
{
    public enum HealPhase { PreHeal, Amplification, PostHeal }

    public interface IHealModifier
    {
        string Name { get; }
        string Tag { get; }
        int Priority { get; }
        HealPhase Phase { get; }

        Heal Apply(Heal heal);
    }

    // Example implementation
    public sealed class BlessingBuff : IHealModifier
    {
        public string Name => "Blessing";
        public string Tag => "Buff:Blessing";
        public int Priority { get; } = 100;
        public HealPhase Phase { get; } = HealPhase.Amplification;

        private float _multiplier;

        public BlessingBuff(float multiplier)
        {
            _multiplier = multiplier;
        }

        public Heal Apply(Heal heal)
        {
            heal.finalAmount *= _multiplier;
            return heal;
        }
    }
}
