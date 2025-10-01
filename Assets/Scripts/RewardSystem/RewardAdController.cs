using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardAdController : MonoBehaviour
{
    const string key = "LastSavedTime";

    [SerializeField] private string id;
    [SerializeField] private float fillTime = 86400;
    [SerializeField] private int max = 1;
    [SerializeField] private Reward reward;

    [Header("UI")]
    public TextMeshProUGUI remainTimeText;
    public TextMeshProUGUI maxText;
    public TextMeshProUGUI amountText;
    public Button adShowButton;
    public GameObject successPopup;

    private int amount;
    private float remainTime;

    private string Key => $"{key}_{id}";
    private string TimeKey => $"{key}_{id}_time";

    private void Awake()
    {
        if(maxText != null) maxText.text = $"/{max}";

        int nowAmount = PlayerPrefs.GetInt(Key, max);

        double diffTime = GetSecondsSinceLastSave();
        int fillAmount = (int)(diffTime / fillTime);

        remainTime = fillTime - (float)diffTime % fillTime;

        SetAmount(nowAmount + fillAmount);

        adShowButton.onClick.AddListener(ShowAd);
    }

    private void OnEnable()
    {
        StartCoroutine(CheckTime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        SaveTime();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 현재 상태 저장
            PlayerPrefs.SetInt(Key, amount);
            SaveTime();
            PlayerPrefs.Save();
        }
        else
        {
            // 복귀: 지난 시간만큼 보충/남은시간 재계산
            double diff = GetSecondsSinceLastSave();
            int fillAmount = (int)(diff / fillTime);
            int newAmount = Mathf.Min(max, amount + fillAmount);
            SetAmount(newAmount);

            // 남은시간 재설정 (최대가 아니면 diff%로 세팅)
            remainTime = (newAmount < max) ? (fillTime - (float)(diff % fillTime)) : fillTime;

            // 텍스트 즉시 갱신
            if (remainTimeText)
            {
                if (newAmount < max) remainTimeText.text = FloatToTimeText(remainTime);
                remainTimeText.gameObject.SetActive(newAmount < max);
            }
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(Key, amount);
        SaveTime();
        PlayerPrefs.Save();
    }

    private void SetAmount(int newAmount)
    {
        if (max <= newAmount)
        {
            newAmount = max;
            remainTimeText.gameObject.SetActive(false);
        }
        else
        {
            remainTimeText.gameObject.SetActive(true);
        }

        adShowButton.interactable = newAmount > 0;

        amount = newAmount;
        PlayerPrefs.SetInt(Key, amount);
        amountText.text = amount.ToString();
    }

    private void Use(int useAmount)
    {
        if (amount - useAmount < 0) return;

        SetAmount(amount - useAmount);
    }

    private IEnumerator CheckTime()
    {
        while (true)
        {
            remainTimeText.text = FloatToTimeText(remainTime);

            if (amount >= max)
            {
                remainTime = fillTime;
                if (remainTimeText) remainTimeText.text = FloatToTimeText(remainTime);
                yield return new WaitUntil(() => amount < max);
                continue;
            }

            //yield return wait1sec;
            yield return new WaitForSecondsRealtime(1f);
            remainTime -= 1;

            if (remainTime <= 0)
            {
                SetAmount(amount+1);
                remainTime = fillTime;
            }

        }
    }

    private void SaveTime()
    {
        DateTime now = DateTime.UtcNow;
        PlayerPrefs.SetString(TimeKey, now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public double GetSecondsSinceLastSave()
    {
        if (!PlayerPrefs.HasKey(TimeKey)) return 0;

        long binary = Convert.ToInt64(PlayerPrefs.GetString(TimeKey));
        DateTime last = DateTime.FromBinary(binary);

        return (DateTime.UtcNow - last).TotalSeconds;
    }

    public static string FloatToTimeText(float time)
    {
        time = Mathf.Max(0f, time);
        int hour = (int)time / 3600;
        int min = (int)time % 3600 / 60;
        int sec = (int)time % 3600 % 60;

        if(hour > 0) return $"{hour:D2}:{min:D2}:{sec:D2}";

        return $"{min:D2}:{sec:D2}";
    }

    public void ShowAd()
    {
        // TODO: add ad action

        // GoogleAdsManager.Instance.ShowRewardedAd(reward, () =>
        // {
        //     FirebaseManager.Instance.LogEvent(
        //     "ShowReward",
        //     new Firebase.Analytics.Parameter("id", id)
        // );

        //     Use(1);
        //     if (successPopup != null) successPopup.gameObject.SetActive(true);
        //     else LogPanel.Instance.Log(Localizing.Get("UI", "BuySuccess"));
        // });
    }

#if UNITY_EDITOR
    [VInspector.Button]
    public void ResetTime()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.DeleteKey(TimeKey);
    }
#endif
}
