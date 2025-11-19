namespace RoguelikeSystem
{
    /// <summary>
    /// Strategy for unit possession constraints.
    /// Check-only constraint - verifies if player owns specific unit without consuming it.
    /// Uses default AfterAction implementation (no resource consumption).
    /// </summary>
    public class UnitConstrictStrategy : IConstrictStrategy
    {
        public bool IsUsable(string name, int needAmount)
        {
            // TODO: Implement unit possession checking logic
            // Example: Check if player has unit with given name/ID
            // needAmount could represent minimum count or rarity threshold
            // For now, returns true as placeholder
            return true;
        }

        // AfterAction uses default implementation (does nothing)
    }
}
