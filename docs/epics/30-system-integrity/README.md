# EPIC 30 — System Integrity & Complete Engineering Audit

Este documento é o Audit Ledger canônico da EPIC 30. As Issues e Sub-issues do GitHub Project
continuam sendo a fonte operacional de escopo, ordem e autorização. O Ledger registra somente
evidência versionada produzida pela execução dessas Issues.

**Fonte da verdade:** inventário obtido diretamente de `git ls-files`, `BeeDay.slnx`, arquivos
`.csproj`, `src/`, `tests/`, `.github/workflows/`, `scripts/`, configurações versionadas,
`docs/`, rotas Razor e execução real dos quality gates em 2026-08-20. O snapshot começou em
`0639e303b742e8c784f1b2620ae9523aabb9026c`, na branch
`sprint/30.1-repository-baseline-audit-ledger`, criada a partir de `hmg` sincronizada com
`origin/hmg`.

## 1. Finalidade e regras do Ledger

O Ledger permite provar que nenhuma área material foi omitida e encaminha cada achado para uma
Sprint proprietária. Ele não substitui uma auditoria profunda das áreas que pertencem às Sprints
30.2–30.30.

Estados usados durante a EPIC:

- `BASELINED`: inventariado com evidência reproduzível; auditoria profunda ainda pertence à Sprint
  indicada;
- `VERIFIED`: contrato inspecionado e confirmado pela evidência registrada;
- `OPEN`: achado material confirmado e atribuído, ainda sem resolução;
- `FIXED`: resolução implementada e validada;
- `ACCEPTED RISK`: risco aceito explicitamente, com justificativa;
- `BLOCKED`: validação impedida por dependência externa identificada.

Nenhum item pode permanecer sem estado ou sem Sprint proprietária. O fechamento da EPIC exige que
todo achado termine como `FIXED`, `VERIFIED` ou `ACCEPTED RISK`.

## 2. Ambiente do baseline

| Item | Evidência |
|---|---|
| Sistema versionado | 1.074 arquivos rastreados no Git |
| SDK | .NET SDK `10.0.400` |
| EF CLI | ferramenta local `dotnet-ef` `10.0.9`, fixada em `dotnet-tools.json` |
| PowerShell | Windows PowerShell `5.1.26100.9168` |
| Banco de testes | instância `MSSQLLocalDB` disponível |
| Browser E2E | Playwright Chromium `1228` disponível |
| Branch base | `hmg` = `origin/hmg` = `0639e303b742e8c784f1b2620ae9523aabb9026c` |
| Divergência inicial | `0/0` contra `origin/hmg` |
| Estado local preservado | somente `.github/upgrades/` não rastreada e fora do escopo |
| Estrutura GitHub | Epic #201 com 30 Sub-issues canônicas; #196 em `In Progress`; #200 ausente da hierarquia |

## 3. Inventário do repositório

