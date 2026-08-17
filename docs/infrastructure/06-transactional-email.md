# Transactional Email

**Source of truth:** verified directly in `src/BeeDay.Application/Common/Identity/*.cs`,
`src/BeeDay.Application/Features/Users/Handlers/UserHandlers.cs`,
`src/BeeDay.Application/Features/Identity/Handlers/IdentityHandlers.cs`,
`src/BeeDay.Infrastructure/Identity/*.cs`, `src/BeeDay.Infrastructure/Configuration/*.cs`,
`src/BeeDay.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`,
`src/BeeDay.Web/appsettings*.json`, `src/BeeDay.Web/web.config`,
`src/BeeDay.Web/Components/Features/Identity/Pages/*.razor`,
`src/BeeDay.Web/Localization/DomainErrorLocalizer.cs`, `tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs`,
and `git log`/`git show` on the files above. Cross-checked against
[`04-services.md`](04-services.md) (already current as of Sprint 18.6) and
[`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md)
(already current as of Sprint 18.4).

**Last verified:** 2026-08-16 (Epic 26, Sprint 26.5 — §12 added: full Identity email flow and
`PublicBaseUrl` link-integrity audit, closing test gaps for the production origin guard and the
persistence-succeeds-delivery-fails boundary; no production code changed. Sprint 26.4 added §10: the
centralized, fail-closed HMG recipient guard, `HmgRecipientGuardedEmailSender`. Sprint 26.3 formally
documented the secrets/configuration contract for `ResendOptions:ApiKey`/`FromAddress` in
[`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) §6;
Sprint 26.2 implemented §4.1/§8's provider-selection recommendation via `EmailProviderSelector`.
Originally written in Sprint 26.1, audit-only, no behavior changed by that sprint).

## 1. Scope

This document maps the transactional-email architecture as it exists today and records the proven
root cause of the empty `C:\Apps\BeeDay-Data\Emails` directory on HMG after account registration.
It is the discovery baseline for Epic 26 (Sprints 26.2–26.10). It does not change behavior; the
"Recommended target" section (§8) is explicitly forward-looking and must not be read as already
implemented.

## 2. Architecture map

```text
BeeDay.Domain          — EmailAddress value object only; no email-sending concept.
    ↑
BeeDay.Application     — IEmailSender, IIdentityEmailComposer, IEmailConfirmationIssuer,
                          IIdentityRequestThrottle contracts (Common/Identity/). Handlers in
                          Features/Users and Features/Identity depend only on these interfaces.
    ↑
BeeDay.Infrastructure  — ResendEmailSender, DevelopmentEmailSender (both IEmailSender),
                          IdentityEmailComposer (IIdentityEmailComposer), MemoryIdentityRequestThrottle,
                          5 Options classes bound in InfrastructureServiceCollectionExtensions.
    ↑
BeeDay.Web             — composition root only (AddBeeDayInfrastructure(configuration) call in
                          Program.cs); appsettings*.json carry the per-environment values; Identity
                          Razor pages (ConfirmEmail, EmailConfirmationSent, ResendConfirmation,
                          ForgotPassword, ResetPassword) call the Application layer via MediatR.
```

No dependency-direction violation was found: `IEmailSender`/`IIdentityEmailComposer` are defined in
Application and implemented in Infrastructure; Web never references a concrete Infrastructure email
type. Confirmed by the existing architecture test
`PersistenceContractBoundaryTests.ApplicationAssembly_DoesNotReferenceInfrastructure` (Application
does not reference Infrastructure at all, email types included).

There is no queue/Outbox in the email path: every flow calls `IEmailSender.SendAsync` synchronously,
in the request's own async flow, awaited directly by the MediatR handler. `BackgroundTaskQueue`
exists (`src/BeeDay.Infrastructure/Background/`) but its only consumer is
`AuditDomainEventHandler` for the Event Journal — email sending never goes through it.

## 3. Email flows discovered

| Flow | Handler | Sends email? | Notes |
|---|---|---|---|
| Account creation (legacy path) | `CreateUserCommandHandler` | Yes — email confirmation | `src/BeeDay.Application/Features/Users/Handlers/UserHandlers.cs:14-56` |
| Account creation (current onboarding path) | `CreateAccountCommandHandler` | Yes — email confirmation | Same file, lines 58-109; also sets nickname/avatar |
| Confirm email | `ConfirmEmailCommandHandler` | No | Consumes the token only; `IdentityHandlers.cs:18-46` |
| Resend confirmation | `ResendEmailConfirmationCommandHandler` | Yes — email confirmation | Throttled (`"email-confirmation"`, 60s), revokes prior active tokens first; `IdentityHandlers.cs:48-87` |
| Forgot password | `RequestPasswordResetCommandHandler` | Yes — password reset | Throttled (`"password-reset"`, 60s), silently no-ops if user unknown/inactive/unconfirmed (enumeration-safe); `IdentityHandlers.cs:89-128` |
| Reset password | `ResetPasswordCommandHandler` | No | Consumes the token, invalidates sessions; `IdentityHandlers.cs:130-165` |

