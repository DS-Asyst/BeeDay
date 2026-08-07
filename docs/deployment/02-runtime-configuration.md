# Runtime Configuration

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/appsettings.json`,
`appsettings.Development.json`, `appsettings.Production.json`, `src/BeeDay.Web/Program.cs`,
`src/BeeDay.Web/Configuration/ProductionHostingOptions.cs`,
`src/BeeDay.Infrastructure/Configuration/*.cs`,
`src/BeeDay.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Documentar toda fonte de configuração de runtime do BeeDay: os 3 arquivos `appsettings*.json`, as
variáveis de ambiente que os sobrescrevem, o binding para `Options`, e as guardas que impedem o
processo de iniciar com configuração inválida.

## 2. Hierarquia de configuração (ASP.NET Core padrão, não customizada)

```text
appsettings.json (base, sempre carregado)
  → appsettings.{ASPNETCORE_ENVIRONMENT}.json (Development ou Production, sobrescreve o base)
    → Variáveis de ambiente (sobrescrevem tudo acima — formato BeeDay__Secao__Chave)
      → User Secrets (Development apenas — UserSecretsId "BeeDay-Web-Identity" no .csproj;
        conteúdo não versionado, não inspecionável a partir do repositório)
```

Nenhum provider de configuração customizado foi encontrado em `Program.cs` — a aplicação usa
exatamente o pipeline padrão de `WebApplication.CreateBuilder(args)`, sem `AddAzureKeyVault`,
`AddJsonFile` extra, ou equivalente.

## 3. Os 3 arquivos `appsettings*.json`

| Arquivo | `SqlServer:ConnectionString` | Propósito |
|---|---|---|
| `appsettings.json` (base) | `Server=(localdb)\mssqllocaldb;Database=BeeDayDev;...` (valor commitado — ver nota abaixo) | Valores de desenvolvimento local por padrão |
| `appsettings.Development.json` | (não definido — herda do base) | Só ajusta `Logging:LogLevel` (mais verboso para `Microsoft.AspNetCore`, silencia `Circuits`) |
| `appsettings.Production.json` | `""` (vazio — **deve** ser injetado via variável de ambiente/secret) | Único arquivo com `Hosting:ForwardedHeaders` habilitado, `Resend:Enabled: true` |

**Nota sobre o arquivo local no momento desta auditoria:** o `appsettings.json` neste checkout tem
uma modificação não commitada (`git diff` confirma) — `ConnectionString` aponta para
`Server=SERV4SQL;Database=BeeDay_HMG;...` em vez do valor commitado acima. Isso foi identificado e
reportado ao usuário na Sprint 16.8; **não é uma configuração alterada por esta Sprint** (nenhum
arquivo de configuração foi editado nesta auditoria) e não deve ser incluído em nenhum commit desta
área de trabalho sem confirmação explícita — ver `CLAUDE.md`, "Safety and Git" (nunca commitar
configuração local).

## 4. Variáveis de ambiente

### 4.1 Lidas diretamente por `Program.cs` (fora do binding de `Options`)

| Variável (config key) | Onde é lida | Efeito |
|---|---|---|
| `AllowedHosts` | `builder.Configuration["AllowedHosts"]` | Fora de Development, deve ser não vazio e sem `*` — senão `InvalidOperationException` no startup |
| `BeeDay:IdentityEmail:PublicBaseUrl` | idem | Fora de Development, deve ser URL absoluta HTTPS |
| `BeeDay:Hosting:DataProtectionKeysDirectory` | `ProductionHostingOptions` (bind manual via `GetSection().Get<T>()`, não `AddOptions`) | Fora de Development, deve ser caminho absoluto |

Essas 3 validações rodam **antes** de `builder.Build()` — ver
[`docs/web/01-composition-root.md`](../web/01-composition-root.md) §3 para o porquê (compatibilidade
com `WebApplicationFactory` de testes).

### 4.2 Ligadas a `Options` com `ValidateOnStart()` (via `AddBeeDayInfrastructure`)

| Options | Seção | Validações |
|---|---|---|
| `IdentityEmailOptions` | `BeeDay:IdentityEmail` | `PublicBaseUrl` absoluta; `ConfirmationPath`/`PasswordResetPath` começam com `/` |
| `DevelopmentEmailOptions` | `BeeDay:Email:Development` | `Directory` obrigatório se `Enabled` |
| `ResendOptions` | `BeeDay:Email:Resend` | `ApiKey`/`FromAddress` obrigatórios se `Enabled`; `FromAddress` contém `@` |
| `SqlServerOptions` | `BeeDay:Persistence:SqlServer` | `ConnectionString` não vazio |
| `EventJournalOptions` | `BeeDay:Auditing:EventJournal` | `Directory` não vazio; `FileName` é um nome de arquivo simples |
| `LoginRateLimiterOptions` | `BeeDay:RateLimiting:Login` | Sem `.Validate()` — só `Bind` (ver [`docs/web/01-composition-root.md`](../web/01-composition-root.md) §9) |

Todas com `.ValidateOnStart()` — uma configuração inválida derruba o processo no boot, não na
primeira requisição. Isso significa que um deploy com configuração quebrada falha rápido e visível
(processo não sobe, IIS reporta 502.5/503), nunca silenciosamente em produção.

### 4.3 Formato de variável de ambiente (IIS Application Pool)

`Deploy-BeeDay.ps1` grava as variáveis diretamente no Application Pool via
`Add-WebConfigurationProperty` (não em `web.config`) usando o separador `__` (duplo underscore) do
`Microsoft.Extensions.Configuration` — ex.: `BeeDay__IdentityEmail__PublicBaseUrl` mapeia para
`BeeDay:IdentityEmail:PublicBaseUrl`. 6 variáveis são definidas: `ASPNETCORE_ENVIRONMENT`,
`DOTNET_ENVIRONMENT` (ambas fixas em `"Production"`), `AllowedHosts`,
`BeeDay__IdentityEmail__PublicBaseUrl`, `BeeDay__Email__Resend__ApiKey`,
`BeeDay__Email__Resend__FromAddress`, `BeeDay__Email__Resend__FromName` — 7 no total, contando as 2
de ambiente.

## 5. Divergência de caminho — `LevelUp-Data` vs. `BeeDay-Data`

3 valores em `appsettings.Production.json` apontam para `C:\Apps\LevelUp-Data\...`:

```json
"Hosting": { "DataProtectionKeysDirectory": "C:\\Apps\\LevelUp-Data\\DataProtection-Keys" },
"Auditing": { "EventJournal": { "Directory": "C:\\Apps\\LevelUp-Data\\Data" } },
"Email": { "Development": { "Directory": "C:\\Apps\\LevelUp-Data\\Emails" } }
```

`scripts/Deploy-BeeDay.ps1` (`$externalRoot = "C:\Apps\BeeDay-Data"`) cria e concede permissão
`Modify` a `IIS AppPool\BeeDayPool` apenas em:

```text
C:\Apps\BeeDay-Data\Data
C:\Apps\BeeDay-Data\Data\Backups
C:\Apps\BeeDay-Data\DataProtection-Keys
C:\Apps\BeeDay-Data\Emails
C:\Apps\BeeDay-Data\Logs
```

**Nenhuma dessas 3 configurações é sobrescrita por variável de ambiente no deploy** (`Set-BeeDayEnvironmentVariables`
só define os 5 `BeeDay__*` da tabela §4.3, nenhum deles relacionado a `Hosting`/`Auditing`) — ou
seja, com o repositório como está hoje, um deploy real usaria os caminhos `LevelUp-Data` do
`appsettings.Production.json` sem que o script de deploy jamais tenha criado essas pastas ou
concedido permissão de escrita nelas ao pool do IIS. Efeito esperado: `DataProtectionKeysDirectory`
falhando por falta de permissão impediria a persistência de chaves de criptografia entre reciclagens
do pool (cookies de autenticação seriam invalidados a cada reciclagem); `EventJournal` falhando ao
escrever perderia silenciosamente o log de auditoria de domain events (o `AppendAsync` não é
aguardado de forma síncrona pelo caminho principal de negócio — ver
[`03-observability.md`](03-observability.md) §2). Não corrigido (`appsettings.Production.json` é
configuração, fora do escopo de alteração desta Sprint).

## 6. Fontes consultadas

- `src/BeeDay.Web/appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.
- `src/BeeDay.Web/Program.cs`, `Configuration/ProductionHostingOptions.cs`.
- `src/BeeDay.Infrastructure/Configuration/*.cs` (5 classes Options),
  `DependencyInjection/InfrastructureServiceCollectionExtensions.cs`.
- `scripts/Deploy-BeeDay.ps1`.
- `git diff`/`git show HEAD` sobre `src/BeeDay.Web/appsettings.json` (confirmação do valor
  commitado vs. o valor local não commitado).
- [`docs/web/01-composition-root.md`](../web/01-composition-root.md) (guardas de produção,
  reaproveitado da Sprint 16.7).
