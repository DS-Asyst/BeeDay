# Routing and Pages

**Fonte da verdade:** verificado por busca direta de `@page` em todo
`src/BeeDay.Web/Components/**/*.razor` e leitura das 3 primeiras linhas de cada arquivo resultante
(`@page`, `@attribute`, `@layout`/`@rendermode`), mais `Components/Routes.razor` e `Components/App.razor`.

**Última verificação:** 2026-08-17 (Sprint 29.2) — adicionado o §9 sobre as 11 páginas
institucionais (EPIC 27) e a correção de layout full-bleed dessa Sprint; a tabela de rotas em §3
**não foi reauditada** nesta passagem e continua sem essas 11 rotas — gap pré-existente desde a
EPIC 27, fora do escopo desta Sprint, reportado e não corrigido silenciosamente.
Verificação anterior: 2026-08-16 (Sprint 25.17 — beeday Experience System) — adicionadas as 21
rotas públicas do `beeday Experience System` (`/experience-system/*`), todas `PublicLayout` e
`AllowAnonymous`; corrigida uma lacuna encontrada nesta Sprint: `/brand/typography`
(`Features/Brand/Pages/TypographyGuidelines.razor`, introduzida na Sprint 25.4) nunca tinha sido
adicionada a este documento — a tabela abaixo já reflete a contagem correta incluindo essa correção,
não apenas as rotas novas desta Sprint. Verificação anterior: 2026-08-13 (Sprint 21.12, EPIC 21) —
`/profile` é a experiência pessoal autenticada e o destino normal após login/onboarding; `/home`
redireciona para `/profile` por compatibilidade; `/daily` permanece o quadro operacional. Em
2026-08-11, Sprint 20.5 da EPIC 20, `/` deixou de redirecionar (`Entry.razor` removido) e passou a
servir a Home pública oficial (`Features/Home/Pages/Home.razor`, layout `PublicLayout`); demais
rotas preservadas da verificação de 2026-08-07.

## 1. Objetivo

Mapear as 42 rotas `@page` do repositório, seu layout e atributo de autorização, e descrever o
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

- `DefaultLayout` é sempre `MainLayout` — uma página só foge dele com um `@layout` explícito. Duas
  famílias de layout explícito existem: `OnboardingLayout` (todas as páginas de
  Authentication/Identity/Onboarding/ProfileCreation, sem navegação) e, desde a Sprint 20.4/20.5
  (EPIC 20), `PublicLayout` (`PublicHeader` + `@Body` + `AppFooter`) — usado por `/`, pela guideline
  pública de marca (`/brand/typography`) e, desde a Sprint 25.17, por todo o `beeday Experience
  System` (`/experience-system/*`, ver §9) — ver tabela abaixo.
- `NotAuthorized`: `RedirectToLogin.razor` — não usa `NavigationManager.NavigateTo` comum; espera o
  primeiro `OnAfterRender` e força `forceLoad: true, replace: true` para `/login?returnUrl=...`,
  preservando o path relativo atual como retorno.
- `Authorizing`: mostra `BeeDayLoading` (Design System) enquanto o estado de autenticação resolve.
- Toda rota autenticada usa `@attribute [Authorize]`; toda rota pública, `@attribute [AllowAnonymous]`
  — não há página sem um dos dois atributos declarado explicitamente.

## 3. Todas as rotas

