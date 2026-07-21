# LevelUp

Aplicação Blazor Server em .NET 10 para gerenciamento de hábitos, tarefas recorrentes, afazeres e projetos, com persistência local em JSON.

## Estrutura da solução

```text
LevelUp/
├── docs/
├── roadmap/
├── src/
│   ├── LevelUp.Domain/
│   ├── LevelUp.Application/
│   ├── LevelUp.Infrastructure/
│   └── LevelUp.Web/
├── tests/
│   ├── LevelUp.Domain.Tests/
│   ├── LevelUp.Application.Tests/
│   └── LevelUp.Infrastructure.Tests/
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
└── LevelUp.slnx
```

Existe apenas um projeto web ativo: `src/LevelUp.Web`.

## Responsabilidades

- **LevelUp.Domain**: entidades, value objects, enums, invariantes e validações de domínio.
- **LevelUp.Application**: casos de uso, contratos, requests e responses organizados por feature.
- **LevelUp.Infrastructure**: persistência JSON, backups, recuperação, configuração e health checks.
- **LevelUp.Web**: interface Blazor, componentes, estados de tela e adaptação para os casos de uso.
- **tests**: testes automatizados separados por camada.

## Frontend

O frontend está organizado por feature em `src/LevelUp.Web/Components/Features`.

- `Dashboard`: página principal, componentes e estado do painel.
- `Habits`, `Tasks`, `Todos`, `Projects`: editores e modelos específicos.
- `Profile`: criação e estado do perfil.
- `Common`: tipos compartilhados apenas pela apresentação.
- `Layout` e `Shared`: estrutura global e componentes reutilizáveis.

Os estados `DashboardState` e `ProfileState` são registrados como `Scoped`, acompanhando o circuito do Blazor Server. O frontend acessa os casos de uso por meio de `LevelUpWebService`.

## Persistência

O arquivo principal é:

```text
src/LevelUp.Web/Data/LevelUpBD.json
```

A infraestrutura realiza gravação atômica, backups rotativos, recuperação de arquivos válidos e validação do estado de domínio.

## Execução

No diretório raiz:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Endereços definidos no perfil local:

```text
https://localhost:7245
http://localhost:5059
```

O endpoint `GET /health` valida a aplicação e o armazenamento JSON.

## Estado atual

- Etapas 1 a 4 concluídas.
- Sprint 5.1 concluída: fundação e organização do frontend.
- Sprint 5.2 concluída: gerenciamento de estado do frontend.
- Build e 19 testes foram validados localmente após a Sprint 5.1.
