# Persistence Contracts (Sprint 13.3 — adoção parcial, Sprints 13.4–13.6)

**Status:** contratos definidos na Sprint 13.3. Adoção parcial: os dois read services (`IWalletReadService`,
`IDashboardReadService`) estão totalmente adotados; as 8 portas de escrita por Aggregate têm um adapter
EF Core e registro em DI (Sprint 14.4), coordenáveis via `IUnitOfWork`/`EfUnitOfWork` (Sprint 14.5), mas
ainda nenhum handler consumidor de nenhum dos dois. `ILevelUpRepository` continua sendo o contrato
efetivamente usado por todos os handlers de escrita e por 2 dos 4 fluxos de leitura. **Ver
[`08-migration-status.md`](migration-status.md) para o inventário completo e verificado — este
cabeçalho é um resumo, aquele documento é a referência precisa.**
**Escopo:** interfaces (portas) e formas de resposta derivadas exclusivamente do
[Aggregate Map (13.1)](domain-aggregate-map.md) e do
[Persistence Map (13.2)](domain-persistence-map.md), ambos tratados como arquitetura aprovada e não
reavaliados.

## 0. Decisão de escopo desta Sprint

O objetivo declarado da Sprint ("Application depender apenas dos contratos") e a restrição declarada
("não implementar Infrastructure nem JSON") não podem ser satisfeitos simultaneamente sem quebrar a
aplicação: religar os handlers a interfaces sem nenhuma implementação concreta derruba a resolução de
DI em runtime para toda a superfície que hoje depende de persistência — a maioria dos 665 testes
automatizados (`Application.Tests`, `Web.Tests` de integração, `E2E.Tests`) sobe a aplicação real via
container de DI.

Decisão explícita, validada com o usuário antes de qualquer alteração: **esta Sprint define os
contratos e não religa nenhum handler**. `ILevelUpRepository` continua sendo o contrato efetivamente
consumido pela Application em produção; os novos contratos abaixo existem, compilam, e não têm nenhum
consumidor ainda. A adoção — trocar cada handler para depender do contrato por agregado, o que só faz
sentido quando um adapter concreto existir — é trabalho de uma Sprint futura. Isso é consistente com
`docs/README.md` §4, que já separa "criar contratos e testes de contrato" (passo 2) de "adaptar a
implementação JSON aos novos contratos" (passo 3) como etapas distintas.

## 1. Arquitetura anterior

```text
Application (todo handler)
    ↓
ILevelUpRepository { LoadAsync, SaveAsync, UpdateAsync(Action<LevelUpData>) }
    ↓
LevelUpData (documento inteiro — todos os Aggregates, em memória, de uma vez)
```

Um único contrato para toda a Application, operando sobre o documento inteiro. Isso é exatamente o
bloqueador já registrado em `01-current-state.md` §3.6 e reconfirmado nas Sprints 13.1/13.2: qualquer
handler que precisa de `Habit` recebe acesso irrestrito a `Users`, `Projects`, `Wallets`, etc. — o
contrato não expressa ownership nem limites de consistência, só "o documento".

## 2. Nova arquitetura de contratos

```text
Application (handler específico do caso de uso)
    ↓
Persistence Contract (uma porta por Aggregate Root, ou um read service por
    necessidade real de projeção cross-aggregate)
    ↓
Infrastructure Adapter (não existe ainda — Sprint futura)
```

Cada porta é dimensionada para exatamente um Aggregate Root do Aggregate Map (13.1) — nunca para uma
tabela, nunca para "todas as entidades". Nenhuma porta conhece `LevelUpData`, JSON, arquivo, SQL Server
ou EF Core; todas expõem apenas tipos de `LevelUp.Domain` (para as portas de escrita) ou tipos de
resposta já existentes/novos do próprio `LevelUp.Application` (para os read services).

## 3. Contratos criados

### 3.1 Portas por Aggregate (escrita)

Todas em `src/LevelUp.Application/Common/Contracts/` — mesma pasta onde `ILevelUpRepository` já vivia,
mantendo a convenção existente de que esse diretório concentra os contratos de persistência
transversais a mais de uma feature.

