using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DeckSystem;

public enum RewardType { Currency, Exp, Unit }

[System.Serializable]
public class Reward
{
    [SerializeField] private RewardType rewardType;
    [SerializeField] private string name;
    [SerializeField] private int amount;

    public RewardType Type => rewardType;
    public string Name => name;
    public int Amount => amount;

    public Reward(Reward reward, float amountRatio = 1f)
    {
        this.rewardType = reward.Type;
        this.name = reward.Name;
        this.amount = Mathf.RoundToInt((float)reward.Amount * amountRatio);
    }

    public Reward(RewardType type, string name, int amount)
    {
        this.rewardType = type;
        this.name = name;
        this.amount = amount;
    }

    public Sprite Sprite()
    {
        switch (rewardType)
        {
            case RewardType.Currency:
                if (CurrencyManager.Instance.FindCurrencyByTitle(name) == null) break;
                return CurrencyManager.Instance.GetCurrencyImage(name);
            case RewardType.Exp:
                // TODO: add exp sprite
                // if (UserInfoManager.Instance == null) break;
                // return UserInfoManager.Instance.expSprite;
                return null;
            case RewardType.Unit:
                if (DeckManager.Instance == null) break;
                UnitInfo unitInfo = DeckManager.Instance.UnitInfo(name);
                return unitInfo.titleImage;
        }

        return null;
    }

    public void GetReward(float ratio = 1f)
    {
        int valueAmount = (int)((float)amount * ratio);

        switch (rewardType)
        {
            case RewardType.Currency:
                if (CurrencyManager.Instance.FindCurrencyByTitle(name) == null) return;
                CurrencyManager.Instance.EarnCurrency(name, valueAmount);
                break;
            case RewardType.Exp:
                // TODO: add exp reward
                // if (UserInfoManager.Instance == null) return;
                // UserInfoManager.Instance.AddExp(valueAmount);
                break;
            case RewardType.Unit:
                if (DeckManager.Instance == null) break;
                UnitInfo unitInfo = DeckManager.Instance.UnitInfo(name);
                if (unitInfo == null) break;
                DeckManager.Instance.CollectUnit(unitInfo, amount);
                break;
        }
    }

    public static List<Reward> CloneRewardList(List<Reward> rewards, float amountRatio)
    {
        return rewards.ConvertAll(reward => new Reward(reward, amountRatio));
    }

    public static List<Reward> RewardParse(string rawString)
    {
        List<Reward> rewards = new List<Reward>();

        char[] splitChar1 = new char[] { ',' };
        char[] splitChar2 = new char[] { ' ' };

        string[] rewardSplits = rawString.Split(splitChar1);

        foreach (string rewardSplit in rewardSplits)
        {
            string trimSplit = rewardSplit.Trim();
            string[] rewardStrings = trimSplit.Split(splitChar2);

            RewardType type = rewardStrings[0] == "gold" ? RewardType.Currency : RewardType.Exp;
            string name = rewardStrings[0];
            int amount = int.Parse(rewardStrings[1]);

            rewards.Add(new Reward(type, name, amount));
        }

        return rewards;
    }

    /// <param name="maxWave">stages last wave num</param>
    /// <param name="baseReward">base reward</param>
    /// <param name="stageSlope">increase by stage(ex. 0.25f -> per stage reward +25%)</param>
    /// <param name="waveSlope">increase by wave(ex, )</param>
    /// <param name="minWaveFloor">min reward</param>
    /// <param name="clearBonus">last wave clear(=stage clear) bonus</param>
    private static float stageSlope = 0.25f;
    private static float waveSlope = 4.0f;
    public static float CalcRewardByStage(int stage, int reachedWave, int maxWave, float clearBonus = 1.2f)
    {
        reachedWave = Mathf.Clamp(reachedWave - 1, 0, maxWave);

        // 1) Stage 선형 스케일
        float stageFactor = 1f + stageSlope * (stage - 1);

        // 2) Wave 지수 곡선 (0~1로 정규화)

        // exp는 double로 계산 후 float 캐스팅(정밀도/호환성)
        float p = Mathf.Clamp01((float)reachedWave / maxWave);
        double denom = Math.Exp(waveSlope) - 1.0;
        float waveFactor = (float)((Math.Exp(waveSlope * p) - 1.0) / denom);

        // 3) 클리어 보너스
        float bonus = (reachedWave >= maxWave) ? clearBonus : 1f;

        // * randomValue
        bonus *= UnityEngine.Random.Range(0.8f, 1.2f);

        // 4) 최종
        float reward = stageFactor * waveFactor * bonus;

        // 소수점 보상 싫으면 반올림
        // return Mathf.Max(0, Mathf.RoundToInt(reward));
        return reward;
    }

    public static List<Reward> CalcRewardsByStage(List<Reward> rewards, int stage, int reachedWave, int maxWave)
    {
        List<Reward> calcRewards = new List<Reward>();

        float ratio = CalcRewardByStage(stage, reachedWave, maxWave);
        Debug.Log($"currentStage: {stage} {reachedWave} {maxWave} {ratio}");

        foreach (Reward reward in rewards)
        {
            calcRewards.Add(new Reward(reward, ratio));
        }

        return calcRewards;
    }
}
