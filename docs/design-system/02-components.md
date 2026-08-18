# Component Library

**Fonte da verdade:** todos os `.razor`/`.razor.cs` sob
`src/BeeDay.Web/Components/DesignSystem`, mais `Components/Behaviors/DragDrop/BeeDaySortable`.
Revalidado em 2026-08-16 pelas Sprints 25.8–25.12; contrato de backdrop revalidado na Sprint 29.3
(2026-08-17).

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
| `BeeDayConfirmDialog` | Confirmação destrutiva. `IsOpen`, `IsBusy`, `Title`, `Message`, textos opcionais, labels opcionais, callbacks | 5 | open/busy; `alertdialog`, modal, labelled/described/busy; foco inicial em Cancel, trap, Escape/backdrop e restore. Backdrop `--beeday-color-overlay` (claro/neutro/translúcido, canônico desde a Sprint 29.3 — não mais um roxo hardcoded). `BeeDayConfirmDialogTests` + E2E; `feedback.css`/focus scope |
| `BeeDayToastHost` | Host da fila de `ToastService`; sem parâmetros | 3 layouts | `status`/`alert`, live region e dismiss nativo com target 44px. `BeeDayToastHostTests`; `feedback.css` |
| `BeeDayLoading` | Feedback global controlado por `IsVisible`, `Label?` | 4 | `role=status`, polite live, label localizada; reduced motion mantém conteúdo. `FeedbackComponentTests`; `feedback.css` |
| `BeeDaySkeleton` | Placeholder com `Lines`, `Class?` | 2 | busy visual, não interativo; shimmer reduzido. `FeedbackComponentTests`; `feedback.css` |
| `BeeDayDashboardSkeleton` | Composição de skeleton; `AriaLabel?` | 2 | section labelled + busy; label vem do caller ou fallback. `BeeDayDashboardSkeletonTests`; `feedback.css` |
| `BeeDayEmptyState` | Empty state com `Title`, `Description`, `Icon?`, `Class?` | 3 | conteúdo semântico, ação composta externamente. `FeedbackComponentTests`; CSS isolado |

### Forms, icon e layout

| Primitive | Responsabilidade e contrato público | Consumers | Estados, a11y, responsive, teste e CSS |
|---|---|---:|---|
| `BeeDayInput` | `Id`, `Label`, `Placeholder`, classes, `MaxLength`, `Required`, `Disabled`, `ReadOnly`, `UpdateOnInput`, validation flag, bind + attributes | 9 | default/hover/focus/disabled/readonly/invalid; live-search opt-in fora de EditContext. `BeeDayFormTests`; `forms.css` |
| `BeeDayTextArea` | Input contract + `CounterCssClass`, `ShowCounter` | 5 | mesmos estados; counter/maxlength. `BeeDayFormTests`; `forms.css` |
| `BeeDaySelect<T>` | `Id`, `Label`, classes, `Required`, `Disabled`, validation flag, bind, options, attributes | 5 | focus/disabled/invalid/selected nativo. `BeeDayFormTests`; `forms.css` |
| `BeeDayDateInput<T>` | `Id`, `Label`, classes, `Required`, `Disabled`, validation flag, bind + attributes | 3 | date nativo com label/invalid. `BeeDayFormTests`; `forms.css` |
| `BeeDayCheckbox` | `Id`, `Label`, classes, `Disabled`, validation flag, bind + attributes | 0 | input checkbox real, checked/focus/disabled/invalid; preservado/testado. `BeeDayFormTests`; `forms.css` |
| `BeeDayValidationMessage<T>` | `For` obrigatório | 10, incluindo wrappers | mensagem associada ao EditContext. `BeeDayFormTests`; `forms.css` |
| `BeeDayIcon` | `Name`, `Size`, `Color`, `Decorative`, `Label?`, `Class?`, attributes | 32 | 5 sizes, 8 color roles; decorative usa `aria-hidden`, semântico usa `role=img` + label. `BeeDayIconTests`; sprite registry + CSS isolado |
| `BeeDayPageHeader` | `Title`, `Eyebrow?`, `Description?`, `Actions?`, `Class?`, attributes | 1 | h1, actions refluem em 42rem. `BeeDayHeaderTests`; `design-system.css`/`polish.css` |
| `BeeDaySectionHeader` | Mesmo shape do PageHeader em escala h2 | 1 direto | heading de seção; responsive compartilhado. `BeeDayHeaderTests`; `design-system.css`/`polish.css` |
| `BeeDayHero` | `Title`, `Eyebrow?`, `Subtitle?`, `Surface?` (`BeeDayPaletteToken`, restrito a `Cor0`/`Cor8` para o papel de page header — [`brand/03-color-palette.md`](../brand/03-color-palette.md)), `HeaderNav?` (Sprint 29.4 — navegação contextual na mesma linha de `BrandContext`, extremidade oposta), illustration/action/support slots, `Variant`, `Compact`, `Class?`, attributes | 13 (InstitutionalPageShell × 12 rotas incluindo Brand guidelines, ExperienceSystemHome) + Wallet | `Default`/`Onboarding`; illustration some no compacto; full-bleed via `--beeday-hero-bleed-inset` (margin-inline negativo, sem `width` explícito — opt-in por container: `.editorial-layout__main` em `polish.css`, Sprint 29.4). `BeeDayHeroTests`, `InstitutionalPageShellTests`; `BeeDayHero.razor.css` |
| `BeeDaySettingsSection` | `Title`, `Eyebrow?`, `Description?`, `Class?`, `ChildContent` | 3 | compõe Card + SectionHeader. `BeeDaySettingsTests`; `settings.css` |
| `BeeDaySettingsForm<T>` | `Model`, `FormName`, `SubmitLabel`, `IsBusy`, button class, valid-submit callback, content | 3 | fieldset disabled/busy e submit canônico. `BeeDaySettingsTests`; `settings.css` |

