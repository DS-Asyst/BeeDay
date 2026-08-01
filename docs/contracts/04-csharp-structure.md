# Estrutura Proposta em C#

## 1. Solução

```text
src/
  LevelUp.Contracts/
    Common/
    Identity/
    Users/
    Dashboard/
    Activities/
    Projects/
    Wallets/

  LevelUp.Domain/
    Aggregates/
    Entities/
    ValueObjects/
    Events/
    Services/

  LevelUp.Application/
    Abstractions/
      Persistence/
      Security/
      Messaging/
      Time/
    Features/
      Habits/
        Create/
        Update/
        Delete/
        RegisterDirection/
      ...

  LevelUp.Infrastructure/
    Persistence/
      Json/
      SqlServer/
    Identity/
    Email/
    Background/

  LevelUp.Web/
    Components/
    Endpoints/
    Mapping/
    Authentication/
```

## 2. Exemplo de feature

```text
Features/Habits/Create/
  CreateHabitCommand.cs
  CreateHabitCommandHandler.cs
  CreateHabitCommandValidator.cs
  CreateHabitMapper.cs
```

## 3. Contrato

```csharp
namespace LevelUp.Contracts.Habits;

public sealed record CreateHabitRequest(
    string Title,
    string? Description,
    string Difficulty,
    string Direction,
    string? Attribute);

public sealed record HabitResponse(
    Guid Id,
    string Title,
    string? Description,
    string Difficulty,
    string Direction,
    string? Attribute,
    int Position,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);
```

## 4. Command interno

```csharp
internal sealed record CreateHabitCommand(
    Guid UserId,
    CreateHabitRequest Request) : IRequest<HabitResponse>;
```

## 5. Handler

```csharp
internal sealed class CreateHabitCommandHandler(
    IActivityRepository activities,
    IUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<CreateHabitCommand, HabitResponse>
{
    public async Task<HabitResponse> Handle(
        CreateHabitCommand command,
        CancellationToken cancellationToken)
    {
        var habit = Habit.Create(
            command.UserId,
            command.Request.Title,
            command.Request.Description,
            command.Request.Difficulty,
            command.Request.Direction,
            command.Request.Attribute,
            clock.UtcNow);

        await activities.AddHabitAsync(habit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return HabitContractMapper.Map(habit);
    }
}
```

## 6. EF adapter

```csharp
internal sealed class EfActivityRepository(LevelUpDbContext db)
    : IActivityRepository
{
    public Task<Habit?> GetHabitAsync(
        UserId userId,
        HabitId id,
        CancellationToken cancellationToken) =>
        db.Habits.SingleOrDefaultAsync(
            item => item.Id == id.Value && item.UserId == userId.Value,
            cancellationToken);

    public Task AddHabitAsync(Habit habit, CancellationToken cancellationToken)
    {
        db.Habits.Add(habit);
        return Task.CompletedTask;
    }
}
```

## 7. Regra contra repositório genérico

Não criar:

```csharp
IRepository<TEntity>
```

como contrato principal. Repositórios devem expressar consultas e operações relevantes ao agregado, sem expor `IQueryable`.
