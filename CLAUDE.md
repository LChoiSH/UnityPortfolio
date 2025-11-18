# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Portfolio Context - CRITICAL

**This is a job-seeking portfolio project designed to demonstrate production-level Unity development skills.**

### Primary Purpose
- Validate real-world game development capabilities for employment
- Demonstrate mastery of design patterns, architecture, and best practices
- Showcase systems that are extensible, maintainable, and performant

### Target Audience
- Hiring managers and technical interviewers
- Senior engineers evaluating code quality
- Recruiters assessing technical competency

### Core Principles When Working on This Codebase

1. **Code Quality Over Speed** - Pattern clarity and architectural soundness are more important than quick implementation
2. **Documentation is Critical** - READMEs must clearly explain design patterns, benefits, and design intent
3. **Demonstrate Expertise** - Every system should showcase specific skills (State Pattern, Factory, Modifiers, etc.)
4. **Real-World Readiness** - Code should reflect production standards (performance optimization, extensibility, maintainability)

### When Writing Documentation/READMEs

**ALWAYS include:**
- **Design patterns used** and why they were chosen
- **Technical benefits** (extensibility, performance, maintainability)
- **Design intent** and considerations behind architectural decisions
- **Code examples** showing how to use and extend the system
- **Trade-offs** and technical decision rationale

**Style:**
- Clear, professional technical writing
- Assume the reader is a senior developer evaluating your skills
- Highlight problem-solving approach and engineering judgment
- Use diagrams/examples to illustrate architecture

**See `Assets/Scripts/UnitSystem/README.md` as the gold standard** - it demonstrates:
- Clear pattern explanations with code examples
- Technical decision rationale ("Why we chose X over Y")
- Extensibility examples
- Real-world considerations

---

## Project Overview

Unity C# portfolio showcasing production-ready systems for mobile games. This is a modular, pattern-driven architecture emphasizing extensibility, maintainability, and performance optimization.

**Unity Version:** 2021 LTS+ recommended
**Language:** C# (.NET Standard 2.1)
**Key Dependencies:** Unity Addressables, Localization, TextMeshPro, VInspector, Newtonsoft.Json

## Core Systems Architecture

The codebase is organized into independent, reusable systems under `Assets/Scripts/`:

### 1. UnitSystem (State + Modifier Patterns)
**Purpose:** Combat and unit management demonstrating advanced design patterns.

**Patterns Demonstrated:**
- **State Pattern** - Clean behavior management through discrete states
- **Template Method Pattern** - Common logic in base class, customization in subclasses
- **Factory Pattern** - Different state combinations per unit type
- **Modifier Pattern (Chain of Responsibility)** - Extensible buff/debuff system
- **Component Pattern** - Separation of concerns (Attacker/Defender/Mover)
- **Object Pooling** - Performance optimization for frequent spawning
- **Singleton Pattern** - Global unit management

