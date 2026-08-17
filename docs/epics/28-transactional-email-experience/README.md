# EPIC 28 — Transactional Email Experience, Deliverability & Observability

**Fonte da verdade:** contexto oficial da EPIC 28 recebido do responsável pelo repositório (pacote
`epic-28-claude-autonomous`, 2026-08-17); achados da Sprint 28.1 verificados diretamente em código
nesta mesma Sprint (branch `sprint/28.1-email-baseline`, criada a partir de `hmg` em `504a2a7`) —
leitura direta de `src/BeeDay.Application/`, `src/BeeDay.Infrastructure/`, `src/BeeDay.Web/`,
`docs/infrastructure/06-transactional-email.md`, `docs/deployment/14-transactional-email-runbook.md`,
`docs/web/07-localization.md`, `docs/brand/02-writing-voice-localization.md`, `web.config`,
`Deploy-BeeDay.ps1`, `deploy-hmg.yml`, e `git log`/`git show`. Nenhuma afirmação de "estado atual"
abaixo vem de memória do pacote da Epic — cada uma foi reverificada no checkout desta Sprint.

**Última verificação:** 2026-08-17 (Sprint 28.1 — Repository Baseline & Owner Map).

**Escopo da Epic:** evoluir os e-mails transacionais entregues pela EPIC 26 (transporte funcional
validado) para uma superfície oficial do beeday Experience System: identidade visual, copy,
localization, deliverability auditada, observability operacional, HMG Guard validado em runtime
(positivo já provado na EPIC 26; negativo ainda pendente), client compatibility. A EPIC 28 não refaz
a EPIC 26. Detalhe completo do objetivo, princípios e fora-de-escopo: ver o pacote de planejamento
fornecido pelo responsável do repositório (não versionado neste diretório).

## Source of Truth

- Arquitetura de e-mail transacional (Epic 26): [`docs/infrastructure/06-transactional-email.md`](../../infrastructure/06-transactional-email.md)
  — dono da narrativa arquitetural/auditoria (*por que*).
