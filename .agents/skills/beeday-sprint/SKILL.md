---
name: beeday-sprint
description: "Execute a BeeDay Sprint. Use whenever the user starts a Sprint. If the user explicitly grants AUTONOMY: SPRINT or equivalent autonomous commit/push/PR authority, execute the complete Sprint through PR creation without separate Class C approvals."
---

# BeeDay Sprint Execution

Follow `AGENTS.md` and all applicable specialized Skills.

## 1. Identify authorization

Determine whether the Sprint is:

- standard implementation; or
- explicitly `AUTONOMY: SPRINT`.

Do not infer Sprint autonomy from the fact that a previous Sprint was autonomous.

## 2. Establish Sprint contract

Identify:

- Sprint number/name;
- objective;
- acceptance criteria;
- explicit out-of-scope items;
- expected architecture areas;
- required tests;
- required documentation;
- approved branch base.

If the Sprint belongs to an autonomous chained Epic, branch-base rules come from the Epic Skill.

## 3. Branch startup

Before editing:

1. inspect current branch;
2. inspect `git status`;
3. classify staged/unstaged/untracked work;
4. verify branch creation is safe;
5. establish the approved base;
6. synchronize the base safely when appropriate;
7. create a dedicated `sprint/<number>-<description>` branch;
8. confirm the created branch.

For a standalone Sprint, default base is `hmg` unless repository documentation or the task explicitly defines another base.

Never silently discard pre-existing work.

## 4. Implement

Use the engineering Skill and any architecture/UI/infrastructure Skills required by scope.

Implement only the Sprint contract. Do not opportunistically start the next Sprint.

## 5. Review and validation

Before delivery:

- run the BeeDay quality gate;
- perform the BeeDay two-pass review;
- resolve all BLOCKER findings;
- ensure documentation is synchronized;
- inspect final `git status` and diff.

## 6. Delivery behavior

### Standard Sprint

Stop after validated implementation and request commit approval as defined by `AGENTS.md`.

### AUTONOMY: SPRINT

If and only if explicit Sprint autonomy is active:

1. stage intended files;
2. commit with a concise accurate message;
3. push the Sprint branch;
4. create the PR against the repository-approved target, normally `hmg`;
5. report branch, SHA, PR, quality verdict, and validation evidence.

Do not merge.

## 7. Stop conditions

Autonomous Sprint execution stops only for the explicit stop conditions in `AGENTS.md`, such as destructive/history/production requirements, unresolved contract conflicts, missing authority, or unsafe unrelated local work.

## 8. Codex approval handling

Repository `.codex` policy may route protected `.git` writes or network-bound delivery through Auto-review.

For a standard Sprint, do not use an escalation to bypass the missing user approval for commit/push/PR.

For `AUTONOMY: SPRINT`, the user has already granted the Class C delivery authority defined by `AGENTS.md`; eligible sandbox escalations may therefore be reviewed automatically. Class D and Class E actions remain outside the authorization.
