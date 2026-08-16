# Accessibility

**Fonte da verdade:** verificado por busca direta de atributos ARIA (`aria-live`, `aria-expanded`,
`aria-pressed`, `role="alert"`, `role="status"`, `aria-modal`, etc.) em
`src/BeeDay.Web/Components/**/*.razor`, leitura de `polish.css`, do behavior
`beeday-dialog-focus.js`, cálculo automatizado de contraste sobre `variables.css` e varreduras axe
em Chromium sobre páginas públicas/autenticadas representativas.

**Última verificação:** 2026-08-16 (Sprint 25.15 — Design System Quality Engineering).

## 1. Objetivo

Documentar o que o código garante hoje em acessibilidade — e onde não garante nada — sem
prescrever um padrão novo. Toda afirmação é "o código faz/não faz X", não "o código deveria fazer
X".

## 2. ARIA — uso confirmado

O inventário atual usa estes contratos recorrentes:
`aria-live`, `aria-expanded`, `aria-pressed`, `role="alert"`, `role="status"`, `aria-modal`,
`aria-current`. Padrões recorrentes:

| Padrão | Onde |
|---|---|
| `role="alertdialog"` + `aria-modal="true"` + `aria-labelledby`/`aria-describedby` | `BeeDayConfirmDialog` |
| `role="dialog"` + `aria-modal="true"` + label | `EditorModalShell`, `BeeDayFeedbackModal`, `ProjectWorkspace` |
| `role="status"`/`role="alert"` (dependendo da severidade) | `BeeDayToastHost` (`RoleFor`: erro → `alert`, senão `status`) |
| `aria-live="polite"` | `BeeDayLoading`, região de status silenciosa do Wallet (`_statusAnnouncement`) |
| `aria-busy` | `main` de várias páginas (`Wallet.razor`, `Account.razor`), refletindo `IsBusy` agregado |
| `aria-expanded`/`aria-pressed` | Botões de toggle (painéis, menus, filtros, `.beeday-icon-toggle`) |
| `aria-hidden="true"` em ícones decorativos | Todo `BeeDayIcon` com `Decorative="true"` (padrão) |
| `role="progressbar"` + `aria-valuemin/max/now/text` | `BeeDayProgressBar` |

Booleanos ARIA dinâmicos são serializados em lowercase. A Sprint 25.10 corrigiu os quatro casos
confirmados que ainda emitiam `True`/`False`: Activity create menu, Project context options,
ProjectWorkspace To-Do toggle e TransactionList busy.

## 3. `BeeDayIcon` — validação de acessibilidade em runtime

`BeeDayIcon.razor.cs.OnParametersSet` lança `InvalidOperationException` se `Decorative="false"` e
nenhum `Label` foi fornecido — nenhum outro componente do Design System tem uma checagem
equivalente (ex.: nada impede um `BeeDayButton` sem `ChildContent` nem `AdditionalAttributes["aria-label"]`
de ser renderizado sem nome acessível). Isso torna `BeeDayIcon` uma garantia real de runtime —
mas só cobre ícones, não os outros componentes.

## 4. Achado — nenhum link "pular para o conteúdo"

Busca por "skip to content"/"skip-link"/`tabindex="-1"` associado a um alvo de skip-link não
encontrou nenhuma ocorrência em `src/BeeDay.Web/`. `Routes.razor` tem
`<FocusOnNavigate RouteData="routeData" Selector="h1" />` (comportamento nativo do Blazor Router:
move o foco para o primeiro `<h1>` após navegação), o que cobre parcialmente o mesmo problema
(usuário de teclado/leitor de tela não precisa tabular desde o topo após navegar), mas não existe
um mecanismo equivalente para pular a navegação/painéis laterais *dentro* de uma página já
carregada.

## 5. Teclado e foco

- O lifecycle canônico de `BeeDayConfirmDialog`, `EditorModalShell` e `BeeDayFeedbackModal` é:
  OPEN → initial focus → contenção de Tab/Shift+Tab → Escape/close → restore. Confirm inicia em
  Cancel; Editor no primeiro field habilitado; feedback no painel para anunciar title/description.
  Confirmação nested restaura o foco no Delete do editor e o editor restaura seu trigger.
- `DialogFocusScope` compartilha somente esse comportamento via `beeday-dialog-focus.js`; não
  unifica markup ou significado dos dialogs. A lista de focusables é recalculada a cada Tab para
  respeitar busy/disabled. Sem controles, o painel recebe foco; se o trigger saiu do DOM, close não
  lança erro e restaura o próximo scope ativo quando houver.
