# Testing Strategy

**Fonte da verdade:** verificado diretamente em `tests/BeeDay.Domain.Tests/`,
`tests/BeeDay.Application.Tests/`, `tests/BeeDay.Infrastructure.Tests/`, `tests/BeeDay.Web.Tests/`,
`tests/BeeDay.E2E.Tests/`, `BeeDay.slnx`, e confirmado por execução real de
`dotnet test BeeDay.slnx` (Debug e Release) ao final da Sprint 30.23.

**Última verificação:** 2026-08-21 (Sprint 31.9, EPIC 31) — inventário dos cinco
projetos e baseline integral reconciliados com o total real atual (1.557, era 1.554 desde a Sprint
30.23 — Web +1, E2E +2 por crescimento orgânico nos dias seguintes). Contagens ficam somente neste
owner e no índice de Testing; outros documentos apontam para cá em vez de copiar números.

## 1. Pirâmide — os 5 projetos de teste e o que cada um verifica

| Projeto | O que valida | Infraestrutura real usada |
|---|---|---|
| `BeeDay.Domain.Tests` | Invariantes de Aggregate/Entity/Value Object, sem nenhuma infraestrutura | Nenhuma — só o assembly `BeeDay.Domain` |
| `BeeDay.Application.Tests` | Handlers de Command/Query com portas fakes | `FakeUnitOfWork` + fakes de repositório, `FakeCurrentUserContext` |
| `BeeDay.Infrastructure.Tests` | Repositórios EF Core, Identity, hashing de senha, Event Journal, health check contra um SQL Server LocalDB real | `EfLocalDbTestBase`/`EfLocalDbCollection` — ver §4 |
| `BeeDay.Web.Tests` | Componentes Blazor (bUnit), source contracts e integração HTTP real (`WebApplicationFactory`) | `BeeDayWebApplicationFactory` e variantes — documentado em [`docs/web/06-testing.md`](../web/06-testing.md) |
| `BeeDay.E2E.Tests` | Fluxos, responsividade e axe via Chromium/Playwright | `PlaywrightAppFixture`/`E2EWebApplicationFactory` — documentado em [`docs/web/06-testing.md`](../web/06-testing.md) |

Total confirmado por execução real na Sprint 31.9: **1.557 testes, 0 falhas** (121 Domain,
119 Application, 216 Infrastructure, 880 Web, 221 E2E) — ver §7.

## 2. Domain.Tests — invariantes sem infraestrutura

13 arquivos cobrindo os Aggregates (`Habit`, `RecurringTask`, `Project`, `Todo`, `Transaction`,
`User`, `Wallet`, `WalletTag`, `UserToken`) e `DomainAssemblyBoundaryTests.cs` — teste
arquitetural (Sprint 12.8) que inspeciona metadados de assembly via reflexão: `BeeDay.Domain` nunca
referencia `System.Text.Json`, `Microsoft.EntityFrameworkCore` ou `BeeDay.Infrastructure`; nenhum
tipo do Domain carrega atributo de serialização. Falha a build como teste (não apenas avisa) se a
fronteira for violada.

## 3. Application.Tests — handlers com portas fakes

19 arquivos de teste (mais `FakeUnitOfWork.cs`/`FakeCurrentUserContext.cs`, que não são testes).
`PersistenceContractBoundaryTests.cs` (Sprint 13.3, estendida na 13.6, âncora de
reflexão trocada para `IUnitOfWork` na 14.6) — nenhum contrato em `Common.Contracts`/`*.Contracts`
expõe tipo `System.Text.Json.*`; nenhuma interface de contrato é genérica (`IRepository<T>`) ou tem
"UnitOfWork" no nome (exceto `IUnitOfWork` propriamente dito); `BeeDay.Application` nunca referencia
`BeeDay.Infrastructure`.

**Fakes** (não recriar cópias locais — reusar estes): `FakeUnitOfWork.cs` implementa `IUnitOfWork`
(métodos de transação são no-op) e expõe 8 listas independentes (`UsersData`, `HabitsData`, etc.) —
nenhum tipo agrega as 8 num documento único, mesmo princípio de "sem estado global" que motivou a
remoção de `LevelUpData` do Domain (Sprint 14.7). `FakeCurrentUserContext` completa o conjunto
padrão (`FakeApplicationCache` foi removido na Sprint 18.6, junto com o `IApplicationCache` que
implementava — código morto comprovado, nunca populado em produção). Fakes com comportamento
realmente divergente entre cenários (ex.:
contagem de chamadas em um teste específico de autenticação) permanecem locais deliberadamente — não
force a consolidação de fakes com comportamento distinto.

