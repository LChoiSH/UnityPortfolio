namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy for movement speed modification effects.
    /// Handles movement speed buffs/debuffs for units.
    /// </summary>
    public class MoveSpeedEffectStrategy : IEffectStrategy
    {
        public void Execute(EffectArgs args)
        {
            // TODO: Implement actual move speed modification logic
            // Example: Modify unit's movement speed stat
            // For now, logs to debug panel
            LogPanel.Instance?.Log($"Click MoveSpeed {args.Str(0)}");
        }
    }
}
