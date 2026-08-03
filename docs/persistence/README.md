# Persistence

Modelo relacional e estratégia de mapeamento EF Core/SQL Server usados por `BeeDay.Infrastructure`.

**Fonte da verdade:** confirmado nas Sprints anteriores desta migração contra
`src/BeeDay.Infrastructure/Persistence/SqlServer` (configurações Fluent API, TPC em `Activity`,
Owned Type `UserExperience`, Complex Type `ExperienceSource`) e a migration `InitialCreate`.

## Documentos

| Documento | Status |
|---|---|
| [`01-relational-model.md`](01-relational-model.md) | Correto — substantivamente preciso contra as configurações EF Core reais; nomenclatura "LevelUp" residual pendente de atualização. |
| [`02-ef-core-strategy.md`](02-ef-core-strategy.md) | Correto — estratégia TPC/Owned Type/Complex Type confirmada como implementada; estilo de diário de sprint e nomenclatura "LevelUp" residual pendentes de atualização. |

## Ordem de leitura recomendada

1. `01-relational-model.md` — o modelo relacional em si.
2. `02-ef-core-strategy.md` — as decisões de mapeamento por trás do modelo.
