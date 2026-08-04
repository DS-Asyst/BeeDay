# Persistence

Modelo relacional e estratégia de mapeamento EF Core/SQL Server — reconstruído por completo na
Sprint 16.6 a partir exclusivamente do código atual (`src/BeeDay.Infrastructure/Persistence/SqlServer/`).
Nenhuma afirmação vem de `docs/history/` ou de sprints anteriores sem reverificação direta no
código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes de verdade".

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-relational-model.md`](01-relational-model.md) | As 11 tabelas: colunas, constraints, índices, chaves estrangeiras e seus `DeleteBehavior` |
| [`02-ef-core-strategy.md`](02-ef-core-strategy.md) | `BeeDayDbContext`, ordem do `OnModelCreating`, TPC, Owned Type, Complex Type, shadow properties, estratégia de migration |

## Ordem de leitura recomendada

1. `02-ef-core-strategy.md` — como o mapeamento é construído (a mecânica).
2. `01-relational-model.md` — o resultado desse mapeamento (o schema).

## Documentação relacionada

- [`docs/infrastructure/01-repositories.md`](../infrastructure/01-repositories.md) — como o
  modelo documentado aqui é lido/escrito em runtime pelos 8 repositórios.
- [`docs/infrastructure/03-concurrency.md`](../infrastructure/03-concurrency.md) — o mecanismo de
  `RowVersion` documentado em `02-ef-core-strategy.md` §Concorrência, em profundidade.
- [`docs/domain/README.md`](../domain/README.md) — os Aggregate Roots que este schema persiste.
