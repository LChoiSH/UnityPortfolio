namespace RoguelikeSystem
{
    public enum RogueConstrictType { Currency, Level, Unit }

    /// <summary>
    /// Data structure representing a constraint requirement.
    /// Holds only data - logic is delegated to IConstrictStrategy implementations.
    ///
    /// Design Decision: Changed from class to struct after introducing Strategy Pattern
    /// because RogueConstrict became a simple data holder with no behavior.
    /// Struct provides better memory efficiency in List collections (contiguous memory).
    ///
    /// Naming: Uses "Rogue" prefix for consistency with other system types
    /// (RogueEffect, RogueEffectPair, RogueTier, etc.)
    /// </summary>
    [System.Serializable]
    public struct RogueConstrictData
    {
        public RogueConstrictType type;
        public string name;
        public int needAmount;

        public RogueConstrictData(RogueConstrictType type, string name, int needAmount)
        {
            this.type = type;
            this.name = name;
            this.needAmount = needAmount;
        }
    }
}
