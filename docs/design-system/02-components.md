# Component Library

**Fonte da verdade:** verificado diretamente em cada arquivo `.razor`/`.razor.cs` sob
`src/BeeDay.Web/Components/DesignSystem/`, mais `src/BeeDay.Web/Components/Behaviors/DragDrop/`
para `BeeDaySortable` (fisicamente fora desta pasta, mas documentado aqui por ser
interop-equivalente aos demais). Componentes de Forms e o `PixelIcon` têm parâmetros completos em
[`04-forms.md`](04-forms.md) e [`03-icons.md`](03-icons.md) respectivamente — este documento os
resume e linka em vez de duplicar (`docs/CONVENTIONS.md` §12).

**Última verificação:** 2026-08-12 (Sprint 20.6, EPIC 20) — §2 (`BeeDayButton`) e §3 (`BeeDayCard`)
atualizados: novo modificador opt-in `--soft` em ambos, target visual da página-modelo (ver
`docs/epics/20-home-visual-experience/README.md`). Correção na mesma Sprint: a cor sob `--soft`
migrou de `--beeday-color-primary` (legado) para a família canônica `--beeday-color-brand-primary`
(ver `01-foundations.md` §2.1/§3) nos dois primeiros consumidores (`PublicHeader`, `Home.razor`); o
modificador `--soft` em si (forma/radius/shadow) não mudou. Verificação anterior: 2026-08-11 (Sprint 20.5) —
§5 (`BeeDayHero`) atualizado: primeiro consumidor de produto real (`Home.razor`, rota `/`). Demais
seções preservadas da verificação de 2026-08-07.

## 1. Objetivo

Catálogo dos 26 componentes reutilizáveis do Design System: para cada um, objetivo, parâmetros,
estados internos, eventos, dependências, interop JS (quando existe) e quem consome.

## 2. Buttons

### `BeeDayButton`

Único botão estilizado do sistema — todo botão de ação do produto passa por ele.

| Parâmetro | Tipo | Padrão | Notas |
|---|---|---|---|
| `Variant` | `BeeDayButtonVariant` | `Primary` | 8 valores: `Primary`, `Secondary`, `Success`, `Warning`, `Back`, `Danger`, `ConfirmationDanger`, `ConfirmationCancel` |
| `Type` | `string` | `"button"` | Atributo HTML `type` |
| `Disabled`, `IsLoading` | `bool` | `false` | `IsDisabled` combina os dois; `IsLoading` também desabilita |
| `FullWidth`, `Compact` | `bool` | `false` | Modificadores de layout |
| `Icon` | `PixelIconName?` | `null` | Ícone opcional antes do texto |
| `IconSize` | `PixelIconSize` | `Small` | — |
| `Class` | `string?` | `null` | Classes extras — é assim que os modificadores opt-in (`beeday-button--comic`, `--comic-blue`, `--skew-press`, `--comic-press`, `--plain`, `--plain-danger`, `--plain-neutral`) são aplicados, já que não existem como `Variant` |
| `OnClick` | `EventCallback<MouseEventArgs>` | — | Não dispara se `IsDisabled` |
| `ChildContent` | `RenderFragment?` | — | Texto/conteúdo do botão |
| `AdditionalAttributes` | `IReadOnlyDictionary<string,object>?` | — | `CaptureUnmatchedValues` |

**Estados visuais, default canônico desde a Sprint 20.8** (`design-system.css`): sem borda,
`border-radius: pill`, sombra `--beeday-shadow-md`; `:hover` (`translateY(-2px)` + `shadow-lg`),
`:active` (`translateY(0)` + `shadow-sm`), `:focus-visible` (`shadow-md` + `--beeday-focus-ring`,
anel canônico azul), `:disabled` (paleta cinza fixa, `cursor: not-allowed`, opacidade .62), loading
(ícone `PixelIconName.Loading` com `beeday-spin` — `steps(8, end)`, respeitando
`prefers-reduced-motion`). **Decisão final de default (Sprint 20.8, EPIC 20):** este era o
modificador opt-in `--soft` introduzido na Sprint 20.6 — auditados repo-wide todos os 40+
consumidores de `<BeeDayButton` antes de decidir; a grande maioria já usa um dos modificadores de
forma abaixo (que se auto-declaram por completo e são, portanto, totalmente não afetados), então o
modificador foi incorporado ao default (removido como classe separada) sem risco real para eles —
apenas ~9 consumidores sem modificador de forma (paginação/estado vazio do Wallet, submits de
recuperação de senha, navegação do Tutorial) passam a herdar a nova aparência, exatamente a
convergência pretendida para ações secundárias/utilitárias.

