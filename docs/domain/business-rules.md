# Business Rules / Invariants

**Fonte da verdade:** cada regra abaixo foi extraída diretamente do código-fonte citado —
nenhuma regra foi inferida ou presumida sem uma linha de código correspondente que a imponha
(lançamento de exceção, no-op condicional, ou normalização). Os testes citados foram confirmados
por nome de método real em `tests/BeeDay.Domain.Tests/`, obtidos por grep, não por leitura de
conteúdo completo de cada arquivo de teste.

## User

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| Data de criação obrigatória | `User.cs`, `Create` | Auditoria/consistência de timestamps | `DomainValidationException` | — |
| Perfil só pode ser completado uma vez | `User.cs`, `CompleteProfile` | Nickname é definido uma vez, não é um campo de edição livre | `InvalidDomainStateException` | `UserProfileRulesTests.CompleteUserProfile_RejectsCompletingProfileTwice` |
| Confirmação de e-mail não pode preceder a criação da conta | `User.cs`, `ConfirmEmail` | Consistência temporal | `DomainValidationException` | `UserIdentityTokenTests.ConfirmEmail_ChangesConfirmationState` |
| Confirmação de e-mail é idempotente | `User.cs`, `ConfirmEmail` | Reenvio de confirmação não deve duplicar efeito | No-op silencioso na segunda chamada | `UserIdentityTokenTests.ConfirmEmail_IsIdempotent` |
| `SetPasswordHash` nunca invalida sessão por si só | `User.cs`, comentário XML doc de `InvalidateSessions` | Re-hash transparente no login não pode encerrar a sessão sendo criada | N/A — ausência deliberada de efeito colateral | — |
| Desativação sempre invalida sessões | `User.cs`, `SetActive` | Uma conta desativada não pode manter sessões ativas | `InvalidateSessions()` chamado incondicionalmente quando `active=false` | `UserSessionHardeningTests.SetActive_False_InvalidatesSessions`, `SetActive_True_DoesNotInvalidateSessions` |
| Nova conta começa em `SessionVersion = 1` | `User.cs`, campo | Baseline verificável | — | `UserSessionHardeningTests.NewUser_StartsAtSessionVersionOne` |
| `InvalidateSessions` é cumulativo | `User.cs`, `InvalidateSessions` | Múltiplas invalidações devem compor, não saturar | Incrementa a cada chamada | `UserSessionHardeningTests.InvalidateSessions_CalledTwice_AdvancesTwice` |
| `SessionVersion` não pode sofrer wraparound silencioso | `User.cs`, `InvalidateSessions` | Uma versão antiga jamais pode voltar a parecer atual | `OverflowException` por incremento `checked` | — |

## UserToken

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| `UserId` obrigatório | `UserToken.cs`, `Create` | Token sempre pertence a um usuário | `DomainValidationException` | — |
| Data de criação obrigatória | `UserToken.cs`, `Create` | A janela de validade precisa de início real | `DomainValidationException` | `DomainInvariantAuditTests.UserToken_RejectsDefaultCreationTimestamp` |
| Expiração deve ser posterior à criação | `UserToken.cs`, `Create` | Token que já nasce expirado é um bug de chamador | `DomainValidationException` | `UserIdentityTokenTests.Token_Create_RejectsInvalidExpiration` |
| Token não pode ser usado antes da criação | `UserToken.cs`, `EnsureCanBeUsed` | A validade é uma janela, não apenas um prazo final | `InvalidDomainStateException` | `DomainInvariantAuditTests.UserToken_CannotBeUsedBeforeCreation` |
| Token expirado não pode ser usado | `UserToken.cs`, `EnsureCanBeUsed` | Janela de validade de segurança | `InvalidDomainStateException` | `UserIdentityTokenTests.ExpiredToken_CannotBeUsed` |
| Token usado não pode ser reusado | `UserToken.cs`, `EnsureCanBeUsed` | Uso único (proteção contra replay) | `InvalidDomainStateException` | `UserIdentityTokenTests.UsedToken_CannotBeReused` |
| Token revogado não pode ser usado | `UserToken.cs`, `EnsureCanBeUsed` | Revogação explícita (novo token substitui o antigo) | `InvalidDomainStateException` | `UserIdentityTokenTests.RevokedToken_CannotBeUsed` |
| Token de um tipo não serve para outro propósito | `UserToken.cs`, `EnsureCanBeUsed` | Token de reset de senha não pode confirmar e-mail e vice-versa | `InvalidDomainStateException` | `UserIdentityTokenTests.PasswordResetToken_CannotConfirmEmail`, `EmailConfirmationToken_CannotResetPassword` |
| Revogação não é retroativa | `UserToken.cs`, `Revoke` | Consistência temporal | `DomainValidationException` se `revokedAtUtc < CreatedAtUtc` | — |

