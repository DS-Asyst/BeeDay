# EPIC 21 — Lingo-Based Product Experience & Design System

**Fonte da verdade:** contexto oficial da EPIC 21 estabelecido pelo responsável pelo repositório
(2026-08-12); descobertas verificadas diretamente em código durante a Sprint 21.1 (branch
`sprint/21.1-lingo-architecture-design-system-mapping`, criada a partir de `hmg` em `5efdb85`) —
leitura direta de `src/BeeDay.Web/`, `src/BeeDay.Domain/`, `src/BeeDay.Application/`,
`docs/design-system/`, `docs/epics/20-home-visual-experience/README.md`, e do repositório de
referência local `C:\DevOps\Projects\duolingo-clone` (READ-ONLY, nunca modificado). Nenhuma
afirmação de "estado atual" abaixo vem de memória — quando este documento evoluir em Sprints
futuras, cada atualização deve reverificar contra o código antes de alterar uma afirmação de
estado atual.

**Última verificação:** 2026-08-12 (Sprint 21.3 — BeeDay Navigation, COMPLETE — segunda Sprint de
implementação da EPIC; ver "Sprint 21.3 — Results" ao final deste documento). Sprint 21.2 — BeeDay
Shell Foundation, COMPLETE (ver "Sprint 21.2 — Results", preservada como registro dessa Sprint).
Sprint 21.1 — Lingo Architecture & Design System Mapping, COMPLETE — especificação técnica,
nenhuma implementação visual realizada (seções 1-21 abaixo, preservadas como registro dessa
Sprint).

**Escopo da Sprint 21.1 (seções 1-21 abaixo):** transformar a referência visual genérica do
Lingo/Duolingo Clone em uma especificação técnica concreta de migração para o BeeDay — medidas,
componentes, contratos, equivalências e decisões. Não incluiu implementação de código de produção.
A Sprint 21.2 (resultados ao final deste documento) é a primeira a alterar `src/BeeDay.Web/`.

---

## 1. Executive Summary

O Lingo (`C:\DevOps\Projects\duolingo-clone`) é um app Next.js 16 / React 19 / Tailwind 3.4 que
usa Clerk para autenticação. Sua "identidade Duolingo" não vem de uma paleta de cores customizada
no Tailwind config (não existe nenhuma) — vem de três coisas concretas e reproduzíveis:

1. **Um shell estrutural muito simples e binário**: sidebar fixa de 256px + rail direita fixa de
   368px no desktop (≥1024px), ambas completamente ausentes abaixo de 1024px, com o header mobile
   de 50px assumindo sozinho a navegação. Não existe estado "tablet" intermediário.
2. **Um único mecanismo de profundidade física para botões**: borda inferior grossa
   (`border-b-4`) que colapsa a `0` no `:active`, sem nenhum `box-shadow`. Esse é o "peso" visual
   Duolingo, não a paleta verde.
3. **Uma única família tipográfica (Nunito) fazendo todo o trabalho de hierarquia** via
   peso/tamanho/uppercase/tracking — sem uma segunda fonte de destaque.

O BeeDay já está estruturalmente mais próximo do Lingo do que o contexto da EPIC presumia: Nunito
já é a fonte de corpo canônica (migrada na Sprint 20.6, EPIC 20, contra uma referência visual
diferente), o cursor customizado já foi removido (Sprint 20.3), e existe uma query de dashboard
(`GetDashboardQuery`/`DashboardResponse`) que já agrega XP/Level/Habits/Tasks/Projects/Wallet —
exatamente os dados que uma rail direita precisaria consumir.

Os gaps reais não são estéticos, são estruturais e de domínio:

- **Shell**: BeeDay não tem uma sidebar persistente nem uma rail direita — hoje é uma top nav bar
  fixa + dois painéis slide-out mutuamente exclusivos (Perfil / Conta). Isso é uma reconstrução de
  shell, não um reskin.
- **Domínio**: Streak, Achievements/Badges, Daily/Weekly Goals e Quests **não existem** no Domain
  atual. Uma rail direita "estilo Lingo" só pode ser preenchida honestamente com o que já existe
  (XP/Level via `ExperienceBar`, resumos de Habits/Tasks/Projects/Wallet via `DashboardResponse`)
  — o resto exige trabalho de produto/domínio antes de virar UI.
- **Token recente em conflito**: a EPIC 20 (Sprint 20.6/20.7) introduziu
  `--beeday-color-brand-primary: #2538d2` como azul canônico contra uma referência visual
  *diferente*. A paleta desta EPIC (`#1023C8` etc.) exige remigrar essa família de token — a lista
  de consumidores já é conhecida (documentada pela própria EPIC 20), não precisa ser redescoberta.
- **Linguagem "comic" de botão**: é hoje a linguagem de ênfase operacional primária do
  `BeeDayButton` (40+ consumidores, incluindo Login), documentada como decisão deliberada, não
  débito acidental. Migrar para a geometria física do Lingo (borda inferior colapsável) é uma
  mudança ampla e multi-Sprint, não uma flag.

Este documento é a especificação de referência para as Sprints 21.2–21.12: toda seção abaixo cita
o arquivo exato (Lingo ou BeeDay) que sustenta a conclusão.

---

## 2. Método

Toda medida "Lingo" abaixo vem de leitura direta de arquivo (não de screenshot, não de README, não
de inferência) em `C:\DevOps\Projects\duolingo-clone`, tratado como READ-ONLY durante toda a
Sprint — nenhum arquivo do Lingo foi criado, editado ou commitado. Toda medida "BeeDay" vem de
leitura direta do estado atual em `sprint/21.1-lingo-architecture-design-system-mapping`
(sincronizado de `hmg` em `5efdb85`, que incluiu mudanças pós-EPIC-20 relevantes: remoção completa
de `LoginBackground`, ajustes em `Home.razor`, `PublicHeader.razor`, `OnboardingLayout.razor.css`,
`design-system.css`, `pixel-nes.css`).

---

## 3. Lingo Application Shell

**Fonte:** `app/(main)/layout.tsx`, `components/sidebar.tsx`, `components/mobile-header.tsx`,
`components/mobile-sidebar.tsx`, `components/feed-wrapper.tsx`, `components/sticky-wrapper.tsx`,
`app/(main)/learn/page.tsx`.

```text
Desktop (≥1024px / lg:)

┌────────────────┬───────────────────────────────────────────┬──────────────────┐
│   Sidebar        │              Main Feed                     │    Right Rail    │
│   256px fixed    │   flex-1, no fixed width                   │    368px          │
│   position: fixed│   wrapped by: max-w-[1056px] mx-auto        │    position:       │
│   top:0 left:0   │   pt-6 (24px), offset lg:pl-[256px]         │    sticky          │
│   border-r-2     │                                             │    top-6 (24px)    │
│   px-4           │   Row gap between feed/rail: 48px           │    gap-y-4 (16px)  │
│                  │   (px-6 = 24px page padding)                │    bottom-6         │
│                  │                                             │    hidden below lg │
└────────────────┴───────────────────────────────────────────┴──────────────────┘

Mobile (<1024px)

┌─────────────────────────────────────────────────────────────────┐
│ Mobile header — fixed, h-[50px], bg-green-500, z-50, hamburger    │
├─────────────────────────────────────────────────────────────────┤
│ main, pt-[50px] (clears fixed header)                             │
│   → Feed only. Sidebar and Right Rail are NOT rendered (display:  │
│     none via `hidden`) — no reflow, no stacking, they simply       │
│     disappear. Sidebar becomes reachable only via hamburger →      │
│     Radix Sheet, slide-in from left, w-3/4 capped sm:max-w-sm.     │
└─────────────────────────────────────────────────────────────────┘
```

Detalhe estrutural chave: a Sidebar em si **não muda de forma** entre desktop e mobile — o mesmo
componente `<Sidebar>` é reaproveitado dentro do `Sheet` mobile (`components/mobile-sidebar.tsx:14`).
Só o container de apresentação muda (fixed-in-flow vs. drawer-overlay). Isso é diretamente
replicável em Blazor: um único componente de sidebar, dois containers de apresentação.

O offset do main content troca de mecanismo no breakpoint, não só de valor:
`pt-[50px] lg:pl-[256px] lg:pt-0` (`app/(main)/layout.tsx:11`) — padding-top no mobile (para
liberar o header fixo), padding-left no desktop (para liberar a sidebar fixa), nunca os dois ao
mesmo tempo.

---

## 4. BeeDay Current-State Comparison — Shell

**Fonte:** `Components/Layout/MainLayout.razor.css`, `TopNavigation.razor.css`,
`ProfileSidePanel.razor.css`, `AccountSidePanel.razor.css`.

| Dimensão | Lingo | BeeDay hoje |
|---|---|---|
| Navegação primária | Sidebar persistente 256px, sempre visível ≥1024px | `TopNavigation` — barra fixa no topo, `4.25rem` de altura, sem sidebar persistente |
| Painéis secundários | Nenhum — sidebar é a única navegação | `ProfileSidePanel` (17rem) e `AccountSidePanel` (24rem) — slide-out, mutuamente exclusivos, controlados por grid `.beeday-workspace` (`1.35rem minmax(0,1fr) 1.35rem` colapsado → `17rem .../1.35rem` ou `1.35rem/... 24rem` quando um painel abre) |
| Rail direita persistente | Sim — 368px, sempre visível ≥1024px, mostra progresso/monetização/quests | Não existe — `AccountSidePanel` é overlay sob demanda, não uma rail de conteúdo sempre visível |
| Breakpoint estrutural | Um único: 1024px (`lg:`) | `MainLayout.razor.css:88` usa `760px`; `TopNavigation.razor.css` usa `920px` e `680px` — três breakpoints distintos já no shell, nenhum compartilhado com o de `1024px` do Lingo |
| Mecanismo de drawer mobile | Radix `Sheet`, `translateX` via Tailwind `data-[state]` | Já existe: `ProfileSidePanel`/`AccountSidePanel` usam `transform:translateX(...)`, `transition 300ms cubic-bezier(.2,.8,.2,1)`, viram `position:fixed` overlay abaixo de 760px — mecanismo de transição é **diretamente reaproveitável** para uma futura sidebar mobile |

**Conclusão estrutural:** o BeeDay não tem uma Sidebar para "evoluir" — tem um TopNavigation (que
pode evoluir para um mobile header) e dois padrões de drawer (que podem evoluir para o mecanismo
de abertura da sidebar mobile). A Sidebar desktop persistente em si é um componente novo.

---

## 5. Typography Specification

**Fonte Lingo:** `app/layout.tsx:3,13,40` (`next/font/google` → `Nunito`, subset `latin`, sem
array de `weight` explícito — resolve para o peso padrão da fonte variável), classes literais em
`components/sidebar.tsx:26`, `promo.tsx:13`, `quests.tsx:14,35`, `unit-banner.tsx:15-16`,
`components/ui/button.tsx:9`, `app/(marketing)/page.tsx:22`.

