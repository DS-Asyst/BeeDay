# Solution Structure

## Production projects

### `src/LevelUp.Domain`

Contains entities, value objects, enums, domain events, exceptions, and experience calculations. It has no external package or project dependency.

Important areas:

- `Entities/`: User, Character, Activity, Habit, RecurringTask, Todo, Project, Wallet, Transaction, InventoryTag, UserToken, and LevelUpData;
- `ValueObjects/`: validated names, email, titles, descriptions, nicknames, and project colors;
- `Experience/`: curves, entries, rewards, sources, transactions, and character experience;
- `Events/`: application-action, experience-granted, and character-level-up events.

### `src/LevelUp.Application`

Contains use cases and technical contracts. Feature directories cover Authentication, Characters, Dashboard, Habits, Identity, Inventory, Ordering, Projects, Tasks, Todos, and Users.

Shared areas contain auditing, caching, background work, events, experience rewards, messaging, identity, security, and validation behavior.

### `src/LevelUp.Infrastructure`

Implements persistence, security, email delivery, auditing, caching, queues, clocks, throttling, and health dependencies.

The JSON subsystem separates paths, serialization, reading, writing, atomic commit, storage locking, initialization, backup, and repository behavior.

### `src/LevelUp.Web`

Hosts ASP.NET Core and the Blazor UI. It contains:

- feature pages and components;
- shared Design System components;
- feature-scoped state and services;
- cookie authentication endpoints;
- diagnostics, middleware, configuration validation, and health responses;
- static assets, images, JavaScript, CSS, and pixel icons.

## Test projects

- `tests/LevelUp.Domain.Tests`
- `tests/LevelUp.Application.Tests`
- `tests/LevelUp.Infrastructure.Tests`
- `tests/LevelUp.Web.Tests`

Web tests use bUnit and AngleSharp. All projects use xUnit v3 and centrally managed package versions.

## Shared repository contracts

- `Directory.Build.props`: .NET 10, nullable reference types, analyzers, deterministic builds, and documentation generation;
- `Directory.Packages.props`: central package version management;
- `.editorconfig`: formatting and code-style policy;
- `.gitattributes`: line-ending and binary-file normalization;
- `.gitignore`: excludes build output, IDE state, runtime data, secrets, logs, backups, and test output.
