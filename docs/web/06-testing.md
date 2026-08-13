# Web Testing Map

**Fonte da verdade:** enumerado diretamente em `tests/BeeDay.Web.Tests/` (64 arquivos `.cs`) e
`tests/BeeDay.E2E.Tests/` (8 arquivos `.cs` + o `.csproj`). A estratégia de teste em si (pirâmide, infraestrutura de
integração, infraestrutura E2E) já é descrita em detalhe em
[`docs/testing/01-testing-strategy.md`](../testing/01-testing-strategy.md) — este documento não a
duplica; mapeia especificamente qual arquivo de teste cobre qual parte da árvore de
`src/BeeDay.Web/` descrita nos 5 documentos anteriores.

**Última verificação:** 2026-08-11 (Sprint 20.5, EPIC 20) — `PublicHeader`/`PublicLayout`/`Home`
(rota `/`) mapeados; contagem de arquivos corrigida (Web.Tests 61→64, E2E.Tests 7→8). Demais seções
preservadas da verificação de 2026-08-07.

## 1. Objetivo

Dar, para cada área documentada em `01`-`05`, a resposta a "onde está o teste disso" — sem repetir
a explicação de infraestrutura (`BeeDayWebApplicationFactory`, `PlaywrightAppFixture`, etc.), já
coberta por `docs/testing/`.

## 2. `BeeDay.Web.Tests` — projeto e stack

`Microsoft.NET.Sdk.Razor`, referencia só `BeeDay.Web.csproj`. Stack: `bunit` (component tests),
`AngleSharp` (asserções sobre o DOM renderizado), `Microsoft.AspNetCore.Mvc.Testing`
(`WebApplicationFactory<Program>`, testes de integração), `xunit.v3`. Padrão observado em todo
teste de componente (`BeeDayButtonTests.cs` como exemplo representativo):

```csharp
using var context = new BunitContext();
var cut = context.Render<BeeDayButton>(parameters => parameters
    .Add(component => component.Variant, variant));
Assert.Contains(expectedClass, cut.Find("button").ClassList);
```

## 3. Mapeamento — Composition root (`01-composition-root.md`)

| Área do código | Teste(s) |
|---|---|
| `CorrelationIdMiddleware` | `Diagnostics/CorrelationIdMiddlewareTests.cs` |
| `GlobalExceptionHandler` / mapeamento de exceção → `ProblemDetails` | `Integration/ProblemDetailsIntegrationTests.cs` |
| Autenticação por cookie, `OnValidatePrincipal`, `SessionVersion` | `Integration/CookieIntegrationTests.cs`, `SessionInvalidationIntegrationTests.cs` |
| `/auth/login`, `/auth/logout` | `Integration/LoginIntegrationTests.cs`, `LogoutIntegrationTests.cs` |
| `LoginRateLimiterFactory`/`Options` (unidade) | `Services/Authentication/LoginRateLimiterFactoryTests.cs` |
| Rate limiting via HTTP real | `Integration/RateLimitingIntegrationTests.cs` (usa `RateLimitingWebApplicationFactory`, limites baixos dedicados) |
| `LoginDestinationResolver` (unidade) | `Services/LoginDestinationResolverTests.cs` |
| Antiforgery/CSRF | `Integration/AntiforgeryIntegrationTests.cs` |
| Autorização por rota/atributo | `Integration/AuthorizationIntegrationTests.cs` |
| Isolamento multiusuário (via `CreateAuthenticatedScope`) | `Integration/MultiUserIsolationIntegrationTests.cs` |
| Reset de senha / confirmação de e-mail, fluxo HTTP completo | `Integration/PasswordResetIntegrationTests.cs`, `EmailConfirmationIntegrationTests.cs` (usam `EmailCaptureWebApplicationFactory`) |
| Security headers | `Integration/SecurityHeadersIntegrationTests.cs` |
| Health checks | Não há teste de integração dedicado a `/health*` neste diretório — não confirmado por teste automatizado nesta auditoria. |

## 4. Mapeamento — Rotas e páginas (`02-routing-and-pages.md`)

| Página/fluxo | Teste(s) |
|---|---|
| `Login.razor` (componente, sem HTTP) | `Components/Authentication/LoginTests.cs` |
| `/wallet` fim a fim | `Components/Wallet/WalletComponentTests.cs`, `WalletUiCoverageTests.cs` |
| `/daily` — arquitetura de scroll da página | `Components/Layout/DailyPageScrollArchitectureTests.cs` |
| Consistência visual entre páginas de entrada sob `OnboardingLayout` (`/welcome`, `/login`, `/profile/create`, Identity, Tutorial) | `Components/Visual/EntryFlowVisualConsistencyTests.cs` — nome preservado da Sprint 16.7; `/` saiu deste grupo na Sprint 20.5 (agora `PublicLayout`, não `OnboardingLayout`), ver linha abaixo |
| `/` — Home pública (EPIC 20, Sprint 20.5) | `Components/Home/HomeTests.cs` (h1 único, capacidades reais, ausência de métricas fabricadas, CTA anônimo/autenticado, IDs de âncora) |
| Fluxo real via browser: criar conta → confirmação pendente | `E2E: AccountLifecycleTests.CreateAccount_ReachesEmailConfirmationPending` |
| Fluxo real via browser: login → onboarding → `/daily` | `E2E: AccountLifecycleTests.Login_CompletesOnboarding_ReachesDashboard` |
| Fluxo real via browser: logout | `E2E: AccountLifecycleTests.Logout_EndsSessionAndBlocksDashboard` |
| Fluxo real via browser: visitante anônimo em `/`, sem redirect, CTA para `/login` | `E2E: HomeTests.AnonymousVisitor_SeesHomeWithoutRedirect`, `HomeTests.AnonymousVisitor_GetStartedCtaReachesLogin` (Sprint 20.5) |

