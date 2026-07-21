# Validação da Sprint 5.2

## Verificações estruturais

- `DashboardState` e `ProfileState` registrados como serviços `Scoped`.
- Flags de editores e estado de exclusão removidos da página do dashboard.
- Estado do fluxo de perfil removido da página de perfil.
- `DashboardModalState` encapsulado por `DashboardState`.
- `LevelUpWebService` preservado como fronteira da apresentação.
- Uma única rota de dashboard e uma única rota de perfil.
- Nenhuma diretiva `@import` em CSS scoped.
- Apenas um projeto `LevelUp.Web` na solução.

## Comandos de validação

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