**Fonte BeeDay:** `wwwroot/css/typography.css:1-54`, `wwwroot/css/typography-policy.css`.

| Uso | Lingo (classe/valor real) | BeeDay hoje (token/valor real) |
|---|---|---|
| Fonte de corpo | `Nunito` (next/font/google, sem peso fixo) | `--beeday-font-body: "Nunito","Segoe UI",sans-serif` — **já idêntico**, migrado na Sprint 20.6 |
| Fonte de título/wordmark | Nunito (mesma fonte, apenas mais pesada — `text-2xl font-extrabold`) | `--beeday-font-ui: "Jersey 25","Segoe UI",sans-serif` — **fonte diferente**, `!important` em `typography-policy.css:19` |
| Texto de botão | `text-sm font-bold uppercase tracking-wide` (14px/700, Nunito) | `.beeday-button` usa `var(--beeday-font-ui)` (Jersey 25) `!important`, `font-size:.88rem`, `font-weight` forçado a 400 `!important` (`typography-policy.css:51-53`) — nem a fonte nem o peso seguem o padrão Lingo hoje |
| Título de card | `text-lg font-bold` (18px/700, Nunito) | `--beeday-type-title: 400 1.5rem/1.2 var(--beeday-font-ui)` — fonte diferente, peso menor |
| Wordmark | `text-2xl font-extrabold tracking-wide` — **mesma fonte do resto do app**, só mais pesada | `BeeDayBrand` usa `--beeday-font-ui` — fonte dedicada, nunca reutilizada em texto comum |

**Confirmação importante:** o Lingo não usa uma segunda família tipográfica em nenhum lugar. A
hierarquia visual "Duolingo" vem inteiramente de peso (400→700→800) + `uppercase tracking-wide` em
botões, nunca de trocar a fonte. Isso é uma característica central e verificável do Design System
de referência, não uma simplificação.

### Recomendação técnica — Jersey 25

**RETIRE** (fidelidade máxima ao Lingo) é a leitura mais literal do que foi encontrado — o próprio
Lingo não separa uma fonte de "chrome" de uma fonte de "corpo", nem mesmo no wordmark. Uma
alternativa de compromisso é **LIMIT TO BRAND**: reter Jersey 25 exclusivamente no wordmark
`BeeDayBrand` (equivalente ao "Lingo" do sidebar, que no próprio Lingo usa Nunito — ou seja, mesmo
essa opção já é menos fiel que o Lingo real, mas preserva uma identidade de marca já estabelecida
e documentada como deliberada).

- **Impacto de RETIRE:** `BeeDayButton` (todas as variantes + todos os modificadores `--comic*`,
  passa a usar `--beeday-font-body`), todos os títulos que usam `--beeday-type-title`/
  `--beeday-type-subtitle`, o link de navegação ativo em `TopNavigation.razor.css:207-218`
  (`font-family:var(--beeday-font-ui)` no estado ativo), e `BeeDayBrand`. Consumidores totais:
  40+ (herdado do inventário de botões) + todo consumidor de `--beeday-type-title`/`-subtitle`.
- **Impacto de LIMIT TO BRAND:** apenas os itens acima *exceto* `BeeDayBrand` — reduz o raio de
  impacto imediato a botões e títulos, mantendo uma única exceção de marca isolada e já
  documentada.
- Esta é uma decisão de identidade de marca, não puramente técnica — ver "Decisions Required"
  (Seção 19). Não decidida nesta Sprint.

---

## 6. Button Specification

**Fonte Lingo:** `components/ui/button.tsx` (cva, linhas 9-56), `app/(main)/learn/lesson-button.tsx:86,104`.

**Fonte BeeDay:** `wwwroot/css/design-system.css:18-464` (`.beeday-button` e modificadores),
`wwwroot/css/polish.css:36-40` (overrides de cascata), `wwwroot/css/pixel-nes.css:44-66`
(`--pixel-cta`), `Components/DesignSystem/Buttons/BeeDayButton.razor(.cs)`.

| Propriedade | Lingo | BeeDay hoje (efetivo, pós-cascata) |
|---|---|---|
| Mecanismo de profundidade | `border-b-4` (colorido, mais escuro) → `active:border-b-0` (colapso total); variante `default` usa `border-b-4 active:border-b-2` (colapso parcial); nós de lição usam `border-b-8` | `box-shadow: var(--beeday-shadow-md)` na variante base (elevação, não borda); modificador `--comic-press` usa offset-shadow hard (`5px 5px 0 #000` → `1px 1px 0` no active, com `translate(4px,4px)`); modificador `--comic` usa `box-shadow` que **desaparece** no active (`box-shadow:none; transform:translateY(4px)`) — três mecanismos de profundidade diferentes coexistindo, nenhum idêntico ao Lingo |
| Radius | `rounded-xl` (12px) em quase tudo; `rounded-full` só no size `rounded` (nós de lição circulares) | Base = `var(--beeday-radius-pill)` (999px, pill total — decisão canônica da Sprint 20.8); `--comic` = `9px`; `--comic-press` = `.625rem`(10px) — nenhum reflete o `12px` predominante do Lingo |
| Altura | `h-11`(44px) default, `h-9`(36px) sm, `h-12`(48px) lg | `min-height` efetivo `3rem`(48px, via `polish.css:36` sobrescrevendo o `2.75rem` de `design-system.css`) — próximo do `lg` do Lingo, não do `default` |
| Tipografia | `text-sm font-bold uppercase tracking-wide` (14px/700, Nunito) | `.88rem`(~14px)/peso **400 forçado** `!important`, `letter-spacing:.035em`, fonte Jersey 25 — tamanho comparável, peso e fonte não |
| Hover | `hover:bg-{color}/90` (leve escurecimento de opacidade) | `translateY(-2px); box-shadow:var(--beeday-shadow-lg)` (elevação por movimento, não mudança de cor) |
| Foco | `focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2` | `box-shadow:var(--beeday-shadow-md), var(--beeday-focus-ring)` (`0 0 0 3px rgb(37 56 210/20%)`) — mecanismo equivalente (anel), cor já é o brand-primary atual (a remigrar) |
| Disabled | `disabled:pointer-events-none disabled:opacity-50` | Paleta cinza dedicada por variável (`#d3d3d3` etc.) + `opacity:.62; cursor:not-allowed` — mais elaborado que o Lingo, funcionalmente equivalente |
| Loading | Sem prop dedicada; `Loader` do lucide com `animate-spin` ad hoc | Sem prop dedicada encontrada no `BeeDayButton` lido — mesmo padrão ad hoc presumido, não confirmado nesta leitura |
| Variantes | 13 (`default/locked/primary/primaryOutline/secondary/secondaryOutline/danger/dangerOutline/super/superOutline/ghost/sidebar/sidebarOutline`) | 8 no enum (`Primary/Secondary/Success/Warning/Back/Danger/ConfirmationDanger/ConfirmationCancel`) + 5 modificadores de geometria opt-in (`--comic`(8 cores)/`--comic-press`/`--skew-press`/`--plain`(2)/`--pixel-cta`) — taxonomias não comparáveis 1:1; Lingo mistura cor+contexto na variante, BeeDay separa cor (variant) de geometria (modifier class) |

### Recomendação de API e estratégia de migração

`BeeDayButton` já é a arquitetura correta para absorver a linguagem física do Lingo — **não criar
um segundo componente de botão.** Estratégia recomendada (não implementada nesta Sprint):

1. A geometria padrão de `.beeday-button` (não um modificador opt-in) evolui para o mecanismo
   `border-bottom` colapsável do Lingo — isso é uma mudança na *base* class, não um novo
   modificador, porque o objetivo é que a linguagem física do Lingo vire o padrão, não uma opção a
   mais entre cinco.
2. `--comic*`/`--skew-press`/`--pixel-cta` são marcados como **deprecated** no mesmo commit que
   introduz a nova geometria base, mas **não removidos** — os 40+ consumidores (Login,
   `EditorModalShell`, `BeeDayConfirmDialog`, Wallet, Identity, Account) migram incrementalmente em
   Sprints seguintes (ver Component Mapping, Seção 15), cada um confirmando visualmente antes do
   `--comic-*` correspondente ser removido.
3. O contrato público (`BeeDayButtonVariant` enum: Primary/Secondary/Success/Warning/Back/Danger/
   ConfirmationDanger/ConfirmationCancel) **não muda** — variantes continuam mapeando cor/função,
   não geometria. Isso preserva compatibilidade de todos os call sites que só passam `Variant`.
4. Radius-padrão migra de `pill`(999px) para algo próximo de `12px` (`--beeday-radius-md`/`-lg` já
   existem na escala atual, `.625rem`/`.875rem` — nenhum bate exatamente com `12px`; decisão de
   qual token usar ou se cunhar um novo fica para a Sprint de implementação, 21.5).

---

## 7. Card/Surface Specification

**Fonte Lingo:** `components/promo.tsx:8`, `components/quests.tsx:12`, `app/(main)/learn/unit-banner.tsx:13`.

**Fonte BeeDay:** `wwwroot/css/design-system.css:477-496`, `wwwroot/css/polish.css:66,98`,
`Components/DesignSystem/Cards/BeeDayCard.razor(.css)`.

