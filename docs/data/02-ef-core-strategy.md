# Estratégia EF Core

**Status:** Sprint 14.2 implementou a fundação técnica (`LevelUpDbContext`, `DbSet`s, DI, Options,
connection string, convenções globais). Sprint 14.3 completou o modelo EF Core — as 10
`IEntityTypeConfiguration<T>`, Owned Type (`UserExperience`) e Complex Type (`ExperienceSource`),
estratégia de herança TPC escopada a `Activity`, e a migration `InitialCreate`, verificada contra um
LocalDB real. Sprint 14.4 implementou os 8 `Ef*Repository` que concretizam os contratos de persistência
por Aggregate (EPIC 13), registrados em DI. **Sprint 14.5 implementou `IUnitOfWork`/`EfUnitOfWork`**,
coordenando esses 8 repositórios contra um único `LevelUpDbContext` compartilhado, com controle
transacional explícito (`BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync`) e
padronização de exceções (`EfConcurrencySaveChanges` → `PersistenceException`/`ConcurrencyConflictException`,
nunca um tipo do EF Core cru). **O Contract Completion Step (pré-Sprint 14.6) fechou as 5 lacunas
encontradas pela matriz de migração de handlers** (G1–G5 — persistência de mutação por Aggregate,
operações de Todo dentro de Project, revogação em lote de UserToken, movimentação de Todo entre
Projects, limpeza de referência WalletTag→Transaction), todas nos 8 contratos já existentes, nenhum
contrato novo. **Ainda não existem**: read service EF, banco real em produção — ver
`docs/architecture/08-migration-status.md` para o estado verificado. JSON continua sendo o único
provider ativo; nenhum handler consome os novos repositórios/Unit of Work/métodos.

## 0. Sprint 14.2 — o que foi implementado

- `LevelUpDbContext` (`internal`, `src/LevelUp.Infrastructure/Persistence/SqlServer/LevelUpDbContext.cs`)
  com **10 `DbSet`s**, um por entidade de Domain com identidade própria: `User`, `UserToken`, `Habit`,
  `RecurringTask`, `Project`, `Todo`, `Wallet`, `WalletTag`, `Transaction`, `ExperienceEntry`. (Uma
  revisão anterior desta Sprint relatou "9" por engano — a contagem correta é 10: dos 11 nomes do brief
  original, apenas `UserExperience` ficou de fora.)
- **`UserExperience` e `ExperienceSource` não têm `DbSet`** — nenhuma das duas tem `Id`/identidade
  própria em Domain (são valores únicos embutidos em `User.Experience`/`ExperienceEntry.Source`).
  `modelBuilder.Ignore<UserExperience>()`/`Ignore<ExperienceSource>()` impede que a convenção do EF Core
  tente descobri-las como entidades independentes e falhe por falta de chave primária — confirmado por
  teste (`LevelUpDbContextTests`) durante a implementação. **São mapeamentos incompletos, deliberados e
  temporários** — o mapeamento correto de ambas (owned type via `OwnsOne`) é trabalho de Aggregate,
  adiado para a Sprint que mapear `User`/`ExperienceEntry`. `IUnitOfWork` também não é implementado por
  `LevelUpDbContext` ainda — não existe em `LevelUp.Application` (ADR-003: "não criado").
- **Estratégia de herança de `Habit`/`RecurringTask`/`Project`/`Todo`: NÃO configurada nesta Sprint,
  de propósito.** Uma primeira versão desta Sprint declarou TPC (`UseTpcMappingStrategy()`) na raiz
  `Entity`, cobrindo também entidades que nunca fizeram parte dessa ambiguidade (`User`, `Wallet`,
  `Transaction` etc.) — um compromisso arquitetural largo demais para o escopo desta Sprint, corrigido a
  pedido explícito antes de prosseguir. **Removido.** Sem uma estratégia explícita, o modelo hoje usa o
  default do EF Core para esse formato (table-per-hierarchy — as 4 tabelas colapsam numa só, com
  discriminador), o que **diverge de `01-relational-model.md`** (4 tabelas independentes) e **precisa
  ser resolvido na Sprint 14.3**, depois de inspecionar o modelo completo e escrever as
  `IEntityTypeConfiguration<T>` reais dessas quatro entidades — antes de qualquer migration ser gerada.
- **`RowVersion` como shadow property, aplicada uma vez, globalmente**, a todo tipo concreto exceto
  `ExperienceEntry`. Mantido (não movido para configuração por entidade) porque `01-relational-model.md`
  §2 exige explicitamente `RowVersion` em toda entidade mutável, **incluindo `Todo`** (tabela `Todos`,
  última coluna), com `ExperienceEntries` como única exceção documentada (§0 item 14, append-only). Como
  a própria documentação já determina "toda entidade mutável, sem exceção além da já registrada", uma
  configuração central única — em vez de repetida em cada futura `IEntityTypeConfiguration<T>` — é a
  leitura mais fiel dessa exigência, não uma antecipação de mapeamento por Aggregate.
