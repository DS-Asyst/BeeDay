# Sprint 21.13 — Color Inventory & Rationalization Audit

**Status:** audit complete; no palette or runtime code changed.

**Snapshot:** branch `sprint/21.13-color-inventory-audit`, after Sprint 20.15.

**Machine-readable inventory:** [`color-inventory-sprint-21.13.csv`](./color-inventory-sprint-21.13.csv).

## 1. Executive Summary

BeeDay has a sound canonical brand core, but the runtime color footprint is much larger than that
core. The audit found **191 normalized values** when `transparent` is included (**190 physical
color/alpha values**), **180 root color/composite tokens**, and **102 normalized hardcoded values**
outside root token declarations. The largest sources of fragmentation are legacy button/pixel
palettes, Project Workspace/Daily neutrals, authentication feedback hardcodes, and derived alpha
values for shadows/overlays.

The official brand tokens are present and heavily consumed. They do not, however, match the base
pixels of the official raster wordmark: the token blue/yellow are `#185ABD`/`#FFE88D`, while the
dominant opaque wordmark pixels are `#195ABE`/`#FFDE59`. This is evidence for a product decision,
not authorization to recolor either side.

No colors, tokens, CSS, Razor, components, routes, or runtime behavior were changed in this Sprint.

## 2. Method

The audit scanned 286 presentation-related source files under `src/BeeDay.Web`: CSS (global,
scoped, vendor), Razor, C# presentation models/services, HTML, SVG assets, the SVG sprite, and the
PNG wordmark. `bin`, `obj`, and generated `App_Data` email captures were excluded. Tests and the
Epic/design-system documentation were separately inspected for color contracts.

Extraction covered HEX, RGB/RGBA, HSL/HSLA, `transparent`, `currentColor`, gradients, token
definitions/references, SVG fill/stroke, and colors embedded in shadows/outlines/borders. HEX
shorthands/casing and opaque RGB were normalized; alpha values remain distinct. `currentColor` is
counted as a pattern, not a physical color. The single named declaration `black` and `white` inside
`color-mix()` were inspected manually; they do not add new normalized values.

“Occurrence” in the full inventory means a source literal, not a computed-style use through a
token. Token heatmap counts are `var()` references and therefore better express runtime reach.
Hardcoded means a literal outside a root declaration in `variables.css`; this intentionally includes
local component custom properties and legitimate asset/provider exceptions.

## 3. Total Color Count

| Metric | Count | Interpretation |
|---|---:|---|
| Files scanned | 286 | Runtime presentation source only |
| Normalized values | 191 | Includes `transparent` |
| Physical color/alpha values | 190 | Excludes `transparent` and `currentColor` |
| Literal occurrences | 321 | 304 CSS, 10 SVG, 7 Razor/C# |
| Root color/composite tokens | 180 | `variables.css`; includes aliases/shadow composites |
| Local component color properties | 10 | Contextual button/habit/activity properties |
| Total color/composite custom properties | 190 | Root plus local |
| Hardcoded normalized values | 102 | Outside root token declarations |
| Hardcoded literal occurrences | 213 | Includes legitimate specialized values |
| Gradients | 2 | One skeleton, one Wallet decoration |
| SVG hardcoded colors | 4 distinct / 10 occurrences | Social providers only |
| `currentColor` | 115 occurrences | 56 in sprite; remainder Lucide/component CSS |
| Official semantic status families | 4 | Success, Danger, Warning, Info |
| Canonical brand physical variants | 6 | Four blue, two yellow; aliases excluded |
| Official neutral physical variants | 8 | Surface/text/border core |

The complete 191-row table, with the required columns `Color`, `Normalized Value`, `Format(s)
Found`, `Token`, `Semantic Role`, `Consumers`, `Occurrences`, and `Status`, is the adjacent CSV.

## 4. Official Tokens

`variables.css` is the canonical declaration source. Its 180 audited properties split into:

- brand/surface/content/status/activity/attribute/Wallet foundations;
- 48 canonical button properties across Primary, Secondary, Success, Warning, Back, Danger,
  Confirmation Cancel, and Reference Blue;
