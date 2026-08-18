---
name: beeday-architecture
description: BeeDay Clean Architecture and contract analysis. Use before changes that affect dependency direction, project boundaries, public contracts, persistence architecture, cross-layer flows, new services/abstractions, or architectural decisions.
---

# BeeDay Architecture Review

Use this Skill before implementing architecture-sensitive changes and again during final review.

## 1. Determine ownership

For each new or changed responsibility, identify its correct owner:

| Concern | Owner |
|---|---|
| Business rules, invariants, aggregates, value objects | Domain |
| Use-case orchestration, commands, queries, validation, application contracts | Application |
| Persistence/external/provider implementation | Infrastructure |
| Presentation, Blazor/Razor, HTTP, host composition, Design System usage | Web |

If responsibility cannot be placed cleanly, investigate existing architecture before creating a new pattern.

## 2. Dependency-direction gate

Reject designs that introduce unintended dependency direction, including:

- Domain → Infrastructure;
- Domain → Web;
- Application → Infrastructure implementation;
- Application → UI framework;
- Web business rules that belong in Domain/Application;
- Infrastructure business rules that belong in Domain/Application.

## 3. Contract gate

Before changing a public or cross-layer contract:

1. identify all consumers;
2. identify serialization/persistence/API/test impact;
3. determine backward compatibility;
4. prefer additive evolution where safe;
5. obtain explicit approval before a breaking change.

Do not create a replacement contract while silently leaving the original path active unless coexistence is part of an approved migration strategy.

## 4. Duplication and abstraction gate

Before adding a new service, repository, component, or abstraction, search the repository for an existing equivalent.

Reject:

- parallel implementations;
- generic abstractions with no demonstrated need;
- architecture introduced only to satisfy one narrow code path;
- wrappers that add indirection without ownership or testability value.

## 5. Persistence gate

When persistence is involved:

- keep provider-specific types inside Infrastructure;
- keep Domain/Application contracts provider-neutral unless repository architecture explicitly says otherwise;
- preserve transactional consistency and ownership boundaries;
- do not introduce silent fallback, dual-write, or split-brain provider behavior without an approved migration design;
- treat migrations as versioned contracts, not disposable generated files.

## 6. Architecture report

For architecture-sensitive work, report:

- affected layers;
- dependency-direction impact;
- public-contract impact;
- new abstractions, if any, and why existing ones were insufficient;
- compatibility impact;
- architectural debt discovered but intentionally left outside scope.
