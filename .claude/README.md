# BeeDay Claude Code Configuration

This directory contains project-level Claude Code configuration for BeeDay.

## Files

- `settings.json` — shared repository permissions and deterministic permission prompts.
- `settings.local.json` — local session behavior only. Keep machine-specific settings here.
- `skills/` — BeeDay operating procedures loaded when relevant.

## Why Skills are separate from CLAUDE.md

`CLAUDE.md` is always present in Claude's project context, so it should contain permanent governance and invariants.

Skills contain specialized procedures. Their descriptions can be discovered at session start, while the full Skill body is loaded only when invoked. This keeps the permanent context smaller and makes each workflow easier to maintain.

## Skill map

| Skill | Primary responsibility |
|---|---|
| `beeday-engineering` | implementation workflow |
| `beeday-architecture` | Clean Architecture and contract review |
| `beeday-ui-ux` | Experience System, Design System, accessibility, responsive UI |
| `beeday-quality` | tests and mandatory validation |
| `beeday-review` | two-pass final review and quality verdict |
| `beeday-git-delivery` | normal explicitly approved Git delivery |
| `beeday-sprint` | single Sprint execution and Sprint autonomy |
| `beeday-epic-autonomy` | autonomous multi-Sprint Epic execution |
| `beeday-infrastructure` | CI/CD, IIS, PowerShell, deployment and privileged boundaries |

## Permission philosophy

Permissions define what Claude Code can technically execute without or with a tool prompt. They do not replace the authorization model in `CLAUDE.md`.

The shared settings intentionally:

- allow normal read-only Git inspection and .NET validation;
- require a permission prompt for destructive/integration/history-rewriting Git operations;
- avoid globally pre-approving commit, push, and PR creation;
- let the delivery/Sprint/Epic Skills grant the Class C tools only when the relevant authorized workflow is active.

This separates **capability** from **authorization**.
