# Architecture Overview

**Fonte da verdade:** este documento foi validado diretamente contra `BeeDay.slnx`,
`Directory.Build.props`, `Directory.Packages.props`, os 4 `.csproj` de `src/*` e uma checagem por
`grep` de namespaces cruzando camadas em `src/BeeDay.Domain` e `src/BeeDay.Application`. Nenhuma
afirmação deste documento vem de `docs/history/` ou de sprints anteriores sem reverificação.

## 1. Arquitetura geral

O BeeDay é uma aplicação Blazor Server (.NET 10) organizada em 4 projetos de `src/`, seguindo
Clean Architecture com uma única direção de dependência:

```text
BeeDay.Domain  ←  BeeDay.Application  ←  BeeDay.Infrastructure  ←  BeeDay.Web
```

Verificado: `BeeDay.Domain.csproj` não declara nenhum `ProjectReference` nem `PackageReference` —
é a única camada sem nenhuma dependência externa. `BeeDay.Web.csproj` é o único projeto que
referencia as outras três (`Application`, `Domain`, `Infrastructure`), confirmando seu papel de
composition root.

## 2. Objetivos arquiteturais

Verificados como princípios efetivamente seguidos no código (não aspiracionais):

- **Isolamento do Domain**: `src/BeeDay.Domain` não contém nenhum `using Microsoft.EntityFrameworkCore`
  nem `using Microsoft.AspNetCore` (confirmado por grep de todo o diretório — zero ocorrências).
- **Persistência única e obrigatória**: SQL Server via EF Core é o único provider de dados em
  execução; `SqlServerOptions.ConnectionString` (`src/BeeDay.Infrastructure/Configuration/SqlServerOptions.cs`)
  é validado no startup via `.ValidateOnStart()` — a aplicação falha ao iniciar sem uma connection
  string válida, nunca degrada silenciosamente.
- **Repositório por Aggregate Root**: 8 interfaces em `src/BeeDay.Application/Common/Contracts/`
  (`IUserRepository`, `IUserTokenRepository`, `IHabitRepository`, `IRecurringTaskRepository`,
  `IProjectRepository`, `IWalletRepository`, `IWalletTagRepository`, `ITransactionRepository`),
  cada uma implementada por um `Ef*Repository` correspondente em `BeeDay.Infrastructure`.
- **Composition root único**: apenas `BeeDay.Web/Program.cs` monta a árvore de DI completa, via
  `AddBeeDayApplication()` (`src/BeeDay.Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`)
  e `AddBeeDayInfrastructure(configuration)` (`src/BeeDay.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`).

## 3. Tecnologias (verificado em `Directory.Packages.props` e `Directory.Build.props`)

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | `net10.0` (`Directory.Build.props`) | Target framework de todos os projetos |
| Blazor Server | via `Microsoft.NET.Sdk.Web` (`BeeDay.Web.csproj`) | UI interativa server-side (`AddInteractiveServerComponents`) |
| Entity Framework Core | `10.0.9` (`Microsoft.EntityFrameworkCore.SqlServer`, `.Design`) | Acesso a dados, único em `BeeDay.Infrastructure` |
| SQL Server | via EF Core SqlServer provider | Persistência única de runtime |
| MediatR | `14.2.0` | Despacho de Commands/Queries em `BeeDay.Application` |
| FluentValidation | `12.1.1` | Validação de Requests, via `ValidationBehavior` no pipeline MediatR |
| xUnit v3 | `3.2.2` (+ `xunit.runner.visualstudio` `3.1.5`) | Framework de testes em todos os 5 projetos de teste |
| bUnit | `2.7.2` | Testes de componentes Blazor (`BeeDay.Web.Tests`) |
| Microsoft.Playwright | `1.61.0` | Testes E2E (`BeeDay.E2E.Tests`) |
| AngleSharp | `1.5.2` | Parsing de HTML server-renderizado em testes de integração |

`Directory.Build.props` aplica a todos os projetos: `Nullable=enable`, `ImplicitUsings=enable`,
`AnalysisLevel=latest`, `EnableNETAnalyzers=true`, `EnforceCodeStyleInBuild=true`,
`GenerateDocumentationFile=true`.

## 4. Princípios de design confirmados no código

- **CQRS leve via MediatR**: Commands (sem retorno, ex. `CreateHabitCommand : IRequest`) e Queries
  (com retorno, ex. `GetDashboardQuery`) são despachados por `ISender`, nunca chamados diretamente
  por um Handler de outro Handler.
- **Pipeline de comportamentos MediatR**, registrado em ordem fixa (`ApplicationServiceCollectionExtensions.cs`):
  `LoggingBehavior` → `PerformanceBehavior` → `ValidationBehavior` → `DomainEventBehavior`.
- **Read services separados de repositórios de escrita**: `IDashboardReadService` e
  `IWalletReadService` (`src/BeeDay.Application/Features/*/Contracts/`) existem só para leitura
  projetada, implementados por `EfDashboardReadService`/`EfWalletReadService` com `AsNoTracking()`.
- **Current User via abstração**: `ICurrentUserContext` (`src/BeeDay.Application/Common/Security/`)
  é implementado apenas em `BeeDay.Web` (`HttpCurrentUserContext`), nunca por Infrastructure —
  Application não conhece `HttpContext`.
- **Ownership obrigatório**: `CurrentUserGuard.RequireUserId(currentUser)` é o único ponto de
  extração de `UserId` autenticado usado pelos Handlers; não há caminho de handler que aceite um
  `UserId` vindo diretamente do Request para operações do usuário autenticado.

## 5. Organização de `src/`

```text
src/
├── BeeDay.Domain/          Entities, Value Objects, Domain Events, Experience (regras de XP)
├── BeeDay.Application/     Features (Commands/Queries/Handlers/Requests/Responses), Contracts
├── BeeDay.Infrastructure/  Persistence (EF Core/SQL Server), Security, Identity, Caching, Background, HealthChecks
└── BeeDay.Web/             Composition root, Components (Blazor), Services, HealthChecks
```

Ver [`02-solution-structure.md`](02-solution-structure.md) para a estrutura completa da solução e
[`03-clean-architecture.md`](03-clean-architecture.md) para a responsabilidade detalhada de cada
camada.

## 6. Como este documento foi validado

- `BeeDay.Domain.csproj`, `BeeDay.Application.csproj`, `BeeDay.Infrastructure.csproj`,
  `BeeDay.Web.csproj` — lidos integralmente.
- `grep -r "Microsoft.EntityFrameworkCore" src/BeeDay.Domain` → 0 ocorrências.
- `grep -r "Microsoft.AspNetCore" src/BeeDay.Domain` → 0 ocorrências.
- `grep -r "using Microsoft.EntityFrameworkCore" src/BeeDay.Application` → 0 ocorrências.
- `Directory.Packages.props` e `Directory.Build.props` — lidos integralmente.
