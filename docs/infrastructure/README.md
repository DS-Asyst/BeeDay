# Infrastructure

Documentação do `BeeDay.Infrastructure` — reconstruída por completo na Sprint 16.6 a partir
exclusivamente do código atual (`src/BeeDay.Infrastructure/`, `tests/BeeDay.Infrastructure.Tests/`,
`src/BeeDay.Application/`, `src/BeeDay.Domain/`). Nenhuma afirmação vem de `docs/history/` ou de
sprints anteriores sem reverificação direta no código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes de verdade".

**Última verificação:** 2026-08-16 (Epic 26, Sprint 26.1) — `06-transactional-email.md` adicionado
(auditoria completa da arquitetura de e-mail transacional e causa raiz comprovada do diretório
`C:\Apps\BeeDay-Data\Emails` vazio em HMG); verificação anterior em 2026-08-09 (Sprint 18.6) —
`Caching/MemoryApplicationCache.cs` removido (código morto comprovado: seu único cache nunca era
populado em produção; ver `04-services.md`).

## Responsabilidade

`BeeDay.Infrastructure` é a única camada que conhece tecnologia concreta de persistência (EF Core/
SQL Server) e de integração externa (Resend, sistema de arquivos para e-mails de desenvolvimento e
Event Journal). Implementa toda interface definida em `BeeDay.Application` que precisa de uma
tecnologia real por trás — repositórios, read services, hashing de senha, envio de e-mail, relógio,
fila de background, journal de auditoria, health check.

## Organização

```text
src/BeeDay.Infrastructure/
├── Auditing/            JsonEventJournal (log de domain events, append-only, NDJSON)
├── Background/           BackgroundTaskQueue + BackgroundTaskWorker (fila + worker)
├── Configuration/          6 classes Options (SqlServer, IdentityEmail, Resend, DevelopmentEmail, EventJournal,
│                           HmgRecipientGuard) + EmailProvider/EmailProviderSelector (seleção de provider, não Options)
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

Toda dependência é por interface — Infrastructure implementa 17 interfaces definidas em
`BeeDay.Application` (8 repositórios, `IUnitOfWork`, 2 read services, `IPasswordService`, `IClock`,
`IUserTokenService`, `IIdentityRequestThrottle`, `IIdentityEmailComposer`, `IEmailSender`,
`IEventJournal`, `IBackgroundTaskQueue`). Confirmado por teste real
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
| [`04-services.md`](04-services.md) | Event Journal, Identity, hashing de senha, e-mail, health check, background |
| [`05-dependency-injection.md`](05-dependency-injection.md) | Os 29 registros de `InfrastructureServiceCollectionExtensions`, lifetimes, `IDbContextFactory` |
| [`06-transactional-email.md`](06-transactional-email.md) | EPIC 26, Sprint 26.1 — mapa completo da arquitetura de e-mail transacional (fluxos, seleção de provider, precedência de configuração), causa raiz comprovada do diretório de e-mail de desenvolvimento vazio em HMG, gaps/riscos e arquitetura-alvo recomendada para as Sprints 26.2+ |

Para o mapeamento objeto-relacional em si (DbSets, Configurations, TPC, Owned/Complex Type,
migration strategy), ver [`docs/persistence/`](../persistence/README.md) — reconstruído nesta
mesma Sprint.

## Ordem de leitura recomendada

1. `05-dependency-injection.md` — visão geral de tudo que existe, via o que é registrado.
2. `docs/persistence/` — o modelo de dados.
3. `01-repositories.md` — como o modelo é persistido/consultado na prática.
4. `02-sql-server.md` e `03-concurrency.md` — o que acontece por baixo de cada `SaveChangesAsync`.
5. `04-services.md` — os serviços de suporte (e-mail, background, auditoria).

## Achados relevantes (reportados, não corrigidos)

- **EPIC 26, Sprint 26.1 (ainda não corrigido nas Sprints 26.2–26.4):** `DevelopmentEmailSender`
  recusa gravar em qualquer diretório fora da content root do host; `appsettings.Homologation.json`
  configura `Email:Development:Directory` como um caminho absoluto externo
  (`C:\Apps\BeeDay-Data\Emails`, fora de `C:\Apps\BeeDay.Web`) — toda chamada de `SendAsync` em HMG
  lança `InvalidOperationException` antes de gravar qualquer arquivo. A Sprint 26.2 endereçou a
  seleção de provider (`EmailProviderSelector`) e a Sprint 26.3 documentou o contrato de secrets —
  nenhuma das duas tocou este guard específico. A Sprint 26.4 adicionou a guarda de destinatário de
  HMG (`HmgRecipientGuardedEmailSender`), um problema relacionado mas distinto. Ver
  [`06-transactional-email.md`](06-transactional-email.md) §6/§8 para a análise completa de causa
  raiz e o status atualizado da correção planejada.
- Comentários de código em `EfConcurrencySaveChanges.cs` e `EventJournalOptions.cs` ainda
  mencionam "o provider JSON" como referência histórica — comentários, não comportamento; fora do
  escopo alterar (código).

`Diagnostics/InfrastructureEventIds.cs` e os 3 subtipos mortos de
`Persistence/Exceptions/` (`BackupRestoreException.cs`, `DataFileCorruptedException.cs`,
`PersistenceAccessException.cs`) foram removidos na Sprint 18.3 — eram código morto comprovado,
vocabulário residual do pipeline JSON removido pela ADR-005.

Ver cada documento individual para achados adicionais específicos de sua área.
