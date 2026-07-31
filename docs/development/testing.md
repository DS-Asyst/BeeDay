# Testing

LevelUp has four test projects aligned with the production layers.

## Domain tests

Validate entities, value objects, invariants, activity behavior, project rules, wallet aggregates, experience calculations, and domain events.

## Application tests

Validate commands, handlers, validators, use-case orchestration, multi-user isolation, authentication, identity, wallet, dashboard behavior, and experience reward idempotency.

## Infrastructure tests

Validate JSON persistence, backups and recovery behavior, password hashing, identity infrastructure, and the event journal.

## Web tests

Use bUnit and AngleSharp to validate components, routes, services, accessibility contracts, Design System behavior, icons, visual states, dashboard behavior, authentication UI, wallet UI, and feature interactions.

## Commands

```bash
dotnet test LevelUp.slnx
```

Release gate:

```bash
dotnet build LevelUp.slnx --configuration Release --warnaserror
dotnet test LevelUp.slnx --configuration Release
```

## Test policy

- Add regression coverage for every corrected defect when practical.
- Test public behavior and contracts rather than private implementation details.
- Keep tests deterministic and isolated from production data.
- Do not weaken an assertion merely to make a failing change pass.
- A changed UI contract should include accessibility and state coverage where relevant.
