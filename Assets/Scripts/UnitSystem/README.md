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

```
UnitSystem/
├── Core/                   # Unit, UnitStat, UnitState (Enum)
├── States/                 # State Pattern 관련 (9개 파일)
├── Components/             # Attacker, Defender, Mover
├── Modifiers/              # Hit/Heal Modifier 시스템
├── Management/             # UnitFactory, UnitManager
└── Sample/                 # 예제 및 테스트
```

<details>
<summary><b>상세 구조 보기</b></summary>

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

### 1. State Pattern ⭐

**목적:** 유닛의 상태별 행동을 캡슐화하여 상태 전환을 명확하게 관리

**구현:**
- `IUnitState` 인터페이스로 상태 계약 정의
- `UnitStateBase` 추상 클래스로 공통 로직 제공 (Template Method Pattern)
- 각 상태(Idle, Attack, Move, Death)를 독립적인 클래스로 구현
- `UnitStateMachine`이 상태 전환 및 검증 담당

**장점:**
- 새로운 상태 추가 시 기존 코드 수정 불필요 (Open-Closed Principle)
- 각 상태의 로직이 명확히 분리 (Single Responsibility Principle)
- 잘못된 상태 전환 방지 (`CanTransitionTo`)

**코드 예시:**
```csharp
// 상태 전환
unit.SetState(UnitState.Attack);

// StateMachine이 자동으로:
// 1. 전환 가능 여부 검증
// 2. 현재 상태 Exit
// 3. 새 상태 Enter (애니메이션 자동 재생)
```

**전투 시스템 흐름:**
```
Attacker.Attack(defender)
    ↓
Hit 생성 → Attacker Modifiers 적용 (정렬)
    ↓
Defender.Damaged(hit) → Defender Modifiers 적용 (정렬)
    ↓
HP 감소 → 이벤트 발생 → Post Callbacks 실행
```

**State Machine 흐름:**
```
Initialize(unitType) → Factory로 State 생성 → 등록 → Idle로 시작
    ↓
매 프레임: Update() → CurrentState.Update(unit)
```

---

### 2. Template Method Pattern

**목적:** 상태의 실행 흐름은 베이스 클래스에서 정의하고, 세부 구현은 서브클래스에서 처리

**구현:**
```csharp
// UnitStateBase.cs
public virtual void Enter(Unit unit)
{
    // 1. 애니메이션 재생 (공통)
    unit.Animator.SetTrigger(StateType.ToString());

    // 2. 서브클래스의 커스텀 로직 호출
    OnEnter(unit);
}

protected virtual void OnEnter(Unit unit) { }  // 서브클래스가 오버라이드
```

**장점:**
- 애니메이션 재생 등 공통 로직이 자동 실행
- 서브클래스는 `OnEnter`, `OnUpdate`, `OnExit`만 구현하면 됨
- 코드 중복 제거

---

### 3. Factory Pattern ⭐

**목적:** 유닛 타입에 따라 다른 State 조합을 제공

**구현:**
```csharp
// UnitStateFactory.cs
public static IEnumerable<IUnitState> CreateStates(UnitType unitType)
{
    switch (unitType)
    {
        case UnitType.Boss:
            return CreateBossStates();  // Boss 전용 상태 세트
        case UnitType.Ranged:
            return CreateRangedStates();  // 원거리 유닛 상태 세트
        default:
            return CreateDefaultStates();  // 기본 상태 세트
    }
}
```

**장점:**
- 유닛 타입별로 다른 상태 조합 가능
- 새 유닛 타입 추가 시 Factory만 수정 (Open-Closed Principle)
- 객체 생성 로직 중앙화

---

### 4. Modifier Pattern (Chain of Responsibility)

**목적:** 데미지/힐에 다양한 수정자(버프/디버프)를 체인 형태로 적용

**구현:**
```csharp
// Attacker에서 공격자의 Modifier 적용
var sortedModifiers = attackModifiers
    .OrderBy(m => m.Phase)         // Phase별 정렬
    .ThenByDescending(m => m.Priority);  // Priority 정렬

foreach (var modifier in sortedModifiers)
{
    hit = modifier.Apply(hit);  // 체인으로 적용
}

// Defender에서 방어자의 Modifier 적용
foreach (var modifier in defenseModifiers)
{
    hit = modifier.Apply(hit);
}
```

**예제 Modifier:**
```csharp
public class RageBuff : IHitModifier
{
    public DamagePhase Phase => DamagePhase.PreHit;
    public int Priority => 100;

    public Hit Apply(Hit hit)
    {
        hit.finalDamage *= 1.5f;  // 50% 데미지 증가
        return hit;
    }
}
```

**장점:**
- 버프/디버프를 동적으로 추가/제거 가능
- Phase와 Priority로 적용 순서 제어
- 새로운 효과 추가가 용이 (Open-Closed Principle)

---

### 5. Component Pattern

**목적:** 기능별로 컴포넌트를 분리하여 재사용성과 유지보수성 향상

