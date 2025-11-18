# Calc System

> 확장 가능하고 성능 최적화된 동적 계산 시스템
>
> Dirty Flag Pattern을 활용한 지연 평가 기반 계산 엔진

## 📋 목차

- [개요](#개요)
- [디렉토리 구조](#디렉토리-구조)
- [핵심 기능](#핵심-기능)
- [주요 API](#주요-api)
- [계산 파이프라인](#계산-파이프라인)
- [사용 예시](#사용-예시)
- [성능 최적화](#성능-최적화)

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
- [`CalcValue.cs`](CalcValue.cs): 공식 관리, 캐시 관리, 지연 평가 담당
- [`CalcFormula.cs`](CalcFormula.cs): `readonly struct`로 구현된 불변 공식
- [`CalcOperator.cs`](CalcOperator.cs): 5단계 계산 순서를 정의하는 Enum

---

## 핵심 기능

### 1. Dirty Flag Pattern (지연 평가)

값이 변경되었을 때만 재계산하여 불필요한 연산을 최소화합니다.

```csharp
// 공식 추가 시: 계산하지 않고 dirty 표시만
public void AddFormula(CalcFormula formula)
{
    formulaDic[formula.Id] = formula;
    _dirty = true;  // "나중에 계산" 표시
}

// Value 접근 시: dirty일 때만 계산
public double Value
{
    get
    {
        if (_dirty) Calculate();  // 이때 계산!
        return _cachedValue;      // 캐시 반환
    }
}
```

**성능:**
- 여러 공식 연속 추가 → Value 접근 시 **단 1번만 계산**
- 값 변경 없으면 캐시 반환 (O(1))
- 초기화 시 계산 부하 없음

**예시:**
```csharp
player.atk.AddFormula("weapon", 50, AddInitial);  // O(1)
player.atk.AddFormula("buff", 10, AddPercent);    // O(1)
var finalAtk = player.atk.Value;                  // O(n) - 1번만!
```

---

### 2. Value Object Pattern (readonly struct)

CalcFormula를 불변 구조체로 설계하여 데이터 무결성을 보장합니다.

```csharp
public readonly struct CalcFormula
{
    public string Id { get; }
    public double Value { get; }
    public CalcOperator Op { get; }
}
```

**왜 readonly struct?**
- Stack 할당으로 GC 부하 감소
- 불변성으로 데이터 무결성 확보
- Value Object 패턴에 적합 (데이터는 값 타입)

---

### 3. 부동소수점 정밀도 처리 (Epsilon)

부동소수점 오차(`0.1 + 0.2 != 0.3`)를 고려한 비교 연산을 사용합니다.

```csharp
private const double Epsilon = 0.0001d;  // 0.01% 정밀도

private bool IsNeutralElement(CalcOperator op, double value)
{
    if (op == CalcOperator.Multiple)
        return Math.Abs(value - 1.0d) < Epsilon;
    else
        return Math.Abs(value) < Epsilon;
}
```

**왜 0.0001d?**
- 게임 수치에 적합한 정밀도 (HP 1000 기준 0.1 차이는 무시 가능)
- 너무 작으면(1e-10) 불필요, 너무 크면(0.01) 유효한 값 오인
- 가독성 좋음 (과학적 표기법보다 명확)

---

## 주요 API

**공식 관리:**
- `AddFormula(id, value, op)` - 공식 추가
- `RemoveFormula(id)` - 공식 제거
- `HasFormula(id)` - 공식 존재 확인
- `TryGetFormula(id, out formula)` - 공식 조회

**값 조회:**
- `Value` - 계산된 최종값 반환
- `Formulas` - 모든 공식의 읽기 전용 뷰

**디버그:**
- `GetDebugInfo()` - 디버그 정보 문자열 반환
- `LogDebugInfo()` - Unity Console 출력 (Editor Only)

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

## 성능 최적화

**시간 복잡도:**
- AddFormula / RemoveFormula: O(1)
- Value 접근: O(n) - 단, dirty일 때만
- 캐싱으로 반복 조회는 O(1)

**메모리:**
- CalcFormula는 struct → Stack 할당, GC 부하 없음