- card and Dashboard chrome properties;
- 14 Habit category properties;
- 45 deprecated `comic-*` compatibility aliases, which introduce no physical colors;
- four canonical elevation shadows, four focus properties, and 12 legacy game/pixel composites.

The aliases are contracts, not independent palettes. Primary and Secondary resolve to brand and
neutral foundations. `comic-blue`, `comic-yellow`, `comic-orange`, and `comic-magenta` all resolve
to Primary, despite names that imply different hues. Back maps to the neutral Back family; Danger
and Success map to their canonical button families.

## 5. Brand Colors

| Family | Value | Token(s) | Main consumers | Status |
|---|---|---|---|---|
| Brand Blue | `#185ABD` | `--beeday-color-brand-primary` | Navigation, links, Primary button, progress, focus outlines; 80 token references | OFFICIAL BRAND |
| Brand Blue hover/light | `#1654B0` | `brand-primary-hover`, `brand-primary-light` | Hover and contextual light alias | DUPLICATE VALUE, DISTINCT RESPONSIBILITY |
| Brand Blue active | `#124490` | `brand-primary-active` | Primary outline/depth/shadow | OFFICIAL BRAND |
| Brand Blue soft | `#EEF0FF` | `brand-primary-soft` | Selected/hover/soft surfaces | OFFICIAL BRAND |
| Brand Yellow | `#FFE88D` | `brand-yellow` | Reward progress, public CTAs, inverse focus; 9 references | OFFICIAL BRAND |
| Brand Yellow hover | `#FBDB6B` | `brand-yellow-hover` | No current `var()` consumer | DEAD CANDIDATE |
| Yellow foreground | `#2F2737` | `brand-yellow-foreground`, `text-primary` | Yellow CTA text and primary text | DUPLICATE RESPONSIBILITY |

Nearby non-canonical blues include Reference Blue (`#20B4F4`, `#2DBCF8`, `#087FBE`, `#075B8F`,
`#064B78`), legacy game blue (`#27A8E8`, `#0879B5`), vendor NES blue (`#006BB3`, `#108DE0`,
`#209CEE`), reconnect blue (`#0087FF`), and LinkedIn `#0076B2`. Nearby yellows include button
Warning, Habit Yellow, card star, animation highlight, and the public CTA local hover/depth. They
must not be called “brand” solely because of hue proximity.

## 6. Wordmark Colors

`wwwroot/beeday-wordmark.png` is a 904×276 RGBA raster. Pixel inspection found all 256 alpha
levels and 8,306 RGBA tuples because antialiasing stores many edge RGB/alpha combinations.

| Role | Dominant opaque pixel | Pixels | Token comparison |
|---|---:|---:|---|
| Word “bee” | `#195ABE` | 23,643 | Near, but not equal to `#185ABD` |
| Word “day” | `#FFDE59` | 12,243 | Materially darker/more saturated than `#FFE88D` |
| Transparent canvas | RGBA `0,0,0,0` | 172,965 | Expected; RGB under zero alpha is irrelevant |

Other frequent opaque pixels such as `#195AC0`, `#195BBC`, `#FFDF58`, and `#FFDE5B` are raster
antialiasing/edge variants, not authored palette entries. The asset has transparency and no third
intentional brand color. Keep the raster evidence separate from CSS occurrence counts.

## 7. Semantic Colors

