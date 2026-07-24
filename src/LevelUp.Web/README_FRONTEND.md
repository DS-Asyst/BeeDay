# LevelUp.Web

Aplicação Blazor Server e camada de apresentação do LevelUp.

## Organização

O frontend é organizado por feature em `Components/Features`:

- `Dashboard`: página, componentes e estados do painel.
- `Habits`, `Tasks`, `Todos`, `Projects`: editores e modelos de formulário.
- `CharacterCreation`: page, model and state for character creation.
- `Common`: tipos compartilhados pela apresentação.
- `Layout`: estrutura global da aplicação.
- `Shared`: componentes reutilizáveis entre features.

## Estado

- `DashboardState`: dados e operações do dashboard.
- `DashboardModalState`: editores e exclusão.
- `CharacterCreationState`: fluxo de criação de perfil.

Os estados principais são registrados como `Scoped` em `Program.cs`.

## Integração

A interface chama a camada Application somente por `LevelUpWebService`. A persistência JSON é responsabilidade exclusiva de `LevelUp.Infrastructure`.

## Execução

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
