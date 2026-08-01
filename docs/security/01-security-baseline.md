# Baseline de Segurança

## 1. Fase imediata — status: implementado (Sprint 12.5)

### Rate limiting de login — implementado

`/auth/login` é protegido por `LevelUpClaimTypes`/`LoginRateLimiterFactory`
(`src/LevelUp.Web/Services/Authentication/LoginRateLimiterFactory.cs`), aplicado ao endpoint via
`AddEndpointFilter` em `Program.cs`:

- limite por IP: sliding window, 10 tentativas/minuto;
- limite por e-mail normalizado: sliding window, 5 tentativas/minuto;
- os dois limitadores são encadeados (`PartitionedRateLimiter.CreateChained`) — qualquer um dos
  dois esgotado bloqueia a tentativa;
- resposta HTTP 429 com corpo genérico ("Too many attempts...") idêntico independentemente de o
  e-mail pertencer a uma conta real;
- o log de rejeição registra apenas `TraceId`, nunca e-mail ou senha.

Cobertura de testes: `tests/LevelUp.Web.Tests/Services/Authentication/LoginRateLimiterFactoryTests.cs`.

### Invalidação de sessão — implementado

`User.SessionVersion` (`src/LevelUp.Domain/Entities/User.cs`), iniciado em `1`.

No login (`Program.cs`, `/auth/login`):

- emitida a claim `levelup:session_version` (`LevelUpClaimTypes.SessionVersion`) com o valor atual.

Na validação do cookie (`OnValidatePrincipal`):

- a claim é comparada ao `SessionVersion` atual do usuário; divergência invalida a sessão.

Incrementado via `User.InvalidateSessions()` quando:

- senha muda (`ChangeCurrentUserPasswordCommandHandler`);
- reset de senha ocorre (`ResetPasswordCommandHandler`);
- conta é desativada (`User.SetActive(false)`).

Logout global por solicitação do usuário e revogação por incidente de segurança **não** têm
acionador de UI nesta Sprint (nenhuma funcionalidade nova foi introduzida) — o mecanismo já
suporta ambos: qualquer código futuro só precisa chamar `user.InvalidateSessions()`.

Rehash-on-login (ver Password policy abaixo) **não** invalida sessões — não é uma mudança de
senha do ponto de vista de segurança, apenas uma atualização silenciosa do formato do hash.

### Remover fallback de usuário — implementado

`CurrentUserGuard.RequireUserId` não usa mais `LevelUpData.CurrentUserId` como fallback.
`ICurrentUserContext` é agora um parâmetro obrigatório (não mais `ICurrentUserContext? = null`)
em todos os handlers de Application que resolvem o usuário autenticado. `LevelUpData.CurrentUserId`
continua existindo apenas como um campo de bootstrapping do documento JSON (usado por
`AddUser`/`EnsureValidState`/migração legada) — não é mais lido por nenhum caminho de autenticação.

### Antiforgery & Security Integration Tests — implementado (Sprint 12.6)

`/auth/login` e `/auth/logout` vinculam parâmetros via `[FromForm]`, o que exige validação
antiforgery automaticamente nos Minimal APIs do ASP.NET Core; os formulários Razor correspondentes
(`Login.razor`, `AccountSidePanel.razor`) renderizam `<AntiforgeryToken />`.

Testes de integração reais (`WebApplicationFactory<Program>`,
`tests/LevelUp.Web.Tests/Integration/`) cobrem, contra uma instância real da aplicação com
armazenamento JSON isolado por execução de teste: antiforgery/CSRF, login, rate limiting, cookies,
logout, invalidação de sessão via `OnValidatePrincipal` real, autorização por rota, isolamento
multiusuário, reset de senha, confirmação de e-mail, Problem Details e security headers. Ver
`docs/testing/01-testing-strategy.md` seção 6 para a lista completa, comandos de execução e
limitações conhecidas.

**Bugs reais encontrados e corrigidos por esses testes:**

- `GlobalExceptionHandler` não tinha um caso para `BadHttpRequestException`/
  `AntiforgeryValidationException` — qualquer requisição com token ausente, inválido, ou corpo
  malformado retornava **500 Internal Server Error** em vez de **400 Bad Request**. Corrigido em
  `src/LevelUp.Web/Diagnostics/GlobalExceptionHandler.cs`.
- `Program.cs` lia `LoginRateLimiterOptions` de `builder.Configuration` **antes** de `Build()`;
  overrides de configuração injetados por testes (`WebApplicationFactory.ConfigureAppConfiguration`)
  só são mesclados ao redor do `Build()`, então o limitador real usava os valores padrão de
  produção mesmo quando a configuração testável indicava outra coisa. Corrigido lendo
  `app.Configuration` após `Build()`.

**Re-verificação do fix de remoção de fallback (Sprint 12.5):** `CurrentUserGuardTests` (em
`tests/LevelUp.Application.Tests/`) prova, no cenário combinado exato — `LevelUpData.CurrentUserId`
apontando para um usuário real E `ICurrentUserContext.UserId` nulo ao mesmo tempo — que a operação é
rejeitada, nunca cai de volta para `CurrentUserId`.

## 2. Password policy — implementado

- PBKDF2 mantido (`Pbkdf2PasswordService`, 120.000 iterações);
- `IPasswordService.NeedsRehash` adicionado — compara iterações do hash armazenado às atuais;
- rehash automático no login bem-sucedido (`AuthenticateUserCommandHandler`), sem invalidar a
  sessão sendo criada nem sessões existentes (não é uma mudança de senha real);
- nenhuma senha ou hash é registrado em log;
- custo (iterações) permanece o mesmo desta Sprint — revisão periódica é um item operacional
  contínuo, não uma mudança de código.

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
  `docs/testing/01-testing-strategy.md` seção 6);
- cookies HttpOnly e Secure — testado via integração (`CookieIntegrationTests`);
- SameSite documentado — testado (`Lax`);
- AllowedHosts explícito — testado indiretamente (produção só aceita hosts configurados);
- forwarded headers apenas para proxies confiáveis;
- CSP — uma CSP completa (script-src etc.) ainda está planejada; o framework Razor Components já
  envia `Content-Security-Policy: frame-ancestors 'self'` e `X-Frame-Options: SAMEORIGIN`
  automaticamente em toda resposta renderizada, independentemente de qualquer configuração desta
  aplicação — confirmado via teste de integração (`SecurityHeadersIntegrationTests`).
  Referrer-Policy, X-Content-Type-Options e Permissions-Policy continuam ausentes;
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