| Rota | Arquivo | Layout | Autorização | `@rendermode` explícito |
|---|---|---|---|---|
| `/` | `Features/Home/Pages/Home.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/welcome` | `Features/ProfileCreation/Pages/Welcome.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/login` | `Features/Authentication/Pages/Login.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/profile/create` | `Features/ProfileCreation/Pages/CreateProfile.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/forgot-password` | `Features/Identity/Pages/ForgotPassword.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/resend-confirmation` | `Features/Identity/Pages/ResendConfirmation.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/email-confirmation-sent` | `Features/Identity/Pages/EmailConfirmationSent.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/confirm-email` | `Features/Identity/Pages/ConfirmEmail.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/account/reset-password` | `Features/Identity/Pages/ResetPassword.razor` | `OnboardingLayout` | `AllowAnonymous` | — |
| `/onboarding/tutorial` | `Features/Onboarding/Pages/Tutorial.razor` | `OnboardingLayout` | `Authorize` | — |
| `/profile` | `Features/Dashboard/Pages/DashboardHome.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/home` | `Features/Dashboard/Pages/LegacyHomeRedirect.razor` | `MainLayout` (padrão) | `Authorize`; redirect para `/profile` | — |
| `/daily` | `Features/Dashboard/Pages/Home.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/wallet` | `Features/Wallets/Pages/Wallet.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/account`, `/settings` (mesmo componente, 2 rotas) | `Features/Account/Pages/Account.razor` | `MainLayout` (padrão) | `Authorize` | `InteractiveServer` |
| `/design-system/icons` | `DesignSystem/Pages/IconCatalog.razor` | `MainLayout` (padrão) | `Authorize` | — |
| `/design-system/hero` | `DesignSystem/Pages/HeroCatalog.razor` | `MainLayout` (padrão) | `Authorize` | — |
| `/brand/typography`, `/experience-system/brand/typography` (mesmo componente, 2 rotas) | `Features/Brand/Pages/TypographyGuidelines.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system` | `Features/ExperienceSystem/Pages/ExperienceSystemHome.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand` | `Features/ExperienceSystem/Pages/Brand/BrandOverview.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand/identity` | `Features/ExperienceSystem/Pages/Brand/BrandIdentity.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand/wordmark` | `Features/ExperienceSystem/Pages/Brand/BrandWordmark.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand/color` | `Features/ExperienceSystem/Pages/Brand/BrandColor.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand/illustration` | `Features/ExperienceSystem/Pages/Brand/BrandIllustration.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand/characters` | `Features/ExperienceSystem/Pages/Brand/BrandCharacters.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/brand/writing` | `Features/ExperienceSystem/Pages/Brand/BrandWriting.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ui` | `Features/ExperienceSystem/Pages/Ui/UiOverview.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ui/foundations` | `Features/ExperienceSystem/Pages/Ui/UiFoundations.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ui/components` | `Features/ExperienceSystem/Pages/Ui/UiComponents.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ui/product-patterns` | `Features/ExperienceSystem/Pages/Ui/UiProductPatterns.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ui/interaction` | `Features/ExperienceSystem/Pages/Ui/UiInteraction.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ui/layout` | `Features/ExperienceSystem/Pages/Ui/UiLayout.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ux` | `Features/ExperienceSystem/Pages/Ux/UxOverview.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ux/accessibility` | `Features/ExperienceSystem/Pages/Ux/UxAccessibility.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ux/responsive` | `Features/ExperienceSystem/Pages/Ux/UxResponsive.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ux/localization` | `Features/ExperienceSystem/Pages/Ux/UxLocalization.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ux/motion` | `Features/ExperienceSystem/Pages/Ux/UxMotion.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/experience-system/ux/performance` | `Features/ExperienceSystem/Pages/Ux/UxPerformance.razor` | `PublicLayout` | `AllowAnonymous` | — |
| `/not-found` | `Pages/NotFound.razor` | `MainLayout` (explícito) | `AllowAnonymous` | — |
| `/Error` | `Pages/Error.razor` | `MainLayout` (padrão) | `AllowAnonymous` | — |

42 rotas em 40 arquivos `.razor`. Duas famílias de rota compartilham componente: `Account.razor`
(`/account`, `/settings`) e `TypographyGuidelines.razor` (`/brand/typography`,
`/experience-system/brand/typography` — a segunda rota foi adicionada na Sprint 25.17 como alias
canônico dentro do `beeday Experience System`, preservando a primeira por compatibilidade e sem
duplicar conteúdo).

## 4. Páginas com `@rendermode` explícito vs. implícito

O Profile autenticado (`/profile`), o Daily (`/daily`), `Wallet` e `Account` declaram
`@rendermode InteractiveServer` por página
— 4 páginas desde a Sprint 21.10: o antigo `Entry.razor` (que declarava
`@rendermode InteractiveServer`) foi removido junto com a rota `/` que resolvia; a nova
`Features/Home/Pages/Home.razor` que atende `/` não declara `@rendermode` por página. As demais
herdam o modo interativo de `<Routes @rendermode="InteractiveServer" />` em `App.razor` —
funcionalmente equivalente hoje (toda a aplicação roda em modo interativo), mas a declaração por
página é redundante nesses 4 casos específicos, não um modo diferente.

## 5. Rotas que ignoram `BeeDayWebService` (`ISender` direto)