| Contrato | Aggregate | Operações | Justificativa |
|---|---|---|---|
| `IUserRepository` | `User` | `GetByIdAsync`, `GetByEmailAsync`, `IsEmailInUseAsync`, `IsNicknameInUseAsync`, `AddAsync` | Cobre os 10 handlers que hoje leem/gravam `User` diretamente (Identity, Users, Authentication). `IsEmailInUseAsync`/`IsNicknameInUseAsync` existem porque a unicidade é uma invariante cross-instância (ver Persistence Map §2.1) que o próprio agregado não consegue proteger sozinho. |
| `IUserTokenRepository` | `UserToken` | `GetByHashAsync`, `ListActiveAsync`, `AddAsync` | Separado de `IUserRepository` porque `UserToken` é um Aggregate Root independente (Aggregate Map §2.2) — revogação em lote e consumo de token nunca precisam carregar `User` para validar o próprio estado do token. |
| `IHabitRepository` | `Habit` | `GetAsync`, `ListAsync`, `AddAsync`, `RemoveAsync`, `ReorderAsync` | `Habit` é Aggregate Root independente. |
| `IRecurringTaskRepository` | `RecurringTask` | `GetAsync`, `ListAsync`, `AddAsync`, `RemoveAsync`, `ReorderAsync` | Mesma forma de `IHabitRepository`, mas **deliberadamente uma interface separada** — ver §4 sobre por que não foi combinada com Habit apesar da forma idêntica. |
| `IProjectRepository` | `Project` (+ `Todo` como filha) | `GetAsync`, `ListAsync`, `GetByTodoIdAsync`, `AddAsync`, `RemoveAsync`, `ReorderAsync`, `ReorderTodosAsync` | Único agregado com entidade filha real (Aggregate Map §2.5); todas as operações sobre `Todo` passam por aqui — não existe porta própria para Todo. `GetByTodoIdAsync` existe porque `ToggleTodoCommandHandler`/`UpdateTodoCommandHandler`/`DeleteTodoCommandHandler` hoje localizam um Todo só pelo próprio id, sem já saber a qual Project ele pertence. |
| `IWalletRepository` | `Wallet` | `GetByUserAsync`, `AddAsync` | Deliberadamente magra — `Wallet` não tem saldo armazenado nem entidade filha (Persistence Map §2.6). |
| `ITransactionRepository` | `Transaction` | `GetAsync`, `ListByTagAsync`, `AddAsync`, `RemoveAsync` | `Transaction` já é independente de `Wallet` no próprio Domain hoje (sem containment). `ListByTagAsync` existe para suportar a limpeza de referências quando uma `WalletTag` é excluída. Listagem filtrada/paginada/ordenada para exibição **não está aqui** — ver `IWalletReadService`. |
| `IWalletTagRepository` | `WalletTag` | `GetAsync`, `ListAsync`, `IsNameInUseAsync`, `AddAsync`, `RemoveAsync` | `WalletTag` pertence a `UserId`, não a `WalletId` (achado do Aggregate Map §2.8, ver §5 desta Sprint sobre a divergência com `01-relational-model.md`). |

Nenhuma interface genérica (`IRepository<T>`, `CrudRepository`, `BaseRepository`) foi criada. Nenhuma
porta expõe `Save`/`Load`/`Update` genéricos — cada operação de escrita é nomeada pelo que
efetivamente representa no domínio (`AddAsync`, `RemoveAsync`, `ReorderAsync`), e a persistência de uma
mutação sobre uma entidade já carregada (ex.: `user.SetPasswordHash(...)`) é deliberadamente deixada
para um Unit of Work — ver §6.

### 3.2 Read services (leitura projetada)

Dois, ambos feature-scoped (não em `Common/Contracts`, já que — diferente das portas por agregado —
nada fora da própria feature os consome):

