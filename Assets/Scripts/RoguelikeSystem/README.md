# Roguelike System

> 확장 가능한 로그라이크 효과 및 제약 관리 시스템
>
> Strategy Pattern과 Registry Pattern을 활용한 유연하고 유지보수 가능한 아키텍처

## 📋 목차

- [개요](#개요)
- [디렉토리 구조](#디렉토리-구조)
- [핵심 기능](#핵심-기능)
- [디자인 패턴](#디자인-패턴)
- [아키텍처](#아키텍처)
- [주요 컴포넌트](#주요-컴포넌트)
- [사용 예시](#사용-예시)
- [확장 방법](#확장-방법)
- [설계 결정 및 Trade-off](#설계-결정-및-trade-off)
- [성능 최적화](#성능-최적화)

---

## 개요

RoguelikeSystem은 **로그라이크 효과**(데미지 증가, 스탯 변경)와 **리소스 제약**(재화 비용, 레벨 요구사항)을 관리하는 시스템입니다.

### 주요 특징

- ✅ **Strategy Pattern**으로 효과/제약 타입 추가 시 기존 코드 수정 불필요 (Open-Closed Principle)
- ✅ **데이터와 로직 분리**: 데이터는 struct/enum, 로직은 Strategy로 관리
- ✅ **Singleton 캐싱**으로 GC 부하 제거 및 성능 최적화
- ✅ **Registry Pattern**으로 중앙집중식 에러 처리 및 로깅
- ✅ **명확한 폴더 구조**로 유지보수성 극대화
- ✅ **CSV 기반 데이터 로딩**으로 디자이너 친화적 워크플로우

### 사용 사례

```csharp
// 데미지 증가 효과 (재화 비용 있음)
RogueEffect damageBoost = new RogueEffect
{
    id = "dmg_boost_rare",
    tier = RogueTier.Rare,
    constricts = new List<RogueConstrictData>
    {
        new RogueConstrictData(RogueConstrictType.Currency, "Gold", 1000)
    },
    effects = new RogueEffectPair[]
    {
        new RogueEffectPair(RogueEffectCategory.Damage, new EffectArgs("50"))
    }
};

// 효과 사용 시도
damageBoost.Action();
// → Gold 1000 이상? ✅ 소비 후 데미지 +50 적용
// → Gold 부족? ❌ 조기 종료, 효과 미적용
```

---

## 디렉토리 구조

```
RoguelikeSystem/
│
├── Constrict/                         # 제약 검증 및 리소스 소비
│   ├── Strategies/                     # 제약 전략 구현체들
│   │   ├── IConstrictStrategy.cs
│   │   └── ... (Currency, Level, Unit)
│   └── RogueConstrictRegistry.cs
│
├── Effect/                            # 효과 실행 시스템
│   ├── Strategies/                     # 효과 전략 구현체들
│   │   ├── IEffectStrategy.cs
│   │   └── ... (Damage, AttackSpeed, MoveSpeed)
│   ├── RogueEffectRegistry.cs
│   ├── RogueEffectCategory.cs
│   └── RogueEffectPair.cs
│
├── Core/                              # 핵심 데이터 구조
│   ├── RogueEffect.cs
│   ├── RogueConstrictData.cs
│   ├── RogueTier.cs
│   └── RoguelikeGachaPool.cs
│
└── Sample/                            # 샘플 및 테스트
```

### 핵심 파일

**[`RogueEffect.cs`](Core/RogueEffect.cs)**
- 효과의 메인 컨테이너
- `Action()`: 제약 검증 → 리소스 소비 → 효과 실행 파이프라인
- `Clone()`: 가챠 풀용 깊은 복사
- CSV 데이터를 런타임 인스턴스로 변환

**[`RogueEffectRegistry.cs`](Effect/RogueEffectRegistry.cs)**
- 효과 Strategy들의 중앙 관리자
- Singleton 패턴으로 전략 캐싱 (GC 없음)
- 중앙집중식 에러 처리 및 로깅

**[`RogueConstrictRegistry.cs`](Constrict/RogueConstrictRegistry.cs)**
- 제약 Strategy들의 중앙 관리자
- `GetStrategy()`: 타입별 전략 조회
- Singleton 캐싱으로 런타임 할당 제거

**[`IConstrictStrategy.cs`](Constrict/Strategies/IConstrictStrategy.cs)**
- 제약 전략 인터페이스
- C# 8.0 Default Interface Implementation 활용
- `AfterAction()`: 선택적 리소스 소비 (기본 구현: 아무것도 안 함)

**[`RogueConstrictData.cs`](Core/RogueConstrictData.cs)**
- 제약 데이터 (struct)
- 성능: 연속 메모리, GC 없음, 캐시 지역성 향상
- 3 필드 (type, name, needAmount)

---

## 핵심 기능

### ✅ 효과 시스템
- **플러그인 가능한 효과**: Damage, AttackSpeed, MoveSpeed (쉽게 확장 가능)
- **Strategy Pattern**: 각 효과 타입이 전용 전략 클래스 보유
- **중앙집중식 실행**: Registry 패턴으로 에러 처리 및 로깅
- **데이터 주도**: CSV로 효과 정의, 런타임 파싱

### ✅ 제약 시스템
- **리소스 검증**: 재화/레벨/유닛 요구사항 체크
- **2단계 실행**:
  1. `IsUsable()` - 모든 제약 검증
  2. `AfterAction()` - 리소스 소비 (선택적)
- **유연한 제약**: 일부는 체크만 (Level), 일부는 소비 (Currency)

### ✅ 가챠 통합
- **등급 기반 풀**: Common, Rare, Unique 분포
- **가중치 선택**: 등급별 확률 설정 가능
- **동적 풀 관리**: 런타임에 효과 추가/제거

---

## 디자인 패턴

### 1. Strategy Pattern

**문제**: 새 효과 타입 추가 시 switch 문 수정 필요

**해결**: 각 효과/제약 타입별 전용 전략 클래스

#### Before (경직된 구조)
```csharp
public static void ExecuteEffect(RogueEffectCategory category, EffectArgs args)
{
    switch (category)
    {
        case RogueEffectCategory.Damage:
            // 데미지 로직
            break;
        case RogueEffectCategory.AttackSpeed:
            // 공속 로직
            break;
        // 새 효과 추가 = switch 수정 필요
    }
}
```

#### After (확장 가능한 구조)
```csharp
// 1. 새 전략 클래스 작성 (기존 코드 수정 불필요)
public class CriticalRateEffectStrategy : IEffectStrategy
{
    public void Execute(EffectArgs args)
    {
        float critRate = args.Float(0);
        // 크리티컬 확률 적용 로직
    }
}

// 2. Registry에 등록
{ RogueEffectCategory.CriticalRate, new CriticalRateEffectStrategy() }

// 3. 완료! 새 효과 사용 가능
```

**장점**:
- ✅ Open-Closed Principle: 확장에는 열려있고, 수정에는 닫혀있음
- ✅ Single Responsibility: 각 전략이 하나의 효과만 담당
- ✅ 테스트 용이성: 개별 전략 단위 테스트 가능

---

### 2. Registry Pattern

**문제**: 효과 실행 로직 분산, 에러 처리 일관성 부족

**해결**: 중앙집중식 Registry로 전략 조회 및 실행 관리

```csharp
public static class RogueEffectRegistry
{
    private static readonly Dictionary<RogueEffectCategory, IEffectStrategy> strategyCache;

    public static void EffectAction(RogueEffectPair effectPair)
    {
        if (strategyCache.TryGetValue(effectPair.effectCategory, out var strategy))
        {
            try
            {
                strategy.Execute(effectPair.args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"효과 실행 오류: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"등록되지 않은 효과: {effectPair.effectCategory}");
        }
    }
}
```

**장점**:
- ✅ 중앙집중식 에러 처리 및 로깅
- ✅ 효과 실행의 단일 진입점
- ✅ 분석, 디버깅 등 횡단 관심사 추가 용이

---

### 3. Data-Driven Design

**문제**: 효과가 C# 코드에 하드코딩되어 수정 시 재컴파일 필요

**해결**: CSV로 효과 정의, 런타임 로딩

```csv
id,title,tier,effect,limit
dmg_boost_1,데미지 증가 I,Common,"Damage 10",99
aspd_boost_1,공속 증가 I,Rare,"AttackSpeed 1.2",5
```

**장점**:
- ✅ 디자이너가 코드 수정 없이 효과 변경 가능
- ✅ 밸런싱 및 반복 작업 용이
- ✅ 현지화 지원 (타이틀 문자열 분리)

---

## 아키텍처

### 실행 플로우

[`RogueEffect.Action()`](Core/RogueEffect.cs) 호출 시:

1. **제약 검증** - 각 제약의 `IsUsable()` 체크 → 하나라도 실패 시 조기 종료
2. **리소스 소비** - 각 제약의 `AfterAction()` 실행 (선택적)
3. **효과 실행** - 각 효과의 `Execute()` 실행
4. **콜백 트리거** - `onAction` 이벤트 발생

### 의존성 관계

```
RogueEffect
  ├─ RogueEffectPair[] → RogueEffectRegistry → IEffectStrategy
  └─ RogueConstrictData[] → RogueConstrictRegistry → IConstrictStrategy
```

---

## 주요 컴포넌트

### [`RogueEffect`](Core/RogueEffect.cs)

**역할**: 제약 검증 → 리소스 소비 → 효과 실행 파이프라인을 관리하는 메인 효과 컨테이너

**주요 메서드**:
- `Action()` - 제약 검증 → 리소스 소비 → 효과 실행 파이프라인
- `Clone()` - 가챠 풀 인스턴스용 깊은 복사
- `DescriptionText()` - 현지화된 효과 설명

**주요 필드**:
- [`RogueEffectPair[]`](Effect/RogueEffectPair.cs) `effects` - 효과 내용
- [`RogueConstrictData[]`](Core/RogueConstrictData.cs) `constricts` - 사용 요구사항

---

### [`IEffectStrategy`](Effect/Strategies/IEffectStrategy.cs)

**역할**: 모든 효과 구현체의 인터페이스

**구현 예시**:
- `DamageEffectStrategy` - 데미지 증가/감소
- `AttackSpeedEffectStrategy` - 공격속도 배율 변경
- `MoveSpeedEffectStrategy` - 이동속도 배율 변경

**새 효과 추가 방법**:
1. [`IEffectStrategy`](Effect/Strategies/IEffectStrategy.cs) 구현 클래스 생성
2. [`RogueEffectCategory`](Effect/RogueEffectCategory.cs) enum에 추가
3. [`RogueEffectRegistry`](Effect/RogueEffectRegistry.cs)에 등록

---

### [`IConstrictStrategy`](Constrict/Strategies/IConstrictStrategy.cs)

**역할**: 제약 검증 및 리소스 소비 인터페이스

**구현 예시**:
- `CurrencyConstrictStrategy` - 재화 검증 + 소비
- `LevelConstrictStrategy` - 플레이어 레벨 체크만 (소비 없음)
- `UnitConstrictStrategy` - 유닛 보유 체크만 (소비 없음)

**설계 특징**: C# 8.0+ Default 구현으로 선택적 리소스 소비 가능

---

### [`RogueEffectRegistry`](Effect/RogueEffectRegistry.cs)

**역할**: 중앙집중식 효과 전략 관리 및 실행

**장점**:
- ✅ Singleton 캐싱 (런타임 할당 없음)
- ✅ 중앙집중식 에러 처리
- ✅ O(1) 전략 조회

---

### [`RogueConstrictData`](Core/RogueConstrictData.cs)

**역할**: 경량 제약 데이터 (struct로 성능 최적화)

**왜 struct?**:
- ✅ Value type → `List<RogueConstrictData>`에서 연속 메모리
- ✅ 작은 데이터(3 필드)라 힙 할당 불필요
- ✅ 캐시 지역성 향상

---

## 사용 예시

### 예시 1: 단순 데미지 증가
```csharp
// 효과 정의 (보통 CSV에서 로딩)
RogueEffect damageBoost = new RogueEffect
{
    id = "dmg_boost_common",
    tier = RogueTier.Common,
    effects = new RogueEffectPair[]
    {
        new RogueEffectPair(RogueEffectCategory.Damage, new EffectArgs("15"))
    }
};

// 효과 실행
damageBoost.Action(); // 플레이어 데미지 +15
```

---

### 예시 2: 재화 비용이 있는 효과
```csharp
RogueEffect expensiveUpgrade = new RogueEffect
{
    id = "rare_upgrade",
    tier = RogueTier.Rare,
    constricts = new List<RogueConstrictData>
    {
        new RogueConstrictData(RogueConstrictType.Currency, "Gold", 1000)
    },
    effects = new RogueEffectPair[]
    {
        new RogueEffectPair(RogueEffectCategory.Damage, new EffectArgs("50")),
        new RogueEffectPair(RogueEffectCategory.AttackSpeed, new EffectArgs("1.5"))
    }
};

// 효과 사용 시도
expensiveUpgrade.Action();
// → 체크: Gold 1000 이상?
// → ✅ 있음: Gold 1000 소비, 데미지 +50, 공속 ×1.5 적용
// → ❌ 부족: 조기 종료, 효과 미적용
```

---

## 확장 방법

### 새 효과 타입 추가하기 (예시: 크리티컬 확률)

**1. Enum 추가** - [`RogueEffectCategory.cs`](Effect/RogueEffectCategory.cs)
```csharp
public enum RogueEffectCategory
{
    Damage = 0,
    AttackSpeed = 1,
    MoveSpeed = 2,
    CriticalRate = 3  // ← 새 효과
}
```

**2. 전략 클래스 생성** - `Effect/Strategies/CriticalRateEffectStrategy.cs`
```csharp
namespace RoguelikeSystem
{
    public class CriticalRateEffectStrategy : IEffectStrategy
    {
        public void Execute(EffectArgs args)
        {
            float critRate = args.Float(0);

            // TODO: 플레이어에게 크리티컬 확률 적용
            Debug.Log($"크리티컬 확률 +{critRate}%");
        }
    }
}
```

**3. Registry에 등록** - [`RogueEffectRegistry.cs`](Effect/RogueEffectRegistry.cs)
```csharp
private static readonly Dictionary<RogueEffectCategory, IEffectStrategy> strategyCache
    = new Dictionary<RogueEffectCategory, IEffectStrategy>
{
    { RogueEffectCategory.Damage, new DamageEffectStrategy() },
    { RogueEffectCategory.AttackSpeed, new AttackSpeedEffectStrategy() },
    { RogueEffectCategory.MoveSpeed, new MoveSpeedEffectStrategy() },
    { RogueEffectCategory.CriticalRate, new CriticalRateEffectStrategy() }  // ← 추가
};
```

**4. CSV에서 사용**
```csv
id,title,tier,effect,limit
crit_boost_1,크리티컬 I,Rare,"CriticalRate 10",99
```

**완료!** [`RogueEffect`](Core/RogueEffect.cs) 수정 없음, switch 문 없음. Open-Closed Principle 실현.

---

### 새 제약 타입 추가하기 (예시: 퀘스트 완료)

**1. Enum 추가** - [`RogueConstrictData.cs`](Core/RogueConstrictData.cs)
```csharp
public enum RogueConstrictType
{
    Currency,
    Level,
    Unit,
    QuestCompleted  // ← 새 제약
}
```

**2. 전략 클래스 생성** - `Constrict/Strategies/QuestConstrictStrategy.cs`
```csharp
public class QuestConstrictStrategy : IConstrictStrategy
{
    public bool IsUsable(string questId, int needAmount)
    {
        // 퀘스트 완료 여부 체크
        return QuestManager.Instance?.IsQuestCompleted(questId) ?? false;
    }

    // AfterAction은 기본 구현 사용 (리소스 소비 없음)
}
```

**3. Registry에 등록** - [`RogueConstrictRegistry.cs`](Constrict/RogueConstrictRegistry.cs)
```csharp
private static readonly Dictionary<RogueConstrictType, IConstrictStrategy> strategyCache
    = new Dictionary<RogueConstrictType, IConstrictStrategy>
{
    { RogueConstrictType.Currency, new CurrencyConstrictStrategy() },
    { RogueConstrictType.Level, new LevelConstrictStrategy() },
    { RogueConstrictType.Unit, new UnitConstrictStrategy() },
    { RogueConstrictType.QuestCompleted, new QuestConstrictStrategy() }  // ← 추가
};
```

**4. 코드에서 사용**
```csharp
RogueEffect questReward = new RogueEffect
{
    constricts = new List<RogueConstrictData>
    {
        new RogueConstrictData(RogueConstrictType.QuestCompleted, "main_quest_1", 1)
    },
    effects = new RogueEffectPair[] { /* ... */ }
};
```

---

## 설계 결정 및 Trade-off

### 1. Registry 통합 (Factory → Registry 병합)

**초기 설계**: `EffectStrategyFactory` + `RogueEffectRegistry` 분리

**문제**: Factory가 Registry 내부에서만 사용됨, 불필요한 추상화

**해결**: Factory를 Registry에 통합

**Trade-off**:
- ✅ **간결함**: 이해할 클래스 1개 감소
- ✅ **YAGNI**: Factory가 외부에서 불필요
- ⚠️ **책임 분리 약화**: Registry가 조회와 실행 모두 담당
- **결정**: 간결성 우선 (YAGNI 원칙)

---

### 2. Struct vs Class for RogueConstrictData

**선택**: Struct

| 측면 | Class | Struct (선택) |
|------|-------|---------------|
| **메모리** | 힙 할당, GC 부하 | 스택/인라인, GC 없음 |
| **캐시 지역성** | 포인터 분산 | List에서 연속 메모리 |
| **크기** | 오버헤드 16+ bytes | 3 필드 = 12-16 bytes |
| **불변성** | 참조로 수정 가능 | 복사로 안전 |

**Trade-off**:
- ✅ **성능**: GC 없음, 캐시 성능 향상
- ✅ **불변성**: 값 의미론으로 의도치 않은 변경 방지
- ⚠️ **null 불가**: null 체크 불가 (기본값 사용)
- **결정**: 성능과 불변성이 nullable보다 중요

---

### 3. Default Interface Implementation for AfterAction

**기능**: C# 8.0 Default Interface Members

```csharp
public interface IConstrictStrategy
{
    bool IsUsable(string name, int needAmount);

    void AfterAction(string name, int needAmount)
    {
        // 기본 구현: 아무 작업 안 함 (체크만 하는 제약용)
    }
}
```

**Trade-off**:
- ✅ **유연성**: 체크만 하는 제약과 소비하는 제약 모두 지원
- ✅ **보일러플레이트 감소**: 체크만 하는 전략에서 빈 `AfterAction()` 불필요
- ⚠️ **C# 버전 의존성**: C# 8.0+ 필요 (Unity 2021+)
- **결정**: 최신 C# 기능 활용 가치 있음 (프로젝트는 2021 LTS+ 대상)

---

### 4. TODO 스텁 vs 전체 구현

**현재 상태**: Strategy `Execute()` 메서드에 TODO 스텁

**왜 구현하지 않았나?**:
- **시스템 통합**: UnitSystem, CalcSystem, DeckManager 연결 필요
- **범위**: 포트폴리오는 아키텍처 시연, 완전한 게임 구현 아님
- **확장성 중심**: 효과 추가 방법 시연이 특정 효과 구현보다 중요

**Trade-off**:
- ✅ **명확한 아키텍처**: 패턴 구현은 완성
- ✅ **확장성 증명**: 새 효과 추가가 쉬움
- ⚠️ **플레이 불가**: 실제 효과 실행 테스트 불가
- **결정**: 포트폴리오는 아키텍처 시연 > 전체 구현

---

## 성능 최적화

### 1. Singleton Strategy 캐싱

**문제**: 매 효과 실행 시 새 전략 인스턴스 생성

**해결**: Registry에서 전략 인스턴스 1회 생성 후 재사용

**장점**:
- ✅ 런타임 할당 없음
- ✅ GC 부하 없음
- ✅ O(1) 조회
- **영향**: 10,000회 효과 실행 = 0 할당 (캐싱 없으면 10,000 할당)

---

### 2. 작은 데이터는 Struct

**최적화**: `RogueConstrictData`를 struct로 (3 필드, ~12 bytes)

**장점**:
- ✅ `List<RogueConstrictData>`에서 연속 메모리
- ✅ 캐시 지역성 향상
- ✅ 제약당 힙 할당 없음

**벤치마크** (가상):
```
100개 RogueEffect × 제약 3개 = 300개 제약
Class: 힙 객체 300개 = ~8.4 KB + GC 오버헤드
Struct: List 인라인 = ~3.6 KB, GC 없음
```

---

### 3. 검증 조기 종료

**최적화**: 제약 실패 시 즉시 종료

```csharp
public void Action()
{
    // 모든 제약 체크
    foreach (var data in constricts)
    {
        if (!strategy.IsUsable(data.name, data.needAmount))
            return;  // ← 조기 종료, 이후 체크 생략
    }

    // 제약 실행 (모두 통과한 경우만)
    foreach (var data in constricts) { /* ... */ }
}
```

**장점**:
- ✅ 불필요한 체크 생략
- ✅ 실패 경로 빠름
- **예**: 제약 5개, 첫 번째 실패 → 4개 체크 절약

---

## 변경 이력

### v2.0 (현재) - 아키텍처 리팩토링
- ✅ 효과 및 제약에 Strategy Pattern 적용
- ✅ Factory를 Registry에 통합 (YAGNI)
- ✅ `ConstrictData`를 struct로 변경 (성능)
- ✅ Constrict/, Effect/, Core/ 폴더로 구조화
- ✅ AfterAction에 Default Interface Implementation 추가
- ✅ 파일명 일관성 확보 (RogueConstrictData, RogueEffectRegistry)

### v1.0 (초기)
- switch 문으로 정적 메서드 구현
- 단일 파일에 모든 로직
- Class 기반 제약

---

## 라이선스 및 크레딧

**Unity 포트폴리오 프로젝트의 일부**
로그라이크 시스템을 위한 프로덕션 레디 디자인 패턴 및 아키텍처 시연

**Unity 버전**: 2021 LTS+
**C# 버전**: 9.0+

---

**질문이나 피드백?** 메인 포트폴리오 문서 참조 또는 이슈 제출
