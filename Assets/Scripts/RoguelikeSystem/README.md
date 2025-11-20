# Roguelike System

> **Strategy Pattern** 기반의 확장 가능한 효과/제약 시스템
>
> **Registry Pattern**으로 중앙집중식 실행 관리 및 에러 처리

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

- ✅ **Strategy Pattern**으로 효과/제약 타입 추가 시 기존 코드 수정 불필요
- ✅ **데이터와 로직 분리**: 데이터는 struct/enum, 로직은 Strategy로 관리
- ✅ **Singleton 캐싱**으로 GC 부하 제거 및 성능 최적화
- ✅ **Registry Pattern**으로 중앙집중식 에러 처리 및 로깅
- ✅ **명확한 폴더 구조**로 유지보수성 극대화
- ✅ **CSV 기반 데이터 로딩**으로 디자이너 친화적 워크플로우

### 사용 사례

```csharp
// GachaPool에서 Rare 등급 효과 획득
RogueEffect reward = rogueGachaPool.GetRandom(RogueTier.Rare);

// 제약 검증 → 리소스 소비 → 효과 적용
if (reward.Action())
{
    Debug.Log($"✅ {reward.title} 획득!");
    // 효과가 플레이어에게 자동 적용됨
}
else
{
    Debug.Log("❌ 조건 불충족 (Gold 부족 등)");
}
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

---

## 디자인 패턴

### 1. Strategy Pattern

각 효과/제약 타입을 독립적인 전략 클래스로 구현하여 확장성 확보

```csharp
// 전략 인터페이스
public interface IEffectStrategy
{
    void Execute(EffectArgs args);
}

// 구체적 전략 구현
public class DamageEffectStrategy : IEffectStrategy
{
    public void Execute(EffectArgs args)
    {
        float damage = args.Float(0);
        // 데미지 적용 로직
    }
}
```

**장점**:
- ✅ 새 효과 추가 시 기존 코드 수정 불필요
- ✅ 각 전략이 하나의 효과만 담당
- ✅ 개별 전략 단위 테스트 가능

---

### 2. Registry Pattern

전략 객체를 중앙에서 관리하고 실행을 중개

```csharp
public static class RogueEffectRegistry
{
    private static readonly Dictionary<RogueEffectCategory, IEffectStrategy> strategyCache;

    public static void EffectAction(RogueEffectPair effectPair)
    {
        if (strategyCache.TryGetValue(effectPair.effectCategory, out var strategy))
            strategy.Execute(effectPair.args);
    }
}
```

**장점**:
- ✅ 중앙집중식 에러 처리 및 로깅
- ✅ Singleton 패턴으로 전략 캐싱 (GC 없음)
- ✅ O(1) 전략 조회

---

### 3. Data-Driven Design

CSV 기반 데이터 정의로 코드 수정 없이 효과 변경 가능

```csv
id,title,tier,effect,limit
dmg_boost_1,데미지 증가 I,Common,"Damage 10",99
aspd_boost_1,공속 증가 I,Rare,"AttackSpeed 1.2",5
```

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

---

### [`IConstrictStrategy`](Constrict/Strategies/IConstrictStrategy.cs)

**역할**: 제약 검증 및 리소스 소비 인터페이스

**구현 예시**:
- `CurrencyConstrictStrategy` - 재화 검증 + 소비
- `LevelConstrictStrategy` - 플레이어 레벨 체크만 (소비 없음)
- `UnitConstrictStrategy` - 유닛 보유 체크만 (소비 없음)

---

### [`RogueConstrictData`](Core/RogueConstrictData.cs)

**역할**: 경량 제약 데이터 (struct로 성능 최적화)

---

## 사용 예시

### 데이터 정의

효과 데이터는 CSV에서 로드되며, `RogueEffect` 인스턴스로 변환됩니다.

**CSV 예시:**
```csv
id,title,tier,effect,constrict,limit
dmg_boost_1,데미지 증가 I,Common,"Damage 15",,99
rare_upgrade,강력한 강화,Rare,"Damage 50|AttackSpeed 1.5","Gold 1000",5
```

---

### 런타임 사용
```csharp
// 적 처치 시 랜덤 효과 획득
enemy.Defender.onDeath += () =>
{
    // 가챠에서 Common 등급 효과 획득
    RogueEffect reward = rogueGachaPool.GetRandom(RogueTier.Common);

    // 제약 확인 및 효과 적용
    if (reward.Action())
    {
        Debug.Log($"✅ {reward.title} 획득! 효과 적용됨");
        // 효과가 자동으로 플레이어에게 적용
        // 예: Damage +15, AttackSpeed x1.2 등
    }
    else
    {
        Debug.Log("❌ 효과 사용 실패 (제약 조건 불충족)");
    }
};

