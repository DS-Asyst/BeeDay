# Stage 3 — Application Layer

The Application project is organized by business feature instead of technical file type.

## Structure

- `Common`: persistence contracts and reusable application-service behavior.
- `Features/Dashboard`: read use case and response model.
- `Features/Profiles`: profile request, contract, and service.
- `Features/Habits`: habit commands and service.
- `Features/Tasks`: recurring task commands and service.
- `Features/Todos`: to-do commands and service.
- `Features/Projects`: project commands and service.

## Design decisions

- The former large `ILevelUpService` facade was removed.
- Each feature exposes a focused interface following interface segregation.
- Request records live beside the feature that consumes them.
- Read operations return an explicit response type.
- Shared repository mutation and entity lookup behavior is centralized in `ApplicationService`.
- Dependency injection registers every feature independently.
- The Blazor adapter composes the feature services while preserving the existing UI API.