- **Convenções globais**: `decimal` → precisão `(19,2)`; `DateTimeOffset` → precisão `(7)`, ambas via
  `ConfigureConventions`. Nomes de tabela/coluna já batem com `01-relational-model.md` pela convenção
  padrão do EF Core (sem necessidade de configuração adicional).
- `SqlServerOptions` (`ConnectionString`, `HealthCheckEnabled` — ambos com binding em
  `AddLevelUpInfrastructure`, sem regra de validação de conteúdo — ver nota abaixo).
  **`AddDbContextFactory<LevelUpDbContext>`, não `AddDbContext`** — `LevelUp.Web` é Blazor Server, cujos
  circuitos SignalR são de vida longa; um `DbContext` registrado como scoped viveria pelo circuito
  inteiro, inseguro para um tipo que não é thread-safe. Todo adapter futuro deve resolver
  `IDbContextFactory<LevelUpDbContext>` e criar/descartar um contexto de vida curta por operação
  (`CreateDbContext()`/`CreateDbContextAsync()`) — `LevelUpDbContext` em si **não é resolvível
  diretamente** do container.
- `LevelUpDbContextFactory` (`IDesignTimeDbContextFactory<LevelUpDbContext>`, distinto do
  `IDbContextFactory<LevelUpDbContext>` acima) — permite `dotnet ef migrations` na Sprint 14.3 sem subir
  o host completo de `LevelUp.Web`.
- `SqlServerHealthCheck` existe mas fica **desligado por padrão** (`SqlServerOptions.HealthCheckEnabled
  = false`, não configurado em nenhum `appsettings*.json`) — depende de
  `IDbContextFactory<LevelUpDbContext>`, cria e descarta seu próprio contexto por checagem. Uma
  connection string de conveniência (LocalDB) já existe em `appsettings.json` para a Sprint 14.3, mas
  isso não deve, por si só, ligar um health check que falharia em todo ambiente sem um SQL Server real —
  `/health` não tem filtro de tag e aplicaria 503 a partir do primeiro `CanConnectAsync()` malsucedido.
  Corrigido durante esta Sprint após o teste de integração
  `AuthorizationIntegrationTests.Anonymous_CanReachHealthEndpoints` capturar exatamente esse efeito
  colateral.
- Nenhuma regra `.Validate(...)` de connection string foi adicionada a `SqlServerOptions` — faria
  `ValidateOnStart()` quebrar o `dotnet run` em Production, onde o placeholder é intencionalmente vazio.
- Nenhum interceptor foi adicionado — nenhuma preocupação transversal em `SaveChanges` existe ainda
  (sem Domain Events/Outbox, sem soft-delete); timestamps já chegam preenchidos pelo Domain
  (`01-relational-model.md` §5.6).

## 0.1 Sprint 14.2 — o que faltava (resolvido na Sprint 14.3, ver §0.2)

Registrado como histórico do que a Sprint 14.2 deixou pendente: estratégia de herança para
`Habit`/`RecurringTask`/`Project`/`Todo`; nenhuma `IEntityTypeConfiguration<T>`; mapeamento owned de
`UserExperience`/`ExperienceSource`; nenhuma migration. Todos resolvidos — ver §0.2.

## 0.2 Sprint 14.3 — o que foi implementado

- **As 10 `IEntityTypeConfiguration<T>`** (`src/LevelUp.Infrastructure/Persistence/SqlServer/Configurations/`):
  colunas, comprimentos, precisão, `CHECK`s, índices, FKs e `DeleteBehavior` de `01-relational-model.md`
  §2/§5.1, todos implementados exatamente como aprovado — incluindo os dois `NO ACTION`
  (`Todos.UserId`, `Wallets.UserId`) e o `SET NULL` (`Transactions.WalletTagId`) que evitam os múltiplos
  caminhos de cascade identificados na Sprint 14.1. `ActivityConfigurationExtensions.cs` centraliza a
  configuração das colunas herdadas de `Activity` (`Title`/`Description`/`Featured`/`Attribute`/
  `Completed`/timestamps), chamada pelas 4 configurations de tipos derivados — evita repetir a mesma
  configuração 4 vezes; cada tabela continua totalmente independente sob TPC.
- **Estratégia de herança implementada: TPC escopado a `Activity`** (não mais em `Entity`, que uma
  primeira versão da Sprint 14.2 havia declarado por engano) — ver `01-relational-model.md` §5.8 para o
  raciocínio completo. Verificado por teste (`ActivityDerivedEntities_MapToFourDistinctTables`) que as
  4 tabelas realmente ficam separadas.
- **`UserExperience` mapeada como Owned Type** (`OwnsOne` a partir de `UserConfiguration`, tabela
  separada compartilhando a PK de `Users`, `Entries` ignorado — ver `01-relational-model.md` "UserExperience").
- **`ExperienceSource` mapeada como Complex Type** (`ComplexProperty`, não `OwnsOne`) a partir de
  `ExperienceEntryConfiguration` — colunas inline na mesma tabela `ExperienceEntries`. Escolhido em vez
  de Owned Type especificamente porque um Complex Type não é uma entidade separada no modelo (sem
  chave, sem FK sombra), o que evitou o problema real encontrado com `OwnsOne` aqui (ver achados
  técnicos abaixo).
