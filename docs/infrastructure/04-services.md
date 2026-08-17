# Infrastructure Services

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Infrastructure/Auditing/JsonEventJournal.cs`,
`Identity/*.cs`, `Security/Pbkdf2PasswordService.cs`,
`HealthChecks/SqlServerHealthCheck.cs`, `Background/*.cs`, `Configuration/*.cs`, e
`InfrastructureServiceCollectionExtensions.cs` (para lifetime/registro).

**Última verificação:** 2026-08-16 (Epic 26, Sprint 26.7) — seção Email atualizada com observabilidade
(logs de estado, classificação de falha, mascaramento de destinatário) e o throttle de registro;
Sprint 26.6 atualizou a mesma seção com a alternativa em texto plano e a correção da cor de marca;
Sprint 26.4 atualizou a mesma seção com `HmgRecipientGuardedEmailSender`; Sprint 26.2 atualizou a
mesma seção com `EmailProviderSelector`; verificação anterior em 2026-08-09 (Sprint 18.6) —
`Caching/MemoryApplicationCache.cs` removido (ver achado abaixo).

## Event Journal — `JsonEventJournal`

| | |
|---|---|
| Interface | `IEventJournal` (Application) |
| Lifetime | `AddSingleton<JsonEventJournal>()`, exposto via `IEventJournal` por uma factory que devolve o mesmo singleton |
| Quem registra | `InfrastructureServiceCollectionExtensions` |
| Quem consome | `AuditDomainEventHandler` (Application), via `IBackgroundTaskQueue` (fire-and-forget) |

Log de auditoria **append-only**, formato **NDJSON** (uma linha JSON por evento), independente de
qualquer persistência de estado da aplicação. `IEventJournal` é write-only — sem API de leitura
pública. Caminho resolvido a partir de `EventJournalOptions` (`Directory` padrão `"Data"`,
`FileName` padrão `"BeeDayEvents.ndjson"`), relativo à raiz de conteúdo do host se não absoluto.

Escrita protegida por `SemaphoreSlim(1, 1)` (não um `lock` comum — permite `await` dentro da seção
crítica). Antes de escrever, `ContainsAsync` varre o arquivo linha a linha (abrindo com
`FileShare.ReadWrite`, então não bloqueia outros leitores/escritores) para deduplicar — se um
evento com o mesmo `EventId` (ou, para `UserLeveledUpDomainEvent`, o mesmo `ExperienceEntryId`
dentro do payload) já existe, `AppendAsync` retorna sem escrever de novo. Linhas malformadas são
ignoradas silenciosamente durante a varredura ("preservar disponibilidade do journal", segundo o
comentário do código) — não interrompem a leitura do restante do arquivo.

Nenhuma rotação ou limite de tamanho — o arquivo cresce indefinidamente.

## Identity — 4 serviços

| Serviço | Interface | Lifetime | Responsabilidade |
|---|---|---|---|
| `SystemClock` | `IClock` | Singleton | `UtcNow => DateTimeOffset.UtcNow` — único membro, existe para tornar o tempo testável (injeção em vez de chamada estática) |
| `SecureUserTokenService` | `IUserTokenService` | Singleton | Gera token (32 bytes CSPRNG, Base64Url) e hash (SHA-256, hex maiúsculo) para tokens de e-mail/reset de senha |
| `MemoryIdentityRequestThrottle` | `IIdentityRequestThrottle` | Singleton | `ConcurrentDictionary<string, DateTimeOffset>` — throttle em memória (não distribuído), chave `"{operation}:{subject normalizado}"`, CAS lock-free via `TryAdd`/`TryUpdate` |
| `IdentityEmailComposer` | `IIdentityEmailComposer` | Singleton | Monta o HTML dos e-mails de confirmação/reset (template inline, tema escuro, `#7A4FCB`), usando `IdentityEmailOptions` para montar a URL |

**Achado sobre `MemoryIdentityRequestThrottle`:** por ser em memória e singleton por processo, o
throttle não sobrevive a um restart nem é compartilhado entre múltiplas instâncias da aplicação —
suficiente para um único processo, mas não é uma garantia distribuída.

**Cleanup do dicionário (Sprint 18.6):** até a Sprint 18.6, nenhuma entrada era jamais removida de
`_requests` — o dicionário crescia indefinidamente com cada par distinto `operação:assunto` já
solicitado. A correção usa expurgo oportunista e amortizado: um contador de chamadas
(`Interlocked.Increment`) dispara uma varredura completa do dicionário a cada 128 chamadas de
`TryAcquire` (`CleanupInterval`), removendo entradas cujo cooldown já expirou
(`entry.Value <= now`) via `ConcurrentDictionary.TryRemove(KeyValuePair)` — que só remove se o
valor não mudou desde a leitura, então uma renovação concorrente do mesmo cooldown nunca é
descartada por engano. Sem lock global, sem `BackgroundService`/`Timer` novo, sem alteração de
`IIdentityRequestThrottle`.

## Email — `ResendEmailSender` vs. `DevelopmentEmailSender` (mutuamente exclusivos)

| | `ResendEmailSender` | `DevelopmentEmailSender` |
|---|---|---|
| Interface | `IEmailSender` | `IEmailSender` |
| Lifetime | Typed `HttpClient` (`AddHttpClient<IEmailSender, ResendEmailSender>`) | Singleton |
| Registrado quando | `BeeDay:Email:Resend:Enabled = true` | Caso contrário (padrão) |
| Mecanismo | `POST https://api.resend.com/emails`, `Authorization: Bearer {ApiKey}`, `User-Agent: BeeDay/1.0`, `Idempotency-Key` novo por request, timeout 30s, campo `text` opcional (`EmailMessage.PlainTextBody`). Lança `HttpRequestException` em falha (não engolida) | Escreve 2 ou 3 arquivos por e-mail em `{ContentRoot}/{Directory}` (padrão `Data/Emails`): `{timestamp}-{hex}.html` (corpo), `.txt` opcional (texto plano, só se `PlainTextBody` presente) e `.json` (metadados: destinatário, assunto, `HtmlFile`, `PlainTextFile`, timestamp) |
| Proteção | — | Guarda contra path traversal — lança `InvalidOperationException` se o diretório resolvido escapar da raiz de conteúdo |

A escolha entre os dois acontece inteiramente em tempo de DI (`InfrastructureServiceCollectionExtensions`),
nunca em runtime por requisição.

**EPIC 26, Sprint 26.7 — observabilidade e classificação de falha:** `ResendEmailSender` agora
registra "attempted" (antes da chamada HTTP) e "accepted" (com o `id` da resposta do Resend, quando
disponível) — sucesso era completamente silencioso antes. Falhas são classificadas em 3 causas
distintas antes de propagar sem retry automático: erro de rede/conexão (`HttpRequestException` da
própria chamada), timeout do `HttpClient` (distinto de cancelamento pelo chamador, que propaga sem
log) e rejeição do provider (resposta HTTP não-2xx). `DevelopmentEmailSender` passou a mascarar o
destinatário em seus 2 logs (`EmailAddressLogMasking`, novo) — o arquivo `.json`/`.html` capturado em
disco continua com o endereço completo (necessário para diagnóstico local); só a linha de log é
mascarada. Ver [`06-transactional-email.md`](06-transactional-email.md) §14 para o modelo de estados
completo e a auditoria de controles de abuso (throttle agora também em `CreateAccountCommandHandler`/
`CreateUserCommandHandler`, reaproveitando `IIdentityRequestThrottle` sem criar mecanismo novo).

**EPIC 26, Sprint 26.6 — alternativa em texto plano:** `EmailMessage` ganhou um 4º membro posicional
opcional, `PlainTextBody` (padrão `null`, não quebra nenhum call site de 3 argumentos existente).
`IdentityEmailComposer` (`Identity/IdentityEmailComposer.cs`) agora produz esse texto para os dois
fluxos (`ComposeEmailConfirmation`/`ComposePasswordReset`), junto com a correção da cor de marca
usada no botão de call-to-action — de `#7A4FCB` (roxo pré-EPIC-25) para `#5247F9`, a única Brand
Color oficialmente aprovada (`docs/design-system/01-foundations.md` §2.2). Ver
[`06-transactional-email.md`](06-transactional-email.md) §13 para a análise completa, incluindo a
decisão documentada de não localizar o conteúdo dos e-mails (fronteira arquitetural preservada com
`docs/web/07-localization.md` §9).

**EPIC 26, Sprint 26.4 — `HmgRecipientGuardedEmailSender`:** whenever `EmailProviderSelector`
resolves `EmailProvider.Resend`, `IEmailSender` is registered as `HmgRecipientGuardedEmailSender`
wrapping `ResendEmailSender` — never the raw `ResendEmailSender` directly. The guard
(`Configuration/HmgRecipientGuardOptions.cs`) defaults to `Enabled=true` with an empty
`AllowedRecipients`, which fails `ValidateOnStart()` — an environment that switches to Resend
without configuring the guard refuses to start rather than sending unprotected. When `Enabled`, only
listed recipients reach `ResendEmailSender` (others are silently suppressed, subject gets the
`SubjectPrefix` — default `"[HMG] "` — prepended once); when explicitly `Enabled=false`
(`appsettings.Production.json`'s deliberate opt-out), every recipient passes through unmodified. The
`Development` branch of the same `if`/`else` never binds `HmgRecipientGuardOptions` at all. See
[`06-transactional-email.md`](06-transactional-email.md) §10 for the full contract.

**EPIC 26, Sprint 26.2 — `EmailProviderSelector`:** `ResendOptions.Enabled` e
`DevelopmentEmailOptions.Enabled` continuam sendo dois booleanos independentes (mesmas chaves de
configuração já implantadas, nenhuma renomeada), mas a escolha do `IEmailSender` efetivo agora passa
por `EmailProviderSelector.Resolve(resendEnabled, developmentEnabled)`
(`Configuration/EmailProviderSelector.cs`), uma função pura que devolve `EmailProvider.Resend` ou
`EmailProvider.Development` para as duas combinações não ambíguas, e lança
`InvalidOperationException` no próprio registro de DI (antes de `builder.Build()`, mesmo momento em
que `resendEnabled` já era lido hoje) para as duas combinações ambíguas: ambos `true` (a antiga
lógica silenciosamente ignorava `Development:Enabled` e escolhia Resend) e ambos `false` (a antiga
lógica registrava `DevelopmentEmailSender`, que então suprimia todo e-mail silenciosamente, só com
log `Information`). Nenhum arquivo `appsettings*.json` precisou mudar — nenhuma das 4 configurações
já commitadas (base, Homologation, Production; Development herda do base) está em um dos dois
estados ambíguos. Ver [`docs/infrastructure/06-transactional-email.md`](06-transactional-email.md)
§4.1/§8 para a análise completa que motivou esta mudança.

## `Pbkdf2PasswordService` — hashing de senha

| | |
|---|---|
| Interface | `IPasswordService` |
| Lifetime | Singleton |
| Algoritmo | PBKDF2-SHA256, 120.000 iterações, salt 16 bytes, hash 32 bytes |
| Formato armazenado | `"PBKDF2-SHA256$120000$<salt-base64>$<hash-base64>"` |
| Comparação | `CryptographicOperations.FixedTimeEquals` (tempo constante) |
| `NeedsRehash` | `true` se o algoritmo armazenado difere, ou a contagem de iterações armazenada é menor que a atual (120.000) — permite verificar hashes antigos com contagens mais fracas antes de forçar upgrade |

`Verify` rejeita graciosamente (retorna `false`, nunca lança) entradas nulas/vazias, formato
malformado, contagem de iterações fora de `1..1_000_000`, ou tamanho de salt/hash incorreto.

## Application Cache — removido na Sprint 18.6

`IApplicationCache`/`MemoryApplicationCache` (wrapper sobre `IMemoryCache`) e
`InvalidateDashboardCacheHandler` existiam desde antes da Sprint 18.6, mas nunca formavam um cache
funcional: `GetOrCreateAsync` (o único método que gravava algo) não tinha nenhum chamador de
produção em todo `src/` — apenas `Remove(CacheKeys.Dashboard)` era exercitado, pelo handler de
invalidação, disparado a cada Command bem-sucedido do sistema, sempre removendo uma chave que nunca
havia sido escrita. `GetDashboardQueryHandler` sempre consultou `IDashboardReadService.GetAsync`
diretamente contra o SQL Server, nunca este cache. Confirmado código morto por busca exaustiva e
removido integralmente (interface, implementação, handler de invalidação, `CacheKeys.Dashboard`,
`AddMemoryCache()`, registros de DI, fakes de teste). Não foi substituído por nenhum outro
mecanismo de cache.

## `SqlServerHealthCheck`

| | |
|---|---|
| Interface | `IHealthCheck` (ASP.NET Core) |
| Registro | `AddHealthChecks().AddCheck<SqlServerHealthCheck>("sql-server", tags: ["ready","storage","sql"])` |
| Classe | `internal` (não `public`) |
| Mecânica | Cria um `BeeDayDbContext` de vida curta via `IDbContextFactory` (nunca reaproveita um contexto de longa duração — mesmo motivo do `AddDbContextFactory` geral), chama `Database.CanConnectAsync()` |
| Resultado | `Healthy`/`Unhealthy` conforme o retorno booleano; qualquer exceção vira `Unhealthy("SQL Server is unavailable.", exception)` |

Ver `docs/architecture/08-deployment-architecture.md` para como os 3 endpoints de health check
(`/health/live`, `/health/ready`, `/health`) consomem este único check registrado.

## Background — `BackgroundTaskQueue` + `BackgroundTaskWorker`

| | |
|---|---|
| `BackgroundTaskQueue` | Singleton, exposta via `IBackgroundTaskQueue`. `Channel<Func<CancellationToken,ValueTask>>` limitado a 256 itens (`BoundedChannelFullMode.Wait` — produtores esperam se cheio, nunca descartam), 1 leitor/múltiplos escritores |
| `BackgroundTaskWorker` | `BackgroundService` (hosted service), registrado via `AddHostedService<BackgroundTaskWorker>()` |

`BackgroundTaskWorker.ExecuteAsync` faz um loop `DequeueAsync` → invoca o item. Se um item lançar
uma exceção que não seja `OperationCanceledException` de shutdown, o erro é **logado e o loop
continua** — uma falha em um item não derruba o worker nem impede itens subsequentes de rodar.
`DequeueAsync` é `internal` na classe concreta `BackgroundTaskQueue` (não exposta em
`IBackgroundTaskQueue`) — só `BackgroundTaskWorker`, no mesmo assembly, pode consumir a fila;
qualquer outro código só pode enfileirar (`QueueAsync`, via a interface pública).

**Único consumidor conhecido:** `AuditDomainEventHandler` (Application), que enfileira a escrita no
Event Journal como trabalho em background — mantém a escrita do journal fora do caminho crítico de
uma requisição.

## `InfrastructureEventIds` — removido na Sprint 18.3

`Diagnostics/InfrastructureEventIds.cs` definia 8 `EventId` estáticos (`DataFileCreated`,
`DataFileLoaded`, `DataFileSaved`, `DataFileInvalid`, `BackupCreated`, `BackupRemoved`,
`BackupInvalid`, `BackupRestored`) sem nenhuma referência além da própria declaração — vocabulário
típico do pipeline JSON removido (ADR-005). Confirmado código morto e removido nesta Sprint.

## Fontes de verdade

**Arquivos consultados:** `Auditing/JsonEventJournal.cs`, `Identity/SystemClock.cs`,
`SecureUserTokenService.cs`, `MemoryIdentityRequestThrottle.cs`, `IdentityEmailComposer.cs`,
`ResendEmailSender.cs`, `DevelopmentEmailSender.cs`, `Security/Pbkdf2PasswordService.cs`,
`HealthChecks/SqlServerHealthCheck.cs`,
`Background/BackgroundTaskQueue.cs`, `BackgroundTaskWorker.cs`,
`Configuration/EmailProvider.cs`, `Configuration/EmailProviderSelector.cs` (EPIC 26, Sprint 26.2),
os 5 arquivos de `Configuration/`,
`DependencyInjection/InfrastructureServiceCollectionExtensions.cs` (para lifetime de cada
registro).
**Testes consultados:** `tests/BeeDay.Infrastructure.Tests/JsonEventJournalTests.cs`
(`Repeated_event_id_is_written_only_once`, `Level_up_entry_contains_summary_and_structured_payload`),
`MemoryIdentityRequestThrottleTests.cs` (comportamento de cooldown, independência por chave,
concorrência, expurgo amortizado de entradas expiradas),
`IdentityInfrastructureTests.cs` (`ResendSender_WhenDisabled_DoesNotCallApi`,
`ResendSender_SendsExpectedAuthenticatedRequest`,
`ResendSender_WhenApiRejectsRequest_ThrowsWithoutExposingApiKey`), `Pbkdf2PasswordServiceTests.cs`.
**Contratos relacionados:** `docs/application/04-contracts.md` §"Outras interfaces (Common)".
**Documentação relacionada:** [`05-dependency-injection.md`](05-dependency-injection.md) (ordem e
lifetime exatos de registro), `docs/architecture/08-deployment-architecture.md` (health check
endpoints), `docs/domain/domain-events.md` (o que `AuditDomainEventHandler` consome).
