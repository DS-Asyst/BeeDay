# BeeDay Codex Skills

This directory contains repository-scoped Agent Skills for OpenAI Codex.

Codex discovers repository Skills from `.agents/skills/`. Each Skill is intentionally focused on one repeatable engineering responsibility so the root `AGENTS.md` can remain the durable governance contract instead of becoming a procedural monolith.

## Skills

| Skill | Responsibility |
|---|---|
| `beeday-engineering` | Standard implementation workflow |
| `beeday-architecture` | Clean Architecture and contract review |
| `beeday-ui-ux` | Experience System, Design System, accessibility, localization, responsive UI |
| `beeday-quality` | Test planning and mandatory validation |
| `beeday-review` | Mandatory two-pass final review and verdict |
| `beeday-git-delivery` | Authorized commit, push, and PR delivery |
| `beeday-sprint` | Sprint orchestration and Sprint autonomy |
| `beeday-epic-autonomy` | Multi-Sprint autonomous Epic execution |
| `beeday-infrastructure` | CI/CD, IIS, PowerShell, deployment, and privileged operational work |

## Authorization rule

A Skill describes **how** to perform work. It never grants permission to perform a sensitive action.

Authorization comes from the current user instruction and the levels defined in root `AGENTS.md`.

Examples:

- loading `beeday-git-delivery` does not authorize a commit;
- `AUTONOMY: SPRINT` authorizes Class C delivery for that Sprint;
- `AUTONOMY: EPIC` authorizes Class C delivery for the defined Sprint sequence;
- merge, force push, history rewrite, and production mutation remain outside Sprint/Epic autonomy.

## Invocation

Codex can select a Skill implicitly from its description. In Codex CLI or the IDE extension, Skills can also be selected explicitly through the Skills UI/command or by mentioning the Skill.
