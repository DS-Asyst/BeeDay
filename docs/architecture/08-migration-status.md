# Contract-First Migration Status (Sprint 13.7)

**Purpose:** the single, precise answer to "how much of the Contract-First migration is actually done
in code today" — verified against the compiled source, not against what any earlier Sprint report
intended or approved for later. Every row below was confirmed by reading the current implementation
during Sprint 13.7, not carried forward from memory of earlier Sprints.

**Do not read this document as a plan.** Plans live in `07-persistence-contracts.md` and the
Sprint 13.4 reports. This document only answers "is it built," as of the state described in §0.

## 0. State this document describes

Branch `Contract-First-Architecture-Migration`, working tree as left by Sprint 13.6 (`git status`
unchanged since then), 682/682 tests passing, `dotnet build -c Release --warnaserror` clean. EPIC 13 is
**not complete**. The Application is **not** fully migrated to the Sprint 13.3 persistence contracts.

## 1. Read flows

| Flow | Legacy path | Contract-First path | Status |
|---|---|---|---|
| Dashboard (`/daily`) | `GetLevelUpQuery` → `GetLevelUpResponse(LevelUpData)` → `ILevelUpRepository` | `GetDashboardQuery` → `DashboardResponse` → `IDashboardReadService` → `JsonDashboardReadService` | **Migrated.** `DashboardState`, `Home.razor`, `ProfileSidePanel.razor`, `ProjectContextFilter`, `ProjectWorkspace`, `TodoEditorModal`, `DashboardModalState` all consume `DashboardResponse` exclusively. |
| Wallet summary/tags/transaction/transactions-list | `ILevelUpRepository.LoadAsync()` + in-memory filtering inside the handler | `IWalletReadService` → `JsonWalletReadService` | **Migrated.** All 4 query handlers in `WalletQueryHandlers.cs` depend only on `IWalletReadService`. |
| Current-user snapshot for `Tutorial.razor`, `Account.razor`, `ProfileCreationState` | `GetLevelUpQuery` → `GetLevelUpResponse(LevelUpData)` → `ILevelUpRepository` | *(none yet)* | **Not migrated.** These three consumers still call `LevelUpWebService.LoadAsync()`, which still sends `GetLevelUpQuery`. Deliberately excluded from the Dashboard lot (Sprint 13.4) — see `07-persistence-contracts.md` §12 and the Sprint 13.4 Dashboard-migration analysis. |
| `GetCurrentUserQuery` (identity-only response) | `ILevelUpRepository` | *(none yet)* | **Not migrated.** Untouched since before Sprint 13.3. |

`GetLevelUpQuery`/`GetLevelUpResponse`/`GetLevelUpQueryHandler` therefore **still exist in production
code and still have real consumers** — they cannot be removed until the row above them is migrated too
(tracked, not scheduled, as of this Sprint).

## 2. Write flows (commands)

**Zero command handlers have been migrated.** Every one below still depends on `ILevelUpRepository`
and mutates `LevelUpData` through `UpdateAsync(Action<LevelUpData>)`, exactly as before Sprint 13.3:

| Feature | Handlers file | Aggregate(s) touched |
|---|---|---|
| Habits | `HabitCommandHandlers.cs` | `Habit` (+ `User` for XP on RegisterPositive) |
| Tasks | `TaskCommandHandlers.cs` | `RecurringTask` (+ `User` for XP on Toggle) |
| Todos | `TodoCommandHandlers.cs` | `Project`/`Todo` (+ `User` for XP on Toggle) |
| Projects | `ProjectCommandHandlers.cs` | `Project` |
| Ordering | `ReorderActivitiesCommandHandler.cs` | `Habit`/`RecurringTask`/`Project`/`Todo` |
| Wallet commands | `WalletCommandHandlers.cs` | `Wallet`, `Transaction`, `WalletTag` |
| Users | `UserHandlers.cs` | `User` (+ `UserToken` via `IEmailConfirmationIssuer`) |
| Authentication | `AuthenticationHandlers.cs` | `User` |
| Identity | `IdentityHandlers.cs` | `User`, `UserToken` |