## 4. Infrastructure.Tests — contra SQL Server LocalDB real

27 arquivos: 13 na raiz (`BeeDayDbContextTests`, `IdentityInfrastructureTests`,
`JsonEventJournalTests`, `MemoryIdentityRequestThrottleTests`, `Pbkdf2PasswordServiceTests`,
`DevelopmentEmailSenderTests`, `EmailAddressLogMaskingTests`,
`EmailProviderDependencyInjectionTests`, `EmailProviderSelectorTests`,
`EmailSecretsConfigurationTests`, `HmgRecipientGuardDependencyInjectionTests`,
`HmgRecipientGuardedEmailSenderTests`, e `InfrastructureAssemblyBoundaryTests` — ver §6) + 1 sob
`HealthChecks/` (`SqlServerHealthCheckTests`) + 13 sob `Persistence/SqlServer/` (3 direto:
`EfDashboardReadServiceTests`, `EfUnitOfWorkTests`, `EfWalletReadServiceTests`; 10 sob
`Repositories/`: os 8 repositórios por Aggregate + `EfLocalDbCollection`/`EfLocalDbTestBase`).

### Test Database — `EfLocalDbTestBase`

Cada classe de teste que herda `EfLocalDbTestBase` (`IAsyncLifetime`) cria, no `InitializeAsync`, um
banco LocalDB com nome único (`BeeDay_EfTests_{Guid}`), aplica a migration real via
`Database.MigrateAsync()` — deliberadamente não `EnsureCreated()`, que pularia a migration inteira
(incluindo o índice `UX_ExperienceEntries_Dedup`, SQL raw, e derrotaria o propósito de testar contra
a migration real) — e derruba o banco (`EnsureDeletedAsync`) no `DisposeAsync`. Cada teste tem seu
próprio banco, nunca compartilhado.

`EfLocalDbCollection` (`[CollectionDefinition("EfLocalDb", DisableParallelization = true)]`)
desabilita paralelismo **apenas** para as classes `Ef*RepositoryTests` — muitas `CREATE
DATABASE`/`DROP DATABASE` concorrentes contra a mesma instância `mssqllocaldb` causam contenção;
`BeeDayDbContextTests` (que não usa `EfLocalDbTestBase`) continua rodando em paralelo, não afetado.
Esse é o mesmo tipo de contenção de recurso observado entre projetos de teste diferentes ao rodar
`dotnet test` na solução inteira — ver §7.

## 5. Web.Tests e E2E.Tests

Mapeamento componente→teste, infraestrutura de `WebApplicationFactory` (`BeeDayWebApplicationFactory`
e as 3 variantes especializadas — `EmailCaptureWebApplicationFactory`,
`ProductionLikeWebApplicationFactory`, `RateLimitingWebApplicationFactory`), infraestrutura Playwright
(`PlaywrightAppFixture`/`E2ETestBase`/`E2EWebApplicationFactory`) e os fluxos E2E cobertos estão
documentados por completo em [`docs/web/06-testing.md`](../web/06-testing.md) (Sprint 16.7) — este
documento não duplica esse conteúdo.

### Limitações conhecidas de testar via `WebApplicationFactory`/TestServer

`TestServer` (usado por toda `Ef*WebApplicationFactory`) nunca realiza handshake TLS real —
`HttpContext.Request.IsHttps` é sempre `false`, mesmo com um `BaseAddress` `https://`. Duas
consequências reais, confirmadas por captura de HTTP real, não presumidas:

- **HSTS não pode ser verificado via `WebApplicationFactory`**: `HstsMiddleware` só adiciona o
  header quando `Request.IsHttps` é verdadeiro — nunca acontece em teste. Limitação de testar HSTS
  por esse mecanismo, não um defeito de `Program.cs`.
