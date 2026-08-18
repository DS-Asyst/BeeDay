---
name: beeday-git-delivery
description: Execute BeeDay Git delivery only after the user explicitly approves a specific commit, push, or PR action. Also use for delivery reporting. Do not use this Skill to infer approval from implementation completion.
allowed-tools: Bash(git status *) Bash(git diff *) Bash(git add *) Bash(git commit *) Bash(git push *) Bash(git rev-parse *) Bash(gh pr create *) Bash(gh pr view *) Bash(gh pr status *)
---

# beeday Git Delivery

This Skill grants Class C delivery capability for the current turn. It does **not** create authorization by itself.

Use only when the current user instruction explicitly authorizes the specific delivery action, or when another active authorization mode in `CLAUDE.md` explicitly covers it.

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