Composition root (`RequestHandlerBase.cs`, `InfrastructureServiceCollectionExtensions.cs`) and two Web
files (`Program.cs`'s `OnValidatePrincipal`, `AuthenticatedUserInitializer.cs`) also still depend on
`ILevelUpRepository` directly.

## 3. Persistence contracts by Aggregate (Sprint 13.3) — defined, not adopted

All 8 exist in `src/LevelUp.Application/Common/Contracts/`, compile cleanly, and are exercised by
`PersistenceContractBoundaryTests` (Sprint 13.6). **None has a concrete adapter. None is registered in
DI. None has a single production consumer.** Verified by source search — zero matches for any of these
interface names outside their own declaration file, anywhere in `src/` or `tests/`, except cross-links
inside their own XML doc comments.

| Contract | Aggregate | Adapter | Registered in DI | Consumed by a handler |
|---|---|---|---|---|
| `IUserRepository` | `User` | None | No | No |
| `IUserTokenRepository` | `UserToken` | None | No | No |
| `IHabitRepository` | `Habit` | None | No | No |
| `IRecurringTaskRepository` | `RecurringTask` | None | No | No |
| `IProjectRepository` | `Project` (+ `Todo`) | None | No | No |
| `IWalletRepository` | `Wallet` | None | No | No |
| `ITransactionRepository` | `Transaction` | None | No | No |
| `IWalletTagRepository` | `WalletTag` | None | No | No |

### 3.1 Approved corrections not yet applied to code

`07-persistence-contracts.md` §10 records 4 approved signature additions (`SaveAsync` on
`ITransactionRepository`/`IWalletTagRepository`/`IProjectRepository`, `MoveTodoAsync` on
`IProjectRepository`, `RevokeActiveAsync` on `IUserTokenRepository`). **Verified: none of these methods
exist in the current `.cs` files.** The approval recorded a decision for whichever lot migrates that
handler — it did not, and was never claimed to, change the files at the time.

### 3.2 Atomicity ports — design corrected, never implemented

`07-persistence-contracts.md` §9 documents the corrected (non-callback, `BeginAsync`/`CommitAsync`)
design for `IHabitProgressionTransaction` and `IIdentityTokenTransaction`. **Verified: neither type
exists anywhere in `src/` or `tests/`.** This is a documented design pending implementation, not a
partially-built feature.

## 4. Read services (Sprint 13.3) — adoption status

| Contract | Adapter | Registered in DI | Consumed |
|---|---|---|---|
| `IWalletReadService` | `JsonWalletReadService` | Yes | Yes — 4 query handlers |
| `IDashboardReadService` | `JsonDashboardReadService` | Yes | Yes — `GetDashboardQueryHandler` |

Both fully adopted end-to-end (handler → contract → adapter → real JSON file), each with dedicated
Infrastructure tests against a real temp file (`JsonWalletReadServiceTests.cs`,
`JsonDashboardReadServiceTests.cs`) and Web-level coverage through the real DI container
(`Web.Tests`/`E2E.Tests`).

## 5. Internal Infrastructure component

`JsonLevelUpDocumentStore` — extracted in Sprint 13.4, confirmed still `internal` and still the single
shared read/write/backup pipeline behind `JsonLevelUpRepository` (legacy wrapper),
`JsonWalletReadService`, and `JsonDashboardReadService`. No duplicate JSON I/O pipeline exists anywhere
in `LevelUp.Infrastructure` (reconfirmed in Sprint 13.5's audit, unchanged since).

## 6. Test suite

Sprint 13.6 consolidated 9 duplicated `ILevelUpRepository` test fakes into `FakeLevelUpRepository`
(+ `FakeCurrentUserContext`, `FakeApplicationCache`), and extended `PersistenceContractBoundaryTests`
with 3 architectural guards (no `System.Text.Json` type in any contract signature, no generic
Repository/UnitOfWork abstraction, `LevelUp.Application` never references `LevelUp.Infrastructure`).
These fakes still back **9 Application.Tests files** exercising the unmigrated handlers listed in §2 —
they remain necessary exactly as long as those handlers do, and are the correct scope for consolidation
(see `07-persistence-contracts.md`'s cross-reference) rather than something to remove now.

## 7. What "done" will look like

This migration is complete only when every row in §2 moves to a contract-backed adapter, every row in
§3 gains a real adapter and at least one consumer, `GetLevelUpQuery`/`GetLevelUpResponse` has zero
consumers and is deleted, `ILevelUpRepository` has zero consumers and is deleted, and `FakeLevelUpRepository`
has zero consumers and is deletable. None of these conditions hold today. Sprint 13.8 should audit
against this document's §1–§6 directly rather than re-deriving the inventory from source again.
