# Responsiveness

**Fonte da verdade:** levantamento completo de toda ocorrência de `@media` em
`src/BeeDay.Web/wwwroot/css/*.css` (19 arquivos) e `src/BeeDay.Web/Components/**/*.razor.css`
(36 arquivos) — 55 arquivos de CSS no total, cada um lido integralmente ou varrido por `@media`
nesta Sprint (16.7 havia enumerado apenas as 19 folhas globais).

**Última verificação:** 2026-08-12 (Sprint 21.3, EPIC 21 — BeeDay Navigation) — `TopNavigation`
removida do repositório (`Layout/TopNavigation.razor.css` deletado junto com o componente), suas
responsabilidades absorvidas por `MobileHeader`/`MobileSidebar` — dois arquivos novos de CSS
isolado, junto com `NavigationItem.razor.css`/`NavigationItems.razor.css` (sem `@media` próprio).
Efeito líquido: 33 → 36 arquivos isolados (+4, -1). A remoção de `TopNavigation` também elimina o
único arquivo que declarava `max-width: 680px` (§2.1: 26 → 25 valores distintos) — `920px` nunca
tinha entrado na tabela do §2.1 apesar de também ter sido um corte próprio de
`TopNavigation.razor.css`, lacuna pré-existente desta seção que se tornou irrelevante com a
remoção do arquivo, não investigada retroativamente. `min-width: 1024px` (§2.2) agora é declarado
em 5 arquivos (`MainLayout` ×2, `DesktopSidebar`, `RightRail`, `MobileHeader`, `MobileSidebar`) —
era 4 com `TopNavigation`. Ver §2/§3/§5. Verificação anterior: 2026-08-12 (Sprint 21.2, EPIC 21 —
BeeDay Shell Foundation) — dois arquivos de CSS isolado novos (`Layout/DesktopSidebar.razor.css`,
`Layout/RightRail.razor.css`) introduziram o primeiro breakpoint `min-width` do novo shell
(`1024px`, replicando o breakpoint estrutural único documentado para o Lingo em
`docs/epics/21-lingo-product-experience/README.md` §13). Nessa verificação a contagem de arquivos
de CSS isolado também foi reconferida por completo (`find`, não amostragem) e corrigida de 29 para
31 **antes** das duas adições daquela Sprint — drift pré-existente, não investigado retroativamente.
Verificação anterior: 2026-08-12 (Sprint 20.8) — contagem de CSS isolado por componente corrigida
de 30 para 29
(`LoginBackground.razor.css` removido junto com o componente — background de imagem descontinuado,
ver `docs/epics/20-home-visual-experience/README.md` seção "Sprint 20.8"); o valor 640px/40rem que
ele compartilhava com `OnboardingLayout.razor.css` permanece (este último manteve seu próprio bloco
`@media`). Verificação anterior: 2026-08-11 (Sprint 20.3) — contagem de folhas globais corrigida de
20 para 19 (`css/cursors.css` removido; sem `@media` próprio). Verificação anterior: 2026-08-10
(Sprint 18.7) — contagem corrigida de 19 para 20.

## 1. Objetivo

Dar a tabela completa e definitiva de todo breakpoint real do repositório — este é o documento
"dono" desse assunto; [`docs/design-system/01-foundations.md`](../design-system/01-foundations.md)
§10 e [`01-guidelines.md`](01-guidelines.md) §4 apontam para cá em vez de repetir a lista.

## 2. Não existe um sistema de breakpoints — existem 29 valores distintos

**29 declarações distintas** de largura/altura de viewport, sem nenhum token compartilhado: 25
valores de `max-width`, 3 de `min-width` (Sprint 21.2 adicionou `1024px`), 1 de `max-height`.

### 2.1 `max-width` (25 valores, ordenados)