No other transactional-email flow exists in the repository today (confirmed by the file inventory
in §2 of the glob run against `src/**/*Email*` and by the full contents of `IdentityHandlers.cs`
and `UserHandlers.cs`).

### 3.1 Registration commits before the email send — a real inconsistency

Both `CreateUserCommandHandler` and `CreateAccountCommandHandler` commit the database transaction
(`unitOfWork.CommitTransactionAsync`) **before** calling `emailSender.SendAsync`, outside the
`try`/`finally` block that owns the transaction. If `SendAsync` throws, the exception propagates
unhandled out of the MediatR handler — but the user row and its `UserToken` are already persisted.
The registration UI has no explicit `catch` for this path, so the exception surfaces through
whatever the calling Razor page's error handling does; `DomainErrorLocalizer.Translate` maps any
exception type it doesn't recognize (which includes `InvalidOperationException` — see §6) to the
generic `DomainErrorGeneric` message ("Something went wrong. Try again in a moment.",
`src/BeeDay.Web/Localization/DomainErrorLocalizer.cs:23-29`).

**Proven consequence:** a user who hits this path sees a generic failure message, but their account
was actually created and no confirmation email was ever sent or captured. A retry then fails with
"Email already registered" (`DomainErrorEmailAlreadyRegistered`), and the user has no way to obtain
a confirmation link from the UI other than `/account/resend-confirmation` — which depends on the
same broken send path.

## 4. Provider selection path

Selection happens once, at DI-registration time, in
`InfrastructureServiceCollectionExtensions.AddBeeDayInfrastructure`
(lines 74-86):

```csharp
var resendEnabled = configuration.GetValue<bool>($"{ResendOptions.SectionName}:Enabled");
if (resendEnabled)
{
    services.AddHttpClient<IEmailSender, ResendEmailSender>(client => { ... });
}
else
{
    services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
}
```

This is a single decision point, not scattered per-request logic — no runtime/per-request branching
exists. `ResendOptions.Enabled` is the only value that decides which `IEmailSender` implementation
is registered; `DevelopmentEmailOptions.Enabled` is a second, independent flag consumed only inside
`DevelopmentEmailSender.SendAsync` itself (it does not affect DI).

### 4.1 The two-boolean model can reach a silently-degraded state (resolved in Sprint 26.2)

**Sprint 26.2 update:** the ambiguous states described below are now rejected at DI-registration
time by `EmailProviderSelector.Resolve` (`src/BeeDay.Infrastructure/Configuration/EmailProviderSelector.cs`),
called from `InfrastructureServiceCollectionExtensions` in place of the old bare `if (resendEnabled)`.
The two configuration keys themselves are unchanged (§5). The description below is preserved as the
proven finding that motivated the change.

`Resend:Enabled` and `Email:Development:Enabled` are validated independently
(`InfrastructureServiceCollectionExtensions.cs:34-46`) and never validated against each other. Two
combinations are possible that a single explicit "effective provider" contract would make
impossible:

- `Resend:Enabled=true` **and** `Development:Enabled=true`: `DevelopmentEmailSender` is never
  registered, so `Development:Enabled` is dead configuration — no validation error, no log, no
  indication that half the configured intent is ignored.
- `Resend:Enabled=false` **and** `Development:Enabled=false`: `DevelopmentEmailSender` is
  registered but every call to `SendAsync` is a silent no-op (`DevelopmentEmailSender.cs:21-25`,
  logs at `Information` only) — every transactional email in that environment is suppressed with
  no error and no operator-visible failure.

Neither state fails startup (`ValidateOnStart()` only checks each Options class in isolation), and
neither state is what actually caused the HMG symptom (§6) — but both are proven-possible ambiguous
states of the *current* model, relevant to the Sprint 26.2 provider-selection redesign.

## 5. Configuration path and precedence

