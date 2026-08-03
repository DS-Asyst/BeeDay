# ADR-005 — Remoção do Código Legado JSON e de `LevelUpData`

**Status:** Aceito e implementado (Sprint 14.7)
**Data:** 2026-08-02

## Contexto

A Sprint 14.6 (ADR-004) cortou o *runtime* para SQL Server: todo handler de produção passou a usar os
8 contratos por Aggregate + `IUnitOfWork` + os 2 read services, e os tipos que existiam apenas para
servir o caminho de escrita antigo (`ILevelUpRepository`, `JsonLevelUpRepository`, `GetLevelUpQuery*`,
`GetLevelUpResponse`, `RequestHandlerBase`, `FakeLevelUpRepository`) foram removidos. Mas essa Sprint
deliberadamente não removeu dois itens maiores: o restante do pipeline JSON
(`JsonLevelUpDocumentStore` e os 13 componentes abaixo dele) e `LevelUpData` (Domain) — ambos ficaram
compilados, sem registro em DI, sem nenhum consumidor real, como código legado explicitamente adiado
para uma decisão futura (ADR-004 §Decisão, último parágrafo).

Essa Sprint responde a essa decisão adiada. Código compilado sem nenhum caminho de execução real é
dívida técnica pura: exige entendimento e manutenção sem retornar nada (nenhum teste de regressão
protege comportamento que nunca roda em produção), e a existência de `JsonLevelUpDocumentStore` ainda
compilado convida à reintrodução acidental de um segundo provider de persistência no futuro.
`LevelUpData`, especificamente, é uma raiz de documento — por definição, o oposto do que Contract-First
e os 8 repositórios por Aggregate estabelecem; mantê-la "inofensiva" ainda deixava um objeto que conhece
todos os Aggregates ao mesmo tempo disponível para qualquer código futuro importar por conveniência —
exatamente o padrão que `FakeUnitOfWork.cs` (Application.Tests) já tinha caído em usar como
armazenamento compartilhado entre seus 8 fakes.

## Decisão

Remover, não apenas desregistrar:

- Toda a pasta `src/LevelUp.Infrastructure/Persistence/Json/` (14 arquivos:
  `JsonLevelUpDocumentStore`, `JsonDashboardReadService`, `JsonWalletReadService`, `JsonStorageGate`,
  `JsonStorageInitializer`, `JsonAtomicFileCommitter`, `JsonFileReader`, `JsonFileWriter`,
  `JsonBackupService`, `DomainJsonContractResolver`, `JsonSerializerOptionsFactory`,
  `JsonStoragePaths`, `LegacyActivityAttributeMigrator`, `LegacyCharacterMigrator`,
  `LegacyInventoryTagMigrator`), `Configuration/JsonStorageOptions.cs` e
  `HealthChecks/JsonStorageHealthCheck.cs`.
- `src/LevelUp.Domain/Entities/LevelUpData.cs` e `LevelUpData.Persistence.cs` — todos os ~55 membros
  foram rastreados individualmente antes da remoção (inventário completo em
  `docs/architecture/08-migration-status.md` §9.2): cada invariante que ainda tinha sentido já estava
  duplicada em um handler de Application e/ou em um índice único do SQL Server; o restante era
  bootstrapping de documento único (sem equivalente necessário — SQL Server nasce vazio, ADR-002) ou o
  padrão de "usuário atual" ambiente, já morto como mecanismo de autenticação desde a Sprint 12.5.
- `tests/LevelUp.Application.Tests/FakeUnitOfWork.cs` foi **reescrito, não apagado**: mesma superfície
  pública (`IUnitOfWork` + 8 fakes por Aggregate), mas cada fake agora tem sua própria `List<T>`
  independente em vez de todas compartilharem uma `LevelUpData` em memória — nenhum tipo novo agrega as
  8 listas, mesmo princípio que motiva a remoção de `LevelUpData` do Domain.
- Testes exclusivos do modelo de documento/JSON: `ActivityOrderingTests.cs`,
  `WalletAggregateRulesTests.cs` (Domain.Tests); `JsonPersistenceTests.cs`,
  `JsonDashboardReadServiceTests.cs`, `JsonWalletReadServiceTests.cs`,
  `DomainJsonContractResolverTests.cs`, `SchemaCompatibilityCharacterizationTests.cs`
  (Infrastructure.Tests); `LevelUpDataTests.cs` (Application.Tests, encontrado durante a auditoria desta
  Sprint, fora do inventário original).