| Meaning | Canonical family | Parallel/competing values | Assessment |
|---|---|---|---|
| Success / Positive / Income | `#2F9E5B`, soft `#E8F7EE` | Button greens `#36A866/#46B976/#287D4D/#1A6139/#174A2D`; auth `#236B37/#1D5D2C/#DFF5DF/#B9DFC3/#EEFAF1`; game/habit greens | Coherent intent, fragmented implementation |
| Danger / Error / Delete / Expense / Logout | `#D33B46`, hover `#B32121`, soft `#FFF0F1` | Button/game reds; auth `#761919/#9D1C1C/#B3261E/#E50000`; toast softs; Habit red scale | Most conflicted family; some category reds are legitimate |
| Warning | `#D89B22`, soft `#FFF5DC` | Button Warning six-color physical palette; Habit Yellow and card star | Separate status from control depth and categories |
| Info / Task | `#335F71`, soft `#E7F0F4` | Same `#335F71` is both Info and Task | Exact duplicate responsibility requires decision |
| Disabled | neutral text/surface plus opacity (`.68`, `.72`) | No dedicated disabled color | Coherent compositional pattern; do not add color without evidence |
| Selected | brand primary + soft, feature-specific tints | Project filters also hardcode neutral/purple tints | Mostly coherent, with feature drift |
| Focus | brand alpha ring and yellow inverse ring | Confirmation/reference focus alphas; legacy game focus | Three focus families remain |
| Validation | canonical Danger in forms/settings | Auth and profile hardcoded reds | Should remap to Danger roles later |

Wallet deliberately maps Income/Positive to Success and Expense/Negative to Danger. These are
financial semantics, not brand colors. Arbitrary tag/project colors are user/category identity and
must remain specialized even if a default is rationalized.

## 8. Neutral Colors

The official eight-value physical core is: `#FFFFFF`, `#F7F7F7`, `#EEEEEE`, `#2F2737`, `#514858`,
`#817789`, `#E5E5E5`, and `#CECECE`. It covers page/surface, muted/subtle surface, primary/secondary/
muted text, border, and strong border. `background`, `surface`, and `text-inverse` intentionally
share white.

The real runtime adds at least four competing neutral clusters:

- Project Workspace: `#44394B`, `#5A4B62`, `#706777`, `#746B7B`, `#B9B1BF`, `#DDD7E0`,
  `#E1DCE4`, `#E4DFE6`, `#EEE9F0`, and multiple whites/alphas;
- Dashboard/Daily: `#AAA2B1`, `#AAA4B2`, `#B8B1C0`, `#C6BDD0`, `#17121D`, purple-tinted shadows;
- legacy/vendor: `#171321`, `#212529`, `#ADAFBC`, `#E7E7E7`, black;
- auth/onboarding: `#D9D2E8`, `#F3ECFF`, `#F4EDFF`, `#FAF7FC`, purple text/shadows.

This is the largest consolidation opportunity. The next Sprint should distinguish required
contrast states from nearly equivalent local grays before mapping them to the official core.

## 9. Feature-specific Colors

| Area | Current family | Classification |
|---|---|---|
| Navigation | Neutral surfaces/text, brand active/focus, Danger logout, alpha mobile overlay | Mostly official; overlays hardcoded |
| Home/Profile | Brand blue/yellow, white alpha ornaments, public footer `#17203B`, XP yellow, success completion | Footer and contextual CTA derivatives need named roles |
| Daily cards | Task `#335F71/#244554`, Todo `#BF2EC7/#92239A`, Project `#8056C7/#613CA2` | SPECIALIZED category colors |
| Habits | White, Yellow, Green, Sky, three Red strengths and dark partners | SPECIALIZED user-selected scale; red overlaps Danger visually |
| Attributes | Strength `#B3432F`, Dexterity `#1F8A99`, Intelligence `#4456C9`, Vitality `#B23A86` | SPECIALIZED but currently token-dead |
| Wallet | Success/Danger for income/expense; default tag `#7A4FCB`; arbitrary user tags with contrast calculator | Financial semantics plus specialized data colors |
| Account/Settings | Official forms and Danger; auth feedback hardcodes | Mixed canonical/legacy |
| Cards | Dedicated title/description/star/meta palette plus official surface/border | Specialized; some responsibility duplicates neutrals |

`BeeDayCard` itself uses official surface, border, shadow and background tokens for default, muted,
prominent and interactive treatments. Selected state is consumer-owned; there is no independent
card-selected palette. `BeeDayProgressBar` uses brand primary, Success for complete, Yellow for
reward, neutral track, and two hardcoded marker alphas.

## 10. Hardcoded Colors

There are 102 normalized hardcoded values across 213 literal occurrences. High-impact candidates:

