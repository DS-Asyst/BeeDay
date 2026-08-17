# Layouts

**Última verificação:** 2026-08-17 — Sprint 29.3 (footer wave asset, hero full-bleed fix, modal
backdrop). Verificação anterior: 2026-08-13 — Sprint 21.12, EPIC 21.

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

Desde a Sprint 29.1, `AppFooter` abre com um separador decorativo (`.app-footer__wave`),
`aria-hidden="true"`, sobre um wrapper com `--beeday-color-background` (branco, a superfície pública
padrão que precede o footer). Substitui a antiga combinação Home-specific de `wave-site.png` + bloco
de tópicos duplicado que ficava entre o conteúdo da Home e o footer real — ver
[`docs/brand/01-character-illustration.md`](../brand/01-character-illustration.md). A Sprint 29.1
implementou esse separador como um `<path>` SVG desenhado à mão (`viewBox 0 0 1600 220`, preenchido
com `--beeday-palette-cor0`); a Sprint 29.3 substituiu essa geometria pelo asset oficial preparado
pelo responsável para essa composição, `assets/footer/footer-wave.svg` (`viewBox 0 0 1920 1060`,
renderizado via `<img>`, largura total, altura automática, sem crop) — sem canvas de fundo próprio,
por isso o wrapper continua branco por trás dele.

Todo modal/drawer de tela cheia (`EditorModalShell`, `BeeDayConfirmDialog`, `BeeDayFeedbackModal`,
`MobileSidebar` — e já antes desta Sprint, `ProjectWorkspace`) compartilha desde a Sprint 29.3 o mesmo
backdrop: `--beeday-color-overlay` (`rgb(23 27 48 / 64%)`, um cinza-ardósia neutro e translúcido,
`wwwroot/css/variables.css`). Antes, cada um hardcodava seu próprio literal roxo/violáceo
(`editor-modal.css`, `feedback.css`, `BeeDayFeedbackModal.razor.css` e `MobileSidebar.razor.css`
usavam quatro valores `rgb()` diferentes) — o padrão aprovado é claro/neutro/translúcido, sem competir
visualmente com o modal. `ProjectWorkspace` já usava esse token desde a Sprint 25.12; os demais foram
convergidos nesta Sprint. Nenhum comportamento de focus trap, Escape, `aria-modal` ou reduced motion
foi alterado — apenas a cor do backdrop.

O `BeeDayHero` compartilhado (usado pelas 11 páginas institucionais e pela raiz do Experience System)
ganhou na Sprint 29.3 `margin-inline: calc(-1 * var(--beeday-hero-bleed-inset, 0px))`, um token que
`PublicLayout`'s `<main>` (`.public-layout__main`, `polish.css`) define como
`var(--beeday-page-gutter)` — cancelando exatamente o padding lateral desse `<main>` para que a
superfície colorida do header alcance as bordas reais do viewport, não apenas a caixa de conteúdo já
com padding que a correção full-bleed da Sprint 29.2 alcançava. O token por padrão vale `0` (sem
efeito), então o hero compacto da Wallet (shell autenticado, sem esse padding para cancelar)
permanece inalterado.

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
