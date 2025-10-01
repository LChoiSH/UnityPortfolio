using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

namespace RoguelikeSystem
{
    [Serializable]
    public class RogueEffectPair
    {
        public RogueEffectCategory effectCategory;
        public EffectArgs args;

        public RogueEffectPair() {}

        public RogueEffectPair(RogueEffectCategory effectCategory, EffectArgs args)
        {
            this.effectCategory = effectCategory;
            this.args = args;
        }

        public RogueEffectPair(RogueEffectCategory effectCategory, string[] args)
        {
            this.effectCategory = effectCategory;
            this.args = new EffectArgs(args);
        }

        public string Description()
        {
            return GetLocalizedDescription(effectCategory, args);
        }

        public static string GetLocalizedDescription(RogueEffectCategory category, EffectArgs args)
        {
            var localized = new LocalizedString("RogueEffects", category.ToString());

            string template = localized.GetLocalizedString();

            return string.Format(template, (args.AllValue ?? Array.Empty<string>()).Cast<object>().ToArray());
        }

        public void Action()
        {
            RogueEffectRegistry.EffectAction(this);
        }
    }

    public static class RogueEffectRegistry // static
    {
        public static void EffectAction(RogueEffectPair effectPair) => effectMap[effectPair.effectCategory]?.Invoke(effectPair.args);

        public static Dictionary<RogueEffectCategory, Action<EffectArgs>> effectMap = new Dictionary<RogueEffectCategory, Action<EffectArgs>>()
        {
            { RogueEffectCategory.Damage, Damage },
            { RogueEffectCategory.AttackSpeed, AttackSpeed },
            { RogueEffectCategory.MoveSpeed, MoveSpeed },
        };

        public static void Damage(EffectArgs args)
        {
            // TODO: add damage code
        }

        public static void AttackSpeed(EffectArgs args)
        {
            // add attackspeed code
        }

        public static void MoveSpeed(EffectArgs args)
        {
            // add movespeed code
            // StatusManager.Instance.AddStatus(Status.MoveSpeed, new CalcFormula($"{Time.unscaledTime}_temp_rogueEffect", args.Int(0), MathCalc.AddInitialByPercent, true));
        }

    }
}