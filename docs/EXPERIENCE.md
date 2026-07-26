# Experience Pipeline

## Scope

The experience module is the single path used to grant XP, persist its audit history, calculate levels, publish progression events, update the Event Journal, and display Level Up feedback. Level Up currently has no attribute points, item rewards, skills, economic bonuses, or automatic attribute progression.

## Reward request

Application command handlers request XP through `IExperienceRewardService.Grant`. They provide the authenticated user, a typed `ExperienceSourceType`, the source identifier, `ExperienceRewardType.Completion`, a short description, and an optional UTC timestamp.

Task, To-Do, and Project handlers use the domain entity identifier as the source identifier. This makes completion rewards idempotent. Positive Habit registrations use a new occurrence identifier for every legitimate registration, allowing recurring rewards without weakening duplicate protection.

## Reward policy

`IExperienceRewardPolicy` is the only automatic XP balance contract. `ExperienceRewardPolicy` currently defines:

| Source | XP |
|---|---:|
| Positive Habit occurrence | 1 |
| Task completion | 5 |
| To-Do completion | 7 |
| Project completion | 20 |

`Reading`, `Manual`, and `System` do not have automatic rewards and are rejected by the policy. To add a new automatic source, add the enum member, define its value in `ExperienceRewardPolicy`, add service and handler tests, and document the balance change here.

## Experience entry and persistence

Every accepted grant creates one `ExperienceEntry`. It records the character, amount, typed source, source reference, reward type, XP before and after, level before and after, and grant time. `CharacterExperience.Entries` is the canonical API.

The JSON property remains named `Transactions` for backward compatibility with existing data files. This is a persistence compatibility detail, not the domain terminology.

The character and its entries are persisted together through the existing JSON repository and atomic file committer. On load, domain validation rejects negative XP, non-positive entries, overflow, and duplicate non-Habit reward keys.

## Level calculation

All level calculations go through `ExperienceCurve`, whose default implementation is `LinearExperienceCurve`. The initial balance uses 100 base XP and the cumulative formula:

```text
Total XP required for level L = BaseXP × (L - 1) × L / 2
```

The curve is independent of application, infrastructure, and UI code. Future balance changes should be made by replacing or configuring the curve, then updating boundary and regression tests. UI components must never calculate levels.

## Idempotency

For Task, To-Do, and Project rewards, the effective duplicate key is:

```text
CharacterId + SourceType + SourceReferenceId + RewardType
```

Repeating the same completion request returns no new entry. Habits remain recurring because each positive occurrence receives a unique occurrence identifier.

Domain events carry a unique `EventId`. The Event Journal rejects repeated `EventId` values and also rejects repeated Level Up records for the same `ExperienceEntryId`.

## Domain events

Every persisted entry publishes one `ExperienceGrantedDomainEvent`. When `LevelAfter` is greater than `LevelBefore`, the publisher also emits one `CharacterLeveledUpDomainEvent` containing the entire transition. Multiple levels gained are represented by one event, with `LevelsGained = NewLevel - PreviousLevel`.

No Level Up event is emitted when the level remains unchanged. Events have no dependency on Web components.

## Event Journal

`AuditDomainEventHandler` forwards domain events to `IEventJournal`. `JsonEventJournal` writes newline-delimited JSON envelopes containing event type, event identifier, UTC occurrence time, an optional readable summary, and the structured event payload.

The journal is append-only, protected by an in-process write lock, tolerant of malformed legacy lines, and idempotent. A Level Up record is traceable to its original `ExperienceEntryId` and typed source.

## UI feedback

`LevelUpFeedbackEventHandler` maps the already-calculated Level Up event into a visual model. `LevelUpFeedbackStore` deduplicates and consumes feedback. The modal displays previous level, new level, levels gained, XP amount, and source summary without recalculating rewards, XP, or levels.

The feedback uses the Design System, accessible dialog semantics, keyboard dismissal, focus management, responsive layout, and reduced-motion support. The in-memory consumed state prevents the same feedback from appearing again after it is closed or after a page refresh.

## Validation

The module is covered by domain, application, infrastructure, and bUnit tests for boundaries, overflow, typed sources, recurring Habits, idempotent completion rewards, multiple levels, event correlation, Event Journal deduplication, accessibility, and feedback consumption.

Run the final validation from the repository root:

```bash
git status
dotnet format --verify-no-changes
dotnet build
dotnet test
```
