using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;

namespace RoguelikeSystem
{
    public class RoguelikeGachaPool : MonoBehaviour
    {
        [SerializeField] private List<RogueEffect> effects;
        [SerializeField] private Gacha<RogueTier> tierGacha;

        [SerializeField] private bool isTierGacha = true;
        [SerializeField] private bool isEachTier = false;
        [SerializeField] private bool allowDuplicates = false;

        private Dictionary<RogueTier, Gacha<RogueEffect>> tierGachaDic;
        private int gachaAttempts = 0;
        public int GachaAttempts => gachaAttempts;
        public bool IsTierGacha => isTierGacha;
        public bool IsEachTier => isEachTier;
        public bool AllowDuplicates => allowDuplicates;

        private void Start()
        {
            SetGachas();
        }

        public void SetGachas()
        {
            tierGachaDic = new Dictionary<RogueTier, Gacha<RogueEffect>>();

            foreach (RogueTier tier in System.Enum.GetValues(typeof(RogueTier)).Cast<RogueTier>())
            {
                tierGachaDic[tier] = new Gacha<RogueEffect>();
            }

            foreach (RogueEffect effect in effects)
            {
                AddEffectToGacha(effect);
            }
        }

        public void AddEffectToGacha(RogueEffect effect, float weight = 1)
        {
            RogueEffect cloneEffect = effect.Clone();

            if (isTierGacha)
            {
                tierGachaDic[effect.tier].AddGacha(new GachaPair<RogueEffect>(cloneEffect, weight));
            }
            else
            {
                cloneEffect.tier = RogueTier.Common;
                tierGachaDic[RogueTier.Common].AddGacha(new GachaPair<RogueEffect>(cloneEffect, weight));
            }

            cloneEffect.onAction += () =>
            {
                if (cloneEffect.CurrentLimit >= cloneEffect.Limit) RemoveEffectFromGacha(cloneEffect);
            };
        }

        public void RemoveEffectFromGacha(RogueEffect effect)
        {
            tierGachaDic[effect.tier].RemoveItem(effect);
        }

        public List<RogueEffect> GetRandomEffects(int count)
        {
            gachaAttempts++;

            List<RogueEffect> result = new List<RogueEffect>();

            if (isTierGacha)
            {
                if (isEachTier)
                {
                    Dictionary<RogueTier, int> gachaCountDic = new Dictionary<RogueTier, int>();

                    for (int i = 0; i < count; i++)
                    {
                        RogueTier targetTier = tierGacha.GetRandom();

                        if (!gachaCountDic.ContainsKey(targetTier)) gachaCountDic[targetTier] = 0;
                        gachaCountDic[targetTier]++;
                    }

                    foreach (RogueTier tier in gachaCountDic.Keys)
                    {
                        result.AddRange(tierGachaDic[tier].GetRandomMultiple(gachaCountDic[tier], allowDuplicates));
                    }
                }
                else
                {
                    RogueTier targetTier = tierGacha.GetRandom();

                    result = tierGachaDic[targetTier].GetRandomMultiple(count, allowDuplicates);
                }
            }
            else
            {
                // common
                RogueTier targetTier = RogueTier.Common;

                result = tierGachaDic[targetTier].GetRandomMultiple(count, allowDuplicates);
            }

            return result;
        }

        public void SetTierGacha(bool isTierGacha)
        {
            if (this.isTierGacha != isTierGacha)
            {
                this.isTierGacha = isTierGacha;
                SetGachas();
            }
        }

        public void SetEachTier(bool isEachTier)
        {
            if(this.isEachTier != isEachTier) this.isEachTier = isEachTier;
        }

        public void SetAllowDuplicates(bool allowDuplicates)
        {
            if (this.allowDuplicates != allowDuplicates) this.allowDuplicates = allowDuplicates;                
        }

#if UNITY_EDITOR
        [ContextMenu("ReadRogue"), EditorButton]
        public async void ReadCSV()
        {
            string Path = "Assets/CSV/Roguelike.csv";

            var CSV = await CSVReader.ReadByAddressablePathAsync(Path);

            if (CSV == null)
            {
                Debug.LogError("There are not csv");
                return;
            }

            List<RogueEffect> newEffects = new List<RogueEffect>();

            foreach (var value in CSV)
            {
                newEffects.Add(RogueEffect.MakeEffect(value));
            }

            effects = newEffects;

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"Effects Read Success");
        }
#endif
    }
}