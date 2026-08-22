# Contracts

**Fonte da verdade:** verificado diretamente nos 8 arquivos `I*Repository.cs` e `IUnitOfWork.cs` em
`src/BeeDay.Application/Common/Contracts/`, `IDashboardReadService.cs`, `IWalletReadService.cs`, e
grep de implementações em `src/BeeDay.Infrastructure`/`src/BeeDay.Web`.

## O que "Contract" significa neste código

Diferente do que a documentação antiga (removida nesta Sprint) descrevia, não existe um projeto
`BeeDay.Contracts` — "contrato" aqui significa **interface definida em Application, implementada
em outra camada**. Há 3 categorias: repositórios de Aggregate (8), read services (2), e
`IUnitOfWork` (1).

## Repositórios por Aggregate (8)

Um por Aggregate Root (ver `docs/domain/README.md` §Aggregate Roots) — nenhum repositório
genérico existe, confirmado por teste real:
`PersistenceContractBoundaryTests.PersistenceContracts_ContainNoGenericRepositoryOrUnapprovedUnitOfWorkAbstraction`
(`tests/BeeDay.Application.Tests/`) reflete sobre todas as interfaces em
`BeeDay.Application.Common.Contracts` e falha se alguma for `IsGenericTypeDefinition`.

| Interface | Implementação (Infrastructure) | Quem consome (Handlers, exemplos) |
|---|---|---|
| `IUserRepository` | `EfUserRepository` | `CreateUserCommandHandler`, `AuthenticateUserCommandHandler`, `ConfirmEmailCommandHandler` |
| `IUserTokenRepository` | `EfUserTokenRepository` | `IdentityHandlers.cs` (confirmação de e-mail, reset de senha) |
| `IHabitRepository` | `EfHabitRepository` | `HabitCommandHandlers.cs` |
| `IRecurringTaskRepository` | `EfRecurringTaskRepository` | `TaskCommandHandlers.cs` |
| `IProjectRepository` | `EfProjectRepository` | `ProjectCommandHandlers.cs`, `TodoCommandHandlers.cs` (Todo só é alcançável por aqui) |
| `IWalletRepository` | `EfWalletRepository` | `WalletCommandHandlers.cs` |
| `IWalletTagRepository` | `EfWalletTagRepository` | `WalletCommandHandlers.cs` |
| `ITransactionRepository` | `EfTransactionRepository` | `WalletCommandHandlers.cs` |

### Padrão de método comum

Todo repositório expõe `GetAsync`/`ListAsync` (leitura com tracking, para mutação), `AddAsync`,
`RemoveAsync` (quando aplicável), e um `UpdateAsync(userId, entityId, Action<TEntity> mutation, ct)`
— este último é o padrão dominante: carrega a entidade rastreada, aplica a mutação (lambda de
Domain puro) e persiste. O XML doc de `IUserRepository.UpdateAsync` explica o motivo: "`mutation`
é lógica de Domain pura; o adapter nunca expõe nenhum conceito de Infrastructure/EF Core (ex.
RowVersion) aqui" — o padrão existe para que Application nunca precise saber como a concorrência
otimista é implementada.

### Particularidades por repositório

- **`IProjectRepository`** é o maior (12 métodos) — inclui `AddTodoAsync`, `UpdateTodoAsync`,
  `RemoveTodoAsync`, `MoveTodoAsync`, `GetByTodoIdAsync`, porque **não existe `ITodoRepository`**.
  O XML doc é explícito: "Não existe deliberadamente um `ITodoRepository`... Todo só é alcançável
  através desta porta."
- **`IWalletRepository`** é o menor (3 métodos: `GetByUserAsync`, `AddAsync`, `UpdateAsync`) —
  XML doc: "deliberadamente magra — Wallet não possui entidades filhas e não armazena saldo; saldo
  é sempre calculado a partir de Transactions fornecidas externamente."
- **`ITransactionRepository.ClearTagReferencesAsync(walletTagId)`** existe especificamente para
  `DeleteWalletTagCommandHandler` desvincular transações antes de remover uma tag.
- **`IUserTokenRepository.RevokeActiveAsync(userId, type, revokedAtUtc)`** garante que nunca haja
  dois tokens do mesmo tipo simultaneamente ativos para o mesmo usuário — chamado antes de emitir
  um novo token.

## Read Services (2)

Diferente dos repositórios (que servem escrita + leitura rastreada), read services existem
exclusivamente para projeções de leitura que cruzam múltiplos Aggregates ou exigem
filtro/ordenação/paginação — carregar cada Aggregate Root inteiro só para achatar em uma view seria
o desperdício que essas interfaces evitam (justificativa no próprio XML doc de
`IDashboardReadService`).