- **Antiforgery em `Production` retorna 400 com corpo vazio**: sob `ProductionLikeWebApplicationFactory`,
  uma requisição sem token antiforgery retorna 400 sem nenhum corpo/log do `GlobalExceptionHandler`
  — diferente de Development, onde o mesmo cenário chega ao handler e retorna
  `application/problem+json` completo. A rejeição em si (400, nenhuma sessão emitida) está correta e
  não vaza mais informação que em Development; causa mais provável é a mesma limitação de `IsHttps`
  interagindo com a máquina de antiforgery — não totalmente root-caused. Ver comentário de classe em
  `tests/BeeDay.Web.Tests/Integration/ProblemDetailsIntegrationTests.cs`.

Duas limitações adicionais, não relacionadas a TLS:

- **Códigos 409/500/503 do `GlobalExceptionHandler`** (`InvalidDomainStateException` fora do catch
  local de `/auth/login`, `PersistenceException`, erro não mapeado) não são alcançáveis por uma
  requisição HTTP real: a superfície HTTP desta aplicação é
  só `/auth/login`, `/auth/logout`, `/health*` e páginas Blazor — nenhuma delas deixa esses tipos de
  exceção escaparem de um handler MediatR para o pipeline HTTP (só acontece a partir do circuito
  SignalR do Blazor). Nenhum endpoint artificial foi criado só para forçar esses códigos.
- **429 do rate limiter não usa `application/problem+json`**: `/auth/login`'s `AddEndpointFilter`
  responde com `Results.Text(...)` — texto simples, diferente de todo outro caminho de erro da
  aplicação. Comportamento real, testado (`ProblemDetailsIntegrationTests.cs`), não uma falha de
  segurança (o corpo já era genérico) — ver
  [`docs/web/01-composition-root.md`](../web/01-composition-root.md) §9.

## 6. Testes arquiteturais — os 3 guardas de fronteira

| Teste | Verifica |
|---|---|
| `BeeDay.Domain.Tests/DomainAssemblyBoundaryTests.cs` | `BeeDay.Domain` nunca referencia `System.Text.Json`/EF Core/`BeeDay.Infrastructure` |
| `BeeDay.Application.Tests/PersistenceContractBoundaryTests.cs` | Nenhum contrato expõe tipo `System.Text.Json.*`; nenhuma interface genérica de repositório; `BeeDay.Application` nunca referencia `BeeDay.Infrastructure` |
| `BeeDay.Infrastructure.Tests/InfrastructureAssemblyBoundaryTests.cs` (Sprint 30.9, `BD30-F009`) | `BeeDay.Infrastructure` nunca referencia `BeeDay.Web`/assemblies de Blazor Components — Domain e Application já tinham guarda real, Infrastructure e Web não tinham nenhuma |

Todos os três falham a build como teste (não apenas avisam) se a fronteira for violada — a única forma de
"quebrar" a Dependency Rule (ver [`docs/architecture/04-dependency-rules.md`](../architecture/04-dependency-rules.md))
sem que um teste falhe é adicionar uma nova violação de um tipo que nenhum dos três testes inspeciona
ainda.

## 7. Fluxo de execução e cobertura

### Local

```powershell
dotnet restore BeeDay.slnx
dotnet format BeeDay.slnx --verify-no-changes --no-restore
dotnet build BeeDay.slnx --configuration Release --no-restore --warnaserror
dotnet test BeeDay.slnx --configuration Release --no-build
```

Apenas um projeto:

```powershell
dotnet test tests/BeeDay.Web.Tests/BeeDay.Web.Tests.csproj --configuration Release --no-build
```

### CI (`.github/workflows/ci.yml` e `release-quality-gate.yml` — responsabilidades divididas, não idênticas)

`ci.yml` roda **apenas** `Domain.Tests` + `Application.Tests` (lista explícita de 2 projetos) — sem
LocalDB, sem browser, sem Playwright. `Format`, `Infrastructure.Tests`, `Web.Tests` e `E2E.Tests`
foram movidos para `release-quality-gate.yml` na Sprint 19.8.5, que é quem de fato instala o
Chromium do Playwright (`pwsh tests/BeeDay.E2E.Tests/bin/Release/net10.0/playwright.ps1 install
chromium`) e executa **todos os 5 projetos** de teste (`Get-ChildItem -Path "tests" -Filter
"*.csproj" -Recurse`), sempre em `--configuration Release`. Ambos os workflows usam
`--logger "trx;LogFileName=$projectBaseName.trx"` — um nome de arquivo dinâmico por projeto, não um
nome fixo único. `deploy-prd.yml` é o workflow de Produção e não executa nenhum teste.