| Contrato | Local | Aggregates cobertos | Justificativa |
|---|---|---|---|
| `IDashboardReadService` | `Features/Dashboard/Contracts/` | `User`, `Habit`, `RecurringTask`, `Project`+`Todo`, `Wallet` | Substitui `GetLevelUpResponse(LevelUpData Data)` — o bloqueador citado em `01-current-state.md` §3.6. É a única tela que genuinamente precisa de uma fatia de cada Aggregate ao mesmo tempo; carregar cada Aggregate Root inteiro só para achatar em uma resposta de exibição é exatamente o que `02-target-architecture.md` §4 pede para os read services evitarem. |
| `IWalletReadService` | `Features/Wallets/Contracts/` | `Wallet`, `Transaction`, `WalletTag` | Cobre `GetWalletSummaryQueryHandler`, `GetWalletTagsQueryHandler` (contagem de transações por tag), `GetTransactionByIdQueryHandler`, `GetTransactionsQueryHandler` (busca/filtro/ordenação/paginação) — todos leitura pura, hoje implementados carregando o documento inteiro e filtrando em memória. |

`DashboardResponse` (novo, em `Features/Dashboard/Responses/DashboardResponse.cs`) substitui
`GetLevelUpResponse` como alvo — não expõe `LevelUpData` nem entidades mutáveis, apenas projeções
(`UserProfileSummary`, `HabitSummary`, `TaskSummary`, `ProjectSummary`, `TodoSummary`). `IWalletReadService`
reaproveita os tipos de resposta que já existiam e já eram limpos (`WalletSummaryResponse`,
`WalletTagResponse`, `TransactionResponse`, `PagedTransactionsResponse` em
`Features/Wallets/Responses/WalletResponses.cs`) — nenhum tipo novo foi necessário ali além do filtro
de consulta (`TransactionQueryFilter`).

## 4. Decisões de "não criar automaticamente"

Por exigência explícita desta Sprint, cada porta acima só foi criada depois de responder três
perguntas. As decisões que divergem da forma mais óbvia (uma interface por entidade "principal", ou
seguir o exemplo ilustrativo do ADR-003 ao pé da letra) estão registradas aqui:

- **`Habit` e `RecurringTask` não foram combinados em uma única `IActivityRepository`**, apesar de
  `docs/contracts/04-csharp-structure.md` §5 e o ADR-003 usarem esse nome como exemplo ilustrativo. O
  Aggregate Map (13.1) validou que são dois Aggregate Roots distintos, com identidade e ciclo de vida
  independentes — uma porta combinada seria dois repositórios colados em uma interface, não uma porta
  mapeada a um agregado. Esta Sprint refina o exemplo do ADR-003 nesse ponto específico.
- **Não existe `ITodoRepository`**. Todo não é Aggregate Root (Aggregate Map §2.5) — criar uma porta
  própria contradiria a própria validação da Sprint 13.1.
- **Não existe read service para Habits/Tasks/Projects isoladamente.** As listagens desses agregados
  para o quadro Daily são "listar por usuário", sem filtro/ordenação/paginação complexa nem
  necessidade de cruzar agregados — `ListAsync` de cada porta já é suficiente. Um read service só foi
  justificado onde a necessidade de projeção cross-aggregate é real e comprovada no código
  (`GetLevelUpQueryHandler`, `WalletQueryHandlers`).
- **Filtro/ordenação/paginação de Transactions não entrou em `ITransactionRepository`.** Colocar ali
  obrigaria a porta de escrita a carregar e filtrar em memória a mesma coisa que o read service já
  resolve como projeção — teria sido reintroduzir o mesmo problema do `ILevelUpRepository` em escala
  menor.

## 5. Contratos removidos

Nenhum. `ILevelUpRepository` **não foi removido nem descontinuado em código** — continua sendo o
contrato efetivamente usado por todos os handlers existentes. Um adapter EF Core para cada uma das 8
portas por Aggregate agora existe (Sprint 14.4, `docs/architecture/08-migration-status.md` §5.2), mas
isso não muda esta seção: nenhum handler foi religado, então `ILevelUpRepository` permanece o único
caminho real. Está formalmente marcado aqui como **substituído na arquitetura aprovada**; sua remoção de
código é trabalho da Sprint em que os handlers forem religados.

### 5.1 Violações encontradas e não corrigidas

Dois contratos existentes, fora de `ILevelUpRepository`, dependem hoje de `LevelUpData` diretamente:

