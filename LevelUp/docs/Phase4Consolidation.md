# Phase 4 Consolidation

## Purpose

This sprint closes Phase 4 without introducing Phase 5 features. It consolidates language, terminology, persistence boundaries, tests, and product documentation.

## Language policy

- Source code, namespaces, types, members, and technical identifiers remain in English.
- All player-facing text is written in Brazilian Portuguese.
- Domain exception messages that can reach the presentation layer are localized in Portuguese.

## Player terminology

| Code term | Player-facing term |
| --- | --- |
| `Project` | Projeto |
| `Milestone` | Capítulo |
| `Quest` | Missão |
| `BossEncounter` | Chefe |
| `Training` | Treinamento |
| `Character` | Personagem |
| `Gold` | Finanças |

## Product rules confirmed

- Projects represent long-term journeys.
- Chapters divide a project into ordered progress sections.
- Missions can be associated with a project and optionally with one of its chapters.
- A boss represents a final challenge in the project progression experience.
- Project and chapter rewards are recognition-based, such as experience and titles.
- The Finances module represents real money reserved by the player. It is not an in-game reward currency.
- Financial deposits, withdrawals, justifications, health consequences, and financial achievements remain future work.

## Technical changes

- Player-facing menus, cards, tables, prompts, validation messages, status labels, and error messages were localized.
- Status rendering was centralized in `DisplayText` for plain-text selectors.
- Existing styled status formatters remain responsible for Spectre.Console markup.
- The chapter UI no longer offers financial rewards.
- Existing persistence contracts and English code identifiers were preserved.

## Validation checklist

Run:

```bash
dotnet format LevelUp.slnx
dotnet build LevelUp.slnx
dotnet test LevelUp.Tests/LevelUp.Tests.csproj
dotnet run --project LevelUp/LevelUp.csproj
```

Manually validate every menu and workflow to ensure no English player-facing text remains.
