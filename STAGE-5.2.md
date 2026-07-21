# Sprint 5.2 — Gerenciamento de estado do frontend

## Objetivo

Retirar das páginas o estado e a orquestração de interface, preservando o comportamento e a apresentação existentes.

## Entregue

- `DashboardState` centraliza dados, busca, filtros, contadores e operações do dashboard.
- `DashboardModalState` centraliza visibilidade dos editores, item em edição, modelos e confirmação de exclusão.
- `ProfileState` centraliza criação de perfil e estado derivado de validação.
- `Home.razor.cs` ficou responsável pelo ciclo de vida e redirecionamento.
- `Profile.razor.cs` ficou responsável pelo ciclo de vida, redirecionamento e navegação pós-gravação.
- Estados registrados como `Scoped`, de acordo com o ciclo do Blazor Server.
- `LevelUpWebService` continua sendo a fronteira entre a interface e os casos de uso.
- Markup, rotas e CSS existentes preservados.

## Fluxo

```text
Home / Profile
      |
      v
DashboardState / ProfileState
      |
      v
LevelUpWebService
      |
      v
Application Features
      |
      v
Infrastructure JSON
```

`DashboardModalState` pertence ao `DashboardState` e não é registrado separadamente.

## Validação

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
