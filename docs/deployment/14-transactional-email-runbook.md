# Transactional Email — Production Readiness Runbook

**Source of truth:** this document is an operational index over facts already established and
verified elsewhere in this repository — it does not re-derive them. Every claim below links to the
document that owns it. Built at the close of Epic 26 (Sprints 26.1–26.10, 2026-08-16); re-verify
against the linked documents if either drifts from this one.

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
| Homologation | `false` (committed) | `true` (committed) | `DevelopmentEmailSender` — **deliberately not yet switched to Resend** (§6 below) |
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

## 6. Current HMG state — Resend is not active

**As of this Epic's close, HMG still resolves to `DevelopmentEmailSender` (§2 table)** —
`Resend:Enabled=false` in the committed `appsettings.Homologation.json`. The empty
`C:\Apps\BeeDay-Data\Emails` directory that originally triggered this Epic (§7 below) was a code
defect (the content-root guard), not a configuration or credential problem — that defect is fixed as
of Sprint 26.9, but **the fix has not yet been deployed to HMG**, and Resend has not been activated
there. Activating real Resend delivery on HMG requires, in order:

1. Merge the full Epic 26 PR stack (#143 → #152, ascending, one at a time — see §14) into `hmg`.
2. Deploy the merged `hmg` to SERV3-WEB through `deploy-hmg.yml` — this alone makes the Sprint 26.9
   directory-guard fix live, independent of whether Resend is ever enabled.
3. The repository owner creates the `BEEDAY_HMG_ALLOWED_RECIPIENTS` GitHub secret (homologation
   environment) with the real, approved recipient list — semicolon-separated (§4).
4. A separate, explicitly-scoped change flips `appsettings.Homologation.json`'s
   `Resend:Enabled`/`Development:Enabled` — **not done by this Epic**, since it is a real behavior
   activation decision requiring deliberate review, not something to bundle into a stacked-branch
   sprint chain.
5. Redeploy through `deploy-hmg.yml` again.
6. Execute the smoke-test checklist in §11 for real, on HMG, with a real allowlisted address.

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
- Real HMG E2E evidence (§11 actually executed) does not exist yet — blocked on infrastructure
  access this Epic never had; see [`06-transactional-email.md`](../infrastructure/06-transactional-email.md) §16.

## 20. Related documentation

- [`docs/infrastructure/06-transactional-email.md`](../infrastructure/06-transactional-email.md) —
  owning document for the architecture, audit trail, and every sprint's findings this runbook
  indexes.
- [`02-runtime-configuration.md`](02-runtime-configuration.md), [`01-deployment.md`](01-deployment.md),
  [`04-operations.md`](04-operations.md), [`05-privileged-iis-control.md`](05-privileged-iis-control.md),
  [`03-observability.md`](03-observability.md) — the general (not email-specific) deployment/
  operations documents this runbook draws from.
