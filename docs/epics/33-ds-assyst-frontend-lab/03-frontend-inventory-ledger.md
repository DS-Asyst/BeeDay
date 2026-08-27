# Frontend Inventory & Parity Ledger — EPIC 33

Ledger canônico de inventário/extração da EPIC 33, produzido pela Sprint 33.4
(Issue [#365](https://github.com/DS-Asyst/BeeDay/issues/365)), conforme o schema exigido por
`templates/FRONTEND_INVENTORY_LEDGER_TEMPLATE.md` do pacote de planejamento aprovado.

**Fonte da verdade desta Sprint:** `docs/web/02-routing-and-pages.md` (54 rotas), `docs/web/04-feature-components.md`
(13 áreas de Feature), `docs/design-system/README.md` + `02-components.md` (25 primitives),
`docs/design-system/03-icons.md` (Icon System), `src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs`
+ `IIdentityEmailComposer.cs` (2 templates de e-mail), `src/BeeDay.Web/Components/**` (listagem direta
de pastas), todos verificados em 2026-08-27. Nenhuma afirmação vem de memória de Sprint anterior sem
reverificação nesta Sprint.

**Baseline de produção fixa (não se move durante a fase de paridade):** `acce26a` — mesma baseline
estabelecida na Sprint 33.1 (`docs/epics/33-ds-assyst-frontend-lab/README.md` §1). Nenhuma mudança de
rota/componente ocorreu entre `acce26a` e o `hmg` atual (Sprints 33.2/33.3 alteraram apenas 1 link e
documentação/ADR) — a baseline permanece válida para os itens abaixo.

**Convenção de `Lab path`:** o projeto/solução exato do Lab só é estabelecido na Sprint 33.5 (`00_EPIC_33_MASTER_PLAN.md`
§7.2: "the exact solution/project name is established in Sprint 33.5 and then becomes canonical").
Até lá, `<LabWeb>` é um placeholder para a raiz do projeto Blazor do Lab; os caminhos abaixo espelham
a estrutura relativa de produção sob esse placeholder, não uma decisão de nomenclatura antecipada.

## Estados

```text
NOT_AUDITED → MAPPED → EXTRACTED → ADAPTED → PARITY_PENDING → VERIFIED
                                                              → EXCLUDED
                                                              → APPROVED_LAB_DIFFERENCE
```

Todo item desta Sprint está em **MAPPED** (inventariado, não extraído) ou **EXCLUDED** (justificativa
explícita registrada na coluna Notes) — nenhum item permanece `NOT_AUDITED`.

## Nota sobre categorias que não recebem linha própria

- **Estados de loading/empty/error/no-results/disabled:** não são artefatos extraíveis por si —
  são estados dos componentes já listados (`BeeDayLoading`, `BeeDaySkeleton`, `BeeDayDashboardSkeleton`,
  `BeeDayEmptyState`, `BeeDayErrorBoundary` — área Component) e das páginas que os consomem. Cobertos
  pelo motor de cenários único (Sprint 33.10), não fragmentados aqui.
- **Variantes responsivas:** aplicam-se por componente/página via os breakpoints já listados
  (FE33-006) — as ~70 media queries documentadas em `docs/ux/03-responsive.md` não recebem uma linha
  por query; são responsabilidade de paridade de cada item que as usa.
- **Localização em UI:** contrato transversal (en-US/pt-BR, `BeeDayCultures`) — ver FE33-107.

---

## Foundation (Sprint 33.6)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-001 | Foundation | `src/BeeDay.Web/wwwroot/css/variables.css` (cores/semantic colors) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/css/variables.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md`; `FoundationTokenParityTests.cs` (Lab) | Paleta + tokens semânticos; `#5247F9` = brand oficial (ADR de marca, EPIC 25). Caminho de produção corrigido nesta Sprint: o arquivo real é `variables.css`, não `design-system.css` (que é component-scoped, Sprint 33.8). |
| FE33-002 | Foundation | tipografia (`typography.css`, `typography-policy.css`; fontes Google `Coiny`/`Nunito`) | `acce26a` | Google Fonts (externo, permitido para artifacts/Lab per política de fontes) | COPY | `src/BeeDayLab.Web/wwwroot/css/typography.css`, `typography-policy.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §3; App.razor real (não `docs/web/02-routing-and-pages.md` §2, que está desatualizado citando "Inter/Jersey 25" — retirados desde as Sprints 21.4/21.9/20.6) | Correção nesta Sprint: fontes reais confirmadas em `src/BeeDay.Web/Components/App.razor` são `Coiny`+`Nunito`, não `Inter`/`Jersey 25`; link de Google Fonts replicado no Lab. |
| FE33-003 | Foundation | tokens de spacing (`variables.css`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/css/variables.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §4; `FoundationTokenParityTests.cs` (9 degraus) | |
| FE33-004 | Foundation | tokens de border-radius (`variables.css`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/css/variables.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §5; `FoundationTokenParityTests.cs` (7 degraus) | |
| FE33-005 | Foundation | tokens de elevação/sombra (`variables.css`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/css/variables.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §6; `FoundationTokenParityTests.cs` (4 degraus) | |
| FE33-006 | Foundation | breakpoints (literais, não CSS custom properties; `polish.css`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/css/polish.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §10; `docs/ux/03-responsive.md` | Literais por design (CSS vars inválidas em media features) — preservados como estão, sem "correção" durante extração. |
| FE33-007 | Foundation | tokens de motion (duration/easing; `variables.css`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/css/variables.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §7; `FoundationTokenParityTests.cs` | |
| FE33-008 | Foundation | grid/z-index/layout primitives (`variables.css`, `polish.css`, `utilities.css`, `animations.css`, `theme.css`, `wwwroot/app.css`) | `acce26a` | nenhuma | COPY (exceto seletores component-scoped de `app.css`, ver Notas) | `src/BeeDayLab.Web/wwwroot/css/{polish,utilities,animations,theme}.css`, `src/BeeDayLab.Web/wwwroot/app.css` | 33.6 | VERIFIED | `docs/design-system/01-foundations.md` §8; `FoundationTokenParityTests.cs` (9 z-index) | Excluídos deliberadamente de `app.css`: seletores `.card-action-menu__panel`/`.editor-modal__*`/`.beeday-field__control`/`.activity-card__checkbox`/`.profile-panel__brand` e o keyframe `card-menu-enter` — pertencem a componentes/features ainda não extraídos (Sprints 33.7-33.14); registrado como exclusão, não omissão silenciosa. |

## Asset (Sprint 33.7)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-009 | Asset | `wwwroot/icons/sprite.svg` + `BeeDayIconRegistry`/`BeeDayIconName`/`BeeDayIconCategory`/`BeeDayIconColor`/`BeeDayIconSize`/`BeeDayIconDefinition` + `design/icons/catalog/icon-mapping.csv` | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/icons/sprite.svg`, `src/BeeDayLab.Web/Components/DesignSystem/Icons/*.cs`, `design/icons/catalog/icon-mapping.csv` | 33.7 | VERIFIED | `docs/design-system/03-icons.md`; `IconSystemParityTests.cs` (Lab, 67 asserções) | Ícones funcionais Lucide, sprite estático local, sem CDN. `BeeDayIcon.razor` (o componente Razor em si) permanece FE33-028/Sprint 33.8. |
| FE33-010 | Asset | 6 ícones de marca social (vetores próprios) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/icons/sprite.svg` | 33.7 | VERIFIED | `docs/design-system/03-icons.md`; `IconSystemParityTests.cs` | `fill: currentColor`, mantidos fora do padrão Lucide outline. |
| FE33-011 | Asset | 8 ilustrações/personagens/mascote (`wwwroot/assets/{brand,dashboard,hero,home}/`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/assets/{brand,dashboard,hero,home}/` | 33.7 | VERIFIED | `docs/brand/01-character-illustration.md` (sem menção de licenciamento restritivo); `AssetExistenceTests.cs` | Auditoria de licença desta Sprint: nenhuma restrição de terceiros encontrada — são 8 artes originais do próprio produto BeeDay (mascote "bee", ilustrações Home/Dashboard/Hero), copiadas byte-a-byte (verificado `Bin` no diff do Git, não texto corrompido). |
| FE33-012 | Asset | logos/wordmarks oficiais (2 flags PNG + `footer-wave.svg`) | `acce26a` | nenhuma | COPY | `src/BeeDayLab.Web/wwwroot/assets/flags/`, `src/BeeDayLab.Web/wwwroot/assets/footer/` | 33.7 | VERIFIED | `docs/brand/`; `Components/DesignSystem/Text/BeeDayBrand.razor`; `AssetExistenceTests.cs` | O wordmark `beeday` em si é consumido via `<BeeDayBrand />` (CSS/tipografia, sem asset de imagem) — não copiado aqui como imagem, coerente com o achado de `docs/web/README.md`. Os 2 flags PNG (`brazil.png`, `united-states.png`) e `footer-wave.svg` são os únicos assets de "logo/wordmark" físicos reais mapeados. |

## Component (Sprint 33.8)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-013 | Component | `Components/DesignSystem/Buttons/BeeDayButton.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Buttons/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Todas as 9 variantes cobertas. |
| FE33-014 | Component | `Components/DesignSystem/Cards/BeeDayCard.razor(.cs)` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Cards/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-015 | Component | `Components/DesignSystem/Feedback/BeeDayConfirmDialog.razor` | `acce26a` | `IStringLocalizer<DesignSystemResources>` (Web) | ADAPT | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Correção nesta Sprint: o inventário original classificava como COPY; a leitura completa do arquivo nesta Sprint encontrou injeção de `IStringLocalizer` não prevista — tratado com a mesma regra já aplicada a FE33-021/027 (nenhum pipeline de localização no Lab), strings padrão em inglês hardcoded. |
| FE33-016 | Component | `Components/DesignSystem/Feedback/BeeDayDashboardSkeleton.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Estado "loading" do Dashboard. |
| FE33-017 | Component | `Components/DesignSystem/Feedback/BeeDayEmptyState.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Estado "empty" canônico. |
| FE33-018 | Component | `Components/DesignSystem/Feedback/BeeDayErrorBoundary.razor` | `acce26a` | `IStringLocalizer<DesignSystemResources>` (Web) | ADAPT | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Correção nesta Sprint: classificado originalmente como COPY; mesma descoberta/tratamento de FE33-015. Estado "error" canônico; `LoggingErrorBoundary.cs` (base, `Microsoft.Extensions.Logging` puro) copiado junto. |
| FE33-019 | Component | `Components/DesignSystem/Feedback/BeeDayLoading.razor` | `acce26a` | `IStringLocalizer<DesignSystemResources>` (Web) | ADAPT | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Correção nesta Sprint: classificado originalmente como COPY; mesma descoberta/tratamento de FE33-015. Usado também por `Routes.razor` (`Authorizing`) na produção — não replicado aqui (fora do escopo desta Sprint). |
| FE33-020 | Component | `Components/DesignSystem/Feedback/BeeDaySkeleton.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-021 | Component | `Components/DesignSystem/Feedback/BeeDayToastHost.razor` | `acce26a` | `ToastService`, `IStringLocalizer<SharedResources>` (Web) | ADAPT | `<LabWeb>/Components/DesignSystem/Feedback/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | `ToastService` reimplementado como serviço local do Lab (`Components/DesignSystem/Feedback/ToastService.cs`, registrado `Scoped` em `Program.cs`), mesmo shape (Show*/Remove/Messages/Changed), sem `IStringLocalizer` — títulos/aria-labels padrão em inglês hardcoded. |
| FE33-022 | Component | `Components/DesignSystem/Forms/BeeDayCheckbox.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Forms/` | 33.8 | VERIFIED | `docs/design-system/04-forms.md`; `FormsAccessibilityTests.cs` (Lab) | |
| FE33-023 | Component | `Components/DesignSystem/Forms/BeeDayDateInput.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Forms/` | 33.8 | VERIFIED | `docs/design-system/04-forms.md`; `FormsAccessibilityTests.cs` (Lab) | |
| FE33-024 | Component | `Components/DesignSystem/Forms/BeeDayInput.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Forms/` | 33.8 | VERIFIED | `docs/design-system/04-forms.md`; `FormsAccessibilityTests.cs` (Lab) | |
| FE33-025 | Component | `Components/DesignSystem/Forms/BeeDaySelect.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Forms/` | 33.8 | VERIFIED | `docs/design-system/04-forms.md`; `FormsAccessibilityTests.cs` (Lab) | |
| FE33-026 | Component | `Components/DesignSystem/Forms/BeeDayTextArea.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Forms/` | 33.8 | VERIFIED | `docs/design-system/04-forms.md`; `FormsAccessibilityTests.cs` (Lab) | |
| FE33-027 | Component | `Components/DesignSystem/Forms/BeeDayValidationMessage.razor` | `acce26a` | `ValidationMessageLocalizer` (Web) | ADAPT | `<LabWeb>/Components/DesignSystem/Forms/` | 33.8 | VERIFIED | `docs/design-system/04-forms.md`; `FormsAccessibilityTests.cs` (Lab) | O mecanismo `EditContext`/`FieldIdentifier` (API padrão do Blazor) foi portado como está; `ValidationMessageLocalizer` **não foi portado** — mapeia mensagens de validação de negócio reais do BeeDay (regras de senha, tamanho de nome/título) que não existem no Lab; portá-lo seria exatamente o "mock de lógica de negócio" que a ADR-008 proíbe. Mensagens renderizadas diretamente de `EditContext.GetValidationMessages(...)`. |
| FE33-028 | Component | `Components/DesignSystem/Icons/BeeDayIcon.razor` | `acce26a` | `BeeDayIconRegistry` (ver FE33-009) | COPY | `<LabWeb>/Components/DesignSystem/Icons/` | 33.8 | VERIFIED | `docs/design-system/03-icons.md`; `SharedComponentsParityTests.cs` (Lab) | Componente Razor (wrapper `.razor`/`.razor.cs`/`.razor.css`); os tipos de registry já existiam desde a Sprint 33.7. |
| FE33-029 | Component | `Components/DesignSystem/Layout/BeeDayHero.razor` | `acce26a` | `BeeDayPaletteToken` (não inventariado individualmente) | COPY | `<LabWeb>/Components/DesignSystem/Layout/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `docs/web/02-routing-and-pages.md` §9; `SharedComponentsParityTests.cs` (Lab) | Inclui parâmetros `HeaderNav`/`Eyebrow`/`BrandContext` (Sprint 29.4). Dependência transitiva descoberta nesta Sprint: `BeeDayHero.Surface` é tipado por `BeeDayPaletteToken` (`Components/DesignSystem/BeeDayPaletteToken.cs`), copiado junto — seus tokens CSS já existiam no Lab desde a Sprint 33.6. |
| FE33-030 | Component | `Components/DesignSystem/Layout/BeeDayPageHeader.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Layout/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-031 | Component | `Components/DesignSystem/Layout/BeeDaySectionHeader.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Layout/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-032 | Component | `Components/DesignSystem/Layout/BeeDaySettingsForm.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Layout/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-033 | Component | `Components/DesignSystem/Layout/BeeDaySettingsSection.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Layout/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-034 | Component | `Components/DesignSystem/Modals/EditorModalShell.razor(.cs)` | `acce26a` | `DialogFocusScope`, `beeday-dialog-focus.js` (ver FE33-106) | ADAPT | `<LabWeb>/Components/DesignSystem/Modals/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `ModalAndSortableTests.cs` (Lab) | Correção nesta Sprint: a referência cruzada "ver FE33-109" no inventário original era um erro de digitação (FE33-109 é uma página de galeria não relacionada); a dependência real é FE33-106. Componente já era presentation-only (`Model`/`Title`/callbacks como parâmetros); ADAPT aqui refere-se apenas à remoção do sufixo `?v=...` hardcoded do caminho de import JS (ver FE33-106). Base dos 4 editores de atividade (ver FE33-091..094) — não portados nesta Sprint. |
| FE33-035 | Component | `Components/DesignSystem/Progress/BeeDayProgressBar.razor` | `acce26a` | `IStringLocalizer<DesignSystemResources>` (Web) | ADAPT | `<LabWeb>/Components/DesignSystem/Progress/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | Correção nesta Sprint: classificado originalmente como COPY; mesma descoberta/tratamento de FE33-015. |
| FE33-036 | Component | `Components/DesignSystem/Text/BeeDayBrand.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Text/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-037 | Component | `Components/DesignSystem/Text/SearchHighlight.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/DesignSystem/Text/` | 33.8 | VERIFIED | `docs/design-system/02-components.md`; `SharedComponentsParityTests.cs` (Lab) | |
| FE33-038 | Component | `Components/Behaviors/DragDrop/BeeDaySortable.razor(.cs)` + `beeday-sortable.js` | `acce26a` | ver FE33-105 | COPY | `<LabWeb>/Components/Behaviors/DragDrop/` | 33.8 | VERIFIED | `docs/design-system/02-components.md` §9; `docs/web/04-feature-components.md` §3; `ModalAndSortableTests.cs` (Lab) | Correção nesta Sprint: a leitura completa do componente mostrou que ele já é presentation-only — expõe apenas `OnReorder` (`EventCallback<SortableReorderEvent>`); não existe chamada direta a `DashboardState`/`store.ReorderAsync` dentro do próprio `BeeDaySortable`, portanto reclassificado de ADAPT para COPY (namespace apenas). `OnReorder` exercitado nos testes do Lab com um handler local de teste, já que a Sprint 33.8 não cria página de galeria/consumidor real (isso é 33.16/33.17). |

## Layout (Sprint 33.9)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-039 | Layout | `Components/Layout/MainLayout.razor` | `acce26a` | `CascadingAuthenticationState`, `AuthorizeRouteView` | ADAPT | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/03-layouts.md` | Lab usa navegação/estado local em vez de autenticação real (ver ADR-008 §2). |
| FE33-040 | Layout | `Components/Layout/OnboardingLayout.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §3 | Sem navegação — usado por Authentication/Identity/Onboarding/ProfileCreation. |
| FE33-041 | Layout | `Components/Layout/PublicLayout.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §3 | `PublicHeader` + `@Body` + `AppFooter`. |
| FE33-042 | Layout | `Components/Layout/EditorialLayout.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Sem `PublicHeader`/`AppFooter` — marca/nav dentro do `BeeDayHero`. |
| FE33-043 | Layout | `Components/Layout/DesktopSidebar.razor` | `acce26a` | rotas autenticadas reais | ADAPT | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/03-layouts.md` | Navegação Lab-local, sem roteamento de produção. |
| FE33-044 | Layout | `Components/Layout/MobileHeader.razor` | `acce26a` | idem FE33-043 | ADAPT | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/03-layouts.md` | |
| FE33-045 | Layout | `Components/Layout/MobileSidebar.razor` | `acce26a` | idem FE33-043 | ADAPT | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/03-layouts.md` | |
| FE33-046 | Layout | `Components/Layout/NavigationItem(s).razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/03-layouts.md` | |
| FE33-047 | Layout | `Components/Layout/PublicHeader.razor` | `acce26a` | `AuthenticatedEntryDestinationResolver` (CTA "Continue to beeday") | ADAPT | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §8 | CTA usa cenário fixo em vez do resolver real. |
| FE33-048 | Layout | `Components/Layout/AppFooter.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `tests/BeeDay.Web.Tests/Components/Layout/AppFooterTests.cs` | Link GitHub de suporte aponta para `DS-Asyst/BeeDay` real — Lab deve usar um placeholder não funcional, não o link de produção. |
| FE33-049 | Layout | `Components/Layout/EditorialFooter.razor(.cs/.css)` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-050 | Layout | `Components/Layout/ReconnectModal.razor` | `acce26a` | SignalR/Blazor Server reconnection real | MOCK | `<LabWeb>/Components/Layout/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §2 (`App.razor`) | Lab representa o estado visual sem depender de reconexão real de circuito. |
| FE33-051 | Layout | `Components/Pages/NotFound.razor` | `acce26a` | `SharedResources` | COPY | `<LabWeb>/Components/Pages/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §7 | |
| FE33-052 | Layout | `Components/Pages/Error.razor` | `acce26a` | `HttpContext`/`Activity.Current` (trace id) | ADAPT | `<LabWeb>/Components/Pages/` | 33.9 | MAPPED | `docs/web/02-routing-and-pages.md` §7 | Lab usa um trace id sintético fixo. |

## Public (Sprint 33.11)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-053 | Public | `/` — `Features/Home/Pages/Home.razor` | `acce26a` | `AuthenticatedEntryDestinationResolver` (CTA) | ADAPT | `<LabWeb>/Components/Pages/Public/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §8 | |
| FE33-054 | Public | `/mission` — `Institutional/Pages/Mission.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | `EditorialStoryBlock`. |
| FE33-055 | Public | `/efficacy` — `Institutional/Pages/Efficacy.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Callout de evidência pendente. |
| FE33-056 | Public | `/brand-guidelines` — `Institutional/Pages/BrandGuidelines.razor` | `acce26a` | `ExperienceSystemPillarNav`/`TopicNav` (ver FE33-076) | ADAPT | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-057 | Public | `/contact` — `Institutional/Pages/Contact.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Link GitHub real (`DS-Asyst/BeeDay`) — Lab usa placeholder, mesmo tratamento do FE33-048. |
| FE33-058 | Public | `/beeday` — `Institutional/Pages/Product.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-059 | Public | `/beeday-plus` — `Institutional/Pages/ProductPlus.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-060 | Public | `/android` — `Institutional/Pages/Android.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Status "Coming soon". |
| FE33-061 | Public | `/ios` — `Institutional/Pages/Ios.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Status "Coming soon". |
| FE33-062 | Public | `/faqs` — `Institutional/Pages/Faqs.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | `<details>/<summary>`. |
| FE33-063 | Public | `/community-guidelines` — `Institutional/Pages/CommunityGuidelines.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-064 | Public | `/terms` — `Institutional/Pages/Terms.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-065 | Public | `/privacy` — `Institutional/Pages/Privacy.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | |
| FE33-066 | Public | 4 templates: `EditorialPageTemplate`, `ProductPageTemplate`, `HelpPageTemplate`, `LegalDocumentPageTemplate` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Institutional/Components/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Compartilhados pelas 12 páginas acima. |
| FE33-067 | Public | `InstitutionalPageShell`, `EditorialSectionNav`, `EditorialSectionRegistry`, `EditorialStoryBlock` | `acce26a` | `NavigationManager` (rota atual) | ADAPT | `<LabWeb>/Components/Pages/Institutional/Components/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §9 | Navegação contextual via router Lab-local. |
| FE33-068 | Public | `/brand/typography`, `/experience-system/brand/typography` — `Brand/Pages/TypographyGuidelines.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Public/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §3, §10 | Mesmo componente, 2 rotas. |
| FE33-069 | Public | `/experience-system` — `ExperienceSystem/Pages/ExperienceSystemHome.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | |
| FE33-070 | Public | `/experience-system/brand` — `Pages/Brand/BrandOverview.razor` | `acce26a` | `ExperienceSystemTopicGrid` | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/Brand/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | |
| FE33-071 | Public | 6 tópicos Brand: `BrandIdentity`, `BrandWordmark`, `BrandColor`, `BrandIllustration`, `BrandCharacters`, `BrandWriting` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/Brand/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | Todas usam `ExperienceSystemPage` (ver FE33-076). |
| FE33-072 | Public | `/experience-system/ui` — `Pages/Ui/UiOverview.razor` | `acce26a` | `ExperienceSystemTopicGrid` | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/Ui/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | |
| FE33-073 | Public | 5 tópicos UI: `UiFoundations`, `UiComponents`, `UiProductPatterns`, `UiInteraction`, `UiLayout` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/Ui/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | |
| FE33-074 | Public | `/experience-system/ux` — `Pages/Ux/UxOverview.razor` | `acce26a` | `ExperienceSystemTopicGrid` | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/Ux/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | |
| FE33-075 | Public | 5 tópicos UX: `UxAccessibility`, `UxResponsive`, `UxLocalization`, `UxMotion`, `UxPerformance` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/ExperienceSystem/Ux/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | |
| FE33-076 | Public | `ExperienceSystemPage`, `ExperienceSystemPillarNav`, `ExperienceSystemTopicNav`, `ExperienceSystemTopicGrid` | `acce26a` | `NavigationManager` | ADAPT | `<LabWeb>/Components/Pages/ExperienceSystem/Components/` | 33.11 | MAPPED | `docs/web/02-routing-and-pages.md` §10 | Composição compartilhada pelas 20 rotas `/experience-system/*` acima. |

## Identity & Account (Sprint 33.12)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-077 | Identity | `/login` — `Authentication/Pages/Login.razor` | `acce26a` | POST real para `/auth/login` (minimal API) | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §7 | Form HTML puro; Lab intercepta submit localmente. |
| FE33-078 | Identity | `/welcome` — `ProfileCreation/Pages/Welcome.razor` | `acce26a` | nenhuma | COPY | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/README.md` "Achados" | Rota de compatibilidade não linkada — extrair mesmo assim (rota real existente). |
| FE33-079 | Identity | `/profile/create` — `ProfileCreation/Pages/CreateProfile.razor` | `acce26a` | `ProfileCreationState` (2 fluxos) | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §7 | Cenário decide qual dos 2 fluxos (anônimo vs. autenticado sem perfil). |
| FE33-080 | Identity | `/account/forgot-password` — `Identity/Pages/ForgotPassword.razor` | `acce26a` | `ISender` direto (`RequestPasswordResetCommand`) | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §8 | |
| FE33-081 | Identity | `/account/resend-confirmation` — `Identity/Pages/ResendConfirmation.razor` | `acce26a` | `ISender` direto + `PeriodicTimer` cooldown 60s | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §7, §8 | |
| FE33-082 | Identity | `/account/email-confirmation-sent` — `Identity/Pages/EmailConfirmationSent.razor` | `acce26a` | idem FE33-081 | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §7, §8 | |
| FE33-083 | Identity | `/account/confirm-email` — `Identity/Pages/ConfirmEmail.razor` | `acce26a` | `ISender` direto (`ConfirmEmailCommand`) | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §8 | |
| FE33-084 | Identity | `/account/reset-password` — `Identity/Pages/ResetPassword.razor` | `acce26a` | `ISender` direto (`ResetPasswordCommand`) | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §8 | |
| FE33-085 | Identity | `/onboarding/tutorial` — `Onboarding/Pages/Tutorial.razor` | `acce26a` | `Store.CompleteOnboardingAsync` | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §7 | 5 slides estáticos — quase todo COPY, só o CTA final é ADAPT. |
| FE33-086 | Identity | `/account`, `/settings` — `Account/Pages/Account.razor` (3 seções) | `acce26a` | `BeeDayWebService` (`UpdateUserAsync`/`ChangePasswordAsync`/`UpdatePreferencesAsync`) | ADAPT | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/04-feature-components.md` §6 | 3 seções independentes, cada uma com seu próprio estado de busy/toast. |
| FE33-087 | Identity | `Authentication/Components/RedirectToLogin.razor` | `acce26a` | `NavigationManager.NavigateTo(forceLoad:true)` | MOCK | `<LabWeb>/Components/Pages/Identity/` | 33.12 | MAPPED | `docs/web/02-routing-and-pages.md` §2 | Não é rota própria — estado `NotAuthorized` do `Router`; Lab representa a tela sem navegação forçada real. |

## Daily / Productivity (Sprint 33.13)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-088 | Daily | `/profile` — `Dashboard/Pages/DashboardHome.razor` | `acce26a` | nenhuma direta (ver FE33-090) | ADAPT | `<LabWeb>/Components/Pages/Daily/` | 33.13 | MAPPED | `docs/web/02-routing-and-pages.md` §3 | |
| FE33-089 | Daily | `/home` — `Dashboard/Pages/LegacyHomeRedirect.razor` | `acce26a` | redirect puro para `/profile` | EXCLUDE | — | 33.13 | EXCLUDED | `docs/web/02-routing-and-pages.md` §4 | Sem superfície visual própria — rota de compatibilidade que só redireciona; nada a extrair. |
| FE33-090 | Daily | `/daily` — `Dashboard/Pages/Home.razor` + `DashboardState` | `acce26a` | `BeeDayWebService` (`GetDashboardQuery`), `BeeDaySortable` reorder | ADAPT | `<LabWeb>/Components/Pages/Daily/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §3 | Área mais densa: busca/filtro client-side, feedback de XP, reordenação — todos via cenário, sem recalcular regra de negócio. |
| FE33-091 | Daily | `Habits/Components/HabitEditorModal.razor(.cs)` | `acce26a` | `HabitVisualState` (7 classes CSS por faixa de saldo) | ADAPT | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §5 | Saldo de exibição vem do cenário, não recalculado. |
| FE33-092 | Daily | `Tasks/Components/TaskEditorModal.razor(.cs)` | `acce26a` | nenhuma além de `EditorModalShell` | COPY | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §5 | |
| FE33-093 | Daily | `Todos/Components/TodoEditorModal.razor(.cs)` | `acce26a` | nenhuma além de `EditorModalShell` | COPY | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §5 | |
| FE33-094 | Daily | `Projects/Components/ProjectEditorModal.razor(.cs)` | `acce26a` | nenhuma além de `EditorModalShell` | COPY | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §5 | |
| FE33-095 | Daily | `Projects/Components/ProjectWorkspace.razor(.cs/.css)` | `acce26a` | `DashboardState.OpenProjectId` | ADAPT | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §5 | Painel de detalhe, não modal. |
| FE33-096 | Daily | `Experience/Components/ExperienceBar.razor` + `BeeDayFeedbackStore`/`Host`/`Modal` | `acce26a` | `UserLeveledUpDomainEvent` (Application) | MOCK | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | `docs/web/04-feature-components.md` §9 | Único fluxo Web reagindo a domain event — cenário dispara o feedback visual diretamente, sem MediatR. |
| FE33-097 | Daily | `Dashboard/Components/DashboardColumn.razor(.cs/.css)` | `acce26a` | `DashboardState` (filtragem) | ADAPT | `<LabWeb>/Components/Pages/Daily/Components/` | 33.13 | MAPPED | Listagem direta `src/BeeDay.Web/Components/Features/Dashboard/Components/` | Coluna de listagem reutilizada pelas 4 entidades de atividade. |

## Wallet (Sprint 33.14)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-098 | Wallet | `/wallet` — `Wallets/Pages/Wallet.razor` + `WalletPageState`/`WalletInteractionState` | `acce26a` | `ISender` direto (9 comandos/queries, ver `docs/web/04-feature-components.md` §8) | ADAPT | `<LabWeb>/Components/Pages/Wallet/` | 33.14 | MAPPED | `docs/web/04-feature-components.md` §4 | Não usa `BeeDayWebService` — todo o carregamento é `ISender` direto; maior superfície de ADAPT da EPIC. |
| FE33-099 | Wallet | `Wallets/Components/TransactionFormModal.razor` | `acce26a` | `EditorModalShell` | COPY | `<LabWeb>/Components/Pages/Wallet/Components/` | 33.14 | MAPPED | Listagem direta `src/BeeDay.Web/Components/Features/Wallets/Components/` | |
| FE33-100 | Wallet | `Wallets/Components/TagFormModal.razor` | `acce26a` | `EditorModalShell` | COPY | `<LabWeb>/Components/Pages/Wallet/Components/` | 33.14 | MAPPED | Listagem direta `src/BeeDay.Web/Components/Features/Wallets/Components/` | |

## Email (Sprint 33.15)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-101 | Email | `IdentityEmailComposer.ComposeEmailConfirmation` | `acce26a` | `IdentityEmailOptions.ConfirmationPath`, token real | MOCK | `<LabWeb>/Emails/` | 33.15 | MAPPED | `src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs` | pt-BR/en-US via `UserLanguage`; link com token sintético não-secreto no Lab. |
| FE33-102 | Email | `IdentityEmailComposer.ComposePasswordReset` | `acce26a` | `IdentityEmailOptions.PasswordResetPath`, token real | MOCK | `<LabWeb>/Emails/` | 33.15 | MAPPED | `src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs` | pt-BR/en-US via `UserLanguage`; token sintético no Lab. |
| FE33-103 | Email | `BuildHtmlTemplate`/`BuildPlainTextTemplate` (shell HTML + alternativa plain-text) | `acce26a` | nenhuma além do conteúdo acima | COPY | `<LabWeb>/Emails/` | 33.15 | MAPPED | `src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs` | Shell compartilhado pelos 2 templates; cor de marca `#5247F9` (corrigida na Sprint 26.6). |

## Cross-cutting (Sprint 33.10 — Mock Data & UI State Engine)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-104 | Localization | `BeeDayCultures`, precedência cookie → `User.Language` → fallback, catálogos `.resx` en-US/pt-BR | `acce26a` | `AuthenticatedAccountCultureProvider` (Web) | ADAPT | `<LabWeb>/Localization/` | 33.10 | MAPPED | `docs/web/07-localization.md` | Motor de cenário deve suportar seleção de locale (ver `04_MOCK_STATE_POLICY.md`). |
| FE33-105 | JS interop | `wwwroot/js/beeday-sortable.js` | `acce26a` | ver FE33-038 | ADAPT | `<LabWeb>/wwwroot/js/` | 33.8 | VERIFIED | `docs/web/README.md` "Achados"; `ModalAndSortableTests.cs` (Lab) | Cache-busting por query string hardcoded (`?v=20260721-f13-dragfix`) removido do caminho de import — não copiar o débito, conforme já previsto nesta nota. |
| FE33-106 | JS interop | `wwwroot/js/beeday-dialog-focus.js` | `acce26a` | `DialogFocusScope.cs` (ver FE33-034) | ADAPT | `<LabWeb>/wwwroot/js/` | 33.8 | VERIFIED | `src/BeeDay.Web/Components/DesignSystem/Modals/DialogFocusScope.cs`; `ModalAndSortableTests.cs` (Lab) | Correção nesta Sprint: reclassificado de COPY para ADAPT — o arquivo `.js` em si é copiado verbatim (focus trap presentation-only real), mas o caminho de import hardcoded com `?v=20260827-1` usado por `DialogFocusScope.cs`/`EditorModalShell.razor.cs` foi normalizado para `./js/beeday-dialog-focus.js` (sem sufixo), mesma razão de FE33-105. |
| FE33-107 | JS interop | `wwwroot/js/beeday-card-menu.js` | `acce26a` | posicionamento de menu de card | EXCLUDE | — | 33.8 | EXCLUDED | `docs/design-system/README.md` "Achados" | **Drift de documentação encontrado nesta Sprint**: o arquivo não existe em `src/BeeDay.Web/wwwroot/js/` no estado atual de `acce26a` — apenas `beeday-sortable.js`, `beeday-culture-sync.js`, `beeday-editorial-footer.js` e `beeday-dialog-focus.js` estão presentes. O item foi removido da produção após o inventário original (Sprint 33.4) ter sido escrito, análogo ao drift de fonte (Inter/Jersey-25 → Coiny/Nunito) já documentado na Sprint 33.6. Não foi fabricado nenhum arquivo para preencher a lacuna. |

## Gallery (referência — Sprints 33.16/33.17 constroem a galeria própria do Lab, não copiam estas páginas)

| ID | Area | Production path | Production source SHA | Runtime dependencies | Strategy | Lab path | Owning Sprint | State | Evidence | Notes |
|---|---|---|---|---|---|---|---:|---|---|---|
| FE33-108 | Gallery | `/design-system/icons` — `DesignSystem/Pages/IconCatalog.razor` | `acce26a` | `IWebHostEnvironment` (não usado para acesso) | EXCLUDE | — | 33.16 | EXCLUDED | `docs/web/02-routing-and-pages.md` §6 | Precedente de composição para a Component Gallery do Lab (33.16), não copiada 1:1 — o Lab constrói sua própria galeria a partir dos componentes já extraídos (FE33-013..038). |
| FE33-109 | Gallery | `/design-system/hero` — `DesignSystem/Pages/HeroCatalog.razor` | `acce26a` | idem FE33-108 | EXCLUDE | — | 33.16 | EXCLUDED | `docs/web/02-routing-and-pages.md` §6 | Idem FE33-108. |

---

## Totais

- **109 itens** (`FE33-001`–`FE33-109`), cobrindo as 54 rotas `@page`, os 25 primitives + `BeeDaySortable` do Design System, 14 peças de layout/shell, 8 categorias de foundation, 4 categorias de asset, 3 módulos JS de interação, 2 templates de e-mail + shell compartilhado, e o contrato de localização.
- Estratégia: nenhum item usa reuso binário (consistente com ADR-008) — distribuição aproximada COPY (maioria dos primitives/páginas puramente apresentacionais) / ADAPT (qualquer item que hoje injeta `BeeDayWebService`/`ISender`/estado de autenticação real) / MOCK (feedback de XP, e-mails, reconexão) / EXCLUDE (`/home` redirect puro; as 2 páginas de catálogo de produção, substituídas pela galeria própria do Lab).
- Nenhum item permanece `NOT_AUDITED`. Nenhum item está em estado terminal de extração (`VERIFIED`) — isso é esperado: Sprint 33.4 é inventário, não extração.
- Nenhuma regra de negócio (cálculo de XP, saldo agregado, regras de recorrência) foi copiada para este Ledger nem será copiada para o Lab — todo valor de exibição citado nas Notes vem de cenário, não de recálculo (ver `04_MOCK_STATE_POLICY.md`).
