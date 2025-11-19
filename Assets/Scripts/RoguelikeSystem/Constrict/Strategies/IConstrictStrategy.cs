namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy interface for different constrict types.
    /// Implements Strategy Pattern to handle different constraint validation and execution logic.
    /// Each constrict type (Currency, Exp, Unit) has its own strategy implementation.
    ///
    /// Uses C# 8.0+ Default Interface Implementation to provide optional AfterAction hook.
    /// </summary>
    public interface IConstrictStrategy
    {
        /// <summary>
        /// Checks if the constrict can be used (e.g., has enough resources).
        /// </summary>
        /// <param name="name">Resource identifier (e.g., currency name, unit ID)</param>
        /// <param name="needAmount">Required amount or threshold</param>
        /// <returns>True if usable, false otherwise</returns>
        bool IsUsable(string name, int needAmount);

        /// <summary>
        /// Optional hook called after effect execution.
        /// Override this to implement resource consumption or other post-processing.
        /// Default implementation does nothing (for check-only constraints).
        /// </summary>
        /// <param name="name">Resource identifier</param>
        /// <param name="needAmount">Amount to consume or process</param>
        void AfterAction(string name, int needAmount)
        {
            // Default: No action (check-only constraint)
        }
    }
}
