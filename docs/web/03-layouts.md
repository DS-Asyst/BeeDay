# Layouts

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/Layout/`.

**Última verificação:** 2026-08-12 (Sprint 21.3, EPIC 21 — BeeDay Navigation) — `TopNavigation`
removida do repositório (responsabilidades totalmente absorvidas por `MobileHeader`/
`MobileSidebar`/`DesktopSidebar`, ver §3-§6); toda a navegação autenticada real (Daily/Wallet como
rotas, Profile/Account como triggers de drawer) passa a existir via os novos primitivos
`NavigationItem`/`NavigationItems`. Ver `docs/epics/21-lingo-product-experience/README.md` seção
"Sprint 21.3" para a especificação e as decisões da mudança. Verificação anterior: 2026-08-12
(Sprint 21.2) — introduziu `DesktopSidebar`/`RightRail` como regiões estruturais vazias, manteve
`TopNavigation` como fallback mobile transitório (papel agora encerrado, ver acima). Verificação
anterior: 2026-08-11 (Sprint 20.4) — §5/§6 corrigidos: o texto de marca `LEVEL`/`UP` em
`AccountSidePanel` e o link para o repositório `LevelUp` em `AppFooter` já não existem no código.

## 1. Objetivo

Descrever os dois `LayoutComponentBase` do repositório e os componentes de navegação/painéis que
`MainLayout` compõe.

## 2. Os dois layouts

| Layout | Usado por | Composição |
|---|---|---|
| `MainLayout.razor` | Todas as rotas autenticadas de produto (`/daily`, `/wallet`, `/account`, `/settings`, catálogos de Design System) e as duas páginas de erro/not-found | `MobileHeader`, `DesktopSidebar`, `ProfileSidePanel`, `AccountSidePanel`, `AppFooter`, `RightRail`, `MobileSidebar`, `BeeDayToastHost` |
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
- Controla três estados locais mutuamente relevantes: `_isProfilePanelOpen`, `_isMenuPanelOpen`,
  `_isMobileNavOpen`. `ToggleProfilePanel`/`ToggleMenuPanel` continuam mutuamente exclusivos entre
  si **e** fecham o drawer de navegação mobile (`_isMobileNavOpen = false`) — Profile/Account só
  são alcançáveis a partir de dentro do `MobileSidebar` em telas estreitas, então abri-los deve
  fechar o drawer que os continha (evita dois overlays ancorados à esquerda, de largura
  semelhante, sobrepostos). `ToggleMobileNav`, deliberadamente, **não** mexe no estado de
  Profile/Account — reabrir o hamburger enquanto um painel já está aberto é o único caminho para
  alcançar de novo o botão "Close profile/support panel" em mobile (não existe outro controle de
  fechamento nesses painéis); zerá-los ali prendia o painel aberto permanentemente, bug real pego
  por `BeeDay.E2E.Tests.ShellResponsiveLayoutTests` antes de chegar ao código promovido. A classe
  CSS do `beeday-workspace` (`has-left-panel`/`has-right-panel`) continua refletindo qual painel
  está aberto, controlando o grid via CSS — inalterado desde antes da Sprint 21.2.
- **Estrutura DOM (Sprint 21.3):** `MobileHeader` (fixo, visível só abaixo de `1024px`) →
  `.beeday-shell` (linha flex) → `DesktopSidebar` (fixo, visível só a partir de `1024px`) +
  `beeday-workspace` (grid de 3 colunas, inalterado: slot esquerdo = `ProfileSidePanel`, centro =
  `beeday-content-shell` com `@Body` + `AppFooter`, slot direito = `AccountSidePanel`) + `RightRail`
  (visível só a partir de `1024px`) → `MobileSidebar` (drawer overlay, visível só abaixo de
  `1024px`, fora do `.beeday-shell` — é um overlay de página inteira, não uma coluna) →
  `BeeDayToastHost`. `TopNavigation` existiu como fallback mobile transitório só durante a Sprint
  21.2; nesta Sprint foi removida junto com seu arquivo (`git rm`), não apenas desligada — sem
  consumidores restantes confirmado por busca repo-wide antes da remoção.

## 4. `MobileHeader.razor` (novo, Sprint 21.3 — substitui `TopNavigation`)

Header fixo simples, visível só abaixo de `1024px` (`display: none` a partir daí — nunca ativo ao
mesmo tempo que `DesktopSidebar`, ver §5). Duas regiões: link de marca (`<BeeDayBrand />`
envolvido num `NavLink` para `/daily`) e um único botão hambúrguer (`aria-expanded`/
`aria-controls="mobile-navigation"`, ícone `Menu`/`Close` conforme estado) que abre/fecha o
`MobileSidebar` (§6). Ao contrário da antiga `TopNavigation`, não expõe Daily/Wallet/Profile/Account
diretamente — todos os destinos vivem dentro do drawer, mesma composição que a `DesktopSidebar`
usa (`NavigationItems`, §6/§7).

## 5. `DesktopSidebar.razor` (Sprint 21.2 introduziu a região; Sprint 21.3 a torna navegação real)

Região persistente de navegação primária, visível apenas a partir de `1024px`. Composição:
`NavLink` de marca para `/daily` (`<BeeDayBrand />`) seguido de `<NavigationItems>` (§7) — a mesma
lista de destinos usada por `MobileSidebar`, garantindo que os dois nunca divirjam. Não possui mais
lógica própria de navegação (delegada a `NavigationItems`/`NavigationItem`) — só o container fixo
(`256px`/`16rem`, `position: fixed`) e o link de marca.

## 6. `MobileSidebar.razor` (novo, Sprint 21.3)

Drawer overlay acionado pelo hambúrguer da `MobileHeader`, visível só abaixo de `1024px`. Backdrop
(`.mobile-nav-backdrop`, fecha ao clicar) + painel (`<aside id="mobile-navigation">`, `width:
min(85vw, var(--beeday-left-panel-width))` — reaproveita o token de largura de drawer já existente
em vez de introduzir um literal novo). Contrato de acessibilidade:

- `aria-hidden` no painel reflete o estado real (string literal `"true"`/`"false"`, não presença/
  ausência do atributo — ver nota de implementação em `NavigationItem`, §7);
- `Escape` fecha (`@onkeydown` no próprio `<aside>`, mesmo padrão de `EditorModalShell`/
  `BeeDayConfirmDialog`);
- ao abrir, o foco move-se programaticamente para o botão de fechar dedicado
  (`ElementReference.FocusAsync()`, sem JS customizado) — verificado com Chromium real em
  `BeeDay.E2E.Tests.NavigationTests`, já que bUnit não tem teclado/foco real;
  `MobileHeader`/`MobileSidebar` não implementam um focus-trap completo (Tab não é impedido de sair
  do drawer) — mesmo patamar dos painéis Profile/Account pré-existentes, não uma regressão;
- fechar via transform (`translateX`) sozinho **não é suficiente** — um elemento deslocado para
  fora da tela continua com bounding box real, focável e detectado como "visível" por ferramentas
  de acessibilidade automatizadas; o CSS também alterna `visibility` (com o mesmo padrão de atraso
  de transição já usado pelo backdrop) para que o conteúdo realmente saia da árvore de
  acessibilidade quando fechado — bug real pego por `BeeDay.E2E.Tests` antes de chegar ao código
  promovido, não uma decisão de design original.

Renderiza `<NavigationItems OnNavigate="HandleNavigate">` — clicar em qualquer item de rota
(Daily/Wallet) fecha o drawer além de navegar (Sprint 21.3 §18 do prompt); ativar Profile/Account
fecha o drawer como efeito colateral de `MainLayout.ToggleProfilePanel`/`ToggleMenuPanel` (ver §3),
não via `OnNavigate`.

## 7. `NavigationItem.razor` / `NavigationItems.razor` (novos, Sprint 21.3)

`NavigationItem` é o primitivo de linha compartilhado por `DesktopSidebar` e `MobileSidebar` — uma
única definição de geometria/interação (`NavigationItem.razor.css`) usada nos dois contextos via
isolamento de CSS do Blazor, então nunca divergem visualmente. Dois modos, mutuamente exclusivos
por presença do parâmetro `Href`:

- **Modo rota** (`Href` definido): renderiza `<NavLink>`. Computa `aria-current="page"` **por
  conta própria** (`NavigationManager.LocationChanged` + comparação de path com a mesma semântica
  de `NavLinkMatch.Prefix`/`.All` que o `NavLink` já usa internamente) porque o `NavLink` do
  Blazor não expõe seu próprio estado "ativo" para o marcador filho — só sua classe CSS. Válido
  também para sub-rotas quando `Match="NavLinkMatch.Prefix"` (verificado em
  `NavigationItemTests.RouteMode_PrefixMatchStaysActiveOnSubRoutes`).
- **Modo ação** (`Href` nulo): renderiza `<button>` com `aria-label`/`aria-expanded` explícitos.
  `aria-expanded` é sempre uma string literal `"true"`/`"false"` computada em C#
  (`AriaExpandedValue`), não um `bool` ligado diretamente ao atributo — Blazor trata atributos
  ligados a um valor `bool` como atributo HTML booleano (presente/ausente), não como a string ARIA
  esperada; o mesmo padrão pré-existia em `DesktopSidebar`/`TopNavigation` antes desta Sprint e foi
  corrigido aqui, não retroativamente nos componentes já removidos/substituídos.

`NavigationItems` é a lista real e atual de destinos — **não** um catálogo de rotas futuras:

| Item | Tipo | Destino/ação |
|---|---|---|
| Daily | Rota | `/daily` |
| Wallet | Rota | `/wallet` |
| Profile | Ação | `OnToggleProfilePanel` (abre `ProfileSidePanel`) |
| Account | Ação | `OnToggleMenuPanel` (abre `AccountSidePanel`, que já contém o link para `/account`, Support, Logout, Donate — ver §8) |

Habits/Tasks/Projects **não** são itens próprios — hoje são colunas dentro de `/daily`
(`Dashboard/Pages/Home.razor`), não experiências independentes; criar rotas/itens de navegação
para eles seria inventar destinos que não existem, proibido explicitamente pelo escopo da Sprint
21.3. Ver `docs/epics/21-lingo-product-experience/README.md` seção "Sprint 21.3" para a
justificativa completa e o que fica para Sprints futuras.

## 8. Painéis laterais

### `ProfileSidePanel.razor`

Injeta `DashboardState` diretamente (único componente de Layout que depende de um state de
Feature) para mostrar avatar/nome/nickname e `ExperienceBar` do usuário atual. Assina
`DashboardState.Changed` no `OnInitializedAsync` e chama `State.InitializeAsync()` se
`State.Data` ainda for nulo — ou seja, pode disparar o próprio carregamento do Dashboard mesmo que
o usuário nunca tenha visitado `/daily` nesta sessão (abrir o painel em qualquer página sob
`MainLayout` já popula `DashboardState`). Estado vazio (`HasProfile == false`) linka para
`/profile/create`. Não redesenhado na Sprint 21.3 (fora de escopo) — só seu trigger mudou de lugar
(§5/§6/§7).

### `AccountSidePanel.razor`

Puramente estático/de navegação: liga para `/account` e para um `<form method="post"
action="/auth/logout">` com `<AntiforgeryToken />` — não injeta nenhum serviço, apenas
`[Parameter] IsOpen`/`OnClose`. Renderiza `<BeeDayBrand />` em `.support-drawer__brand-mark`. Não
redesenhado na Sprint 21.3 — só seu trigger mudou de lugar (§5/§6/§7); Logout continua acessível
por este mesmo formulário, inalterado.

## 9. `AppFooter.razor`

Estático, sem `@code`. Três colunas de links (`BeeDay`/`Developers`/`Social`) — a maioria são
placeholders (`href="#"`: News, Contact, Documentation, GitHub, Release Notes, Community, Privacy
Policy, Terms of Service) exceto os dois links de "Social" (LinkedIn, GitHub pessoal do mantenedor,
reais) e "About", que aponta para `https://github.com/tiagoarrigoni/BeeDay`. Conteúdo genérico o
suficiente para ser reutilizado sem alteração em contextos autenticados e públicos — reaproveitado
diretamente por `PublicLayout` na Sprint 20.4 (EPIC 20).