**Modificadores opt-in via `Class`** (não são `Variant` — são combináveis por cima de qualquer
variante; cada um redeclara sua própria forma por completo — borda/radius/shadow — e por isso não é
afetado pelo default acima): `--skew-press` (botão inclinado, ação operacional principal),
`--comic-press` (contorno grosso, sombra offset, usado em confirmações destrutivas), `--comic` + 7
paletas (`-blue`, `-yellow`, `-back`, `-danger`, `-neutral`, `-success`, `-orange`, `-magenta` —
estilo "quadrinho", a linguagem de **ênfase operacional primária** já estabelecida e preservada
intacta: Save/Delete/Sign in/Create em Wallet, editores de atividade, Login, Identity, Account),
`--plain` + `--plain-danger`/`--plain-neutral` (ação de texto puro, sem chrome, para popovers/
filtros), `--pixel-cta` (`pixel-nes.css` — experiência pixel única e deliberadamente restrita ao
modal de celebração de Level Up; ganhou `border-width`/`border-style` explícitos na Sprint 20.8
porque deixou de poder depender da borda da base, agora `0`).

**Consumidores:** todo componente do repositório com uma ação — 40+ pontos de uso confirmados por
busca de `<BeeDayButton`.

## 3. Cards

### `BeeDayCard`

Container genérico (`Class`, `Padded`, `Muted`, `Interactive` — este último ativa hover
`translateY(-2px)` + sombra `md`, `ChildContent`, `AdditionalAttributes`). Sem estado interno, sem
JS. **Default canônico desde a Sprint 20.8** (era o modificador opt-in `--soft` da Sprint 20.6, agora
incorporado): sem borda, `--beeday-radius-2xl`, `--beeday-shadow-lg`. **Decisão final de default
(Sprint 20.8, EPIC 20):** auditados todos os consumidores reais antes de decidir — Wallet
(`.wallet-summary__card`/`.wallet-tag-item`/`.wallet-transaction-card`), a página Account
(`.beeday-settings-section`) e os cards de atividade (ver abaixo) **já redeclaram por completo**
sua própria borda/radius/sombra, então o novo default não os afeta visualmente; só a Home realmente
dependia da aparência default (usava a classe `--soft` explicitamente, agora redundante e removida
dos seus 2 consumidores). **Correção de achado desatualizado:** a documentação anterior afirmava que
"a maioria dos cards de produto (Activity/Habit) não usa `BeeDayCard`" — verificado diretamente nesta
Sprint que isso está incorreto: `HabitCard.razor`/`ActivityCard.razor`
(`Features/Dashboard/Components/`) **renderizam `<BeeDayCard Class="@CardCssClass">` como raiz**;
o que é verdade é que `cards.css` estiliza as classes computadas (`.habit-card`, `.activity-card` +
variantes de cor) com sua própria borda/radius/sombra completas (`.habit-card` inclusive com
`!important`), então visualmente eles não herdam nada do default de `BeeDayCard` além de layout/
comportamento genérico — mas arquitetural e semanticamente, eles **são** consumidores reais do
componente compartilhado, não markup paralelo independente.

### `BeeDayCardMenu`

Menu de ações posicionado dinamicamente (Edit/Delete) sobre um card.

| Parâmetro | Tipo | Notas |
|---|---|---|
| `Title` (`EditorRequired`) | `string` | Usado no `aria-label` ("Options for {Title}") |
| `Class`, `TriggerClass` | `string` | — |
| `Disabled` | `bool` | — |
| `OnEdit`, `OnDelete` | `EventCallback` | — |
| `OpenChanged` | `EventCallback<bool>` | Notifica o pai quando abre/fecha |

