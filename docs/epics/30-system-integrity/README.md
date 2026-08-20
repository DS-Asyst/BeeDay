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
| INV-003 | Domain | 47 arquivos rastreados e auditados integralmente; inventário por artefato em `docs/domain/audit-inventory.md`; guards rejeitam dependências de framework/camadas superiores | `VERIFIED` | 30.5 |
| INV-004 | Application | 95 arquivos rastreados; 10 diretórios de Feature; nenhuma referência a Infrastructure, Web ou EF Core | `VERIFIED` | 30.6 |
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
| BD30-F003 | baixa | `docs/application/README.md` declara 9 Features, mas enumera e o repositório contém 10 diretórios | `FIXED` | 30.6 |
| BD30-F031 | média | 17 dos 27 handlers de Application não tinham teste direto em `tests/BeeDay.Application.Tests` (confirmado por busca de referência), incluindo dois handlers multi-write com transação (`UpdateTodoCommandHandler` no branch cross-Project, `UpdateTransactionCommandHandler`, `DeleteTransactionCommandHandler`) cuja correção de fronteira transacional só era provada por inspeção de código | `FIXED` | 30.6 |
| BD30-F004 | baixa | `docs/architecture/02-solution-structure.md` descreve Solution Items antigos (`docs/ai` e `docs/development`); `BeeDay.slnx` aponta atualmente para `docs/developer/README.md` e outros itens existentes | `OPEN` | 30.28 |
| BD30-F005 | baixa | 27 referências, em 19 arquivos de código/teste, apontam para 7 caminhos de documentação removidos ou movidos | `OPEN` | 30.26 |
| BD30-F006 | alta | o estado versionado de HMG seleciona Resend (`true`) e Development (`false`), enquanto `docs/deployment/01-deployment.md` e `02-runtime-configuration.md` ainda descrevem a seleção inversa; o runbook mais novo distingue corretamente repository state de runtime state | `OPEN` | 30.25 |
| BD30-F007 | média | não existe `.runsettings`, referência a coverlet ou coleta formal de cobertura | `OPEN` | 30.24 |
| BD30-F008 | média | não existe workflow CodeQL nem configuração Dependabot versionada | `OPEN` | 30.22 |
| BD30-F009 | média | existem apenas dois guards automatizados de dependência, cobrindo Domain e Application; Infrastructure e Web não têm guard equivalente | `OPEN` | 30.9 |
| BD30-F010 | baixa | o índice de documentação classifica `authentication/` e `developer/` como reservados e `api/` como não reauditado | `OPEN` | 30.28 |
| BD30-F011 | baixa | `docs/infrastructure/README.md` registra 5 classes Options; o repositório possui 6 Options atuais, além de `EmailProvider` e `EmailProviderSelector` | `OPEN` | 30.7 |
| BD30-F012 | baixa | existe documentação versionada da EPIC 28, mas ela não aparece no índice `docs/README.md` | `OPEN` | 30.28 |
| BD30-F013 | alta | em HMG, validar `TransactionFormModel.Amount` sob `pt-BR` lançava `ArgumentException`/`FormatException` em `RangeAttribute.SetupConversion` ao interpretar o limite textual `"0.01"` pela cultura corrente; a falha ocorria no `EditForm`, antes de MediatR e antes de qualquer `INSERT` | `FIXED` | 30.2 |
| BD30-F014 | baixa | os logs do mesmo período contêm warnings do EF Core sobre MARS/savepoints, mas a cadeia causal confirmada do incidente termina na validação DataAnnotations antes de MediatR/persistência; não há evidência de participação desses warnings nesta falha | `OPEN` | 30.7 |
| BD30-F015 | média | `docs/deployment/04-operations.md` ainda afirmava que não existiam deploy automatizado de HMG nem aplicação de migrations, além de registrar caminhos e fluxo de release obsoletos; os workflows e a execução real provam o fluxo CI artifact -> HMG Deployment -> HMG Verification | `FIXED` | 30.3 |
| BD30-F016 | alta | o rollback de HMG restaura aplicação e configuração do App Pool, mas não desfaz migrations; embora `Deploy-BeeDay.ps1` implemente `-BackupDatabase`, `deploy-hmg.yml` não o habilita e não há evidência versionada de backup SQL externo correlacionado ao deploy | `OPEN` | 30.25 |
| BD30-F017 | média | cada deploy cria backups de aplicação e dados em `C:\Apps\BeeDay-Backups`, mas não existe política versionada de retenção, expurgo ou restore automatizado de uma execução histórica | `OPEN` | 30.25 |
| BD30-F018 | alta | a confirmação de e-mail tem cobertura robusta de Application/Integration para sucesso, token inválido/expirado/replay, reenvio e throttle, mas nenhuma jornada Chromium atravessa um link real até liberar o login | `OPEN` | 30.10 |
| BD30-F019 | alta | não existe E2E de to-do; criação, edição, conclusão, reload e exclusão dentro do workspace são provados apenas parcialmente por componentes, Application e repositories | `OPEN` | 30.13 |
| BD30-F020 | média | o E2E de projeto cria e abre o workspace, mas não prova mutações de to-do nem persistência do workspace após reload | `OPEN` | 30.14 |
| BD30-F021 | média | os E2Es de conta cobrem perfil e idioma, mas não tema, alteração de senha nem recovery visível dos demais saves suportados | `OPEN` | 30.11 |
| BD30-F022 | baixa | dez suítes repetiam seletores e submissão do mesmo formulário de login como arranjo, aumentando drift sem acrescentar evidência funcional | `FIXED` | 30.4 |
| BD30-F023 | alta | doze tipos controlados por factory expunham construtor público implícito e permitiam criar aggregates/entities/values sem qualquer invariante; não havia consumer versionado desses construtores | `FIXED` | 30.5 |
| BD30-F024 | alta | `Activity.AssignOwner`, `Todo.Update` e `Project.AddTodo` permitiam transferência direta de owner/Project, composição cross-user e identidade duplicada na coleção | `FIXED` | 30.5 |
| BD30-F025 | alta | `UserToken` aceitava `createdAtUtc` default e podia ser marcado como usado antes de sua própria criação | `FIXED` | 30.5 |
| BD30-F026 | média | `EmailAddress.Create` aceitava sintaxe de display-name do `MailAddress`, preservando o wrapper completo como identidade em vez do endereço canônico | `FIXED` | 30.5 |
| BD30-F027 | alta | `ExperienceSource` tinha igualdade por referência e `ExperienceEntry.Create` aceitava reward, totais, níveis, enum e timestamp mutuamente inconsistentes | `FIXED` | 30.5 |
| BD30-F028 | média | `Transaction` protegia positividade e escala, mas não o máximo monetário de `999999999999` já exposto pelo contrato público do formulário | `FIXED` | 30.5 |
| BD30-F029 | baixa | comentários de `Profile` ainda justificavam a modelagem pelo adapter JSON já removido | `FIXED` | 30.5 |
| BD30-F030 | alta | `UserExperience.Entries` participa da deduplicação em memória, porém é ignorada no mapping relacional; `ExperienceEntry` é top-level, o repositório não hidrata a coleção e `EnsureExperienceState` não possui consumer | `OPEN` | 30.7 (revalidar impacto em 30.16) |

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