- **Migration `InitialCreate` gerada** (`Persistence/SqlServer/Migrations/`, via
  `dotnet ef migrations add`, usando `LevelUpDbContextFactory` — sem subir `LevelUp.Web`). 11 tabelas
  físicas (10 `DbSet`s + `UserExperience` como Owned Type), sem nenhum `InsertData`/seed. `dotnet-ef`
  instalado como ferramenta local do repositório (`dotnet-tools.json`), não uma `PackageReference`.
- **`UX_ExperienceEntries_Dedup` adicionado via SQL bruto diretamente na migration**
  (`migrationBuilder.Sql(...)`), não via Fluent API — achado técnico central desta Sprint: confirmado,
  esgotando toda a superfície disponível (lambda `HasIndex`, array de strings, dotted-path, e até a API
  de metadata bruta `IMutableEntityType.AddIndex`), que o EF Core não consegue expressar um índice que
  cruze propriedades da entidade dona com propriedades de um Complex/Owned Type aninhado. É uma
  limitação real e confirmada da ferramenta, não uma escolha de mapeamento — o SQL bruto é a solução
  padrão e já conhecida para esse cenário específico.
- **Achado técnico — `Project.Completed` precisou de `UsePropertyAccessMode(PropertyAccessMode.Field)`**:
  sob TPC, uma propriedade herdada da raiz (`Activity.Completed`) só pode ser configurada/ignorada uma
  única vez, na raiz — nunca por tipo derivado. Como `Project` sobrescreve `Completed` com uma
  implementação totalmente computada (sem campo de apoio próprio) enquanto `Habit`/`RecurringTask`/`Todo`
  precisam do valor real, a única saída (sem tocar Domain) foi forçar acesso por campo — a tabela
  `Projects` ganha uma coluna `Completed` que o Domain nunca lê para esse tipo. Ver
  `01-relational-model.md` "Projects" e `ActivityConfigurationExtensions.cs` para o detalhe completo.
- **Correção em `01-relational-model.md`**: `UserExperience.UpdatedAtUtc` removida — `Experience/UserExperience.cs`
  não tem nenhuma propriedade de timestamp, e §5.6 já exige que só Application/Domain preencha `*AtUtc`
  (nunca o banco); não havia fonte real para essa coluna.
- **Todas as demais decisões da Sprint 14.2 permanecem sem alteração**: `AddDbContextFactory` (não
  `AddDbContext`), `RowVersion` global via shadow property (agora também alcançando `UserExperience`,
  já que deixou de ser `Ignore()`ada), `SqlServerHealthCheck` desligado por padrão, nenhuma regra
  `.Validate(...)` de connection string, nenhum interceptor.

## 0.3 O que ainda falta (Sprint 14.4+)

- Nenhum repositório EF (`IUserRepository`, etc.), nenhum read service EF, nenhum `IUnitOfWork` —
  handlers continuam 100% em JSON.
- `SqlServerHealthCheck` permanece desligado até existir uma razão real para monitorar SQL Server (i.e.,
  até um adapter SQL real começar a ser usado por algum handler).

## 0.4 Sprint 14.3 closeout — verificação final de infraestrutura

Sprint 14.3 só foi considerada concluída depois de 5 verificações adicionais, pedidas explicitamente
antes de autorizar a Sprint 14.4, todas positivas:

1. **`dotnet ef database update` aplicado com sucesso** contra um banco LocalDB descartável
   (`LevelUp_Sprint14_3_Verify`, `(localdb)\mssqllocaldb`) — `Applying migration
   '20260802131230_InitialCreate'. Done.`, sem erro.
2. **Schema criado sem erro**: `sys.tables` confirmou as 11 tabelas físicas esperadas (`Users`,
   `UserTokens`, `Habits`, `RecurringTasks`, `Projects`, `Todos`, `Wallets`, `WalletTags`,
   `Transactions`, `ExperienceEntries`, `UserExperience`) mais `__EFMigrationsHistory`.
3. **Erro 1785 (multiple cascade paths) confirmado ausente** — `sys.foreign_keys` mostrou as 12 FKs
   reais criadas com o `delete_referential_action_desc` exatamente aprovado: `NO_ACTION` em
   `FK_Todos_Users_UserId` e `FK_Wallets_Users_UserId`, `SET_NULL` em
   `FK_Transactions_WalletTags_WalletTagId`, `CASCADE` nas demais 9. O SQL Server real aceitou todas as
   12 sem rejeitar nenhuma por múltiplos caminhos de cascade — a correção desenhada na Sprint 14.1 e
   implementada na 14.3 funciona contra um engine real, não só na teoria do modelo.
4. **Modelo determinístico confirmado**: `dotnet ef migrations has-pending-model-changes` retornou "No
   changes have been made to the model since the last migration." — o modelo atual e a migration
   `InitialCreate` existente são exatamente equivalentes, nada ficou de fora.
