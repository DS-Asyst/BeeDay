# Accessibility

**Fonte da verdade:** verificado por busca direta de atributos ARIA (`aria-live`, `aria-expanded`,
`aria-pressed`, `role="alert"`, `role="status"`, `aria-modal`, etc.) em
`src/BeeDay.Web/Components/**/*.razor`, leitura de `src/BeeDay.Web/wwwroot/css/cursors.css`,
`polish.css`, e cálculo manual de contraste (fórmula WCAG 2.x de luminância relativa) sobre os
valores hexadecimais de `variables.css`.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Documentar o que o código garante hoje em acessibilidade — e onde não garante nada — sem
prescrever um padrão novo. Toda afirmação é "o código faz/não faz X", não "o código deveria fazer
X".

## 2. ARIA — uso confirmado

28 arquivos `.razor`/`.razor.css` usam pelo menos um destes atributos:
`aria-live`, `aria-expanded`, `aria-pressed`, `role="alert"`, `role="status"`, `aria-modal`,
`aria-current`. Padrões recorrentes:

| Padrão | Onde |
|---|---|
| `role="alertdialog"` + `aria-modal="true"` + `aria-labelledby`/`aria-describedby` | `BeeDayConfirmDialog` |
| `role="status"`/`role="alert"` (dependendo da severidade) | `BeeDayToastHost` (`RoleFor`: erro → `alert`, senão `status`) |
| `aria-live="polite"` | `BeeDayLoading`, região de status silenciosa do Wallet (`_statusAnnouncement`) |
| `aria-busy` | `main` de várias páginas (`Wallet.razor`, `Account.razor`), refletindo `IsBusy` agregado |
| `aria-expanded`/`aria-pressed` | Botões de toggle (painéis, menus, filtros, `.beeday-icon-toggle`) |
| `aria-hidden="true"` em ícones decorativos | Todo `PixelIcon` com `Decorative="true"` (padrão) |

## 3. `PixelIcon` — o único componente que valida acessibilidade em runtime

`PixelIcon.razor.cs.OnParametersSet` lança `InvalidOperationException` se `Decorative="false"` e
nenhum `Label` foi fornecido — nenhum outro componente do Design System tem uma checagem
equivalente (ex.: nada impede um `BeeDayButton` sem `ChildContent` nem `AdditionalAttributes["aria-label"]`
de ser renderizado sem nome acessível). Isso torna `PixelIcon` uma garantia real de build-time —
mas só cobre ícones, não os outros ~25 componentes.

## 4. Achado — nenhum link "pular para o conteúdo"

Busca por "skip to content"/"skip-link"/`tabindex="-1"` associado a um alvo de skip-link não
encontrou nenhuma ocorrência em `src/BeeDay.Web/`. `Routes.razor` tem
`<FocusOnNavigate RouteData="routeData" Selector="h1" />` (comportamento nativo do Blazor Router:
move o foco para o primeiro `<h1>` após navegação), o que cobre parcialmente o mesmo problema
(usuário de teclado/leitor de tela não precisa tabular desde o topo após navegar), mas não existe
um mecanismo equivalente para pular a navegação/painéis laterais *dentro* de uma página já
carregada.

## 5. Teclado e foco

- `Escape` fecha: `BeeDayConfirmDialog`, `EditorModalShell`, `HabitEditorModal`/`TaskEditorModal`
  (confirmação de exclusão interna primeiro, depois o modal), `BeeDayCardMenu` — padrão consistente
  em todo elemento tipo popover/modal auditado.
- `:focus-visible` é estilizado globalmente (`theme.css`:
  `:where(button, a, input, select, textarea, [tabindex]):focus-visible { box-shadow:
  var(--beeday-focus-ring); }`) e reforçado por `polish.css` com `outline: var(--beeday-focus-outline)`
  (3px sólido, cor `--beeday-game-blue-dark`) mais `scroll-margin: 5rem` — um elemento focado por
  teclado nunca fica escondido atrás do cabeçalho fixo ao rolar até ele.
- `BeeDayCheckbox` mantém o `<input>` real focável (posicionado fora da tela, não `display:none`) e
  aplica o indicador de foco ao elemento visual irmão — o controle nativo continua sendo o alvo real
  de Tab/Espaço.
- `polish.css`: `:where(button, a, input, select, textarea)[aria-disabled="true"] { pointer-events:
  none; }` — um elemento `aria-disabled` (em vez de `disabled` nativo) ainda é alcançável por Tab
  mas não clicável; não confirmado se algum componente usa `aria-disabled` em vez de `disabled`
  nativo na prática (a maioria dos componentes auditados usa `disabled` nativo, que já remove o
  elemento da ordem de tabulação).
- `.beeday-side-slot` (painéis laterais) usa `overscroll-behavior: contain` (`polish.css`) —
  rolagem dentro do painel não "vaza" para a página por trás.

## 6. Movimento — `prefers-reduced-motion`

12+ blocos `@media (prefers-reduced-motion: reduce)` distintos, um por arquivo de CSS que declara
`@keyframes`/`transition` decorativa: `animations.css`, `pixel-ui.css`, `activity-design-system.css`,
`cards.css` (via `design-system.css`), `feedback.css`, `design-system.css`, `editor-modal.css`,
`dragdrop.css`, `wallet.css`, `polish.css`, `PixelIcon.razor.css`, `TopNavigation.razor.css`,
`MainLayout.razor.css`, `BeeDayCardMenu.razor.css`, `BeeDayFeedbackModal.razor.css`,
`ExperienceBar.razor.css`, `LoginBackground.razor.css` — cobertura consistente através de todo o
CSS de produto, tanto global quanto isolado por componente. `animations.css` tem o bloco mais amplo:
um seletor universal (`*, *::before, *::after`) que zera `animation-duration`/
`transition-duration` para `.01ms` — uma rede de segurança que cobre qualquer animação futura que
esqueça seu próprio bloco `reduced-motion` individual.

