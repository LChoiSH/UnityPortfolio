# Calc System

> 확장 가능하고 성능 최적화된 동적 계산 시스템
>
> Dirty Flag Pattern을 활용한 지연 평가 기반 계산 엔진

## 📋 목차

- [개요](#개요)
- [디렉토리 구조](#디렉토리-구조)
- [적용된 디자인 패턴](#적용된-디자인-패턴)
- [핵심 기능](#핵심-기능)
- [계산 파이프라인](#계산-파이프라인)
- [사용 예시](#사용-예시)
- [확장 가능성](#확장-가능성)
- [기술적 의사결정](#기술적-의사결정)
- [성능 고려사항](#성능-고려사항)

---

## 개요

CalcSystem은 게임 내 **동적 수치 계산**을 담당하는 핵심 모듈입니다. HP, 공격력, 방어력 등 게임 내 모든 수치가 다양한 버프/디버프/장비 효과를 받아 최종값을 계산하는 시스템입니다.

### 주요 특징

- ✅ **Dirty Flag Pattern**을 활용한 지연 평가로 불필요한 계산 최소화
- ✅ **Value Object Pattern** 기반의 불변 공식(CalcFormula) 구조
- ✅ **5단계 계산 파이프라인**으로 명확한 계산 순서 보장
- ✅ **부동소수점 정밀도 처리**로 오차 누적 방지
- ✅ **쿼리 및 디버그 시스템**으로 개발/디버깅 편의성 극대화
- ✅ **중복 ID 경고 시스템**으로 실수 방지

### 사용 사례

```csharp
// HP, 공격력, 방어력 등 모든 수치 계산에 활용
CalcValue hp = new CalcValue(100);           // 기본 HP 100
hp.AddFormula("armor", 50, AddInitial);       // 방어구로 +50
hp.AddFormula("buff", 20, AddPercent);        // 버프로 +20%
hp.AddFormula("item", 30, Add);               // 아이템으로 +30
Debug.Log(hp.Value);  // 최종 HP = (100 + 50 + 30) * 1.2 = 216
```

---

## 디렉토리 구조

```
CalcSystem/
├── CalcValue.cs          # 메인 계산 엔진 (Dirty Flag Pattern)
├── CalcFormula.cs        # 불변 공식 구조체 (Value Object)
├── CalcOperator.cs       # 계산 연산자 정의 (Enum)
├── LongFormatter.cs      # 큰 숫자 포맷팅 유틸리티 (1000 → 1a)
└── README.md             # 문서 (현재 파일)
```

**핵심 파일:**
- `CalcValue.cs`: 공식 관리, 캐시 관리, 지연 평가 담당
- `CalcFormula.cs`: `readonly struct`로 구현된 불변 공식
- `CalcOperator.cs`: 5단계 계산 순서를 정의하는 Enum

---

## 적용된 디자인 패턴

### 1. Dirty Flag Pattern ⭐

**목적:** 값이 변경되었을 때만 재계산하여 불필요한 연산 최소화

**구현:**
```csharp
private bool _dirty = false;
private double _cachedValue = 0;

public double Value
{
    get
    {
        if (_dirty) Calculate();  // dirty일 때만 계산
        return _cachedValue;
    }
}

public void AddFormula(CalcFormula formula)
{
    formulaDic[formula.Id] = formula;
    _dirty = true;  // 값이 변경되었음을 표시
}
```

**장점:**
- 여러 공식을 연속으로 추가해도 Value 접근 시 **단 한 번만 계산**
- 매 프레임 Value를 참조해도 값이 변경되지 않았다면 **캐시된 값 반환** (O(1))
- 게임 로직과 계산 로직의 분리

**성능 개선 예시:**
```csharp
// 5개 공식 추가 후 1번 조회
player.atk.AddFormula("weapon", 50, AddInitial);     // O(1)
player.atk.AddFormula("buff1", 10, AddPercent);      // O(1)
player.atk.AddFormula("buff2", 5, Add);              // O(1)
player.atk.AddFormula("skill", 20, Multiple);        // O(1)
player.atk.AddFormula("debuff", -10, AddInitial);    // O(1)
var finalAtk = player.atk.Value;                     // O(n) 계산 1회만!
```

**Without Dirty Flag:**
- 공식 추가마다 계산 → 5번 계산 (비효율!)

**With Dirty Flag:**
- Value 접근 시점에 1번만 계산 → **5배 효율 향상**

---

### 2. Value Object Pattern

**목적:** CalcFormula를 불변 객체로 만들어 데이터 무결성 보장

**구현:**
```csharp
public readonly struct CalcFormula
{
    public string Id { get; }
    public double Value { get; }
    public CalcOperator Op { get; }

    public CalcFormula(string id, double value, CalcOperator op)
    {
        Id = id;
        Value = value;
        Op = op;
    }
}
```

**왜 `readonly struct`인가?**
- **Value Semantics**: 복사 시 새로운 인스턴스 생성 (참조 공유 X)
- **불변성**: 생성 후 수정 불가능 → 실수로 값 변경 방지
- **성능**: Stack 할당으로 GC 부하 감소
- **Thread-Safe**: 불변이므로 멀티스레드 환경에서 안전

**장점:**
- 데이터 무결성 보장
- 의도하지 않은 수정 불가능
- 디버깅 용이

---

### 3. Lazy Evaluation (지연 평가)

**목적:** 값이 실제로 필요할 때까지 계산을 미룸

**구현:**
```csharp
// 공식 추가 시 = 계산하지 않음
public void AddFormula(CalcFormula formula)
{
    formulaDic[formula.Id] = formula;
    _dirty = true;  // 표시만 함
}

// Value 접근 시 = 그때 계산
public double Value
{
    get
    {
        if (_dirty) Calculate();  // 지금 계산!
        return _cachedValue;
    }
}
```

**실제 시나리오:**
```csharp
// 로딩 중 모든 장비/버프 설정
for (int i = 0; i < 100; i++)
{
    player.hp.AddFormula($"item_{i}", values[i], ops[i]);  // 계산 안 함!
}

// 전투 시작 - 최종 HP가 필요할 때
var currentHp = player.hp.Value;  // 이때 1번만 계산!
```

**장점:**
- 초기화 시점에 계산 부하 없음
- 사용하지 않는 값은 계산하지 않음
- 배치 추가(bulk insert) 시 효율적

---

### 4. Template Pattern (계산 파이프라인)

**목적:** 계산 순서를 명확히 정의하여 예측 가능한 결과 보장

**구현:**
```csharp
// Enum으로 계산 순서 정의
public enum CalcOperator
{
    AddInitial = 0,           // 1단계: 기본값 설정
    AddInitialByPercent = 10, // 2단계: 기본값 % 증가
    Add = 40,                 // 4단계: 고정값 추가
    AddPercent = 20,          // 3단계: % 증가
    Multiple = 30             // 5단계: 곱셈
}

// 계산 템플릿
private void Calculate()
{
    // 1단계: AddInitial 합산
    // 2단계: AddInitialByPercent 적용
    // 3단계: Add 합산
    // 4단계: AddPercent 적용
    // 5단계: Multiple 적용

    double calcedInitial = initialValue * (1 + initialPercent * 0.01) + addValue;
    _cachedValue = calcedInitial * (1 + addPercent * 0.01) * multipleValue;
}
```

**장점:**
- 공식 추가 순서와 무관하게 **항상 동일한 결과**
- 계산 순서가 명확하여 디버깅 용이
- 복잡한 계산도 예측 가능

---

## 핵심 기능

### 1. 공식 관리

#### 공식 추가
```csharp
// 방법 1: 직접 추가
calcValue.AddFormula(new CalcFormula("weapon", 50, CalcOperator.AddInitial));

// 방법 2: 편의 메서드
calcValue.AddFormula("weapon", 50, CalcOperator.AddInitial);
```

**중복 ID 처리:**
- 동일 ID로 재추가 시 **Warning 로그** 출력 후 덮어쓰기
- 의도하지 않은 중복을 빠르게 발견 가능

```csharp
calcValue.AddFormula("buff", 10, AddPercent);
calcValue.AddFormula("buff", 20, AddPercent);  // Warning!
// [CalcValue] Overwriting formula 'buff'. Old: 10 (AddPercent) → New: 20 (AddPercent)
```

#### 공식 제거
```csharp
calcValue.RemoveFormula("weapon");
```

#### 공식 조회
```csharp
// 존재 여부 확인
if (calcValue.HasFormula("weapon"))
{
    // ...
}

// 안전하게 조회
if (calcValue.TryGetFormula("weapon", out var formula))
{
    Debug.Log($"Weapon: {formula.Value}");
}

// 모든 공식 순회
foreach (var formula in calcValue.Formulas.Values)
{
    Debug.Log($"{formula.Id}: {formula.Value} ({formula.Op})");
}
```

---

### 2. 중립 원소 최적화

**목적:** 계산에 영향을 주지 않는 값은 저장하지 않음

**구현:**
```csharp
// 덧셈의 중립 원소: 0
calcValue.AddFormula("useless", 0, AddInitial);  // 무시됨

// 곱셈의 중립 원소: 1
calcValue.AddFormula("useless", 1, Multiple);    // 무시됨
```

**장점:**
- 메모리 절약
- 불필요한 순회 감소
- 부동소수점 오차 누적 방지

---

### 3. 부동소수점 정밀도 처리

**문제:** `0.1 + 0.2 != 0.3` (부동소수점 오차)

**해결:**
```csharp
private const double Epsilon = 0.0001d;  // 0.01% 정밀도

private bool IsNeutralElement(CalcOperator op, double value)
{
    if (op == CalcOperator.Multiple)
    {
        return Math.Abs(value - 1.0d) < Epsilon;  // 1.0과 거의 같으면
    }
    else
    {
        return Math.Abs(value) < Epsilon;  // 0.0과 거의 같으면
    }
}
```

**장점:**
- 부동소수점 오차로 인한 버그 방지
- 안정적인 비교 연산

---

### 4. 디버그 시스템

#### GetDebugInfo()
```csharp
string info = calcValue.GetDebugInfo();
Debug.Log(info);
```

**출력 예시:**
```
=== CalcValue Debug Info ===
Final Value: 216

Formula Count: 4

Formulas (by operator):
  [AddInitial] base: 100
  [AddInitial] armor: 50
  [AddPercent] buff: 20
  [Add] item: 30

Cached Intermediate Values:
  initialValue: 150
  initialPercent: 0%
  addValue: 30
  addPercent: 20%
  multipleValue: 1x
```

#### LogDebugInfo() (Editor Only)
```csharp
#if UNITY_EDITOR
calcValue.LogDebugInfo();  // Unity Console에 출력
#endif
```

**장점:**
- 계산 과정 전체를 한눈에 파악
- 버그 발견 용이
- Release 빌드에서 자동 제거 (조건부 컴파일)

---

## 계산 파이프라인

CalcSystem은 **5단계 계산 파이프라인**을 사용합니다.

### 계산 순서

```
1단계: AddInitial
    ↓ (합산)
initialValue = 모든 AddInitial 합

2단계: AddInitialByPercent
    ↓ (% 증가)
initialValue *= (1 + initialPercent%)

3단계: Add
    ↓ (합산)
initialValue += addValue

4단계: AddPercent
    ↓ (% 증가)
result *= (1 + addPercent%)

5단계: Multiple
    ↓ (곱셈)
finalValue = result * multipleValue
```

### 수식

```
최종값 = ((initialValue * (1 + initialPercent%)) + addValue) * (1 + addPercent%) * multipleValue
```

### 예제 계산

```csharp
CalcValue atk = new CalcValue();
atk.AddFormula("base", 100, AddInitial);           // 기본 공격력 100
atk.AddFormula("str", 50, AddInitialByPercent);    // 힘 스탯 +50%
atk.AddFormula("weapon", 30, Add);                 // 무기 +30
atk.AddFormula("buff", 20, AddPercent);            // 버프 +20%
atk.AddFormula("crit", 2, Multiple);               // 크리티컬 2배

// 계산 과정:
// 1단계: initialValue = 100
// 2단계: 100 * (1 + 0.5) = 150
// 3단계: 150 + 30 = 180
// 4단계: 180 * (1 + 0.2) = 216
// 5단계: 216 * 2 = 432

Debug.Log(atk.Value);  // 432
```

### 왜 이 순서인가?

**게임 디자인 관점:**
1. **AddInitial**: 캐릭터의 기본 스탯
2. **AddInitialByPercent**: 레벨/스탯에 비례한 증가
3. **Add**: 장비/아이템의 고정 보너스
4. **AddPercent**: 버프/디버프 효과
5. **Multiple**: 크리티컬, 약점 공격 등 특수 효과

**밸런스 관점:**
- 고정값(Add)이 %보다 먼저 적용되면 % 버프의 효과가 너무 강력해짐
- Multiple을 마지막에 적용하여 크리티컬 등 특수 효과가 모든 보너스를 곱하도록 함

---

## 사용 예시

### 기본 사용법

```csharp
// 1. 생성
CalcValue hp = new CalcValue(100);  // 기본값 100으로 시작

// 2. 공식 추가
hp.AddFormula("armor", 50, CalcOperator.AddInitial);
hp.AddFormula("vitality", 30, CalcOperator.AddInitialByPercent);
hp.AddFormula("buff", 20, CalcOperator.AddPercent);

// 3. 값 조회
Debug.Log($"Max HP: {hp.Value}");  // (100 + 50) * 1.3 * 1.2 = 234

// 4. 공식 제거
hp.RemoveFormula("buff");
Debug.Log($"Max HP: {hp.Value}");  // (100 + 50) * 1.3 = 195
```

---

### 실전 시나리오: RPG 캐릭터 스탯

```csharp
public class Character : MonoBehaviour
{
    // 주요 스탯
    public CalcValue maxHp = new CalcValue(100);
    public CalcValue atk = new CalcValue(10);
    public CalcValue def = new CalcValue(5);
    public CalcValue moveSpeed = new CalcValue(5.0);

    private void Start()
    {
        // 레벨업 시 스탯 증가
        int level = 10;
        maxHp.AddFormula("level", level * 10, CalcOperator.AddInitial);
        atk.AddFormula("level", level * 2, CalcOperator.AddInitial);

        // 장비 착용
        EquipWeapon("legendary_sword", 50);
        EquipArmor("plate_armor", 100, 30);

        // 버프 적용
        ApplyBuff("rage", 50);  // 공격력 +50%
    }

    private void EquipWeapon(string id, double atkBonus)
    {
        atk.AddFormula(id, atkBonus, CalcOperator.Add);
    }

    private void EquipArmor(string id, double hpBonus, double defBonus)
    {
        maxHp.AddFormula(id, hpBonus, CalcOperator.Add);
        def.AddFormula(id, defBonus, CalcOperator.Add);
    }

    private void ApplyBuff(string id, double percentBonus)
    {
        atk.AddFormula(id, percentBonus, CalcOperator.AddPercent);
    }

    private void RemoveBuff(string id)
    {
        atk.RemoveFormula(id);
    }

    // UI 업데이트
    private void UpdateUI()
    {
        Debug.Log($"HP: {maxHp.Value}");
        Debug.Log($"ATK: {atk.Value}");
        Debug.Log($"DEF: {def.Value}");
    }
}
```

---

### 실전 시나리오: 타워 디펜스 업그레이드

```csharp
public class Tower : MonoBehaviour
{
    public CalcValue damage = new CalcValue(10);
    public CalcValue attackSpeed = new CalcValue(1.0);  // 초당 공격 횟수
    public CalcValue range = new CalcValue(5.0);

    // 업그레이드 시스템
    public void Upgrade(string upgradeType, int level)
    {
        switch (upgradeType)
        {
            case "damage":
                // 레벨당 +5 데미지
                damage.AddFormula("upgrade_damage", level * 5, CalcOperator.Add);
                break;

            case "attackSpeed":
                // 레벨당 +10% 공격속도
                attackSpeed.AddFormula("upgrade_speed", level * 10, CalcOperator.AddPercent);
                break;

            case "range":
                // 레벨당 +15% 사거리
                range.AddFormula("upgrade_range", level * 15, CalcOperator.AddPercent);
                break;
        }
    }

    // 글로벌 버프 (모든 타워에 영향)
    public void ApplyGlobalBuff(string buffId, double percent)
    {
        damage.AddFormula($"global_{buffId}", percent, CalcOperator.AddPercent);
    }

    // 크리티컬 데미지
    public double GetCriticalDamage()
    {
        // 임시 CalcValue로 크리티컬 계산
        var critDamage = new CalcValue();

        // 현재 공식들을 복사
        foreach (var formula in damage.Formulas.Values)
        {
            critDamage.AddFormula(formula);
        }

        // 크리티컬 배율 적용
        critDamage.AddFormula("critical", 2.0, CalcOperator.Multiple);

        return critDamage.Value;
    }
}
```

---

### 실전 시나리오: 디버깅

```csharp
public class DebugManager : MonoBehaviour
{
    public Character player;

    [ContextMenu("Debug Player Stats")]
    private void DebugPlayerStats()
    {
        Debug.Log("=== Player HP ===");
        Debug.Log(player.maxHp.GetDebugInfo());

        Debug.Log("\n=== Player ATK ===");
        Debug.Log(player.atk.GetDebugInfo());

        Debug.Log("\n=== Player DEF ===");
        Debug.Log(player.def.GetDebugInfo());
    }

    // 특정 공식 찾기
    [ContextMenu("Find Legendary Sword")]
    private void FindLegendarySword()
    {
        if (player.atk.TryGetFormula("legendary_sword", out var formula))
        {
            Debug.Log($"Legendary Sword: +{formula.Value} ATK ({formula.Op})");
        }
        else
        {
            Debug.Log("Legendary Sword not equipped!");
        }
    }
}
```

---

## 확장 가능성

### 새로운 연산자 추가

```csharp
// 1. Enum에 추가
public enum CalcOperator
{
    AddInitial = 0,
    AddInitialByPercent = 10,
    AddPercent = 20,
    Multiple = 30,
    Add = 40,
    Exponential = 50  // 추가: 지수 계산
}

// 2. Calculate()에 로직 추가
private void Calculate()
{
    // ... 기존 코드 ...

    double exponentialValue = 1.0d;

    foreach (var formula in formulaDic.Values)
    {
        switch (formula.Op)
        {
            // ... 기존 케이스 ...

            case CalcOperator.Exponential:
                exponentialValue = Math.Pow(exponentialValue, formula.Value);
                break;
        }
    }

    _cachedValue = calcedInitial * (1 + addPercent * 0.01) * multipleValue * exponentialValue;
    _dirty = false;
}
```

---

### Min/Max 클램핑

CalcValue는 **순수 계산만** 담당합니다. 값 제한은 외부에서 처리하세요.

```csharp
// CalcValue는 순수 계산
public CalcValue moveSpeed = new CalcValue(5.0);

// 클램핑은 외부에서
public double GetClampedMoveSpeed()
{
    return Math.Clamp(moveSpeed.Value, 0.0, 10.0);  // 0 ~ 10 제한
}

// 또는 프로퍼티로
public double MoveSpeed => Math.Max(0, moveSpeed.Value);  // 최소 0
```

**왜 CalcValue 내부에 Min/Max가 없나?**
- 단일 책임 원칙 (SRP): CalcValue는 계산만 담당
- 유연성: 상황에 따라 다른 범위 적용 가능
- 테스트 용이: 계산 로직과 제한 로직을 분리

---

### 이벤트 시스템 추가 (확장 예시)

```csharp
// CalcValue.cs에 추가
public event Action<double> OnValueChanged;

private void Calculate()
{
    // ... 계산 로직 ...

    double oldValue = _cachedValue;
    _cachedValue = /* 계산 결과 */;
    _dirty = false;

    if (Math.Abs(oldValue - _cachedValue) > Epsilon)
    {
        OnValueChanged?.Invoke(_cachedValue);
    }
}

// 사용
player.maxHp.OnValueChanged += (newHp) =>
{
    healthBar.UpdateUI(newHp);
};
```

---

## 기술적 의사결정

### 1. Dirty Flag vs 즉시 계산

**문제:** 공식 추가/제거 시점에 계산 vs Value 접근 시점에 계산?

**결정:** Dirty Flag (지연 평가) 방식 선택

**이유:**
```csharp
// 시나리오: 여러 공식을 연속으로 추가 후 1번 조회
player.atk.AddFormula("weapon", 50, Add);
player.atk.AddFormula("buff1", 10, AddPercent);
player.atk.AddFormula("buff2", 5, AddPercent);
player.atk.AddFormula("skill", 20, Multiple);
var finalAtk = player.atk.Value;
```

**즉시 계산 방식:**
- AddFormula 4번 → 계산 4번 (O(4n))
- 비효율적!

**Dirty Flag 방식:**
- AddFormula 4번 → _dirty = true (O(4))
- Value 접근 1번 → 계산 1번 (O(n))
- **4배 효율 향상!**

**추가 근거:**
- 게임에서는 설정(Add/Remove)보다 조회(Value)가 훨씬 많음
- 초기화 시 대량의 공식을 추가하는 경우가 많음
- 매 프레임 Value를 참조해도 변경이 없으면 O(1)

---

### 2. CalcFormula를 struct vs class

**문제:** CalcFormula를 참조 타입(class)으로 할지 값 타입(struct)으로 할지?

**결정:** `readonly struct` 선택

**이유:**

**성능:**
- Stack 할당 → GC 부하 감소
- 복사 비용이 낮음 (3개 필드만)

**안정성:**
- 불변성 보장 → 실수로 수정 불가능
- Thread-Safe

**값 의미:**
- CalcFormula는 '데이터'이지 '엔티티'가 아님
- 같은 값이면 같은 객체로 취급되어야 함

**근거:**
```csharp
// struct: 값 복사
var formula1 = new CalcFormula("weapon", 50, Add);
var formula2 = formula1;
formula2.Value = 100;  // 컴파일 에러! (readonly)

// class였다면: 참조 공유
var formula1 = new CalcFormula("weapon", 50, Add);
var formula2 = formula1;
formula2.Value = 100;  // formula1도 영향받음! (버그)
```

---

### 3. 중간 캐시 vs 매번 Dictionary 순회

**문제:** Calculate()에서 매번 Dictionary를 순회할지, 중간 캐시를 유지할지?

**결정:** 중간 캐시 유지

**이유:**

**현재 구조:**
```csharp
private double initialValue = 0;
private double initialPercent = 0;
// ...

private void Calculate()
{
    // Dictionary 순회 → 중간 캐시 재구성
    // 중간 캐시 → 최종값 계산
}
```

**중간 캐시가 필요한 이유:**
1. **GetDebugInfo()**: 중간 캐시를 출력하여 디버깅 지원
2. **가독성**: 계산 과정을 명확히 분리
3. **확장성**: 중간값을 활용한 추가 기능 구현 가능

**Trade-off:**
- 메모리: 중간 캐시 5개 변수 유지 (40 bytes)
- 속도: Dictionary 순회는 어차피 필요 (큰 차이 없음)

---

### 4. Epsilon 값 선택

**문제:** 부동소수점 비교 정밀도를 얼마로 할까?

**결정:** `0.0001d` (0.01% 정밀도)

**이유:**

**너무 작으면 (1e-10):**
- 게임에서 불필요한 정밀도
- 가독성 떨어짐

**너무 크면 (0.01):**
- 유효한 값을 중립 원소로 오인

**0.0001 (0.01%):**
- 게임 수치에 적합한 정밀도
- 가독성 좋음
- 협업 시 이해하기 쉬움

**근거:**
```csharp
// HP 1000에 대해
1000 * 0.0001 = 0.1

// 0.1 HP 차이는 무시해도 됨 (게임 플레이에 영향 없음)
```

---

### 5. RecalculateAllCaches 제거

**초기 설계:**
```csharp
AddFormula() → RecalculateAllCaches() (O(n)) → _dirty = true
Value 접근 → Calculate() (O(1))
```

**개선된 설계:**
```csharp
AddFormula() → _dirty = true (O(1))
Value 접근 → Calculate() (O(n))
```

**왜 변경했나?**

**Dirty Flag 패턴의 핵심:** "필요할 때까지 계산을 미룬다"
- 초기 설계는 AddFormula 시점에 이미 계산 (패턴 위반!)
- 개선된 설계는 Value 접근 시점까지 계산 미룸 (패턴 준수!)

**성능 비교:**
```csharp
// 5개 공식 추가 후 1번 조회
// Before: 5n (RecalculateAllCaches 5번) + 1 (Calculate)
// After: n (Calculate 1번)
// 개선: 약 5배 효율 향상
```

**결론:** 불필요한 중간 단계 제거로 Lazy Evaluation 제대로 구현

---

## 성능 고려사항

### Dirty Flag 최적화

**O(1) 추가/제거:**
```csharp
AddFormula();   // Dictionary 추가 O(1) + _dirty = true
RemoveFormula(); // Dictionary 제거 O(1) + _dirty = true
```

**O(n) 계산 (필요할 때만):**
```csharp
Value;  // Dictionary 순회 O(n)
```

**실제 성능:**
- n = 10~30 (일반적인 공식 개수)
- O(n) 계산도 충분히 빠름
- 캐싱으로 반복 조회는 O(1)

---

### 메모리 사용

**CalcValue 1개당:**
- Dictionary: ~16 bytes (overhead) + n * (string + CalcFormula)
- 중간 캐시: 40 bytes (double * 5)
- 기타: 16 bytes (bool, double)
- **총: ~72 bytes + 공식 데이터**

**최적화:**
- `CalcFormula`를 struct로 → GC 부하 감소
- 중립 원소 필터링 → 불필요한 저장 방지

---

### GC 최적화

**Good:**
- `CalcFormula`는 struct → Stack 할당
- Dictionary는 재사용 (Clear하지 않음)
- 문자열 캐싱 (ID는 const string 사용 권장)

**Tips:**
```csharp
// Bad: 매번 새 문자열 생성
calcValue.AddFormula($"buff_{i}", 10, AddPercent);  // GC 발생!

// Good: const string 사용
private const string BUFF_ID = "rage_buff";
calcValue.AddFormula(BUFF_ID, 10, AddPercent);  // GC 없음!
```

---

### 대량 연산 최적화

**시나리오:** 1000명의 플레이어 HP를 동시에 업데이트

```csharp
// Bad: 매번 Value 접근
foreach (var player in players)
{
    player.hp.AddFormula("buff", 10, AddPercent);
    UpdateUI(player.hp.Value);  // 즉시 계산
}

// Good: 배치 처리
foreach (var player in players)
{
    player.hp.AddFormula("buff", 10, AddPercent);  // _dirty = true만
}
// ... 나중에 필요할 때
foreach (var player in players)
{
    UpdateUI(player.hp.Value);  // 이때 계산
}
```

---

## 베스트 프랙티스

### 1. ID 네이밍 컨벤션

```csharp
// Good: 명확하고 일관된 네이밍
"base_hp"
"item_legendary_sword"
"buff_rage"
"debuff_poison"
"skill_critical_strike"

// Bad: 모호하거나 일관되지 않은 네이밍
"buff1"
"test"
"temp"
```

---

### 2. const string 사용

```csharp
public class BuffIDs
{
    public const string RAGE = "buff_rage";
    public const string ARMOR = "buff_armor";
    public const string SPEED = "buff_speed";
}

// 사용
player.atk.AddFormula(BuffIDs.RAGE, 50, AddPercent);
player.atk.RemoveFormula(BuffIDs.RAGE);
```

**장점:**
- 오타 방지
- 자동완성 지원
- 리팩토링 용이
- GC 부하 감소

---

### 3. 외부 클램핑

```csharp
// Good: CalcValue는 계산만
public CalcValue moveSpeed = new CalcValue(5.0);

public double GetMoveSpeed()
{
    return Math.Max(0, moveSpeed.Value);  // 외부에서 클램핑
}

// Bad: CalcValue 내부에 클램핑 로직 (단일 책임 위반)
```

---

### 4. 디버그 정보 활용

```csharp
// 개발 중: 계산 과정 확인
#if UNITY_EDITOR
player.atk.LogDebugInfo();
#endif

// 버그 리포트: 상세 정보 저장
if (isWeirdBehavior)
{
    string report = player.atk.GetDebugInfo();
    File.WriteAllText("bug_report.txt", report);
}
```

---

### 5. 공식 존재 확인

```csharp
// Good: 존재 확인 후 제거
if (player.atk.HasFormula("buff_rage"))
{
    player.atk.RemoveFormula("buff_rage");
}

// 또는 TryGetFormula 사용
if (player.atk.TryGetFormula("weapon", out var weapon))
{
    Debug.Log($"Current weapon: +{weapon.Value} ATK");
}

// Bad: 무조건 제거 (없어도 에러는 안 나지만 의도 불명확)
player.atk.RemoveFormula("buff_rage");
```

---

## 라이선스

이 프로젝트는 포트폴리오 목적으로 제작되었습니다.