**구현:**
- `Attacker` - 공격 전담
- `Defender` - 방어 및 HP 관리
- `Mover` - 이동 전담

**장점:**
- 각 컴포넌트가 단일 책임 (Single Responsibility Principle)
- 필요한 기능만 조합 가능 (예: 이동만 가능한 유닛)
- 테스트가 용이

---

### 6. Object Pooling (UnitFactory)

**목적:** 빈번한 생성/파괴로 인한 GC 부하 감소

**구현:**
```csharp
// UnitFactory.cs
ObjectPool<Unit> pool = new ObjectPool<Unit>(
    createFunc: () => Create(unitPrefab),
    actionOnGet: PoolOnGet,
    actionOnRelease: PoolOnRelease,
    defaultCapacity: 4,
    maxSize: maxSize
);
```

**장점:**
- 메모리 할당/해제 최소화
- 성능 향상 (특히 많은 유닛 생성 시)

---

### 7. Singleton Pattern (UnitManager)

**목적:** 전역적으로 유닛을 관리하는 단일 인스턴스

**기능:**
- 팀별 유닛 관리
- 유닛 등록/해제
- 팀별 유닛 조회

---

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
// 1. 유닛 생성 (Factory에서)
Unit unit = unitFactory.GetUnit("Warrior");
unit.SetTeam(1);

// 2. 상태 변경
unit.SetState(UnitState.Attack);

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

### 커스텀 Modifier 생성

```csharp
public class CriticalHitBuff : IHitModifier
{
    public string Name => "Critical Hit";
    public string Tag => "Buff:Critical";
    public int Priority => 100;
    public DamagePhase Phase => DamagePhase.PreHit;

    private float critChance = 0.3f;
    private float critMultiplier = 2.0f;

    public Hit Apply(Hit hit)
    {
        if (Random.value < critChance)
        {
            hit.finalDamage *= critMultiplier;

            // Post Callback으로 크리티컬 이펙트 재생
            hit.postCallbacks.Add(() =>
            {
                Debug.Log("CRITICAL HIT!");
                // 파티클 재생 등
            });
        }

        return hit;
    }
}
```

---

## 확장 가능성

### 새로운 State 추가

```csharp
// 1. State 클래스 생성
public class StunState : UnitStateBase
{
    public override UnitState StateType => UnitState.Stun;

    protected override HashSet<UnitState> AllowedTransitions => new HashSet<UnitState>
    {
        UnitState.Idle,
        UnitState.Death
    };

    protected override void OnEnter(Unit unit)
    {
        // 기절 처리: 이동, 공격 비활성화
    }
}

// 2. Enum에 추가
public enum UnitState
{
    Idle, Attack, Move, Death,
    Stun  // 추가
}

// 3. Factory에 등록
private static IEnumerable<IUnitState> CreateDefaultStates()
{
    return new IUnitState[]
    {
        new IdleState(),
        new AttackState(),
        new MoveState(),
        new StunState(),  // 추가
        new DeathState()
    };
}
```

### 새로운 유닛 타입 추가

```csharp
// 1. Enum에 추가
public enum UnitType
{
    Default, Melee, Ranged, Tank, Support, Boss,
    Healer  // 추가
}

// 2. Factory에 케이스 추가
case UnitType.Healer:
    return CreateHealerStates();
```

### 새로운 Modifier 추가

```csharp
public class PoisonDebuff : IHitModifier
{
    public string Name => "Poison";
    public DamagePhase Phase => DamagePhase.PostHit;

    public Hit Apply(Hit hit)
    {
        hit.postCallbacks.Add(() =>
        {
            // 지속 데미지 적용
            ApplyDamageOverTime(hit.defender, duration: 5f, tickDamage: 10f);
        });
        return hit;
    }
}
```

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

### 3. Modifier 정렬 시스템

**문제:** Modifier를 어떤 순서로 적용할 것인가?

**결정:** Phase → Priority 순으로 정렬

**이유:**
- Phase로 큰 흐름 제어 (PreHit → Mitigation → PostHit)
- Priority로 같은 Phase 내 우선순위 제어
- 명확하고 예측 가능한 동작

**구현:**
```csharp
var sortedModifiers = modifiers
    .OrderBy(m => m.Phase)
    .ThenByDescending(m => m.Priority);
```

---

### 4. Component vs Inheritance

**문제:** 기능을 상속으로 구현 vs Component로 분리

**결정:** Component 방식 선택

**이유:**
- C#은 단일 상속만 지원
- 기능별로 독립적으로 테스트 가능
- 유닛마다 필요한 기능만 조합 가능 (공격만, 이동만 등)

---

### 5. Factory Pattern으로 확장성 확보

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
- LINQ 사용하지만 매 공격마다 정렬하지 않도록 최적화 가능
- Modifier 추가/제거 시에만 재정렬하는 방식으로 개선 가능

### Event System
- C# event 사용으로 느슨한 결합 유지
- 불필요한 참조 방지

---

## 라이선스

이 프로젝트는 포트폴리오 목적으로 제작되었습니다.
