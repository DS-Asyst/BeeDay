# Aggregate Map do Domain (Sprint 13.1)

**Status:** validado — revisão arquitetural concluída, sem alterações de comportamento nesta Sprint.
**Escopo:** exclusivamente `LevelUp.Domain`. Nenhuma porta, repositório, Unit of Work ou implementação
de persistência foi criada ou antecipada por este documento.

> **Atualização Sprint 14.7:** a decomposição que este documento previa como "trabalho de Sprint
> futura" (§1) foi concluída — os 8 repositórios por Aggregate existem desde a Sprint 13.3/14.4, e
> `LevelUpData` (o documento único descrito abaixo no presente do indicativo) foi removido do Domain.
> O restante deste documento é o registro histórico da análise que levou a essa decomposição — leia os
> verbos no presente abaixo como descrevendo o estado da Sprint 13.1, não o atual. Ver
> `docs/architecture/08-migration-status.md` §9 para o estado atual.

Este documento registra o resultado da Sprint 13.1 — revisão de Aggregate & Domain Boundary — e serve
de baseline para o desenho de repositórios por agregado nas Sprints seguintes (ver ADR-003).

## 1. Premissa

`LevelUpData` não é um Aggregate Root de domínio. É um documento de persistência JSON que hoje contém,
em listas de nível superior, todos os agregados reais do sistema. Isso é consistente com o que
`01-current-state.md` §3.1/3.2 já registra do lado de Application/Infrastructure ("unidade de trabalho
global"). A decomposição de `LevelUpData` em portas por agregado é trabalho de Sprint futura
(Contract-First, ADR-003) — este documento não a antecipa.

Critério usado para reconhecer um Aggregate Root: existência de uma invariante real aplicada em tempo
de execução ou de uma dependência de ciclo de vida comprovada no código — não a mera aparência de
"entidade principal". Nenhuma entidade foi promovida a Aggregate Root apenas por conveniência.

## 2. Aggregates identificados

### 2.1 `User`

- **Root:** `User`.
- **Contido no agregado:** `UserExperience` (wrapper sem identidade própria contendo
  `IReadOnlyList<ExperienceEntry>`, entidades usadas apenas para deduplicação por referência dentro do
  próprio agregado). `Profile` é uma projeção somente-leitura, não uma entidade persistida.
- **Value Objects na construção:** `EmailAddress`, `UserName`, `Nickname` — validadores transitórios;
  o valor validado é desembrulhado para `string` no campo do entity.
- **Domain Events associados:** `ExperienceGrantedDomainEvent`, `UserLeveledUpDomainEvent` (ver §3.4 —
  hoje não são levantados pelo próprio agregado).
- **Invariantes reais aplicadas no agregado:** idempotência de XP por
  `(UserId, Source.Type, Source.ReferenceId, RewardType)` em `UserExperience.TryAdd`; perfil completado
  uma única vez; `SessionVersion` para invalidação de sessão.
- **Invariantes cross-aggregate (fora do escopo do agregado):** e-mail único e nickname único —
  aplicadas hoje em `LevelUpData`; em SQL tornam-se índice único, não responsabilidade do agregado.
- **Ciclo de vida:** não é dono de `UserToken`, `Wallet`, `Habit`, `RecurringTask`, `Project` ou
  `WalletTag` — todos referenciados por `UserId` (FK), nunca contidos.

### 2.2 `UserToken`

- **Root:** `UserToken` (não é entidade filha de `User`). FK: `UserId`.
- **Invariantes:** autocontidas (`EnsureCanBeUsed`, `MarkAsUsed`, `Revoke` — tipo, expiração, uso,
  revogação).
- **Justificativa de fronteira própria:** revogação em lote (`RevokeActiveUserTokens`) não exige
  atomicidade forte com `User` — falha parcial apenas atrasa uma expiração natural. Nenhuma leitura de
  `User` depende do estado de tokens para manter uma invariante.
- **Gap registrado:** o catálogo de portas do ADR-003 não lista uma porta para `UserToken`. Não
  corrigido nesta Sprint (proibido criar portas); registrado para Sprint 13.2.

### 2.3 `Habit`

- **Root:** `Habit`. Sem entidades filhas. Invariantes (`RegisterPositive`/`RegisterNegative`
  respeitando `Direction`) autocontidas. FK: `UserId`.

### 2.4 `RecurringTask`

- **Root:** `RecurringTask`. Sem entidades filhas, sem invariante cruzando outro agregado. FK:
  `UserId`.

### 2.5 `Project` (com `Todo` como entidade filha)

- **Root:** `Project`. **Entidade filha real:** `Todo` (`Project.Todos: List<Todo>`).
- **Justificativa do containment:** confirmado no código que `Todo` não existe em nenhuma lista de
  nível superior — só existe dentro de `project.Todos`. `DeleteProjectCommandHandler` remove o projeto
  sem limpeza explícita de Todos, o que só está correto porque o ciclo de vida de `Todo` está
  genuinely amarrado ao de `Project` (padrão Order/OrderLine). Reordenação (`ReorderTodos`) também é
  escopada por projeto.
- **Ressalvas registradas (ver §3), sem correção nesta Sprint:**
  - inconsistência de padrão: é o único containment do Domain; todo o resto usa lista plana + FK;
  - violação de Liskov: `Project` herda `Activity.ToggleCompletion()` mas não pode suportá-lo, e
    sobrescreve `Completed` com um setter vazio apenas para compatibilidade de serialização.
- **Nota de desenho:** mover um `Todo` entre dois `Project` (`UpdateTodoCommandHandler`) toca duas
  instâncias do mesmo tipo de Aggregate Root na mesma transação — legítimo, mas relevante para o
  desenho de `IProjectRepository`/`ITodoRepository` na Sprint 13.2.

### 2.6 `Wallet`

- **Root:** `Wallet`, deliberadamente magro. Sem entidades filhas — `CalculateBalance` e afins são
  métodos de domínio stateless recebendo transações de fora; nenhum saldo é armazenado no agregado.
- **Invariante real:** um Wallet por User (`LevelUpData.AddWallet`).
- Confirmado que `Wallet` não contém `List<Transaction>` — já consistente com o padrão lista plana +
  FK do restante do Domain.

### 2.7 `Transaction`

- **Root:** `Transaction`. FK: `WalletId`, `WalletTagId?`. Já modelado como lista de nível superior,
  não como filha de `Wallet` — nenhuma mudança necessária.
- **Invariante cross-aggregate:** a tag referenciada deve pertencer ao mesmo dono do Wallet
  (`ValidateTransactionTagOwnership`), aplicada hoje em `LevelUpData` por ser o único ponto que enxerga
  os dois agregados ao mesmo tempo sob o documento único; em SQL torna-se validação de aplicação
  consultando os repositórios envolvidos.

### 2.8 `WalletTag`

- **Root:** `WalletTag`. FK: `UserId` — não possui `WalletId`; pertence ao usuário, não a uma carteira
  específica (nome levemente enganoso, sem impacto de fronteira — ver §3).
- **Invariante cross-aggregate:** exclusão de uma tag limpa `WalletTagId` de toda `Transaction` que a
  referencia (`LevelUpData.RemoveWalletTag`); em SQL torna-se `ON DELETE SET NULL` ou orquestração
  explícita no handler.

## 3. Achados classificados

| # | Achado | Severidade | Ação nesta Sprint |
|---|---|---|---|
| 1 | `Experience/ExperienceTransaction.cs` é código morto — sem nenhuma referência fora do próprio arquivo. | Médio | Nenhuma — registrado para remoção em Sprint futura |
| 2 | `Project` quebra o contrato de `Activity.ToggleCompletion()` (lança exceção sempre) e usa um setter vazio em `Completed` só para compatibilidade de serialização — violação de Liskov. | Médio | Nenhuma — registrado; qualquer correção deve ser decidida explicitamente (ex. extrair `ICompletable`) por mudar uma validação de integridade usada em `EnsureValidState` |
| 3 | `Todo` é o único caso de containment do Domain; todo o resto usa lista plana + FK. Containment é justificado pelo ciclo de vida real, mas a inconsistência gera dois caminhos de acesso a Todo em `LevelUpData`. | Médio | Nenhuma — mudança tocaria Application, fora do escopo autorizado nesta Sprint |
| 4 | `ExperienceGrantedDomainEvent`/`UserLeveledUpDomainEvent` vivem em `Domain.Events`, mas são inteiramente construídos por `Application.Common.Experience.ExperienceRewardEventPublisher`; `Entity` não tem mecanismo de outbox (`AddDomainEvent`/`DomainEvents`). Nada no agregado garante que o evento seja disparado quando a invariante muda. | Médio | Nenhuma — introduzir infraestrutura de Domain Events é explicitamente fora do escopo desta Sprint |
| 5 | `WalletTag.UserId` existe, mas não há `WalletId` — nome sugere pertencimento a `Wallet` quando pertence a `User`. | Baixo | Nenhuma — nomenclatura, sem impacto de fronteira |
| 6 | `Entity` não sobrescreve `Equals`/`GetHashCode` por `Id` — igualdade por referência. Nenhum bug identificado hoje; lacuna relevante antes de introduzir cache ou dupla materialização via EF Core. | Baixo | Nenhuma — mudança de comportamento adiada por decisão explícita desta Sprint |
| 7 | `CurrentUserId`/`CurrentUser` em `LevelUpData` — estado de sessão em documento de persistência. | Falso positivo / já rastreado | Nenhuma — já documentado em `01-current-state.md` §3.4 |
| 8 | Value Objects (`EmailAddress`, `UserName`, `Nickname`, `ActivityTitle`, `ActivityDescription`, `ProjectColor`) usados só como validadores transitórios, nunca como tipo persistido. | Falso positivo | Nenhuma — decisão deliberada da Sprint 12.8 (compatibilidade com `DomainJsonContractResolver`) |
| 9 | Herança compartilhada (`Activity`) entre 4 Aggregate Roots distintos (`Habit`, `RecurringTask`, `Project`, `Todo`). | Falso positivo | Nenhuma — reuso de implementação entre agregados independentes, sem fronteira transacional compartilhada. O único problema real dessa herança é o item #2, não a herança em si |

Nenhum achado foi classificado como crítico. Nenhuma invariante de negócio está sendo violada
silenciosamente hoje — confirmado pelos 665 testes automatizados existentes no momento da revisão.

## 4. Decisão explícita desta Sprint

A Sprint 13.1 termina sem nenhuma alteração de comportamento no Domain. Os achados #1, #2, #3, #4, #6
foram avaliados como corrigíveis com risco baixo a médio, mas foram deliberadamente adiados para
Sprints futuras a pedido explícito, para manter esta Sprint estritamente como revisão arquitetural.

## 5. Escopo explicitamente fora desta Sprint

Não foram criados nesta Sprint: Repository Pattern, Unit of Work, portas de persistência,
implementação SQL Server/EF Core, mecanismo de Domain Events. Esses itens pertencem à Sprint 13.2 e
seguintes, condicionados a este Aggregate Map.
