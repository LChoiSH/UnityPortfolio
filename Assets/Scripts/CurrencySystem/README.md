# Currency System

> 확장 가능하고 안전한 인게임 화폐 관리 시스템
>
> ScriptableObject Database와 Save/Load를 활용한 중앙 집중식 경제 시스템

## 📋 목차

- [개요](#개요)
- [디렉토리 구조](#디렉토리-구조)
- [적용된 디자인 패턴](#적용된-디자인-패턴)
- [핵심 기능](#핵심-기능)
- [주요 API](#주요-api)
- [사용 예시](#사용-예시)
- [기술적 의사결정](#기술적-의사결정)

---

## 개요

CurrencySystem은 게임 내 **화폐 관리**를 담당하는 핵심 모듈입니다. 골드, 다이아, 티켓 등 다양한 화폐를 중앙에서 관리하며, ScriptableObject 기반 데이터베이스와 Save/Load 시스템을 제공합니다.

### 주요 특징

- ✅ **ScriptableObject Database**로 모든 화폐를 중앙 관리
- ✅ **영구/비영구 화폐** 구분 및 자동 저장
- ✅ **이벤트 기반 아키텍처**로 느슨한 결합
- ✅ **중복 검증 시스템**으로 데이터 무결성 보장
- ✅ **Dictionary 캐싱**으로 O(1) 조회 성능

---

## 디렉토리 구조

```
CurrencySystem/
├── CurrencyManager.cs          # Singleton 매니저 (Save/Load)
├── Currency.cs                 # 화폐 데이터 클래스
├── CurrencyDatabaseSO.cs       # ScriptableObject 데이터베이스
├── CurrencyData.cs             # 저장용 데이터 구조체
├── CurrencyState.cs            # 화폐 상태 (amount, totalAmount)
├── Editor/
│   ├── CurrencyDatabaseWindow.cs    # 커스텀 에디터 윈도우
│   └── CurrencyDatabaseCreator.cs   # 데이터베이스 생성 유틸리티
└── Sample/                     # 샘플 씬 및 예제
```

**핵심 파일:**
- [`CurrencyManager.cs`](CurrencyManager.cs): Singleton 패턴, Save/Load 담당
- [`Currency.cs`](Currency.cs): 화폐 데이터 및 로직 (Earn, Use)
- [`CurrencyDatabaseSO.cs`](CurrencyDatabaseSO.cs): ScriptableObject 기반 DB

---

## 적용된 디자인 패턴

### 1. Singleton Pattern

전역 접근 가능한 단일 매니저 인스턴스 (DontDestroyOnLoad)

---

### 2. Event-Driven Architecture

화폐 변경 시 `onAmountChanged` 이벤트 발생으로 UI 등 다른 시스템에 자동 통지

---

### 3. Repository Pattern (Dictionary Cache)

Title 기반 Dictionary 캐싱으로 O(1) 조회 성능

---

## 핵심 기능

### 1. 영구 / 비영구 화폐

**영구 화폐 (Permanent):**
- 게임 종료 후에도 유지 (예: 골드, 다이아)
- 변경 시 자동 저장
- `IsPermanent = true`

```csharp
if (currency.IsPermanent)
{
    madeCurrency.onAmountChanged += (noUse) => Save();
}
```

**비영구 화폐 (Non-Permanent):**
- 게임 세션 종료 시 리셋 (예: 스테이지 코인)
- 저장하지 않음
- `IsPermanent = false`

---

### 2. 데이터 무결성 보장

중복 Title 검증 (`Editor_Validate()`) 및 잔액 부족 시 실패 반환

---

### 3. 커스텀 에디터 윈도우

`Window → Game → Currency Database`에서 화폐 추가/편집/삭제

---

## 주요 API

### CurrencyManager

**화폐 조회:**
- `GetCurrencyAmount(string title)` - 현재 보유량
- `GetCurrencyTotalAmount(string title)` - 누적 획득량
- `FindCurrencyByTitle(string title)` - Currency 객체 조회

**화폐 사용:**
- `EarnCurrency(string title, long amount)` - 화폐 획득
- `UseCurrency(string title, long amount)` - 화폐 사용 (잔액 부족 시 false)

**이벤트 구독:**
- `AddActionCurrency(string title, Action<long> action)` - 이벤트 구독
- `RemoveActionCurrency(string title, Action<long> action)` - 이벤트 해제

**저장/로드:**
- `Save()` - 수동 저장 (영구 화폐는 자동 저장)
- `Load()` - 게임 시작 시 자동 호출

---

### Currency

**데이터 접근:**
- `Title` - 화폐 이름 (고유 ID)
- `Description` - 화폐 설명
- `Icon` - 화폐 아이콘
- `IsPermanent` - 영구 화폐 여부
- `Amount` - 현재 보유량
- `TotalAmount` - 누적 획득량

**로직:**
- `Earn(long amount)` - 화폐 획득
- `Use(long amount)` - 화폐 사용 (성공 여부 반환)

**이벤트:**
- `onAmountChanged` - 보유량 변경 시
- `onTotalAmountChanged` - 누적량 변경 시

---

## 사용 예시

### 기본 사용법

```csharp
// 1. 화폐 획득
CurrencyManager.Instance.EarnCurrency("Gold", 100);

// 2. 화폐 사용
bool success = CurrencyManager.Instance.UseCurrency("Gold", 50);
if (success)
{
    Debug.Log("구매 성공!");
}
else
{
    Debug.Log("골드 부족!");
}

// 3. 현재 보유량 조회
long goldAmount = CurrencyManager.Instance.GetCurrencyAmount("Gold");
Debug.Log($"보유 골드: {goldAmount}");
```

---

### 이벤트 구독

```csharp
public class ShopSystem : MonoBehaviour
{
    private void Start()
    {
        // 골드 변경 시 UI 업데이트
        CurrencyManager.Instance.AddActionCurrency("Gold", OnGoldChanged);
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        CurrencyManager.Instance.RemoveActionCurrency("Gold", OnGoldChanged);
    }

    private void OnGoldChanged(long newAmount)
    {
        Debug.Log($"현재 골드: {newAmount}");
    }
}
```

---

### 새로운 화폐 추가

1. `Window → Game → Currency Database` 열기
2. Add New Currency → Title, Icon, IsPermanent 설정
3. 코드에서 바로 사용 가능: `CurrencyManager.Instance.EarnCurrency("NewCurrency", 10);`

---

## 기술적 의사결정

### 1. ScriptableObject vs MonoBehaviour

**문제:** 화폐 데이터를 어디에 저장할 것인가?

**결정:** ScriptableObject Database 사용

**이유:**
- 씬 독립적 (모든 씬에서 동일 데이터 참조)
- 에셋으로 관리 (버전 관리 용이)
- Inspector 편집 가능 (디자이너 친화적)

---

### 2. Dictionary vs List

**문제:** 화폐 검색을 어떻게 최적화할 것인가?

**결정:** Dictionary 캐싱 사용

**이유:**
- `FindCurrencyByTitle("Gold")`가 빈번하게 호출됨
- List: O(n) 순회
- Dictionary: O(1) 조회

**구현:**
```csharp
// Load 시 Dictionary 생성
foreach (Currency currency in currencyDatabase.Items)
{
    currencyDic[currency.Title] = madeCurrency;
}
```

---

### 3. 자동 저장 vs 수동 저장

**문제:** 언제 Save()를 호출할 것인가?

**결정:** 영구 화폐는 자동 저장, 비영구는 저장 안 함

**이유:**
- 영구 화폐: 사용자 진행도 보존 필요 → 변경 즉시 저장
- 비영구 화폐: 스테이지 재시작 시 리셋 → 저장 불필요
- 이벤트 기반 자동 저장으로 저장 누락 방지

**구현:**
```csharp
if (currency.IsPermanent)
{
    madeCurrency.onAmountChanged += (noUse) => Save();
}
```

