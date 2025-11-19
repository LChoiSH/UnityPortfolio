namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy for damage-related effects.
    /// Handles damage calculations and application to units.
    /// </summary>
    public class DamageEffectStrategy : IEffectStrategy
    {
        public void Execute(EffectArgs args)
        {
            // TODO: Implement actual damage logic
            // Example: Apply damage to player or enemies
            // For now, logs to debug panel
            LogPanel.Instance?.Log($"Click Damage {args.Str(0)}");
        }
    }
}