| ID | Categoria | Evidência atual | Estado | Sprint proprietária |
|---|---|---|---|---|
| INV-001 | Governança e raiz | 10 arquivos na raiz, incluindo `CLAUDE.md`, solução, contratos de build e manifesto de ferramentas | `VERIFIED` | 30.1 |
| INV-002 | Solução e dependências | 9 projetos: 4 em `src/` e 5 em `tests/`; referências preservam `Domain <- Application <- Infrastructure <- Web` | `VERIFIED` | 30.9 |
| INV-003 | Domain | 47 arquivos rastreados; nenhum uso de EF Core ou ASP.NET Core | `BASELINED` | 30.5 |
| INV-004 | Application | 95 arquivos rastreados; 10 diretórios de Feature; nenhuma referência a Infrastructure, Web ou EF Core | `BASELINED` | 30.6 |
| INV-005 | Infrastructure | 58 arquivos rastreados; SQL Server, serviços técnicos, DI, health checks e configuração | `BASELINED` | 30.7 |
| INV-006 | Persistência e migrations | um `BeeDayDbContext`, uma migration versionada e o model snapshot, em 3 arquivos de migration | `BASELINED` | 30.7 |
| INV-007 | Web e composição | 460 arquivos rastreados; 17 diretórios de Feature; nenhum acesso direto a `BeeDayDbContext` | `BASELINED` | 30.8 |
| INV-008 | Rotas e shell | 54 declarações `@page` encontradas diretamente em componentes Razor | `BASELINED` | 30.17 |
| INV-009 | Fluxos funcionais | Identity/Auth/User, Dashboard, Habits, Tasks, Todos, Projects, Wallets, Experience, Onboarding e páginas públicas identificados | `BASELINED` | 30.4, 30.10–30.18 |
| INV-010 | Testes | 198 arquivos rastreados em 5 projetos; baseline executado contra LocalDB e Chromium | `BASELINED` | 30.24 |
| INV-011 | Workflows | 6 workflows: PR Validation, HMG Deployment, HMG Verification, Release Quality Gate, Production Deployment e Promotion Policy | `BASELINED` | 30.25 |
| INV-012 | Scripts | 12 scripts PowerShell; todos passam pelo parser do PowerShell sem erro sintático | `BASELINED` | 30.25, 30.26 |
| INV-013 | Configuração | contratos raiz, 4 `appsettings`, `launchSettings.json`, `web.config` e 8 tipos auxiliares sob `Infrastructure/Configuration` | `BASELINED` | 30.3, 30.22, 30.25 |
| INV-014 | Documentação | 123 arquivos rastreados; 537 links Markdown relativos verificados, sem link quebrado real | `BASELINED` | 30.28 |
| INV-015 | Design System | 69 arquivos de componentes, 18 arquivos CSS, 5 arquivos JS e 64 assets de ícone sob `design/` | `BASELINED` | 30.19 |
| INV-016 | UX, responsividade e localização | 133 arquivos Razor, 66 CSS e 60 recursos `.resx` no conjunto `src/` + `tests/` | `BASELINED` | 30.20 |
| INV-017 | Segurança e privacidade | autenticação, rate limiting, session version, antiforgery, headers e limites operacionais localizados | `BASELINED` | 30.22 |
| INV-018 | Resiliência e observabilidade | exception handling, health checks, Event Journal, logs e recuperação de deploy localizados | `BASELINED` | 30.23 |
| INV-019 | Performance | pipeline de performance, comportamento assíncrono, CI e documentação de medições localizados | `BASELINED` | 30.21 |
| INV-020 | CI/CD e ambientes | fluxo branch -> HMG -> main -> prd, artifacts, EF bundle, deploy e gates localizados | `BASELINED` | 30.2, 30.3, 30.25 |
| INV-021 | Naming e higiene | superfícies públicas `beeday`, identificadores técnicos `BeeDay.*` e referências históricas localizados | `BASELINED` | 30.26, 30.27 |
| INV-022 | Regressão e encerramento | comandos, suites e estados realistas identificados como gate integrado final | `BASELINED` | 30.29, 30.30 |

Distribuição rastreada no snapshot:

| Área | Arquivos |
|---|---:|
| raiz | 10 |
| `.github/` | 7 |
| `design/` | 64 |
| `docs/` | 123 |
| `scripts/` | 12 |
| `src/` | 660 |
| `tests/` | 198 |
| **Total** | **1.074** |

## 4. Inventário de projetos e dependências

| Projeto | Arquivos rastreados | ProjectReference |
|---|---:|---|
| `BeeDay.Domain` | 47 | nenhum |
| `BeeDay.Application` | 95 | `BeeDay.Domain` |
| `BeeDay.Infrastructure` | 58 | `BeeDay.Application` |
| `BeeDay.Web` | 460 | `BeeDay.Application`, `BeeDay.Domain`, `BeeDay.Infrastructure` |
| `BeeDay.Domain.Tests` | 13 | `BeeDay.Domain` |
| `BeeDay.Application.Tests` | 19 | `BeeDay.Application` |
| `BeeDay.Infrastructure.Tests` | 27 | `BeeDay.Infrastructure` |
| `BeeDay.Web.Tests` | 115 | `BeeDay.Web` |
| `BeeDay.E2E.Tests` | 24 | `BeeDay.Web`, `BeeDay.Infrastructure` |

O acesso de `BeeDay.Web` a Infrastructure continua restrito ao composition root; a busca atual
encontrou zero uso de `BeeDayDbContext` em código Web. Os dois guards arquiteturais automatizados
atuais vivem em Domain.Tests e Application.Tests.

## 5. Mapa de cobertura funcional

| Área observada | Evidência de inventário | Sprint de auditoria |
|---|---|---|
| incidente real de Wallet/HMG | Wallet, transactions, persistence e pipeline de deploy | 30.2 |
| paridade runtime/banco/deploy | configurações, migration, workflows e scripts | 30.3 |
| jornadas realistas | 54 rotas, 17 Features Web e E2E | 30.4, 30.29 |
| regras de domínio | Aggregates, Entities, Value Objects, Events e Experience | 30.5 |
| casos de uso | 10 Features Application, CQRS, validation e contracts | 30.6 |
| persistência | DbContext, repositories, read services, transaction e migration | 30.7 |
| runtime Blazor/DI | `Program.cs`, component tree, services e render modes | 30.8 |
| arquitetura | referências de projeto, tipos internal e boundary tests | 30.9 |
| identidade e conta | Authentication, Identity, Profile, Account e Onboarding | 30.10, 30.11 |
| produtividade | Habits, Tasks, Todos e Projects | 30.12–30.14 |
| finanças | Wallets, Transactions e Tags | 30.15 |
| progressão | Experience, XP, level e rewards | 30.16 |
| navegação e público | shell, routing, Home, Institutional e Experience System | 30.17, 30.18 |
| experiência visual | Design System, UX, a11y, responsive e localization | 30.19, 30.20 |
| qualidades sistêmicas | performance, security, resilience e observability | 30.21–30.23 |
| engenharia e entrega | tests, CI/CD, scripts, naming, docs e regressão | 30.24–30.30 |

