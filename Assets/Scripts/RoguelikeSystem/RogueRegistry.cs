using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeSystem
{
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
            LogPanel.Instance?.Log($"Click Damage {args.Str(0)}");
        }

        public static void AttackSpeed(EffectArgs args)
        {
            // TODO: add attackspeed code
            LogPanel.Instance?.Log($"Click AttackSpeed {args.Str(0)}");
        }

        public static void MoveSpeed(EffectArgs args)
        {
            // TODO: add movespeed code
            LogPanel.Instance?.Log($"Click MoveSpeed {args.Str(0)}");
        }

    }
}