Standard ASP.NET Core precedence, unmodified (already documented in
[`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) §2; no
divergence found):

```text
appsettings.json (base)
  → appsettings.{ASPNETCORE_ENVIRONMENT}.json
    → environment variables (BeeDay__Email__Resend__ApiKey, etc. — "__" separator)
      → User Secrets (Development only)
```

Relevant keys: `BeeDay:Email:Resend:{Enabled,ApiKey,FromName,FromAddress}`,
`BeeDay:Email:Development:{Enabled,Directory}`, `BeeDay:IdentityEmail:{PublicBaseUrl,
ConfirmationPath,PasswordResetPath}`. All bound with `.ValidateOnStart()` — an invalid value fails
the process at boot, not at first request (confirmed in
`InfrastructureServiceCollectionExtensions.cs:26-59`).

`BeeDay:IdentityEmail:PublicBaseUrl` is read twice: once via `IdentityEmailOptions` for
`IdentityEmailComposer`'s URL building (`Infrastructure/Identity/IdentityEmailComposer.cs:42-52`),
and once directly off `IConfiguration` in `Program.cs` for a startup guard that requires it to be an
absolute HTTPS URL outside Development (documented in `docs/deployment/02-runtime-configuration.md`
§4.1). Both reads use the same configuration key and therefore always agree once bound — no
divergence risk found.

### 5.1 Per-environment effective provider (as committed today)

| Environment | `Resend:Enabled` | `Development:Enabled` | `Development:Directory` | Effective sender |
|---|---|---|---|---|
| Development (`appsettings.json` base) | not set (defaults `false`) | not set (defaults `true`) | not set (defaults `"Data/Emails"`, relative) | `DevelopmentEmailSender`, writes inside content root |
| Homologation (`appsettings.Homologation.json`) | `false` | `true` | `"C:\\Apps\\BeeDay-Data\\Emails"` (absolute, external) | `DevelopmentEmailSender`, **directory outside content root** |
| Production (`appsettings.Production.json`) | `true` | not set in this file | not set in this file | `ResendEmailSender` (never actually loaded today — see §5.2) |

### 5.2 Production file is inert today

Per `docs/deployment/README.md` ("Estado real de HMG e PRD") and
`docs/deployment/02-runtime-configuration.md` §5.1, PRD has no provisioned runtime as of Sprint 18.4
and no deployment path (`web.config`, `deploy-hmg.yml`, or `deploy-prd.yml` in its current,
never-executed-against-a-real-server form) ever selects `ASPNETCORE_ENVIRONMENT=Production`. This
audit did not re-verify PRD runtime state directly (no PRD environment exists to check) — it relies
on the existing, already-current `docs/deployment/` findings, which is why `appsettings.Production.json`
is out of scope for the root-cause analysis in §6.

## 6. Root cause of the empty `C:\Apps\BeeDay-Data\Emails` directory

### 6.1 The guard that fires

`DevelopmentEmailSender.SendAsync` (`src/BeeDay.Infrastructure/Identity/DevelopmentEmailSender.cs:17-60`)
resolves the write directory and enforces it stays inside the host's content root:

```csharp
var contentRoot = Path.GetFullPath(environment.ContentRootPath);
var directory = Path.GetFullPath(Path.Combine(contentRoot, _options.Directory));
var contentRootPrefix = contentRoot.EndsWith(Path.DirectorySeparatorChar)
    ? contentRoot
    : contentRoot + Path.DirectorySeparatorChar;

if (!directory.StartsWith(contentRootPrefix, StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("The development email directory must remain inside the application content root.");
}

Directory.CreateDirectory(directory);
```

`Path.Combine(contentRoot, _options.Directory)` returns `_options.Directory` unchanged whenever it
is itself rooted/absolute (documented .NET `Path.Combine` behavior). On HMG,
`_options.Directory` is `C:\Apps\BeeDay-Data\Emails` (§5.1) and `environment.ContentRootPath` is
`C:\Apps\BeeDay.Web` (confirmed HMG Runtime State per
`docs/deployment/02-runtime-configuration.md` §5.2). `C:\Apps\BeeDay-Data\Emails` does not start
with `C:\Apps\BeeDay.Web\`, so the guard's condition is true on every single call, and the method
throws **before** `Directory.CreateDirectory` or any `File.WriteAllTextAsync` executes.

### 6.2 Why the directory was pointed there in the first place

`git log` on `appsettings.Homologation.json` shows commit `9439bd8` ("fix: reconcile HMG
configuration paths and document PRD provisioning state (#40)") moved
`Email:Development:Directory` from a path resolved inside the deploy target (which
`docs/deployment/02-runtime-configuration.md` §5.2 records was wiped on every deploy — the row
"DevelopmentEmail" documents the pre-fix value `App_Data\Emails` never existing on disk) to the
same externally-provisioned `C:\Apps\BeeDay-Data\...` tree already used for Data Protection keys and
the Event Journal, specifically so captured emails would survive a redeploy. That change achieved
its own goal (the path is stable across deploys) but was not cross-checked against
`DevelopmentEmailSender`'s own content-root guard, which was written earlier for a different purpose
(path-traversal protection against a misconfigured *relative* `Directory` value escaping via `..`
segments) and was never designed to allow an intentionally external absolute path.

### 6.3 Classification

- **Observed repository fact:** `DevelopmentEmailSender` throws `InvalidOperationException` for any
  configured `Directory` that resolves outside `IHostEnvironment.ContentRootPath`, with no
  exception (`appsettings.Homologation.json:26` sets exactly such a directory). Verified by reading
  the source directly; not runtime-observed in this session.
- **Observed repository fact:** no automated test exercises `DevelopmentEmailSender` at all —
  `tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs` covers `ResendEmailSender` and
  `IdentityEmailComposer` only (verified: the class name `DevelopmentEmailSender` does not appear in
  any test file under `tests/`). This is a coverage gap, not a defect by itself, but it explains why
  CI never caught the HMG-specific failure mode.
- **Observed deployment fact (already recorded in `docs/deployment/`, not re-verified live in this
  session):** HMG's effective content root is `C:\Apps\BeeDay.Web` and its effective
  `Email:Development:Directory` is `C:\Apps\BeeDay-Data\Emails` (Sprint 18.4 Runtime State
  verification, `docs/deployment/02-runtime-configuration.md` §5.2).
- **Inference, not directly observed this sprint:** combining the two facts above means every
  registration/resend/forgot-password attempt on HMG throws `InvalidOperationException` from
  `DevelopmentEmailSender.SendAsync`, which is consistent with — and sufficient to fully explain —
  the reported symptom ("registration produced no email file"). No HMG stdout log
  (`C:\Apps\BeeDay-Data\Logs\stdout`, per `web.config:10-11`) was inspected in this session to
  directly confirm the exception was actually thrown and logged at runtime.
- **Unverified external prerequisite:** direct confirmation requires reading HMG's stdout log file
  or reproducing a registration attempt against HMG (or a byte-for-byte equivalent local
  configuration) and observing the same `InvalidOperationException`. That access was not available
  in this session. Sprint 26.9 (HMG Deployment & End-to-End Validation) is the designated sprint for
  real-environment confirmation; this audit does not fabricate that evidence.

No other candidate cause (DI misregistration, ACL/permissions, Identity logic, environment
selection, a feature flag) was found to be consistent with the evidence: `Resend:Enabled=false` and
`Development:Enabled=true` correctly select `DevelopmentEmailSender` at DI time (§4), and
`ASPNETCORE_ENVIRONMENT=Homologation` correctly loads `appsettings.Homologation.json` (confirmed
Runtime State, `docs/deployment/02-runtime-configuration.md` §5.2) — the failure is specific to the
content-root guard against this one configured value.

## 7. Gaps and risks

- **Confirmed architectural gap:** the two-boolean provider model can reach ambiguous/dead-config
  states with no startup validation catching them (§4.1).
- **Confirmed architectural gap:** `DevelopmentEmailSender`'s path-traversal guard and the
  deploy-stability requirement (external, redeploy-surviving directory) are currently incompatible
  — the guard was not designed with an intentional external path in mind.
- **Confirmed data-consistency gap:** registration commits the user/token before sending email, with
  no compensating action if the send fails (§3.1) — an operator cannot distinguish "never
  registered" from "registered, confirmation never delivered" without inspecting the database
  directly.
- **Confirmed test-coverage gap:** zero automated coverage of `DevelopmentEmailSender`, including
  its content-root guard — the exact condition that (per §6) explains the reported HMG symptom.
- **Resolved in Sprint 26.4:** HMG previously had no centralized recipient safety guard —
  `Resend:Enabled=false` on HMG made this moot in practice, but nothing in the architecture stopped
  a future `Resend:Enabled=true` on HMG from delivering to arbitrary real recipient addresses.
  `HmgRecipientGuardedEmailSender` now wraps `ResendEmailSender` unconditionally whenever Resend is
  selected as the provider, fails closed by default (see §10), and requires Production to opt out
  explicitly rather than by omission.
- **Not a gap, confirmed correct:** `ResendEmailSender` already fails closed on non-2xx responses
  (throws `HttpRequestException`, never swallows) and never logs the API key
  (`ResendSender_WhenApiRejectsRequest_ThrowsWithoutExposingApiKey`,
  `tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs:119-135`).
- **Not a gap, confirmed correct:** `IdentityEmailComposer` HTML-encodes both the display name and
  the callback URL before interpolating them into the template
  (`IdentityEmailComposer.cs:15-16, 30-31`), and `RequestPasswordResetCommandHandler` is already
  enumeration-safe (silent no-op for unknown/inactive/unconfirmed users, `IdentityHandlers.cs:106-110`).

## 8. Recommended target architecture for Sprint 26.2+

**This section was written in Sprint 26.1 as a recommendation for future sprints. Sprints 26.2 and
26.4 below are now implemented; the rest remain forward-looking recommendations, not yet
implemented.**

- ~~Sprint 26.2: replace the two independent booleans with a single explicit provider-selection
  contract~~ — **done.** `EmailProviderSelector.Resolve(resendEnabled, developmentEnabled)` returns
  one `EmailProvider` (`Development`/`Resend`) for the two valid combinations and throws
  `InvalidOperationException` for the two ambiguous ones, at DI-registration time (effectively
  startup, before `builder.Build()`). Both `ResendEmailSender` and `DevelopmentEmailSender` kept
  unchanged as the two concrete implementations — this was a selection-contract change only, not a
  rewrite of either sender. No `appsettings*.json` key was renamed; none of the 4 committed
  configurations needed a value change, since none was already in an ambiguous state (§5.1).
- Sprint 26.3: make `DevelopmentEmailSender`'s directory guard aware of an explicitly-configured
  external absolute path (distinct from an accidental relative-path traversal escape), without
  weakening the traversal protection for the relative-path case it was originally built for.
- ~~Sprint 26.4: add the centralized, fail-closed HMG recipient guard at the final
  external-delivery boundary~~ — **done.** See §10.
- Do not introduce a queue/Outbox/durable retry for email sends merely to fix §3.1 — the existing
  `BackgroundTaskQueue` is a fire-and-forget mechanism with no retry/durability semantics of its own
  (§2), so using it here would trade a visible failure for a silent one. Sprint 26.5 should evaluate
  whether email send failure needs to be reflected back to the user/registration flow explicitly
  (e.g. account created, confirmation email failed, offer immediate resend) rather than solved with
  new infrastructure.

## 9. Compatibility constraints for later sprints

- `IEmailSender`, `IIdentityEmailComposer`, `IEmailConfirmationIssuer`, and `EmailMessage` are public
  Application contracts already consumed by 4 handlers across 2 files — any signature change must
  update every caller in the same sprint.
- `BeeDay:IdentityEmail:PublicBaseUrl`, `BeeDay:Email:Resend:*`, and `BeeDay:Email:Development:*`
  are live configuration keys already deployed to HMG (`appsettings.Homologation.json`) and read by
  `Deploy-BeeDay.ps1` (`docs/deployment/02-runtime-configuration.md` §4.3) — renaming any of them is
  a deployment-breaking change requiring coordinated updates to the deploy script, not just the
  Options classes.
- `Resend:Enabled` currently defaults to `false` (`ResendOptions.cs:7`) and `Development:Enabled`
  currently defaults to `true` (`DevelopmentEmailOptions.cs:7`) — any new provider-selection contract
  (§8) must preserve today's actual Development-environment behavior (file capture, no external
  dependency) without requiring every environment's `appsettings.json` to be rewritten just to keep
  current behavior.

## 10. HMG recipient safety guard (Epic 26, Sprint 26.4)

**Source of truth for this section:** `src/BeeDay.Infrastructure/Identity/HmgRecipientGuardedEmailSender.cs`,
`Configuration/HmgRecipientGuardOptions.cs`, `DependencyInjection/InfrastructureServiceCollectionExtensions.cs`,
`src/BeeDay.Web/appsettings.Production.json`,
`tests/BeeDay.Infrastructure.Tests/HmgRecipientGuardedEmailSenderTests.cs`,
`HmgRecipientGuardDependencyInjectionTests.cs`.

### 10.1 Placement

`HmgRecipientGuardedEmailSender` sits at the single external-delivery boundary, wrapping whichever
concrete sender is resolved as `ResendEmailSender` — it is registered as `IEmailSender` **only**
when `EmailProviderSelector.Resolve(...)` returns `EmailProvider.Resend`
(`InfrastructureServiceCollectionExtensions.cs`). Every current and future caller of `IEmailSender`
(all 4 sending flows in §3) is protected automatically; no Identity handler, no Application code, and
no environment-name check were added anywhere outside Infrastructure's own DI composition. The
`Development` branch of the same `if`/`else` is completely untouched — `DevelopmentEmailSender` is
registered directly, never wrapped, and `HmgRecipientGuardOptions` is not even bound in that branch.

### 10.2 Fail-closed contract

`HmgRecipientGuardOptions.Enabled` defaults to `true`. Its `.ValidateOnStart()` (registered only
inside the `EmailProvider.Resend` branch, so it is never evaluated for the `Development` provider)
requires `AllowedRecipients` to be non-empty whenever `Enabled` is `true`. The practical consequence:

```text
Resend selected + HmgRecipientGuard section absent or its AllowedRecipients empty
→ the process refuses to start (OptionsValidationException)
```

There is no code path where Resend is selected and the guard is silently inert by omission — an
environment must **explicitly** set `Enabled: false` to send unprotected (Production's own choice,
§10.4), or explicitly provide `AllowedRecipients` to send protected. Proven end to end (not just
read from the `.Validate()` predicate) by
`HmgRecipientGuardDependencyInjectionTests.Host_WhenResendSelectedAndGuardLeftAtDefault_FailsToStartPredictably`.

### 10.3 Guard behavior

| Recipient state | `Enabled` | Result |
|---|---|---|
| In `AllowedRecipients` (case-insensitive, trimmed) | `true` | Delegates to `ResendEmailSender`, with `SubjectPrefix` (default `"[HMG] "`) prepended once (never doubled if already present) |
| Not in `AllowedRecipients` | `true` | Silently suppressed — `ResendEmailSender` is never called. Matches the existing "disabled = silent no-op" idiom already used by `DevelopmentEmailSender`/`ResendEmailSender` themselves, rather than throwing and breaking the calling flow (§3.1's existing send-failure propagation problem is not made worse by this guard) |
| Any | `false` | Delegates to `ResendEmailSender` unmodified — no allowlist check, no subject prefix. This is the explicit Production opt-out, not a default |

Logging never interpolates the raw recipient address in either the allowed or blocked branch
(`HmgRecipientGuardedEmailSender.cs`) — proven by
`HmgRecipientGuardedEmailSenderTests.SendAsync_NeverLogsTheRawRecipientAddress_ForAllowedOrBlockedRecipients`,
which asserts the test's own recipient strings never appear in any captured log message.

### 10.4 Production does not inherit HMG blocking

`appsettings.Production.json` now explicitly sets `BeeDay:Email:HmgRecipientGuard:Enabled: false` —
a deliberate, auditable line in source control, not an inferred default (Production's `Resend`
section was already `Enabled: true` before this sprint; this is the one new key). Guarded by
`HmgRecipientGuardDependencyInjectionTests.CommittedProductionAppsettings_ExplicitlyDisablesHmgRecipientGuard`,
which reads the real committed file. `appsettings.Homologation.json` is untouched by this sprint —
it still resolves to the `Development` provider (§5.1), so the guard's default (`Enabled: true`,
fail-closed) is not yet exercised there; it only becomes relevant the moment a future sprint flips
Homologation's `Resend:Enabled` to `true`, at which point the guard's own default protects it
automatically.

### 10.5 Allowlist storage — not committed to source control

`AllowedRecipients` is real-world PII (personal email addresses) and must never be hardcoded in
`appsettings*.json` or anywhere else in the repository — no value was added anywhere by this sprint.
The approved channel is the same one already proven for the Resend API key itself
(`docs/deployment/02-runtime-configuration.md` §6.2): a GitHub Environment secret →
`Deploy-BeeDay.ps1` → the privileged IIS CONFIGURE operation → an App Pool environment variable
(`BeeDay__Email__HmgRecipientGuard__AllowedRecipients__0`, `__1`, ... — the standard
`Microsoft.Extensions.Configuration` array-binding convention for that key). Wiring a new
`-HmgAllowedRecipients` parameter through `deploy-hmg.yml`/`Deploy-BeeDay.ps1` (mirroring
`-ResendApiKey`) is deployment-automation work, not addressed by this sprint's own scope
(configuration/policy code) — tracked as a prerequisite for actually enabling Resend on HMG in a
later sprint, alongside the Sprint 26.1 §6 directory-guard fix.

## 11. Evidence inspected this sprint

**Source files read in full:** `Common/Identity/IEmailSender.cs`, `IIdentityEmailComposer.cs`,
`IEmailConfirmationIssuer.cs`, `IIdentityRequestThrottle.cs`, `Features/Users/Handlers/UserHandlers.cs`,
`Features/Identity/Handlers/IdentityHandlers.cs` (Application);
`Identity/DevelopmentEmailSender.cs`, `ResendEmailSender.cs`, `IdentityEmailComposer.cs`,
`Configuration/DevelopmentEmailOptions.cs`, `IdentityEmailOptions.cs`, `ResendOptions.cs`,
`DependencyInjection/InfrastructureServiceCollectionExtensions.cs` (Infrastructure);
`appsettings.Homologation.json`, `web.config`, `Localization/DomainErrorLocalizer.cs`,
`Components/Features/Identity/Pages/ConfirmEmail.razor` (Web);
`tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs`,
`tests/BeeDay.Web.Tests/Integration/EmailCaptureWebApplicationFactory.cs`.

**Documentation cross-checked (already current, not modified by this sprint):**
[`04-services.md`](04-services.md), [`05-dependency-injection.md`](05-dependency-injection.md),
[`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md),
[`docs/deployment/README.md`](../deployment/README.md).

**Git history inspected:** `git log --oneline --all` on `DevelopmentEmailSender.cs`,
`DevelopmentEmailOptions.cs`, and `appsettings.Homologation.json`.

**Not performed this sprint (out of scope for an audit-only sprint, and no access available):**
reading HMG's live stdout log; reproducing a registration attempt against HMG or an equivalent local
configuration; any change to code, configuration, or tests.

**Epic 26, Sprint 26.5 additionally read in full:** `Components/Features/Identity/Pages/ForgotPassword.razor`,
`ResendConfirmation.razor`, `ResetPassword.razor` (already read: `ConfirmEmail.razor`, §11 above);
`Common/Identity/IEmailConfirmationIssuer.cs`/`EmailConfirmationIssuer.cs`; `Domain/Entities/UserToken.cs`;
`Web/Localization/DomainErrorLocalizer.cs` (full file); `Web/Services/BeeDayWebService.cs`;
`Program.cs` production startup guards (§4.1 below).
**Tests read:** `tests/BeeDay.Application.Tests/IdentityHandlersTests.cs`, `AccountRegistrationTests.cs`;
`tests/BeeDay.Web.Tests/Integration/EmailConfirmationIntegrationTests.cs`, `PasswordResetIntegrationTests.cs`,
`BeeDayWebApplicationFactory.cs`, `ProductionLikeWebApplicationFactory.cs`.

## 12. Identity email flows & link integrity (Epic 26, Sprint 26.5)

Audit of every Identity flow that sends or consumes a transactional email, plus the `PublicBaseUrl`
origin contract those emails' callback links depend on. No production code changed by this sprint —
findings below are either confirmed-safe (with the test that proves it) or a documented boundary.

### 12.1 Flow inventory (extends §3)

All 6 flows in §3 were re-inspected end to end. Two already had thorough integration coverage
predating this sprint (`EmailConfirmationIntegrationTests.cs`, `PasswordResetIntegrationTests.cs` —
token validity/expiry/reuse/revocation, throttle behavior, enumeration-safety, and real `/auth/login`
gating, all verified against the real MediatR handlers and a real SQL Server LocalDB instance). This
sprint closed the remaining gaps found by direct inspection, listed in §12.2–§12.5.

### 12.2 `PublicBaseUrl` as the callback origin — confirmed single-sourced, now guard-tested

Re-confirms §5's finding that `IdentityEmailComposer` builds every callback URL from exactly one
value — the bound `IdentityEmailOptions.PublicBaseUrl` — never from `HttpContext`, a forwarded
header, or any other request-derived input. Combined with each environment loading its own
`appsettings.{Environment}.json` independently (standard ASP.NET Core precedence, §5), there is no
runtime code path that could let one environment's link cross into another
(HMG→Production/localhost, Production→HMG/localhost, or the reverse) — confirmed by reading
`IdentityEmailComposer.cs` and `Program.cs` again in full, not merely re-asserted from §5.

The one real enforcement gap found: **zero test exercised the Program.cs guard** that requires
`PublicBaseUrl` to be an absolute HTTPS URL outside Development (`Program.cs`, ~line 37) — nor its
two sibling guards (`AllowedHosts`, `DataProtectionKeysDirectory`). Closed for `PublicBaseUrl`
specifically (the security-sensitive origin contract this sprint is scoped to) by
`ProductionOriginGuardTests.cs`: non-HTTPS, relative, and missing values each proven to fail startup
with the exact guard message. `AllowedHosts`/`DataProtectionKeysDirectory` remain untested — noted
as a gap for a future sprint, not fixed here (out of this sprint's stated flow/link-integrity scope).
Because this test (like `ProductionLikeWebApplicationFactory`) mutates process-wide environment
variables to defeat Program.cs's pre-`Build()` guards, and xUnit parallelizes across test classes by
default, both factories now serialize their mutate-boot-restore lifecycle through a shared
`ProductionEnvironmentVariableTestLock` (new this sprint) — without it, this test's deliberately
invalid `PublicBaseUrl` values could leak into an unrelated, concurrently-booting host and fail it
with the same `OptionsValidationException`, which is exactly what happened once during this sprint's
own validation before the lock was added.

### 12.3 Token and failure behavior

- **Expiration/reuse/revocation:** entirely `UserToken`'s own domain logic (`EnsureCanBeUsed`,
  `MarkAsUsed`, `Revoke`) — a token cannot be used twice, used after revocation, or used after
  `ExpiresAtUtc`; confirmed by domain-level state (`Domain/Entities/UserToken.cs`) and already
  covered by both integration suites (§12.1) and `IdentityHandlersTests.cs`.
- **User-enumeration resistance:** confirmed for both throttled and non-throttled paths.
  `RequestPasswordResetCommandHandler` returns silently (no exception, no email) for both an unknown
  email and a throttled one. `ResendEmailConfirmationCommandHandler` differs by design — it *does*
  throw a "please wait N seconds" `InvalidDomainStateException` when throttled — but the throttle
  fires identically whether or not the submitted email belongs to a real account (the throttle key is
  the normalized email string itself, never account existence), so the exception carries no
  existence signal. Proven by
  `IdentityHandlersTests.ResendConfirmation_WhenThrottled_BehavesIdenticallyForAnUnknownEmail`
  (new this sprint) alongside the pre-existing
  `EmailConfirmationIntegrationTests.ResendEmailConfirmation_ForNonexistentEmail_CompletesSilentlyWithoutSendingEmail`.
  `DomainErrorLocalizer` already translates this message into a proper localized string
  (`DomainErrorWaitBeforeResend`), not a raw leak of the Domain-layer English text.
- **Persistence succeeds, delivery fails — the known transactional boundary:** both
  `CreateAccountCommandHandler` and `CreateUserCommandHandler` commit the user/token transaction
  *before* calling `emailSender.SendAsync`, outside the owning `try`/`finally` (§3.1). This sprint
  proves — rather than only asserts — that the account is not lost when delivery fails:
  `AccountRegistrationTests.CreateAccount_WhenEmailSendFails_UserAndTokenArePersistedDespiteTheFailure`
  and its `CreateUser` counterpart (new this sprint) seed a throwing `IEmailSender`, assert the
  exception propagates, and assert the user/token rows exist anyway. **This is accepted as the
  correct boundary, not fixed with an Outbox/queue/distributed transaction** — per the master
  instructions' explicit prohibition and because the existing resend-confirmation flow (§12.1) is
  already the correct recovery path for exactly this case: an operator or the user themselves can
  request a fresh confirmation email for an account that exists but never received one.
- **Provider accepts, a later step fails:** does not apply to any current flow — `SendAsync` is
  always the last statement in every handler that calls it (`UserHandlers.cs`, `IdentityHandlers.cs`);
  there is no step after delivery that could still fail.
- **Token-bearing URLs never logged:** confirmed by re-reading every logging call site in the email
  path (`ResendEmailSender`, `DevelopmentEmailSender`, `HmgRecipientGuardedEmailSender`,
  `IdentityEmailComposer`) — none logs the message body or the raw token; `DevelopmentEmailSender`
  writes the body to its capture file (the intended local-dev preview mechanism, not a log) and logs
  only the file path.

### 12.4 A pre-existing DI/configuration-timing subtlety, not a defect

`BeeDayWebApplicationFactory` (the base fixture used by nearly every Web integration test) overrides
`BeeDay:Email:Development:Enabled=false` via `ConfigureAppConfiguration`. That override never
reaches `EmailProviderSelector.Resolve` (called eagerly, before `Build()`, per the same
before-vs-after-`Build()` distinction documented on `ProductionLikeWebApplicationFactory`), so
provider *selection* still sees the base `appsettings.json` default (`Development:Enabled=true`) and
correctly registers `DevelopmentEmailSender` — never ambiguous. The override *does* reach the
lazily-bound `IOptions<DevelopmentEmailOptions>` that `DevelopmentEmailSender.SendAsync` reads at
send time (resolved after `Build()`), so sends are silently suppressed instead of writing files
during ordinary (non-email-capture) integration tests. Confirmed by running the full Web.Tests suite
after Sprint 26.2/26.4 landed — no test broke — and by direct inspection of the timing distinction.
Documented here because a future engineer attempting the same override-via-`ConfigureAppConfiguration`
trick for `Resend:Enabled` (expecting it to switch providers) would be surprised to find it silently
does not.

### 12.5 `CreateUserCommand` has no UI caller

`CreateUserCommandHandler`/`CreateUserCommand` (`Features/Users/Handlers/UserHandlers.cs`,
`Commands.cs`) is fully wired (MediatR, DI, `BeeDayWebService.CreateUserAsync`) but nothing in
`src/BeeDay.Web/Components/` calls `BeeDayWebService.CreateUserAsync` — only
`CreateAccountAsync`/`CreateAccountCommand` (the nickname/avatar onboarding flow) is reachable from
the UI today. Not proven dead (a MediatR command can be invoked by other means, e.g. a future admin
tool), so not removed by this sprint — removing a live, exported handler is a scope decision for
whoever owns that call site, not an email-architecture change. Given `CreateUserCommand` sends a real
confirmation email through the exact same path as `CreateAccountCommand`, it now has the same test
coverage (§12.3) so it is not a silent gap if it is ever wired up.

## 13. Related documentation

- [`04-services.md`](04-services.md) — existing Infrastructure services inventory, including the
  `ResendEmailSender`/`DevelopmentEmailSender` summary this document expands on with the HMG root
  cause.
- [`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) —
  existing per-environment configuration reference; §5.2 of that document is the source for HMG's
  confirmed Runtime State used in §6 above, and §6 there covers the Resend secret contract the
  guard's own allowlist channel (§10.5) extends.
- [`docs/architecture/07-security-architecture.md`](../architecture/07-security-architecture.md) —
  broader security-boundary context for the HMG recipient guard (§10).
