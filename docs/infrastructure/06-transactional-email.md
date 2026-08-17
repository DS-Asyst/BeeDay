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

**Last verified:** 2026-08-16 (Epic 26, Sprint 26.10 — §17 added: final production-readiness audit,
re-verified against current `src/` (not re-asserted from memory), verdict **Production Ready, not
Production Activated**. Companion operational runbook published:
[`docs/deployment/14-transactional-email-runbook.md`](../deployment/14-transactional-email-runbook.md).
This closes Epic 26. Sprint 26.9 added §16: Gate D verdict — blocked on the
documented external prerequisite (no SERV3-WEB/Resend-secret access this session), not fabricated.
Repository-side readiness completed instead: the actual Sprint 26.1 HMG root cause is fixed
(`DevelopmentEmailSender`'s content-root guard now trusts a deliberately-configured absolute
`Directory`, relative-path traversal protection unchanged) and `HmgRecipientGuardOptions:AllowedRecipients`
is wired through `Deploy-BeeDay.ps1`/`deploy-hmg.yml` (Sprint 26.4 §10.5's deferred item) — neither
deployed to HMG by this sprint, both code-complete only. Sprint 26.8 added §15: cross-sprint
coverage-matrix audit
(Gate C — PASS), closing one real gap found by the audit (provider-failure handling for
resend-confirmation/forgot-password had no coverage; proven, not fixed — see §15.4) and explicitly
carrying forward the two residual items that remain out of scope for an audit sprint (the HMG
directory-guard bug, §6/§7; the mass-registration volume gap, §14.4). Sprint 26.7 added §14: the
observable state model
(requested/blocked/attempted/accepted/failed), 3-way failure classification in `ResendEmailSender`
with no automatic retries, recipient-address masking in `DevelopmentEmailSender`'s log lines
(`EmailAddressLogMasking`, new), and a per-email throttle now protecting `CreateAccountCommandHandler`/
`CreateUserCommandHandler` — reusing `IIdentityRequestThrottle`, with the mass-registration
volume/distinct-address gap explicitly documented as not addressed. Sprint 26.6 added §13:
transactional email template ownership/conventions, the brand-color correction (`#7A4FCB` →
`#5247F9`), the new plain-text alternative, and the documented decision not to localize email content
(architectural boundary, `docs/web/07-localization.md` §9). Sprint 26.5 added §12: full Identity
email flow and
`PublicBaseUrl` link-integrity audit, closing test gaps for the production origin guard and the
persistence-succeeds-delivery-fails boundary. Sprint 26.4 added §10: the
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

## 13. Transactional email templates (Epic 26, Sprint 26.6)

### 13.1 Ownership and extension convention

`IdentityEmailComposer` (`src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs`) is the single
owner of every transactional email template in the product — confirmed exhaustive by the flow
inventory in §3/§12.1 (only `ComposeEmailConfirmation`/`ComposePasswordReset` exist; no other
template-producing code was found anywhere in `src/`). Both methods share one HTML builder
(`BuildHtmlTemplate`) and one plain-text builder (`BuildPlainTextTemplate`) parameterized by title,
greeting name, introduction, action label/URL, and footer — a new transactional email flow extends
this composer with a third `Compose*` method reusing the same two builders, not a new template
system. This mirrors the "one class per Options-bound concern" pattern already used throughout
Infrastructure (e.g. `ResendEmailSender`/`DevelopmentEmailSender` each owning one delivery mechanism)
and matches the sprint's own constraint: centralize without introducing a parallel Design System or a
general-purpose template engine.

### 13.2 Brand color corrected

The HTML template used `#7A4FCB` (a pre-EPIC-25 purple) for its call-to-action button — inconsistent
with `#5247F9`, the single officially approved beeday Brand Color
(`docs/design-system/01-foundations.md` §2.2; `CLAUDE.md` §13; confirmed against
`src/BeeDay.Web/wwwroot/css/variables.css:4`, `--beeday-color-brand-primary: #5247f9`). Corrected in
this sprint (`IdentityEmailComposer.BrandColor`). The template's dark background/surface (`#17131f`)
does not correspond to any current design-system surface token (the product's actual default surface
is light, `--beeday-color-surface: #ffffff`) — left unchanged deliberately: no design-system contract
names an "official" email surface color to converge on, and a full visual redesign of an
email-client-rendered template (no visual-regression tooling exists for that) is a materially
different, riskier scope than the sprint's "reuse applicable tokens conceptually" mandate. Flagged
here as a candidate for a future, deliberately-scoped design sprint if the owner wants full parity.

### 13.3 Plain-text alternative

`EmailMessage` (`src/BeeDay.Application/Common/Identity/IEmailSender.cs`) gained an optional 4th
positional member, `PlainTextBody` (defaults to `null` — every existing 3-argument call site remains
valid; not a breaking change to this public Application contract). `IdentityEmailComposer` now
populates it for both flows. `ResendEmailSender` forwards it as Resend's documented `text` field
(`null` when absent, matching Resend's own optional-field contract).
`DevelopmentEmailSender` captures it as a third `{base-name}.txt` file alongside the existing
`.html`/`.json` pair when present, and records the file name (or `null`) in the metadata JSON's new
`PlainTextFile` field — additive only; `EmailCaptureWebApplicationFactory`
(`tests/BeeDay.Web.Tests/Integration/`) filters strictly by `*.html`, so its token-recovery and
email-count helpers are unaffected.

### 13.4 HMG subject distinction — already centralized, not duplicated here

The sprint's own requirement ("HMG subject distinction through the official safety/config/template
path rather than duplicated conditionals") is already satisfied structurally, by
`HmgRecipientGuardedEmailSender` (§10, Sprint 26.4): it prepends `HmgRecipientGuardOptions.SubjectPrefix`
(default `"[HMG] "`) to every allowed message's subject at the delivery boundary, once, regardless of
which flow produced the email. `IdentityEmailComposer` has no HMG/environment awareness and must not
gain any — adding a conditional here would be exactly the duplicated-conditional pattern the sprint
explicitly prohibits. No change was needed or made in this sprint for this requirement.

### 13.5 Content localization — not implemented, architectural boundary documented

Every template string is English-only, hardcoded in `IdentityEmailComposer`. The product has a full
en-US/pt-BR localization system (`docs/web/07-localization.md`), so this sprint evaluated reusing it
per the master instructions' "do not create a second source of truth for localization; reuse/extend
if it exists."

**Not implemented, because the existing localization system is documented and tested as exclusively
Web-owned:** `docs/web/07-localization.md` §9 states, as an audited, zero-exception invariant,
"`Domain` e `Application` permanecem inteiramente livres de `IStringLocalizer`/`CultureInfo`... Nenhum
dos dois tradutores existe em `BeeDay.Domain`/`BeeDay.Application`/`BeeDay.Infrastructure`" —
confirmed by that document's own grep-verified claim of 0 occurrences of `IStringLocalizer`/
`ResourceManager` outside `BeeDay.Web`. `IdentityEmailComposer` lives in Infrastructure. Adding
`IStringLocalizer`/`.resx` there would not be "reusing" the existing system — it would violate this
specific, deliberately-audited architectural boundary and create a second pattern for how localization
enters non-Web layers, which CLAUDE.md's core operating principles rank preserving architecture above
completing an individual task's nice-to-have. The alternative — moving email *composition* into Web,
with Infrastructure only sending an already-rendered message — is a legitimate design, but it is a
materially larger architectural change (moving ownership of `IIdentityEmailComposer`'s implementation
across a layer boundary, touching every call site in `UserHandlers.cs`/`IdentityHandlers.cs`) than
this sprint's "centralize templates" mandate justifies unilaterally.

**Recommendation, not actioned:** if transactional email localization is wanted, it should be its own
scoped decision — either (a) extend `docs/web/07-localization.md`'s boundary explicitly to allow a
narrow, Infrastructure-owned resource catalog for email strings only (a deliberate architecture
change, not a silent one), or (b) move composition to Web (calling `IStringLocalizer` there) and have
Infrastructure senders accept a fully-rendered `EmailMessage`. Either requires the repository owner's
explicit approval per `CLAUDE.md` §3.5 ("Do not create... new architectural patterns... unless the
user explicitly approves").

## 14. Observability, resilience & abuse controls (Epic 26, Sprint 26.7)

### 14.1 Observable state model

Terminology for reading logs/troubleshooting, mapped to where each state is actually produced:

| State | Where | Notes |
|---|---|---|
| Send requested | (not separately logged) | The MediatR handler calling `IEmailSender.SendAsync` is the request; no dedicated log line exists at that call site — the next state below is the first log evidence. |
| Safety blocked | `HmgRecipientGuardedEmailSender` (§10) | `LogWarning`, no recipient logged. Only reachable when Resend is selected and the guard is enabled. |
| Provider request attempted | `ResendEmailSender.SendAsync`, before the HTTP call | `LogInformation`, includes `Subject` (safe — distinguishes confirmation vs. reset, never the recipient or body). |
| Provider accepted | `ResendEmailSender.SendAsync`, on a 2xx response | `LogInformation`, includes Resend's own response `id` as `ProviderMessageId` when parseable (`"(unavailable)"` otherwise) — a safe, non-secret correlation identifier. **This is provider acceptance, not mailbox delivery** — no code in this repository observes or claims delivery; do not read this log line as proof the recipient received anything. |
| Provider request failed | `ResendEmailSender.SendAsync`, three distinct causes | See §14.2 — each classified and logged distinctly. |
| Development capture (not a "provider" state) | `DevelopmentEmailSender.SendAsync` | `LogInformation`, recipient masked (§14.3) — this path never talks to Resend at all; it is not part of the acceptance/failure state model above. |

`send requested` has no dedicated log line because the calling MediatR handlers (`CreateAccountCommandHandler`, `CreateUserCommandHandler`, `ResendEmailConfirmationCommandHandler`, `RequestPasswordResetCommandHandler`) have no `ILogger` today and this sprint did not add one — the state is implied by "attempted" (or "blocked") appearing at all.

### 14.2 Failure classification (no automatic retries)

`ResendEmailSender` distinguishes three failure causes, each logged at `Error` before rethrowing — never swallowed, never retried automatically (the master instructions explicitly prohibit adding retries that could duplicate a side effect unless proven safe; Resend's `Idempotency-Key` header, sent on every request, would make a retry *technically* safe, but no proven operational requirement justifies adding one in this sprint, so none was added):

1. **Transient network/connection error** — `httpClient.SendAsync` itself throws `HttpRequestException` (DNS failure, connection refused, TLS failure, ...); no HTTP response was ever received.
2. **Timeout** — `httpClient.SendAsync` throws `OperationCanceledException`/`TaskCanceledException` whose token is **not** the caller's own `cancellationToken` — HttpClient's own configured `Timeout` (30s, set at DI registration) fired. Distinguished from caller-requested cancellation, which propagates silently (unlogged, since it is expected/intentional, e.g. a shutdown or a genuinely cancelled request) rather than being reported as a failure.
3. **Provider rejection** — a real HTTP response with a non-2xx status code; logged with the status code.

Configuration errors (missing API key/`FromAddress`) are caught earlier, at startup, by `ResendOptions`'s `ValidateOnStart()` (`docs/deployment/02-runtime-configuration.md` §6) — they never reach `ResendEmailSender.SendAsync` at all, so they are not part of this runtime classification.

### 14.3 Logging: what is safe, what is masked, what never appears

- **Never logged anywhere in the email path** (confirmed by re-reading every log call site in `ResendEmailSender`, `DevelopmentEmailSender`, `HmgRecipientGuardedEmailSender`, `IdentityEmailComposer`): the Resend API key, password hashes, confirmation/reset tokens, or a full token-bearing callback URL.
- **`ResendEmailSender`/`HmgRecipientGuardedEmailSender`:** never log the recipient address, in any state, in any branch — the strictest policy in the codebase, appropriate for the path that can reach real external delivery.
- **`DevelopmentEmailSender`** (local-only, file-capture path — see §6): its two log lines named the full recipient address before this sprint. Now masked via `EmailAddressLogMasking.Mask` (new, `Identity/EmailAddressLogMasking.cs`) — at most the first 1–2 local-part characters plus the domain (`ti***@beeday.example`), never the full address. The captured `.json`/`.html` files on disk still contain the full address (required for a developer to identify which captured email is which) — only the *log line* is masked, consistent with the sprint's "never log unnecessary plaintext recipient PII" requirement, which is about logs specifically. This is the one and only masking rule introduced; it is not a general PII framework — no equivalent existed before, and none was needed anywhere else, since the other two senders already never log the recipient at all.

### 14.4 Abuse controls: registration now throttled, mass-registration volume is not

**Audited, reused, extended by exactly one call site pattern already established:** `IIdentityRequestThrottle`/`MemoryIdentityRequestThrottle` already protected `ResendEmailConfirmationCommandHandler` and `RequestPasswordResetCommandHandler` (per-submitted-email, 60s cooldown; §12.3). `CreateAccountCommandHandler` and `CreateUserCommandHandler` had no throttle check at all before this sprint — closed by adding the identical pattern (`throttle.TryAcquire("account-creation", email, 60s, ...)`, throws the same "please wait N seconds" shape `DomainErrorLocalizer` already translates generically) to both. No new abuse-control mechanism was built — this is 100% reuse of existing infrastructure, per the master instructions' explicit preference.

**What this does and does not protect against:** the per-email throttle stops a rapid double-submit of the *same* address from creating two accounts or issuing two confirmation emails (a real, if narrow, gap — see §12.3's transactional-boundary note for why two concurrent requests for the same not-yet-existing email could otherwise both reach the email-send step). It does **not**, and structurally cannot, limit registration *volume* across many *distinct* email addresses — an attacker registering many different fake or victim addresses in rapid succession is throttled per-address, so each new address resets the cooldown. That is a different abuse vector (mass email-bombing via registration, or resource exhaustion), requiring a volume/IP-based control — the same category of protection `LoginRateLimiterOptions`/`LoginRateLimiterFactory` already provides for `/auth/login` specifically. Extending an equivalent control to registration was evaluated and **not implemented in this sprint**: it would be new abuse-control infrastructure (not reuse of an existing, provably-applicable mechanism), which the master instructions explicitly restrict ("Do not introduce... an alternate rate limiter... unless the existing repository audit demonstrates that it is necessary and consistent with current architecture" — necessity is plausible here, but the decision belongs to whoever owns Identity/security scope broadly, not unilaterally to an email-focused sprint). Recorded here as an explicit, open finding rather than silently left unaddressed.

## 15. Cross-sprint coverage matrix — Gate C (Epic 26, Sprint 26.8)

Consolidates the automated coverage built across Sprints 26.2–26.7 against the Epic's own coverage
matrix, closes the one substantial gap found, and states the Gate C verdict (roadmap: "repository
quality and safety contract are proven enough for controlled HMG activation"). This sprint is not
the first testing sprint — the matrix below is mostly *verification* that earlier sprints already
did their job, not new test authorship, per the sprint's own instructions.

### 15.1 Environment/provider

| Item | Covered by | Status |
|---|---|---|
| Development → File/local provider | `EmailProviderDependencyInjectionTests.AddBeeDayInfrastructure_DevelopmentEnvironmentSettings_...` | ✅ |
| Homologation → Resend provider | No HMG-labeled test exists, because HMG's actual committed configuration resolves to the Development provider today (proven root cause, §6) — a literal "Homologation" test would be indistinguishable from the generic Resend-selected tests below, since `EmailProviderSelector`/DI wiring is entirely environment-name-agnostic (confirmed by reading the code: no branch anywhere keys off environment name). Covered by the generic case. | ✅ (via generic case) |
| Production → Resend provider | `EmailProviderDependencyInjectionTests.AddBeeDayInfrastructure_ProductionEnvironmentSettings_ResolveHmgRecipientGuardedEmailSenderWrappingResendExactlyOnce`; `HmgRecipientGuardDependencyInjectionTests` (generic Resend+guard host boot) | ✅ |
| Invalid/unknown provider configuration | `EmailProviderDependencyInjectionTests.AddBeeDayInfrastructure_WhenBothProvidersEnabled_ThrowsAtRegistrationTime` / `_WhenNoProviderEnabled_...` | ✅ |
| Missing required provider config | `EmailSecretsConfigurationTests.Host_WhenResendSelectedWithoutApiKey_...` / `_WithoutFromAddress_...` | ✅ |
| Provider DI composition | `EmailProviderDependencyInjectionTests`, `HmgRecipientGuardDependencyInjectionTests`, `BeeDayDbContextTests.AddBeeDayInfrastructure_ResolvesDbContextFactoryWithoutThrowing` (proves the full `AddBeeDayInfrastructure` graph, not just email) | ✅ |

### 15.2 HMG safety

| Item | Covered by | Status |
|---|---|---|
| Allowed recipient | `HmgRecipientGuardedEmailSenderTests.SendAsync_WhenRecipientIsAllowed_...` | ✅ |
| Blocked recipient | `SendAsync_WhenRecipientIsBlocked_DoesNotInvokeInnerSender` | ✅ |
| Safety config absent/invalid → fail closed | `HmgRecipientGuardDependencyInjectionTests.Host_WhenResendSelectedAndGuardLeftAtDefault_FailsToStartPredictably` | ✅ |
| External provider not invoked when blocked | Same test as "blocked recipient" — asserts the inner (real) sender is never called | ✅ |
| Production not accidentally subject to the allowlist | `CommittedProductionAppsettings_ExplicitlyDisablesHmgRecipientGuard` (parses the real committed JSON) + the Production DI test above (`Enabled=false`) | ✅ |
| HMG sender/subject distinction | `SendAsync_WhenRecipientIsAllowed_InvokesInnerSenderWithPrefixedSubject`, `SendAsync_DoesNotDoublePrefixAnAlreadyPrefixedSubject` | ✅ |

### 15.3 Identity

| Item | Covered by | Status |
|---|---|---|
| Account creation confirmation request | `AccountRegistrationTests.CreateAccount_CreatesUserWithProfileAtomically` / `CreateUser_CreatesUserAndSendsConfirmation` | ✅ |
| Confirmation callback | `EmailConfirmationIntegrationTests.ConfirmEmail_WithValidToken_...` (+ invalid/expired/reused) | ✅ |
| Confirmation resend | `EmailConfirmationIntegrationTests.ResendEmailConfirmation_*`, `IdentityHandlersTests.ResendConfirmation_*` | ✅ |
| Forgot password | `PasswordResetIntegrationTests.RequestPasswordReset_*`, `IdentityHandlersTests.RequestPasswordReset_*` | ✅ |
| Password reset request/callback | `PasswordResetIntegrationTests.ResetPassword_*` | ✅ |
| Invalid/expired/used token behavior | Both integration suites (§12.1) + `IdentityHandlersTests` (`ConfirmEmail_RejectsExpiredToken`, `ResetPassword_RejectsReusedToken`) | ✅ |
| User-enumeration-safe responses | `RequestPasswordReset_DoesNotRevealMissingEmail`/`_ForNonexistentEmail_CompletesSilently...`, `ResendConfirmation_WhenThrottled_BehavesIdenticallyForAnUnknownEmail` (§12.3) | ✅ |
| Provider failure handling | Registration: `AccountRegistrationTests.*_WhenEmailSendFails_...` (§12.3). **Gap found and closed this sprint:** resend-confirmation/forgot-password had zero coverage of this case — see §15.4. | ✅ (closed this sprint) |
| Callback/base URL integrity | §12.2, `ProductionOriginGuardTests` | ✅ |

### 15.4 New this sprint: provider failure has no transaction to protect it, for two flows

Registration's "persistence succeeds, delivery fails" boundary (§3.1/§12.3) is a *transactional*
boundary — one `IUnitOfWork` transaction commits, then the email send happens outside it.
`ResendEmailConfirmationCommandHandler`/`RequestPasswordResetCommandHandler` are architecturally
different and were never audited for this specific question before: they take
`IUserRepository`/`IUserTokenRepository` directly, not `IUnitOfWork`, and
`EfUserTokenRepository.RevokeActiveAsync`/`AddAsync` (`src/BeeDay.Infrastructure/Persistence/SqlServer/Repositories/EfUserTokenRepository.cs`)
each acquire their own short-lived `DbContext` and call `SaveChanges` immediately — there is no
transaction spanning the two calls, let alone one that also covers the email send after them. By the
time `emailSender.SendAsync` runs, the previous token is already revoked and the new one already
persisted, independently of the send's outcome.

**Proven, not fixed:** `IdentityHandlersTests.ResendConfirmation_WhenEmailSendFails_TokenMutationsArePersistedDespiteTheFailure`
and `RequestPasswordReset_WhenEmailSendFails_NewTokenIsPersistedDespiteTheFailure` (new this sprint)
assert exactly this: the token mutations survive a thrown `SendAsync`, and the exception still
propagates (not swallowed). This is now a second, explicitly documented instance of the same
accepted transactional boundary as §3.1/§12.3 — not introduced by this sprint, only proven and
recorded. No production code changed: per the master instructions and the roadmap ("test/audit
consolidation + minimal production change only if a proven coverage gap requires a testability seam
consistent with architecture"), the appropriate response to a documented, accepted boundary is a
test proving its actual behavior, not a new Outbox/distributed-transaction mechanism.

**Practical consequence for resend-confirmation specifically:** a user whose resend attempt hits a
transient provider failure has their previous (possibly still valid) token revoked and a new one
minted that they never receive — they must wait out the 60s throttle and try again. Forgot-password
has a milder version of the same shape (a new token is persisted but never delivered); its own next
request simply revokes and replaces it, same as the success path already does.

### 15.5 Templates, Observability/security

Templates (subject/link contracts, escaping, plain text, localization) — §13, all covered, including
the documented non-localization decision (§13.5). Observability/security (state semantics, no
secret/token logging, PII minimization, failure classification, cancellation) — §14, all covered,
including the one documented practical limit (HttpClient's own `Timeout` firing is not separately
simulated — impractical to do deterministically in a fast unit test; its code path is the same
try/catch structure already exercised by the network-failure test).

### 15.6 Gate C verdict

**PASS**, with the residual scope explicitly carried forward rather than silently closed:

- Repository quality and safety contract (provider selection, HMG guard, template safety, failure
  classification, abuse controls) are proven by the matrix above — deterministic, provider-faked,
  zero real Resend calls anywhere in the suite.
- Two residual items, both already tracked in earlier sections, not resolved by this sprint because
  they are out of an audit sprint's scope: the HMG directory-guard bug itself (§6/§7 — the actual
  root cause of the empty `C:\Apps\BeeDay-Data\Emails`, still unfixed as of this sprint) and the
  mass-registration volume/IP-based abuse gap (§14.4).
- Sprint 26.9 (HMG Deployment & End-to-End Validation) is the designated gate for real-environment
  evidence — nothing in this sprint substitutes for that; this matrix proves the code paths are
  correct under test doubles, not that HMG itself currently sends real email (§6.3 already states
  that classification explicitly).

## 16. HMG deployment & end-to-end validation — Gate D (Epic 26, Sprint 26.9)

### 16.1 Repository state vs. environment state (`CLAUDE.md` §8.2)

This section distinguishes what this sprint actually did (repository changes, code-complete,
locally validated) from what it did not and could not do (anything requiring SERV3-WEB access or
the real Resend secret). No claim below conflates the two.

### 16.2 Blocker: no infrastructure/secret access available to this sprint

Per the master instructions' explicit secret-handling boundary, the real HMG Resend API key was
never requested, read, or referenced. No SSH/RDP/PowerShell-remoting access to SERV3-WEB was
available in this session, so no step of the "controlled HMG sequence" in the Sprint 26.9 prompt
(deploy → recycle → health check → controlled allowlisted account creation → provider acceptance →
inbox receipt → HMG callback confirmation → resend/forgot-password/reset validation) was executed
against the real environment. **None of that sequence is claimed as done here.** This is the
external prerequisite the sprint's own instructions anticipate recording rather than fabricating.

### 16.3 What was completed instead: repository-side deployment readiness

Two concrete gaps that specifically blocked a future real HMG activation were closed:

1. **The actual HMG root cause, fixed** (`DevelopmentEmailSender.cs`) — the Sprint 26.1-proven bug
   (§6): the content-root guard now trusts a deliberately-configured *absolute* `Directory` value
   as-is (HMG's own `C:\Apps\BeeDay-Data\Emails`, outside content root
   `C:\Apps\BeeDay.Web`), while a *relative* `Directory` still cannot escape the content root via
   `..` segments — the guard's original, still-intact purpose. Proven by two new tests
   (`DevelopmentEmailSenderTests.SendAsync_WithAbsoluteDirectoryOutsideContentRoot_Succeeds` /
   `_WithRelativeDirectoryEscapingContentRoot_StillThrows`) reproducing the exact configuration
   shape committed in `appsettings.Homologation.json`. **This is code-complete, not environment
   validated** (§8.2) — it was never deployed to or exercised against the real SERV3-WEB. Until it
   is promoted through `deploy-hmg.yml`, HMG's currently-running binary still has the old, broken
   guard; this fix only takes effect on HMG's *next* real deployment.
2. **`HmgRecipientGuardOptions:AllowedRecipients` wired through the deploy chain** — the item
   Sprint 26.4 §10.5 explicitly deferred ("tracked as a prerequisite for actually enabling Resend on
   HMG in a later sprint"). `Deploy-BeeDay.ps1` gained an optional `-HmgAllowedRecipients` parameter
   (semicolon-separated, mirroring `AllowedHosts`'s own convention — .NET's array-binding needs one
   indexed App Pool variable per recipient, unlike `AllowedHosts` itself, which ASP.NET Core reads
   as a single delimited string), redacted from `$deployLogsPath` the same way connection
   strings/the Resend API key already are (recipient addresses are PII). `deploy-hmg.yml` reads an
   optional `BEEDAY_HMG_ALLOWED_RECIPIENTS` secret and passes it through — **not yet created as a
   GitHub secret by this sprint** (no access to configure repository secrets, and the value is real
   recipient PII that must never enter source control). This section originally claimed that, absent
   the secret, `Deploy-BeeDay.ps1` "skips these App Pool variables entirely — zero effect on today's
   deployments," exactly like the equivalent Resend variables.

   **Correction (Hotfix 26.9.1):** that claim was wrong and was disproven by a real incident. The
   first real run of `deploy-hmg.yml` after this sprint's PR merged (GitHub Actions run
   `31986772973`, both attempts) failed in "Deploy to IIS with rollback" with `The property 'Count'
   cannot be found on this object` — a PowerShell pipeline that filters out every element returns
   `$null`, not an empty array, and `$null.Count` throws under `Set-StrictMode -Version Latest`. The
   documented rollback (see [`docs/deployment/01-deployment.md`](../deployment/01-deployment.md) §6)
   also never ran during that incident, from a second, independent defect: the `Write-Error` that
   logged the failure inherited the script-wide
   `$ErrorActionPreference = "Stop"` and itself became terminating, aborting the `catch` block before
   "Starting rollback..." was reached. Both defects were fixed in **Hotfix 26.9.1**
   (`fix/epic-26-hmg-deploy-recovery`, a dedicated branch off `hmg`) — the recipient list is now
   built by `ConvertTo-BeeDayRecipientList`, which always returns a real collection even when empty,
   and both `Write-Error` calls in the rollback path use `-ErrorAction Continue`. With the hotfix
   applied, an absent secret does behave as originally intended (no `AllowedRecipients__N` variables
   emitted, no exception) — but that was only true starting with the hotfix, not since this sprint.
   No email was sent during the incident itself: the application process never restarted in either
   failed attempt, so no email-sending code path ever executed. See
   [`docs/deployment/01-deployment.md`](../deployment/01-deployment.md) §6 for the equivalent
   correction on the deployment-pipeline side.

### 16.4 What remains explicitly not done

- HMG has **not** been redeployed with either change above — both are code-complete, sitting in
  this stacked PR chain, not yet promoted.
- `appsettings.Homologation.json`'s `Resend:Enabled`/`Development:Enabled` were **not** changed —
  HMG still resolves to the Development/file provider today, deliberately. Flipping this is a real
  behavior activation decision (real outbound email starts flowing) that requires the owner's
  explicit approval and the real secret being available through the now-prepared injection channel
  — this sprint prepared the mechanism, it did not pull the trigger.
- `BEEDAY_HMG_ALLOWED_RECIPIENTS` does not exist as a GitHub secret; no real recipient address was
  requested, seen, or written anywhere in this sprint's diff.
- No account was created against HMG, no email was sent through Resend, no inbox was checked, no
  HMG callback was exercised. Zero evidence exists from this sprint for any state in the "controlled
  HMG sequence" described in §16.2, because none of that sequence ran.

### 16.5 Gate D verdict

**Not met — blocked on the documented external prerequisite (§16.2), not fabricated.** Per the
roadmap's own definition, this is an accepted outcome for Sprint 26.9: "Gate D: real HMG evidence
exists, **or** the PR explicitly records which external prerequisite prevented that evidence."
Repository-side readiness (§16.3) is complete and locally validated (mandatory + Release gates, see
below); real-environment validation requires the repository owner to run `deploy-hmg.yml` (or an
equivalent manual promotion) with SERV3-WEB access this session never had, and — only when the
owner is ready to activate real Resend delivery on HMG — to create the
`BEEDAY_HMG_ALLOWED_RECIPIENTS` secret and flip `appsettings.Homologation.json`'s provider flags in
a future, explicitly-scoped change.

## 17. Production readiness — final audit (Epic 26, Sprint 26.10)

Closes the Epic. Re-verified against the current repository state (not re-asserted from memory of
earlier sprints) on 2026-08-16.

### 17.1 Epic invariants

| Invariant | Status | Evidence |
|---|---|---|
| Development uses local/file delivery, no Resend credential required | ✅ | `appsettings.json` base: `Resend:Enabled=false`, `Development:Enabled=true` |
| Homologation uses the external provider only behind centralized fail-closed safety | ✅ (architecturally guaranteed, not yet exercised) | Whenever Resend is selected, `IEmailSender` is always `HmgRecipientGuardedEmailSender` — §10 |
| Production configured conceptually for Resend, not automatically activated | ✅ | `appsettings.Production.json`: `Resend:Enabled=true`; PRD has no runtime (Not Provisioned by Design) |
| HMG and PRD secret identities separate by contract | ✅ contract / ⚠️ not yet a live guarantee | Same secret name, different GitHub Environment scope — but `production` Environment doesn't exist yet; runbook §4.1 |
| No HMG API key committed, logged, documented, or embedded | ✅ | Confirmed across every sprint's diff review; never requested |
| Identity/Application code does not depend on Resend/IIS/Homologation | ✅ | `PersistenceContractBoundaryTests`; `Common/Identity/*` contracts only |
| Provider selection is deterministic | ✅ | `EmailProviderSelector.Resolve` — §4 |
| Critical configuration is validated | ✅ | `ValidateOnStart()` on all 6 Options classes |
| `PublicBaseUrl` behavior is environment-safe | ✅ | §12.2; `ProductionOriginGuardTests` |
| Automated tests do not call real Resend | ✅ | Every HTTP call stubbed; confirmed by re-reading every test touching `ResendEmailSender` |
| Provider acceptance is not mislabeled as delivery | ✅ | Explicit terminology throughout §14.1, this doc, and the runbook |
| PII/token logging is controlled | ✅ | §14.3 |
| Existing throttling/resilience infrastructure reused, not duplicated | ✅ | §14.4; no new rate limiter, no Outbox/retry subsystem introduced anywhere in the Epic |
| Rollback path is documented | ✅ | Runbook §12, linking the existing (not Epic-26-specific) `Deploy-BeeDay.ps1` mechanism |
| Documentation matches implementation | ✅ | This document + the runbook, both re-verified against current `src/` in this sprint |

### 17.2 Final code sweep

Searched for stale two-boolean provider logic, old sender names, and obsolete configuration
references left behind by the Epic's own changes: none found (`EmailProviderSelector.Resolve` fully
replaced the old bare `if (resendEnabled)` branch everywhere; no leftover reference to it exists).
One unrelated, pre-existing historical comment was found
(`IEmailConfirmationIssuer.cs`'s doc comment mentions `LevelUpData`, predating the BeeDay rename by
many sprints) — not touched, since it is not a reference this Epic introduced or replaced, and
`CLAUDE.md`'s change-discipline rule is to avoid unrelated cleanup, not to open old scars an Epic
never owned.

### 17.3 Production readiness verdict

**Production Ready, not Production Activated** — exactly the distinction the roadmap requires.
Every invariant above holds today, in the repository, under test. Nothing in this Epic deployed to
Production, enabled Production email delivery, created or requested a PRD API key, reused HMG's
credential anywhere, or sent a Production smoke email. The residual gaps are listed in the runbook
§19 and were carried forward explicitly across every sprint that found them — none were silently
dropped.

## 18. Related documentation

- [`04-services.md`](04-services.md) — existing Infrastructure services inventory, including the
  `ResendEmailSender`/`DevelopmentEmailSender` summary this document expands on with the HMG root
  cause.
- [`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) —
  existing per-environment configuration reference; §5.2 of that document is the source for HMG's
  confirmed Runtime State used in §6 above, and §6 there covers the Resend secret contract the
  guard's own allowlist channel (§10.5) extends.
- [`docs/architecture/07-security-architecture.md`](../architecture/07-security-architecture.md) —
  broader security-boundary context for the HMG recipient guard (§10).
