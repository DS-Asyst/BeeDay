# Modelo Relacional Proposto

## 1. Princípios

- banco iniciado vazio;
- chaves primárias `uniqueidentifier`;
- timestamps UTC;
- concorrência otimista com `rowversion`;
- índices por ownership e consultas reais;
- exclusões físicas inicialmente, salvo requisitos de auditoria;
- nomes de tabela e coluna definidos por migrations;
- nenhuma tabela `LevelUpData`.

## 2. Tabelas

### Users

- `Id`
- `Name`
- `NormalizedName`
- `Nickname`
- `NormalizedNickname`
- `Email`
- `NormalizedEmail`
- `PasswordHash`
- `IsActive`
- `IsEmailConfirmed`
- `HasCompletedProfile`
- `HasCompletedOnboarding`
- `AvatarKey`
- `Theme`
- `Language`
- `SessionVersion`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `LastLoginAtUtc`
- `RowVersion`

Índices únicos:

- `NormalizedEmail`
- `NormalizedNickname`, quando preenchido.

### UserTokens

- `Id`
- `UserId`
- `Type`
- `TokenHash`
- `ExpiresAtUtc`
- `UsedAtUtc`
- `RevokedAtUtc`
- `CreatedAtUtc`

Índices:

- `(TokenHash, Type)` único;
- `(UserId, Type, ExpiresAtUtc)`.

### Habits

- `Id`
- `UserId`
- `Title`
- `Description`
- `Difficulty`
- `Direction`
- `ResetCounter`
- `Attribute`
- `PositiveCount`
- `NegativeCount`
- `Position`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `RowVersion`

### RecurringTasks

- `Id`
- `UserId`
- `Title`
- `Description`
- `Repeat`
- `DueDate`
- `Attribute`
- `IsCompleted`
- `CompletedAtUtc`
- `Position`
- timestamps e `RowVersion`.

### Projects

- `Id`
- `UserId`
- `Title`
- `Description`
- `Color`
- `Status`
- `Attribute`
- `Position`
- timestamps e `RowVersion`.

### Todos

- `Id`
- `UserId`
- `ProjectId` anulável
- `Title`
- `Description`
- `DueDate`
- `Attribute`
- `IsCompleted`
- `CompletedAtUtc`
- `Position`
- timestamps e `RowVersion`.

### UserExperience

- `UserId` PK/FK
- `TotalExperience`
- `CurrentLevel`
- `UpdatedAtUtc`
- `RowVersion`

### ExperienceEntries

- `Id`
- `UserId`
- `SourceType`
- `SourceId`
- `RewardType`
- `Amount`
- `IdempotencyKey`
- `OccurredAtUtc`
- `MetadataJson` opcional.

Índice único:

- `(UserId, IdempotencyKey)`.

### Wallets

- `Id`
- `UserId` único
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `RowVersion`

### WalletTags

- `Id`
- `WalletId`
- `Name`
- `NormalizedName`
- timestamps e `RowVersion`.

Índice único:

- `(WalletId, NormalizedName)`.

### Transactions

- `Id`
- `WalletId`
- `TagId` anulável
- `Type`
- `Description`
- `Amount`
- `OccurredOn`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `RowVersion`

### OutboxMessages

- `Id`
- `Type`
- `Payload`
- `OccurredAtUtc`
- `ProcessedAtUtc`
- `AttemptCount`
- `LastError`

### AuditEntries

- `Id`
- `UserId` anulável
- `Action`
- `EntityType`
- `EntityId`
- `OccurredAtUtc`
- `CorrelationId`
- `MetadataJson`.

## 3. Relações

```text
User 1 ── * Habits
User 1 ── * RecurringTasks
User 1 ── * Projects
User 1 ── * Todos
Project 1 ── * Todos
User 1 ── 1 UserExperience
User 1 ── * ExperienceEntries
User 1 ── 1 Wallet
Wallet 1 ── * WalletTags
Wallet 1 ── * Transactions
WalletTag 1 ── * Transactions
User 1 ── * UserTokens
```
