using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;
using CurrencySystem;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CurrencyManager : MonoBehaviour, HaveSave, HaveLoad
{
    public static CurrencyManager Instance;
    private static readonly string PATH = "/currencyData.json";
    public bool IsLoaded { get; private set; } = false;

    // [SerializeField] private Currency[] currencies;
    [SerializeField] private CurrencyDatabaseSO currencyDatabase;

    private CurrencyData currencyData;
    private Dictionary<string, Currency> currencyDic = new Dictionary<string, Currency>();

    // ========== Async Save Pattern Fields ==========
    [Header("Save Settings")]
    [SerializeField] private float debounceTime = 0.5f;  // 일반 대기 시간 (초)
    [SerializeField] private float maxWaitTime = 5.0f;   // 최대 대기 시간 (초) - 기아 상태 방지

    private bool isDirty = false; // 저장이 필요한지 여부 (Dirty Flag Pattern)
    private bool isSaving = false; // 현재 저장 중인지 여부 (Pending Save Pattern)
    private float? firstCallTime = null; // 첫 호출 시간 기록 (Max Wait Time)
    private CancellationTokenSource debounceCts; // Debounce 취소 토큰

    public Sprite GetCurrencyImage(string currencyName) => currencyDic[currencyName].Icon;
    public Currency FindCurrencyByTitle(string title) => currencyDic.ContainsKey(title) ? currencyDic[title] : null;
    public IReadOnlyList<Currency> Currencies => currencyDatabase.Items; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await LoadAsync();
    }

    private void OnDestroy()
    {
        // 앱 종료 시 pending save 처리
        debounceCts?.Cancel();
        debounceCts?.Dispose();

        // Dirty flag가 있으면 즉시 저장 (동기)
        if (isDirty)
        {
            SaveImmediate();
        }
    }

    // ========== Async Save Pattern Implementation ==========

    /// <summary>
    /// 저장 요청을 받습니다. Debouncing + Pending Save + Max Wait Time 패턴을 사용합니다.
    /// - Debouncing: 짧은 시간 내 여러 호출 시 마지막만 실행
    /// - Max Wait Time: 일정 시간 이상 대기 시 강제 저장 (기아 상태 방지)
    /// - Pending Save: I/O 진행 중이면 완료 후 재실행
    /// - Dirty Flag: 실제 변경사항이 있을 때만 저장
    /// </summary>
    public async void RequestSave()
    {
        isDirty = true; // 저장이 필요함을 표시

        // 1. 첫 호출 시간 기록 (Max Wait Time 계산용)
        if (firstCallTime == null) firstCallTime = Time.time;

        // 2. Max Wait Time 체크 - 너무 오래 기다렸으면 즉시 저장
        float waitedTime = Time.time - firstCallTime.Value;
        if (waitedTime >= maxWaitTime)
        {
            firstCallTime = null; // 타이머 리셋
            await ExecuteSave();
            return;
        }

        // 3. Debouncing: 기존 타이머 취소 및 새 타이머 시작
        debounceCts?.Cancel();
        debounceCts = new CancellationTokenSource();

        try
        {
            // 4. Debounce 대기 (남은 시간만큼만 대기)
            float remainingTime = maxWaitTime - waitedTime;
            float actualDebounceTime = Mathf.Min(debounceTime, remainingTime);

            await UniTask.Delay(TimeSpan.FromSeconds(actualDebounceTime), cancellationToken: debounceCts.Token);

            // 5. 저장 성공 → 타이머 리셋
            firstCallTime = null;

            // 6. 실제 저장 실행
            await ExecuteSave();
        }
        catch (OperationCanceledException)
        {
            // Debounce 취소됨 (새로운 RequestSave가 호출됨), 정상 동작
            // firstCallTime은 유지 (MaxWaitTime 계산 계속)
        }
    }

    /// <summary>
    /// 실제 저장 로직 실행 (Pending Save + Dirty Flag 체크)
    /// </summary>
    private async UniTask ExecuteSave()
    {
        // Pending Save: 저장 중이면 완료 대기
        while (isSaving)
        {
            await UniTask.Yield(); // 한 프레임 대기
        }

        // Dirty Flag 체크 후 저장
        if (isDirty)
        {
            isDirty = false;
            isSaving = true;

            try
            {
                await SaveAsync();
            }
            finally
            {
                isSaving = false; // 저장 완료 (에러 발생해도 플래그 해제)
            }
        }
    }

    /// <summary>
    /// 비동기로 데이터를 저장합니다.
    /// CPU Bound(직렬화, 암호화) + I/O Bound(파일 쓰기)를 백그라운드 스레드에서 처리합니다.
    /// </summary>
    private async UniTask SaveAsync()
    {
        // 메인 스레드에서 데이터 수집
        foreach (string currencyTitle in currencyDic.Keys)
        {
            currencyData.currencyStates[currencyTitle] = currencyDic[currencyTitle].CurrencyState;
        }

        // 백그라운드 스레드에서 저장 (암호화 + 파일 I/O)
        await DataManager.SaveToFileAsync<CurrencyData>(PATH, currencyData);

        Debug.Log("[CurrencyManager] Currency saved asynchronously");
    }

    /// <summary>
    /// 동기적으로 즉시 저장합니다. (앱 종료 시 사용)
    /// </summary>
    private void SaveImmediate()
    {
        foreach (string currencyTitle in currencyDic.Keys)
        {
            currencyData.currencyStates[currencyTitle] = currencyDic[currencyTitle].CurrencyState;
        }

        DataManager.SaveToFile<CurrencyData>(PATH, currencyData);
        Debug.Log("[CurrencyManager] Currency saved immediately (sync)");
    }

    /// <summary>
    /// Legacy 동기 Save (하위 호환성용)
    /// </summary>
    public void Save()
    {
        SaveImmediate();
    }

    /// <summary>
    /// 비동기로 데이터를 로드합니다.
    /// </summary>
    private async UniTask LoadAsync()
    {
        currencyData = await DataManager.LoadFromFileAsync<CurrencyData>(PATH);

        if (currencyData == null)
        {
            currencyData = new CurrencyData();
        }

        foreach (Currency currency in currencyDatabase.Items)
        {
            Currency madeCurrency = currency;

            if (!currencyData.currencyStates.ContainsKey(currency.Title) || !currency.IsPermanent)
            {
                currencyData.currencyStates[currency.Title] = new CurrencyState();
            }

            madeCurrency.SetDefaultValue(currencyData.currencyStates[currency.Title]);

            // IsPermanent 화폐는 변경 시 자동 저장 (비동기)
            if (currency.IsPermanent)
            {
                madeCurrency.onAmountChanged += (noUse) => RequestSave();
            }

            currencyDic[currency.Title] = madeCurrency;
        }

        IsLoaded = true;
        Debug.Log("[CurrencyManager] Currency Data Loaded");
    }

    /// <summary>
    /// Legacy 동기 Load (HaveLoad 인터페이스 구현)
    /// </summary>
    public void Load()
    {
        currencyData = DataManager.LoadFromFile<CurrencyData>(PATH);

        if (currencyData == null)
        {
            currencyData = new CurrencyData();
        }

        foreach (Currency currency in currencyDatabase.Items)
        {
            Currency madeCurrency = currency;

            if (!currencyData.currencyStates.ContainsKey(currency.Title) || !currency.IsPermanent)
            {
                currencyData.currencyStates[currency.Title] = new CurrencyState();
            }

            madeCurrency.SetDefaultValue(currencyData.currencyStates[currency.Title]);
            if (currency.IsPermanent) madeCurrency.onAmountChanged += (noUse) => RequestSave();

            currencyDic[currency.Title] = madeCurrency;
        }

        IsLoaded = true;
        Debug.Log("[CurrencyManager] Currency Data Loaded (sync)");
    }

    public long GetCurrencyAmount(string title) => currencyDic[title].Amount;

    public long GetCurrencyAmount(Currency currency) => currencyDic[currency.Title].Amount;

    public long GetCurrencyTotalAmount(string title) => currencyDic[title].TotalAmount;

    public long GetCurrencyTotalAmount(Currency currency) => currencyDic[currency.Title].TotalAmount;

    public bool UseCurrency(string title, long useAmount) => currencyDic[title].Use(useAmount);

    public bool UseCurrency(Currency currency, long useAmount) => UseCurrency(currency.Title, useAmount);

    public void EarnCurrency(string title, long amount)
    {
        if (amount < 0) return;

        currencyDic[title].Earn(amount);
    }
    public void EarnCurrency(Currency currency, long amount) => EarnCurrency(currency.Title, amount);

    public void AddActionCurrency(string title, Action<long> action, bool isTotal = false)
    {
        if (isTotal)
        {
            currencyDic[title].onTotalAmountChanged += action;
        }
        else
        {
            currencyDic[title].onAmountChanged += action;
        }
    }

    public void AddActionCurrency(Currency currency, Action<long> action, bool isTotal = false) => AddActionCurrency(currency.Title, action, isTotal);

    public void RemoveActionCurrency(string title, Action<long> action, bool isTotal = false)
    {
        if (isTotal)
        {
            currencyDic[title].onTotalAmountChanged -= action;
        }
        else
        {
            currencyDic[title].onAmountChanged -= action;
        }
    }

    public void RemoveActionCurrency(Currency currency, Action<long> action, bool isTotal = false) => RemoveActionCurrency(currency.Title, action, isTotal);

    public void ResetCurrency(Currency currency)
    {
        currency.ResetCurrency();
    }

    public void ResetCurrency(string currencyName) => currencyDic[currencyName].ResetCurrency();

    [Button, EditorButton]
    public void ResetCurrencies()
    {
        foreach (Currency currency in currencyDic.Values)
        {
            currency.ResetCurrency();
        }

        Save();
    }    

#if UNITY_EDITOR
    [Button]
    public void TestEarnCoin(Currency currency, int quantity)
    {
        EarnCurrency(currency, quantity);
    }
#endif
}