| Propriedade | Lingo (Promo/Quests) | BeeDay `.beeday-card` hoje |
|---|---|---|
| Border | `border-2` (2px, cor neutra padrão do tema) | `border:0` (sem borda — decisão canônica Sprint 20.8) |
| Radius | `rounded-xl` (12px) | `var(--beeday-radius-2xl)` (1.75rem/28px) |
| Padding | `p-4` (16px) | `--padded`: `clamp(1rem,2.5vw,1.5rem)` (16–24px, via `polish.css:66`) — comparável |
| Background | sem classe explícita (herda branco) | `var(--beeday-color-surface)` (#fff) — equivalente |
| Profundidade | **nenhuma** (`shadow` nunca usado em Promo/Quests/UnitBanner) | `box-shadow:var(--beeday-shadow-lg)` — o Lingo usa borda fina como única indicação de superfície; o BeeDay usa sombra elevada como padrão |
| Espaçamento interno | `space-y-4` (16px entre blocos) | não padronizado centralmente — depende do conteúdo |

**Conclusão:** o card do Lingo é deliberadamente "chato" — borda fina, sem sombra, sem
profundidade. O `BeeDayCard` atual é o oposto (sem borda, com sombra grande) — outra decisão da
Sprint 20.8 que precisa de reversão consciente de direção, não de ajuste incremental, para atingir
fidelidade ao Lingo.

---

## 8. Navigation Specification

**Fonte Lingo:** `components/sidebar.tsx`, `components/sidebar-item.tsx`.

**Fonte BeeDay:** `Components/Layout/TopNavigation.razor.css`.

- Item de navegação Lingo: `h-[52px] justify-start` (altura fixa 52px), ícone `32×32` com
  `mr-5`(20px), ativo = `bg-sky-500/15 text-sky-500 border-sky-300 border-2`, inativo =
  `bg-transparent text-slate-500 border-2 border-transparent`.
- Item de navegação BeeDay hoje (`TopNavigation.razor.css:110-111,207-218`): links horizontais,
  `font-size:1.18rem; font-weight:700`, estado ativo salta para `font-size:1.45rem` +
  `text-transform:uppercase` + `text-shadow:2px 2px 0 var(--beeday-game-ink)` — comunica estado
  ativo por *tamanho de fonte e sombra de texto*, não por preenchimento de fundo/borda como o
  Lingo.

**Mapeamento futuro de itens de navegação (hipótese a validar em 21.3, não implementar agora):**
Home / Habits / Tasks / Projects / Wallet — todos já existem como conceitos de produto reais
(confirmado na Seção 14), diferente de "Leaderboard"/"Shop" do Lingo que não têm equivalente
BeeDay e não devem ser forçados.

---

## 9. Login/Modal Specification

**Fonte Lingo:** `app/(auth)/layout.tsx`, `app/(auth)/header.tsx`, `app/(auth)/sign-in/[[...sign-in]]/page.tsx`, `components/ui/dialog.tsx`, `components/modals/exit-modal.tsx`.

**Fonte BeeDay:** `Components/Features/Authentication/Pages/Login.razor(.css)`,
`Components/DesignSystem/Modals/EditorModalShell.razor`, `wwwroot/css/editor-modal.css`,
`Components/DesignSystem/Feedback/BeeDayConfirmDialog.razor`, `wwwroot/css/feedback.css`.

### Login

| | Lingo | BeeDay hoje |
|---|---|---|
| Apresentação | Página dedicada, **zero wrapper custom** — `<SignIn/>` do Clerk renderiza sozinho, todo o chrome vem do tema Clerk (`colorPrimary:"#22C55E"`) | Página dedicada `Login.razor` sob `OnboardingLayout`, **card custom** `.auth-card` |
| Largura do card | N/A (Clerk controla) | `width:min(100%,31rem)` (496px) |
| Border/radius | N/A | `border:1px solid var(--beeday-color-border); border-radius:var(--beeday-radius-xs)`(.2rem — quase quadrado) |
| Padding | N/A | `clamp(1.6rem,5vw,2.5rem)` |
| Centralização | `flex flex-1 items-center justify-center` em viewport cheio | mesma ideia (`OnboardingLayout` minimalista, sem nav) |
| Botão de submit | Seria `variant="primary"` (Nunito, border-b-4) | `beeday-button--comic beeday-button--comic-blue` — linguagem comic ainda em uso no ponto de entrada mais visível do produto |

**Nota:** o comportamento de autenticação do BeeDay (form POST server-side com antiforgery,
e-mail/senha, remember-me) **não muda** — só a apresentação do card é candidata a evoluir para a
simplicidade do Lingo (mais raio, sem borda, ou geometria equivalente à decidida para `BeeDayCard`
na Seção 7).

### Modal genérico

| | Lingo `Dialog` | BeeDay `EditorModalShell` | BeeDay `BeeDayConfirmDialog` |
|---|---|---|---|
| Backdrop | `bg-black/80` | `rgb(35 25 45/70%)` + `blur(2px)` | `rgb(47 27 72/78%)` + `blur(2px)` |
| Max-width | `max-w-md`(448px, override do app) | `min(30rem, 100vw-1.25rem)`(480px) | `min(31rem, 100vw-2rem)`(496px) |
| Radius | `sm:rounded-lg`(8px, responsivo — 0 abaixo de `sm`) | `var(--beeday-radius-lg)`(.875rem/14px) | `var(--beeday-radius-lg)`(14px) |
| Animação | zoom 95%→100% + slide + fade, 200ms | **nenhuma** (`editor-modal.css` não define `@keyframes` para o modal/backdrop) | `translateY(-.75rem) scale(.97)→none`, com `--beeday-transition-normal` |
| Botão de fechar (X) | Sim, `h-4 w-4`, canto superior direito | **Não existe** — fecha só por backdrop-click ou botão Cancelar | **Não existe** — mesmo padrão |

**Achado relevante:** BeeDay já tem *dois* componentes de modal com backdrop/radius/animação
ligeiramente diferentes entre si (`EditorModalShell` sem animação, `BeeDayConfirmDialog` com) — a
migração para a linguagem do Lingo é uma oportunidade de convergir os dois, não só de aproximá-los
do Lingo.

---

## 10. Icon Specification

**Fonte Lingo:** `package.json` (`lucide-react@^1.25.0`), uso disperso em 21 arquivos, mais SVGs
brutos em `public/` (`mascot.svg`, `heart.svg`, `points.svg`, ícones de nav, bandeiras/personagens).

**Fonte BeeDay:** `Components/DesignSystem/Icons/PixelIcon.razor(.cs/.css)`,
`PixelIconRegistry.cs`, `PixelIconName.cs`.

| | Lingo | BeeDay hoje |
|---|---|---|
| Biblioteca funcional | `lucide-react` (ícones utilitários: menu, X, loader, seta) | Sprite SVG único (`/icons/sprite.svg`), 61 ícones nomeados, fonte = Material Symbols + Devicon (não pixel-art, apesar do nome do componente) |
| Ilustrações/marca | SVGs/PNGs soltos em `public/`, consumidos via `next/image`, sem abstração central | Não inventariado nesta leitura — presumivelmente inexistente hoje (BeeDay não tem mascote) |
| Abstração central | **Nenhuma** — cada call site escolhe `lucide-react` ou `<Image>` diretamente, tamanhos definidos ad hoc por classe Tailwind em cada uso | `PixelIcon` — componente tipado (`PixelIconName` enum, 61 valores), `PixelIconSize` enum (12/16/20/24/32px), `PixelIconColor` enum mapeado a tokens semânticos, **contrato de acessibilidade obrigatório** (lança exceção se `Decorative=false` sem `Label`) |
| Categorias | Implícitas por diretório (`public/`) | Explícitas (`PixelIconCategory`: Actions, Feedback, System, Navigation, Social, Activities, Forms, Statistics) — já inclui ícones de Statistics (`Experience`, `Level`, `Streak`, `Wallet`, `Progress`, etc.) |

### Recomendação

**KEEP ARCHITECTURE / REPLACE VISUAL SOURCE.** A abstração tipada do `PixelIcon` (enum + registry
+ contrato de acessibilidade) é estruturalmente superior à abordagem ad hoc do próprio Lingo — não
há razão técnica para substituí-la. O que precisa mudar é o *conteúdo visual* do sprite: hoje vem
de Material Symbols/Devicon (linguagem geométrica neutra), e a fidelidade ao Lingo pede um traço
mais próximo do estilo arredondado/preenchido usado nos ícones de navegação e nós de lição do
Lingo. Isso é substituição de asset dentro do contrato existente (`PixelIconRegistry`), não
mudança de arquitetura — mudança confinada à camada Web, sem impacto em Application/Domain.

---

## 11. Main Feed Specification

**Fonte:** `app/(main)/learn/page.tsx`, `header.tsx`, `unit.tsx`, `unit-banner.tsx`, `lesson-button.tsx`.

Composição real (não os componentes isolados, a ordem e a relação entre eles):

1. Header sticky do curso (`text-lg font-bold`, fundo branco, `lg:z-50` com truque de
   margem negativa para ficar sticky sem sobrepor a página).
2. Lista de `Unit` (`mb-10` entre unidades), cada uma:
   - `UnitBanner` colorido (`bg-green-500`, `p-5`, texto branco) — contexto/título da unidade,
     com um botão "Continue" que só aparece em `xl:` (1280px).
   - Corrente zig-zag de `LessonButton` circulares (`70×70px`, `border-b-8`), indentação
     alternando por `index % 8` — o "caminho" visual clássico do Duolingo.
3. A rail direita (Seção 12) é **irmã**, não filha, do feed — ambas são filhas diretas do
   `flex flex-row-reverse` (`app/(main)/learn/page.tsx:49`).

**Mapeamento proposto para BeeDay Home (hipótese, a validar em 21.7):**

```text
Lingo Main/Learn         → BeeDay Home
Lessons (corrente zig-zag) → Habits / Tasks (lista de itens acionáveis do dia)
Course progression         → Projects (progresso agregado, já com ProgressPercentage real)
User Progress (pill)       → XP / Level (ExperienceBar, já existe)
Quests                     → sem equivalente de domínio hoje — não forçar (ver Seção 13)
```

Não force o "caminho zig-zag" de lições como forma — isso é específico do modelo de curso
sequencial do Lingo; Habits/Tasks do BeeDay não têm ordem sequencial obrigatória. A fidelidade
aqui deve ser de *hierarquia e composição* (header → itens acionáveis → contexto), não da forma
geométrica específica do caminho.

---

## 12. Right Rail Specification

**Fonte Lingo:** `components/sticky-wrapper.tsx`, `components/promo.tsx`, `components/quests.tsx`.

```text
StickyWrapper (368px, lg:block only)
├── sticky top-6 (24px), gap-y-4 (16px) entre cards
├── UserProgress  — pill compacta: curso/avatar (32×32), pontos (28×28, laranja),
│                    corações (22×22, vermelho, ou infinito 16×16)
├── Promo         — card de upsell (não aplicável ao BeeDay — sem equivalente de produto)
└── Quests        — lista de metas com barra de progresso mini (h-2/8px), sem equivalente
                     de domínio no BeeDay hoje
```

### SUPPORTED NOW vs. REQUIRES PRODUCT/DOMAIN WORK

| Elemento da rail | Status |
|---|---|
| XP / Nível (equivalente a `UserProgress`) | **SUPPORTED NOW** — `ExperienceBar.razor` já existe, já consome dados reais (`DashboardResponse.UserProfileSummary`), hoje vive no `ProfileSidePanel`; só precisa de relocação/reestilização, não de novo dado |
| Resumo de Habits/Tasks/Projects | **SUPPORTED NOW** — `DashboardResponse` já expõe `HabitSummary`/`TaskSummary`/`ProjectSummary` (este último já com `ProgressPercentage` real) |
| Resumo de Wallet | **SUPPORTED NOW** — `WalletSummaryResponse` (Balance/TotalIncome/TotalExpenses) já incluso no `DashboardResponse` |
| Quests / Goals (diário ou semanal) | **REQUIRES PRODUCT/DOMAIN WORK** — não existe `Quest`/`Goal` no Domain (ver Seção 14); não deve ser simulado com dado decorativo |
| Streak | **REQUIRES PRODUCT/DOMAIN WORK** — não existe conceito de continuidade consecutiva no Domain; o ícone `Streak` já existe no `PixelIconRegistry` mas sem lógica por trás |
| Achievements/Badges | **REQUIRES PRODUCT/DOMAIN WORK** — não existe no Domain |
| Promo (upsell) | Sem equivalente de produto no BeeDay — não aplicável, não incluir |

**Consequência direta para o escopo de Sprint 21.6/21.7:** a rail direita pode nascer honesta com
XP/Nível + resumos de Habits/Tasks/Projects/Wallet. Qualquer card de "meta"/"quest"/"streak" exige
uma decisão de produto anterior — ver Seção 19.

---

## 13. Responsive Specification

**Fonte Lingo:** grep de `sm:`/`md:`/`lg:`/`xl:` nos arquivos de shell/componentes lidos (ver
relatório completo de extração, citações por arquivo já registradas nas seções anteriores).

- Breakpoints Tailwind padrão (`sm`640/`md`768/`lg`1024/`xl`1280/`2xl`1400 — este último só no
  `container` do `tailwind.config.ts:18`, não usado no shell).
- **`lg:`(1024px) é o único breakpoint estrutural** — sidebar, header mobile, offset do main
  content e visibilidade da rail direita pivotam todos nele. Não existe nenhum ajuste de shell em
  `md:`(768px) nos arquivos lidos.
- `sm:`(640px) é usado só para ajustes cosméticos menores (radius de dialog, largura de sheet,
  direção de footer) — nunca para reestruturar o shell.
- `xl:`(1280px) aparece uma única vez, cosmético (visibilidade do botão "Continue" do UnitBanner).
- A rail direita **desaparece completamente** abaixo de `lg:` — não reflow para dentro do feed, só
  `hidden`.

**Fonte BeeDay:** ver Seção "Breakpoints" da extração de dados brutos (30 regras `@media` com
valor literal `max-width`/`max-height` encontradas apenas no subconjunto de arquivos lido para
esta Sprint — 18 valores distintos, unidades mistas `rem`/`px`, nenhum token de breakpoint
compartilhado em `variables.css`).

### Recomendação

O BeeDay deve considerar consolidar breakpoints como parte da EPIC 21 (candidato natural: Sprint
21.11 — Responsive & Mobile Experience), já que o próprio shell alvo do Lingo é estruturalmente
mais simples (um breakpoint dominante) do que o modelo atual do BeeDay (três breakpoints só no
shell: 760px/920px/680px). **Não fazer essa refatoração nesta Sprint** — só registrar a
recomendação, como pedido explicitamente pelo escopo da Sprint 21.1.

---

## 14. Cores

Paleta desta EPIC: `#1023C8` / `#1E33ED` / `#0C1B99` / `#FACF39` / `#FBDB6B` / `#FFFFFF`. Sem
escala cromática extensa — cores sólidas, mapeadas por **função**, não por substituição direta de
matiz.

### Função das cores no Lingo (fonte: extração de `button.tsx`, `mobile-header.tsx`,
`unit-banner.tsx`, `progress.tsx`, `sidebar-item.tsx`)

