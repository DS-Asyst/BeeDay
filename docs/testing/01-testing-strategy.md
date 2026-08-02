# Estratégia de Testes

## 1. Pirâmide

### Domain tests

Validam invariantes sem infraestrutura.

### Application tests

Validam handlers com portas simuladas ou fakes controlados.

### Contract tests

Validam que qualquer adapter cumpre a mesma semântica.

### Integration tests

Validam JSON, SQL Server, autenticação, antiforgery e e-mail fake.

### Component tests

Validam componentes Blazor com bUnit.

### E2E

Validam fluxos reais com Playwright.

## 2. Contract test kit

Criar classes abstratas reutilizáveis:

```csharp
public abstract class UserRepositoryContractTests
{
    protected abstract Task<IUserRepositoryFixture> CreateFixtureAsync();

    [Fact]
    public async Task Email_lookup_is_normalized() { }

    [Fact]
    public async Task Duplicate_email_is_rejected() { }

    [Fact]
    public async Task User_from_another_owner_is_not_exposed() { }
}
```

Implementações:

```text
JsonUserRepositoryContractTests
SqlUserRepositoryContractTests
```

Repetir para:

- atividades;
- projetos;
- Wallet;
- experiência;
- tokens;
- unit of work.

## 3. E2E mínimo

Escopo planejado inicialmente para uma futura Sprint de hardening de sessão/segurança (troca de
senha, invalidação de sessão anterior, isolamento entre dois usuários reais via browser). O
subconjunto de jornada de usuário abaixo foi implementado na Sprint 12.7 — ver seção 7:

1. criar conta;
2. autenticar (com onboarding);
3. criar hábito e concluir, validando atualização visual de XP;
4. criar tarefa e concluir;
5. criar tag e transação no Wallet, validando saldo atualizado;
6. editar perfil;
7. logout.

Ainda não implementado (fora do escopo da Sprint 12.7): confirmação de e-mail por fake sender via
browser, troca de senha, invalidação de sessão anterior, segundo usuário sem acesso aos dados do
primeiro — esses já são cobertos por `tests/LevelUp.Web.Tests/Integration/` (seção 6) a nível de
HTTP e não duplicados aqui.

## 4. Testes de banco

- migration inicial cria banco vazio;
- unicidade de e-mail;
- unicidade de nickname;
- ownership por FK e consulta;
- cascade behavior explícito;
- rowversion gera conflito;
- idempotência de XP;
- exclusão de tag em uso;
- transação atômica.

## 5. Quality gate

```powershell
dotnet restore LevelUp.slnx
dotnet format LevelUp.slnx --verify-no-changes --no-restore
dotnet build LevelUp.slnx -c Release --no-restore --warnaserror
dotnet test LevelUp.slnx -c Release --no-build
```

Apenas os testes E2E:

```powershell
dotnet test tests/LevelUp.E2E.Tests/LevelUp.E2E.Tests.csproj -c Release
```

## 6. Integration tests (Web) — implementado (Sprint 12.6)

`tests/LevelUp.Web.Tests/Integration/` contém testes de integração reais contra a aplicação
completa, via `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>` — TestServer real,
pipeline HTTP real, cookies reais, antiforgery real, `OnValidatePrincipal` real. Mocks só isolam
armazenamento (JSON em diretório temporário) e envio de e-mail (capturado em disco em vez de SMTP
real); o mecanismo sob teste nunca é substituído.

Executar apenas esses testes:

```powershell
dotnet test tests/LevelUp.Web.Tests/LevelUp.Web.Tests.csproj --configuration Release --filter "FullyQualifiedName~Integration"
```

### Infraestrutura

- `LevelUpWebApplicationFactory` — factory base; storage JSON isolado por instância (diretório
  temporário único, apagado no Dispose); ambiente configurável (Development por padrão);
  rate limiter de login com limites generosos por padrão para não interferir em testes não
  relacionados a rate limiting.
- `RateLimitingWebApplicationFactory` — mesma factory com limites baixos e janela curta; uma
  instância nova por teste (nunca compartilhada), para esgotar o limitador sem depender de tempo
  real.
- `ProductionLikeWebApplicationFactory` — sobe a aplicação em `Environment = Production`,
  satisfazendo todos os guard clauses de produção do `Program.cs` com valores de teste (variáveis
  de ambiente, já que esses guards leem `builder.Configuration` antes do `Build()`).
- `EmailCaptureWebApplicationFactory` — habilita o `DevelopmentEmailSender` real (não um fake) com
  um diretório de captura único por instância, usado para recuperar o token bruto de e-mails de
  reset de senha/confirmação de e-mail a partir do link real enviado.
