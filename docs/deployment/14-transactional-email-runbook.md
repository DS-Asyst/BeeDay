# Transactional Email — Production Readiness Runbook

**Source of truth:** this document is an operational index over facts already established and
verified elsewhere in this repository — it does not re-derive them. Every claim below links to the
document that owns it. Built at the close of Epic 26 (Sprints 26.1–26.10, 2026-08-16); re-verify
against the linked documents if either drifts from this one.

**Post-close update (2026-08-17):** the first two real HMG deployments after this Epic's own PR
stack merged both failed inside `Deploy-BeeDay.ps1` itself — not in application code, not in
Resend (still inactive at the time) — and were fixed by Hotfix 26.9.1 and Hotfix 26.9.2. Both are
now Environment Validated: HMG deployment run
[`32004712401`](https://github.com/tiagoarrigoni/BeeDay/actions/runs/32004712401) (run #188)
completed successfully end to end, including the new pre-deployment regression-suite gate.
`DevelopmentEmailSender` was itself then Environment Validated on that same HMG state (a real
account-creation flow produced `.txt`/`.json`/`.html` artifacts in `C:\Apps\BeeDay-Data\Emails`).
With that proven and the four Resend secrets now present in the GitHub `homologation` Environment,
a dedicated activation change (`appsettings.Homologation.json`: `Resend:Enabled` → `true`,
`Development:Enabled` → `false`) flips Homologation to the Resend provider — see §2 and §6, updated
accordingly. The first deployment against that activation (run `32009214798`, #190) failed on a
third, distinct root cause — a privileged-boundary contract mismatch between `Deploy-BeeDay.ps1`
and `Invoke-BeeDayIisControl.ps1`, unrelated to Resend itself — fixed by **Hotfix 26.9.3**; see §6.
Real Resend inbox E2E (§11) has not yet been executed and can only happen after Hotfix 26.9.3 is
deployed — see §19.

**Audience:** whoever performs a transactional-email-related deployment, troubleshoots a delivery
issue, or evaluates activating Resend on a new environment (HMG or PRD). For the *architecture*
narrative (how the current system was audited, designed, and tested sprint by sprint), see
[`docs/infrastructure/06-transactional-email.md`](../infrastructure/06-transactional-email.md) — the
owning document for everything about *why* the system looks the way it does. This runbook is the
owning document for *how to operate it*.

## 1. Architecture overview and ownership boundaries

```text
BeeDay.Application  — IEmailSender, IIdentityEmailComposer, IEmailConfirmationIssuer,
                       IIdentityRequestThrottle contracts only. No Resend/IIS/Homologation
                       knowledge anywhere in this layer (BeeDay.Domain has none of this at all).
BeeDay.Infrastructure — ResendEmailSender, DevelopmentEmailSender, HmgRecipientGuardedEmailSender,
                        IdentityEmailComposer, EmailProviderSelector, 6 Options classes.
BeeDay.Web           — composition root only (AddBeeDayInfrastructure call in Program.cs);
                        appsettings*.json carry per-environment values.
```

Full detail, dependency-direction proof, and the flow inventory:
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §2–§3.

## 2. Provider selection by environment

| Environment | `Resend:Enabled` | `Development:Enabled` | Effective provider today |
|---|---|---|---|
| Development | `false` (base default) | `true` (base default) | `DevelopmentEmailSender` — no Resend credential required |
| Homologation | `true` (committed) | `false` (committed) | `ResendEmailSender` wrapped by `HmgRecipientGuardedEmailSender` — **activated; not yet deployed to SERV3WEB, real inbox E2E not yet executed** (§6 below) |
| Production | `true` (committed) | `false` (committed) | `ResendEmailSender` wrapped by `HmgRecipientGuardedEmailSender` — **not deployed anywhere; PRD has no runtime** (§4) |

Selection is deterministic and validated at startup by `EmailProviderSelector.Resolve` — an
environment cannot silently land in an ambiguous or unprotected state; both ambiguous combinations
(both flags true, both false) fail the host at boot. Detail:
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §4.

## 3. Required non-secret settings

| Key | Owner Options class | Required when |
|---|---|---|
| `BeeDay:IdentityEmail:PublicBaseUrl` | `IdentityEmailOptions` | Always — absolute HTTPS URL outside Development (Program.cs guard + Options validation) |
| `BeeDay:IdentityEmail:ConfirmationPath` / `PasswordResetPath` | `IdentityEmailOptions` | Always — must start with `/` |
| `BeeDay:Email:Development:Directory` | `DevelopmentEmailOptions` | When the Development provider is selected — relative *or* absolute (Epic 26, Sprint 26.9 fix) |
| `BeeDay:Email:Resend:FromName` | `ResendOptions` | When Resend is selected (defaults to `"BeeDay"`) |
| `BeeDay:Email:HmgRecipientGuard:SubjectPrefix` | `HmgRecipientGuardOptions` | Optional, defaults to `"[HMG] "` — only consulted when Resend is selected and the guard is enabled |

Runtime configuration precedence (appsettings → environment variables → User Secrets) and the full
options/validation inventory: [`02-runtime-configuration.md`](02-runtime-configuration.md) §2, §4.

## 4. Secret injection contract

Real secrets never live in `appsettings*.json` — always injected as IIS App Pool environment
variables by `Deploy-BeeDay.ps1`, sourced from GitHub Environment secrets, `__` as the
`Microsoft.Extensions.Configuration` section separator:

| Secret (GitHub) | App Pool variable | Required for |
|---|---|---|
| `BEEDAY_RESEND_API_KEY` | `BeeDay__Email__Resend__ApiKey` | Resend provider |
| `BEEDAY_RESEND_FROM_ADDRESS` | `BeeDay__Email__Resend__FromAddress` | Resend provider |
| `BEEDAY_RESEND_FROM_NAME` | `BeeDay__Email__Resend__FromName` | Resend provider (optional, has a script default) |
| `BEEDAY_HMG_ALLOWED_RECIPIENTS` | `BeeDay__Email__HmgRecipientGuard__AllowedRecipients__0`, `__1`, ... | HMG recipient guard, when the Resend provider is selected — **wired in Sprint 26.9, secret not yet created in GitHub** |
| `BEEDAY_PUBLIC_BASE_URL` | `BeeDay__IdentityEmail__PublicBaseUrl` | Always outside Development |

All five follow the same graceful-absence pattern: unset in GitHub → the workflow's env var resolves
empty → `Deploy-BeeDay.ps1` skips writing that specific App Pool variable rather than overwriting it
with a blank, leaving whatever is already configured on the server untouched. Full secret/variable
table including validation-step coverage: [`01-deployment.md`](01-deployment.md) §5.

### 4.1 HMG and PRD secret identity separation

`deploy-hmg.yml` declares `environment: homologation`; `deploy-prd.yml` declares
`environment: production`. Both reference the *same* secret name, `BEEDAY_RESEND_API_KEY` — the
separation is enforced by GitHub Environment secret scoping, not by using different names.
**This separation is not yet a live guarantee for PRD**: `environment: production` is declared in
the workflow but does not exist as a configured GitHub Environment today (confirmed via `gh api
repos/.../environments` — only `copilot` and `homologation` exist;
[`01-deployment.md`](01-deployment.md) §4.2/§6). Creating that Environment (and populating its own,
PRD-distinct `BEEDAY_RESEND_API_KEY`) is part of PRD's future real provisioning — consistent with
PRD being Not Provisioned by Design ([`README.md`](README.md) "Estado real de HMG e PRD"). **Never
populate the `production` GitHub Environment's secret with HMG's value.**

## 5. HMG safety policy

Whenever the Resend provider is selected, `IEmailSender` is *always* `HmgRecipientGuardedEmailSender`
wrapping `ResendEmailSender` — never the raw sender. `HmgRecipientGuardOptions.Enabled` defaults to
`true` with an empty allowlist, which **fails the host at startup** rather than sending unprotected —
an environment that switches to Resend without configuring the guard refuses to boot. Only
`appsettings.Production.json` explicitly opts out (`Enabled: false`, a committed, deliberate value,
not a default). Full contract, decorator behavior, and fail-closed proof:
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §10.

## 6. Current HMG state — Resend activated in configuration, deployment/E2E pending

**Committed `appsettings.Homologation.json` now specifies the Resend provider** (`Resend:Enabled:
true`, `Development:Enabled: false`) — activated by a dedicated, narrowly-scoped change after the
history below. This is a configuration change only: no architecture, Identity flow, template, or
recipient-safety code changed. Until this change is deployed through `deploy-hmg.yml`, HMG's
*running* binary still resolves to `DevelopmentEmailSender` — see the Repository State vs. Runtime
State distinction (`CLAUDE.md` §8.2).

**Deployment and validation history since this Epic's PR stack merged (2026-08-16/17):**

- The first real HMG deployment after Sprint 26.9's PR merged (run `31986772973`) failed in
  "Deploy to IIS with rollback" with a PowerShell null/`.Count` crash while building the
  `HmgRecipientGuardOptions.AllowedRecipients` App Pool variables — fixed by **Hotfix 26.9.1**,
  which also fixed a second, independent defect that had silently prevented the documented
  rollback from ever running on any deployment failure.
- The next real deployment (run `31993611105`) proved Hotfix 26.9.1's rollback fix worked
  end-to-end on real HMG (STOP → restore-skip → START → health check → "Rollback completed and
  previous version is healthy.") but still failed the same way — a second, distinct root cause:
  `$script:hmgAllowedRecipients` collided, case-insensitively, with the `-HmgAllowedRecipients`
  script parameter of the same name. Fixed by **Hotfix 26.9.2**, which also added a read-only
  regression-suite preflight to `deploy-hmg.yml`, running before IIS is ever stopped.
- HMG deployment run [`32004712401`](https://github.com/tiagoarrigoni/BeeDay/actions/runs/32004712401)
  (run #188) completed successfully end to end on SERV3WEB: the new "Validate deployment script
  regression suite" preflight passed, "Deploy to IIS with rollback" passed, and the deployment
  completed successfully — **Environment Validated**, not merely code-complete.
- `DevelopmentEmailSender` itself was then Environment Validated on that same HMG state: a real
  account-creation flow produced `.txt`/`.json`/`.html` artifacts in `C:\Apps\BeeDay-Data\Emails`,
  proving the Sprint 26.9 content-root guard fix (§7 below) works against the real, committed
  absolute-path configuration.
- After Resend activation (step 4 below) merged, the first real deployment against it (run
  [`32009214798`](https://github.com/tiagoarrigoni/BeeDay/actions/runs/32009214798), #190) reached
  privileged IIS CONFIGURE — one HMG recipient configured, so `Deploy-BeeDay.ps1` wrote 9 App Pool
  variables — and failed there: `Invoke-BeeDayIisControl.ps1`'s own CONFIGURE allow-list (a fixed
  exact-name list, enforced on the SYSTEM-run privileged side of the boundary, independent of
  `Deploy-BeeDay.ps1`'s own contract) did not yet permit
  `BeeDay__Email__HmgRecipientGuard__AllowedRecipients__N`, so the very first recipient variable was
  rejected outright. Rollback ran exactly as designed (STOP → RESTORE → START → health check →
  "Rollback completed and previous version is healthy.") — Hotfix 26.9.1's rollback fix is unchanged
  and remains Environment Validated. Fixed by **Hotfix 26.9.3**, which extends the privileged
  allow-list with a narrow regex accepting only the exact
  `BeeDay__Email__HmgRecipientGuard__AllowedRecipients__<non-negative integer>` shape
  `Deploy-BeeDay.ps1` can produce — never a prefix/wildcard — and adds a second, read-only
  regression suite to the same preflight, covering the privileged script's own validator directly
  (the gap that let 18/18 pass on run #190 despite this exact mismatch).

Activating real Resend delivery on HMG required, in order:

1. ~~Merge the full Epic 26 PR stack (#143 → #152) and the two hotfixes (#154, #155) into `hmg`.~~
   **Done** — all merged, deployed, and Environment Validated per the history above.
2. ~~Deploy the merged `hmg` to SERV3-WEB through `deploy-hmg.yml`.~~ **Done** — run `32004712401`.
3. ~~The repository owner creates `BEEDAY_RESEND_API_KEY`, `BEEDAY_RESEND_FROM_ADDRESS`,
   `BEEDAY_RESEND_FROM_NAME`, and `BEEDAY_HMG_ALLOWED_RECIPIENTS` as GitHub secrets in the
   `homologation` Environment.~~ **Done** — all four confirmed present (names only; values never
   inspected, printed, or logged by this repository's tooling).
4. ~~A separate, explicitly-scoped change flips `appsettings.Homologation.json`'s
   `Resend:Enabled`/`Development:Enabled`.~~ **Done** — this section's own change, kept deliberately
   separate from Epic 26's sprint chain and from both hotfixes, exactly as originally planned.
5. **Pending** — redeploy through `deploy-hmg.yml` to make this activation live on SERV3WEB.
6. **Pending** — execute the smoke-test checklist in §11 for real, on HMG, with a real allowlisted
   address.

**Real email/inbox E2E through Resend remains pending** — steps 5–6 above have not occurred. Do not
infer or claim Resend delivery evidence from anything in this section: the deployment and
`DevelopmentEmailSender` successes above prove the deployment *mechanism* and the *previous*
provider are reliable, not that any email has been sent through Resend. The HMG recipient guard
(§5 above) stays enabled and fail-closed throughout — `HmgRecipientGuardOptions.Enabled` was not
touched, and the allowlist now comes from the real `BEEDAY_HMG_ALLOWED_RECIPIENTS` secret rather
than resolving empty.

## 7. Sender/subject behavior

- HTML body: brand color `#5247F9` (the sole approved beeday Brand Color), plain-text alternative
  always generated alongside it. Escaping/encoding, subject contract, and the documented decision
  not to localize email content:
  [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §13.
- Subject prefix (`[HMG] ` by default) is applied **once**, centrally, by
  `HmgRecipientGuardedEmailSender` — never duplicated per-flow, never applied twice to an
  already-prefixed subject.

## 8. `PublicBaseUrl` expectations

Every callback link is built from exactly one value, `IdentityEmailOptions.PublicBaseUrl`, never
from `HttpContext`/a forwarded header — no code path exists that could let one environment's link
cross into another (HMG↔Production↔localhost). Outside Development, Program.cs's own startup guard
requires an absolute HTTPS URL — proven by `ProductionOriginGuardTests`, which launches the real
built app as a separate process to verify the guard actually fires. Full audit:
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §12.2.

## 9. Deployment procedure

Standard `deploy-hmg.yml`/`deploy-prd.yml` flow — nothing email-specific changes it; the same
`Deploy-BeeDay.ps1` invocation now also accepts `-HmgAllowedRecipients` (§4). Full deployment
pipeline, artifact provenance, and the Build-Once-Deploy-Many contract:
[`01-deployment.md`](01-deployment.md); privileged IIS control mechanics:
[`05-privileged-iis-control.md`](05-privileged-iis-control.md).

## 10. Health/readiness verification

`/health/ready` includes the SQL Server check only — there is no dedicated email-subsystem health
check (an `IEmailSender`/Resend reachability probe was evaluated and not built: it would either make
a real, non-idempotent-adjacent network call as part of routine health polling, or only prove
network reachability, not delivery — low value for the operational cost). After any deploy touching
this Epic's code, verifying health means the standard checks in
[`03-observability.md`](03-observability.md), plus manually confirming (§11) that a real send still
reaches the expected state.

## 11. Controlled smoke-test checklist (for future real activation)

Never run against an unapproved real recipient. Order matches the Epic's own evidence semantics —
each step's evidence is distinct from the next; do not infer a later step from an earlier one:

1. Deploy completes; standard health check passes.
2. Trigger account creation (or resend-confirmation) for an address already on the
   `HmgRecipientGuardOptions.AllowedRecipients` list.
3. **Application requested send** — confirm via the "provider request attempted" log line
   ([`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §14.1), `Subject`
   only, no recipient/token in the log.
4. **HMG safety allowed the send** — confirm the guard's "allowed" log line, not a "blocked" one.
5. **Resend accepted the request** — confirm the "provider accepted" log line and its
   `ProviderMessageId`. This is provider acceptance, not delivery — do not stop here.
6. **Inbox received the message** — check the actual mailbox. This is the only step that proves
   delivery; nothing earlier substitutes for it.
7. Click the confirmation link; confirm it points at the expected `PublicBaseUrl` host and the
   callback succeeds.
8. Repeat steps 2–7 for resend-confirmation and for forgot-password/reset-password.
9. Final health check.

Capture evidence as: timestamps, log excerpts with the recipient already masked/omitted, the
provider message id, and a plain confirmation ("inbox received: yes/no") — never a screenshot or
copy of the email body containing the live token, never the raw allowlisted address beyond what
already appears in the masked/redacted evidence above.

### 11.1 Negative smoke — non-allowlisted recipient (EPIC 28, Sprint 28.8)

**Threat model:** prove that a recipient who is *not* on `HmgRecipientGuardOptions.AllowedRecipients`
is blocked before Resend is ever reached — i.e. the guard is not merely present in code, but actually
fail-closed in the deployed configuration. The risk this protects against: a misconfiguration or
regression that lets Homologation's Resend integration deliver to an arbitrary, non-approved address.

**Preconditions:**

- The guard is enabled (`HmgRecipientGuardOptions:Enabled` — default `true`; confirm it wasn't
  explicitly disabled for the environment under test).
- The recipient used for the negative case is **never** a real external address — use a
  syntactically-valid but non-deliverable address (an RFC 2606 reserved TLD, e.g.
  `something@example.invalid`) that is guaranteed absent from the allowlist. Never widen
  `AllowedRecipients` to "test" the block — that would test nothing.

**Automated evidence (proven this Sprint, safe to re-run anywhere, including CI):**
`tests/BeeDay.Infrastructure.Tests/HmgRecipientGuardDependencyInjectionTests.cs` —
`Host_WhenRecipientIsNotAllowlisted_TheRealResendHttpClientIsNeverInvoked` builds the real DI graph
(`AddBeeDayInfrastructure`, unmodified), resolves the real `HmgRecipientGuardedEmailSender` wrapping
the real `ResendEmailSender`, and replaces only the deepest possible seam — `ResendEmailSender`'s own
`HttpClient` transport — with a call-counting stub. Sending to a non-allowlisted recipient asserts
`CallCount == 0` at that exact boundary. Its positive-path counterpart,
`Host_WhenRecipientIsAllowlisted_TheRealResendHttpClientIsInvokedExactlyOnce`, proves the same harness
doesn't just happen to never call anything — the allowlisted case reaches the transport exactly once.
This is the strongest automated proof available without deploying and sending through the real
Resend API.

**Runtime evidence on real HMG (`POST-MERGE PENDING` until the merged commits are deployed):**

1. Deploy completes; standard health check passes.
2. Trigger a flow (e.g. resend-confirmation or forgot-password) for the same kind of
   `*.invalid`-style, deliberately non-allowlisted address used above — never a real external
   address.
3. Confirm the guard's **blocked** log line appears
   (`EmailEventIds.GuardBlocked`, 7101 — [`03-observability.md`](03-observability.md) §2.1), with no
   recipient in the log line (proven never to happen, `HmgRecipientGuardedEmailSenderTests.SendAsync_NeverLogsTheRawRecipientAddress_ForAllowedOrBlockedRecipients`).
4. Confirm **no** "provider request attempted" (`ProviderAttempted`, 7103) log line follows — the
   absence of that EventId in the stdout window around the blocked line is the runtime evidence that
   Resend was never invoked (this repository has no code path that could call Resend without first
   logging that attempt, per §14.1 of the transactional-email doc).
5. Confirm the subject prefix (`[HMG] `) is irrelevant to this outcome — the guard blocks by
   recipient, never by subject/body content (`HmgRecipientGuardedEmailSenderTests` proves the prefix
   logic and the allow/block decision are independent).

If real HMG does not offer a safe way to trigger step 2 without also attempting a real send to a
non-allowlisted address through some other path, do not invent one — stop, document the limitation,
and rely on the automated evidence above as the Gate D-local proof; runtime confirmation stays
`POST-MERGE PENDING` explicitly, not silently assumed.

**Explicit rule:** the guard blocking a recipient must always be provable as "the provider was never
reached," never merely "the app returned success/silently" — a future change that made the block
happen *after* a Resend call (e.g. by discarding the response) would violate this rule even though
the email would still not be delivered; this negative smoke exists specifically to catch that class
of regression, not just "no email arrived."

## 12. Rollback procedure

Not email-specific — the same mechanism protects every deploy. `Deploy-BeeDay.ps1` automatically
rolls back **application files only** (never the SQL Server schema/data, never the `Data`
directory's backup — that's captured but never auto-restored) if any step between stopping IIS and
the post-deploy health check throws. Manual restore of an older backup
(`C:\Apps\BeeDay-Backups\Application\BeeDay-{timestamp}`) is not automated. Full procedure and its
explicit limits: [`04-operations.md`](04-operations.md) §3.

## 13. Troubleshooting flow

| Symptom | Likely cause | Where to look |
|---|---|---|
| No captured file in `Data/Emails` (Development/HMG) | Before Sprint 26.9: the content-root guard rejecting an absolute `Directory`. After: check `Enabled`, disk permissions, or an unrelated exception in the "provider request attempted" log gap | [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §6, §16.3 |
| Host fails to start with an `OptionsValidationException` mentioning Resend | Missing `ApiKey`/`FromAddress` when `Resend:Enabled=true` | §4 above, `ResendOptions` validation |
| Host fails to start mentioning "allowed recipient" | `HmgRecipientGuardOptions` left at its fail-closed default (Resend selected, no allowlist configured) | §4/§5 above |
| Host fails to start mentioning `PublicBaseUrl` | Missing/relative/non-HTTPS value outside Development | §8 above |
| "Provider request attempted" logged, no "accepted"/"rejected" follows | Transient network error or `HttpClient` timeout — check the `Error`-level log line's classification | [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §14.2 |
| User reports registration failed but the account exists | The known persistence-succeeds/delivery-fails boundary — direct them to resend-confirmation | [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §3.1/§12.3/§15.4 |
| Email sent to the wrong environment's recipient | Should be structurally impossible (§8) — if observed, treat as a P0 regression in `IdentityEmailComposer`/`PublicBaseUrl` resolution, not a configuration issue |  |
| Need to filter stdout logs for one specific email state (e.g. only "guard blocked") | Every log line in the email path now carries a typed `EventId` (7100-7109) — filter on `EventId.Id` in the JSON stdout, not message text | [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §14.1, [`03-observability.md`](03-observability.md) §2.1 (EPIC 28, Sprint 28.7) |
| stdout log directory accumulating old files | No automatic rotation is provisioned — run `scripts/Clear-BeeDayStdoutLogs.ps1` manually, or schedule it | [`03-observability.md`](03-observability.md) §7 (EPIC 28, Sprint 28.7) — Code Complete, not yet Environment Validated |

## 14. Key rotation procedure

Not yet exercised for real (no Resend key has been rotated in this Epic — HMG's key predates it, and
no PRD key exists). Based on the existing secret-injection contract (§4), rotation is:

1. Generate the new key in Resend's own dashboard (outside this repository).
2. Update the GitHub Environment secret (`homologation` or `production`) with the new value — never
   via chat/prompt to an AI agent, never committed.
3. Redeploy through `deploy-hmg.yml`/`deploy-prd.yml` — `Deploy-BeeDay.ps1` overwrites the App Pool's
   `BeeDay__Email__Resend__ApiKey` variable on every run when the secret is present (§4).
4. Verify with §11's smoke test before considering rotation complete.
5. Revoke the old key in Resend only after confirming the new one works — never revoke first.

## 15. Observable delivery-state terminology

`send requested` → `safety blocked` | `provider request attempted` → `provider accepted` |
`provider request failed` (3 classified causes: transient network error, `HttpClient` timeout,
provider rejection). Full state table, where each is logged, and what "provider accepted" explicitly
does *not* mean (mailbox delivery):
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §14.1–§14.2.

## 16. Common failure classes

Transient network/connection error, `HttpClient` timeout (distinct from caller cancellation),
provider rejection (non-2xx), configuration error (caught earlier, at startup, never reaching the
sender at all), HMG safety block (not a failure — a deliberate, logged suppression). No automatic
retry exists for any of these — by design, not omission:
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §14.2.

## 17. Security/logging restrictions

Never logged anywhere in the email path: the Resend API key, password hashes, confirmation/reset
tokens, a full token-bearing callback URL. `ResendEmailSender`/`HmgRecipientGuardedEmailSender` never
log the recipient address in any branch; `DevelopmentEmailSender`'s two log lines mask it
(`EmailAddressLogMasking`) — the captured `.html`/`.json`/`.txt` files on disk still carry the full
address, needed for local diagnosis. Full policy:
[`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §14.3.

## 18. Evidence to capture during a future PRD promotion

Same shape as §11's checklist, plus: the exact `hmg`→`main`→`prd` PR provenance chain
([`CLAUDE.md`](../../CLAUDE.md) §5.7.2, [`12-artifact-provenance.md`](12-artifact-provenance.md)),
confirmation that PRD's `BEEDAY_RESEND_API_KEY` differs from HMG's (§4.1), and confirmation that the
`production` GitHub Environment was created with its own protection rules before first real use.
Never capture: the API key value in any form, a raw confirmation/reset token, a screenshot of an
email body containing a live callback link.

## 19. Residual known gaps (carried forward, not resolved by this Epic)

- The mass-registration volume/IP-based abuse vector — the per-email throttle added in Sprint 26.7
  cannot address it; would need a new, separately-scoped rate limiter.
- `AllowedHosts`/`DataProtectionKeysDirectory` production startup guards remain untested (only
  `PublicBaseUrl` was, per this Epic's scope).
- No dedicated email-subsystem health check exists (§10) — evaluated, not built.
- `deploy-prd.yml`'s `production` GitHub Environment does not exist yet (§4.1) — part of PRD's
  future real provisioning, not this Epic's scope.
- Real HMG *email* E2E evidence (§11 actually executed against Resend) does not exist yet. This is
  no longer an infrastructure-access blocker — Hotfixes 26.9.1/26.9.2 proved real HMG deployment
  evidence is obtainable (GitHub Actions run logs, `gh` CLI), run `32004712401` confirmed the
  deployment mechanism itself is reliable, and the Resend secrets and configuration activation
  (§6 steps 1–4) are all complete. The first deployment attempt against the activated
  configuration (run `32009214798`, #190) itself failed on a *third*, distinct root cause — a
  privileged-boundary contract mismatch, fixed by Hotfix 26.9.3 — proving the deployment mechanism
  was not yet fully exercised for this specific payload shape (one HMG recipient variable) before
  that run. The remaining blocker is §6 steps 5–6: Hotfix 26.9.3 has not yet been deployed through
  `deploy-hmg.yml`, and the real smoke test (§11) has not been executed. See
  [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §16.

## 20. Related documentation

- [`docs/infrastructure/06-transactional-email.md`](../infrastructure/06-transactional-email.md) —
  owning document for the architecture, audit trail, and every sprint's findings this runbook
  indexes.
- [`02-runtime-configuration.md`](02-runtime-configuration.md), [`01-deployment.md`](01-deployment.md),
  [`04-operations.md`](04-operations.md), [`05-privileged-iis-control.md`](05-privileged-iis-control.md),
  [`03-observability.md`](03-observability.md) — the general (not email-specific) deployment/
  operations documents this runbook draws from.
