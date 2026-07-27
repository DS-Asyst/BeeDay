# AI Rules — LevelUp

## Non-negotiable rules

1. Inspect the relevant code, tests, configuration, and documentation before changing anything.
2. Keep changes within the requested scope.
3. Preserve existing behavior unless the task explicitly changes it.
4. Respect layer ownership and dependency direction.
5. Do not invent APIs, files, test results, runtime behavior, or validation success.
6. Add or update tests for behavior changes.
7. Update documentation when a public contract, architecture rule, workflow, configuration requirement, or domain behavior changes.
8. Reuse existing abstractions and Design System components before adding new ones.
9. Avoid new dependencies unless the benefit is clear and approved.
10. Treat `.editorconfig`, `.gitattributes`, build props, and central package management as mandatory contracts.

## Security and data

- Never expose or commit secrets.
- Never log plaintext passwords, hashes, identity tokens, API keys, or protected values.
- Preserve user isolation and ownership checks.
- Do not delete or overwrite runtime data, backups, or production directories.
- Do not weaken antiforgery, authentication, authorization, HTTPS, host validation, or secure cookie behavior.

## Git and destructive operations

Require explicit approval before:

- mass deletion or broad generated rewrites;
- force-push, history rewrite, reset, rebase, merge, or branch deletion;
- commit, push, release, publish, or deployment;
- destructive data-reset or restore operations.

## Completion standard

Run:

```bash
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
git status
```

Report the exact outcome. When a command cannot run, state why. Never present an unexecuted command as validated.