**Key Classes:**
- `Unit.cs` - Main MonoBehaviour entity
- `UnitStateMachine.cs` - Manages state transitions with validation
- `UnitStateBase.cs` - Template Method implementation
- `UnitStateFactory.cs` - Creates different state sets per UnitType
- `Attacker.cs` / `Defender.cs` / `Mover.cs` - Component-based design
- `IHitModifier` / `IHealModifier` - Modifier chain for buffs/debuffs
- `UnitFactory.cs` - Object pooling (Unity's ObjectPool)

**Design Intent:**
- **Extensibility:** Add new states/modifiers without modifying existing code (Open-Closed Principle)
- **Maintainability:** Each state's logic is isolated, easy to debug
- **Flexibility:** Different unit types get different state combinations via Factory
- **Performance:** Object pooling minimizes GC pressure

**To Extend:**
- New state: Extend `UnitStateBase`, register in `UnitStateFactory.CreateStates()`
- New modifier: Implement `IHitModifier`, attach via `Attacker.AddAttackModifier()`
- New unit type: Add to enum, create case in `UnitStateFactory`

**Full documentation:** `Assets/Scripts/UnitSystem/README.md` (includes pattern rationale, code examples, technical decisions)

### 2. CurrencySystem (Singleton + Save/Load Pattern)
**Purpose:** In-game economy with ScriptableObject database and runtime cloning.

**Patterns Demonstrated:**
- **Singleton Pattern** - Global currency management
- **ScriptableObject Database** - Designer-friendly configuration
- **Runtime Cloning** - Prevent asset pollution
- **Debouncing** - Performance optimization (autosave)
- **Event-Driven** - Loose coupling for UI updates

**Key Classes:**
- `CurrencyManager.cs` - Singleton with Save/Load (implements `HaveSave`, `HaveLoad`)
- `Currency.cs` - Currency instance with state tracking
- `CurrencyDatabaseSO.cs` - ScriptableObject definitions
- `CurrencyDatabaseWindow.cs` - Editor GUI for managing currencies

**Design Intent:**
- **Integrity:** Duplicate/empty titles blocked
- **Separation:** Runtime data separated from ScriptableObject assets
- **Efficiency:** Burst transactions collapsed into single save
- **Usability:** Editor window for non-programmer workflow

### 3. DeckSystem (Collection Management)
**Purpose:** Player's unit deck with gacha integration.

**Key Classes:**
- `DeckManager.cs` - Singleton with Save/Load
- `UnitInfo.cs` - Unit definitions
- `UserUnitInfo.cs` - Player's unit instances
- `UnitLevelStat.cs` - Level progression data

**Design Intent:** Integration point between Gacha and Unit systems, demonstrates system composition.

### 4. GachaSystem (Weighted Random Selection)
**Purpose:** Generic weighted gacha demonstrating algorithm implementation.

**Patterns Demonstrated:**
- **Generic Programming** - Reusable across different types
- **Dirty Flag Pattern** - Cached TotalWeight for performance
- **Algorithm Implementation** - Weighted random selection

**Key Classes:**
- `Gacha<T>.cs` - Generic gacha with `GetRandom()`, `GetRandomMultiple()`, `GetRates()`
- `GachaPair<T>.cs` - Item + weight pair

**Design Intent:**
- **Performance:** Avoid recalculating total weight on every draw
- **Flexibility:** Multi-draw with/without replacement
- **Transparency:** Rate export for QA verification

### 5. RoguelikeSystem (Registry Pattern)
**Purpose:** Effect system for roguelike progression.

**Patterns Demonstrated:**
- **Registry Pattern** - Extensible effect execution system
- **Strategy Pattern** - Different effect handlers mapped by category
- **Data-Driven Design** - Effects defined in data, not code

**Key Classes:**
- `RogueRegistry.cs` - Static registry mapping `RogueEffectCategory` to `Action<EffectArgs>`
- `RogueEffect.cs` - Effect with tier and constraints
- `RoguelikeGachaPool.cs` - Tier-based effect pools

**Design Intent:** Easy to add new effects without code changes, demonstrates extensibility at scale.

**To Extend:** Add to `RogueEffectCategory` enum, register handler in `RogueRegistry.effectMap`

### 6. CalcSystem (Formula Evaluation)
**Purpose:** Formula-based calculations with lazy evaluation.

**Patterns Demonstrated:**
- **Dirty Flag Pattern** - Lazy evaluation for performance
- **Data-Driven Design** - Formulas defined as data

**Key Classes:**
- `CalcFormula.cs` - Formula with ID, value, operator
- `CalcOperator.cs` / `CalcValue.cs`

**Design Intent:**
- **Performance:** Multiple changes coalesce, compute once on demand
- **Reusability:** HP/ATK/ASPD, growth curves, drop modifiers, economy scaling
- **Extensibility:** Add new operators without breaking existing formulas

### 7. RewardSystem
**Purpose:** Reward distribution with enum-based dispatch.

**Key Classes:**
- `Reward.cs` - Reward struct with `GetReward()` logic

### 8. Utils (Cross-Cutting Concerns)
**Purpose:** Shared utilities demonstrating production concerns.

**Key Components:**
- `DataManager.cs` + `FileIO.cs` - JSON save/load with **AES encryption** (security consideration)
- `CSVReader.cs` - Data-driven design workflow
- `Localization/` - Per-locale glyph extraction for TMP fonts (mobile optimization)
- `Attributes/EditorButton/` - Custom inspector buttons with **reflection-based parameter drawing**
- `LogUI/` - In-game logging panel for debugging

**Design Intent:** Demonstrates production concerns beyond gameplay (security, performance, tooling)

---

## Common Patterns & Conventions

### Design Patterns Used

| Pattern | Location | Purpose | Benefit |
|---------|----------|---------|---------|
| **State Pattern** | UnitSystem/States/ | Behavior management | Clean state transitions, easy to extend |
| **Template Method** | UnitStateBase | Common logic in base | Reduce duplication, enforce consistency |
| **Factory Pattern** | UnitFactory, UnitStateFactory | Object creation | Different configurations per type |
| **Modifier/Chain of Responsibility** | Hit/Heal Modifiers | Extensible effects | Add buffs without modifying core |
| **Component Pattern** | Attacker/Defender/Mover | Separation of concerns | Single responsibility, testable |
| **Registry Pattern** | RogueRegistry | Extensible mapping | Easy to add new categories |
| **Singleton Pattern** | Managers | Global access | Single source of truth |
| **Object Pooling** | UnitFactory | Performance | Reduce GC pressure |
| **Dirty Flag** | CalcSystem, Gacha | Lazy evaluation | Avoid redundant computation |

### Save/Load System

**Pattern:** Interface-based persistence (`HaveSave`, `HaveLoad`)

**Implementation:**
- **Interfaces:** `HaveSave`, `HaveLoad` in `Base/`
- **Serialization:** Newtonsoft.Json (`JsonConvert`)
- **Security:** AES encryption via `FileIO.cs` (production-ready consideration)
- **Optimization:** Debouncing to reduce I/O (CurrencyManager example)

**Design Intent:** Demonstrates production concerns (security, performance, standardization)

### Event-Driven Architecture

Heavy use of C# `Action` delegates for loose coupling:
- State changes: `onStateChanged`
- Currency updates: `onCurrencyChanged`
- HP changes: `onCurrentHpChanged`
- Combat events: `onDeath`, `onDamaged`

**Design Intent:** Decoupled systems, easy to extend with new listeners

### Component-Based Design

Unity's component model heavily utilized:
- `Unit` = MonoBehaviour + `Attacker` + `Defender` + `Mover`
- Heavy use of `GetComponent<T>()` - ensure components exist
- Favor composition over inheritance (C# single inheritance limitation)

**Design Intent:** Flexible, testable, adheres to Unity conventions

### Editor Tooling (Production Workflow)

| Tool | Location | Purpose |
|------|----------|---------|
| **EditorButton Attribute** | Utils/Attributes/EditorButton/ | One-click inspector actions with parameters |
| **CurrencyDatabase Window** | CurrencySystem/Editor/ | Designer-friendly GUI for managing currencies |
| **LocalizationFontCharExtractor** | Utils/Localization/Editor/ | Extract exact glyphs used (mobile font optimization) |
| **CSV Reader** | Utils/ | Data-driven workflow for designers |

**Design Intent:** Demonstrates understanding of production pipeline and non-programmer workflows

### Coding Style & Conventions

1. **Namespaces:** Systems in namespaces (UnitSystem, CurrencySystem, etc.) for organization
2. **Readonly Properties:** `public IReadOnlyList<T>` for safe collection exposure
3. **SerializeField:** Private fields with `[SerializeField]` (Unity best practice)
4. **#if UNITY_EDITOR:** Guard editor-only code to prevent build bloat
5. **Enums:** Preferred for serialization, network sync, inspector exposure
6. **Events:** Prefer events over direct coupling
7. **Interfaces:** Used for contracts (`IUnitState`, `IHitModifier`, `HaveSave`)

---

## Working with This Portfolio Codebase

### Adding New Features

**ALWAYS consider:**
1. **What pattern does this demonstrate?** (State, Factory, Modifier, etc.)
2. **How does this show extensibility?** (Can new features be added without modifying existing code?)
3. **What's the design intent?** (Why this approach over alternatives?)
4. **How does this reflect production quality?** (Performance, maintainability, security)

### Extending UnitSystem

**Example: Adding a new state (Stun)**
```csharp
// 1. Create state class
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
        // Disable movement and attacks
    }
}

// 2. Add to enum (UnitState.cs)
public enum UnitState { Idle, Attack, Move, Death, Stun }

// 3. Register in Factory (UnitStateFactory.cs)
private static IEnumerable<IUnitState> CreateDefaultStates()
{
    return new IUnitState[]
    {
        new IdleState(),
        new AttackState(),
        new MoveState(),
        new StunState(), // Added
        new DeathState()
    };
}
```

**Example: Adding damage modifier**
```csharp
public class CriticalHitBuff : IHitModifier
{
    public string Name => "Critical Hit";
    public DamagePhase Phase => DamagePhase.PreHit;
    public int Priority => 100;

    private float critChance = 0.3f;
    private float critMultiplier = 2.0f;

    public Hit Apply(Hit hit)
    {
        if (Random.value < critChance)
        {
            hit.finalDamage *= critMultiplier;
            hit.postCallbacks.Add(() => Debug.Log("CRITICAL!"));
        }
        return hit;
    }
}

// Usage
unit.Attacker.AddAttackModifier(new CriticalHitBuff());
```

### Extending RoguelikeSystem

```csharp
// 1. Add to enum
public enum RogueEffectCategory { DamageBoost, HealBoost, CriticalRate }

// 2. Register handler
RogueRegistry.effectMap[RogueEffectCategory.CriticalRate] = (args) =>
{
    float rate = args.GetFloat("rate");
    // Apply critical rate buff
};
```

### Working with Gacha

```csharp
var gacha = new Gacha<UnitInfo>();
gacha.AddPair(new GachaPair<UnitInfo>(commonUnit, weight: 70));
gacha.AddPair(new GachaPair<UnitInfo>(rareUnit, weight: 30));

UnitInfo result = gacha.GetRandom();
List<UnitInfo> multiDraw = gacha.GetRandomMultiple(10, allowDuplicates: false);
```

### Testing Approach

**No automated unit tests** - Portfolio demonstrates system design, not test coverage.

**Testing methods:**
- Sample scenes (`**/Sample/` folders)
- Editor buttons with `[EditorButton]`
- Context menus with `[ContextMenu]`
- Manual play testing

---

## Critical Files for Understanding Architecture

| File | Purpose |
|------|---------|
| `Assets/Scripts/UnitSystem/README.md` | **Gold standard documentation** - pattern explanations, rationale, examples |
| `Base/HaveSave.cs` / `Base/HaveLoad.cs` | Save/load interface contract |
| `Base/EffectArgs.cs` | Type-safe effect argument parser |
| `Utils/DataManager.cs` | Central save/load with encryption |
| `UnitSystem/Core/Unit.cs` | Main unit entity demonstrating component composition |
| `UnitSystem/States/UnitStateMachine.cs` | State management with validation |
| `UnitSystem/States/UnitStateBase.cs` | Template Method Pattern implementation |
| `UnitSystem/Modifiers/IHitModifier.cs` | Modifier interface + usage example |

---

## Performance Considerations (Production-Ready)

| Optimization | Location | Benefit |
|--------------|----------|---------|
| **Object Pooling** | UnitFactory | Minimize GC for frequent spawning |
| **Dirty Flags** | CalcSystem, Gacha | Lazy evaluation, avoid redundant computation |
| **Debouncing** | CurrencyManager | Reduce I/O (burst transactions → single save) |
| **Cached TotalWeight** | Gacha | Avoid recalculation on every draw |
| **Event System** | All managers | Loose coupling, no unnecessary references |
| **Component-Based** | UnitSystem | Only include needed functionality per unit |

---

## Git Workflow

**Main branch:** Used for development (no separate dev branch)

**Recent commits demonstrate iterative development:**
- `feature: unit system with state pattern and modifiers`
- `feature: unitsystem factory pattern`
- `feature: unit system basic`

---

## Additional Context

- **Unity Addressables:** Asset management system, build reports in `Library/com.unity.addressables/BuildReports/`
- **VInspector:** Enhanced inspector (uses `[Button]` attribute)
- **Bilingual Documentation:** Korean comments may exist - portfolio targets both Korean and international companies
- **Modularity:** Each system is independent and reusable across projects

---

## For AI Agents: Portfolio-Specific Guidelines

1. **Code Quality First** - This is not a shipped game, it's a demonstration of skills
2. **Pattern Clarity** - Make design patterns obvious and well-implemented
3. **Documentation Matters** - READMEs should teach, not just describe
4. **Show Trade-offs** - Include technical decision rationale (demonstrates engineering judgment)
5. **Production Concerns** - Consider performance, security, extensibility, maintainability
6. **Demonstrate Breadth** - Each system showcases different skills/patterns
7. **Real-World Readiness** - Code should reflect how you'd write in a professional team

**When in doubt:** Look at `Assets/Scripts/UnitSystem/README.md` as the documentation standard.
