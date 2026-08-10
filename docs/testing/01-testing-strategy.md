# Testing Strategy

**Fonte da verdade:** verificado diretamente em `tests/BeeDay.Domain.Tests/`,
`tests/BeeDay.Application.Tests/`, `tests/BeeDay.Infrastructure.Tests/`, `tests/BeeDay.Web.Tests/`,
`tests/BeeDay.E2E.Tests/`, `BeeDay.slnx`, e confirmado por execução real de
`dotnet test BeeDay.slnx --configuration Release --no-build` nesta Sprint (16.9).

**Última verificação:** 2026-08-10 (Sprint 18.8) — contagens de teste e composição de arquivos
atualizadas após as adições das Sprints 18.5/18.6; conteúdo estrutural desta Sprint 16.9 preservado.

## 1. Pirâmide — os 5 projetos de teste e o que cada um verifica

| Projeto | Arquivos `.cs` (sem `bin`/`obj`) | O que valida | Infraestrutura real usada |
|---|---|---|---|
| `BeeDay.Domain.Tests` | 12 | Invariantes de Aggregate/Entity/Value Object, sem nenhuma infraestrutura | Nenhuma — só o assembly `BeeDay.Domain` |
| `BeeDay.Application.Tests` | 18 | Handlers de Command/Query com portas fakes | `FakeUnitOfWork` + 8 fakes de repositório, `FakeCurrentUserContext` |
| `BeeDay.Infrastructure.Tests` | 19 | Repositórios EF Core, Identity, hashing de senha, Event Journal, health check contra um SQL Server LocalDB real | `EfLocalDbTestBase`/`EfLocalDbCollection` — ver §4 |
| `BeeDay.Web.Tests` | 61 | Componentes Blazor (bUnit) + integração HTTP real (`WebApplicationFactory`) | `BeeDayWebApplicationFactory` e variantes — documentado em [`docs/web/06-testing.md`](../web/06-testing.md) |
| `BeeDay.E2E.Tests` | 7 (+ infraestrutura) | Fluxos de usuário reais via Chromium/Playwright | `PlaywrightAppFixture`/`E2EWebApplicationFactory` — documentado em [`docs/web/06-testing.md`](../web/06-testing.md) |

Total confirmado por execução real na Sprint 18.7: **752 testes, 0 falhas** (93 Domain, 73
Application, 129 Infrastructure, 450 Web, 7 E2E) — ver §7.

## 2. Domain.Tests — invariantes sem infraestrutura

12 arquivos cobrindo os Aggregates (`Habit`, `RecurringTask`, `Project`, `Todo`, `Transaction`,
`User`, `Wallet`, `WalletTag`, `UserToken`) e `DomainAssemblyBoundaryTests.cs` — teste
arquitetural (Sprint 12.8) que inspeciona metadados de assembly via reflexão: `BeeDay.Domain` nunca
referencia `System.Text.Json`, `Microsoft.EntityFrameworkCore` ou `BeeDay.Infrastructure`; nenhum
tipo do Domain carrega atributo de serialização. Falha a build como teste (não apenas avisa) se a
fronteira for violada.

## 3. Application.Tests — handlers com portas fakes

18 arquivos. `PersistenceContractBoundaryTests.cs` (Sprint 13.3, estendida na 13.6, âncora de
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

19 arquivos: 5 na raiz (`BeeDayDbContextTests`, `IdentityInfrastructureTests`,
`JsonEventJournalTests`, `MemoryIdentityRequestThrottleTests`, `Pbkdf2PasswordServiceTests`) + 1 sob
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
e as 4 variantes especializadas), infraestrutura Playwright
(`PlaywrightAppFixture`/`E2ETestBase`/`E2EWebApplicationFactory`) e os 7 fluxos E2E cobertos estão
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

## 6. Testes arquiteturais — os 2 guardas de fronteira

| Teste | Verifica |
|---|---|
| `BeeDay.Domain.Tests/DomainAssemblyBoundaryTests.cs` | `BeeDay.Domain` nunca referencia `System.Text.Json`/EF Core/`BeeDay.Infrastructure` |
| `BeeDay.Application.Tests/PersistenceContractBoundaryTests.cs` | Nenhum contrato expõe tipo `System.Text.Json.*`; nenhuma interface genérica de repositório; `BeeDay.Application` nunca referencia `BeeDay.Infrastructure` |

Ambos falham a build como teste (não apenas avisam) se a fronteira for violada — a única forma de
"quebrar" a Dependency Rule (ver [`docs/architecture/04-dependency-rules.md`](../architecture/04-dependency-rules.md))
sem que um teste falhe é adicionar uma nova violação de um tipo que nenhum dos dois testes inspeciona
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

### CI (`.github/workflows/ci.yml`/`deploy-prd.yml`)

Idêntico, mais `--logger "trx;LogFileName=beeday-tests.trx" --results-directory ...` e, só em
`ci.yml`, instalação do Chromium do Playwright antes de rodar os testes (necessário para
`BeeDay.E2E.Tests`) — ver [`docs/deployment/01-deployment.md`](../deployment/01-deployment.md) §3.

### Resultado mais recente (Sprint 18.7), executado localmente

752 testes, 0 falhas: 93 Domain, 73 Application, 129 Infrastructure, 450 Web, 7 E2E. Uma execução
da solução completa em paralelo pode ocasionalmente reportar falha transiente em
`BeeDay.Web.Tests`/`BeeDay.E2E.Tests` por contenção de LocalDB/porta Kestrel entre os dois projetos
rodando ao mesmo tempo (observado e diagnosticado na Sprint 16.7); os mesmos testes passam 100%
quando o projeto afetado roda isolado, e uma nova execução da solução completa tipicamente passa
752/752.

### Cobertura formal (`dotnet test --collect:"XPlat Code Coverage"` ou equivalente)

Não configurada neste repositório — nenhum step de CI coleta métrica de cobertura de linha/branch,
nenhum arquivo `.runsettings`/`coverlet.runsettings` foi encontrado. A "cobertura" real e verificável
hoje é a lista de cenários por classe de teste documentada neste arquivo e em
[`docs/web/06-testing.md`](../web/06-testing.md), não uma porcentagem calculada.

## 8. Fontes consultadas

- Contagem de arquivos `.cs` (excluindo `bin`/`obj`) em `tests/BeeDay.Domain.Tests/`,
  `tests/BeeDay.Application.Tests/`, `tests/BeeDay.Infrastructure.Tests/`.
- `tests/BeeDay.Application.Tests/FakeUnitOfWork.cs`, `FakeCurrentUserContext.cs`,
  `PersistenceContractBoundaryTests.cs`.
- `tests/BeeDay.Domain.Tests/DomainAssemblyBoundaryTests.cs`.
- `tests/BeeDay.Infrastructure.Tests/Persistence/SqlServer/Repositories/EfLocalDbTestBase.cs`,
  `EfLocalDbCollection.cs`, `HealthChecks/SqlServerHealthCheckTests.cs`,
  `MemoryIdentityRequestThrottleTests.cs` (Sprints 18.5/18.6).
- `tests/BeeDay.Web.Tests/Integration/ProblemDetailsIntegrationTests.cs` (comentário de classe —
  limitações de `WebApplicationFactory`/TestServer, §5).
- `dotnet test BeeDay.slnx --configuration Release --no-build`, executado na Sprint 18.7 (752/752).
- `.github/workflows/ci.yml`, `deploy-prd.yml` (fluxo de execução em CI).
- [`docs/web/06-testing.md`](../web/06-testing.md) (Web.Tests/E2E.Tests, Sprint 16.7, reaproveitado
  sem duplicar).
