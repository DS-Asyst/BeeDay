# Domain Refactor — Schema 7

## Source of truth

This document records the model agreed for the next LevelUp iteration. The console UI is retained only as a temporary client. The domain and persistence model are designed independently from Blazor.

## Ubiquitous language

- **Habit** replaces Training. A habit may be positive, negative, or both. It has no duration in minutes.
- **Task** is an independent recurring activity. It may repeat daily or weekly; weekly tasks select one or more weekdays.
- **To-do** belongs to a Project, optionally to a Milestone. It is one-time, can only be completed once, and is not archived. It can only be created through a Project workflow.
- **Project** and **Milestone** keep their existing purpose.

## Compatibility

Schema 7 migrates legacy Quests: independent quests become Tasks and project-linked quests become To-dos. The legacy Quest model remains temporarily for backward compatibility and should not receive new features.

## Front-end direction

The future Blazor UI may use Habitica as interaction inspiration, but LevelUp keeps its own code, assets, visual identity, rules, and terminology.


## Console compatibility boundary

`GameData.Quests` has been removed. Existing Console quest workflows are temporarily serialized through `GameData.LegacyQuests` while the Console UI is migrated to Tasks and Project To-dos. New features must not depend on this compatibility collection. No Blazor work is included in this change.
