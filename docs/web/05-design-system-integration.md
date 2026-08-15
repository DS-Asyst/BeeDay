# Design System Integration

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/DesignSystem/`,
`src/BeeDay.Web/Components/Behaviors/DragDrop/`, `src/BeeDay.Web/Components/App.razor` e
`src/BeeDay.Web/wwwroot/`.

**Última verificação:** 2026-08-13 (Sprint 21.12).

## 1. Objetivo

Descrever como `BeeDay.Web` compõe o Design System (`Components/DesignSystem/`, ~2000 linhas em
~50 arquivos) e a mecânica de interop JavaScript usada pelos 3 componentes que precisam medir o DOM
real. `docs/design-system/` (ver [`docs/README.md`](../README.md)) permanece reservado para uma
Sprint futura dedicada exclusivamente ao catálogo de componentes reutilizáveis em si — este
documento cobre apenas o que é específico da integração com a Web (composição, interop, ordem de
CSS), não uma referência exaustiva de cada Parameter de cada componente.

## 2. Categorias de componentes

| Pasta | Componentes | Papel |
|---|---|---|
| `Buttons/` | `BeeDayButton` | Único botão estilizado do sistema — 8 `BeeDayButtonVariant` |
| `Cards/` | `BeeDayCard`, `BeeDayCardMenu` | Container genérico; menu de ações posicionado dinamicamente (interop) |
| `Forms/` | `BeeDayInput`, `BeeDayCheckbox`, `BeeDayDateInput`, `BeeDaySelect`, `BeeDayTextArea`, `BeeDayValidationMessage<TValue>` | Wrappers de `InputBase`/`EditForm`, todos com `Expression<Func<...>>` para bind a `EditContext` |
| `Feedback/` | `BeeDayToastHost`, `BeeDayLoading`, `BeeDaySkeleton`, `BeeDayDashboardSkeleton`, `BeeDayEmptyState`, `BeeDayConfirmDialog` | Estados assíncronos e vazios |
| `Icons/` | `PixelIcon`, `PixelIconRegistry` | Sprite SVG único (`/icons/sprite.svg`) — ver §4 |
| `Layout/` | `BeeDayHero`, `BeeDayPageHeader`, `BeeDaySectionHeader`, `BeeDaySettingsForm<TModel>`, `BeeDaySettingsSection` | Blocos de página compartilhados entre Account/Wallet/Dashboard |
| `Modals/` | `EditorModalShell` | Esqueleto comum aos 4 editores de atividade (ver `04-feature-components.md` §5) |
| `Text/` | `BeeDayBrand`, `SearchHighlight` | Marca oficial; realce de termo buscado |
| `Pages/` | `IconCatalog`, `HeroCatalog` | Catálogos visuais roteáveis — ver `02-routing-and-pages.md` §6 |

## 3. Ordem de carregamento de CSS (`App.razor`)

```text
app.css
→ css/variables.css              (custom properties: cores, espaçamento, tipografia)
→ css/design-system.css          (base do Design System)
→ css/activity-design-system.css (tokens específicos de Habit/Task/Todo/Project)
→ css/pixel-ui.css
→ css/typography.css
→ css/editor-modal.css
→ css/forms.css
→ css/settings.css
→ css/cards.css
→ css/feedback.css
→ css/dragdrop.css
→ css/theme.css
→ css/utilities.css
→ css/animations.css
→ css/polish.css
→ css/wallet.css
→ css/identity.css
→ BeeDay.Web.styles.css          (bundle isolado, gerado pelo SDK a partir de todo *.razor.css)
→ css/typography-policy.css
→ css/pixel-nes.css              (excerto do tema NES.css — ver css/vendor/NES_ATTRIBUTION.md)
```

`variables.css` precisa carregar antes de qualquer folha que consuma `var(--beeday-*)` — é a
primeira folha específica do projeto (depois só de `app.css`, que é o CSS isolado padrão do
projeto Blazor). `BeeDay.Web.styles.css` (isolamento de CSS por componente, `*.razor.css`) carrega
deliberadamente **depois** das 15 folhas globais, para que overrides por componente vençam em caso
de conflito de especificidade igual.

## 4. `PixelIcon` / `PixelIconRegistry`

Um único `<svg><use href="/icons/sprite.svg#{symbolId}" /></svg>` por ícone — `PixelIconRegistry`
mapeia 60 valores de `PixelIconName` para `(symbolId, assetPath, PixelIconCategory,
semanticName)`; `Resolve` cai para `PixelIconName.Warning` se o nome não existir no dicionário
(nunca lança). `PixelIcon.razor.cs` recusa renderizar sem `Label` quando `Decorative="false"`
(`InvalidOperationException` em `OnParametersSet`) — força que todo ícone não-decorativo declare
texto acessível.

## 5. Interop JavaScript — os 2 módulos

Todo componente com JS interop segue o mesmo padrão: import dinâmico de módulo ES
(`JS.InvokeAsync<IJSObjectReference>("import", "./js/...")`) no primeiro `OnAfterRenderAsync`,
`DotNetObjectReference` quando o JS precisa chamar de volta para C# (`[JSInvokable]`), e
`DisposeAsync` que engole `JSDisconnectedException` (o circuito já pode ter caído quando o
`Dispose` roda).

| Componente | Módulo | Direção da chamada | Propósito |
|---|---|---|---|
| `Behaviors/DragDrop/BeeDaySortable.razor` | `js/beeday-sortable.js?v=20260721-f13-dragfix` | JS → C# (`[JSInvokable] NotifyReorderAsync`) | Drag-and-drop de cards; C# nunca lê posição do mouse, só recebe o resultado final (`itemId`, `targetItemId`, `placeAfter`) |
| `DesignSystem/Cards/BeeDayCardMenu.razor` | `js/beeday-card-menu.js?v=20260729-1` | C# → JS (`measureGeometry`) e JS → C# (`[JSInvokable] NotifyOutsideClickAsync`) | Mede `getBoundingClientRect` do trigger/painel para decidir abrir para cima/baixo e deslocamento horizontal (`CardMenuPlacementCalculator`, lógica pura, testável sem DOM); detecta clique fora do menu |
Os dois módulos usam sufixo de versão hardcoded na própria string de import
(`?v=20260721-f13-dragfix`, `?v=20260729-1`) como cache-busting manual.
Nenhum dos 2 módulos é auditado linha a linha nesta Sprint (escopo: código C#/Razor, não os
arquivos `.js` em si).

`CardActionMenuCoordinator` (ver `01-composition-root.md` §6) é o que faz múltiplos
`BeeDayCardMenu` num mesmo circuito se fecharem mutuamente — um evento C#-para-C#, não JS.

## 6. Formulários

Todo wrapper em `Forms/` (`BeeDayInput`, `BeeDayDateInput`, etc.) segue o mesmo contrato:
`[Parameter, EditorRequired] Id`, `Value`/`ValueChanged`/`ValueExpression` (bind two-way manual,
não `InputBase<T>` herdado), e `[Parameter(CaptureUnmatchedValues = true)]
AdditionalAttributes`. `BeeDayValidationMessage<TValue>` é a exceção: usa
`[CascadingParameter] EditContext` real (lança se ausente) e se inscreve em
`EditContext.OnValidationStateChanged`, replicando o que `Microsoft.AspNetCore.Components.Forms.ValidationMessage`
já faz — reimplementado aqui só para aplicar a classe CSS do Design System em vez do wrapper
padrão do framework.

`BeeDaySettingsForm<TModel>` e `EditorModalShell` (ambos genéricos/com `Model` livre) são os dois
pontos onde um `EditForm` é montado uma vez e reaproveitado por múltiplas Features (`Account` usa
`BeeDaySettingsForm<TModel>` três vezes; os 4 editores de atividade usam `EditorModalShell`).

## 7. Fontes de verdade

- Todos os arquivos sob `src/BeeDay.Web/Components/DesignSystem/` e
  `src/BeeDay.Web/Components/Behaviors/DragDrop/`.
- `src/BeeDay.Web/Components/App.razor` (ordem de `<link rel="stylesheet">`).
- `src/BeeDay.Web/wwwroot/js/*.js` (nomes de arquivo e strings de import, não o conteúdo interno).