- `AntiforgeryTestHelper` — extrai o token antiforgery real de uma página renderizada.
- `CreateAuthenticatedScope` (na factory base) — para fluxos que só existem via MediatR/Blazor (sem
  endpoint HTTP próprio: Habits, Tasks, Projects, Todos, Wallet, Tags, Profile), constrói um
  `HttpContext` autenticado de verdade e resolve os handlers a partir dele — exercita o
  `HttpCurrentUserContext` real; `ICurrentUserContext` nunca é substituído por um fake.

### Cobertura

- Antiforgery/CSRF (login, logout);
- Login (credenciais válidas/inválidas, normalização de e-mail, conta desativada/não confirmada,
  cookie + claim de SessionVersion, redirecionamento pós-login, não vazamento de dados sensíveis);
- Rate limiting via HTTP real (IP, e-mail normalizado, genericidade do 429, não afeta outros
  endpoints);
- Cookies (nome, HttpOnly, Secure por ambiente, SameSite, Path, expiração, remember-me);
- Logout (limpeza de cookie, revogação de acesso, idempotência, escopo local por dispositivo);
- Invalidação de sessão via `OnValidatePrincipal` real (cookie forjado com claim ausente/inválida/
  antiga, usuário inexistente/desativado, e o cenário real de troca de senha/reset/desativação
  invalidando um cookie previamente emitido);
- Autorização (páginas protegidas/públicas, endpoints protegidos por `RequireAuthorization()`);
- Isolamento multiusuário (Habits, Tasks, Projects, Todos, Wallet, Tags, Profile) via
  `CreateAuthenticatedScope`;
- Reset de senha e confirmação de e-mail, fluxo completo (token real capturado do e-mail, inválido,
  expirado, já usado, revogação, indistinguibilidade existente/inexistente, throttle);
- Problem Details (status, content-type, estrutura, correlationId, ausência de vazamento);
- Security headers (o que está de fato configurado hoje).

### Limitações conhecidas (documentadas, não contornadas artificialmente)

- **HSTS não pôde ser verificado via TestServer**: `HstsMiddleware` só adiciona o header quando
  `HttpContext.Request.IsHttps` é verdadeiro; o TestServer do `Microsoft.AspNetCore.Mvc.Testing`
  nunca realiza um handshake TLS real — um `BaseAddress` `https://` só afeta a construção de URLs
  relativas, não `Request.IsHttps`. Confirmado via observação direta (resposta real em produção,
  status 200, sem o header). Limitação conhecida de se testar HSTS via `WebApplicationFactory`, não
  um defeito do `Program.cs`.
- **Antiforgery em "Production" retorna 400 com corpo vazio**: sob `ProductionLikeWebApplicationFactory`,
  uma requisição sem token antiforgery retorna 400 sem nenhum corpo/log do `GlobalExceptionHandler`
  — diferente do Development, onde o mesmo cenário chega ao handler e retorna
  `application/problem+json` completo. A rejeição em si (400, nenhuma sessão emitida) está correta
  e não vaza mais informação que em Development; a causa mais provável é a mesma limitação de
  `IsHttps` acima interagindo com a máquina de antiforgery, mas isso não foi confirmado com certeza
  nesta Sprint. Ver `ProblemDetailsIntegrationTests.cs` para o teste que documenta isso.
- **Códigos 404/409/500/503 do `GlobalExceptionHandler`** (`ActivityNotFoundException`,
  `InvalidDomainStateException` fora do catch local de `/auth/login`, `PersistenceException`, erro
  não mapeado) não são alcançáveis por uma requisição HTTP real hoje: a superfície HTTP desta
  aplicação é só `/auth/login`, `/auth/logout`, `/health*` e páginas Blazor — nenhuma delas deixa
  esses tipos de exceção escaparem de um MediatR handler para o pipeline HTTP (isso só acontece a
  partir do circuito SignalR do Blazor). Não foi criado um endpoint artificial só para forçar esses
  códigos.
- **429 do rate limiter não usa `application/problem+json`**: `/auth/login`'s `AddEndpointFilter`
  responde com `Results.Text(...)`, texto simples, diferente de todo outro caminho de erro da
  aplicação. Comportamento real, documentado e testado (`ProblemDetailsIntegrationTests.cs`); não é
  uma falha de segurança (o corpo já era genérico), mas é uma inconsistência de contrato que pode
  valer a pena alinhar em uma Sprint futura.

## 7. E2E tests (Playwright) — implementado (Sprint 12.7)

