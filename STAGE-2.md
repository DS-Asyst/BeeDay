# Stage 2 — Domain

## Applied changes

- Introduced `Entity` as the identity base abstraction.
- Encapsulated entity state with private/protected setters and domain methods.
- Added factories for `Habit`, `RecurringTask`, `Todo`, `Project`, and `Profile`.
- Added value objects for activity title, activity description, profile name, and nickname.
- Added domain-specific exceptions for validation and invalid persisted state.
- Added enum validation to prevent undefined values entering the domain.
- Preserved the existing JSON schema through `JsonInclude` and string-backed serialized properties.
- Centralized aggregate mutations in `LevelUpData` methods.
- Preserved the invariant between `Project.Status` and `Activity.Completed`.
- Expanded domain tests for value objects, enum validation, counters, and project status.
- Updated JSON recovery to treat invalid domain state as corrupted persisted data.

## Domain structure

```text
LevelUp.Domain/
├── Abstractions/
│   └── Entity.cs
├── Common/
│   └── EnumValidation.cs
├── Entities/
├── Enums/
├── Exceptions/
│   ├── DomainException.cs
│   ├── DomainValidationException.cs
│   └── InvalidDomainStateException.cs
└── ValueObjects/
    ├── ActivityDescription.cs
    ├── ActivityTitle.cs
    ├── ProfileName.cs
    └── ProfileNickname.cs
```

## Local validation

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet publish src/LevelUp.Web/LevelUp.Web.csproj -c Release
```