### Modal, text, progress e behavior

| Primitive | Responsabilidade e contrato público | Consumers | Estados, a11y, responsive, teste e CSS |
|---|---|---:|---|
| `EditorModalShell` | `Model`, `Title`, `TitleId`, `SubmitLabel?`, `ShowDelete`, `IsBusy`, 3 slots e 3 callbacks | 6 | modal labelled/busy; foco inicial no primeiro field, trap, Escape, nested-confirm e restore; delete mantém 44px. Backdrop `--beeday-color-overlay`, mesmo token de `BeeDayConfirmDialog`/`BeeDayFeedbackModal`/`MobileSidebar` (Sprint 29.3). `SecondaryAction` não deve receber `Compact` — o mesmo footer já usa a escala padrão em Cancel/Delete. `EditorModalShellTests` + E2E; `editor-modal.css`/focus scope |
| `BeeDayBrand` | `OnDarkSurface` compatível | 11 | wordmark textual `beeday`, `role=img`, label fixa lowercase; sem variants de cor. `BeeDayBrandTests`; CSS isolado |
| `SearchHighlight` | `Text`, `SearchTerm` | 2 | `<mark>` para matches case-insensitive; sem estado interativo. `SearchHighlightTests`; `animations.css` |
| `BeeDayProgressBar` | `Label`, `AriaLabel?`, `Value`, `Maximum`, `ValueText?`, `Tone` | 4 | `Primary`/`Reward`; empty/partial/complete/unavailable; label visível pode ter contexto acessível mais específico. `BeeDayProgressBarTests`; CSS isolado |
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
- dialogs canônicos usam `DialogFocusScope` + `beeday-dialog-focus.js`: OPEN → initial focus →
  contenção de Tab/Shift+Tab → Escape/close com busy guard → restore quando o trigger ainda existe;
  trigger removido e escopo sem controles degradam sem erro.

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

Após a convergência Daily/Project da Sprint 25.12, o baseline tem **43 tags nativas em 20 arquivos**:
29 `<button>`, 14 `<input>`, zero `<select>` e zero `<textarea>` direto.

| Classificação | Quantidade | Exemplos e decisão |
|---|---:|---|
| `FRAMEWORK / INTERNAL` | 14 | internals de Button/CardMenu/Toast/Input live-search; Reconnect; culture form; triggers do shell. Permanecem nativos |
| `LEGITIMATE SPECIALIZED WIDGET / ADAPTER` | 26 | activity checkbox/score, menus, drag/project toolbar, color controls e os 8 adapters HTML de Login/ProfileCreation convergidos visualmente na 25.9 |
| `DESIGN-SYSTEM DUPLICATION` / migration candidate | 3 | Casos remanescentes sem contrato equivalente confirmado; revisão final na 25.16 |

Wallet eliminou seus selects/dates/busca paralelos; o picker de cor e o valor monetário continuam
especializados. Daily eliminou o chrome paralelo da busca; menu, score, completion e listbox
continuam especializados porque codificam interação real de produto.

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
drawer/menu/modal/Daily. A Sprint 25.10 testa initial focus, trap nos dois sentidos, Escape,
nested-dialog restore, trigger removido, dialog sem controles, busy e ARIA lowercase. Testes
priorizam markup, callbacks, ARIA e estados públicos; detalhes de classe só são fixados quando
representam variants/tokens do contrato.
