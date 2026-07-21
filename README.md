# LevelUp

Aplicação Blazor Server em .NET 10, organizada em camadas e com persistência local em JSON.

## Estrutura da solução

```text
LevelUp/
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
├── Directory.Build.props
├── Directory.Packages.props
└── LevelUp.slnx
```

## Responsabilidades

- **LevelUp.Domain**: entidades, enums e regras de domínio sem dependências externas.
- **LevelUp.Application**: contratos, serviços e casos de uso da aplicação.
- **LevelUp.Infrastructure**: persistência JSON, configuração e health checks.
- **LevelUp.Web**: aplicação Blazor, componentes e adaptação para a interface.
- **tests**: testes automatizados separados pela camada que validam.

## Injeção de dependência

Cada camada possui seu próprio método de composição:

- `AddLevelUpApplication()` registra serviços e casos de uso.
- `AddLevelUpInfrastructure(configuration)` registra persistência, opções e health checks.
- `Program.cs` atua somente como composition root da aplicação web.

## Configuração centralizada

- `Directory.Build.props`: framework, nullable, analyzers e propriedades comuns de compilação.
- `Directory.Packages.props`: versões centralizadas dos pacotes NuGet.
- `.editorconfig`: convenções de formatação e estilo compartilhadas por toda a solução.

## Execução

No diretório raiz:

```powershell
dotnet restore .\LevelUp.slnx
dotnet build .\LevelUp.slnx
dotnet test .\LevelUp.slnx
dotnet run --project .\src\LevelUp.Web\LevelUp.Web.csproj
```

O endpoint `GET /health` verifica a aplicação e o armazenamento JSON.
