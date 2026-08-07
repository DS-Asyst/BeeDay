# Layouts

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/Layout/`.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Descrever os dois `LayoutComponentBase` do repositório e os componentes de navegação/painéis que
`MainLayout` compõe.

## 2. Os dois layouts

| Layout | Usado por | Composição |
|---|---|---|
| `MainLayout.razor` | Todas as rotas autenticadas de produto (`/daily`, `/wallet`, `/account`, `/settings`, catálogos de Design System) e as duas páginas de erro/not-found | `TopNavigation`, `ProfileSidePanel`, `AccountSidePanel`, `AppFooter`, `BeeDayToastHost` |
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
  `has-right-panel`) reflete qual está aberto, controlando o layout de grid via CSS.
- Estrutura DOM: `TopNavigation` (fixo) → `beeday-workspace` (grid de 3 colunas: slot esquerdo =
  `ProfileSidePanel`, centro = `beeday-content-shell` com `@Body` + `AppFooter`, slot direito =
  `AccountSidePanel`) → `BeeDayToastHost` (fora do grid, `position: fixed` via CSS).

## 4. `TopNavigation.razor`

Header fixo com 3 regiões: botão de marca (abre/fecha `ProfileSidePanel`, ícone
`ChevronLeft`/`ChevronRight` conforme estado), navegação central (`NavLink` para `/daily` e
`/wallet`, `Match="NavLinkMatch.Prefix"`), botão de menu (abre/fecha `AccountSidePanel`, ícone
`Close`/`Menu`). Renderiza o texto da marca como
`<span class="top-navigation__brand-level">LEVEL</span><span class="top-navigation__brand-up">UP</span>`
— literal, não `BeeDayBrand` — ver achado em [`README.md`](README.md#achados-relevantes-reportados-não-corrigidos).

## 5. Painéis laterais

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
`[Parameter] IsOpen`/`OnClose`. Contém o mesmo texto de marca duplicado (`LEVEL`/`UP`) do
`TopNavigation` — mesmo achado.

## 6. `AppFooter.razor`

Estático, sem `@code`. Três colunas de links (`BeeDay`/`Developers`/`Social`) — a maioria são
placeholders (`href="#"`: News, Contact, Documentation, GitHub, Release Notes, Community, Privacy
Policy, Terms of Service) exceto os dois links de "Social" (LinkedIn, GitHub pessoal do mantenedor,
reais) e "About", que aponta para
`https://github.com/tiagoarrigoni/LevelUp` — nome de repositório antigo, ver achado no
[`README.md`](README.md#achados-relevantes-reportados-não-corrigidos).

## 7. `ReconnectModal.razor`

Não é filho de nenhum `LayoutComponentBase` — é renderizado direto em `App.razor`, fora de
`<Routes>`, então cobre qualquer página independentemente do layout. Markup padrão do template
Blazor Web App (`.NET 10`) para o modal de "Rejoining the server..." exibido pelo cliente SignalR
(`blazor.web.js`) quando o circuito cai e tenta reconectar; carrega seu próprio módulo JS
(`ReconnectModal.razor.js`, não auditado nesta Sprint — é gerado/mantido pelo template, não
código de Feature). Não injeta nenhum serviço C#.

## 8. Fontes de verdade

- `src/BeeDay.Web/Components/Layout/MainLayout.razor`, `OnboardingLayout.razor`,
  `TopNavigation.razor`, `ProfileSidePanel.razor`, `AccountSidePanel.razor`, `AppFooter.razor`,
  `ReconnectModal.razor`.