### Resultado mais recente (Sprint 31.9), executado localmente

1.557 testes, 0 falhas: 121 Domain, 119 Application, 216 Infrastructure, 880 Web, 221 E2E —
confirmado em Debug (execução completa registrada no início da Sprint 31.1). A suíte completa usa
LocalDB e Chromium e, por isso, deve receber timeout compatível.

### `BD30-F042` — confiabilidade de `dotnet test BeeDay.slnx` (whole-solution) em Debug

Causa raiz confirmada na Sprint 30.24, com evidência real (não hipótese): rodar `dotnet test
BeeDay.slnx` como um único comando contra a solução inteira executa os hosts de teste de múltiplos
projetos **concorrentemente**, não em sequência — confirmado observando a própria saída de comandos
já executados nesta EPIC (Sprint 30.23): a linha "Execução de teste para... Infrastructure.Tests.dll"
aparece logo após Domain/Application, muito antes de Web.Tests/E2E.Tests terminarem de compilar, mas
o resultado final de Infrastructure.Tests (34s de duração) só é impresso **depois** do resultado de
Web.Tests (23s) — prova de que Infrastructure.Tests (que cria/derruba bancos LocalDB reais, ver §4)
ainda estava rodando enquanto Web.Tests (`TestServer`) e E2E.Tests (Kestrel + Chromium reais) também
rodavam. Esse é o mesmo tipo de contenção de recurso já documentado no §4 para `EfLocalDbCollection`,
só que entre projetos diferentes, não entre classes do mesmo projeto.

Isso explica por completo o padrão observado em `BD30-F042` ao longo da EPIC (Sprints 30.10, 30.17,
30.18, 30.19): sempre `TimeoutException` de navegação/screenshot do Playwright, nunca o mesmo teste
duas vezes, nunca em `--configuration Release`, nunca em CI. Nenhum dos dois workflows de CI jamais
reproduz essa condição: `ci.yml` só roda Domain+Application (sem LocalDB/browser/host); `release-
quality-gate.yml` roda os 5 projetos, mas via loop PowerShell explícito, um projeto por vez,
sempre `--configuration Release` — nunca concorrente, nunca Debug. O comando "Local" documentado em
§7 acima (`dotnet test BeeDay.slnx --configuration Release --no-build`) já evita esse padrão ao
recomendar Release; a instabilidade documentada nesta EPIC ocorreu sempre em execuções ad-hoc
`dotnet test BeeDay.slnx` (Debug implícito, comando também exigido pelo gate de validação obrigatório
de `CLAUDE.md`) durante a auditoria, não pelo fluxo local/CI documentado aqui.

**Contrato de repetibilidade**: uma falha `TimeoutException` de navegação/screenshot do Playwright
observada especificamente durante um `dotnet test BeeDay.slnx` (Debug, sem `--configuration`)
executado contra a solução inteira é consistente com este padrão conhecido — reexecutar o(s)
projeto(s) afetado(s) isoladamente (`dotnet test tests/BeeDay.E2E.Tests/...`) ou em
`--configuration Release` antes de classificar como `CHANGE-CAUSED`.

### Hang determinístico em `AccountLifecycleTests.Login_CompletesOnboarding_ReachesDashboard` — descoberto na Sprint 32.5, `UNRESOLVED`

Distinto de `BD30-F042` acima, apesar da semelhança superficial (ambos envolvem Playwright/E2E). `BD30-F042`
é uma `TimeoutException` de navegação/screenshot causada por concorrência entre projetos, nunca repete no
mesmo teste, nunca em execução isolada. O achado desta seção é um **hang indefinido real** (sem exceção —
exige `--blame-hang` para ser encerrado), **determinístico no mesmo teste**, e **reproduzido em isolamento
total** (projeto único, teste único filtrado, sem nenhuma concorrência), portanto não é uma instância de
`BD30-F042` e não deve ser classificado como tal.

**Evidência (4 reproduções independentes, mesma assinatura):**