- Estado operacional/deploy do e-mail transacional: [`docs/deployment/14-transactional-email-runbook.md`](../../deployment/14-transactional-email-runbook.md)
  — dono do estado corrente/*como operar* (atualizado hoje mesmo, 2026-08-17, antes desta Sprint).
- Localization Web: [`docs/web/07-localization.md`](../../web/07-localization.md).
- Voice/Tone/Writing/Localization de produto, incluindo a seção "E-mail transacional": [`docs/brand/02-writing-voice-localization.md`](../../brand/02-writing-voice-localization.md).
- Brand contract (`beeday` lowercase, `#5247F9`): [`CLAUDE.md`](../../../CLAUDE.md) §13,
  [`docs/epics/25-design-system-brand-evolution/README.md`](../25-design-system-brand-evolution/README.md).
- Governança de Git/aprovação: [`CLAUDE.md`](../../../CLAUDE.md).

---

## Sprint 28.1 — Repository Baseline & Owner Map

### Repository State

- Branch: `sprint/28.1-email-baseline`, criada a partir de `hmg` em `504a2a7` (fast-forward local
  antes da criação da branch; `hmg` estava 26 commits atrás do remote e foi sincronizada com
  `git pull --ff-only`).
- Camadas por `CLAUDE.md` §3: `BeeDay.Domain`, `BeeDay.Application`, `BeeDay.Infrastructure`,
  `BeeDay.Web`, com projetos de teste espelhando cada camada mais `BeeDay.E2E.Tests`.
- A EPIC 26 (infraestrutura de e-mail transacional) está integralmente documentada em
  `docs/infrastructure/06-transactional-email.md` (fechada na Sprint 26.10, verdict "Production
  Ready, not Production Activated"). Não existe pasta `docs/epics/26-*` — a narrativa da EPIC 26 vive
  inteiramente nesse doc de infraestrutura.
- **Achado inesperado, não fabricado pela EPIC 28:** no mesmo dia desta Sprint, ANTES do início da
  EPIC 28, o commit `4e98e1d8` ("feat(email): activate Resend for Homologation") flipou
  `appsettings.Homologation.json` para `Resend:Enabled: true` / `Development:Enabled: false` e
  atualizou o runbook (`14-transactional-email-runbook.md` §2/§6/§19) de acordo. Isso muda o
  provider *comprometido* (committed) de Homologation, mas **não** o *implantado* (runtime) — o
  runbook já documenta isso corretamente como código-completo, ainda não redeployado (Hotfix 26.9.3
  pendente). Ver Divergence Matrix, item D1.

### Mandatory Documentation Reviewed

`CLAUDE.md`, `README.md` (raiz), `docs/README.md`, `docs/infrastructure/06-transactional-email.md`,
`docs/deployment/14-transactional-email-runbook.md`, `docs/deployment/03-observability.md`,
`docs/web/07-localization.md`, `docs/brand/02-writing-voice-localization.md`,
`docs/design-system/01-foundations.md` (typography/brand color sections), `web.config`,
`.github/workflows/deploy-hmg.yml`, `scripts/Deploy-BeeDay.ps1`.

### Transactional Email Architecture Found

```text
BeeDay.Domain          — no email-sending concept (EmailAddress value object only).
BeeDay.Application      — IEmailSender, IIdentityEmailComposer, IEmailConfirmationIssuer,
  (Common/Identity/)      IIdentityRequestThrottle contracts; EmailMessage record. Handlers in
                          Features/Users, Features/Identity depend only on these interfaces.
BeeDay.Infrastructure   — ResendEmailSender, DevelopmentEmailSender, HmgRecipientGuardedEmailSender
  (Identity/,             (all IEmailSender), IdentityEmailComposer (IIdentityEmailComposer),
   Configuration/,        EmailProviderSelector, 5 Options classes bound in
   DependencyInjection/)  InfrastructureServiceCollectionExtensions.
BeeDay.Web              — composition root only (AddBeeDayInfrastructure call in Program.cs);
                          appsettings*.json carry per-environment values; Identity Razor pages call
                          Application via MediatR; never reference a concrete Infrastructure email
                          type.
```

No dependency-direction violation found — confirmed by the existing
`PersistenceContractBoundaryTests` (`tests/BeeDay.Application.Tests/PersistenceContractBoundaryTests.cs:23`),
which already asserts Application does not reference Infrastructure.

### EmailMessage Contract

`src/BeeDay.Application/Common/Identity/IEmailSender.cs:8`:

```csharp
public sealed record EmailMessage(string Recipient, string Subject, string HtmlBody, string? PlainTextBody = null);
```

Carries `Recipient`, `Subject`, `HtmlBody`, `PlainTextBody` (optional). Does **not** carry preheader,
Reply-To, From/FromName (set at the sender level via `ResendOptions`, not per-message), locale/culture,
or a template identifier. Any of these additions is a public Application contract change touching
every call site (4 today) — must be assessed explicitly in 28.2/28.3, not introduced incidentally.

### IdentityEmailComposer Analysis

`IIdentityEmailComposer` (`src/BeeDay.Application/Common/Identity/IIdentityEmailComposer.cs:3-7`,
Application, contract only) exposes `ComposeEmailConfirmation`/`ComposePasswordReset`, both taking
`(recipient, displayName, rawToken)` — **no locale/culture parameter today.**

Implementation `IdentityEmailComposer` (`src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs`,
Infrastructure):
- Subject/intro/footer are hardcoded English `const string` literals (lines 15-16, 26-27).
- URLs built via `BuildUrl` (lines 34-44) from exactly one config value
  (`IdentityEmailOptions.PublicBaseUrl`) — never from `HttpContext`/forwarded headers.
- `BuildHtmlTemplate` (lines 54-86) HTML-encodes every interpolated value via `WebUtility.HtmlEncode`;
  `BuildPlainTextTemplate` (lines 92-108) does not (correct — plain text has no injection surface).
- Brand color `#5247F9` hardcoded as `private const string BrandColor` (line 52) — matches
  `--beeday-color-brand-primary` in `wwwroot/css/variables.css:4`, tracked only by code comment
  convention, not an automated check.
- Template surface (dark background `#17131f`/text `#f4efff`, line 73) does not match the product's
  actual default light surface (`--beeday-color-surface: #ffffff`) — already flagged by
  `06-transactional-email.md` §13.2 as a known, not-yet-addressed visual gap. Direct input for 28.4.
- One shared `BuildHtmlTemplate`/`BuildPlainTextTemplate` pair serves both flows today — the
  single-template-owner extension point for whatever 28.3/28.4 build on top of it.

### Provider & Guard Analysis

All three senders implement `Application.Common.Identity.IEmailSender`, live in
`BeeDay.Infrastructure/Identity/`:

| Class | Role |
|---|---|
| `ResendEmailSender` | Wraps `HttpClient`, POSTs to `https://api.resend.com/emails`. Never logs recipient or API key. |
| `DevelopmentEmailSender` | Writes `.html`/`.txt`/`.json` to a configured directory; logs recipient masked via `EmailAddressLogMasking.Mask`. |
| `HmgRecipientGuardedEmailSender` | Decorator wrapping `IEmailSender innerSender`. |

**Call chain when Resend is selected** (confirmed guard-before-provider):

```text
MediatR Handler (Application)
  → IEmailSender.SendAsync
  → HmgRecipientGuardedEmailSender.SendAsync   (HmgRecipientGuardedEmailSender.cs:26)
      !Enabled              → innerSender.SendAsync(...)                          [30-34]
      recipient not allowed → LogWarning, RETURN (inner never called)             [36-40]
      else                   → prefix subject once, LogInformation, → inner       [42-47]
  → ResendEmailSender.SendAsync → HttpClient POST https://api.resend.com/emails
```

**Fail-closed confirmed:** `HmgRecipientGuardOptions` (`Infrastructure/Configuration/HmgRecipientGuardOptions.cs:11-18`,
`Enabled` default `true`, `AllowedRecipients` default `[]`) is bound + `.Validate(...).ValidateOnStart()`
only inside the Resend DI branch (`InfrastructureServiceCollectionExtensions.cs:89-93`) — an
environment selecting Resend with an empty allowlist **fails to start**
(`HmgRecipientGuardDependencyInjectionTests.Host_WhenResendSelectedAndGuardLeftAtDefault_FailsToStartPredictably`).
`appsettings.Production.json` is the one deliberate, committed opt-out (`Enabled: false`).

**Provider selection**: `EmailProviderSelector.Resolve` (`Configuration/EmailProviderSelector.cs:12-21`),
called once at DI-registration time, throws if `Resend:Enabled`/`Development:Enabled` are both true or
both false — no silent ambiguous state.

### Identity Handler Call Graphs

Both flows live entirely in `BeeDay.Application`, depending only on Application contracts; `BeeDay.Web`
never references a concrete Infrastructure email type.

**Confirm Email / Account Creation:**
```text
CreateAccountCommandHandler.Handle / CreateUserCommandHandler.Handle (legacy)  [UserHandlers.cs]
  → confirmationIssuer.Issue(user) → emailComposer.ComposeEmailConfirmation(...)
  → (DB transaction commits FIRST, outside the try/finally that owns the send — §"registration
     commits before email send", known accepted gap, `06-transactional-email.md` §3.1/§12.3/§15.4)
  → emailSender.SendAsync(...)  → guarded/dev sender per DI selection

Resend confirmation: ResendEmailConfirmationCommandHandler.Handle (IdentityHandlers.cs:48-87)
  → emailComposer.ComposeEmailConfirmation(...) → emailSender.SendAsync(...)

Callback: ConfirmEmail.razor (Web) → ConfirmEmailCommandHandler.Handle — consumes token only, no email.
```

**Reset Password:**
```text
ForgotPassword.razor (Web) → RequestPasswordResetCommandHandler.Handle (IdentityHandlers.cs:89-128)
  throttled (60s), silent no-op if user unknown/inactive/unconfirmed (enumeration-safe)
  → emailComposer.ComposePasswordReset(...) → emailSender.SendAsync(...)

Callback: ResetPassword.razor (Web) → ResetPasswordCommandHandler.Handle — consumes token, no email.
```

### Config Binding Map

| Options class | `SectionName` | Key fields |
|---|---|---|
| `IdentityEmailOptions` | `BeeDay:IdentityEmail` | `PublicBaseUrl`, `ConfirmationPath`, `PasswordResetPath` |
| `ResendOptions` | `BeeDay:Email:Resend` | `Enabled`, `ApiKey`, `FromName` (default `"BeeDay"`), `FromAddress` |
| `DevelopmentEmailOptions` | `BeeDay:Email:Development` | `Enabled`, `Directory` |
| `HmgRecipientGuardOptions` | `BeeDay:Email:HmgRecipientGuard` | `Enabled`, `AllowedRecipients`, `SubjectPrefix` (default `"[HMG] "`) |

`AllowedRecipients` is never a literal in any committed `appsettings*.json` — bound only via
`BeeDay__Email__HmgRecipientGuard__AllowedRecipients__<index>` App Pool env vars, written by
`Deploy-BeeDay.ps1` from the `BEEDAY_HMG_ALLOWED_RECIPIENTS` GitHub secret.

**Per-environment effective provider (current committed state, 2026-08-17):**

| Environment | Effective provider (committed) | Effective provider (deployed/runtime) |
|---|---|---|
| Development | `DevelopmentEmailSender` | n/a (local only) |
| Homologation | `ResendEmailSender` behind `HmgRecipientGuardedEmailSender` (as of `4e98e1d8`, today) | Still `DevelopmentEmailSender` — SERV3WEB has not been redeployed since the activation commit (runbook §6) |
| Production | `ResendEmailSender`, guard explicitly `Enabled: false` | No runtime exists (PRD not provisioned, `CLAUDE.md` §8.2) |

### Localization Current State

- Culture pipeline is entirely Web-owned and HTTP-request-shaped: `RequestLocalizationOptions` with
  `CookieRequestCultureProvider` + `AuthenticatedAccountCultureProvider`
  (`src/BeeDay.Web/Localization/AuthenticatedAccountCultureProvider.cs:29-51`), pipeline ordered so
  the latter can read the authenticated `User` off `HttpContext.Items` (`Program.cs:255-260`).
- **User language IS persisted independently of any HTTP request**: `User.Language`
  (`src/BeeDay.Domain/Entities/User.cs:15`, Domain enum `UserLanguage` — `English`/`Portuguese`),
  settable via `User.UpdatePreferences`. This value is reachable by Application/Infrastructure via
  `IUserRepository` regardless of HTTP context — nothing currently reads it for email composition,
  but the data already exists at the correct layer. This is the key building block for 28.2's
  localization contract decision.
- `IdentityEmailComposer` uses **zero localization** today — hardcoded English `const string`
  literals, no `IStringLocalizer`, no culture parameter on the composer interface. Confirmed by code
  and by explicit documentation in both `06-transactional-email.md` §13.5 and
  `docs/brand/02-writing-voice-localization.md` ("E-mail transacional" section, lines 147-158),
  which already states the deferral and the two candidate directions (narrow Infrastructure-owned
  email resource catalog, or move composition to Web) without picking one — that decision belongs to
  Sprint 28.2, not 28.1.
- 19 resx catalogs exist under `src/BeeDay.Web/` (1181 keys per culture in `.en-US.resx`), all
  Web-owned. `docs/web/07-localization.md` §8 said 17/650 (Sprint 25.14 snapshot) — corrected in this
  Sprint (see Divergence Matrix D2).
- Identity Razor pages around the email (ConfirmEmail, ForgotPassword, etc.) are already fully
  localized via `IStringLocalizer<IdentityResources>` — the web page around the link is localized,
  the email itself is not. Clean before/after boundary for 28.2+.

### Experience System Current State

- Brand primary `#5247F9` — `src/BeeDay.Web/wwwroot/css/variables.css:4`
  (`--beeday-color-brand-primary`). Matches `CLAUDE.md` §13.
- Typography (`docs/design-system/01-foundations.md`): Product/UI = `--beeday-font-body` = "Nunito",
  "Segoe UI", sans-serif; Brand/Display = `--beeday-font-display` = "Coiny", "Nunito", "Segoe UI",
  sans-serif — Coiny explicitly not a product/body font. Email is a client-constrained surface; 28.2
  must decide font policy for email (webfonts are not reliable in email clients) without inventing a
  parallel design system, per the package's explicit constraint.
- `docs/brand/02-writing-voice-localization.md` is the existing Writing/Voice/Tone/Glossary system
  and already names the transactional email gap — the natural anchor for 28.4's copy work.

### Observability, IIS, and Deployment Current State

- **Logging**: `HmgRecipientGuardedEmailSender` logs allow/block with **no recipient, no typed
  `EventId`** (`LogWarning`/`LogInformation`, plain message templates). `ResendEmailSender` logs
  attempt (Subject only)/accepted (`ProviderMessageId` only)/rejected (3 classified failure causes) —
  never the recipient. `DevelopmentEmailSender` is the one sender that logs a recipient, and only
  masked (`EmailAddressLogMasking.Mask`). No typed `EventId` exists anywhere in the email path —
  consistent with `docs/deployment/03-observability.md:42-44`, which states `WebEventIds.RequestFailed`
  (6100) is the only typed `EventId` in the entire repository.
- `web.config:8-17`: `stdoutLogEnabled="true"`, `stdoutLogFile="C:\Apps\BeeDay-Data\Logs\stdout"`,
  `hostingModel="inprocess"`. No app-level file sink — IIS's `AspNetCoreModuleV2` owns stdout capture.
- **Deployment**: `Deploy-BeeDay.ps1` creates the `Logs` directory (`$externalDirectories`, alongside
  `Emails`, `EventJournal`, `DataProtection-Keys`) but explicitly does **not** set ACLs — ACL
  provisioning is administratively performed out-of-band (script comment, line ~953: "ACLs are
  provisioned administratively — this deploy never modifies them"). No script in this repository was
  found that provisions ACLs for the stdout log directory specifically.
- **Documentation gap, not a divergence** (nothing contradicts, it is simply silent): `docs/deployment/03-observability.md`
  was last verified 2026-08-09 (Sprint 18.5), predating all of Epic 26 — it contains no mention of
  Resend/HMG-guard/DevelopmentEmailSender logging at all. This is squarely Sprint 28.7's scope
  ("Observability Operationalization") and was deliberately **not** edited in this Sprint to avoid
  prejudging that Sprint's design — flagged here as its primary input instead.

### Test Inventory

| Concern | Test file(s) |
|---|---|
| `EmailMessage`/`IdentityEmailComposer`/`ResendEmailSender` | `tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs` |
| `DevelopmentEmailSender` | `tests/BeeDay.Infrastructure.Tests/DevelopmentEmailSenderTests.cs` |
| `HmgRecipientGuardedEmailSender` | `HmgRecipientGuardedEmailSenderTests.cs`, `HmgRecipientGuardDependencyInjectionTests.cs` |
| Provider selection | `EmailProviderSelectorTests.cs`, `EmailProviderDependencyInjectionTests.cs` |
| Recipient masking | `EmailAddressLogMaskingTests.cs` |
| Identity handlers (Application) | `IdentityHandlersTests.cs`, `AccountRegistrationTests.cs` (`tests/BeeDay.Application.Tests/`) |
| Confirm/reset integration (Web) | `tests/BeeDay.Web.Tests/Integration/EmailConfirmationIntegrationTests.cs`, `PasswordResetIntegrationTests.cs` |
| No real external send in tests | `EmailCaptureWebApplicationFactory.cs` (config override captures files instead of calling Resend); all `HttpClient` calls stubbed in `IdentityInfrastructureTests.cs` |
| Localization (Web-side, adjacent) | `IdentityFlowLocalizationIntegrationTests.cs`, `CultureCookieIntegrationTests.cs`, `AuthenticatedCultureIntegrationTests.cs`, `ResourceCatalogContractTests.cs`, `BeeDayCulturesTests.cs`, `AuthenticatedAccountCultureProviderTests.cs` |
| Architecture boundary | `PersistenceContractBoundaryTests.cs` (Application does not reference Infrastructure) |

No test asserts culture-specific email body content — consistent with localization being "not
implemented" today.

### Deliverability Repository Baseline

- **Critical, unresolved discrepancy (flagged, not silently resolved):** the Epic 28 initial-prompt
  package states a real Resend send to a real mailbox landed in Junk for at least one case. The
  currently-committed runbook (`14-transactional-email-runbook.md` §6, updated *this same day*,
  before this Sprint started) states explicitly: **"Real email/inbox E2E through Resend remains
  pending"** — steps 5-6 of the activation checklist (redeploy, real smoke test) have not occurred.
  This Sprint does not resolve this discrepancy — it is evidence outside the repository (an inbox
  observation) that cannot be verified from code/docs alone. **Decision required before 28.5**: the
  deliverability audit must treat any "Junk placement" claim as an unverified hypothesis unless the
  repository owner confirms when/how that observation was made (e.g., a manual test predating the
  documented activation, or evidence to be supplied separately) — 28.5 must not assume it as
  established fact from the package alone.
- SPF/DKIM/DMARC, sending domain, and Resend Insights were not investigated in this Sprint (out of
  scope for 28.1 — Sprint 28.5's job) beyond confirming no DNS records are managed inside this
  repository (no `.zone` files, no DNS-as-code found).

### Owner Matrix

| Concern | Owner | Layer | Contract | Implementation | Consumers | Tests | Docs |
|---|---|---|---|---|---|---|---|
| Email intent/message | `EmailMessage` | Application | `Common/Identity/IEmailSender.cs` | (record, no separate impl) | All 4 handlers | `IdentityInfrastructureTests.cs` | `06-transactional-email.md` §2 |
| Composition | `IIdentityEmailComposer` / `IdentityEmailComposer` | Application (contract) / Infrastructure (impl) | `Common/Identity/IIdentityEmailComposer.cs` | `Infrastructure/Identity/IdentityEmailComposer.cs` | 4 handlers | `IdentityInfrastructureTests.cs` | `06-transactional-email.md` §13 |
| Provider abstraction | `IEmailSender` | Application | `Common/Identity/IEmailSender.cs` | 3 Infrastructure senders | DI-selected | multiple | `06-transactional-email.md` §2/§4 |
| Resend | `ResendEmailSender` | Infrastructure | `IEmailSender` | `Identity/ResendEmailSender.cs` | Guard | `IdentityInfrastructureTests.cs` | `06-transactional-email.md`, runbook |
| Development/File | `DevelopmentEmailSender` | Infrastructure | `IEmailSender` | `Identity/DevelopmentEmailSender.cs` | DI (non-Resend envs) | `DevelopmentEmailSenderTests.cs` | `06-transactional-email.md` §6-7 |
| HMG Guard | `HmgRecipientGuardedEmailSender` | Infrastructure | `IEmailSender` (decorator) | `Identity/HmgRecipientGuardedEmailSender.cs` | DI (Resend envs) | `HmgRecipientGuardedEmailSenderTests.cs`, `HmgRecipientGuardDependencyInjectionTests.cs` | `06-transactional-email.md` §10, runbook §5-6 |
| `PublicBaseUrl` | `IdentityEmailOptions` | Infrastructure (options) | config-bound | `Configuration/IdentityEmailOptions.cs` | `IdentityEmailComposer` | — | runbook §8 |
| Localization (Web) | `BeeDayCultures`, `AuthenticatedAccountCultureProvider` | Web | — | `Localization/` | Razor pages | `BeeDayCulturesTests.cs`, integration tests | `07-localization.md` |
| User language (Domain) | `User.Language` | Domain | `Entities/User.cs` | persisted field | `AuthenticatedCultureSynchronizer`, (potentially) email composer post-28.2 | — | `07-localization.md` (indirect) |
| Brand/Writing owner | `docs/brand/02-writing-voice-localization.md` | (doc) | — | — | — | — | itself |
| HTML/plain text | `IdentityEmailComposer.BuildHtmlTemplate`/`BuildPlainTextTemplate` | Infrastructure | — | same file | — | `IdentityInfrastructureTests.cs` | `06-transactional-email.md` §13 |
| Logs | `HmgRecipientGuardedEmailSender`/`ResendEmailSender`/`DevelopmentEmailSender` `ILogger` calls | Infrastructure | — | inline | — | `EmailAddressLogMaskingTests.cs` | `06-transactional-email.md` §14 (gap: `03-observability.md`) |
| IIS stdout | `web.config` | Web (config) | — | `web.config:8-17` | IIS `AspNetCoreModuleV2` | — | `03-observability.md` (gap, see above) |
| Deploy | `Deploy-BeeDay.ps1` / `deploy-hmg.yml` | (script/workflow) | — | `scripts/Deploy-BeeDay.ps1` | CI | script regression suite (per runbook §6) | runbook §9 |
| Privileged IIS control | `Invoke-BeeDayIisControl.ps1` | (script) | — | (not re-read this Sprint) | `Deploy-BeeDay.ps1` | (per runbook §6 history) | `05-privileged-iis-control.md` |

### Divergence Matrix

| ID | Documentation says | Implementation says | Evidence | Severity | Owner/decision |
|---|---|---|---|---|---|
| D1 | `06-transactional-email.md` §5.1 (as of Sprint 26.10, still present-tense before this Sprint's fix): Homologation resolves to `DevelopmentEmailSender` | Committed `appsettings.Homologation.json` (commit `4e98e1d8`, 2026-08-17, same day, before 28.1) resolves to `ResendEmailSender`/guard | `git show 4e98e1d8`; file read | High (materially affects 28.2/28.5 assumptions) | **Resolved this Sprint**: added Update note + inline footnotes to `06-transactional-email.md`, deferred "current state" ownership to the already-correct runbook §2/§6. No functional code changed. |
| D2 | `docs/web/07-localization.md` §8: "17 catálogos... 650 chaves" (Sprint 25.14 snapshot) | 19 resx catalogs exist, 1181 keys/culture (`ExperienceSystemResources`, `InstitutionalResources` added after Sprint 25.14) | direct `find`/`grep` count | Medium | **Resolved this Sprint**: corrected count/list/key-count in `07-localization.md` §8 and its "Update" note; historical Sprint 25.14 prose left otherwise intact. |
| D3 | Epic 28 package (`02_INITIAL_USER_PROMPT.txt`): real Resend send observed landing in Junk | `14-transactional-email-runbook.md` §6/§19 (updated same day, before 28.1): real Resend inbox E2E "remains pending" | doc comparison | High (blocks 28.5 assumptions) | **Not resolved — flagged, not silently picked.** Requires owner clarification before 28.5 treats any Junk-placement claim as established evidence. |
| D4 | `docs/deployment/03-observability.md` (Sprint 18.5) is silent on all Epic-26 email logging | `06-transactional-email.md` §14 documents a detailed email observable-state model | doc comparison | Informational | Not a contradiction, a gap. Deliberately left for Sprint 28.7 ("Observability Operationalization") rather than edited here, to avoid prejudging that Sprint's design. |

### Decisions Required for 28.2

1. **Culture transport contract**: `User.Language` (Domain) is the only persisted, provider-neutral
   source of a user's language, reachable outside HTTP context via `IUserRepository`. 28.2 must
   decide the exact mechanism to carry it into `IIdentityEmailComposer` (e.g., extend the composer
   interface with a culture parameter that handlers populate from `user.Language`) without adding
   `IStringLocalizer`/Web dependencies to Application or Infrastructure, and without creating a
   second resource catalog system. Two candidate directions already named by
   `06-transactional-email.md` §13.5 and `docs/brand/02-writing-voice-localization.md` — 28.2 picks
   and justifies one.
2. **Resource catalog ownership for email strings**: given the existing 19-catalog Web-owned
   convention (`07-localization.md` §8) is explicitly out of reach for Infrastructure (architectural
   boundary), 28.2 must decide where email copy strings live so both the composer (Infrastructure)
   and the visible product (Web) stay consistent without duplicating a translation source.
3. **`EmailMessage` contract extension**: whether preheader/Reply-To/From-name-per-message are needed
   for 28.4's experience work, and if so, the compatible way to add them (default-valued optional
   parameters, matching the precedent set by `PlainTextBody` in Epic 26 Sprint 26.6).
4. **Deliverability evidence discipline (D3)**: 28.5 needs an explicit answer from the repository
   owner on the Junk-placement claim's provenance before building remediation hypotheses on it.
5. **Font/brand policy for email clients**: confirm Nunito/Coiny fallback strategy for HTML email
   (webfonts unreliable) without importing the site's CSS wholesale — input for 28.4's template work,
   decided in principle by 28.2/28.3.

---

## Sprint 28.2 — Transactional Email Experience & Localization Contract

**Base local:** `sprint/28.1-email-baseline` (not `hmg` — per the stacked-branching rule).
**Branch:** `sprint/28.2-email-experience-localization-contract`.
**Gate:** Gate A (Experience & Localization Architecture) — satisfied at the level this Sprint owns.

### Decisions taken (answering the 9 questions from the Sprint prompt)

1. **Culture source:** `User.Language` (Domain, already persisted) — the only approved source.
2. **Transport:** a new required 4th parameter, `UserLanguage language`, on
   `IIdentityEmailComposer.ComposeEmailConfirmation`/`ComposePasswordReset`. All 3 real call sites
   (`EmailConfirmationIssuer.Issue`, `ResendEmailConfirmationCommandHandler`,
   `RequestPasswordResetCommandHandler`) now pass `user.Language` explicitly.
3. **Owning layer:** the decision itself is Application-contract-shaped (the interface lives in
   Application); the resolution mechanism is entirely Infrastructure-internal.
4. **Reusing the official Localization System:** not reused directly — Infrastructure cannot depend on
   Web. A new, narrow, Infrastructure-owned `.resx` catalog was created instead (see below), formalized
   in **ADR-006**.
5. **Where transactional strings live:** `src/BeeDay.Infrastructure/Identity/EmailResources.resx` /
   `.en-US.resx` / `.pt-BR.resx` — 9 keys (`Greeting`, `Confirmation{Title,Introduction,Footer,ActionLabel}`,
   `Reset{Title,Introduction,Footer,ActionLabel}`).
6. **HTML/plain text:** unchanged pattern — the same `BuildHtmlTemplate`/`BuildPlainTextTemplate` pair
   in `IdentityEmailComposer`, now both taking the resolved `CultureInfo` and reading from the new
   catalog instead of `const string` literals.
7. **Preheader:** stays an internal composer detail, not promoted to `EmailMessage`/the public
   contract, this Sprint — no confirmed need yet; revisit in 28.3/28.4 if visual work requires it.
8. **Typography/fallback principle for email:** not implemented visually this Sprint (explicitly out of
   scope — "no redesign completo"); the existing HTML template's `font-family:Arial,sans-serif` inline
   style already follows the "safe system fallback, no custom webfont" principle Nunito/Coiny cannot
   satisfy in email clients — 28.3/28.4 own any visual change.
9. **Reply-To:** evaluated, left unchanged — a deliverability/alignment concern (28.5/28.6), not a
   localization one.

### Implementation

- `src/BeeDay.Application/Common/Identity/IIdentityEmailComposer.cs` — added `UserLanguage language`
  parameter to both methods, documented the contract (recipient's persisted language only, never
  ambient/request state).
- `src/BeeDay.Application/Common/Identity/IEmailConfirmationIssuer.cs` — `Issue` now passes
  `user.Language` to the composer.
- `src/BeeDay.Application/Features/Identity/Handlers/IdentityHandlers.cs` — both direct composer call
  sites (`ResendEmailConfirmationCommandHandler`, `RequestPasswordResetCommandHandler`) pass
  `user.Language`.
- `src/BeeDay.Infrastructure/Identity/EmailResources.resx`/`.en-US.resx`/`.pt-BR.resx` (new) — the
  9-key catalog.
- `src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs` — rewritten to resolve strings via
  `ResourceManager.GetString(name, explicitCultureInfo)`, with a private `UserLanguage → CultureInfo`
  mapping (deliberately not reusing `BeeDay.Web.Localization.BeeDayCultures`, which Infrastructure
  cannot reference). HTML `<html lang="...">` now reflects the resolved culture.
- `docs/adr/ADR-006-transactional-email-localization-boundary.md` (new) — formal record of this
  decision, alternatives rejected, and explicit restrictions on future work at this boundary.
- `docs/adr/README.md` — indexed ADR-006.
- `docs/infrastructure/06-transactional-email.md` §13.5 — dated Update note recording that Sprint
  26.6's deferred option (a) was adopted, pointing to ADR-006; historical Sprint 26.6 prose left
  intact.

### Architectural impact

No dependency-direction change. Domain/Application/Infrastructure/Web boundaries unchanged;
`PersistenceContractBoundaryTests.ApplicationAssembly_DoesNotReferenceInfrastructure` still passes
unmodified. The only new "dependency" is Infrastructure's own `EmailResources.resx`
(`System.Resources.ResourceManager`, part of the BCL) — no new package reference, no
`Microsoft.Extensions.Localization`/`IStringLocalizer` usage anywhere in Infrastructure.

### Compatibility

`IIdentityEmailComposer`'s two methods gained a required 4th parameter — a breaking signature change,
but to an internal-only contract with exactly 3 real call sites, all updated in this same commit; no
external consumer exists. No overload/default was added (see ADR-006 "Compatibilidade").

### Tests added or updated

- `tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs` — 6 existing tests updated to the
  new signature (all still assert English content, `UserLanguage.English`); 4 new tests:
  `EmailComposer_ComposesConfirmationInTheRequestedLanguage`/`...PasswordResetInTheRequestedLanguage`
  (theory, en-US + pt-BR, asserts subject/greeting/`<html lang>`), and
  `EmailComposer_NeverThrowsForAnyApprovedLanguage` (both languages resolve every key without
  exception).
- `tests/BeeDay.Application.Tests/IdentityHandlersTests.cs` — `FakeEmailComposer` now records the
  language it received; 2 new tests
  (`ResendConfirmation_PassesTheUsersOwnLanguageToTheComposer`,
  `RequestPasswordReset_PassesTheUsersOwnLanguageToTheComposer`) prove the handlers forward the real
  user's `Language`, not a default, end-to-end through the MediatR handler.
- No test sends real email (unchanged — all existing fakes/stubs preserved).

### Documentation updated

`docs/adr/ADR-006-transactional-email-localization-boundary.md` (new), `docs/adr/README.md`,
`docs/infrastructure/06-transactional-email.md` §13.5, this file.

### Validation Results

```
dotnet format BeeDay.slnx --verify-no-changes   → clean after one auto-fix pass (CRLF line endings on
                                                    the rewritten IdentityEmailComposer.cs — the Write
                                                    tool produced LF; `dotnet format` corrected it,
                                                    re-verified clean)
dotnet build BeeDay.slnx                         → 0 errors, 0 warnings
dotnet test BeeDay.slnx                          → 1366/1366 passed (93 Domain + 85 Application +
                                                    182 Infrastructure + 165 E2E + 841 Web)
git status                                       → clean after commit (see Git section below)
```

### Security / Production

No secrets touched. No new external dependency. Production (`prd`) untouched — no file under
`appsettings.Production.json`'s scope was changed. pt-BR translations contain no PII, tokens, or
secrets — static UI copy only.

### Runtime validation

Not applicable — this Sprint is architecture/composer-level, no deployment-dependent behavior.
`POST-MERGE PENDING`: none introduced by this Sprint specifically (the pre-existing Homologation
Resend-activation POST-MERGE-PENDING item from Sprint 28.1 is unaffected by this Sprint's changes).

### Risks / Known Limitations

- pt-BR copy is a first-pass functional translation, not a brand-voice-reviewed final copy — Sprint
  28.4 owns revising both languages together.
- The `UserLanguage ↔ CultureInfo` mapping now exists in two places (Web's `BeeDayCultures`,
  Infrastructure's private switch in `IdentityEmailComposer`) — an unavoidable, explicitly-accepted
  duplication at the layer boundary (ADR-006). Any future third language must update both.
- Preheader remains unaddressed — deferred, not forgotten (tracked for 28.3/28.4).

---

## Sprint 28.3 — Transactional Email Composition Foundation

**Base local:** `sprint/28.2-email-experience-localization-contract`.
**Branch:** `sprint/28.3-email-composition-foundation`.
**Gate:** foundation for Gate B (final Gate B check happens at the close of 28.4).

### Audit (re-confirmed before changing anything)

- `EmailMessage` shape unchanged since 28.1 (`Recipient`, `Subject`, `HtmlBody`, `PlainTextBody?`).
- Consumers unchanged: `EmailConfirmationIssuer.Issue`, `ResendEmailConfirmationCommandHandler`,
  `RequestPasswordResetCommandHandler` (same 3 call sites as 28.2).
- Escaping: `WebUtility.HtmlEncode` on every interpolated HTML value, confirmed still applied to all 6
  content fields after the 28.2 refactor.
- URL generation: unchanged, one `BuildUrl(path, rawToken)` shared by both flows.
- **Duplication found:** after ADR-006 added the 4th parameter, both `Compose*` public methods had
  grown to ~8 nearly-identical lines differing only in which resource keys and which path they used —
  real, provable duplication, not hypothetical. This Sprint's one substantive change addresses exactly
  this.
- Tests: 27 tests existed on `IdentityInfrastructureTests.cs` after 28.2; audited for HTML-safety gaps
  — found no test for `&`, `"`, or `'` in display names (only `<`/`>` via `Tiago <Admin>`), no long-URL
  test, no explicit HTML/plain-text parity test, no determinism test.
- Provider boundary / HMG subject prefix boundary: confirmed unchanged — `HmgRecipientGuardedEmailSender`
  still applies `SubjectPrefix` centrally, once, never touched by the composer.

### Implementation

- `IdentityEmailComposer` — both `Compose*` public methods now delegate to one private
  `Compose(recipient, displayName, rawToken, language, path, EmailContentKeys keys)`; the 6 resolved
  strings are carried as one private `EmailContent` record instead of positional parameters.
  `BuildHtmlTemplate`/`BuildPlainTextTemplate` now take `EmailContent` instead of 6-7 loose strings.
  No behavior change — same HTML/plain text output for the same inputs (proven by the new determinism
  test below).
- No new public contract, no new template engine, no preheader (still deferred — no confirmed need
  found this Sprint either), no visual/copy change.

### Tests added

`tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs`:

- `EmailComposer_EncodesEveryHtmlSignificantCharacterInDisplayName` (`<script>`, `&`, `"`).
- `EmailComposer_NeverEmitsUnescapedScriptTagsFromDisplayName`.
- `EmailComposer_PreservesApostrophesAcrossHtmlEncodingAndPlainText` (proves, doesn't assume, that
  `WebUtility.HtmlEncode` also encodes `'`).
- `EmailComposer_HandlesLongTokensWithoutTruncatingOrBreakingTheUrl` (512-char token).
- `EmailComposer_HtmlAndPlainTextCarryTheSameEssentialFacts` (both flows — same subject/URL present in
  both bodies, without requiring byte-identical output).
- `EmailComposer_ProducesDeterministicOutputForTheSameInputs`.

### Documentation updated

`docs/infrastructure/06-transactional-email.md` §13.1 — dated Update note describing the
`Compose`/`EmailContentKeys`/`EmailContent` refactor as the extension seam for future flows;
historical Sprint 26.6 prose left intact. No new "Email Design System" document created.

### Validation Results

```
dotnet format BeeDay.slnx --verify-no-changes   → clean (one auto-fix pass needed first — the Write
                                                    tool produced LF on the rewritten
                                                    IdentityEmailComposer.cs again; `dotnet format`
                                                    corrected it, re-verified clean)
dotnet build BeeDay.slnx                         → 0 errors, 0 warnings
dotnet test BeeDay.slnx                          → 1375/1375 passed (93 Domain + 85 Application +
                                                    191 Infrastructure + 841 Web + 165 E2E)
git status                                       → clean after commit
```

### Security / Production

No secrets touched. Production untouched. New HTML-safety tests strengthen, not weaken, injection
coverage.

### Runtime validation

Not applicable — internal refactor only, no deployment-dependent behavior. No new
POST-MERGE-PENDING items.

### Risks / Known Limitations

None new. Same residual items as Sprint 28.2 (pt-BR copy not brand-voice-reviewed yet; preheader
still deferred) carried forward unchanged to Sprint 28.4.

---

## Sprint 28.4 — Identity Transactional Email Experience

**Base local:** `sprint/28.3-email-composition-foundation`.
**Branch:** `sprint/28.4-identity-email-experience`.
**Gate:** Gate B (Transactional Email Experience) — satisfied at the implementation/test level; real
HMG/inbox evidence is `POST-MERGE PENDING` per the shared rules (unchanged from prior Sprints).

### Before implementing (re-read per the Sprint's own instruction)

`docs/design-system/01-foundations.md` §2 (color tokens) and §3 (typography),
`docs/brand/02-writing-voice-localization.md` (voice/tone/glossary, "E-mail transacional" section),
`docs/web/07-localization.md`, `docs/ux/` (accessibility-adjacent), and
`docs/infrastructure/06-transactional-email.md` (own prior sections). Brand values reconfirmed
directly in `src/BeeDay.Web/wwwroot/css/variables.css` — `#5247F9` primary,
`#FFFFFF`/`#F7F7F7` surface, `#2F2737`/`#514858`/`#817789` text, `#E5E5E5` border — no divergence
found between docs and code for these values.

### What changed

**`IdentityEmailComposer`** — full visual/content rewrite of `BuildHtmlTemplate` (table-based,
`role="presentation"`, inline styles, light Surface/Content tokens replacing the old dark off-token
palette §13.2 had flagged) and `BuildPlainTextTemplate` (now opens with the `beeday` wordmark and
includes the fallback-link block). `EmailContentKeys`/`EmailContent` (28.3) extended with
`Preheader`/`FallbackLinkIntro` fields — same extension seam, no new abstraction.

**`EmailResources.*.resx`** — grew from 9 to 12 keys: added `FallbackLinkIntro` (shared),
`ConfirmationPreheader`, `ResetPreheader`. Rewrote `ConfirmationFooter`/`ResetFooter` in both cultures
to a consistent, Security/Identity-toned pattern ("valid for X, single use, ignore if not requested")
— the Confirmation flow gained the ignore-instruction it was missing; Reset's equivalent sentence was
reworded for parity, not changed in meaning. No expiry duration changed — both still match
`IdentityTokenLifetimes` exactly (24h/1h), unchanged code.

**Content per flow** (subject/preheader/heading/greeting/body/CTA/fallback-link/footer/plain-text) —
all reviewed against the checklist; no item invented (validity durations verified against real code,
not assumed).

### Visual identity applied

- `beeday` wordmark, lowercase, Brand Primary `#5247F9`, Coiny-first font stack — scoped to exactly
  one `<span>`, never body/CTA text (Nunito-first stack there).
- Light Surface (`#FFFFFF`)/Content (`#2F2737`/`#514858`/`#817789`)/Border (`#E5E5E5`) tokens replace
  the prior dark palette.
- Table-based (`role="presentation"`) layout for Outlook/Gmail/iCloud compatibility; every style
  inlined; no remote images anywhere (nothing to block).
- Hidden preheader (with zero-width-space/combining-grapheme-joiner padding) — culture-aware, distinct
  from the subject, one per flow.
- Callback URL shown twice: CTA `href` and a visible, clickable fallback link with its own intro
  sentence — for clients/policies that strip buttons.

### Tests added

`tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs` (11 new tests, on top of 28.1-28.3's
31): brand wordmark lowercase in both bodies, culture-aware preheader distinct from subject (both
languages), callback URL present twice in HTML (CTA + fallback), Nunito/Coiny font-stack scoping
(Coiny appears exactly once), table-based layout + no `<img>` tags, ignore-instruction present in both
languages for both flows. Existing footer-text assertions updated for the new "valid for X" wording.

### Documentation updated

`docs/infrastructure/06-transactional-email.md` — new §13.6 ("Transactional Email as an Experience
System surface") declaring the relationship to Brand/Design System/Writing/Localization explicitly,
per this Sprint's own instruction, without creating a parallel "Email Design System" document.

### Validation Results

```
dotnet format BeeDay.slnx --verify-no-changes   → clean
dotnet build BeeDay.slnx                         → 0 errors, 0 warnings
dotnet test BeeDay.slnx                          → 1386/1386 passed (93 Domain + 85 Application +
                                                    202 Infrastructure + 841 Web + 165 E2E)
git status                                       → clean after commit
```

### Security / Production

No secrets touched. Production untouched. Encoding/escaping unchanged from 28.3 (same
`WebUtility.HtmlEncode` calls, now applied to two additional fields — preheader and fallback-link
intro — both proven encoded by the existing safety tests' pattern).

### Runtime validation

Not applicable — no deployment-dependent behavior. `POST-MERGE PENDING`: real-client rendering
(Gmail/Outlook/iCloud actual inbox appearance) is Sprint 28.9's explicit scope, not claimed here —
this Sprint's evidence is code-level (deterministic HTML/plain-text assertions), not a rendered
screenshot in a real client.

### Risks / Known Limitations

- Real email-client rendering (Outlook desktop's Word engine especially) has not been visually
  verified in an actual client — Sprint 28.9 owns that QA pass explicitly.
- pt-BR copy is still a functional translation reviewed against the Voice/Tone doc's rules by this
  Sprint (not by a native-speaker product review outside this session) — flagged, not claimed as a
  final linguistic sign-off.
- No preheader/fallback-link A/B or deliverability-impact evidence exists yet — that belongs to
  Sprint 28.5/28.6.

---

## Sprint 28.5 — Deliverability & Inbox Placement Diagnostic Audit

**Base local:** `sprint/28.4-identity-email-experience`.
**Branch:** `sprint/28.5-deliverability-audit`.
**Gate:** Gate C — partial (baseline complete; remediation is Sprint 28.6's scope). **No DNS changed
by this Sprint.**

### Repository audit

| Concern | Finding | Classification |
|---|---|---|
| `From`/`FromName` | `ResendEmailSender.cs:34` builds `"{FromName} <{FromAddress}>"`. C# class default (`ResendOptions.cs:9`) is `"BeeDay"` (wrong brand casing), **but every committed config that actually runs overrides it to `"beeday"`** — confirmed by reading `src/BeeDay.Web/appsettings.json:34` and `appsettings.Production.json:48` directly (both literal `"FromName": "beeday"`); `appsettings.Homologation.json` has no override, so it inherits the base file's `"beeday"`. Effective FromName in every real environment is already lowercase-correct. | `confirmed-good` (effective config); the unreachable C# default is `suspicious-but-unproven` severity-`informational` (dead code path today, only a defense-in-depth cleanup candidate) |
| `Reply-To` | No `reply_to` field is ever sent to Resend (`ResendEmailSender.cs:32-39` sends only `from`/`to`/`subject`/`html`/`text`) — Resend's own default behavior when `reply_to` is omitted is to use the `From` address, so no orphaned reply-to exists. Whether `FromAddress` (secret) is itself reply-capable or a no-reply-style address is unknown to this audit. | `confirmed-good` (no gap in code); `external-verification-needed` (whether the real address accepts replies) |
| Sending domain/subdomain | `FromAddress` is a GitHub Environment secret (`BEEDAY_RESEND_FROM_ADDRESS`) — never committed, never logged, not inspected or requested by this Sprint (`docs/deployment/02-runtime-configuration.md` §6 confirms it's always empty in every committed `appsettings*.json`). **This is the single biggest blocker to a complete deliverability audit** — SPF/DKIM/DMARC alignment cannot be checked against the correct domain without knowing it. | `external-verification-needed` — repository owner must supply the domain (not the secret value) for 28.6, or run the DNS commands below directly |
| `PublicBaseUrl`/links | `appsettings.Homologation.json` sets both `AllowedHosts` and `IdentityEmail:PublicBaseUrl` to `h-beeday.com.br` / `https://h-beeday.com.br`. **Direct DNS verification this session found `h-beeday.com.br` does not exist in public DNS at all** (`nslookup h-beeday.com.br` → `Non-existent domain`, confirmed twice, independent of any local override). It resolves only on this development machine, to a private RFC1918 address `192.168.15.9`, via a literal entry in `C:\Windows\System32\drivers\etc\hosts` (`Get-Content ...\hosts \| Select-String beeday` confirmed the line directly). **This means: every confirmation/reset link Resend would deliver to a real HMG recipient points at a hostname unreachable from outside this machine's local network/VPN**, unless the recipient's own device has an equivalent hosts entry or is on the same private network as `192.168.15.9`. | `confirmed-defect` **if** allowlisted HMG recipients are expected to click links from arbitrary networks; `suspicious-but-unproven` **if** HMG is deliberately an internal/VPN-only environment and every `AllowedRecipients` entry already has the required network access (plausible given HMG's small, curated allowlist — but not confirmed either way this session) |
| HTML/plain text | Both present on every message (`EmailMessage.HtmlBody`/`PlainTextBody`), confirmed by Sprints 28.3/28.4's own test suite. | `confirmed-good` |
| Tracking settings | No tracking-related field is set anywhere in the Resend request payload — click/open tracking, if any, is entirely controlled by Resend's own account/domain-level dashboard settings, invisible to this repository. | `external-verification-needed` |
| Provider `MessageId`/event semantics | `ResendEmailSender.cs:71-74` logs the provider's own response `id` on acceptance — correctly distinguished from delivery (§14.1 of the infrastructure doc). No provider webhook/event ingestion exists in this repository (no code consumes Resend's delivery/bounce/complaint webhooks). | `confirmed-good` (accept-vs-deliver distinction already correct in code); `not-applicable` today for event ingestion (not built, not claimed) |
| Headers configurable by the app | Only `Authorization`, `User-Agent` (`"BeeDay/1.0"`, not brand-visible), and `Idempotency-Key` (a fresh GUID per send — correct, prevents accidental duplicate sends on retry) are set. No `List-Unsubscribe` or similar bulk-mail signal headers — not expected to matter for low-volume transactional mail, but noted. | `confirmed-good` (nothing incorrect); `informational` |
| Documentation/runbooks | `14-transactional-email-runbook.md` already distinguishes "provider accepted" from "delivered" from "inbox placement" (§15, §6) and explicitly states real Resend inbox E2E has not yet occurred (carried over from Sprint 28.1's D3 finding — still unresolved this Sprint, still flagged, not assumed). | `confirmed-good` (doc discipline); D3 itself remains `external-verification-needed` |

### External audit (read-only, no authentication, no secrets touched)

Performed via this session's own outbound DNS resolution (`nslookup` and PowerShell `Resolve-DnsName`,
cross-checked against each other and against known-good public domains `google.com`/`github.com` to
confirm the resolver path reaches real public DNS and isn't purely sandboxed) — **2026-08-17**:

- `beeday.com.br` (the apex the HMG subdomain belongs to): registered (has `SOA`/`NS` — primary
  `e.sec.dns.br`, `registro.br`), but **no SPF, no DMARC, no MX record at all** — a bare registration,
  not configured for mail in either direction.
- `beeday.com` (a *different* domain — no `.br`): apex SPF is `v=spf1 -all` (hard-fail — "nothing is
  authorized to send as this domain") and MX is a null MX (`0 .` — "accepts no mail"). Multiple
  arbitrary subdomains (`hmg.beeday.com`, `app.beeday.com`, `www.beeday.com`, and even an invented
  `h.beeday.com`) all resolved to the identical IP pair (`76.223.54.146`, `13.248.169.48`), and
  differently-purposed TXT lookups (`_dmarc.beeday.com`, `resend._domainkey.beeday.com`) both returned
  the identical `v=spf1 -all` string — strong evidence of a **wildcard DNS zone** (likely a
  parking/placeholder configuration), meaning none of these specific-hostname answers under
  `beeday.com` can be trusted as intentionally-configured, distinct records. Only the apex SPF/MX are
  credible as deliberate configuration.
- **This audit could not determine which of these domains (if either) is the real Resend `FromAddress`
  domain** — that value is a secret, correctly never exposed to this session. If Resend's `FromAddress`
  uses `beeday.com` or any subdomain of it, the apex `v=spf1 -all` would cause **every message to fail
  SPF** for that domain (a hard, confirmed-cause explanation *if* that's the sending domain — but this
  audit does not know that it is, so this is listed as a ranked hypothesis, not a proven cause).

**Exact commands for the repository owner to re-run once the real `FromAddress` domain is known** (safe,
public, read-only, no credentials needed):

```text
nslookup -type=TXT <domain>                    # SPF
nslookup -type=TXT _dmarc.<domain>              # DMARC
nslookup -type=TXT resend._domainkey.<domain>   # Resend's documented DKIM selector convention
nslookup -type=MX <domain>
```

### Baseline classification of delivery-state evidence (per the Sprint's required separation)

| State | Evidence available to this session |
|---|---|
| Provider request | Code-confirmed (`ResendEmailSender.cs:43`, "Resend request attempted" log line) |
| Provider accepted | Code-confirmed logging exists (line 72-74); no real send has occurred with the current code (Homologation not yet redeployed post-activation, per Sprint 28.1's baseline) |
| Provider delivered/event | Not implemented (no webhook ingestion) — `not-applicable` today |
| Mailbox receipt | No evidence this session — requires a real send |
| Spam/Junk placement | The EPIC package's own claim of a prior Junk-placement observation remains **unverified** (Sprint 28.1 finding D3, still open) — not treated as established fact |

`accepted != delivered != inbox placement` is preserved throughout this table — no state is inferred
from another.

### Hypotheses ranked by evidence (for Sprint 28.6)

1. **(Strong evidence, but domain-identity-dependent)** If Resend's `FromAddress` uses `beeday.com` or
   a subdomain, the domain's own `v=spf1 -all` guarantees SPF failure for every send — this alone would
   fully explain any delivery problem, without needing a Junk-placement anecdote to justify
   investigating it. Confirming the domain is the single highest-leverage next step.
2. **(Confirmed, independent of the domain question)** `h-beeday.com.br` is not publicly resolvable —
   any real, non-VPN recipient cannot load the confirmation/reset link at all. This is a link-usability
   defect regardless of whether it also affects deliverability scoring.
3. **(Speculative, no evidence either way)** Content/reputation signals (new domain age, low sending
   volume, generic transactional content) — plausible contributors to any provider's spam
   classification, but nothing in this repository can confirm or rule this out; would need Resend
   Insights (credential-gated).

### Remediation candidates for Sprint 28.6, priority-ordered (not implemented this Sprint)

1. **External, requires repository owner decision — not auto-executable:** confirm the real
   `FromAddress` domain and its SPF/DKIM/DMARC state using the commands above; if it resolves to
   `beeday.com`'s reject-all SPF, that is very likely the root cause and needs a DNS change (out of
   this Epic's authority — `04_EXECUTION_CONSIDERATIONS.md` §E: "DNS é operação externa e não está
   autorizada automaticamente").
2. **External, requires repository owner decision:** confirm whether `h-beeday.com.br`'s public
   unresolvability is intentional (VPN/LAN-only HMG, all `AllowedRecipients` have that access) or a
   real gap that needs a public DNS record.
3. **Code, low-risk, safe for 28.6 to implement directly:** correct `ResendOptions.FromName`'s C#
   class-level default from `"BeeDay"` to `"beeday"` for consistency (currently unreachable dead code
   — every committed environment already overrides it — but still worth correcting so the default
   itself can never regress brand casing if a future environment omits the override).
4. **Deferred pending #1's answer:** whether to explicitly set `reply_to` in the Resend payload,
   depending on whether the real `FromAddress` is reply-capable.

### Tests

None added — this Sprint audited configuration and external DNS state, not application behavior. No
test sends real email (unchanged from all prior Sprints).

### Documentation updated

This section (baseline report, owner-existing document — no new documentation tree created, per the
Sprint's own instruction).

### Validation Results

```
dotnet format BeeDay.slnx --verify-no-changes   → clean (no source changes this Sprint)
dotnet build BeeDay.slnx                         → 0 errors, 0 warnings
dotnet test BeeDay.slnx                          → 1386/1386 passed, unchanged from Sprint 28.4
                                                    (93+85+202+841+165 across 5 projects)
git status                                       → clean after commit
```

### Security / Production

No secrets read, requested, or exposed — `FromAddress`'s actual value was never inspected, only its
absence from committed config confirmed. No DNS record was changed. Production untouched — this audit
did not query anything specific to `prd` (which has no runtime, `CLAUDE.md` §8.2).

### Runtime validation

Not applicable — audit only. The `h-beeday.com.br` DNS finding is itself a piece of environment
evidence (not code-dependent), gathered directly this session, not `POST-MERGE PENDING` — it is true
regardless of which EPIC 28 commit is deployed.

### Risks / Known Limitations

- The real `FromAddress` domain remains unknown to this session — the single largest gap in this
  audit, entirely by design (secret protection), not an oversight.
- DNS wildcard behavior on `beeday.com` means several of this session's TXT lookups under that domain
  are not trustworthy as distinct, intentional records — flagged explicitly rather than reported as
  fact.
- Resend Insights/dashboard delivery events were not accessible this session (credential-gated) — no
  attempt was made to obtain or bypass that.

---

## Sprint 28.6 — Evidence-Based Deliverability Remediation

**Base local:** `sprint/28.5-deliverability-audit`.
**Branch:** `sprint/28.6-deliverability-remediation`.
**Gate:** Gate C — satisfied at the configuration/code-evidence level. Real-HMG retest of the new
template remains `POST-MERGE PENDING` (unchanged — depends on deployment, not this Sprint).

### Rule followed: every change traces to a specific 28.5 finding — no shotgun debugging

Of the 4 priority-ordered candidates 28.5 produced, exactly **one** was repo-side and evidenced enough
to act on without inventing a change. The other three are marked `EXTERNAL ACTION REQUIRED` below and
were **not executed**.

### Applied: repo-side remediation #3 (FromName default casing)

- **Evidence (28.5):** `ResendOptions.FromName`'s C# class-level default was `"BeeDay"` — wrong brand
  casing for a visible, marketing-adjacent surface (every recipient's inbox "From" name) — while every
  real committed environment already overrides it correctly to `"beeday"` via `appsettings.json`
  (base) and `appsettings.Production.json` (explicit), with Homologation inheriting the base value.
  The default itself was unreachable in practice, but a latent regression risk for any future
  environment that omitted the override.
- **Change:** `src/BeeDay.Infrastructure/Configuration/ResendOptions.cs` — `FromName` default
  `"BeeDay"` → `"beeday"`. One line.
- **Test:** `ResendOptions_DefaultFromNameMatchesTheBrandContract` (new) — locks the correct default in
  so it can't silently regress again.
- **Before/after:** before, a brand-new environment that forgot to set `BeeDay:Email:Resend:FromName`
  would have sent as `"BeeDay <address>"`; after, it sends as `"beeday <address>"`, consistent with
  every already-deployed environment's actual behavior. **No claim of Inbox Placement improvement is
  made** — this is a brand-consistency correction to a currently-unreachable default, not a
  deliverability fix; it was already the effective behavior in HMG/Production before this change.

### Not applied — `EXTERNAL ACTION REQUIRED`, no automatic action taken

**1. SPF/DKIM/DMARC for the real `FromAddress` domain**

- Evidence: 28.5 found `beeday.com` (a candidate, unconfirmed domain) has a hard-fail `v=spf1 -all`
  and a null MX; the actual `FromAddress` domain remains a secret, never inspected this session.
- Exact proposed change: none proposable yet without knowing the real domain. If it is confirmed to be
  `beeday.com` (or a subdomain), the domain owner would need to add Resend's specific verification
  records (obtained from the Resend dashboard's domain-verification page — an SPF `include:` addition
  alongside or replacing the current `-all`, plus the DKIM selector TXT record Resend issues per
  domain) rather than a value this session can safely guess.
- Risk/rollback: DNS TXT changes are globally propagating and cached per-record TTL; changing a
  domain's SPF affects every service that currently relies on its `-all` reject-all policy (unknown
  whether anything does). Rollback is reverting the TXT record to its prior value.
- **Status: EXTERNAL ACTION REQUIRED. Not executed.** Owner must first confirm the real sending domain.

**2. `h-beeday.com.br` public DNS resolvability**

- Evidence: 28.5 confirmed this domain (HMG's committed `PublicBaseUrl`/`AllowedHosts`) does not exist
  in public DNS; it only resolves locally via a hosts-file override to a private address.
- Exact proposed change: two candidate directions, not a single obvious fix — (a) publish a real public
  DNS record for `h-beeday.com.br` pointing at a publicly-reachable endpoint for SERV3WEB (a security
  posture decision, not just a DNS action — it changes whether HMG becomes internet-reachable), or
  (b) formally document HMG as VPN/LAN-only and confirm every `HmgRecipientGuardOptions:AllowedRecipients`
  entry has that network access, closing the question without a DNS change at all.
- Risk/rollback: (a) has real security implications (exposing an internal server publicly) and must not
  be decided by inference; (b) is zero-risk (documentation only) but requires the owner's confirmation
  of who is actually on the allowlist and their network access.
- **Status: EXTERNAL ACTION REQUIRED / decision required. Not executed.**

**3. Reply-To semantics**

- Evidence: 28.5 found no gap in code (Resend already defaults `reply_to` to `From` when omitted); the
  question of whether the real `FromAddress` is reply-capable depends on the same unknown domain as
  item 1.
- **Status: no repo-side defect to remediate — deferred pending item 1's answer, not a `TODO` left
  behind.** `no repo-side remediation justified` for this item specifically.

**4. Tracking configuration**

- Evidence: no tracking-related field is set anywhere in the app's Resend request payload — tracking
  behavior, if any, is entirely a Resend account/domain dashboard setting, invisible to and
  uncontrolled by this repository.
- **Status: not applicable to repo-side remediation** — there is no code to change; this is
  `external-verification-needed` only (Resend dashboard, credential-gated).

### Conclusion

`no repo-side remediation justified` for 3 of 4 candidates (items 1/3/4 above are either external or
already correct); one small, evidenced, zero-risk correction applied (FromName default). **No claim
of improved Inbox Placement is made anywhere in this Sprint** — that requires a real retest, which is
`POST-MERGE PENDING`.

### Tests

`ResendOptions_DefaultFromNameMatchesTheBrandContract` (new, `IdentityInfrastructureTests.cs`). No
other test changed. No test sends real email.

### Documentation updated

This section. `ResendOptions.cs`'s own inline shape didn't carry a comment needing an update (plain
POCO, no prior comment asserting the old default was intentional).

### Validation Results

```
dotnet format BeeDay.slnx --verify-no-changes   → clean
dotnet build BeeDay.slnx                         → 0 errors, 0 warnings
dotnet test BeeDay.slnx                          → 1387/1387 passed (93 Domain + 85 Application +
                                                    203 Infrastructure + 841 Web + 165 E2E)
git status                                       → clean after commit
```

### Security / Production

No secrets touched or exposed. No DNS changed. Production untouched — `appsettings.Production.json`
already had the explicit `"beeday"` override before this Sprint; this change only affects the
unreachable class-level default, not Production's actual committed behavior.

### Runtime validation

Not applicable — the applied change affects only an already-unreachable code path in every real
environment. `POST-MERGE PENDING`: none newly introduced by this Sprint.

### Risks / Known Limitations

- Items 1 and 2 above remain open, blocking a complete Gate C closure at the "real deliverability
  proven" level (as opposed to "configuration/code evidence" level, which this Sprint does satisfy) —
  both require the repository owner's input/decision before any further code or DNS action.
- No experiment/retest has been run — this Sprint intentionally did not claim one.