5. **Justificativa do índice `UX_ExperienceEntries_Dedup` (SQL bruto) reforçada com prova empírica**:
   além da limitação de API já documentada em §0.2, `sys.indexes`/`filter_definition` no banco de
   verificação confirmou que o índice criado via `migrationBuilder.Sql(...)` é exatamente
   `UNIQUE, WHERE ([SourceId] IS NOT NULL AND [SourceType]<>(0))` — idêntico à regra de dedup do Domain
   (`UserExperience.EnsureValidState()`). Ver o comentário atualizado em
   `ExperienceEntryConfiguration.cs` e a nota abaixo em §0.2.

Também confirmados: nenhum `CHECK` faltando (`sys.check_constraints` listou as 17 constraints
esperadas, uma por tipo/enum aprovado), e o banco de verificação foi descartado ao final
(`dotnet ef database drop --force`) — nenhum artefato de banco real permanece deste processo;
`git status` seguiu idêntico ao estado anterior a essas verificações (nenhum arquivo novo, nenhuma
migration extra gerada).

## 0.5 Sprint 14.4 — repositórios EF Core implementados

Os 8 adapters concretos dos contratos de persistência por Aggregate (EPIC 13,
`07-persistence-contracts.md`): `EfUserRepository`, `EfUserTokenRepository`, `EfHabitRepository`,
`EfRecurringTaskRepository`, `EfProjectRepository`, `EfWalletRepository`, `EfWalletTagRepository`,
`EfTransactionRepository` (`src/LevelUp.Infrastructure/Persistence/SqlServer/Repositories/`,
`internal sealed`, uma classe por Aggregate — nenhuma abstração genérica). Registrados em DI via
`AddScoped<IXxxRepository, EfXxxRepository>()`, logo após `AddDbContextFactory` — isso os torna
*resolvíveis*, não *consumidos*: nenhum handler depende de nenhum deles, `ILevelUpRepository`/JSON
continua sendo o único caminho real.

**Estratégia de contexto**: cada método de cada repositório abre e descarta seu próprio
`LevelUpDbContext` (`await using var context = await contextFactory.CreateDbContextAsync(ct);`) — nunca
um campo de instância, nunca um contexto compartilhado entre métodos.

**Estratégia de leitura**: `AsNoTracking()` em toda consulta; filtro sempre pelo identificador de posse
que a própria assinatura do contrato já expressa (`UserId`/`WalletId`/`ProjectId`/`WalletTagId`);
ordenação pelas shadow properties `Position` já existentes (`EF.Property<int>(e, "Position")`);
`EfProjectRepository` usa Filtered Include (`Include(p => p.Todos.OrderBy(...))`) para a única navegação
real do escopo. Nenhuma paginação — nenhum dos 8 contratos aceita parâmetros de página (fica em
`IWalletReadService`, fora deste escopo).

**Estratégia de escrita — dois pontos não pedidos por nenhum contrato, mas necessários**:

1. **`Position` na criação**: a shadow property não tem `HasDefaultValue` (Sprint 14.3) — sem
   atribuição explícita, todo novo registro chegaria com `Position = 0`. `AddAsync` calcula o próximo
   ordinal livre (`MAX(Position)` no mesmo escopo + 1) antes de inserir.
2. **`RemoveAsync`/`ReorderAsync` rebuscam por `Id`** em vez de anexar a instância recebida: essa
   instância veio de um `DbContext` diferente e já descartado, e o Domain nunca expõe `RowVersion` (é
   shadow, só existe em Infrastructure) — anexar o objeto recebido diretamente executaria o
   `DELETE`/`UPDATE` com `RowVersion` no valor `CLR` padrão, garantindo um falso positivo de
   `DbUpdateConcurrencyException` em vez de uma checagem real.

**Confirmado, não assumido**: nenhum dos 8 contratos expõe `Update`/`Save`/`SaveChanges` hoje —
`07-persistence-contracts.md` §6/§10/§13 já documentava isso como lacuna deliberada de Unit of Work,
pendente. Esta Sprint implementa exatamente os métodos que existem em cada interface, sem inventar
nenhum novo.

**Testes**: primeiro padrão de teste desta base contra um SQL Server LocalDB genuíno (não apenas o
modelo em memória) — `EfLocalDbTestBase` (`tests/LevelUp.Infrastructure.Tests/Persistence/SqlServer/Repositories/`)
cria um banco descartável com nome único por instância de teste, aplica a migration real
(`Database.MigrateAsync()`, não `EnsureCreated`), executa o cenário, remove o banco
(`Database.EnsureDeletedAsync()`). As 8 classes de teste (29 testes) compartilham uma collection xunit
com paralelização desligada, evitando contenção contra a mesma instância `mssqllocaldb` — mesmo padrão já
usado por `LevelUp.E2E.Tests` para Playwright/Kestrel. Confirmado por execução real: FKs para `Users`
exigiram criar um `User` de verdade em cada cenário (as constraints geradas na Sprint 14.3 realmente são
aplicadas, não apenas declaradas).

