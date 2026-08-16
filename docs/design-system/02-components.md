# Component Library

**Fonte da verdade:** todos os `.razor`/`.razor.cs` sob
`src/BeeDay.Web/Components/DesignSystem`, mais `Components/Behaviors/DragDrop/BeeDaySortable`.
Revalidado em 2026-08-16 pela Sprint 25.8.

## 1. Inventário atual

Existem **25 primitives físicas** em `Components/DesignSystem` (excluídas as páginas de catálogo
`HeroCatalog` e `IconCatalog`) e um comportamento compartilhado fora da pasta, `BeeDaySortable`:
**26 contratos** no total. Não existe componente `V2` nem biblioteca paralela.

Forms e Icon têm detalhes adicionais em [`04-forms.md`](04-forms.md) e
[`03-icons.md`](03-icons.md). “Consumers” abaixo conta arquivos Razor de produção com uso direto;
um zero significa primitive preservada/testada sem consumer runtime atual.

### Actions, cards e feedback

| Primitive | Responsabilidade e contrato público | Consumers | Estados, a11y, responsive, teste e CSS |
|---|---|---:|---|
| `BeeDayButton` | Ação canônica. `Variant`, `Type`, `Disabled`, `IsLoading`, `FullWidth`, `Compact`, `Icon`, `IconSize`, `Class`, `OnClick`, `ChildContent`, attributes | 22 | 8 variants; default/hover/pressed/focus/disabled/loading. `<button>`, `disabled`, `aria-busy`; loading preserva o label acessível. `BeeDayButtonTests`; `design-system.css` |
| `BeeDayCard` | Surface sem estado interno. `Class`, `Padded`, `Muted`, `Prominent`, `Interactive`, `ChildContent`, attributes | 9 | `<article>`; interactive oferece chrome/focus, mas semântica/teclado pertencem ao consumer. `BeeDayCardTests`; `design-system.css` |
| `BeeDayCardMenu` | Menu Edit/Delete posicionado. `Title`, `Class`, `TriggerClass`, `Disabled`, `OnEdit`, `OnDelete`, `OpenChanged` | 0 | trigger nativo, `aria-expanded`, `role=menu/menuitem`, Escape/outside click, medição JS. Mantido por contrato/testes. `BeeDayCardMenuTests`/placement tests; CSS isolado |
| `BeeDayConfirmDialog` | Confirmação destrutiva. `IsOpen`, `IsBusy`, `Title`, `Message`, textos opcionais, labels opcionais, callbacks | 5 | open/busy; `alertdialog`, modal, labelled/described, Escape/backdrop. Focus lifecycle profundo → 25.10. `BeeDayConfirmDialogTests`; `feedback.css` |
| `BeeDayToastHost` | Host da fila de `ToastService`; sem parâmetros | 3 layouts | `status`/`alert`, live region e dismiss nativo. `BeeDayToastHostTests`; `feedback.css` |
| `BeeDayLoading` | Feedback global controlado por `IsVisible`, `Label?` | 4 | `role=status`, polite live, label localizada; reduced motion mantém conteúdo. `FeedbackComponentTests`; `feedback.css` |
| `BeeDaySkeleton` | Placeholder com `Lines`, `Class?` | 2 | busy visual, não interativo; shimmer reduzido. `FeedbackComponentTests`; `feedback.css` |
| `BeeDayDashboardSkeleton` | Composição de skeleton; `AriaLabel?` | 2 | `role=status`, label localizada/caller. `BeeDayDashboardSkeletonTests`; `feedback.css` |
| `BeeDayEmptyState` | Empty state com `Title`, `Description`, `Icon?`, `Class?` | 3 | conteúdo semântico, ação composta externamente. `FeedbackComponentTests`; CSS isolado |

### Forms, icon e layout

