# Unity Client Portfolio · by 최성훈

**프로덕션 수준의 Unity C# 시스템 모음**

모바일 게임 개발을 위한 재사용 가능한 시스템들로, 디자인 패턴과 아키텍처 설계 능력을 시연합니다.

**Repository:** [github.com/LChoiSH/UnityPortfolio](https://github.com/LChoiSH/UnityPortfolio)

---

## 🎯 핵심 강점

- ✅ **디자인 패턴 중심 설계** - State, Modifier, Factory, Registry 등 실무 패턴 적용
- ✅ **확장성과 유지보수성** - 새 기능 추가 시 기존 코드 수정 최소화 (Open-Closed Principle)
- ✅ **성능 최적화** - Object Pooling, Dirty Flag, Debouncing 등 최적화 기법
- ✅ **모듈화** - 각 시스템이 독립적으로 동작, 프로젝트 간 재사용 가능

---

## 📋 요구사항

- **Unity:** 2021 LTS 이상
- **의존성:** Addressables, Localization, TextMeshPro, VInspector, Newtonsoft.Json
- **C#:** .NET Standard 2.1

---

## 🚀 실행 방법

1. 원하는 시스템 폴더를 프로젝트 `Assets/`에 복사
2. 각 시스템의 `Sample/` 폴더에서 샘플 씬 확인
3. 각 시스템 README에서 상세 사용법 확인

---

## 🔗 빠른 네비게이션

- [UnitSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/UnitSystem)
- [CalcSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CalcSystem)
- [CurrencySystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CurrencySystem)
- [GachaSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/GachaSystem)
- [RoguelikeSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/RoguelikeSystem)
- [Utils / Localization](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Localization)
- [Utils / EditorButton](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Attributes)

---

## 🧰 시스템 소개

### UnitSystem ⭐
> 전투 및 유닛 관리 시스템

**주요 기능**
- State Pattern 기반 상태 관리 (Idle, Attack, Move, Death)
- Modifier Pattern으로 확장 가능한 버프/디버프 시스템
- Factory Pattern으로 유닛 타입별 다른 상태 조합
- Component 기반 설계 (Attacker, Defender, Mover)
- Object Pooling 성능 최적화

**적용 패턴:** State, Template Method, Factory, Modifier, Component, Singleton, Object Pooling

**상세 문서:** [UnitSystem README](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/UnitSystem)

---

### CalcSystem
> 수식 기반 계산 시스템

**주요 기능**
- Dirty Flag 패턴으로 지연 평가 (여러 변경사항을 한 번에 계산)
- 연산자 집계 (가산/곱셈/지수 연산 최적화)
- 확장 가능한 연산자 구조
- HP/ATK/공속, 성장 곡선, 경제 스케일링 등 범용 활용

**적용 패턴:** Dirty Flag, Data-Driven Design

**상세 문서:** [CalcSystem README](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CalcSystem)

---

### CurrencySystem
> 인게임 화폐 관리 시스템

**주요 기능**
- ScriptableObject 기반 화폐 데이터베이스
- 런타임 클론으로 에셋 오염 방지
- Save/Load with AES 암호화
- Debouncing으로 자동 저장 최적화
- 커스텀 Editor Window 제공

**적용 패턴:** Singleton, ScriptableObject Database, Event-Driven

**상세 문서:** [CurrencySystem README](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CurrencySystem)

---

### GachaSystem
> 가중치 기반 추첨 시스템

**주요 기능**
- 캐시된 TotalWeight로 효율적인 추첨
- 중복 허용/비허용 다중 추첨
- 확률 내보내기 (QA 검증용)
- Generic 타입으로 범용 활용 가능

**적용 패턴:** Generic Programming, Dirty Flag

**상세 문서:** [GachaSystem README](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/GachaSystem)

---

### RoguelikeSystem
> 로그라이크 효과 시스템

**주요 기능**
- Registry Pattern으로 효과 실행 시스템
- 티어별 효과 풀 관리
- CSV 기반 효과 데이터 저작
- 중복/사용 제한 처리
- 제약 조건 훅

**적용 패턴:** Registry, Strategy, Data-Driven Design

**상세 문서:** [RoguelikeSystem README](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/RoguelikeSystem)

---

### RewardSystem
> 보상 분배 시스템

**주요 기능**
- Enum 기반 보상 타입 (Currency, Exp, Unit)
- 다른 시스템과 통합 (CurrencySystem, DeckSystem)
- 비율 기반 보상 조정

---

### Utils
> 범용 유틸리티 및 에디터 툴

**주요 기능**
- **Localization:** 로케일별 사용 문자 추출, TMP 폰트 최적화
- **EditorButton Attribute:** Inspector에서 원클릭 실행 (리플렉션 기반 파라미터 자동 드로잉)
- **DataManager:** JSON Save/Load with AES 암호화
- **CSVReader:** 데이터 기반 워크플로우

**상세 문서:**
- [Localization](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Localization)
- [EditorButton](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Attributes)

---

## 📊 적용된 디자인 패턴

| 패턴 | 적용 시스템 | 목적 |
|------|------------|------|
| **State Pattern** | UnitSystem | 상태별 행동 캡슐화 및 명확한 전환 관리 |
| **Template Method** | UnitSystem | 공통 로직 베이스 클래스, 세부 구현 서브클래스 |
| **Factory Pattern** | UnitSystem | 유닛 타입별 다른 상태 조합 제공 |
| **Modifier (Chain of Responsibility)** | UnitSystem | 확장 가능한 버프/디버프 체인 |
| **Component Pattern** | UnitSystem | 기능별 분리 및 재사용성 |
| **Singleton Pattern** | UnitSystem, CurrencySystem | 전역 관리자 |
| **Object Pooling** | UnitSystem | GC 부하 감소 |
| **Registry Pattern** | RoguelikeSystem | 확장 가능한 효과 매핑 |
| **Strategy Pattern** | RoguelikeSystem | 효과 카테고리별 다른 핸들러 |
| **Dirty Flag** | CalcSystem, GachaSystem | 지연 평가로 성능 최적화 |
| **ScriptableObject Database** | CurrencySystem | 데이터 중앙 관리 |
| **Event-Driven** | 전체 시스템 | 느슨한 결합 |

---

## 📧 연락처

- **GitHub:** [github.com/LChoiSH](https://github.com/LChoiSH)
- **Email:** csh42504@gmail.com

---

## 🗒 변경 이력

- **2025-11-20** — RoguelikeSystem README 수정
- **2025-11-19** — README 재작성, RoguelikeSystem 리팩토링
- **2025-11-18** — UnitSystem 추가 (State Pattern, Modifier Pattern), CalcSystem 리팩토링 + README
- **2025-10-28** — UnitSystem Factory Pattern 추가
- **2025-10-22** — UnitSystem 기본 구조, Main README 작성
- **2025-10-16** — CalcSystem Dirty Flag 패턴 적용
- **2025-10-15** — RoguelikeSystem 리팩토링 + Sample Scene
- **2025-10-14** — CurrencySystem Sample Scene 추가
- **2025-10-13** — CurrencySystem Window 추가
- **2025-10-10** — EditorButton Attribute 추가
- **2025-10-01** — 초기 업로드 (CalcSystem, CurrencySystem, GachaSystem, RoguelikeSystem, Localization)

---