- `Escape` continua passando pelos busy guards de Confirm/Editor. `BeeDayCardMenu` e drawer mantêm
  seus lifecycles próprios porque são menu/navigation, não dialogs.
- `:focus-visible` é estilizado globalmente (`theme.css`:
  `:where(button, a, input, select, textarea, [tabindex]):focus-visible { box-shadow:
  var(--beeday-focus-ring); }`) e complementado por `polish.css` com `scroll-margin: 5rem`. O ring
  usa o token de foco derivado de Brand Primary e o scroll margin reduz o risco de o alvo ficar sob
  o cabeçalho fixo.
- `BeeDayCheckbox` mantém o `<input>` real focável (posicionado fora da tela, não `display:none`) e
  aplica o indicador de foco ao elemento visual irmão — o controle nativo continua sendo o alvo real
  de Tab/Espaço.
- `polish.css`: `:where(button, a, input, select, textarea)[aria-disabled="true"] { pointer-events:
  none; }` — um elemento `aria-disabled` (em vez de `disabled` nativo) ainda é alcançável por Tab
  mas não clicável; não confirmado se algum componente usa `aria-disabled` em vez de `disabled`
  nativo na prática (a maioria dos componentes auditados usa `disabled` nativo, que já remove o
  elemento da ordem de tabulação).
- `MobileSidebar` é removida da árvore de foco quando fechada e seu backdrop/drawer controlam o
  lifecycle de navegação sem depender dos painéis laterais aposentados.

## 6. Movimento — `prefers-reduced-motion`

O inventário direto da Sprint 25.6 encontrou 31 stylesheets com `animation`, `transition` ou
`@keyframes`: 18 tinham fallback local e 13 dependiam apenas da rede global. A Sprint adicionou
fallback a `AppFooter`, `PublicLanguageSwitcher`, `ReconnectModal`, `ActivityFilterBar` e
`ProjectContextFilter`, chegando a **23/31**. Os oito restantes pertencem a Auth/ProfileCreation,
Habit, ProjectWorkspace, DashboardColumn ou motion interno dos activity cards e ficam com seus
owners de convergência; a contagem anterior de "cobertura consistente em todo CSS" era imprecisa.

`animations.css` mantém o safety net universal (`*, *::before, *::after`) que reduz duration para
`.01ms`, mas ele não substitui fallback local: delay, opacity e transform ainda podem ocultar
feedback. Por isso loading passa a manter a cápsula visível sem spinner/shimmer, reconnect mantém o
dialog e um indicador estático, menus/modais aparecem sem entrada, e a Home preserva sua cor de
seção intermediária sem scroll motion. Feedback textual, ARIA e controles continuam presentes.

## 7. `forced-colors` (modo de alto contraste)

`polish.css` trata explicitamente `@media (forced-colors: active)` e força
`border-color: CanvasText` em campos/card. É o único dos 55 sources CSS atuais (37 isolados,
17 em `wwwroot/css` e `app.css`) com tratamento local — a cobertura existe, mas é pontual.

## 8. Cursor customizado — removido na Sprint 20.3 (histórico)

Até a Sprint 20.3 (EPIC 20), `cursors.css` substituía o cursor do sistema inteiro por imagens
customizadas (`cursor: url(...) !important` em `html, body, body *`), incluindo variantes para
clique, arrastar (`grab`/`grabbing`) e desabilitado. Não havia nenhum mecanismo no código
(configuração de usuário, media query, preferência salva) para desativar isso e voltar ao cursor
nativo do sistema operacional — um cursor customizado de baixo contraste ou pequeno podia ser mais
difícil de localizar na tela para uma pessoa com baixa visão do que o cursor do sistema, que já
respeita o tamanho/cor configurados no SO. O fallback nativo (`, auto`/`, pointer`/`, grab` após a
URL) só entrava em ação se a imagem falhasse ao carregar — não era uma opção de acessibilidade, era
uma degradação técnica.

