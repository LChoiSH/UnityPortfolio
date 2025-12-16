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

### 4. Debouncing Pattern ⭐

짧은 시간 내 여러 저장 요청 발생 시 마지막 요청만 실행하여 I/O 빈도를 감소시킵니다.

**동작 원리:**
- 저장 요청 발생 → 타이머 시작 (0.5초)
- 타이머 완료 전 재요청 → 기존 타이머 취소, 새 타이머 시작
- 타이머 완료 → 실제 저장 실행

**효과:**
- 0.1초마다 10번 호출 → 실제 저장은 1번만 실행
- I/O 부하 감소, 배터리 소모 최적화 (모바일)

```csharp
// Debouncing 구현 (CurrencyManager.cs:74-105)
public async void RequestSave()
{
    isDirty = true;

    // 기존 타이머 취소 및 새 타이머 시작
    debounceCts?.Cancel();
    debounceCts = new CancellationTokenSource();

    // 0.5초 대기 (재호출 시 취소됨)
    await UniTask.Delay(TimeSpan.FromSeconds(debounceTime),
        cancellationToken: debounceCts.Token);

    // ... 실제 저장 로직
}
```

---

### 5. Pending Save Pattern ⭐

I/O 작업 진행 중 새로운 저장 요청이 오면 완료 후 재실행하여 데이터 손실을 방지합니다.

**동작 원리:**
- 저장 중(`currentSaveTask != null`) → `isDirty` 플래그만 세움
- I/O 완료 → `isDirty` 체크 후 재저장

**효과:**
- I/O 중복 실행 방지
- 데이터 손실 방지 (마지막 변경사항 보장)

```csharp
// Pending Save 구현 (CurrencyManager.cs:87-98)
if (currentSaveTask != null && !currentSaveTask.Value.Status.IsCompleted())
{
    await currentSaveTask.Value; // I/O 완료 대기
}

if (isDirty)
{
    isDirty = false;
    currentSaveTask = SaveAsync(); // 재저장
    await currentSaveTask.Value;
}
```

---

### 6. Dirty Flag Pattern ⭐

실제 데이터 변경이 있을 때만 저장을 수행하여 불필요한 I/O를 방지합니다.

**동작 원리:**
- 데이터 변경 발생 → `isDirty = true`
- 저장 실행 → `isDirty = false`
- 저장 시점에 `isDirty` 체크 → true일 때만 실행

**효과:**
- 중복 저장 방지
- I/O 횟수 최소화

---

### 7. Async/Await Pattern (UniTask) ⭐

CPU Bound(직렬화, 암호화)와 I/O Bound(파일 쓰기) 작업을 백그라운드 스레드에서 처리하여 메인 스레드 블록을 방지합니다.

**CPU Bound vs I/O Bound:**
- **CPU Bound**: JSON 직렬화, AES 암호화 (CPU 연산)
- **I/O Bound**: 파일 읽기/쓰기 (디스크 대기)

**UniTask 선택 이유:**
- ✅ **제로 GC 할당** - ValueTask 기반 (모바일 최적화)
- ✅ **Unity PlayerLoop 통합** - Unity 생명주기와 자연스럽게 동작
- ✅ **Cancellation 편리** - `CancellationToken` 관리 용이
- ✅ **성능** - C# Task보다 Unity에서 최적화됨

```csharp
// UniTask.RunOnThreadPool로 백그라운드 처리 (DataManager.cs:55-70)
await UniTask.RunOnThreadPool(() =>
{
    string jsonData = JsonConvert.SerializeObject(data); // CPU Bound
    string encryptedData = FileIO.EncryptString(jsonData); // CPU Bound
    File.WriteAllText(fullPath, encryptedData); // I/O Bound
});
```

**효과:**
- ✅ 메인 스레드 블록 방지 → **프레임 드롭 0**
- ✅ 암호화 연산을 백그라운드에서 처리
- ✅ 파일 I/O 대기 시간 동안 게임 로직 계속 실행

**Trade-off:**
- C# Task보다 UniTask 패키지 의존성 추가
- 하지만 Unity 프로젝트에서는 성능 이득이 명확함

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

### 3. 비동기 저장 시스템 ⭐

**패턴 조합으로 프로덕션 수준 최적화:**