## 0.6 Sprint 14.5 — Unit of Work, transações e padronização de exceções

- **`IUnitOfWork`** (`src/LevelUp.Application/Common/Contracts/IUnitOfWork.cs`) — novo contrato,
  Contract-First (Application define, Infrastructure implementa), ao lado dos 8 já existentes. Expõe as
  8 portas por Aggregate (`Users`, `UserTokens`, `Habits`, `RecurringTasks`, `Projects`, `Wallets`,
  `WalletTags`, `Transactions`), `SaveChangesAsync`, `BeginTransactionAsync`, `CommitTransactionAsync`,
  `RollbackTransactionAsync`. `IAsyncDisposable`.
- **`EfUnitOfWork`** (`Persistence/SqlServer/EfUnitOfWork.cs`) — cria **um único** `LevelUpDbContext` no
  construtor (`IDbContextFactory<LevelUpDbContext>.CreateDbContext()`, síncrono — construir a instância
  não abre conexão), vivo até `DisposeAsync()`. Cada propriedade de repositório constrói, lazily, a
  mesma classe `Ef*Repository` já existente, usando um **segundo construtor interno**
  (`EfHabitRepository(LevelUpDbContext sharedContext)`) que reusa o contexto do Unit of Work em vez de
  criar um próprio. Registrado via **`AddTransient<IUnitOfWork, EfUnitOfWork>()`** (não `AddScoped` —
  Blazor Server tem escopo de DI por circuito; um `EfUnitOfWork` `Scoped` reproduziria o problema que
  `AddDbContextFactory` evita, já que guardaria um contexto vivo pela vida do circuito inteiro).
- **Comportamento transacional — preciso, não aproximado** (esclarecimento pedido explicitamente após a
  validação desta Sprint): cada método de escrita dos 8 `Ef*Repository` continua chamando
  `SaveChangesAsync` internamente — isso é aceito, não é um defeito a corrigir. O que muda é *onde* esse
  `SaveChangesAsync` se encaixa:
  1. **Uso standalone** (fora de um `EfUnitOfWork`): cada operação persiste a si mesma atomicamente, seu
     próprio `SaveChangesAsync` já é a unidade inteira — exatamente o comportamento da Sprint 14.4, sem
     mudança.
  2. **Uso via `EfUnitOfWork` com transação explícita aberta** (`BeginTransactionAsync()` já chamado):
     todo `SaveChangesAsync` interno de qualquer repositório resolvido daquele mesmo `EfUnitOfWork`
     participa da mesma transação ambiente — comportamento nativo do EF Core, nenhum dos 8 repositórios
     precisa saber que uma transação está aberta.
  3. `CommitTransactionAsync()` confirma a transação SQL — só então tudo o que foi persistido por cada
     `SaveChangesAsync` interno dentro dela se torna durável de fato.
  4. Um rollback (explícito via `RollbackTransactionAsync()`, ou automático por `DisposeAsync()` sem
     commit prévio) reverte **todo** `SaveChangesAsync` executado dentro daquela transação — não apenas
     a última chamada, nem apenas a que eventualmente falhou.
  5. `IUnitOfWork.SaveChangesAsync()` (exposto no próprio contrato) só é necessário para persistir uma
     mutação sobre uma entidade **ainda rastreada** que nenhuma operação de repositório já persistiu
     sozinha — o cenário testado é mutar `habit` logo após `Habits.AddAsync(habit)` (que já chamou seu
     próprio `SaveChangesAsync`) e precisar persistir essa mutação adicional. Não é a forma normal de
     persistir uma escrita — cada `AddAsync`/`RemoveAsync`/`ReorderAsync` já faz isso por si só.
- **Rollback**: explícito (`RollbackTransactionAsync`) ou automático — descartar o `EfUnitOfWork` com uma
  transação ainda aberta (nunca commitada) reverte tudo, mesma semântica de aborto já aprovada para
  `IHabitProgressionScope` em `07-persistence-contracts.md` §9 ("`DisposeAsync` sem `CommitAsync` prévio
  não persiste nada").
- **Padronização de exceções**: novo helper interno `EfConcurrencySaveChanges`
  (`Persistence/SqlServer/EfConcurrencySaveChanges.cs`) — único ponto onde todo `SaveChangesAsync` dos
  adapters SQL Server roda; `DbUpdateConcurrencyException` → novo `ConcurrencyConflictException`
  (`Persistence/Exceptions/`, subtipo de `PersistenceException`, ao lado dos já existentes
  `PersistenceAccessException`/`DataFileCorruptedException`/`BackupRestoreException`);
  `DbUpdateException` (qualquer outra) → `PersistenceException`. Nenhum tipo do EF Core cru escapa de
  Infrastructure — mesma hierarquia pública já usada pelo provider JSON, não uma paralela. Consequência:
  `EfWalletRepositoryTests.AddAsync_SecondWalletForSameUser_ViolatesUniqueIndex` (Sprint 14.4) passou a
  esperar `PersistenceException` em vez do `DbUpdateException` cru que esperava antes — mudança
  deliberada, é exatamente o que "padronizar exceções" pede.
- **`EfRepositoryBase`** (`Persistence/SqlServer/Repositories/EfRepositoryBase.cs`) — plumbing
  compartilhada pelos 8 repositórios, **não** uma abstração `Repository<T>`: resolve apenas "de onde vem
  o `LevelUpDbContext` desta chamada" (contexto próprio e descartável, modo standalone; ou contexto
  compartilhado do Unit of Work, nunca descartado pelo repositório) — nenhum conhecimento de forma de
  entidade ou operação CRUD genérica.