| Primitive | Responsabilidade e contrato público | Consumers | Estados, a11y, responsive, teste e CSS |
|---|---|---:|---|
| `BeeDayInput` | `Id`, `Label`, `Placeholder`, classes, `MaxLength`, `Required`, `Disabled`, `ReadOnly`, validation flag, bind + attributes | 8 | default/hover/focus/disabled/readonly/invalid. Label + validation ligada ao EditContext. `BeeDayFormTests`; `forms.css` |
| `BeeDayTextArea` | Input contract + `CounterCssClass`, `ShowCounter` | 5 | mesmos estados; counter/maxlength. `BeeDayFormTests`; `forms.css` |
| `BeeDaySelect<T>` | `Id`, `Label`, classes, `Required`, `Disabled`, validation flag, bind, options, attributes | 5 | focus/disabled/invalid/selected nativo. `BeeDayFormTests`; `forms.css` |
| `BeeDayDateInput<T>` | `Id`, `Label`, classes, `Required`, `Disabled`, validation flag, bind + attributes | 3 | date nativo com label/invalid. `BeeDayFormTests`; `forms.css` |
| `BeeDayCheckbox` | `Id`, `Label`, classes, `Disabled`, validation flag, bind + attributes | 0 | input checkbox real, checked/focus/disabled/invalid; preservado/testado. `BeeDayFormTests`; `forms.css` |
| `BeeDayValidationMessage<T>` | `For` obrigatório | 10, incluindo wrappers | mensagem associada ao EditContext. `BeeDayFormTests`; `forms.css` |
| `BeeDayIcon` | `Name`, `Size`, `Color`, `Decorative`, `Label?`, `Class?`, attributes | 32 | 5 sizes, 8 color roles; decorative usa `aria-hidden`, semântico usa `role=img` + label. `BeeDayIconTests`; sprite registry + CSS isolado |
| `BeeDayPageHeader` | `Title`, `Eyebrow?`, `Description?`, `Actions?`, `Class?`, attributes | 1 | h1, actions refluem em 42rem. `BeeDayHeaderTests`; `design-system.css`/`polish.css` |
| `BeeDaySectionHeader` | Mesmo shape do PageHeader em escala h2 | 1 direto | heading de seção; responsive compartilhado. `BeeDayHeaderTests`; `design-system.css`/`polish.css` |
| `BeeDayHero` | `Title`, `Eyebrow?`, `Subtitle?`, illustration/action/support slots, `Variant`, `Class?`, attributes | catálogo apenas | `Default`/`Onboarding`; illustration some no compacto. `ActivityComponentsTests`; `design-system.css`/`polish.css` |
| `BeeDaySettingsSection` | `Title`, `Eyebrow?`, `Description?`, `Class?`, `ChildContent` | 3 | compõe Card + SectionHeader. `BeeDaySettingsTests`; `settings.css` |
| `BeeDaySettingsForm<T>` | `Model`, `FormName`, `SubmitLabel`, `IsBusy`, button class, valid-submit callback, content | 3 | fieldset disabled/busy e submit canônico. `BeeDaySettingsTests`; `settings.css` |

### Modal, text, progress e behavior

| Primitive | Responsabilidade e contrato público | Consumers | Estados, a11y, responsive, teste e CSS |
|---|---|---:|---|
| `EditorModalShell` | `Model`, `Title`, `TitleId`, `SubmitLabel?`, `ShowDelete`, `IsBusy`, 3 slots e 3 callbacks | 6 | modal labelled, busy, submit/cancel/delete, Escape. Focus lifecycle → 25.10. `EditorModalShellTests`; `editor-modal.css` |
| `BeeDayBrand` | `OnDarkSurface` compatível | 11 | wordmark textual `beeday`, `role=img`, label fixa lowercase; sem variants de cor. `BeeDayBrandTests`; CSS isolado |
| `SearchHighlight` | `Text`, `SearchTerm` | 2 | `<mark>` para matches case-insensitive; sem estado interativo. `SearchHighlightTests`; `animations.css` |
| `BeeDayProgressBar` | `Label`, `Value`, `Maximum`, `ValueText?`, `Tone` | 3 | `Primary`/`Reward`; empty/partial/complete/unavailable; `progressbar` + aria values. `BeeDayProgressBarTests`; CSS isolado |
| `BeeDaySortable` | `ItemIds`, template, reorder callback, `CollectionKey`, `AriaLabel`, `Class?`, virtualization params, `RemovingItemId` | 1 | drag/touch/keyboard ownership, removing state, JS result → C#. `SortableOrderTests` + E2E Daily; `dragdrop.css` |

## 2. Matriz formal de estados

`N/A` é intencional: components apresentacionais não ganham estados apenas para preencher matriz.