| px | rem original (se aplicável) | Arquivo(s) |
|---:|---|---|
| 352 | 22rem | `identity.css` |
| 380 | — | `Tutorial.razor.css` |
| 420 | — | `wallet.css` |
| 448 | 28rem | `Login.razor.css` |
| 480 | 30rem | `design-system.css`, `polish.css` |
| 520 | — | `feedback.css`, `AccountSidePanel.razor.css`, `HabitEditorModal.razor.css`, `ActivityFilterBar.razor.css`, `ProjectWorkspace.razor.css` |
| 544 | 34rem | `identity.css`, `Login.razor.css` |
| 560 | — | `feedback.css`, `BeeDayFeedbackModal.razor.css` |
| 580 | — | `CreateProfile.razor.css` |
| 600 | — | `editor-modal.css`, `Tutorial.razor.css` (×2 regras) |
| 620 | — | `Home.razor.css` |
| 640 | 40rem (2×) | `wallet.css` (×2), `OnboardingLayout.razor.css`, `IconCatalog.razor.css`, `HeroCatalog.razor.css` |
| 650 | — | `cards.css` (×5 regras) |
| 672 | 42rem | `design-system.css`, `polish.css` |
| 700 | — | `AppFooter.razor.css`, `Home.razor.css`, `ProjectWorkspace.razor.css` |
| 720 | — | `settings.css`, `wallet.css` (×3), `Account.razor.css` |
| 760 | — | `AccountSidePanel.razor.css`, `ProfileSidePanel.razor.css`, `MainLayout.razor.css` (×2), `ActivityFilterBar.razor.css` |
| 767.84 | 47.99rem | `BeeDayEmptyState.razor.css` |
| 860 | — | `wallet.css` |
| 900 | — | `feedback.css`, `Home.razor.css` |
| 960 | 60rem | `polish.css` |
| 1000 | — | `wallet.css` |
| 1100 | — | `wallet.css`, `Home.razor.css` |
| 1200 | — | `wallet.css` |

### 2.2 `min-width` (3 valores)

| px | Arquivo | Contexto |
|---|---|---|
| 641 | `wallet.css:393` | Complementa o `max-width: 640px` da mesma folha — único par min/max explicitamente complementar encontrado |
| 1024 | `MainLayout.razor.css` (×2 regras), `DesktopSidebar.razor.css`, `RightRail.razor.css`, `MobileHeader.razor.css`, `MobileSidebar.razor.css` | Breakpoint estrutural do shell — replica o único breakpoint dominante documentado para o Lingo em `docs/epics/21-lingo-product-experience/README.md` §13. Usado com o **mesmo valor, coordenadamente, em 5 arquivos**: `DesktopSidebar`/`RightRail` aparecem, `MobileHeader`/`MobileSidebar` desaparecem (Sprint 21.3 — substituíram `TopNavigation`, que usava este mesmo corte até ser removida), `MainLayout` recalcula `--beeday-top-navigation-height` para `0px` (cascata para `.beeday-workspace`/`.beeday-side-slot`) e desloca `.beeday-workspace` com `padding-left`. |
| 1101 | `Home.razor.css:66` | Complementa o `max-width: 1100px` da mesma folha — segundo par complementar |

### 2.3 `max-height` (1 valor)

| px | Arquivo | Contexto |
|---|---|---|
| 700 | `AccountSidePanel.razor.css:273` | Único breakpoint de altura de viewport do repositório — ajusta o painel lateral em telas baixas (não estreitas) |

## 3. O mesmo propósito visual, cortes diferentes

Três casos confirmados onde componentes com o mesmo papel visual usam um valor de corte diferente,
sem coordenação entre si:

- **Cartão estreito**: `cards.css` usa `650px`, `wallet.css` usa `640px` para o mesmo tipo de ajuste
  (reduzir densidade de um cartão de item).
- **"Layout mobile" da casca do produto**: `760px` aparece em 4 arquivos de `Components/Layout/`
  (`AccountSidePanel`, `ProfileSidePanel`, `MainLayout` ×2) — consistente entre si. Até a Sprint
  21.3, `TopNavigation.razor.css` (também Layout, também "a casca do produto") divergia desse
  corte com `920px`/`680px` próprios — exemplo histórico, não mais reproduzível: o arquivo foi
  removido junto com o componente.
- **Cabeçalho de página estreito**: `design-system.css`/`polish.css` cortam `BeeDayPageHeader`/
  `BeeDaySectionHeader`/`BeeDayHero` em `42rem` (672px), mas `Account.razor.css` (a única página que
  usa `BeeDayPageHeader` em produção) tem seu próprio corte em `720px` para o restante do layout da
  página — um usuário girando a tela por essa faixa de 48px vê o cabeçalho e o corpo da página
  mudarem de layout em momentos ligeiramente diferentes.

**Contraexemplo (Sprint 21.2/21.3, EPIC 21):** o `min-width: 1024px` do shell (§2.2) é o primeiro
caso do repositório onde o mesmo propósito visual usa o mesmo corte, coordenadamente, em múltiplos
arquivos de `Components/Layout/`. A Sprint 21.3 foi além de só coordenar esse corte: removeu de
vez a divergência histórica citada no segundo bullet acima, deletando `TopNavigation.razor.css` em
vez de deixá-lo divergir indefinidamente ao lado do novo shell.

## 4. Media features não relacionadas a largura