| Value/group | Location/context | Existing equivalent | Future recommendation |
|---|---|---|---|
| White and white alphas | Project Workspace, cards, Home, footer, progress | `surface`, `text-inverse` | Tokenize only repeated responsibility/alpha roles |
| `#761919`, `#FFD9D9` | Login/identity validation | Danger family | REMAP |
| `#B3261E` fallback | Create Account validation | Danger family | REMAP/remove fallback after compatibility check |
| `#26B050`, `#E50000` | Blazor validation outlines | Success/Danger | REMAP |
| `#9D1C1C/#EFB9B9/#FFF0F0` and success counterparts | legacy auth feedback | Semantic family | REMAP |
| `#AEA5B5` | scrollbar | neutral muted/strong border candidates | MERGE after contrast review |
| Project Workspace neutral cluster | 30 literals | official neutral scale | Highest-value neutral mapping exercise |
| `#17203B` | public footer | none | KEEP SPECIALIZED or create public-footer token |
| `#F5D75B/#C7A91F/#C8D7ED` | contextual public CTA states | yellow/blue families | Name derived state or map after interaction review |
| `#7A4FCB/#8056C7` in C#/Razor defaults | tag/project defaults | existing specialized tokens | Centralize source contract without changing persisted meaning |
| `#17111F/#FFFFFF` | tag contrast calculator | no semantic token | KEEP algorithmic contrast constants |

The CSV gives every value, source, count and status. Hardcodes are not automatically defects:
provider colors, raster pixels, data-entry defaults, algorithmic contrast colors and purposeful
alpha overlays are legitimate exceptions.

## 11. SVG / Asset Colors

All 56 system symbols in `sprite.svg` use `currentColor`; source Lucide files do the same. Across
all source assets, `currentColor` occurs 115 times. `BeeDayIcon` owns semantic classes for Success,
Warning, Danger and Information and otherwise inherits consumer text color.

The only four physical SVG colors are legitimate social-provider values:

| Value | Asset | Occurrences | Classification |
|---|---|---:|---|
| `#3D5A98` | Facebook source + sprite | 2 | SOCIAL PROVIDER |
| `#0076B2` | LinkedIn source + sprite | 2 | SOCIAL PROVIDER |
| `#181616` | GitHub source + sprite | 2 | SOCIAL PROVIDER |
| `#FFFFFF` | Facebook/LinkedIn glyphs in source + sprite | 4 | ASSET-SPECIFIC |

Official X, Instagram and YouTube SVGs already use `currentColor`. No unjustified hardcoded color
was found in system icons.

## 12. Gradient Inventory

| File | Declaration | Classification | Recommendation |
|---|---|---|---|
| `wwwroot/css/feedback.css:274` | skeleton `linear-gradient(90deg, #EBE7EF 25%, surface-muted 50%, #EBE7EF 75%)` | REQUIRED motion/loading | Keep behavior; map hardcoded stop later |
| `wwwroot/css/wallet.css:121` | `linear-gradient(180deg, color-mix(...brand-primary 7%, transparent), transparent)` | DECORATIVE | Review against solid-surface Epic direction |

No radial, conic, or additional repeating gradients were found.

## 13. Shadow / Overlay Colors

Canonical elevation uses brand-active RGB (`18 68 144`) at 6%, 8%, 10%, and 16%. Focus uses
brand primary at 28%; inverse focus uses yellow at 42%. Activity cards define purple/brown shadow
alphas locally; game/pixel shadows use solid `#171321` plus an 18% alpha; dialogs/drawers use
hardcoded dark overlays ranging from 55% to 82%; the canonical `--beeday-color-overlay` at 64% has
no consumer. Physical button “depth” is a bottom border but consumes color families equivalent to
shadows and is included in the token audit.

The overlay conflict is architectural: a dead canonical blue overlay coexists with multiple active
purple/black overlays. Do not remap until modal/drawer contrast and intended mood are compared.

## 14. Duplicate Colors

Exact normalized duplicate groups:

- `#FFFFFF`: background, surface, inverse text, three button foregrounds, cancel background,
  game panel, plus many hardcodes;
