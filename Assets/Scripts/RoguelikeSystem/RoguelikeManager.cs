using System.Collections.Generic;
using UnityEngine;
using RoguelikeSystem;
using System.Linq;
using VInspector;
using UnityEditor;

public class RoguelikeManager : MonoBehaviour
{
    [SerializeField] private List<RogueEffect> effects;
    [SerializeField] private Gacha<RogueTier> tierGacha;
    [SerializeField] private int gachaChoiceCount = 3;

    private Dictionary<RogueTier, Gacha<RogueEffect>> tierGachaDic;

    [SerializeField] private bool useTier = true;
    [SerializeField] private bool allowDuplicates = false;
    [SerializeField] private bool isEachTier = false;

    private int gachaAttempts = 0;

    public float CurrentTotalWeight => tierGachaDic[RogueTier.Common].TotalWeight;
    public int GachaAttempts => gachaAttempts;

    private void Start()
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

        tierGachaDic[effect.tier].AddGacha(new GachaPair<RogueEffect>(cloneEffect, weight));

        cloneEffect.onAction += () =>
        {
            if(cloneEffect.CurrentLimit >= cloneEffect.Limit) RemoveEffectFromGacha(cloneEffect);
        };
    }

    public void RemoveEffectFromGacha(RogueEffect effect)
    {
        tierGachaDic[effect.tier].RemoveItem(effect);
    }

    public List<RogueEffect> GetRandomEffects() => GetRandomEffects(gachaChoiceCount);

    [Button]
    public List<RogueEffect> GetRandomEffects(int count)
    {
        List<RogueEffect> result = new List<RogueEffect>();
        HashSet<RogueEffect> uniqueSet = new HashSet<RogueEffect>();

        gachaAttempts++;

        if (!allowDuplicates)
        {
            // 중복 비허용일 때 가능한 최대 수 체크
            HashSet<RogueEffect> allCandidates = new HashSet<RogueEffect>();

            if (useTier && isEachTier)
            {
                // 모든 tier에서 candidate 모으기
                foreach (var entry in tierGachaDic.Values)
                {
                    allCandidates.UnionWith(entry.AllItems);
                }
            }
            else
            {
                // 공통 tier만 사용
                RogueTier tier = useTier ? tierGacha.GetRandom() : RogueTier.Common;
                if (!tierGachaDic.ContainsKey(tier))
                {
                    Debug.LogError($"Tier {tier} not found in tierGachaDic");
                    return result;
                }

                allCandidates.UnionWith(tierGachaDic[tier].AllItems);
            }

            if (allCandidates.Count < count)
            {
                Debug.LogWarning($"중복 없이 뽑을 수 있는 최대 수는 {allCandidates.Count}개입니다. 요청한 {count}개보다 적음.");
                count = allCandidates.Count;
            }
        }

        RogueTier cachedTier = useTier ? tierGacha.GetRandom() : RogueTier.Common;

        int attempts = 0;
        int maxAttempts = 1000;

        while (result.Count < count && attempts++ < maxAttempts)
        {
            RogueTier tier = useTier
                ? (isEachTier ? tierGacha.GetRandom() : cachedTier)
                : RogueTier.Common;

            RogueEffect picked = tierGachaDic[tier].GetRandom();

            if (allowDuplicates || uniqueSet.Add(picked))
            {
                result.Add(picked);
            }
        }
        
        return result;
    }

    public void AddGachaCount()
    {
        gachaChoiceCount++;
    }

#if UNITY_EDITOR
    [ContextMenu("ReadRogue"), Button]
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

    [Button]
    public void SetLimit(int limit)
    {
        foreach(RogueEffect effect in effects)
        {
            effect.limit = limit;
        }
    }
#endif
}