Mantido, com justificativa: `JsonEventJournal` — `IEventJournal.AppendAsync` é write-only (sem leitura
de volta), seu único consumidor é `AuditDomainEventHandler` (fire-and-forget via
`IBackgroundTaskQueue`), e grava um arquivo (`LevelUpEvents.ndjson`) que nenhum outro componente lê. É
auditoria de domain events, não persistência funcional de nenhum Aggregate — removê-lo quebraria
auditoria sem relação com esta decisão. Foi desacoplado de tudo removido acima: ganhou
`EventJournalOptions` (opções mínimas, `Directory`/`FileName`, sem nenhum campo em comum com o extinto
`JsonStorageOptions`) e resolve seu próprio caminho de arquivo inline, em vez de depender de
`JsonStoragePaths`/`JsonSerializerOptionsFactory`. Confirmado que domain events (`DomainEvent` e
subtipos) são records imutáveis com propriedades públicas `init` — não precisam do
`DomainJsonContractResolver`, que existia especificamente para os setters privados de
`LevelUpData`/entidades Domain.

## Consequências

- Não existe mais nenhum provider de persistência JSON no código — `JsonEventJournal` é o único tipo
  com "Json" no nome remanescente, e não participa da persistência de nenhum Aggregate.
- Não existe mais nenhuma raiz de documento global no Domain — todo tipo em `Entities/` é um Aggregate
  Root ou entidade filha legítima, nenhum conhece os outros.
- Nenhuma regra de negócio mudou. Uma investigação mais profunda durante esta Sprint corrigiu um
  achado do plano aprovado (não o comportamento do código): a suspeita de que
  `ReorderActivitiesCommandHandler` divergia do `LevelUpData` original para um id genuinamente
  desconhecido em reorder era um falso positivo — o teste Domain que parecia provar isso exercitava um
  overload de conveniência (usuário-atual-ambiente) que nenhum handler de produção jamais chamou.
  Nenhum código de produção foi alterado por esse achado; um teste de regressão foi adicionado em seu
  lugar (`FeatureServicesTests.ReorderHandler_RejectsGenuinelyUnknownIdentifier`).
- Duas lacunas de cobertura foram fechadas com testes novos, antes da cobertura antiga ser removida —
  não depois: nome de tag de wallet duplicado no *Create* (`WalletHandlersTests.CreateTag_RejectsDuplicateNameForCurrentUser`)
  e nickname duplicado no `CompleteUserProfileCommandHandler`
  (`FeatureServicesTests.CompleteUserProfileHandler_RejectsNicknameAlreadyUsedByAnotherUser`).
- Nenhuma migration foi criada ou alterada — `LevelUpData` nunca teve mapeamento EF Core, então sua
  remoção não tem impacto de schema. `dotnet ef migrations has-pending-model-changes` confirma que o
  modelo é exatamente o que `InitialCreate` já descreve.
- `PersistenceContractBoundaryTests` perdeu o guard específico "nenhum contrato expõe `LevelUpData`"
  (`ExposesLevelUpData`) — não há mais nada desse tipo para vazar. Os outros 3 guards (nenhum tipo
  `System.Text.Json.*` em contrato, nenhuma abstração de Repository genérico/segundo Unit of Work,
  `LevelUp.Application` nunca referencia `LevelUp.Infrastructure`) continuam intactos.
- Toda a suíte de testes (742 testes: 93 Domain, 72 Application, 120 Infrastructure, 7 E2E, 450 Web) roda
  limpa após a remoção, sem redução de cobertura — cada teste removido tem uma linha em
  `docs/architecture/08-migration-status.md` §9.5 apontando o equivalente que já existia ou o teste novo
  criado antes da remoção.
- Risco de corrida em invariantes de unicidade sob SQL Server (documentado, não introduzido por esta
  Sprint): ver `docs/architecture/08-migration-status.md` §9.9.

## Referências

- `docs/architecture/08-migration-status.md` §9 — estado de código verificado após a remoção,
  inventário completo arquivo por arquivo.
- `docs/data/03-json-to-sql-transition.md` §"Sprint 14.7" — fecha a transição JSON → SQL no sentido de
  código, não apenas runtime.
- ADR-002 — decisão de banco vazio, sem importação de dados (inalterada por esta ADR).
- ADR-004 — corte de runtime (Sprint 14.6); este ADR completa a decisão que aquele deliberadamente
  adiou.
