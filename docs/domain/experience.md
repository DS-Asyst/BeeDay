# Experience

Progression is represented by persisted total experience and experience transaction history on `User`, with level values derived from the configured curve. This is the application's only gamification mechanic: a light, non-competitive incentive layer, not an RPG progression system.

## Core types

- `UserExperience`: authoritative total XP and transaction history, owned by `User`;
- `IExperienceCurve`: curve abstraction;
- `LinearExperienceCurve`: current curve implementation;
- `ExperienceEntry`, `ExperienceReward`, `ExperienceSource`, and `ExperienceTransaction`: reward and audit data;
- `ExperienceRewardType` and `ExperienceSourceType`: semantic classifications.

## Rules

- Feature handlers must not write derived level fields directly.
- Level, current-level progress, required XP, and remaining XP are calculated by the curve.
- Automatic rewards are coordinated by the centralized Application experience service.
- Reward idempotency is based on source type, source identifier, and reward type recorded in history.
- Successful grants produce experience events; crossing a level boundary produces a level-up event (`UserLeveledUpDomainEvent`).
- Activity attributes have no effect on progression.
- Experience does not depend on, or require, a completed profile — it accrues on `User` from account creation.

## Presentation

Presentation feedback for XP gain and level up belongs in Web; calculation and invariants belong in Domain/Application.
