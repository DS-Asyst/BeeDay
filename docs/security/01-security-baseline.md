# Baseline de Segurança

**Fonte da verdade:** verificado em Sprints anteriores desta migração contra
`src/BeeDay.Infrastructure/Security` e `src/BeeDay.Web/Services` (declaração movida para dentro do
documento na Sprint 16.10 — antes vivia apenas em `docs/security/README.md`, inconsistente com o
padrão inline usado por todo o restante do corpus). Nomenclatura de caminho/tipo corrigida na
Sprint 16.10 (`LevelUp*` → `BeeDay*` em referências de código atual); mantidas as menções
históricas a `LevelUpData` (tipo removido no código, não renomeado) e ao valor literal da claim
`"levelup:session_version"` (ainda presente assim no código atual — ver
[`docs/architecture/README.md`](../architecture/README.md)).

## 1. Fase imediata — status: implementado (Sprint 12.5)

### Rate limiting de login — implementado

`/auth/login` é protegido por `BeeDayClaimTypes`/`LoginRateLimiterFactory`
(`src/BeeDay.Web/Services/Authentication/LoginRateLimiterFactory.cs`), aplicado ao endpoint via
`AddEndpointFilter` em `Program.cs`:

- limite por IP: sliding window, 10 tentativas/minuto;
- limite por e-mail normalizado: sliding window, 5 tentativas/minuto;
- os dois limitadores são encadeados (`PartitionedRateLimiter.CreateChained`) — qualquer um dos
  dois esgotado bloqueia a tentativa;
- resposta HTTP 429 com corpo genérico ("Too many attempts...") idêntico independentemente de o
  e-mail pertencer a uma conta real;
- o log de rejeição registra apenas `TraceId`, nunca e-mail ou senha.

Cobertura de testes: `tests/BeeDay.Web.Tests/Services/Authentication/LoginRateLimiterFactoryTests.cs`.

### Invalidação de sessão — implementado

`User.SessionVersion` (`src/BeeDay.Domain/Entities/User.cs`), iniciado em `1`.

No login (`Program.cs`, `/auth/login`):

- emitida a claim `levelup:session_version` (`BeeDayClaimTypes.SessionVersion`) com o valor atual —
  o valor literal da claim continua `"levelup:session_version"` mesmo após o rebrand (achado já
  documentado em [`docs/architecture/README.md`](../architecture/README.md), não uma referência
  desatualizada deste documento).

Na validação do cookie (`OnValidatePrincipal`):

- a claim é comparada ao `SessionVersion` atual do usuário; divergência invalida a sessão.

Incrementado via `User.InvalidateSessions()` quando:

- senha muda (`ChangeCurrentUserPasswordCommandHandler`);
- reset de senha ocorre (`ResetPasswordCommandHandler`).

`User.SetActive(false)` também dispara `InvalidateSessions()` no Domain, mas nenhum Command/Handler
de Application ou fluxo de UI chama `SetActive(false)` hoje — os únicos call sites do repositório
inteiro são fixtures de teste (`UserSessionHardeningTests`, `AuthenticationHandlersTests`,
`BeeDayWebApplicationFactory`). Desativação de conta não é, portanto, um gatilho de invalidação de
sessão alcançável em produção nesta Sprint — mecanismo de Domain pronto, sem caso de uso que o
acione.

Logout global por solicitação do usuário e revogação por incidente de segurança **não** têm
acionador de UI nesta Sprint (nenhuma funcionalidade nova foi introduzida) — o mecanismo já
suporta ambos: qualquer código futuro só precisa chamar `user.InvalidateSessions()`.

Rehash-on-login (ver Password policy abaixo) **não** invalida sessões — não é uma mudança de
senha do ponto de vista de segurança, apenas uma atualização silenciosa do formato do hash.

### Remover fallback de usuário — implementado

`CurrentUserGuard.RequireUserId` não usa mais `LevelUpData.CurrentUserId` como fallback.
`ICurrentUserContext` é agora um parâmetro obrigatório (não mais `ICurrentUserContext? = null`)
em todos os handlers de Application que resolvem o usuário autenticado. `LevelUpData` (e com ele,
`CurrentUserId`) foi removido do código na Sprint 14.7 — não existe mais nenhum campo persistido de
"usuário atual" em lugar nenhum, ambiente ou não.

