using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeckSystem
{
    [System.Serializable]
    public class UnitLevelStat
    {
        public double damage = 0;
        public double attackSpeed = 0;
        public double hp = 0;

        public double additionalDamage(int level, double baseDamage) => baseDamage * (Mathf.Pow((float)damage, level - 1) - 1);
        public double additionalAttackSpeed(int level, double baseAttackSpeed) => baseAttackSpeed * (Mathf.Pow((float)attackSpeed, level - 1) - 1);
        public double additionalHp(int level, double baseHp) => baseHp * (Mathf.Pow((float)hp, level - 1) - 1);

        
        // public void SetUnitLevelStat(Unit unit, int level)
        // {
        //     if (level <= 0) return;

        //     unit.Attacker.AddFormulaDamage(new CalcFormula("level_stat", (Mathf.Pow((float)damage, level - 1) - 1) * 100, MathCalc.AddInitialByPercent));
        //     unit.Attacker.AddFormulaAttackSpeed(new CalcFormula("level_stat", (Mathf.Pow((float)attackSpeed, level - 1) - 1) * 100, MathCalc.AddInitialByPercent));
        //     unit.Defender.AddFormulaHp(new CalcFormula("level_stat", (Mathf.Pow((float)hp, level - 1) - 1) * 100, MathCalc.AddInitialByPercent));
        // }
    }
}
