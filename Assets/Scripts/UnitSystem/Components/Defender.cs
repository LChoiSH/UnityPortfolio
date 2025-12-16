using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitSystem
{
    [RequireComponent(typeof(Unit))]
    public class Defender : MonoBehaviour
    {
        private Unit unit;
        private float currentHp;
        private List<IHitModifier> defenseModifiers = new();
        private List<IHealModifier> healModifiers = new();

        // Dirty Flag Pattern for performance optimization
        private List<IHitModifier> sortedDefenseModifiers = new();
        private List<IHealModifier> sortedHealModifiers = new();
        private bool isDefenseModifiersDirty = true;
        private bool isHealModifiersDirty = true;

        public int MaxHp => unit != null ? unit.Stat.maxHp : 0;
        public float CurrentHp => currentHp;
        public float HpRatio => MaxHp > 0 ? currentHp / (float)MaxHp : 0;
        public List<IHitModifier> DefenseModifiers => defenseModifiers;
        public List<IHealModifier> HealModifiers => healModifiers;

        public event Action<float> onCurrentHpChanged;
        public event Action<float> onMaxHpChanged;
        public event Action onDeath;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        private void Start()
        {
            ResetHp();
        }

        public void ResetHp()
        {
            currentHp = MaxHp;
            onMaxHpChanged?.Invoke(MaxHp);
            onCurrentHpChanged?.Invoke(currentHp);
        }

        public void Damaged(Hit hit)
        {
            // Sort modifiers only when changed (Dirty Flag Pattern)
            if (isDefenseModifiersDirty)
            {
                sortedDefenseModifiers = defenseModifiers
                    .OrderBy(m => m.Phase)
                    .ThenByDescending(m => m.Priority)
                    .ToList();
                isDefenseModifiersDirty = false;
            }

            // Apply sorted defense modifiers
            foreach (var modifier in sortedDefenseModifiers)
            {
                hit = modifier.Apply(hit);
            }

            if (hit.finalDamage <= 0) return;

            currentHp = Mathf.Max(0, currentHp - hit.finalDamage);
            onCurrentHpChanged?.Invoke(currentHp);

            // Execute post-hit callbacks
            if (hit.postCallbacks != null)
            {
                foreach (var callback in hit.postCallbacks)
                {
                    callback?.Invoke();
                }
            }

            if (currentHp <= 0)
            {
                onDeath?.Invoke();
            }
        }

        public void Healed(Heal heal)
        {
            // Sort modifiers only when changed (Dirty Flag Pattern)
            if (isHealModifiersDirty)
            {
                sortedHealModifiers = healModifiers
                    .OrderBy(m => m.Phase)
                    .ThenByDescending(m => m.Priority)
                    .ToList();
                isHealModifiersDirty = false;
            }

            // Apply sorted heal modifiers
            foreach (var modifier in sortedHealModifiers)
            {
                heal = modifier.Apply(heal);
            }

            if (heal.finalAmount <= 0) return;

            currentHp = Mathf.Min(MaxHp, currentHp + heal.finalAmount);
            onCurrentHpChanged?.Invoke(currentHp);

            // Execute post-heal callbacks
            if (heal.postCallbacks != null)
            {
                foreach (var callback in heal.postCallbacks)
                {
                    callback?.Invoke();
                }
            }
        }

        public void AddDefenseModifier(IHitModifier modifier)
        {
            defenseModifiers.Add(modifier);
            isDefenseModifiersDirty = true;
        }

        public void RemoveDefenseModifier(IHitModifier modifier)
        {
            defenseModifiers.Remove(modifier);
            isDefenseModifiersDirty = true;
        }

        public void ClearDefenseModifiers()
        {
            defenseModifiers.Clear();
            isDefenseModifiersDirty = true;
        }

        public void AddHealModifier(IHealModifier modifier)
        {
            healModifiers.Add(modifier);
            isHealModifiersDirty = true;
        }

        public void RemoveHealModifier(IHealModifier modifier)
        {
            healModifiers.Remove(modifier);
            isHealModifiersDirty = true;
        }

        public void ClearHealModifiers()
        {
            healModifiers.Clear();
            isHealModifiersDirty = true;
        }

        // --------- Editor Debug ---------
        [ContextMenu("Debug HP Info")]
        private void DebugHPInfo()
        {
            Debug.Log($"=== [{gameObject.name}] HP Debug ===\n" +
                      $"Current HP: {currentHp}\n" +
                      $"Max HP: {MaxHp}\n" +
                      $"HP Ratio: {HpRatio:P0}\n" +
                      $"Defense Modifiers: {defenseModifiers.Count}\n" +
                      $"Heal Modifiers: {healModifiers.Count}");
        }
    }
}