# Responsiveness

**Fonte da verdade:** levantamento completo de toda ocorrência de `@media` em
`src/BeeDay.Web/wwwroot/css/*.css` (20 arquivos) e `src/BeeDay.Web/Components/**/*.razor.css`
(30 arquivos) — 50 arquivos de CSS no total, cada um lido integralmente ou varrido por `@media`
nesta Sprint (16.7 havia enumerado apenas as 19 folhas globais).

**Última verificação:** 2026-08-10 (Sprint 18.7) — contagem de folhas globais corrigida de 19 para
20 (`Glob` direto de `wwwroot/css/*.css` confirma 20 arquivos hoje).

## 1. Objetivo

Dar a tabela completa e definitiva de todo breakpoint real do repositório — este é o documento
"dono" desse assunto; [`docs/design-system/01-foundations.md`](../design-system/01-foundations.md)
§10 e [`01-guidelines.md`](01-guidelines.md) §4 apontam para cá em vez de repetir a lista.

## 2. Não existe um sistema de breakpoints — existem 29 valores distintos

**29 declarações distintas** de largura/altura de viewport, sem nenhum token compartilhado: 26
valores de `max-width`, 2 de `min-width`, 1 de `max-height`.

### 2.1 `max-width` (26 valores, ordenados)

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
| 640 | 40rem (2×) | `wallet.css` (×2), `OnboardingLayout.razor.css`, `LoginBackground.razor.css`, `IconCatalog.razor.css`, `HeroCatalog.razor.css` |
| 650 | — | `cards.css` (×5 regras) |
| 672 | 42rem | `design-system.css`, `polish.css` |
| 680 | — | `TopNavigation.razor.css` |
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

### 2.2 `min-width` (2 valores)

| px | Arquivo | Contexto |
|---|---|---|
| 641 | `wallet.css:393` | Complementa o `max-width: 640px` da mesma folha — único par min/max explicitamente complementar encontrado |
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
  (`AccountSidePanel`, `ProfileSidePanel`, `MainLayout` ×2) — consistente entre si — mas
  `TopNavigation.razor.css` (também Layout, também "a casca do produto") corta em `920px` e depois
  de novo em `680px`, dois valores próprios que não coincidem com o `760px` do resto da casca.
- **Cabeçalho de página estreito**: `design-system.css`/`polish.css` cortam `BeeDayPageHeader`/
  `BeeDaySectionHeader`/`BeeDayHero` em `42rem` (672px), mas `Account.razor.css` (a única página que
  usa `BeeDayPageHeader` em produção) tem seu próprio corte em `720px` para o restante do layout da
  página — um usuário girando a tela por essa faixa de 48px vê o cabeçalho e o corpo da página
  mudarem de layout em momentos ligeiramente diferentes.

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
  paradigma de layout (colunas → drawer sobreposto), não apenas redimensionamento.
- **`TopNavigation`**: em `920px`, a grade de 4 colunas estreita a coluna da marca (17rem→13rem); em
  `680px`, os links centrais (`Daily`/`Wallet`) desaparecem inteiramente (`display: none`) — não há
  um menu alternativo visível para alcançá-los nessa largura além da navegação por URL direta ou os
  painéis laterais.
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

- Todas as ocorrências de `@media` em `src/BeeDay.Web/wwwroot/css/*.css` (20 arquivos) e
  `src/BeeDay.Web/Components/**/*.razor.css` (30 arquivos) — 50 arquivos, levantamento completo,
  contagem cruzada manual (26 `max-width` + 2 `min-width` + 1 `max-height` = 29).
- [`docs/design-system/01-foundations.md`](../design-system/01-foundations.md) §9-10 (camadas de
  CSS, cross-referenciado, não duplicado).
- [`02-accessibility.md`](02-accessibility.md) (media features de acessibilidade, cross-referenciado).
