# Use Cases

**Fonte da verdade:** verificado diretamente em cada `Handlers/*.cs` das 9 Features sob
`src/BeeDay.Application/Features/`, cruzado com as interfaces em `Common/Contracts/` e
`Features/*/Contracts/`.

Todo caso de uso real do BeeDay, agrupado por Feature. "Resultado" indica o tipo de retorno do
Command/Query (`void` = `IRequest` sem resposta).

## Authentication

| Caso de uso | Command/Query | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Autenticar usuário | `AuthenticateUserCommand` | `AuthenticateUserCommandHandler` | `IUserRepository`, `IPasswordService` | `User` | `IUserRepository` | `AuthenticatedUserResponse` |

## Dashboard

| Caso de uso | Command/Query | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Obter projeção da tela inicial | `GetDashboardQuery` | `GetDashboardQueryHandler` | `IDashboardReadService`, `ICurrentUserContext` | `User`, `Habit`, `RecurringTask`, `Project`/`Todo`, `Wallet` (todos em leitura) | `IDashboardReadService` (read service, não repositório) | `DashboardResponse` |

## Habits

| Caso de uso | Command | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Criar hábito | `CreateHabitCommand` | `CreateHabitCommandHandler` | `IHabitRepository`, `ICurrentUserContext` | `Habit` | `IHabitRepository` | `void` |
| Editar hábito | `UpdateHabitCommand` | `UpdateHabitCommandHandler` | `IHabitRepository` | `Habit` | `IHabitRepository` | `void` |
| Registrar reforço positivo (+ XP) | `RegisterHabitPositiveCommand` | `RegisterHabitPositiveCommandHandler` | `IHabitRepository`, `IExperienceRewardService`, `ICurrentUserContext` | `Habit` + `User` (XP) | `IHabitRepository` | `void` |
| Registrar reforço negativo (sem XP) | `RegisterHabitNegativeCommand` | `RegisterHabitNegativeCommandHandler` | `IHabitRepository` | `Habit` | `IHabitRepository` | `void` |
| Remover hábito | `DeleteHabitCommand` | `DeleteHabitCommandHandler` | `IHabitRepository` | `Habit` | `IHabitRepository` | `void` |

## Identity

| Caso de uso | Command | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Confirmar e-mail via token | `ConfirmEmailCommand` | `ConfirmEmailCommandHandler` | `IUserTokenRepository`, `IUserRepository` | `UserToken` + `User` | ambos | `void` |
| Reenviar confirmação de e-mail | `ResendEmailConfirmationCommand` | `ResendEmailConfirmationCommandHandler` | `IUserRepository`, `IUserTokenRepository`, `IEmailConfirmationIssuer`, `IIdentityRequestThrottle`, `IEmailSender` | `UserToken` | `IUserTokenRepository` | `void` |
| Solicitar reset de senha | `RequestPasswordResetCommand` | `RequestPasswordResetCommandHandler` | `IUserRepository`, `IUserTokenRepository`, `IIdentityRequestThrottle`, `IUserTokenService`, `IEmailSender`, `IIdentityEmailComposer` | `UserToken` | `IUserTokenRepository` | `void` |
| Resetar senha via token | `ResetPasswordCommand` | `ResetPasswordCommandHandler` | `IUserTokenRepository`, `IUserRepository`, `IPasswordService` | `User` + `UserToken` | ambos | `void` |

## Ordering

| Caso de uso | Command | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Reordenar Habits/Tasks/Todos/Projects | `ReorderActivitiesCommand` | `ReorderActivitiesCommandHandler` | `IHabitRepository`/`IRecurringTaskRepository`/`IProjectRepository` (conforme `Collection`) | `Habit`/`RecurringTask`/`Project`/`Todo` | o repositório correspondente à coleção | `void` |

Único caso de uso desta Feature — atua sobre 4 tipos de Aggregate diferentes dependendo do valor
do enum `ActivityCollection` (`Habits`/`Tasks`/`Todos`/`Projects`) no Request.

## Projects

| Caso de uso | Command | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Criar projeto | `CreateProjectCommand` | `CreateProjectCommandHandler` | `IProjectRepository`, `ICurrentUserContext` | `Project` | `IProjectRepository` | `void` |
| Editar projeto | `UpdateProjectCommand` | `UpdateProjectCommandHandler` | `IProjectRepository` | `Project` | `IProjectRepository` | `void` |
| Remover projeto | `DeleteProjectCommand` | `DeleteProjectCommandHandler` | `IProjectRepository` | `Project` (e seus `Todo`, por composição) | `IProjectRepository` | `void` |

