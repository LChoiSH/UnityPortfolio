using System;
using System.Collections.Generic;

namespace RoguelikeSystem
{
    /// <summary>
    /// Registry for constrict strategies using Registry Pattern + Strategy Pattern.
    /// Manages constrict strategy instances with Singleton caching for performance.
    ///
    /// Renamed from ConstrictStrategyFactory to RogueConstrictRegistry for:
    /// - Consistency with RogueEffectRegistry naming
    /// - Better semantic meaning (registry manages registered strategies)
    /// - Aligns with Registry Pattern terminology
    ///
    /// Uses singleton strategy instances for performance optimization:
    /// - Strategies are stateless, so they can be safely reused
    /// - Eliminates GC pressure from repeated instantiation
    /// - O(1) lookup via Dictionary
    /// </summary>
    public static class RogueConstrictRegistry
    {
        /// <summary>
        /// Cached strategy instances (strategies are stateless, can be reused).
        /// Initialized once at startup, no runtime allocation.
        /// </summary>
        private static readonly Dictionary<RogueConstrictType, IConstrictStrategy> strategyCache
            = new Dictionary<RogueConstrictType, IConstrictStrategy>
        {
            { RogueConstrictType.Currency, new CurrencyConstrictStrategy() },
            { RogueConstrictType.Level, new LevelConstrictStrategy() },
            { RogueConstrictType.Unit, new UnitConstrictStrategy() }
        };

        /// <summary>
        /// Retrieves a strategy instance for the given constrict type.
        /// Returns cached singleton instance for performance.
        /// </summary>
        /// <param name="type">The constrict type</param>
        /// <returns>Singleton strategy instance</returns>
        /// <exception cref="ArgumentException">If type is not supported</exception>
        public static IConstrictStrategy GetStrategy(RogueConstrictType type)
        {
            if (strategyCache.TryGetValue(type, out IConstrictStrategy strategy))
            {
                return strategy;
            }

            throw new ArgumentException($"Unsupported RogueConstrictType: {type}");
        }
    }
}
