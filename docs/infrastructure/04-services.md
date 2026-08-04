# Infrastructure Services

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Infrastructure/Auditing/JsonEventJournal.cs`,
`Identity/*.cs`, `Security/Pbkdf2PasswordService.cs`, `Caching/MemoryApplicationCache.cs`,
`HealthChecks/SqlServerHealthCheck.cs`, `Background/*.cs`, `Configuration/*.cs`, e
`InfrastructureServiceCollectionExtensions.cs` (para lifetime/registro).

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

## Email — `ResendEmailSender` vs. `DevelopmentEmailSender` (mutuamente exclusivos)

| | `ResendEmailSender` | `DevelopmentEmailSender` |
|---|---|---|
| Interface | `IEmailSender` | `IEmailSender` |
| Lifetime | Typed `HttpClient` (`AddHttpClient<IEmailSender, ResendEmailSender>`) | Singleton |
| Registrado quando | `BeeDay:Email:Resend:Enabled = true` | Caso contrário (padrão) |
| Mecanismo | `POST https://api.resend.com/emails`, `Authorization: Bearer {ApiKey}`, `User-Agent: BeeDay/1.0`, `Idempotency-Key` novo por request, timeout 30s. Lança `HttpRequestException` em falha (não engolida) | Escreve 2 arquivos por e-mail em `{ContentRoot}/{Directory}` (padrão `Data/Emails`): `{timestamp}-{hex}.html` (corpo) e `.json` (metadados: destinatário, assunto, arquivo, timestamp) |
| Proteção | — | Guarda contra path traversal — lança `InvalidOperationException` se o diretório resolvido escapar da raiz de conteúdo |

A escolha entre os dois acontece inteiramente em tempo de DI (`InfrastructureServiceCollectionExtensions`),
nunca em runtime por requisição.

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

## `MemoryApplicationCache`

| | |
|---|---|
| Interface | `IApplicationCache` |
| Lifetime | Singleton, sobre `IMemoryCache` (também registrado via `AddMemoryCache()`) |
| `GetOrCreateAsync` | Implementação manual (não usa a extensão nativa do `IMemoryCache`): `TryGetValue`, se hit retorna direto; senão invoca a `factory` e armazena com `Set(key, value, duration)` |
| `Remove` | Delegação direta a `memoryCache.Remove(key)` |

**Achado:** `GetOrCreateAsync` não tem nenhuma trava contra corrida em cache-miss concorrente —
duas chamadas simultâneas para a mesma chave ausente podem ambas invocar `factory` (não há
semáforo por chave nem `Lazy<T>`). Consumidor conhecido:
`InvalidateDashboardCacheHandler`/dashboard (`docs/application/04-contracts.md`) — impacto
provável baixo (recomputação redundante, não corrupção de dado), mas não avaliado a fundo nesta
Sprint (fora do escopo — apenas documentar).

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

## `InfrastructureEventIds` — achado: definido, nunca usado

`Diagnostics/InfrastructureEventIds.cs` define 8 `EventId` estáticos (`DataFileCreated=6001`,
`DataFileLoaded=6002`, `DataFileSaved=6003`, `DataFileInvalid=6004`, `BackupCreated=6010`,
`BackupRemoved=6011`, `BackupInvalid=6012`, `BackupRestored=6013`). Busca em todo `src/`/`tests/`
não encontra nenhuma referência além da própria declaração — nenhuma classe de logging estruturado
os utiliza hoje. O vocabulário ("DataFile", "Backup") é típico do pipeline JSON removido
(ADR-005), reforçando que são resíduos não removidos junto com o resto daquele pipeline.

## Fontes de verdade

**Arquivos consultados:** `Auditing/JsonEventJournal.cs`, `Identity/SystemClock.cs`,
`SecureUserTokenService.cs`, `MemoryIdentityRequestThrottle.cs`, `IdentityEmailComposer.cs`,
`ResendEmailSender.cs`, `DevelopmentEmailSender.cs`, `Security/Pbkdf2PasswordService.cs`,
`Caching/MemoryApplicationCache.cs`, `HealthChecks/SqlServerHealthCheck.cs`,
`Background/BackgroundTaskQueue.cs`, `BackgroundTaskWorker.cs`,
`Diagnostics/InfrastructureEventIds.cs`, os 5 arquivos de `Configuration/`,
`DependencyInjection/InfrastructureServiceCollectionExtensions.cs` (para lifetime de cada
registro).
**Testes consultados:** `tests/BeeDay.Infrastructure.Tests/JsonEventJournalTests.cs`
(`Repeated_event_id_is_written_only_once`, `Level_up_entry_contains_summary_and_structured_payload`),
`IdentityInfrastructureTests.cs` (`ResendSender_WhenDisabled_DoesNotCallApi`,
`ResendSender_SendsExpectedAuthenticatedRequest`,
`ResendSender_WhenApiRejectsRequest_ThrowsWithoutExposingApiKey`), `Pbkdf2PasswordServiceTests.cs`.
**Contratos relacionados:** `docs/application/04-contracts.md` §"Outras interfaces (Common)".
**Documentação relacionada:** [`05-dependency-injection.md`](05-dependency-injection.md) (ordem e
lifetime exatos de registro), `docs/architecture/08-deployment-architecture.md` (health check
endpoints), `docs/domain/domain-events.md` (o que `AuditDomainEventHandler` consome).