Nenhum teste de componente/integração dedicado foi encontrado para as 5 páginas de `Identity`
individualmente (`ConfirmEmail`, `ResetPassword`, `ForgotPassword`, `ResendConfirmation`,
`EmailConfirmationSent`) além da cobertura HTTP em `EmailConfirmationIntegrationTests.cs`/
`PasswordResetIntegrationTests.cs` (§3) — essas cobrem o comando/handler por trás da página, não a
renderização do componente Razor em si via bUnit.

## 5. Mapeamento — Layouts (`03-layouts.md`)

| Componente | Teste(s) |
|---|---|
| `ProfileSidePanel.razor` | `Components/Layout/ProfileSidePanelTests.cs` |
| `AccountSidePanel.razor` | `Components/Layout/AccountSidePanelTests.cs` |
| `BeeDayPageHeader`/`BeeDaySectionHeader` (Design System, usado por `Account`/`Wallet`) | `Components/Layout/BeeDayHeaderTests.cs` — nome do arquivo sugere a navegação, mas testa os cabeçalhos do Design System |
| `BeeDaySettingsForm`/`BeeDaySettingsSection` (Design System, usado por `Account`) | `Components/Layout/BeeDaySettingsTests.cs` |
| `BeeDayHero` (Design System, catálogo — primeiro consumidor de produto real desde a Sprint 20.5, ver `Home.razor`) | `Components/Layout/BeeDayHeroTests.cs` |
| `PublicHeader.razor`/`PublicLayout.razor` (EPIC 20, Sprint 20.4/20.5) | `Components/Layout/PublicHeaderTests.cs`, `PublicLayoutTests.cs` |
| `DesktopSidebar.razor` (EPIC 21, Sprint 21.3) | `Components/Layout/DesktopSidebarTests.cs` |
| `MobileHeader.razor` (EPIC 21, Sprint 21.3) | `Components/Layout/MobileHeaderTests.cs` |
| `MobileSidebar.razor` (EPIC 21, Sprint 21.3) | `Components/Layout/MobileSidebarTests.cs` |
| `NavigationItem.razor`/`NavigationItems.razor` (EPIC 21, Sprint 21.3) | `Components/Layout/NavigationItemTests.cs`, `NavigationItemsTests.cs` |
| Contrato de shell (`MainLayout`/`DesktopSidebar`/`RightRail`/`MobileHeader`/`MobileSidebar`, EPIC 21) | `Components/Layout/ShellFoundationTests.cs` |

`MainLayout.razor`, `OnboardingLayout.razor` e `AppFooter.razor` não têm arquivo de teste dedicado
identificado nesta auditoria. `TopNavigation.razor` foi removida na Sprint 21.3 (EPIC 21).

## 6. Mapeamento — Feature components (`04-feature-components.md`)

| Área | Teste(s) |
|---|---|
| Dashboard — colunas, filtro, contexto de projeto | `Components/Dashboard/DashboardColumnTests.cs`, `ActivityFilterBarTests.cs`, `ProjectContextFilterTests.cs` |
| Habits — visual/métricas | `Components/Dashboard/HabitCardMetricsTests.cs`, `HabitVisualStateTests.cs` |
| Projects — editor | `Components/Projects/ProjectEditorModalTests.cs` |
| Wallet — cartão de transação, modais de transação/tag | `Components/Wallet/TransactionCardTests.cs`, `TransactionFormModalTests.cs`, `TagFormModalTests.cs` |
| Experience — feedback de level-up, barra de XP | `Components/Experience/BeeDayFeedbackTests.cs`, `ExperienceBarTests.cs` |
| Reordenação (`SortableOrder.Move`, puro) | `Components/Behaviors/SortableOrderTests.cs` |
| Toast/confirmação/estados vazios (Feedback do Design System, usado por toda Feature) | `Components/Feedback/BeeDayConfirmDialogTests.cs`, `BeeDayToastHostTests.cs`, `FeedbackComponentTests.cs` |
| Editores genéricos (`EditorModalShell`) | `Components/DesignSystem/EditorModalShellTests.cs`, `CardClickToEditTests.cs` |
| Formulários (Design System) | `Components/Forms/BeeDayFormTests.cs` |

## 7. Mapeamento — Design System / interop (`05-design-system-integration.md`)