| Família | Default | Hover | Pressed | Focus-visible | Disabled | Loading/busy | Invalid | Selected | Expanded/open |
|---|---|---|---|---|---|---|---|---|---|
| Button | sim | sim | sim | sim | sim | sim | N/A | N/A | N/A |
| Interactive Card | sim | sim | consumer | sim | consumer | consumer | N/A | consumer | N/A |
| CardMenu | sim | sim | trigger | sim | sim | measuring | N/A | N/A | sim |
| Input/TextArea/Date | sim | sim | N/A | sim | sim | form owner | sim | N/A | N/A |
| Select/Checkbox | sim | sim | checkbox | sim | sim | form owner | sim | sim | select nativo |
| Confirm/Editor modal | fechado | N/A | actions | sim | busy | sim | form owner | N/A | sim |
| Loading/Skeleton/Toast | conforme presença | dismiss toast | N/A | dismiss | N/A | próprio estado | N/A | N/A | host-controlled |
| Progress | sim | N/A | N/A | N/A | N/A | value update | unavailable | complete/tone | N/A |
| Headers/Brand/Icon/Text | sim | N/A | N/A | N/A | N/A | N/A | N/A | N/A | N/A |
| Sortable | sim | drag affordance | drag | keyboard target | owner | removing | N/A | drag item | N/A |

Regras transversais:

- hover nunca substitui focus-visible;
- loading/busy mantém nome/feedback acessível e bloqueia callbacks incompatíveis;
- disabled não dispara ação;
- invalid pertence a controles integrados ao `EditContext`, não a Card/Button;
- selected/expanded deve existir em atributo/semântica, não somente em cor;
- dialogs têm contrato básico aqui; trap/restore/inert aprofundados pertencem à 25.10.

## 3. Contratos de composição

- Use Button para ações; links continuam links quando navegam.
- Card é content surface, não substitui panel/dialog. `Interactive` não inventa `role` ou teclado:
  o consumer deve fornecer semântica coerente ou usar um controle interno.
- Menu é uma ação contextual e não um select. Seu estado aberto é coordenado por
  `CardActionMenuCoordinator` e posicionado por medição real.
- Headers possuem hierarquia h1/h2; Hero é composição rica, não cabeçalho default.
- Feedback vazio/loading/skeleton não calcula regra de domínio.
- Progress recebe valores prontos; não calcula XP/task completion.
- Forms permanecem uma família única; Auth/Identity converge na 25.9, sem wrappers `V2`.

## 4. Inventário de controles nativos

O baseline tem **49 tags nativas em 20 arquivos**: 29 `<button>`, 17 `<input>`, três `<select>` e
zero `<textarea>` direto.

| Classificação | Quantidade | Exemplos e decisão |
|---|---:|---|
| `FRAMEWORK / INTERNAL` | 13 | internals de Button/CardMenu/Toast; Reconnect; culture form; triggers do shell. Permanecem nativos |
| `LEGITIMATE SPECIALIZED WIDGET` | 18 | activity checkbox/score, menus de filtro, drag/project toolbar e color controls. Sem migração genérica |
| `DESIGN-SYSTEM DUPLICATION` / migration candidate | 18 | Auth/ProfileCreation/Account inputs e Wallet filters/tag input. Owners: 25.9 e 25.11 |

Dashboard search e menu triggers continuam especializados e são revistos com Daily na 25.12.
Nenhum native control foi migrado nesta Sprint: igualdade de tag não prova equivalência de contrato.

## 5. Foundations, responsive e localização

Shared primitives consomem tokens de Color/Typography/Shape/Motion/Layer estabelecidos nas 25.3–
25.7. CSS global é owner de Button/Card/Headers/Forms/Feedback/Editor/Settings/Sortable; CSS isolado
é owner de Menu/Empty/Icon/Progress/Brand. Feature CSS pode compor layout, não redefinir o chrome.

Button/Card/Headers refluem pelos contratos 42rem/40rem documentados em
[`docs/ux/03-responsive.md`](../ux/03-responsive.md). Strings internas de Menu, Confirm, Loading,
Skeleton e Toast vêm de `DesignSystemResources` en-US/pt-BR. Demais textos são parâmetros e devem
chegar localizados pelo consumer. Brand casing é contrato fixo, não tradução.

## 6. Catálogos e testes

`HeroCatalog` e `IconCatalog` são páginas técnicas de desenvolvimento, não Brand Guidelines nem
primitives contadas. Não foram expandidas nesta Sprint porque os contratos reais já são melhor
cobertos por bUnit/E2E.

Cobertura relevante: Button, Card/Menu/placement, Forms, Icon, Feedback/Toast/Confirm/Skeleton,
Headers/Settings/Hero, EditorModal, Progress, Brand, SearchHighlight, SortableOrder e fluxos E2E de
drawer/menu/modal/Daily. Testes priorizam markup, callbacks, ARIA e estados públicos; detalhes de
classe só são fixados quando representam variants/tokens do contrato.