- `#1654B0`: brand hover and brand light;
- `#2F2737`: primary text and yellow foreground;
- `#335F71`: Info and Task;
- `#171321`: Warning/cancel foreground and game ink;
- `#7A4FCB`: tag token and hardcoded Wallet defaults;
- `#8056C7`: project token and C# default, differing only by case;
- `#D93640`/`#D71920`/`#A90F15`: Habit tokens are also close to Danger/button reds in purpose
  perception, though not exact duplicates;
- `#FFF/#FFFFFF`, `#7A4FCB/#7a4fcb`, and `#8056C7/#8056c7` are representation duplicates.

Near-duplicate responsibility groups include at least ten local neutral borders/texts, six Success
greens, multiple Danger reds/soft reds, three focus systems, four yellow/reward/warning groups, and
canonical versus wordmark blue/yellow.

## 15. Unused Tokens

Static `var()` analysis found 20 root properties without consumers:

`attribute-strength`, `attribute-dexterity`, `attribute-intelligence`, `attribute-vitality`,
`brand-yellow-hover`, `filter-surface-tint`, `info-soft`, `overlay`, `success-soft`, `warning-soft`,
`focus-color-inverse`, `focus-outline`, `game-blue-dark`, `game-green`, `game-ink-soft`, `game-panel`,
`game-shadow-sm`, `game-shadow-md`, `game-shadow-lg`, and `shadow-xs`.

The four attribute tokens are specialized reservations mirrored by enum-to-class behavior and may
be intentionally dormant; verify planned Attribute UI before removal. The game/pixel tokens are
legacy candidates. Soft status and overlay tokens are more likely candidates for adoption during
remapping than immediate deletion.

## 16. Legacy Colors

Legacy is concentrated in `vendor/nes-core.beeday-excerpt.css`, `pixel-nes.css`, the `--beeday-game-*`
family, old auth rules at the end of `app.css`, and deprecated `comic-*` aliases. The aliases are
already semantic mappings and should be removed only with their class-string consumers. Vendor
colors should not be merged into official tokens unless the vendor excerpt remains a supported
runtime dependency.

## 17. Conflicting Responsibilities

1. Danger has canonical status, button/game, auth validation, toast, Blazor outline, Expense, Logout,
   Delete, and Habit Red values.
2. Success has canonical status, button, auth feedback, Blazor outline, Income, Positive, and Habit
   Green values.
3. Warning Yellow, reward Yellow, brand Yellow, Habit Yellow and star Yellow look related but encode
   different meanings.
4. Info and Task share exactly `#335F71`.
5. Brand Blue competes visually with Reference Blue, game/vendor blue, social blue and reconnect
   blue.
6. Project/Daily invent local neutral scales instead of consistently using the official core.
7. A canonical overlay is dead while consumers hardcode several dark overlay families.
8. Card semantic text/meta tokens overlap the neutral text/surface responsibilities.
9. The raster wordmark and CSS brand tokens do not match exactly.

## 18. Usage Heatmap

Token references, ordered by reach:

| Token/role | References | Principal consumers |
|---|---:|---|
| Brand primary | 80 | Navigation, buttons, links, progress, focus |
| Neutral border | 62 | Cards, forms, layout, controls |
| Surface white | 56 | Cards, panels, buttons, forms |
| Primary text | 54 | Global text and component headings |
| Muted text | 30 | Metadata, hints, disabled content |
| Secondary text | 25 | Supporting copy and Back controls |
| Danger | 23 | Validation, delete, logout, expense |
| Focus ring | 20 | Global and component focus-visible |
| Game ink | 17 | Legacy physical/button/card styling |
| Surface muted | 17 | Hover, fields, tracks, muted cards |
| Strong border | 16 | Interactive/secondary controls |
| Small shadow | 15 | Cards and low elevation |
| Brand soft | 13 | Selected and soft interaction surfaces |
| Contextual button background | 11 | Public blue-surface CTA overrides |
| Large shadow | 11 | Dialogs/cards/high elevation |
| Focus color | 10 | Ring and compatibility aliases |
| Page background | 10 | App/layout regions |
| Brand yellow | 9 | Reward/public CTA/inverse focus |
| Contextual button depth | 9 | Public yellow CTA |
| Brand active | 9 | Primary depth/outline/shadow |