`tests/LevelUp.E2E.Tests/` dirige a aplicação real através de um Chromium real (headless), sem
substituir nada do pipeline HTTP/SignalR. Não duplica o que a Sprint 12.6 já cobre via
`WebApplicationFactory`/TestServer (antiforgery, cookies, `SessionVersion`, `CurrentUser`, rate
limiting, autorização interna) — valida apenas o comportamento observável pelo usuário.

### Infraestrutura

- `E2EWebApplicationFactory` — `WebApplicationFactory<Program>` própria (não referencia
  `LevelUp.Web.Tests`; apenas o projeto de produção `LevelUp.Web`), usando a API nativa do .NET 10
  `UseKestrel(port: 0)` para expor um endpoint Kestrel real em porta dinâmica (TestServer não é
  acessível por um browser real). O `Server` da factory não pode ser tocado diretamente sob Kestrel
  (lança `NotSupportedException`); o startup é disparado via `CreateClient()` e o endereço real lido
  de `ClientOptions.BaseAddress`. Storage JSON isolado por instância, apagado no `Dispose`.
- `PlaywrightAppFixture` (`IAsyncLifetime`, compartilhada via `IClassFixture` por classe de teste) —
  sobe a factory acima e um único `IBrowser` Chromium por classe.
- `E2ETestBase` — abre um `IBrowserContext`/`IPage` por teste; inicia trace do Playwright
  (screenshots + snapshots + sources) em todo teste; ao final, descarta o trace silenciosamente se o
  teste passou, ou grava screenshot full-page + `trace.zip` em `e2e-artifacts/` (apagado apenas se o
  teste falhar) usando `Xunit.TestContext.Current.TestState` (mecanismo do xunit v3) para saber o
  resultado do teste dentro do próprio `DisposeAsync`.
- `[assembly: CollectionBehavior(DisableTestParallelization = true)]` — cada classe de teste sobe seu
  próprio Chromium + Kestrel; rodar classes em paralelo (padrão do xunit) satura a máquina e produz
  falhas por contenção de recursos, não por defeito real.

### Navegador

Apenas Chromium (headless). Firefox/WebKit não fazem parte do escopo — não há necessidade de
cobertura cross-browser nesta Sprint e rodar múltiplos navegadores só aumentaria o tempo de
execução sem adicionar confiança.

### Seletores

Preferência por `GetByRole`/`GetByLabel`/texto estável, nunca CSS frágil, índice ou XPath. Onde o
sinal mais óbvio (ex.: `aria-expanded` de um menu) se mostrou não confiável por um defeito real de
acessibilidade pré-existente (ver abaixo), o teste passou a depender de um sinal alternativo
igualmente real (o próprio rótulo acessível do elemento gatilho, ou a visibilidade do próprio
container do menu) em vez de contornar com um seletor frágil.

### Esperas

Apenas auto-waiting nativo do Playwright (`Locator.ClickAsync`, `Expect(locator).ToBeVisibleAsync()`
etc.) — nenhum `Thread.Sleep`/`Task.Delay`/timeout arbitrário. Uma navegação disparada por um
redirecionamento do servidor (login bem-sucedido, clique num link) estabelece seu próprio circuito
SignalR que o `WaitForLoadStateAsync(NetworkIdle)` de uma navegação explícita anterior não cobre —
por isso os helpers de login/navegação repetem essa espera logo após o clique que dispara o
redirecionamento.

### Fluxos cobertos (7 testes)

1. `CreateAccount_ReachesEmailConfirmationPending` — criação de conta até a tela de confirmação.
2. `Login_CompletesOnboarding_ReachesDashboard` — login, onboarding completo, chegada ao dashboard.
3. `Logout_EndsSessionAndBlocksDashboard` — logout e bloqueio de acesso subsequente ao dashboard.
4. `EditProfile_UpdatesName` — edição de perfil.
5. `CreateAndCompleteHabit_UpdatesBalanceAndXp` — criação/conclusão de hábito com atualização visual
   de saldo e XP (sem validar o cálculo interno, já coberto por testes unitários).
6. `CreateAndCompleteTask_TogglesCompletion` — criação/conclusão de tarefa; ver nota abaixo sobre a
   visão Completed/Active do `DashboardColumn`.
7. `CreateTagAndTransaction_UpdatesBalance` — criação de tag e transação no Wallet com saldo
   atualizado.

### Defeitos reais encontrados (reportados, não corrigidos nesta Sprint — fora de escopo)