## 9. Sprint 30.2 — Wallet Transaction Runtime Root Cause

### 9.1 Evidência de runtime e cadeia causal

Os logs `stdout` do SERV3WEB reproduziram múltiplas vezes a falha ao submeter uma transação no
Wallet:

```text
System.ArgumentException: 0.01 is not a valid value for Decimal
  ---> System.FormatException: The input string '0.01' was not in a correct format.
  at System.ComponentModel.DecimalConverter.FromString(...)
  at System.ComponentModel.DataAnnotations.RangeAttribute.SetupConversion()
  at System.ComponentModel.DataAnnotations.RangeAttribute.IsValid(...)
```

O código em `TransactionFormModel.Amount` usava
`[Range(typeof(decimal), "0.01", "999999999999")]`. O overload recebe limites textuais e, sem uma
política explícita, tentava converter `"0.01"` pela cultura corrente. Em `pt-BR`, que espera vírgula
como separador decimal, a conversão do limite falhava dentro do pipeline de validação do
`EditForm`/`InputNumber`. Portanto, `CreateTransactionCommand` não chegava ao MediatR e nenhum
`INSERT` era emitido para o SQL Server. Constraints, migration, FK e persistência não participam da
causa confirmada.

### 9.2 Correção mínima

O limite de negócio foi preservado em `0.01m`. A anotação existente passou a declarar
`ParseLimitsInInvariantCulture = true`, fazendo apenas os limites textuais do atributo serem
interpretados de forma determinística. Não houve mudança em Domain, Application, Infrastructure,
schema, migration, contrato público ou Design System.

