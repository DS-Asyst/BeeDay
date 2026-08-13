# Composition Root

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Program.cs`,
`src/BeeDay.Web/Diagnostics/`, `src/BeeDay.Web/HealthChecks/`, `src/BeeDay.Web/Configuration/` e
`src/BeeDay.Web/Services/Authentication/`. Trechos de código citados abaixo são reais.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Descrever tudo que `Program.cs` monta antes de qualquer componente Razor ser renderizado: registro
de DI, guardas de produção, pipeline HTTP, autenticação por cookie, os dois endpoints minimal API
(`/auth/login`, `/auth/logout`) e health checks.

## 2. Escopo

Dentro: `Program.cs` de ponta a ponta, `Diagnostics/`, `HealthChecks/`, `Configuration/`,
`Services/Authentication/`. Fora: registro de DI feito dentro de `AddBeeDayApplication()`/
`AddBeeDayInfrastructure()` (ver [`docs/application/`](../application/README.md) e
[`docs/infrastructure/05-dependency-injection.md`](../infrastructure/05-dependency-injection.md));
o roteamento Blazor em si (ver [`02-routing-and-pages.md`](02-routing-and-pages.md)).

## 3. Guardas de produção (antes do `Build()`)

Quando `!builder.Environment.IsDevelopment()`, `Program.cs` lança `InvalidOperationException` no
startup se qualquer um destes não estiver satisfeito:

- `BeeDay:IdentityEmail:PublicBaseUrl` deve ser uma URL absoluta HTTPS.
- `BeeDay:Hosting:DataProtectionKeysDirectory` (`ProductionHostingOptions`, seção
  `BeeDay:Hosting`) deve ser um caminho absoluto — usado para persistir as chaves de Data
  Protection em disco (`PersistKeysToFileSystem`), protegidas via DPAPI quando `OperatingSystem.IsWindows()`.
- `AllowedHosts` deve listar hosts explícitos (não pode conter `*`).

Essas validações rodam **antes** de `builder.Build()` — o comentário em `Program.cs` linha 195-197
documenta por que: testes de integração baseados em `WebApplicationFactory` injetam overrides de
configuração que só existem depois do `Build()`, então uma leitura antecipada de outras opções
(como `LoginRateLimiterOptions`, lida logo após o `Build()`) veria valores de produção não
sobrescritos se fosse feita cedo demais.

Se `ProductionHosting.ForwardedHeaders.Enabled` for verdadeiro, `X-Forwarded-For/Proto/Host` são
configurados com `RequireHeaderSymmetry = true` e listas explícitas de proxies/redes confiáveis
(`KnownProxies`/`KnownNetworks`, validadas como IP/CIDR válidos ou a aplicação recusa iniciar).

## 4. Logging

`builder.Logging.ClearProviders()` remove os providers padrão e adiciona apenas
`AddJsonConsole` (`IncludeScopes = true`, timestamp UTC ISO-8601). Não há provider de arquivo, sink
externo (Application Insights, Seq, etc.) ou provider de Debug configurado.

## 5. Autenticação por cookie

```csharp
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { /* ... */ });
```

- Nome do cookie: `BeeDay.Auth`; `HttpOnly = true`; `SameSite = Lax`; `Secure` = `SameAsRequest` em
  Development, `Always` fora dele.
- `SlidingExpiration = true`, `ExpireTimeSpan = 8 horas`; com `rememberMe = true` no login,
  `AuthenticationProperties.ExpiresUtc` é sobrescrito para 14 dias.
- `OnRedirectToLogin`: redireciona para `/login?expired=true&returnUrl=...` (path relativo
  escapado) em vez do comportamento padrão do middleware.
- `OnValidatePrincipal`: reexecuta em **toda** requisição autenticada — extrai
  `ClaimTypes.NameIdentifier` e a claim custom `BeeDayClaimTypes.SessionVersion`
  (`Services/Authentication/BeeDayClaimTypes.cs`, valor literal `"levelup:session_version"` —
  resíduo do rebrand, já reportado em `docs/architecture/README.md`); rejeita e faz `SignOutAsync`
  se a claim faltar/for inválida, se o `User` não existir, estiver inativo (`!user.IsActive`) ou se
  `user.SessionVersion` não bater com a claim do cookie. Isso é o que torna troca de senha, reset e
  desativação efetivos imediatamente, sem sessão persistida em banco — ver
  [`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md) §4 para o diagrama
  completo.

`AddCascadingAuthenticationState()` expõe o estado de autenticação para todo componente via
`<CascadingAuthenticationState>` em `Routes.razor`.