- **Gap conhecido, não fechado nesta Sprint**: `IUnitOfWork.SaveChangesAsync()` só persiste uma mutação
  feita diretamente sobre uma entidade ainda rastreada pelo contexto compartilhado (ex.: logo após
  `Habits.AddAsync(habit)`, mutar `habit` e chamar `SaveChangesAsync` de novo) — não existe um caminho
  para "carregar via `GetAsync`/`ListAsync` (sempre `AsNoTracking`, decisão da Sprint 14.4, não alterada
  aqui) e depois salvar uma mutação" separadamente, porque nenhum dos 8 contratos tinha `Update`/`Save`
  nesta Sprint (já documentado em `07-persistence-contracts.md` §6/§10/§13). **Fechado no Contract
  Completion Step (pré-Sprint 14.6, ver §0.7)** — não com um `Save` desanexado, mas com
  `UpdateAsync(id, Action<T> mutation, ct)`: carrega, muta e salva na mesma chamada, evitando o problema
  de concorrência que um `Save` desanexado teria (ver §0.7 para o porquê).
- **Testes** (`tests/LevelUp.Infrastructure.Tests/Persistence/SqlServer/EfUnitOfWorkTests.cs`, 6 novos,
  mesma collection/`EfLocalDbTestBase` da Sprint 14.4): commit e rollback explícitos coordenando 2
  repositórios diferentes; dispose sem commit revertendo automaticamente; rollback por exceção
  provando que a escrita **anterior**, já bem-sucedida, também é revertida (não só a que falhou);
  `SaveChangesAsync` persistindo uma mutação sobre uma entidade ainda rastreada; conflito de concorrência
  genuíno (duas instâncias de `LevelUpDbContext` competindo pela mesma linha) resultando em
  `ConcurrencyConflictException` com `DbUpdateConcurrencyException` como `InnerException`.

## 0.7 Contract Completion Step (pré-Sprint 14.6) — G1–G5 fechados nos 8 contratos existentes

A matriz de migração de handlers (apresentada antes desta etapa) encontrou 4 lacunas de contrato
(G1–G4) e uma quinta descoberta durante a própria matriz (G5) — nenhum handler real poderia migrar sem
elas. Todas as 5 foram fechadas **nos 8 contratos/adapters já existentes** — nenhum contrato novo,
nenhum `Repository<T>`, Todo permanece inteiramente dentro de `IProjectRepository`. Ver
`07-persistence-contracts.md` §14 para o registro formal por gap; aqui, o detalhe técnico de
implementação.

**G1 — forma corrigida em relação ao que a Sprint 13.4 (§10) tinha aprovado nominalmente.** A forma
original, `SaveAsync(TAggregate entity, ct)`, foi substituída por
`Task UpdateAsync(Guid userId, Guid aggregateId, Action<TAggregate> mutation, ct = default)` (e
variações sem `userId` onde o Aggregate já é resolvido por outro identificador — `IWalletRepository`
por `userId` só, `IUserTokenRepository`/`ITransactionRepository` por `tokenId`/`(walletId,
transactionId)`). Motivo, **verificado durante a implementação, não assumido**: um `SaveAsync(entity,
ct)` receberia um objeto desanexado, de um contexto diferente e já descartado (toda leitura é
`AsNoTracking` desde a Sprint 14.4). Reanexá-lo e marcá-lo `Modified` (`context.Update(entity)`)
exigiria o `RowVersion` original para a checagem otimista do EF Core — mas o Domain nunca expõe
`RowVersion` (é shadow, só existe em Infrastructure). Sem ele, a única saída seria "espiar" o
`RowVersion` atual do banco só no momento de salvar, o que pararia de detectar exatamente o conflito que
o `RowVersion` existe para pegar (uma escrita concorrente entre a leitura original do caller e este
"Save"). A forma `UpdateAsync` resolve isso carregando a entidade **rastreada** dentro do mesmo
método/contexto, aplicando a `mutation` do caller (lógica pura de Domain — `Action<T>`, nenhum tipo do
EF Core na assinatura) e salvando imediatamente — mesmo padrão já aprovado e testado para
`RemoveAsync`/`ReorderAsync` desde a Sprint 14.4.

Implementado em `EfUserRepository`, `EfUserTokenRepository`, `EfHabitRepository`,
`EfRecurringTaskRepository`, `EfProjectRepository` (nível Project), `EfWalletRepository`,
`EfWalletTagRepository`, `EfTransactionRepository`.