| Feature | Ocorrências | Onde |
|---|---|---|
| `prefers-reduced-motion: reduce` | 20+ blocos | Ver [`02-accessibility.md`](02-accessibility.md) §6 |
| `forced-colors: active` | 4 arquivos | Ver [`02-accessibility.md`](02-accessibility.md) §7 |
| `hover: none` | 4 arquivos (`cards.css` ×3, `wallet.css` ×1) | Menus de card ficam sempre visíveis (não só no hover) em dispositivos sem hover real |
| `pointer: coarse` | 2 arquivos (`polish.css`, `wallet.css`) | Alvos de toque maiores (`min-height: 3rem` em botões/checkbox) |

## 5. Comportamento adaptativo por componente — casos notáveis

- **`MainLayout`** (`MainLayout.razor.css`): abaixo de `760px`, os 2 painéis laterais (Profile,
  Account) deixam de ocupar uma coluna do grid (`grid-template-columns: 0 minmax(0,1fr) 0`) e
  passam a `position: fixed`, sobrepondo o conteúdo em vez de dividir espaço com ele — mudança de
  paradigma de layout (colunas → drawer sobreposto), não apenas redimensionamento. **Sprint 21.2
  (EPIC 21):** em `1024px`, `--beeday-top-navigation-height` é redefinida para `0px` — como
  `.beeday-workspace`/`.beeday-side-slot` já derivam dessa variável, a única redeclaração recalcula
  a altura reservada no topo e o offset `sticky` dos painéis sem precisar repetir a mudança em cada
  regra; `.beeday-workspace` também ganha `padding-left: var(--beeday-sidebar-width)` para
  compensar o `DesktopSidebar` (`position: fixed`, fora do fluxo).
- **`MobileHeader`/`MobileSidebar`** (novos em `Components/Layout/`, Sprint 21.3 — substituem
  `TopNavigation`, removida): `display: none` a partir de `1024px` — `DesktopSidebar` assume a
  navegação primária a partir daí; abaixo de `1024px` continuam sendo o único acesso a ela, papel
  explicitamente transitório (ver `docs/epics/21-lingo-product-experience/README.md` §8/§10). Sem
  os cortes próprios (`920px`/`680px`) que `TopNavigation` tinha — `MobileHeader` não tem grade de
  colunas nem links centrais a esconder, então não há mais nenhum breakpoint intermediário
  específico de navegação abaixo de `1024px`.
- **`DesktopSidebar`/`RightRail`** (novos em `Components/Layout/`, Sprint 21.2): `display: none` por
  padrão, mostrados apenas a partir de `1024px` — independente do breakpoint `760px` dos painéis
  Profile/Account. Entre `760px` e `1024px`, os painéis já saíram do modo overlay fixo e ocupam
  coluna de grid (comportamento definido só pelo corte de `760px`), mas `DesktopSidebar`/
  `RightRail` continuam ausentes (comportamento definido só pelo corte de `1024px`) — os dois
  breakpoints não se coordenam entre si nessa faixa intermediária.
- **`Home` (`/daily`)**: `min-width: 1101px` mostra 4 colunas (Habits/Tasks/Todos/Projects) lado a
  lado; abaixo de `1100px`, `900px` e `700px`/`620px` há degraus sucessivos de recomposição do grid
  do Dashboard — o arquivo com mais degraus de breakpoint do repositório (5 regras distintas).
- **`Wallet`**: `wallet.css` reflui de um layout de 2 colunas (`workspace` + `tags-panel` sticky)
  para 1 coluna em `1000px`, e o card de transação individual muda de grid horizontal para blocos
  empilhados em `720px`.
- **`BeeDayEmptyState`**: o único componente com um breakpoint fracionário
  (`47.99rem` = 767.84px) — valor deliberadamente escolhido logo abaixo de 768px (o breakpoint
  "tablet" clássico popularizado pelo Bootstrap), embora nenhum outro arquivo do repositório use
  768px ou um valor próximo — não há evidência de uma decisão de design de "abaixo do tablet" sendo
  aplicada consistentemente em outros componentes.

## 6. Fontes consultadas

- Todas as ocorrências de `@media` em `src/BeeDay.Web/wwwroot/css/*.css` (19 arquivos) e
  `src/BeeDay.Web/Components/**/*.razor.css` (36 arquivos) — 55 arquivos, levantamento completo
  nesta Sprint, contagem cruzada manual (25 `max-width` + 3 `min-width` + 1 `max-height` = 29).
- [`docs/design-system/01-foundations.md`](../design-system/01-foundations.md) §9-10 (camadas de
  CSS, cross-referenciado, não duplicado).
- [`02-accessibility.md`](02-accessibility.md) (media features de acessibilidade, cross-referenciado).
- [`docs/epics/21-lingo-product-experience/README.md`](../epics/21-lingo-product-experience/README.md)
  §3/§13 (especificação do breakpoint único do Lingo que motivou o novo `1024px`).