## 6. Registros de DI específicos da Web

Além de `AddBeeDayApplication()`/`AddBeeDayInfrastructure(configuration)` (que registram as outras
duas camadas), `Program.cs` registra localmente, todos como `Scoped` (ciclo de vida = 1 circuito
Blazor):

| Serviço | Papel |
|---|---|
| `ICurrentUserContext` → `HttpCurrentUserContext` | Única implementação Web de uma interface de `Application` — lê `ClaimTypes.NameIdentifier` do `HttpContext` atual |
| `BeeDayWebService` | Fachada MediatR — ver [`docs/web/README.md`](README.md) "Integração com Application" |
| `ToastService` | Fila de notificações in-memory, consumida por `BeeDayToastHost.razor` |
| `AuthenticatedUserInitializer` | Garante que o `User` do cookie ainda existe antes de qualquer página autenticada renderizar dados |
| `DashboardState` | Estado agregado da página `/daily` — ver `04-feature-components.md` |
| `BeeDayFeedbackStore` + `INotificationHandler<DomainEventNotification>` → `BeeDayFeedbackEventHandler` | Escuta `UserLeveledUpDomainEvent` via pipeline MediatR e alimenta o feedback visual de level-up |
| `ProfileCreationState` | Estado do fluxo de criação de conta/perfil (`/profile/create`) |
| `CardActionMenuCoordinator` | Coordena `BeeDayCardMenu` para que abrir um menu feche qualquer outro já aberto no mesmo circuito |

`BeeDayFeedbackEventHandler` é o único ponto em que `BeeDay.Web` participa do pipeline de
notificações do MediatR (não apenas `ISender.Send`) — qualquer `UserLeveledUpDomainEvent`
publicado por um handler de Application (Habits, Tasks, Todos, Projects) é entregue a este handler
no mesmo circuito que originou a requisição, sem endpoint HTTP ou polling envolvido.

## 7. Pipeline HTTP (ordem real, `app.Use*`)

```text
UseForwardedHeaders (se produção + habilitado)
→ CorrelationIdMiddleware
→ UseExceptionHandler (→ GlobalExceptionHandler)
→ UseHsts + UseHttpsRedirection (se não-Development)
→ UseAuthentication
→ UseAuthorization
→ UseAntiforgery
→ MapStaticAssets
```

- **`CorrelationIdMiddleware`** (`Diagnostics/CorrelationIdMiddleware.cs`): lê/gera
  `X-Correlation-ID` (aceita o header do cliente apenas se alfanumérico + `-_.`, máx. 128
  caracteres; caso contrário gera um `Guid` novo), grava no `HttpContext.TraceIdentifier`, ecoa no
  header de resposta e abre um `logger.BeginScope` com `CorrelationId`/`RequestId` — todo log
  emitido durante a requisição carrega esse escopo.
- **`GlobalExceptionHandler`** (`Diagnostics/GlobalExceptionHandler.cs`, implementa
  `IExceptionHandler`): mapeia exceção → `ProblemDetails`. Tabela de mapeamento:

  | Exceção | Status | Observação |
  |---|---|---|
  | `ApplicationValidationException` | 400 | `ValidationProblemDetails` com os erros por campo |
  | `DomainValidationException` | 400 | Inclui `field` como extension |
  | `InvalidDomainStateException` | 409 | — |
  | `AntiforgeryValidationException` / `BadHttpRequestException` | 400 | Mensagem técnica só em `IsDevelopment()` |
  | `PersistenceException` | 503 | Mensagem genérica sempre |
  | `OperationCanceledException` (request abortada) | 499 | — |
  | qualquer outra | 500 | Mensagem técnica só em `IsDevelopment()` |

  Toda resposta ganha `correlationId`/`requestId` (= `TraceIdentifier`) como extension; erros ≥500
  são logados como `LogError` (com `WebEventIds.RequestFailed`, `EventId` 6100), os demais como
  `LogWarning`. Ver `docs/testing/01-testing-strategy.md` §6 "Limitações conhecidas" para os
  status desta tabela que não são hoje alcançáveis por uma requisição HTTP real (a superfície HTTP
  desta aplicação é só `/auth/login`, `/auth/logout`, `/health*` e páginas Blazor).
- `UseHsts`/`UseHttpsRedirection` só fora de Development.
- `UseAntiforgery` protege os formulários HTML puros (`/auth/login`, `/auth/logout`, e o
  `<form method="post">` usado por `AccountSidePanel` para logout — o único componente de Layout
  que renderiza esse form; `DesktopSidebar`/`MobileHeader`/`MobileSidebar` só disparam o botão que
  abre esse painel) — os componentes Blazor interativos usam `EditForm`/`AntiforgeryToken` própria
  do framework.
