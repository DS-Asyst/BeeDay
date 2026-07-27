# Development

## Documents

- [Getting started](getting-started.md)
- [Coding standards](coding-standards.md)
- [Testing](testing.md)
- [Git and delivery workflow](workflow.md)
- [Configuration and secrets](configuration.md)
- [CI/CD and IIS deployment](ci-cd.md)

## Standard local cycle

```bash
git status
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Use the smallest command set appropriate during implementation, but complete the mandatory quality gate before declaring work finished.
