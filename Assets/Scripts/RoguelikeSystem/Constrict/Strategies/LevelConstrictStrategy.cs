namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy for level-based constraints.
    /// Check-only constraint - does not consume or modify level after effect execution.
    /// Uses default AfterAction implementation (no resource consumption).
    /// </summary>
    public class LevelConstrictStrategy : IConstrictStrategy
    {
        public bool IsUsable(string name, int needAmount)
        {
            // TODO: Implement level checking logic
            // Example: Check if player level >= needAmount
            // For now, returns true as placeholder
            return true;
        }

        // AfterAction uses default implementation (does nothing)
    }
}
