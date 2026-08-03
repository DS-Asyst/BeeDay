# Security Architecture

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Program.cs`,
`src/BeeDay.Web/Services/Authentication/`, `src/BeeDay.Infrastructure/Security/`,
`src/BeeDay.Infrastructure/Identity/`, `src/BeeDay.Application/Common/Security/`, e os testes de
integração correspondentes em `tests/BeeDay.Web.Tests/Integration/`.

## 1. Autenticação por cookie

Configurada em `src/BeeDay.Web/Program.cs:124-171` via
`AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(...)`:

| Opção | Valor |
|---|---|
| `Cookie.Name` | `"BeeDay.Auth"` |
| `Cookie.HttpOnly` | `true` |
| `Cookie.SameSite` | `Lax` |
| `Cookie.SecurePolicy` | `SameAsRequest` em Development, `Always` em produção |
| `SlidingExpiration` | `true` |
| `ExpireTimeSpan` | 8 horas (sessão comum); 14 dias se "lembrar de mim" (`AuthenticationProperties.ExpiresUtc`) |
| `LoginPath` / `AccessDeniedPath` | `/login` |

`OnRedirectToLogin` customizado redireciona para `/login?expired=true&returnUrl=<path atual>` em
vez do comportamento padrão do framework.

## 2. Validação de sessão a cada requisição (`OnValidatePrincipal`)

`Program.cs:144-170`, executado em toda requisição autenticada:

1. Extrai `ClaimTypes.NameIdentifier` — se ausente ou não for GUID, `RejectPrincipal()` + `SignOutAsync`.
2. Extrai a claim `BeeDayClaimTypes.SessionVersion` — se ausente ou não-inteira, rejeita.
3. Resolve `IUserRepository` via `context.HttpContext.RequestServices` e carrega o usuário pelo id.
4. Se o usuário não existir, estiver inativo, ou `user.SessionVersion != sessionVersion` da claim,
   rejeita e força sign-out.

**Observação verificada:** a constante de claim
(`src/BeeDay.Web/Services/Authentication/BeeDayClaimTypes.cs`) tem valor literal
`"levelup:session_version"` — o nome do tipo já é `BeeDayClaimTypes`, mas a string de valor
retém o prefixo histórico `levelup:`. É um artefato de nomenclatura sem efeito funcional (o valor
é opaco, comparado por igualdade de string entre emissão e verificação, nunca exposto ao usuário) —
reportado aqui como achado, não corrigido nesta Sprint (fora de escopo: "não alterar código").

## 3. Invalidação de sessão (`SessionVersion`)

`User.SessionVersion` (`src/BeeDay.Domain/Entities/User.cs:33`, inicia em `1`).
`User.InvalidateSessions()` (linha 131) incrementa o valor; chamado explicitamente em três pontos:

| Evento | Handler |
|---|---|
| Troca de senha pelo próprio usuário | `ChangeCurrentUserPasswordCommandHandler` (`Features/Users/Handlers/UserHandlers.cs`) |
| Reset de senha via token | `ResetPasswordCommandHandler` (`Features/Identity/Handlers/IdentityHandlers.cs`) |
| Desativação de conta | `User.SetActive(false)` chama `InvalidateSessions()` internamente |

Qualquer cookie emitido antes dessas ações passa a falhar na próxima checagem de
`OnValidatePrincipal` (§2), sem precisar de uma lista de revogação de tokens.

## 4. Rate limiting de login

`src/BeeDay.Web/Services/Authentication/LoginRateLimiterFactory.cs`: dois
`PartitionedRateLimiter<HttpContext>` encadeados via `PartitionedRateLimiter.CreateChained(...)`:

| Limiter | Chave de partição | Limite padrão | Janela |
|---|---|---|---|
| IP | `Connection.RemoteIpAddress` | 10 tentativas | 1 min (4 segmentos) |
| E-mail | `Request.Form["email"]` normalizado (trim + upper) | 5 tentativas | 1 min (4 segmentos) |

Config: seção `BeeDay:RateLimiting:Login` (`LoginRateLimiterOptions.cs`). Aplicado ao endpoint
`POST /auth/login` via `.AddEndpointFilter(...)` (`Program.cs:302-316`); resposta ao exceder:
HTTP 429, texto genérico `"Too many attempts. Please wait and try again."` — idêntica
independentemente de o e-mail pertencer a uma conta real (verificado por teste, ver §8).

## 5. `ICurrentUserContext` e `CurrentUserGuard`

`ICurrentUserContext` (`src/BeeDay.Application/Common/Security/ICurrentUserContext.cs`) expõe só
`Guid? UserId`. Implementado por `HttpCurrentUserContext`
(`src/BeeDay.Web/Services/HttpCurrentUserContext.cs`), que lê `ClaimTypes.NameIdentifier` do
`HttpContext` atual via `IHttpContextAccessor`.

`CurrentUserGuard.RequireUserId(currentUser)`
(`src/BeeDay.Application/Common/Security/CurrentUserGuard.cs`) lança `InvalidDomainStateException`
se `UserId` for nulo — não faz nenhuma checagem de existência/ownership além disso (documentado
explicitamente no XML doc do método); cada Handler é responsável por validar existência/ownership
via sua própria chamada de repositório.

## 6. Hash de senha

`Pbkdf2PasswordService` (`src/BeeDay.Infrastructure/Security/Pbkdf2PasswordService.cs`):
PBKDF2-SHA256, 120.000 iterações, salt de 16 bytes, hash de 32 bytes, formato armazenado
`"{algoritmo}${iterações}${salt-base64}${hash-base64}"`. Comparação em `Verify` usa
`CryptographicOperations.FixedTimeEquals` (tempo constante, evita timing attack).
`NeedsRehash` retorna `true` se o algoritmo ou a contagem de iterações armazenados forem mais
fracos que os atuais — acionado transparentemente em
`AuthenticateUserCommandHandler` (`Features/Authentication/Handlers/AuthenticationHandlers.cs:36-41`)
sem invalidar a sessão que está sendo criada.

## 7. Confirmação de e-mail / reset de senha

Tokens gerados por `SecureUserTokenService` (`src/BeeDay.Infrastructure/Identity/`): 32 bytes
aleatórios (`RandomNumberGenerator`), codificados base64url; apenas o **hash SHA-256** do token é
persistido (`UserToken.TokenHash`) — o token bruto só existe no e-mail enviado, nunca no banco.

| Fluxo | Expiração | Handler |
|---|---|---|
| Confirmação de e-mail | 24 horas | `ConfirmEmailCommandHandler` |
| Reset de senha | 1 hora | `ResetPasswordCommandHandler` |

Ambos os fluxos de *solicitação* (`ResendEmailConfirmationCommandHandler`,
`RequestPasswordResetCommandHandler`) são limitados por `IIdentityRequestThrottle`
(`MemoryIdentityRequestThrottle`, `ConcurrentDictionary` em processo, sem persistência entre
reinícios) com cooldown de 60 segundos por operação+e-mail, e **nunca revelam se a conta existe**
— retornam silenciosamente em caso de conta inexistente/inativa/já confirmada.

## 8. CSRF / Antiforgery

Nenhuma configuração customizada de `AddAntiforgery(...)` existe no repositório — usa-se o serviço
padrão de antiforgery do Blazor Server (registrado implicitamente por `AddRazorComponents()`).
`app.UseAntiforgery()` está no pipeline (`Program.cs:219`), após autenticação/autorização.

Como esta é uma aplicação Blazor Server (não MVC), não existe atributo
`[ValidateAntiForgeryToken]` em lugar nenhum — a proteção vem de:

- Formulários HTML tradicionais (`<form method="post">`) contendo `<AntiforgeryToken />`
  (ex. `Login.razor:42-43`), que emite o campo oculto `__RequestVerificationToken`.
- Os dois únicos endpoints minimal API que recebem POST fora do circuito SignalR
  (`/auth/login`, `/auth/logout`) são protegidos pelo middleware `UseAntiforgery()`, não por
  atributo.
- Toda a UI interativa restante roda sobre o circuito SignalR do Blazor Server, protegido por seu
  próprio mecanismo de conexão — não por token antiforgery.

Verificado por `tests/BeeDay.Web.Tests/Integration/AntiforgeryIntegrationTests.cs`: token ausente
→ 400; token malformado → 400; token de outro contexto de cookie → 400; `returnUrl` externo
ignorado (proteção contra open redirect).

## 9. Autorização

`AddAuthorization()` (`Program.cs:172`) sem nenhuma policy nomeada — todo `[Authorize]` usa a
policy padrão (apenas exige autenticação). Enforcement de rota via `Routes.razor`:
`<CascadingAuthenticationState>` + `<AuthorizeRouteView>` + `<NotAuthorized>` (redireciona para
`/login?returnUrl=...`).

Páginas com `[Authorize]`: `/account`, `/settings`, `/design-system/icons`, `/design-system/hero`,
`/daily`, `/wallet`, `/onboarding/tutorial`. Páginas com `[AllowAnonymous]`: login, todas as
páginas de Identity (confirmação de e-mail, reset de senha, etc.), criação de perfil, página raiz,
`/welcome`, páginas de erro/not-found.

O endpoint `/auth/logout` usa `.RequireAuthorization()` diretamente (não é uma página Razor).

## 10. Cobertura de teste relevante

- `tests/BeeDay.Web.Tests/Services/Authentication/LoginRateLimiterFactoryTests.cs` — testes de
  unidade contra os limiters isolados.
- `tests/BeeDay.Web.Tests/Integration/RateLimitingIntegrationTests.cs` — HTTP real, confirma que
  a resposta 429 é idêntica para conta existente/inexistente.
- `tests/BeeDay.Web.Tests/Integration/SessionInvalidationIntegrationTests.cs` — cobre cookie
  forjado, claim ausente, usuário inexistente, desativação, troca/reset de senha.
- `tests/BeeDay.Web.Tests/Integration/AntiforgeryIntegrationTests.cs` — cobre os cenários do §8.
- `tests/BeeDay.Infrastructure.Tests/Pbkdf2PasswordServiceTests.cs` — cobre hashing/verificação/rehash.
