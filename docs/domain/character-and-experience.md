# Character and Experience

Character progression is represented by persisted total experience and experience transaction history, with level values derived from the configured curve.

## Core types

- `CharacterExperience`: authoritative total XP and transaction history;
- `IExperienceCurve`: curve abstraction;
- `LinearExperienceCurve`: current curve implementation;
- `ExperienceEntry`, `ExperienceReward`, `ExperienceSource`, and `ExperienceTransaction`: reward and audit data;
- `ExperienceRewardType` and `ExperienceSourceType`: semantic classifications.

## Rules

- Feature handlers must not write derived level fields directly.
- Level, current-level progress, required XP, and remaining XP are calculated by the curve.
- Automatic rewards are coordinated by the centralized Application experience service.
- Reward idempotency is based on source type, source identifier, and reward type recorded in history.
- Successful grants produce experience events; crossing a level boundary produces a level-up event.
- Activity attributes have no effect on progression.

## Character state

A Character has a validated nickname, selectable class, avatar/onboarding state, and experience. Presentation feedback for XP gain and level up belongs in Web; calculation and invariants belong in Domain/Application.
