# Architecture Decision Records (ADR)

Registros imutáveis de decisões arquiteturais, cada um datado no momento em que a decisão foi
tomada.

**Fonte da verdade:** cada ADR é, por definição, sua própria fonte da verdade histórica — não é
revalidado contra o código atual. O status de adoção de cada decisão é rastreado no cabeçalho do
próprio ADR e, quando aplicável, em `docs/history/migration-status.md`.

**Validade atual (baseline estabelecida na Sprint 31.2, EPIC 31):** a coluna "Validade atual" abaixo
classifica se a *decisão* de cada ADR continua sendo a arquitetura vigente hoje — sem alterar o
corpo imutável de nenhum ADR. Vocabulário: `VALID` (decisão e implementação coincidem hoje),
`SUPERSEDED` (revertida/substituída por outro ADR posterior, com link), `OBSOLETE` (decisão não se
aplica mais, sem substituto formal), `CONFLICTING` (a implementação atual diverge da decisão
literal, sem ADR posterior que formalize a divergência), `NEW ADR REQUIRED` (uma decisão real
existe em código sem nenhum ADR que a documente).

## Regra de imutabilidade

Diferente de todo outro documento em `docs/`, ADRs **não são atualizados** para refletir
renomeações posteriores (ex.: `LevelUp` → `BeeDay`). Um ADR datado de antes da migração de nome
legitimamente cita `LevelUp.Contracts`, `LevelUpDbContext`, etc., porque esses eram os nomes reais
no momento da decisão. Reescrever essas referências destruiria o valor de registro histórico.

## Documentos

| ADR | Decisão | Validade atual |
|---|---|---|
| [`ADR-001-contract-first.md`](ADR-001-contract-first.md) | Adotar Contract-First em todas as fronteiras (adoção parcial em código — nenhum projeto Contracts separado foi criado). | `CONFLICTING` — o objetivo (Application depende de abstrações, nunca de Infrastructure) é a realidade atual, mas o mecanismo literal decidido (projeto `LevelUp.Contracts`/`BeeDay.Contracts` separado) nunca existiu; os 8 contratos vivem dentro de `BeeDay.Application` mesmo, ver [`docs/architecture/03-clean-architecture.md`](../architecture/03-clean-architecture.md) e [`docs/application/04-contracts.md`](../application/04-contracts.md). Nenhum ADR posterior formaliza essa divergência — candidata a `NEW ADR REQUIRED` (documentar a forma final adotada) ou a uma nota de supersessão formal; decisão de produto/arquitetura não tomada nesta Sprint, cabe ao owner. |
| [`ADR-002-greenfield-database.md`](ADR-002-greenfield-database.md) | Banco SQL Server começa vazio, sem importação de dados JSON. | `VALID` — confirmado contra o `README.md` raiz e `docs/persistence/`. |
| [`ADR-003-aggregate-repositories.md`](ADR-003-aggregate-repositories.md) | Repositórios por Aggregate Root + read services, em vez de repositório genérico. | `VALID` — o próprio ADR já traz status atualizado (nota de andamento, não reescrita) listando os 8 contratos finais + `IUnitOfWork`, idêntico à implementação atual. |
| [`ADR-004-sql-server-runtime-cutover.md`](ADR-004-sql-server-runtime-cutover.md) | Corte de runtime para SQL Server como único provider. | `VALID`, com **supersessão parcial já autodocumentada**: o próprio cabeçalho do ADR-004 registra que seu parágrafo sobre manter `LevelUpData`/pipeline JSON como legado não registrado foi revertido e formalmente superado por [ADR-005](ADR-005-json-legacy-removal.md) (Sprint 14.7). O corte de runtime em si permanece válido e inalterado. |
| [`ADR-005-json-legacy-removal.md`](ADR-005-json-legacy-removal.md) | Remoção completa do pipeline JSON legado e de `LevelUpData`. | `VALID` — supersede parte de [ADR-004](ADR-004-sql-server-runtime-cutover.md) (ver acima); confirmado sem nenhuma referência a JSON legado em código de produção atual. |
| [`ADR-006-transactional-email-localization-boundary.md`](ADR-006-transactional-email-localization-boundary.md) | Culture de e-mail transacional transportada via `User.Language`, catálogo `.resx` estreito Infrastructure-owned, sem `IStringLocalizer`/estado global. | `VALID` — confirmado contra `docs/infrastructure/06-transactional-email.md` e `IdentityEmailComposer.cs` atual. |

## Ordem de leitura recomendada

Ordem numérica (001 → 006) — cada ADR referencia explicitamente os anteriores que ainda são
válidos.