- `IExperienceRewardService.Grant(LevelUpData data, ...)` — usado por `ToggleTaskCommandHandler`,
  `ToggleTodoCommandHandler`, `RegisterHabitPositiveCommandHandler`.
- `IEmailConfirmationIssuer.Issue(LevelUpData data, User user)` — usado por
  `CreateUserCommandHandler`, `CreateAccountCommandHandler`.

Ambos qualificam como o problema que esta Sprint pede para corrigir ("se identificar que algum
contrato depende de LevelUpData, corrija"). Não foram corrigidos porque a correção exigiria trocar sua
implementação concreta para depender de `IUserTokenRepository`/`IUserRepository` — que não têm adapter
ainda —, quebrando em runtime os quatro handlers acima e os testes que os exercitam. Registrados como
achado, com a forma-alvo já implícita nas portas criadas nesta Sprint: `IExperienceRewardService`
deveria depender só de `User` (já carregado pelo handler via `IUserRepository`), e
`IEmailConfirmationIssuer` deveria depender de `IUserTokenRepository`. Correção prevista para a Sprint
de adoção.

## 6. Unit of Work — necessidade identificada (Sprint 13.3), infraestrutura implementada (Sprint 14.5)

Nenhuma das 8 portas acima expõe `SaveChangesAsync`/`Update` genérico — decisão original desta Sprint,
que pressupunha uma peça ainda não projetada para persistir a mutação de um Aggregate já carregado (ex.:
`user.SetPasswordHash(...)` depois de um `GetByIdAsync`). O Persistence Map (13.2 §4) identificou duas
fronteiras que **exigem** essa peça cobrir mais de um Aggregate na mesma unidade:

- `Habit ↔ User` (concessão de XP sem chave de deduplicação em `RegisterPositive`/`RegisterNegative`);
- `UserToken ↔ User` (consumo de token de reset de senha).

**Sprint 14.5 implementou a infraestrutura**: `IUnitOfWork`/`EfUnitOfWork`
(`docs/data/02-ef-core-strategy.md` §0.6), coordenando as 8 portas contra um único `LevelUpDbContext`
compartilhado, com `SaveChangesAsync`/`BeginTransactionAsync`/`CommitTransactionAsync`/
`RollbackTransactionAsync`, testado (commit/rollback com múltiplos repositórios, rollback por exceção,
concorrência). **As duas fronteiras acima continuam não migradas** — nenhum handler foi alterado nesta
Sprint (`RegisterPositiveCommandHandler`/`RegisterNegativeCommandHandler`/o fluxo de consumo de token
continuam em JSON via `ILevelUpRepository`); `IUnitOfWork` existe e está testado, mas sem consumidor
real, mesmo padrão de "porta pronta, zero handler" já visto nas Sprints 13.3/14.4. Gap também parcial:
`SaveChangesAsync()` só persiste uma mutação sobre uma entidade **ainda rastreada** pelo contexto
compartilhado (ex.: logo após um `AddAsync` do próprio Unit of Work) — como as 8 portas continuam sem
`Update`/`Save` e toda leitura continua `AsNoTracking()` (decisão da Sprint 14.4, não alterada), ainda
não existe um caminho "carregar via `GetAsync`, mutar, salvar" através destes contratos.

## 7. Transações — necessidade identificada (Sprint 13.3), suporte genérico implementado (Sprint 14.5)

As duas fronteiras do §6 continuam sendo as únicas do Domain inteiro que exigem consistência imediata
entre tipos de Aggregate diferentes (Persistence Map 13.2 §4) — isso não mudou. O que mudou: **existe
agora um mecanismo genérico de transação explícita** (`EfUnitOfWork.BeginTransactionAsync`/
`CommitTransactionAsync`/`RollbackTransactionAsync`, sobre `IDbContextTransaction` do EF Core), disponível
para qualquer Sprint futura que precise coordenar essas fronteiras (ou outras) atomicamente — mas nenhuma
delas foi religada a esse mecanismo ainda; permanece decisão da Sprint de adoção, não desta.

## 8. Divergência de `docs/data/01-relational-model.md` (reafirmada)

Já registrada na Sprint 13.2 (`06-domain-persistence-map.md` §0) e reafirmada aqui porque
`IWalletTagRepository` a torna concreta: aquele documento modela `WalletTags` com `WalletId` e
unicidade `(WalletId, NormalizedName)`; o contrato desta Sprint usa `UserId` e
`IsNameInUseAsync(userId, ...)`, seguindo o Aggregate Map validado. Não corrigido em
`01-relational-model.md` nesta Sprint — é um documento de tecnologia específica (SQL Server), fora do
princípio agnóstico das Sprints 13.1–13.3.

**Resolvido na Sprint 14.1:** `01-relational-model.md` agora modela `WalletTags.UserId` com unicidade
`(UserId, Name)` via collation case-insensitive, alinhado a `IsNameInUseAsync(userId, ...)`.

## 9. Sprint 13.4 — Correção da abstração de atomicidade (supersede §6)

§6 propunha inicialmente portas orientadas a execução de delegate (`Func<Habit, User, TResult>`
injetado em Infrastructure). Revisão arquitetural (13.4): mesmo não conhecendo `LevelUpData`, um
delegate executado dentro de Infrastructure ainda transfere decisão de negócio para lá — Application
nunca deveria "emprestar" sua lógica de domínio para ser executada por outra camada.

**Design corrigido — escopo explícito, sem callback:**

```csharp
public interface IHabitProgressionTransaction
{
    Task<IHabitProgressionScope> BeginAsync(
        Guid userId, Guid habitId, CancellationToken cancellationToken = default);
}

public interface IHabitProgressionScope : IAsyncDisposable
{
    Habit Habit { get; }
    User User { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
}
```

Mesma forma para `IIdentityTokenTransaction`/`IIdentityTokenScope` (`UserToken?`/`User?`). Application
chama `BeginAsync`, invoca os métodos de domínio diretamente sobre `scope.Habit`/`scope.User` no
próprio handler, e então chama `scope.CommitAsync()`. Nenhum delegate cruza a fronteira
Application→Infrastructure. `DisposeAsync` sem `CommitAsync` prévio não persiste nada (aborto),
espelhando o comportamento atual de `UpdateAsync` quando a mutação lança exceção.

No adapter JSON, `BeginAsync` adquire o `JsonStorageGate` uma única vez e mantém o mesmo `LevelUpData`
carregado vivo até `CommitAsync`/`DisposeAsync` — os objetos `Habit`/`User` expostos são as mesmas
referências dentro desse grafo, então `CommitAsync` só precisa validar e gravar, sem religar por id.
Um adapter EF Core futuro abriria um `DbContext`/transação em `BeginAsync` e chamaria
`SaveChangesAsync` em `CommitAsync` — mesma interface pública, nenhuma mudança necessária.

Ainda não implementado — pertence aos Lots 5/6 (User/UserToken; XP e consistência cross-aggregate).

## 10. Sprint 13.4 — Correções de granularidade aprovadas (fechadas no Contract Completion Step, ver §14)

Revisão pontual de 5 contratos (sem reabrir Aggregate Map/Persistence Map), aprovada nesta Sprint,
**implementada no Contract Completion Step** (pré-Sprint 14.6, ver §14 — a forma final difere da
nominal abaixo em um ponto importante, também explicado ali):

- `ITransactionRepository` — adicionar `SaveAsync(Transaction, ct)` (Lot 4).
- `IWalletTagRepository` — adicionar `SaveAsync(WalletTag, ct)` (Lot 4).
- `IProjectRepository` — adicionar `SaveAsync(Project, ct)` e `MoveTodoAsync(Guid userId, Guid todoId, Guid destinationProjectId, ct)` (Lot 3).
- `IUserTokenRepository` — adicionar `RevokeActiveAsync(Guid userId, UserTokenType type, DateTimeOffset revokedAtUtc, ct)` (Lot 5).
- `IWalletRepository` — revisado, **confirmado adequado sem alteração** (o único mutador, `Wallet.Touch()`, é subproduto mecânico absorvido internamente pelo adapter de `ITransactionRepository`).

Cada `SaveAsync` persiste o estado atual de um Aggregate Root já obtido via `GetAsync`/`AddAsync` da
mesma interface e mutado através de comportamento de domínio — nunca um `Save` genérico entre
agregados; declarado independentemente em cada interface, sem interface-base compartilhada.

Gap adicional identificado (não coberto pela revisão original, sinalizado para os Lots 2/5): `IUserRepository`, `IHabitRepository` e `IRecurringTaskRepository` também vão precisar de `SaveAsync` quando seus handlers de atualização migrarem.

## 11. Sprint 13.4 — Lot 1 implementado e validado

Escopo: extração de `JsonLevelUpDocumentStore` (pipeline único de leitura/escrita/backup/gate,
compartilhado — não duplicado — por todo adapter JSON atual e futuro), `IWalletReadService` com
adapter `JsonWalletReadService`, registro em DI, migração dos 4 handlers de consulta de
`WalletQueryHandlers.cs`, `CurrentUserGuard` com sobrecarga nova (sem `LevelUpData`, sem checagem de
existência — delegada à próxima chamada de repositório do handler).

`IDashboardReadService`/`GetLevelUpQueryHandler`/`DashboardState`/`ProfileCreationState` foram
explicitamente excluídos deste Lot — blast radius muito maior (ver relatório da Sprint 13.4), tratado
como Lot dedicado à parte.

Detalhes completos — arquivos, testes, validação — no relatório final da Sprint 13.4.

## 12. Sprint 13.4 — Migração do Dashboard implementada e validada

Escopo aprovado e concluído nesta mesma Sprint, em lote separado do Lot 1: `IDashboardReadService` com
adapter `JsonDashboardReadService`; `DashboardResponse` corrigido antes da adoção (adição de
`ProjectSummary.Attribute`/`.Featured`/`.Completed`, `TodoSummary.ProjectId`/`.Featured`,
`UserProfileSummary.HasProfile` — nenhuma introdução de uma interface compartilhada `IActivitySummary`,
por decisão explícita: `DashboardState` foi refatorado para filtrar via um helper privado com seletores
de campo, não via uma nova hierarquia pública em Application). `DashboardState`, `Home.razor`,
`ProfileSidePanel.razor`, `ProjectContextFilter`, `ProjectWorkspace`, `TodoEditorModal`,
`DashboardModalState` migrados para `DashboardResponse`.

**`GetLevelUpQuery`/`GetLevelUpResponse`/`GetLevelUpQueryHandler` foram deliberadamente preservados sem
alteração** — uma nova query paralela (`GetDashboardQuery`/`GetDashboardQueryHandler`) foi criada em vez
de reaproveitar a existente, porque `Tutorial.razor`, `Account.razor` e `ProfileCreationState` continuam
chamando `LevelUpWebService.LoadAsync()` (que ainda envia `GetLevelUpQuery`) e não fazem parte do escopo
desta migração. Consequência: o bloqueador §3.6 de `01-current-state.md` ("`GetLevelUpResponse` ainda
expõe `LevelUpData`") está resolvido **apenas para o Dashboard** — o tipo `GetLevelUpResponse` continua
existindo e sendo retornado para os 3 consumidores acima. Ver `08-migration-status.md` §1 para o
inventário verificado.

## 13. Nota sobre §9/§10 — decisões aprovadas, ainda não aplicadas ao código (histórico até o Contract Completion Step)

Uma verificação de código feita na Sprint 13.7 confirmou que **nenhuma** das correções aprovadas no §10
(os 4 métodos `SaveAsync`/`MoveTodoAsync`/`RevokeActiveAsync`) e **nenhum** dos tipos do design corrigido
no §9 (`IHabitProgressionTransaction`, `IIdentityTokenTransaction`, seus respectivos `*Scope`) existem
nos arquivos `.cs` atuais. §9 e §10 registram decisões de design aprovadas para quando as Sprints/Lots
correspondentes forem executadas — não implementações parciais. Ver `08-migration-status.md` §3.1/§3.2.

**Atualização — Contract Completion Step (pré-Sprint 14.6)**: os 4 métodos de §10 (G1/G3/G4, ver §14)
foram implementados, com uma correção de forma para G1 explicada ali. §9
(`IHabitProgressionTransaction`/`IIdentityTokenTransaction`) **continua sem implementação** — fora do
escopo do Contract Completion Step, que resolveu apenas G1–G5 (persistência de mutação por Aggregate,
não a abstração de transação cross-aggregate por delegate de §9, que o `IUnitOfWork` da Sprint 14.5 já
torna redundante: coordenar `Habit`+`User` ou `UserToken`+`User` na mesma transação hoje se faz via
`IUnitOfWork.BeginTransactionAsync()` + os `UpdateAsync` novos de cada porta, não via um tipo dedicado
por fronteira).

## 14. Contract Completion Step (pré-Sprint 14.6) — G1–G5 fechados

A matriz de migração de handlers (apresentada antes desta Sprint) encontrou 4 lacunas (G1–G4) e uma
quinta encontrada durante a própria matriz (G5), bloqueando a migração de qualquer handler real. Todas
as 5 foram fechadas, mantendo 1 repositório por Aggregate e Todo exclusivamente dentro de
`IProjectRepository` — nenhum contrato novo, nenhuma `Repository<T>`/`ITodoRepository`.

**G1 — persistência de mutação sobre um Aggregate já carregado.** A forma nominal de §10
(`SaveAsync(TAggregate entity, ct)`, recebendo a entidade já mutada e desanexada) foi **substituída**
por:

```csharp
Task UpdateAsync(Guid userId, Guid aggregateId, Action<TAggregate> mutation, CancellationToken ct = default);
```

Motivo, verificado durante a implementação, não assumido: `SaveAsync(entity, ct)` receberia um objeto
desanexado de um contexto diferente e já descartado (toda leitura é `AsNoTracking` desde a Sprint 14.4).
Reanexá-lo e marcá-lo `Modified` exigiria o `RowVersion` original para a checagem otimista — mas o
Domain nunca expõe `RowVersion` (é shadow, só existe em Infrastructure). Sem ele, a única saída seria
"espiar" o `RowVersion` atual do banco só no momento de salvar, o que deixaria de detectar exatamente o
conflito que o `RowVersion` existe para pegar (uma escrita concorrente entre a leitura original do
caller e este `SaveAsync`). A forma `UpdateAsync(id, Action<T>, ct)` carrega a entidade **rastreada**
dentro do mesmo método/contexto, aplica a mutação do caller, salva imediatamente — mesmo padrão já
aprovado e testado para `RemoveAsync`/`ReorderAsync` na Sprint 14.4, e mecanicamente correto para
concorrência (testado com um conflito genuíno de duas escritas, ver
`docs/data/02-ef-core-strategy.md` §0.7). Implementado em `IUserRepository`, `IUserTokenRepository`,
`IHabitRepository`, `IRecurringTaskRepository`, `IProjectRepository`, `IWalletRepository`,
`IWalletTagRepository`, `ITransactionRepository`.

**G2 — operações de `IProjectRepository` sobre um Todo de um Project já existente**: `AddTodoAsync`,
`UpdateTodoAsync`, `RemoveTodoAsync` — Todo permanece inteiramente dentro de `IProjectRepository`,
nenhuma `ITodoRepository` foi criada.

**G3 — `IUserTokenRepository.RevokeActiveAsync`**: forma exatamente como aprovada em §10, implementada.

**G4 — `IProjectRepository.MoveTodoAsync`**: forma exatamente como aprovada em §10, implementada —
reusa `Project.RemoveTodo`/`Project.AddTodo` (API pública de Domain) em vez de qualquer atalho de
Infrastructure, preservando `Touch()` em ambos os Projects envolvidos.

**G5 — descoberta durante a própria matriz, não nomeada em G1–G4, confirmada e aprovada antes de
implementar**: excluir uma `WalletTag` também limpa `WalletTagId` de toda `Transaction` que a referencia
(`LevelUpData.RemoveWalletTag`) — sem contrato equivalente até então. Fechada com
`ITransactionRepository.ClearTagReferencesAsync(Guid walletTagId, ct)`, espelhando exatamente esse
comportamento.

Detalhe completo de implementação, testes e o achado de bug real encontrado durante os testes (não
relacionado às 5 lacunas em si) em `docs/data/02-ef-core-strategy.md` §0.7.
