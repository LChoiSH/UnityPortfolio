using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeSystem;
using System.Linq;
using VInspector;
using DeckSystem;
using CurrencySystem;

public class DeckManager : MonoBehaviour, HaveSave, HaveLoad
{
    public static DeckManager Instance;
    private static readonly string PATH = "/DeckData.json";
    public bool IsLoaded { get; private set; } = false;

    private DeckData deckData;

    [SerializeField] private int deckCount = 5;
    [SerializeField] private List<UnitInfo> unitInfos = new List<UnitInfo>();

    [Header("Unit Level")]
    [SerializeField] private int[] needCard;
    [SerializeField] private int[] needGold;

    [Header("Unit Gacha")]
    [SerializeField] private Currency gachaCurrency;
    [SerializeField] private int[] gachaCosts = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 90 };
    [SerializeField] private Gacha<UnitTier> tierGacha;

    private Dictionary<string, UnitInfo> unitInfoDic;
    private Dictionary<UnitTier, Gacha<string>> unitTierGachas;

    public List<RogueEffect> AllRogueEffects => unitInfos.SelectMany(info => info.rogueEffects).ToList();
    public List<UnitInfo> UnitInfos => unitInfos;
    public List<UnitInfo> SelectedUnitInfos => deckData.selected.Select(id => unitInfoDic[id]).ToList();
    public UnitInfo UnitInfo(string id) => unitInfoDic.ContainsKey(id) ? unitInfoDic[id] : null;
    public Sprite UnitSprite(string id) => unitInfoDic[id].titleImage;
    public UserUnitInfo UserUnitInfo(string unitId)
    {
        if (!deckData.collected.ContainsKey(unitId)) deckData.collected[unitId] = new UserUnitInfo(); 

        return deckData.collected[unitId];
    }
    public UserUnitInfo UserUnitInfo(UnitInfo unitInfo) => UserUnitInfo(unitInfo.id);
    public UnitLevelStat LevelStat(string unitId) => UnitInfo(unitId).levelStat;

    public int NeedCard(int level) => needCard[level];
    public int NeedGold(int level) => needGold[level];
    public int HaveCount(UnitInfo unitInfo) => UserUnitInfo(unitInfo).Count;
    public int GachaNeedCost(int amount) => gachaCosts.Length < amount ? 1000 : gachaCosts[amount];
    public int CollectedCount => deckData.collected.Values.Where(userUnitInfo => userUnitInfo.Level > 0).ToArray().Length;
    public int UnCollectedCount => unitInfos.Count - deckData.collected.Values.Where(userUnitInfo => userUnitInfo.Level > 0).ToArray().Length;

    public System.Action onSelectedChanged;
    public System.Action<UnitInfo> onCollectedChanged;
    public System.Action<UnitInfo> onUnitChanged;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Load();
    }

    public void Save()
    {
        DataManager.SaveToFile<DeckData>(PATH, deckData);
    }

    public void Load()
    {
        unitInfoDic = new Dictionary<string, UnitInfo>();
        unitTierGachas = new Dictionary<UnitTier, Gacha<string>>();

        foreach (var gachaTierPair in tierGacha.GachaItems)
        {
            unitTierGachas[gachaTierPair.item] = new Gacha<string>();
        }

        foreach (UnitInfo unitInfo in unitInfos)
        {
            unitInfoDic[unitInfo.id] = unitInfo;
            if (!unitTierGachas.ContainsKey(unitInfo.tier)) continue;
            unitTierGachas[unitInfo.tier].AddGacha(unitInfo.id);
        }

        DeckData loaded = DataManager.LoadFromFile<DeckData>(PATH);

        if (loaded == null)
        {
            loaded = new DeckData();

            // add initial cards
            for(int i = 0; i < deckCount; i++)
            {
                loaded.collected[UnitInfos[i].id] = new UserUnitInfo(1, 0);
                loaded.selected.Add(UnitInfos[i].id);
            }
        }

        deckData = loaded;

        foreach (UnitInfo unitInfo in UnitInfos)
        {
            if (!deckData.collected.ContainsKey(unitInfo.id))
            {
                deckData.collected[unitInfo.id] = new UserUnitInfo();
            }
        }

        IsLoaded = true;
    }

    public void SetSelected(UnitInfo newSelect, int index)
    {
        if (deckData.selected.Count <= index)
        {
            Debug.LogError("out range");
            return;
        }

        int beforeIndex = deckData.selected.IndexOf(newSelect.id);

        if(beforeIndex != -1)
        {
            if (index == beforeIndex) return;

            string changingId = deckData.selected[index];
            deckData.selected[index] = newSelect.id;
            deckData.selected[beforeIndex] = changingId;
        }
        else
        {
            deckData.selected[index] = newSelect.id;
        }

        onSelectedChanged?.Invoke();

        Save();
    }

    public void TryLevelUp(UnitInfo unitInfo)
    {
        UserUnitInfo userUnitInfo = UserUnitInfo(unitInfo);

        int haveCount = HaveCount(unitInfo);
        int needCount = NeedCard(userUnitInfo.Level);
        int haveGold = (int)CurrencyManager.Instance.GetCurrencyAmount("gold");
        int needGold = NeedGold(userUnitInfo.Level);

        if(haveCount < needCount)
        {
            // TODO: add log ui
            // LogPanel.Instance.LocalizingLog("UI", "NeedMoreCard");
            return;
        }

        if( haveGold < needGold)
        {
            // TODO: add log ui
            // LogPanel.Instance.LocalizingLog("UI", "NeedMoreGold");
            return;
        }

        userUnitInfo.SetCount(userUnitInfo.Count - needCount);
        CurrencyManager.Instance.UseCurrency("gold", needGold);

        LevelUp(unitInfo);
    }

    private void LevelUp(UnitInfo unitInfo)
    {
        if(unitInfo == null || !deckData.collected.ContainsKey(unitInfo.id)) return;

        UserUnitInfo userUnitInfo = UserUnitInfo(unitInfo.id);
        userUnitInfo.SetLevel(userUnitInfo.Level + 1);

        Save();
        
        // TODO: add log ui
        // LogPanel.Instance?.Log(Localizing.GetFormat("UI", "UnitLevelUpSuccess", Localizing.GetFormat("Units", unitInfo.id)));

        onUnitChanged?.Invoke(unitInfo);
    }

    public void CollectUnit(UnitInfo unitInfo, int count)
    {
        if(!deckData.collected.ContainsKey(unitInfo.id))
        {
            deckData.collected[unitInfo.id] = new UserUnitInfo();
        }

        if (deckData.collected[unitInfo.id].Level == 0)
        {
            LevelUp(unitInfo);
            count -= 1;
            onCollectedChanged?.Invoke(unitInfo);
        }

        deckData.collected[unitInfo.id].AddCount(count);
        Save();
    }

    public List<Reward> UnitGacha(int gachaCount)
    {
        if (gachaCosts[gachaCount] > CurrencyManager.Instance.GetCurrencyAmount(gachaCurrency))
        {
            Debug.LogError("Need more scroll");
            return null;
        }

        CurrencyManager.Instance.UseCurrency(gachaCurrency, gachaCosts[gachaCount]);

        return UnitGachaRewards(gachaCount);
    }

    private List<Reward> UnitGachaRewards(int gachaCount)
    {
        List<Reward> rewardResult = new List<Reward>();

        for (int i = 0;i < gachaCount;i++)
        {
            UnitTier gachedTier = tierGacha.GetRandom();
            string gachedId = unitTierGachas[gachedTier].GetRandom();
            Reward newReward = new Reward(RewardType.Unit, gachedId, 1);
            rewardResult.Add(newReward);

            newReward.GetReward();
        }

        return rewardResult;
    }

#if UNITY_EDITOR

    [VInspector.Button]
    private void AddAllUnits()
    {
        DeckData deckData = new DeckData();

        foreach (UnitInfo unitInfo in UnitInfos)
        {
            if (!deckData.collected.ContainsKey(unitInfo.id))
            {
                deckData.collected[unitInfo.id] = new UserUnitInfo(1, 0);
            }
        }

        // add first cards
        for (int i = 0; i < deckCount; i++)
        {
            deckData.selected.Add(UnitInfos[i].id);
        }

        Debug.Log($"{deckData.collected.Count} {deckData.selected.Count}");


        DataManager.SaveToFile<DeckData>(PATH, deckData);
    }

    [VInspector.Button]
    private void RemoveData() => DataManager.RemoveFile(PATH);
#endif
}

