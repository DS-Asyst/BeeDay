# Infrastructure

Documentação do `BeeDay.Infrastructure` — reconstruída por completo na Sprint 16.6 a partir
exclusivamente do código atual (`src/BeeDay.Infrastructure/`, `tests/BeeDay.Infrastructure.Tests/`,
`src/BeeDay.Application/`, `src/BeeDay.Domain/`). Nenhuma afirmação vem de `docs/history/` ou de
sprints anteriores sem reverificação direta no código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes de verdade".

## Responsabilidade

`BeeDay.Infrastructure` é a única camada que conhece tecnologia concreta de persistência (EF Core/
SQL Server) e de integração externa (Resend, sistema de arquivos para e-mails de desenvolvimento e
Event Journal, `IMemoryCache`). Implementa toda interface definida em `BeeDay.Application` que
precisa de uma tecnologia real por trás — repositórios, read services, hashing de senha, envio de
e-mail, relógio, cache, fila de background, journal de auditoria, health check.

## Organização

```text
src/BeeDay.Infrastructure/
├── Auditing/            JsonEventJournal (log de domain events, append-only, NDJSON)
├── Background/           BackgroundTaskQueue + BackgroundTaskWorker (fila + worker)
├── Caching/               MemoryApplicationCache (IMemoryCache)
├── Configuration/          5 classes Options (SqlServer, IdentityEmail, Resend, DevelopmentEmail, EventJournal)
├── DependencyInjection/     InfrastructureServiceCollectionExtensions — único ponto de registro
├── Diagnostics/             (vazio — InfrastructureEventIds removido na Sprint 18.3, era código morto)
├── HealthChecks/            SqlServerHealthCheck
├── Identity/                 SystemClock, SecureUserTokenService, MemoryIdentityRequestThrottle,
│                              IdentityEmailComposer, ResendEmailSender, DevelopmentEmailSender
├── Persistence/
│   ├── Exceptions/           PersistenceException + ConcurrencyConflictException (ambos ativos)
│   └── SqlServer/            BeeDayDbContext, Configurations/, Migrations/, Repositories/,
│                               EfUnitOfWork, EfDashboardReadService, EfWalletReadService,
│                               EfConcurrencySaveChanges
└── Security/                  Pbkdf2PasswordService
```

## Integração com Application

Toda dependência é por interface — Infrastructure implementa 18 interfaces definidas em
`BeeDay.Application` (8 repositórios, `IUnitOfWork`, 2 read services, `IPasswordService`, `IClock`,
`IUserTokenService`, `IIdentityRequestThrottle`, `IIdentityEmailComposer`, `IEmailSender`,
`IEventJournal`, `IApplicationCache`, `IBackgroundTaskQueue`). Confirmado por teste real
(`PersistenceContractBoundaryTests.ApplicationAssembly_DoesNotReferenceInfrastructure`, em
`tests/BeeDay.Application.Tests/`): a dependência é sempre `Infrastructure → Application`, nunca o
inverso.

## Integração com Web

`BeeDay.Web` referencia `BeeDay.Infrastructure` apenas para chamar
`AddBeeDayInfrastructure(configuration)` em `Program.cs` (registro de DI) — nenhum componente
Blazor referencia um tipo concreto de Infrastructure. Única exceção observada:
`ICurrentUserContext` é implementada em Web (`HttpCurrentUserContext`), não em Infrastructure —
documentado em `docs/architecture/07-security-architecture.md`, fora do escopo desta Sprint.

## Integração com SQL Server

Única via de acesso a dados: `BeeDayDbContext` (EF Core), registrado como
`IDbContextFactory<BeeDayDbContext>` (não `AddDbContext`) — decisão deliberada para não manter um
`DbContext` vivo pela duração de um circuito Blazor Server. Ver [`02-sql-server.md`](02-sql-server.md).

## Serviços externos

| Serviço | Uso | Condicional |
|---|---|---|
| Resend (API HTTP) | Envio de e-mail transacional em produção | Só registrado se `BeeDay:Email:Resend:Enabled=true` |
| Sistema de arquivos local | E-mails de desenvolvimento (HTML+JSON por mensagem), Event Journal (NDJSON) | Sempre, mas paths configuráveis |
| SQL Server (LocalDB ou real) | Único provider de persistência | Sempre — startup falha sem connection string válida |

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-repositories.md`](01-repositories.md) | Os 8 repositórios, `EfUnitOfWork`, 2 read services — mecânica interna de Add/Update/Remove/Reorder |
| [`02-sql-server.md`](02-sql-server.md) | Connection string, `SqlServerOptions`, migrations, ciclo de vida do banco, startup |
| [`03-concurrency.md`](03-concurrency.md) | RowVersion, `DbUpdateConcurrencyException`, tradução de exceções, fluxo completo |
| [`04-services.md`](04-services.md) | Event Journal, Identity, hashing de senha, e-mail, cache, health check, background |
| [`05-dependency-injection.md`](05-dependency-injection.md) | Os 32 registros de `InfrastructureServiceCollectionExtensions`, lifetimes, `IDbContextFactory` |

Para o mapeamento objeto-relacional em si (DbSets, Configurations, TPC, Owned/Complex Type,
migration strategy), ver [`docs/persistence/`](../persistence/README.md) — reconstruído nesta
mesma Sprint.

## Ordem de leitura recomendada

1. `05-dependency-injection.md` — visão geral de tudo que existe, via o que é registrado.
2. `docs/persistence/` — o modelo de dados.
3. `01-repositories.md` — como o modelo é persistido/consultado na prática.
4. `02-sql-server.md` e `03-concurrency.md` — o que acontece por baixo de cada `SaveChangesAsync`.
5. `04-services.md` — os serviços de suporte (e-mail, cache, background, auditoria).

## Achados relevantes (reportados, não corrigidos)

- Comentários de código em `EfConcurrencySaveChanges.cs` e `EventJournalOptions.cs` ainda
  mencionam "o provider JSON" como referência histórica — comentários, não comportamento; fora do
  escopo alterar (código).

`Diagnostics/InfrastructureEventIds.cs` e os 3 subtipos mortos de
`Persistence/Exceptions/` (`BackupRestoreException.cs`, `DataFileCorruptedException.cs`,
`PersistenceAccessException.cs`) foram removidos na Sprint 18.3 — eram código morto comprovado,
vocabulário residual do pipeline JSON removido pela ADR-005.

Ver cada documento individual para achados adicionais específicos de sua área.
