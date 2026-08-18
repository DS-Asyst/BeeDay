---
name: beeday-engineering
description: BeeDay implementation workflow for features, fixes, refactors, and normal Domain, Application, Infrastructure, or Web changes. Use whenever repository code is being changed unless a more specialized Skill completely covers the task.
---

# BeeDay Engineering Workflow

Follow `AGENTS.md` first. This Skill defines the standard implementation procedure.

## 1. Establish scope

Before editing:

1. Restate the requested behavior internally as acceptance criteria.
2. Identify affected feature flows and architectural layers.
3. Read the relevant `docs/` sections and tests.
4. Search for existing implementations before designing a new one.
5. Identify compatibility and documentation impact.

Do not begin with a rewrite proposal when a focused extension or refactor can satisfy the requirement.

## 2. Repository reconnaissance

Search for:

- existing entities/value objects/domain rules;
- commands, queries, handlers, validators, and contracts;
- provider implementations;
- Razor/Blazor components and Design System primitives;
- DI registrations;
- configuration keys;
- tests at all relevant levels;
- feature documentation;
- scripts/workflows if the flow crosses an operational boundary.

Build a mental dependency map before editing.

## 3. Design the smallest safe change

The implementation should:

- preserve dependency direction;
- preserve existing public contracts unless explicitly changed;
- reuse existing abstractions and components;
- avoid duplicated behavior;
- preserve user/ownership boundaries;
- maintain deterministic behavior and error handling;
- remain testable without leaking implementation details into contracts.

If a new abstraction is required, be able to explain why extending an existing one is insufficient.

## 4. Implement incrementally

Prefer small cohesive edits.

After each meaningful slice:

- compile or run focused tests when useful;
- check for obvious contract drift;
- keep the working tree understandable.

Do not use temporary production shortcuts that will need a cleanup Sprint to become correct.

## 5. Test behavior

Add or update tests based on changed behavior, not merely changed files.

A bug fix should normally introduce a regression test that fails for the old behavior and passes for the corrected behavior.

Do not weaken assertions, remove legitimate coverage, or replace realistic provider tests with weaker substitutes simply to make the suite pass.

## 6. Synchronize documentation

Update maintained documentation when the change affects:

- architecture;
- public behavior;
- configuration;
- persistence;
- security;
- deployment;
- workflow;
- Experience System/Design System contracts.

Do not rewrite historical ADRs as if the current state had always existed.

## 7. Mandatory handoff

Before completion, invoke/use the BeeDay quality and review procedures.

A feature is not done merely because it compiles.
