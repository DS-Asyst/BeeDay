# Runtime Configuration

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/appsettings.json`,
`appsettings.Development.json`, `appsettings.Homologation.json`, `appsettings.Production.json`,
`src/BeeDay.Web/Program.cs`, `src/BeeDay.Web/Configuration/ProductionHostingOptions.cs`,
`src/BeeDay.Infrastructure/Configuration/*.cs`,
`src/BeeDay.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`, e
Runtime State real de SERV3WEB/HMG verificado diretamente no servidor na Sprint 18.4.

**Última verificação:** 2026-08-16 (Epic 26, Sprint 26.3) — §6 adicionado: contrato oficial de
secrets/configuração de e-mail transacional, formalizando o mecanismo já existente
(`Deploy-BeeDay.ps1` + GitHub Environment secrets + Scheduled Task privilegiado). Verificação
anterior em 2026-08-09 (Sprint 18.4 — inclui os 4 arquivos `appsettings*.json` e verificação de
Runtime State em HMG; seções anteriores a esta data cobriam só 3 arquivos).

## 1. Objetivo

Documentar toda fonte de configuração de runtime do BeeDay: os 4 arquivos `appsettings*.json`, as
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

## 3. Os 4 arquivos `appsettings*.json`

| Arquivo | `SqlServer:ConnectionString` | Propósito |
|---|---|---|
| `appsettings.json` (base) | `Server=(localdb)\mssqllocaldb;Database=BeeDayDev;...` (valor commitado — ver nota abaixo) | Valores de desenvolvimento local por padrão |
| `appsettings.Development.json` | (não definido — herda do base) | Só ajusta `Logging:LogLevel` (mais verboso para `Microsoft.AspNetCore`, silencia `Circuits`) |
| `appsettings.Homologation.json` | `Server=SERV4SQL;Database=BeeDay_HMG;Trusted_Connection=True;...` (commitado; `deploy-hmg.yml` sobrescreve via `BEEDAY_APP_CONNECTION`) | **É o arquivo que HMG realmente carrega hoje** — `ASPNETCORE_ENVIRONMENT=Homologation` é fixado em `web.config` e passado explicitamente por `deploy-hmg.yml`, confirmado por Runtime State real (Sprint 18.4). `AllowedHosts=h-beeday.com.br`, `Resend:Enabled=true`, `Email:Development:Enabled=false` (invertido desde a ativação do Resend em HMG — corrigido pela `BD30-F006`, Sprint 30.25; ver [`14-transactional-email-runbook.md`](14-transactional-email-runbook.md) §2) |
| `appsettings.Production.json` | `""` (vazio — **deve** ser injetado via variável de ambiente/secret) | **Não corresponde a nenhum ambiente provisionado hoje** — ver §5. `Hosting:ForwardedHeaders` habilitado, `Resend:Enabled: true` |

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

## 5. `LevelUp-Data` vs. `BeeDay-Data` e o estado real de HMG/PRD (Sprint 18.4)

### 5.1 PRD não está provisionado — decisão arquitetural

**PRD não existe como ambiente runtime hoje, por decisão deliberada.** O único ambiente real em
execução é HMG (SERV3WEB). A branch Git `prd` e o workflow `deploy-prd.yml` são artefatos
preparatórios/futuros — `deploy-prd.yml` nunca foi executado com sua configuração atual (sem
`-Environment`, sem `-AppConnectionString`, sem `-RunMigrations`) contra um servidor real, e não há
GitHub Environment `production` provisionado. Produção será provisionada futuramente em Azure,
quando decidido — nesse momento, `appsettings.Production.json` e `deploy-prd.yml` provavelmente
serão redesenhados para a infraestrutura real escolhida, não apenas corrigidos incrementalmente.

Consequência direta: **`appsettings.Production.json` não corresponde a nenhum Runtime State
existente.** Nenhum mecanismo commitado hoje (nem `web.config`, que fixa
`ASPNETCORE_ENVIRONMENT=Homologation`, nem `deploy-hmg.yml`, que passa `-Environment "Homologation"`
explicitamente, nem `deploy-prd.yml`, que usa o default `"Homologation"` de `Deploy-BeeDay.ps1` por
nunca sobrescrevê-lo) jamais seleciona `ASPNETCORE_ENVIRONMENT=Production` — logo esse arquivo nunca
é carregado por nenhum processo real hoje.

### 5.2 Runtime State de HMG confirmado (Sprint 18.4)

Verificado diretamente em SERV3WEB: Site `BeeDay-HMG`, App Pool `BeeDay-Web-AppPool`,
`physicalPath=C:\Apps\BeeDay.Web`, `ASPNETCORE_ENVIRONMENT=Homologation`,
`DOTNET_ENVIRONMENT=Homologation`, banco `SERV4SQL/BeeDay_HMG` (com override de connection string
presente no App Pool, via `BEEDAY_APP_CONNECTION`). Confirma que `appsettings.Homologation.json` é
o arquivo real em uso.

| Item | Path configurado (`appsettings.Homologation.json`) | Provisionado/ACL por `Deploy-BeeDay.ps1`? | Runtime State confirmado |
|---|---|---|---|
| Data Protection Keys | `C:\Apps\BeeDay-Data\DataProtection-Keys` | Sim | Ativo, chaves existentes — **correto, sem divergência** |
| Event Journal | `C:\Apps\BeeDay-Data\EventJournal` | Não explicitamente (só `...\Data` era verificado) — corrigido na Sprint 18.4 | Ativo, `BeeDayEvents.ndjson` existente — funcionava por permissão não verificada explicitamente; `Deploy-BeeDay.ps1` agora cria/verifica ACL nesse path também, sem mover o arquivo existente |
| DevelopmentEmail | Era `App_Data\Emails` (relativo, resolvia dentro de `$DestinationPath` — apagado a cada deploy) | Não (path nunca era externo) | `App_Data\Emails` não existia; `C:\Apps\BeeDay-Data\Emails` existia vazio — corrigido na Sprint 18.4 para apontar ao path externo já provisionado, sem perda de dado |
| stdout (`web.config`) | Fixo em `web.config`, não depende de `appsettings` | Não (`Deploy-BeeDay.ps1` só protege `...\Logs` sob `BeeDay-Data`) | Confirmado ativo em `C:\Apps\LevelUp-Data\Logs` — corrigido no repositório para `BeeDay-Data\Logs` na Sprint 18.4; **migração operacional (promoção + validação pós-deploy) ainda pendente**, path antigo não foi apagado |

### 5.3 `appsettings.Production.json` corrigido por consistência, não por uso real

As mesmas 3 chaves foram corrigidas de `LevelUp-Data` para `BeeDay-Data` nesta Sprint, alinhando ao
padrão de nomenclatura já usado em `Deploy-BeeDay.ps1`/`appsettings.Homologation.json` — mas, como
§5.1 estabelece, isso é reconciliação de nomenclatura em um arquivo hoje inerte, não uma correção de
comportamento observável (nada muda em runtime, pois nada carrega este arquivo).

## 6. Contrato oficial de secrets e configuração de e-mail transacional (EPIC 26, Sprint 26.3)

**Fonte da verdade desta seção:** verificado diretamente em `scripts/Deploy-BeeDay.ps1`,
`.github/workflows/deploy-hmg.yml`, `.github/workflows/deploy-prd.yml`, os 4 arquivos
`appsettings*.json`, `InfrastructureServiceCollectionExtensions.cs`, e
`tests/BeeDay.Infrastructure.Tests/EmailSecretsConfigurationTests.cs`. Não requereu nem inspecionou
o valor real de nenhum secret — apenas a mecânica que os transporta.

Formaliza o mecanismo já existente (não uma nova infraestrutura) que decide o que é secret,
onde ele vive, como chega a cada ambiente, e o que acontece quando está ausente ou inválido.

### 6.1 Configuração versionada × secret

| Chave | Versionada em `appsettings*.json`? | Valor commitado |
|---|---|---|
| `BeeDay:Email:Resend:Enabled` | Sim | `false` (base/Homologation), `true` (Production) |
| `BeeDay:Email:Resend:FromName` | Sim | `"beeday"` |
| `BeeDay:Email:Resend:ApiKey` | Sim, mas **sempre vazio ou ausente** | `""` (base/Production) ou chave ausente (Homologation) |
| `BeeDay:Email:Resend:FromAddress` | Sim, mas **sempre vazio** | `""` |

Apenas `ApiKey`/`FromAddress` são secret — nunca um valor real chega ao Git. `Enabled`/`FromName`
são configuração normal versionada, sem risco de exposição.
`EmailSecretsConfigurationTests.CommittedAppsettings_NeverCarriesAResendApiKeyValue` (Sprint 26.3)
varre os 4 arquivos e falha caso qualquer um passe a commitar um `ApiKey` não vazio — regressão
detectada no `dotnet test` normal, sem depender de revisão manual de PR para pegar o vazamento.

### 6.2 Canal de entrega do secret — já existente, não uma nova escolha desta Sprint

```text
GitHub Environment secret (escopo "homologation" ou "production", MESMO nome BEEDAY_RESEND_API_KEY)
  → env: do job em deploy-hmg.yml / deploy-prd.yml
    → parâmetro -ResendApiKey de Deploy-BeeDay.ps1
      → Set-BeeDayEnvironmentVariables monta o hashtable de variáveis
        → HMG: Invoke-BeeDayPrivilegedIisControl -Operation CONFIGURE (Scheduled Task \BeeDay\HMG-IisControl, SYSTEM)
          → variável de App Pool BeeDay__Email__Resend__ApiKey (nunca appsettings*.json)
        → Produção (não-HMG): Add-WebConfigurationProperty direto (sem a boundary privilegiada — ver docs/deployment/05-privileged-iis-control.md)
```

`deploy-hmg.yml` e `deploy-prd.yml` leem o **mesmo nome** de secret (`secrets.BEEDAY_RESEND_API_KEY`
etc.), mas cada workflow declara um `environment:` do GitHub Actions diferente (`homologation` vs.
`production`) — o mecanismo padrão do GitHub Environments para ter o mesmo nome de secret com
valores diferentes por ambiente. **Isso satisfaz o invariante "HMG e Produção nunca compartilham
credencial" apenas se o dono do repositório efetivamente configurou valores distintos nos dois
GitHub Environments** — um pré-requisito externo, do lado do GitHub, que este código não pode
verificar nem substituir; não foi (e não pode ser) confirmado nesta Sprint a partir do repositório.

`Deploy-BeeDay.ps1` só inclui as 3 variáveis de Resend no payload quando `ResendApiKey` **e**
`ResendFromAddress` chegam não vazios (`Set-BeeDayEnvironmentVariables`, guarda em
`if (-not [string]::IsNullOrWhiteSpace(...))`) — se os secrets do GitHub Environment nunca foram
preenchidos, a variável de App Pool simplesmente não é tocada, preservando o que já estiver
configurado manualmente, em vez de sobrescrever com uma string vazia.

### 6.3 Comportamento de startup quando Resend está selecionado mas o secret está ausente

`ResendOptions` já falha via `.ValidateOnStart()` (`InfrastructureServiceCollectionExtensions.cs`)
se `Enabled=true` e `ApiKey`/`FromAddress` estiverem vazios — o processo nunca sobe, IIS reporta
502.5/503, nunca uma falha silenciosa em produção. Provado ponta a ponta (não apenas lido no
código) por `EmailSecretsConfigurationTests.Host_WhenResendSelectedWithoutApiKey_FailsToStartPredictably`
e `..._WithoutFromAddress_...` (Sprint 26.3): um `Microsoft.Extensions.Hosting.Host` real,
`ConfigureServices` chamando `AddBeeDayInfrastructure`, `StartAsync()` lança
`OptionsValidationException`. O provider `Development` nunca depende de nenhum secret de Resend —
provado pelo teste irmão `..._StartsWithoutRequiringAnyResendSecret`.

### 6.4 Nunca vaza para logs/artefatos

`Deploy-BeeDay.ps1` mantém `$script:secretValuesToRedact` (inclui `$ResendApiKey`) e todo
`Write-DeployMessage`/mensagem de erro passa por `Protect-DeploySecret`, que substitui qualquer
ocorrência literal do valor por `[REDACTED]` antes de gravar no arquivo de log em
`C:\Apps\BeeDay-Data\DeployLogs\` — nunca apenas confiando na mascaração automática de secrets do
GitHub Actions (que não alcança esse arquivo, escrito direto em disco no runner). O payload da
operação CONFIGURE (`env-config.secret`) nunca passa por `Write-DeployMessage`/`Write-Host` — só a
contagem de variáveis é logada, nunca nomes ou valores (`Invoke-BeeDayPrivilegedIisControl`).

### 6.5 Rollback quando a configuração é inválida

Se qualquer etapa falhar após `Set-BeeDayEnvironmentVariables` já ter rodado (CONFIGURE já
aplicado), o bloco `catch` de `Deploy-BeeDay.ps1` chama `Restore-BeeDayIisEnvironmentVariables`, que
dispara a operação `RESTORE` do mesmo Scheduled Task privilegiado, devolvendo as variáveis de
ambiente do App Pool ao snapshot anterior ao CONFIGURE desta tentativa — inclusive um eventual
`BeeDay__Email__Resend__ApiKey` que já estivesse configurado antes do deploy. Se CONFIGURE nunca
chegou a rodar nesta tentativa (`$script:lastConfigureRequestId` continua `$null`), nada é
restaurado — não há o que desfazer.

### 6.6 Rotação do secret

Não há mecanismo de rotação automática — é operacional, fora do escopo de código: substituir o
valor do secret no GitHub Environment (`homologation` ou `production`) correspondente, depois
disparar um novo deploy (`workflow_dispatch` em `deploy-hmg.yml`/`deploy-prd.yml`, ou um push normal
em `hmg`/`prd`) para que `Set-BeeDayEnvironmentVariables`/CONFIGURE grave o novo valor na variável
de App Pool. Nenhuma etapa do código precisa mudar para uma rotação — o mecanismo do §6.2 já é
genérico o suficiente.

### 6.7 Produção permanece não ativada por esta Sprint

Nada nesta Sprint habilita `Resend:Enabled=true` em `appsettings.Homologation.json` nem executa
`deploy-prd.yml` contra um ambiente real. Na época desta Sprint, `appsettings.Homologation.json`
continuava com `Resend:Enabled=false`/`Development:Enabled=true` — a mesma seleção de provider
documentada em [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §5.1,
inalterada por esta Sprint. Ligar Resend em HMG dependia da guarda de destinatário centralizada da
Sprint 26.4 (Gate B do roadmap do EPIC 26) — não deste contrato de secrets isoladamente. **Esse
estado foi invertido posteriormente**: o `appsettings.Homologation.json` atual tem
`Resend:Enabled=true`/`Development:Enabled=false` (corrigido pela `BD30-F006`, Sprint 30.25); ver
[`14-transactional-email-runbook.md`](14-transactional-email-runbook.md) §2 para o estado real
vigente.

## 7. Fontes consultadas

- `src/BeeDay.Web/appsettings.json`, `appsettings.Development.json`, `appsettings.Homologation.json`,
  `appsettings.Production.json`.
- `src/BeeDay.Web/Program.cs`, `Configuration/ProductionHostingOptions.cs`, `web.config`.
- `src/BeeDay.Infrastructure/Configuration/*.cs`,
  `DependencyInjection/InfrastructureServiceCollectionExtensions.cs`.
- `scripts/Deploy-BeeDay.ps1`.
- `.github/workflows/deploy-hmg.yml`, `deploy-prd.yml`.
- `tests/BeeDay.Infrastructure.Tests/EmailSecretsConfigurationTests.cs` (EPIC 26, Sprint 26.3 — §6).
- `git diff`/`git show HEAD` sobre `src/BeeDay.Web/appsettings.json` (confirmação do valor
  commitado vs. o valor local não commitado).
- Runtime State real de SERV3WEB/HMG, verificado diretamente no servidor (Sprint 18.4): Site, App
  Pool, `physicalPath`, `ASPNETCORE_ENVIRONMENT`/`DOTNET_ENVIRONMENT` efetivos, presença de Data
  Protection keys, Event Journal e diretórios de e-mail, `web.config` instalado.
- [`docs/web/01-composition-root.md`](../web/01-composition-root.md) (guardas de produção,
  reaproveitado da Sprint 16.7).