## Tasks (RecurringTask)

| Caso de uso | Command | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Criar tarefa recorrente | `CreateTaskCommand` | `CreateTaskCommandHandler` | `IRecurringTaskRepository`, `ICurrentUserContext` | `RecurringTask` | `IRecurringTaskRepository` | `void` |
| Editar tarefa recorrente | `UpdateTaskCommand` | `UpdateTaskCommandHandler` | `IRecurringTaskRepository` | `RecurringTask` | `IRecurringTaskRepository` | `void` |
| Concluir/reabrir tarefa (+ XP na conclusão) | `ToggleTaskCommand` | `ToggleTaskCommandHandler` | `IRecurringTaskRepository`, `IExperienceRewardService` | `RecurringTask` + `User` (XP) | `IRecurringTaskRepository` | `void` |
| Remover tarefa recorrente | `DeleteTaskCommand` | `DeleteTaskCommandHandler` | `IRecurringTaskRepository` | `RecurringTask` | `IRecurringTaskRepository` | `void` |

## Todos

| Caso de uso | Command | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Criar Todo em um Project | `CreateTodoCommand` | `CreateTodoCommandHandler` | `IProjectRepository`, `ICurrentUserContext` | `Todo` (dentro de `Project`) | `IProjectRepository` (não existe `ITodoRepository`) | `void` |
| Editar Todo / mover entre Projects | `UpdateTodoCommand` | `UpdateTodoCommandHandler` | `IProjectRepository`, `IUnitOfWork` (movimentação cross-Project via `Projects.MoveTodoAsync`) | `Todo` | `IProjectRepository` | `void` |
| Concluir/reabrir Todo (+ XP do Todo e, se completar o Project, + XP do Project) | `ToggleTodoCommand` | `ToggleTodoCommandHandler` | `IProjectRepository`, `IExperienceRewardService` | `Todo` + `Project` (conclusão em cascata) + `User` (XP, até 2 concessões) | `IProjectRepository` | `void` |
| Remover Todo | `DeleteTodoCommand` | `DeleteTodoCommandHandler` | `IProjectRepository` | `Todo` | `IProjectRepository` | `void` |

## Users

| Caso de uso | Command/Query | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Criar usuário (sem perfil) | `CreateUserCommand` | `CreateUserCommandHandler` | `IUserRepository`, `IPasswordService`, `IEmailConfirmationIssuer`, `IUserTokenRepository`, `IEmailSender` | `User` + `UserToken` | `IUserRepository`, `IUserTokenRepository` | `Guid` |
| Criar conta completa (com perfil) | `CreateAccountCommand` | `CreateAccountCommandHandler` | mesmos de `CreateUserCommand` + `User.CompleteProfile` | `User` + `UserToken` | `IUserRepository`, `IUserTokenRepository` | `Guid` |
| Completar perfil (nickname/nome) | `CompleteUserProfileCommand` | `CompleteUserProfileCommandHandler` | `IUserRepository` | `User` | `IUserRepository` | `void` |
| Atualizar avatar | `UpdateCurrentUserAvatarCommand` | `UpdateCurrentUserAvatarCommandHandler` | `IUserRepository`, `ICurrentUserContext` | `User` | `IUserRepository` | `void` |
| Atualizar preferências (idioma/tema) | `UpdateCurrentUserPreferencesCommand` | `UpdateCurrentUserPreferencesCommandHandler` | `IUserRepository`, `ICurrentUserContext` | `User` | `IUserRepository` | `void` |
| Atualizar nome/e-mail | `UpdateCurrentUserAccountCommand` | `UpdateCurrentUserAccountCommandHandler` | `IUserRepository`, `ICurrentUserContext` | `User` | `IUserRepository` | `void` |
| Trocar senha | `ChangeCurrentUserPasswordCommand` | `ChangeCurrentUserPasswordCommandHandler` | `IUserRepository`, `IPasswordService`, `ICurrentUserContext` | `User` | `IUserRepository` | `void` |
| Concluir onboarding | `CompleteCurrentUserOnboardingCommand` | `CompleteCurrentUserOnboardingCommandHandler` | `IUserRepository`, `ICurrentUserContext` | `User` | `IUserRepository` | `void` |
| Obter usuário atual | `GetCurrentUserQuery` | (Handler correspondente em `UserHandlers.cs`) | `IUserRepository`, `ICurrentUserContext` | `User` | `IUserRepository` | `CurrentUserResponse?` |