`Wallet.razor` e as 5 páginas de `Features/Identity/Pages/` injetam `MediatR.ISender` diretamente
em vez de `BeeDayWebService` — ver
[`04-feature-components.md`](04-feature-components.md#8-acesso-direto-a-isender-desvio-do-padrão-beedaywebservice)
para o detalhamento e o achado de documentação relacionado.

## 6. Páginas de catálogo do Design System

`/design-system/icons` e `/design-system/hero` (`DesignSystem/Pages/`) não pertencem a nenhuma área
de Feature — são páginas de desenvolvimento/QA visual que renderizam todo `BeeDayIconRegistry` e
todas as variantes de `BeeDayHero`, respectivamente, injetando `IWebHostEnvironment` (não usado para
restringir acesso — ambas exigem apenas `[Authorize]`, acessíveis a qualquer usuário autenticado em
qualquer ambiente). Ver [`05-design-system-integration.md`](05-design-system-integration.md).

## 7. Páginas de erro

`/not-found` (`NotFound.razor`) e `/Error` (`Error.razor`) usam `SharedResources` para título e
mensagens em `en-US`/`pt-BR`. `/Error` exibe
`Activity.Current?.Id ?? HttpContext.TraceIdentifier` (via `[CascadingParameter] HttpContext`)
quando não vazio e instruções localizadas sobre Development; não há checagem de ambiente no
componente em si. Nenhuma das duas páginas é acionada
automaticamente pelo `GlobalExceptionHandler` (que responde `ProblemDetails`, não uma página Blazor)
— `/Error` só é alcançável por navegação direta; nada no repositório redireciona para ela.

## 8. A rota `/` — Home pública (Sprint 20.5, EPIC 20)

Até a Sprint 20.4, `/` era atendida por `Entry.razor` (`Features/ProfileCreation/Pages/`), que não
tinha conteúdo próprio — renderizava um estado de loading e, no `OnAfterRenderAsync`, resolvia
silenciosamente o destino real do usuário (anônimo → `/login`; autenticado sem perfil →
`/profile/create`; com onboarding incompleto → `/onboarding/tutorial`; pronto → `/daily`) via
`NavigateTo(..., forceLoad: true, replace: true)`.

Na Sprint 20.5, `Entry.razor` foi removido (sem outros consumidores confirmados por busca
repo-wide) e `/` passou a ser atendida por `Features/Home/Pages/Home.razor`, sob `PublicLayout` —
uma Home pública real, com conteúdo institucional, visível tanto para visitantes anônimos quanto
para usuários autenticados, sem nenhum redirecionamento automático.

A política de destino pós-autenticação (perfil → onboarding → `/profile`) **não foi removida** — ela
continua ativa em `LoginDestinationResolver.Resolve` (pós-login, `Program.cs`) e em
`CreateProfile.razor.cs` (pós-criação de perfil). Um terceiro consumidor foi adicionado:
`AuthenticatedEntryDestinationResolver` (`Services/Authentication/`), que envolve
`BeeDayWebService.GetCurrentUserAsync()` + `LoginDestinationResolver.Resolve` para o CTA
"Continue to BeeDay" tanto de `PublicHeader` quanto da própria `Home.razor` — reutilizando a regra
existente em vez de duplicá-la uma quarta vez.

## 9. As 11 páginas institucionais (EPIC 27, layout corrigido na Sprint 29.2)

`/mission`, `/efficacy`, `/contact`, `/beeday`, `/beeday-plus`, `/android`, `/ios`, `/faqs`,
`/community-guidelines`, `/terms`, `/privacy` — `Components/Features/Institutional/Pages/`, sob
`PublicLayout`. Cada rota usa um de quatro templates compartilhados
(`Components/Institutional/Components/`) — `EditorialPageTemplate` (Mission, Efficacy, Contact),
`ProductPageTemplate` (beeday, beeday Plus, Android, iOS), `HelpPageTemplate` (FAQs) ou
`LegalDocumentPageTemplate` (Community guidelines, Terms, Privacy) — todos passando apenas
`PageContext`/`Title`/`Description` (mais `PrimaryAction` no Product) para o shell real,
`InstitutionalPageShell`, que renderiza um `BeeDayHero` colorido seguido do corpo da página. Nenhuma
página institucional define CSS próprio; o vocabulário compartilhado vive em `wwwroot/css/
institutional.css`.

`Surface` (a cor sólida do `BeeDayHero`) tem um default por template — `Cor0` para
Editorial/Help/Product, `Cor8` para Legal — e nenhuma das 11 páginas o sobrescreve; ver
[`brand/03-color-palette.md`](../brand/03-color-palette.md) para a paleta completa e a regra que
restringe page headers a `Cor0`/`Cor8` (os dois únicos tokens cujo contraste com texto branco passa
WCAG AA).

**Sprint 29.2 — full-bleed e alinhamento de eixo:** até então, `InstitutionalPageShell` envolvia o
`BeeDayHero` e o corpo da página num único `<article class="institutional-page">`. A regra
compartilhada de reading-width de `polish.css` (`.beeday-main > :where(section, article, .beeday-page,
.page-content)`) casa apenas com filhos diretos de `.beeday-main` — como esse `<article>` era um filho
direto, ele (hero incluído) ficava limitado a 72rem, fazendo o header colorido renderizar como um
pequeno card centralizado em vez de uma faixa full-bleed. O shell agora renderiza o hero e o corpo
como irmãos, filhos diretos de `.beeday-main` — o mesmo padrão que `ExperienceSystemPage` já usava
para seu próprio `BeeDayHero` (§10). Isso também corrigiu um desalinhamento de eixo: os quatro
templates antes estreitavam o corpo para 42-48rem (editorial/help/legal — product já usava os 72rem
da regra base, por falta de uma classe `--product` correspondente) enquanto o hero permanecia em
72rem; as quatro famílias agora compartilham a mesma largura de 72rem do hero.

Diferente do Experience System (§10), as páginas institucionais não têm uma navegação lateral
contextual: a taxonomia real que as agrupa é apenas a lista plana de grupos do `AppFooter` (About us,
Products, Apps, Help, Privacy and terms), sem subcategorias — insuficiente para justificar uma
sidebar sem inventar uma hierarquia que não existe.

## 10. O `beeday Experience System` (Sprint 25.17)

`/experience-system` é o ponto de entrada público e navegável para tudo que a EPIC 25 formalizou —
Brand System, UI Design System e UX System — distinto de `docs/` (que continua sendo a
documentação técnica para quem desenvolve o repositório, não uma superfície do produto). A área
vive inteira em `Components/Features/ExperienceSystem/`, com um único catálogo de recursos
(`ExperienceSystemResources`, `en-US`/`pt-BR`) cobrindo a raiz, as três páginas de overview de
pilar e 15 dos 17 tópicos — `/brand/typography` continua sendo o dono do seu próprio conteúdo
(`BrandTypographyResources`), apenas ganhando uma segunda rota dentro desta área.

Composição, não 21 implementações independentes: toda página usa o componente compartilhado
`ExperienceSystemPage` (`Components/ExperienceSystemPage.razor`), que já resolve `BeeDayPageHeader`
(eyebrow/título/descrição), a navegação entre os três pilares (`ExperienceSystemPillarNav`) e,
quando aplicável, a navegação entre os tópicos do pilar atual (`ExperienceSystemTopicNav`) —
suprimida nas três páginas de overview de pilar porque elas já listam os mesmos tópicos, com mais
contexto, através de `ExperienceSystemTopicGrid` (reaproveitado também pela raiz, para os três
pilares). Cada página individual contribui apenas seu próprio conteúdo de corpo, componentes e CSS
já existentes do Design System (`BeeDayPageHeader`, `BeeDayBrand`, tabelas e `BeeDayCard`-adjacent
patterns) — nenhuma folha de CSS global paralela foi criada; o vocabulário visual compartilhado
(`.experience-system-section`, tabelas, callouts) vive no CSS isolado de `ExperienceSystemPage`, via
seletores `::deep`.

## 11. Fontes de verdade

- Busca `@page` em `src/BeeDay.Web/Components/**/*.razor` (42 ocorrências, 40 arquivos — inclui a
  rota de compatibilidade `LegacyHomeRedirect.razor` da Sprint 21.12 e as 21 rotas do `beeday
  Experience System`/Brand Guidelines reconciliadas na Sprint 25.17).
- Primeiras linhas de cada um dos 40 arquivos resultantes.
- `src/BeeDay.Web/Components/Routes.razor`, `App.razor`.
- `src/BeeDay.Web/Components/Features/Home/Pages/Home.razor`,
  `src/BeeDay.Web/Components/Layout/PublicLayout.razor`, `PublicHeader.razor`,
  `src/BeeDay.Web/Services/Authentication/AuthenticatedEntryDestinationResolver.cs` (Sprint 20.5).
- `src/BeeDay.Web/Components/Features/ExperienceSystem/` completo, incluindo
  `Components/ExperienceSystemPage.razor(.css)`, `ExperienceSystemPillarNav.razor`,
  `ExperienceSystemTopicNav.razor`, `ExperienceSystemTopicGrid.razor` e os 20 arquivos de página sob
  `Pages/` (Sprint 25.17).