1. Execução isolada do projeto `BeeDay.E2E.Tests` inteiro (sem filtro), instrumentada com `--blame-hang
   --blame-hang-timeout 3m`: 86 testes aprovados em sequência (`NavigationTests`, `EmailClientCompatibilityTests`,
   `ProjectLifecycleTests`, `InstitutionalPagesTests`, `Epic21ConsolidationTests`, dois `AccountLifecycleTests`
   anteriores) e trava exatamente em `Login_CompletesOnboarding_ReachesDashboard`. `Sequence_*.xml` do Blame
   Collector confirma `Completed="False"` apenas para este teste.
2. Execução isolada do mesmo projeto completo, sem instrumentação: mesmo teste trava; processo confirmado
   sem progresso (CPU acumulada desprezível ao longo de ~45 min) e encerrado manualmente.
3. Execução com filtro restrito a **apenas este teste** (`--filter
   "FullyQualifiedName~AccountLifecycleTests.Login_CompletesOnboarding_ReachesDashboard"`), sem nenhum outro
   teste antes: trava do mesmo jeito, do início ao fim da execução — descarta contenção por acúmulo de
   estado de testes anteriores como causa.
4. Repetição da execução (3) após uma tentativa de correção (ver abaixo): trava novamente, idêntico.

**Causa raiz confirmada por análise de memory dump** (`dotnet-dump analyze` sobre os `.dmp` gerados pelo
Blame Collector — `clrstack -all`, `dumpasync`, `dumpobj`/`dumpvc`): em três dumps independentes (execuções
1, 3 e 4), a continuação assíncrona do teste (`AccountLifecycleTests+<Login_CompletesOnboarding_ReachesDashboard>d__4`,
estado `4`) está suspensa aguardando uma chamada de API do Playwright presa em
`Microsoft.Playwright.Transport.Connection.WrapApiCallAsync` → `InnerSendMessageToServerAsync` →
`StdIOTransport.GetResponseAsync` — ou seja, uma mensagem JSON-RPC foi enviada ao processo driver Node.js
embutido do Playwright e a resposta correspondente nunca chega. `clrstack -all` confirma que **nenhuma
thread do processo está bloqueada** em lock, I/O síncrono ou chamada SQL — todas as threads do pool estão
ociosas (`LowLevelLifoSemaphore.Wait`) e as únicas threads ativas pertencentes à aplicação hospedada
(`Program.<Main>$` do `BeeDay.Web`, via `WebApplicationFactory`) também estão ociosas. Isso descarta deadlock
gerenciado, lock de LocalDB e wait síncrono sem timeout no código de produção — é uma resposta de protocolo
genuinamente perdida na comunicação stdio entre o cliente .NET do Playwright e seu driver Node.js.

**Não é regressão da Sprint 32.5.** O diff da Sprint 32.5 contra `hmg` toca apenas
`Components/Features/Authentication/Pages/Login.razor` (atributos `oninvalid`/`oninput` de `EXP32-F007` —
nunca disparam neste teste, que sempre submete credenciais válidas) e `EditorModalShell`/`DialogFocusScope`
(foco pós-validação de `EXP32-F021`, exclusivo do fluxo de Wallet). `Tutorial.razor`,
`BeeDayWebService.CompleteOnboardingAsync`, `PlaywrightAppFixture` e `E2ETestBase` — todo o caminho real
percorrido por este teste — permanecem inalterados nesta Sprint.

**Tentativa de correção investigada e descartada**: o único padrão estruturalmente incomum deste teste frente
aos demais é seu laço de retry (`NEXT`/`ENTER beeday`) combinando `ClickAsync` repetido com um
`WaitForAsync` de timeout curto (800 ms). Aumentar esse timeout para 2000 ms (mudança mínima, só no arquivo
de teste) foi testado isoladamente e **não** resolveu o hang (reprodução 4 acima) — revertido. Isso descarta
o laço de retry como gatilho e reforça que a causa é externa ao código do teste, no próprio transporte do
Playwright.

**Classificação**: `ENVIRONMENT` — defeito pré-existente na infraestrutura de teste E2E (camada de
transporte cliente Playwright ↔ driver Node.js), determinístico neste teste/ambiente, ortogonal ao código
de produção do beeday e à Sprint 32.5.