Os warnings EF Core sobre MARS/savepoints observados no mesmo conjunto de logs foram registrados
separadamente como `BD30-F014`. Correlação temporal não estabelece causalidade, e a evidência atual
prova que esta exceção acontece antes da fronteira de persistência.

### 9.3 Regressão e comportamento funcional

- teste determinístico executa a validação de `Amount` sob `en-US` e `pt-BR`, aceitando o mínimo
  `0.01m` e rejeitando `0m`;
- antes da correção, o caso `pt-BR` reproduziu a mesma `ArgumentException` com
  `RangeAttribute.SetupConversion`, enquanto `en-US` passou;
- após a correção, os dois casos passaram;
- E2E autenticado muda a preferência real para português, cria tag e transação de receita no mínimo
  `0.01`, edita para `0.02`, exclui a transação e confirma saldo/estado interativo após cada ação;
- o diálogo fecha normalmente e `aria-busy` retorna a `false`, comprovando que a exceção não encerra
  mais o circuito Blazor nos fluxos de criação e edição.

### 9.4 Quality gates locais

| Comando | Resultado observado |
|---|---|
| teste unitário direcionado antes da correção | reprodução determinística: `en-US` PASS; `pt-BR` FAIL com a exceção de runtime |
| teste unitário direcionado após a correção | PASS, 2/2 |
| E2E Wallet direcionado | PASS, 1/1 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 após correção mecânica de line endings somente nos três arquivos C# alterados |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| primeira execução integral Debug | interrompida deliberadamente para o refresh obrigatório de governança; nenhuma falha de teste observada |
| `dotnet test BeeDay.slnx` após o refresh | PASS, 1.446/1.446 (93 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E) |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.446/1.446 (93 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E) |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `git status` | branch dedicada; quatro arquivos da Sprint; governança local, `CLAUDE.md` modificado e `.github/upgrades/` preservados fora do escopo e do staging |

## 10. Sprint 30.3 — HMG Runtime, Database & Deployment Parity

### 10.1 Inventário auditado

| Inventário | Estado na Sprint 30.3 | Evidência e limite |
|---|---|---|
| INV-006 — persistência e migrations | `VERIFIED` | migration, snapshot, EF bundle, ausência de model drift e estado `up to date` em HMG; auditoria interna da persistência permanece em 30.7 |
| INV-011 — workflows | `VERIFIED` | cadeia HMG de `ci.yml` para `deploy-hmg.yml` e `verify-hmg.yml`; auditoria ampla de CI/CD permanece em 30.25 |
| INV-013 — configuração | `VERIFIED` | nomes dos oito secrets obrigatórios presentes, payload de dez variáveis aceito pelo App Pool e startup saudável; valores permaneceram secretos |
| INV-018 — resiliência/observabilidade | `VERIFIED` | readiness SQL, smoke e contrato de rollback inspecionados; auditoria sistêmica permanece em 30.23 |
| INV-020 — CI/CD e ambientes | `VERIFIED` | proveniência code -> artifact -> HMG comprovada por SHAs, run IDs, artifacts e digests |

`VERIFIED` nesta matriz significa que o contrato de paridade pertencente à 30.3 foi inspecionado e
confirmado. Não encerra as auditorias profundas já atribuídas às Sprints indicadas.

### 10.2 Proveniência confirmada

O merge de HMG `9b87ff2c05d9715dc7026879b59c866bccc2c372` foi associado pelo workflow ao
PR #266 e ao `head_sha` validado `069ad8465a684c5e5c5e6641cd97928a598ce437`. A execução CI
`32385656296` produziu `beeday-publish` e `beeday-migrations`; os digests observados no download
foram, respectivamente, `45fd08cbe22792421eb8aa12a42dfd3cee0bae859775520ff41c78fa65a9b616` e
`79e1af27d8f0dc7870a58bb82ddee5f8fe9152e59a73b8781e905714d7316d7c`.