Largest literal-bearing files are `variables.css` (108), `ProjectWorkspace.razor.css` (30),
`app.css` (21), `cards.css` (18), vendor NES (18), `design-system.css` (12), Welcome (8), feedback,
identity and DashboardColumn (7 each).

## 19. Risks

- A global replacement by nearest HEX could merge brand, semantic, finance and category identities.
- Removing “unused” tokens without checking dynamically constructed classes or planned Attribute UI
  could break contracts.
- Changing tag/project defaults may affect persisted data and contrast calculation.
- Recoloring social assets would violate provider identity.
- Replacing alpha overlays with opaque neutrals could change elevation/focus perception and contrast.
- Aligning the wordmark to CSS (or CSS to wordmark) is a product/brand decision, not cleanup.
- Vendor/pixel removal must be coupled to class-consumer removal, not token deletion alone.

## 20. Proposed Rationalization Strategy

### KEEP

- Canonical Brand Blue/Yellow tokens pending the wordmark decision.
- Eight-value official neutral core.
- Four semantic status roles (Success, Danger, Warning, Info).
- Task/Todo/Project, Habit, Attribute, and arbitrary Wallet tag colors as specialized identities.
- Provider SVG colors and tag contrast constants.
- `currentColor` system icon pattern.
- Skeleton gradient unless loading behavior is redesigned.

### MERGE

- Duplicate representations/casing of white, tag purple and project purple.
- `brand-primary-light` with hover unless a distinct future light value is required.
- Project/Daily neutral clusters into evidence-backed official neutral roles.
- Card text/meta neutrals where contrast permits.
- Repeated white-alpha marker/border roles into a small derived-state set.

### REMAP

- Auth/Profile/Blazor validation hardcodes to semantic Danger/Success.
- Income/Expense consumers through explicit finance aliases resolving to Success/Danger.
- Active overlays to one or two intentional overlay roles after visual/contrast review.
- Public footer and CTA derivatives to named contextual tokens if they remain product-wide contracts.
- Hardcoded C#/Razor default values to shared presentation constants without changing stored HEX.

### REMOVE

- Dead game/pixel tokens only after confirming zero class/runtime dependency.
- Deprecated `comic-*` aliases only after all compatibility class consumers disappear.
- Duplicate local hardcodes after remapping.
- Decorative Wallet gradient if the solid-surface review rejects it.

### SPECIALIZED

- Social provider assets, raster wordmark pixels, user-selected tag/project colors, Habit category
  scale, activity categories, attributes, and algorithmic contrast colors.

### NEEDS PRODUCT DECISION

- Whether CSS brand or raster wordmark values are authoritative.
- Whether Info and Task may intentionally share one color.
- Whether Habit Red may overlap Danger and Habit Green may overlap Success.
- Whether finance gets explicit aliases or directly uses semantic status tokens.
- Whether Reference Blue and legacy game/vendor palettes remain supported.
- Whether public footer `#17203B` becomes a formal brand-neutral token.
- Whether selected/focus/overlay require separate semantic families.

## 21. Recommended Scope for the Next Sprint

Implement rationalization in bounded passes: (1) lock brand/wordmark product decisions; (2) publish
the canonical neutral and semantic role matrix; (3) remap auth/forms/navigation and finance aliases;
(4) map Project/Daily neutrals with screenshot and contrast evidence; (5) formalize contextual
public colors; (6) remove verified dead legacy tokens and compatibility aliases; (7) leave provider,
user-data and legitimate category colors untouched. Each pass should include computed-style E2E
assertions and contrast checks; do not attempt a repository-wide nearest-color replacement.

## Validation Record

- Runtime source changes: none.
- Documentation added: this report and the 191-row CSV inventory.
- Tests inspected: VisualFoundation, Home, LoginExperience, ShellResponsiveLayout, Wallet component
  tests and color validators.
- Build/test: not required or executed because no runtime/project input changed.
- Final `git diff` and `git status` are recorded in the Sprint handoff after removing the temporary
  analysis utility.
