# Solution Structure

**Fonte da verdade:** verificado diretamente em `BeeDay.slnx` e nos 9 arquivos `.csproj` de
`src/*` e `tests/*`.

## 1. `BeeDay.slnx`

O arquivo de solução organiza os projetos em pastas lógicas (formato `.slnx`, o sucessor XML do
`.sln` clássico):

```xml
<Solution>
  <Folder Name="/src/">
    <Folder Name="/src/Core/">
      <Project Path="src/BeeDay.Domain/BeeDay.Domain.csproj" />
      <Project Path="src/BeeDay.Application/BeeDay.Application.csproj" />
    </Folder>
    <Folder Name="/src/Infrastructure/">
      <Project Path="src/BeeDay.Infrastructure/BeeDay.Infrastructure.csproj" />
    </Folder>
    <Folder Name="/src/Presentation/">
      <Project Path="src/BeeDay.Web/BeeDay.Web.csproj" />
    </Folder>
  </Folder>
  <Folder Name="/tests/">
    <!-- 5 projetos de teste, um por projeto de src/ mais BeeDay.E2E.Tests -->
  </Folder>
  <Folder Name="/Solution Items/">
    <!-- arquivos de configuração de repositório, ver seção 4 -->
  </Folder>
</Solution>
```

As pastas lógicas `Core`, `Infrastructure` e `Presentation` são apenas organização visual no IDE —
não geram nenhuma pasta física em disco nem afetam o build; a estrutura física real em disco é
`src/BeeDay.Domain/`, `src/BeeDay.Application/`, `src/BeeDay.Infrastructure/`, `src/BeeDay.Web/`,
todas irmãs sob `src/`.

## 2. Projetos e suas responsabilidades físicas

| Projeto | Pasta em disco | Tipo de SDK |
|---|---|---|
| `BeeDay.Domain` | `src/BeeDay.Domain/` | `Microsoft.NET.Sdk` |
| `BeeDay.Application` | `src/BeeDay.Application/` | `Microsoft.NET.Sdk` |
| `BeeDay.Infrastructure` | `src/BeeDay.Infrastructure/` | `Microsoft.NET.Sdk` |
| `BeeDay.Web` | `src/BeeDay.Web/` | `Microsoft.NET.Sdk.Web` (único com SDK Web) |

Projetos de teste (`tests/`), um por projeto de produção mais um projeto E2E:

| Projeto de teste | Testa |
|---|---|
| `BeeDay.Domain.Tests` | `BeeDay.Domain` |
| `BeeDay.Application.Tests` | `BeeDay.Application` |
| `BeeDay.Infrastructure.Tests` | `BeeDay.Infrastructure` (contra SQL Server LocalDB real, descartável) |
| `BeeDay.Web.Tests` | `BeeDay.Web` (componentes bUnit + integração `WebApplicationFactory`) |
| `BeeDay.E2E.Tests` | Aplicação completa via Playwright/Chromium contra um Kestrel real |

## 3. Dependências entre projetos (`ProjectReference`, verificado em cada `.csproj`)

```text
BeeDay.Domain.csproj           → (nenhuma)
BeeDay.Application.csproj      → BeeDay.Domain
BeeDay.Infrastructure.csproj    → BeeDay.Application  (e transitivamente BeeDay.Domain)
BeeDay.Web.csproj              → BeeDay.Application, BeeDay.Domain, BeeDay.Infrastructure
```

`BeeDay.Web.csproj` referencia as 3 outras camadas diretamente (não apenas transitivamente) —
confirmando que é o único composition root: precisa de `BeeDay.Domain` para tipos de erro de
domínio usados em tratamento de exceção, de `BeeDay.Application` para Commands/Queries/interfaces,
e de `BeeDay.Infrastructure` para registrar `AddBeeDayInfrastructure(configuration)`.

`PackageReference`/`FrameworkReference` relevantes por projeto:

| Projeto | Referências notáveis |
|---|---|
| `BeeDay.Domain` | Nenhuma. |
| `BeeDay.Application` | `PackageReference FluentValidation`, `FluentValidation.DependencyInjectionExtensions`, `MediatR`. |
| `BeeDay.Infrastructure` | `FrameworkReference Microsoft.AspNetCore.App`; `PackageReference Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets=all`); `InternalsVisibleTo` para `BeeDay.Infrastructure.Tests`, `BeeDay.Web.Tests`, `BeeDay.E2E.Tests`. |
| `BeeDay.Web` | Nenhum `PackageReference` próprio além do SDK Web (Blazor Server vem do SDK). |

`BeeDay.Application.csproj` declarava `<FrameworkReference Include="Microsoft.AspNetCore.App" />`
desde o commit inicial do projeto, mas nenhum arquivo `.cs` sob `src/BeeDay.Application` jamais
importou um namespace `Microsoft.AspNetCore.*` — os únicos `using` de framework usados são
`Microsoft.Extensions.DependencyInjection` e `Microsoft.Extensions.Logging`, ambos disponíveis
independentemente dessa `FrameworkReference`. Confirmado via build isolado do projeto e da solução
completa sem a referência (0 erros). Removida na Sprint 18.3.

## 4. `Solution Items` (arquivos sem pasta de projeto própria)

Listados no `.slnx` como itens soltos, apenas para exibição no Visual Studio:
`.editorconfig`, `.gitattributes`, `.gitignore`, `CLAUDE.md`, `Directory.Build.props`,
`Directory.Packages.props`, `LICENSE`, `README.md`, mais 5 referências a `docs/*/README.md`
(`docs/README.md`, `docs/architecture/README.md`, `docs/design-system/README.md`,
`docs/developer/README.md`, `docs/domain/README.md`) — todas existem atualmente, confirmado por
leitura direta do `.slnx` e do disco na Sprint 30.28 (`BD30-F004`). O `.slnx` já foi corrigido desde
o achado original da Sprint 16.2 (que citava `docs/ai/README.md`/`docs/development/README.md` como
referências mortas); este texto só não havia sido atualizado para acompanhar essa correção.

## 5. Central Package Management

`Directory.Packages.props` centraliza a versão de todo pacote NuGet (`ManagePackageVersionsCentrally=true`)
— nenhum `.csproj` individual especifica uma versão de pacote, apenas `PackageReference Include="Nome"`
sem atributo `Version`.