O HMG Deployment `32390796350` implantou esses artifacts e publicou
`beeday-hmg-deployment-info`. O HMG Verification `32391001814` consumiu esse registro e confirmou o
mesmo `sourceSha`, sem depender do SHA diferente do merge.

### 10.3 Banco, configuração e runtime

- o repositório contém somente `20260803111144_InitialCreate` e o model snapshot correspondente;
- `dotnet ef migrations has-pending-model-changes` confirmou ausência de deriva local;
- o bundle executado contra HMG informou que nenhuma migration precisava ser aplicada e que o
  banco já estava atualizado;
- a API do GitHub confirmou a presença dos oito nomes de secrets exigidos no Environment
  `homologation`, sem expor valores;
- o deploy configurou dez variáveis permitidas no App Pool;
- `BeeDay-HMG` e `BeeDay-Web-AppPool` convergiram com `exitCode=0` em `STOP`, `CONFIGURE` e `START`;
- readiness, incluindo `SqlServerHealthCheck`, passou com HTTP 200; o smoke `/login` também passou
  com HTTP 200 e conteúdo esperado.

Não houve consulta a dados de negócio, leitura de secret, alteração manual do banco/IIS ou ação em
produção.

### 10.4 Rollback, findings e correção

O deploy criou backups reais de aplicação e dados antes da promoção. As regressões de script
confirmaram que falhas chegam ao rollback, restauram a configuração do App Pool, reiniciam IIS,
executam health check após restore e ainda encerram o job com erro.

O runbook operacional obsoleto foi reescrito conforme o comportamento atual, fechando
`BD30-F015`. Os limites já comprovados foram explicitados: rollback não restaura `Data`, não desfaz
migrations, o workflow de HMG não habilita o backup SQL já suportado pelo script e não existe
retenção/restore histórico automatizado. `BD30-F016` e `BD30-F017` foram atribuídos à Sprint 30.25,
que é proprietária do endurecimento amplo de CI/CD e deployment. `BD30-F006` também permanece nessa
Sprint para reconciliar a documentação/configuração de providers de e-mail.

Não houve mudança de código, arquitetura, contrato público, schema, workflow ou estado do ambiente.
Testes novos não se aplicam a esta correção documental; as suites existentes de deploy foram
executadas pelo pipeline observado.

### 10.5 Quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.446/1.446 (93 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E); E2E 6m25s |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.446/1.446 (93 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E); E2E 6m27s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

## 11. Sprint 30.4 — Functional Journey Matrix & E2E Foundation

### 11.1 Inventário auditado

| Inventário | Estado na Sprint 30.4 | Evidência e limite |
|---|---|---|
| INV-004 — páginas e rotas | `MAPPED` | jornadas públicas, identidade, Daily, Wallet, conta e recovery mapeadas; a reconciliação do inventário de 54 rotas permanece em 30.17 por BD30-F002 |
| INV-005 — componentes Blazor | `MAPPED` | componentes de auth, onboarding, dashboard, editores, workspace, Wallet e conta ligados às jornadas correspondentes; auditorias funcionais permanecem nas Sprints 30.10–30.20 |
| INV-010 — testes automatizados | `VERIFIED` | evidência Domain/Application/Infrastructure/Web/E2E correlacionada por jornada e gaps explicitamente atribuídos |
| INV-015 — autenticação e autorização | `MAPPED` | cadastro, confirmação, login, autorização, cultura de sessão e logout correlacionados entre navegador e Integration; auditoria profunda permanece em 30.10 |
| INV-016 — localização | `MAPPED` | en-US/pt-BR, cookies, preferências e fluxos público/autenticado correlacionados; auditoria profunda permanece em 30.20 |

`MAPPED` significa que a jornada e suas provas foram localizadas, não que a área funcional inteira
foi aprovada antecipadamente. `VERIFIED` em INV-010 significa que a fundação pertencente à Sprint
30.4 foi inspecionada e que ausência de evidência material virou finding com owner.

### 11.2 Matriz e findings

