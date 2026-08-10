# Observability

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Program.cs`,
`src/BeeDay.Web/Diagnostics/`, `src/BeeDay.Web/HealthChecks/`,
`src/BeeDay.Infrastructure/Auditing/JsonEventJournal.cs`,
`src/BeeDay.Infrastructure/Background/BackgroundTaskWorker.cs`,
`src/BeeDay.Infrastructure/HealthChecks/SqlServerHealthCheck.cs`.

**Última verificação:** 2026-08-07.

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
convencional `ILogger<T>`). `WebEventIds.RequestFailed` (`EventId` 6100) é o único `EventId`
tipado do projeto Web, usado por `GlobalExceptionHandler`.

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

## 4. Event Journal — o log de auditoria de domain events

`JsonEventJournal` (`IEventJournal`, `src/BeeDay.Infrastructure/Auditing/`): log de auditoria
append-only, formato NDJSON (uma linha JSON por evento), completamente independente da persistência
de estado funcional (SQL Server) — grava em arquivo próprio, nunca lido de volta pela aplicação
(write-only, sem API de leitura). Cada linha: `Type` (nome da classe do evento), `EventId`,
`OccurredOnUtc`, `Summary` (texto legível só para `UserLeveledUpDomainEvent`; `null` para os
demais), `Payload` (o domain event serializado por completo).

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
- Sem rotação/retenção configurada para o Event Journal (§4) nem para o log de stdout do IIS — a
  responsabilidade de gerenciar o crescimento de ambos os arquivos não está automatizada em nenhum
  script deste repositório.
- Nenhum alerta automatizado (e-mail, Slack, PagerDuty) configurado a partir de qualquer sinal
  descrito acima — o único consumidor automatizado de `/health/ready` é `Deploy-BeeDay.ps1`, e
  apenas durante a janela do próprio deploy (6 tentativas, então para de verificar).

## 8. Fontes consultadas

- `src/BeeDay.Web/Program.cs`, `Diagnostics/CorrelationIdMiddleware.cs`,
  `GlobalExceptionHandler.cs`, `WebEventIds.cs`, `HealthChecks/HealthCheckResponseWriter.cs`.
- `src/BeeDay.Infrastructure/Auditing/JsonEventJournal.cs`,
  `Background/BackgroundTaskWorker.cs`, `Background/BackgroundTaskQueue.cs`,
  `HealthChecks/SqlServerHealthCheck.cs`.
- Busca por "Serilog", `IHostApplicationLifetime`, `BackgroundService` em `src/BeeDay.Web/` e
  `src/BeeDay.Infrastructure/`.
- [`docs/web/01-composition-root.md`](../web/01-composition-root.md),
  [`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md) (reaproveitados,
  não duplicados).
