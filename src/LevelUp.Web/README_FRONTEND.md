# LevelUp.Web

Blazor frontend for the LevelUp dashboard.

## Project structure

```text
Components/
  Activities/   Task, To-Do and Project editor modal
  Dashboard/    Dashboard columns, cards and filters
  Habits/       Habit editor modal
  Layout/       Navigation, main layout, footer and reconnect UI
  Profile/      Profile summary panel
  Pages/        Routable pages
Data/           JSON persistence in LevelUpBD.json
Models/         Form and persistence models
Services/       JSON data store
wwwroot/css/    Global design tokens, utilities and animations
```

## Domain

The dashboard supports independent registration and management of:

- Habits
- Tasks
- To-Dos
- Projects

To-Dos do not depend on Projects. Habits support direction, difficulty and reset counter. XP, levels, attributes, Gold, Wallet, Books, Bosses and Milestones are not part of this frontend.

## Persistence

`JsonLevelUpRepository` uses `Data/LevelUpBD.json`, creates a backup before replacement, writes through a temporary file and returns cloned state objects to components. The current service is intended for a single shared local data source.

## Styling convention

- Global tokens and utilities belong in `wwwroot/css`.
- Page layout belongs in the page `.razor.css` file.
- Component-specific styles stay beside their `.razor` component.
- The footer was intentionally preserved for future link configuration.
