using UnityEngine;
using UnityEngine.Localization;
using System;
using System.Linq;

namespace RoguelikeSystem
{
    [System.Serializable]
    public class RogueEffectPair
    {
        public RogueEffectCategory effectCategory;
        public EffectArgs args;

        public RogueEffectPair() { }

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
}