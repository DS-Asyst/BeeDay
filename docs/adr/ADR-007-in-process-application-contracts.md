# ADR-007 — Contratos como Interfaces Internas de `BeeDay.Application`

**Status:** Aceito e implementado (adoção completa confirmada desde a Sprint 14.6 — este ADR
registra formalmente, pela primeira vez, a forma final já em produção; não é uma decisão nova).
Supersede especificamente o mecanismo decidido em [ADR-001](ADR-001-contract-first.md) (projeto
`Contracts` separado) — ver nota no Status daquele ADR.  
**Data:** 2026-08-22

## Contexto

O [ADR-001](ADR-001-contract-first.md) (2026-07-31) decidiu adotar Contract-First em todas as
fronteiras da aplicação e, como mecanismo concreto dessa decisão, "criar `LevelUp.Contracts` como
projeto independente". Esse projeto separado nunca foi criado — em nenhum momento da história do
repositório existiu um `.csproj` de nome `Contracts` na solução. A auditoria de validade de ADR da
EPIC 31 (Sprint 31.2) confirmou essa divergência e classificou o ADR-001 como `CONFLICTING`: o
objetivo arquitetural foi alcançado, mas não pelo meio literalmente decidido, sem nenhum ADR
posterior formalizando a forma real adotada. Este ADR fecha essa lacuna.

## Decisão

Os contratos de fronteira entre `BeeDay.Application` e `BeeDay.Infrastructure`/`BeeDay.Web` são
interfaces C# comuns, definidas dentro do próprio assembly `BeeDay.Application` — não em um projeto
`.csproj` separado. Especificamente, hoje:

- 8 interfaces de repositório por Aggregate Root (`IUserRepository`, `IUserTokenRepository`,
  `IHabitRepository`, `IRecurringTaskRepository`, `IProjectRepository`, `IWalletRepository`,
  `ITransactionRepository`, `IWalletTagRepository`) mais `IUnitOfWork`, todas em
  `src/BeeDay.Application/Common/Contracts/`;
- 2 interfaces de read service (`IDashboardReadService`, `IWalletReadService`), cada uma em
  `Features/<Feature>/Contracts/`, próximas ao caso de uso que as consome;
- `BeeDay.Application.csproj` declara exatamente um `ProjectReference` (`BeeDay.Domain`) e nenhum
  outro projeto de produção depende dele além de `BeeDay.Infrastructure`/`BeeDay.Web` — a direção de
  dependência que o ADR-001 pretendia proteger (Application nunca conhece Infrastructure) é real e
  testada, só que sem um projeto físico extra.

O teste `PersistenceContractBoundaryTests` (`tests/BeeDay.Application.Tests/`) protege esta decisão
em código: nenhum contrato expõe tipo de serialização vazando de Infrastructure, nenhuma interface é
um repositório genérico (`IRepository<T>`), e nenhuma interface além de `IUnitOfWork` propriamente
dito tem "UnitOfWork" no nome.

## Consequências positivas

- zero custo de manutenção de um projeto adicional (build, versionamento, referência) só para
  hospedar interfaces;
- navegação mais simples — o contrato de um caso de uso vive ao lado do próprio caso de uso
  (`Features/<Feature>/Contracts/`) quando é específico o suficiente, ou em `Common/Contracts/`
  quando é compartilhado entre Aggregates;
- o objetivo original do ADR-001 (Application depende de abstrações, nunca de Infrastructure
  diretamente) permanece verificado por teste real, não apenas por convenção.

## Consequências negativas

- se um consumidor externo ao processo (ex.: uma API pública futura) precisar depender só dos
  contratos sem trazer `BeeDay.Application` inteiro, essa separação teria que ser reintroduzida
  então — não é possível hoje sem um projeto próprio;
- a intenção original do ADR-001 de "preparação para API futura" via pacote de contratos
  independente não é servida por esta forma; `docs/api/beeday.v1.yaml` permanece um rascunho
  especulativo não implementado (ver `docs/api/README.md`), sem relação com este ADR.

## Restrições

- não criar um projeto `Contracts` separado sem evidência de necessidade real (ex.: um consumidor
  de processo externo genuíno) — reintroduzir a estrutura do ADR-001 "porque estava no plano
  original" não é justificativa suficiente;
- continuar proibido, como no ADR-001: contratos que espelham tabelas, exposição de entidades,
  repositório genérico como solução universal, microserviços.

## Referências

- [ADR-001](ADR-001-contract-first.md) — decisão original que este ADR supersede parcialmente
  (o mecanismo, não o objetivo).
- [ADR-003](ADR-003-aggregate-repositories.md) — decisão de repositórios por Aggregate Root, que já
  documenta a lista final dos 8 contratos.
- [`docs/architecture/03-clean-architecture.md`](../architecture/03-clean-architecture.md) e
  [`docs/application/04-contracts.md`](../application/04-contracts.md) — documentação técnica atual
  desta mesma estrutura.
- `docs/epics/31-documentation-knowledge-consolidation/README.md` — Sprint 31.2, classificação de
  validade que identificou a lacuna fechada por este ADR.
