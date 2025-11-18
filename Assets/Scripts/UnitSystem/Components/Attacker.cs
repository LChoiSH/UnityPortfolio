using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitSystem
{
    [RequireComponent(typeof(Unit))]
    public class Attacker : MonoBehaviour
    {
        private Unit unit;
        private List<IHitModifier> attackModifiers = new();

        public float Damage => unit != null ? unit.Stat.damage : 0;
        public List<IHitModifier> AttackModifiers => attackModifiers;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        public void Attack(Defender defender)
        {
            // Create hit with proper initialization
            Hit hit = Hit.Create(this, defender, Damage);

            // Apply attack modifiers sorted by Phase and Priority
            var sortedModifiers = attackModifiers
                .OrderBy(m => m.Phase)
                .ThenByDescending(m => m.Priority);

            foreach (var modifier in sortedModifiers)
            {
                hit = modifier.Apply(hit);
            }

            defender.Damaged(hit);
        }

        public void AddAttackModifier(IHitModifier modifier)
        {
            attackModifiers.Add(modifier);
        }

        public void RemoveAttackModifier(IHitModifier modifier)
        {
            attackModifiers.Remove(modifier);
        }

        public void ClearAttackModifiers()
        {
            attackModifiers.Clear();
        }

        // --------- Editor Debug ---------
        [ContextMenu("Debug Attack Info")]
        private void DebugAttackInfo()
        {
            Debug.Log($"=== [{gameObject.name}] Attack Debug ===\n" +
                      $"Base Damage: {Damage}\n" +
                      $"Attack Modifiers: {attackModifiers.Count}");
        }
    }
}