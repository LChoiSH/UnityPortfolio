namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy for attack speed modification effects.
    /// Handles attack speed buffs/debuffs for units.
    /// </summary>
    public class AttackSpeedEffectStrategy : IEffectStrategy
    {
        public void Execute(EffectArgs args)
        {
            // TODO: Implement actual attack speed modification logic
            // Example: Modify unit's attack speed stat
            // For now, logs to debug panel
            LogPanel.Instance?.Log($"Click AttackSpeed {args.Str(0)}");
        }
    }
}
