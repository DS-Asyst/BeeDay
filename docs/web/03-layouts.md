# Layouts

**Última verificação:** 2026-08-13 — Sprint 21.12, EPIC 21.

## Layouts ativos

| Layout | Uso | Estrutura |
|---|---|---|
| `MainLayout` | Rotas autenticadas | `MobileHeader` + (`DesktopSidebar` + Workspace) + `MobileSidebar` + `BeeDayToastHost` |
| `PublicLayout` | Home pública `/` | `PublicHeader` + conteúdo + `AppFooter` |
| `OnboardingLayout` | Login, identidade, perfil inicial e tutorial | Conteúdo focado sem navegação de produto |

O shell autenticado possui somente duas regiões permanentes no desktop: Navigation e Workspace.
`RightRail`, `ProfileSidePanel` e `AccountSidePanel` foram aposentados e removidos. O footer
institucional continua legítimo no `PublicLayout`, aparecendo em Home, `/brand/typography` e, desde
a Sprint 25.17, em toda rota `/experience-system/*` (`beeday Experience System`, ver
[`docs/web/02-routing-and-pages.md`](02-routing-and-pages.md) §9), mas não nas rotas autenticadas
Daily, Wallet ou Account. O link do footer para Typography foi substituído por um link para
`/experience-system` nesta Sprint — `/brand/typography` continua acessível diretamente e a partir da
navegação interna do Experience System, apenas deixou de ter um link próprio no footer.

Desde a Sprint 29.1, `AppFooter` abre com um separador ondulado (`.app-footer__wave`): um `<path>`
SVG inline, `aria-hidden="true"`, preenchido com `--beeday-palette-cor0` (o mesmo token do fundo do
footer) sobre um wrapper com `--beeday-color-background` (branco, a superfície pública padrão que
precede o footer). Substitui a antiga combinação Home-specific de `wave-site.png` + bloco de tópicos
duplicado que ficava entre o conteúdo da Home e o footer real — ver
[`docs/brand/01-character-illustration.md`](../brand/01-character-illustration.md).

## Navegação autenticada

`NavigationItems` é a fonte compartilhada por `DesktopSidebar` e `MobileSidebar`:

| Grupo | Item | Destino/ação |
|---|---|---|
| Primário | Profile | `/profile` |
| Primário | Daily | `/daily` |
| Primário | Wallet | `/wallet` |
| Secundário | Account | `/settings` (`/account` permanece alias da mesma página) |
| Sessão | Logout | `POST /auth/logout` com `AntiforgeryToken` |

Support e Donate dos painéis antigos eram botões sem destino nem efeito e foram classificados como
legado não funcional. Não foram substituídos por links fictícios. `NavigationItem` continua sendo a
única primitive de linha; um boundary com `::deep` garante que o `NavLink` renderizado receba a
mesma composição flex de ícone + label dos botões, além de active, hover e focus-visible.

## Sidebar e identidade

No desktop (a partir de 1200px), `DesktopSidebar` é fixa, mede `15.5rem` (248px) e usa superfície
neutra com border sutil. A wordmark oficial é preservada sem distorção. Brand Blue fica concentrado
em active/focus/hover e ações importantes, em vez de preencher toda a região. Itens têm altura
mínima de 52px, ícone de 32px e gap de 1.1rem, próximos às proporções medidas no Lingo (sidebar
256px, item 52px, ícone 32px), adaptados à identidade BeeDay.

## Estratégia de largura

Os tokens semânticos do shell são:

- `--beeday-sidebar-width: 15.5rem`, consumido pela Sidebar e pelo offset do Workspace;
- `--beeday-top-navigation-height`, 3.75rem abaixo de 1200px e 0 no desktop.

O conteúdo autenticado não herda o reading-width/gutter público: Profile, Daily e Wallet são owners
de sua largura. Os overrides scoped sem leitura efetiva `--beeday-reading-width: 48rem` e
`--beeday-workspace-width: 100rem` foram removidos no sweep final; não eram API. Wallet preserva
container e grid responsivo próprios.

## Responsividade

Há um breakpoint estrutural único:

- desktop `>= 1200px`: DesktopSidebar fixa + Workspace;
- tablet/mobile `< 1200px`: MobileHeader + MobileSidebar overlay + Workspace integral;
- Daily entre `901px` e `1199px`: board de quatro colunas com scroll horizontal interno controlado;
- Daily entre `621px` e `900px`: duas colunas;
- Daily `<= 620px`: uma coluna.

Assim, 1024/900/768 pertencem conscientemente ao mesmo shell tablet e não disputam com o paradigma
desktop. O drawer preserva backdrop, close, Escape, foco inicial, `aria-hidden` e ausência da árvore
de foco quando fechado. O documento inteiro não deve produzir overflow horizontal; somente o board
operacional pode fazê-lo internamente na faixa tablet.

## Progresso

`DashboardState` continua scoped e idempotente. Level, XP total, XP progress/remaining e progresso
real de tarefas/project tasks foram integrados à Home por `ExperienceBar`, `ProgressMetricCard` e
`BeeDayProgressBar`; não há request ou store duplicado. Streak continua ausente porque não existe
backing de domínio.