- `MapStaticAssets()` (API nativa do .NET 10) serve `wwwroot/` com o mapeamento usado por
  `@Assets["..."]` em `App.razor`.

## 8. Endpoints minimal API

Só dois endpoints HTTP fora de Blazor e health checks, ambos em `Program.cs`:

### `POST /auth/login`

- Parâmetros de formulário: `email`, `password`, `returnUrl?`, `rememberMe?`.
- Protegido por um `AddEndpointFilter` que aplica `LoginRateLimiterFactory.Create(...)` — ver §9.
- Sucesso: monta claims (`NameIdentifier`, `Name`, `Email`, `SessionVersion`), `SignInAsync`, loga
  `Authentication.LoginSucceeded`, resolve destino via `LoginDestinationResolver.Resolve(...)` e
  redireciona (`Results.LocalRedirect`).
- Falha (`InvalidDomainStateException` — credenciais inválidas): loga
  `Authentication.LoginFailed Reason=InvalidCredentials` e redireciona para
  `/login?error=invalid`, **mesma resposta genérica** para "conta não existe" e "senha errada" (não
  vaza qual delas).

### `POST /auth/logout`

- `[RequireAuthorization()]`; `SignOutAsync` + loga `Authentication.LogoutSucceeded`; redireciona via
  `LoginDestinationResolver.ResolveLogout(returnUrl)`.

### `LoginDestinationResolver` (`Services/Authentication/LoginDestinationResolver.cs`)

```csharp
if (!hasProfile) return "/profile/create";
if (!hasCompletedOnboarding) return "/onboarding/tutorial";
return IsLocalPath(returnUrl) ? returnUrl! : "/daily";
```

`IsLocalPath` exige que o valor comece com `/` e não com `//` nem `/\` — mitiga open redirect via
`returnUrl` forjado (`//evil.com` ou `/\evil.com` são tratados por browsers como URL absoluta).

## 9. Rate limiting de login

`LoginRateLimiterFactory.Create` (`Services/Authentication/LoginRateLimiterFactory.cs`) encadeia
dois `PartitionedRateLimiter<HttpContext>` sliding-window independentes via
`PartitionedRateLimiter.CreateChained`:

- Por IP remoto (`IpPermitLimit`, padrão 10).
- Por e-mail normalizado (`ToUpperInvariant`, extraído do form; `EmailPermitLimit`, padrão 5).

Ambos com a mesma `Window` (padrão 1 minuto) e `SegmentsPerWindow` (padrão 4), `QueueLimit = 0`
(rejeita imediatamente, nunca enfileira). Configurável via `LoginRateLimiterOptions`
(`BeeDay:RateLimiting:Login`), o que permite aos testes de integração usar janelas curtas sem
depender de tempo real (ver [`06-testing.md`](06-testing.md)). Ao esgotar o limite, o filtro
responde **texto simples** (`Results.Text`, 429), não `application/problem+json` — inconsistência
de contrato já documentada em `docs/testing/01-testing-strategy.md` §6.

## 10. Health checks

Três endpoints mapeados (`HealthCheckResponseWriter.WriteAsync` formata todos como JSON:
`status`, `durationMs`, `correlationId`, `checks[]`):

| Rota | `Predicate` | Propósito |
|---|---|---|
| `/health/live` | `_ => false` | Nenhum check roda — só confirma que o processo responde |
| `/health/ready` | `tag == "ready"` | Checks marcados prontos para receber tráfego |
| `/health` | `_ => true` | Todos os checks; `Degraded`→200, `Unhealthy`→503 |

Único check registrado no repositório: `SqlServerHealthCheck` (`BeeDay.Infrastructure`, tags
`ready`/`storage`/`sql`) — ver
[`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md) §5 para o diagrama.

## 11. Fontes de verdade

- `src/BeeDay.Web/Program.cs`
- `src/BeeDay.Web/Diagnostics/CorrelationIdMiddleware.cs`, `GlobalExceptionHandler.cs`, `WebEventIds.cs`
- `src/BeeDay.Web/HealthChecks/HealthCheckResponseWriter.cs`
- `src/BeeDay.Web/Configuration/ProductionHostingOptions.cs`
- `src/BeeDay.Web/Services/Authentication/LoginDestinationResolver.cs`, `LoginRateLimiterFactory.cs`,
  `LoginRateLimiterOptions.cs`, `BeeDayClaimTypes.cs`