`docs/testing/03-functional-journey-matrix.md` registra as quatorze jornadas pedidas, incluindo
happy path, validação/recovery, persistência, autorização/navegação, nível da evidência e plano de
verificação. Visitante, cadastro, login, onboarding, Daily, hábitos, tarefas, Wallet, localização e
logout possuem prova E2E representativa combinada com testes nas camadas proprietárias.

Quatro lacunas materiais foram abertas, sem implementação em massa fora do escopo:

- `BD30-F018`: confirmação de e-mail não atravessa um link real em Chromium (30.10);
- `BD30-F019`: to-do não possui jornada E2E (30.13);
- `BD30-F020`: workspace de projeto não prova mutações de to-do/reload (30.14);
- `BD30-F021`: conta não possui E2E representativo para tema, senha e recovery dos demais saves
  (30.11).

### 11.3 Fundação E2E

O arranjo de login repetido em dez suítes foi consolidado em `SubmitLoginAsync`, fechando
`BD30-F022`. O helper reutiliza `GotoAsync` e somente abre, preenche e submete o formulário. Ele não
semeia usuário, não escolhe destino e não contém assertions; URLs, navegação e resultados continuam
visíveis em cada teste. `AccountLifecycleTests` permaneceu inline porque ali o próprio login é o
comportamento sob teste.

Não houve mudança de comportamento de produto, regra de negócio, arquitetura, contrato público,
schema, migration ou Design System.

### 11.4 Quality gates locais

| Comando | Resultado observado |
|---|---|
| E2E focado das jornadas migradas | PASS, 3/3 (`HabitAndTaskTests`, `WalletTests`, `SettingsLocalizationTests`) |
| primeira verificação de `dotnet format BeeDay.slnx --verify-no-changes` | FAIL somente por `ENDOFLINE` nas linhas C# alteradas pelo patch; `dotnet format ... whitespace --include` foi aplicado exclusivamente aos onze arquivos E2E da Sprint |
| `dotnet format BeeDay.slnx --verify-no-changes` após a correção mecânica | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.446/1.446 (93 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E); E2E 6m31s |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.446/1.446 (93 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E); E2E 6m32s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| links relativos dos documentos alterados | PASS, nenhum destino ausente |
| `git status --short` | branch dedicada; 14 arquivos da Sprint; governança local, `CLAUDE.md` modificado e `.github/upgrades/` preservados fora do escopo |

## 12. Sprint 30.5 — Domain Complete Audit

### 12.1 Inventário e fronteiras

Os 47 arquivos rastreados em `src/BeeDay.Domain` foram lidos e classificados individualmente em
`docs/domain/audit-inventory.md`. `INV-003` passa a `VERIFIED`: o projeto não referencia outra
camada nem package de framework, e o guard automatizado agora rejeita ASP.NET Core, EF Core,
Application, Infrastructure, Web e serialização JSON.

Factories, mutações, enums, nullability, datas, dinheiro, igualdade e estados derivados foram
confrontados com todos os testes de Domain e com os consumers necessários para validar contratos.
Construtores de materialização permanecem privados e as factories públicas são o caminho de
criação. A compatibilidade do mapping EF foi verificada separadamente pelos testes de DbContext.

### 12.2 Invariantes corrigidas

- ownership de `Activity` não pode ser transferido; `Project` rejeita To-Do cross-user/duplicado e
  a mudança direta de `Todo.ProjectId` foi fechada, preservando a movimentação pelo aggregate root;
- tokens exigem creation time real e somente são usáveis dentro da janela criação-expiração;
- display-name de e-mail não pode entrar como identidade canônica;
- `ExperienceSource` possui igualdade por valor e `ExperienceEntry` revalida reward, total,
  níveis, enum, overflow e timestamp;
- `Transaction` protege o máximo monetário já assumido pelo formulário público;
- `SessionVersion` não sofre wraparound silencioso;
- comentários de `Profile` passaram a refletir a fronteira atual, sem citar o adapter JSON removido.

Os doze tipos controlados por factory não possuem mais construtor público implícito. A busca por
consumers confirmou que nenhum código versionado dependia desse caminho inválido, e os testes de
Domain/EF cobrem criação e materialização válidas.

### 12.3 Finding cross-layer encaminhado