**Estado atual (Sprint 20.3):** `cursors.css` e os assets `wwwroot/cursors/{cursor-normal,
cursor-click}.png` foram removidos estruturalmente — o BeeDay usa cursores nativos do
navegador/sistema operacional em toda a aplicação. A tensão de acessibilidade descrita acima não se
aplica mais. A semântica de `grab`/`grabbing` para itens arrastáveis (`.beeday-sortable__item`) e de
`pointer` para o corpo clicável de cards (`role="button"`, `.activity-card__body--openable`/
`.habit-card__body--openable`) foi preservada como declarações CSS nativas (sem imagem), movidas
para `dragdrop.css`/`cards.css`, os stylesheets que já possuem esses seletores.

## 9. Contraste de cor — contrato automatizado

`DesignSystemContrastTests` resolve tokens e aliases de `variables.css` e aplica a fórmula WCAG 2.x
de luminância relativa/razão. O gate cobre brand primary, texto primary/secondary, botões primary,
success, warning e danger, info sobre info-soft e foco inverso sobre brand.

| Par | Uso | Contraste calculado | Limiar WCAG AA |
|---|---|---|---|
| `--beeday-color-text-secondary` (#514858) sobre `--beeday-color-surface` (#fff) | Descrições, corpo de texto secundário | **≈8.69:1** | Passa AA (4.5:1) e AAA (7:1) para texto normal |
| `--beeday-color-text-muted` (#817789) sobre `--beeday-color-surface` (#fff) | Texto auxiliar, meta-informação, estado vazio | **≈4.26:1** | **Abaixo de 4.5:1** (texto normal AA) — passa o limiar de 3:1 (texto grande/UI, AA) mas não o de texto normal |

O par `text-muted` sobre branco continua abaixo de 4.5:1 e não é promovido artificialmente a contrato
de texto normal. A varredura axe levou os consumers pequenos realmente renderizados de EmptyState,
Footer, Login, Wallet e editor modal para `text-secondary`. Outros usos precisam ser avaliados no
contexto renderizado; o token muted não foi alterado globalmente.

## 10. Semântica e leitores de tela

- Botões usam `<button>` real em todo componente do Design System auditado — nenhum `<div
  onclick>` fazendo o papel de botão foi encontrado dentro de `Components/DesignSystem/`.
- Cards de atividade (`ActivityCard`/`HabitCard`, em `Components/Features/Dashboard/`) usam
  `role="button"` em uma área clicável não-semântica (`.activity-card__body--openable`) — confirmado
  diretamente em `ActivityCard.razor` na Sprint 20.3 (a semântica `cursor: pointer` para essa área
  vive hoje em `cards.css`, ver §8).
- Formulários usam `<label>` associado (via `Id`) em todo componente de `Forms/` — nenhum campo foi
  encontrado sem label programaticamente associado.

## 11. Testes automatizados de acessibilidade

`AccessibilityQualityTests` usa axe no Chromium, sem exclusões, em Home, Typography, Login, Daily,
Wallet e no diálogo de transação. bUnit continua validando roles, labels, descriptions, busy e
booleanos lowercase; E2E valida também initial focus, Tab/Shift+Tab containment, Escape, nested
restore, trigger removido e scope sem controles.

Automação detecta somente uma parte dos problemas possíveis. Resultado verde não declara
conformidade WCAG, certificação legal nem substitui teclado, leitor de tela e revisão humana. O mapa
de cobertura e limitações vive em [`docs/testing/02-design-system-quality-gates.md`](../testing/02-design-system-quality-gates.md).

## 12. Fontes consultadas

- Busca de atributos ARIA em `src/BeeDay.Web/Components/**/*.razor` (28 arquivos com pelo menos uma
  ocorrência).
- `src/BeeDay.Web/wwwroot/css/polish.css`, `theme.css`, `animations.css`, `feedback.css`.
- Levantamento de `@media (forced-colors: active)` e `@media (prefers-reduced-motion: reduce)` em
  todo `src/BeeDay.Web/wwwroot/css/*.css` e `src/BeeDay.Web/Components/**/*.razor.css`.
- `src/BeeDay.Web/Components/DesignSystem/Icons/BeeDayIcon.razor.cs`.
- `src/BeeDay.Web/Components/DesignSystem/Modals/DialogFocusScope.cs` e
  `src/BeeDay.Web/wwwroot/js/beeday-dialog-focus.js`.
- `src/BeeDay.Web/Components/Routes.razor` (`FocusOnNavigate`).
- `DesignSystemContrastTests` e `AccessibilityQualityTests`.
- [`docs/web/06-testing.md`](../web/06-testing.md) (cobertura de teste, Sprint 16.7, reaproveitado).