## Activity (base de Habit/RecurringTask/Project/Todo)

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| `UserId` obrigatório ao atribuir dono | `Activity.cs`, `AssignOwner` | Toda Activity pertence a um usuário | `ArgumentException` | — |
| Owner não pode ser transferido diretamente | `Activity.cs`, `AssignOwner` | Impede cruzamento silencioso de dados entre usuários | `InvalidDomainStateException`; repetir o mesmo owner é idempotente | `DomainInvariantAuditTests.ActivityOwner_CannotBeReassigned` |
| Título/descrição sempre normalizados via VO | `Activity.cs`, `UpdateDetails` | Consistência de formatação em toda a hierarquia | `DomainValidationException` (delegada ao VO) | `ValueObjectTests.ActivityTitle_NormalizesWhitespace`, `ActivityDescription_RejectsTextAboveLimit` |
| `Attribute` deve ser um enum válido, se fornecido | `Activity.cs`, `SetAttribute` | Consistência de dado persistido | `DomainValidationException` | `ActivityAttributeTests.SetAttribute_RejectsUndefinedValue` |

## Habit

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| Direção restringe qual contador avança | `Habit.cs`, `RegisterPositive`/`RegisterNegative` | Um hábito "só negativo" não deve acumular reforço positivo, e vice-versa | No-op silencioso (sem exceção) | `HabitTests.RegisterPositive_DoesNotChangeNegativeOnlyHabit` |
| Contadores protegidos contra overflow | `Habit.cs`, `checked(...)` | Consistência de dado em uso de longuíssimo prazo | `OverflowException` | — |
| Título obrigatório | `Activity.cs`/`ActivityTitle`, via `Update` | Herdada de Activity | `DomainValidationException` | `HabitTests.Create_RejectsEmptyTitle` |
| Direção deve ser um enum válido | `Habit.cs`, `Update` | Consistência de dado | `DomainValidationException` | `HabitTests.Create_RejectsUndefinedDirection` |

## Project / Todo

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| `Completed` de Project é sempre computado | `Project.cs`, propriedade `Completed` (override) | Refletir o estado real dos Todos, não um campo editável independentemente | Setter vazio — atribuição aceita sintaticamente, sem efeito | `ProjectTests.Empty_project_is_planned_with_zero_progress` |
| Project não pode ser completado manualmente | `Project.cs`, `ToggleCompletion` (override) | Conclusão só é significativa via conclusão de todos os Todos | `InvalidDomainStateException` | `ProjectTests.Project_cannot_be_completed_manually` |
| Completar todos os Todos completa o Project automaticamente | `Project.cs`, propriedade `Status` (computada) | Regra de negócio central do agregado | — (efeito automático, não exceção) | `ProjectTests.Completing_all_todos_completes_project_automatically` |
| Reabrir um Todo tira o Project de "Completed" | `Project.cs`, `Status` | Mesma regra, direção inversa | — | `ProjectTests.Reopening_todo_returns_completed_project_to_in_progress` |
| Todo só é adicionado via `Project.AddTodo` | `Project.cs`, `AddTodo` | Garantir `ProjectId` consistente | `AssignTo` (internal) é o único caminho que define `ProjectId` a partir do agregado pai | — |
| `Todo.ProjectId` nunca pode ser vazio | `Todo.cs`, `Update`/`AssignTo` | Todo sem projeto não tem sentido de negócio | `DomainValidationException` | — |
| `Todo.Update` não move entre Projects | `Todo.cs`, `Update` | A mudança de aggregate exige o caminho do Project | `InvalidDomainStateException` | `DomainInvariantAuditTests.TodoProject_CanOnlyChangeThroughTheOwningProject` |
| Project não aceita o mesmo To-Do duas vezes | `Project.cs`, `AddTodo` | A coleção do aggregate não pode conter identidade duplicada | `InvalidDomainStateException` | `DomainInvariantAuditTests.Project_RejectsTheSameTodoTwice` |
| Project não aceita To-Do de outro owner | `Project.cs`, `AddTodo` | Impede composição cross-user | `InvalidDomainStateException`; owner ausente é herdado do Project | `DomainInvariantAuditTests.Project_RejectsTodoOwnedByAnotherUser` |

