# Unity Client Portfolio · by Seonghoon Choi

Production-ready **Unity C# systems & editor tooling** for mobile games.  
This README emphasizes **programming** and **feature-focused** descriptions.

**Repository:** [github.com/LChoiSH/UnityPortfolio](https://github.com/LChoiSH/UnityPortfolio)

---

## 🔗 Quick Navigation

- [CalcSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CalcSystem)
- [CurrencySystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CurrencySystem)
- [GachaSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/GachaSystem)
- [RoguelikeSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/RoguelikeSystem)
- [Utils / Localization](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Localization)
- [Utils / Attributes (CustomEditorButton)](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Attributes)
- [Base / Common](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Base)

---

## 🛠️ Setup

1. Copy the needed module folders into your project’s `Assets/`.
2. Open any sample scenes (if provided) or plug modules into your project.
3. Unity **2021 LTS+** recommended.

---

## 🧰 Systems (Feature-Focused)

### CalcSystem
Feature Highlights
- **Formula Stack with Lazy Evaluation:** multiple changes coalesce; value computes once on demand via a dirty flag.
- **Operator Aggregation:** additive / multiplicative / exponential operators merge without per-frame allocations.
- **Extensible Operators & Ordering:** add new operators or retune evaluation order for balance.
- **Deterministic & Debuggable:** consistent evaluation path; easy to surface internals for inspector/logs.
- **Reusable Everywhere:** HP/ATK/ASPD, growth curves, drop modifiers, economy scaling, etc.

**Path:** [Assets/Scripts/CalcSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CalcSystem)

---

### CurrencySystem
Feature Highlights
- **ScriptableObject Database:** one source of truth across scenes/projects.
- **Runtime Clone + Read-Only UI View:** prevents asset pollution; UI consumes safe interfaces only.
- **Autosave Debounce:** bursts of Earn/Use collapse to a single persisted write.
- **Integrity & Guardrails:** duplicate/empty titles blocked; event-driven UI binding.
- **Sample & Editor Tooling:** includes a **Currency Editor Window** and a sample wiring.

**Path:** [Assets/Scripts/CurrencySystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CurrencySystem)

---

### GachaSystem
Feature Highlights
- **Weighted Sampling Core:** stable draws with cached `TotalWeight`, avoiding repeated recomputation.
- **Multi-Draw Without Replacement:** pull N unique results in a single pass (or allow duplicates if desired).
- **Rate Export Helpers:** export current pool rates for design/QA review.
- **Generic & Composable:** type-agnostic container that integrates with other gameplay systems.

**Path:** [Assets/Scripts/GachaSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/GachaSystem)

---

### RoguelikeSystem
Feature Highlights
- **Tiered Effect Pools:** organize effects by tier; pick per-tier or combined when needed.
- **Duplicate Handling & Limits:** toggle duplicates on/off; usage limits naturally exhaust effects.
- **CSV-Driven Authoring:** designers author titles/params via CSV; importer builds scriptable data.
- **Typed Parameters per Effect:** a single effect can carry string/int/float parameters together.
- **Constraint Hooks:** optional rules to enable/disable effects based on the current run state.

**Path:** [Assets/Scripts/RoguelikeSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/RoguelikeSystem)

---

## 🧩 Utilities

### Utils / Localization
Feature Highlights
- **Per-Locale Used-Character Export:** scan localized tables and output the exact glyph set per locale.
- **TMP Font Asset Pipeline:** build lean font assets while guaranteeing glyph coverage.
- **Formatting Helpers:** safe placeholder formatting and consistent output across languages.
- **CSV-Friendly Workflow:** quick updates by non-programmers with reduced duplication/omissions.

**Path:** [Assets/Scripts/Utils/Localization](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Localization)

---

### Utils / Attributes (CustomEditorButton)
Feature Highlights
- **One-Click Actions in Inspector:** expose safe buttons on MonoBehaviours/ScriptableObjects to trigger editor-time operations.
- **Non-Developer Friendly:** producers/designers can run routine tasks (reset data, bake assets, refetch CSV, etc.) **without code**.
- **Faster Iteration & Lower Error Rate:** eliminates ad-hoc menu hunting and reduces copy-paste mistakes.
- **Supports Real-World Flows:** batch-create ScriptableObjects from CSV, rebuild lookup tables, trigger validation passes, snapshot data, etc.
- **Portfolio-Oriented UX:** clear button labels, optional confirmation prompts, and guardrails to protect runtime assets.

**Path:** [Assets/Scripts/Utils/Attributes](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Attributes)

---

### Base / Common
Feature Highlights
- **Inspector Utilities & Extensions:** small, dependency-light helpers reused across modules.
- **Iteration Enablers:** patterns and snippets that standardize editor/runtime behavior.

**Path:** [Assets/Scripts/Base](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Base)

---

## 🗒 Changelog

- **2025-10-01** — Initial upload (Calc, Currency, Gacha, Roguelike + utils)  
- **2025-10-10** — Inspector Button (Editor attribute)  
- **2025-10-13** — CurrencySystem Window  
- **2025-10-14** — CurrencySystem Sample Scene
- **2025-10-22** — Add README

---

# (KOR) 한국어 버전

모바일 게임을 위한 **Unity C# 시스템/에디터 툴** 모음입니다.  
본 문서는 각 시스템의 **기능 위주**로 설명합니다.

**Repository:** [github.com/LChoiSH/UnityPortfolio](https://github.com/LChoiSH/UnityPortfolio)

---

## 🔗 빠른 이동

- [CalcSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CalcSystem)
- [CurrencySystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CurrencySystem)
- [GachaSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/GachaSystem)
- [RoguelikeSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/RoguelikeSystem)
- [Utils / Localization](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Localization)
- [Utils / Attributes (CustomEditorButton)](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Attributes)
- [Base / Common](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Base)

---

## 🧰 시스템 (기능 중심)

### CalcSystem
기능
- **지연 평가(Dirty Flag):** 변경을 모아 **요청 시 1회** 계산.
- **연산자 집계:** 합/곱/지수 등의 연산을 캐시 기반으로 빠르게 합성.
- **연산자 확장 & 순서 조정:** 신규 연산 추가, 밸런싱 목적의 평가 순서 수정이 용이.
- **디버그 친화:** 일관된 평가 경로, 인스펙터/로그로 내부 상태 확인 용이.
- **범용 재사용:** HP/ATK/공속, 성장 곡선, 드랍 보정, 경제 스케일링 등.

**경로:** [Assets/Scripts/CalcSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CalcSystem)

---

### CurrencySystem
기능
- **SO 데이터베이스:** 모든 씬에서 동일 데이터로 운영.
- **런타임 클론 + UI 읽기 전용:** 에셋 오염 방지, UI는 안전한 인터페이스만 접근.
- **자동 저장 디바운스:** 잦은 Earn/Use를 **1회 저장**으로 묶음.
- **무결성 가드:** 중복/공백 타이틀 차단, 이벤트 기반 UI 바인딩.
- **샘플/에디터 툴:** **Currency Editor Window** 및 샘플 씬 제공.

**경로:** [Assets/Scripts/CurrencySystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/CurrencySystem)

---

### GachaSystem
기능
- **가중치 추첨 코어:** 캐시된 `TotalWeight`로 안정적, 무의미한 재계산 없이 동작.
- **중복 없는 멀티 추첨:** 한 번에 N개 유니크 결과 추출(필요 시 중복 허용).
- **확률 내보내기:** 현재 풀 상태를 디자인/QA 검증용으로 익스포트.
- **일반화된 구조:** 타입 제약 없이 다른 시스템과 쉽게 결합.

**경로:** [Assets/Scripts/GachaSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/GachaSystem)

---

### RoguelikeSystem
기능
- **티어 기반 효과 풀:** 티어별/통합 선택을 유연하게 지원.
- **중복/소진 관리:** 중복 허용 토글, 사용 제한으로 자연스러운 소진 처리.
- **CSV 기반 저작:** 디자이너가 CSV로 타이틀/파라미터를 관리, 임포터로 스크립터블 데이터 생성.
- **타입드 파라미터:** 하나의 효과가 문자열/정수/실수 파라미터를 동시에 가질 수 있음.
- **제약 훅:** 진행 상태에 따라 효과 활성/비활성 조건을 손쉽게 부여.

**경로:** [Assets/Scripts/RoguelikeSystem](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/RoguelikeSystem)

---

## 🧩 유틸리티

### Utils / Localization
기능
- **로케일별 사용 문자 추출:** 테이블을 스캔하여 실제 쓰이는 글리프만 수집.
- **TMP 폰트 에셋 파이프라인:** 용량을 줄이면서 글리프 누락을 방지.
- **포맷팅 헬퍼:** 안전한 플레이스홀더 처리와 일관된 문자열 출력.
- **CSV 친화 워크플로:** 비개발자도 빠르게 갱신 가능, 중복/누락 감소.

**경로:** [Assets/Scripts/Utils/Localization](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Localization)

---

### Utils / Attributes (CustomEditorButton)
기능
- **인스펙터 원클릭 액션:** MonoBehaviour/ScriptableObject에 에디터 전용 버튼을 노출해 반복 작업을 즉시 실행.
- **비개발자 친화:** 기획/아트도 코드 없이 데이터 리셋, 에셋 베이크, CSV 재가져오기 등 루틴 작업 수행.
- **작업 효율↑, 오류↓:** 메뉴 탐색/복붙/콘솔 명령을 버튼화하여 속도와 안정성 동시 확보.
- **현실적인 시나리오:** CSV → ScriptableObject 일괄 생성, 룩업 테이블 리빌드, 검증 패스 실행, 데이터 스냅샷 등.
- **포트폴리오 지향 UX:** 명확한 버튼 라벨, 선택적 확인 팝업, 런타임 에셋 보호 가드레일.

**경로:** [Assets/Scripts/Utils/Attributes](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Utils/Attributes)

---

### Base / Common
기능
- **인스펙터 유틸 & 확장:** 모듈 전반에서 재사용되는 경량 헬퍼.
- **개발 반복 최적화:** 에디터/런타임 공통 패턴을 표준화해 일관된 동작 확보.

**경로:** [Assets/Scripts/Base](https://github.com/LChoiSH/UnityPortfolio/tree/main/Assets/Scripts/Base)

---

## 🗒 변경 이력

- **2025-10-01** — 초기 업로드 (Calc, Currency, Gacha, Roguelike + utils)  
- **2025-10-10** — 인스펙터 버튼(에디터 어트리뷰트)  
- **2025-10-13** — CurrencySystem 윈도우  
- **2025-10-14** — CurrencySystem 샘플 씬
- **2025-10-22** — ReadMe 파일 반영
