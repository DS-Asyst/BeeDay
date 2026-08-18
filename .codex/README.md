# BeeDay Codex Project Configuration

This directory contains project-scoped Codex execution policy.

It is deliberately separate from `AGENTS.md`:

- `AGENTS.md` defines BeeDay engineering governance and task authorization;
- `.agents/skills/` defines reusable engineering procedures;
- `.codex/config.toml` defines the default technical sandbox and approval posture;
- `.codex/rules/*.rules` defines command-level escalation gates.

## Default posture

The project configuration uses:

```toml
approval_policy = "on-request"
approvals_reviewer = "auto_review"
default_permissions = ":workspace"
```

This intentionally keeps Codex inside the workspace sandbox rather than enabling unrestricted full access.

Codex protects `.git`, `.agents`, and `.codex` inside the normal workspace sandbox. Operations such as commits can therefore require boundary escalation. Auto-review can evaluate eligible escalation requests without interrupting an autonomous Sprint/Epic, but only the current task instruction determines whether the operation is actually authorized.

## Why Auto-review is used

BeeDay needs two properties at the same time:

1. autonomous Sprint/Epic delivery when explicitly requested;
2. a persistent sandbox boundary that prevents autonomy from silently becoming unrestricted machine access.

Auto-review provides that middle layer. It reviews an escalation; it does not expand the sandbox or create business authorization.

## Rules

`rules/beeday.rules` places explicit review gates around Git history/remote mutation and high-risk Git operations.

Normal commit/push/PR creation can proceed when the current task grants Class C authority. Merge, rebase, reset, clean, branch deletion, tags, force-push behavior, and similar Class D operations still require explicit operation-specific authority under `AGENTS.md`.

## Project trust

Codex only loads project-scoped `.codex/` configuration and rules when the repository is trusted by the local Codex client.

After installing these files, restart/reopen the Codex session and inspect the effective configuration with the Codex status/permissions controls before relying on the policy.

## Do not use broad bypass as a workflow

Do not normalize `danger-full-access`, `--yolo`, or equivalent sandbox/approval bypasses for BeeDay development. If a legitimate task requires broader access, authorize and scope that exception explicitly rather than weakening the permanent repository posture.
