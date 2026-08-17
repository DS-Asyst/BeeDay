# Architecture Decision Records (ADR)

Registros imutáveis de decisões arquiteturais, cada um datado no momento em que a decisão foi
tomada.

**Fonte da verdade:** cada ADR é, por definição, sua própria fonte da verdade histórica — não é
revalidado contra o código atual. O status de adoção de cada decisão é rastreado no cabeçalho do
próprio ADR e, quando aplicável, em `docs/history/migration-status.md`.

## Regra de imutabilidade

Diferente de todo outro documento em `docs/`, ADRs **não são atualizados** para refletir
renomeações posteriores (ex.: `LevelUp` → `BeeDay`). Um ADR datado de antes da migração de nome
legitimamente cita `LevelUp.Contracts`, `LevelUpDbContext`, etc., porque esses eram os nomes reais
no momento da decisão. Reescrever essas referências destruiria o valor de registro histórico.

## Documentos

| ADR | Decisão |
|---|---|
| [`ADR-001-contract-first.md`](ADR-001-contract-first.md) | Adotar Contract-First em todas as fronteiras (adoção parcial em código — nenhum projeto Contracts separado foi criado). |
| [`ADR-002-greenfield-database.md`](ADR-002-greenfield-database.md) | Banco SQL Server começa vazio, sem importação de dados JSON. |
| [`ADR-003-aggregate-repositories.md`](ADR-003-aggregate-repositories.md) | Repositórios por Aggregate Root + read services, em vez de repositório genérico. |
| [`ADR-004-sql-server-runtime-cutover.md`](ADR-004-sql-server-runtime-cutover.md) | Corte de runtime para SQL Server como único provider. |
| [`ADR-005-json-legacy-removal.md`](ADR-005-json-legacy-removal.md) | Remoção completa do pipeline JSON legado e de `LevelUpData`. |
| [`ADR-006-transactional-email-localization-boundary.md`](ADR-006-transactional-email-localization-boundary.md) | Culture de e-mail transacional transportada via `User.Language`, catálogo `.resx` estreito Infrastructure-owned, sem `IStringLocalizer`/estado global. |

## Ordem de leitura recomendada

Ordem numérica (001 → 006) — cada ADR referencia explicitamente os anteriores que ainda são
válidos.
