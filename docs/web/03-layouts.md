# Layouts

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/Layout/`.

**Última verificação:** 2026-08-12 (Sprint 21.2, EPIC 21 — BeeDay Shell Foundation) — MainLayout
passa a compor `DesktopSidebar`/`RightRail`, novos; §4 corrigido (achado desatualizado desde a
Sprint 20.7, nunca registrado aqui: `TopNavigation` já delega a `<BeeDayBrand />`, não renderiza
`BEE`/`DAY` como spans literais próprios — o texto antigo desta seção nunca foi atualizado quando
essa migração aconteceu, embora `AccountSidePanelTests.cs` já assumisse o comportamento correto).
Ver `docs/epics/21-lingo-product-experience/README.md` seção "Sprint 21.2" para a especificação e
as decisões da mudança. Verificação anterior: 2026-08-11 (Sprint 20.4) — §5/§6 corrigidos: o texto
de marca `LEVEL`/`UP` em `AccountSidePanel` e o link para o repositório `LevelUp` em `AppFooter` já
não existem no código (achados desatualizados, corrigidos); demais seções preservadas da
verificação de 2026-08-07.

## 1. Objetivo

Descrever os dois `LayoutComponentBase` do repositório e os componentes de navegação/painéis que
`MainLayout` compõe.

## 2. Os dois layouts

| Layout | Usado por | Composição |
|---|---|---|
| `MainLayout.razor` | Todas as rotas autenticadas de produto (`/daily`, `/wallet`, `/account`, `/settings`, catálogos de Design System) e as duas páginas de erro/not-found | `TopNavigation`, `DesktopSidebar`, `ProfileSidePanel`, `AccountSidePanel`, `AppFooter`, `RightRail`, `BeeDayToastHost` |
| `OnboardingLayout.razor` | Todas as páginas de Authentication/Identity/Onboarding/ProfileCreation (9 rotas) | Apenas `<main>@Body</main>` + `BeeDayToastHost` — sem navegação |

`OnboardingLayout.razor` inteiro:

```razor
@inherits LayoutComponentBase
<div class="onboarding-layout"><main>@Body</main><BeeDayToastHost /></div>
```

Deliberadamente mínimo — cada página sob esse layout monta seu próprio card centralizado
(`auth-card`, `identity-card`, `tutorial-card`) e injeta `BeeDayBrand` individualmente.

## 3. `MainLayout`

```razor
@inject AuthenticatedUserInitializer AuthenticatedUserInitializer
@inherits LayoutComponentBase
```

- `OnInitializedAsync` chama `AuthenticatedUserInitializer.EnsureInitializedAsync()` **antes** de
  renderizar `@Body` — garante que qualquer página sob este layout só renderiza depois que a
  aplicação confirmou que o `UserId` do cookie ainda corresponde a um `User` real (mesma checagem
  que `Home.razor`/`Wallet.razor` repetem individualmente por segurança; ver
  [`04-feature-components.md`](04-feature-components.md)).
- Controla dois painéis laterais mutuamente exclusivos via dois `bool` locais
  (`_isProfilePanelOpen`, `_isMenuPanelOpen`) — abrir um sempre fecha o outro
  (`ToggleProfilePanel`/`ToggleMenuPanel`). A classe CSS do `beeday-workspace` (`has-left-panel`/
  `has-right-panel`) reflete qual está aberto, controlando o layout de grid via CSS. Os mesmos dois
  `bool`/callbacks são passados tanto a `TopNavigation` quanto a `DesktopSidebar` — só um dos dois
  componentes fica visível em cada largura (ver §4/§5), mas ambos disparam exatamente os mesmos
  handlers, então o estado dos painéis nunca diverge por causa de qual deles está na tela.
- **Estrutura DOM (Sprint 21.2, EPIC 21):** `TopNavigation` (fixo, visível só abaixo de `1024px`) →
  `.beeday-shell` (linha flex) → `DesktopSidebar` (fixo, visível só a partir de `1024px`) +
  `beeday-workspace` (grid de 3 colunas, inalterado desde antes desta Sprint: slot esquerdo =
  `ProfileSidePanel`, centro = `beeday-content-shell` com `@Body` + `AppFooter`, slot direito =
  `AccountSidePanel`) + `RightRail` (visível só a partir de `1024px`) → `BeeDayToastHost` (fora do
  shell, `position: fixed` via CSS). Antes desta Sprint, `TopNavigation` era a única navegação em
  qualquer largura e `beeday-workspace` era filho direto de `.beeday-app`; ver
  `docs/epics/21-lingo-product-experience/README.md` seção "Sprint 21.2" para a motivação completa.

## 4. `TopNavigation.razor`

Header fixo com 3 regiões: botão de marca (abre/fecha `ProfileSidePanel`, ícone
`ChevronLeft`/`ChevronRight` conforme estado, renderiza `<BeeDayBrand />` — não spans `BEE`/`DAY`
literais próprios; achado desatualizado desta seção, corrigido na Sprint 21.2), navegação central
(`NavLink` para `/daily` e `/wallet`, `Match="NavLinkMatch.Prefix"`), botão de menu (abre/fecha
`AccountSidePanel`, ícone `Close`/`Menu`). **Sprint 21.2:** `display: none` a partir de `1024px` —
`DesktopSidebar` (§5) assume a navegação primária nessa largura; abaixo de `1024px` continua sendo
o único acesso a ela, papel agora explicitamente transitório (`docs/epics/21-lingo-product-experience/README.md`
§8/§10).

## 5. `DesktopSidebar.razor` (novo, Sprint 21.2)

Região persistente de navegação primária, visível apenas a partir de `1024px` (`display: none`
abaixo disso — nunca ativa ao mesmo tempo que `TopNavigation`, ver §4). Reaproveita exatamente os
mesmos gatilhos que `TopNavigation` já expunha, apenas realocados: botão de marca (`<BeeDayBrand
/>`, abre/fecha `ProfileSidePanel`), `NavLink` para `/daily`/`/wallet`, botão de menu (abre/fecha
`AccountSidePanel`). Deliberadamente não recebe o design final de navegação (ícones por item,
estados ativos elaborados, item set completo) — isso é escopo da Sprint 21.3 (BeeDay Navigation);
ver `docs/epics/21-lingo-product-experience/README.md` §16 (Component Mapping).

## 6. `RightRail.razor` (novo, Sprint 21.2)

Região estrutural vazia, visível apenas a partir de `1024px`. Não injeta nenhum serviço, não
renderiza nenhum conteúdo — existe só para provar a geometria (largura `23rem`/368px, `position:
sticky`, offset de topo compartilhado com o resto do shell via `--beeday-top-navigation-height`).
Conteúdo real (XP/Nível, resumos de Habits/Tasks/Projects/Wallet) é escopo da Sprint 21.6 (Progress
& Right Rail); Quests/Streak/Achievements não têm suporte de Domain hoje e não devem ser simulados
aqui — ver `docs/epics/21-lingo-product-experience/README.md` §12/§15.

## 7. Painéis laterais

### `ProfileSidePanel.razor`

Injeta `DashboardState` diretamente (único componente de Layout que depende de um state de
Feature) para mostrar avatar/nome/nickname e `ExperienceBar` do usuário atual. Assina
`DashboardState.Changed` no `OnInitializedAsync` e chama `State.InitializeAsync()` se
`State.Data` ainda for nulo — ou seja, pode disparar o próprio carregamento do Dashboard mesmo que
o usuário nunca tenha visitado `/daily` nesta sessão (abrir o painel em qualquer página sob
`MainLayout` já popula `DashboardState`). Estado vazio (`HasProfile == false`) linka para
`/profile/create`.

### `AccountSidePanel.razor`

Puramente estático/de navegação: liga para `/account` e para um `<form method="post"
action="/auth/logout">` com `<AntiforgeryToken />` — não injeta nenhum serviço, apenas
`[Parameter] IsOpen`/`OnClose`. Renderiza `<BeeDayBrand />` em `.support-drawer__brand-mark`
(**corrigido na Sprint 21.2** — esta seção afirmava markup `BEE`/`DAY` próprio desde a Sprint 20.4,
mas o código já usa o componente compartilhado, confirmado tanto pela leitura direta do arquivo
quanto por `AccountSidePanelTests.RendersInstitutionalBrandingInsteadOfSocialMedia`, que já
verifica `.support-drawer__brand-mark .beeday-brand`; a mesma correção feita em §4 para
`TopNavigation` nunca tinha sido replicada aqui).

## 8. `AppFooter.razor`

Estático, sem `@code`. Três colunas de links (`BeeDay`/`Developers`/`Social`) — a maioria são
placeholders (`href="#"`: News, Contact, Documentation, GitHub, Release Notes, Community, Privacy
Policy, Terms of Service) exceto os dois links de "Social" (LinkedIn, GitHub pessoal do mantenedor,
reais) e "About", que aponta para `https://github.com/tiagoarrigoni/BeeDay` (corrigido; o achado
anterior de link para o repositório `LevelUp` estava desatualizado, ver `README.md`). Conteúdo
genérico o suficiente para ser reutilizado sem alteração em contextos autenticados e públicos —
reaproveitado diretamente por `PublicLayout` na Sprint 20.4 (EPIC 20).

## 9. `ReconnectModal.razor`

Não é filho de nenhum `LayoutComponentBase` — é renderizado direto em `App.razor`, fora de
`<Routes>`, então cobre qualquer página independentemente do layout. Markup padrão do template
Blazor Web App (`.NET 10`) para o modal de "Rejoining the server..." exibido pelo cliente SignalR
(`blazor.web.js`) quando o circuito cai e tenta reconectar; carrega seu próprio módulo JS
(`ReconnectModal.razor.js`, não auditado nesta Sprint — é gerado/mantido pelo template, não
código de Feature). Não injeta nenhum serviço C#.

## 10. Fontes de verdade

- `src/BeeDay.Web/Components/Layout/MainLayout.razor(.css)`, `OnboardingLayout.razor`,
  `TopNavigation.razor(.css)`, `DesktopSidebar.razor(.css)`, `ProfileSidePanel.razor`,
  `AccountSidePanel.razor`, `RightRail.razor(.css)`, `AppFooter.razor`, `ReconnectModal.razor`.
- `docs/epics/21-lingo-product-experience/README.md` (especificação Lingo → BeeDay que motivou a
  Sprint 21.2) e `docs/ux/03-responsive.md` (breakpoint `1024px`, cross-referenciado).