| Cor Lingo | Função real observada |
|---|---|
| `green-500`/`green-600` | Ação primária/de continuidade **na prática** — header mobile, banner de unidade, preenchimento de progress bar, botão `secondary`, cor do wordmark. É a cor mais prevalente do produto, apesar do nome de variante ser `secondary`. |
| `sky-400`/`sky-500` | Rotulada `primary` no código, e usada para item de navegação ativo — mas visualmente menos dominante que o verde. |
| `rose-500`/`rose-600` | Danger — exclusivamente ações destrutivas/negativas. |
| `indigo-500`/`indigo-600` | "Super"/premium — uma ênfase secundária distinta da ação primária. |
| `neutral-200`/`neutral-400`, `slate-200`/`slate-500` | Locked/disabled/bordas neutras — nunca carrega significado de marca. |
| `orange-500` | Acento quente — usado especificamente para pontos/XP. |
| `black/80` | Overlay de modal — não faz parte da paleta de marca. |

### Proposta de equivalente BeeDay (por função, não por matiz)

| Função | Cor BeeDay | Justificativa |
|---|---|---|
| Ação primária / marca / continuidade (papel do verde do Lingo) | `#1023C8` (base) / `#1E33ED` (hover) / `#0C1B99` (profundidade/pressed) | O BeeDay não tem dois tons de marca como o Lingo (verde dominante + azul "primary" nominal) — unifica os dois papéis em um único azul, com o navy `#0C1B99` assumindo o papel do `border-b-600` mais escuro do Lingo (mecanismo de profundidade do botão) |
| Ênfase secundária/premium (papel do indigo "super") | `#FACF39` (base) / `#FBDB6B` (hover/claro) | Reaproveita o acento quente que o BeeDay já reserva para XP/recompensa (`--beeday-game-yellow` já existe e já é usado no preenchimento da `ExperienceBar`) — mantém consistência com o que já existe, em vez de introduzir um terceiro tom |
| Danger | Tokens de status já existentes (`--beeday-color-danger` etc.) | Fora da paleta sólida de 6 cores por necessidade funcional explícita — permitido pela Seção 6 do contexto da EPIC |
| Locked/disabled/neutro | Tokens de conteúdo/borda já existentes (`--beeday-color-text-muted`, `--beeday-color-border`) | Nenhuma cor nova necessária |

**Registro obrigatório (Seção 14 do contexto da EPIC):** a família `--beeday-color-brand-primary`
(`#2538d2`/`-hover #1d2fb8`/`-active #16268c`/`-light #4458dc`/`-soft #e6e9fb`) foi introduzida na
Sprint 20.6 e consolidada na Sprint 20.7 (remoção do antigo `--beeday-color-primary` roxo,
`#673ab7`) — **não é um token inexistente a criar, é um token recente a remigrar**. A lista de
consumidores já foi levantada pela própria EPIC 20 (`PublicHeader`, `Home.razor`, focus ring,
`TopNavigation`, `MainLayout`, `AccountSidePanel`/`ProfileSidePanel`) — a remigração para
`#1023C8`/`#1E33ED`/`#0C1B99` deve reusar essa lista, não redescobri-la.

---

## 15. Gamification Capability Matrix

**Fonte:** leitura direta de `src/BeeDay.Domain/Entities/`, `src/BeeDay.Domain/Experience/`,
`src/BeeDay.Application/Common/Experience/`, `src/BeeDay.Application/Features/Dashboard/`,
`src/BeeDay.Application/Features/Wallets/`, busca textual por `streak`/`achievement`/`badge`/
`dailygoal`/`weeklygoal` em `src/BeeDay.Domain`, `src/BeeDay.Application`, `src/BeeDay.Infrastructure`.

| Capability | Status | Evidência |
|---|---|---|
| XP | **SUPPORTED** | `User.Experience` (`UserExperience` value object), `ExperienceEntry` (histórico auditável), `ExperienceRewardPolicy` (Habit +1/Task +5/Todo +7/Project +20), `ExperienceRewardService.Grant` |
| Level | **SUPPORTED** | `IExperienceCurve`/`LinearExperienceCurve`, `UserExperience.CurrentLevel`/`ExperienceForNextLevel`, evento `UserLeveledUpDomainEvent` |
| Streak | **NOT SUPPORTED** | Busca textual sem resultado em Domain/Application; confirmado como gap conhecido pelo próprio discovery da EPIC 20 (`docs/epics/20-home-visual-experience/README.md`, Sprint 20.1: "não confirmado") |
| Achievements/Badges | **NOT SUPPORTED** | Único hit é `ActivityAttributeBadge.razor` (chip de categoria de atividade, não conquista desbloqueável) |
| Daily Goals | **NOT SUPPORTED** | `HabitResetCounter.Daily`/`TaskRepeat.Daily` são cadência de reset por item, não meta agregada do usuário |
| Weekly Goals | **NOT SUPPORTED** | Mesmo padrão do item acima, sem meta agregada |
| Habit Progress | **PARTIAL** | `Habit.PositiveCount`/`NegativeCount` rastreiam contagem; não existe conceito de meta/percentual de conclusão por hábito |
| Task Completion | **SUPPORTED** | `RecurringTask.Completed`/`ToggleCompletion()`, XP concedido por conclusão |
| Project Progress | **SUPPORTED** | `Project.ProgressPercentage` calculado a partir de `Todo`s reais (`CompletedTodos*100/TotalTodos`), `Status` derivado |
| Completion Feedback | **PARTIAL** | Level-up: `BeeDayFeedbackModal` real, disparado por `UserLeveledUpDomainEvent` via MediatR; toast "+N XP" inline no `ExperienceBar`. Sem feedback de streak-break (não há streak) ou de achievement (não há achievement) |

**Entidades reais de domínio confirmadas** (vocabulário real do produto, não terminologia
Duolingo): `User`, `Profile`, `Activity` (base abstrata), `Habit`, `RecurringTask` ("Task" na UI),
`Project`, `Todo`, `Wallet`/`Transaction`/`WalletTag`, `UserExperience`/`ExperienceEntry`.

`Wallet` é rastreamento financeiro real (saldo/receita/despesa em `decimal`) — **não é** um sistema
de moeda/pontos de gamificação e não deve ser tratado como tal em nenhum design de rail direita.

---

## 16. Component Mapping