| Interface | Implementação | Método(s) | Por que não é um repositório |
|---|---|---|---|
| `IDashboardReadService` (`Features/Dashboard/Contracts/`) | `EfDashboardReadService` | `GetAsync(userId, ct) → DashboardResponse` | Única tela que precisa de uma fatia de User + Habit + RecurringTask + Project/Todo + Wallet ao mesmo tempo |
| `IWalletReadService` (`Features/Wallets/Contracts/`) | `EfWalletReadService` | `GetSummaryAsync`, `ListTagsAsync`, `GetTransactionAsync`, `ListTransactionsAsync(filter)` | Listagem de transações filtrada/ordenada/paginada — deliberadamente fora de `ITransactionRepository` |

**Achado de nomenclatura (documentado, não corrigido):** `IWalletReadService.TransactionQueryFilter`
é um record **distinto** de `Features.Wallets.Queries.GetTransactionsQuery` (o Command MediatR) —
o próprio código comenta que isso é intencional ("os dois têm permissão de divergir... hoje
carregam os mesmos campos por coincidência"), mas é uma duplicação estrutural real: os mesmos 11
campos de filtro existem em dois tipos diferentes.

## `IUnitOfWork`

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    public IUserRepository Users { get; }
    public IUserTokenRepository UserTokens { get; }
    public IHabitRepository Habits { get; }
    public IRecurringTaskRepository RecurringTasks { get; }
    public IProjectRepository Projects { get; }
    public IWalletRepository Wallets { get; }
    public IWalletTagRepository WalletTags { get; }
    public ITransactionRepository Transactions { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

Implementado por `EfUnitOfWork` (Infrastructure). É a **única** abstração permitida a ter
"UnitOfWork" no nome — confirmado por teste real
(`PersistenceContractBoundaryTests.PersistenceContracts_ContainNoGenericRepositoryOrUnapprovedUnitOfWorkAbstraction`,
que explicitamente pula `IUnitOfWork` da checagem e rejeitaria qualquer outra interface cujo nome
contenha "UnitOfWork").

`SaveChangesAsync` só é necessário quando uma mutação foi feita diretamente sobre uma entidade
ainda rastreada de uma chamada anterior no mesmo `IUnitOfWork` — a maioria dos métodos de escrita
dos repositórios já chama isso internamente. `BeginTransactionAsync` só é necessária quando duas ou
mais chamadas de `SaveChangesAsync` precisam ser atômicas juntas — uma única escrita já é atômica
por si só.

**Quem usa `IUnitOfWork` em vez de um repositório isolado:** é o padrão comum, não a exceção — 13
Handlers em 6 Features injetam `IUnitOfWork` inteiro (a maioria com `BeginTransactionAsync`/
`CommitTransactionAsync` explícitos coordenando 2+ Aggregates): `RegisterHabitPositiveCommandHandler`
(Habits), `ToggleTaskCommandHandler` (Tasks), `UpdateTodoCommandHandler`/`ToggleTodoCommandHandler`
(Todos), `CreateTransactionCommandHandler`/`UpdateTransactionCommandHandler`/
`DeleteTransactionCommandHandler`/`DeleteWalletTagCommandHandler` (Wallets),
`ConfirmEmailCommandHandler`/`ResetPasswordCommandHandler` (Identity), e
`CreateUserCommandHandler`/`CreateAccountCommandHandler`/`UpdateCurrentUserAccountCommandHandler`
(Users). Handlers que só precisam de uma única escrita atômica (ex.:
`CreateHabitCommandHandler`, `CreateProjectCommandHandler`, `CreateWalletTagCommandHandler`)
continuam injetando diretamente o repositório específico, sem `IUnitOfWork`.

## Requests e Responses

Não são "contratos" no sentido de interface — são `sealed record` simples, sem comportamento,
usados como payload de entrada/saída dos Commands/Queries. Ver
[`02-use-cases.md`](02-use-cases.md) para o catálogo completo por Feature. Nenhum Request/Response
é reaproveitado entre Features diferentes; cada Feature define os seus.

## Fontes de verdade

**Arquivos consultados:** os 8 `I*Repository.cs`, `IUnitOfWork.cs`, `IDashboardReadService.cs`,
`IWalletReadService.cs` (lidos integralmente, incluindo XML docs), mais os `Ef*`
correspondentes em `src/BeeDay.Infrastructure` (confirmados por grep de assinatura de classe, não
lidos integralmente nesta Sprint — implementação de Infrastructure já documentada em
`docs/architecture/06-persistence-architecture.md`).
**Testes consultados:**
`tests/BeeDay.Application.Tests/PersistenceContractBoundaryTests.cs` (as 3 asserções
arquiteturais, lidas integralmente).
**Features relacionadas:** todas.
**Documentação relacionada:** `docs/architecture/06-persistence-architecture.md` (implementação
EF Core dos mesmos contratos), `docs/domain/relationships.md` (fronteiras de agregado que motivam
o desenho de cada repositório).