| Componente | Teste(s) |
|---|---|
| `BeeDayButton` | `Components/Buttons/BeeDayButtonTests.cs` |
| `BeeDayCard` / `BeeDayCardMenu` (+ cálculo de posicionamento) | `Components/Cards/BeeDayCardTests.cs`, `BeeDayCardMenuTests.cs`, `CardMenuPlacementCalculatorTests.cs` |
| `ActivityAttributeSelect`/`Badge` (+ cálculo de posicionamento) | `Components/Attributes/ActivityAttributeComponentTests.cs`, `ActivityAttributeSelectTests.cs`, `AttributeSelectPlacementCalculatorTests.cs` |
| `PixelIcon` | `Components/Icons/PixelIconTests.cs` |
| `BeeDayBrand` / `SearchHighlight` | `Components/Text/BeeDayBrandTests.cs`, `SearchHighlightTests.cs` |
| Isolamento do adapter de tema NES (`pixel-nes.css`) | `Components/DesignSystem/PixelNesAdapterIsolationTests.cs` |
| Componentes de atividade (badges/atributos combinados) | `Components/DesignSystem/ActivityComponentsTests.cs` |

Os 3 módulos `.js` em si (`beeday-sortable.js`, `beeday-card-menu.js`,
`activity-attribute-select.js`) não são executados por `BeeDay.Web.Tests` (bUnit não roda um
browser real) — sua cobertura prática vem apenas dos testes E2E (Playwright, browser real) listados
em §4 e §8, que exercitam o comportamento visível resultante (reordenar cards, abrir/fechar menus),
não o código JS isoladamente.

## 8. `BeeDay.E2E.Tests`

6 classes de teste, 18 fluxos (`grep -c "\[Fact\]"`, verificado diretamente na Sprint 21.3), sobre
a infraestrutura descrita em `docs/testing/01-testing-strategy.md` §7 (`PlaywrightAppFixture`,
`E2ETestBase`, `E2EWebApplicationFactory`):

| Classe | Fluxos |
|---|---|
| `AccountLifecycleTests.cs` | Criar conta → confirmação pendente; login → onboarding → `/daily`; logout; editar perfil |
| `HabitAndTaskTests.cs` | Criar/completar hábito (saldo + XP visíveis); criar/completar task |
| `WalletTests.cs` | Criar tag + transação no Wallet, saldo atualizado |
| `HomeTests.cs` (Sprint 20.5, EPIC 20) | Visitante anônimo vê a Home em `/` sem redirect; CTA "Get started" alcança `/login` |
| `ShellResponsiveLayoutTests.cs` (EPIC 21, Sprint 21.2/21.3) | Geometria real da sidebar/rail desktop; visibilidade condicional mobile vs. desktop sem sobreposição; ausência de overflow horizontal real |
| `NavigationTests.cs` (EPIC 21, Sprint 21.3) | `aria-current` real ao navegar e em deep link; abrir/fechar o drawer mobile via hambúrguer/backdrop/Escape/botão dedicado; foco real move para o drawer ao abrir; navegar por um item do drawer fecha-o; Logout continua acessível |

## 9. Contagem de referência

`docs/testing/01-testing-strategy.md` §1 é a fonte canônica da contagem de testes por projeto —
768 testes aprovados (93 Domain, 73 Application, 129 Infrastructure, 464 Web, 9 E2E), confirmado
por execução real na Sprint 20.5 (não repetido aqui em detalhe para evitar duplicação).

## 10. Achado

- `Components/Layout/BeeDayHeaderTests.cs` — o nome sugere que testa a navegação da aplicação; na
  verdade testa `BeeDayPageHeader`/`BeeDaySectionHeader` do Design System, sem relação com
  `Components/Layout/`'s navegação real (`DesktopSidebar`/`MobileHeader`/`MobileSidebar`, com seus
  próprios arquivos de teste — ver §5). `TopNavigation.razor`, que este achado apontava como sem
  cobertura, foi removida na Sprint 21.3 (EPIC 21) — achado encerrado, não mais aplicável.

## 11. Fontes de verdade

- Lista de arquivos de `tests/BeeDay.Web.Tests/**/*.cs` (64 arquivos, `Glob` direto nesta Sprint —
  61 na Sprint 18.8, `+1` de `Components/Home/HomeTests.cs`; a diferença residual de +2 não é
  atribuída a esta Sprint especificamente, não reauditada arquivo a arquivo) e
  `tests/BeeDay.E2E.Tests/*.cs` (8 arquivos — `+1` de `HomeTests.cs`, Sprint 20.5), incluindo
  `E2EWebApplicationFactory.cs`/`PlaywrightAppFixture.cs`/`E2ETestBase.cs`/`Usings.cs` como
  infraestrutura.
- `tests/BeeDay.Web.Tests/BeeDay.Web.Tests.csproj`, `tests/BeeDay.E2E.Tests/BeeDay.E2E.Tests.csproj`.
- [`docs/testing/01-testing-strategy.md`](../testing/01-testing-strategy.md) para a infraestrutura
  compartilhada (não reverificada em detalhe nesta Sprint — a leitura desta sessão confirma que a
  descrição de `BeeDayWebApplicationFactory`, `PlaywrightAppFixture` e `E2ETestBase` continua
  batendo com o código atual).