**Estado interno:** `isOpen`, `isMeasuring` (abre invisível para medir o painel real antes de
posicionar — evita "chutar" altura/largura), `flipUp`, `horizontalShiftPx`.

**JS interop:** `js/beeday-card-menu.js?v=20260729-1` — `measureGeometry` (C#→JS,
`getBoundingClientRect` do trigger/painel) alimenta `CardMenuPlacementCalculator.Calculate` (lógica
pura em C#, testável sem DOM — decide `FlipUp` e `HorizontalShiftPx` clampando a posição desejada
dentro da viewport com uma margem de 8px); `registerOutsideClick`/`unregisterOutsideClick` +
`[JSInvokable] NotifyOutsideClickAsync` (JS→C#) fecham o menu ao clicar fora, com fallback
silencioso (`catch (JSException)`) se o interop falhar — o menu continua utilizável via clique no
trigger, Escape, Edit, Delete.

**Coordenação entre instâncias:** `CardActionMenuCoordinator` (`BeeDay.Web/Services/`, `Scoped`) —
abrir um `BeeDayCardMenu` fecha qualquer outro já aberto no mesmo circuito, via evento C#-para-C#
(`MenuOpened`), não JS.

**Consumidores:** todo card de atividade no Dashboard (Habit/Task/Todo/Project), `TransactionCard`
(Wallet).

## 4. Feedback

| Componente | Objetivo | Parâmetros-chave | Estado/eventos |
|---|---|---|---|
| `BeeDayToastHost` | Renderiza a fila de `ToastService` (`Scoped`) | Nenhum — lê `ToastService` injetado | Assina `ToastService.Changed`; cada toast some sozinho (4s info/sucesso, 7s erro) ou por clique em fechar |
| `BeeDayLoading` | Overlay de "salvando..." fixo, inferior | `IsVisible`, `Label` | Puramente controlado pelo pai — sem estado próprio |
| `BeeDaySkeleton` | N linhas de skeleton shimmer | `Lines` (padrão 3), `Class` | Primeira linha = "title" (larga), demais alternam "wide"/"medium" |
| `BeeDayDashboardSkeleton` | Skeleton completo da tela `/daily` (resumo + 4 colunas × 3 cards) | Nenhum | Composição fixa de `BeeDaySkeleton` |
| `BeeDayEmptyState` | Estado vazio com ícone/título/descrição | `Title`, `Description` (`EditorRequired`), `Icon?`, `Class?` | Puramente apresentacional |
| `BeeDayConfirmDialog` | Diálogo de confirmação genérico (não só exclusão, apesar do CSS se chamar `delete-confirmation`) | `IsOpen`, `IsBusy`, `Title`/`Message` (`EditorRequired`), `ItemTitle?`, `Warning?`/`WarningDetails?`, `ConfirmLabel`/`CancelLabel`, `OnConfirm`/`OnCancel` | `role="alertdialog"`, `Escape` fecha (via `CancelAsync`), clique no backdrop fecha, clique dentro do painel não propaga (`@onclick:stopPropagation`) |

**Consumidores:** `BeeDayToastHost`/`BeeDayLoading` são globais (montados em `MainLayout`/
`OnboardingLayout`); `BeeDayConfirmDialog` é usado por toda exclusão de atividade/transação/tag;
`BeeDayDashboardSkeleton` só por `Home.razor`.

## 5. Layout

| Componente | Objetivo | Parâmetros-chave |
|---|---|---|
| `BeeDayPageHeader` | Cabeçalho de página: eyebrow + título + descrição + ações | `Title` (`EditorRequired`), `Eyebrow?`, `Description?`, `Actions?` (RenderFragment) |
| `BeeDaySectionHeader` | Mesmo papel que `BeeDayPageHeader`, em escala menor (`h2` vs `h1`), para seções dentro de uma página | Mesmo shape de parâmetros |
| `BeeDayHero` | Painel introdutório mais rico: ilustração opcional, uma ação primária emphasized, conteúdo de suporte, variante contextual | `Title` (`EditorRequired`), `Eyebrow?`, `Subtitle?`, `Illustration?`, `PrimaryAction?`, `SupportingContent?`, `Variant` (`Default`\|`Onboarding`) |
| `BeeDaySettingsSection` | Card de seção de configurações (cabeçalho + conteúdo) | `Title` (`EditorRequired`), `Eyebrow?`, `Description?`, `ChildContent` |
| `BeeDaySettingsForm<TModel>` | `EditForm` genérico com fieldset, grid de 2 colunas opcional, hint, ação de submit alinhada à direita | `Model`/`FormName`/`SubmitLabel` (`EditorRequired`), `IsBusy`, `SubmitButtonClass?`, `OnValidSubmit`, `ChildContent` |

`BeeDayPageHeader`/`BeeDaySectionHeader`/`BeeDayHero` compartilham o mesmo bloco CSS
(`.beeday-page-header, .beeday-section-header, .beeday-hero__row { display:flex; ... }` em
`design-system.css`) — mudar o layout de um dos três nesse bloco afeta os três. O comentário em
`design-system.css:547` documenta a distinção de uso pretendida: `BeeDayHero` para quando se precisa
de ilustração + uma única ação emphasized; `BeeDayPageHeader` para o caso comum sem ilustração.

**Consumidores:** `BeeDayPageHeader` em `Account.razor`; `BeeDaySettingsSection`/`Form` nas 3 seções
de `Account.razor` (Profile/Security/Preferences); `BeeDaySectionHeader` usado principalmente nas
páginas de catálogo (`HeroCatalog.razor`). **`BeeDayHero` teve seu primeiro consumidor de produto
real na Sprint 20.5 (EPIC 20):** `Features/Home/Pages/Home.razor` (rota `/`, variante `Default`,
sem ilustração) — até então só era usado em `HeroCatalog.razor`.

## 6. Modals

### `EditorModalShell`

Esqueleto compartilhado pelos 4 editores de atividade (Habit/Task/Todo/Project).

| Parâmetro | Tipo | Notas |
|---|---|---|
| `Model` (`EditorRequired`) | `object` | Não genérico — aceita qualquer `*EditorModel` |
| `Title`, `TitleId`, `SubmitLabel` (`EditorRequired`) | `string` | `TitleId` liga o `<h2>` ao `aria-labelledby` do modal |
| `ShowDelete` | `bool` | — |
| `IsBusy` | `bool` | Desabilita submit enquanto `true` |
| `HeroContent`, `BodyContent`, `SecondaryAction` | `RenderFragment?` | Slots de composição — cada editor injeta seus próprios campos aqui |
| `OnSubmit`, `OnCancel`, `OnDelete` | `EventCallback` | `Escape` chama `Cancel()` |

Não tem interop JS próprio; a superfície visual vem de `editor-modal.css`. Consumido por
`HabitEditorModal`, `TaskEditorModal`, `TodoEditorModal`, `ProjectEditorModal` (todos em
`Components/Features/*`, ver [`docs/web/04-feature-components.md`](../web/04-feature-components.md) §5).

## 7. Attributes

### `ActivityAttributeBadge`

Puramente apresentacional: `Attribute` (`ActivityAttribute?`) → classe CSS
(`activity-attribute-badge--{strength|dexterity|intelligence|vitality}`) + `title` com o tooltip
nativo do navegador (`"{Attribute} activity attribute"`). Sem estado, sem JS.

### `ActivityAttributeSelect`

Select customizado (não um `<select>` nativo) com posicionamento dinâmico — mesmo padrão de
`BeeDayCardMenu`.

| Parâmetro | Tipo | Notas |
|---|---|---|
| `Id` (`EditorRequired`) | `string` | — |
| `Label`, `FieldCssClass`, `LabelCssClass` | `string` | Segue o mesmo contrato visual dos componentes de `Forms/` (ver [`04-forms.md`](04-forms.md)) apesar de viver em `Attributes/`, não `Forms/` |
| `Disabled` | `bool` | — |
| `Value` | `ActivityAttribute?` | — |
| `ValueChanged` | `EventCallback<ActivityAttribute?>` | — |

**JS interop:** `js/activity-attribute-select.js` (sem sufixo de versão, diferente dos outros 2
módulos — ver achado em [`docs/web/README.md`](../web/README.md#achados-relevantes-reportados-não-corrigidos)).
`measureGeometry` alimenta `AttributeSelectPlacementCalculator.Calculate` (lógica pura, mesmo
padrão de `CardMenuPlacementCalculator`) — decide `FlipUp`, `TopPx`, `MaxHeightPx`. Diferente de
`BeeDayCardMenu`, não tem detecção de clique fora — o menu só fecha ao selecionar um valor.

**Consumidores:** os 4 editores de atividade (via `EditorModalShell.BodyContent`), possivelmente
`Wallets` (não confirmado nesta auditoria).

## 8. Text

| Componente | Objetivo |
|---|---|
| `BeeDayBrand` | Marca oficial: `<span class="beeday-brand">` com "BEE" + "DAY" (acento) — usado em todo `OnboardingLayout`, e também por `AccountSidePanel`/`DesktopSidebar`/`MobileHeader`/`MobileSidebar` (achado histórico de markup `LEVEL`/`UP` próprio corrigido na Sprint 20.4/20.7, ver `docs/web/03-layouts.md`; `TopNavigation` foi removida na Sprint 21.3, EPIC 21). |
| `SearchHighlight` | Divide `Text` em segmentos por ocorrência de `SearchTerm` (case-insensitive, `IndexOf` iterativo, sem regex) e envolve cada match em `<mark>`/span destacado (`beeday-search-highlight`, fundo `#ffe49a`). Lógica de segmentação (`BuildSegments`) é `internal static`, testável isoladamente. |

## 9. Interop — `BeeDaySortable` (fora de `DesignSystem/`, documentado aqui por simetria)

`Components/Behaviors/DragDrop/BeeDaySortable.razor` — motor de reordenação drag-and-drop genérico
usado pelas 4 colunas do Dashboard.

| Parâmetro | Tipo | Notas |
|---|---|---|
| `ItemIds`, `ItemTemplate`, `OnReorder`, `CollectionKey` (todos `EditorRequired`) | — | `ItemTemplate` é um `RenderFragment<Guid>` — o consumidor decide o que cada item renderiza |
| `AriaLabel` | `string` | Padrão "Reorderable activity list" |
| `VirtualizationThreshold` | `int` | Padrão 30 — acima disso, usa `<Virtualize>` |
| `ItemSize`, `OverscanCount` | `float`/`int` | Parâmetros de `Virtualize` |
| `RemovingItemId` | `Guid?` | Anima a saída de um item específico (170ms, ver `DashboardState.AnimateRemovalAsync`) |

**JS interop:** `js/beeday-sortable.js?v=20260721-f13-dragfix` (291 linhas — o maior dos 3 módulos).
`[JSInvokable] NotifyReorderAsync(itemId, targetItemId, placeAfter)` é a única direção JS→C# — o C#
nunca lê posição de mouse/touch, só recebe o resultado final. `SortableOrder.Move` (C# puro,
testável) calcula a nova ordem a partir desse resultado.

## 10. Fontes consultadas

- Todos os arquivos `.razor`/`.razor.cs`/`.cs` sob `src/BeeDay.Web/Components/DesignSystem/`
  (exceto `Forms/` e `Icons/`, documentados em `04-forms.md`/`03-icons.md`).
- `src/BeeDay.Web/Components/Behaviors/DragDrop/BeeDaySortable.razor(.cs)`,
  `SortableOrder.cs`, `SortableReorderEvent.cs`.
- `src/BeeDay.Web/wwwroot/css/design-system.css`, `cards.css`, `feedback.css`, `editor-modal.css`,
  `dragdrop.css`, `settings.css`.
- `src/BeeDay.Web/wwwroot/js/beeday-card-menu.js`, `activity-attribute-select.js`,
  `beeday-sortable.js` (nomes de import e assinaturas invocadas, não o conteúdo interno completo).
- [`docs/web/04-feature-components.md`](../web/04-feature-components.md),
  [`docs/web/05-design-system-integration.md`](../web/05-design-system-integration.md) (Sprint
  16.7, reaproveitado sem duplicar).
