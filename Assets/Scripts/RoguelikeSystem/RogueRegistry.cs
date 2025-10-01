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