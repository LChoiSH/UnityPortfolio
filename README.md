# Unity Client Portfolio · by Seonghoon Choi

> **KOR/ENG** · Production-ready Unity C# snippets & systems extracted from shipped/prototype work.  
> 이 저장소는 실전에서 검증한 **Unity C# 모듈**을 정리한 포트폴리오입니다.

---

## 🔎 Contents
- **Core Systems**
  - `CurrencySystem` — ScriptableObject **runtime clone**
  - `RoguelikeSystem` — Tiered effect gacha, duplicate handling, per-tier draws
  - `GachaSystem` — Weighted sampling, without-replacement sampling, rate export
  - `DeckSystem` — (WIP) deck & selection helpers
  - `RewardSystem` — (WIP) reward pipeline

> Tip: Folder map lives under `Assets/Scripts/{CurrencySystem, RoguelikeSystem, GachaSystem, DeckSystem, RewardSystem, Utils}` (see repo tree).

---

## 🧩 Highlights
- **SO Runtime Clone Pattern**: prevent asset pollution; UI only sees **read-only interfaces**.
- **Auto-save Debounce (No Update polling)**: event-driven scheduling, optional **max wait** to guarantee flush.
- **Data-driven & Testable**: pure logic separated from framework; clear seams for unit tests.
- **Perf Hygiene**: no per-frame allocations; cached totals; safe event patterns.
- **Security Hygiene**: no hardcoded secrets; production uses platform secure storage (Android Keystore / iOS Keychain / DPAPI).

---

## 🚀 Quick Start
1. Just Clone this project. Or Copy desired folders under your project’s `Assets/`.
2. Use quickly!
---

## 🧰 Modules

### CurrencySystem
- `Currency` (ScriptableObject): `Amount`, `TotalAmount`, `Earn/Use/Reset`, events `onAmountChanged`, `onTotalAmountChanged`.
- `CurrencyManager` (MonoBehaviour): runtime **clone** & registry; exposes **read-only view** to UI; **auto-save debounce** (Invoke/CancelInvoke).
- Save format (example): `CurrencyData { version:int, currencyStates: Dictionary<string, CurrencyState> }`.
- Guards: duplicate/empty title validation, TryGet* API, duplicate-subscription safe.

### RoguelikeSystem
- `RoguelikeManager`: per-tier pools (`RogueTier`), **duplicate control**, **per-tier draw** or single-tier cached draw.
- CSV import helper (Editor only) to build effects.
- Example choice flow: build candidate pool → draw `N` effects (with/without replacement) → enforce `Limit` by removing exhausted effects.

### GachaSystem
- `Gacha<T>`: weighted draw; cached `TotalWeight`; **without-replacement** multi-draw; rate export.
- API sketch:
  - `AddGacha(item, weight)`, `RemoveItem(item)`, `GetRandom()`
  - `GetRandomMultiple(count, allowDuplicate)`, `AllItems` / `GachaItems`

> For without-replacement across multiple tiers, compose tier pools into a temporary list and apply the same algorithm.

---

## ✅ Quality Checklist
- Readability: intention-revealing names, short functions.
- Maintainability: single responsibility, low coupling/high cohesion.
- Testability: pure logic seams; deterministic RNG injection when needed.
- Performance: avoid per-frame LINQ/boxing/allocations; cached sums.
- Security: no secrets committed; dummy tokens only.

---

## 🔬 Testing Ideas
- Currency: Earn/Use boundary (0, cap, overflow); autosave debounce **N changes → 1 save**.
- Gacha: distribution sanity (large sims, tolerance); without-replacement draws keep monotonic total weight.
- Roguelike: duplicate flag and per-tier mode work as intended; CSV import validation.

---

## 📁 Folder Structure (example)
```
/Assets
  /Scripts
    /Base
    /CalcSystem
    /CurrencySystem
    /DeckSystem
    /GachaSystem
    /RewardSystem
    /RoguelikeSystem
    /Utils
      /Localization
README.md
```
---

## 📜 License & Credits
- Author: **Seonghoon Choi**
- Contact: csh42504@gmail.com
- License: [MIT / Proprietary]
- Shipped/Prototypes (selected code only): War and Peas, My Stellar Idle, Box Inc.

---

## 🗒 Changelog
- 2025‑10‑01: initial upload
- 2025‑10‑01: CalcSystem, CurrencySystem, DeckSystem, GachaSystem, RewardSystem, RoguelikeSystem
