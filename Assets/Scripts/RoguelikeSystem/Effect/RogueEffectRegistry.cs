using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeSystem
{
    /// <summary>
    /// Registry for effect strategies using Registry Pattern + Strategy Pattern.
    /// Manages effect strategy instances and executes effects with centralized error handling.
    ///
    /// Design evolution:
    /// - Initially: Dictionary-based handler mapping with static methods
    /// - Then: Separated Factory + Registry (unnecessary layer)
    /// - Now: Integrated Registry (YAGNI - Factory only used internally)
    ///
    /// Benefits of integration:
    /// - Eliminated unnecessary abstraction layer
    /// - Simpler, more direct code flow
    /// - Still demonstrates Strategy Pattern with Singleton caching
    /// </summary>
    public static class RogueEffectRegistry
    {
        /// <summary>
        /// Cached strategy instances (strategies are stateless, can be reused).
        /// Initialized once at startup, no runtime allocation.
        /// </summary>
        private static readonly Dictionary<RogueEffectCategory, IEffectStrategy> strategyCache
            = new Dictionary<RogueEffectCategory, IEffectStrategy>
        {
            { RogueEffectCategory.Damage, new DamageEffectStrategy() },
            { RogueEffectCategory.AttackSpeed, new AttackSpeedEffectStrategy() },
            { RogueEffectCategory.MoveSpeed, new MoveSpeedEffectStrategy() }
        };

        /// <summary>
        /// Executes the effect using the appropriate strategy.
        /// Retrieves strategy from cache and delegates execution with error handling.
        /// </summary>
        /// <param name="effectPair">Effect data containing category and arguments</param>
        public static void EffectAction(RogueEffectPair effectPair)
        {
            if (strategyCache.TryGetValue(effectPair.effectCategory, out IEffectStrategy strategy))
            {
                try
                {
                    strategy.Execute(effectPair.args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Effect execution error: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogError($"No strategy registered for effect category: {effectPair.effectCategory}");
            }
        }
    }
}