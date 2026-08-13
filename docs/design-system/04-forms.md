# Forms

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/DesignSystem/Forms/`
(6 componentes) e `src/BeeDay.Web/wwwroot/css/forms.css`, `polish.css`, `editor-modal.css`,
`identity.css` (cada área de produto tem seu próprio CSS de formulário — ver §5).

**Última verificação:** 2026-08-12 (Sprint 21.5, EPIC 21 — Interactive Components).

## 1. Objetivo

Documentar o contrato comum aos 6 componentes de formulário do Design System, como cada um se liga
a `EditContext`/`DataAnnotations`, seus estados visuais, e como `BeeDayButton` participa de um
formulário.

## 2. Os 6 componentes e seu contrato comum

| Componente | Tipo de valor | Genérico? |
|---|---|---|
| `BeeDayInput` | `string?` | Não |
| `BeeDayTextArea` | `string?` | Não |
| `BeeDayCheckbox` | `bool` | Não |
| `BeeDayDateInput<TValue>` | `TValue` | Sim |
| `BeeDaySelect<TValue>` | `TValue` | Sim |
| `BeeDayValidationMessage<TValue>` | `TValue` (via `For`) | Sim |

Os 5 primeiros compartilham o mesmo padrão de parâmetros: `Id` (`EditorRequired`), `Label`,
3 parâmetros de classe CSS sobrescrevíveis (`FieldCssClass` = `"beeday-field"`,
`LabelCssClass` = `"beeday-field__label"`, `InputCssClass` = `"beeday-field__control"` —
`BeeDayCheckbox` usa `"beeday-checkbox"`/`"beeday-checkbox__label"`/`"beeday-checkbox__control"` em
vez disso), `Disabled`, `ShowValidationMessage` (padrão `true`), `Value`/`ValueChanged`/
`ValueExpression` (bind two-way manual — **nenhum herda `InputBase<T>`** do framework;
`ValueExpression` é declarado `[EditorRequired]` em todos, então o componente sempre sabe a que
campo do `EditContext` está ligado, mesmo sem herdar a classe base do ASP.NET Core). Todos aceitam
`AdditionalAttributes` (`CaptureUnmatchedValues`).

Parâmetros específicos: `BeeDayInput` tem `Placeholder`, `MaxLength`, `Required`, `ReadOnly`;
`BeeDayTextArea` tem os mesmos mais `ShowCounter`/`CounterCssClass`; `BeeDayCheckbox` não tem
`Placeholder`/`MaxLength`/`ReadOnly` (não fazem sentido para um booleano); `BeeDaySelect<TValue>`
tem `ChildContent` (as `<option>`, fornecidas pelo consumidor — o componente não gera opções
sozinho).

## 3. `BeeDayValidationMessage<TValue>` — reimplementação, não wrapper

Diferente dos outros 5, não segue o padrão `Value`/`ValueChanged` — usa
`[CascadingParameter] EditContext` real (lança `InvalidOperationException` se ausente) e
`For` (`Expression<Func<TValue>>`, `EditorRequired`). Reimplementa o que
`Microsoft.AspNetCore.Components.Forms.ValidationMessage<TValue>` do framework já faz
(`FieldIdentifier.Create(For)`, assinar `EditContext.OnValidationStateChanged`, reagir com
`StateHasChanged`) — a única razão observável para a reimplementação é aplicar a classe CSS do
Design System (`beeday-validation-message`, com ícone) em vez do `<div class="validation-message">`
padrão do framework. Isso é duplicação de lógica de framework por motivo puramente visual — não é
um bug, mas é uma escolha arquitetural que aumenta a superfície de manutenção (qualquer mudança de
comportamento do `ValidationMessage` nativo do .NET não se propaga automaticamente aqui).

## 4. Estados visuais (`forms.css`, `polish.css`)

| Estado | Regra CSS |
|---|---|
| Default | altura mínima 48px, borda neutra 2px, radius 12px, surface sólida, sem sombra |
| `:focus-visible` | borda interactive + `--beeday-focus-ring`; não depende apenas de cor |
| `:disabled`/`[readonly]` | cursor bloqueado, surface/text muted e opacidade controlada |
| `:hover` (não desabilitado) | borda neutra forte, transition global fast |
| Erro | borda danger via `.invalid`/`aria-invalid=true`, além da mensagem com `role=alert` |
| Erro de validação | `.beeday-validation-message` — texto `var(--beeday-color-danger)`, ícone à esquerda, peso bold |

`forms.css` é agora o único owner da geometria/motion das primitives; regras duplicadas de fields
foram removidas de `polish.css` e `pixel-ui.css`. `polish.css` mantém somente layout/touch policy.
Ele unifica a altura mínima de todo controle de formulário
(`--beeday-control-height-md`, 3rem) e a eleva para 3rem fixo sob `(pointer: coarse)` — telas de
toque recebem alvos maiores independente do valor base.

### 4.1 `BeeDayCheckbox` — padrão de substituição visual

`forms.css` implementa o checkbox como um `<input type="checkbox">` real, visualmente oculto
(`position: absolute; width: 1px; height: 1px; opacity: 0`, não `display: none` — mantém o elemento
focável e anunciável por leitor de tela), mais um `<span class="beeday-checkbox__visual">` com dois
`BeeDayIcon` sobrepostos na mesma célula de grid (`CheckboxChecked`/`CheckboxUnchecked`), alternando
opacidade via `:checked +`. `:focus-visible` no input real aplica outline no `__visual` irmão — o
indicador de foco do teclado nunca desaparece, mesmo com o controle nativo oculto.

## 5. CSS de formulário é fragmentado por área de produto

As primitives oficiais e o Login agora consomem `forms.css`; os editores foram alinhados aos mesmos
valores de height/border/radius/focus sem reestruturar sua composição. Ainda existem casos
especializados em Identity e Wallet. Historicamente, cada área reimplementava
sua própria versão do mesmo padrão visual (campo com borda, foco, erro):

| Arquivo | Escopo | Reaproveita `.beeday-field`? |
|---|---|---|
| `forms.css` | Componentes `Forms/` do Design System | É a origem de `.beeday-field*` |
| `editor-modal.css` | Os 4 editores de atividade (Habit/Task/Todo/Project) | Não — declara `.editor-modal__hero input`, `.editor-modal__field input` com seu próprio border/box-shadow/focus, valores próximos mas não idênticos aos de `.beeday-field__control` |
| `identity.css` | As 7 páginas de Login/Identity | Não — declara `.identity-field input` com seu próprio conjunto de regras, incluindo um `background: var(--beeday-color-surface-subtle)` que os outros dois não têm |
| `wallet.css` | `WalletFilters`, `TransactionFormModal`, `TagFormModal` | Não — `.wallet-filters input/select` com regras próprias, mais compactas |

As 4 implementações convergem visualmente (mesma paleta de token, mesma ideia de borda + foco) mas
divergem em detalhe (raio, box-shadow exato, cor de fundo em repouso) porque nenhuma delas
referencia as outras — um ajuste em `.beeday-field__control` não se propaga para
`.editor-modal__field input`, `.identity-field input` ou `.wallet-filters input`. Não é um bug
funcional (cada tela renderiza corretamente), mas é 4 pontos de manutenção para uma única
intenção visual.

## 6. Botões dentro de formulários

`BeeDayButton` (documentado por completo em
[`02-components.md`](02-components.md#2-buttons)) é o único botão de submit/ação usado dentro de
`EditForm`/`BeeDaySettingsForm<TModel>`/`EditorModalShell`. Padrões observados:

- Submit: `<BeeDayButton Type="submit" IsLoading="@_busy" Disabled="@_busy">` — o mesmo booleano
  controla `IsLoading` e `Disabled` em todo formulário auditado, nunca só um dos dois.
  `Login.razor` é a única exceção: é HTML puro (não `EditForm`), então o "loading" é feito por um
  `onsubmit` inline em JavaScript vanilla que desabilita o botão (ver
  [`docs/web/04-feature-components.md`](../web/04-feature-components.md) §7), não por
  `IsLoading`.
- Cancelar/voltar: `BeeDayButtonVariant.Back` ou o modificador `--plain`/`--plain-neutral`.
- Excluir: `BeeDayButtonVariant.ConfirmationDanger`/`ConfirmationCancel`, sempre dentro de um
  `BeeDayConfirmDialog`, nunca como ação direta de um único clique.

## 7. Validação — `DataAnnotations` vs. `FluentValidation`

Todo formulário de produto usa `<EditForm Model="..." OnValidSubmit="...">` +
`<DataAnnotationsValidator />` — a validação client-side (Web) é sempre `DataAnnotations`
(`[Required]`, `[EmailAddress]`, `[MinLength]`, `[Compare]`), nunca `FluentValidation` diretamente
no componente Razor. `FluentValidation` existe na camada Application (ver
[`docs/application/03-pipeline.md`](../application/03-pipeline.md)) e roda de novo, no servidor,
quando o comando chega via `BeeDayWebService`/`ISender` — ou seja, toda submissão passa por 2
validações independentes (`DataAnnotations` no Blazor, `FluentValidation` no handler), não uma
delegando para a outra. Um erro só de `FluentValidation` (regra que `DataAnnotations` não expressa)
aparece como exceção capturada no `catch` do método de submit do componente, mostrada via
`ToastService.ShowError` — não como uma `ValidationMessage` inline por campo.

## 8. Fontes consultadas

- `src/BeeDay.Web/Components/DesignSystem/Forms/BeeDayInput.razor(.cs)`, `BeeDayCheckbox.razor(.cs)`,
  `BeeDayDateInput.razor(.cs)`, `BeeDaySelect.razor(.cs)`, `BeeDayTextArea.razor(.cs)`,
  `BeeDayValidationMessage.razor(.cs)`.
- `src/BeeDay.Web/wwwroot/css/forms.css`, `polish.css`, `editor-modal.css`, `identity.css`,
  `wallet.css` (seletores `.wallet-filters input/select`).
- [`docs/web/04-feature-components.md`](../web/04-feature-components.md) (padrões de submit por
  página, reaproveitado da Sprint 16.7).
- [`docs/application/03-pipeline.md`](../application/03-pipeline.md) (validação FluentValidation no
  servidor, referenciada não duplicada).