`BD30-F030` permanece `OPEN`: `UserExperience.Entries` é usada para histórico/deduplicação em
memória, mas o mapping relacional atual ignora a coleção, `ExperienceEntry` é top-level, o
repositório não hidrata o histórico e `EnsureExperienceState` não é chamado. A correção envolve
responsabilidade de persistência e pertence à Sprint 30.7; a Sprint 30.16 deve revalidar o impacto
funcional sobre rewards. Nenhuma alteração de mapping ou migration foi feita na 30.5.

### 12.4 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| testes novos antes da correção | reprodução determinística: FAIL, 22/22 estados inválidos aceitos pelo código anterior |
| testes novos após a correção | PASS, 24/24 |
| `dotnet test tests/BeeDay.Domain.Tests/BeeDay.Domain.Tests.csproj --no-restore` | PASS, 117/117 |
| testes direcionados de mapping do DbContext | PASS, 35/35 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.470/1.470 (117 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E); E2E 6m46s |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.470/1.470 (117 Domain, 85 Application, 212 Infrastructure, 863 Web, 193 E2E); E2E 6m43s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 12.5 Continuidade e entrega

A Sprint foi retomada de um handoff Codex -> Claude Code com a implementação, os testes e o
Ledger já escritos até a lacuna dos gates finais; esta sessão revisou o diff completo linha a
linha (entidades, Experience, EmailAddress, testes e documentação), confirmou por busca de
consumers que `Todo.Update`/`Project.AddTodo`/`ExperienceSource` não quebram nenhum chamador
versionado de Application/Infrastructure, e então executou os gates acima do zero. `CLAUDE.md`
permaneceu modificado na árvore de trabalho antes desta Sprint (reescrita de governança alheia ao
escopo do Domain audit) e foi deliberadamente mantido fora do commit da Sprint 30.5, junto com os
diretórios locais `.claude/`, `.agents/`, `.codex/`, o arquivo `AGENTS.md` e `.github/upgrades/`.

## 13. Sprint 30.6 — Application / CQRS Complete Audit

### 13.1 Inventário e fronteiras

Os 11 arquivos de Handlers (1.387 linhas) e os 9 contratos em `Common/Contracts/` (8 repositórios +
`IUnitOfWork`) foram auditados integralmente contra os critérios do Issue #203: autorização/
ownership, cancellation token, acoplamento a Infrastructure/UI, fronteira transacional, duplicação/
tamanho de handler, semântica de exceção e contrato de resposta. `INV-004` passa a `VERIFIED`.

Resultado: zero violação confirmada em qualquer uma das sete categorias. Toda operação por Id
resolve `userId` via `CurrentUserGuard.RequireUserId` e escopa a busca pelo repositório
correspondente; nenhum `CancellationToken.None` aparece em `src/BeeDay.Application`; nenhum arquivo
referencia `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.*` ou tipo de Infrastructure/Web;
todo handler multi-write usa `IUnitOfWork` dentro de `try { Begin → ... → Commit } finally {
DisposeAsync }`; nenhum handler contém `catch` (exceções de Domain/Application propagam sem
wrapper); e toda Response é um record read-only mapeado explicitamente, sem Aggregate vazando para
fora da camada.

### 13.2 Achados

- `BD30-F003` fechado: `docs/application/README.md` contava "9 Features" enquanto já listava e o
  repositório contém 10 diretórios (`Authentication`, `Dashboard`, `Habits`, `Identity`,
  `Ordering`, `Projects`, `Tasks`, `Todos`, `Users`, `Wallets`). Corrigido nas duas ocorrências.
- `BD30-F031` (nova, média, fechada nesta Sprint): 17 dos 27 handlers não tinham teste direto —
  `CreateProjectCommandHandler`, `UpdateProjectCommandHandler`, `DeleteProjectCommandHandler`,
  `CreateTodoCommandHandler`, `UpdateTodoCommandHandler`, `DeleteTodoCommandHandler`,
  `UpdateHabitCommandHandler`, `DeleteHabitCommandHandler`, `RegisterHabitNegativeCommandHandler`,
  `UpdateTaskCommandHandler`, `DeleteTaskCommandHandler`, `UpdateTransactionCommandHandler`,
  `DeleteTransactionCommandHandler`, `GetTransactionByIdQueryHandler`, `GetWalletTagsQueryHandler`,
  `GetDashboardQueryHandler`, `UpdateCurrentUserAvatarCommandHandler`. Confirmado por busca de
  referência de cada nome de classe em `tests/BeeDay.Application.Tests` antes de escrever qualquer
  teste. Isso incluía dois dos handlers multi-write com `IUnitOfWork` (`UpdateTodoCommandHandler`
  no branch cross-Project, `UpdateTransactionCommandHandler`, `DeleteTransactionCommandHandler`)
  cuja correção transacional só era comprovada por inspeção de código, não por teste determinístico.
