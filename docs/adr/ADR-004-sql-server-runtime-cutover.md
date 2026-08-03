# ADR-004 — SQL Server como Único Provider de Runtime

**Status:** Aceito e implementado (Sprint 14.6). **Parcialmente superseded por
[ADR-005](ADR-005-json-legacy-removal.md) (Sprint 14.7)** — a decisão abaixo de manter `LevelUpData` e
o pipeline JSON como código legado não registrado (§Decisão, último parágrafo) foi revertida: ambos
foram removidos do repositório na Sprint 14.7. O restante desta ADR (o corte de runtime em si)
permanece válido e inalterado.  
**Data:** 2026-08-02

## Contexto

ADR-001/002/003 e a EPIC 13 estabeleceram os contratos por Aggregate, o modelo relacional e a política
de banco vazio, mas deliberadamente sem trocar o provider ativo — cada Sprint de 14.1 a 14.5 e o
Contract Completion Step construíram peça por peça (modelo EF, migration, 8 adapters, `IUnitOfWork`)
com **zero consumidor real**, para que cada peça pudesse ser validada isoladamente antes do corte. Ao
final da Sprint 14.5, JSON continuava sendo o único provider que a aplicação em execução realmente
usava — toda a infraestrutura SQL Server existia, mas não fazia nada.

## Decisão

Migrar todo handler de produção (leitura e escrita) dos contratos JSON/`ILevelUpRepository` para os 8
contratos por Aggregate + `IUnitOfWork` + `IDashboardReadService`/`IWalletReadService` (agora
implementados por `EfDashboardReadService`/`EfWalletReadService`), em um único corte — não um estado
híbrido, não uma bandeira de configuração escolhendo o provider, sem dual-write, sem fallback automático
para JSON em caso de falha de configuração. `SqlServerOptions.ConnectionString` passou a ser obrigatório
(`ValidateOnStart()`); a ausência de configuração agora impede o startup, nunca degrada para JSON
silenciosamente.

Como consequência direta, os tipos que existiam apenas para servir esse caminho de escrita antigo foram
removidos (não apenas desativados): `ILevelUpRepository`, `JsonLevelUpRepository`, `GetLevelUpQuery`,
`GetLevelUpQueryHandler`, `GetLevelUpResponse`, `RequestHandlerBase`, e o fake de teste
`FakeLevelUpRepository`. `LevelUpData` (tipo de Domain) e o restante do pipeline JSON
(`JsonLevelUpDocumentStore` e os componentes abaixo dele) **não** foram removidos — permanecem no
repositório como código legado, compilado mas não registrado em DI, sem nenhuma leitura ou escrita
acontecendo por esse caminho enquanto a aplicação roda. `JsonEventJournal` (auditoria de domain events)
é a única peça JSON que segue ativa, por ser um mecanismo à parte, não relacionado à persistência de
`LevelUpData` que este corte trata.

## Consequências

- SQL Server é o único provider exercitado pela aplicação em execução — não apenas "capaz de ser usado".
- Nenhuma regra de negócio mudou: toda invariante que antes vivia em métodos de `LevelUpData`
  (unicidade de e-mail/nickname/tag, ownership, etc.) já tinha equivalente nos 8 contratos ou foi
  preservada porque a mutação continua passando pelo mesmo método de Domain — só mudou quem
  carrega/persiste.
- `IUnitOfWork` (`EfUnitOfWork`) passa a ser o único mecanismo de transação cross-Aggregate em uso, com
  7 fronteiras confirmadas em código (`Habit+User`, `RecurringTask+User`, `Project/Todo+User`,
  `Project+Todo` na movimentação entre Projects, `User+UserToken`, `Wallet+Transaction`,
  `WalletTag+Transaction+Wallet`) — as duas últimas encontradas durante a implementação, não nas 4
  originalmente nomeadas, sinalizadas para aprovação antes de implementar.
- Toda a suíte de testes (Domain, Application, Infrastructure, Web, E2E — 780 testes) roda contra SQL
  Server LocalDB descartável (nunca InMemory/SQLite); `LevelUpWebApplicationFactory`/
  `E2EWebApplicationFactory` migram/derrubam um banco único por instância.
- Rollback não é automático nem por configuração — reverter esta decisão significa não publicar o
  commit desta Sprint (ou `git revert` dele), nunca uma bandeira de runtime alternando entre providers.
- `LevelUp.Infrastructure.csproj` passou a conceder `InternalsVisibleTo` também a `LevelUp.Web.Tests`/
  `LevelUp.E2E.Tests` (além de `LevelUp.Infrastructure.Tests`), para que os factories de teste possam
  nomear `LevelUpDbContext` e aplicar/derrubar a migration.

## Referências

- `docs/architecture/08-migration-status.md` §8 — estado de código verificado após o corte.
- `docs/data/03-json-to-sql-transition.md` — estratégia de transição original (Passos A–G); este ADR
  fecha os Passos A–D no sentido de código/runtime local.
- ADR-002 — decisão de banco vazio, sem importação de dados (inalterada por este ADR).
- ADR-003 — decisão de repositórios por Aggregate (agora totalmente adotada; ver a atualização de
  status naquele documento).