## Wallet

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| `UserId` obrigatório | `Wallet.cs`, `Create` | Wallet sempre pertence a um usuário | `DomainValidationException` | `WalletTests.Create_requires_user_identifier` |
| Cálculos sempre filtram por `WalletId` | `Wallet.cs`, `FilterTransactions` (privado) | Uma Wallet nunca deve somar transações de outra | Filtragem silenciosa, não exceção | `WalletTests.Calculations_ignore_transactions_from_another_wallet` |
| Saldo pode ser negativo | `Wallet.cs`, `CalculateBalance` | Não há trava de saldo mínimo no Domain | N/A — comportamento permitido, não uma restrição | `WalletTests.Balance_can_be_negative` |

## WalletTag

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| Nome obrigatório e normalizado (colapso de espaços) | `WalletTag.cs`, `NormalizeName` | Consistência de exibição/busca | `DomainValidationException` se vazio | `WalletTagTests.Create_normalizes_name_and_color` |
| Nome limitado a 40 caracteres | `WalletTag.cs`, `NormalizeName` | Restrição de UI/schema | `DomainValidationException` | — |
| Cor deve casar com `#RRGGBB`, senão usa padrão | `WalletTag.cs`, `NormalizeColor` | Cor ausente é aceitável (usa `DefaultColor`); cor presente e inválida não é | `DomainValidationException` só se uma cor *inválida* for explicitamente fornecida | `WalletTagTests.Create_uses_default_color`, `Create_rejects_invalid_colors` |

## Transaction

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| `Amount` deve ser positivo | `Transaction.cs`, `ValidateAmount` | Sinal vem de `Type`, não de `Amount` | `DomainValidationException` | `TransactionTests.Create_rejects_invalid_amounts` |
| `Amount` no máximo 2 casas decimais | `Transaction.cs`, `ValidateAmount` | Consistência monetária | `DomainValidationException` | `TransactionTests.Create_rejects_invalid_amounts` |
| `Amount` não excede `999999999999` | `Transaction.cs`, `ValidateAmount` | Mantém o contrato monetário público representável no Domain | `DomainValidationException` | `DomainInvariantAuditTests.Transaction_RejectsAmountAboveTheSupportedBusinessMaximum` |
| `SignedAmount` deriva de `Type` | `Transaction.cs`, propriedade `SignedAmount` | Fonte única de verdade para o sinal | — | `TransactionTests.Create_sets_signed_amount_from_type` |
| Descrição obrigatória, normalizada, máx. 120 caracteres | `Transaction.cs`, `ValidateDescription` | Consistência de exibição | `DomainValidationException` | `TransactionTests.Create_rejects_empty_description_invalid_type_and_date` |
| `WalletTagId`, se fornecido, não pode ser vazio | `Transaction.cs`, `ValidateTagId` | Distinguir "sem tag" (`null`) de "tag inválida" (`Guid.Empty`) | `DomainValidationException` | `TransactionTests.Tag_can_be_assigned_and_removed` |
| Notas limitadas a 500 caracteres | `Transaction.cs`, `ValidateNotes` | Restrição de schema | `DomainValidationException` | — |

## Experience (subsistema, dentro de User)

