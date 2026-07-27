# AI Collaboration Contract

The files in this directory provide a shared, versioned context for ChatGPT, Claude Code, and future agents.

## Files

1. [AI context](AI_CONTEXT.md): product, stack, status, and repository map.
2. [AI rules](AI_RULES.md): non-negotiable engineering and safety constraints.
3. [AI architecture](AI_ARCHITECTURE.md): layer ownership and allowed dependencies.
4. [AI workflow](AI_WORKFLOW.md): required process for analysis, implementation, validation, and handoff.

## Source-of-truth order

1. Current code and automated tests
2. Repository contracts and configuration
3. Maintained documentation under `docs/`
4. Current task requirements
5. Agent-specific chat context

When sources conflict, the agent must identify the conflict instead of silently guessing. Permanent project rules belong in this repository, not only inside a ChatGPT Project or Claude configuration.
