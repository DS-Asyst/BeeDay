# Web

Documentação do `BeeDay.Web` — reconstruída por completo na Sprint 16.7 a partir exclusivamente do
código atual (`src/BeeDay.Web/`, `tests/BeeDay.Web.Tests/`, `tests/BeeDay.E2E.Tests/`). Nenhuma
afirmação vem de `docs/history/` ou de sprints anteriores sem reverificação direta no código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes de verdade".

## Responsabilidade

`BeeDay.Web` é o composition root e a única camada de apresentação do sistema: aplicação Blazor
Server (.NET 10, `Microsoft.NET.Sdk.Web`), autenticação por cookie, health checks, e todo componente
Razor (páginas, layouts, Design System, Feature components). É a única camada que pode depender de
`BeeDay.Infrastructure` (apenas para registrar DI em `Program.cs` — ver
[`docs/infrastructure/README.md`](../infrastructure/README.md), seção "Integração com Web") e
implementa, localmente, a única interface técnica que a Web possui: `ICurrentUserContext`
(`HttpCurrentUserContext`, lida do `HttpContext` via cookie).

## Organização

```text
src/BeeDay.Web/
├── Program.cs                composition root: DI, pipeline HTTP, endpoints /auth/*, health checks
├── Configuration/             ProductionHostingOptions (DataProtection, forwarded headers)
├── Diagnostics/                CorrelationIdMiddleware, GlobalExceptionHandler, WebEventIds
├── HealthChecks/                HealthCheckResponseWriter (formato JSON de /health*)
├── Services/
│   ├── BeeDayWebService.cs       fachada MediatR usada pela maioria dos componentes Feature
│   ├── AuthenticatedUserInitializer, HttpCurrentUserContext, ToastService,
│   │   CardActionMenuCoordinator
│   └── Authentication/            LoginDestinationResolver, LoginRateLimiterFactory/Options,
│                                    BeeDayClaimTypes
├── Components/
│   ├── App.razor, Routes.razor    shell HTML raiz e Router
│   ├── Layout/                     MainLayout, OnboardingLayout, DesktopSidebar, MobileHeader/
│   │                                  MobileSidebar, NavigationItem(s), side panels, footer
│   ├── DesignSystem/                 Buttons, Cards, Forms, Feedback, Icons, Layout, Modals, Text —
│   │                                  ver docs/design-system/ (reservado) e §5 abaixo
│   ├── Behaviors/DragDrop/            BeeDaySortable (JS interop de reordenação)
│   ├── Features/                       13 áreas de funcionalidade — ver 04-feature-components.md
│   └── Pages/                           NotFound, Error
└── wwwroot/                               css/, js/ (3 módulos ES), icons/ (sprite SVG), images/
```

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-composition-root.md`](01-composition-root.md) | `Program.cs`: DI, pipeline HTTP, autenticação por cookie, endpoints `/auth/*`, rate limiting, health checks |
| [`02-routing-and-pages.md`](02-routing-and-pages.md) | `Routes.razor`, `App.razor`, as 18 rotas `@page`, layout e atributo de autorização de cada uma |
| [`03-layouts.md`](03-layouts.md) | `MainLayout`, `OnboardingLayout`, navegação, painéis laterais, rodapé, `ReconnectModal` |
| [`04-feature-components.md`](04-feature-components.md) | As 13 áreas de `Components/Features/` — componentes, state, models, como cada uma chama Application |
| [`05-design-system-integration.md`](05-design-system-integration.md) | Como a Web compõe o Design System, os 3 módulos de JS interop, ordem de carregamento de CSS |
| [`06-testing.md`](06-testing.md) | Mapeamento componente → teste em `BeeDay.Web.Tests` (bUnit + integração) e `BeeDay.E2E.Tests` (Playwright) |

## Integração com Application

A grande maioria dos componentes Feature chama `BeeDay.Application` através de uma única fachada,
`BeeDayWebService` (`src/BeeDay.Web/Services/BeeDayWebService.cs`), que envolve `ISender.Send(...)`
para Habits, Tasks, Todos, Projects, Ordering, Users/Account, Onboarding e o carregamento do
Dashboard. **Isso não é universal**: `Wallet.razor` e as 5 páginas de `Features/Identity/Pages/`
(`ConfirmEmail`, `ResetPassword`, `ForgotPassword`, `ResendConfirmation`, `EmailConfirmationSent`)
injetam `MediatR.ISender` diretamente e nunca passam por `BeeDayWebService` — ver
[`04-feature-components.md`](04-feature-components.md#8-acesso-direto-a-isender-desvio-do-padrão-beedaywebservice).

## Ordem de leitura recomendada

1. `01-composition-root.md` — o que roda antes de qualquer componente Razor existir.
2. `02-routing-and-pages.md` — o mapa de todas as 18 rotas.
3. `03-layouts.md` — a casca visual comum a toda rota autenticada.
4. `04-feature-components.md` — cada área de funcionalidade em detalhe.
5. `05-design-system-integration.md` — os blocos reutilizáveis que as Features compõem.
6. `06-testing.md` — como cada camada acima é coberta por teste.

## Achados relevantes (reportados, não corrigidos)

- **`docs/architecture/05-runtime-flows.md` §2 afirma "nenhum componente Razor injeta `ISender`
  diretamente"** — essa afirmação está desatualizada/incorreta: `Wallet.razor` e as 5 páginas de
  `Features/Identity/Pages/` fazem exatamente isso (`@inject MediatR.ISender Sender`). Fora do
  escopo desta Sprint corrigir `docs/architecture/`; reportado aqui para correção em Sprint futura.
- **Achado totalmente resolvido (histórico — verificado na Sprint 20.4, EPIC 20; reconfirmado na
  Sprint 21.3, EPIC 21):** o texto literal `<span>LEVEL</span><span>UP</span>` em
  `Components/Layout/AccountSidePanel.razor` (e no extinto `TopNavigation.razor`) **não existe
  mais**. O ponto que este documento ainda registrava como "permanece válido" — `TopNavigation`/
  `AccountSidePanel` usarem markup próprio em vez de `Components/DesignSystem/Text/BeeDayBrand.razor`
  — também está resolvido: a Sprint 20.7 (EPIC 20) migrou ambos para `<BeeDayBrand />` via o hook
  `--beeday-brand-color` (ver `docs/epics/20-home-visual-experience/README.md`, "TopNavigation
  Migration"), e a Sprint 21.3 removeu `TopNavigation.razor` inteiramente — `DesktopSidebar`/
  `MobileHeader`/`MobileSidebar` (seus sucessores) já nascem usando `<BeeDayBrand />`. Nenhum
  componente de `Components/Layout/` renderiza marca própria hoje.
- `Components/Features/ProfileCreation/Pages/Welcome.razor` (rota `/welcome`) define
  `<PageTitle>Login | BeeDay</PageTitle>` — título incorreto para uma página que só redireciona para
  `/login`. A rota `/` (`Entry.razor`) já resolve o destino real via estado de autenticação; `/welcome`
  não é linkada por nenhum outro componente do repositório (busca por `href="/welcome"` e
  `NavigateTo("/welcome"` sem resultado) — possível rota morta.
- `Services/Authentication/BeeDayClaimTypes.SessionVersion` tem valor literal `"levelup:session_version"`
  — já reportado em `docs/architecture/README.md`, não duplicado aqui além desta referência.
- Os dois módulos JS com estado de posicionamento (`beeday-sortable.js`, `beeday-card-menu.js`) são
  invalidados por query string hardcoded no C# que os importa (`?v=20260721-f13-dragfix`,
  `?v=20260729-1`) em vez de um mecanismo de cache-busting automático (hash de conteúdo, `Assets[]`
  do mapa de estáticos usado pelo resto do projeto). Funciona, mas exige lembrar de trocar a string a
  cada mudança no arquivo — `activity-attribute-select.js` não tem esse sufixo.
