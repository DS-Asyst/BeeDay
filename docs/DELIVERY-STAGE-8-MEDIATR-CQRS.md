# Stage 8 — MediatR and CQRS

## Delivered
- MediatR 14.2.0 and assembly registration.
- Commands and handlers for profile, habits, tasks, to-dos, projects, and ordering.
- Dashboard query and query handler.
- ValidationBehavior integrating FluentValidation automatically.
- LoggingBehavior for request lifecycle and failures.
- PerformanceBehavior warning for requests taking 500 ms or longer.
- Blazor facade migrated to ISender.
- Handler-oriented application tests.

## Architectural result
The Web project no longer coordinates feature services directly. It sends application messages through MediatR. Validation and cross-cutting concerns execute in pipeline behaviors before and after handlers.

## Validation
```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
