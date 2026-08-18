# Layouts

**Última verificação:** 2026-08-18 — Sprint 29.4 (novo `EditorialLayout`). Verificação anterior:
2026-08-13 — Sprint 21.12, EPIC 21.

## Layouts ativos

| Layout | Uso | Estrutura |
|---|---|---|
| `MainLayout` | Rotas autenticadas | `MobileHeader` + (`DesktopSidebar` + Workspace) + `MobileSidebar` + `BeeDayToastHost` |
| `PublicLayout` | Home pública `/`, `/experience-system/*`, `/brand/typography` | `PublicHeader` + conteúdo + `AppFooter` |
| `EditorialLayout` | As 12 páginas editoriais do footer (§9 de `02-routing-and-pages.md`) | `@Body` (hero+corpo, sem header separado) + `EditorialFooter` + `BeeDayToastHost` |
| `OnboardingLayout` | Login, identidade, perfil inicial e tutorial | Conteúdo focado sem navegação de produto |

O shell autenticado possui somente duas regiões permanentes no desktop: Navigation e Workspace.
`RightRail`, `ProfileSidePanel` e `AccountSidePanel` foram aposentados e removidos. O footer
institucional (`AppFooter`) continua legítimo no `PublicLayout`, aparecendo em Home, `/brand/typography`
e em toda rota `/experience-system/*`, mas não nas rotas autenticadas Daily, Wallet ou Account, nem
— desde a Sprint 29.4 — nas 12 páginas editoriais do footer, que usam `EditorialFooter` em vez disso
(ver "`EditorialLayout` (Sprint 29.4)" abaixo). O link do footer para Typography foi substituído por
um link para `/experience-system` na Sprint 25.17 — `/brand/typography` continua acessível
diretamente e a partir da navegação interna do Experience System, apenas deixou de ter um link
próprio no footer.

Desde a Sprint 29.1, `AppFooter` abre com um separador ondulado (`.app-footer__wave`): um `<path>`
SVG inline, `aria-hidden="true"`, preenchido com `--beeday-palette-cor0` (o mesmo token do fundo do
footer) sobre um wrapper com `--beeday-color-background` (branco, a superfície pública padrão que
precede o footer). Substitui a antiga combinação Home-specific de `wave-site.png` + bloco de tópicos
duplicado que ficava entre o conteúdo da Home e o footer real — ver
[`docs/brand/01-character-illustration.md`](../brand/01-character-illustration.md). `EditorialFooter`
(Sprint 29.4) não usa essa onda nem esse fundo colorido — é branco e minimalista (ver abaixo).

## `EditorialLayout` (Sprint 29.4)

`Components/Layout/EditorialLayout.razor` é o layout do microsite editorial público (as 12 páginas
ligadas pelo `AppFooter` — ver [`02-routing-and-pages.md`](02-routing-and-pages.md) §9). Diferente de
`PublicLayout`, não renderiza `PublicHeader`: sem a barra branca fixa, sem bandeiras de idioma, sem o
botão "Continue to beeday" nessas páginas especificamente. Header e hero passam a ser uma única
superfície colorida — a marca beeday e a navegação contextual (`EditorialSectionNav`) fazem parte do
próprio `BeeDayHero`, via seu novo parâmetro `HeaderNav`, renderizado por `InstitutionalPageShell`. A
troca de idioma continua funcionando por baixo (cookie de cultura, `pt-BR`/`en-US`) — apenas o
seletor visual de bandeiras não aparece nessas páginas. `AppFooter` é substituído por
`EditorialFooter` (ver abaixo). `Home`, `Login`, os fluxos de autenticação e `/experience-system/*`
continuam em `PublicLayout`, inalterados — a remoção do header branco é específica das páginas
listadas em `02-routing-and-pages.md` §9.

`.editorial-layout__main` (`polish.css`) define `--beeday-hero-bleed-inset: var(--beeday-page-gutter)`
— o mesmo mecanismo de full-bleed que `BeeDayHero.razor.css` já usa (`margin-inline: calc(-1 *
var(--beeday-hero-bleed-inset, 0px))`, sem `width` explícito para que o auto-width absorva a margem
negativa simetricamente): a superfície colorida do header+hero alcança as bordas reais do viewport,
não apenas a caixa de conteúdo com padding. Contextos que nunca definem essa variável (o hero
compacto da Wallet, por exemplo) permanecem inalterados — o token vale `0px` por padrão.

## `EditorialFooter` (Sprint 29.4)

`Components/Layout/EditorialFooter.razor` — fundo branco, sem colunas, sem mascote, sem seletor de
idioma, sem links duplicados (contrato explícito: nada do `AppFooter` grande deve aparecer aqui).
Composição: um botão circular "Back to top" no início da linha (reaproveita o ícone `ChevronDown` do
Icon System, rotacionado 180° via CSS — nenhum asset novo), o link central "BUY ME A COFFEE"
(`href="/buy-me-a-coffee"` — contrato de rota; a página em si está fora do escopo desta Sprint, sem
integração de pagamento), e o copyright (`SharedResources.FooterCopyright`, a mesma chave que
`AppFooter` já usa, não duplicada). O scroll do Back to Top usa um módulo JS dedicado
(`wwwroot/js/beeday-editorial-footer.js`) que respeita `prefers-reduced-motion` antes de escolher
`behavior: 'smooth'` vs. `'auto'`.

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