- `ActivityFilterBar.razor` renderiza `aria-expanded="@showCreateMenu"` com o `bool.ToString()`
  padrão do C# (`"True"`/`"False"`, maiúsculo), enquanto a spec ARIA exige `"true"`/`"false"`
  minúsculo — diferente do padrão correto já usado em `ProjectContextFilter.razor`
  (`isOpen.ToString().ToLowerInvariant()`). O filtro `Expanded` do Playwright nunca casa com o valor
  quebrado; o teste passou a verificar a visibilidade do próprio menu em vez do atributo.

### Uma investigação que não era um defeito

A conclusão de uma tarefa pareceu, à primeira vista, recarregar a página (dashboard vazio na tela
final). Uma captura completa de logs do servidor + rede/WebSocket do Playwright confirmou que o
circuito SignalR nunca cai (nenhum `_blazor/disconnect`, nenhum `GET /daily` repetido) e que o
`ToggleTaskCommand`/persistência JSON são bem-sucedidos. A causa real: `DashboardColumn` só renderiza
`CompletedContent` quando seu próprio toggle interno `showCompleted` está ativo
(`DashboardColumn.razor:45`); ao completar a única tarefa ativa, `ActiveCount` cai a zero e a coluna
mostra o empty state em vez do card. Era uma suposição incorreta do teste (que nunca alternava para
a visão Completed antes de asserir), não um defeito de produção — corrigido apenas no teste
(`HabitAndTaskTests.cs`), que agora clica em "Show completed tasks" antes de validar o card
concluído.

### Executar localmente

```powershell
dotnet build tests/LevelUp.E2E.Tests/LevelUp.E2E.Tests.csproj --configuration Release
pwsh tests/LevelUp.E2E.Tests/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test tests/LevelUp.E2E.Tests/LevelUp.E2E.Tests.csproj --configuration Release
```

### Falhas: screenshot e trace

Gerados apenas quando um teste falha, em
`tests/LevelUp.E2E.Tests/bin/Release/net10.0/e2e-artifacts/<nome-do-teste>.png` e `.trace.zip`.
Visualizar um trace:

```powershell
pwsh tests/LevelUp.E2E.Tests/bin/Release/net10.0/playwright.ps1 show-trace tests/LevelUp.E2E.Tests/bin/Release/net10.0/e2e-artifacts/<nome-do-teste>.trace.zip
```

### CI

O workflow `.github/workflows/ci.yml` instala o Chromium do Playwright após o build (o script de
instalação é gerado no output de build) e publica `e2e-artifacts/` como artefato sempre que existir
conteúdo (só existe em caso de falha).

## 8. Testes arquiteturais — implementado (Sprints 12.8, 13.6)

Dois arquivos, um por fronteira, inspecionam metadados de assembly/reflexão compilados — não texto-fonte:

- `LevelUp.Domain.Tests/DomainAssemblyBoundaryTests.cs` (Sprint 12.8) — `LevelUp.Domain` nunca referencia
  `System.Text.Json`, `Microsoft.EntityFrameworkCore` ou `LevelUp.Infrastructure`; nenhum tipo do Domain
  carrega atributo de serialização.
- `LevelUp.Application.Tests/PersistenceContractBoundaryTests.cs` (Sprint 13.3, estendido na 13.6) —
  nenhum contrato em `Common.Contracts`/`*.Contracts` expõe `LevelUpData` ou qualquer tipo
  `System.Text.Json.*` em parâmetro ou retorno (exceto `ILevelUpRepository`, a exceção legada
  explicitamente rastreada); nenhuma interface de contrato é uma definição genérica
  (`IRepository<T>`)/tem "UnitOfWork" no nome; `LevelUp.Application` nunca referencia
  `LevelUp.Infrastructure`.

Ambos falham a build (`--warnaserror` não afeta isso — são testes, falham como teste) se a fronteira for
violada, não apenas avisam. Ver `docs/architecture/08-migration-status.md` §6 para o que esses testes
cobrem hoje vs. o que ainda depende de `ILevelUpRepository`.

## 9. Fakes de teste — padronizados (Sprint 13.6)

`LevelUp.Application.Tests` usa três fakes compartilhados (`FakeLevelUpRepository`,
`FakeCurrentUserContext`, `FakeApplicationCache`) em vez de cópias privadas por arquivo de teste — ver
`docs/architecture/08-migration-status.md` §6. Não criar uma nova cópia privada de `ILevelUpRepository`
em um teste novo; reusar `FakeLevelUpRepository`. Fakes com comportamento realmente distinto entre
cenários (ex.: `FakePasswordService` com contagem de chamadas em `AuthenticationHandlersTests`)
permanecem locais deliberadamente — não force a consolidação de fakes com comportamento divergente.
