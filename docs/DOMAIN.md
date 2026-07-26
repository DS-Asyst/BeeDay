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
ExperienceTransaction is persisted
       ↓
ExperienceCurve derives level and progress
```

### Persisted and derived state

Persisted:

- total experience;
- experience transaction history.

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

The current automatic completion rewards are centralized in `ExperienceRewardService`:

| Source | Completion reward |
| --- | ---: |
| Habit | 10 XP |
| Recurring task | 20 XP |
| Todo | 25 XP |
| Project | 50 XP |
| Reading | 10 XP |

Reading is reserved for the future Library module.

Automatic rewards require a source identifier. The idempotency key is:

```text
(SourceType, SourceId, RewardType)
```

Repeating the same completion command cannot grant the same reward twice. Successful grants publish `ExperienceGrantedDomainEvent` after persistence.

Reward values are balancing configuration embedded in the current service implementation and may change in a dedicated product-balancing sprint.

## Domain rules

- Entities enforce their own invariants.
- Invalid state transitions raise domain exceptions.
- Domain events communicate meaningful state changes without infrastructure coupling.
- Persistence serialization details must not redefine business behavior.
- New modules must define ownership and invariants before extending shared aggregates.
