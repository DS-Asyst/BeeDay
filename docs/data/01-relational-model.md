# Modelo Relacional Definitivo (Sprint 14.1)

**Status:** validado — modelo relacional completo, verificado diretamente contra o código atual de
`LevelUp.Domain` (não contra o rascunho anterior deste documento nem contra os Maps de 13.1/13.2 de
memória). **Implementado via Fluent API na Sprint 14.3** — `IEntityTypeConfiguration<T>` para as 10
entidades, Owned/Complex Types, TPC escopado a `Activity`, migration `InitialCreate` gerada. Ver §0
(atualizado), §5.8 e `docs/data/02-ef-core-strategy.md` §0/§0.1 para o estado exato do que existe em
código hoje.
**Escopo:** exclusivamente o desenho de tabelas, colunas, chaves, constraints e índices do SQL Server.
Nenhum `DbContext`, nenhuma migration, nenhuma tabela real foi criada nesta Sprint. Nenhum código em
`src/`/`tests/` foi alterado.
**Fonte de verdade usada:** [`docs/architecture/05-domain-aggregate-map.md`](../architecture/05-domain-aggregate-map.md)
e [`docs/architecture/06-domain-persistence-map.md`](../architecture/06-domain-persistence-map.md), ambos
tratados como arquitetura aprovada e não reavaliados — mais uma leitura direta e independente do código
de `src/LevelUp.Domain` feita nesta Sprint, já que este documento (diferente dos dois acima) é
especificamente sobre SQL Server e portanto não pode se apoiar apenas em decisões agnósticas de
tecnologia.

---

## 0. Divergências encontradas e corrigidas nesta Sprint

