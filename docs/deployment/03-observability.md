# Observability

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Program.cs`,
`src/BeeDay.Web/Diagnostics/`, `src/BeeDay.Web/HealthChecks/`,
`src/BeeDay.Infrastructure/Auditing/JsonEventJournal.cs`,
`src/BeeDay.Infrastructure/Background/BackgroundTaskWorker.cs`,
`src/BeeDay.Infrastructure/HealthChecks/SqlServerHealthCheck.cs`,
`src/BeeDay.Application/Common/Behaviors/*.cs`, `src/BeeDay.Domain/Events/*.cs`.

**Última verificação:** 2026-08-09 (Sprint 18.5 — classificação do Event Journal como Audit/Business
History e auditoria exaustiva de `EventId`; seções anteriores a esta data cobriam só logging de
`Program.cs`).

**Update (2026-08-17, EPIC 28 Sprint 28.7 — Observability Operationalization):** este documento
predatava por completo a EPIC 26 (e-mail transacional) e por isso não cobria nenhum dos sinais de
log que já existiam para esse fluxo — lacuna encerrada por §2.1 abaixo. O achado de §7 sobre ausência
de rotação/retenção para o log de stdout do IIS agora tem uma ferramenta dedicada (§7), mantendo o
restante da auditoria da Sprint 18.5 intacto.

## 1. Objetivo

Documentar o que é observável no BeeDay em execução hoje: logging, o log de auditoria de domain
events (Event Journal), health checks, e como a aplicação reage ao ciclo de vida do host
(startup/shutdown).

## 2. Logging

`Program.cs`: `builder.Logging.ClearProviders()` seguido de `AddJsonConsole` apenas — um único
provider, saída JSON estruturada em stdout, `IncludeScopes=true`, timestamp UTC ISO-8601.
**Nenhuma biblioteca de logging estruturado de terceiros está em uso** — busca por "Serilog" em
todo o repositório (código-fonte, `.csproj`, `Directory.Packages.props`) não encontrou nenhuma
ocorrência. Se um dia for adotado, é uma mudança de código fora do escopo desta Sprint (documentação
apenas).

`CorrelationIdMiddleware` (ver [`docs/web/01-composition-root.md`](../web/01-composition-root.md)
§7) abre um `logger.BeginScope` por requisição com `CorrelationId`/`RequestId` — todo log emitido
durante essa requisição carrega esses 2 campos automaticamente via `IncludeScopes`. Em HMG, o IIS
grava a saída padrão do processo em `stdoutLogFile` (`web.config`, corrigido na Sprint 18.4 para
`C:\Apps\BeeDay-Data\Logs\stdout` — era `LevelUp-Data`, confirmado ativo nesse path por verificação
direta do servidor antes da correção; ver [`02-runtime-configuration.md`](02-runtime-configuration.md)
§5) — não há um sink de arquivo próprio da aplicação; a persistência do log em disco é
responsabilidade do `AspNetCoreModuleV2` do IIS, não de `Program.cs`.

Eventos nomeados observados nesta auditoria (`LogInformation`/`LogWarning` com mensagem em formato
`Categoria.Evento`, não apenas texto livre): `Authentication.LoginSucceeded`,
`Authentication.LoginFailed`, `Authentication.LoginRateLimited`, `Authentication.LogoutSucceeded` —
todos em `Program.cs`, categoria de logger fixa `"BeeDay.Authentication"` (não o nome de classe
convencional `ILogger<T>`). `WebEventIds.RequestFailed` (`EventId` 6100) é o **único** `EventId`
tipado em todo o repositório (confirmado por auditoria exaustiva na Sprint 18.5 — `Infrastructure`
e `Domain` não definem nenhum outro), usado por `GlobalExceptionHandler`.

**Logging estruturado por request (pipeline MediatR):** além do que está descrito acima,
`LoggingBehavior<,>`/`PerformanceBehavior<,>` (`BeeDay.Application`) logam Information/Warning/Error
para **todo** Command/Query que passa por `ISender.Send` — mecanismo separado do logging de
`Program.cs`, documentado em detalhe em
[`docs/application/03-pipeline.md`](../application/03-pipeline.md) §1-2 (não duplicado aqui).

### 2.1 Transactional email logging (EPIC 28, Sprint 28.7)

`EmailEventIds` (`src/BeeDay.Infrastructure/Diagnostics/EmailEventIds.cs`, new this Sprint) is the
second typed-`EventId` class in the repository — `WebEventIds.RequestFailed` (6100, §2 above) was
the only one before it. Infrastructure-owned (it cannot reference `BeeDay.Web.Diagnostics`), numbered
in the `71xx` block to stay clearly distinct:

| EventId | Value | Emitted by | Level |
|---|---|---|---|
| `GuardAllowed` | 7100 | `HmgRecipientGuardedEmailSender` | Information |
| `GuardBlocked` | 7101 | `HmgRecipientGuardedEmailSender` | Warning |
| `ProviderDisabled` | 7102 | `ResendEmailSender` | Information |
| `ProviderAttempted` | 7103 | `ResendEmailSender` | Information |
| `ProviderAccepted` | 7104 | `ResendEmailSender` | Information |
| `ProviderRejected` | 7105 | `ResendEmailSender` | Error |
| `ProviderTimedOut` | 7106 | `ResendEmailSender` | Error |
| `ProviderNetworkFailure` | 7107 | `ResendEmailSender` | Error |
| `DevelopmentCaptureDisabled` | 7108 | `DevelopmentEmailSender` | Information |
| `DevelopmentCaptured` | 7109 | `DevelopmentEmailSender` | Information |

Retrofitted onto the exact log call sites that already existed — no new log statement was added, no
message text changed, no new information is logged. `accepted != delivered != inbox placement` still
applies (§14.1 of the owning document below); "send requested" remains without a dedicated log line
by design (the guard-allowed/blocked or provider-attempted line is the first real evidence — see that
document's own note on this). Full observable-state model, recipient-sanitization policy, and
troubleshooting table: [`docs/infrastructure/06-transactional-email.md`](../infrastructure/06-transactional-email.md)
§14 — not duplicated here, this section only adds the typed `EventId` layer on top of it.

**Operator recipe** (never exposes PII — stdout already never carries recipient addresses in the
Resend/Guard path, and `DevelopmentEmailSender`'s two lines mask the address before it reaches the
log):

```text
Select-String -Path "C:\Apps\BeeDay-Data\Logs\stdout_*.log" -Pattern '"EventId":\{"Id":710[0-9]'
```

(`AddJsonConsole`'s structured output carries `EventId.Id` as a JSON field — filtering on it is
exact, unlike matching message text, which can drift if a message is ever reworded.)

## 3. Health Checks

3 endpoints (`Program.cs`), formatados em JSON por `HealthCheckResponseWriter`:

| Rota | `Predicate` | Uso pretendido |
|---|---|---|
| `/health/live` | `_ => false` (nenhum check roda) | Liveness — "o processo responde" |
| `/health/ready` | `tag == "ready"` | Readiness — consultado por `Deploy-BeeDay.ps1` pós-deploy |
| `/health` | `_ => true` | Todos os checks — `Degraded`→200, `Unhealthy`→503 |

Único check registrado no repositório: `SqlServerHealthCheck` (`BeeDay.Infrastructure`, tags
`ready`/`storage`/`sql`, `CanConnectAsync`) — ver
[`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md) §5 para o diagrama.
Corpo de resposta: `status`, `durationMs`, `correlationId`, e por check `name`/`status`/
`description`/`durationMs`/`data`. Nenhum dos 3 endpoints exige autenticação — ver
[`docs/security/02-operational-security.md`](../security/02-operational-security.md) §10.

## 4. Event Journal — trilha de auditoria de negócio (não logging técnico)

**Classificação confirmada na Sprint 18.5, após auditoria dedicada:** o Event Journal **não é um
mecanismo de logging/observabilidade técnica** — é uma trilha de auditoria de histórico de negócio.
A distinção importa porque `DomainEventBehavior` (ver
[`docs/application/03-pipeline.md`](../application/03-pipeline.md) §4) publica um
`ApplicationActionDomainEvent` para **todo Command que termina com sucesso** — o journal registra
sistematicamente "qual ação de negócio aconteceu, quando, sobre qual entidade", não "o que o
processo estava fazendo tecnicamente" (isso é papel do logging estruturado, §2 acima). Os 2 domain
events específicos de XP (`ExperienceGrantedDomainEvent`, `UserLeveledUpDomainEvent`) somam-se a
essa mesma trilha. Nenhum dos 3 domain events carrega dado pessoal — só identificadores opacos
(`Guid`), enums, valores numéricos e timestamps; `Action`/`Category` do `ApplicationActionDomainEvent`
vêm sempre do nome do tipo C# do Command, nunca de texto livre digitado pelo usuário.

`JsonEventJournal` (`IEventJournal`, `src/BeeDay.Infrastructure/Auditing/`): append-only, formato
NDJSON (uma linha JSON por evento), completamente independente da persistência de estado funcional
(SQL Server) — grava em arquivo próprio, nunca lido de volta pela aplicação (write-only, sem API de
leitura; nenhuma ferramenta operacional deste repositório o lê de volta). Cada linha: `Type` (nome
da classe do evento), `EventId`, `OccurredOnUtc`, `Summary` (texto legível só para
`UserLeveledUpDomainEvent`; `null` para os demais), `Payload` (o domain event serializado por
completo).

**Deduplicação**: antes de gravar, `ContainsAsync` lê o arquivo inteiro linha a linha procurando o
mesmo `EventId` (ou, para level-up, o mesmo `ExperienceEntryId`) — evita duplicata se o mesmo evento
for processado 2 vezes. Isso significa o custo de escrita cresce linearmente com o tamanho do
arquivo (cada `AppendAsync` relê o arquivo inteiro) — sem rotação/arquivamento automático
encontrado; um journal de produção de longa duração cresce indefinidamente e cada escrita fica mais
lenta. Concorrência dentro do processo é serializada por um `SemaphoreSlim(1,1)` — escreve nunca
colidem entre si dentro da mesma instância do processo (não protege contra 2 instâncias/processos
diferentes escrevendo o mesmo arquivo simultaneamente).

**Caminho de resolução**: relativo a `IHostEnvironment.ContentRootPath` quando
`EventJournalOptions.Directory` não é absoluto (caso do `appsettings.json` base, `"Data"`), absoluto
quando já é (caso de `appsettings.Homologation.json`, `C:\Apps\BeeDay-Data\EventJournal` — o path
real em HMG, confirmado ativo com dados existentes na Sprint 18.4; ver
[`02-runtime-configuration.md`](02-runtime-configuration.md) §5 para a análise completa, incluindo
`appsettings.Production.json`, que não corresponde a nenhum ambiente provisionado hoje).

**Quem escreve**: `AuditDomainEventHandler` (não lido nesta auditoria em detalhe — Application
layer, ver [`docs/application/`](../application/README.md)), disparado de forma fire-and-forget via
`BackgroundTaskQueue`/`BackgroundTaskWorker` (ver §5) — a gravação do journal nunca bloqueia o
handler de comando que originou o domain event.

## 5. Ciclo de vida da aplicação — `BackgroundTaskWorker`

Único `BackgroundService` do repositório (`src/BeeDay.Infrastructure/Background/`): laço
`while (!stoppingToken.IsCancellationRequested)` que desenfileira e executa itens de
`BackgroundTaskQueue` (usado para a escrita do Event Journal — ver §4). `stoppingToken` é fornecido
pelo generic host do .NET, cancelado automaticamente quando o host entra em
`ApplicationStopping` — nenhum código customizado de `IHostApplicationLifetime` foi encontrado;
a aplicação depende inteiramente do comportamento padrão de shutdown gracioso do host genérico
(`BackgroundService.StopAsync`, com o timeout padrão do host).

**Implicação de observabilidade**: um item enfileirado (ex.: gravar um evento de level-up no
journal) durante um shutdown gracioso pode não terminar de ser processado antes do timeout de parada
do host — o design é fire-and-forget deliberadamente (não bloqueia o caminho de negócio principal),
então a perda ocasional de uma entrada de auditoria durante um deploy é uma característica aceita do
design, não um defeito — mas não há métrica/log que confirme quantos itens ficaram pendentes na fila
no momento do shutdown.

## 6. Diagnostics — `CorrelationIdMiddleware` e `GlobalExceptionHandler`

Documentados por completo em [`docs/web/01-composition-root.md`](../web/01-composition-root.md) §7
— resumo: todo erro ≥500 é `LogError`, os demais `LogWarning`, ambos com `WebEventIds.RequestFailed`;
toda resposta de erro carrega `correlationId`/`requestId` como extension do `ProblemDetails`.

## 7. O que não é observável hoje (achados)

- Nenhuma métrica exportada (Prometheus, `System.Diagnostics.Metrics`, Application Insights) — só
  logs e os 3 health checks.
- Nenhum tracing distribuído (`OpenTelemetry`, `Activity`/`ActivitySource` customizado) — o único
  uso de `Activity` é o padrão do ASP.NET Core em `Error.razor` (`Activity.Current?.Id`, ver
  [`docs/web/02-routing-and-pages.md`](../web/02-routing-and-pages.md) §7), não instrumentação
  própria.
- **Event Journal (§4):** ainda sem rotação/retenção — permanece exatamente como a Sprint 18.5
  encontrou, fora do escopo da Sprint 28.7 (que tratou apenas de e-mail transacional/stdout).
- **Log de stdout do IIS:** a Sprint 18.5 registrou esse gap como risco operacional conhecido, sem
  implementá-lo (fora do escopo daquela auditoria). A **Sprint 28.7 (EPIC 28) fecha esse gap**:
  `scripts/Clear-BeeDayStdoutLogs.ps1` (novo) remove, de forma idempotente, apenas arquivos
  `stdout_*.log` (a convenção de nomes do próprio ANCM) mais antigos que `-RetentionDays` (padrão 30
  dias) no diretório informado — nunca toca em nenhum outro arquivo, nunca lança erro se o diretório
  ainda não existir, suporta `-WhatIf`. Deliberadamente **não** foi acoplado ao caminho crítico de
  deploy/rollback de `Deploy-BeeDay.ps1` — uma falha nele nunca pode afetar um deploy. Coberto por
  `scripts/tests/Test-ClearBeeDayStdoutLogs.ps1` (8 asserções), executado no mesmo preflight
  `deploy-hmg.yml` já usa para os outros dois suites de regressão do deploy.
  **Estado: Code Complete, não Environment Validated** — este script não foi ainda agendado (ex.:
  Windows Scheduled Task) em nenhum ambiente real; agendá-lo é uma decisão/execução operacional fora
  do escopo desta auditoria de repositório. Comando de referência:
  `powershell -File scripts\Clear-BeeDayStdoutLogs.ps1 -Directory "C:\Apps\BeeDay-Data\Logs"
  -RetentionDays 30`.
- Nenhum alerta automatizado (e-mail, Slack, PagerDuty) configurado a partir de qualquer sinal
  descrito acima — o único consumidor automatizado de `/health/ready` é `Deploy-BeeDay.ps1`, e
  apenas durante a janela do próprio deploy (6 tentativas, então para de verificar).

## 8. Fontes consultadas

- `src/BeeDay.Web/Program.cs`, `Diagnostics/CorrelationIdMiddleware.cs`,
  `GlobalExceptionHandler.cs`, `WebEventIds.cs`, `HealthChecks/HealthCheckResponseWriter.cs`.
- `src/BeeDay.Infrastructure/Auditing/JsonEventJournal.cs`,
  `Background/BackgroundTaskWorker.cs`, `Background/BackgroundTaskQueue.cs`,
  `HealthChecks/SqlServerHealthCheck.cs`.
- `src/BeeDay.Application/Common/Behaviors/LoggingBehavior.cs`, `PerformanceBehavior.cs`,
  `DomainEventBehavior.cs`; `src/BeeDay.Domain/Events/*.cs` (verificação de ausência de PII nos
  domain events, Sprint 18.5).
- Busca por "Serilog", `IHostApplicationLifetime`, `BackgroundService` em `src/BeeDay.Web/` e
  `src/BeeDay.Infrastructure/`.
- [`docs/web/01-composition-root.md`](../web/01-composition-root.md),
  [`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md) (reaproveitados,
  não duplicados).
- EPIC 28, Sprint 28.7: `src/BeeDay.Infrastructure/Diagnostics/EmailEventIds.cs`,
  `Identity/{HmgRecipientGuardedEmailSender,ResendEmailSender,DevelopmentEmailSender}.cs`,
  `scripts/Clear-BeeDayStdoutLogs.ps1`, `scripts/tests/Test-ClearBeeDayStdoutLogs.ps1`,
  `.github/workflows/deploy-hmg.yml`.
