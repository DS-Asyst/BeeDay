# ADR-003 — Repositórios por Agregado e Read Services

**Status:** Aceito, com refinamentos (Sprint 13.3) — **totalmente implementado e adotado desde a
Sprint 14.6** (todo handler de produção usa um dos 8 contratos, os 2 read services, ou `IUnitOfWork`);
ver `docs/architecture/08-migration-status.md` §8 para o estado verificado.  
**Data:** 2026-07-31

## Contexto

`ILevelUpRepository` trabalha com todo o documento. Um repositório genérico também não expressaria adequadamente ownership, consultas e invariantes.

## Decisão

Criar repositórios orientados a agregados para escrita e read services orientados a projeções para consulta.

## Exemplos

Lista original, ilustrativa — não literal. A Sprint 13.3, ao validar contra o Aggregate Map
(`05-domain-aggregate-map.md`), refinou parte dela; ver `docs/architecture/07-persistence-contracts.md`
§4 para a justificativa de cada divergência:

- `IUserRepository` — criado como proposto.
- `IActivityRepository` — **não criado**; `Habit` e `RecurringTask` são Aggregate Roots distintos, então
  viraram `IHabitRepository`/`IRecurringTaskRepository` separados em vez de uma porta combinada.
- `IProjectRepository` — criado como proposto (inclui `Todo` como entidade filha, sem porta própria).
- `IWalletRepository` — criado como proposto.
- `IExperienceRepository` — **não criado**; XP vive inteiramente dentro do agregado `User`
  (`UserExperience`), sem agregado próprio a ter uma porta.
- `IDashboardReadService` — criado como proposto; adotado (adapter + consumidor reais).
- `IWalletReadService` — criado como proposto; adotado (adapter + consumidor reais).
- `IUnitOfWork` — criado na Sprint 14.5 (`EfUnitOfWork`), mais estreito que um Unit of Work genérico:
  coordena os 8 repositórios contra um único `LevelUpDbContext` compartilhado, com
  `BeginTransactionAsync`/`CommitTransactionAsync`/`RollbackTransactionAsync` explícitos. Desde a
  Sprint 14.6, é o único mecanismo de transação cross-Aggregate em uso — as portas de atomicidade mais
  estreitas propostas em `07-persistence-contracts.md` §9 (`IHabitProgressionTransaction`/
  `IIdentityTokenTransaction`) nunca foram implementadas; `IUnitOfWork` cobriu essa necessidade.

Também criados, fora desta lista original: `IUserTokenRepository`, `ITransactionRepository`,
`IWalletTagRepository` — cada um mapeado a um Aggregate Root validado no Aggregate Map que a lista
ilustrativa original não previa.

## Consequências

- contratos mais expressivos;
- melhor teste;
- queries eficientes;
- menor dependência do ORM;
- maior número de interfaces, controlado por feature.