| Regra | Arquivo / Método | Motivo | Violação | Teste |
|---|---|---|---|---|
| Recompensa deve ser positiva | `ExperienceReward.cs`, `Create` | XP negativo/zero não faz sentido | `DomainValidationException` | `ExperienceDomainTests.Reward_rejects_non_positive_experience` |
| `TotalExperience` nunca excede `long.MaxValue` | `UserExperience.cs`, `Add` | Proteção de overflow | `InvalidDomainStateException` | — |
| Concessão automática de XP é deduplicada | `UserExperience.cs`, `TryAdd` | Evita conceder XP duas vezes pela mesma ação (ex. duplo clique) | Retorna `null` silenciosamente em vez de lançar | `ExperienceRewardPipelineTests` (arquivo, `tests/BeeDay.Application.Tests/`) |
| Concessão automática exige `Source.ReferenceId` | `UserExperience.cs`, `TryAdd` | Deduplicação depende de uma referência de origem | `DomainValidationException` | — |
| Curva de nível é determinística e monotônica | `LinearExperienceCurve.cs`, `GetLevel` | Nível nunca pode "regredir" pela mesma quantidade total de XP | — | `ExperienceDomainTests.Curve_derives_level_progress_and_remaining_experience` |
| Nível "level up" é detectado por comparação antes/depois no Domain | `UserExperience.cs` (`levelBefore`/`levelAfter` em `Add`), consumido por `ExperienceRewardEventPublisher` em Application | A decisão de "subiu de nível" não deve ser recalculada em duas camadas diferentes | — | `ExperienceDomainTests.Add_experience_updates_total_and_records_source_history` |
| Histórico de XP revalida toda a transição | `ExperienceEntry.cs`, `Create` | Impede reward, total, nível ou timestamp inconsistentes mesmo por chamada direta | `DomainValidationException` | `DomainInvariantAuditTests.ExperienceEntry_RejectsInconsistentExperienceTransition` |
| Source de XP tem igualdade por valor | `ExperienceSource.cs`, record | Identidade da origem depende dos valores, não da referência CLR | Igualdade estrutural | `DomainInvariantAuditTests.ExperienceSource_UsesValueEquality` |

**Exclusão de item recompensado não revoga XP concedido (`BD30-F062`, decisão de persistência, não
invariante de Domain)**: apagar um Habit/RecurringTask/Todo/Project que já gerou um
`ExperienceEntry` não reverte o XP concedido. Não há FK entre `ExperienceEntries.SourceId` e a
origem — `ExperienceEntries` é histórico (uma cópia do que aconteceu no momento da concessão), não
uma referência viva ao agregado de origem. A decisão está corretamente implementada e comentada em
`src/BeeDay.Infrastructure/Persistence/SqlServer/Configurations/ExperienceEntryConfiguration.cs`; ver
também `docs/persistence/01-relational-model.md` §2 "ExperienceEntries".

## Fontes de verdade

**Arquivos consultados:** todos os arquivos de `src/BeeDay.Domain/Entities/`,
`src/BeeDay.Domain/Experience/`, `src/BeeDay.Domain/ValueObjects/` (mesmos já citados nos
documentos por agregado); `src/BeeDay.Infrastructure/Persistence/SqlServer/Configurations/
ExperienceEntryConfiguration.cs` (Sprint 30.28, `BD30-F062` — a única citação de Infrastructure
neste documento, necessária porque a decisão de não revogar XP na exclusão é implementada e
comentada na configuração de persistência, não em nenhum arquivo de Domain).
**Testes consultados (nomes de método confirmados por grep, não por leitura integral):**
`tests/BeeDay.Domain.Tests/HabitTests.cs`, `ProjectTests.cs`, `WalletTests.cs`, `WalletTagTests.cs`,
`TransactionTests.cs`, `UserProfileRulesTests.cs`, `UserSessionHardeningTests.cs`,
`UserIdentityTokenTests.cs`, `ExperienceDomainTests.cs`, `ValueObjectTests.cs`,
`ActivityAttributeTests.cs`; `tests/BeeDay.Application.Tests/ExperienceRewardPipelineTests.cs`.
**Entidades relacionadas:** todos os documentos em `docs/domain/`.
**Nota de honestidade metodológica:** algumas regras acima não têm um teste citado na última
coluna — isso significa que, na varredura por nome de método feita nesta Sprint, nenhum teste com
nome claramente correspondente foi encontrado, não que a regra seja não testada (pode estar coberta
por um teste com nome menos óbvio, não identificável sem ler o conteúdo completo de cada arquivo).