## 10. `ReconnectModal.razor`

Não é filho de nenhum `LayoutComponentBase` — é renderizado direto em `App.razor`, fora de
`<Routes>`, então cobre qualquer página independentemente do layout. Markup padrão do template
Blazor Web App (`.NET 10`) para o modal de "Rejoining the server..." exibido pelo cliente SignalR
(`blazor.web.js`) quando o circuito cai e tenta reconectar; carrega seu próprio módulo JS
(`ReconnectModal.razor.js`, não auditado nesta Sprint — é gerado/mantido pelo template, não
código de Feature). Não injeta nenhum serviço C#.

## 11. Fontes de verdade

- `src/BeeDay.Web/Components/Layout/MainLayout.razor(.css)`, `OnboardingLayout.razor`,
  `MobileHeader.razor(.css)`, `MobileSidebar.razor(.css/.cs)`, `DesktopSidebar.razor(.css)`,
  `NavigationItem.razor(.css/.cs)`, `NavigationItems.razor(.css)`, `ProfileSidePanel.razor`,
  `AccountSidePanel.razor`, `RightRail.razor(.css)`, `AppFooter.razor`, `ReconnectModal.razor`.
- `docs/epics/21-lingo-product-experience/README.md` (especificação Lingo → BeeDay que motivou as
  Sprints 21.2/21.3) e `docs/ux/03-responsive.md` (breakpoint `1024px`, cross-referenciado).
