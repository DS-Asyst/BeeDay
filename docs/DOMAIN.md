# Domain

## Core Entities

- `User`
- `Character`
- `Habit`
- `RecurringTask`
- `Project`
- `Todo`
- `Wallet`
- `Transaction`
- `InventoryTag`
- `UserToken`
- `CharacterExperience`
- `ExperienceTransaction`

## Value Objects

The domain uses value objects for validated concepts such as email addresses, user names, character nicknames, activity titles, activity descriptions, project colors, experience rewards, and experience sources.

## Experience Domain

Experience progression is independent from the UI and from concrete activity types. Habits, tasks, todos, projects, reading, and future modules must not mutate character XP directly. They calculate or request a reward and pass it through the central experience model.

```text
Activity completed
       ↓
Experience reward calculated
       ↓
Character.AddExperience
       ↓
CharacterExperience updates total XP
       ↓
ExperienceCurve derives progression
       ↓
ExperienceTransaction records the source
```

`CharacterExperience.TotalExperience` is the only persisted progression value. The following values are derived and excluded from persistence:

- current level;
- experience earned inside the current level;
- experience required to advance the current level;
- experience remaining until the next level.

The progression formula is represented by the `IExperienceCurve` contract. The initial implementation is `LinearExperienceCurve`, where advancing from level `n` requires `BaseExperience × n` XP. With the default base value of `100`, cumulative thresholds are 0 XP for level 1, 100 XP for level 2, 300 XP for level 3, and 600 XP for level 4.

The default curve has no product-defined maximum level. Its effective range is constrained only by the persisted `long` XP total and the `int` level type. A configured `LinearExperienceCurve` may define an explicit maximum; at that terminal level, the advance cost is zero. This keeps the initial balancing replaceable without coupling character state or persistence to one permanent formula.

Initial balancing intentionally remains conservative:

- `BaseExperience = 100` preserves the current progression pace;
- the exponent-style progression proposed for later balancing is not hard-coded yet;
- cumulative thresholds are derived rather than persisted;
- binary search calculates a level from XP without iterating through every prior level;
- boundary tests cover zero XP, exact thresholds, invalid values, configured caps, and `long.MaxValue`.

Every accepted reward must be greater than zero and generates an `ExperienceTransaction` containing the amount, source type, optional source identifier, optional description, and UTC occurrence time. Negative XP and experience removal are intentionally outside Sprint 3.1.

## Rules

- Domain entities enforce their own invariants.
- Invalid state transitions raise domain exceptions.
- Domain events communicate relevant state changes without coupling entities to infrastructure.
- Persistence-specific serialization concerns are isolated from business behavior.
- Future modules should extend the domain only after their invariants and ownership boundaries are defined.