### Antiforgery & Security Integration Tests — implementado (Sprint 12.6)

`/auth/login` e `/auth/logout` vinculam parâmetros via `[FromForm]`, o que exige validação
antiforgery automaticamente nos Minimal APIs do ASP.NET Core; os formulários Razor correspondentes
(`Login.razor`, `NavigationItems.razor` — o formulário de logout do shell autenticado, movido de
`AccountSidePanel.razor` quando esse componente foi removido no redesign da Sprint de 2026-08-13)
renderizam `<AntiforgeryToken />`.

Testes de integração reais (`WebApplicationFactory<Program>`,
`tests/BeeDay.Web.Tests/Integration/`) cobrem, contra uma instância real da aplicação com um banco
SQL Server LocalDB isolado por execução de teste (substituiu o armazenamento JSON isolado por
execução de teste no corte da Sprint 14.6 — ver [`docs/web/06-testing.md`](../web/06-testing.md)):
antiforgery/CSRF, login, rate limiting, cookies,
logout, invalidação de sessão via `OnValidatePrincipal` real, autorização por rota, isolamento
multiusuário, reset de senha, confirmação de e-mail, Problem Details e security headers. Ver
[`docs/testing/01-testing-strategy.md`](../testing/01-testing-strategy.md) §5 e
[`docs/web/06-testing.md`](../web/06-testing.md) para a lista completa, comandos de execução e
limitações conhecidas.

**Bugs reais encontrados e corrigidos por esses testes:**

- `GlobalExceptionHandler` não tinha um caso para `BadHttpRequestException`/
  `AntiforgeryValidationException` — qualquer requisição com token ausente, inválido, ou corpo
  malformado retornava **500 Internal Server Error** em vez de **400 Bad Request**. Corrigido em
  `src/BeeDay.Web/Diagnostics/GlobalExceptionHandler.cs`.
- `Program.cs` lia `LoginRateLimiterOptions` de `builder.Configuration` **antes** de `Build()`;
  overrides de configuração injetados por testes (`WebApplicationFactory.ConfigureAppConfiguration`)
  só são mesclados ao redor do `Build()`, então o limitador real usava os valores padrão de
  produção mesmo quando a configuração testável indicava outra coisa. Corrigido lendo
  `app.Configuration` após `Build()`.

**Re-verificação do fix de remoção de fallback (Sprint 12.5):** `CurrentUserGuardTests` (em
`tests/BeeDay.Application.Tests/`) prova que, mesmo com um usuário real existente no repositório e
`ICurrentUserContext.UserId` nulo, a operação é rejeitada — nunca há fallback para nenhum "usuário
atual" implícito. Até a Sprint 14.7, o cenário testado era literalmente `LevelUpData.CurrentUserId`
apontando para esse usuário; com `LevelUpData` removido, o teste (renomeado
`Handler_WithNullContextUserId_RejectsTheOperationEvenWhenAnotherUserExists`) prova a mesma garantia
sem precisar de um campo ambiente para apontar — a ausência desse campo em qualquer lugar do código é,
em si, parte da garantia.

### Isolamento de propriedade (ownership) — implementado

Nenhuma verificação de posse separada existe como camada própria — a fronteira é estrutural: toda
consulta de leitura em cada um dos 8 repositórios (`IHabitRepository`, `IProjectRepository`, etc.)
recebe o `userId` do usuário autenticado como parâmetro obrigatório e filtra por ele na query, então
um recurso de outro usuário nunca é retornado, nem por engano. Handlers que precisam confirmar posse
antes de mutar (ex. `ReorderActivitiesCommandHandler.EnsureOwned`,
`WalletLookup.RequireOwnedTagAsync`/`RequireOwnedTransactionAsync`) lançam
`InvalidDomainStateException` quando o lookup escopado por `userId` não encontra o recurso — ver
[`docs/application/04-contracts.md`](../application/04-contracts.md). `CurrentUserGuard` resolve
apenas a identidade do usuário; a checagem de posse em si é responsabilidade de cada repositório
Aggregate, nunca centralizada. Coberto por `MultiUserIsolationIntegrationTests`.