- Duas observações menores sem ação: `AddAsync`/`RemoveAsync` dos 8 repositórios escopam ownership
  pelo `UserId` já atribuído ao Aggregate (não por parâmetro `userId` explícito, ao contrário de
  `Get`/`Update`/`List`) — padrão uniforme e seguro, todo call site atribui o owner antes de chamar;
  e `ITransactionRepository` escopa por `walletId`/`walletTagId`, não por `userId` diretamente —
  seguro porque todo call site resolve esse id a partir de uma busca já escopada por `userId`
  (`WalletLookup.RequireCurrentWalletAsync`/`RequireOwnedTagAsync`). Nenhuma das duas é um defeito;
  registradas aqui para rastreabilidade, sem finding próprio.
- Confirmado (não novo): a duplicação estrutural entre `IWalletReadService.TransactionQueryFilter`
  e `Features.Wallets.Queries.GetTransactionsQuery` já é documentada em
  `docs/application/04-contracts.md` como decisão intencional; e as ~19 referências XML doc a
  caminhos de `docs/architecture/*` removidos (`Common/Contracts/*.cs` e arquivos adjacentes) são a
  mesma família já coberta por `BD30-F005`, de propriedade da Sprint 30.26 — nenhuma das duas foi
  alterada nesta Sprint.

### 13.3 Implementação

Nenhuma mudança de comportamento em `src/BeeDay.Application`. A remediação de `BD30-F031` foi
inteiramente em testes novos:

- `tests/BeeDay.Application.Tests/ProjectHandlersTests.cs` (novo) — Create/Update/Delete de
  Project, incluindo rejeição cross-user.
- `tests/BeeDay.Application.Tests/TodoHandlersTests.cs` (novo) — Create/Update (mesmo Project e
  movimentação entre Projects)/Delete de Todo, incluindo rejeição cross-user.
- `tests/BeeDay.Application.Tests/HabitTaskManagementHandlersTests.cs` (novo) — Update/Delete de
  Habit, `RegisterHabitNegative`, Update/Delete de Task, incluindo rejeição cross-user.
- `tests/BeeDay.Application.Tests/DashboardHandlerTests.cs` (novo) — `GetDashboardQueryHandler`
  encaminha o `userId` autenticado e devolve a resposta do read service sem alteração.
- `tests/BeeDay.Application.Tests/WalletHandlersTests.cs` (estendido) — Update/Delete de
  Transaction (incluindo rejeição cross-user), `GetTransactionByIdQueryHandler` e
  `GetWalletTagsQueryHandler`.
- `tests/BeeDay.Application.Tests/UserAccountHandlersTests.cs` (estendido) —
  `UpdateCurrentUserAvatarCommandHandler`.

`docs/application/README.md` corrigido (9 → 10 Features, duas ocorrências).

### 13.4 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Application.Tests/BeeDay.Application.Tests.csproj` | PASS, 113/113 (85 preexistentes + 28 novos) |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.498/1.498 (117 Domain, 113 Application, 212 Infrastructure, 863 Web, 193 E2E) |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.498/1.498 (mesma distribuição) |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 13.5 Continuidade e entrega

Reconhecimento amplo (11 handlers, 9 contratos) foi delegado a um subagente Explore somente-leitura
para acelerar a cobertura; cada achado relatado foi verificado independentemente nesta sessão antes
de qualquer ação — a lacuna de cobertura de teste foi confirmada por busca de referência própria
(zero resultado para os 17 nomes de handler em `tests/`), e as assinaturas de método/registro de
cada Handler tocado foram lidas diretamente do código antes de escrever qualquer teste novo.
