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
        private List<IHitModifier> sortedAttackModifiers = new();
        private bool isModifiersDirty = true;

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

            // Sort modifiers only when changed (Dirty Flag Pattern)
            if (isModifiersDirty)
            {
                sortedAttackModifiers = attackModifiers
                    .OrderBy(m => m.Phase)
                    .ThenByDescending(m => m.Priority)
                    .ToList();
                isModifiersDirty = false;
            }

            // Apply sorted modifiers
            foreach (var modifier in sortedAttackModifiers)
            {
                hit = modifier.Apply(hit);
            }

            defender.Damaged(hit);
        }

        public void AddAttackModifier(IHitModifier modifier)
        {
            attackModifiers.Add(modifier);
            isModifiersDirty = true;
        }

        public void RemoveAttackModifier(IHitModifier modifier)
        {
            attackModifiers.Remove(modifier);
            isModifiersDirty = true;
        }

        public void ClearAttackModifiers()
        {
            attackModifiers.Clear();
            isModifiersDirty = true;
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