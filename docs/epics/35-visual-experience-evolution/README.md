# EPIC 35 — beeday Visual Experience Evolution

**Issue:** [#403](https://github.com/DS-Asyst/BeeDay/issues/403)
**Status:** OPEN — Sprint 35.1 in progress, awaiting owner visual review.

## Objective

Establish the next owner-directed visual evolution of the beeday Experience System using the
independently runnable Frontend Lab (`DS-Asyst/beeday-frontend-lab`) as the primary design and
validation environment, preserving the EPIC 33 approved visual baseline while evolving shared page
composition patterns before any deliberate promotion into the production `DS-Asyst/BeeDay`
repository.

This EPIC follows the closure of EPIC 33 ([#361](https://github.com/DS-Asyst/BeeDay/issues/361)).
It does not reopen EPIC 33, does not modify its history, and does not touch production BeeDay
implementation code.

## Baseline this EPIC starts from

| Coordinate | Value |
|---|---|
| Approved Lab `hmg` (EPIC 33 final) | `357dc9db59a665bc324d281ce374bb63e058779f` |
| Lab `prd` (promoted) | `a0f380e0542392874df6b780062a685a3c314800` |
| Baseline tag (current) | `v1.1.0-lab-baseline` |
| Baseline tag (historical, preserved) | `v1.0.0-lab-baseline` → `923bee3` |
| EPIC 33 Ledger | 115/115 `FE33-*` terminal (111 VERIFIED + 4 EXCLUDED) |

See `docs/epics/33-ds-assyst-frontend-lab/` for the full EPIC 33 record (unchanged by this EPIC).

## Core principles

- Frontend Lab first — implementation happens in `DS-Asyst/beeday-frontend-lab` before any
  production consideration.
- Owner visual review is authoritative — passing tests is never a substitute for explicit approval.
- `DS-Asyst/BeeDay` remains the runtime/product source of truth.
- No second backend — the Lab stays database-free and presentation-only (ADR-008).
- No blind Lab → BeeDay synchronization.
- Existing Design System components are extended, not duplicated.
- Public and authenticated experiences share a coherent visual language without becoming
  structurally identical.
- Production promotion is explicitly outside Sprint 35.1.

## Boundaries

**IN:** shared visual composition patterns; Design System evolution where justified; public
informational-page experience; authenticated workspace presentation; responsive visual behavior;
accessibility; Frontend Lab review surfaces; deterministic Lab-only presentation scenarios.

**OUT unless explicitly authorized by a future Sprint:** BeeDay Domain/Application/Infrastructure;
production backend behavior; database changes; migrations; production authentication behavior;
production business workflows; direct Lab → production synchronization; BeeDay `main`/`prd`
promotion; redesign of application navigation architecture.

## Roadmap

### Phase A — Unified Hero Experience
- 35.1 Unified Hero Experience — Issue [#404](https://github.com/DS-Asyst/BeeDay/issues/404)

Only Sprint 35.1 is defined at this stage. Additional Sprints are created only when owner direction
or validated findings justify them.

## Sprint 35.1 — Unified Hero Experience

**Issue:** [#404](https://github.com/DS-Asyst/BeeDay/issues/404)
**Primary repository:** `DS-Asyst/beeday-frontend-lab`

### Pre-implementation audit

Before any code was written, the existing Lab repository was audited against the owner's request:

1. **Public informational pages (Work Area A) — already fully compliant, no change made.** All 12
   footer/informational routes (`/mission`, `/efficacy`, `/brand-guidelines`, `/contact`, `/beeday`,
   `/beeday-plus`, `/android`, `/ios`, `/faqs`, `/community-guidelines`, `/terms`, `/privacy`)
   already compose through a shared `InstitutionalPageShell` → `BeeDayHero`, via four page-family
   templates (`EditorialPageTemplate`, `HelpPageTemplate`, `LegalDocumentPageTemplate`,
   `ProductPageTemplate`) that preserve family-specific behavior (contextual nav family, surface
   token, FAQ accordion, legal ToC, product primary action) while sharing one top composition. This
   was ported verbatim from `BeeDay.Web` during EPIC 33 Sprint 33.11 (FE33-054..067) — i.e. it
   already existed in production before this EPIC, and is covered end-to-end by
   `InstitutionalPagesTests.AllTwelveInstitutionalPagesRenderANonEmptyHeroHeading`. No Mission-only
   duplication exists; nothing needed to change.
2. **`BeeDayHero` authenticated precedent already exists.** `Wallet.razor` (`/wallet`) already uses
   `BeeDayHero` with `Compact="true" Surface="Cor0"`, though constrained to the page's own 76rem
   width rather than the full workspace width.
3. **The authenticated shell already isolates the sidebar correctly.** `MainLayout.razor.css`:
   `.beeday-workspace` reserves `padding-left: var(--beeday-sidebar-width)` at ≥1200px;
   `DesktopSidebar` is `position: fixed`, entirely independent of document flow. Placing a full-bleed
   `BeeDayHero` directly inside `@Body` therefore automatically spans only the workspace width —
   **no layout/shell CSS changes were required or made.**
4. **`/profile` (`ProfileHome.razor`)** was the gap: its "Welcome back" summary used
   `BeeDayPageHeader` (a plain, uncolored primitive shared by 5 other Lab pages/galleries), wrapped
   together with its cards inside one `max-width: 64rem` section — not full-bleed, not `BeeDayHero`.

### Implementation

- `ProfileHome.razor`: the welcome summary now renders as a `BeeDayHero` (`Compact="true"
  Surface="Cor0"`, same tokens as `Wallet.razor`) placed as a sibling *before* the existing
  `.product-home` section, so it spans the full workspace width while the experience bar, weekly
  activity, and project cards below keep their existing constrained width and are otherwise
  unchanged.
- `BeeDayPageHeader` and its other 5 consumers (`Account`, `PreviewHub`, `EmailPreview`,
  `ExperienceSystemPage`, `ComponentGallery`) were **not** touched.
- `BeeDayHero` itself required **no changes** — its existing `Default` variant plus `Compact`/
  `Surface`/`PrimaryAction` parameters already satisfy the authenticated composition, matching the
  `Wallet.razor` precedent. No new variant, enum value, or component was introduced.
- `MainLayout`, `DesktopSidebar`, `MobileHeader`, `MobileSidebar`: unchanged.

### Tests added

- `DailyPageTests.ProfileRendersItsWelcomeSummaryAsAFullWidthHeroBeforeTheConstrainedProductHomeSection`
  — the hero renders with the expected title/subtitle/eyebrow/primary action, `.product-home` no
  longer carries a page header, and the hero precedes `.product-home` in document order.
- `LayoutShellTests.MainLayoutPlacesAWorkspaceHeroInsideMainWithTheSidebarStructurallyUnaffected` —
  layout-level contract, independent of any one page: a `BeeDayHero` in `Body` renders inside
  `main#main-content`/`.beeday-workspace`, while `aside.desktop-sidebar` stays a direct sibling of
  `.beeday-workspace` under `.beeday-shell`, present exactly once, not nested inside the workspace.

### Validation (Lab, no LocalDB)

```text
dotnet format BeeDayLab.slnx --verify-no-changes   → clean
dotnet build BeeDayLab.slnx -c Release --warnaserror → 0 warnings/errors
dotnet test BeeDayLab.slnx -c Release              → 535/535 passed (30 architecture + 505 web;
                                                        533 → 535, 2 new tests)
git diff --check                                   → clean
```

`dotnet run` verified locally: `/`, `/profile`, `/mission`, `/wallet`, `/daily`, `/design-system`,
`/preview` all `HTTP 200`. `/profile` HTML confirmed to render
`header.beeday-hero.beeday-surface-cor0.beeday-hero--compact.product-home__hero` containing "Welcome
back, jordan.silva", the eyebrow (today's weekday/date), the subtitle, and the "Open Daily" primary
action, followed by the unchanged `.product-home` cards. `/mission` and `/wallet` spot-checked
unaffected.

### Git

- Branch: `sprint/35.1-unified-hero-experience` (`DS-Asyst/beeday-frontend-lab`)
- PR: [#17](https://github.com/DS-Asyst/beeday-frontend-lab/pull/17), `Lab CI` green, merged into
  `hmg` as `ad1339ff9fd7a62bbe52f14fcfecd7f747f942fb`
- **Not** promoted to `prd`; no new baseline tag — pending owner visual approval per Sprint 35.1's
  own boundary.

### Owner visual review

**Status: PENDING.** Passing automated tests does not constitute Sprint 35.1 approval. Exact
`localhost` routes and review points are in the Sprint 35.1 issue (#404) and the chat report. This
document will be updated with the owner's explicit decision (APPROVED or CHANGES REQUIRED) once
recorded — not inferred.
