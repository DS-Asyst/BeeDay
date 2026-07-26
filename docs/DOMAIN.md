# Domain

## Core entities

- `User`
- `Character`
- `Habit`
- `RecurringTask`
- `Todo`
- `Project`
- `Wallet`
- `Transaction`
- `InventoryTag`
- `UserToken`
- `CharacterExperience`
- `ExperienceTransaction`

## Value objects

Validated concepts include email addresses, user names, character nicknames, activity titles, activity descriptions, project colors, experience rewards, and experience sources.

## Daily domain

The Daily context manages habits, recurring tasks, todos, and projects. Entities enforce their own invariants, while Application handlers coordinate persistence and cross-feature effects.

## Inventory domain

The Inventory context owns wallets, transactions, transaction types, and inventory tags. Wallet balances are derived from transactions rather than independently edited totals. Transaction and tag mutations are validated through Domain and Application boundaries.

## Experience domain

Experience progression is centralized and independent from UI components.

```text
Supported activity transition
       ↓
Application requests automatic reward
       ↓
IExperienceRewardService calculates the amount
       ↓
Character.TryAddExperience
       ↓
CharacterExperience checks idempotency
       ↓
ExperienceEntry is persisted
       ↓
ExperienceCurve derives level and progress
```

### Persisted and derived state

Persisted:

- total experience;
- experience entry history.

Derived:

- current level;
- XP earned inside the current level;
- XP required to advance;
- XP remaining until the next level.

The default `LinearExperienceCurve` uses a base cost of 100 XP. Advancing from level `n` requires `100 × n` XP. Cumulative thresholds therefore begin at:

| Level | Total XP required |
| ---: | ---: |
| 1 | 0 |
| 2 | 100 |
| 3 | 300 |
| 4 | 600 |

The default curve has no product-defined maximum level. A configured curve may define one.

### Automatic reward pipeline

The current automatic completion rewards are centralized in `ExperienceRewardPolicy`:

| Source | Completion reward |
| --- | ---: |
| Positive habit | 1 XP |
| Recurring task | 5 XP |
| Todo | 7 XP |
| Project | 20 XP |

Automatic rewards require a source identifier. Task, todo, and project rewards use the idempotency key:

```text
(CharacterId, SourceType, SourceId, RewardType)
```

Habit rewards use a unique occurrence identifier so repeated legitimate actions remain rewardable. Successful grants publish `ExperienceGrantedDomainEvent` after persistence. When an entry crosses one or more level boundaries, the same pipeline also publishes one aggregate `CharacterLeveledUpDomainEvent`.

## Domain rules

- Entities enforce their own invariants.
- Invalid state transitions raise domain exceptions.
- Domain events communicate meaningful state changes without infrastructure coupling.
- Persistence serialization details must not redefine business behavior.
- New modules must define ownership and invariants before extending shared aggregates.


## Experience progression

All automatic XP rewards flow through `IExperienceRewardPolicy` and `IExperienceRewardService`.
The initial balance is centralized as: positive habit `1 XP`, task completion `5 XP`, todo completion `7 XP`, and project completion `20 XP`.

Every accepted reward creates an `ExperienceEntry` containing the character, typed source, source reference, amount, XP before/after, level before/after, and UTC grant time. Task, todo, and project rewards are idempotent by character, source type, and source reference. Positive habit actions use a unique occurrence reference so legitimate repeated actions remain rewardable.
### Level-up events and journal

A completed `ExperienceEntry` is the source of truth for level transitions. `CharacterLeveledUpDomainEvent` is published only when `LevelAfter` is greater than `LevelBefore`. The event records the previous level, new level, number of levels gained, reward amount, typed experience source, and the `ExperienceEntry` identifier that caused the transition.

Multiple crossed levels produce one aggregate event rather than one event per level. The JSON Event Journal persists a readable summary and the structured event payload. Journal writes are idempotent by `EventId`; level-up events are additionally idempotent by `ExperienceEntryId`, preventing duplicate audit records when the same transition is processed again.

## Experience progression

The complete reward, persistence, idempotency, level calculation, events, journal, and UI flow is documented in [`EXPERIENCE.md`](EXPERIENCE.md).