**G2/G4 — `IProjectRepository.AddTodoAsync`/`UpdateTodoAsync`/`RemoveTodoAsync`/`MoveTodoAsync`**:

- `AddTodoAsync`/`RemoveTodoAsync` carregam o Project **com Todos rastreados** e chamam
  `Project.AddTodo`/`RemoveTodo` (API pública de Domain) — preserva o `Touch()` do Project que o Domain
  já faz nessas operações, em vez de recriar essa lógica em Infrastructure.
- `UpdateTodoAsync` carrega só o Todo (confirmado lendo `Todo.cs`/`Activity.cs`: nenhuma mutação de Todo
  toca o Project — só `AddTodo`/`RemoveTodo` fazem isso).
- `MoveTodoAsync` chama `sourceProject.RemoveTodo(todoId)` seguido de `destinationProject.AddTodo(todo)`
  — ambos API pública; `Todo.AssignTo(projectId)` é `internal` a `LevelUp.Domain` (sem
  `InternalsVisibleTo` para Infrastructure), então só pode ser acionado indiretamente, através de
  `Project.AddTodo`, nunca chamado direto por Infrastructure.

**G3 — `IUserTokenRepository.RevokeActiveAsync`**: carrega todos os tokens ativos (`UserId`+`Type`+ainda
não usados/revogados) rastreados, chama `token.Revoke(...)` em cada um, salva uma vez.

**G5 — `ITransactionRepository.ClearTagReferencesAsync`**: espelha exatamente
`LevelUpData.RemoveWalletTag` — carrega toda Transaction com aquele `WalletTagId` rastreada, chama
`transaction.RemoveTag()` em cada uma, salva uma vez.

