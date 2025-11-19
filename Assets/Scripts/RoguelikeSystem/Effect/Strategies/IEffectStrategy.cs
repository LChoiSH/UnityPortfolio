namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy interface for different effect types.
    /// Implements Strategy Pattern to handle different effect execution logic.
    /// Each effect category (Damage, AttackSpeed, MoveSpeed) has its own strategy implementation.
    ///
    /// Design: Separates effect logic into individual files for better organization and extensibility.
    /// </summary>
    public interface IEffectStrategy
    {
        /// <summary>
        /// Executes the effect with given arguments.
        /// </summary>
        /// <param name="args">Effect parameters (e.g., damage amount, speed multiplier)</param>
        void Execute(EffectArgs args);
    }
}
