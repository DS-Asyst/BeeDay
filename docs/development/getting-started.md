# Getting Started

## Prerequisites

- .NET 10 SDK
- Git
- Visual Studio 2022 with ASP.NET and web development workload, or another compatible editor
- PowerShell 7 for repository scripts

## Clone and validate

```bash
git clone <repository-url>
cd LevelUp
dotnet restore LevelUp.slnx
dotnet build LevelUp.slnx
dotnet test LevelUp.slnx
```

## Run the application

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

The application uses interactive server rendering. Development email delivery can write generated messages to the configured local email directory when the development sender is enabled.

## Local data

The default development storage area is under `src/LevelUp.Web/Data`. The directory placeholder is versioned, but runtime JSON, backups, and local files are ignored.

Use `scripts/Reset-TestData.ps1` only when you understand which local data it will remove. Operational scripts should be reviewed before execution.
