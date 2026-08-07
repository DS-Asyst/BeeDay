# Routing and Pages

**Fonte da verdade:** verificado por busca direta de `@page` em todo
`src/BeeDay.Web/Components/**/*.razor` e leitura das 3 primeiras linhas de cada arquivo resultante
(`@page`, `@attribute`, `@layout`/`@rendermode`), mais `Components/Routes.razor` e `Components/App.razor`.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Mapear as 18 rotas `@page` do repositório, seu layout e atributo de autorização, e descrever o
shell HTML (`App.razor`) e o `Router` (`Routes.razor`) que as hospedam.

## 2. Shell e Router

`App.razor` é o único `.razor` sem `@page` que produz HTML de documento completo (`<!DOCTYPE html>`).
Carrega, nesta ordem: fontes Google (`Inter`, `Jersey 25`), `app.css`, 15 folhas de CSS específicas
sob `css/` (ver [`05-design-system-integration.md`](05-design-system-integration.md) §3 para a
ordem completa e por que ela importa), o bundle isolado de `BeeDay.Web.styles.css`, `<ImportMap />`
e `<HeadOutlet />`. O `<body>` contém apenas `<Routes @rendermode="InteractiveServer" />`,
`<ReconnectModal />` e o script `_framework/blazor.web.js`.

`Routes.razor`:

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
                <NotAuthorized>
                    <BeeDay.Web.Components.Features.Authentication.Components.RedirectToLogin />
                </NotAuthorized>
                <Authorizing>
                    <BeeDayLoading IsVisible="true" Label="Checking your session..." />
                </Authorizing>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="routeData" Selector="h1" />
        </Found>
    </Router>
</CascadingAuthenticationState>
```

- `DefaultLayout` é sempre `MainLayout` — uma página só foge dele com um `@layout` explícito
  (todas as páginas de Authentication/Identity/Onboarding/ProfileCreation usam
  `OnboardingLayout`, ver tabela abaixo).
- `NotAuthorized`: `RedirectToLogin.razor` — não usa `NavigationManager.NavigateTo` comum; espera o
  primeiro `OnAfterRender` e força `forceLoad: true, replace: true` para `/login?returnUrl=...`,
  preservando o path relativo atual como retorno.
- `Authorizing`: mostra `BeeDayLoading` (Design System) enquanto o estado de autenticação resolve.
- Toda rota autenticada usa `@attribute [Authorize]`; toda rota pública, `@attribute [AllowAnonymous]`
  — não há página sem um dos dois atributos declarado explicitamente.

## 3. Todas as rotas

| Rota | Arquivo | Layout | Autorização | `@rendermode` explícito |
|---|---|---|---|---|
| `/` | `Features/ProfileCreation/Pages/Entry.razor` | `OnboardingLayout` | `AllowAnonymous` | `InteractiveServer` |
| `/welcome` | `Features/ProfileCreation/Pages/Welcome.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/login` | `Features/Authentication/Pages/Login.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/profile/create` | `Features/ProfileCreation/Pages/CreateProfile.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/forgot-password` | `Features/Identity/Pages/ForgotPassword.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/resend-confirmation` | `Features/Identity/Pages/ResendConfirmation.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/email-confirmation-sent` | `Features/Identity/Pages/EmailConfirmationSent.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/confirm-email` | `Features/Identity/Pages/ConfirmEmail.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/reset-password` | `Features/Identity/Pages/ResetPassword.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/onboarding/tutorial` | `Features/Onboarding/Pages/Tutorial.razor` | `OnboardingLayout` | `Authorize` | — |
| `/daily` | `Features/Dashboard/Pages/Home.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/wallet` | `Features/Wallets/Pages/Wallet.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/account`, `/settings` (mesmo componente, 2 rotas) | `Features/Account/Pages/Account.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/design-system/icons` | `DesignSystem/Pages/IconCatalog.razor` | `MainLayout` (padrão) | `Authorize` | — |
| `/design-system/hero` | `DesignSystem/Pages/HeroCatalog.razor` | `MainLayout` (padrão) | `Authorize` | — |
| `/not-found` | `Pages/NotFound.razor` | `MainLayout` (explícito) | `AllowAnonymous` | — |
| `/Error` | `Pages/Error.razor` | `MainLayout` (padrão) | `AllowAnonymous` | — |

18 rotas em 17 arquivos `.razor` (`Account.razor` declara duas rotas para o mesmo componente).

## 4. Páginas com `@rendermode` explícito vs. implícito

Apenas `Entry`, `Home` (`/daily`), `Wallet` e `Account` declaram `@rendermode InteractiveServer`
por página. As demais herdam o modo interativo de `<Routes @rendermode="InteractiveServer" />` em
`App.razor` — funcionalmente equivalente hoje (toda a aplicação roda em modo interativo), mas a
declaração por página é redundante nesses 4 casos específicos, não um modo diferente.

## 5. Rotas que ignoram `BeeDayWebService` (`ISender` direto)

`Wallet.razor` e as 5 páginas de `Features/Identity/Pages/` injetam `MediatR.ISender` diretamente
em vez de `BeeDayWebService` — ver
[`04-feature-components.md`](04-feature-components.md#8-acesso-direto-a-isender-desvio-do-padrão-beedaywebservice)
para o detalhamento e o achado de documentação relacionado.

## 6. Páginas de catálogo do Design System

`/design-system/icons` e `/design-system/hero` (`DesignSystem/Pages/`) não pertencem a nenhuma área
de Feature — são páginas de desenvolvimento/QA visual que renderizam todo `PixelIconRegistry` e
todas as variantes de `BeeDayHero`, respectivamente, injetando `IWebHostEnvironment` (não usado para
restringir acesso — ambas exigem apenas `[Authorize]`, acessíveis a qualquer usuário autenticado em
qualquer ambiente). Ver [`05-design-system-integration.md`](05-design-system-integration.md).

## 7. Páginas sem estado de erro dedicado

`/not-found` (`NotFound.razor`) é estático (`<h3>Not Found</h3>`); `/Error` (`Error.razor`) é o
template padrão gerado pelo scaffolding ASP.NET Core, sem nenhuma customização: exibe
`Activity.Current?.Id ?? HttpContext.TraceIdentifier` (via `[CascadingParameter] HttpContext`)
sempre que não vazio, e um bloco estático de texto explicando como habilitar o ambiente Development
— não há checagem de ambiente no componente em si. Nenhuma das duas páginas é acionada
automaticamente pelo `GlobalExceptionHandler` (que responde `ProblemDetails`, não uma página Blazor)
— `/Error` só é alcançável por navegação direta; nada no repositório redireciona para ela.

## 8. Fontes de verdade

- Busca `@page` em `src/BeeDay.Web/Components/**/*.razor` (18 ocorrências, 17 arquivos).
- Primeiras linhas de cada um dos 17 arquivos resultantes.
- `src/BeeDay.Web/Components/Routes.razor`, `App.razor`.