**Contrato de repetibilidade** (inverso ao de `BD30-F042`): ao contrário de `BD30-F042` — que exige
concorrência entre projetos e nunca repete no mesmo teste —, este hang é determinístico mesmo em isolamento
total de processo único. Uma futura ocorrência de hang indefinido (não `TimeoutException`) especificamente
em `Login_CompletesOnboarding_ReachesDashboard`, que exija `--blame-hang` para ser encerrado, é consistente
com este padrão e não deve ser atribuída a `BD30-F042` nem a uma regressão de Sprint sem evidência adicional.

**Status: `ACCEPTED RISK` (decisão do proprietário do repositório, Sprint 32.5, 2026-08-23).** Causa raiz
não resolvida — corrigi-la exigiria investigação no próprio cliente/driver Playwright (possível bug
conhecido, downgrade/upgrade de versão, ou mudança de ambiente), fora do escopo de "menor correção
coerente" disponível nesta Sprint. O teste **não** foi marcado `Skip`; permanece na suíte como está. A
validação obrigatória da Sprint 32.5 trata este achado como `PASS WITH FINDINGS`, não como bloqueador —
uma futura execução de `dotnet test BeeDay.slnx` ou de `BeeDay.E2E.Tests` isolado que trave exatamente
neste teste, exigindo `--blame-hang`, é consistente com este achado já registrado e não deve, por si só,
impedir a conclusão de uma Sprint. Investigação da causa raiz no transporte do Playwright permanece como
item de acompanhamento separado, sem Sprint dona atribuída.

### Cobertura formal (`dotnet test --collect:"XPlat Code Coverage"`)

`coverlet.collector` adicionado aos 5 projetos de teste na Sprint 30.24 (`BD30-F007`) — rodar com
`--collect:"XPlat Code Coverage"` produz um `coverage.cobertura.xml` por projeto em
`TestResults/<run-id>/`. Nenhum threshold/gate de cobertura foi adicionado — decisão de política
(qual % é aceitável, se deveria bloquear PR) fora da autoridade de uma auditoria de engenharia de
teste. A "cobertura" real e verificável continua sendo a lista de cenários por classe de teste
documentada neste arquivo e em [`docs/web/06-testing.md`](../web/06-testing.md), não uma porcentagem
calculada — o número de linha/branch agora disponível é um sinal complementar, não o objetivo.

## 8. Fontes consultadas

- Inventário atual dos cinco projetos sob `tests/` (excluindo `bin`/`obj`).
- `tests/BeeDay.Application.Tests/FakeUnitOfWork.cs`, `FakeCurrentUserContext.cs`,
  `PersistenceContractBoundaryTests.cs`.
- `tests/BeeDay.Domain.Tests/DomainAssemblyBoundaryTests.cs`.
- `tests/BeeDay.Infrastructure.Tests/Persistence/SqlServer/Repositories/EfLocalDbTestBase.cs`,
  `EfLocalDbCollection.cs`, `HealthChecks/SqlServerHealthCheckTests.cs`,
  `MemoryIdentityRequestThrottleTests.cs` (Sprints 18.5/18.6).
- `tests/BeeDay.Web.Tests/Integration/ProblemDetailsIntegrationTests.cs` (comentário de classe —
  limitações de `WebApplicationFactory`/TestServer, §5).
- `dotnet test BeeDay.slnx` (Debug), executado no início da Sprint 31.1 (1.557/1.557) — saída bruta
  usada como evidência direta da concorrência entre projetos documentada em `BD30-F042` acima.
- Sprint 32.5: 4 execuções isoladas de `dotnet test tests/BeeDay.E2E.Tests/...` (com e sem `--blame-hang`,
  com e sem filtro), `Sequence_*.xml` do Blame Collector, e análise de memory dump via `dotnet-dump analyze`
  (`clrstack -all`, `dumpasync`, `dumpobj`/`dumpvc`) sobre os `.dmp` gerados — evidência do hang documentado
  na seção acima.
- `.github/workflows/ci.yml`, `release-quality-gate.yml` (fluxo de execução em CI).
- [`docs/web/06-testing.md`](../web/06-testing.md) e
  [`02-design-system-quality-gates.md`](02-design-system-quality-gates.md).
