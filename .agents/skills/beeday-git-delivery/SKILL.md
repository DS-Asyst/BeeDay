---
name: beeday-git-delivery
description: Execute BeeDay Git delivery only after the user explicitly approves a specific commit, push, or PR action. Also use for delivery reporting. Do not use this Skill to infer approval from implementation completion.
---

# BeeDay Git Delivery

This Skill grants Class C delivery capability for the current turn. It does **not** create authorization by itself.

Use only when the current user instruction explicitly authorizes the specific delivery action, or when another active authorization mode in `AGENTS.md` explicitly covers it.

## Before commit

Verify:

1. mandatory validation status is known;
2. final diff has been reviewed;
3. only intended files are staged;
4. no secret or generated noise is included;
5. commit message accurately describes the implementation.

Prefer explicit staging of intended paths when practical.

Do not use `git add .` blindly when the working tree contains unrelated files.

## Commit

After commit, report:

- commit SHA;
- commit message;
- included files/scope;
- current branch;
- working-tree status.

## Push

Normal push only.

This Skill does not authorize:

- `--force`;
- `--force-with-lease`;
- rebase;
- merge;
- history rewrite.

## Pull Request

Create a PR only when explicitly approved or covered by active Sprint/Epic autonomy.

PR description should include:

- objective;
- implementation summary;
- architecture impact;
- tests and validation;
- deployment/environment considerations;
- risks/follow-up.

Do not merge the PR. Merge is outside this Skill.

## Codex boundary behavior

The default BeeDay Codex posture keeps `.git` protected by the workspace sandbox. Git history or remote delivery can therefore require a sandbox escalation even when this Skill is active.

That escalation is a **technical boundary check**, not a new business-approval request. If Auto-review is enabled, it may approve the escalation only when the current task already authorizes the Class C action under `AGENTS.md`.

If the task does not authorize the action, do not request or attempt the escalation.