| Lingo | Responsabilidade | Equivalente BeeDay | Decisão | Sprint alvo |
|---|---|---|---|---|
| `app/(main)/layout.tsx` (shell) | Composição geral do shell autenticado | `MainLayout.razor` (grid de 3 colunas, hoje 1.35rem/1fr/1.35rem colapsado) | **REFACTOR** — paradigma estrutural diferente (top-nav + 2 drawers vs. sidebar+rail persistentes), não é reskin | 21.2 |
| `components/sidebar.tsx` | Navegação primária persistente | Nenhum — `TopNavigation` é barra superior, não sidebar | **NEW** | 21.3 |
| `components/sidebar-item.tsx` | Item de nav (ativo/inativo) | `.top-navigation__links a` (padrão de link de nav existente) | **EVOLVE** — reaproveitar padrão de estado ativo/hover, não duplicar do zero | 21.3 |
| `components/mobile-header.tsx` | Chrome mobile mínimo (50px) | `TopNavigation` já colapsa em `680px` (esconde links) | **EVOLVE** — comportamento mobile já existe conceitualmente, ajustar altura/conteúdo | 21.3 / 21.11 |
| `components/mobile-sidebar.tsx` (Sheet) | Drawer de navegação mobile | `ProfileSidePanel`/`AccountSidePanel` (mecanismo de `translateX` + overlay fixo <760px já implementado) | **EVOLVE** — mecânica de transição diretamente reaproveitável | 21.3 / 21.11 |
| `components/feed-wrapper.tsx` | Coluna central de conteúdo | `.beeday-content-shell` (coluna central do `MainLayout`) | **EVOLVE** — já é a coluna central conceitual, ajustar max-width/gap | 21.2 |
| `components/sticky-wrapper.tsx` | Rail direita persistente (368px) | Nenhum — `AccountSidePanel` é overlay sob demanda, não rail sempre visível | **NEW** | 21.6 |
| `components/user-progress.tsx` | Pill compacta de XP/nível/vidas | `ExperienceBar.razor` (dado real, hoje só no Profile side-drawer) | **EVOLVE** — componente e dado já existem, precisa de relocação/reestilização | 21.6 / 21.7 |
| `components/quests.tsx` | Lista de metas com progresso | Nenhum — sem entidade `Quest`/`Goal` no Domain | **NEW**, bloqueado por decisão de produto/domínio (ver Seção 15) | Não agendado — depende de escopo de produto |
| `components/ui/button.tsx` | Controle interativo primário | `BeeDayButton.razor` | **EVOLVE** — arquitetura correta, geometria base migra para o mecanismo de borda-inferior; modificadores `--comic*` deprecados incrementalmente | 21.5 |
| Cards (`promo.tsx`/`quests.tsx`) | Superfície de conteúdo genérica | `BeeDayCard.razor` | **EVOLVE** — já é o alvo canônico (Sprint 20.8), precisa reverter para borda fina/sem sombra em vez de borderless/shadow-lg | 21.5 |
| `components/ui/progress.tsx` (Radix) | Barra de progresso linear | Markup inline em `ExperienceBar.razor` (`__track`/`__fill`), não é primitive reutilizável | **REFACTOR** — extrair um `BeeDayProgressBar` reutilizável a partir do markup existente | 21.6 |
| `components/ui/dialog.tsx` (Radix) | Superfície modal | `EditorModalShell.razor` + `BeeDayConfirmDialog.razor` (duas implementações distintas hoje) | **EVOLVE** — convergir linguagem visual (radius/backdrop/animação) entre os dois, sem fundir seus contratos comportamentais | 21.5 |
| `components/ui/sheet.tsx` (Radix, drawer genérico) | Primitive de drawer lateral | Padrão de CSS duplicado em `ProfileSidePanel`/`AccountSidePanel` | **REFACTOR** — extrair um primitive de drawer compartilhado para a futura sidebar mobile reutilizar | 21.3 |
| Auth pages (`app/(auth)/`) | Apresentação de login | `Login.razor` (comportamento preservado) | **EVOLVE** — só geometria do `.auth-card` | 21.5 (ou Sprint dedicada) |
| Ícones (lucide + SVG solto) | Iconografia | `PixelIcon.razor` + `PixelIconRegistry` | **KEEP ARCHITECTURE / REPLACE VISUAL SOURCE** | 21.4 |
| `unit-banner.tsx` | Banner de seção/contexto | `BeeDaySectionHeader.razor`/`BeeDayPageHeader.razor` | **EVOLVE** | 21.7+ |

Legenda usada: `REUSE` (nenhum equivalente aplicável nesta leitura — nenhum componente do BeeDay
foi encontrado pronto para reuso direto sem qualquer ajuste), `EVOLVE`, `REFACTOR`, `REPLACE`,
`NEW`, `DO NOT PORT` (nenhum item classificado assim nesta Sprint — nada do Lingo foi descartado
como inaplicável).

---

## 17. Visual Debt Map

O que precisa ser removido ou migrado ao longo da EPIC, em ordem aproximada de raio de impacto:

1. **Linguagem "comic" de botão** (`--comic*`, `--skew-press`, `--comic-press`,
   `variables.css:159-212`) — 40+ consumidores incluindo `Login.razor`, `EditorModalShell`,
   `BeeDayConfirmDialog`, Wallet, Identity, Account. Migração multi-Sprint, consumidor por
   consumidor (ver Seção 6).
2. **`Jersey 25`** (`--beeday-font-ui`) — decisão de marca pendente (Seção 5), não débito
   acidental; impacto em `BeeDayButton`, títulos, nav ativo, `BeeDayBrand`.
3. **`pixel-nes.css`/remanescente NES.css** (`--pixel-cta`, `.beeday-pixel-panel/-cta`) — 1
   consumidor real (botão Continue do modal Level-Up), baixo custo de remoção assim que a
   geometria padrão do `BeeDayButton` absorver a linguagem do Lingo.
4. **`--beeday-color-brand-primary`** (`#2538d2` etc.) — remigração para `#1023C8` etc., lista de
   consumidores já conhecida (Seção 14).
5. **Paleta "comic"** (8 modificadores de cor) e bloco de tokens "game"/pixel-console
   (`variables.css:270-296`) — peso morto assim que os botões comic forem descontinuados; remover
   junto.
6. **Três escalas de spacing paralelas** (`--beeday-spacing-*` em `variables.css`, escala "grid"
   em `polish.css`, escala "activity" em `activity-design-system.css`) e conflitos de cascata já
   documentados (ex.: `polish.css` sobrescreve silenciosamente `min-height`/`padding` de
   `.beeday-button` declarados em `design-system.css`, por ordem de carregamento, não por
   especificidade) — débito preexistente, autodocumentado, vale consolidar já que a geometria de
   botão/card está sendo tocada de qualquer forma.
7. **Regra `--interactive` de card duplicada** (`design-system.css` vs. `pixel-ui.css`, timings
   diferentes) — mesma fragilidade de ordem de carregamento do item 6.
8. **18 valores de breakpoint hardcoded distintos** (só no subconjunto de arquivos lido nesta
   Sprint) vs. o breakpoint único dominante do Lingo — candidato a consolidação em 21.11.
9. **Variante `reference-blue`** (`variables.css:118-124`, já rotulada "legacy/compat" no próprio
   token) — confirmar consumidores vivos antes de qualquer mudança de geometria de botão a
   afetar.

**O cursor customizado não entra nesta lista** — já removido na Sprint 20.3 (EPIC 20), confirmado
nesta Sprint sem nenhum remanescente encontrado.

---

## 18. Target BeeDay Web Architecture

Proposta de composição visual alvo (camada Web apenas — nenhuma mudança de Domain/Application
implícita além do que já está registrado como bloqueado na Seção 15):

```text
Desktop (≥1024px — breakpoint único, alinhado ao Lingo)

┌────────────────┬─────────────────────────────────┬────────────────────┐
│  Sidebar (NEW)   │        Main Feed (EVOLVE)          │  Right Rail (NEW)   │
│  ~256px, fixa    │  centro, max-width ~1056px,        │  ~368px, sticky      │
│  Home/Habits/    │  reaproveita .beeday-content-shell │  ┌────────────────┐ │
│  Tasks/Projects/ │                                     │  │ ExperienceBar   │ │ EVOLVE
│  Wallet          │                                     │  │ (XP/Level)      │ │ (relocado)
│                  │                                     │  └────────────────┘ │
│                  │                                     │  ┌────────────────┐ │
│                  │                                     │  │ Habits/Tasks/   │ │ NEW
│                  │                                     │  │ Projects/Wallet │ │ (dado já
│                  │                                     │  │ resumo          │ │  existe)
│                  │                                     │  └────────────────┘ │
│                  │                                     │  [Goals/Quests —    │ │ bloqueado,
│                  │                                     │   pendente decisão  │ │ ver Seção 19
│                  │                                     │   de produto]       │ │
└────────────────┴─────────────────────────────────┴────────────────────┘

Mobile (<1024px)

┌───────────────────────────────────────────────────────────────┐
│ Header mobile fixo — hamburger abre drawer (EVOLVE do mecanismo  │
│ translateX já existente em ProfileSidePanel/AccountSidePanel)    │
├───────────────────────────────────────────────────────────────┤
│ Main Feed apenas — Right Rail desaparece por completo (hidden,   │
│ sem reflow), igual ao comportamento real do Lingo                │
└───────────────────────────────────────────────────────────────┘
```

Esta é uma proposta estrutural para orientar as Sprints 21.2–21.7 — nenhuma parte foi
implementada nesta Sprint.

---

## 19. Risks

1. **Colisão de remigração de token de marca.** A EPIC 20 já migrou `--beeday-color-brand-primary`
   uma vez (Sprint 20.6/20.7) contra uma referência diferente. Fazer isso de novo sem uma
   migração limpa e única corre o risco de tocar a mesma lista de consumidores duas vezes em duas
   EPICs consecutivas — deve ser uma mudança isolada e completa, não incremental/deriva.