// 레벨업 시 선택지 제공
void OnLevelUp()
{
    // 3개의 Rare 효과 중 선택
    List<RogueEffect> choices = rogueGachaPool.GetRandomMultiple(
        tier: RogueTier.Rare,
        count: 3,
        allowDuplicates: false
    );

    // UI에 표시 후 플레이어 선택
    ShowEffectSelectionUI(choices, selectedEffect =>
    {
        selectedEffect.Action(); // 선택한 효과 적용
    });
}
```

---

## 확장 방법

### 새 효과 타입 추가하기 (예시: 크리티컬 확률)

**1. Enum 추가** - [`RogueEffectCategory.cs`](Effect/RogueEffectCategory.cs)
```csharp
public enum RogueEffectCategory { Damage, AttackSpeed, MoveSpeed, CriticalRate }
```

**2. 전략 클래스 생성** - `Effect/Strategies/CriticalRateEffectStrategy.cs`
```csharp
public class CriticalRateEffectStrategy : IEffectStrategy
{
    public void Execute(EffectArgs args)
    {
        float critRate = args.Float(0);
        // 플레이어에게 크리티컬 확률 적용
    }
}
```

**3. Registry에 등록** - [`RogueEffectRegistry.cs`](Effect/RogueEffectRegistry.cs)
```csharp
{ RogueEffectCategory.CriticalRate, new CriticalRateEffectStrategy() }
```

**완료!** 기존 코드 수정 없이 확장 가능

---

### 새 제약 타입 추가하기 (예시: 퀘스트 완료)

**1. Enum 추가** - [`RogueConstrictData.cs`](Core/RogueConstrictData.cs)
```csharp
public enum RogueConstrictType { Currency, Level, Unit, QuestCompleted }
```

**2. 전략 클래스 생성** - `Constrict/Strategies/QuestConstrictStrategy.cs`
```csharp
public class QuestConstrictStrategy : IConstrictStrategy
{
    public bool IsUsable(string questId, int needAmount)
    {
        return QuestManager.Instance?.IsQuestCompleted(questId) ?? false;
    }
    // AfterAction은 기본 구현 사용 (리소스 소비 없음)
}
```

**3. Registry에 등록** - [`RogueConstrictRegistry.cs`](Constrict/RogueConstrictRegistry.cs)
```csharp
{ RogueConstrictType.QuestCompleted, new QuestConstrictStrategy() }
```

---

## 설계 결정 및 Trade-off

### 1. Registry 통합 (Factory → Registry 병합)

**초기 설계**: `EffectStrategyFactory` + `RogueEffectRegistry` 분리

**문제**: Factory가 Registry 내부에서만 사용됨, 불필요한 추상화

**해결**: Factory를 Registry에 통합

**Trade-off**:
- ✅ **간결함**: 이해할 클래스 1개 감소
- ✅ Factory가 외부에서 사용되지 않음
- ⚠️ **책임 분리 약화**: Registry가 조회와 실행 모두 담당
- **결정**: 간결성 우선

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

### 2. 메모리 최적화 (Struct 사용)

**최적화**: `RogueConstrictData`를 struct로 구현하여 연속 메모리 배치 및 GC 부하 제거

