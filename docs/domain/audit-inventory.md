# Domain Complete Audit — Sprint 30.5

Inventário canônico dos 47 arquivos rastreados em `src/BeeDay.Domain`, auditados integralmente na
Sprint 30.5. `VERIFIED` significa que responsabilidade, dependências, construção e invariantes
foram confrontadas com o código e os testes atuais. `FIXED` identifica um defeito confirmado e
corrigido nesta Sprint. O inventário não antecipa auditorias de Application ou Infrastructure.

## Inventário por artefato

| Artefato | Categoria | Estado | Evidência/decisão |
|---|---|---|---|
| `BeeDay.Domain.csproj` | projeto | `VERIFIED` | Sem `ProjectReference` ou package de framework |
| `Abstractions/Entity.cs` | abstração | `VERIFIED` | Identidade gerada e guard de identidade vazio |
| `Common/EnumValidation.cs` | regra compartilhada | `VERIFIED` | Rejeita valores de enum não definidos |
| `Entities/Activity.cs` | entidade base | `FIXED` | Ownership agora é idempotente e não pode ser transferido diretamente |
| `Entities/Habit.cs` | aggregate root | `FIXED` | Factory exclusiva; enums e contadores protegidos |
| `Entities/Profile.cs` | projeção de Domain | `FIXED` | Comentário JSON obsoleto removido; sem estado de autenticação |
| `Entities/Project.cs` | aggregate root | `FIXED` | Factory exclusiva; duplicidade e ownership de To-Do protegidos |
| `Entities/RecurringTask.cs` | aggregate root | `FIXED` | Factory exclusiva e repetição validada |
| `Entities/Todo.cs` | entidade filha | `FIXED` | Factory exclusiva; mudança de Project somente pelo aggregate root |
| `Entities/Transaction.cs` | aggregate root | `FIXED` | Factory exclusiva; precisão e máximo monetário público protegidos |
| `Entities/User.cs` | aggregate root | `FIXED` | Factory exclusiva; incremento de sessão com overflow verificado |
| `Entities/UserToken.cs` | aggregate root | `FIXED` | Factory exclusiva; criação e janela temporal protegidas |
| `Entities/Wallet.cs` | aggregate root | `FIXED` | Factory exclusiva; cálculos filtram pelo WalletId |
| `Entities/WalletTag.cs` | aggregate root | `FIXED` | Factory exclusiva; nome e cor normalizados |
| `Enums/ActivityAttribute.cs` | enum | `VERIFIED` | Validado nos pontos de mutação |
| `Enums/ExperienceRewardType.cs` | enum | `VERIFIED` | Validado ao criar histórico de XP |
| `Enums/ExperienceSourceType.cs` | enum | `VERIFIED` | Validado pela factory de source |
| `Enums/HabitDifficulty.cs` | enum | `VERIFIED` | Validado por Habit |
| `Enums/HabitDirection.cs` | enum | `VERIFIED` | Validado por Habit |
| `Enums/HabitResetCounter.cs` | enum | `VERIFIED` | Validado por Habit |
| `Enums/ProjectStatus.cs` | enum | `VERIFIED` | Estado derivado, não armazenado |
| `Enums/TaskRepeat.cs` | enum | `VERIFIED` | Validado por RecurringTask |
| `Enums/TransactionType.cs` | enum | `VERIFIED` | Validado por Transaction |
| `Enums/UserLanguage.cs` | enum | `VERIFIED` | Validado por User |
| `Enums/UserTheme.cs` | enum | `VERIFIED` | Validado por User |
| `Enums/UserTokenType.cs` | enum | `VERIFIED` | Validado por UserToken |
| `Events/ApplicationActionDomainEvent.cs` | evento | `VERIFIED` | Carrier imutável produzido após validação do caso de uso |
| `Events/DomainEvent.cs` | evento base | `VERIFIED` | Timestamp imutável obrigatório pelo construtor |
| `Events/ExperienceGrantedDomainEvent.cs` | evento | `VERIFIED` | Carrier imutável de concessão já validada |
| `Events/IDomainEvent.cs` | contrato | `VERIFIED` | Contrato puro, sem dependência de mediator/framework |
| `Events/UserLeveledUpDomainEvent.cs` | evento | `VERIFIED` | Carrier imutável de transição já validada |
| `Exceptions/DomainException.cs` | exceção | `VERIFIED` | Base de falhas de regra de negócio |
| `Exceptions/DomainValidationException.cs` | exceção | `VERIFIED` | Preserva campo inválido e mensagem |
| `Exceptions/InvalidDomainStateException.cs` | exceção | `VERIFIED` | Distingue transição impossível de input inválido |
| `Experience/ExperienceCurve.cs` | serviço de domínio | `VERIFIED` | Fachada stateless da curva padrão |
| `Experience/ExperienceEntry.cs` | entidade | `FIXED` | Factory exclusiva e transição XP/nível integralmente validada |
| `Experience/ExperienceReward.cs` | value object | `VERIFIED` | Valor positivo via factory; `default` é estado não utilizável |
| `Experience/ExperienceSource.cs` | value object | `FIXED` | Record com igualdade por valor e factory exclusiva |
| `Experience/IExperienceCurve.cs` | contrato de domínio | `VERIFIED` | Abstração pura da curva |
| `Experience/LinearExperienceCurve.cs` | serviço de domínio | `VERIFIED` | Base positiva, overflow e níveis protegidos |
| `Experience/UserExperience.cs` | objeto do aggregate User | `FIXED` | Factory exclusiva; total e deduplicação protegidos em memória |
| `ValueObjects/ActivityDescription.cs` | value object | `VERIFIED` | Normalização e limite via factory |
| `ValueObjects/ActivityTitle.cs` | value object | `VERIFIED` | Obrigatoriedade, normalização e limite via factory |
| `ValueObjects/EmailAddress.cs` | value object | `FIXED` | Rejeita display-name e preserva endereço canônico |
| `ValueObjects/Nickname.cs` | value object | `VERIFIED` | Normalização e limites via factory |
| `ValueObjects/ProjectColor.cs` | value object | `VERIFIED` | Cor hexadecimal normalizada via factory |
| `ValueObjects/UserName.cs` | value object | `VERIFIED` | Obrigatoriedade, normalização e limite via factory |