2. **Ambição da rail direita sem lastro de domínio.** Streak/Achievements/Quests/Goals não têm
   entidade no Domain. Construir esses cards de forma decorativa violaria diretamente a Seção 10
   do contexto da EPIC ("Funcionalidade inexistente não deve ser simulada apenas para preencher um
   card"). Precisa de decisão de produto explícita antes de qualquer Sprint tocar esses cards.
3. **Raio de impacto amplo da migração do botão comic.** 40+ consumidores fora do
   `Components/DesignSystem` (Login, editores, Wallet, Identity, Account) — migração de
   consumidor único por vez ao longo de múltiplas Sprints, não um único flip de Sprint.
4. **Fragilidade de cascata CSS.** Arquivos carregados depois (`polish.css`) já sobrescrevem
   silenciosamente valores declarados em `design-system.css` por ordem de carregamento, não por
   especificidade — mudar geometria de botão/card sem entender essa cadeia pode produzir valores
   que não batem com o que está "documentado" na declaração original.
5. **Baseline da EPIC 20 ainda não é "Environment Validated".** `docs/epics/20-home-visual-experience/README.md`
   registra "READY FOR HMG VISUAL VALIDATION, not COMPLETE" — EPIC 21 constrói sobre uma base cujo
   estado em produção/homologação real ainda não foi confirmado.
6. **Retirada de Jersey 25 é decisão de marca, não só técnica.** Precisa de aprovação explícita do
   responsável pelo produto antes de qualquer Sprint de implementação tocar tipografia de botão/
   título — ver Seção 5.

---

## 20. Decisions Required

Decisões que bloqueiam o escopo de Sprints futuras e não foram tomadas nesta Sprint (spec-only):

1. **Jersey 25 — `KEEP` / `LIMIT TO BRAND` / `RETIRE`?** Recomendação técnica: `RETIRE` é a leitura
   mais fiel ao Lingo real (nem o wordmark do Lingo usa fonte separada); `LIMIT TO BRAND` é o
   compromisso que preserva a identidade de marca já documentada como deliberada. Ver Seção 5 para
   impacto e consumidores de cada opção.
2. **Ícones — confirmar `KEEP ARCHITECTURE / REPLACE VISUAL SOURCE`?** Recomendação técnica: sim
   (Seção 10) — pedindo confirmação porque envolve trocar o conteúdo de um sprite usado em 61
   pontos do produto.
3. **Escopo da rail direita para 21.6/21.7 — aceitar lançar só com o que é `SUPPORTED NOW`?**
   (XP/Nível + resumos de Habits/Tasks/Projects/Wallet), deixando Quests/Goals/Streak
   explicitamente fora até haver decisão de domínio (Seção 12/15).
4. **Estratégia de migração do botão comic — confirmar depreciação incremental** (novo padrão vira
   base da classe, modificadores `--comic*` marcados deprecated mas não removidos até cada
   consumidor migrar) **em vez de um flip único de Sprint?** (Seção 6).
5. **Remigração do token de marca — confirmar que substitui limpamente a família `#2538d2`** da
   EPIC 20 pela família `#1023C8` desta EPIC, reusando a lista de consumidores já conhecida, em vez
   de tratar como token novo? (Seção 14).
6. **Consolidação de breakpoints — dentro do escopo da EPIC 21 (Sprint 21.11) ou tratada como
   débito técnico separado, fora da EPIC?** (Seção 13).

---

## 21. Recommended Scope for Sprint 21.2

Recomendação, não decisão — a Sprint 21.2 só será definida após revisão deste documento.

- Introduzir o scaffold estrutural do novo shell de 3 regiões (Sidebar / Main Feed / Right Rail)
  em `MainLayout`, usando as larguras reais do Lingo (`256px`/`max-width:1056px`/`368px`) adaptadas
  à escala de tokens do BeeDay — **scaffold estrutural apenas**, sem o visual final da Sidebar
  (21.3) nem o conteúdo da Right Rail (21.6).
- Avaliar se a remigração de `--beeday-color-brand-primary` (`#2538d2` → `#1023C8` etc.) deve
  entrar nesta Sprint ou ser adiantada para uma Sprint isolada anterior — é uma mudança pequena e
  já mapeada, e bloqueia comparação visual precisa em todas as Sprints seguintes se ficar pendente.
- **Não tocar** `BeeDayButton`, `Login`, ícones, ou conteúdo da Home nesta Sprint — cada um tem
  Sprint dedicada (21.4/21.5/21.7).

---

## Fontes consultadas nesta Sprint

- Lingo (READ-ONLY, `C:\DevOps\Projects\duolingo-clone`): `package.json`, `tailwind.config.ts`,
  `app/globals.css`, `app/layout.tsx`, `components/ui/{button,dialog,sheet,progress,avatar,
  separator,sonner}.tsx`, `components/{sidebar,sidebar-item,mobile-header,mobile-sidebar,
  feed-wrapper,sticky-wrapper,promo,quests,banner}.tsx`, `app/(main)/layout.tsx`,
  `app/(main)/learn/{page,header,unit,unit-banner,lesson-button}.tsx`, `app/(auth)/{layout,
  header,sign-in/[[...sign-in]]/page}.tsx`, `app/(marketing)/{header,page}.tsx`,
  `components/modals/{exit-modal,hearts-modal,practice-modal}.tsx`.
- BeeDay: `wwwroot/css/{variables,typography,typography-policy,design-system,cards,feedback,
  forms,utilities,pixel-ui,pixel-nes,polish,editor-modal}.css`, `Components/DesignSystem/**`,
  `Components/Layout/{MainLayout,TopNavigation,ProfileSidePanel,AccountSidePanel,AppFooter,
  OnboardingLayout,PublicLayout,PublicHeader}.razor(.css)`, `Components/Features/Authentication/
  Pages/Login.razor(.css)`, `Components/Features/Home/Pages/Home.razor(.css)`,
  `Components/Features/Experience/Components/ExperienceBar.razor(.css)`, `src/BeeDay.Domain/
  Entities/*`, `src/BeeDay.Domain/Experience/*`, `src/BeeDay.Application/Common/Experience/*`,
  `src/BeeDay.Application/Features/{Dashboard,Wallets,Users}/*`, `docs/epics/
  20-home-visual-experience/README.md`, `docs/design-system/*`.

Nenhum arquivo do Lingo foi criado, editado ou commitado. Nenhum arquivo de código de produção do
BeeDay foi alterado na Sprint 21.1 (ver "Sprint 21.2 — Results" abaixo para a primeira Sprint que
altera `src/BeeDay.Web/`).

---

## Sprint 21.2 — BeeDay Shell Foundation — Results

**Branch:** `sprint/21.2-beeday-shell-foundation` (criada a partir de `hmg` já sincronizado com a
Sprint 21.1 mergeada).

**Status:** COMPLETE — primeira Sprint de implementação da EPIC 21. Fundação estrutural do shell
estabelecida; nenhum acabamento visual final (navegação, ícones, conteúdo da Right Rail) foi
tentado, por escopo explícito da Sprint.

### Shell implementado

Estrutura DOM de `MainLayout` (autenticado) evoluiu de:

```text
TopNavigation (fixo, todas as larguras)
  → beeday-workspace (grid 3 colunas: ProfileSidePanel | conteúdo | AccountSidePanel)
  → BeeDayToastHost
```

para:

```text
TopNavigation (fixo, visível só abaixo de 1024px)
  → .beeday-shell (linha flex)
      → DesktopSidebar (fixo, visível só a partir de 1024px)
      → beeday-workspace (grid 3 colunas, inalterado: ProfileSidePanel | conteúdo | AccountSidePanel)
      → RightRail (visível só a partir de 1024px)
  → BeeDayToastHost
```

`1024px` é o mesmo breakpoint estrutural único documentado para o Lingo na Sprint 21.1 (§3/§13) —
`TopNavigation` e `DesktopSidebar`/`RightRail` nunca ficam visíveis ao mesmo tempo, eliminando o
risco de "dois shells desktop concorrentes" apontado no escopo da Sprint.

### Componentes criados

- `Components/Layout/DesktopSidebar.razor(.css)` — região persistente de navegação primária
  (256px/16rem, fixa, visível ≥1024px). Reaproveita, sem redesenho, os mesmos três gatilhos que
  `TopNavigation` já expunha: botão de marca (`<BeeDayBrand />`, abre/fecha `ProfileSidePanel`),
  `NavLink` para `/daily`/`/wallet`, botão de menu (abre/fecha `AccountSidePanel`). Design final de
  navegação (ícones por item, estados ativos elaborados, item set completo) deliberadamente **não**
  incluído — escopo da Sprint 21.3.
- `Components/Layout/RightRail.razor(.css)` — região estrutural vazia (368px/23rem, sticky, visível
  ≥1024px). Nenhum conteúdo, nenhum serviço injetado — existe só para provar a geometria. XP/Nível
  e resumos de Habits/Tasks/Projects/Wallet (já `SUPPORTED NOW` per a Gamification Capability
  Matrix da Sprint 21.1, §15) ficam para a Sprint 21.6; Quests/Streak/Achievements permanecem sem
  suporte de Domain e não foram simulados.

### Componentes modificados

- `Components/Layout/MainLayout.razor` — adiciona o wrapper `.beeday-shell` e os elementos
  `<DesktopSidebar>`/`<RightRail>`; `beeday-workspace` (painéis Profile/Account) preservado sem
  alteração de comportamento.
- `Components/Layout/MainLayout.razor.css` — três novas custom properties em `.beeday-app`
  (`--beeday-sidebar-width: 16rem`, `--beeday-right-rail-width: 23rem`,
  `--beeday-content-max-width: 66rem`), seguindo o padrão já existente de tokens de shell escopados
  (mesmo mecanismo de `--beeday-top-navigation-height`/`--beeday-left-panel-width`/
  `--beeday-right-panel-width` — nenhuma infraestrutura de tokens nova); `.beeday-app` redefine
  `--beeday-top-navigation-height` para `0px` a partir de `1024px` (cascata automática para
  `.beeday-workspace`/`.beeday-side-slot`, que já derivavam dessa variável); `.beeday-workspace`
  ganha `padding-left: var(--beeday-sidebar-width)` a partir de `1024px` para compensar
  `DesktopSidebar` (`position: fixed`, fora do fluxo — mesma técnica usada pelo próprio Lingo,
  `lg:pl-[256px]`); `.beeday-content-shell` ganha `max-width: var(--beeday-content-max-width)` +
  `margin-inline: auto`.
- `Components/Layout/TopNavigation.razor.css` — uma regra nova (`display: none` a partir de
  `1024px`); nenhuma outra alteração visual.

Nenhum componente de `Components/DesignSystem/` foi alterado — a fundação do shell não duplicou
nem tocou o Design System existente.

### Decisões tomadas

1. **Gatilhos de Profile/Account reaproveitados, não redesenhados.** `DesktopSidebar` dispara
   exatamente os mesmos dois `EventCallback`s (`ToggleProfilePanel`/`ToggleMenuPanel`) que
   `MainLayout` já passava para `TopNavigation` — nenhuma navegação fictícia foi criada para
   preencher a sidebar, conforme exigido pelo escopo da Sprint.
2. **`TopNavigation` preservado como fallback mobile explícito**, não removido — abaixo de `1024px`
   continua sendo o único acesso à navegação/aos painéis, papel agora documentado como transitório
   até a Sprint 21.3/21.11 (docs/web/03-layouts.md §4).
3. **Right Rail nasce genuinamente vazia**, sem card de XP/placeholder visual permanente — decisão
   explícita para não antecipar a Sprint 21.6 nem simular funcionalidade inexistente (Streak/Quests
   não têm suporte de Domain, confirmado na Sprint 21.1 §15).
4. **Larguras do shell viraram tokens escopados em `.beeday-app`**, não literais repetidos — `256px`
   (16rem), `368px` (23rem) e `1056px` (66rem) aparecem uma vez cada, no bloco de custom properties
   já existente, evoluindo a infraestrutura atual em vez de criar uma paralela.
5. **`--beeday-color-brand-primary` (`#2538d2` → `#1023C8`) não foi remigrado nesta Sprint** — a
   Sprint 21.1 §21 havia levantado adiantar essa remigração; ficou explicitamente para a Sprint de
   Visual Foundations (21.4), conforme o escopo desta Sprint (§13 do prompt da Sprint) determinou.
   Nenhum elemento novo introduzido nesta Sprint usa cor hardcoded fora do sistema de tokens.

### Comportamento desktop (≥1024px, validado)

`DesktopSidebar` (256px, fixa) e `RightRail` (368px, sticky, vazia) visíveis; `TopNavigation`
oculta; `.beeday-workspace` desloca-se `256px` para compensar a sidebar fixa; conteúdo principal
centralizado com `max-width: 1056px`.

### Comportamento responsivo (validado)

Abaixo de `1024px`: `DesktopSidebar`/`RightRail` ocultas, `TopNavigation` visível e funcional
exatamente como antes desta Sprint — nenhuma regressão. Entre `760px` e `1024px`, os painéis
Profile/Account já operam em modo coluna de grid (comportamento pré-existente, breakpoint
independente) enquanto `DesktopSidebar`/`RightRail` continuam ausentes — os dois breakpoints não se
coordenam entre si nessa faixa intermediária, registrado como observação para a Sprint 21.11
(Responsive & Mobile Experience), não corrigido aqui (fora do escopo desta Sprint).

### Compatibilidade temporária necessária

`TopNavigation` continua sendo renderizado e funcional em todas as larguras — apenas ocultado via
CSS acima de `1024px` — exatamente o mecanismo transitório e explícito autorizado pelo escopo da
Sprint (§8). Nenhuma outra compatibilidade temporária foi necessária: `ProfileSidePanel`,
`AccountSidePanel`, o formulário de logout e o `AppFooter` continuam inalterados e acessíveis.

### Responsabilidades deixadas explicitamente para Sprints futuras

- **Sprint 21.3 (BeeDay Navigation):** design final de `DesktopSidebar` — ícones por item, estados
  ativos, item set completo, navegação mobile definitiva (hoje ainda é o `TopNavigation` herdado).
- **Sprint 21.4 (Visual Foundations & Typography):** remigração de `--beeday-color-brand-primary`
  (`#2538d2` → `#1023C8`), decisão sobre `Jersey 25`, migração tipográfica global.
- **Sprint 21.5 (Interactive Components):** geometria física do `BeeDayButton`, migração dos
  consumidores `--comic*`.
- **Sprint 21.6 (Progress & Right Rail):** conteúdo real da `RightRail` (XP/Nível via
  `ExperienceBar` relocado, resumos de Habits/Tasks/Projects/Wallet via `DashboardResponse`).
- **Sprint 21.11 (Responsive & Mobile Experience):** consolidação de breakpoints (agora 30 valores
  distintos, ver `docs/ux/03-responsive.md`), incluindo a faixa `760px`-`1024px` sem coordenação
  entre os painéis existentes e o novo shell, registrada acima.

### Testes adicionados

- `tests/BeeDay.Web.Tests/Components/Layout/DesktopSidebarTests.cs` (3 testes, bUnit) — gatilhos,
  estados `aria-expanded`/`aria-label`, callbacks.
- `tests/BeeDay.Web.Tests/Components/Layout/RightRailTests.cs` (1 teste, bUnit) — região vazia,
  sem conteúdo simulado.
- `tests/BeeDay.Web.Tests/Components/Layout/ShellFoundationTests.cs` (6 testes, contrato
  texto/CSS) — composição do `MainLayout` sem remoção de região existente; visibilidade
  condicional `DesktopSidebar`/`RightRail`/`TopNavigation` no breakpoint `1024px`; recálculo de
  `--beeday-top-navigation-height`; `max-width` do conteúdo principal.
- `tests/BeeDay.E2E.Tests/ShellResponsiveLayoutTests.cs` (2 testes, Playwright/Chromium real) —
  geometria real (largura da sidebar ≈256px, da rail ≈368px), visibilidade condicional real,
  ausência de overflow horizontal real (`scrollWidth`/`clientWidth`), e que o gatilho de
  `ProfileSidePanel` continua funcional via `TopNavigation` em viewport estreito.

Nenhum teste pré-existente precisou de ajuste — os 49 testes de `Components.Layout`/
`Components.Visual` do `BeeDay.Web.Tests` e os 11 testes de `BeeDay.E2E.Tests` (9 pré-existentes +
2 novos) passaram sem modificação.

### Documentação atualizada

- `docs/web/03-layouts.md` — composição do shell reescrita (§2-§6); corrige dois achados
  desatualizados descobertos incidentalmente nesta Sprint (não introduzidos por ela): `TopNavigation`
  e `AccountSidePanel` já delegavam a `<BeeDayBrand />` desde a Sprint 20.7/20.4, mas esta seção
  ainda descrevia markup `BEE`/`DAY` literal para ambos.
- `docs/ux/03-responsive.md` — novo breakpoint `min-width: 1024px` (§2.2), contraexemplo de corte
  coordenado entre arquivos (§3), comportamento adaptativo de `MainLayout`/`TopNavigation`/
  `DesktopSidebar`/`RightRail` (§5); corrige uma inconsistência de contagem de arquivos
  pré-existente e independente desta Sprint entre o cabeçalho do documento e sua própria seção de
  fontes consultadas (29/48 vs. 30/49 — nenhum batia com a contagem direta; verdade agora é 33/52).
- `docs/design-system/01-foundations.md` §10 — novos tokens de shell, novo breakpoint, contagem de
  CSS isolado corrigida.
- Este documento (`docs/epics/21-lingo-product-experience/README.md`).

### Validação executada

- `dotnet format BeeDay.slnx --verify-no-changes` — sucesso.
- `dotnet build BeeDay.slnx` — 0 erros, 0 avisos.
- `dotnet test BeeDay.slnx` — todos os projetos, incluindo os 10 testes novos de
  `BeeDay.Web.Tests` e a suíte completa de `BeeDay.E2E.Tests` (Playwright/Chromium real, servidor
  Kestrel real, usuário seedado real via `SeedUserAsync`).
- `git status` — apenas os arquivos intencionais desta Sprint.

**Environment Validated, não apenas Code Complete:** a geometria responsiva do shell foi
confirmada em um Chromium real (`BeeDay.E2E.Tests`), não apenas em CSS lido estaticamente —
largura da sidebar (~256px) e da rail (~368px) medidas via `BoundingBoxAsync`, visibilidade
condicional e ausência de overflow horizontal confirmadas via `scrollWidth`/`clientWidth` reais,
em viewport desktop (1280×800) e mobile (390×844). Isso cobre o ambiente de teste E2E local — não
substitui validação em HMG, que segue pendente do fluxo normal de promoção do repositório.

### Riscos residuais

- A faixa `760px`-`1024px` (painéis em modo coluna de grid, mas `DesktopSidebar`/`RightRail` ainda
  ausentes) não foi projetada deliberadamente — é uma consequência de dois breakpoints
  independentes coexistindo. Não causa overflow nem quebra funcionalidade (validado por E2E), mas
  merece revisão de composição visual na Sprint 21.11.
- `DesktopSidebar` tem estilo intencionalmente neutro/mínimo — qualquer comparação visual lado a
  lado com o Lingo antes da Sprint 21.3 vai parecer incompleta por design, não por erro.

---

## Sprint 21.3 — BeeDay Navigation — Results

**Branch:** `sprint/21.3-beeday-navigation` (criada a partir de `hmg` já sincronizado com a Sprint
21.2 mergeada, PR #84).

**Status:** COMPLETE — segunda Sprint de implementação da EPIC 21. A `DesktopSidebar` estrutural da
Sprint 21.2 passa a ser a navegação real do produto; `TopNavigation` (o fallback mobile transitório
da Sprint 21.2) foi removida — não apenas desligada — porque suas responsabilidades foram
totalmente absorvidas.

### Inventário de rotas (base real da navegação)

Levantamento direto de todo `@page` sob `MainLayout` antes de desenhar a hierarquia (§4 do prompt
da Sprint): `/daily` (Dashboard — já concentra Habits/Tasks/Todos/Projects em colunas, não são
experiências próprias hoje), `/wallet`, `/account` e `/settings` (mesma página, `Account.razor`
com duas rotas). Não foram criadas `/habits`, `/tasks` ou `/projects` — essas continuam
inexistentes como rotas independentes, exatamente como o escopo da Sprint exigiu. Profile não é
uma rota — é acessado hoje via `ProfileSidePanel`, um drawer, não uma página.

### Componentes criados

- `Components/Layout/NavigationItem.razor(.css/.cs)` — primitivo de linha compartilhado entre
  desktop e mobile (uma única definição visual via isolamento de CSS do Blazor). Dois modos: rota
  (`NavLink`, computa `aria-current="page"` por conta própria — o `NavLink` do Blazor não expõe seu
  próprio estado ativo) ou ação (`button`, `aria-expanded` sempre como string literal `"true"`/
  `"false"`, não `bool` ligado direto ao atributo — Blazor trata esse caso como atributo HTML
  booleano presente/ausente, não a string ARIA esperada).
- `Components/Layout/NavigationItems.razor(.css)` — a lista real e atual de destinos (Daily/Wallet
  como rotas; Profile/Account como triggers de drawer), usada verbatim por `DesktopSidebar` e
  `MobileSidebar` — os dois nunca podem divergir porque compartilham a mesma definição.
- `Components/Layout/MobileHeader.razor(.css)` — substitui `TopNavigation` abaixo de `1024px`:
  marca (link para `/daily`) + um único botão hambúrguer que abre `MobileSidebar`.
- `Components/Layout/MobileSidebar.razor(.css/.cs)` — drawer overlay com backdrop, `Escape` fecha,
  botão de fechar dedicado, foco movido programaticamente para esse botão ao abrir
  (`ElementReference.FocusAsync()`, sem JS customizado), `aria-hidden`/`aria-expanded`/
  `aria-controls` corretos.

### Componentes modificados

- `Components/Layout/DesktopSidebar.razor(.css)` — brand vira `NavLink` para `/daily` (era um
  botão que só abria o Profile panel); navegação delegada a `<NavigationItems>`.
- `Components/Layout/MainLayout.razor` — troca `<TopNavigation>` por `<MobileHeader>` +
  `<MobileSidebar>`; novo estado `_isMobileNavOpen`.
- `Components/Layout/TopNavigation.razor(.css)` — **removidos** (`git rm`), não apenas
  desconectados do shell — busca repo-wide confirmou zero consumidores restantes antes da remoção.

### Decisões tomadas

1. **Perfil e Conta viram itens dedicados na navegação**, não mais sobrecarregados no botão de
   marca (Profile) e num botão de menu genérico (Account) — mais próximo da composição real do
   Lingo (logo separado do `UserButton`), sem inventar nenhuma funcionalidade nova: os mesmos dois
   drawers (`ProfileSidePanel`/`AccountSidePanel`) continuam sendo abertos pelos mesmos dois
   `EventCallback`s de sempre.
2. **`TopNavigation` foi deletada, não descontinuada silenciosamente** — Sprint 21.3 §17 exigia
   avaliar explicitamente se suas responsabilidades tinham sido totalmente absorvidas antes de
   remover; foram (marca/Profile → `MobileHeader`+drawer; Daily/Wallet → `NavigationItems`;
   Account/menu → `NavigationItems`), então o arquivo foi removido de fato.
3. **`ToggleMobileNav` não fecha mais Profile/Account ao abrir o drawer** — decisão corrigida
   durante a própria Sprint, não planejada desde o início: a primeira implementação fechava os
   dois painéis sempre que o hambúrguer era acionado (para evitar dois drawers ancorados à
   esquerda sobrepostos); isso **prendia** um painel já aberto permanentemente aberto em mobile,
   porque reabrir o hambúrguer era o único jeito de alcançar o botão "Close profile/support panel"
   de novo — bug real, pego por `BeeDay.E2E.Tests.ShellResponsiveLayoutTests`, não por revisão
   estática. A colisão visual (dois overlays de largura semelhante, mesmo lado) é resolvida pelo
   `z-index` mais alto do `MobileSidebar` (150 vs. 20), que simplesmente desenha por cima — não
   precisa de um bloqueio de estado.
4. **`aria-expanded`/`aria-hidden` renderizados como strings literais**, não `bool` ligado direto
   ao atributo — Blazor trata um valor `bool` como atributo HTML booleano (presente/ausente)
   independente do nome do atributo, o que já divergia da string `"true"`/`"false"` que a ARIA
   espera; padrão pré-existente em `DesktopSidebar`/`TopNavigation` (Sprint 21.2), corrigido nos
   componentes novos desta Sprint, não retroativamente nos já removidos/substituídos.
5. **Ícones**: nenhum novo — `PixelIconName.Daily`/`Wallet`/`Profile`/`Account`/`Menu`/`Close` já
   existiam no registry (`PixelIconRegistry.cs`) e cobrem exatamente os destinos reais; nenhuma
   biblioteca nova, nenhuma migração global (Sprint 21.3 §11).

### Comportamento desktop (≥1024px, validado)

`DesktopSidebar` (256px, fixa) mostra marca + Daily/Wallet (rotas reais, `aria-current="page"`
correto inclusive após navegação real e em deep link) + Profile/Account (triggers de drawer,
estado refletido em `aria-expanded`). `MobileHeader`/`MobileSidebar` ausentes.

### Comportamento responsivo (mobile <1024px, validado)

`MobileHeader` (marca + hambúrguer) sempre visível; hambúrguer abre `MobileSidebar` com a mesma
lista de destinos do desktop. Fechamento por: botão dedicado, clique no backdrop, `Escape`,
navegação por um item de rota. Foco move-se para o botão de fechar ao abrir (verificado com
Chromium real, não presumido). Logout continua acessível via `AccountSidePanel` (aberto pelo item
"Account"), inalterado.

### Testes adicionados

- `tests/BeeDay.Web.Tests/Components/Layout/NavigationItemTests.cs` (7 testes) — modo rota/ação,
  `aria-current` real por rota atual (incluindo prefixo/sub-rota), `aria-expanded` como string
  literal, `OnNavigate`.
- `tests/BeeDay.Web.Tests/Components/Layout/NavigationItemsTests.cs` (3 testes) — só destinos
  reais, sem links mortos; `OnNavigate` só dispara para itens de rota.
- `tests/BeeDay.Web.Tests/Components/Layout/MobileHeaderTests.cs` (4 testes) — marca, contrato
  ARIA aberto/fechado, callback do hambúrguer.
- `tests/BeeDay.Web.Tests/Components/Layout/MobileSidebarTests.cs` (8 testes) — `aria-hidden`
  real, fechar por botão/backdrop/Escape, navegação fecha o drawer, ativar Profile/Account dispara
  seu próprio callback.
- `tests/BeeDay.Web.Tests/Components/Layout/DesktopSidebarTests.cs` (reescrito, 6 testes) —
  agora cobre a navegação real, não mais os antigos botões de marca/menu genéricos.
- `tests/BeeDay.Web.Tests/Components/Layout/ShellFoundationTests.cs` (reescrito) — confirma que
  `TopNavigation.razor(.css)` não existe mais no repositório, além dos contratos de shell já
  cobertos na Sprint 21.2.
- `tests/BeeDay.E2E.Tests/NavigationTests.cs` (novo, 7 testes, Chromium real) — `aria-current` real
  ao clicar e em deep link; abrir/fechar o drawer por hambúrguer/backdrop/Escape/botão dedicado;
  foco real move para o drawer ao abrir; navegar fecha o drawer; Logout acessível.
- `tests/BeeDay.E2E.Tests/ShellResponsiveLayoutTests.cs` (atualizado) — troca `.top-navigation` por
  `.mobile-header`; o teste de acesso ao Profile panel em mobile passa a refletir o novo caminho
  (abrir hambúrguer → Profile) e verifica que reabrir o hambúrguer continua alcançando "Close
  profile panel" (prova, não presume, que o bug da decisão 3 acima está corrigido).

770 testes de `BeeDay.Web.Tests`/`BeeDay.E2E.Tests` existentes antes desta Sprint continuam
passando sem modificação além dos arquivos listados acima.

### Documentação atualizada

- `docs/web/03-layouts.md` — reescrita das seções de navegação (§3-§7); `TopNavigation` removida
  da narrativa.
- `docs/ux/03-responsive.md` — remoção do arquivo/breakpoints exclusivos de `TopNavigation`
  (`680px`; `920px` já não constava na tabela — lacuna pré-existente, não investigada
  retroativamente), atualização do grupo de arquivos que compartilham `min-width: 1024px` (4→5).
- `docs/design-system/01-foundations.md` §10 — mesma atualização de contagem/arquivos.
- `docs/design-system/02-components.md` — corrige uma afirmação sobre `BeeDayBrand` que já estava
  desatualizada antes desta Sprint (dizia que `TopNavigation`/`AccountSidePanel` não usavam o
  componente — ambos já usavam desde a Sprint 20.7/20.4).
- `docs/web/README.md` — árvore de `Components/Layout/` atualizada; achado histórico sobre marca
  duplicada marcado como totalmente resolvido (estava parcialmente desatualizado antes desta
  Sprint também).
- `docs/web/06-testing.md` — tabelas de mapeamento componente→teste atualizadas; contagem de
  classes/fluxos de E2E corrigida (`grep -c`, verificado diretamente — estava desatualizada desde
  antes da Sprint 21.2, que não tinha registrado `ShellResponsiveLayoutTests.cs` aqui).
- `docs/web/01-composition-root.md` — corrige a atribuição do form de logout (só `AccountSidePanel`
  o renderiza, nunca foi `TopNavigation`).
- `docs/ux/02-accessibility.md` — lista de arquivos com `prefers-reduced-motion` atualizada
  (também desatualizada desde a Sprint 21.2, que não tinha registrado `DesktopSidebar.razor.css`).
- Este documento (`docs/epics/21-lingo-product-experience/README.md`).

### Validação executada

- `dotnet format BeeDay.slnx --verify-no-changes` — sucesso.
- `dotnet build BeeDay.slnx` — 0 erros, 0 avisos.
- `dotnet test BeeDay.slnx` — todos os projetos, incluindo os testes novos/reescritos acima.
- `git status` — apenas os arquivos intencionais desta Sprint (mais a remoção deliberada de
  `TopNavigation.razor(.css)`).

**Environment Validated, não apenas Code Complete:** `aria-current` real por navegação e deep
link, geometria e visibilidade condicional reais, abertura/fechamento real do drawer por
hambúrguer/backdrop/Escape/botão, movimento real de foco de teclado ao abrir, e a correção do bug
de painel preso aberto — todos confirmados em Chromium real via `BeeDay.E2E.Tests`, não apenas
inferidos de CSS/markup estático. Cobre o ambiente de teste E2E local — não substitui validação em
HMG, que segue pendente do fluxo normal de promoção do repositório.

### Riscos residuais

- A faixa `760px`-`1024px` sem coordenação entre os breakpoints dos painéis existentes e do novo
  shell (registrada na Sprint 21.2) permanece — não piorada nem corrigida nesta Sprint, ainda para
  a Sprint 21.11.
- `MobileHeader`/`MobileSidebar` não implementam um focus-trap completo (Tab pode sair do drawer
  enquanto aberto) — mesmo patamar dos painéis Profile/Account pré-existentes, não uma regressão,
  mas também não uma melhoria; se um focus-trap completo for desejado no futuro, é trabalho novo,
  não coberto por esta Sprint.
- Estilo da navegação ainda é o definido na Sprint 21.3 diretamente a partir dos valores do Lingo
  (§6/§8 do prompt) — não passou pela consolidação tipográfica/cromática global da Sprint 21.4
  (`Jersey 25`, `--beeday-color-brand-primary`), então cores/tipografia atuais são as mesmas de
  antes da EPIC 21 aplicadas à nova geometria, não a paleta final `#1023C8` etc.

---

## Sprint 21.4 — Visual Foundations & Typography — Results

**Status:** COMPLETE — as foundations globais foram migradas para a direção visual da Epic 21 sem
criar um segundo Design System.

- A família existente `--beeday-color-brand-primary*` agora usa `#1023C8`, `#1E33ED`,
  `#0C1B99` e um soft funcional derivado; `--beeday-game-yellow` passou a `#FACF39` e o hover
  amarelo funcional a `#FBDB6B`. Nenhum namespace `lingo`/`epic21` foi criado.
- Cores semânticas de success/warning/danger/info e identidades de atividade foram preservadas.
- Background, superfícies, bordas, overlay, sombras e focus ring globais foram ajustados para
  fundos sólidos, bordas claras e profundidade controlada; a textura global foi removida.
- Nunito é a única família tipográfica carregada e consumida. Jersey 25, seu import e o token
  `--beeday-font-ui` foram removidos; `BeeDayBrand` preserva o mesmo contrato com apresentação
  Nunito 800.
- Os tokens compostos de display/title/subtitle/button agora expressam a hierarquia Nunito; o peso
  extrabold foi corrigido para 800 e `BeeDayButton` passou a Nunito 700. Sua geometria física
  continua reservada à Sprint 21.5.
- `VisualFoundationTests` (unitário/estrutural e E2E/Chromium) protege paleta, ausência de
  namespaces paralelos/Jersey, fundo sólido, Nunito real, foco e ausência de overflow nas
  superfícies públicas e autenticadas em desktop/mobile.
- A escala de radius foi consolidada para `3.2/6/10/12/16/24px`, mais pill/círculo; foram
  adicionados tokens globais mínimos de border-width (`2px`), physical depth (`2/4/8px`), focus em
  brand surfaces e visibility transition. Spacing foi preservado por já cobrir os contratos reais.
- Dívida comic/pixel, geometria de `BeeDayButton`, cards, ícones e breakpoints 760–1024px permanecem
  deliberadamente para as Sprints 21.5+; nenhum contrato público foi alterado e a RightRail segue vazia.

---

## Sprint 21.5 — Interactive Components — Results

**Status:** COMPLETE — `BeeDayButton` permanece a única primitive oficial e agora reproduz a
mecânica física do Lingo com identidade BeeDay.

- Base: 44px, compact 36px, radius 12px, border 2px + bottom depth 4px, Nunito 700 uppercase,
  hover por surface e pressed por colapso do depth + translateY(4px), sem sombra SaaS/comic.
- Variantes públicas foram preservadas. Primary usa a brand oficial; Secondary/Back/Cancel usam
  surface neutra; Success/Warning/Danger mantêm responsabilidade semântica.
- Loading preserva a largura pelo label invisível, mantém `aria-busy`/disabled e spinner central;
  disabled não recebe hover/pressed. FullWidth, Compact, Icon e AdditionalAttributes foram preservados.
- Classes `comic*`/`skew-press` continuam como aliases temporários para consumidores existentes,
  remapeados à geometria e paletas semânticas. Nenhum novo uso foi criado. `beeday-pixel-cta` foi removido.
- Inputs/TextArea/Select/Date/Checkbox oficiais foram consolidados em `forms.css`; Login passou a
  consumir a primitive visual e editors foram alinhados sem mudanças de composição/contrato.
- Icon toggles passaram a 40px, radius/timing globais; navegação mobile/desktop mantém seus contratos.
- Cards, Icon System, layouts de páginas e RightRail não foram alterados além dos controles contidos.