```
화폐 변경 → RequestSave() 호출
                ↓
    [Debouncing] 0.5초 대기 (빠른 연속 호출 필터링)
                ↓
    [Pending Save] I/O 진행 중이면 완료 대기
                ↓
    [Dirty Flag] 변경사항 있으면 저장
                ↓
    [Async/Await] 백그라운드 스레드에서 실행
        - JSON 직렬화 (CPU Bound)
        - AES 암호화 (CPU Bound)
        - 파일 쓰기 (I/O Bound)
                ↓
    메인 스레드는 계속 게임 로직 실행 (프레임 드롭 0)
```

**시나리오 예시:**

**시나리오 1: 빠른 연속 호출 (Debouncing 효과)**
```
0.0초: EarnCurrency 호출 → RequestSave()
0.1초: EarnCurrency 호출 → RequestSave() (기존 타이머 취소)
0.2초: EarnCurrency 호출 → RequestSave() (기존 타이머 취소)
0.7초: Debounce 완료 → SaveAsync() 1번만 실행 ✅
```

**시나리오 2: I/O 진행 중 호출 (Pending Save 효과)**
```
0.0초: RequestSave() → SaveAsync() 시작 (2초 소요)
0.5초: RequestSave() 호출 → isDirty = true, I/O 대기
2.0초: SaveAsync() 완료 → isDirty 체크 → 재저장 ✅
```

**성능 개선 효과:**
- I/O 빈도: 매번 저장 → 0.5초 단위 저장 (**최대 90% 감소**)
- 메인 스레드 블록: 있음 → 없음 (**프레임 드롭 0**)
- 배터리 소모: 높음 → 낮음 (모바일 최적화)

---

### 4. 커스텀 에디터 윈도우

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
- `RequestSave()` - 비동기 저장 요청 (Debouncing + Pending Save) ⭐ 권장
- `Save()` - 즉시 동기 저장 (Legacy, 에디터 전용)
- `Load()` - 동기 로드 (Legacy, HaveLoad 인터페이스 구현)

**비동기 내부 메서드:**
- `SaveAsync()` - UniTask 기반 비동기 저장
- `LoadAsync()` - UniTask 기반 비동기 로드 (Start()에서 호출)

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
    madeCurrency.onAmountChanged += (noUse) => RequestSave(); // 비동기 저장
}
```

---

### 4. 동기 저장 vs 비동기 저장 ⭐

**문제:** 저장 시 메인 스레드 블록으로 인한 프레임 드롭 발생

**원인 분석:**
1. **CPU Bound 작업** - JSON 직렬화, AES 암호화 (연산 시간)
2. **I/O Bound 작업** - 파일 쓰기 (디스크 대기 시간)
3. **빈번한 호출** - 화폐 변경마다 저장 → I/O 폭증

**결정:** Debouncing + Pending Save + Async/Await 조합

**이유:**

| 문제 | 해결 방법 | 효과 |
|------|----------|------|
| I/O 빈도 과다 | Debouncing Pattern | 0.5초 내 여러 호출 → 1번만 저장 |
| I/O 중복 실행 | Pending Save Pattern | 진행 중이면 대기 후 재실행 |
| 불필요한 저장 | Dirty Flag Pattern | 변경사항 있을 때만 저장 |
| 메인 스레드 블록 | Async/Await (UniTask) | 백그라운드 스레드에서 처리 |

**Trade-off 고려:**

✅ **선택한 방식: 비동기 저장**
- 장점: 프레임 드롭 0, I/O 최적화, 모바일 배터리 절약
- 단점: 복잡도 증가, UniTask 의존성

❌ **배제한 방식: 동기 저장만 사용**
- 장점: 구현 간단
- 단점: 메인 스레드 블록, 프레임 드롭, 빈번한 I/O

**왜 UniTask를 선택했나?**

| 항목 | C# Task | UniTask |
|------|---------|---------|
| GC 할당 | 있음 (class 기반) | **제로** (ValueTask 기반) |
| Unity 통합 | 수동 처리 필요 | **PlayerLoop 자동 통합** |
| 성능 | 범용 | **Unity 최적화** |
| 의존성 | 없음 (표준) | 패키지 필요 |

**결론:** 모바일 타겟 + 빈번한 저장 → UniTask의 제로 GC가 중요

**실제 측정 결과 (예상):**
- 동기 저장: 10번 호출 → 10번 I/O (100ms × 10 = 1000ms 블록)
- 비동기 저장: 10번 호출 → 1번 I/O (0ms 블록, 백그라운드 처리)

**참고:** [`CurrencyManager.cs:74-105`](CurrencyManager.cs), [`DataManager.cs:55-99`](../Utils/DataManager.cs)

