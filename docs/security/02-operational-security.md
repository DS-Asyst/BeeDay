# Operational Security

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Program.cs`,
`src/BeeDay.Web/Services/Authentication/*.cs`, `src/BeeDay.Infrastructure/Security/Pbkdf2PasswordService.cs`,
`src/BeeDay.Web/appsettings.Production.json`, `.github/workflows/deploy-prd.yml`,
`scripts/Deploy-BeeDay.ps1`. Complementa [`01-security-baseline.md`](01-security-baseline.md) com o
recorte especificamente operacional: onde cada mecanismo de segurança é configurado/implantado, não
apenas que ele existe.

**Última verificação:** 2026-08-21 (Sprint 31.10, EPIC 31) — verificação anterior 2026-08-07 não
capturou `SecurityHeadersMiddleware`, adicionado na Sprint 30.22 (§5 corrigido).

## 1. Objetivo

Reunir, num único documento, os 10 mecanismos de segurança que têm uma dimensão **operacional**
(configuração de deploy, secret, variável de ambiente) além da lógica de aplicação já coberta em
`01-security-baseline.md` e [`docs/web/01-composition-root.md`](../web/01-composition-root.md).

## 2. Cookies

Cookie único: `BeeDay.Auth` (`Program.cs`, `AddCookie`). `HttpOnly=true` sempre;
`SecurePolicy = SameAsRequest` em Development, `Always` fora dele — **determinado pelo
`ASPNETCORE_ENVIRONMENT` no momento do boot**, não por uma opção de configuração separada; o script
de deploy fixa essa variável em `"Production"` diretamente no Application Pool (ver
[`02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) §4.3), então um deploy
via `Deploy-BeeDay.ps1` sempre resulta em `Secure=Always`. `SameSite=Lax`. `ExpireTimeSpan=8h`,
estendido a 14 dias só quando `rememberMe=true` no login.

## 3. Secrets

| Secret | Onde vive | Quem o injeta |
|---|---|---|
| Connection string SQL Server | `BeeDay:Persistence:SqlServer:ConnectionString` | Vazio em `appsettings.Production.json` — deve vir de variável de ambiente/User Secret não versionado; **não está entre os 5 secrets que `deploy-prd.yml` injeta** (ver achado §9) |
| Resend API key | GitHub Secret `BEEDAY_RESEND_API_KEY` | `deploy-prd.yml` → `Deploy-BeeDay.ps1` → variável de ambiente do App Pool |
| Chaves de Data Protection | Arquivos em disco (`DataProtectionKeysDirectory`), protegidas por DPAPI (`ProtectKeysWithDpapi(protectToLocalMachine: true)`, só em Windows) | Geradas automaticamente no primeiro boot, nunca em um secret externo |
| Hash de senha | Coluna no banco (`User.PasswordHash`) | PBKDF2, nunca reversível — ver §7 |

Nenhum segredo é logado — confirmado por `Program.cs`'s handlers de log de autenticação (só
`UserId`/`TraceId`/booleanos), e `01-security-baseline.md` §2 confirma que hash/senha nunca são
registrados.

**E-mail (PII) em log:** `ResendEmailSender`/`HmgRecipientGuardedEmailSender` nunca logam o
endereço do destinatário, em nenhuma forma. `DevelopmentEmailSender` (só ativo fora de produção) é
a única exceção — sua linha de log usa `EmailAddressLogMasking.Mask`
(`src/BeeDay.Infrastructure/Identity/EmailAddressLogMasking.cs`), que preserva só os 2 primeiros
caracteres da parte local do endereço (`ab***@domain`), suficiente para diferenciar execuções de
teste locais sem imprimir o e-mail completo. Não é um framework geral de mascaramento de PII —
existe apenas para essa linha específica.

## 4. Rate Limiting

Detalhado por completo em [`docs/web/01-composition-root.md`](../web/01-composition-root.md) §9 —
resumo operacional: 2 limitadores sliding-window encadeados (IP: 10/min: e-mail normalizado: 5/min),
configuráveis via `BeeDay:RateLimiting:Login` (única `Options` sem `.Validate()` — ver
[`02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) §4.2), sem override por
variável de ambiente de deploy — os valores de produção são sempre os padrões hardcoded em
`LoginRateLimiterOptions` (`IpPermitLimit=10`, `EmailPermitLimit=5`, `Window=1min`), já que
`Deploy-BeeDay.ps1` nunca define `BeeDay__RateLimiting__Login__*`.

## 5. Headers de segurança

Conforme `01-security-baseline.md` §4 (testado via `SecurityHeadersIntegrationTests`): o framework
Blazor Server já envia `Content-Security-Policy: frame-ancestors 'self'` e
`X-Frame-Options: SAMEORIGIN` automaticamente, independente de configuração desta aplicação.

`SecurityHeadersMiddleware` (`src/BeeDay.Web/Diagnostics/SecurityHeadersMiddleware.cs`, adicionado
na Sprint 30.22, registrado logo após `CorrelationIdMiddleware` em `Program.cs`) define os três
headers que faltavam: `X-Content-Type-Options: nosniff`, `Referrer-Policy:
strict-origin-when-cross-origin`, `Permissions-Policy: camera=(), microphone=(), geolocation=()`.
Deliberadamente não define `X-Frame-Options`/CSP — o framework já os envia (parágrafo acima); uma
CSP completa (`script-src` etc.) exige uma Sprint dedicada, não uma adição pontual de header (ver
Audit Ledger da Sprint 30.22).

`UseHsts()` roda fora de Development, mas — conforme já documentado em
[`docs/testing/`](../testing/README.md) — não é verificável via `WebApplicationFactory`/TestServer
por essa nunca realizar handshake TLS real; em produção real (atrás de IIS com HTTPS), o header é
enviado.

## 6. Data Protection

`Program.cs`: fora de Development, `AddDataProtection().SetApplicationName("BeeDay")
.PersistKeysToFileSystem(...)`, mais `ProtectKeysWithDpapi(protectToLocalMachine: true)` quando
`OperatingSystem.IsWindows()`. `DataProtectionKeysDirectory` deve ser um caminho absoluto (guarda de
startup, ver [`02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) §4.1). Em
HMG, o caminho real (`appsettings.Homologation.json`, `C:\Apps\BeeDay-Data\DataProtection-Keys`) já
batia com o que `Deploy-BeeDay.ps1` prepara e protege — confirmado ativo com chaves existentes por
verificação direta do servidor (Sprint 18.4), sem divergência. `appsettings.Production.json` também
apontava para `C:\Apps\LevelUp-Data\DataProtection-Keys` (divergente), mas esse arquivo não
corresponde a nenhum ambiente provisionado hoje (PRD não existe — ver
[`docs/deployment/02-runtime-configuration.md`](../deployment/02-runtime-configuration.md) §5.1);
corrigido por consistência de nomenclatura na Sprint 18.4, sem migração de chaves necessária (nunca
esteve em uso).

## 7. Identity / Password Hashing

`Pbkdf2PasswordService` (`src/BeeDay.Infrastructure/Security/`): PBKDF2-SHA256, **120.000
iterações**, salt de 16 bytes (`RandomNumberGenerator`, criptograficamente seguro), hash de 32
bytes, formato serializado `PBKDF2-SHA256${iterações}${salt-base64}${hash-base64}` — o número de
iterações fica embutido no próprio hash armazenado, não é um valor global fixo assumido na
verificação. `Verify` usa `CryptographicOperations.FixedTimeEquals` (comparação em tempo constante,
resistente a timing attack) e rejeita (retorna `false`, nunca lança) hash malformado, iterações fora
de `(0, 1_000_000]`, tamanho de salt/hash incorreto, ou exceção de decodificação Base64 —
superfície de entrada totalmente contida. `NeedsRehash` compara as iterações do hash armazenado às
120.000 atuais; `AuthenticateUserCommandHandler` faz rehash silencioso no login bem-sucedido quando
`NeedsRehash` é verdadeiro (ver `01-security-baseline.md` §2) — o mecanismo já suporta aumentar o
custo de iteração numa Sprint futura simplesmente mudando a constante `Iterations`, sem migração de
dados: o próximo login de cada usuário faz o rehash.

## 8. CSRF

Antiforgery é validado automaticamente pelo ASP.NET Core em `/auth/login`/`/auth/logout` (parâmetros
via `[FromForm]`) e pelos `EditForm`/`<AntiforgeryToken />` do Blazor. Nenhuma configuração
customizada de antiforgery (`AddAntiforgery(...)`) foi encontrada — usa os padrões do framework.
`GlobalExceptionHandler` trata `AntiforgeryValidationException`/`BadHttpRequestException`
explicitamente como 400 (ver [`docs/web/01-composition-root.md`](../web/01-composition-root.md)
§7) — um bug real encontrado e corrigido em Sprint anterior (`01-security-baseline.md` §1).

## 9. CORS

**Não configurado.** Busca por `AddCors`/`UseCors` em `src/BeeDay.Web/` e
`src/BeeDay.Infrastructure/` não encontrou nenhuma ocorrência. Esperado para uma aplicação Blazor
Server: não há uma API pública consumida por um frontend de outra origem — toda comunicação
cliente-servidor é o próprio circuito SignalR do Blazor (mesma origem) mais os 2 endpoints minimal
API (`/auth/login`, `/auth/logout`), também mesma origem.

## 10. Health Endpoints — exposição

`/health/live`, `/health/ready`, `/health` **não têm `.RequireAuthorization()`** — são acessíveis
sem autenticação, por design (um health check que exige login não pode ser consultado por um load
balancer/monitor externo antes de uma sessão existir). O corpo da resposta
(`HealthCheckResponseWriter`, ver [`docs/deployment/03-observability.md`](../deployment/03-observability.md) §3)
expõe `status`, `durationMs`, `correlationId` e, por check, `name`/`status`/`description`/
`durationMs`/`data` — não foi encontrado nenhum dado sensível (connection string, stack trace) no
formato de saída; `SqlServerHealthCheck` (único check registrado) reporta apenas conectividade.
`Deploy-BeeDay.ps1` consulta `/health/ready` com um header `Host: beeday` explícito — implica que o
IIS pode estar configurado com múltiplos bindings de host, e o script precisa forçar qual site
responder.

## 11. Achado

- `01-security-baseline.md` referenciava `LevelUpClaimTypes`, `src/LevelUp.Web/...`,
  `tests/LevelUp.Web.Tests/...` — nomenclatura pré-rebrand (`523728d`, `b1e9f53`, `6ae465b`).
  Corrigido na Sprint 16.10 (auditoria de nomenclatura, EPIC 16 consolidação final); o valor da
  claim continua literal `"levelup:session_version"` no código atual — mantido no texto por ser
  fato de código, não referência desatualizada (já reportado em
  `docs/architecture/README.md`).

## 12. Fontes consultadas

- `src/BeeDay.Web/Program.cs`, `Services/Authentication/BeeDayClaimTypes.cs`,
  `LoginRateLimiterFactory.cs`, `LoginRateLimiterOptions.cs`.
- `src/BeeDay.Infrastructure/Security/Pbkdf2PasswordService.cs`.
- `src/BeeDay.Web/appsettings.Production.json`, `web.config`.
- `.github/workflows/deploy-prd.yml`, `scripts/Deploy-BeeDay.ps1`.
- Busca por `AddCors`/`UseCors` em `src/BeeDay.Web/`, `src/BeeDay.Infrastructure/` (zero resultados).
- [`01-security-baseline.md`](01-security-baseline.md), [`docs/web/01-composition-root.md`](../web/01-composition-root.md),
  [`docs/testing/`](../testing/README.md) (reaproveitados, não duplicados).
