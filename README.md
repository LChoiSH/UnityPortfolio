# DDGames · Unity Client Portfolio  (KOR/ENG)

> Summary
> - KOR: 상용/프로토타입에서 실제로 사용한 **Unity C# 모듈과 샘플 코드**를 정리한 포트폴리오 저장소입니다.
> - ENG: A curated portfolio of **Unity C# modules & code samples** drawn from shipped titles and prototypes.

## What’s inside | 구성
- **Core Systems**
  - `Currency` & `CurrencyManager` (SO runtime clone, read-only view, auto-save debounce)
  - `Gacha<T>` (weighted sampling, prefix sums/cache, data-driven)
  - [More: Save/Load, Addressables helpers, Event Bus]
- **Samples / Scenes**
  - `Samples/CurrencyDemo` : Earn/Use UI with autosave
  - `Samples/GachaSample` : probability sampler & viz
- **Docs**
  - design intent, perf/quality checklist, test guide

## Highlights | 핵심 포인트
- ScriptableObject runtime clone pattern (no asset pollution), UI gets read-only interface
- Auto-save Debounce (no Update polling): event-driven scheduling with optional max wait
- Data-driven & testable: pure logic separated from framework
- Perf hygiene: zero per-frame allocations, events and caching
- Security hygiene: never hardcode secrets (samples are dummy)

## Tech Stack
- Engine: Unity (URP, 2D first)
- Language: C# 10+
- Patterns: SO Runtime Clone, ReadOnly View, Event Aggregator, CQRS-lite
- Tooling: Unity Profiler, Frame Debugger, Addressables, Rider/VS, Git

## Quick Start | 빠른 시작
1. Copy needed folders under `Assets/` of your Unity project
2. (Optional) add asmdef references if you use them
3. Open `Samples/CurrencyDemo` scene and Play. Use buttons to Earn/Use. Check console for debounced saves.

## Modules | 모듈 요약

### Currency & Manager
- `Currency` (ScriptableObject): `Amount`, `TotalAmount`, `Earn/Use/Reset`, events `onAmountChanged`, `onTotalAmountChanged`
- `CurrencyManager` (MonoBehaviour): creates runtime clones, exposes read-only view to UI, auto-save debounce
- Save format: `CurrencyData { version:int, currencyStates: Dictionary<string, CurrencyState> }`
- Guards: duplicate/empty title validation, `TryGet*` API, duplicate-subscription safe

### Gacha<T>
- cached total weights, edge cases (0/negative), sample scene included

## Code Quality | 품질 기준
- Readability: intention-revealing names, sub-30 line functions
- Maintainability: single responsibility, low coupling/high cohesion
- Testability: pure logic separated, clear test seams
- Performance: no per-frame unnecessary LINQ/boxing/allocs
- Security: no secrets/tokens committed (all samples are dummy)

## How to Test | 테스트 가이드 (예시)
- Earn/Use boundary cases (0, Max, overflow)
- Auto-save debounce: N changes -> 1 save
- Gacha distribution sampling (large sims, tolerance)

## Folder Structure | 폴더 구조 (예시)
```
/Assets
  /Portfolio
    /Currency
      Currency.cs
      CurrencyManager.cs
      CurrencyState.cs
      CurrencyData.cs
      README.md
    /Gacha
      Gacha.cs
      GachaPair.cs
      GachaSample.cs
      README.md
    /Samples
      /CurrencyDemo
      /GachaSample
    /Docs
      architecture.md
      quality-checklist.md
README.md
```

## Security | 보안
- No real service keys/tokens in this repo.
- For production, use platform secure storage (Android Keystore / iOS Keychain / DPAPI).

## Metrics (Optional) | 성능 지표 (선택)
- Example: GC alloc/frame: 0B, Debounced saves: <= 2/sec, UI batches 120 -> 60

## Projects & Credits
- Shipped / Prototypes (selected code only)
  - War and Peas (Angry Birds-like)
  - My Stellar Idle (Incremental)
  - Box Inc. (Clicker/Merge)
- Author: DDGames / Seonghoon Choi
- Contact: [email/linkedin/site]
- License: [MIT/Proprietary]

## Changelog
- 2025-10-01: initial upload (currency/gacha modules, sample scenes)