O rascunho anterior deste documento foi escrito antes do Aggregate Map (Sprint 13.1) e nunca foi
corrigido — `06-domain-persistence-map.md` §0 já registrava duas divergências sem as corrigir aqui
("é um documento de uma tecnologia específica... sua correção pertence à Sprint que tratar da
modelagem SQL concreta"). Esta é essa Sprint. Uma leitura completa do código de `src/LevelUp.Domain`
confirmou as duas divergências já conhecidas e encontrou dez adicionais:

| # | Tabela/campo | Rascunho anterior | Realidade confirmada no código | Correção aplicada |
|---|---|---|---|---|
| 1 | `WalletTags` | `WalletId` como referência; único `(WalletId, NormalizedName)` | `WalletTag.UserId` existe; **não há `WalletId`** no arquivo (`WalletTag.cs`) | `WalletTags.UserId`; único `(UserId, Name)` |
| 2 | `Todos.ProjectId` | anulável | `Update`/`AssignTo` rejeitam `Guid.Empty` explicitamente — **obrigatório** | `ProjectId` `NOT NULL` |
| 3 | `Habits`/`RecurringTasks`/`Projects`/`Todos` | sem `Featured` | `Activity.Featured` (`bool`) é propriedade real da base compartilhada | `Featured bit NOT NULL` adicionado às quatro tabelas |
| 4 | `Transactions.TagId` | nome do campo | propriedade real chama-se `WalletTagId` | renomeado |
| 5 | `Transactions.OccurredOn` | nome do campo | propriedade real chama-se `TransactionDate` (`DateOnly`) | renomeado |
| 6 | `Transactions` | sem `Notes` | `Transaction.Notes` (`string`, ≤ 500) existe e é validado | `Notes nvarchar(500) NOT NULL` adicionado |
| 7 | `Projects.Status` | coluna armazenada | `Project.Status` é **inteiramente computado** a partir de `Todos` (sem setter, sem campo de apoio) | coluna removida — projeção de leitura, não persistência |
| 8 | `Habits`/`RecurringTasks`/`Projects`/`Todos.Position` | coluna armazenada | **não existe em Domain** — ordenação hoje é a ordem física da `List<T>` no JSON | mantida como decisão explícita desta Sprint (ver §5) — coluna de Infrastructure, não um campo de Domain |
| 9 | `RecurringTasks.DueDate` | coluna presente | **não existe** — `RecurringTask` só tem `Repeat` como propriedade própria | removida |
| 10 | `Habits`/`RecurringTasks`/`Todos.IsCompleted`/`CompletedAtUtc` | colunas separadas | conclusão vive inteiramente em `Activity.Completed` (bool herdado); **não existe timestamp de conclusão em lugar nenhum do Domain** | consolidado em `Completed bit`; nenhuma coluna `CompletedAtUtc` |
| 11 | `Users` | `NormalizedName`, `NormalizedNickname`, `NormalizedEmail`, `HasCompletedProfile`, `AvatarKey`, sem `EmailConfirmedAtUtc` | nenhuma dessas quatro colunas Normalized/HasCompletedProfile existe em `User.cs`; o campo real chama-se `Avatar` (não `AvatarKey`); `EmailConfirmedAtUtc` (`DateTimeOffset?`) existe e não estava no rascunho | colunas normalizadas removidas (ver §5 — decisão de collation); `AvatarKey` → `Avatar`; `HasCompletedProfile` não persistido (computado de `Nickname`); `EmailConfirmedAtUtc` adicionado |
| 12 | `ExperienceEntries` | `IdempotencyKey`, `MetadataJson`; sem `ExperienceBefore/After`, `LevelBefore/After` | **não existe** `IdempotencyKey` nem `MetadataJson` em `ExperienceEntry`/`ExperienceSource` — a deduplicação é uma regra de unicidade composta; quatro campos de auditoria (`ExperienceBefore/After: long`, `LevelBefore/After: int`) existem e não estavam no rascunho | `IdempotencyKey`/`MetadataJson` removidos; substituídos por índice único filtrado (ver §5); quatro colunas de auditoria adicionadas; `SourceDescription` (≤ 160) adicionado |
| 13 | `UserExperience.CurrentLevel` | coluna armazenada | `CurrentLevel` é **computado** a partir de `TotalExperience` via `ExperienceCurve.GetLevel` — sem campo de apoio | coluna removida |
| 14 | Todas as tabelas | `RowVersion` presente | nenhuma entidade de Domain tem `RowVersion`/token de concorrência — é decisão de Infrastructure, já aprovada em [`02-ef-core-strategy.md`](02-ef-core-strategy.md) §5 | mantido, mas explicitamente marcado como campo de Infrastructure em cada tabela, não um fato de Domain |

`ExperienceTransaction` (`Experience/ExperienceTransaction.cs`) foi confirmado como código morto — sem
nenhuma referência fora do próprio arquivo, já registrado como achado #1 do Aggregate Map (13.1) para
remoção em Sprint futura. Nenhuma tabela foi criada para ele.

---

## 1. Princípios

- banco iniciado vazio (ADR-002);
- chaves primárias `uniqueidentifier`, geradas pela aplicação (`Entity.Id = Guid.NewGuid()` no
  construtor de Domain) — nenhuma coluna usa `NEWID()`/`NEWSEQUENTIALID()` como default;
- timestamps de instante: `datetimeoffset`; datas civis (`DateOnly` em Domain): `date`;
- concorrência otimista com `rowversion` em toda entidade mutável — decisão de Infrastructure (ver §0
  item 14), não um campo de Domain;
- enums armazenados como `tinyint`/`int` com `CHECK` refletindo `Enum.IsDefined` (`EnumValidation.Defined`
  em Domain) — ver §4 para a tabela de valores; decisão de Infrastructure, Domain não define
  representação de armazenamento;
- unicidade case-insensitive (e-mail, nickname, nome de tag) resolvida pela collation padrão do SQL
  Server (`SQL_Latin1_General_CP1_CI_AS`), sem colunas `Normalized*` — ver §5.3;
- exclusões físicas inicialmente, salvo requisitos de auditoria (nenhum requisito de auditoria
  aprovado existe nesta Sprint — ver §5.1);
- nomes de tabela e coluna definidos por migrations (Sprint 14.2/14.3, fora do escopo aqui);
- nenhuma tabela `LevelUpData`;
- índices por ownership (`UserId`) e pelas consultas típicas já registradas em
  `06-domain-persistence-map.md` §8.

---

## 2. Tabelas

### Users

| Coluna | Tipo SQL Server | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `Name` | `nvarchar(100)` | não | `UserName.MaximumLength = 100` |
| `Email` | `nvarchar(254)` | não | `EmailAddress.MaximumLength = 254`; já normalizado (lowercase) por Domain antes de persistir |
| `PasswordHash` | `nvarchar(200)` | não | Domain não define tamanho máximo; 200 é escolha de Infrastructure, ajustável sem impacto em Domain |
| `IsActive` | `bit` | não | default `1` |
| `IsEmailConfirmed` | `bit` | não | default `0` |
| `EmailConfirmedAtUtc` | `datetimeoffset` | sim | ausente no rascunho anterior |
| `HasCompletedOnboarding` | `bit` | não | default `0` |
| `Nickname` | `nvarchar(20)` | não | `Nickname.MaximumLength = 20`; Domain nunca persiste `null`, usa `""` como "ainda não definido" — ver índice filtrado abaixo |
| `Avatar` | `nvarchar(200)` | não | nome real da propriedade (não `AvatarKey`); tamanho é escolha de Infrastructure |
| `Language` | `tinyint` | não | `UserLanguage`; default `0` (English) |
| `Theme` | `tinyint` | não | `UserTheme`; default `0` (System) |
| `SessionVersion` | `int` | não | default `1` |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `LastLoginAtUtc` | `datetimeoffset` | sim | |
| `RowVersion` | `rowversion` | não | campo de Infrastructure, ver §0 item 14 |

Constraints:

- `CHECK (Language IN (0,1))`, `CHECK (Theme IN (0,1,2))`.
- `UNIQUE INDEX UX_Users_Email (Email)`.
- `UNIQUE INDEX UX_Users_Nickname (Nickname) WHERE Nickname <> N''` — índice filtrado: `Nickname` vazio
  ("ainda não definido") é um valor real e repetível em Domain, não pode entrar numa constraint de
  unicidade comum. Ver §5.3 para a decisão de depender da collation padrão em vez de uma coluna
  `NormalizedNickname`.

`HasCompletedProfile` não é persistido: é `!string.IsNullOrEmpty(Nickname)` em Domain, sem campo de
apoio — deriva-se na leitura, não na tabela.

### UserTokens

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, `ON DELETE CASCADE` |
| `Type` | `tinyint` | não | `UserTokenType`; `CHECK (Type IN (1,2))` |
| `TokenHash` | `nvarchar(200)` | não | Domain não define tamanho máximo; 200 é escolha de Infrastructure |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `ExpiresAtUtc` | `datetimeoffset` | não | |
| `UsedAtUtc` | `datetimeoffset` | sim | |
| `RevokedAtUtc` | `datetimeoffset` | sim | |
| `RowVersion` | `rowversion` | não | |

Índices:

- `UNIQUE INDEX UX_UserTokens_Hash (TokenHash, Type)`.
- `INDEX IX_UserTokens_User (UserId, Type, ExpiresAtUtc)` — revogação em lote, checagem de ativos.

### Habits

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, `ON DELETE CASCADE` |
| `Title` | `nvarchar(100)` | não | `ActivityTitle.MaximumLength = 100` |
| `Description` | `nvarchar(500)` | não | `ActivityDescription.MaximumLength = 500`; Domain nunca persiste `null`, sempre `""` |
| `Featured` | `bit` | não | default `0` — ver §0 item 3 |
| `Attribute` | `tinyint` | sim | `ActivityAttribute`; `CHECK (Attribute IN (1,2,3,4) OR Attribute IS NULL)` |
| `Completed` | `bit` | não | default `0` — `Activity.Completed` herdado |
| `Direction` | `tinyint` | não | `HabitDirection`; `CHECK (Direction IN (0,1,2))`; default `2` (Both) |
| `Difficulty` | `tinyint` | não | `HabitDifficulty`; `CHECK (Difficulty IN (0,1,2,3))`; default `1` (Easy) |
| `ResetCounter` | `tinyint` | não | `HabitResetCounter`; `CHECK (ResetCounter IN (0,1,2))`; default `0` (Daily) |
| `PositiveCount` | `int` | não | default `0`; `checked` em Domain — overflow lança exceção antes de persistir |
| `NegativeCount` | `int` | não | default `0` |
| `Position` | `int` | não | campo de Infrastructure — ver §0 item 8 e §5.2 |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

Índice: `INDEX IX_Habits_User_Position (UserId, Position)`.

### RecurringTasks

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, `ON DELETE CASCADE` |
| `Title` | `nvarchar(100)` | não | |
| `Description` | `nvarchar(500)` | não | |
| `Featured` | `bit` | não | |
| `Attribute` | `tinyint` | sim | `ActivityAttribute`; `CHECK (Attribute IN (1,2,3,4) OR Attribute IS NULL)` |
| `Completed` | `bit` | não | |
| `Repeat` | `tinyint` | não | `TaskRepeat`; `CHECK (Repeat IN (0,1,2,3))`; default `1` (Daily) |
| `Position` | `int` | não | |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

Sem `DueDate`, sem `CompletedAtUtc` — ver §0 itens 9 e 10.

Índice: `INDEX IX_RecurringTasks_User_Position (UserId, Position)`.

### Projects

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, `ON DELETE CASCADE` |
| `Title` | `nvarchar(100)` | não | |
| `Description` | `nvarchar(500)` | não | |
| `Featured` | `bit` | não | |
| `Attribute` | `tinyint` | sim | `ActivityAttribute`; `CHECK (Attribute IN (1,2,3,4) OR Attribute IS NULL)` |
| `Color` | `nchar(7)` | não | `ProjectColor`, formato `#RRGGBB`; default `'#7A4FCB'`; `CHECK (Color LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]')` — Domain sempre normaliza para maiúsculas antes de persistir (construtor de `ProjectColor` aplica `ToUpperInvariant()`) |
| `ExpectedDate` | `date` | sim | `Project.ExpectedDate` (`DateOnly?`) |
| `Archived` | `bit` | não | default `0` |
| `Position` | `int` | não | |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

**Sem coluna `Status`** — inteiramente computado em Domain a partir de `Todos` (sem campo de apoio),
deriva-se numa projeção de leitura (mesmo padrão já usado por `IDashboardReadService`), não pertence à
tabela.

**`Completed` existe nesta tabela apesar do Domain nunca a ler para `Project`** — divergência técnica
descoberta e corrigida na Sprint 14.3, não uma decisão de modelagem. `Habit`/`RecurringTask`/`Project`/
`Todo` compartilham a base abstrata `Activity` sob TPC (§5.8); sob TPC, o EF Core só permite
configurar/ignorar uma propriedade herdada da base **uma única vez, na raiz da hierarquia** — não é
possível ignorá-la só para `Project`, mesmo sabendo que `Project.Completed` é um override totalmente
computado (`get => Status == ProjectStatus.Completed; protected set { }`, sem campo de apoio próprio) e
que os outros três tipos precisam do valor real. A correção usa modo de acesso por campo
(`UsePropertyAccessMode(PropertyAccessMode.Field)`) para gravar/ler diretamente o campo de apoio
herdado de `Activity.Completed`, que existe em todo `Project` mesmo sem ser lido pelo override — a
coluna acaba existindo na tabela `Projects`, mas nunca é lida pela lógica de negócio do agregado
`Project` (que sempre recalcula a partir de `Todos`). Ver `ActivityConfigurationExtensions.cs` para o
detalhe completo.

Índice: `INDEX IX_Projects_User_Position (UserId, Position)`.

### Todos

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, **`ON DELETE NO ACTION`** — ver §5.1 sobre múltiplos caminhos de cascade |
| `ProjectId` | `uniqueidentifier` | **não** | FK → `Projects(Id)`, `ON DELETE CASCADE` — corrigido de "anulável", ver §0 item 2 |
| `Title` | `nvarchar(100)` | não | |
| `Description` | `nvarchar(500)` | não | |
| `Featured` | `bit` | não | |
| `Attribute` | `tinyint` | sim | `ActivityAttribute`; `CHECK (Attribute IN (1,2,3,4) OR Attribute IS NULL)` |
| `DueDate` | `date` | sim | `Todo.DueDate` (`DateOnly?`) |
| `Completed` | `bit` | não | |
| `Position` | `int` | não | escopado por `ProjectId`, não por `UserId` — reordenação de Todos é sempre "dentro do mesmo Project" (Persistence Map §2.5) |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

Índices:

- `INDEX IX_Todos_Project_Position (ProjectId, Position)`.
- `INDEX IX_Todos_User (UserId, Id)` — suporta "por `(UserId, TodoId)`" sem já saber o Project (Persistence Map §2.5 item 7).

**Nota arquitetural — `Todos.UserId` não é garantido pelo banco como igual a `Projects.UserId`:** `Todo`
carrega `UserId` (herdado de `Activity`) e `ProjectId` simultaneamente (Ownership Matrix,
`06-domain-persistence-map.md` §3). O SQL Server não consegue impor sozinho, via `FOREIGN KEY`/`CHECK`
declarativa, que `Todos.UserId` seja sempre igual ao `UserId` do `Project` referenciado por
`Todos.ProjectId` — isso exigiria uma constraint que lê outra tabela, o que uma `CHECK` de coluna não
suporta nativamente no SQL Server sem uma function escalar (mesma classe de limitação já registrada para
a ownership de `WalletTag` em `Transactions`, §2 acima). Esta é uma invariante protegida por
Domain/Application, não pelo banco: `Project.AddTodo` é o único caminho de criação (`todo.AssignTo(Id)`),
e nenhum handler de Application deve permitir criar ou mover um `Todo` com um `UserId` diferente do
`UserId` do `Project` de destino. Nenhuma constraint artificial (trigger, function escalar, coluna
computada) foi introduzida para reforçar isso no banco — decisão consciente, não uma lacuna pendente.

### UserExperience

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `UserId` | `uniqueidentifier` | não | PK, FK → `Users(Id)`, `ON DELETE CASCADE` |
| `TotalExperience` | `bigint` | não | default `0` |
| `RowVersion` | `rowversion` | não | |

**Sem `CurrentLevel`** — computado por `ExperienceCurve.GetLevel(TotalExperience)`, sem campo de apoio
em Domain (ver §0 item 13). **Sem `UpdatedAtUtc`** — corrigido na Sprint 14.3: `Experience/UserExperience.cs`
não tem nenhuma propriedade de timestamp (só `TotalExperience` e `Entries`); diferente de `RowVersion`
(gerado pelo próprio SQL Server, sem depender de Domain), `UpdatedAtUtc` exigiria alguém preenchendo o
valor, e §5.6 já decidiu que só Application/Domain preenche `*AtUtc`, nunca o banco — não há fonte real
para essa coluna sem inventar um dado que Domain não tem. Mapeada na Sprint 14.3 como Owned Type
(`OwnsOne` a partir de `IEntityTypeConfiguration<User>`), tabela separada compartilhando a PK de `Users`.

### ExperienceEntries

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, `ON DELETE CASCADE` |
| `SourceType` | `tinyint` | não | `ExperienceSourceType`; `CHECK (SourceType BETWEEN 0 AND 6)` |
| `SourceId` | `uniqueidentifier` | sim | `ExperienceSource.ReferenceId` |
| `SourceDescription` | `nvarchar(160)` | não | `ExperienceSource.Description`, `MaximumDescriptionLength = 160`; ausente no rascunho anterior |
| `RewardType` | `tinyint` | não | `ExperienceRewardType`; `CHECK (RewardType = 0)` — único valor definido hoje (`Completion`) |
| `Amount` | `bigint` | não | |
| `ExperienceBefore` | `bigint` | não | ausente no rascunho anterior |
| `ExperienceAfter` | `bigint` | não | ausente no rascunho anterior |
| `LevelBefore` | `int` | não | ausente no rascunho anterior |
| `LevelAfter` | `int` | não | ausente no rascunho anterior |
| `GrantedAtUtc` | `datetimeoffset` | não | |

Tabela append-only (histórico de `User.Experience.Entries`) — **sem `RowVersion`**: nunca atualizada
nem removida fora do próprio agregado.

**Sem `IdempotencyKey`/`MetadataJson`** — não existem em Domain (ver §0 item 12). A deduplicação real,
lida diretamente de `UserExperience.EnsureValidState()`, é uma regra de unicidade composta que exclui
explicitamente a origem `Habit` (que pode repetir a mesma origem por natureza — ver Aggregate Map §2.3):

```sql
CREATE UNIQUE INDEX UX_ExperienceEntries_Dedup
    ON ExperienceEntries (UserId, SourceType, SourceId, RewardType)
    WHERE SourceId IS NOT NULL AND SourceType <> 0; -- 0 = Habit, isento por Domain
```

**Implementação (Sprint 14.3):** `ExperienceSource` (`Type`/`ReferenceId`/`Description`, colunas
`SourceType`/`SourceId`/`SourceDescription`) é mapeado como EF Core Complex Type (`ComplexProperty`,
não `OwnsOne`/Owned Type) a partir de `IEntityTypeConfiguration<ExperienceEntry>` — sem tabela própria,
sem identidade, exatamente como uma coluna a mais na mesma linha. O índice de dedup acima **não pôde
ser declarado via Fluent API**: confirmado, esgotando toda a superfície disponível (lambda, array de
strings, e até a API de metadata bruta `IMutableEntityType.AddIndex`), que o EF Core não consegue
expressar um índice que cruze propriedades próprias da entidade com propriedades de um Complex Type
aninhado — é uma limitação real da ferramenta, não uma escolha de mapeamento. O índice foi adicionado
via SQL bruto diretamente na migration (`migrationBuilder.Sql(...)` em `Migrations/*_InitialCreate.cs`),
a solução padrão e já conhecida para esse cenário específico.

Índice adicional: `INDEX IX_ExperienceEntries_User_Time (UserId, GrantedAtUtc)` — histórico.

**Nota arquitetural — `SourceId` é polimórfico por desenho, não um FK ausente por omissão:** `SourceId`
(`ExperienceSource.ReferenceId` em Domain) pode apontar para `Habits.Id`, `RecurringTasks.Id`,
`Todos.Id` ou `Projects.Id`, dependendo de `SourceType` (`Habit=0`/`Task=1`/`Todo=2`/`Project=3`); para
`Reading=4`/`Manual=5`/`System=6` não existe linha nenhuma para apontar, e `SourceId` é `NULL`. Uma
`FOREIGN KEY` declarativa exige uma única tabela de destino — impossível aqui sem quatro colunas de FK
mutuamente exclusivas, o que o Domain não modela e esta revisão não introduz (adicionar colunas está
fora do escopo). Mais importante: mesmo que fosse tecnicamente viável, uma FK seria semanticamente
errada. `ExperienceEntries` é histórico — uma cópia do que aconteceu no momento da concessão de XP, não
uma referência viva ao agregado de origem. O Aggregate Map já registra exatamente essa distinção para o
caso de exclusão de `Habit`/`RecurringTask`: "histórico de XP já concedido permanece em
`User.Experience`, pois é uma cópia de dados no momento da concessão, não uma referência viva"
(`05-domain-aggregate-map.md` §7). Uma FK com `CASCADE` apagaria história de XP legítima ao excluir a
atividade de origem; `NO ACTION` impediria excluir a atividade enquanto XP dela existisse (nunca a
intenção); `SET NULL` destruiria a rastreabilidade sem necessidade. Nenhuma dessas opções é aceitável —
a ausência de FK é a modelagem correta, não uma lacuna. Não adicionar uma FK aqui em Sprints futuras.

### Wallets

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, **`ON DELETE NO ACTION`** — ver §5.1 |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

Índice: `UNIQUE INDEX UX_Wallets_User (UserId)` — um Wallet por User (`LevelUpData.AddWallet`).

### WalletTags

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `UserId` | `uniqueidentifier` | não | FK → `Users(Id)`, `ON DELETE CASCADE` — corrigido de `WalletId`, ver §0 item 1 |
| `Name` | `nvarchar(40)` | não | `WalletTag.MaximumNameLength = 40` |
| `Color` | `nchar(7)` | não | default `'#7A4FCB'`; `CHECK (Color LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]')` — Domain sempre normaliza para maiúsculas antes de persistir (`WalletTag.NormalizeColor` aplica `ToUpperInvariant()` antes do regex) |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

Índice: `UNIQUE INDEX UX_WalletTags_User_Name (UserId, Name)` — depende da collation padrão
case-insensitive do SQL Server; sem coluna `NormalizedName` (ver §5.3).

### Transactions

| Coluna | Tipo | Nulo | Notas |
|---|---|---|---|
| `Id` | `uniqueidentifier` | não | PK |
| `WalletId` | `uniqueidentifier` | não | FK → `Wallets(Id)`, `ON DELETE CASCADE` |
| `WalletTagId` | `uniqueidentifier` | sim | FK → `WalletTags(Id)`, `ON DELETE SET NULL`; renomeado de `TagId`, ver §0 item 4 |
| `Description` | `nvarchar(120)` | não | `Transaction.MaximumDescriptionLength = 120` |
| `Notes` | `nvarchar(500)` | não | `Transaction.MaximumNotesLength = 500`; ausente no rascunho anterior, ver §0 item 6 |
| `Amount` | `decimal(19,2)` | não | `CHECK (Amount > 0)`; Domain exige no máximo 2 casas decimais |
| `Type` | `tinyint` | não | `TransactionType`; `CHECK (Type IN (1,2))` |
| `TransactionDate` | `date` | não | renomeado de `OccurredOn`, ver §0 item 5 |
| `CreatedAtUtc` | `datetimeoffset` | não | |
| `UpdatedAtUtc` | `datetimeoffset` | não | |
| `RowVersion` | `rowversion` | não | |

Índices:

- `INDEX IX_Transactions_Wallet_Date (WalletId, TransactionDate)` — extrato, saldo, totais.
- `INDEX IX_Transactions_Tag (WalletTagId)` — contagem de uso antes de excluir uma tag.

**Invariante cross-aggregate não modelável como FK simples:** a `WalletTag` referenciada deve pertencer
ao mesmo `UserId` do `Wallet` (`LevelUpData.ValidateTransactionTagOwnership`). Isso cruza duas cadeias
de ownership diferentes (`Transaction → Wallet → UserId` e `Transaction → WalletTag → UserId`) que o
SQL Server não consegue expressar como uma única `FOREIGN KEY`/`CHECK` declarativa sem uma function
escalar (evitado deliberadamente — colocaria regra de negócio dentro do banco). Permanece validação de
Application, como já registrado em `06-domain-persistence-map.md` §2.7.

---

## 3. Relações

```text
User 1 ── * UserTokens                          (CASCADE)
User 1 ── * Habits                               (CASCADE)
User 1 ── * RecurringTasks                       (CASCADE)
User 1 ── * Projects                             (CASCADE)
Project 1 ── * Todos                             (CASCADE)
User 1 ── * Todos                                (NO ACTION — ver §5.1)
User 1 ── 1 UserExperience                       (CASCADE)
User 1 ── * ExperienceEntries                    (CASCADE)
User 1 ── 1 Wallet                               (NO ACTION — ver §5.1)
Wallet 1 ── * Transactions                       (CASCADE)
User 1 ── * WalletTags                           (CASCADE)
WalletTag 1 ── * Transactions (WalletTagId)      (SET NULL)
```

Nada em `Profile` (`Entities/Profile.cs`) — é uma view construída a partir de campos já existentes em
`User`/`UserExperience` a cada chamada, sem `Id`, sem base `Entity`, sem persistência própria.

---

## 4. Enums — valores e representação de armazenamento

Nenhum enum de Domain é `[Flags]`; nenhum tem atributo de serialização (`System.Text.Json` não é
referenciado por `LevelUp.Domain`). A representação abaixo (`tinyint` + `CHECK`) é decisão de
Infrastructure — Domain não define como um enum é armazenado.

| Enum | Valores (nome = valor declarado) | Coluna(s) que usam |
|---|---|---|
| `ActivityAttribute` | Strength=1, Dexterity=2, Intelligence=3, Vitality=4 | `Habits/RecurringTasks/Projects/Todos.Attribute` |
| `ExperienceRewardType` | Completion=0 | `ExperienceEntries.RewardType` |
| `ExperienceSourceType` | Habit=0, Task=1, Todo=2, Project=3, Reading=4, Manual=5, System=6 | `ExperienceEntries.SourceType` |
| `HabitDifficulty` | Trivial=0, Easy=1, Medium=2, Hard=3 | `Habits.Difficulty` |
| `HabitDirection` | Positive=0, Negative=1, Both=2 | `Habits.Direction` |
| `HabitResetCounter` | Daily=0, Weekly=1, Monthly=2 | `Habits.ResetCounter` |
| `TaskRepeat` | None=0, Daily=1, Weekly=2, Monthly=3 | `RecurringTasks.Repeat` |
| `TransactionType` | Income=1, Expense=2 | `Transactions.Type` |
| `UserLanguage` | English=0, Portuguese=1 | `Users.Language` |
| `UserTheme` | System=0, Light=1, Dark=2 | `Users.Theme` |
| `UserTokenType` | EmailConfirmation=1, PasswordReset=2 | `UserTokens.Type` |

`ProjectStatus` não aparece aqui — não é persistido (§0 item 7, §2 `Projects`).

---

## 5. Decisões desta Sprint (requerem nenhuma ação de código — registradas para a Sprint 14.2/14.3)

### 5.1 Múltiplos caminhos de cascade (restrição real do SQL Server)

Aplicar `ON DELETE CASCADE` em toda FK de ownership, ingenuamente, cria dois conflitos que o SQL Server
rejeita na criação da constraint (erro 1785, "may cause cycles or multiple cascade paths"):

- `Users → Projects → Todos` (CASCADE) coexistindo com `Users → Todos` direto (CASCADE) — dois caminhos
  convergindo em `Todos`.
- `Users → Wallets → Transactions` (CASCADE) coexistindo com `Users → WalletTags → Transactions`
  (CASCADE até `WalletTags`, depois `SET NULL` até `Transactions`) — dois caminhos convergindo em
  `Transactions`; `SET NULL` conta como caminho de cascade para esta restrição, não apenas `CASCADE`.

Resolução aplicada: quebrar o caminho redundante, mantendo o caminho mais específico e semanticamente
correto:

- `Todos.UserId` → `ON DELETE NO ACTION`. A limpeza de `Todos` ao excluir um `User` continua acontecendo
  de qualquer forma, por transitividade via `Users → Projects (CASCADE) → Todos (CASCADE)` — a FK direta
  em `UserId` existe só para leitura/ownership, nunca precisou ser a via de exclusão (o próprio Domain
  já trata `Todo` como sem ciclo de vida independente de `Project`, ver Aggregate Map §2.5).
- `Wallets.UserId` → `ON DELETE NO ACTION`. Hoje não existe operação de exclusão de `Wallet` em
  Domain/Application (Aggregate Map §7 — "Sem operação de exclusão hoje"); quando essa operação existir,
  será uma exclusão explícita de `Wallet`, que já cascateia para suas `Transactions` via
  `Wallets → Transactions (CASCADE)`, preservando intacto o comportamento mais importante e ativamente
  usado hoje: `WalletTag → Transactions (SET NULL)` ao excluir uma tag.

Nenhuma dessas duas mudanças reduz uma capacidade real hoje: exclusão de `User` não é uma operação
implementada em Domain/Application em nenhum dos dois casos.

### 5.2 Coluna `Position`

Não existe em Domain — confirmado por leitura direta do código (`Habit`, `RecurringTask`, `Project`,
`Todo`, `Activity` não têm nenhuma propriedade de ordinal). A ordenação hoje é a ordem física da
`List<T>` dentro do documento JSON. SQL Server não garante ordem implícita de linhas; sem uma coluna
ordinal explícita, reordenar (`ReorderHabitsAsync`, `ReorderTodosAsync`, etc., já contratados em
`07-persistence-contracts.md`) não seria representável. Decisão aprovada nesta Sprint: adicionar
`Position int NOT NULL` como campo exclusivo de Infrastructure/persistência — não um fato de Domain, não
deve ser adicionado a nenhuma entidade de `LevelUp.Domain`. Escopo do ordinal: por `UserId` para
Habits/RecurringTasks/Projects; por `ProjectId` para Todos (reordenação de Todos é sempre dentro do
mesmo Project, nunca global ao usuário — Persistence Map §2.5).

### 5.3 Unicidade case-insensitive sem colunas `Normalized*`

`User.Email` já chega normalizado (lowercase) em Domain antes de persistir (`EmailAddress.Create`).
`User.Nickname` preserva a capitalização original do usuário, mas a unicidade é checada
case-insensitivamente em `LevelUpData.CompleteUserProfile`/`EnsureUniqueNicknames`. `WalletTag.Name`
segue o mesmo padrão (`LevelUpData.AddWalletTag`/`EnsureUniqueWalletTagNames`). Decisão aprovada: usar a
collation padrão do SQL Server (`SQL_Latin1_General_CP1_CI_AS`, case-insensitive) nos índices únicos de
`Users.Email`, `Users.Nickname` e `WalletTags(UserId, Name)`, sem introduzir colunas
`NormalizedEmail`/`NormalizedNickname`/`NormalizedName` — nenhuma delas existe como fato de Domain, e uma
coluna normalizada duplicada é uma segunda cópia do valor que pode divergir do original. Se o banco de
destino algum dia usar uma collation case-sensitive, esta decisão precisa ser revisitada explicitamente
— registrado aqui para essa eventualidade, não implementado como contingência agora.

### 5.4 `OutboxMessages`/`AuditEntries` — adiados

O rascunho anterior incluía as duas tabelas especulativamente. Nenhum requisito aprovado hoje as exige:
infraestrutura de Domain Events não existe (Aggregate Map achado #4), e `04-runtime-flows.md` §6 só
menciona Outbox como necessidade futura condicional ("quando se tornarem críticas"). Consistente com a
regra desta Sprint ("dados técnicos estritamente indispensáveis, apenas se forem realmente necessários e
previamente aprovados"), nenhuma das duas tabelas está neste modelo. Adicionar quando uma Sprint futura
aprovar e implementar Domain Events/Outbox como mecanismo real.

### 5.5 Estratégia de chaves GUID — geração, tipo, índice clusterizado

- **Geração:** `Guid.NewGuid()` dentro de `Entity` (`Abstractions/Entity.cs`), em memória, no momento em
  que o agregado de Domain é construído — nunca gerado pelo SQL Server. É um GUID aleatório (v4), não
  sequencial; `Id` é `private set`, nunca reatribuído após a construção.
- **Tipo SQL:** `uniqueidentifier` em toda tabela — já declarado em §1, sem alteração.
- **Impacto em índices:** um `uniqueidentifier` aleatório como chave de um índice `CLUSTERED` insere
  linhas em posições aleatórias dentro das páginas de dados, causando divisões de página e fragmentação
  proporcional ao volume de inserção — risco concentrado nas tabelas de maior volume de escrita
  (`Transactions`, `ExperienceEntries`, `UserTokens`), irrelevante nas de baixo volume (`Users`,
  `Wallets`, `WalletTags`, `UserExperience`).
- **Decisão aprovada:** `Id` permanece `PRIMARY KEY` **e** índice `CLUSTERED` — o comportamento padrão do
  SQL Server ao declarar uma `PRIMARY KEY` sem `NONCLUSTERED` explícito. Nenhuma coluna adicional de
  clusterização foi introduzida. A fragmentação resultante é um custo operacional aceito nesta decisão, a
  ser mitigado por manutenção de índice periódica (`ALTER INDEX ... REORGANIZE`/`REBUILD`) na operação do
  banco, não por mudança de schema.
- **Alternativas descartadas nesta revisão**, por exigirem alterar Domain ou adicionar colunas — ambos
  fora do escopo desta atividade: GUID sequencial gerado por Domain; `NEWSEQUENTIALID()` como `DEFAULT`
  (incompatível com o princípio já aprovado de que todo `Id` é fornecido pela aplicação, nunca gerado
  pelo banco — §1); coluna surrogate de clusterização separada da `PRIMARY KEY`.

### 5.6 Responsabilidade de preenchimento dos timestamps (UTC)

- **Responsável: Application/Domain — nunca o SQL Server.** Toda entidade de Domain já chega em
  Infrastructure com `CreatedAtUtc`/`UpdatedAtUtc`/demais timestamps preenchidos antes de qualquer
  persistência — por inicializador de campo (`= DateTimeOffset.UtcNow` em `Activity`, `Wallet`,
  `WalletTag`, `Transaction`, `ExperienceEntry`) ou por parâmetro explícito validado no construtor
  (`User.Create(..., createdAtUtc)`, que lança `DomainValidationException` se `createdAtUtc == default`).
  Em nenhum dos dois casos o valor é decidido pela Infrastructure ou pelo banco.
- **Consequência para o schema:** nenhuma coluna `*AtUtc` deve ter `DEFAULT SYSUTCDATETIME()`/
  `DEFAULT GETUTCDATE()`. Um default no banco seria redundante (o EF Core sempre envia um valor
  explícito, já materializado no objeto de Domain) e arriscado (duas fontes de verdade para o mesmo
  instante, caso o relógio da aplicação e o do banco divirjam).
- **Tipo SQL confirmado:** `datetimeoffset(7)` (precisão padrão do SQL Server, 100ns) para todo instante;
  `date` para toda data civil (`DateOnly` em Domain: `Project.ExpectedDate`, `Todo.DueDate`,
  `Transaction.TransactionDate`). Escolha explícita de manter a precisão padrão do SQL Server — nenhuma
  tabela reduz para `datetimeoffset(3)` ou outra precisão menor.

### 5.7 Convenção de nomenclatura de constraints e índices

| Tipo | Padrão | Exemplo |
|---|---|---|
| Chave primária | `PK_<Tabela>` | `PK_Users` |
| Chave estrangeira | `FK_<TabelaOrigem>_<TabelaDestino>_<Coluna>` | `FK_Todos_Projects_ProjectId` |
| Índice não único | `IX_<Tabela>_<Coluna(s)>` | `IX_Habits_User_Position` (já usado) |
| Índice único | `UX_<Tabela>_<Coluna(s)>` | `UX_Users_Email` (já usado) |
| Check constraint | `CK_<Tabela>_<Coluna>` | `CK_Habits_Direction` |

Convenção vinculante para a Sprint 14.2/14.3 — substitui a ressalva anterior de §6 de que nomes de
constraint/índice "não são uma exigência literal de migration". Os nomes de índice já usados nas tabelas
deste documento (`UX_`/`IX_`) já seguem este padrão e devem ser mantidos literalmente. Nomes individuais
de `PRIMARY KEY`/`FOREIGN KEY`/`CHECK` não foram enumerados tabela a tabela nesta revisão, para não
reestruturar o documento inteiro — aplicar o padrão acima a cada constraint é trabalho da Sprint 14.2 ao
escrever `IEntityTypeConfiguration<T>`.

### 5.8 Estratégia de herança implementada (Sprint 14.3)

TPC (`UseTpcMappingStrategy()`), escopado exclusivamente a `Activity` — não mais no repositório `Entity`,
como uma versão anterior da fundação (Sprint 14.2) chegou a declarar antes de ser corrigida. `Activity`
é o único tipo abstrato deste modelo cujos descendentes concretos múltiplos (`Habit`/`RecurringTask`/
`Project`/`Todo`) são cada um exposto por seu próprio `DbSet` — exatamente a forma que aciona o default
table-per-hierarchy do EF Core (uma tabela só, com discriminador), que não corresponde a este documento
(4 tabelas independentes). Os outros 6 tipos mapeados nunca tiveram essa ambiguidade — confirmado
empiricamente, já na Sprint 14.2, por testes que passam com cada um em sua própria tabela sem nenhuma
configuração de herança. TPC (não TPT) porque cada tabela é totalmente autocontida, sem tabela-base
compartilhada nem joins — exatamente o que este documento sempre definiu. Nenhuma chave é `IDENTITY`
(todo `Id` é `Guid` atribuído por Domain), então a ressalva histórica de TPC sobre geração de chave não
se aplica. Uma consequência colateral confirmada — `Projects` ganha uma coluna `Completed` que o Domain
nunca lê para esse tipo — está documentada na tabela `Projects` (§2) e em
`ActivityConfigurationExtensions.cs`.

---

## 6. O que este documento não decide

- nomes literais de cada constraint individual, tabela a tabela (a convenção `PK_`/`FK_`/`IX_`/`UX_`/`CK_`
  é definida e vinculante em §5.7; aplicar o padrão a cada uma das dezenas de constraints é trabalho de
  implementação da Sprint 14.2 ao escrever `IEntityTypeConfiguration<T>`, não uma enumeração exaustiva
  aqui);
- estratégia de particionamento, arquivamento ou retenção;
- qualquer schema de banco além de `dbo`;
- a tabela de histórico de migrations do EF Core (`__EFMigrationsHistory`) — criada automaticamente pela
  ferramenta, não desenhada aqui;
- implementação de `IEntityTypeConfiguration<T>`, `DbContext`, migrations — pertence às Sprints 14.2/14.3
  (ver [`02-ef-core-strategy.md`](02-ef-core-strategy.md)).