## Wallets

| Caso de uso | Command/Query | Handler | Contracts | Aggregate | Repository | Resultado |
|---|---|---|---|---|---|---|
| Garantir que a Wallet do usuário existe | `EnsureCurrentWalletCommand` | `EnsureCurrentWalletCommandHandler` | `IWalletRepository`, `ICurrentUserContext` | `Wallet` (lazy-create) | `IWalletRepository` | `Guid` |
| Criar transação (com lazy-create de Wallet) | `CreateTransactionCommand` | `CreateTransactionCommandHandler` | `IWalletRepository`, `ITransactionRepository`, `IWalletTagRepository` (validar dono da tag) | `Wallet` + `Transaction` | `IWalletRepository`, `ITransactionRepository` | `Guid` |
| Editar transação | `UpdateTransactionCommand` | `UpdateTransactionCommandHandler` | `ITransactionRepository`, `IWalletRepository` (`Touch()`) | `Transaction` | `ITransactionRepository` | `void` |
| Remover transação | `DeleteTransactionCommand` | `DeleteTransactionCommandHandler` | `ITransactionRepository`, `IWalletRepository` (`Touch()`) | `Transaction` | `ITransactionRepository` | `void` |
| Criar tag de Wallet | `CreateWalletTagCommand` | `CreateWalletTagCommandHandler` | `IWalletTagRepository` | `WalletTag` | `IWalletTagRepository` | `Guid` |
| Editar tag de Wallet | `UpdateWalletTagCommand` | `UpdateWalletTagCommandHandler` | `IWalletTagRepository` | `WalletTag` | `IWalletTagRepository` | `void` |
| Remover tag de Wallet (desvincula transações) | `DeleteWalletTagCommand` | `DeleteWalletTagCommandHandler` | `IWalletTagRepository`, `ITransactionRepository` (`ClearTagReferencesAsync`), `IWalletRepository` (`Touch()`) | `WalletTag` + `Transaction` | `IWalletTagRepository`, `ITransactionRepository` | `void` |
| Obter resumo da Wallet | `GetWalletSummaryQuery` | `WalletQueryHandlers.cs` | `IWalletReadService` | `Wallet` (leitura) | `IWalletReadService` | `WalletSummaryResponse?` |
| Listar tags da Wallet | `GetWalletTagsQuery` | `WalletQueryHandlers.cs` | `IWalletReadService` | `WalletTag` (leitura) | `IWalletReadService` | `IReadOnlyList<WalletTagResponse>` |
| Obter transação por id | `GetTransactionByIdQuery` | `WalletQueryHandlers.cs` | `IWalletReadService` | `Transaction` (leitura) | `IWalletReadService` | `TransactionResponse?` |
| Listar transações filtradas/ordenadas/paginadas | `GetTransactionsQuery` | `WalletQueryHandlers.cs` | `IWalletReadService` | `Transaction` (leitura) | `IWalletReadService` | `PagedTransactionsResponse` |

## Observação sobre concessão de XP

5 casos de uso concedem XP (`RegisterHabitPositiveCommand`, `ToggleTaskCommand` na conclusão,
`ToggleTodoCommand` até duas vezes) — nenhum outro Command concede ou revoga experiência. Ver
`docs/domain/domain-events.md` para o detalhamento completo de quando `ExperienceGrantedDomainEvent`/
`UserLeveledUpDomainEvent` são publicados a partir desses casos de uso.

## Fontes de verdade

**Arquivos consultados:** todos os `Handlers/*.cs` das 9 Features, todos os `Commands/*.cs`,
`Queries/*.cs`, `Contracts/*.cs` correspondentes.
**Handlers consultados:** os ~35 Handlers inventariados na Fase 1, todos citados na tabela acima
pelo nome exato da classe.
**Testes consultados:** `tests/BeeDay.Application.Tests/FeatureServicesTests.cs`,
`WalletHandlersTests.cs`, `IdentityHandlersTests.cs`, `AuthenticationHandlersTests.cs`,
`UserAccountHandlersTests.cs`, `ExperienceRewardPipelineTests.cs`, `AccountRegistrationTests.cs`.
**Features relacionadas:** todas as 9.
**Documentação relacionada:** [`01-cqrs.md`](01-cqrs.md), [`04-contracts.md`](04-contracts.md),
`docs/domain/README.md` (Aggregate Roots referenciados), `docs/domain/domain-events.md` (fluxo de
XP).