## 2. Password policy — implementado

- PBKDF2 mantido (`Pbkdf2PasswordService`, 120.000 iterações);
- `IPasswordService.NeedsRehash` adicionado — compara iterações do hash armazenado às atuais;
- rehash automático no login bem-sucedido (`AuthenticateUserCommandHandler`), sem invalidar a
  sessão sendo criada nem sessões existentes (não é uma mudança de senha real);
- nenhuma senha ou hash é registrado em log;
- custo (iterações) permanece o mesmo desta Sprint — revisão periódica é um item operacional
  contínuo, não uma mudança de código.

## 2.1 Tokens de e-mail/reset (confirmação de conta, redefinição de senha)

`SecureUserTokenService` (`src/BeeDay.Infrastructure/Identity/SecureUserTokenService.cs`,
implementa `IUserTokenService`) gera 32 bytes aleatórios (`RandomNumberGenerator.Fill`, CSPRNG),
codificados em Base64Url para o token enviado por e-mail, e armazena apenas o hash SHA-256
(hexadecimal maiúsculo) — nunca o token em claro. Usado por `ConfirmEmailCommandHandler` e
`ResetPasswordCommandHandler` (`src/BeeDay.Application/Features/Identity/Handlers/
IdentityHandlers.cs`). As invariantes de janela temporal e uso único (`expiresAtUtc >
createdAtUtc`, revogação idempotente, `EnsureCanBeUsed`) pertencem ao Domain — ver
[`docs/domain/user-token.md`](../domain/user-token.md), não duplicadas aqui.

## 3. Dados

- connection string em secrets;
- conta SQL com menor privilégio;
- criptografia em trânsito;
- backup criptografado;
- ACL restrita aos arquivos antigos JSON;
- política de retenção de auditoria e tokens.

## 4. HTTP

- HTTPS obrigatório;
- HSTS — configurado em `Program.cs` (`UseHsts()`, fora de Development); não pôde ser verificado via
  `WebApplicationFactory`/TestServer, que nunca realiza handshake TLS real (ver
  [`docs/testing/01-testing-strategy.md`](../testing/01-testing-strategy.md) §5);
- cookies HttpOnly e Secure — testado via integração (`CookieIntegrationTests`);
- SameSite documentado — testado (`Lax`);
- AllowedHosts explícito — testado indiretamente (produção só aceita hosts configurados);
- forwarded headers apenas para proxies confiáveis;
- CSP — uma CSP completa (script-src etc.) ainda está planejada; o framework Razor Components já
  envia `Content-Security-Policy: frame-ancestors 'self'` e `X-Frame-Options: SAMEORIGIN`
  automaticamente em toda resposta renderizada, independentemente de qualquer configuração desta
  aplicação — confirmado via teste de integração (`SecurityHeadersIntegrationTests`);
- Referrer-Policy (`strict-origin-when-cross-origin`), X-Content-Type-Options (`nosniff`) e
  Permissions-Policy (`camera=(), microphone=(), geolocation=()`) — implementados na Sprint 30.22
  da EPIC 30 (`SecurityHeadersMiddleware`, `src/BeeDay.Web/Diagnostics/SecurityHeadersMiddleware.cs`),
  registrado logo após `CorrelationIdMiddleware` em `Program.cs`, aplicado a toda resposta.
  Deliberadamente não define `X-Frame-Options` nem CSP — o framework Razor Components já os
  controla, e sobrescrevê-los aqui arriscaria um header conflitante ou silenciosamente substituído;
- headers de segurança testados — `SecurityHeadersIntegrationTests` cobre o estado real acima.

## 5. LGPD

Antes de produção pública:

- política de privacidade;
- base legal e finalidade;
- exportação de dados;
- exclusão de conta;
- retenção;
- canal para titular;
- inventário de dados;
- procedimento de incidente;
- minimização de logs e auditoria.
