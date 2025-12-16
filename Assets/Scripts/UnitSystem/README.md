# Unit System

> 확장 가능하고 유지보수가 용이한 유닛 전투 시스템
>
> 다양한 디자인 패턴을 활용한 객체지향 설계

## 📋 목차

- [개요](#개요)
- [디렉토리 구조](#디렉토리-구조)
- [적용된 디자인 패턴](#적용된-디자인-패턴)
- [핵심 기능](#핵심-기능)
- [사용 예시](#사용-예시)
- [확장 가능성](#확장-가능성)
- [기술적 의사결정](#기술적-의사결정)

---

## 개요

UnitSystem은 게임 내 유닛의 **상태 관리**, **전투 시스템**, **이동 시스템**을 담당하는 핵심 모듈입니다.

### 주요 특징

- ✅ **State Pattern** 기반의 명확한 상태 관리
- ✅ **Modifier Pattern**을 활용한 확장 가능한 데미지/힐 시스템
- ✅ **Factory Pattern**으로 유닛 타입별 다른 상태 조합 제공
- ✅ **Component 기반 설계**로 기능별 분리 및 재사용성 극대화
- ✅ **Object Pooling**을 통한 성능 최적화

---

## 디렉토리 구조

<details>
<summary><b>펼쳐보기</b></summary>

```
UnitSystem/
├── Core/
│   ├── Unit.cs                     # 유닛 메인 클래스
│   ├── UnitStat.cs                 # 유닛 스탯 데이터
│   └── UnitState.cs                # 상태 Enum (식별자)
│
├── States/
│   ├── IUnitState.cs               # State 인터페이스
│   ├── UnitStateBase.cs            # State 베이스 (Template Method Pattern)
│   ├── UnitStateMachine.cs         # State 관리 및 전환
│   ├── UnitStateFactory.cs         # Factory Pattern
│   ├── IdleState.cs
│   ├── AttackState.cs
│   ├── MoveState.cs
│   └── DeathState.cs
│
├── Components/
│   ├── Attacker.cs                 # 공격 처리
│   ├── Defender.cs                 # 방어 및 HP 관리
│   └── Mover.cs                    # 이동 처리
│
├── Modifiers/
│   ├── Hit.cs                      # 공격 데이터 구조체
│   ├── Heal.cs                     # 힐 데이터 구조체
│   ├── IHitModifier.cs             # Hit Modifier 인터페이스 + 예제
│   └── IHealModifier.cs            # Heal Modifier 인터페이스 + 예제
│
├── Management/
│   ├── UnitFactory.cs              # Object Pooling 기반 유닛 생성
│   └── UnitManager.cs              # 전역 유닛 관리 (Singleton)
│
└── Sample/
```

</details>

---

## 적용된 디자인 패턴

이 시스템에 적용된 주요 패턴들입니다. 각 패턴의 구체적인 설계 결정과 Trade-off는 [기술적 의사결정](#기술적-의사결정) 섹션에서 확인할 수 있습니다.

| 패턴 | 적용 위치 | 목적 |
|------|----------|------|
| **[State Pattern](#1-enum과-state-클래스를-함께-사용하는-이유)** | States/ | 상태별 행동 캡슐화, 명확한 상태 전환 관리 |
| **[Template Method](#2-template-method-pattern-사용-이유)** | UnitStateBase | 공통 로직 자동화, 서브클래스는 커스텀 로직만 구현 |
| **[Factory Pattern](#7-factory-pattern으로-확장성-확보)** | UnitStateFactory, UnitFactory | 유닛 타입별 다른 상태 조합, Object Pooling |
| **[Modifier (Dirty Flag)](#4-modifier-정렬-최적화-dirty-flag-pattern)** | IHitModifier, IHealModifier | 버프/디버프 동적 추가/제거, 변경 시에만 재정렬 |
| **[Component Pattern](#5-component-vs-inheritance)** | Attacker, Defender, Mover | 기능별 분리, 단일 책임 원칙 |
| **[Event-Driven Architecture](#6-event-driven-architecture-채택)** | 모든 컴포넌트 | 느슨한 결합, 다중 구독 가능 |
| **Singleton** | UnitManager | 전역 유닛 관리, 팀별 유닛 조회 |
| **[Object Pooling](#object-pooling)** | UnitFactory | GC 부하 감소, 성능 최적화 |

**⚠️ 현재 구현 상태:**
- `Boss`와 `Ranged` 유닛은 기본 상태와 동일한 세트를 사용합니다 (Factory Pattern의 확장성 시연용 구조)
- 실제 게임에서는 Boss 전용 상태(`BossSkillState`, `EnrageState` 등) 추가 가능


## 핵심 기능

### 1. 상태 관리 (State Machine)

- **자동 애니메이션 재생**: 상태 전환 시 해당 상태의 애니메이션 자동 실행
- **전환 검증**: `CanTransitionTo`로 잘못된 상태 전환 방지
- **이벤트 시스템**: 상태 변경 시 `onStateChanged` 이벤트 발생

### 2. 전투 시스템

#### Attacker (공격자)
- Hit 생성 및 초기화
- 공격 Modifier 적용 (Phase/Priority 정렬)
- Modifier 동적 추가/제거

#### Defender (방어자)
- HP 관리 (Current HP, Max HP, HP Ratio)
- 방어 Modifier 적용
- 사망 이벤트 발생
- Heal 시스템

#### Hit/Heal Modifier
- **Phase 기반 처리**: PreHit → Mitigation → PostHit
- **Priority 기반 정렬**: 같은 Phase 내에서 우선순위 적용
- **Post Callback**: 데미지/힐 적용 후 추가 로직 실행

### 3. 이동 시스템 (Mover)

- **부드러운 이동**: `Vector3.MoveTowards` 사용
- **이동 이벤트**: `onMoveStart`, `onMoveComplete`
- **이동 중단**: `Stop()` 메서드
- **즉시 이동**: `SnapTo()` 메서드

### 4. 유닛 생성 및 관리

#### UnitFactory
- Object Pooling 기반 유닛 생성
- Prewarm 지원 (미리 생성)
- 자동 재활용

#### UnitManager
- 팀별 유닛 그룹화
- 유닛 검색 (팀별, ID별)
- 자동 등록/해제

---

## 사용 예시

### 기본 사용법

```csharp
// 1. 유닛 생성 (UnitManager를 통해 Factory 접근)
Unit unit = UnitManager.Instance.UnitFactory.GetUnit("Warrior");
unit.SetTeam(1);

// 위치를 지정하여 생성
Unit spawnedUnit = UnitManager.Instance.UnitFactory.GetUnit("Warrior", new Vector3(0, 0, 5));

// 유닛 존재 확인
if (UnitManager.Instance.UnitFactory.HasUnit("Warrior"))
{
    Unit warrior = UnitManager.Instance.UnitFactory.GetUnit("Warrior");
}

// 사용 가능한 유닛 ID 목록 조회
var availableUnits = UnitManager.Instance.UnitFactory.GetAvailableUnitIds();
Debug.Log($"Available units: {string.Join(", ", availableUnits)}");

// 2. 상태 변경
unit.SetState(UnitState.Attack);

// 강제 상태 전환 (검증 무시 - 특수 상황용)
unit.ForceSetState(UnitState.Death);  // 즉사 처리 등

// 3. 공격
Defender target = enemyUnit.Defender;
unit.Attacker.Attack(target);

// 4. Modifier 추가
unit.Attacker.AddAttackModifier(new RageBuff(1.5f));  // 50% 데미지 증가
unit.Defender.AddDefenseModifier(new ArmorBuff(0.7f));  // 30% 데미지 감소

// 5. 이동
unit.Mover.MoveTo(targetPosition);

// 6. 이벤트 구독
unit.StateMachine.onStateChanged += (prev, next) =>
{
    Debug.Log($"State changed: {prev} → {next}");
};

unit.Defender.onCurrentHpChanged += (newHp) =>
{
    UpdateHealthBar(newHp);
};
```

### UnitManager 팀 관리

```csharp
// 팀별 유닛 조회
var myTeamUnits = UnitManager.Instance.GetTeamUnits(1);
var enemyUnits = UnitManager.Instance.GetEnemyUnits(1);

// 특정 ID의 유닛 찾기
var allWarriors = UnitManager.Instance.GetUnitsById(1, "Warrior");

// 팀 유닛 수 확인
int teamSize = UnitManager.Instance.TeamCount(1);

// 전체 유닛 조회
var allUnits = UnitManager.Instance.MadeUnits;

// UnitManager 이벤트 구독
UnitManager.Instance.onUnitRegister += (unit) =>
{
    Debug.Log($"Unit registered: {unit.Id}");
};

UnitManager.Instance.onUnitUnregister += (unit) =>
{
    Debug.Log($"Unit unregistered: {unit.Id}");
};

UnitManager.Instance.onTeamCountChanged += (team) =>
{
    UpdateTeamUI(team);
};
```

### 유닛 타입별 설정

```csharp
// Inspector에서 설정
[SerializeField] private UnitType unitType = UnitType.Boss;

// 런타임에서 확인
if (unit.StateMachine.IsInState(UnitState.Death))
{
    // 사망 상태 처리
}
```

---

## 확장 가능성

시스템은 Open-Closed Principle을 준수하여 기존 코드 수정 없이 확장할 수 있습니다:

- **새로운 State 추가:** `UnitStateBase`를 상속하여 State 클래스 생성 → Enum에 추가 → Factory에 등록
- **새로운 유닛 타입 추가:** `UnitType` Enum 추가 → `UnitStateFactory`에 케이스 추가
- **새로운 Modifier 추가:** `IHitModifier` 또는 `IHealModifier` 구현 → `AddAttackModifier()` 또는 `AddDefenseModifier()`로 동적 추가

---

## 기술적 의사결정

### 1. Enum과 State 클래스를 함께 사용하는 이유

**문제:** UnitState enum과 State 클래스가 중복되는 것처럼 보임

**결정:** 둘 다 유지

**이유:**
- **Enum**: 빠른 비교, Unity Inspector 노출, 직렬화, 네트워크 동기화에 최적화
- **State 클래스**: 상태별 복잡한 로직 캡슐화

**근거:**
- 성능: `enum` 비교는 O(1), Type 비교보다 빠름
- 가독성: `if (state == UnitState.Idle)`이 `if (state is IdleState)`보다 직관적
- 실용성: Unity에서 enum을 많이 사용하는 표준 방식

---

### 2. Template Method Pattern 사용 이유

**문제:** State에서 Enter/Update/Exit를 직접 오버라이드 vs Template Method

**결정:** Template Method 사용

**이유:**
- 애니메이션 재생 등 공통 로직을 자동화
- 서브클래스가 실수로 `base.Enter()` 호출을 잊는 것 방지
- 일관성 유지

---

### 3. Hit/Heal은 Struct, Modifier는 Class인 이유

**문제:** Hit/Heal과 Modifier를 Class vs Struct 중 어떤 것으로 구현할까?

**결정:**
- **Hit/Heal:** Struct (값 타입)
- **Modifier:** Class (참조 타입, Interface 구현)

**이유:**

**Hit/Heal을 Struct로 구현:**
- **짧은 수명:** 공격/힐 1회 동안만 존재하고 즉시 소멸
- **Value Semantics:** Modifier 체인에서 값 복사로 안전성 확보
- **스택 할당:** GC 부하 제거 (공격이 빈번하게 발생)
- **불변성:** Modifier가 원본을 수정하지 않고 새 값을 반환하도록 강제
- **작은 크기:** 약 24-32바이트로 복사 비용이 낮음

**Modifier를 Class로 구현:**
- **긴 수명:** 버프/디버프가 지속되는 동안 계속 존재
- **동적 관리:** List에 추가/제거가 빈번함
- **참조 동일성:** 같은 버프를 찾아서 제거 가능 (`RemoveAttackModifier(modifier)`)
- **상태 보유:** multiplier, duration, stack 등 변경 가능한 필드
- **Interface 사용:** Struct를 Interface로 참조하면 Boxing 발생 → 성능 악화

**만약 Modifier를 Struct로 만들면:**
```csharp
// ❌ 문제 1: Boxing 발생
List<IHitModifier> modifiers = new();
modifiers.Add(new RageBuff(1.5f));  // Struct → Interface = Boxing!

// ❌ 문제 2: Remove 불가능
RageBuff buff = new RageBuff(1.5f);
attacker.AddAttackModifier(buff);    // 복사본이 추가됨
attacker.RemoveAttackModifier(buff); // 다른 인스턴스라 못 찾음!

// ❌ 문제 3: 상태 변경 불가
public struct TimedBuff : IHitModifier
{
    private float duration;
    public Hit Apply(Hit hit)
    {
        duration -= Time.deltaTime;  // 복사본만 수정됨!
        return hit;
    }
}
```

**Trade-off:**
- Struct는 16바이트 이하일 때 최적이지만, Hit은 약 24-32바이트
- 단, 공격 빈도가 높고 GC 회피 이점이 크기 때문에 Struct 선택이 합리적

---

### 4. Modifier 정렬 최적화 (Dirty Flag Pattern)

**문제:** 매 공격/힐마다 Modifier를 정렬하면 불필요한 연산 발생

**결정:** Dirty Flag 패턴으로 변경 시에만 재정렬

**이유:**
- **공격/힐 빈도:** 초당 수십~수백 회 발생 (매우 빈번)
- **Modifier 변경 빈도:** 버프 획득/만료 시에만 발생 (드묾)
- **성능 개선:** O(n log n) 정렬을 캐싱 후 O(n) 순회로 전환
- **실전 적합성:** CalcSystem에서도 동일한 패턴 사용 (일관성)

**구현:**
```csharp
// Dirty Flag와 캐시
private List<IHitModifier> sortedAttackModifiers = new();
private bool isModifiersDirty = true;

public void Attack(Defender defender)
{
    // 변경된 경우에만 재정렬
    if (isModifiersDirty)
    {
        sortedAttackModifiers = attackModifiers
            .OrderBy(m => m.Phase)
            .ThenByDescending(m => m.Priority)
            .ToList();
        isModifiersDirty = false;
    }

    foreach (var modifier in sortedAttackModifiers)
    {
        hit = modifier.Apply(hit);
    }
}

public void AddAttackModifier(IHitModifier modifier)
{
    attackModifiers.Add(modifier);
    isModifiersDirty = true;  // 재정렬 필요 플래그
}
```

**Trade-off:**
- **메모리:** 정렬된 리스트 추가 (Modifier당 8바이트 참조, 무시 가능)
- **복잡도:** Dirty flag 관리 로직 추가
- **성능:** 실전에서 체감 가능한 개선 (정렬 99% 이상 제거)

**정렬 순서:**
- **Phase:** PreHit → Mitigation → PostHit (큰 흐름 제어)
- **Priority:** 같은 Phase 내에서 높은 Priority 먼저 적용

---

### 5. Component vs Inheritance

**문제:** 기능을 상속으로 구현 vs Component로 분리

**결정:** Component 방식 선택

**이유:**
- C#은 단일 상속만 지원
- 기능별로 독립적으로 테스트 가능
- 유닛마다 필요한 기능만 조합 가능 (공격만, 이동만 등)

---

### 6. Event-Driven Architecture 채택

**문제:** 시스템 간 통신을 직접 참조 vs 이벤트 기반으로 할 것인가?

**결정:** C# event 기반 느슨한 결합

**구현된 이벤트:**
- **Unit:** `onDeath`, `onDestroy`
- **Defender:** `onCurrentHpChanged`, `onMaxHpChanged`, `onDeath`
- **Mover:** `onMoveStart`, `onMoveComplete`
- **UnitStateMachine:** `onStateChanged`
- **UnitManager:** `onUnitRegister`, `onUnitUnregister`, `onTeamCountChanged`

**장점:**
- **느슨한 결합:** UI는 Unit을 직접 참조하지 않고도 HP 변화 감지 가능
- **확장성:** 새로운 리스너 추가 시 기존 코드 수정 불필요
- **테스트 용이:** Mock 리스너로 검증 가능
- **다중 구독:** 여러 시스템이 독립적으로 같은 이벤트 구독 가능

**예시:**
```csharp
// 여러 시스템이 독립적으로 구독
unit.Defender.onCurrentHpChanged += (hp) => healthBar.SetValue(hp);
unit.Defender.onCurrentHpChanged += (hp) => audioManager.PlayHurtSound();
unit.Defender.onCurrentHpChanged += (hp) => damageNumberSpawner.Spawn(hp);

unit.Defender.onDeath += () => questSystem.OnEnemyKilled(unit.Id);
unit.Defender.onDeath += () => particleSystem.PlayDeathEffect(unit.transform.position);
```

**Trade-off:**
- **장점:** 의존성 역전 (UI가 Unit을 몰라도 됨)
- **단점:** 이벤트 구독 해제를 잊으면 메모리 누수 (Unity에서는 GameObject 파괴 시 자동 해제되므로 일반적으로 문제 없음)

---

### 7. Factory Pattern으로 확장성 확보

**문제:** StateMachine에서 State를 하드코딩 vs Factory 사용

**결정:** Factory Pattern 사용

**이유:**
- 유닛 타입별로 다른 State 조합 제공 가능
- Boss는 특수 State, 일반 유닛은 기본 State
- Open-Closed Principle 준수: 새 유닛 타입 추가 시 Factory만 수정

---

## 성능 고려사항

### Object Pooling
- 빈번한 유닛 생성/파괴 시 GC 부하 최소화
- Prewarm으로 초기 로딩 시간 분산

### Modifier 정렬
- **현재 구현:** 매 공격마다 LINQ로 정렬 (코드 간결성과 명확성 우선)
- **성능 영향:** Modifier 개수가 적을 때는 무시 가능 (일반적으로 3-5개)
- **최적화 방향:** Modifier 추가/제거 시에만 재정렬하여 정렬된 리스트 캐싱
- **Trade-off:** 현재 방식은 버그 방지와 코드 가독성을 우선하며, 프로파일링 결과 병목 발생 시 최적화 적용 예정

### Event System
- C# event 사용으로 느슨한 결합 유지
- 불필요한 참조 방지

---

## 라이선스

이 프로젝트는 포트폴리오 목적으로 제작되었습니다.
