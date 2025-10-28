using UnityEngine;

public enum TargetingKind
{
    Nearest,
    LowestHpRatio,
    
}

public interface ITargetingStrategy
{
    // ITargetable SelectTarget();
}