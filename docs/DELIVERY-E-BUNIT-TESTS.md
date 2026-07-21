# Delivery E — Frontend component tests with bUnit

## Objective

Introduce automated component tests for the LevelUp Web Design System, protecting the UI foundation delivered in Deliveries A through D against regressions.

## Test project

`tests/LevelUp.Web.Tests`

The project uses:

- bUnit 2.7.2;
- xUnit v3;
- Microsoft.NET.Test.Sdk;
- direct reference to `LevelUp.Web`.

## Initial coverage

The suite covers 37 test cases across:

- `LevelUpButton` and all button variants;
- `LevelUpCard` and `LevelUpCardMenu`;
- `LevelUpInput`, `LevelUpTextArea`, `LevelUpCheckbox`, `LevelUpSelect` and `LevelUpDateInput`;
- `LevelUpEmptyState`, `LevelUpLoading`, `LevelUpSkeleton` and `LevelUpDashboardSkeleton`;
- `LevelUpConfirmDialog`;
- `LevelUpToastHost` and `ToastService` integration.

The tests validate rendered semantics, CSS contracts, accessibility attributes, callbacks, input binding, menu behavior, confirmation behavior, loading states and notification dismissal.

## Validation

Run from the repository root:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

The existing backend suite contains 19 tests. With this delivery, the expected total is 56 automated tests.

## E.1 — Test infrastructure corrections

The first local execution exposed assumptions that were not represented in the original test harness. Delivery E.1 corrects the suite without changing application behavior:

- form components are rendered with a cascading `EditContext`, matching their real use inside `EditForm`;
- a reusable `RenderInsideEditContext` helper removes repeated test setup;
- boolean ARIA values are rendered explicitly as `true` or `false`;
- AngleSharp is pinned to 1.5.2 to replace the vulnerable transitive 1.4.0 version;
- the unnecessary global using was removed.

Expected result: 56 tests passing and no NU1902 or IDE0005 warning from the Web test project.