## 6. Findings

| ID | Severidade | Evidência confirmada | Estado | Sprint proprietária |
|---|---|---|---|---|
| BD30-F001 | média | `docs/testing/README.md` e `01-testing-strategy.md` registram 1.116 testes (93/73/129/741/80); o baseline atual executou 1.443 (93/85/212/861/192) | `OPEN` | 30.24 |
| BD30-F002 | média | `docs/web/02-routing-and-pages.md` registra 42 rotas; a busca atual encontrou 54 declarações `@page` | `OPEN` | 30.17 |
| BD30-F003 | baixa | `docs/application/README.md` declara 9 Features, mas enumera e o repositório contém 10 diretórios | `OPEN` | 30.6 |
| BD30-F004 | baixa | `docs/architecture/02-solution-structure.md` descreve Solution Items antigos (`docs/ai` e `docs/development`); `BeeDay.slnx` aponta atualmente para `docs/developer/README.md` e outros itens existentes | `OPEN` | 30.28 |
| BD30-F005 | baixa | 27 referências, em 19 arquivos de código/teste, apontam para 7 caminhos de documentação removidos ou movidos | `OPEN` | 30.26 |
| BD30-F006 | alta | o estado versionado de HMG seleciona Resend (`true`) e Development (`false`), enquanto `docs/deployment/01-deployment.md` e `02-runtime-configuration.md` ainda descrevem a seleção inversa; o runbook mais novo distingue corretamente repository state de runtime state | `OPEN` | 30.25 |
| BD30-F007 | média | não existe `.runsettings`, referência a coverlet ou coleta formal de cobertura | `OPEN` | 30.24 |
| BD30-F008 | média | não existe workflow CodeQL nem configuração Dependabot versionada | `OPEN` | 30.22 |
| BD30-F009 | média | existem apenas dois guards automatizados de dependência, cobrindo Domain e Application; Infrastructure e Web não têm guard equivalente | `OPEN` | 30.9 |
| BD30-F010 | baixa | o índice de documentação classifica `authentication/` e `developer/` como reservados e `api/` como não reauditado | `OPEN` | 30.28 |
| BD30-F011 | baixa | `docs/infrastructure/README.md` registra 5 classes Options; o repositório possui 6 Options atuais, além de `EmailProvider` e `EmailProviderSelector` | `OPEN` | 30.7 |
| BD30-F012 | baixa | existe documentação versionada da EPIC 28, mas ela não aparece no índice `docs/README.md` | `OPEN` | 30.28 |

Os achados acima não foram corrigidos na Sprint 30.1 porque pertencem explicitamente às Sprints
proprietárias. Nenhum problema descoberto foi omitido ou expandido silenciosamente para fora do
baseline.

## 7. Baseline de qualidade

O baseline foi executado de forma sequencial para evitar contenção artificial entre LocalDB e
Chromium. Uma primeira chamada interativa de `dotnet test BeeDay.slnx` ultrapassou o limite do
cliente enquanto o E2E ainda estava ativo; não houve falha de teste. O E2E foi então executado em
processo acompanhado, com TRX fora do repositório, e concluiu 192/192.

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.443/1.443 (93 Domain, 85 Application, 212 Infrastructure, 861 Web, 192 E2E) |
| `dotnet test tests/BeeDay.E2E.Tests/BeeDay.E2E.Tests.csproj --no-build` | PASS, 192/192 em 6m04s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.443/1.443 (93 Domain, 85 Application, 212 Infrastructure, 861 Web, 192 E2E) |
| `git diff --check` | PASS |
| `git status` | branch dedicada; `.github/upgrades/` preservada fora do escopo |

## 8. Decisão da Sprint 30.1

A estrutura de inventário e o Ledger foram criados sem alterar comportamento, arquitetura,
contratos públicos, banco, workflows ou Design System. Não houve motivo técnico para modificar
código ou testes nesta Sprint: o trabalho implementado é a evidência baseline e o encaminhamento
auditável dos achados às Sprints corretas.

O readiness local foi atingido. O encerramento da Sprint depende da PR aprovada pelos checks
obrigatórios, merge em `hmg`, atualização do Project para `Done` e fechamento da Issue #196.