**Bug real encontrado e corrigido durante os testes (não uma das 5 lacunas em si)**: a primeira versão
de `AddTodoAsync` chamava `context.Entry(todo).Property("Position").CurrentValue = ...` antes de
qualquer `Add()` explícito no novo Todo. `context.Entry(entity)` em uma entidade ainda não rastreada a
anexa como `Unchanged` (comportamento documentado do EF Core), não `Added` — diferente de
`DbSet.Add(entity)`, que propaga `Added` por todo o grafo alcançável (por isso o `AddAsync` original,
que chama `context.Projects.Add(project)`, sempre funcionou: o `Add()` já propagava `Added` para os
Todos do grafo antes de qualquer `context.Entry(...)` tocá-los). Sem esse `Add()` explícito, o INSERT
pretendido virava um UPDATE que não batia com nenhuma linha (`DbUpdateConcurrencyException`, "0 rows
affected") — descoberto pelo próprio teste de caminho feliz de `AddTodoAsync`, não por inspeção manual.
Corrigido adicionando `context.Todos.Add(todo)` explicitamente antes de tocar `context.Entry(todo)`.

**Testes**: 26 novos, mesma `EfLocalDbTestBase`/collection das Sprints anteriores — para cada
`UpdateAsync` novo, um caminho feliz e um conflito de concorrência genuíno (a própria `mutation`
passada ao método sob teste executa, no meio da sua execução, uma escrita concorrente via um segundo
`LevelUpDbContext` direto na mesma linha, provando que o método detecta o conflito de verdade, não só na
teoria); mais `RevokeActiveAsync` (3 tokens revogados, nenhum apagado), `ClearTagReferencesAsync` (2
Transactions marcadas, 1 sem tag intocada), e os 4 métodos de Todo (posição, ownership, efeito visível
via `GetAsync`/`GetByTodoIdAsync`).

## 1. Introdução controlada

EF Core só deve ser adicionado depois que:

- contratos públicos estiverem criados;
- `ILevelUpRepository` não for mais consumido pelos handlers novos;
- portas por agregado existirem;
- testes de contrato existirem;
- ownership estiver centralizado.

## 2. DbContext

Implementado na Sprint 14.2, completado na 14.3
(`src/LevelUp.Infrastructure/Persistence/SqlServer/LevelUpDbContext.cs`) — skeleton abaixo reflete o
código real, não mais um exemplo ilustrativo. Difere da versão original deste documento em três pontos,
cada um com razão técnica concreta (ver §0.2): sem `IUnitOfWork` (não existe em Application ainda);
`DbSet<RecurringTask>` chama-se `RecurringTasks`, não `Tasks` (evita colisão de leitura com
`System.Threading.Tasks.Task`, consistente com o próprio nome do tipo em Domain); inclui `UserToken`,
`WalletTag`, `ExperienceEntry`, que a versão original omitia sem explicação — e **não** inclui
`UserExperience`, que não pode ser um `DbSet` (é Owned Type, mapeada a partir de `UserConfiguration`).

```csharp
internal sealed class LevelUpDbContext(DbContextOptions<LevelUpDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<RecurringTask> RecurringTasks => Set<RecurringTask>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTag> WalletTags => Set<WalletTag>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ExperienceEntry> ExperienceEntries => Set<ExperienceEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPC scoped to Activity (not Entity) — see 01-relational-model.md §5.8.
        modelBuilder.Entity<Activity>().UseTpcMappingStrategy();

        // Owned Type (UserExperience) and Complex Type (ExperienceSource) are configured from
        // UserConfiguration/ExperienceEntryConfiguration — applied before the RowVersion loop below,
        // deliberately (see the loop's own comment for why).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LevelUpDbContext).Assembly);

        // RowVersion shadow property, once, globally, for every concrete non-owned entity type except
        // ExperienceEntry (append-only) — see §0.2/§4 below and the method's own comments for the two
        // pitfalls found here (abstract types leaking RowVersion via inheritance; owned/complex types
        // rejecting modelBuilder.Entity(Type) entirely).
    }
}
```

Registrado via `AddDbContextFactory<LevelUpDbContext>` (não `AddDbContext`) em
`InfrastructureServiceCollectionExtensions.AddLevelUpInfrastructure` — ver §0.2.

## 3. Configuração

**Implementado (Sprint 14.3)** — `IEntityTypeConfiguration<T>` por entidade, 10 arquivos em
`Persistence/SqlServer/Configurations/`, mais `ActivityConfigurationExtensions.cs` (helper compartilhado
pelas 4 configurations de tipos derivados de `Activity`).

Não usar Data Annotations de persistência no Domain — confirmado: nenhuma foi adicionada.

## 4. Migrations

- migrations ficam em `LevelUp.Infrastructure` — **implementado**: `InitialCreate`
  (`Persistence/SqlServer/Migrations/`), gerada via `dotnet tool run dotnet-ef migrations add
  InitialCreate --project src/LevelUp.Infrastructure --startup-project src/LevelUp.Infrastructure`
  (Infrastructure como seu próprio startup project — evita precisar adicionar
  `Microsoft.EntityFrameworkCore.Design` a `LevelUp.Web`, que só é exigido pela ferramenta no projeto de
  startup, não no projeto alvo);
- uma migration inicial cria o schema vazio — **confirmado**: `Up()` só tem `CreateTable`/`CreateIndex`/
  `Sql()` (para `UX_ExperienceEntries_Dedup`, ver §0.2); nenhum `InsertData`;
- aplicação não executa migrations automaticamente em produção — ainda não implementado (Sprint 14.4+,
  junto com o Repository/adapter real);
- pipeline de deploy executa etapa controlada ou script idempotente — `dotnet ef migrations script`
  gerado e inspecionado nesta Sprint, não commitado como arquivo próprio;
- rollback de aplicação e rollback de schema devem ser planejados separadamente — `Down()` da migration
  existe (reverte as 11 tabelas), não testado contra um banco real nesta Sprint.

## 5. Concorrência

**Mapeamento implementado (Sprint 14.3)** — `rowversion` como shadow property em toda entidade mutável
(ver §0.2). O tratamento de conflito abaixo (`DbUpdateConcurrencyException`, código de erro, HTTP 409)
é trabalho de Application/Web, ainda não implementado — depende de um Repository/adapter real existir
primeiro (Sprint 14.4+). Usar `rowversion` nas entidades mutáveis. Em conflito:

- capturar `DbUpdateConcurrencyException`;
- mapear para `activity.version_conflict` ou equivalente;
- retornar 409 no HTTP;
- UI recarrega o estado e informa o usuário.

## 6. Transações

O `DbContext` atua como Unit of Work. Uma transação explícita só é necessária quando houver múltiplos commits ou integração especial. O fluxo normal usa um `SaveChangesAsync`.

## 7. Domain events

Eventos devem ser coletados dos agregados antes ou depois do commit conforme semântica:

- eventos internos que atualizam o mesmo banco: antes do commit;
- integrações externas: registrar Outbox no mesmo commit;
- publicação externa: depois do commit pelo worker.

## 8. Queries

Queries complexas podem usar projeção direta com `AsNoTracking()` para contratos de leitura. Não forçar todas as leituras por repositórios de agregados.

## 9. Testes

**Parcialmente implementado (Sprint 14.2/14.3)** — `tests/LevelUp.Infrastructure.Tests/LevelUpDbContextTests.cs`
(35 testes) constrói o modelo diretamente (sem SQL Server real) e verifica: nomes de tabela por
entidade, TPC separando as 4 tabelas de `Activity`, `UserExperience` como Owned Type com PK
compartilhada, `ExperienceSource` como Complex Type inline, `RowVersion` presente/ausente conforme
aprovado, `DeleteBehavior` de cada FK sensível (os dois `NO ACTION`, o `SET NULL`), índices filtrados
(`UX_Users_Nickname`), binding de `SqlServerOptions`, resolução de `IDbContextFactory<LevelUpDbContext>`
via DI. Não substitui os itens abaixo (SQL Server real, comportamento de banco em conflito/concorrência):

- testes de mapping com SQL Server real em container ou ambiente dedicado;
- não usar EF InMemory para validar comportamento relacional;
- SQLite pode apoiar testes rápidos, mas SQL Server deve existir nos testes de integração críticos.
