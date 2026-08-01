# Catálogo de Contratos

## 1. Identity

### Requests

- `RegisterAccountRequest`
- `LoginRequest`
- `ConfirmEmailRequest`
- `ResendEmailConfirmationRequest`
- `RequestPasswordResetRequest`
- `ResetPasswordRequest`
- `ChangePasswordRequest`

### Responses

- `AccountResponse`
- `AuthenticationResponse`
- `OperationAcceptedResponse`
- `SessionResponse`

## 2. Users

### Requests

- `CompleteProfileRequest`
- `UpdateAccountRequest`
- `UpdateAvatarRequest`
- `UpdatePreferencesRequest`
- `CompleteOnboardingRequest`

### Responses

- `CurrentUserResponse`
- `UserProfileResponse`
- `UserPreferencesResponse`

## 3. Dashboard

- `DashboardResponse`
- `ProfileSummaryResponse`
- `ExperienceSummaryResponse`
- `DailyActivitiesResponse`

## 4. Habits

### Requests

- `CreateHabitRequest`
- `UpdateHabitRequest`
- `RegisterHabitDirectionRequest`
- `ReorderHabitsRequest`

### Responses

- `HabitResponse`
- `HabitProgressResponse`
- `ActivityMutationResponse`

## 5. Tasks

- `CreateTaskRequest`
- `UpdateTaskRequest`
- `ToggleTaskRequest`
- `TaskResponse`

## 6. Todos

- `CreateTodoRequest`
- `UpdateTodoRequest`
- `ToggleTodoRequest`
- `TodoResponse`

## 7. Projects

- `CreateProjectRequest`
- `UpdateProjectRequest`
- `ProjectResponse`
- `ProjectDetailsResponse`

## 8. Ordering

- `ReorderActivitiesRequest`
- `OrderedActivityItem`
- `ReorderActivitiesResponse`

## 9. Wallet

### Requests

- `CreateTransactionRequest`
- `UpdateTransactionRequest`
- `CreateWalletTagRequest`
- `UpdateWalletTagRequest`
- `TransactionFilterRequest`

### Responses

- `WalletSummaryResponse`
- `TransactionResponse`
- `TransactionPageResponse`
- `WalletTagResponse`

## 10. Shared

- `PagedRequest`
- `PagedResponse<T>`
- `SortDirectionContract`
- `ValidationErrorContract`
- `ProblemContract`
- `OperationResultContract<T>`

## 11. Campos mínimos recomendados

### Activity response

```csharp
public sealed record ActivityResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Attribute,
    int Position,
    bool IsCompleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);
```

`Version` permite concorrência otimista futura.

### Mutation response

```csharp
public sealed record ActivityMutationResponse(
    Guid Id,
    long Version,
    ExperienceDeltaResponse? Experience,
    DateTimeOffset UpdatedAt);
```