## Decisões explícitas e limites

- Os `readonly record struct` continuam podendo existir como `default(T)` pela semântica do CLR.
  Esse valor não é um caminho público de criação válido: consumers devem usar `Create`, e as
  entidades que recebem esses values revalidam invariantes materiais.
- Domain Events são carriers imutáveis. A validação da operação pertence ao aggregate/caso de uso
  que os produz; duplicar todas as regras no evento criaria uma segunda fonte de verdade.
- `Activity.AssignOwner` continua permitindo a primeira atribuição após a factory porque o fluxo
  atual constrói o objeto antes de anexar o owner. Repetir o mesmo owner é idempotente; trocar por
  outro é estado inválido.
- `Project.AddTodo` é a única operação que pode realocar o `ProjectId` de um To-Do. A API pública
  `Todo.Update` não atravessa essa fronteira.
- O setter protegido sem corpo de `Project.Completed` é uma acomodação de materialização; o estado
  público permanece sempre derivado dos To-Dos.
- `UserExperience.Entries` é consistente em memória, mas a coleção não é hidratada pelo mapping
  relacional atual e `EnsureExperienceState` não possui consumer. Essa divergência cross-layer está
  registrada como `BD30-F030`, com owner na Sprint 30.7 e impacto a rever na 30.16.

## Evidência automatizada

`DomainInvariantAuditTests` cobre factories sem construtor público, ownership de Activity/Project,
movimentação de To-Do, janela temporal de UserToken, sintaxe canônica de e-mail, igualdade de
ExperienceSource, consistência de ExperienceEntry e limite monetário de Transaction. O guard
arquitetural rejeita referências a ASP.NET Core, EF Core, Application, Infrastructure, Web e
serialização JSON.

**Fontes de verdade:** todos os arquivos rastreados em `src/BeeDay.Domain`, todos os testes em
`tests/BeeDay.Domain.Tests`, mappings de `ExperienceEntry`/`UserExperience` e contratos consumidores
consultados durante a Sprint 30.5.