## 7. `forced-colors` (modo de alto contraste)

4 arquivos tratam explicitamente `@media (forced-colors: active)`: `pixel-ui.css` (permite
`forced-color-adjust: auto` em botão/card/scrollbar), `pixel-nes.css` (remove `border-image`,
que é ignorado por navegadores neste modo de qualquer forma — torna explícito), `polish.css`
(força `border-color: CanvasText` em campos de formulário/card). Nenhum outro arquivo dos 49
(19 globais + 30 isolados) trata este modo — a cobertura existe, mas é pontual, não sistemática.

## 8. Cursor customizado — tensão com acessibilidade

`cursors.css` substitui o cursor do sistema inteiro por imagens customizadas
(`cursor: url(...) !important` em `html, body, body *`), incluindo variantes para clique, arrastar
(`grab`/`grabbing`) e desabilitado. Não há nenhum mecanismo no código (configuração de usuário,
media query, preferência salva) para desativar isso e voltar ao cursor nativo do sistema
operacional — um cursor customizado de baixo contraste ou pequeno pode ser mais difícil de
localizar na tela para uma pessoa com baixa visão do que o cursor do sistema, que já respeita o
tamanho/cor configurados no SO. O fallback nativo (`, auto`/`, pointer`/`, grab` após a URL) só
entra em ação se a imagem falhar ao carregar — não é uma opção de acessibilidade, é uma
degradação técnica.

## 9. Contraste de cor — cálculo manual (fórmula WCAG 2.x)

Verificado a partir dos valores hexadecimais de `variables.css`, aplicando a fórmula oficial de
luminância relativa e razão de contraste do WCAG 2.x. Cálculo manual, não validado por ferramenta
automatizada — tratar como indicativo, sujeito a nova verificação com um contrast checker real
antes de qualquer decisão de correção.

| Par | Uso | Contraste calculado | Limiar WCAG AA |
|---|---|---|---|
| `--beeday-color-text-secondary` (#514858) sobre `--beeday-color-surface` (#fff) | Descrições, corpo de texto secundário | **≈8.69:1** | Passa AA (4.5:1) e AAA (7:1) para texto normal |
| `--beeday-color-text-muted` (#817789) sobre `--beeday-color-surface` (#fff) | Texto auxiliar, meta-informação, estado vazio | **≈4.26:1** | **Abaixo de 4.5:1** (texto normal AA) — passa o limiar de 3:1 (texto grande/UI, AA) mas não o de texto normal |

O par `text-muted` sobre branco é usado extensivamente (`BeeDayEmptyState`, meta de card, texto de
ajuda de formulário) — em qualquer lugar onde esse texto seja renderizado abaixo do tamanho "grande"
do WCAG (24px regular ou 19px bold), o contraste calculado fica abaixo do limiar AA para texto
normal. Não foi possível confirmar, sem inspeção visual real da aplicação renderizada, se algum
desses usos específicos usa um tamanho de fonte grande o suficiente para cair na exceção "texto
grande" (limiar 3:1) do WCAG.

## 10. Semântica e leitores de tela

- Botões usam `<button>` real em todo componente do Design System auditado — nenhum `<div
  onclick>` fazendo o papel de botão foi encontrado dentro de `Components/DesignSystem/`.
- Cards de atividade (`ActivityCard`/`HabitCard`, em `Components/Features/Dashboard/`) usam
  `role="button"` em uma área clicável não-semântica (`.activity-card__body--openable`) — confirmado
  indiretamente pelo comentário em `cursors.css` ("The card's clickable body has role='button', see
  cards.css"); não foi lido o markup completo do card nesta Sprint (já coberto em profundidade na
  Sprint 16.7).
- Formulários usam `<label>` associado (via `Id`) em todo componente de `Forms/` — nenhum campo foi
  encontrado sem label programaticamente associado.

## 11. Testes automatizados de acessibilidade

Nenhum teste de acessibilidade automatizado (axe-core, Pa11y, ou equivalente) foi encontrado em
`tests/BeeDay.Web.Tests/` ou `tests/BeeDay.E2E.Tests/`. A cobertura existente valida presença de
atributos ARIA pontuais via asserção bUnit direta (`cut.Find(...).GetAttribute("aria-...")`), não
uma varredura de regras de acessibilidade. `Components/DesignSystem/PixelNesAdapterIsolationTests.cs`
(ver [`docs/web/06-testing.md`](../web/06-testing.md) §7) testa isolamento de um adapter visual, não
acessibilidade em si.

## 12. Fontes consultadas

- Busca de atributos ARIA em `src/BeeDay.Web/Components/**/*.razor` (28 arquivos com pelo menos uma
  ocorrência).
- `src/BeeDay.Web/wwwroot/css/cursors.css`, `polish.css`, `theme.css`, `animations.css`.
- Levantamento de `@media (forced-colors: active)` e `@media (prefers-reduced-motion: reduce)` em
  todo `src/BeeDay.Web/wwwroot/css/*.css` e `src/BeeDay.Web/Components/**/*.razor.css`.
- `src/BeeDay.Web/Components/DesignSystem/Icons/PixelIcon.razor.cs`.
- `src/BeeDay.Web/Components/Routes.razor` (`FocusOnNavigate`).
- Cálculo próprio de contraste WCAG a partir de `src/BeeDay.Web/wwwroot/css/variables.css`.
- [`docs/web/06-testing.md`](../web/06-testing.md) (cobertura de teste, Sprint 16.7, reaproveitado).
