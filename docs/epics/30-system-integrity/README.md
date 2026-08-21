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
| INV-005 | Infrastructure | 58 arquivos rastreados; SQL Server, serviços técnicos, DI, health checks e configuração | `VERIFIED` | 30.7 |
| INV-006 | Persistência e migrations | um `BeeDayDbContext`, uma migration versionada e o model snapshot, em 3 arquivos de migration | `VERIFIED` | 30.7 |
| INV-007 | Web e composição | 460 arquivos rastreados; 17 diretórios de Feature; nenhum acesso direto a `BeeDayDbContext` | `VERIFIED` | 30.8 |
| INV-008 | Rotas e shell | 54 declarações `@page` em 52 arquivos, reconfirmadas byte-a-byte na Sprint 30.17 contra `docs/web/02-routing-and-pages.md` §3 — zero rota indocumentada, zero rota obsoleta documentada, zero rota duplicada | `VERIFIED` | 30.17 |
| INV-009 | Fluxos funcionais | Identity/Auth/User, Dashboard, Habits, Tasks, Todos, Projects, Wallets, Experience, Onboarding e páginas públicas identificados | `MAPPED` (Identity/Auth `VERIFIED` na 30.10) | 30.4, 30.10–30.18 |
| INV-010 | Testes | 198 arquivos rastreados em 5 projetos; baseline executado contra LocalDB e Chromium | `BASELINED` | 30.24 |
| INV-011 | Workflows | 6 workflows: PR Validation, HMG Deployment, HMG Verification, Release Quality Gate, Production Deployment e Promotion Policy. **Reverificado na Sprint 30.25**: os 6 continuam corretos frente a `docs/deployment/*.md`, exceto um comentário desatualizado em `deploy-hmg.yml` (parte de `BD30-F006`, corrigido). 7º workflow novo adicionado nesta Sprint: `codeql.yml` (`BD30-F008`) | `VERIFIED` | 30.25 |
| INV-012 | Scripts | 12 scripts PowerShell; todos passam pelo parser do PowerShell sem erro sintático. **Reverificado na Sprint 30.25**: os 12 continuam parseando sem erro; 2 novos adicionados (`Clear-BeeDayBackups.ps1`, `scripts/tests/Test-ClearBeeDayBackups.ps1`, `BD30-F017`), total agora 14 | `VERIFIED` | 30.25, 30.26 |
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
| BD30-F001 | média | `docs/testing/README.md` e `01-testing-strategy.md` registram 1.116 testes (93/73/129/741/80); o baseline atual executou 1.443 (93/85/212/861/192). **Corrigido na Sprint 30.24**: os dois documentos reconciliados para o total real ao final da Sprint 30.23 (1.554: 121/119/216/879/219), confirmado por execução direta de `dotnet test BeeDay.slnx` (Debug e Release) | `FIXED` | 30.24 |
| BD30-F002 | média | `docs/web/02-routing-and-pages.md` registra 42 rotas; a busca atual encontrou 54 declarações `@page`. **Corrigido na Sprint 30.17**: a tabela do §3 já estava correta (54 rotas/52 arquivos) desde a Sprint 29.4 — a divergência estava só em 3 menções de prosa (§1, rodapé do §3, §11) nunca atualizadas desde antes da Sprint 25.17, agora corrigidas para o valor real | `FIXED` | 30.17 |
| BD30-F003 | baixa | `docs/application/README.md` declara 9 Features, mas enumera e o repositório contém 10 diretórios | `FIXED` | 30.6 |
| BD30-F031 | média | 17 dos 27 handlers de Application não tinham teste direto em `tests/BeeDay.Application.Tests` (confirmado por busca de referência), incluindo dois handlers multi-write com transação (`UpdateTodoCommandHandler` no branch cross-Project, `UpdateTransactionCommandHandler`, `DeleteTransactionCommandHandler`) cuja correção de fronteira transacional só era provada por inspeção de código | `FIXED` | 30.6 |
| BD30-F004 | baixa | `docs/architecture/02-solution-structure.md` descreve Solution Items antigos (`docs/ai` e `docs/development`); `BeeDay.slnx` aponta atualmente para `docs/developer/README.md` e outros itens existentes | `OPEN` | 30.28 |
| BD30-F005 | baixa | 27 referências, em 19 arquivos de código/teste, apontam para 7 caminhos de documentação removidos ou movidos | `OPEN` | 30.26 |
| BD30-F006 | alta | o estado versionado de HMG seleciona Resend (`true`) e Development (`false`), enquanto `docs/deployment/01-deployment.md` e `02-runtime-configuration.md` ainda descrevem a seleção inversa; o runbook mais novo distingue corretamente repository state de runtime state. **Corrigido na Sprint 30.25**: os dois documentos corrigidos (§6/nota do incidente e tabela §3/§6.7 respectivamente, preservando o texto histórico do incidente original — só a afirmação de estado "hoje" foi corrigida, não a narrativa do que aconteceu na época). Descoberta adicional durante a correção: a mesma afirmação invertida também existia como comentário de código em `deploy-hmg.yml` e duas vezes em `scripts/Deploy-BeeDay.ps1` — corrigidos também, embora não afetassem comportamento (a lógica do script já era condicionada aos valores reais dos parâmetros, não ao texto do comentário). `docs/infrastructure/06-transactional-email.md` já estava correto, não precisou de correção | `FIXED` | 30.25 |
| BD30-F007 | média | não existe `.runsettings`, referência a coverlet ou coleta formal de cobertura. **Corrigido na Sprint 30.24**: `coverlet.collector` adicionado aos 5 projetos de teste (`Directory.Packages.props` + `PackageReference` em cada `.csproj`, mesmo padrão de `PrivateAssets`/`IncludeAssets` já usado para `xunit.runner.visualstudio`) — `dotnet test --collect:"XPlat Code Coverage"` agora produz `coverage.cobertura.xml` por projeto, verificado por execução real. Nenhum threshold/gate de cobertura adicionado deliberadamente — decisão de política (qual % é aceitável, se deveria bloquear PR) fora da autoridade de uma auditoria de engenharia de teste; o limite explícito da Sprint também instruía não otimizar para cobertura de linha como objetivo primário | `FIXED` | 30.24 |
| BD30-F008 | média | não existe workflow CodeQL nem configuração Dependabot versionada. **Reverificado na Sprint 30.22**: metade corrigida — `.github/dependabot.yml` adicionado (ecossistemas `nuget` e `github-actions`, semanal, sem impacto operacional — não cria check obrigatório, não roda código, apenas configura o serviço nativo do GitHub para abrir PRs de atualização). CodeQL deliberadamente não adicionado nesta Sprint: é um novo workflow do GitHub Actions que consome minutos de CI a cada push/PR e tipicamente se torna um check obrigatório uma vez configurado — mudança de maior impacto operacional na pipeline de CI/CD, mais adequada à Sprint dedicada a CI/CD do que a uma correção pontual de segurança. Reatribuída (só a parte de CodeQL) para 30.25. **Corrigido na Sprint 30.25**: novo `.github/workflows/codeql.yml` (`csharp`, `build-mode: autobuild`, PR para `hmg` + semanal), deliberadamente não obrigatório — ver §32.5 para o resultado real do primeiro run (disparado pelo próprio PR desta Sprint) | `FIXED` | 30.25 |
| BD30-F009 | média | existem apenas dois guards automatizados de dependência, cobrindo Domain e Application; Infrastructure e Web não têm guard equivalente | `FIXED` | 30.9 |
| BD30-F010 | baixa | o índice de documentação classifica `authentication/` e `developer/` como reservados e `api/` como não reauditado | `OPEN` | 30.28 |
| BD30-F011 | baixa | `docs/infrastructure/README.md` registra 5 classes Options; o repositório possui 6 Options atuais, além de `EmailProvider` e `EmailProviderSelector` | `OPEN` | 30.7 |
| BD30-F012 | baixa | existe documentação versionada da EPIC 28, mas ela não aparece no índice `docs/README.md` | `OPEN` | 30.28 |
| BD30-F013 | alta | em HMG, validar `TransactionFormModel.Amount` sob `pt-BR` lançava `ArgumentException`/`FormatException` em `RangeAttribute.SetupConversion` ao interpretar o limite textual `"0.01"` pela cultura corrente; a falha ocorria no `EditForm`, antes de MediatR e antes de qualquer `INSERT` | `FIXED` | 30.2 |
| BD30-F014 | baixa | os logs do mesmo período contêm warnings do EF Core sobre MARS/savepoints, mas a cadeia causal confirmada do incidente termina na validação DataAnnotations antes de MediatR/persistência; não há evidência de participação desses warnings nesta falha | `OPEN` | 30.7 |
| BD30-F015 | média | `docs/deployment/04-operations.md` ainda afirmava que não existiam deploy automatizado de HMG nem aplicação de migrations, além de registrar caminhos e fluxo de release obsoletos; os workflows e a execução real provam o fluxo CI artifact -> HMG Deployment -> HMG Verification | `FIXED` | 30.3 |
| BD30-F016 | alta | o rollback de HMG restaura aplicação e configuração do App Pool, mas não desfaz migrations; embora `Deploy-BeeDay.ps1` implemente `-BackupDatabase`, `deploy-hmg.yml` não o habilita e não há evidência versionada de backup SQL externo correlacionado ao deploy. **Reverificado na Sprint 30.25**: confirmado por leitura direta do bloco de rollback completo (`Deploy-BeeDay.ps1`) — a função `Backup-BeeDayDatabase` existe, funciona (`Invoke-Sqlcmd ... BACKUP DATABASE`), mas é genuinamente inalcançável de qualquer workflow hoje; o rollback nunca reverte migration alguma, só restaura arquivos de aplicação. **Não corrigido deliberadamente**: habilitar `-BackupDatabase` mudaria o comportamento do próximo deploy real de HMG contra o SQL Server real (permissão de escrita da conta de serviço não verificada, espaço em disco não verificado) — mutação de ambiente fora da autoridade desta auditoria, e dependeria de `BD30-F017` já estar resolvido para não acumular backups SQL sem limite. Decisão do proprietário | `OPEN` | decisão do proprietário |
| BD30-F017 | média | cada deploy cria backups de aplicação e dados em `C:\Apps\BeeDay-Backups`, mas não existe política versionada de retenção, expurgo ou restore automatizado de uma execução histórica. **Corrigido na Sprint 30.25**: novo `scripts/Clear-BeeDayBackups.ps1` — mesmo padrão autônomo/idempotente de `Clear-BeeDayStdoutLogs.ps1` (não vinculado a nenhum deploy/rollback crítico, não agendado automaticamente), com um piso de segurança adicional (`-MinimumToKeep`, default 3) que nunca expurga os N pares mais recentes mesmo que todos estejam além de `-RetentionDays` — justificado porque, até `BD30-F016` ser resolvido, esse é o único material de rollback que o processo de deploy possui. Cobertura de regressão nova (`scripts/tests/Test-ClearBeeDayBackups.ps1`, 15 asserções) provada por execução real, incluindo o piso de segurança e `-WhatIf`; adicionada ao mesmo preflight de `deploy-hmg.yml` que já valida as demais suítes de regressão do deploy. Restore automatizado de um backup histórico específico permanece manual — fora do escopo desta correção pontual de retenção | `FIXED` | 30.25 |
| BD30-F018 | alta | a confirmação de e-mail tem cobertura robusta de Application/Integration para sucesso, token inválido/expirado/replay, reenvio e throttle, mas nenhuma jornada Chromium atravessa um link real até liberar o login | `FIXED` | 30.10 |
| BD30-F019 | alta | não existia E2E de to-do; criação, edição, conclusão, reload e exclusão eram provados apenas parcialmente por componentes, Application e repositories. Corrigido: `tests/BeeDay.E2E.Tests/TodoLifecycleTests.cs` (novo) prova, via Chromium real, criar um To-Do dentro de um Project, alternar conclusão, editar (com persistência após reload) e excluir (com confirmação) | `FIXED` | 30.13 |
| BD30-F020 | média | o E2E de projeto criava e abria o workspace, mas não provava mutações de to-do nem persistência do workspace após reload. A Sprint 30.13 fechou a primeira metade (`TodoLifecycleTests.cs` prova mutações de Todo dentro do workspace, com persistência verificada no board após reload) mas nunca reabria o próprio workspace pós-reload. Corrigido: `tests/BeeDay.E2E.Tests/ProjectLifecycleTests.cs` (novo) prova a barra de progresso e a lista de To-Dos do workspace sobrevivendo a um reload real de página, além de editar e excluir um Project, ambos antes sem qualquer cobertura E2E | `FIXED` | 30.14 |
| BD30-F021 | média | os E2Es de conta cobrem perfil e idioma, mas não tema, alteração de senha nem recovery visível dos demais saves suportados | `OPEN` | 30.11 |
| BD30-F022 | baixa | dez suítes repetiam seletores e submissão do mesmo formulário de login como arranjo, aumentando drift sem acrescentar evidência funcional | `FIXED` | 30.4 |
| BD30-F023 | alta | doze tipos controlados por factory expunham construtor público implícito e permitiam criar aggregates/entities/values sem qualquer invariante; não havia consumer versionado desses construtores | `FIXED` | 30.5 |
| BD30-F024 | alta | `Activity.AssignOwner`, `Todo.Update` e `Project.AddTodo` permitiam transferência direta de owner/Project, composição cross-user e identidade duplicada na coleção | `FIXED` | 30.5 |
| BD30-F025 | alta | `UserToken` aceitava `createdAtUtc` default e podia ser marcado como usado antes de sua própria criação | `FIXED` | 30.5 |
| BD30-F026 | média | `EmailAddress.Create` aceitava sintaxe de display-name do `MailAddress`, preservando o wrapper completo como identidade em vez do endereço canônico | `FIXED` | 30.5 |
| BD30-F027 | alta | `ExperienceSource` tinha igualdade por referência e `ExperienceEntry.Create` aceitava reward, totais, níveis, enum e timestamp mutuamente inconsistentes | `FIXED` | 30.5 |
| BD30-F028 | média | `Transaction` protegia positividade e escala, mas não o máximo monetário de `999999999999` já exposto pelo contrato público do formulário | `FIXED` | 30.5 |
| BD30-F029 | baixa | comentários de `Profile` ainda justificavam a modelagem pelo adapter JSON já removido | `FIXED` | 30.5 |
| BD30-F030 | alta | `UserExperience.Entries` participa da deduplicação em memória, porém era ignorada no mapping relacional; `ExperienceEntry` é top-level e nada jamais adicionava novas entries ao `DbSet` — confirmado por teste real contra LocalDB: nenhuma linha era persistida, e a mesma fonte podia ser recompensada indefinidamente (recompletar um Todo/Task/Project já concluído antes) | `FIXED` | 30.7 (revalidar impacto em 30.16) |
| BD30-F032 | baixa | `EfHabitRepository.AddAsync`/`EfProjectRepository.AddAsync`/`EfRecurringTaskRepository.AddAsync`/`EfProjectRepository.AddTodoAsync` calculam a próxima `Position` via `MaxAsync` seguido de um insert separado, sem índice/constraint único em `(UserId, Position)` (ou `(ProjectId, Position)` para Todo) — duas inserções concorrentes do mesmo usuário podem computar o mesmo `maxPosition` e persistir ordinais duplicados; não há perda de dado, apenas dessincronia cosmética de ordenação, autocorrigível no próximo reorder. **Reverificado na Sprint 30.25**: confirmado ainda preciso — os 4 índices (`IX_Habits_User_Position`, `IX_Projects_User_Position`, `IX_RecurringTasks_User_Position`, `IX_Todos_Project_Position`) existem, nenhum com `.IsUnique()`, ao contrário do idioma já estabelecido no mesmo código-base para colunas genuinamente únicas (`WalletTagConfiguration`, `UserConfiguration`, `UserTokenConfiguration` chamam `.IsUnique()` explicitamente). **Não corrigido deliberadamente**: adicionar a constraint é uma migration de schema que falharia (ou, pior, teria efeito indefinido) se qualquer duplicata de `Position` já existir hoje em dados reais de HMG/produção — verificar isso exigiria acesso de leitura a um banco de HMG/produção que esta auditoria não tem nem está autorizada a usar. Decisão do proprietário: alguém com acesso de leitura autorizado ao banco de HMG precisa verificar ausência de duplicatas antes que essa migration possa ser considerada segura | `OPEN` | decisão do proprietário |
| BD30-F033 | baixa | `EfWalletReadService.ApplyOrdering` ordena `Transaction` por `Description`/`Amount`/`CreatedAtUtc` sem índice cobrindo esses campos (apenas `IX_Transactions_Wallet_Date` existe) — SQL Server ordena em tempdb após o seek por `WalletId`; impacto real baixo dado o volume típico de transações por usuário em um app financeiro pessoal. **Decisão da Sprint 30.21** (Sprint proprietária): reverificado, premissa inalterada. Risco aceito explicitamente — adicionar 3 índices novos tem custo real de escrita (todo insert/update de Transaction passa a manter mais índices) sem evidência de consulta lenta real ou de volume que justifique o trade-off; adicionar índice especulativo contraria o limite explícito desta Sprint contra otimização especulativa. Reavaliar se o volume de transações por usuário mudar materialmente | `ACCEPTED RISK` | 30.21 |
| BD30-F034 | alta | histórico de `ExperienceEntry` não era persistido antes da correção da Sprint 30.7 (`BD30-F030`); alternar conclusão/reabertura repetida de Todo/Task/Project podia conceder XP duplicado sem limite antes da correção. Existência e magnitude de inflação histórica em HMG/produção **não quantificadas** por esta Sprint — nenhuma consulta ou mutação de banco de HMG/produção foi executada. As linhas de `ExperienceEntry` persistidas antes da correção podem ser insuficientes para reconstruir `TotalExperience` corretamente de forma determinística (o histórico anterior à correção nunca existiu). Nenhuma mutação de banco está autorizada por este achado; nenhum reset/recálculo arbitrário é permitido. **Investigação concluída na Sprint 30.16** (§23.2): as 7 perguntas encaminhadas foram todas respondidas com evidência — nenhuma pode ser resolvida com os dados/ferramentas atuais. Reconstrução determinística não é possível (o próprio escalar `TotalExperience` já incorpora qualquer inflação histórica, indistinguível de XP legítimo, porque as entries que provariam a diferença nunca existiram); correção automatizada não seria segura; reconciliação manual não é viável sem elas; e não existe no repositório nenhum mecanismo seguro e somente-leitura para quantificar o raio de impacto em HMG/produção. Prosseguir exigiria duas decisões do proprietário fora da autoridade desta auditoria: construir uma capacidade de leitura segura contra HMG/produção, e decidir se algum esforço de reconciliação vale a pena dado que a reconstrução completa é matematicamente impossível | `OPEN` | decisão do proprietário |
| BD30-F035 | média | `BeeDayWebService` (20 métodos) e os call sites diretos de `ISender.Send` em `Wallet.razor` e nas páginas de Identity/Account/Onboarding nunca propagavam um `CancellationToken` real — toda chamada usava implicitamente `CancellationToken.None`, então navegar para longe ou fechar o circuito Blazor Server nunca cancelava uma mutação/query em andamento no servidor | `FIXED` | 30.8 |
| BD30-F036 | baixa | `EmailConfirmationSent.razor`/`ResendConfirmation.razor`: `StartCountdown()` reatribui `_timer`/`_cts` sem descartar a instância anterior se chamado uma segunda vez antes do `Dispose()` do componente — hoje inalcançável em uso normal (o botão fica desabilitado enquanto `_secondsRemaining > 0`), portanto latente, não explorável | `OPEN` | 30.10 |
| BD30-F037 | baixa | polimento de UX não-bloqueante: cards individuais do Dashboard (Habit/Task/Todo/Project) não têm `Disabled` vinculado a `State.IsBusy` (só o overlay global `BeeDayLoading` reflete ocupado — a proteção contra double-submit é real, aplicada em `DashboardState.ExecuteAsync`, mas o clique num segundo card fica sem feedback visual imediato); e `Wallet.razor.RefreshAfterMutationAsync` não chama `StateHasChanged()` uma segunda vez após zerar `_highlightBalance`, então o destaque visual do saldo pode não sumir até outro render não relacionado ocorrer | `OPEN` | 30.20 |
| BD30-F038 | média | `Login.razor` mantinha uma implementação própria de `IsLocalPath` para sanitizar `ReturnUrl`, distinta e mais fraca que a canônica de `LoginDestinationResolver` — só rejeitava `//`, não a variante `/\` que navegadores normalizam para URL absoluta; a decisão de redirecionamento real em `POST /auth/login` já usava a canônica completa, então não era explorável via o próprio fluxo de login, mas era uma fronteira de segurança duplicada e incompleta | `FIXED` | 30.10 |
| BD30-F039 | alta | `ConfirmEmail.razor` enviava `ConfirmEmailCommand` em `OnInitializedAsync`, que roda duas vezes sob o `@rendermode="InteractiveServer"` global (`<Routes>` em `App.razor`) — uma vez no prerender estático, outra na reconexão interativa. A primeira chamada confirmava o e-mail corretamente; a segunda, idêntica, era corretamente rejeitada pela proteção contra replay de token — mas essa segunda rejeição é o estado final que o navegador do usuário real exibe, então todo usuário real via "Link já utilizado" no primeiro clique legítimo em um link de confirmação real. Só detectável por um teste de navegador real (Chromium) — nenhum teste unitário/bUnit/integração via `HttpClient` exercita as duas passagens de render do Blazor Server | `FIXED` | 30.10 |
| BD30-F040 | baixa | `User.SetActive` (que corretamente invalida sessões ao desativar) não é chamado por nenhum Command/handler alcançável — só por testes. **Determinação da Sprint 30.11** (auditoria funcional completa de Profile/Onboarding/Account/Settings, incluindo inspeção direta de `Account.razor` e as três seções de Settings): classificado como **fluxo de produto ausente**, não código morto — o método de Domain está correto, testado e o guard de `OnValidatePrincipal` (`!user.IsActive`) funciona; simplesmente não existe hoje nenhuma entrada de produto (autoatendimento ou administrativa) que o alcance. Decidir se/como construir essa entrada é uma decisão de política de produto fora da autoridade desta auditoria — não inventada aqui | `OPEN` | decisão do proprietário |
| BD30-F041 | baixa | `MemoryIdentityRequestThrottle` e `LoginRateLimiterFactory` (`PartitionedRateLimiter`) são ambos em memória, por instância de processo — corretos para o único servidor IIS de HMG hoje, mas não compartilhados entre instâncias; uma futura implantação horizontalmente escalada (PRD em Azure, ainda não provisionada) contornaria o limite de taxa distribuindo requisições entre instâncias. **Reverificado na Sprint 30.22**: premissa inalterada, nenhuma evidência de escalonamento horizontal introduzida desde a Sprint 30.10. A condição que tornaria isso relevante (PRD provisionado com múltiplas instâncias) ainda não existe — reatribuído para decisão do proprietário, a ser revisitado quando/se essa condição mudar | `OPEN` | decisão do proprietário |
| BD30-F042 | média | confiabilidade da suíte E2E em Debug: três execuções completas de `dotnet test BeeDay.slnx` durante a Sprint 30.10 produziram, respectivamente, 6/194, 1/194 e 1/194 falhas — nunca o mesmo teste duas vezes, sempre `TimeoutException` de navegação (`GotoAsync`)/screenshot, nunca um teste de Identity/Auth desta Sprint. A execução `--configuration Release` subsequente passou 194/194 sem qualquer falha. **Causa raiz confirmada na Sprint 30.24** (com evidência real, não hipótese): `dotnet test BeeDay.slnx` como comando único contra a solução inteira executa os hosts de teste de múltiplos projetos concorrentemente — provado observando a própria saída já capturada nesta EPIC (Sprint 30.23): o resultado final de `Infrastructure.Tests` (34s) só é impresso depois do resultado de `Web.Tests` (23s), mesmo `Infrastructure.Tests` tendo começado a executar muito antes — prova de que `Infrastructure.Tests` (que cria/derruba bancos LocalDB reais) ainda rodava enquanto `Web.Tests`/`E2E.Tests` também rodavam. Nenhum dos dois workflows de CI jamais reproduz essa condição (`ci.yml` não roda LocalDB/browser; `release-quality-gate.yml` roda um projeto por vez, sempre Release). Documentado em `docs/testing/01-testing-strategy.md` §7 com o contrato de repetibilidade: uma falha `TimeoutException` nesse cenário específico é consistente com este padrão conhecido, não motivo automático para `CHANGE-CAUSED` — reexecutar isolado ou em Release antes de classificar | `FIXED` | 30.24 |
| BD30-F043 | baixa | `ProfileCreationState.cs` (state class `AddScoped`, mesmo padrão de ciclo de vida de `DashboardState`) nunca propagava `CancellationToken` em suas 4 chamadas a `BeeDayWebService` — a varredura da Sprint 30.8 (`BD30-F035`) só buscou `*.razor`/`*.razor.cs`, e esta é uma classe `.cs` simples, fora do glob | `FIXED` | 30.11 |
| BD30-F044 | alta | `UpdateCurrentUserAccountCommandHandler` permitia alterar o e-mail da conta sem reverificar a senha atual e sem resetar `IsEmailConfirmed` — uma sessão sequestrada (cookie roubado, XSS) bastava para trocar silenciosamente o e-mail para um endereço controlado pelo atacante, que `RequestPasswordResetCommandHandler` então tratava como já confirmado, habilitando um fluxo completo de esqueci-minha-senha e bloqueio do dono legítimo. Primitivo de account takeover real | `FIXED` | 30.11 |
| BD30-F045 | média | `Tutorial.razor.NextAsync`, no slide final, chamava `Store.CompleteOnboardingAsync()` sem `try/catch` e o componente não injetava `ToastService` — uma falha (rede, 5xx transitório, sessão expirada) se propagava como exceção não tratada no circuito Blazor, sem nenhum feedback ao usuário, ao contrário de todo outro caminho de salvamento do app | `FIXED` | 30.11 |
| BD30-F046 | baixa | `docs/web/04-feature-components.md` descrevia `Tutorial.razor` navegando para `/daily` ao concluir o onboarding; o código real (e `LoginDestinationResolver.Resolve`) sempre navegou para `/profile` | `FIXED` | 30.11 |
| BD30-F047 | média | `AuthenticatedCultureSynchronizer.SynchronizeAtLoginAsync`: um cookie `BeeDay.Culture` desatualizado em um segundo dispositivo/navegador, ao logar, silenciosamente sobrescreve uma alteração de idioma deliberada feita em Settings — comportamento documentado e intencional (cookie explícito sempre vence naquela sessão), mas conflita com o critério de aceite "preferência de idioma permanece consistente entre sessões autenticadas". Decisão de produto necessária: cookie deveria ceder à conta no login, ou cookies não deveriam sobreviver a uma troca de idioma na conta | `OPEN` | 30.20 |
| BD30-F048 | baixa | `Program.cs` grava `ClaimTypes.Name`/`ClaimTypes.Email` no cookie `BeeDay.Auth` (até 14 dias, "remember me") no login, mas nunca os atualiza após uma edição de Nome/E-mail em Account — hoje inofensivo (`grep` confirma que nada no código lê essas duas claims de volta), mas é PII potencialmente desatualizada sentada num cookie de longa duração. **Corrigido na Sprint 30.22**: as duas claims removidas na emissão do cookie em vez de mantidas sincronizadas — nenhum código em `src/` lê `ClaimTypes.Name`/`ClaimTypes.Email` de volta, incluindo a propriedade implícita `ClaimsPrincipal.Identity.Name` (confirmado por busca dedicada, zero usos), então minimização de dados (não guardar PII que nada consome) é a correção mais simples e segura, em vez de adicionar lógica para reemitir o cookie a cada edição de Account. Suíte completa de Web.Tests (875/875) e E2E de login/conta/home autenticada (25/25) confirmam nenhuma regressão | `FIXED` | 30.22 |
| BD30-F049 | média | nenhum teste baseado em viewport real (Playwright) cobria `/profile/create` Etapa 2 (apelido), `/account`/`/settings` (as 3 seções) ou `/onboarding/tutorial` — `LoginExperienceTests` só provava a Etapa 1 do cadastro; `AccountLifecycleTests`/`SettingsLocalizationTests` nunca chamavam `SetViewportSizeAsync` | `FIXED` | 30.11 |
| BD30-F050 | média | `HabitResetCounter` (Daily/Weekly/Monthly) está totalmente cabeado — coluna no banco, validação de Domain, editor de UI, documentação — mas nada no código jamais reseta `PositiveCount`/`NegativeCount` com base em tempo decorrido, fronteira de calendário ou job agendado; `RegisterPositive`/`RegisterNegative` só incrementam, para sempre. O campo é hoje decoração de UI sem efeito comportamental. Definir a semântica real de reset (quando? em qual fuso/cultura? via leitura ou job?) é uma decisão de produto, não inventada por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F051 | média | `EfUserRepository.UpdateAsync` recarrega **todas** as `ExperienceEntry` de um usuário a cada chamada — não só em registros de Habit, mas em qualquer mutação de User (confirmação de e-mail, redefinição de senha, troca de nome/e-mail). Habit é a única fonte de XP deliberadamente isenta de deduplicação (`UserExperience.cs`, por design — cada clique deve premiar XP independentemente), então é a única cujo volume de `ExperienceEntry` cresce sem limite por usuário ativo; nenhuma estratégia de arquivamento/paginação existe. Lista de Habits em `/daily` também não é paginada/virtualizada — nenhum teste cria mais de 2 Habits para o mesmo usuário em toda a suíte, então isso nunca foi exercitado em escala. **Reverificado na Sprint 30.21** (Sprint proprietária): consulta ainda usa o índice `IX_ExperienceEntries_User_Time` (seek, não scan) — o problema não é lentidão de busca, é volume de linhas retornadas e hidratadas sem limite. Magnitude real em produção **não determinável** por esta auditoria (mesma restrição já estabelecida pela `BD30-F034` — nenhum acesso de leitura a HMG/produção). Uma correção real exigiria trocar o mecanismo de dedup de "carregar toda a coleção" para uma consulta pontual por chave (`SourceType`/`SourceId`/`RewardType`) — uma mudança de arquitetura na fronteira Domain/Infrastructure do sistema de XP, já auditada e corrigida múltiplas vezes nesta EPIC (`BD30-F030`), com risco real de reintroduzir um bug de correção em um subsistema sensível, sem evidência de que o problema já é real hoje. Reatribuído para decisão do proprietário | `OPEN` | decisão do proprietário |
| BD30-F052 | baixa | `ActivityAttribute` (Strength/Dexterity/Intelligence/Vitality) é persistido, validado e round-tripa corretamente, mas não existe controle de UI para defini-lo em nenhum dos 4 editores de atividade (Habit/Task/Todo/Project) — confirmado por busca por `ActivityAttribute` em todo `*.razor` de `src/BeeDay.Web`, zero resultados fora de sintaxe `@attributes` não relacionada. Toda atividade criada pelo produto hoje tem `Attribute = null` para sempre; um valor já existente (via API/dados diretos) sobrevive a uma edição intacto, só não pode ser definido pela UI. Gap cross-cutting, não específico de Habit. **Reverificado na Sprint 30.19**: ainda preciso, sem mudança. Construir 4 novos controles de formulário com plumbing completo (Application, resx, testes) é trabalho de feature genuíno, não consolidação — fora do limite explícito desta Sprint ("audit/consolidation Sprint, not a redesign"). Reatribuído de "30.19" para decisão do proprietário: a auditoria já confirmou o gap três vezes (30.11, 30.15/similar, 30.19) sem informação nova; o próximo passo é priorização de produto, não mais investigação | `OPEN` | decisão do proprietário |
| BD30-F053 | baixa | `EfHabitRepository.RemoveAsync` (e o mesmo padrão em `EfTaskRepository`/`EfTodoRepository`/`EfProjectRepository` conforme aplicável) busca a linha só por `Id`, sem reverificar `UserId` — depende inteiramente do único call site (`DeleteHabitCommandHandler`) já ter verificado posse via `HabitLookup.RequireExistsAsync` antes. Seguro hoje (único call site confirmado, corretamente guardado), mas é uma lacuna de defesa em profundidade: o método do repositório não é seguro por posse isoladamente, então um futuro chamador direto que pule a pré-verificação poderia excluir silenciosamente o Habit de outro usuário. **Reverificado e corrigido na Sprint 30.22**: escopo confirmado por inspeção direta como idêntico em `EfHabitRepository`, `EfRecurringTaskRepository` (Task), `EfProjectRepository`, `EfTransactionRepository` e `EfWalletTagRepository` — 5 repositórios, não só Habit/Task. `Todo` não faz parte (a exclusão de Todo já é escopada por `UserId` via o Project pai). Corrigido nos 5: o predicado de re-busca em `RemoveAsync` agora também exige `UserId`/`WalletId` igual ao da entidade já verificada recebida pelo chamador — nenhuma mudança de comportamento no caminho legítimo (a entidade recebida já é a correta em todo call site real, confirmado pelos 216 testes de Infrastructure passando sem alteração), só torna um chamador futuro que pule a verificação de posse falhar alto (exceção) em vez de excluir silenciosamente a linha de outro usuário. Não foi viável escrever um teste automatizado do caso "objeto de domínio malformado com UserId adulterado" sem reflexão — a API pública do Domain estruturalmente não permite construir essa combinação inválida; correção validada por inspeção de código (uma cláusula de predicado adicionada) e pela suíte de testes existente permanecendo verde | `FIXED` | 30.22 |
| BD30-F054 | alta | `RecurringTask.Repeat` (Daily/Weekly/Monthly) está totalmente cabeado — Domain, persistência, editor de UI, documentação — mas `RecurringTask` não sobrescreve `ToggleCompletion()` (herda a implementação padrão de `Activity`, um simples flip de booleano); nenhum código no repositório jamais reabre uma Task recorrente com base em fronteira de calendário. É a mesma classe de lacuna que `BD30-F050` (Habit), confirmada de forma independente para Task. Efeito colateral agravante: como a dedução de XP por origem é permanente e correta por design para Task (ao contrário de Habit), uma Task "Diária" completada uma vez **nunca mais pode gerar XP**, mesmo desmarcando e marcando de novo manualmente — os dois mecanismos, cada um correto isoladamente, se combinam para anular o propósito de gamificação de uma Task "recorrente". Definir a semântica real de reabertura é uma decisão de produto, não inventada por esta auditoria — mesma decisão pendente de `BD30-F050`, possivelmente a mesma decisão para os dois achados | `OPEN` | decisão do proprietário |
| BD30-F055 | baixa | `Todo.DueDate` é persistido e exibido como texto formatado, mas não tem nenhum efeito funcional: nenhum indicador visual de atraso existe em `src/` (`grep` por "overdue"/"atrasad" não encontra nada), a ordenação da lista usa exclusivamente `Position` (drag-and-drop), não `DueDate`, e `Todo` não sobrescreve `ToggleCompletion()` — completar um To-Do atrasado se comporta identicamente a completar um no prazo. Nenhum bug de fuso/cultura na conversão de data em si (`BeeDayWebService.ToDateOnly` é direto; `DueDateInput_StaysIsoFormatted_RegardlessOfCulture` já prova o campo permanece ISO sob pt-BR) | `OPEN` | 30.20 |
| BD30-F056 | média | `Project.Archived` é persistido, validado e round-tripa corretamente (`EfProjectRepositoryTests` prova a persistência), mas `ProjectEditorModal.razor` não tem nenhum controle de UI para defini-lo — pior que `BD30-F050`/`BD30-F054` (que ao menos têm um seletor visível, só sem efeito), este campo é inteiramente inalcançável pela UI. Mesmo que fosse setado diretamente no banco, nada a jusante o trata de forma diferente: `EfDashboardReadService` não filtra por ele, `DashboardState.FilteredProjects`/`ProjectContextOptions` não o excluem, o board Ativo/Concluído usa `Status` (não `Archived`), e o reorder de Projects compartilha uma única sequência de `Position` sem particionar por `Archived`. Decisão de produto necessária: construir a UI de arquivamento + filtragem, ou remover o campo morto | `OPEN` | decisão do proprietário |
| BD30-F057 | baixa | `DashboardState.DeleteCurrentEditorItemAsync` (compartilhado por Habit/Task/Todo/Project) toca a animação de remoção do card (`RemovingItemId`, ~170ms) **antes** de emitir a requisição de exclusão ao servidor — se a exclusão subsequente falhar (rede, conflito), `RemovingItemId` já foi limpo e `ReloadAsync()` nunca é alcançado (o catch só mostra um toast de erro), então o card reaparece no estado normal após já ter "desaparecido" visualmente um instante antes, ao lado de um toast de erro. Padrão cross-cutting pré-existente, não introduzido nem específico desta Sprint | `OPEN` | 30.20 |
| BD30-F058 | média | Sort por Amount/Description em Wallet estava totalmente cabeado Application→Infrastructure→testes (`Wallet.razor.ResolveSort`, `TransactionSortField`, `EfWalletReadService.ApplyOrdering`), mas o `<select>` renderizado só oferecia as duas opções de data — as outras 4 opções só eram alcançáveis setando o parâmetro `Sort` diretamente em teste, nunca por um usuário real. **Corrigido nesta Sprint**: 4 novas `<option>` adicionadas a `WalletFilters.razor` (mesmos valores já suportados pelo backend). Já o filtro de faixa de valor (`GetTransactionsQuery.MinimumAmount`/`MaximumAmount`, validado e testado em Application/Infrastructure) continua com **zero superfície de UI** — nenhum input, nenhuma propriedade de estado, nada em `WalletFilters.razor`. Diferente de `BD30-F050`/`BD30-F054`/`BD30-F056`, não é uma questão de semântica de produto ambígua (ordenar por valor e filtrar por faixa de valor têm significado óbvio e não-controverso) — é trabalho de engenharia represado, não decisão de produto. **Reverificado na Sprint 30.19**: `WalletFilters.razor` ainda não tem nenhum input Min/Max amount; `grep` por `MinimumAmount`/`MaximumAmount` em `src/BeeDay.Web` continua zero. Construir essa UI é trabalho de feature genuíno (novo input, novo estado, nova validação client-side), fora do limite desta Sprint de consolidação. Reatribuído para decisão do proprietário — não por ambiguidade de produto (o significado é claro), mas porque não há mais Sprint de auditoria dedicada a construir features novas no restante do roteiro da EPIC 30 | `OPEN` | decisão do proprietário |
| BD30-F059 | alta | Cards de `WalletTag`/`Transaction` (`WalletTagManager.razor`, `TransactionList.razor`) perdem toda interatividade de clique/teclado para itens adicionados a uma lista já populada dentro da mesma sessão de circuito Blazor Server — confirmado reproduzível para o primeiro Tag criado (lista vazia→1), um segundo Tag criado logo em seguida, e uma segunda Transaction criada logo em seguida; imune a espera explícita (até 1s), a `Force: true` (bypassa verificações de actionability do Playwright, descartando interceptação/overlay como causa), e independente de clique vs. `Enter` via teclado. Um `GotoAsync` real (reload completo de página) sempre restaura a interatividade. **Causa raiz não identificada** — `@key` foi adicionado a ambos os `@foreach` como bom-senso defensivo (Blazor best practice já ausente), mas comprovadamente **não** resolveu o sintoma nos testes que o reproduziram; `DialogFocusScope`/`beeday-dialog-focus.js` foi inspecionado por completo sem revelar um bug óbvio. Cards de Transaction/Habit/Task/Todo/Project em Sprints anteriores desta EPIC nunca expuseram isso porque toda sequência de duas interações em um mesmo teste já continha um `GotoAsync` de reload no meio (para provar persistência) — não porque o padrão estivesse imune. Workaround confirmado (reload) aplicado nos dois novos testes E2E desta Sprint que o encontraram. **Hipótese testada e refutada na Sprint 30.24**: `WalletTagManager`/`TransactionCard` são os únicos consumidores de `BeeDayCard` que passam `@onclick`/`@onkeydown` como atributos splatados via `AdditionalAttributes`/`@attributes` (todo outro card interativo do produto — `ActivityCard`, `HabitCard` — liga esses handlers diretamente, sem splat); essa era uma hipótese estrutural plausível e testável. Experimento real conduzido: `BeeDayCard` ganhou `OnClick`/`OnKeyDown` tipados, os dois consumidores migrados para usá-los em vez do splat, e os dois testes E2E que reproduzem o defeito foram executados repetidamente com o workaround de reload removido. Resultado: **a hipótese não se sustentou** — `CreateExpenseTransaction_DecreasesBalanceCorrectly` (reabrir a segunda Transaction) continuou falhando de forma consistente e idêntica (mesmo sintoma, mesma linha) mesmo com a correção aplicada. A mudança foi revertida por completo (`git checkout --`) antes de qualquer commit, exatamente pelo mesmo princípio já estabelecido na Sprint 30.19 (`BD30-F075`): não enviar uma correção não comprovada. Causa raiz genuína permanece desconhecida; o splat de atributos via `AdditionalAttributes` está descartado como explicação (refutado por evidência direta, não apenas não confirmado) | `OPEN` | 30.26 |
| BD30-F060 | baixa | não existia cobertura E2E provando que completar Task/Todo/Project concede XP visivelmente (só Habit tinha, desde a Sprint 30.12) e o modal de level-up (`BeeDayFeedbackModal`) nunca havia sido exercitado ponta a ponta (só bUnit) — nenhum teste anterior disparava uma execução real de handler + publicação real de domain event + render real do Blazor Server através de um level-up de fato. **Corrigido nesta Sprint**: `CompleteTask_UpdatesXp` prova visibilidade de XP para Task via navegador real; `CompleteTask_AtALevelBoundary_ShowsTheLevelUpModalExactlyOnce` semeia o usuário 5 XP abaixo do limite documentado/testado de Level 2 (100 XP) via novo parâmetro `initialExperience` de `E2EWebApplicationFactory.SeedUserAsync` (usa `User.AddExperience`, não-dedup, só para arranjo de teste) e prova o modal aparecendo exatamente uma vez, com os níveis corretos, e não reaparecendo após reload. Residual não corrigido, de baixo valor: Todo/Project não têm o mesmo teste de visibilidade de XP especificamente — aceito porque os três dividem exatamente o mesmo `ToggleTodoCommandHandler`/`ExecuteExperienceOperationAsync`, já provado correto nas camadas Application/Infrastructure (§23.1, item D.8) | `FIXED` | 30.16 |
| BD30-F061 | baixa | não existe nenhuma UI (nem endpoint de leitura em Application) que exponha o histórico de `ExperienceEntry` ao usuário ou a um admin — a única superfície visível é o toast efêmero de level-up (`BeeDayFeedbackStore`, escopo de circuito, últimos 3 itens, nunca lê do banco). O trabalho de persistência corrigido pela `BD30-F030` (Sprint 30.7) não tem, hoje, nenhum consumidor além dessa lógica de dedup interna. Construir uma tela de histórico é uma decisão de produto, não inventada por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F062 | baixa | excluir um Habit/Task/Todo/Project já recompensado não revoga nem ajusta o XP concedido — comportamento deliberado por design (`ExperienceEntryConfiguration.cs` documenta em comentário: sem FK para a origem, `ExperienceEntries` é histórico append-only, cópia do que aconteceu, não referência viva). A decisão está corretamente implementada e comentada no código, mas não está ratificada em nenhum lugar de `docs/` (`docs/domain/business-rules.md` só declara a curva/nível como determinística, nada sobre revogação por exclusão) | `OPEN` | 30.28 |
| BD30-F063 | alta | `app.MapRazorComponents<App>()` registra um endpoint só para cada `@page` descoberto — não existe fallback catch-all implícito. Qualquer requisição para uma URL sem `@page` correspondente (erro de digitação, link externo obsoleto, favorito antigo) terminava o roteamento do ASP.NET Core com um 404 vazio (`Content-Length: 0`, sem HTML algum), sem nunca alcançar o `NotFoundPage` do `Router` do Blazor — confirmado empiricamente via `curl` contra o servidor real, contrastado com `/login` (200, HTML completo) e `/not-found` (200, mesma rota funcionando quando acessada diretamente). Nenhum teste anterior (E2E ou integração) exercitava uma URL genuinamente inexistente contra o pipeline HTTP real — só bUnit, que renderiza `NotFound.razor` diretamente, sem passar pelo roteamento. **Corrigido nesta Sprint**: `app.UseStatusCodePagesWithReExecute("/not-found")` reexecuta a requisição contra a rota `/not-found` real (já existente, estilizada, localizada) sempre que a resposta termina em um status 4xx/5xx sem corpo já escrito — não interfere com nenhuma resposta que já tenha corpo (JSON de `GlobalExceptionHandler`) nem com redirects de autenticação (302, com `Location`). Verificado com `curl` antes/depois da correção e com a suíte completa de `AuthorizationIntegrationTests`/`AntiforgeryIntegrationTests`/`ProblemDetailsIntegrationTests` (892/892 em `BeeDay.Web.Tests`, nenhuma regressão) | `FIXED` | 30.17 |
| BD30-F064 | baixa | `EditorialFooter.razor` linka para `/buy-me-a-coffee` em todas as 12 páginas institucionais; a rota não existe (contrato de rota já pré-anunciado como fora de escopo em `docs/web/02-routing-and-pages.md` desde a Sprint 29.4). Antes da `BD30-F063`, clicar mostrava uma página completamente em branco; após a correção desta Sprint, mostra a página real de Not Found (estilizada, localizada) — o link continua não-funcional, mas já não produz mais uma tela em branco. Construir a página ou remover o link é decisão de produto, não inventada por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F065 | média | `/Error` (`Pages/Error.razor`) existe mas nunca é produzida por nenhum caminho de código — `GlobalExceptionHandler` sempre emite JSON `ProblemDetails` para qualquer exceção na pipeline HTTP, nunca redireciona para `/Error`. Separadamente, nenhum `<ErrorBoundary>` existe em toda a árvore de componentes (`grep` por `ErrorBoundary` em `src/BeeDay.Web` = zero resultados) — uma exceção não tratada dentro do render/event-handler de qualquer página interativa (ex.: um clique em `Wallet.razor`) encerra o circuito SignalR sem nenhuma tela de recuperação além do `ReconnectModal` genérico tentando reconectar a um circuito que já não existe. Ambos são gaps de arquitetura de resiliência a erros, não defeitos pontuais de rota — encaminhados à Sprint 30.23 (Resilience & Observability) em vez de corrigidos aqui, dado o risco de uma mudança especulativa na pipeline global de exceções sem o escopo dedicado que o tema merece. **Corrigido na Sprint 30.23**: novo `BeeDayErrorBoundary.razor` (composição sobre `LoggingErrorBoundary : ErrorBoundary`, que sobrescreve o extension point documentado `OnErrorAsync` para logar com `WebEventIds.CircuitError`) envolve `@Body` nos 4 layouts (`MainLayout`, `PublicLayout`, `OnboardingLayout`, `EditorialLayout`), renderizando um fallback de marca (`BeeDayEmptyState` + botão Recarregar) em vez de encerrar o circuito sem recuperação. `/Error` (órfã, nunca produzida por nenhum caminho real) permanece fora de escopo — decisão de mantê-la ou removê-la é de produto, não desta correção pontual de resiliência | `FIXED` | 30.23 |
| BD30-F090 | média | `ConcurrencyConflictException` é `PersistenceException` (herança) mas não tinha nenhum `case` próprio em `GlobalExceptionHandler.Map` — caía no `case PersistenceException`, mapeado para 503 "Persistence unavailable... Try again shortly". Uma resposta enganosa: um conflito de concorrência otimista significa que o registro *mudou* sob o usuário, não que o armazenamento está indisponível — repetir a mesma escrita obsoleta falha de novo, sempre; o usuário precisa recarregar o registro primeiro, não só tentar de novo. **Corrigido nesta Sprint**: novo `case ConcurrencyConflictException`, posicionado antes do `case PersistenceException` mais amplo (a ordem importa — pattern matching de tipo por herança usa o primeiro `case` compatível), mapeado para 409 Conflict com mensagem acionável ("This record was changed by another operation. Reload the page and try again."). Não é alcançável pela suíte de integração HTTP existente pelo mesmo motivo já documentado em `ProblemDetailsIntegrationTests` (só ocorre dentro de uma chamada MediatR feita por um componente Razor sobre o circuito SignalR, nunca a partir de uma requisição HTTP crua) — coberto em vez disso por teste unitário direto contra `GlobalExceptionHandler.Map` (novo `GlobalExceptionHandlerTests.cs`), habilitado por um novo `InternalsVisibleTo` de `BeeDay.Web` para `BeeDay.Web.Tests`, mesmo padrão já usado por `BeeDay.Infrastructure.csproj` | `FIXED` | 30.23 |
| BD30-F091 | baixa | `CorrelationIdMiddleware` só existe na pipeline HTTP ASP.NET Core (`app.UseMiddleware<CorrelationIdMiddleware>()`) e seu `logger.BeginScope` só está ativo durante `await next(context)` daquela requisição HTTP específica — confirmado por leitura direta do middleware e por `builder.Logging.AddJsonConsole(options => options.IncludeScopes = true)` em `Program.cs` (a infraestrutura de log já grava scopes quando presentes). Em Blazor Server, isso cobre a requisição HTTP inicial (primeiro render/negotiate), mas cada interação subsequente do usuário (clique disparando um comando MediatR) roda sobre o circuito SignalR já estabelecido, fora de qualquer nova invocação desse middleware — então `LoggingBehavior`/`LoggingErrorBoundary` e qualquer outro log emitido durante uma mutação disparada por circuito **não carrega `CorrelationId`** nos logs de produção, ao contrário de uma falha capturada por `GlobalExceptionHandler` na pipeline HTTP. Construir uma correlação com escopo de circuito (ex.: um `CircuitHandler` gerando um id por circuito, propagado via DI escopado ao MediatR) é uma mudança de arquitetura de observability genuína, fora do limite desta auditoria — não implementada especulativamente | `OPEN` | decisão do proprietário |
| BD30-F092 | baixa | nenhuma configuração explícita de `CommandTimeout` existe em toda a base (`grep` por `CommandTimeout` em `src/` = zero resultados) — toda consulta/gravação SQL Server depende do default implícito do ADO.NET/EF Core (30s). Definir um valor explícito é uma decisão de política (qual latência é aceitável para o workload real do BeeDay?), não inventada por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F093 | baixa | `LoggingBehavior.Handle` (Application) loga sucesso/falha de todo request MediatR sem nenhum `EventId` — inconsistente com a convenção já estabelecida em `WebEventIds.cs` (Web) para logs estruturados pesquisáveis por id. Application não tem hoje nenhuma convenção equivalente de `EventId`; criar uma agora para um único call site seria uma nova abstração sem uso comprovado além dele — encaminhado como hygiene, não corrigido especulativamente | `OPEN` | 30.26 |
| BD30-F094 | média | `E2ETestBase.cs` já captura screenshot + trace do Playwright em toda falha de teste E2E (salvos em `e2e-artifacts/` sob o próprio output de build do projeto de teste), mas `release-quality-gate.yml` (único workflow que roda `BeeDay.E2E.Tests` — `ci.yml` não) só faz upload de `${{ runner.temp }}\TestResults` (arquivos `.trx`), nunca do diretório `e2e-artifacts` — um E2E falhando em CI perde o screenshot/trace assim que o runner é destruído, exatamente o cenário em que esses artefatos mais fazem falta (não é possível reproduzir localmente sem eles). **Corrigido nesta Sprint**: novo step `Upload E2E failure artifacts` em `release-quality-gate.yml`, `if: always()`, `if-no-files-found: ignore` (testes passando não produzem nenhum arquivo, por design) | `FIXED` | 30.24 |
| BD30-F066 | baixa | não existia teste E2E/integração provando o ciclo completo `returnUrl` (hit anônimo em rota protegida → redirect para `/login?returnUrl=...` → login → volta exatamente para a página originalmente pedida) nem uma URL genuinamente inexistente atingindo o `NotFoundPage` do Router real (só bUnit, que renderiza `NotFound.razor` direto). **Corrigido nesta Sprint**: `AuthorizationIntegrationTests.Anonymous_ProtectedPageRedirect_CarriesTheOriginalPathAsReturnUrl` (nova) prova o `returnUrl` correto no redirect anônimo; `LoginIntegrationTests.Login_WithLocalReturnUrl_RedirectsToTheOriginallyRequestedPage` (nova) completa o ciclo até o destino pós-login; `NavigationTests.NonexistentRoute_RendersTheNotFoundPage` (E2E, nova) prova a `BD30-F063` corrigida contra um navegador real | `FIXED` | 30.17 |
| BD30-F067 | baixa | a subárvore `/experience-system` (21 rotas públicas de documentação) não tem nenhum ponto de entrada direto no header/footer/nav de topo — só é alcançável via múltiplos saltos a partir do link `/brand-guidelines` no rodapé institucional, depois pela navegação de pilar/tópico interna. Não é uma rota quebrada (toda a subárvore é alcançável), apenas discoverability fraca para uma área de 21 rotas. Decisão de produto/IA de navegação, não inventada por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F068 | baixa | os dois wizards de onboarding (`Tutorial.razor`, `CreateProfile.razor`) mantêm o passo atual fora da URL (campo privado / `ProfileCreationState` escopado por DI, nenhum query string) — padrão consistente entre os dois, não um defeito isolado. Voltar/avançar no navegador sai do wizard inteiro em vez de andar entre os passos, comportamento previsível mas não documentado como decisão. Fora do escopo de roteamento propriamente dito (nenhuma rota quebra ou produz 404); observação de arquitetura de interação encaminhada a uma Sprint de UX. **Reverificado na Sprint 30.20**: ainda preciso, sem mudança. Tornar o passo refletido na URL é trabalho de arquitetura de interação genuíno (query string, guards de navegação, testes), fora do limite de uma auditoria; reatribuído para decisão do proprietário | `OPEN` | decisão do proprietário |
| BD30-F069 | baixa | `/terms`, `/privacy` e `/community-guidelines` não contêm nenhum texto legal real — cada uma renderiza só um aviso proeminente de "revisão pendente" (`LegalPendingReview`) mais uma lista de títulos de seção sem corpo. Comportamento deliberado, testado (E2E e bUnit) e documentado no próprio código como divulgação honesta em vez de cláusulas inventadas — não é um defeito de engenharia. Registrado nesta Sprint exatamente como o critério de aceite exige: item pendente de revisão jurídica/aprovação do proprietário, nenhum texto legal foi inventado ou proposto por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F070 | baixa | `RepresentativeRoutesRenderWithoutHorizontalOverflowOnMobile` cobria 9 das 12 rotas institucionais em viewport mobile — `/brand-guidelines` (a rota estruturalmente mais complexa da família, única que embute a navegação de pilar/tópico do Experience System) nunca tinha sido testada especificamente em mobile. **Corrigido nesta Sprint**: `/brand-guidelines` adicionada ao teste. `/privacy` e `/community-guidelines` foram deliberadamente mantidas fora — compartilham o mesmo template exato de `/terms` (já coberto), mesmo padrão de amostragem representativa já usado em `MobileEditorialHeaderStaysUsableWithoutHorizontalOverflowOnTheDenseAboutUsFamily` (testa só `/mission` pela família "About us") | `FIXED` | 30.18 |
| BD30-F071 | baixa | a cobertura E2E de troca de idioma ao vivo (via seletor de idioma real) para o `beeday Experience System` existe só para a página raiz (`/experience-system`) — as outras 20 rotas da subárvore dependem só de correção testada em nível de `resx`/componente, nunca exercitadas com o seletor real de idioma em navegador. Baixo risco (mesmo padrão resx comprovadamente correto em todas), mas gap de cobertura real. Encaminhado como expansão opcional de cobertura, não defeito. **Reatribuído na Sprint 30.20**: esta Sprint auditou superfícies autenticadas; expansão de cobertura E2E de superfícies públicas é melhor encaixada na Sprint dedicada a completude de testes. **Reverificado na Sprint 30.24**: gap confirmado inalterado (ainda 1 de 21 rotas). Não corrigido nesta Sprint — 20 novos testes de troca de idioma são expansão de cobertura de baixo risco, não defeito, e o tempo desta Sprint foi priorizado para os achados com maior severidade/certeza de causa (`BD30-F042`, `BD30-F059`, `BD30-F083`). Encaminhado para consolidação de hygiene | `OPEN` | 30.26 |
| BD30-F072 | baixa | drift de token de cor confirmado em 2 pontos: `CreateProfile.razor.css` referenciava `var(--beeday-color-danger-text, #b3261e)` — um custom property nunca definido em `variables.css` em lugar nenhum, então o fallback hardcoded `#b3261e` era sempre o valor real aplicado, divergindo silenciosamente do token canônico `--beeday-color-danger` (`#d33b46`) usado em todo o resto do app; `feedback.css` usava hex bruto (`#e2f5e9`/`#fde8e8`) quase-idêntico, mas não igual, aos tokens já existentes `--beeday-color-success-soft`/`--beeday-color-danger-soft` para o fundo dos ícones de toast. **Corrigido nesta Sprint**: ambos convergidos para os tokens canônicos existentes | `FIXED` | 30.19 |
| BD30-F073 | baixa | `@keyframes beeday-spin` definido de forma idêntica em dois stylesheets globais diferentes (`design-system.css` e `feedback.css`), ambos carregados em toda página — duplicação sem propósito, cada consumidor (`.beeday-button__loader`, spinner de `BeeDayLoading`) referenciando por nome, então qualquer uma das duas definições já bastava. **Corrigido nesta Sprint**: consolidado em uma única definição | `FIXED` | 30.19 |
| BD30-F074 | baixa | `BeeDayCardMenu` (mais `CardActionMenuCoordinator`/`CardMenuPlacement`) tinha zero consumidores de produção em todo `src/` — confirmado por busca completa por `<BeeDayCardMenu`. Superado por um refactor anterior que tornou os cards inteiros clicáveis para editar (commit `05a7ad3`), mas o componente, seu serviço de coordenação, sua geometria de posicionamento e dois arquivos de teste dedicados nunca foram removidos. **Corrigido nesta Sprint**: componente, serviço, geometria, os dois arquivos de teste dedicados, o registro de DI em `Program.cs` e as 3 chaves `resx` exclusivas (`CardMenu*`) removidos; `CoreComponentContractTests` (inventário de componentes e contagem de controles nativos) atualizado para refletir a remoção | `FIXED` | 30.19 |
| BD30-F075 | baixa | CSS morta remanescente em `cards.css`, ligada ao mesmo padrão de interação superado pela `BD30-F074`: `.activity-card__menu`, `.habit-card__menu`, `.activity-card--menu-open`, `.habit-card--menu-open`, `.activity-card__actions` — confirmado por busca de marcação que nenhum desses seletores casa com qualquer elemento hoje. Tentativa de remoção iniciada nesta Sprint e revertida ao descobrir que o escopo real é maior do que o inicialmente visível — pelo menos 6 localizações separadas no arquivo, algumas como regras isoladas e outras entrelaçadas em grupos de seletores separados por vírgula que também contêm seletores ainda vivos (ex.: `.activity-card__checkbox, .habit-card__score-button, .activity-card__menu, .habit-card__menu { ... }`). Remover parcialmente sob pressão de tempo de uma Sprint de auditoria é mais arriscado do que documentar e encaminhar para uma limpeza dedicada com verificação visual adequada | `OPEN` | 30.26 |
| BD30-F076 | baixa | regra global obsoleta `.beeday-hero__eyebrow` em `design-system.css` já causou uma falha real de WCAG-AA (capturada pelo scan axe-core do repositório) por conflitar com o CSS isolado de `BeeDayHero.razor.css` em superfícies COR0-COR9 multi-cor; corrigida à época via um truque de especificidade (`color: inherit` no CSS isolado, que tecnicamente ganha da regra global) em vez de remover/escopar a regra obsoleta. Funcionalmente correta hoje, mas frágil — depende da ordem de cascata entre dois arquivos permanecer exatamente como está, em vez da regra obsoleta simplesmente não existir mais. Já autodocumentada em comentário no código; registrada aqui para acompanhamento formal | `OPEN` | 30.26 |
| BD30-F077 | alta | regressão confirmada de touch target: o comentário e os valores de `cards.css` (`"Sprint 21.14: activity actions retain a 44px target"`) declaram a intenção explícita de checkbox/badge de Task/Todo/Project em `2.75rem` (44px, alvo WCAG comum), adicionados no commit `698e157` (2026-08-13) logo após a definição base do seletor — mas um bloco "compact layout" mais antigo (commit `5532d327`, 2026-07-25) já existia mais adiante no mesmo arquivo redeclarando os mesmos seletores para `1.55rem` (~24.8px) e a coluna de grid para `2.35rem`, sem nenhuma relação com acessibilidade em seu próprio comentário. Como CSS resolve por ordem de código-fonte entre seletores de especificidade igual, a regra mais recente (e com intenção de acessibilidade documentada) perdia silenciosamente para a mais antiga, meramente por estar posicionada antes no arquivo. Habit não é afetado (nunca teve um fix de 44px documentado). **Corrigido nesta Sprint**: removida a redeclaração conflitante de `grid-template-columns`/`width`/`height` para `.activity-card`/`.activity-card__checkbox`/`.activity-card__project-status` no bloco "compact layout", deixando o valor de 44px já declarado (e já correto) prevalecer; `.habit-card__score-button` mantido intocado (nenhuma intenção de 44px jamais registrada para ele). Verificado visualmente via captura de tela real e por toda a suíte E2E de Habit/Task/Todo/Project/Shell (31/31), sem regressão de layout | `FIXED` | 30.20 |
| BD30-F078 | baixa | `BeeDaySortable.razor` aplicava um `aria-label` literal em inglês (`"Hold and drag this card to reorder it. Use the arrow keys when focused."`) a cada item arrastável de Habits/Tasks/Todos/Projects em `/daily`, em ambas as culturas — o `AriaLabel` de nível de lista já era corretamente localizado por cada consumidor, só o rótulo por item estava hardcoded no componente compartilhado. **Corrigido nesta Sprint**: novo parâmetro `ItemAriaLabel` com valor padrão em inglês (mesmo contrato de `AriaLabel`), conectado a uma nova chave `resx` (`ReorderItemAriaLabel`) em `DashboardResources`, usada nos 4 pontos de uso em `Home.razor` | `FIXED` | 30.20 |
| BD30-F079 | média | nenhum dos 5 componentes de formulário compartilhados (`BeeDayInput`, `BeeDayTextArea`, `BeeDaySelect`, `BeeDayDateInput`, `BeeDayCheckbox`) associava programaticamente seu controle à própria mensagem de validação (`BeeDayValidationMessage`) — sem `aria-describedby`, um usuário de leitor de tela que navega de volta a um campo já inválido não recebe nenhum sinal da causa, violando o padrão WCAG 4.1.2/3.3.1 de "identificação de erro associada programaticamente ao controle", já implementado corretamente para o próprio `role="alert"` da mensagem. Afeta todos os 6 editores de atividade/tag/transação, já que todos compartilham esses 5 componentes. **Corrigido nesta Sprint**: `BeeDayValidationMessage` ganha um `Id` opcional aplicado a um novo wrapper em torno das mensagens; cada um dos 5 componentes de formulário passa `aria-describedby="{Id}-validation"` para seu controle nativo e `Id="{Id}-validation"` para `BeeDayValidationMessage`, sempre que `ShowValidationMessage` está ativo. Novo teste bUnit (`InputAssociatesItsValidationMessageViaAriaDescribedby`) prova a associação, incluindo a atualização em tempo real quando uma mensagem de validação aparece | `FIXED` | 30.20 |
| BD30-F080 | baixa | `ProjectWorkspace.razor` — a superfície autenticada estruturalmente mais complexa depois do Wallet, com sua própria lista de To-Dos e barra de progresso — não tinha nenhuma cobertura E2E de overflow horizontal em viewport estreito, ao contrário de `/daily`, `/wallet`, `/account` e as rotas de onboarding. **Corrigido nesta Sprint**: novo teste `ProjectWorkspace_RendersWithoutHorizontalOverflowOnMobile` (390×844), que passou de primeira, confirmando ausência de overflow real | `FIXED` | 30.20 |
| BD30-F081 | média | o drawer de navegação mobile (`MobileSidebar.razor`) já movia o foco para seu próprio botão de fechar ao abrir, mas nada devolvia o foco ao gatilho (`MobileHeader`'s hamburger) ao fechar — via Escape, clique no backdrop, ou o próprio botão de fechar —, deixando o foco de teclado/leitor de tela perdido em `<body>`. Diferente dos diálogos baseados em `EditorModalShell`/`DialogFocusScope` (confirmados corretos nesta Sprint), o drawer nunca usou esse primitivo compartilhado. **Corrigido nesta Sprint**: `MobileHeader.razor` ganha uma `ElementReference` para seu próprio botão e devolve o foco a ele quando `IsNavOpen` transiciona de `true` para `false` — espelhando exatamente o padrão já usado por `MobileSidebar` para a transição inversa (abrir). Novo teste E2E (`Mobile_ClosingTheDrawerReturnsFocusToTheHamburgerTrigger`) prova o foco restaurado após Escape em um navegador real | `FIXED` | 30.20 |
| BD30-F082 | baixa | o menu de criação de atividade (`ActivityFilterBar.razor`, `role="menu"`) não tem nenhum tratamento de teclado além de Tab/Enter alcançarem os itens — sem Escape para fechar, sem navegação por setas (que a própria semântica ARIA de `role="menu"` implica), sem dispensa por clique fora, e não está conectado a `DialogFocusScope` como todo outro overlay do app. Não é um bloqueio duro (Tab/Enter continuam funcionando), mas não cumpre o contrato de interação que seu próprio papel ARIA anuncia. Corrigir com segurança exigiria adicionar suporte a `ElementReference`/foco em `BeeDayButton` (componente compartilhado do Design System) ou um helper de foco via JS interop — infraestrutura não construída nesta Sprint; um meio-conserto (só Escape, sem devolução de foco) foi deliberadamente descartado por poder deixar o foco pior do que está hoje. Trabalho de engenharia represado, não decisão de produto | `OPEN` | decisão do proprietário |
| BD30-F083 | baixa | existe uma alternativa de teclado real para reordenar Habits/Tasks/Todos/Projects (`beeday-sortable.js`, `ArrowUp`/`ArrowDown` no item focado, mesmo caminho `NotifyReorderAsync` do arrasto por mouse), mas nenhum teste E2E em toda a suíte exercita isso (`grep` por `ArrowUp\|ArrowDown\|Reorder` em `tests/BeeDay.E2E.Tests` não encontra nenhum). Gap de cobertura real para uma capacidade de acessibilidade já implementada e correta. **Corrigido na Sprint 30.24**: novo `ArrowDown_ReordersHabitsAndPersistsAfterReload` em `HabitAndTaskTests.cs` — cria 2 Habits, confirma a ordem inicial via `[data-sortable-item]`, foca o corpo editável do primeiro, pressiona `ArrowDown` (teclado real, não simulação de evento), confirma a troca de ordem no DOM e a persistência após um reload real de página. 3/3 execuções isoladas aprovadas antes da entrega | `FIXED` | 30.24 |
| BD30-F084 | alta | `EfDashboardReadService.GetAsync` e `EfWalletReadService.GetSummaryAsync` carregavam **todas** as transações do Wallet de um usuário pela rede só para somar `Balance`/`TotalIncome`/`TotalExpenses` em memória via `Wallet.CalculateBalance`/`CalculateTotalIncome`/`CalculateTotalExpenses` (métodos de Domain, sem acesso a EF, corretos para consultas já carregadas mas inadequados como estratégia de leitura). O custo era pago duplamente: em todo carregamento de `/daily` **e** em toda visita a `/wallet`, e — pior — `DashboardState.ReloadAsync` recarrega o resumo do Wallet após **qualquer** mutação de Habit/Task/Todo/Project, não só de Wallet, então o carregamento completo da tabela de transações acontecia a cada clique em um Habit, não só ao abrir o Wallet. **Corrigido nesta Sprint**: as duas leitoras agora agregam em SQL (`SumAsync` condicional por `TransactionType`, `CountAsync`) em vez de materializar cada linha — mesmo resultado, transferência de dados mínima independente do volume de transações. Equivalência funcional comprovada pelos testes já existentes (`EfWalletReadServiceTests.GetSummaryAsync_CalculatesBalanceIncomeAndExpenses`, `EfDashboardReadServiceTests` — ambos passaram sem alteração, mesmos valores exatos antes/depois) | `FIXED` | 30.21 |
| BD30-F085 | baixa | nenhuma paginação/arquivamento existe para as coleções de Habits/Tasks/Projects/Todos de um usuário na camada de leitura (`EfDashboardReadService.GetAsync` carrega o conjunto completo, sempre) — mesma classe estrutural de crescimento ilimitado por usuário já registrada para `ExperienceEntry` (`BD30-F051`), mas aqui afetando os próprios itens do dashboard, não só seu histórico de XP. A única mitigação existente é de renderização (`BeeDaySortable`'s virtualização de DOM a partir de um limiar), que não reduz a consulta SQL nem o payload transferido. Nenhum teste no repositório cria mais de poucos itens por usuário, então isso nunca foi exercitado em escala real. Decisão de produto sobre limites/paginação necessária, não inventada por esta auditoria | `OPEN` | decisão do proprietário |
| BD30-F086 | baixa | `DashboardState.ReloadAsync` sempre recarrega o dashboard inteiro (Habits+Tasks+Projects+Todos+resumo do Wallet+perfil/XP) após qualquer mutação individual — registrar um único Habit dispara a mesma recarga completa que abrir a página do zero. Prioridade reduzida nesta Sprint após a correção da `BD30-F084` já ter eliminado o componente mais caro (transferência completa da tabela de transações); o que resta é uma contagem pequena e já fixa de consultas (6, sem N+1, ver `BD30-F084`), então o ganho de uma recarga granular por seção é incerto sem medição adicional, e reescrever o contrato de recarga do `DashboardState` é mudança de arquitetura genuína, não uma correção pontual. Encaminhado sem correção nesta Sprint | `OPEN` | decisão do proprietário |
| BD30-F087 | baixa | a Home pública (`/`) referencia duas imagens PNG não otimizadas de 1,7–1,8 MB cada (`home-team-fall.png`, `home-team.png` — a segunda com `fetchpriority="high"`, carregada antecipadamente acima da dobra), sem variante WebP/AVIF nem `srcset` responsivo; os ícones de bandeira do seletor de idioma público também carregam em tamanho muito maior (140–171 KB) do que o necessário para um ícone pequeno. Fora do caminho crítico autenticado que esta Sprint nomeia (`/daily`, `/wallet`) — confirmado que nenhuma imagem acima de 200 KB é referenciada em Dashboard/Wallet. Encaminhado como hygiene de assets, não defeito de performance do caminho crítico | `OPEN` | 30.26 |
| BD30-F088 | baixa | não existe nenhum passo de build dedicado a bundling/minificação de CSS/JS (`grep` por bundler/webpack/esbuild/LibMan em toda configuração de build não encontra nada) — a minificação em `cards.css`/`wallet.css` é acidental (já commitada minificada), enquanto `design-system.css`/`variables.css`/`app.css` permanecem formatados. Mitigado hoje por `app.MapStaticAssets()`, que já aplica compressão gzip/Brotli e fingerprinting de conteúdo em tempo de build/publish independentemente do arquivo-fonte estar minificado — gap de hygiene, não defeito de performance medido | `OPEN` | 30.26 |
| BD30-F089 | média | nenhum header de segurança além de HSTS (`UseHsts()`, fora de Development) era definido pela aplicação — `Referrer-Policy`, `X-Content-Type-Options` e `Permissions-Policy` estavam ausentes de toda resposta, confirmado por um teste de integração dedicado (`SecurityHeadersIntegrationTests`) que já provava esse estado explicitamente. `X-Frame-Options` e uma CSP básica (`frame-ancestors 'self'`) já eram enviados automaticamente pelo próprio framework Razor Components, independente de configuração da aplicação — não fazem parte deste achado. **Corrigido nesta Sprint**: novo `SecurityHeadersMiddleware` (`src/BeeDay.Web/Diagnostics/`) define os 3 headers ausentes em toda resposta; deliberadamente não toca `X-Frame-Options`/CSP para não competir com os valores já corretos do framework (sobrescrever arriscaria um valor silenciosamente substituído ou um header conflitante, dependendo do tipo de resposta) — verificado empiricamente que o teste que prova os headers do framework continua passando sem alteração. Uma CSP completa (`script-src` etc.) permanece planejada, não tentada aqui — precisa de desenho e verificação dedicados, não uma adição pontual de header | `FIXED` | 30.22 |

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

## 14. Sprint 30.7 — Infrastructure / EF Core / SQL Server Audit

### 14.1 Inventário e fronteiras

Os 58 arquivos de `src/BeeDay.Infrastructure` foram auditados contra o Issue #204: índices,
constraints, comportamento de FK, precisão decimal, armazenamento de data/hora, tracking, risco de
N+1, atomicidade de transação e ciclo de vida de contexto para uso em Blazor Server. `INV-005` e
`INV-006` passam a `VERIFIED`.

`EfRepositoryBase`/`IDbContextFactory` confirmados corretos: todo repositório cria um `DbContext`
de vida curta por operação (nunca compartilhado entre requisições de um circuito Blazor Server);
`EfUnitOfWork` é registrado `AddTransient`, não `AddScoped`, com contexto próprio descartado ao
final da unidade de trabalho. Precisão decimal (`decimal(19,2)` via convenção global), datas
(`date`/`datetimeoffset(7)`), tracking (`AsNoTracking` em toda leitura, tracking apenas onde
RowVersion exige) e transação explícita (`BeginTransactionAsync`/`CommitTransactionAsync` com
rollback implícito ao descartar sem commit) — todos verificados sem achado. A migration única
(`InitialCreate`) não diverge do modelo atual; as duas seções de SQL bruto (`UX_Users_Nickname`,
`UX_ExperienceEntries_Dedup`) batem exatamente com a justificativa documentada nas Configurations.

### 14.2 BD30-F030 — causa raiz confirmada e corrigida

Investigação dirigida por evidência (não pela suposição do achado herdado da Sprint 30.5) confirmou
que o problema é mais severo do que o texto original descrevia. `UserConfiguration.cs` ignora
deliberadamente `UserExperience.Entries` (correto: `ExperienceEntry` já é uma entidade top-level
relacionada a `User` diretamente, mapear de novo sob `UserExperience` duplicaria o relacionamento),
mas nenhum outro código em `src/BeeDay.Infrastructure` jamais adicionava uma nova `ExperienceEntry`
ao `DbSet` correspondente — confirmado por busca (`grep -rn "ExperienceEntries.Add"` = zero
resultados) e por um teste real contra LocalDB escrito nesta Sprint
(`EfUserRepositoryTests.UpdateAsync_MutationGrantsExperience_PersistsTheExperienceEntry`), que
falhou antes da correção (`Assert.Single() Failure: The collection was empty`).

Consequência confirmada, não hipotética: como `Entries` nunca é hidratada ao carregar o `User`,
`UserExperience.TryAdd` — o único caminho de concessão automática de XP, usado por toda conclusão
de Habit/Task/Todo/Project — sempre comparava contra uma coleção vazia. Recompletar um Todo/Task/
Project já concedido antes (desmarcar e marcar de novo, um fluxo de UI real e suportado) concedia
XP outra vez, sem limite, sem nunca gravar o histórico correspondente em `ExperienceEntries`.

### 14.3 Correção mínima

- `UserExperience.Hydrate(IReadOnlyList<ExperienceEntry>)` (novo, `internal`): atribui entries já
  persistidas sem repassar por `Add` (que duplicaria `TotalExperience`, já carregado
  independentemente de sua própria coluna). Hook de materialização, não uma regra de negócio nova.
- `BeeDay.Domain.csproj` ganha `InternalsVisibleTo` para `BeeDay.Infrastructure` — grant de
  visibilidade unidirecional (Infrastructure enxerga internals de Domain), não uma referência de
  assembly; `DomainAssemblyBoundaryTests` confirma que Domain continua sem depender de
  Infrastructure.
- `EfUserRepository.UpdateAsync` agora: (1) carrega as `ExperienceEntries` existentes do usuário
  (`WHERE UserId = @userId`, coberta pelo índice `IX_ExperienceEntries_User_Time`) e hidrata
  `user.Experience` antes de invocar a mutação — assim `TryAdd` compara contra histórico real, não
  uma coleção sempre vazia; (2) após a mutação, adiciona ao `DbSet` qualquer entry cujo `Id` não
  estava no conjunto pré-carregado — fechando o outro defeito (linhas nunca persistidas).
- Escopo da mudança: apenas `UpdateAsync` (todas as mutações de `User`, não só concessão de XP)
  paga uma consulta indexada adicional; `GetByIdAsync`/`GetByEmailAsync` (chamadas a cada requisição
  autenticada) permanecem inalterados.
- Confirmado que a exceção de Habit ao dedup continua correta: `ExperienceRewardService.Grant` já
  usa `Guid.NewGuid()` como `SourceId` para `ExperienceSourceType.Habit` (nunca o Id do próprio
  Habit), então a hidratação não pode gerar falso-positivo bloqueando registro repetido de Habit —
  provado por teste dedicado.

### 14.4 Achados menores (não corrigidos, encaminhados)

- `BD30-F032` (nova, baixa): corrida de `Position` sob inserção concorrente em Habit/RecurringTask/
  Project/Todo — sem índice único, apenas cosmética, autocorrigível no próximo reorder. Encaminhada
  à Sprint 30.25 (endurecimento amplo já proprietário de CI/CD e deployment nesta EPIC).
- `BD30-F033` (nova, baixa): ordenação de `Transaction` por `Description`/`Amount`/`CreatedAtUtc`
  sem índice cobrindo esses campos além do seek por `WalletId`. Encaminhada à Sprint 30.21
  (Performance & Efficiency Audit).
- `BD30-F011`/`BD30-F014` confirmados ainda precisos nesta auditoria; não corrigidos, permanecem de
  propriedade de suas Sprints já atribuídas (30.7 documental / 30.7 já fechado por causa raiz na
  30.2 — mantidos conforme o Ledger original).

### 14.4.1 `BD30-F034` — integridade histórica de `TotalExperience`, encaminhado à Sprint 30.16

A correção de `BD30-F030` (§14.2–14.3) é a remediação correta e completa para o defeito de
persistência/deduplicação daqui em diante — aceita explicitamente pelo owner como tal. Ela não
constitui, e não tenta constituir, uma correção retroativa de dados. Isso é registrado aqui como um
achado de integridade de dados **separado**, distinto da causa raiz já fechada:

- histórico de `ExperienceEntry` não era persistido antes da correção desta Sprint;
- alternar conclusão/reabertura repetidamente de um Todo/Task/Project podia, antes da correção,
  conceder XP duplicado sem limite a cada repetição;
- a existência e a magnitude de qualquer inflação histórica em HMG/produção **não foram
  quantificadas** por esta Sprint — nenhuma consulta ou mutação foi executada contra banco de
  HMG/produção;
- as linhas de `ExperienceEntry` que porventura já existam de antes da correção podem ser
  insuficientes para reconstruir `TotalExperience` de forma determinística, precisamente porque o
  histórico correspondente às concessões duplicadas nunca foi persistido;
- **nenhuma mutação de banco de dados está autorizada por este achado**;
- **nenhum reset ou recálculo arbitrário de `TotalExperience` é permitido** sem um plano de correção
  revisado e aprovação explícita do owner.

Este achado é atribuído principalmente à Sprint 30.16 — Experience, XP, Level & Rewards Audit, que
deve determinar, usando evidência de repositório e de banco:

1. se o XP histórico afetado pode ser identificado;
2. se o XP correto pode ser reconstruído de forma determinística;
3. quais fontes de recompensa (Habit/Task/Todo/Project) são afetadas;
4. se repetições legítimas de Habit podem ser distinguidas de concessões duplicadas de
   Todo/Task/Project;
5. se uma correção automatizada é segura;
6. se seria necessária reconciliação manual ou parcial;
7. o raio de impacto exato em HMG e, separadamente, em produção.

Qualquer mutação real de dados em HMG/produção permanece fora desta autorização e exige aprovação
explícita do owner após um plano de correção revisado. Este achado não bloqueia as demais Sprints
de auditoria da EPIC 30.

### 14.5 Implementação

- `src/BeeDay.Domain/Experience/UserExperience.cs` — novo método `internal Hydrate(...)`.
- `src/BeeDay.Domain/BeeDay.Domain.csproj` — `InternalsVisibleTo` para `BeeDay.Infrastructure`.
- `src/BeeDay.Infrastructure/Persistence/SqlServer/Repositories/EfUserRepository.cs` —
  `UpdateAsync` hidrata e persiste `ExperienceEntries` corretamente.
- `tests/BeeDay.Infrastructure.Tests/Persistence/SqlServer/Repositories/EfUserRepositoryTests.cs`
  — 3 testes novos contra LocalDB real: persistência da entry, dedup entre chamadas separadas
  (recompletar o mesmo Todo/Task/Project não concede XP duas vezes), e não-regressão do registro
  repetido de Habit.

Nenhuma mudança de contrato público de Application, de schema (nenhuma migration nova — a tabela e
o índice já existiam desde `InitialCreate`), ou de Design System.

### 14.6 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Infrastructure.Tests/... --filter EfUserRepositoryTests` antes da correção | reprodução determinística: FAIL (`Assert.Single() Failure: The collection was empty`) |
| `dotnet test tests/BeeDay.Infrastructure.Tests/... --filter EfUserRepositoryTests` após a correção | PASS, 9/9 |
| `dotnet test tests/BeeDay.Domain.Tests/...` | PASS, 117/117 |
| `dotnet test tests/BeeDay.Infrastructure.Tests/...` completo | PASS, 215/215 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.501/1.501 (117 Domain, 113 Application, 215 Infrastructure, 863 Web, 193 E2E); E2E 6m47s |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.501/1.501 (mesma distribuição); E2E 6m39s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

Os suites E2E (Chromium, jornadas de Habit/Task/Todo incluindo conclusão e XP) passaram em Debug e
Release após a correção, confirmando o caminho de concessão de experiência de ponta a ponta.

### 14.7 Continuidade e entrega

Reconhecimento amplo do restante de Infrastructure (Configurations, repositórios, migration,
Options) foi delegado a um subagente Explore somente-leitura, com instrução explícita para não
reinvestigar o que já havia sido confirmado e corrigido nesta sessão. O achado mais severo
(`BD30-F030`) foi investigado diretamente por esta sessão, não pelo subagente — incluindo a prova
empírica contra LocalDB real antes de qualquer alteração de código, seguindo a mesma disciplina de
"evidência substitui suposição" já aplicada nas Sprints anteriores da EPIC 30.

## 15. Sprint 30.8 — Blazor Runtime & DI Audit

### 15.1 Inventário e fronteiras

Auditados contra o Issue #205: registros de DI além de `Program.cs`, interações do circuito de
longa duração com `IDbContextFactory`/repositórios/serviços de estado/disposables, handlers
assíncronos, re-entrância, double-submit, cancelamento, rendering loops e recuperação de erro em
componentes de mutação. `INV-007` passa a `VERIFIED`.

Reconhecimento amplo foi delegado a um subagente Explore somente-leitura; cada achado relatado foi
verificado nesta sessão antes de qualquer ação (grep direto de `AddScoped`/`AddSingleton`/
`AddTransient`, leitura completa de `BeeDayWebService.cs`, contagem de métodos e busca por
`CancellationToken`).

Resultado por categoria:

- **Registros de DI**: as 11 chamadas encontradas em todo `src/BeeDay.Web` estão em `Program.cs`,
  todas `Scoped` (correto para Blazor Server — vida do circuito). Nenhum `AddSingleton` capturando
  estado por usuário/circuito existe. Zero achado.
- **Disposables de circuito longo**: 14 implementações de `IDisposable`/`IAsyncDisposable`
  auditadas; toda assinatura de evento tem sua desinscrição correspondente em `Dispose`. Zero
  achado, exceto `BD30-F036` (abaixo).
- **Re-entrância/double-submit**: protegido de forma centralizada e consistente — `DashboardState.
  ExecuteAsync` (guarda `IsBusy`) cobre todas as mutações de Habit/Task/Todo/Project;
  `WalletInteractionState.TryBegin/End` cobre Wallet; `Account.razor` usa guardas `_xBusy` por
  seção. Zero achado de correção; `BD30-F037` registra um polimento visual não-bloqueante.
- **Cancelamento**: achado sistêmico real, ver §15.2.
- **Rendering loops**: nenhuma chamada de `StateHasChanged()` fora da thread de UI ou dentro de
  loop/timer sem `InvokeAsync`. Zero achado de correção; parte de `BD30-F037` (Wallet.razor).
- **Recuperação de erro em mutações**: todo componente de mutação amostrado envolve a chamada em
  `try/catch` e traduz a exceção para toast/mensagem inline — nenhum caminho de exceção não tratada
  encontrado capaz de derrubar o circuito. Zero achado.
- **Reconexão**: `ReconnectModal` customizado e localizado, construído sobre o template padrão do
  Blazor Web App; nenhum `CircuitHandler` customizado. Apenas informativo, não é achado.

### 15.2 `BD30-F035` — cancelamento nunca propagado, corrigido

`BeeDayWebService` (20 métodos) e os call sites diretos de `ISender.Send` em `Wallet.razor` e nas
páginas de Identity/Account/Onboarding nunca aceitavam nem propagavam um `CancellationToken` —
toda chamada usava implicitamente `CancellationToken.None`, confirmado por
`grep -n "CancellationToken" src/BeeDay.Web/Services/BeeDayWebService.cs` (zero resultado) antes da
correção. A cadeia Application/Infrastructure já respeitava corretamente um token real em toda a
sua extensão (confirmado nas Sprints 30.6/30.7) — a lacuna era exclusivamente a origem, em Web.

Correção mínima, sem novo contrato público além de parâmetros opcionais com valor padrão:

- `BeeDayWebService`: todos os 20 métodos ganham `CancellationToken cancellationToken = default`,
  repassado a `sender.Send(request, cancellationToken)`.
- `DashboardState` (Scoped, vida do circuito) passa a `IDisposable`, possui um
  `CancellationTokenSource` próprio cancelado em `Dispose()` (chamado automaticamente pelo
  container de DI ao final do circuito), e o token é encaminhado em todo `store.XAsync(...)`
  disparado por esse único ponto de escolha — cobrindo de uma vez Habits, Tasks, Todos e Projects.
  `ExecuteAsync` ganha um `catch (OperationCanceledException) when (cancellation.
  IsCancellationRequested)` para não mostrar um toast de erro genérico quando o cancelamento veio
  do próprio `Dispose`.
- `Wallet.razor` ganha `@implements IDisposable` com seu próprio `CancellationTokenSource`
  (cancelado quando o roteador do Blazor descarta a página ao navegar para longe — granularidade
  de página, não de circuito inteiro), encaminhado nos 10 call sites de `Sender.Send`, com a mesma
  proteção contra toast espúrio em cada `catch`.
- `Account.razor`, `Tutorial.razor` (Onboarding) e as 5 páginas de Identity
  (`ConfirmEmail`/`EmailConfirmationSent`/`ForgotPassword`/`ResendConfirmation`/`ResetPassword`)
  recebem o mesmo padrão — cada uma com seu próprio `CancellationTokenSource` de vida da página.
  Nas duas páginas que já tinham um `CancellationTokenSource` para o timer de contagem regressiva,
  um segundo token dedicado (`_pageCts`) foi adicionado deliberadamente separado, para não
  confundir o cancelamento da mutação com o ciclo de reinício do timer.
- Escopo deliberadamente não estendido: `InitializeCoreAsync`/`OnInitializedAsync` de carregamento
  inicial (chamados uma única vez por circuito/página) não ganharam o mesmo guard de exceção —
  risco de cancelamento concorrente nesse ponto específico é despreziável.

### 15.3 Achados menores (não corrigidos, encaminhados)

- `BD30-F036` (nova, baixa): `PeriodicTimer`/`CancellationTokenSource` do contador regressivo em
  `EmailConfirmationSent.razor`/`ResendConfirmation.razor` não são descartados antes de serem
  reatribuídos numa segunda chamada a `StartCountdown()` — hoje inalcançável (botão desabilitado
  enquanto a contagem corre). Encaminhado à Sprint 30.10.
- `BD30-F037` (nova, baixa): cards individuais do Dashboard não vinculam `Disabled` a
  `State.IsBusy` (proteção real existe, só falta feedback visual por card); `Wallet.razor` não
  chama `StateHasChanged()` uma segunda vez ao encerrar o destaque de saldo. Encaminhado à Sprint
  30.20.

### 15.4 Implementação

- `src/BeeDay.Web/Services/BeeDayWebService.cs` — `CancellationToken` opcional em todos os métodos.
- `src/BeeDay.Web/Components/Features/Dashboard/State/DashboardState.cs` — `IDisposable` +
  `CancellationTokenSource` de circuito, encaminhado em todo call site.
- `src/BeeDay.Web/Components/Features/Wallets/Pages/Wallet.razor` — `IDisposable` +
  `CancellationTokenSource` de página, encaminhado nos 10 call sites de `Sender.Send`.
- `src/BeeDay.Web/Components/Features/Account/Pages/Account.razor`,
  `src/BeeDay.Web/Components/Features/Onboarding/Pages/Tutorial.razor`,
  `src/BeeDay.Web/Components/Features/Identity/Pages/{ConfirmEmail,EmailConfirmationSent,
  ForgotPassword,ResendConfirmation,ResetPassword}.razor` — mesmo padrão.
- `tests/BeeDay.Web.Tests/Components/Dashboard/DashboardStateCancellationTests.cs` (novo) — prova
  que o token encaminhado não está cancelado antes de `Dispose`, que `Dispose` cancela o mesmo token
  usado pela mutação, e que um cancelamento disparado durante uma operação em andamento não produz
  um toast de erro genérico.

Nenhuma mudança de contrato público de Application/Domain, de schema, ou de comportamento visível
ao usuário em operação normal — o efeito só é observável quando uma requisição é genuinamente
abandonada (navegação, fechamento de aba, queda de circuito).

### 15.5 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Web.Tests/... --filter DashboardStateCancellationTests` | PASS, 3/3 |
| `dotnet test tests/BeeDay.Web.Tests/...` completo | PASS, 866/866 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.504/1.504 (117 Domain, 113 Application, 215 Infrastructure, 866 Web, 193 E2E); E2E 6m41s |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.504/1.504 (mesma distribuição); E2E 6m29s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

O suite E2E completo (Chromium — Dashboard, Wallet, Identity, Account, Onboarding) passou em Debug
e Release após a mudança, confirmando que nenhum fluxo real de usuário foi afetado pela propagação
de cancelamento.

### 15.6 Continuidade e entrega

Reconhecimento amplo (DI, disposables, re-entrância, cancelamento, rendering, recuperação de erro,
reconexão) foi delegado a um subagente Explore somente-leitura. O único achado MAJOR
(`BD30-F035`) foi verificado de forma independente nesta sessão antes de qualquer correção (leitura
direta de `BeeDayWebService.cs`, confirmação de zero uso de `CancellationToken`) e corrigido de
ponta a ponta nos dois pontos de escolha centralizados (`DashboardState`, `WalletInteractionState`
via `Wallet.razor`) mais as sete páginas restantes que chamam `ISender`/`BeeDayWebService`
diretamente — fechando a lacuna sistêmica por completo, não apenas parcialmente.

## 16. Sprint 30.9 — Architecture Consolidation & Module Boundaries

### 16.1 Método e evidência

O escopo desta Sprint é sintetizar os achados das Sprints 30.5–30.8 (não reabrir a auditoria
baseline inteira) em busca de módulos superdimensionados, responsabilidades mal alocadas,
abstrações duplicadas e fronteiras fracas — e só alterar estrutura onde a evidência já coletada
justifica.

Revisão de todo achado aberto nas Sprints 30.5–30.8 (`BD30-F023`–`BD30-F037`): `BD30-F030`/
`BD30-F035` (persistência e cancelamento) já foram corrigidos em suas Sprints de origem;
`BD30-F032`/`BD30-F033`/`BD30-F034`/`BD30-F036`/`BD30-F037` são achados pontuais de índice,
integridade de dado histórica ou polimento de UX, já corretamente encaminhados às suas Sprints
proprietárias (30.10, 30.16, 30.20, 30.21, 30.25) — nenhum descreve um módulo superdimensionado,
responsabilidade mal alocada ou abstração duplicada que pertença à consolidação desta Sprint.
Nenhum dos quatro achados de auditoria de camada (Domain 30.5, Application 30.6, Infrastructure
30.7, Web 30.8) relatou duplicação estrutural não documentada, camada oversized, ou abstração
paralela — os quatro concluíram a camada correspondente já limpa, com achados isolados e pequenos.

O único item genuinamente estrutural, de propriedade explícita desta Sprint, é `BD30-F009`
(baseline da Sprint 30.1): Domain e Application já tinham um guard automatizado real de fronteira
de assembly (`DomainAssemblyBoundaryTests`, `PersistenceContractBoundaryTests.
ApplicationAssembly_DoesNotReferenceInfrastructure`); Infrastructure e Web não tinham nenhum —
a fronteira era verdadeira apenas por convenção observada manualmente (INV-005/INV-007), não por
teste que a trava contra regressão futura.

### 16.2 `BD30-F009` — guards de fronteira ausentes em Infrastructure e Web, corrigidos

Antes de escrever qualquer guard, as referências reais de `BeeDay.Infrastructure.dll` e
`BeeDay.Web.dll` foram inspecionadas via `Assembly.GetReferencedAssemblies()` (não suposição) para
evitar um forbidden-list com falso positivo:

- `BeeDay.Infrastructure.dll` não referencia `BeeDay.Web` nem qualquer assembly de Blazor
  Components, apesar do `FrameworkReference` a `Microsoft.AspNetCore.App` (que só disponibiliza o
  framework compartilhado para resolução, não força referência real a tipo algum) — confirmado
  antes de travar o guard.
- `BeeDay.Web.dll` não referencia `Microsoft.EntityFrameworkCore`/`.Relational`/`.SqlServer` nem
  `Microsoft.Data.SqlClient` diretamente — confirma com evidência automatizada o que INV-007 já
  afirmava por inspeção manual.

Dois guards novos, no mesmo estilo dos dois já existentes (inspeção de metadata do assembly
compilado, não busca em texto-fonte):

- `tests/BeeDay.Infrastructure.Tests/InfrastructureAssemblyBoundaryTests.cs` — trava que
  Infrastructure nunca referencia `BeeDay.Web` ou assemblies de Blazor Components.
- `tests/BeeDay.Web.Tests/WebAssemblyBoundaryTests.cs` — trava que Web nunca referencia EF Core ou
  um client SQL Server diretamente, preservando a regra de que toda persistência passa
  exclusivamente pelos contratos públicos de Infrastructure.

Nenhuma mudança de comportamento, contrato público, schema ou Design System — os dois guards
apenas tornam permanente, contra regressão futura, uma fronteira que já era verdadeira.

### 16.3 Decisão da Sprint

Nenhuma consolidação/divisão estrutural adicional foi identificada com evidência suficiente das
Sprints 30.5–30.8 para justificar mudança de código além de `BD30-F009`. Consistente com o próprio
princípio da Sprint ("Split or consolidate modules... only where evidence justifies the change" e
"No rewrite for aesthetic preference"), nenhuma reestruturação especulativa foi proposta.

### 16.4 Implementação

- `tests/BeeDay.Infrastructure.Tests/InfrastructureAssemblyBoundaryTests.cs` (novo).
- `tests/BeeDay.Web.Tests/WebAssemblyBoundaryTests.cs` (novo).

### 16.5 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Infrastructure.Tests/... --filter InfrastructureAssemblyBoundaryTests` | PASS, 1/1 |
| `dotnet test tests/BeeDay.Web.Tests/... --filter WebAssemblyBoundaryTests` | PASS, 1/1 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` | PASS, 1.506/1.506 (117 Domain, 113 Application, 216 Infrastructure, 867 Web, 193 E2E); E2E 6m24s |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.506/1.506 (mesma distribuição); E2E 6m23s |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 16.6 Continuidade e entrega

Sprint deliberadamente pequena e focada: a evidência já coletada nas quatro Sprints anteriores não
apontava para nenhuma reestruturação de módulo, e a Sprint corretamente reconheceu isso em vez de
inventar trabalho. O único item estrutural de propriedade real desta Sprint (`BD30-F009`) foi
fechado com evidência empírica (inspeção real do assembly compilado antes de escrever o guard),
não suposição sobre o que "deveria" estar referenciado.

## 17. Sprint 30.10 — Identity & Authentication Functional Audit

### 17.1 Escopo e método

Auditado com rigor de evidência explicitamente reforçado pelo owner sobre: cadastro e confirmação
de e-mail, reenvio, login/logout, esquecimento/redefinição de senha, tokens expirados/inválidos/
reutilizados, cookies de autenticação, invalidação de sessão após reset/troca de senha/
desativação, rate limiting, fronteiras cross-user/cross-session, mensagens de falha localizadas,
exposição de informação sensível em UI/respostas/logs, e integração de autenticação/autorização no
host Web. `INV-009` (fatia Identity/Auth) passa a `VERIFIED`.

Cada handler de Application (`IdentityHandlers.cs`, `AuthenticationHandlers.cs`), a implementação
real de hashing de senha (`Pbkdf2PasswordService` — PBKDF2-SHA256, 120.000 iterações, salt de 16
bytes aleatório, comparação em tempo constante via `CryptographicOperations.FixedTimeEquals`), de
geração/hash de token (`SecureUserTokenService` — 256 bits de `RandomNumberGenerator`, SHA-256 antes
de persistir), o throttle em memória (`MemoryIdentityRequestThrottle`), o rate limiter de login
(`LoginRateLimiterFactory`), e a validação de cookie em `Program.cs` (`OnValidatePrincipal`, recarga
do `User` a cada requisição autenticada, rejeição por `NameIdentifier`/`SessionVersion` ausentes ou
divergentes, ou `!IsActive`) foram lidos integralmente nesta Sprint — não apenas referenciados a
partir de documentação anterior.

### 17.2 `BD30-F038` — guard de open-redirect duplicado e incompleto, corrigido

`LoginDestinationResolver.IsLocalPath` (canônico, já testado contra `//` e `/\`) e uma segunda
implementação local em `Login.razor` — usada para sanitizar `ReturnUrl` antes de embuti-lo no campo
oculto do formulário de login — divergiam: a local só rejeitava `//`, nunca a variante `/\` que
alguns navegadores normalizam para URL absoluta. A decisão real de redirecionamento em
`POST /auth/login` já usa a canônica completa (`LoginDestinationResolver.Resolve`), então o próprio
fluxo de login não era explorável por essa lacuna — mas era uma segunda fronteira de segurança,
divergente e incompleta, sem motivo para existir separadamente. `Login.razor` agora reutiliza
`LoginDestinationResolver.IsLocalPath` diretamente; a implementação local foi removida. Coberto
pelos testes já existentes e exaustivos de `LoginDestinationResolverTests` (incluindo o caso
`/\example.com`); os 13 testes de `LoginTests`/`LoginDestinationResolverTests` passam.

### 17.3 `BD30-F039` — confirmação de e-mail duplamente enviada, mostrando "já usado" no primeiro clique legítimo

Investigação dirigida por evidência: ao escrever o primeiro teste E2E real (Chromium) que
efetivamente atravessa um link de confirmação (fechando `BD30-F018`), a jornada falhou
consistentemente mostrando "Link já utilizado" em vez de "E-mail confirmado" — no primeiro e único
uso do link, nunca reutilizado pelo teste.

**Causa raiz confirmada:** `ConfirmEmail.razor` herda `@rendermode="InteractiveServer"` do
`<Routes @rendermode="InteractiveServer" />` global em `App.razor`. Um componente interativo do
Blazor Server ainda pré-renderiza estaticamente uma vez antes de o circuito interativo assumir —
`OnInitializedAsync` roda nas duas passagens. A primeira (estática) enviava `ConfirmEmailCommand` e
confirmava o e-mail corretamente; a segunda (interativa), idêntica, era corretamente rejeitada pela
proteção contra replay de token de `UserToken.EnsureCanBeUsed` — mas é o resultado dessa SEGUNDA
chamada que o navegador do usuário real efetivamente exibe (a passagem interativa substitui o HTML
estático). Um usuário real, no seu primeiro e único clique legítimo, sempre veria a mensagem de link
já usado — apesar de sua conta ter sido corretamente confirmada.

Esse defeito era invisível a toda a cobertura existente: `EmailConfirmationIntegrationTests`
invoca o handler MediatR diretamente (nunca duas vezes); `IdentityFlowLocalizationIntegrationTests`
usa `HttpClient.GetStringAsync` (observa só a passagem estática, nunca a interativa). Nenhum teste
de unidade, bUnit ou integração HTTP deste repositório simula a transição prerender-estático →
interativo real do Blazor Server — só um navegador genuíno a exercita. Isso confirma exatamente a
premissa de `BD30-F018`.

**Correção mínima:** a validação de token ausente (sem efeito colateral, sem risco de replay)
continua rodando nas duas passagens. Só o envio mutante de `ConfirmEmailCommand` foi adiado para a
passagem interativa, via `if (!RendererInfo.IsInteractive) { return; }` — o padrão recomendado pela
própria documentação da Microsoft para efeito colateral não-idempotente em `OnInitializedAsync` de
componente pré-renderizado. A passagem estática passa a exibir o estado "Processing" padrão, que já
é a UI de espera correta.

**Efeito colateral no teste de integração existente:** `IdentityFlowLocalizationIntegrationTests.
ConfirmEmail_WithExpiredToken_RendersLocalizedMessage_NotTheRawDomainText` dependia, sem saber, do
próprio bug — sua asserção só passava porque a passagem estática (a única que `HttpClient` observa)
chegava a executar o envio mutante antes da correção. Removido e substituído por
`BeeDay.Web.Tests.Components.Identity.ConfirmEmailTests` (bUnit, com controle explícito de
`RendererInfo.IsInteractive` via `TestContext.Renderer.SetRendererInfo`), que agora prova
precisamente as duas passagens: a estática nunca envia o comando; a interativa envia, classifica e
localiza corretamente a mensagem de expiração (en-US e pt-BR), sem nunca vazar o texto bruto da
exceção de Domain.

### 17.4 Evidência adicional coletada (sem achado de correção)

- **Hashing de senha e token**: PBKDF2-SHA256/120k iterações/salt aleatório/comparação em tempo
  constante para senha; 256 bits de CSPRNG + SHA-256 antes de persistir para token de uso único —
  ambos apropriados às suas respectivas ameaças (senha de baixa entropia vs. token de alta
  entropia). Zero achado.
- **Uniformidade de mensagens de falha**: login rejeita conta inexistente, senha errada e e-mail
  não confirmado com a mesma mensagem genérica (`"Invalid email or password."`); reenvio de
  confirmação e esquecimento de senha completam silenciosamente para e-mail inexistente/já
  confirmado/inativo — nenhum dos três vaza existência de conta por diferença de resposta. Zero
  achado.
- **Invalidação de sessão**: `ResetPasswordCommandHandler`, `ChangeCurrentUserPasswordCommandHandler`
  e `User.SetActive(false)` chamam `InvalidateSessions()`; `OnValidatePrincipal` recarrega o `User`
  e recompara `SessionVersion` a cada requisição autenticada (não just no login), então o efeito é
  imediato na próxima requisição, não apenas na próxima sessão. Zero achado de correção — ver
  `BD30-F040` para a lacuna de alcançabilidade de `SetActive`.
- **Logs**: `Authentication.LoginFailed`/`LoginSucceeded`/`LogoutSucceeded` registram `UserId`/
  `TraceId`, nunca e-mail/senha em texto puro; `DevelopmentEmailSender`/outros pontos de log de
  e-mail já usam `EmailAddressLogMasking.Mask`. Mensagem técnica de exceção só aparece em
  `IsDevelopment()` (`GlobalExceptionHandler`, já auditado na Sprint 30.8/documentado em
  `docs/web/01-composition-root.md`). Zero achado novo.
- **Localização**: `AuthenticationResources`/`IdentityResources` en-US/pt-BR têm exatamente as
  mesmas chaves (16 e 54 respectivamente, diff nomes-a-nomes vazio). Zero achado.
- **Cross-user/cross-session**: tokens de confirmação/reset resolvem exclusivamente pelo hash
  armazenado, nunca por contexto do usuário autenticado — não há caminho para um token de um
  usuário afetar outro. Coberto por `EmailConfirmationIntegrationTests`/`PasswordResetIntegrationTests`
  já existentes. Zero achado novo.

### 17.5 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F040` (nova, baixa): `User.SetActive` está implementado e testado (invalida sessão
  corretamente), mas nenhum Command/handler de produto o alcança — não existe hoje autoatendimento
  nem fluxo administrativo de desativação de conta. Encaminhada à Sprint 30.11 (Profile, Onboarding,
  Account & Settings Audit) para decidir se é escopo de produto ausente ou aceito como está.
- `BD30-F041` (nova, baixa): o throttle de identidade e o rate limiter de login são ambos em
  memória, por instância de processo — corretos para a única instância IIS de HMG hoje, mas não
  sobreviveriam a uma implantação horizontalmente escalada (PRD em Azure, ainda não provisionada).
  Encaminhada à Sprint 30.22 (Security & Privacy Audit).

### 17.6 Implementação

- `src/BeeDay.Web/Components/Features/Authentication/Pages/Login.razor` — reutiliza
  `LoginDestinationResolver.IsLocalPath`, remove a implementação local incompleta.
- `src/BeeDay.Web/Components/Features/Identity/Pages/ConfirmEmail.razor` — adia o envio mutante de
  `ConfirmEmailCommand` para a passagem interativa via `RendererInfo.IsInteractive`.
- `tests/BeeDay.E2E.Tests/E2EWebApplicationFactory.cs` — captura de e-mail de desenvolvimento
  habilitada (diretório descartável por instância), com busca de token filtrada pelo destinatário
  real via o metadata `.json` companheiro (não apenas "o arquivo mais recente no diretório", que
  seria uma corrida real dado que a fixture/factory é compartilhada entre todos os métodos de teste
  da classe via `IClassFixture`).
- `tests/BeeDay.E2E.Tests/AccountLifecycleTests.cs` — novo teste
  `CreateAccount_ConfirmsEmailThroughARealLink_ThenUnlocksLogin`, fechando `BD30-F018`.
- `tests/BeeDay.Web.Tests/Components/Identity/ConfirmEmailTests.cs` (novo) — cobertura bUnit das
  duas passagens de render, substituindo a asserção de `IdentityFlowLocalizationIntegrationTests`
  que dependia do bug.
- `tests/BeeDay.Web.Tests/Integration/IdentityFlowLocalizationIntegrationTests.cs` — remove o teste
  agora inexprimível via `HttpClient` puro, com nota explícita apontando para a cobertura
  substituta.

Nenhuma mudança de política de produto de identidade; nenhuma mudança de contrato público de
Application/Domain; nenhuma mudança de schema.

### 17.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test .../AccountLifecycleTests` (antes da correção do double-render) | reprodução determinística: FAIL — "Link já utilizado" em vez de "E-mail confirmado" no primeiro uso |
| `dotnet test .../AccountLifecycleTests` (após a correção) | PASS, 5/5 |
| `dotnet test .../LoginTests` + `LoginDestinationResolverTests` | PASS, 13/13 |
| `dotnet test .../ConfirmEmailTests` (bUnit, novo) | PASS, 4/4 |
| `dotnet test tests/BeeDay.Web.Tests/...` completo | PASS, 870/870 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (1ª execução) | Debug/Application/Infrastructure/Web 100% PASS; E2E 188/194 — 6 falhas, todas `TimeoutException` de navegação/screenshot, em `Epic21ConsolidationTests`/`BrandTypographyTests`/`WalletTests`/`LoginExperienceTests` (nenhum arquivo tocado nesta Sprint); duração 14m43s (~2× o normal) |
| `dotnet test BeeDay.slnx` (2ª execução, retry) | E2E 193/194 — 1 falha nova, `InstitutionalPagesTests`, mesmo padrão de timeout; nenhuma repetição das 6 falhas anteriores |
| `dotnet test tests/BeeDay.E2E.Tests/...` (3ª execução, retry) | E2E 193/194 — 1 falha nova, `HomeTests`, mesmo padrão; nenhuma repetição |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.510/1.510 (117 Domain, 113 Application, 216 Infrastructure, 870 Web, 194 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

**Classificação da falha E2E em Debug:** `TRANSIENT/FLAKY` (ambiente), não `CHANGE-CAUSED`. Três
execuções, três conjuntos de testes diferentes falhando (nunca o mesmo teste duas vezes, nunca um
teste desta Sprint), sempre `TimeoutException` de navegação/screenshot — o padrão de contenção
LocalDB/Playwright já registrado em memória de projeto anterior a esta Sprint. A execução Release
subsequente rodou 194/194 sem qualquer falha. Todo teste de Identity/Auth (`AccountLifecycleTests`,
`LoginTests`, `LoginDestinationResolverTests`, `ConfirmEmailTests`) passou consistentemente nas três
execuções Debug e na execução Release.

### 17.8 Continuidade e entrega

Esta Sprint reforça por que a auditoria com browser real é irrecusável: dois dos três achados
corrigidos (`BD30-F038`, `BD30-F039`) eram invisíveis a qualquer teste unitário, bUnit padrão ou de
integração HTTP já existente — só surgiram ao efetivamente construir o primeiro teste Chromium que
atravessa um link de confirmação de ponta a ponta, exatamente o gap que `BD30-F018` apontava.
`BD30-F039` em particular é um defeito de produção real e confirmado: todo usuário que já clicou em
um link de confirmação de e-mail real via este código viu a mensagem incorreta de "link já usado" em
vez de "e-mail confirmado", embora a confirmação em si tenha sido aplicada corretamente. Nenhuma
mutação de banco HMG/produção foi executada ou é necessária para esta correção — o defeito era
inteiramente de apresentação (dupla execução de um efeito colateral idempotente-mas-não-realmente),
não de dado corrompido.

## 18. Sprint 30.11 — Profile, Onboarding, Account & Settings Audit

### 18.1 Escopo e método

Issue #208. Auditoria funcional completa de `/profile/create` (as duas etapas), `/onboarding/tutorial`,
`/account`/`/settings` (as três seções: Profile, Security, Preferences) e da state class dedicada de
cada jornada. Método explicitamente exigido pelo proprietário: round-trip de cada campo editável até
seu Command/Handler e de volta; recuperação de estado inválido/parcial; consistência de idioma entre
sessões/dispositivos; existência de fonte de verdade duplicada (localStorage, claims obsoletas em
cookie); comportamento em conta já existente (reentrada em `/profile/create`, `/onboarding/tutorial`,
`/welcome`); cobertura mobile/desktop; lacunas de teste; e deriva de documentação. Revisão adicional
específica do achado encaminhado pela Sprint 30.10: `BD30-F040` (`User.SetActive` inalcançável).

Toda a lógica de Command/Handler de User (`UserHandlers.cs`), as três seções de `Account.razor`, os
dois passos de `CreateProfile.razor`/`ProfileCreationState.cs`, `Tutorial.razor`, e a sincronização de
cultura autenticada (`AuthenticatedCultureSynchronizer.cs`) foram lidos integralmente.

### 18.2 `BD30-F044` — troca de e-mail sem reverificação de senha, primitivo de account takeover, corrigido (BLOCKER)

`UpdateCurrentUserAccountCommandHandler` persistia uma troca de e-mail sem exigir a senha atual e sem
resetar `IsEmailConfirmed`. Como `RequestPasswordResetCommandHandler` só envia o e-mail de redefinição
quando `IsEmailConfirmed == true`, uma conta cujo e-mail acabara de ser trocado continuava sendo
tratada como "confirmada" — mesmo para um endereço que o usuário nunca provou controlar. Cadeia de
exploração completa: uma sessão sequestrada (cookie roubado, XSS, dispositivo destravado) bastava para
trocar silenciosamente o e-mail para um endereço do atacante; a partir daí, o fluxo padrão de
"esqueci minha senha" ficava imediatamente disponível para o atacante, sem qualquer notificação ao
endereço antigo e sem qualquer prova de posse do novo. Isso é um primitivo completo de account
takeover, não um problema teórico — a mesma classe de defeito documentada pela OWASP como
reautenticação insuficiente em alteração de e-mail.

Correção mínima e completa, seguindo padrões já estabelecidos no próprio código:

- `User.UpdateAccount` (Domain): ao detectar que o e-mail normalizado realmente mudou, reseta
  `IsEmailConfirmed = false` e `EmailConfirmedAtUtc = null` — o mesmo estado de uma conta recém-criada.
- `UpdateCurrentUserAccountCommandHandler` (Application): exige e verifica a senha atual
  (`IPasswordService.Verify`) sempre que o e-mail muda — exatamente o mesmo padrão já usado por
  `ChangeCurrentUserPasswordCommandHandler` para trocar a senha — e, dentro da mesma transação, revoga
  qualquer token de confirmação ativo e emite um novo via `IEmailConfirmationIssuer` (o mesmo
  componente que `CreateAccountCommandHandler` usa no cadastro), enviando um e-mail de confirmação real
  ao endereço novo.
- `ProfileSection.razor`/`Account.razor.cs` (Web): novo campo "Current password" (mesmo padrão visual
  de `SecuritySection.razor`, `type="password"`, `autocomplete="current-password"`), exigido no
  cliente somente quando o e-mail realmente muda (rastreado via `_initialEmail`) — uma troca de Nome
  isolada nunca pede senha. Toast dedicado avisa que a nova senha... (a nova confirmação de e-mail
  precisa ser verificada no novo endereço).

Nenhuma mudança de contrato público quebra compatibilidade: `UpdateUserAccountRequest.CurrentPassword`
tem valor padrão `""`, e `BeeDayWebService.UpdateUserAsync` mantém um parâmetro opcional. Nenhuma
mutação de banco HMG/produção foi necessária — o defeito era inteiramente de lógica de aplicação,
sem dado corrompido a corrigir.

4 novos testes em `UserAccountHandlersTests.cs` provam: troca só de Nome não exige senha e mantém
`IsEmailConfirmed`; troca de e-mail com senha incorreta é rejeitada (e-mail não muda); troca de e-mail
sem senha é rejeitada; troca de e-mail com senha correta reseta a confirmação, revoga tokens antigos e
envia exatamente um e-mail de confirmação novo ao endereço novo.

### 18.3 `BD30-F045` — exceção não tratada ao concluir onboarding, corrigido

`Tutorial.razor.NextAsync`, no último slide, chamava `Store.CompleteOnboardingAsync()` sem
`try/catch`, e o componente não injetava `ToastService`. Uma falha (rede, 5xx transitório, sessão
expirada) se propagava como exceção não tratada dentro do circuito Blazor Server, sem qualquer
feedback ao usuário — o único caminho de salvamento do app sem esse tratamento; `Account.razor`,
`ProfileCreationState.CompleteProfileAsync` e todo o resto já seguiam o padrão
`catch (OperationCanceledException) when (...) { } catch (Exception ex) { Toast.ShowError(...) }`.
Corrigido replicando exatamente esse padrão, com um novo recurso `ToastCompleteErrorTitle` en-US/pt-BR/
neutro. Novo teste em `TutorialTests.cs` (`WhenCompletingOnboardingFails_...`) simula a falha via
`StubSender` e prova que um toast localizado aparece e o usuário permanece na última tela, em vez de
uma exceção não tratada.

### 18.4 `BD30-F043` — `ProfileCreationState` nunca propagava cancelamento, corrigido

Gap residual do `BD30-F035` (Sprint 30.8): aquela varredura buscou apenas `*.razor`/`*.razor.cs`, e
`ProfileCreationState.cs` é uma state class `.cs` simples (`AddScoped`, mesmo ciclo de vida de
`DashboardState`) — suas 4 chamadas a `BeeDayWebService` sempre usavam `CancellationToken.None`
implícito. Corrigido com o mesmo padrão já estabelecido: `CancellationTokenSource` de ciclo de vida do
circuito + `IDisposable` + token propagado em toda chamada + `catch (OperationCanceledException) when
(cancellation.IsCancellationRequested)`. O método `GetStatusAsync()` (código morto, zero chamadores
confirmados por busca em `src`/`tests`) foi removido no mesmo commit. 3 novos testes em
`ProfileCreationStateCancellationTests.cs`, espelhando `DashboardStateCancellationTests.cs`.

### 18.5 `BD30-F049` — cobertura de viewport ausente para Account/Tutorial/Etapa 2 do cadastro, corrigido

`LoginExperienceTests.CreateAccountMatchesPublicAuthenticationLayout` já provava a Etapa 1 (conta) do
cadastro em 3 larguras; nada provava a Etapa 2 (apelido), `/account` (as 3 seções) ou
`/onboarding/tutorial` — confirmado por busca de `SetViewportSizeAsync` em todo `BeeDay.E2E.Tests`
(15 arquivos usam o padrão) cruzada com os arquivos que tocam essas três rotas. Novo arquivo
`tests/BeeDay.E2E.Tests/AccountResponsiveLayoutTests.cs`: 3 testes × 2 larguras (390×844, 1280×800),
mesmo padrão de `LoginExperienceTests` (`document.documentElement.scrollWidth >
document.documentElement.clientWidth`). Executados via browser Chromium real contra as três rotas —
6/6 passaram, nenhum overflow horizontal encontrado em nenhuma largura.

### 18.6 `BD30-F040` — determinação: fluxo de produto ausente, não código morto

Sprint 30.10 encaminhou a esta Sprint a pergunta: `User.SetActive` é código morto, fluxo de produto
ausente, capacidade reservada intencionalmente, ou outro estado documentado? A auditoria funcional
completa desta Sprint — inspeção direta de `Account.razor` e das três seções de Settings, e busca por
qualquer Command/handler alcançável que chame `SetActive` — confirma que **nenhuma** entrada de
produto (autoatendimento em Settings ou fluxo administrativo) existe hoje. `ProfileSection.razor` só
edita Nome/E-mail (mais o novo campo de senha do `BD30-F044`); `SecuritySection.razor` só troca senha;
`PreferencesSection.razor` só ajusta idioma/tema. Nenhum botão, link ou rota "excluir conta"/
"desativar conta" existe em lugar nenhum do `src/BeeDay.Web` auditado nesta e nas Sprints anteriores.

Classificação final: **fluxo de produto ausente**, não código morto — o método de Domain está correto,
testado, e o guard de segurança em `OnValidatePrincipal` funciona; simplesmente não há hoje nenhuma
ação real de usuário que o alcance. Não é capacidade "reservada" documentada em nenhum ADR ou
documento de produto encontrado.

Consistente com a instrução explícita do proprietário para esta Sprint ("não inventar uma política de
produto de desativação"): esta auditoria **não** decide se/como construir autoatendimento ou fluxo
administrativo de desativação — isso é uma decisão de política de produto, fora da autoridade de uma
auditoria. O achado permanece `OPEN`, reclassificado de "Sprint 30.11" para "decisão do proprietário"
na tabela de achados — não há mais trabalho de auditoria a fazer aqui até que o proprietário decida se
quer essa capacidade.

### 18.7 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F047` (nova, média): `AuthenticatedCultureSynchronizer.SynchronizeAtLoginAsync` implementa,
  deliberadamente e por documentação existente, a regra "um cookie `BeeDay.Culture` explícito sempre
  vence naquela sessão, e a conta converge para ele". Consequência não coberta pelo critério de aceite
  desta auditoria ("preferência de idioma permanece consistente entre sessões autenticadas"): trocar o
  idioma em Settings no Dispositivo A e depois logar no Dispositivo B com um cookie `BeeDay.Culture`
  desatualizado silenciosamente reverte a preferência de conta recém-alterada de volta ao valor do
  cookie antigo, sem qualquer aviso. Comportamento documentado, não um bug de implementação — mas
  conflita com o critério de aceite conforme escrito. Decisão de produto necessária (cookie deveria
  ceder à conta no login? cookies não deveriam sobreviver a uma troca de idioma via conta?).
  Encaminhada à Sprint 30.20 (UX, responsividade e localização), que já é proprietária de `INV-016`.
- `BD30-F048` (nova, baixa): `Program.cs` grava `ClaimTypes.Name`/`ClaimTypes.Email` no cookie
  `BeeDay.Auth` no login (até 14 dias sob "remember me"), mas nunca os atualiza após uma edição de
  Nome/E-mail em Account — hoje inofensivo (busca confirma que nada em
  `AuthenticatedUserInitializer.cs`/`HttpCurrentUserContext.cs`/`OnValidatePrincipal` lê essas duas
  claims de volta), mas é PII potencialmente desatualizada sentada num cookie de longa duração.
  Encaminhada à Sprint 30.22 (Segurança e privacidade), que já é proprietária de `INV-017`.
- Achado de UX não-bloqueante, corrigido diretamente (sem necessidade de nova entrada no Ledger):
  `SecurityHint` (Account → Security) não avisava que trocar a senha desconecta todos os dispositivos,
  incluindo o atual — comportamento correto e já intencional (`InvalidateSessions()`), só faltava a
  mensagem proativa. Texto adicionado em en-US/pt-BR/neutro.

### 18.8 Implementação

- `src/BeeDay.Domain/Entities/User.cs` — `UpdateAccount` reseta `IsEmailConfirmed`/
  `EmailConfirmedAtUtc` quando o e-mail muda de fato (`BD30-F044`).
- `src/BeeDay.Application/Features/Users/Handlers/UserHandlers.cs` — `UpdateCurrentUserAccountCommandHandler`
  reescrito: reverificação de senha, transação, revogação/emissão de token de confirmação, envio de
  e-mail (`BD30-F044`).
- `src/BeeDay.Application/Features/Users/Requests/UpdateUserAccountRequest.cs` — novo campo opcional
  `CurrentPassword`.
- `src/BeeDay.Application/Features/Users/Validation/CommandValidators.cs` — `MaximumLength` defensivo
  em `CurrentPassword` (sem `NotEmpty`, já que é condicional).
- `src/BeeDay.Web/Services/BeeDayWebService.cs` — `UpdateUserAsync` propaga `currentPassword`.
- `src/BeeDay.Web/Components/Features/Account/Components/ProfileSection.razor`,
  `src/BeeDay.Web/Components/Features/Account/Models/ProfileFormModel.cs`,
  `src/BeeDay.Web/Components/Features/Account/Pages/Account.razor` — campo de senha atual condicional,
  toast dedicado de confirmação pendente, guard client-side (`BD30-F044`); hint de logout em toda troca
  de senha (`SecurityHint`).
- `src/BeeDay.Web/Components/Features/Onboarding/Pages/Tutorial.razor` — `try/catch` +
  `ToastService` em `NextAsync` (`BD30-F045`).
- `src/BeeDay.Web/Components/Features/ProfileCreation/State/ProfileCreationState.cs` — cancelamento
  propagado, `GetStatusAsync()` morto removido (`BD30-F043`).
- `src/BeeDay.Web/Components/Features/Account/AccountResources.*.resx`,
  `src/BeeDay.Web/Components/Features/Onboarding/OnboardingResources.*.resx` — novos recursos en-US/
  pt-BR/neutro.
- `docs/web/04-feature-components.md` — corrige `/daily` → `/profile` na descrição de `Tutorial.razor`
  (`BD30-F046`).
- `tests/BeeDay.Application.Tests/UserAccountHandlersTests.cs` — 4 novos testes (`BD30-F044`).
- `tests/BeeDay.Application.Tests/FeatureServicesTests.cs` — novo teste
  `CompleteUserProfileHandler_RejectsASecondProfileCompletion`, fechando a única lacuna real que a
  auditoria confirmou nos handlers de perfil (os outros dois cenários já eram cobertos).
- `tests/BeeDay.Web.Tests/Components/Onboarding/TutorialTests.cs` — novo teste de falha (`BD30-F045`).
- `tests/BeeDay.Web.Tests/Components/ProfileCreation/ProfileCreationStateCancellationTests.cs` (novo)
  — 3 testes (`BD30-F043`).
- `tests/BeeDay.E2E.Tests/AccountResponsiveLayoutTests.cs` (novo) — 3 testes × 2 larguras (`BD30-F049`).

Nenhuma mudança de política de produto de identidade além do que `BD30-F044` exige por segurança;
nenhuma mudança de schema (`IsEmailConfirmed`/`EmailConfirmedAtUtc` já existiam); nenhuma mutação de
banco HMG/produção.

### 18.9 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Application.Tests/...` | PASS, 117/117 |
| `dotnet test tests/BeeDay.Web.Tests/...` | PASS, 873/873 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter AccountResponsiveLayoutTests` | PASS, 6/6 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter TutorialTests` | PASS, 8/8 |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.525/1.525 (117 Domain, 117 Application, 216 Infrastructure, 875 Web, 200 E2E) — execução limpa, 0 falhas, ~7min de E2E |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.525/1.525 — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 18.10 Continuidade e entrega

`BD30-F044` é o achado mais severo desta EPIC até agora: um primitivo completo de account takeover
alcançável por qualquer sessão sequestrada, não uma fraqueza teórica. Como `BD30-F038`/`BD30-F039` na
Sprint 30.10, só se tornou visível auditando o fluxo completo de ponta a ponta (Domain → Application →
Web → e-mail) em vez de qualquer camada isolada — nenhum teste unitário anterior falhava, porque
nenhum testava o comportamento de confirmação em uma troca de e-mail. Nenhuma mutação de banco HMG/
produção foi executada ou é necessária: o defeito era inteiramente de lógica de aplicação (nenhum
usuário real precisa de correção retroativa, já que `IsEmailConfirmed` só é lido para decidir se um
e-mail de redefinição é enviado — não há histórico incorreto a reconciliar).

## 19. Sprint 30.12 — Habits Complete Audit

### 19.1 Escopo e método

Issue #209. Auditoria funcional completa de Habit: CRUD (criar/editar/excluir), direção (Positive/
Negative/Both), dificuldade, `ResetCounter`, registro positivo/negativo, isolamento de posse,
integração com XP/Experience, eficiência de consulta, feedback de mutação na UI, e paridade de
documentação. `Habit.cs`, `HabitCommandHandlers.cs`, `EfHabitRepository.cs`,
`ReorderActivitiesCommandHandler.cs`, `UserExperience.cs`, `EfUserRepository.cs`, `HabitCard.razor`/
`HabitEditorModal.razor` e seus code-behind, `DashboardState.cs`, e todos os testes relacionados a
Habit em Domain/Application/Infrastructure/Web/E2E foram lidos integralmente, junto com
`docs/domain/habit.md` e a seção de Habit em `docs/web/04-feature-components.md`.

Nenhum achado aberto pré-existente estava atribuído a esta Sprint no Ledger — auditoria partiu de
evidência nova, não de um backlog a fechar.

### 19.2 Achados confirmados — corretos, sem defeito

- **Isolamento de posse**: `HabitLookup.RequireExistsAsync` escopa toda leitura por `userId` antes de
  qualquer mutação; `EfHabitRepository` refiltra por `UserId` a nível SQL em `UpdateAsync`/`GetAsync`/
  `ReorderAsync`; `ReorderActivitiesCommandHandler.EnsureOwned` rejeita um payload de reorder contendo
  o Id de Habit de outro usuário. Coberto por `HabitTaskManagementHandlersTests`,
  `MultiUserIsolationTests` e um teste de integração real contra LocalDB
  (`MultiUserIsolationIntegrationTests.User_CannotRegisterAnotherUsersHabit`).
- **Transação de XP atômica**: `RegisterHabitPositiveCommandHandler` incrementa o contador e concede
  XP na mesma transação (`EfUnitOfWork`); uma falha em qualquer ponto reverte ambos via dispose de
  transação não commitada. Habit é deliberadamente isento da deduplicação por origem que Task/Todo/
  Project usam (`UserExperience.cs` — cada clique deve premiar XP independentemente, por design,
  coberto por `ExperienceRewardPipelineTests.Positive_habit_grants_experience_for_each_distinct_occurrence`).
  **Não é** um caso análogo a `BD30-F030` (Sprint 30.7) — lá a persistência falhava silenciosamente;
  aqui o comportamento de não-dedução é intencional e testado.
- **Cancelamento e feedback de mutação**: `DashboardState` propaga um `CancellationTokenSource` de
  ciclo de vida do circuito (padrão `BD30-F035`) por toda operação de Habit; toasts de sucesso/erro
  cobrem criar/editar/excluir/reordenar (registrar positivo/negativo intencionalmente não emite toast
  de sucesso — o pulso de XP na barra de Experience é o único feedback positivo, consistente com o
  mesmo padrão em Task/Todo).
- **Eficiência de consulta na leitura**: `EfDashboardReadService` carrega Habits com uma única query
  `AsNoTracking()`; `ExperienceEntry` nunca é junto/carregado na leitura do dashboard (só os campos
  escalares pré-computados `TotalExperience`/`CurrentLevel` são usados). Sem N+1 nem dado não
  relacionado carregado.
- **Estado vazio**: `/daily` renderiza um card de estado vazio apropriado quando `ActiveCount == 0`.
- **Documentação**: `docs/domain/habit.md` e a seção de Habit em `docs/web/04-feature-components.md`
  batem exatamente com `Habit.cs`/`HabitCommandHandlers.cs`/`HabitEditorModal.razor.cs`/
  `HabitVisualState.cs`, incluindo a ausência documentada (não inventada) de qualquer mecanismo de
  reset para `ResetCounter` — confirma que `BD30-F050` é uma lacuna funcional real do produto, não uma
  deriva de documentação.

### 19.3 `BD30-F050` — `HabitResetCounter` sem efeito comportamental, decisão de produto necessária

`ResetCounter` (Daily/Weekly/Monthly) é validado no Domain, persistido, exposto no editor de UI e
documentado — mas nenhuma linha de código jamais reseta `PositiveCount`/`NegativeCount` com base em
tempo decorrido ou fronteira de calendário; `RegisterPositive`/`RegisterNegative` só incrementam, para
sempre, e não existe job/`BackgroundService` relacionado a Habit no único `BackgroundTaskWorker` do
app. Um usuário que cria um hábito "Beber água" com reset Diário, esperando o saldo zerar a cada novo
dia (comportamento que `docs/domain/habit.md` descreve como inspiração de apps de hábito comparáveis),
vê o saldo acumular indefinidamente — o campo é hoje decoração de UI sem efeito.

Esta auditoria **não** implementa a semântica de reset: decidir quando o reset ocorre (fuso horário?
cultura? no próximo registro após a fronteira? via job agendado?) é uma decisão de produto genuína,
com múltiplas opções válidas, fora da autoridade de uma auditoria — consistente com a instrução do
proprietário de não inventar política de produto (mesmo princípio já aplicado a `BD30-F040` na Sprint
30.11). Registrado com evidência completa; nenhuma Sprint futura foi atribuída até que o proprietário
defina a semântica esperada.

### 19.4 `BD30-F051` — crescimento ilimitado de `ExperienceEntry`, recarregado em toda mutação de User

`EfUserRepository.UpdateAsync` recarrega **todas** as `ExperienceEntry` de um usuário antes de
qualquer mutação — não só em registros de Habit, mas em qualquer chamada que passe por esse método
(confirmação de e-mail, redefinição de senha, troca de nome/e-mail via `BD30-F044`). Habit é a única
origem de XP deliberadamente isenta de deduplicação, então é a única cujo volume de linhas cresce sem
limite por usuário ativo ao longo do tempo; nenhuma estratégia de arquivamento, expurgo ou paginação
existe. A lista de Habits em `/daily` também não é paginada/virtualizada, e nenhum teste da suíte cria
mais de 2 Habits para o mesmo usuário — este cenário nunca foi exercitado em escala real. Sem
impacto observado hoje (nenhuma evidência de degradação em HMG), mas é uma lacuna de performance
real, direta consequência do design correto (não-dedução) de Habit. Encaminhada à Sprint 30.21
(Performance), que já é proprietária de `INV-019`.

### 19.5 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F052` (nova, baixa): `ActivityAttribute` não tem controle de UI em nenhum dos 4 editores de
  atividade (Habit/Task/Todo/Project) — cross-cutting, não específico de Habit. Encaminhada à Sprint
  30.19 (Design System), que já é proprietária de `INV-015`.
- `BD30-F053` (nova, baixa): `EfHabitRepository.RemoveAsync` não reverifica `UserId` isoladamente,
  depende do único call site já ter verificado posse — sem exploração ativa hoje, mas uma lacuna de
  defesa em profundidade. Encaminhada à Sprint 30.22 (Segurança e privacidade), que já é proprietária
  de `INV-017`.
- Reconfirmação (sem novo ID): `BD30-F037` (Sprint 30.9, `OPEN`, atribuída à 30.20) — cards individuais
  do Dashboard sem `Disabled` vinculado a `State.IsBusy` — já cobre exatamente o padrão observado nos
  botões +/− de `HabitCard.razor`; a proteção real contra duplo-clique existe (`DashboardState.
  ExecuteAsync` seta `IsBusy` de forma síncrona antes do primeiro `await`), mas depende inteiramente
  da garantia de despacho single-threaded do Blazor Server, sem reforço visual. Nenhuma entrada nova
  necessária.

### 19.6 Implementação

- `tests/BeeDay.Domain.Tests/HabitTests.cs` — 2 novos testes: `RegisterNegative_IncrementsNegativeCounter`
  e o par que faltava de `RegisterPositive_DoesNotChangeNegativeOnlyHabit`
  (`RegisterNegative_DoesNotChangePositiveOnlyHabit`).
- `tests/BeeDay.Web.Tests/Components/Habits/HabitEditorModalTests.cs` — 3 novos testes: prova que
  `OnSave` recebe exatamente os campos editados (Título/Notas/Dificuldade/ResetCounter/Direção), e que
  `TogglePositive`/`ToggleNegative` de fato alternam `Direction` (não só a classe CSS `active`).
- `tests/BeeDay.E2E.Tests/HabitAndTaskTests.cs` — 3 novos testes via Chromium real: registrar negativo
  atualiza o saldo; editar um Habit persiste o novo título após reload; excluir um Habit (via
  confirmação) o remove do board e do reload subsequente. Antes desta Sprint, a única cobertura E2E de
  Habit era criar + registrar positivo.

Nenhuma mudança de comportamento de produção nesta Sprint — os dois achados material (`BD30-F050`,
`BD30-F051`) exigem decisão do proprietário/Sprint futura antes de qualquer correção; o trabalho desta
Sprint é inteiramente fechamento de lacunas de teste sobre comportamento já correto, mais achados
registrados com evidência.

### 19.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Domain.Tests/... --filter HabitTests` | PASS, 6/6 |
| `dotnet test tests/BeeDay.Web.Tests/... --filter HabitEditorModalTests` | PASS, 14/14 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter HabitAndTaskTests` | PASS, 9/9 |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `dotnet test BeeDay.slnx` (Debug, completo, 1ª execução) | 1.527/1.528 — 1 falha: `LoginExperienceTests.CreateAccountMatchesPublicAuthenticationLayout(width: 390, height: 844)`, `TimeoutException` em `GotoAsync("/profile/create")`. Arquivo não tocado nesta Sprint (só Habit foi alterado) |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter LoginExperienceTests` (retry) | PASS, 10/10 — inclusive o caso exato que falhou antes |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.533/1.533 (119 Domain, 117 Application, 216 Infrastructure, 878 Web, 203 E2E) — execução limpa, 0 falhas |

**Classificação da falha Debug:** `TRANSIENT/FLAKY` (mesmo padrão já registrado em `BD30-F042`), não
`CHANGE-CAUSED`. `LoginExperienceTests.cs` não foi tocado por esta Sprint (escopo desta Sprint é
inteiramente Habit); o retry imediato do mesmo teste, incluindo o caso exato que falhou, passou
100% (10/10). Nenhuma nova investigação de causa raiz feita aqui — `BD30-F042` já é proprietário
dessa investigação (Sprint 30.24).

### 19.8 Continuidade e entrega

Esta Sprint confirma que a arquitetura de Habit (isolamento de posse, atomicidade de XP, cancelamento,
eficiência de leitura) está correta e bem testada onde já era exercitada — os dois achados materiais
(`BD30-F050`, `BD30-F051`) são lacunas de completude de produto/performance, não bugs de correção
ativos, e ambos exigem uma decisão fora da autoridade de auditoria antes de qualquer implementação:
`BD30-F050` porque a semântica de reset tem múltiplas opções de design válidas; `BD30-F051` porque
qualquer estratégia de arquivamento/expurgo de `ExperienceEntry` toca infraestrutura compartilhada
usada por toda mutação de User, não só Habit, e afeta decisões de retenção de histórico de XP que vão
além do escopo desta Sprint. Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

## 20. Sprint 30.13 — Tasks & To-Dos Complete Audit

### 20.1 Escopo e método

Issue #210. Auditoria funcional completa de `RecurringTask` (Task) e `Todo`, tratados separadamente
onde seu comportamento diverge (recorrência vs. data de vencimento) e em conjunto onde compartilham
implementação (`Activity`, `ActivityCard.razor`, `DashboardState`). `RecurringTask.cs`, `Todo.cs`,
`TaskCommandHandlers.cs`, `TodoCommandHandlers.cs`, `EfRecurringTaskRepository.cs`,
`EfProjectRepository.cs`, `ReorderActivitiesCommandHandler.cs`, `TaskEditorModal`/`TodoEditorModal`
(razor + code-behind), `DashboardModalState.cs`, `DashboardState.cs`, `ActivityCard.razor`, `Home.razor`
e todos os testes relacionados a Task/Todo em Domain/Application/Infrastructure/Web/E2E foram lidos
integralmente, cruzados com o Ledger existente (`BD30-F019`, `BD30-F020`, já atribuídos a esta Sprint e
à 30.14) para não reabrir achados já rastreados sob um ID novo.

### 20.2 Achados confirmados — corretos, sem defeito

- **Round-trip de CRUD**: todo campo editável de Task (Título/Notas/Repeat) e Todo (Título/Notas/
  Projeto/Data de vencimento) é hidratado corretamente no editor e persistido de volta —
  `DashboardModalState.OpenTask/OpenTodo` e `BeeDayWebService.AddTaskAsync/UpdateTaskAsync/
  AddTodoAsync/UpdateTodoAsync` confirmados por leitura completa.
- **Idempotência de conclusão e dedução de XP**: `ToggleTaskCommandHandler`/`ToggleTodoCommandHandler`
  seguem o padrão de transação atômica já corrigido na Sprint 30.7 (`BD30-F030`); a dedução por origem
  `(UserId, SourceType, ReferenceId, RewardType)` (`UserExperience.cs`) é permanente e testada —
  desmarcar e marcar de novo o mesmo Task/Todo concede XP só uma vez
  (`ExperienceRewardPipelineTests.Completing_task_twice_grants_experience_only_once`,
  `Completing_last_todo_grants_todo_and_project_rewards_once`). Comportamento correto e intencional,
  distinto por design da isenção de Habit — mas ver `BD30-F054` para a interação inesperada que isso
  cria quando combinado com a lacuna de recorrência de Task.
- **Isolamento de posse**: confirmado em toda a superfície de mutação de Task/Todo, incluindo reorder
  (`ReorderActivitiesCommandHandler.EnsureOwned` para Task; resolução por Project do chamador para
  Todo) — `EfProjectRepository.UpdateTodoAsync/RemoveTodoAsync/MoveTodoAsync` de fato escopam por
  `userId` diretamente na query SQL, mais defendido que o padrão `RemoveAsync`-só-por-`Id` de Habit/
  Task (`BD30-F053`). Provado por `MultiUserIsolationIntegrationTests` (real MediatR + real EF).
- **Cascata Project → Todo**: `OnDelete(DeleteBehavior.Cascade)` remove Todos ao excluir o Project
  proprietário; mover um Todo entre Projects do mesmo usuário é atômico
  (`UpdateTodoCommandHandler`, transação explícita quando `ProjectId` muda).
- **Freshness de filtros pós-mutação**: `DashboardState.ReloadAsync()` é chamado após toda mutação;
  `FilteredTasks`/`FilteredTodos` são computados ao vivo, sem coleção derivada que possa ficar obsoleta.
- **Cancelamento e feedback de UI**: padrão `BD30-F035` já aplicado a toda operação de Task/Todo.
- **Documentação**: `docs/domain/recurring-task.md` já documenta corretamente que `ToggleCompletion`
  usa a implementação padrão de `Activity` sem override — confirma que `BD30-F054` é uma lacuna
  funcional real, não deriva de documentação. `Todo` é intencionalmente não-Aggregate-Root e não tem
  `todo.md` próprio, por design já documentado em `docs/domain/README.md`.

### 20.3 `BD30-F019` — ausência de E2E de To-Do, corrigido

Nenhum teste Chromium jamais exercitou Todo — criação, edição, conclusão e exclusão eram provadas
apenas por handlers de Application e por `TodoEditorModalTests` (bUnit). Como um Todo pertence sempre
a um Project, e "Add To-Do" só existe dentro do workspace do Project (`ProjectWorkspace.razor`), a
jornada real de um usuário passa por: criar Project → abrir seu workspace → adicionar o To-Do lá →
manipulá-lo de volta no board `/daily` (onde `ActivityCard` para Todo vive, igual a Habit/Task).

Novo arquivo `tests/BeeDay.E2E.Tests/TodoLifecycleTests.cs`: 3 testes via Chromium real —
criar um Todo dentro de um Project e alternar sua conclusão; editar um Todo com persistência do novo
título após reload; excluir um Todo (via confirmação) com remoção confirmada do board e do reload
subsequente. Todos passaram na primeira execução real contra o app completo.

### 20.4 `BD30-F054` — `RecurringTask.Repeat` sem efeito comportamental, decisão de produto necessária

Mesma classe exata de `BD30-F050` (Habit), confirmada de forma independente para Task:
`RecurringTask.Repeat` é validado, persistido, editável e documentado, mas `RecurringTask` não
sobrescreve `ToggleCompletion()` — herda o flip de booleano padrão de `Activity` — e nenhum código do
repositório (incluindo o único `BackgroundTaskWorker` do app) jamais reabre uma Task com base em
fronteira de calendário. Uma Task "Diária" completada fica `Completed = true` para sempre até ser
desmarcada manualmente.

Efeito colateral agravante, novo nesta Sprint: como a dedução de XP por origem é **correta e
intencional** para Task (ao contrário de Habit — ver `§20.2`), e é permanente por design, uma Task
"recorrente" completada uma vez **nunca mais concede XP**, mesmo que o usuário desmarque e marque de
novo manualmente simulando "fazer de novo amanhã". Dois mecanismos, cada um correto isoladamente
(sem auto-reabertura + dedução permanente), se combinam para anular o propósito de gamificação
específico de uma Task marcada como recorrente.

Como em `BD30-F050`, esta auditoria não implementa a semântica de reabertura — decidir quando ela
ocorre (fuso? cultura? no próximo carregamento após a fronteira? via job agendado? e se sim, uma nova
"ocorrência" deveria contar como uma nova chave de dedução de XP?) é uma decisão de produto genuína,
possivelmente a **mesma** decisão de `BD30-F050` já que ambos os campos (Habit `ResetCounter`, Task
`Repeat`) representam o mesmo conceito de produto sob nomes diferentes. Registrado com evidência
completa; nenhuma Sprint futura atribuída até definição do proprietário.

### 20.5 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F055` (nova, baixa): `Todo.DueDate` não tem efeito funcional — sem indicador de atraso, sem
  ordenação por data (a lista usa só `Position`/drag-and-drop), e completar um Todo atrasado se
  comporta identicamente a completar um no prazo. Decisão de escopo de produto, não bug de data/fuso
  (a conversão em si já é testada e correta sob pt-BR). Encaminhada à Sprint 30.20 (UX), que já é
  proprietária de `INV-016`.
- Reconfirmação (sem novo ID): `BD30-F052` (`ActivityAttribute` sem controle de UI) — confirmado
  aplicável a Task e Todo também; o campo round-tripa corretamente na Application/Domain mas nenhum
  dos dois editores tem controle para defini-lo.
- Reconfirmação (sem novo ID): `BD30-F037` (cards do Dashboard sem `Disabled` vinculado a
  `State.IsBusy`) — confirmado aplicável ao checkbox de conclusão de `ActivityCard`, compartilhado por
  Task/Todo/Habit/Project. Proteção real contra duplo-clique existe (`DashboardState.ExecuteAsync`),
  só falta o reforço visual.
- Reconfirmação (sem novo ID): `BD30-F053` (repositório não reverifica `UserId` em `RemoveAsync`) —
  confirmado aplicável a `EfRecurringTaskRepository.RemoveAsync` também, com o mesmo comentário no
  código apontando para o padrão de Habit. Observação positiva: `EfProjectRepository.RemoveTodoAsync`
  **não** tem essa lacuna — já escopa por `userId` diretamente na query, mais defendido que o padrão
  de Habit/Task.

### 20.6 Implementação

- `tests/BeeDay.Web.Tests/Components/Tasks/TaskEditorModalTests.cs` — novo teste
  `Save_PassesTheEditedFieldsToOnSave`, provando que `OnSave` recebe exatamente Título/Notas/Repeat
  editados.
- `tests/BeeDay.Web.Tests/Components/Todos/TodoEditorModalTests.cs` — novo teste equivalente para
  Título/Notas/Projeto/Data de vencimento.
- `tests/BeeDay.Web.Tests/Integration/MultiUserIsolationIntegrationTests.cs` —
  `User_CannotUpdateOrDeleteAnotherUsersTask` renomeado para
  `User_CannotToggleUpdateOrDeleteAnotherUsersTask` e estendido para também enviar
  `ToggleTaskCommand` como usuário não-proprietário (única mutação de Task sem essa cobertura;
  Todo já a tinha).
- `tests/BeeDay.E2E.Tests/HabitAndTaskTests.cs` — 2 novos testes: editar uma Task persiste o novo
  título após reload; excluir uma Task (via confirmação) a remove do board e do reload subsequente
  (mesmo padrão adicionado para Habit na Sprint 30.12; Task só tinha criar + alternar conclusão).
- `tests/BeeDay.E2E.Tests/TodoLifecycleTests.cs` (novo) — 3 testes, fechando `BD30-F019`.

Nenhuma mudança de comportamento de produção nesta Sprint — os dois achados materiais (`BD30-F054`,
`BD30-F055`) exigem decisão do proprietário/Sprint futura antes de qualquer correção; o trabalho desta
Sprint é inteiramente fechamento de lacunas de teste sobre comportamento já correto, mais achados
registrados com evidência.

### 20.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Web.Tests/... --filter TaskEditorModalTests\|TodoEditorModalTests` | PASS, 19/19 |
| `dotnet test tests/BeeDay.Web.Tests/... --filter MultiUserIsolationIntegrationTests` | PASS, 8/8 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter HabitAndTaskTests` | PASS, 11/11 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter TodoLifecycleTests` | PASS, 3/3 (primeira execução) |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.540/1.540 (119 Domain, 117 Application, 216 Infrastructure, 880 Web, 208 E2E) — execução limpa, 0 falhas |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.540/1.540 (119 Domain, 117 Application, 216 Infrastructure, 880 Web, 208 E2E) — execução limpa, 0 falhas |

### 20.8 Continuidade e entrega

`BD30-F054` reforça um padrão que já apareceu em `BD30-F050`: um campo de recorrência/reset totalmente
implementado do banco à UI, mas sem efeito comportamental real — desta vez com uma consequência mais
severa (bloqueio permanente de XP para o único tipo de atividade explicitamente rotulado como
"recorrente"). As duas Sprints seguidas encontrando a mesma classe de lacuna, em duas entidades
diferentes, sugerem fortemente que ambas compartilham a mesma causa raiz de produto (um conceito de
"recorrência"/"reset" projetado na UI/Domain antes de sua semântica de runtime ter sido decidida) e
provavelmente merecem uma única decisão do proprietário cobrindo os dois. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária.

## 21. Sprint 30.14 — Projects & Project Workspace Audit

### 21.1 Escopo e método

Issue #211. Auditoria funcional completa de Project e seu workspace: CRUD, cálculo de progresso,
ordenação, filtro de contexto, impacto de exclusão em views relacionadas, isolamento de posse,
suposições de UI otimista, cancelamento/feedback, e estados vazio/muitos-projects/recurso-deletado.
`Project.cs`, `ProjectCommandHandlers.cs`, `EfProjectRepository.cs`, `ReorderActivitiesCommandHandler.cs`,
`ProjectEditorModal`/`ProjectWorkspace` (razor + code-behind), `DashboardModalState.cs`,
`DashboardState.cs`, `ProjectContextFilter.razor`, `Home.razor`, e todos os testes relacionados a
Project em Domain/Application/Infrastructure/Web/E2E foram lidos integralmente. Cruzado com
`BD30-F020` (já atribuído a esta Sprint) e revisão explícita de `tests/BeeDay.E2E.Tests/
TodoLifecycleTests.cs` (Sprint 30.13) para determinar exatamente o que esse achado ainda deixava sem
prova.

### 21.2 Achados confirmados — corretos, sem defeito

- **Isolamento de posse**: confirmado em toda a superfície de Project (Create/Update/Delete/Reorder),
  tanto na guarda do handler (`ProjectLookup.RequireExistsAsync`) quanto na query SQL de cada método
  de `EfProjectRepository`, mais a verificação redundante em `ReorderActivitiesCommandHandler.EnsureOwned`.
- **Cálculo de progresso**: `Project.ProgressPercentage` é sempre derivado ao vivo da coleção
  `Todos` em memória (nunca persistido — `Ignore()` no EF Core), com guarda explícita para 0 Todos
  (`TotalTodos == 0 ? 0m : ...`). Não há contador em cache que possa dessincronizar após add/toggle/
  delete/move — coberto por `ProjectTests.Empty_project_is_planned_with_zero_progress` e os casos de
  100%/50%.
- **Recurso deletado / rota obsoleta enquanto workspace aberto**: `DashboardState.OpenProject` é uma
  propriedade computada que rederiva de `data?.Projects` a cada leitura — nunca uma referência
  cacheada obsoleta. `Home.razor` só renderiza `&lt;ProjectWorkspace&gt;` quando essa propriedade não é
  nula, então um Project excluído em outra aba simplesmente some do workspace no próximo reload, sem
  exceção nem dialog órfão. Mesma garantia para `ProjectContextFilter`/`selectedProjectId`
  (`ReloadAsync` já zerava esse campo se o Project selecionado sumisse). Único ponto de assimetria
  encontrado e corrigido nesta Sprint: `OpenProjectId` (ao contrário de `selectedProjectId`) não era
  resetado para `null` em `ReloadAsync` — sem efeito observável (já que `OpenProject` sempre rederiva),
  mas corrigido por simetria e higiene de estado.
- **Cascata de exclusão / views relacionadas**: `ON DELETE CASCADE` remove Todos ao excluir o Project
  (reconfirmado, já provado na Sprint 30.13); não existe caminho de UI para ter o editor (com o botão
  Delete) e o workspace do mesmo Project abertos simultaneamente.
- **Cancelamento e feedback de UI**: toda mutação de Project (criar/editar/excluir/reordenar) usa o
  mesmo `ExecuteAsync` compartilhado com `cancellation.Token` real e toasts de sucesso/erro — padrão
  `BD30-F035` já aplicado sem exceção para Project.
- **`Project.Color`**: campo mantido apenas porque o value object de Domain ainda o exige, mas a UI
  não expõe seleção manual há Sprints — decisão de produto já documentada, não um bug de round-trip.
- **Documentação**: `docs/domain/project.md` documenta `Archived` apenas como campo persistido, sem
  reivindicar nenhum comportamento de filtragem — confirma que `BD30-F056` é uma lacuna funcional
  real, não deriva de documentação desatualizada.

### 21.3 `BD30-F020` — cobertura E2E de workspace incompleta, corrigido

A Sprint 30.13 fechou a metade de "mutações de Todo dentro do workspace" (`TodoLifecycleTests.cs`),
mas nenhum teste jamais reabria o próprio workspace após um reload real para reconferir sua barra de
progresso ou lista de To-Dos — a outra metade do achado, ainda aberta. Novo arquivo
`tests/BeeDay.E2E.Tests/ProjectLifecycleTests.cs`, 3 testes via Chromium real:

- Barra de progresso e lista de To-Dos do workspace sobrevivem a um reload de página real (criar
  Project, adicionar 2 To-Dos no workspace, completar 1 no board, `GotoAsync("/daily")`, reabrir o
  workspace, reconferir "Project progress 50%" e o estado concluído/pendente de cada item).
- Editar os campos do próprio Project (Título/Notas) persiste após reload — nunca provado antes.
- Excluir um Project (com confirmação) o remove do board e do reload subsequente — nunca provado
  antes.

Uma race condition real foi encontrada e corrigida durante a escrita do primeiro teste: clicar em
"Complete" só despacha o evento DOM, não espera o round-trip do Blazor Server que persiste a
mutação — navegar imediatamente após (`GotoAsync`) podia recarregar a página antes do toggle ter
sido de fato persistido. Corrigido aguardando a confirmação visual do toggle (o texto acessível do
botão mudar) antes de navegar, mesmo padrão já usado em outros testes desta suíte.

### 21.4 `BD30-F056` — `Project.Archived` sem nenhum controle de UI, decisão de produto necessária

Pior que `BD30-F050`/`BD30-F054` (que ao menos têm um seletor visível, só sem efeito de runtime):
`Archived` é persistido e round-tripa corretamente (`EfProjectRepositoryTests`), mas
`ProjectEditorModal.razor` não tem absolutamente nenhum controle vinculado a ele — mesmo que o campo
fosse setado diretamente no banco, nada a jusante o trataria de forma diferente: `EfDashboardReadService`
não filtra por ele, `DashboardState.FilteredProjects`/`ProjectContextOptions` não o excluem, o board
Ativo/Concluído usa `Status` (não `Archived`), e o reorder de Projects compartilha uma única sequência
de `Position` sem particionar por `Archived` (projects arquivados e ativos podem se intercalar
livremente no drag-and-drop). Novo teste `ArchivedField_HasNoRenderedControlToSetIt` em
`ProjectEditorModalTests.cs` torna essa ausência explícita e à prova de regressão, em vez de depender
apenas da leitura do arquivo razor.

Decisão de produto necessária: construir a UI de arquivamento + filtragem correspondente, ou remover
o campo morto. Não inventada por esta auditoria — mesmo princípio já aplicado a `BD30-F040`/`BD30-F050`/
`BD30-F054`.

### 21.5 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F057` (nova, baixa): `DashboardState.DeleteCurrentEditorItemAsync` (compartilhado por
  Habit/Task/Todo/Project) toca a animação de remoção do card antes de emitir a requisição de
  exclusão ao servidor — se a exclusão subsequente falhar, o card reaparece após já ter "desaparecido"
  visualmente um instante antes. Padrão cross-cutting pré-existente, não específico de Project.
  Encaminhada à Sprint 30.20 (UX).
- Reconfirmação (sem novo ID): mesmo padrão de lista não-paginada/não-virtualizada já observado para
  Habit na Sprint 30.12 (`BD30-F051`) também se aplica ao board de Projects — nenhuma paginação,
  nenhum teste cria mais de 2 Projects para o mesmo usuário.

### 21.6 Implementação

- `src/BeeDay.Web/Components/Features/Dashboard/State/DashboardState.cs` — `ReloadAsync` agora também
  reseta `OpenProjectId` quando o Project correspondente não existe mais na resposta fresca,
  simétrico ao reset já existente de `selectedProjectId`.
- `tests/BeeDay.Web.Tests/Components/Dashboard/DashboardStateTests.cs` — novo teste
  `OpenProjectWorkspace_WhenTheProjectDisappearsOnReload_ClosesWithoutCrashing`, tornando à prova de
  regressão o critério de aceite "nenhum crash de recurso deletado permanece sem tratamento".
- `tests/BeeDay.Web.Tests/Components/Projects/ProjectEditorModalTests.cs` — novo teste
  `Save_PassesTheEditedFieldsToOnSave` (Título/Notas/Data prevista) e
  `ArchivedField_HasNoRenderedControlToSetIt` (`BD30-F056`).
- `tests/BeeDay.E2E.Tests/ProjectLifecycleTests.cs` (novo) — 3 testes, fechando `BD30-F020`.

Nenhuma mudança de comportamento de produção nesta Sprint além da correção de higiene de estado
(`OpenProjectId`, sem efeito observável antes ou depois); o achado material (`BD30-F056`) exige
decisão do proprietário antes de qualquer implementação.

### 21.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Web.Tests/... --filter DashboardStateTests` | PASS, 9/9 |
| `dotnet test tests/BeeDay.Web.Tests/... --filter ProjectEditorModalTests` | PASS, 12/12 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter HabitAndTaskTests\|TodoLifecycleTests\|ProjectLifecycleTests` | PASS, 17/17 |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `dotnet test BeeDay.slnx` (Debug, completo, 1ª execução) | 1.545/1.546 — 1 falha: `ActivityFilterBarTests.SharedSearchInputPreservesTheDebouncedFilterContract`, timeout de `WaitForAssertion` do bUnit (a própria mensagem do bUnit aponta "highly utilized or slower hardware"). Arquivo/componente não tocados nesta Sprint |
| `dotnet test tests/BeeDay.Web.Tests/... --filter ActivityFilterBarTests` (retry) | PASS, 3/3 — inclusive o caso exato que falhou antes |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.546/1.546 (119 Domain, 117 Application, 216 Infrastructure, 883 Web, 211 E2E) — execução limpa, 0 falhas |

**Classificação da falha Debug:** `TRANSIENT/FLAKY` (ambiente/carga de máquina), não `CHANGE-CAUSED`.
`ActivityFilterBarTests.cs` e o componente que exercita não foram tocados por esta Sprint (escopo
desta Sprint é inteiramente Project); o retry imediato do mesmo teste, incluindo o caso exato que
falhou, passou 100% (3/3). Padrão distinto do já registrado em `BD30-F042` (aquele é `TimeoutException`
de navegação Playwright em E2E; este é timeout de `WaitForAssertion` do bUnit em Web.Tests) — não
adicionado a `BD30-F042` para não diluir sua evidência específica; registrado aqui apenas como
evidência de execução, sem novo ID.

### 21.8 Continuidade e entrega

Terceira Sprint seguida (30.12 Habit, 30.13 Task, agora 30.14 Project) a encontrar um campo
totalmente implementado do banco à UI sem efeito comportamental real — desta vez o caso mais extremo,
já que `Archived` nem sequer tem um controle de UI para ser exercitado. O padrão consistente nas três
Sprints sugere que uma única revisão de produto, cobrindo os três achados (`BD30-F050`, `BD30-F054`,
`BD30-F056`) juntos, provavelmente é mais eficiente do que três decisões separadas. `BD30-F020` está
agora genuinamente fechado (não apenas parcialmente, como a Sprint 30.13 deixou) — o workspace, não
só o board, agora tem prova de persistência real após reload. Nenhuma mutação de banco HMG/produção
foi executada ou é necessária.

## 22. Sprint 30.15 — Wallet, Transactions & Tags Complete Audit

### 22.1 Escopo e método

Issue #212. Esta Sprint carrega um contexto histórico explícito: a Sprint 30.2 já investigou e
corrigiu um incidente real de produção em HMG (`BD30-F013` — `RangeAttribute.SetupConversion`
falhava ao interpretar o limite textual `"0.01"` sob `pt-BR`, bloqueando toda submissão de transação
para usuários em português; corrigido com `ParseLimitsInInvariantCulture = true`). O proprietário
instruiu explicitamente a não presumir que a Sprint 30.2 resolveu todos os defeitos de Wallet — esta
Sprint teve como primeira obrigação **reverificar** a correção intacta e **caçar a mesma classe de
bug** em qualquer outro lugar do código, antes de auditar o restante do escopo (CRUD, saldo,
precisão decimal, filtros/ordenação/paginação, ciclo de vida de Tag, dados antigos/representativos,
concorrência, isolamento de posse, recuperação de erro de UI).

`Wallet.cs`, `WalletTag.cs`, `Transaction.cs`, `WalletCommandHandlers.cs`/`WalletQueryHandlers.cs`,
`WalletValidators.cs`, `EfWalletReadService.cs`, `Ef{Wallet,WalletTag,Transaction}Repository.cs`,
`TransactionConfiguration.cs`, `Wallet.razor` e todos os componentes/state da Feature, e todos os
testes relacionados a Wallet em Domain/Application/Infrastructure/Web/E2E foram lidos integralmente.

### 22.2 Reverificação do incidente da Sprint 30.2 e busca pela mesma classe de bug

**Correção intacta, confirmada testada em ambas as culturas.** `TransactionFormModel.Amount` ainda
declara `[Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true)]`;
`TransactionFormModalTests.AmountRange_ValidatesMinimumWithoutDependingOnCurrentCulture` prova
`0.01m` válido e `0m` inválido sob `en-US` e `pt-BR`; o E2E dedicado
`MinimumTransaction_CreateEditAndDeleteInPortuguese_KeepsCircuitInteractive` continua passando —
cria, edita e exclui uma transação de `0.01`/`0.02` inteiramente em português, confirmando o
circuito Blazor nunca quebra.

**Mesma classe de bug: não encontrada em nenhum outro lugar.** `grep` por `[Range(` em todo `src/`
retorna exatamente essa única ocorrência. Nenhuma outra anotação DataAnnotations usa limite
numérico como literal de string culture-sensível em todo o repositório; nenhum parsing manual de
decimal/moeda existe na Feature Wallet — o único input numérico é `InputNumber`, que o Blazor sempre
interpreta com semântica invariante/HTML, independente de `CultureInfo.CurrentCulture` (confirmado
pelo próprio E2E em português, que digita `"0.01"` com ponto enquanto a UI exibe saldo com vírgula).

### 22.3 Achados confirmados — corretos, sem defeito

- **Saldo**: `Wallet` não persiste saldo algum — sempre recalculado ao vivo a partir de
  `Sum(Transaction.SignedAmount)` carregado fresco do SQL Server a cada leitura; sem cache, sem
  risco de dessincronia. `SignedAmount` deriva o sinal exclusivamente de `Type`, nunca de `Amount`
  negativo.
- **Ciclo de vida de Tag / risco de órfão** (critério de aceite explícito do Issue): confirmado
  seguro em duas camadas independentes — FK `SET NULL` no banco e
  `DeleteWalletTagCommandHandler` chamando `ClearTagReferencesAsync` explicitamente antes de
  remover a tag, na mesma transação. `TransactionCard.razor` renderiza "No tag" graciosamente
  quando `WalletTagId` é nulo. Já testado em Application
  (`DeleteTag_RemovesAssociationAndKeepsTransaction`); agora também em E2E real (§22.5).
- **Concorrência**: `RowVersion` shadow property aplicado globalmente a Wallet/WalletTag/Transaction
  (`BeeDayDbContext.cs`), com `EfConcurrencySaveChanges` convertendo `DbUpdateConcurrencyException`
  em `ConcurrencyConflictException` — testado para as três entidades em
  `Ef{Wallet,WalletTag,Transaction}RepositoryTests`.
- **Isolamento de posse**: guardado e testado exaustivamente (5 testes dedicados em
  `WalletHandlersTests.cs`) em toda a superfície de Transaction/Tag, incluindo reorder.
- **Datas antigas/futuras**: sem limite de intervalo de negócio além de rejeitar `default`;
  comparação de data é por coluna SQL, não por string culture-sensível — sem risco análogo ao
  incidente da Sprint 30.2.
- **Documentação**: `docs/domain/{wallet,wallet-tag,transaction}.md` e a seção Wallet de
  `docs/web/04-feature-components.md` batem exatamente com o código, incluindo a nuance de que a
  Application não depende só do `SET NULL` do FK.

### 22.4 `BD30-F058` — precisão decimal inconsistente entre camadas, parcialmente corrigido; sort/filtro cabeados mas inalcançáveis

A regra "mínimo `0.01`, máximo `999999999999`, no máximo 2 casas decimais" tinha completude
desigual entre as quatro camadas: Domain (`Transaction.ValidateAmount`) sempre aplicava as três;
`SaveTransactionRequestValidator` (Application) aplicava mínimo e casas decimais, mas **não** o
máximo; a constraint `CK_Transactions_Amount` (banco) só aplicava `> 0`, sem máximo — a escala da
coluna `decimal(19,2)` já torna ">2 casas decimais" estruturalmente impossível no banco, então só o
máximo faltava ali também. Sem risco de integridade de dado hoje (Domain sempre executa antes de
qualquer `SaveChangesAsync`, e roda em toda gravação real do produto), mas uma gravação direta via
EF/SQL que pulasse Domain (import em lote, ferramenta administrativa futura) não seria bloqueada.

**Corrigido nesta Sprint**: `LessThanOrEqualTo(Transaction.MaximumAmount)` adicionado a
`SaveTransactionRequestValidator`; nova migration `AddTransactionAmountUpperBoundCheckConstraint`
aperta `CK_Transactions_Amount` para `[Amount] > 0 AND [Amount] <= 999999999999`. 4 novos testes
provam o limite exato (`999999999999` aceito, `1000000000000` rejeitado) em Domain e Application —
nenhum teste em toda a suíte exercitava esse valor-limite antes.

Também descoberto durante esta auditoria e **corrigido**: ordenação por Amount/Description estava
totalmente cabeada Application→Infrastructure→testes (`Wallet.razor.ResolveSort`,
`EfWalletReadService.ApplyOrdering`), mas o `<select>` de ordenação só oferecia as duas opções de
data — as outras 4 só eram alcançáveis via parâmetro de teste, nunca por um usuário real. 4 novas
`<option>` adicionadas a `WalletFilters.razor`. O filtro de faixa de valor
(`MinimumAmount`/`MaximumAmount`, também totalmente cabeado e testado em Application/Infrastructure)
continua sem qualquer superfície de UI — diferente de `BD30-F050`/`BD30-F054`/`BD30-F056`, não é uma
questão de semântica de produto ambígua, é trabalho de engenharia represado; encaminhado à Sprint
30.19 (Design System) sem correção nesta Sprint por ser uma feature de UI nova, não uma lacuna de
teste ou correção pontual.

### 22.5 `BD30-F059` — cards de Tag/Transaction perdem interatividade para itens adicionados na mesma sessão

Descoberto ao escrever os dois novos testes E2E desta Sprint (§22.6), não por auditoria de código: um
clique (ou `Enter`) em um card de `WalletTag` ou `Transaction` recém-adicionado a uma lista já
populada, na mesma sessão de circuito, não abre o editor — nenhum diálogo aparece, nenhuma exceção é
lançada. Confirmado reproduzível para o primeiro Tag criado (lista vazia→1), um segundo Tag criado
em seguida, e uma segunda Transaction criada em seguida. Descartado como causa: interceptação/overlay
(`Force: true`, que ignora as verificações de actionability do Playwright, não resolveu); timing
(espera explícita de até 1s antes do clique não resolveu); clique vs. teclado (ambos falham
igualmente). `@key="item.Id"` foi adicionado a `WalletTagManager.razor` e `TransactionList.razor`
(ausente antes — boa prática Blazor de qualquer forma, mantido por higiene), mas comprovadamente
**não** resolveu o sintoma nos testes que o reproduziram. `DialogFocusScope.cs`/
`beeday-dialog-focus.js` (o mecanismo de focus-trap dos diálogos) foi lido por completo, sem revelar
um bug óbvio no código-fonte. Um `GotoAsync` real (reload completo) sempre restaura a interatividade.

Achado retroativo importante: nenhum teste E2E de Habit/Task/Todo/Project das Sprints 30.12–30.14
jamais expôs isso, mas não porque estivessem imunes — toda sequência de duas interações de edição
nesses testes já continha um `GotoAsync` de reload no meio, executado por outro motivo (provar
persistência após reload), o que mascarou coincidentemente o mesmo sintoma. Esta Sprint é a primeira
a encadear duas aberturas de editor sem reload entre elas por um motivo não relacionado a
persistência, e por isso a primeira a expor o defeito.

Sem causa raiz identificada nesta Sprint — registrado com evidência completa e workaround
confirmado (reload) aplicado nos testes que o encontraram, em vez de uma tentativa especulativa de
correção sem certeza do mecanismo real. Encaminhada à Sprint 30.24, ao lado de `BD30-F042`
(confiabilidade da suíte E2E) — sintomas distintos (este é perda de vínculo de evento Blazor, aquele
é timeout de navegação Playwright), mas ambos relacionados a comportamento sob interação rápida
sucessiva, possivelmente dignos de investigação conjunta.

### 22.6 Cobertura de teste fechada

- `tests/BeeDay.Web.Tests/Components/Wallet/WalletUiCoverageTests.cs` — novo teste
  `SortSelect_OffersAllSixOptions_AndSelectingAmountInvokesSortChangedWithTheRightValue`.
- `tests/BeeDay.E2E.Tests/WalletTests.cs` — 2 novos testes: `CreateExpenseTransaction_
  DecreasesBalanceCorrectly` (nenhum teste E2E prévio jamais criava uma transação de Expense — só
  Income — deixando o caminho de sinal `SignedAmount` sem cobertura de browser) e
  `DeletingATag_LeavesItsTransactionVisibleWithNoTag` (o critério de aceite explícito do Issue sobre
  ciclo de vida de Tag, antes provado só em Application).

### 22.7 Implementação

- `src/BeeDay.Application/Features/Wallets/Validation/WalletValidators.cs` — regra de máximo
  adicionada a `SaveTransactionRequestValidator` (`BD30-F058`).
- `src/BeeDay.Infrastructure/Persistence/SqlServer/Configurations/TransactionConfiguration.cs` +
  nova migration `AddTransactionAmountUpperBoundCheckConstraint` — `CK_Transactions_Amount` aperta
  o máximo (`BD30-F058`).
- `src/BeeDay.Web/Components/Features/Wallets/Components/WalletFilters.razor` +
  `WalletResources.*.resx` — 4 novas opções de ordenação (`BD30-F058`).
- `src/BeeDay.Web/Components/Features/Wallets/Components/TransactionList.razor`,
  `WalletTagManager.razor` — `@key` adicionado aos `@foreach` (higiene relacionada a `BD30-F059`,
  não uma correção comprovada dele).
- `tests/BeeDay.Domain.Tests/TransactionTests.cs`,
  `tests/BeeDay.Application.Tests/WalletValidatorTests.cs` — testes de fronteira do valor máximo.
- `tests/BeeDay.Web.Tests/Components/Wallet/WalletUiCoverageTests.cs`,
  `tests/BeeDay.E2E.Tests/WalletTests.cs` — testes descritos em §22.6.

Nenhuma mutação de banco HMG/produção foi executada ou é necessária — a nova constraint é aditiva e
não rejeita nenhum dado que o Domain já não rejeitasse antes em toda gravação real do produto.

### 22.8 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Domain.Tests/... --filter TransactionTests` | PASS, 11/11 |
| `dotnet test tests/BeeDay.Application.Tests/... --filter WalletValidatorTests` | PASS, 5/5 |
| `dotnet test tests/BeeDay.Web.Tests/... --filter WalletFiltersTests` | PASS, 13/13 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter WalletTests` | PASS, 8/8 (2 execuções consecutivas, ambas limpas) |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.553/1.553 (121 Domain, 119 Application, 216 Infrastructure, 884 Web, 213 E2E) — execução limpa, 0 falhas |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.553/1.553 (121 Domain, 119 Application, 216 Infrastructure, 884 Web, 213 E2E) — execução limpa, 0 falhas |

### 22.9 Continuidade e entrega

Esta Sprint cumpriu sua obrigação mais explícita — reverificar o incidente real de HMG da Sprint
30.2 e caçar a mesma classe de bug — com resultado limpo: correção intacta, testada, e nenhuma
recorrência em nenhum outro lugar do código. O achado mais significativo (`BD30-F059`) não veio da
auditoria de código propriamente dita, mas da própria escrita de testes E2E novos: um defeito real
de interatividade, reproduzível e evidenciado com rigor, mas cuja causa raiz não foi alcançada dentro
do escopo desta Sprint — registrado com workaround confirmado em vez de uma correção especulativa
sem certeza do mecanismo. Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

## 23. Sprint 30.16 — Experience, XP, Level & Rewards Audit

### 23.1 Escopo e método

Auditoria completa do subsistema de gamificação (XP, nível, recompensas) contra o estado atual do
repositório, incluindo a obrigação específica encaminhada pela `BD30-F034` (Sprint 30.7, §14.4.1):
determinar, usando evidência de repositório e de banco, se o XP histórico anterior à correção da
`BD30-F030` pode ser identificado, reconstruído ou reconciliado.

Nota de escopo: o repositório contém dois conceitos distintos ambos chamados "Experience System" —
`src/BeeDay.Web/Components/Features/ExperienceSystem/*` é o site de documentação do Design System
(Brand/UI/UX), não gamificação. Esta Sprint audita exclusivamente `src/BeeDay.Domain/Experience/*`
e `src/BeeDay.Application/Common/Experience/*` (o sistema de XP/Nível), confirmado por leitura
completa de `UserExperience.cs`, `ExperienceEntry.cs`, `ExperienceSource.cs`, `ExperienceReward.cs`,
`ExperienceCurve.cs`/`LinearExperienceCurve.cs`, todos os handlers de Habit/Task/Todo que concedem
XP, `EfUserRepository.cs`, `BeeDayFeedback*` (modal de level-up), `ExperienceBar.razor`, e toda a
suíte de testes relacionada em `tests/BeeDay.Domain.Tests`, `tests/BeeDay.Application.Tests`,
`tests/BeeDay.Infrastructure.Tests`, `tests/BeeDay.Web.Tests` e `tests/BeeDay.E2E.Tests`.

**A. Mecânica de concessão de XP — confirmada correta.** A chave de dedup de `UserExperience.TryAdd`
é `(UserId, Source.Type, Source.ReferenceId, RewardType)`. Habit está corretamente isento (usa
sempre `Guid.NewGuid()` como `SourceId`, nunca o Id do próprio Habit), confirmado tanto no Domain
(`EnsureValidState`) quanto por um índice único filtrado no banco que exclui explicitamente
`SourceType = 0` (Habit). Todos os 3 pontos de concessão de produção (`RegisterHabitPositiveCommandHandler`,
`ToggleTaskCommandHandler`, `ToggleTodoCommandHandler` — este último também concede o reward de
Project quando o último Todo do Project completa, não existe handler dedicado de "completar Project")
passam pelo mesmo caminho de persistência já corrigido pela `BD30-F030`; `grep` por `.AddExperience(`
(caminho não-dedup) fora de teste retorna zero resultados de produção. Onboarding não concede XP
(não é bug — simplesmente não existe essa concessão).

**Corrida/concorrência — confirmada protegida em duas camadas independentes**, ambas com prova
empírica contra LocalDB real (`EfUserRepositoryTests.cs`): (1) índice único filtrado
`UX_ExperienceEntries_Dedup` no banco rejeita duas inserções concorrentes da mesma chave de dedup;
(2) `RowVersion` otimista em `Users`/`UserExperience` (via o loop global de `BeeDayDbContext.cs`)
faz o segundo `SaveChangesAsync` perdedor lançar `DbUpdateConcurrencyException`, mapeada para
`ConcurrencyConflictException`. Como cada concessão ocorre dentro de uma transação explícita
descartada (rollback automático) em caso de exceção, uma corrida real nunca produz duplicação
silenciosa nem estado parcial.

**B. Cálculo de nível — confirmado correto.** `LinearExperienceCurve` implementa uma curva
triangular (`100 * (nível-1) * nível / 2`), com custo por nível linearmente crescente (100, 200,
300, 400 XP). Não existe documento em `docs/` especificando a curva numérica pretendida — não é
evidência de divergência, apenas ausência de uma especificação externa para comparar. Fronteiras
exatas (99→nível 1, 100→nível 2, 299→nível 2, 300→nível 3) e overflow em `long.MaxValue` já eram
testados em `ExperienceDomainTests.cs`; nenhum off-by-one encontrado.

**Feedback de level-up — confirmado correto.** O modal (`BeeDayFeedbackModal`/`BeeDayFeedbackStore`,
escopo de circuito, dedup por `HashSet<Guid>` de `ExperienceEntryId`) nunca repete o mesmo level-up
e nunca reaparece após reload (novo circuito = store vazia). Provado em bUnit
(`BeeDayFeedbackTests.cs`) mas, antes desta Sprint, nunca ponta a ponta contra um level-up real (ver
`BD30-F060`).

**C. Histórico de recompensa — nenhuma UI/query expõe `ExperienceEntry`.** Confirmado por busca
completa em `src/BeeDay.Application` e `src/BeeDay.Web`: não existe query, read-service, nem método
de repositório que retorne o histórico persistido de `ExperienceEntry` — a única superfície visível
é o toast efêmero de level-up (últimos 3 itens, escopo de sessão, nunca lê do banco). Registrado como
`BD30-F061` (decisão de produto, não corrigida).

**D. Testes e integrações cruzadas.** Cobertura forte e já existente para as duas garantias centrais:
`Completing_task_twice_grants_experience_only_once` e `Completing_last_todo_grants_todo_and_project_rewards_once`
(`ExperienceRewardPipelineTests.cs`) provam que alternar conclusão 3× não concede XP duplicado para
Task/Todo/Project; `Positive_habit_grants_experience_for_each_distinct_occurrence` prova o oposto
para Habit (repetição legítima). Gap real confirmado e corrigido nesta Sprint: nenhum E2E provava
visibilidade de XP para Task/Todo/Project (só Habit), e o modal de level-up nunca fora exercitado
ponta a ponta (`BD30-F060`).

Exclusão de um item já recompensado nunca revoga o XP concedido — comportamento deliberado (comentário
de código em `ExperienceEntryConfiguration.cs` confirma o racional: histórico append-only, sem FK
para a origem), mas não ratificado em `docs/`. Registrado como `BD30-F062`, encaminhado à Sprint 30.28
(documentação).

### 23.2 `BD30-F034` — conclusão da investigação de integridade histórica de `TotalExperience`

As 7 perguntas encaminhadas pela Sprint 30.7 (§14.4.1) foram investigadas com evidência de
repositório. Nenhuma consulta ou mutação foi executada contra banco de HMG/produção — essa restrição
permanece integralmente respeitada.

1. **O XP histórico afetado pode ser identificado?** Não. Antes da correção da `BD30-F030`,
   nenhuma linha de `ExperienceEntry` era persistida — nem para concessões legítimas, nem para as
   duplicadas. Não existe, portanto, nenhum registro diferencial que distinga uma da outra para o
   período afetado.
2. **O XP correto pode ser reconstruído de forma determinística?** Não. O escalar `TotalExperience`
   em si (coluna simples em `Users`) era corretamente persistido a cada mutação mesmo antes da
   correção — só o *histórico auditável* (`ExperienceEntry`) não era. Isso significa que qualquer
   inflação histórica já está incorporada ao valor atual do total, de forma indistinguível de XP
   legítimo, precisamente porque a evidência que provaria a diferença nunca existiu.
3. **Quais fontes são afetadas?** Task, Todo e Project — qualquer aggregate cujo `SourceId` de
   dedup é o Id do próprio item, e que pode ser desmarcado/remarcado repetidamente pela UI real.
   Habit nunca é afetado: cada registro é, por design, uma concessão legítima e independente (nunca
   dedup), então "repetir" não é um bug para Habit.
4. **Repetições legítimas de Habit podem ser distinguidas de duplicatas de Todo/Task/Project?** Sim,
   mecanicamente (chave de dedup, `SourceId` fixo vs. sempre novo) — mas não a nível de dados
   históricos para o período afetado, pelo mesmo motivo do item 2: não há entries para interrogar.
5. **Uma correção automatizada é segura?** Não. Qualquer heurística de correção seria, por
   definição, um palpite sobre dados que não existem — sem base factual para validar o resultado.
6. **Seria necessária reconciliação manual ou parcial?** Mesmo uma reconciliação manual não é viável
   com a estrutura de dados atual: não há trilha de auditoria a examinar para o período afetado, com
   ou sem acesso ao banco.
7. **Qual o raio de impacto exato em HMG e, separadamente, em produção?** Não determinável por esta
   Sprint. Busca completa em `scripts/`, `.github/workflows/` e `docs/deployment/` confirma que **não
   existe no repositório nenhum mecanismo seguro, documentado e somente-leitura para consultar o SQL
   Server de HMG ou produção** a partir de um contexto local/CI — o único acesso com capacidade de
   escrita SQL real é `scripts/Deploy-BeeDay.ps1` (migração/backup, protegido por secrets do
   GitHub Actions, executado apenas dentro dos jobs `deploy-hmg.yml`/`deploy-prd.yml`), e
   `verify-hmg.yml` faz apenas checagens HTTP (`/health/ready`, `/login`), nunca SQL. Nenhuma
   conexão foi tentada, conforme a restrição desta investigação.

**Conclusão**: a integridade histórica de `TotalExperience` não pode ser resolvida com os dados e
ferramentas disponíveis hoje — o próprio dado que permitiria resolvê-la nunca foi persistido. Prosseguir
exige duas decisões do proprietário, fora da autoridade desta auditoria: (a) se vale investir na
construção de uma capacidade de leitura segura contra HMG/produção; e (b) se, dado que a reconstrução
determinística e completa é matematicamente impossível, algum esforço de reconciliação parcial ainda
teria valor. `BD30-F034` permanece `OPEN`, reatribuída de "30.16" para "decisão do proprietário" — a
investigação que lhe cabia está concluída; nenhuma ação adicional de auditoria pode avançá-la.

### 23.3 Implementação

- `tests/BeeDay.E2E.Tests/E2EWebApplicationFactory.cs` — `SeedUserAsync` ganha o parâmetro opcional
  `initialExperience` (usa `User.AddExperience`, não-dedup, só para arranjo de teste), permitindo
  posicionar um usuário perto de um limite de nível antes de dirigir a ação real de level-up pelo
  navegador. Assinatura anterior preservada por compatibilidade (todo call site existente usa o
  parâmetro nomeado `onboardingCompleted:`, então nenhum call site precisou mudar).
- `tests/BeeDay.E2E.Tests/HabitAndTaskTests.cs` — `LoginToDailyAsync` ganha o mesmo parâmetro
  opcional; dois novos testes: `CompleteTask_UpdatesXp` (visibilidade de XP para Task, espelhando o
  teste já existente de Habit) e `CompleteTask_AtALevelBoundary_ShowsTheLevelUpModalExactlyOnce`
  (semeia 95 XP, completa uma Task de 5 XP, cruza exatamente o limite de 100 XP do Nível 2, prova o
  modal aparecendo uma vez com os níveis corretos e não reaparecendo após reload).

Nenhuma mudança de comportamento de produto, regra de domínio, contrato público, schema, migration
ou Design System. Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

### 23.4 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors (corrigido 1 warning CS0419 de `<see cref>` ambíguo introduzido durante a implementação) |
| `dotnet format BeeDay.slnx whitespace --include ...` | aplicado aos 2 arquivos novos/alterados |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter HabitAndTaskTests` | PASS, 13/13 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter CompleteTask_UpdatesXp\|CompleteTask_AtALevelBoundary` | PASS, 2/2 (execução isolada de confirmação, adicional à suíte completa acima) |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.555/1.555 (121 Domain, 119 Application, 216 Infrastructure, 884 Web, 215 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.555/1.555 (121 Domain, 119 Application, 216 Infrastructure, 884 Web, 215 E2E) — execução limpa, 0 falhas |

### 23.5 Continuidade e entrega

O achado mais significativo desta Sprint não foi um defeito de código — a mecânica de XP, dedup,
concorrência e nível está correta e bem testada — mas a conclusão definitiva de que `BD30-F034` não
pode ser resolvida pela auditoria: os dados que permitiriam quantificar ou corrigir qualquer inflação
histórica nunca existiram, e não existe ferramenta segura no repositório para sequer consultar
HMG/produção. Essa conclusão evita que Sprints futuras repitam a mesma investigação sem uma nova
capacidade (leitura segura de HMG/produção) ou uma decisão explícita do proprietário. Cobertura E2E
de XP foi ampliada de Habit-somente para incluir Task e o modal de level-up ponta a ponta pela
primeira vez (`BD30-F060`). Dois achados de baixa severidade foram encaminhados (`BD30-F061` decisão
de produto, `BD30-F062` gap de documentação). Nenhuma mutação de banco HMG/produção foi executada ou
é necessária.

## 24. Sprint 30.17 — Navigation, Routing & Application Shell Audit

### 24.1 Escopo e método

Auditoria completa de navegação, roteamento e shell da aplicação: inventário de todas as rotas
`@page`, links, redirects, gates de autorização, estados de layout, navegação de header/footer,
parâmetros de rota, comportamento de voltar/avançar do navegador, refresh/deep link, acesso não
autorizado, e experiências de not-found/erro/reconexão — contra o estado atual do repositório,
incluindo a reverificação explícita de `BD30-F002` (encaminhada pela Sprint 30.1).

Leitura completa de: todos os 54 `@page` do repositório e seus atributos de layout/autorização;
`docs/web/02-routing-and-pages.md`; `MainLayout`/`DesktopSidebar`/`MobileSidebar`/`MobileHeader`/
`NavigationItems`/`PublicHeader`/`AppFooter`/`EditorialFooter`; `LoginDestinationResolver.cs`,
`Program.cs` (autenticação/autorização/pipeline HTTP), `RedirectToLogin.razor`; `Routes.razor`,
`App.razor`, `NotFound.razor`, `Error.razor`, `GlobalExceptionHandler.cs`, `ReconnectModal.razor`;
`Tutorial.razor`, `CreateProfile.razor`/`ProfileCreationState.cs`; e toda a suíte de testes de
navegação/roteamento em `tests/BeeDay.Web.Tests/Integration` e `tests/BeeDay.E2E.Tests`.

### 24.2 Inventário de rotas — confirmado correto, `BD30-F002` corrigido

**54 declarações `@page` em 52 arquivos `.razor`**, reconfirmadas exaustivamente (nenhuma rota
parametrizada existe hoje no repositório; toda página declara exatamente um de
`[Authorize]`/`[AllowAnonymous]`). Comparação byte-a-byte contra a tabela do §3 de
`docs/web/02-routing-and-pages.md`: **100% de correspondência** — zero rota indocumentada, zero rota
obsoleta ainda documentada, zero rota duplicada.

A divergência original de `BD30-F002` ("doc registra 42, busca encontrou 54") não estava na tabela
em si — a tabela já estava correta desde a Sprint 29.4 — mas em três menções de prosa isoladas (§1,
rodapé do §3, §11) nunca atualizadas desde antes da Sprint 25.17 (quando as 21 rotas do `beeday
Experience System` foram adicionadas). Corrigidas nesta Sprint para o valor real. `INV-008` passa de
`BASELINED` para `VERIFIED`.

### 24.3 `BD30-F063` — 404 vazio para qualquer rota inexistente, corrigido (achado mais significativo)

Escrever `NavigationTests.NonexistentRoute_RendersTheNotFoundPage` (E2E, nova) revelou que
`app.MapRazorComponents<App>()` só registra um endpoint para cada `@page` descoberto — não existe
fallback catch-all implícito. Confirmado empiricamente via `curl` direto contra o servidor real
(não apenas leitura de código): uma requisição para uma URL sem `@page` correspondente retornava

```text
HTTP/1.1 404 Not Found
Content-Length: 0
```

— nenhum HTML, nenhum conteúdo, nada — contra `/login` (200, HTML completo) e `/not-found` (200,
a mesma página funcionando quando acessada diretamente pela sua própria rota). O `NotFoundPage` do
`Router` do Blazor (`Routes.razor`) nunca era alcançado: o roteamento do ASP.NET Core terminava
antes de a árvore de componentes sequer começar a renderizar. Nenhum teste anterior (E2E ou
integração) exercitava uma URL genuinamente inexistente contra o pipeline HTTP real —
`NotFoundTests.cs` (bUnit) renderiza `NotFound.razor` diretamente, contornando o roteamento por
completo.

Impacto real: qualquer erro de digitação de URL, link externo obsoleto, favorito antigo, ou
resultado de busca desatualizado mostrava uma tela completamente em branco em produção — não uma
página amigável, não uma mensagem, nada. Isso também explica e agrava `BD30-F064` (link morto
`/buy-me-a-coffee`, presente em todas as 12 páginas institucionais).

**Correção mínima**: `app.UseStatusCodePagesWithReExecute("/not-found")`, adicionado logo após
`app.UseExceptionHandler()` em `Program.cs`. Reexecuta a requisição contra a rota `/not-found` real
(já existente, estilizada, localizada) sempre que a resposta termina em um status 4xx/5xx sem corpo
já escrito. Não interfere com respostas que já têm corpo (JSON de `GlobalExceptionHandler`, usado
por endpoints de API) nem com redirects de autenticação (302 com `Location`, usado pelo cookie
middleware) — verificado por reexecução manual (`curl` antes/depois) e pela suíte completa de
`AuthorizationIntegrationTests`/`AntiforgeryIntegrationTests`/`ProblemDetailsIntegrationTests`
(892/892 em `BeeDay.Web.Tests`, nenhuma regressão).

### 24.4 Outros achados confirmados — corretos, sem defeito

- **Shell desktop/mobile**: `DesktopSidebar`/`MobileSidebar` renderizam o mesmo componente
  compartilhado `NavigationItems` — paridade de destinos garantida por construção, não por dois
  inventários mantidos separadamente. Confirmado por `NavigationTests.cs`/`ShellResponsiveLayoutTests.cs`.
- **Duplicação/sobreposição de rotas**: zero rotas duplicadas em toda a base.
- **`LoginDestinationResolver`/`IsLocalPath`**: reverificado — implementação única, canônica,
  reutilizada em todos os 3 pontos que precisam de proteção contra open-redirect
  (`Login.razor`, `/auth/login`, `/auth/logout`, `/culture/set`). `BD30-F038` (Sprint 30.10)
  permanece corrigida, sem regressão nem implementação duplicada mais fraca em nenhum outro lugar.
- **`ReturnUrl` através do login**: preservado corretamente ponta a ponta, agora com cobertura de
  teste explícita (§24.5/`BD30-F066`) em vez de apenas inferido por inspeção de código.
- **Modal de reconexão** (`ReconnectModal.razor`): totalmente customizado, estilizado e localizado
  — não é o padrão não-estilizado do SDK.

### 24.5 `BD30-F066` — cobertura de teste ausente para o ciclo de `returnUrl` e para 404 real, corrigido

Nenhum teste anterior provava o ciclo completo `returnUrl` (hit anônimo em rota protegida →
redirect com `returnUrl` correto → login → volta exatamente à página pedida) nem uma URL
genuinamente inexistente atingindo o roteamento real. Corrigido com 3 testes novos:
`AuthorizationIntegrationTests.Anonymous_ProtectedPageRedirect_CarriesTheOriginalPathAsReturnUrl`,
`LoginIntegrationTests.Login_WithLocalReturnUrl_RedirectsToTheOriginallyRequestedPage`, e
`NavigationTests.NonexistentRoute_RendersTheNotFoundPage` (E2E) — este último é também a prova de
regressão da `BD30-F063`.

### 24.6 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F064` (baixa, decisão do proprietário): link morto `/buy-me-a-coffee` em todas as 12 páginas
  institucionais — já pré-anunciado como fora de escopo desde a Sprint 29.4; após a correção da
  `BD30-F063` já não mostra mais página em branco, mostra a página real de Not Found.
- `BD30-F065` (média, → 30.23): `/Error` nunca é produzida por nenhum caminho de código (órfã) e
  nenhum `<ErrorBoundary>` existe em toda a árvore de componentes — uma exceção não tratada dentro
  de qualquer página interativa encerra o circuito sem tela de recuperação além do Reconnect
  genérico. Gap de arquitetura de resiliência a erros, fora do escopo estrito de roteamento;
  encaminhado à Sprint dedicada a resiliência/observability em vez de uma correção especulativa na
  pipeline global de exceções.
- `BD30-F067` (baixa, decisão do proprietário): subárvore `/experience-system` (21 rotas) sem ponto
  de entrada direto no nav de topo — discoverability fraca, não rota quebrada.
- `BD30-F068` (baixa, → 30.20): os dois wizards de onboarding mantêm o passo atual fora da URL —
  padrão consistente, comportamento de voltar/avançar previsível (sai do wizard), observação de
  interação encaminhada a uma Sprint de UX.

### 24.7 Implementação

- `src/BeeDay.Web/Program.cs` — `app.UseStatusCodePagesWithReExecute("/not-found")` (`BD30-F063`).
- `docs/web/02-routing-and-pages.md` — 3 linhas de prosa corrigidas de "42"/"40" para "54"/"52"
  (`BD30-F002`), changelog atualizado.
- `tests/BeeDay.Web.Tests/Integration/AuthorizationIntegrationTests.cs` — novo teste
  `Anonymous_ProtectedPageRedirect_CarriesTheOriginalPathAsReturnUrl`.
- `tests/BeeDay.Web.Tests/Integration/LoginIntegrationTests.cs` — novo teste
  `Login_WithLocalReturnUrl_RedirectsToTheOriginallyRequestedPage`.
- `tests/BeeDay.E2E.Tests/NavigationTests.cs` — novo teste `NonexistentRoute_RendersTheNotFoundPage`.

Nenhuma mudança de contrato público de Application, schema, migration ou Design System. Nenhuma
mutação de banco HMG/produção foi executada ou é necessária.

### 24.8 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `curl` manual contra `/this-route-does-not-exist-e2e` (antes da correção) | 404, `Content-Length: 0`, corpo vazio — reprodução confirmada |
| `curl` manual contra `/this-route-does-not-exist-e2e` (depois da correção) | 404, HTML completo, `Not Found`/`does not exist` presentes no corpo |
| `curl` manual contra `/login` e `/not-found` (antes e depois) | 200 em ambos os casos, sem alteração — nenhuma regressão nas rotas existentes |
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Web.Tests/...` (completo) | PASS, 892/892 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter NavigationTests` | PASS, 8/8 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | 1.563/1.564 na primeira execução (121 Domain, 119 Application, 216 Infrastructure, 892 Web, 215/216 E2E) — 1 falha em `InstitutionalPagesTests.EveryRouteRendersItsHeroAndHeadingWithoutHorizontalOverflowOnDesktop(route: "/brand-guidelines")`, `TimeoutException` em `GotoAsync`/screenshot. Classificada `TRANSIENT/FLAKY` com evidência: rota institucional não tocada por esta Sprint, assinatura idêntica à `BD30-F042` já documentada (contenção LocalDB/Playwright), reexecução isolada do teste PASSOU limpa (12/12, incluindo o caso `/brand-guidelines`) |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.564/1.564 (121 Domain, 119 Application, 216 Infrastructure, 892 Web, 216 E2E) — execução limpa, 0 falhas; confirma `TRANSIENT/FLAKY` da execução Debug (o mesmo caso `/brand-guidelines` passou aqui de primeira) |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 24.9 Continuidade e entrega

O achado mais significativo desta Sprint foi descoberto pela própria escrita de um teste E2E novo,
não pela auditoria de código isolada: `MapRazorComponents<App>()` nunca teve um fallback catch-all
implícito, então toda URL inexistente — erro de digitação, link externo obsoleto, favorito antigo —
mostrava uma tela completamente em branco em produção, sem nunca alcançar o `NotFoundPage` estilizado
e localizado que já existia e funcionava perfeitamente quando acessado diretamente. A correção
(`UseStatusCodePagesWithReExecute`) é mínima, padrão do ASP.NET Core, verificada empiricamente
antes/depois via `curl` contra o servidor real e sem nenhuma regressão na suíte completa de
autenticação/autorização/antiforgery/problem-details. `BD30-F002` foi fechada com uma correção
estritamente documental. Quatro achados menores foram encaminhados com evidência completa, nenhum
inventado. Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

## 25. Sprint 30.18 — Public & Experience System Audit

### 25.1 Escopo e método

Auditoria completa das superfícies públicas (anônimas) da aplicação: Home pública (`/`), as 12
páginas institucionais (`Mission`, `Efficacy`, `BrandGuidelines`, `Contact`, `Product`,
`ProductPlus`, `Android`, `Ios`, `Faqs`, `CommunityGuidelines`, `Terms`, `Privacy`) e as 21 rotas do
`beeday Experience System` (documentação pública de Brand/UI/UX — não confundir com o sistema de
gamificação XP auditado na Sprint 30.16, que compartilha o nome mas é um conceito completamente
diferente). A Sprint 30.17 já auditou o roteamento/shell propriamente dito (54 rotas, todas
corretamente autorizadas, `BD30-F063` corrigida); esta Sprint audita completude de conteúdo,
integridade de links, independência de estado autenticado, responsividade e localização.

Esta Issue referencia um "EPIC 30 Remaining Sprint Global Execution Contract" que não foi
encontrado em nenhum lugar do repositório nem das Issues do GitHub (busca completa executada,
sem resultado). Na ausência desse documento, esta Sprint seguiu o `CLAUDE.md` vigente e o padrão
já demonstrado nas 17 Sprints anteriores desta EPIC, que cobrem integralmente governança de
repositório, método de auditoria, validação, relato e autoridade de Git — nenhuma lacuna de
autoridade foi identificada.

### 25.2 Achados confirmados — corretos, sem defeito

- **Integridade de links internos**: todos os links de `AppFooter`, `EditorialFooter`,
  `PublicHeader`, navegação do Experience System e CTAs de Home/institucionais resolvem para rotas
  reais, exceto `/buy-me-a-coffee` (já `BD30-F064`, Sprint 30.17, decisão do proprietário —
  reconfirmado ainda presente e ainda um link morto, não reinvestigado).
- **Links externos**: apenas domínios reais (LinkedIn, GitHub) — zero `example.com`, `href="#"`
  ou `href=""` em toda a superfície auditada.
- **CTAs**: todos os botões "Get started"/"Try beeday today"/"Continue to beeday" apontam para
  rotas/handlers reais, nenhum `<a href="#">` morto.
- **"Coming soon" honestos**: Android/iOS (sem link de loja inventado), beeday Plus (sem preço
  inventado), Efficacy (sem métrica/estudo inventado), ícones sociais Instagram/X (`<span>`
  não-interativo, não link morto) — todos deliberados, testados, com comentário de código
  explicando a decisão de não inventar conteúdo.
- **Independência de estado autenticado**: nenhuma das 12 páginas institucionais nem das 21 do
  Experience System injeta `ICurrentUserContext`, `ISender` ou qualquer serviço de acesso a dados —
  são puramente estáticas, dirigidas por `resx`. Os únicos dois usos de `AuthorizeView` (`Home.razor`,
  `PublicHeader.razor`) estão corretamente restritos ao branch `<Authorized>`, sem dependência para
  o visitante anônimo.
- **Duplicação de conteúdo**: `/beeday` vs `/beeday-plus` e `/android` vs `/ios` são
  diferenciados de propósito, não duplicação acidental.
- **Localização**: 100% dirigida por `.resx` (`en-US`/`pt-BR`), nenhum texto hardcoded encontrado.
- **Placeholder/conteúdo obsoleto**: zero ocorrências reais de `Lorem ipsum`/`TODO`/`TBD` em toda
  a superfície (duas correspondências de grep eram falsos positivos verificados).

### 25.3 `BD30-F069` — conteúdo legal pendente, registrado conforme critério de aceite

`/terms`, `/privacy` e `/community-guidelines` não contêm texto legal real — cada uma renderiza um
aviso proeminente e testado ("revisão legal pendente") mais uma lista de títulos de seção sem corpo.
Este é exatamente o comportamento que o critério de aceite desta Sprint exige ("Missing/unapproved
legal content is recorded for owner/legal review rather than invented") — nenhum texto legal foi
proposto, inventado ou avaliado quanto à correção jurídica por esta auditoria. Registrado como
achado de baixa severidade, aberto, aguardando decisão/aprovação do proprietário — não é um defeito
de engenharia (a divulgação honesta já está corretamente implementada e testada).

### 25.4 `BD30-F070` — cobertura mobile institucional incompleta, corrigido

`RepresentativeRoutesRenderWithoutHorizontalOverflowOnMobile` cobria 9 das 12 rotas institucionais.
`/brand-guidelines` — a rota estruturalmente mais complexa da família institucional, a única que
embute a navegação de pilar/tópico do Experience System — nunca tinha sido testada especificamente
em viewport mobile. Corrigida com uma nova linha `InlineData`. `/privacy` e `/community-guidelines`
foram deliberadamente mantidas fora da adição: compartilham o template exato de `/terms` (já
coberto), seguindo o mesmo padrão de amostragem representativa já estabelecido no restante da suíte.

### 25.5 Achados menores/informativos (não corrigidos, encaminhados)

- `BD30-F071` (baixa, → 30.20): troca de idioma ao vivo via seletor real só é testada em E2E para
  a página raiz do Experience System (`/experience-system`) — as outras 20 rotas dependem só de
  correção testada em nível de `resx`/componente. Baixo risco, gap de cobertura real.
- Reconfirmados sem alteração de estado: `BD30-F064` (link morto `/buy-me-a-coffee`, decisão do
  proprietário) e `BD30-F067` (subárvore `/experience-system` sem entrada direta no nav de topo,
  decisão do proprietário) — ambos da Sprint 30.17, ainda precisos.

### 25.6 Implementação

- `tests/BeeDay.E2E.Tests/InstitutionalPagesTests.cs` — `/brand-guidelines` adicionada a
  `RepresentativeRoutesRenderWithoutHorizontalOverflowOnMobile` (`BD30-F070`).
- `docs/epics/30-system-integrity/README.md` — nova Seção 25, achados `BD30-F069`–`BD30-F071`.

Nenhuma mudança de comportamento de produto, regra de negócio, contrato público, schema, migration
ou Design System. Nenhum texto legal foi escrito ou proposto. Nenhuma mutação de banco HMG/produção
foi executada ou é necessária.

### 25.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter InstitutionalPagesTests` | PASS, 49/49 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.565/1.565 (121 Domain, 119 Application, 216 Infrastructure, 892 Web, 217 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | 1.564/1.565 na primeira execução (121 Domain, 119 Application, 216 Infrastructure, 892 Web, 216/217 E2E) — 1 falha em `ShellResponsiveLayoutTests.TabletAndMobileUseOneDrawerShellWithoutDocumentOverflow(width: 900, height: 800)`, `TimeoutException` em `GotoAsync("/login")`/screenshot. Classificada `TRANSIENT/FLAKY` com evidência: teste de shell responsivo não tocado por esta Sprint, assinatura idêntica à `BD30-F042` já documentada, reexecução isolada PASSOU limpa (6/6) |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 25.8 Continuidade e entrega

Esta Sprint confirmou que as superfícies públicas do produto estão em bom estado: nenhum link
morto novo, nenhuma dependência acidental de estado autenticado, nenhum conteúdo placeholder real,
localização completa e consistente. O único achado de conteúdo genuíno (`BD30-F069`, texto legal
pendente) já estava corretamente tratado como divulgação honesta pelo produto — o papel desta
Sprint foi confirmar isso com evidência e registrá-lo formalmente para decisão do proprietário,
exatamente como o critério de aceite exige, sem inventar nenhum texto. Um gap real de cobertura
mobile foi fechado (`BD30-F070`); um gap de cobertura de localização de baixo risco foi encaminhado
(`BD30-F071`). Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

## 26. Sprint 30.19 — Design System Complete Audit

### 26.1 Escopo e método

Auditoria completa da biblioteca de Design System compartilhada (`src/BeeDay.Web/Components/DesignSystem/`,
27 componentes `.razor`) e sua camada de fundação (`variables.css`, sistema de ícones, 18 stylesheets
globais) contra o inventário `INV-015`, ainda `BASELINED` desde a Sprint 30.1. Sprint de
auditoria/consolidação, não de redesign — o boundary explícito da Issue proíbe criar nova linguagem
visual ou reescrever componentes funcionais sem evidência.

Duas obrigações encaminhadas de Sprints anteriores foram reverificadas: `BD30-F052` (Sprint 30.11,
gap de UI de `ActivityAttribute`) e a metade restante de `BD30-F058` (Sprint 30.15, filtro de faixa
de valor do Wallet). Ambas confirmadas ainda precisas — ver §26.5.

### 26.2 Achados confirmados — corretos, sem defeito

- **Sistema de ícones**: padrão único e canônico (sprite SVG via `BeeDayIcon.razor`), nenhuma
  implementação concorrente encontrada em toda a base.
- **Reuso de componentes estruturais**: `BeeDayButton` (17 consumidores), `BeeDayIcon` (20),
  `BeeDayInput` (13), `EditorModalShell` (todos os 6 editores de formulário), `BeeDayConfirmDialog`
  (5) — nenhuma reimplementação local encontrada em Wallet/Habits/Tasks/Todos/Projects/Account.
  Componentes "molécula" (`ActivityCard`, `HabitCard`, `TransactionCard`, `WalletSummary`,
  `ProgressMetricCard`, `ExperienceBar`) corretamente envolvem `BeeDayCard` em vez de duplicar a
  base do cartão.
- **Variantes de botão**: 9 variantes declaradas, todas com CSS e tokens dedicados; zero
  `style=` inline sobrescrevendo `<BeeDayButton>` em qualquer consumidor.
- **Shell de modal/diálogo**: todos os 6 editores reutilizam `EditorModalShell`; os dois desvios
  encontrados (`BeeDayConfirmDialog`/`BeeDayFeedbackModal` para diálogos não-editor,
  `ProjectWorkspace` para um painel de workspace completo) reutilizam corretamente o primitivo de
  foco `DialogFocusScope` e são variantes de produto legítimas, não duplicação.
- **Foco visível**: baseline global de especificidade zero (`::where(...):focus-visible` em
  `app.css`) mais ajustes locais por componente, todos derivando cor do mesmo token — inclusive
  evidência positiva de autocorreção documentada (um anel duplo em Wallet já foi encontrado e
  removido em favor da regra fundacional).
- **Variantes de produto legítimas**: paleta dedicada de Habit (`--beeday-habit-color-*`), cartão
  de saldo do Wallet, checkbox nativo de `Login.razor` (única exceção técnica justificada — formulário
  HTML puro, sem `EditContext`, mas replica manualmente o mesmo contrato visual de `BeeDayCheckbox`)
  — nenhuma flagrada como defeito.
- **`BeeDayCheckbox`**: zero consumidores de produção hoje, mas não é código morto — nenhum editor
  atual tem campo booleano dentro de um `EditForm`; o componente já está testado e pronto para o
  primeiro campo que precisar dele. Decisão explícita desta Sprint: manter, não remover.

### 26.3 `BD30-F072`/`BD30-F073` — drift de token e duplicação de `@keyframes`, corrigidos

Dois pontos de drift de valor hardcoded confirmados e corrigidos: `CreateProfile.razor.css`
referenciava um custom property nunca definido (`--beeday-color-danger-text`), então seu fallback
hardcoded sempre vencia, divergindo do token canônico `--beeday-color-danger`; `feedback.css` usava
hex bruto quase-idêntico aos tokens `--beeday-color-success-soft`/`--beeday-color-danger-soft` já
existentes para o fundo dos ícones de toast. `@keyframes beeday-spin` estava definido identicamente
em dois stylesheets globais sempre carregados juntos — consolidado em uma única definição.

### 26.4 `BD30-F074` — `BeeDayCardMenu` órfão, removido

`BeeDayCardMenu` (mais seu serviço de coordenação `CardActionMenuCoordinator` e sua geometria de
posicionamento `CardMenuPlacement`) tinha zero consumidores de produção em toda a base — confirmado
por busca completa por `<BeeDayCardMenu`. O histórico do Git mostra que um refactor anterior tornou
os cards inteiros clicáveis para editar (`05a7ad3`), superando o propósito original do componente
(um menu kebab Editar/Excluir), mas o componente, seu serviço, sua geometria e dois arquivos de
teste dedicados nunca foram removidos.

**Removidos nesta Sprint**: `BeeDayCardMenu.razor`/`.razor.cs`/`.razor.css`, `CardMenuPlacement.cs`,
`CardActionMenuCoordinator.cs`, `BeeDayCardMenuTests.cs`, `CardMenuPlacementCalculatorTests.cs`, o
registro de DI em `Program.cs`, as 3 chaves `resx` exclusivas (`CardMenuEditLabel`/
`CardMenuDeleteLabel`/`CardMenuOptionsForAriaLabel`, em 3 arquivos), e a menção obsoleta no
comentário de `DesignSystemResources.cs`. `CoreComponentContractTests.cs` — o teste de inventário
que trava a lista canônica de componentes compartilhados e a contagem de controles nativos — foi
atualizado para refletir a remoção (lista de componentes, contagem de `<button>` 31→28, soma total
44→41, arquivos com controle nativo 21→20). `WalletUiCoverageTests.cs` tinha um registro de DI
supérfluo para o serviço removido (nunca efetivamente consumido por aquele teste), também limpo.

### 26.5 Reverificação de achados encaminhados — sem correção, escopo confirmado fora desta Sprint

`BD30-F052` (gap de UI de `ActivityAttribute`) e a metade restante de `BD30-F058` (filtro de faixa
de valor do Wallet) foram reverificados: ambos continuam precisos, sem mudança desde suas Sprints
originais. Construir essas duas superfícies de UI (um seletor de atributo em 4 editores; um filtro
de Min/Max amount no Wallet) é trabalho de feature genuíno — novos campos, novo estado, nova
validação client-side, novos testes — não consolidação de duplicatas, e está fora do limite
explícito desta Sprint ("audit/consolidation Sprint, not a redesign"). Ambos reatribuídos de suas
Sprints de auditoria para decisão do proprietário: a investigação já foi repetida sem informação
nova, e não existe nenhuma Sprint restante no roteiro da EPIC 30 dedicada a construir features —
o próximo passo é priorização de produto, não mais auditoria.

### 26.6 `BD30-F075`/`BD30-F076` — achados confirmados, não corrigidos, encaminhados

**`BD30-F075`** (CSS morta remanescente ligada ao mesmo padrão superado pela `BD30-F074`): uma
tentativa de remover `.activity-card__menu`, `.habit-card__menu`, `.activity-card--menu-open`,
`.habit-card--menu-open` e `.activity-card__actions` de `cards.css` foi iniciada e revertida nesta
Sprint. A busca de marcação confirma que nenhum desses seletores casa com qualquer elemento
renderizado hoje — mas o escopo real da CSS morta é maior do que ficou visível inicialmente: pelo
menos 6 localizações separadas no arquivo, algumas como regras `!important` isoladas cuja remoção
já provou ser sutil nesta mesma Sprint (ver a nota de cautela abaixo), e outras entrelaçadas em
grupos de seletores separados por vírgula que também contêm seletores ainda vivos (por exemplo,
`.activity-card__checkbox, .habit-card__score-button, .activity-card__menu, .habit-card__menu { ... }`
— remover só a parte morta exige editar cada grupo individualmente, não apagar o bloco inteiro).
Corrigir isso sob a janela de uma Sprint de auditoria, sem verificação visual real disponível neste
ambiente, era mais arriscado do que documentar com precisão e encaminhar para uma limpeza dedicada.

**Nota de cautela registrada durante esta Sprint**: uma primeira tentativa de consolidar a regra
`:focus-within` duplicada de `.activity-card`/`.habit-card` (que parecia, à primeira vista, ser uma
cópia 100% redundante de uma regra anterior no mesmo arquivo) foi corretamente revertida ao
perceber que a cópia posterior usa `!important` — e por isso não é redundante: sem ela, uma regra
`!important` diferente e mais tardia no arquivo (que define `border-color`/`box-shadow` da base do
cartão, também com `!important`) venceria mesmo com o cartão em foco, silenciosamente eliminando o
indicador visual de foco. Este é exatamente o tipo de comportamento não-intuitivo de cascata que
justifica a decisão de não prosseguir com a limpeza mais ampla da `BD30-F075` sem verificação visual
real.

**`BD30-F076`** (regra global obsoleta `.beeday-hero__eyebrow`): já causou uma falha real de
WCAG-AA no passado (capturada pelo scan axe-core do repositório), corrigida à época via um truque de
especificidade em vez de remover a regra obsoleta. Funcionalmente correta hoje, mas frágil — depende
da ordem de cascata entre dois arquivos permanecer exatamente como está. Já autodocumentada em
comentário no código; registrada aqui para acompanhamento formal, sem alteração nesta Sprint.

### 26.7 Implementação

- `src/BeeDay.Web/Components/Features/ProfileCreation/Pages/CreateProfile.razor.css` — token de cor
  corrigido (`BD30-F072`).
- `src/BeeDay.Web/wwwroot/css/feedback.css` — fundos de ícone de toast convergidos para tokens
  existentes (`BD30-F072`); `@keyframes beeday-spin` duplicado removido (`BD30-F073`).
- `src/BeeDay.Web/Components/DesignSystem/Cards/BeeDayCardMenu.razor`/`.razor.cs`/`.razor.css`,
  `CardMenuPlacement.cs`, `src/BeeDay.Web/Services/CardActionMenuCoordinator.cs` — removidos
  (`BD30-F074`).
- `src/BeeDay.Web/Program.cs` — registro de DI do serviço removido removido.
- `src/BeeDay.Web/Components/DesignSystem/DesignSystemResources.resx`/`.en-US.resx`/`.pt-BR.resx` —
  3 chaves `CardMenu*` removidas de cada; `DesignSystemResources.cs` — comentário atualizado.
- `tests/BeeDay.Web.Tests/Components/Cards/BeeDayCardMenuTests.cs`,
  `CardMenuPlacementCalculatorTests.cs` — removidos.
- `tests/BeeDay.Web.Tests/Components/DesignSystem/CoreComponentContractTests.cs` — inventário e
  contagens de controle nativo atualizados.
- `tests/BeeDay.Web.Tests/Components/Wallet/WalletUiCoverageTests.cs` — registro de DI supérfluo
  removido.
- `docs/epics/30-system-integrity/README.md` — nova Seção 26; achados `BD30-F072`–`BD30-F076`;
  `BD30-F052` e a metade restante de `BD30-F058` reatribuídos.

Nenhuma mudança de contrato público, comportamento de produto, schema, migration. Nenhuma mutação de
banco HMG/produção foi executada ou é necessária.

### 26.8 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet build BeeDay.slnx` | PASS, 0 warnings, 0 errors |
| `dotnet test tests/BeeDay.Web.Tests/...` (completo) | PASS, 874/874 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | 1.546/1.547 na primeira execução (121 Domain, 119 Application, 216 Infrastructure, 874 Web, 216/217 E2E) — 1 falha em `HomeTests.PublicHomeIsResponsiveAccessibleAndDoesNotOverflow(width: 1920, height: 1080)`, `TimeoutException` em `GotoAsync("/")`/screenshot. Classificada `TRANSIENT/FLAKY` com evidência: Home pública não tocada por esta Sprint, terceira ocorrência consecutiva (Sprints 30.17/30.18/30.19) da mesma assinatura já documentada em `BD30-F042`, sempre um teste diferente, reexecução isolada PASSOU limpa (7/7) |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.547/1.547 (121 Domain, 119 Application, 216 Infrastructure, 874 Web, 217 E2E) — execução limpa, 0 falhas; confirma `TRANSIENT/FLAKY` da execução Debug |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 26.9 Continuidade e entrega

Esta Sprint confirmou que o Design System é maduro e ativamente reusado — a grande maioria da
auditoria (variantes de botão, canonicidade do shell de modal, sistema de ícones, foco visível,
padrões de composição de cartão/empty-state) não encontrou nenhum defeito. Os achados reais foram
todos de baixa severidade: drift de token pontual, duplicação de `@keyframes`, e um subsistema
inteiro (`BeeDayCardMenu`) órfão desde um refactor anterior e agora removido. Uma tentativa de
limpeza mais ampla de CSS morta foi corretamente revertida ao descobrir escopo maior do que o
inicialmente visível — e ao descobrir, no processo, uma dependência de cascata `!important`
não-óbvia que uma remoção apressada teria quebrado silenciosamente. Isso reforça a disciplina desta
auditoria: evidência forte não é o mesmo que risco baixo, e a menor correção correta às vezes é não
corrigir ainda. Os dois achados de UI ausente encaminhados por Sprints anteriores (`BD30-F052`,
`BD30-F058`) foram reverificados uma última vez e reatribuídos à decisão do proprietário — a
auditoria já cumpriu seu papel investigativo nesses dois casos. Nenhuma mutação de banco HMG/produção
foi executada ou é necessária.

## 27. Sprint 30.20 — UX, Accessibility, Responsive & Localization Audit

### 27.1 Escopo e método

Auditoria de UX/acessibilidade/responsividade/localização nas superfícies **autenticadas** do
produto (Daily, Habits/Tasks/Todos/Projects, Wallet, Account/Settings, Onboarding) — as superfícies
públicas/institucionais já foram cobertas na Sprint 30.18, e a consistência do Design System em
nível de componente já foi auditada na Sprint 30.19. Esta Sprint audita jornadas reais: operabilidade
por teclado, ordem/visibilidade/restauração de foco, rotulagem semântica, tamanho de alvos de toque,
zoom/overflow, `prefers-reduced-motion`, acessibilidade de diálogos/overlays, e correção de
localização `pt-BR`/`en-US`.

Seis achados encaminhados por Sprints anteriores foram reverificados: `BD30-F037`, `BD30-F047`,
`BD30-F055`, `BD30-F057`, `BD30-F068`, `BD30-F071` — ver §27.6.

Esta Issue também referencia o "EPIC 30 Remaining Sprint Global Execution Contract" não encontrado
em nenhum lugar do repositório (mesma situação já documentada nas Sprints 30.18/30.19) — seguido o
`CLAUDE.md` vigente e o padrão já demonstrado nas Sprints anteriores desta EPIC.

### 27.2 Achados confirmados — corretos, sem defeito

- **Restauração de foco em diálogos**: todos os diálogos baseados em `EditorModalShell`/
  `BeeDayConfirmDialog`/`BeeDayFeedbackModal`/`ProjectWorkspace` (via `DialogFocusScope`) restauram
  o foco corretamente ao trigger original ao fechar — já provado ponta a ponta por
  `InteractiveComponentsTests.NestedDialogsTrapKeyboardAndRestoreFocusAcrossEscapeClosures`, incluindo
  o caso de borda de um diálogo sem filhos focáveis e o trigger removido antes da restauração.
- **`prefers-reduced-motion`**: respeitado globalmente (`app.css`) e em 32 arquivos CSS adicionais,
  incluindo um kill-switch universal de duração de animação/transição em `animations.css`.
- **Alternativa de teclado para drag-and-drop**: `beeday-sortable.js` implementa `ArrowUp`/
  `ArrowDown` no item focado, usando o mesmo caminho `NotifyReorderAsync` do arrasto por mouse — uma
  capacidade real, não apenas mouse/touch (cobertura de teste ausente, ver `BD30-F083`).
- **Chaves `resx`**: nenhuma divergência encontrada entre os pares neutro/`en-US`/`pt-BR` em 9
  famílias de recursos de área autenticada (531 chaves comparadas no total), nenhum valor vazio.
- **Independência de estado de circuito**: mecânica de cultura (`AuthenticatedCultureSynchronizer`)
  inalterada desde a Sprint 30.4, confirmando `BD30-F047` ainda precisa sem drift adicional.

### 27.3 `BD30-F077` — regressão de touch target, corrigida (achado mais significativo)

`cards.css` continha uma correção de 44px (`2.75rem`) para o checkbox de Task/Todo/Project,
adicionada em 2026-08-13 com comentário explícito referenciando um alvo de toque acessível — mas um
bloco "compact layout" mais antigo (2026-07-25), posicionado mais adiante no mesmo arquivo,
redeclarava os mesmos seletores para `1.55rem` (~24.8px) sem nenhuma menção a acessibilidade. Como
CSS resolve por ordem de código-fonte entre seletores de especificidade igual, a intenção mais
recente e documentada perdia silenciosamente para a mais antiga. Confirmado via `git log -L` em
ambos os blocos, cronologia das duas datas de commit, e leitura completa do arquivo.

**Corrigido**: removida a redeclaração conflitante de `grid-template-columns`/`width`/`height` para
os seletores afetados, deixando a regra de 44px já existente prevalecer. `.habit-card__score-button`
mantido intocado — nunca teve um fix de 44px documentado, então nenhuma regressão foi confirmada
para ele especificamente. Verificado visualmente via captura de tela de um teste E2E temporário
(removido após a verificação) e por toda a suíte E2E de Habit/Task/Todo/Project/Shell (31/31, sem
regressão de overflow ou layout).

### 27.4 `BD30-F079` — mensagens de validação sem associação `aria-describedby`, corrigido

Nenhum dos 5 componentes de formulário compartilhados associava seu controle à própria mensagem de
validação. `BeeDayValidationMessage` ganhou um `Id` opcional envolvendo suas mensagens; cada um dos
5 componentes (`BeeDayInput`, `BeeDayTextArea`, `BeeDaySelect`, `BeeDayDateInput`, `BeeDayCheckbox`)
agora passa `aria-describedby`/`Id` consistentes, cobrindo todos os 6 editores de atividade/tag/
transação que os reutilizam. Novo teste bUnit prova a associação inclusive na atualização em tempo
real quando uma mensagem de validação aparece.

### 27.5 `BD30-F078`/`BD30-F080`/`BD30-F081` — demais achados corrigidos

- `BD30-F078`: `aria-label` por item hardcoded em inglês no componente de reordenação compartilhado
  (`BeeDaySortable.razor`), afetando as 4 listas de `/daily` em ambas as culturas — corrigido com um
  novo parâmetro `ItemAriaLabel` localizado.
- `BD30-F080`: `ProjectWorkspace` (superfície autenticada mais complexa depois do Wallet) sem
  nenhuma cobertura E2E de overflow mobile — novo teste fechou o gap, passou de primeira.
- `BD30-F081`: o drawer de navegação mobile nunca devolvia o foco ao seu gatilho (hamburger) ao
  fechar — `MobileHeader.razor` corrigido para espelhar o padrão de foco já usado por
  `MobileSidebar` na direção inversa; novo teste E2E prova a restauração após Escape em navegador
  real.

### 27.6 Reverificação de achados encaminhados

Todos os seis reconfirmados sem alteração desde suas Sprints originais: `BD30-F037` (Disabled não
vinculado a `IsBusy`; segundo `StateHasChanged()` ausente em `Wallet.razor`), `BD30-F047` (precedência
de cookie de cultura, decisão do proprietário pendente), `BD30-F055` (`Todo.DueDate` sem efeito
funcional), `BD30-F057` (animação antes da requisição em `DeleteCurrentEditorItemAsync`). `BD30-F068`
(estado de passo do wizard fora da URL) e `BD30-F071` (cobertura de troca de idioma do Experience
System) foram reatribuídos — ver §6 — por serem trabalho de arquitetura/feature genuíno fora do
limite de uma Sprint de auditoria, e por não haver mais nenhuma Sprint de auditoria restante que
naturalmente os absorva além de decisão do proprietário ou da Sprint de completude de testes.

### 27.7 Achados menores não corrigidos, encaminhados

`BD30-F082` (menu de criação de atividade sem Escape/clique-fora/navegação por setas): corrigir com
segurança exigiria adicionar suporte a `ElementReference`/foco em `BeeDayButton` (componente
compartilhado) ou um helper de foco via JS interop — infraestrutura não construída nesta Sprint. Um
meio-conserto (só Escape, sem devolução de foco) foi deliberadamente descartado por poder deixar o
foco pior do que hoje. `BD30-F083` (nenhum teste E2E para a reordenação por teclado já implementada).

### 27.8 Implementação

- `src/BeeDay.Web/wwwroot/css/cards.css` — conflito de touch target removido (`BD30-F077`).
- `src/BeeDay.Web/Components/DesignSystem/Forms/BeeDayInput.razor`/`.razor.cs`,
  `BeeDayTextArea.razor`/`.razor.cs`, `BeeDaySelect.razor`/`.razor.cs`,
  `BeeDayDateInput.razor`/`.razor.cs`, `BeeDayCheckbox.razor`/`.razor.cs`,
  `BeeDayValidationMessage.razor`/`.razor.cs` — `aria-describedby` (`BD30-F079`).
- `src/BeeDay.Web/Components/Behaviors/DragDrop/BeeDaySortable.razor`/`.razor.cs`,
  `src/BeeDay.Web/Components/Features/Dashboard/Pages/Home.razor`,
  `DashboardResources.resx`/`.en-US.resx`/`.pt-BR.resx` — `ItemAriaLabel` (`BD30-F078`).
- `src/BeeDay.Web/Components/Layout/MobileHeader.razor` — restauração de foco (`BD30-F081`).
- `tests/BeeDay.Web.Tests/Components/Forms/BeeDayFormTests.cs` — novo teste (`BD30-F079`).
- `tests/BeeDay.E2E.Tests/ProjectLifecycleTests.cs` — novo teste (`BD30-F080`).
- `tests/BeeDay.E2E.Tests/NavigationTests.cs` — novo teste (`BD30-F081`).
- `docs/epics/30-system-integrity/README.md` — nova Seção 27; achados `BD30-F077`–`BD30-F083`;
  `BD30-F068`/`BD30-F071` reatribuídos.

Nenhuma mudança de contrato público de Application, schema, migration. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária.

### 27.9 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| captura de tela de verificação visual (teste E2E temporário, removido) | checkbox de Task renderiza em 44px, sem overflow ou quebra de layout |
| `dotnet test tests/BeeDay.Web.Tests/...` (completo) | PASS, 875/875 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter HabitAndTaskTests\|ShellResponsiveLayoutTests\|TodoLifecycleTests\|ProjectLifecycleTests` | PASS, 35/35 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter NavigationTests` | PASS, 9/9 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.550/1.550 (121 Domain, 119 Application, 216 Infrastructure, 875 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.550/1.550 (121 Domain, 119 Application, 216 Infrastructure, 875 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 27.10 Continuidade e entrega

O achado mais significativo desta Sprint (`BD30-F077`) é uma regressão real e silenciosa: uma
correção de acessibilidade explicitamente documentada e commitada em agosto foi derrotada por uma
regra CSS mais antiga posicionada mais adiante no arquivo, sem que nenhum teste jamais tivesse
verificado o tamanho computado do elemento. Cinco achados foram corrigidos com evidência completa e
verificação visual/E2E; dois foram encaminhados com justificativa precisa de por que corrigi-los
agora seria mais arriscado do que documentá-los (`BD30-F082`, que exigiria infraestrutura nova de
foco em um componente compartilhado) ou fora do escopo de consolidação (`BD30-F083`, cobertura de
teste). Seis achados encaminhados por Sprints anteriores foram reverificados; dois exigiam trabalho
de arquitetura genuíno e foram reatribuídos para decisão do proprietário ou para a Sprint de
completude de testes, em vez de permanecerem indefinidamente presos a uma Sprint de auditoria que já
cumpriu seu papel investigativo. Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

## 28. Sprint 30.21 — Performance & Efficiency Audit

### 28.1 Escopo e método

Auditoria de performance/eficiência mensurável: contagem de consultas SQL, padrões N+1, trabalho
repetido de persistência/rede, renderização desnecessária, paginação, e assets estáticos grandes.
Limite explícito da Sprint: nenhuma micro-otimização especulativa, nenhuma reescrita de arquitetura
justificada só por teoria — apenas gargalos confirmados com evidência.

Dois achados encaminhados por Sprints anteriores foram reverificados nesta Sprint (proprietária de
ambos): `BD30-F033` (índice de ordenação do Wallet) e `BD30-F051` (recarga completa de
`ExperienceEntry`) — ver §28.4.

### 28.2 Achados confirmados — corretos, sem defeito

- **`EfDashboardReadService.GetAsync`**: exatamente 6 consultas SQL fixas por carregamento de
  `/daily` (Users, Habits, RecurringTasks, Projects+Todos via `Include`, Wallets, Transactions) —
  contagem fixa, sem padrão N+1 (nenhum loop emitindo uma consulta por item).
- **Paginação do Wallet**: `EfWalletReadService.ListTransactionsAsync` usa `Skip`/`Take` sobre o
  `IQueryable` antes de `ToListAsync`, traduzido para `OFFSET`/`FETCH` real no SQL Server — não é
  "buscar tudo e fatiar em memória". `CountAsync` também traduzido para SQL, não contado em memória.
  Lookup de `WalletTag` por transação é feito em lote sobre a página já carregada, não por linha.
- **`@key` em loops de lista do Dashboard**: presente em todos os loops de Habit/Task/Todo/Project,
  tanto no branch virtualizado quanto no plano de `BeeDaySortable`, e nas listas de itens concluídos
  em `Home.razor`.
- **`IDbContextFactory` — nenhum `DbContext` de ciclo de vida do circuito**: `grep` por
  `BeeDayDbContext` em `src/BeeDay.Web` retorna zero resultados; todo repositório/leitora cria um
  contexto novo por operação via `AddDbContextFactory`, documentado explicitamente no código como
  decisão deliberada (memória/correção sob circuitos Blazor Server de longa duração).
- **Nenhum outro repositório repete o padrão da `BD30-F051`**: dos 8 repositórios auditados, só
  `EfUserRepository.UpdateAsync` faz o eager-load desproporcional; os demais (`AddAsync` de
  Habit/Project/RecurringTask) fazem só um `MaxAsync` escalar para `Position`, proporcional à
  mutação.

### 28.3 `BD30-F084` — carga completa de transações para calcular resumo do Wallet, corrigido (achado mais significativo)

`EfDashboardReadService.GetAsync` e `EfWalletReadService.GetSummaryAsync` carregavam **todas** as
transações de um usuário pela rede só para somar `Balance`/`TotalIncome`/`TotalExpenses` em memória
via métodos de Domain (`Wallet.CalculateBalance`/`CalculateTotalIncome`/`CalculateTotalExpenses`) —
corretos como lógica de cálculo, mas inadequados como estratégia de leitura, já que `Domain` não tem
acesso a `SUM`/`COUNT` do SQL Server. O custo era pago duplamente: em todo carregamento de `/daily`
e em toda visita a `/wallet` — e, mais grave, `DashboardState.ReloadAsync` recarrega o resumo do
Wallet após **qualquer** mutação de Habit/Task/Todo/Project, não só de Wallet, então a tabela
completa de transações era transferida a cada clique em um Habit, não só ao abrir o Wallet.

**Corrigido**: as duas leitoras agora agregam em SQL (`SumAsync` condicional por `TransactionType`,
`CountAsync`) em vez de materializar cada linha em `List<Transaction>` — mesmo resultado, transferência
de dados independente do volume de transações do usuário. Equivalência funcional comprovada pelos
testes de integração já existentes contra LocalDB real (`EfWalletReadServiceTests.
GetSummaryAsync_CalculatesBalanceIncomeAndExpenses`, `EfDashboardReadServiceTests`) — ambos passaram
sem nenhuma alteração de asserção, confirmando valores exatos idênticos antes/depois da mudança.
`Wallet.CalculateBalance`/`CalculateTotalIncome`/`CalculateTotalExpenses` permanecem no Domain,
usados por outro código (testes de Domain, handlers de Application que já operam sobre um conjunto
pequeno já carregado) — não removidos, não código morto.

### 28.4 Reverificação de achados encaminhados

**`BD30-F033`** (ordenação do Wallet sem índice cobrindo `Description`/`Amount`/`CreatedAtUtc`):
premissa reverificada e inalterada. Decisão desta Sprint (proprietária): risco aceito explicitamente
— adicionar 3 índices novos tem custo real de escrita (todo insert/update de `Transaction` passa a
manter mais índices) sem evidência de consulta lenta real ou de volume que justifique o trade-off;
adicionar um índice especulativo contrariaria o limite explícito desta Sprint. Estado alterado de
`OPEN` para `ACCEPTED RISK`.

**`BD30-F051`** (`EfUserRepository.UpdateAsync` recarrega toda `ExperienceEntry`): reverificado,
premissa inalterada — a consulta já usa `IX_ExperienceEntries_User_Time` (seek, não scan); o problema
é volume de linhas retornadas sem limite, não velocidade de busca. Magnitude real em produção **não
determinável** por esta auditoria, mesma restrição já estabelecida pela `BD30-F034` (nenhum acesso de
leitura a HMG/produção). Uma correção real exigiria trocar o mecanismo de dedup de "carregar toda a
coleção" para uma consulta pontual por chave — mudança de arquitetura na fronteira Domain/
Infrastructure do sistema de XP, já auditada e corrigida múltiplas vezes nesta EPIC (`BD30-F030`),
com risco real de reintroduzir um bug de correção sem evidência de que o problema já é real hoje.
Reatribuído de "30.21" para decisão do proprietário.

### 28.5 Achados menores confirmados, não corrigidos, encaminhados

- `BD30-F085` (baixa, decisão do proprietário): nenhuma paginação/arquivamento existe para as
  coleções de Habits/Tasks/Projects/Todos na camada de leitura — mesma classe estrutural de
  crescimento ilimitado por usuário já registrada para `ExperienceEntry` (`BD30-F051`), mas aqui
  afetando os próprios itens do dashboard.
- `BD30-F086` (baixa, decisão do proprietário): `DashboardState.ReloadAsync` sempre recarrega o
  dashboard inteiro após qualquer mutação individual. Prioridade reduzida nesta Sprint após a
  correção da `BD30-F084` já ter eliminado o componente mais caro dessa recarga.
- `BD30-F087` (baixa, → 30.26): imagens PNG grandes (1,7–1,8 MB) não otimizadas na Home pública,
  fora do caminho crítico autenticado que esta Sprint audita.
- `BD30-F088` (baixa, → 30.26): ausência de passo de build para bundling/minificação de CSS/JS,
  já mitigada por `MapStaticAssets` — gap de hygiene, não defeito de performance medido.

### 28.6 Implementação

- `src/BeeDay.Infrastructure/Persistence/SqlServer/EfDashboardReadService.cs`,
  `EfWalletReadService.cs` — agregação SQL em vez de carga completa de transações (`BD30-F084`).
- `docs/epics/30-system-integrity/README.md` — nova Seção 28; achados `BD30-F084`–`BD30-F088`;
  `BD30-F033` alterado para `ACCEPTED RISK`; `BD30-F051` reatribuído.

Nenhuma mudança de contrato público de Application (mesmo `WalletSummaryResponse`, mesmos valores),
schema, migration ou Design System. Nenhuma mutação de banco HMG/produção foi executada ou é
necessária.

### 28.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Infrastructure.Tests/...` (completo, contra LocalDB real) | PASS, 216/216 — inclui `GetSummaryAsync_CalculatesBalanceIncomeAndExpenses` e as asserções de `Balance` em `EfDashboardReadServiceTests`, mesmos valores exatos antes/depois |
| `dotnet test tests/BeeDay.Application.Tests/...` (completo) | PASS, 119/119 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.550/1.550 (121 Domain, 119 Application, 216 Infrastructure, 875 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.550/1.550 (121 Domain, 119 Application, 216 Infrastructure, 875 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 28.8 Continuidade e entrega

O achado mais significativo desta Sprint (`BD30-F084`) tinha uma característica rara: o custo não
era pago só ao visitar o Wallet, mas a cada carregamento de `/daily` e após qualquer mutação de
Habit/Task/Todo/Project, por causa de como `DashboardState.ReloadAsync` sempre recarrega o resumo
do Wallet junto. A correção (agregação em SQL) é mínima, preserva o contrato público exatamente, e
foi verificada por igualdade de resultado antes/depois nos testes de integração já existentes contra
LocalDB real — nenhum teste novo foi necessário para provar correção, só a execução da suíte já
existente. Dois achados encaminhados de Sprints anteriores foram reverificados e receberam disposição
final desta Sprint (proprietária de ambos): um risco aceito explicitamente (`BD30-F033`) e uma
reatribuição para decisão do proprietário (`BD30-F051`), ambos com justificativa registrada em vez de
permanecerem indefinidamente abertos sem próximo passo claro. Quatro achados menores foram
encaminhados com evidência completa, nenhum corrigido especulativamente. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária.

## 29. Sprint 30.22 — Security & Privacy Audit

### 29.1 Escopo e método

Auditoria de segurança e privacidade: autenticação, autorização, isolamento de posse (IDOR),
sessão, cookies, antiforgery/CSRF, validação de entrada, rate limiting, dados sensíveis em logs/
diagnóstico/erros/configuração, headers de segurança, e privilégio mínimo em workflows/scripts.
Limite explícito: nenhum teste destrutivo, ataque de credencial, ou probe inseguro contra ambientes
compartilhados sem autorização explícita do proprietário — nenhum foi executado.

Quatro achados encaminhados por Sprints anteriores foram reverificados nesta Sprint (proprietária de
todos): `BD30-F008` (CodeQL/Dependabot), `BD30-F041` (rate limiter em memória), `BD30-F048` (PII em
cookie de longa duração), `BD30-F053` (defesa em profundidade de posse em `RemoveAsync`) — ver §29.3.

### 29.2 Achados confirmados — corretos, sem defeito (nenhum IDOR ou bypass de autenticação encontrado)

**Nenhum IDOR ou bypass de autenticação atualmente explorável foi encontrado.** Todo handler que
aceita um id de entidade do cliente (Habit/Task/Todo/Project/Transaction/WalletTag) verifica posse
duas vezes de forma independente: um helper `*Lookup.RequireExistsAsync`/`RequireOwned*Async` na
camada de Application, e um filtro `UserId`/`WalletId` na própria consulta EF da camada de
Infrastructure — nenhum handler faz um `FindById` puro. Confirmado por inspeção direta de todos os
handlers de mutação de Habit/Task/Todo/Project/Wallet.

- **Wallet Transaction/WalletTag**: nenhum caminho de leitura ou escrita permite acessar/editar/
  excluir a Transaction ou WalletTag de outro usuário adivinhando/enumerando um Guid — todo caminho
  resolve primeiro o Wallet do usuário chamador, depois escopa a busca a esse Wallet específico.
- **`MultiUserIsolationIntegrationTests.cs`**: prova ponta a ponta, contra o pipeline HTTP real com
  dois usuários reais, que Dashboard, Habit (registro positivo), Task, Project, Todo, Transaction,
  WalletTag e avatar de Perfil não vazam nem podem ser mutados entre usuários. Gap de cobertura (não
  de código): `UpdateHabitCommand`/`DeleteHabitCommand`/`RegisterHabitNegativeCommand` não são
  exercitados nesse arquivo especificamente — código já provado seguro por inspeção (mesmo padrão de
  `RegisterHabitPositiveCommand`, já coberto), mas não por este teste de integração específico.
- **Cookie de autenticação**: `HttpOnly`, `SameSite=Lax`, `Secure` forçado em Produção
  independentemente do esquema da requisição — provado por teste dedicado contra uma factory
  "production-like".
- **`OnValidatePrincipal`**: controle de invalidação de sessão (`SessionVersion`) intacto e
  incondicional em toda requisição autenticada — provado por 9 cenários reais (troca de senha, reset,
  desativação) em `SessionInvalidationIntegrationTests.cs`.
- **Antiforgery/CSRF**: `/auth/login`, `/auth/logout`, `/culture/set` rejeitam requisições sem token
  válido, provado por testes de integração reais, não só inspeção de middleware; GET nunca muta
  estado.
- **Rate limiting**: valores de produção (`IpPermitLimit=10`, `EmailPermitLimit=5`, janela de 1 min)
  não sobrescritos por nenhum `appsettings*.json`, então se aplicam incondicionalmente em Homologação
  e Produção; resend de confirmação e reset de senha usam a mesma resposta genérica independente de a
  conta existir, sem enumeração possível.
- **`GlobalExceptionHandler`**: detalhes técnicos de exceção só incluídos quando
  `IsDevelopment()` — nunca em Homologação/Produção, em nenhum dos branches.
- **Logging de senha/token**: nenhuma ocorrência de senha ou token bruto passado a um logger em todo
  `src/`; endereço de e-mail só logado (em `DevelopmentEmailSender`) já mascarado.
- **Secrets commitados**: nenhum segredo real encontrado em `appsettings*.json` — o único valor com
  formato de segredo é um placeholder de desenvolvimento (`CHANGEME`) já documentado como nunca
  conectado.
- **Privilégio mínimo em workflows**: todos os 6 workflows declaram um bloco `permissions:` mínimo e
  explícito; nenhum segredo é ecoado em log em nenhum deles.

### 29.3 `BD30-F089` — headers de segurança ausentes, corrigido

`Referrer-Policy`, `X-Content-Type-Options` e `Permissions-Policy` estavam ausentes de toda resposta
— confirmado por um teste de integração já existente e dedicado a esse estado
(`SecurityHeadersIntegrationTests`). Descoberta importante durante a implementação: `X-Frame-Options`
e uma CSP básica (`frame-ancestors 'self'`) **já** são enviados automaticamente pelo próprio
framework Razor Components, independente de qualquer configuração desta aplicação — confirmado pelo
mesmo arquivo de teste e por `docs/security/01-security-baseline.md`. Definir `X-Frame-Options`
diretamente no novo middleware teria arriscado um valor silenciosamente sobrescrito pelo framework
(ou um header conflitante, dependendo do tipo de resposta) — evitado deliberadamente.

**Corrigido**: novo `SecurityHeadersMiddleware` define os 3 headers genuinamente ausentes em toda
resposta, registrado logo após `CorrelationIdMiddleware`. Verificado que o teste que prova os
headers do framework (`X-Frame-Options`/CSP) continua passando sem nenhuma alteração — confirmando
que não foi introduzido nenhum conflito. Teste existente que provava a ausência
(`LoginPage_DoesNotYetSendReferrerPolicyContentTypeOptionsOrPermissionsPolicy`) atualizado para
provar a presença, exatamente como sua própria documentação instruía. `docs/security/
01-security-baseline.md` §4 atualizado para o novo estado real. Uma CSP completa (`script-src` etc.)
permanece planejada, deliberadamente não tentada aqui.

### 29.4 Reverificação de achados encaminhados

**`BD30-F008`** (CodeQL/Dependabot ausentes): metade corrigida — `.github/dependabot.yml`
adicionado (ecossistemas `nuget` e `github-actions`, semanal). Sem impacto operacional: não cria
check obrigatório, não executa código, apenas configura o serviço nativo do GitHub. CodeQL
deliberadamente não adicionado — é um novo workflow que consome minutos de CI a cada push/PR e
tipicamente se torna check obrigatório, mudança de maior impacto na pipeline de CI/CD mais adequada
à Sprint dedicada a isso. Reatribuído (só a parte de CodeQL) para 30.25.

**`BD30-F041`** (rate limiter em memória, por processo): premissa reverificada e inalterada; nenhuma
evidência de escalonamento horizontal desde a Sprint 30.10. A condição que tornaria isso relevante
(PRD provisionado com múltiplas instâncias) ainda não existe. Reatribuído para decisão do
proprietário, a revisitar quando essa condição mudar.

**`BD30-F048`** (PII potencialmente desatualizada em cookie de 14 dias): corrigido — as claims
`ClaimTypes.Name`/`ClaimTypes.Email` removidas da emissão do cookie em vez de mantidas sincronizadas.
Confirmado por busca dedicada (incluindo a propriedade implícita `ClaimsPrincipal.Identity.Name`)
que nada em `src/` as lê de volta — minimização de dados é a correção mais simples e segura.

**`BD30-F053`** (`RemoveAsync` sem reverificação de `UserId`, defesa em profundidade): escopo
confirmado idêntico em 5 repositórios (Habit/Task/Project/Transaction/WalletTag — Todo já é seguro
via o Project pai). Corrigido nos 5: o predicado de re-busca em `RemoveAsync` agora também exige
`UserId`/`WalletId` igual ao da entidade já verificada pelo chamador. Nenhuma mudança de
comportamento no caminho legítimo — confirmado pelos 216 testes de Infrastructure passando sem
alteração.

### 29.5 Implementação

- `src/BeeDay.Web/Diagnostics/SecurityHeadersMiddleware.cs` (novo) — 3 headers ausentes (`BD30-F089`).
- `src/BeeDay.Web/Program.cs` — middleware registrado; claims `Name`/`Email` removidas do login
  (`BD30-F048`).
- `.github/dependabot.yml` (novo) — ecossistemas `nuget`/`github-actions` (`BD30-F008`, metade).
- `src/BeeDay.Infrastructure/Persistence/SqlServer/Repositories/EfHabitRepository.cs`,
  `EfRecurringTaskRepository.cs`, `EfProjectRepository.cs`, `EfTransactionRepository.cs`,
  `EfWalletTagRepository.cs` — filtro de posse em `RemoveAsync` (`BD30-F053`).
- `tests/BeeDay.Web.Tests/Integration/SecurityHeadersIntegrationTests.cs` — teste atualizado para o
  novo estado.
- `docs/security/01-security-baseline.md` — §4 atualizado.
- `docs/epics/30-system-integrity/README.md` — nova Seção 29; achado `BD30-F089`; `BD30-F008`/
  `BD30-F041`/`BD30-F048`/`BD30-F053` dispositionados.

Nenhuma mudança de contrato público de Application, schema, migration. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária. Nenhum probe destrutivo ou ataque de credencial foi
executado contra nenhum ambiente.

### 29.6 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Web.Tests/... --filter SecurityHeadersIntegrationTests` | PASS, 3/3 |
| `dotnet test tests/BeeDay.Web.Tests/...` (completo) | PASS, 875/875 |
| `dotnet test tests/BeeDay.Infrastructure.Tests/...` (completo, contra LocalDB real) | PASS, 216/216 |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter LoginExperienceTests\|AccountLifecycleTests\|AuthenticatedHomeTests` | PASS, 25/25 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 warnings, 0 errors |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.550/1.550 (121 Domain, 119 Application, 216 Infrastructure, 875 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.550/1.550 (121 Domain, 119 Application, 216 Infrastructure, 875 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 29.7 Continuidade e entrega

O resultado mais importante desta Sprint é negativo, no bom sentido: nenhum IDOR ou bypass de
autenticação explorável foi encontrado em toda a superfície de mutação por id do produto — cada
handler verifica posse duas vezes, de forma independente, em duas camadas diferentes. O achado real
mais significativo (`BD30-F089`, headers de segurança ausentes) foi corrigido com cuidado extra ao
descobrir, através de um teste de integração já existente e muito bem documentado, que o próprio
framework Razor Components já controla `X-Frame-Options` e uma CSP básica — evitando duplicar ou
competir com esse controle. Quatro achados encaminhados de Sprints anteriores foram todos
dispositionados nesta Sprint (proprietária de todos): dois corrigidos (`BD30-F048`, `BD30-F053`,
este último estendendo a correção a 5 repositórios em vez de só Habit/Task), um parcialmente
corrigido com a parte de maior impacto operacional encaminhada à Sprint de CI/CD (`BD30-F008`), e um
reatribuído para decisão do proprietário por depender de uma condição de infraestrutura que ainda
não existe (`BD30-F041`). Nenhuma mutação de banco HMG/produção foi executada ou é necessária.

## 30. Sprint 30.23 — Resilience, Errors & Observability Audit

### 30.1 Escopo e método

Auditoria de resiliência a erros e observability: comportamento de exceção não tratada em página
interativa Blazor Server (ausência de `ErrorBoundary`), mapeamento de exceções conhecidas para
`ProblemDetails` em `GlobalExceptionHandler`, presença/consistência de `EventId` em logs
estruturados, e se o `CorrelationId` gerado por requisição realmente se propaga aos logs emitidos
por mutações disparadas pelo circuito SignalR já estabelecido (não só pela requisição HTTP inicial).
Limite explícito: nenhuma mudança especulativa na pipeline global de exceções sem certeza do
mecanismo — quando a evidência estática não bastou para decidir, o achado foi registrado como
encaminhado em vez de corrigido por suposição.

Um achado encaminhado por Sprint anterior foi reverificado nesta Sprint (proprietária): `BD30-F065`
(ausência de `ErrorBoundary` e rota `/Error` órfã) — ver §30.2. Dois achados de Sprints anteriores,
sem relação direta com o escopo desta auditoria mas cronologicamente revisitáveis por tratarem de
confiabilidade/observability, foram reconferidos por leitura direta do código sem mudança de estado:
`BD30-F014` (warnings de EF Core MARS/savepoints correlacionados a um incidente histórico — os dois
arquivos de log daquele período já não existem mais para reanálise; nenhuma evidência nova possível
nesta Sprint, estado inalterado) e `BD30-F036` (`StartCountdown()` reatribuindo `_timer`/`_cts` sem
`Dispose` da instância anterior em `EmailConfirmationSent.razor`/`ResendConfirmation.razor` —
confirmado que ambos os arquivos e o padrão descrito continuam idênticos; ainda inalcançável em uso
normal, estado inalterado).

### 30.2 `BD30-F065` — ausência de `ErrorBoundary`, corrigido

Nenhum `<ErrorBoundary>` existia em toda a árvore de componentes: uma exceção não tratada dentro do
render ou de um event handler de qualquer página interativa (ex.: um clique em `Wallet.razor`)
encerrava o circuito SignalR sem nenhuma tela de recuperação além do `ReconnectModal` genérico
tentando reconectar a um circuito que já não existe mais — o usuário via a interface travar sem
explicação nem ação de recuperação disponível.

**Corrigido**: novo `BeeDayErrorBoundary.razor`, envolvendo `@Body` nos 4 layouts do app
(`MainLayout`, `PublicLayout`, `OnboardingLayout`, `EditorialLayout`). Internamente compõe um novo
`LoggingErrorBoundary : ErrorBoundary` (C# puro, sem `.razor`) em vez do `<ErrorBoundary>` de estoque
— `ErrorBoundaryBase.CurrentException` é `protected`, inacessível a partir de um componente que
apenas compõe (`@ref`) um `<ErrorBoundary>` filho; o padrão de herança, sobrescrevendo o extension
point documentado `OnErrorAsync(Exception)`, é a forma correta e suportada de interceptar a exceção
para log (`WebEventIds.CircuitError`, novo). O `ErrorContent` renderiza um `BeeDayEmptyState` de
marca mais um botão "Recarregar página" (`Navigation.NavigateTo(Navigation.Uri, forceLoad: true)`),
localizado via `DesignSystemResources` (en-US/pt-BR).

A rota órfã `/Error` (`Pages/Error.razor`, nunca produzida por nenhum caminho de código real,
confirmado desde a `BD30-F065` original) permanece deliberadamente fora de escopo — mantê-la como
está, redirecioná-la para dentro do novo `ErrorBoundary`, ou removê-la é decisão de produto, não
inventada por esta correção pontual de resiliência.

Cobertura nova: `FeedbackComponentTests.cs` ganhou 2 testes dedicados —
`ErrorBoundary_WhenChildContentThrows_RendersTheBrandedFallbackInsteadOfCrashing` (prova o fallback
localizado renderizando e nenhum texto de exceção crua vazando ao usuário) e
`ErrorBoundary_WithNoException_RendersChildContentUnchanged` (prova que o caminho normal, sem
exceção, não é afetado). `CoreComponentContractTests.SharedPrimitiveInventoryHasOneCanonicalImplementationPerContract`
atualizado com o novo componente compartilhado (mesma manutenção de "lock" de inventário já feita
nas Sprints 30.19/30.20); contagens de `NativeControlInventoryStaysExplicitUntilOwningFeatureSprints`
não mudaram — `BeeDayErrorBoundary.razor` usa `<BeeDayButton>` (tag de componente), não uma tag
`<button>` nativa, então o regex de varredura não a conta.

### 30.3 `BD30-F090` — `ConcurrencyConflictException` mapeada incorretamente para 503, corrigido

`ConcurrencyConflictException` é `PersistenceException` por herança (`EfConcurrencySaveChanges.cs`
lança essa subclasse especificamente ao capturar `DbUpdateConcurrencyException`), mas
`GlobalExceptionHandler.Map` não tinha nenhum `case` próprio para ela — caía no `case
PersistenceException` mais amplo, mapeado para 503 "Persistence unavailable... Try again shortly".
Resposta enganosa: um conflito de concorrência otimista significa que o registro em si mudou sob o
usuário (outra aba, outro dispositivo, outra sessão), não que o SQL Server está indisponível —
repetir a escrita idêntica e já obsoleta falha de novo, sempre, então "tente novamente em breve" é
literalmente o conselho errado.

**Corrigido**: novo `case ConcurrencyConflictException`, posicionado antes do `case
PersistenceException` (a ordem no `switch` importa — pattern matching por tipo com herança usa o
primeiro `case` compatível), mapeado para 409 Conflict com mensagem acionável ("This record was
changed by another operation. Reload the page and try again.").

Assim como os demais tipos dessa família (documentado em `ProblemDetailsIntegrationTests.cs`), esta
exceção não é alcançável pela suíte de integração HTTP existente — só ocorre dentro de uma chamada
MediatR feita por um componente Razor sobre o circuito SignalR já estabelecido, nunca a partir de uma
requisição HTTP crua nesta aplicação. Em vez de fabricar um endpoint artificial só para forçá-la (o
que a documentação da suíte de integração já rejeita deliberadamente), a cobertura nova é um teste
unitário direto contra `GlobalExceptionHandler.Map` (`GlobalExceptionHandlerTests.cs`, novo),
habilitado por um novo `<InternalsVisibleTo Include="BeeDay.Web.Tests" />` em `BeeDay.Web.csproj` —
mesmo padrão já em uso por `BeeDay.Infrastructure.csproj` para `BeeDay.Infrastructure.Tests`/
`BeeDay.Web.Tests`/`BeeDay.E2E.Tests`. Um segundo teste (`Map_PlainPersistenceException_...`) prova
que o `case PersistenceException` mais amplo continua correto para o caso não especializado.

### 30.4 `BD30-F091`/`BD30-F092`/`BD30-F093` — gaps de observability confirmados, encaminhados

**`BD30-F091`** (baixa): `CorrelationIdMiddleware` só existe na pipeline HTTP ASP.NET Core e seu
`logger.BeginScope` só permanece ativo durante `await next(context)` daquela requisição HTTP
específica — confirmado por leitura direta do middleware e por `builder.Logging.AddJsonConsole(...
IncludeScopes = true)` em `Program.cs` (a infraestrutura de log já grava scopes quando presentes, não
é uma questão de configuração faltando). Em Blazor Server isso cobre só a requisição HTTP inicial; a
partir daí cada interação do usuário roda sobre o circuito SignalR já estabelecido, fora de qualquer
nova invocação desse middleware — então logs emitidos por `LoggingBehavior`/`LoggingErrorBoundary`
durante uma mutação disparada por clique **não carregam `CorrelationId`**, ao contrário de uma falha
capturada por `GlobalExceptionHandler` na pipeline HTTP inicial. Mecanismo confirmado por leitura de
código com confiança suficiente para registrar como achado (não é mais "não resolvido só por leitura
estática" como constava na investigação inicial desta Sprint) — mas construir uma correlação com
escopo de circuito (ex.: `CircuitHandler` gerando um id por circuito, propagado ao pipeline MediatR
via serviço escopado a esse circuito) é mudança de arquitetura de observability genuína, fora do
limite de uma auditoria. Não implementada especulativamente.

**`BD30-F092`** (baixa): nenhuma configuração explícita de `CommandTimeout` existe em toda a base
(`grep` por `CommandTimeout` em `src/` = zero resultados) — toda consulta/gravação SQL Server depende
do default implícito do ADO.NET/EF Core (30s). Definir um valor explícito é decisão de política (qual
latência é aceitável para o workload real do BeeDay?), não inventada por esta auditoria.

**`BD30-F093`** (baixa): `LoggingBehavior.Handle` (Application) loga sucesso/falha de todo request
MediatR sem nenhum `EventId` — inconsistente com a convenção já estabelecida em `WebEventIds.cs`
(Web) para logs estruturados pesquisáveis por id. Application não tem hoje nenhuma convenção
equivalente; criar uma agora para um único call site seria nova abstração sem uso comprovado além
dele. Encaminhado como hygiene para 30.26, mesma Sprint já destino de outros achados de hygiene de
baixo risco (`BD30-F087`/`BD30-F088`).

### 30.5 Implementação

- `src/BeeDay.Web/Diagnostics/WebEventIds.cs` — novo `CircuitError` (6101).
- `src/BeeDay.Web/Components/DesignSystem/Feedback/LoggingErrorBoundary.cs` (novo) —
  `ErrorBoundary` com `OnErrorAsync` sobrescrito para log.
- `src/BeeDay.Web/Components/DesignSystem/Feedback/BeeDayErrorBoundary.razor`/`.razor.css` (novos) —
  fallback de marca (`BD30-F065`).
- `src/BeeDay.Web/Components/DesignSystem/DesignSystemResources.resx`/`.en-US.resx`/`.pt-BR.resx` —
  3 chaves novas (`ErrorBoundaryTitle`/`ErrorBoundaryMessage`/`ErrorBoundaryReloadButton`).
- `src/BeeDay.Web/Components/Layout/{MainLayout,PublicLayout,OnboardingLayout,EditorialLayout}.razor`
  — `@Body` envolvido por `BeeDayErrorBoundary`.
- `src/BeeDay.Web/Diagnostics/GlobalExceptionHandler.cs` — `case ConcurrencyConflictException`
  (`BD30-F090`).
- `src/BeeDay.Web/BeeDay.Web.csproj` — `InternalsVisibleTo` para `BeeDay.Web.Tests`.
- `tests/BeeDay.Web.Tests/Components/Feedback/FeedbackComponentTests.cs` — 2 testes novos
  (`BD30-F065`).
- `tests/BeeDay.Web.Tests/Components/DesignSystem/CoreComponentContractTests.cs` — inventário
  atualizado.
- `tests/BeeDay.Web.Tests/Diagnostics/GlobalExceptionHandlerTests.cs` (novo) — 2 testes (`BD30-F090`).
- `docs/epics/30-system-integrity/README.md` — nova Seção 30; `BD30-F065`/`BD30-F090` corrigidos;
  `BD30-F091`/`BD30-F092`/`BD30-F093` encaminhados; `BD30-F014`/`BD30-F036` reconfirmados sem mudança.

Nenhuma mudança de contrato público de Application, schema, migration. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária.

### 30.6 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet build BeeDay.slnx` | PASS, 0 avisos, 0 erros |
| `dotnet test tests/BeeDay.Web.Tests/...` (completo) | PASS, 879/879 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 avisos, 0 erros |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.554/1.554 (121 Domain, 119 Application, 216 Infrastructure, 879 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.554/1.554 (121 Domain, 119 Application, 216 Infrastructure, 879 Web, 219 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 30.7 Continuidade e entrega

O achado mais significativo desta Sprint (`BD30-F065`) fechou uma lacuna estrutural real: até esta
correção, qualquer exceção não tratada no render ou em um event handler de qualquer página
interativa encerrava o circuito Blazor Server inteiro sem nenhuma tela de recuperação própria do
produto — só o `ReconnectModal` genérico, tentando reconectar a um circuito que já não existe.
`BD30-F090`, descoberto durante a implementação (não fazia parte do escopo original do achado), evita
um conselho de erro literalmente incorreto ("tente novamente em breve" para um conflito de
concorrência, onde repetir a mesma escrita falha sempre). Os três achados de observability
remanescentes (`BD30-F091`/`BD30-F092`/`BD30-F093`) foram confirmados por evidência de código, não
suposição, e corretamente não corrigidos especulativamente — cada um exigiria uma decisão de política
(qual timeout é aceitável, vale a pena construir correlação com escopo de circuito) ou uma nova
convenção sem uso comprovado além de um único call site. Nenhuma mutação de banco HMG/produção foi
executada ou é necessária.

## 31. Sprint 30.24 — Test Engineering Complete Audit

### 31.1 Escopo e método

Auditoria do próprio sistema de testes: mapeamento de cobertura Domain/Application/Infrastructure/
Web/E2E contra jornadas críticas e achados já registrados no Audit Ledger; identificação de falsos
positivos, asserções fracas, testes "só de markup", dados artificiais, estado compartilhado,
dependência de ordem e flakiness; realismo do E2E (cultura, estado de dados existentes,
comportamento real de navegador, artefatos de falha); e reverificação com investigação real (não só
reclassificação) dos achados de engenharia de teste encaminhados por Sprints anteriores:
`BD30-F001`, `BD30-F007`, `BD30-F042`, `BD30-F059`, `BD30-F071`, `BD30-F083`. Limite explícito
(herdado de `CLAUDE.md` e do padrão já demonstrado nas 23 Sprints anteriores desta EPIC, já que o
"EPIC 30 Remaining Sprint Global Execution Contract" referenciado pela Issue não existe em nenhum
lugar do repositório nem do GitHub — mesma ausência já documentada nas Sprints 30.18/30.20): não
otimizar para cobertura de linha como objetivo primário, e não substituir evidência real de
integração/E2E por mocks só por velocidade.

Investigação delegada a um agente de exploração somente-leitura antes de qualquer mudança de código
— metodologia igual à já usada em todas as Sprints anteriores desta EPIC. Achados de qualidade geral
(não mapeados a um `BD30-Fxxx` existente): nenhum teste `Skip`/desabilitado em toda a suíte; nenhum
padrão óbvio de dependência de ordem (`Step1_`, sufixos numéricos); nenhum estado mutável
compartilhado entre testes na varredura realizada; nenhuma asserção `Assert.NotNull(cut)` (tautológica)
na suíte bUnit. Um footgun latente foi identificado e registrado para quem revisitar `BeeDayCard`: se
um futuro consumidor passar `class=` dentro de `AdditionalAttributes` junto com `Class=` já
tipado, a ordem dos frames de atributo do Blazor poderia fazer um silenciosamente vencer o outro —
não observado acontecendo hoje, apenas uma fragilidade estrutural a evitar.

### 31.2 `BD30-F001` — contagem de testes desatualizada em `docs/testing/`, corrigido

`docs/testing/README.md` e `01-testing-strategy.md` registravam 1.116 testes (93/73/129/741/80) desde
a Sprint 25.16 — desatualizado em ~40% frente ao total real. Confirmado por execução direta
(`dotnet test BeeDay.slnx`, Debug e Release, ao final da Sprint 30.23): **1.554 testes, 0 falhas**
(121 Domain, 119 Application, 216 Infrastructure, 879 Web, 219 E2E). Corrigido nos dois documentos
(§1, §7, §8 de `01-testing-strategy.md`; cabeçalho e tabela de `README.md`) — a suíte cresceu
organicamente ao longo das Sprints 26–30 sem que o contador fosse revisitado; os detalhamentos por
arquivo/cenário dentro de `01-testing-strategy.md` (ex.: "12 arquivos", "18 arquivos") não foram
reauditados nesta passada, apenas os totais de topo.

### 31.3 `BD30-F042` — causa raiz da flakiness do E2E em Debug, confirmada

Achado aberto desde a Sprint 30.10, reobservado nas Sprints 30.17/30.18/30.19, nunca root-causado —
apenas uma hipótese de contenção LocalDB/Playwright carregada de uma anotação de memória de sessão
anterior, nunca provada por esta auditoria.

**Confirmado nesta Sprint, com evidência real e direta, não uma nova hipótese**: a própria saída já
capturada nesta EPIC (execuções de `dotnet test BeeDay.slnx` ao final da Sprint 30.23, Debug e
Release) mostra `Infrastructure.Tests` — que cria e derruba bancos LocalDB reais por classe
(`EfLocalDbTestBase`) — tendo seu "Execução de teste para" impresso muito antes de `Web.Tests`/
`E2E.Tests` terminarem sequer de compilar, mas seu resultado final (34s de duração) só sendo impresso
**depois** do resultado de `Web.Tests` (23s). Isso só é possível se `Infrastructure.Tests` ainda
estava rodando enquanto `Web.Tests` (`TestServer`) e `E2E.Tests` (Kestrel + Chromium reais) também
rodavam — ou seja, `dotnet test BeeDay.slnx` como comando único contra a solução inteira executa os
hosts de teste de múltiplos projetos **concorrentemente**, não em sequência. Mesmo tipo de
contenção de recurso já documentado no §4 de `01-testing-strategy.md` para `CREATE`/`DROP DATABASE`
concorrentes dentro de um único projeto, só que agora confirmado também **entre** projetos.

Isso explica por completo o padrão observado ao longo da EPIC: sempre `TimeoutException` de
navegação/screenshot do Playwright (o componente mais sensível a atraso de rede/CPU sob contenção),
nunca o mesmo teste duas vezes (qual teste está "por perto" no momento da contenção é não-
determinístico), nunca em `--configuration Release` (Debug tem JIT/startup mais lento, empurrando uma
primeira requisição já marginal para além do timeout padrão de 30s do Playwright), e nunca em CI
(`ci.yml` não roda LocalDB/browser/host algum; `release-quality-gate.yml` roda os 5 projetos, mas via
loop explícito, um projeto por vez, sempre Release — nunca a combinação que causa o problema). Não
verificado por um trace de processo ao vivo (custaria uma nova execução de ~9 minutos com risco de
não reproduzir de forma determinística) — a evidência indireta já capturada é considerada suficiente
para fechar este achado como causa-raiz confirmada, sem exigir uma nova reprodução instrumentada.

**Corrigido**: mecanismo e contrato de repetibilidade documentados em `01-testing-strategy.md` §7
(nova subseção `BD30-F042`) — uma falha `TimeoutException` de navegação/screenshot observada
especificamente durante um `dotnet test BeeDay.slnx` (Debug, comando também exigido pelo gate de
validação obrigatório de `CLAUDE.md`) contra a solução inteira é consistente com este padrão
conhecido; reexecutar isolado ou em Release antes de classificar como `CHANGE-CAUSED`. Nenhuma
mudança ao comando de gate obrigatório de `CLAUDE.md` foi feita ou está dentro da autoridade desta
Sprint — a correção aqui é entendimento documentado e um contrato de repetibilidade, não uma mudança
de processo de validação.

### 31.4 `BD30-F007` — cobertura formal ausente, corrigido (coleta, não threshold)

Confirmado ainda verdadeiro: nenhum `.runsettings`, nenhuma referência a coverlet em nenhum `.csproj`
ou workflow. **Corrigido**: `coverlet.collector` (10.0.1) adicionado a `Directory.Packages.props` e
aos 5 `.csproj` de teste, mesmo padrão de `PrivateAssets`/`IncludeAssets` já usado para
`xunit.runner.visualstudio`. Verificado por execução real (`dotnet test
tests/BeeDay.Domain.Tests/... --collect:"XPlat Code Coverage"`) produzindo um `coverage.cobertura.xml`
válido. Nenhum threshold/gate de cobertura foi adicionado — decisão de política (qual % é aceitável,
se deveria bloquear PR) fora da autoridade de uma auditoria de engenharia de teste, e o limite
explícito desta Sprint já instruía não tratar cobertura de linha como objetivo primário. A
"cobertura" real e verificável continua sendo o mapeamento de cenários por classe já documentado —
o novo número complementa, não substitui, esse mapeamento.

### 31.5 `BD30-F059` — hipótese estrutural testada e refutada (não corrigido, evidência negativa)

Achado de alta severidade, aberto desde a Sprint 30.15: cards de `WalletTag`/`Transaction` perdem
interatividade de clique/teclado para itens de uma lista já mutada na mesma sessão de circuito;
`@key` já descartado como explicação nas Sprints anteriores.

Investigação desta Sprint encontrou uma correlação estrutural real e testável: `WalletTagManager`/
`TransactionCard` são os únicos consumidores de `BeeDayCard` em todo o produto que passam
`@onclick`/`@onkeydown` como atributos splatados via `[Parameter(CaptureUnmatchedValues = true)]`/
`@attributes` — todo outro card interativo (`ActivityCard`, `HabitCard`) liga esses handlers
diretamente, como atributo Razor literal, sem indireção por dicionário.

**Experimento real conduzido, com reversão honesta ao ser refutado** (mesmo princípio já estabelecido
na Sprint 30.19 para `BD30-F075`): `BeeDayCard` ganhou `OnClick`/`OnKeyDown` tipados; os dois
consumidores migrados para usá-los; os dois testes E2E que reproduzem o defeito
(`CreateExpenseTransaction_DecreasesBalanceCorrectly`, `DeletingATag_LeavesItsTransactionVisibleWithNoTag`)
executados repetidamente com o workaround de `GotoAsync` removido. Resultado:
`CreateExpenseTransaction_DecreasesBalanceCorrectly` (reabrir a segunda Transaction criada na mesma
sessão) continuou falhando de forma consistente e idêntica (mesma linha, mesmo sintoma) mesmo com a
correção aplicada — a hipótese não se sustentou. A mudança completa (`BeeDayCard.razor`/`.razor.cs`,
`WalletTagManager.razor`, `TransactionCard.razor`, e a edição temporária dos dois testes) foi
revertida por completo via `git checkout --` antes de qualquer commit.

**Não corrigido — encaminhado com evidência negativa de valor real**: o splat de atributos via
`AdditionalAttributes` está descartado como explicação (refutado por experimento direto, não apenas
"não confirmado" como nas Sprints anteriores). Causa raiz genuína permanece desconhecida. Reatribuído
para Sprint 30.26; os dois workarounds de `GotoAsync` nos testes existentes permanecem intactos e
necessários.

### 31.6 `BD30-F083` — sem cobertura E2E de reorder por teclado, corrigido

Confirmado sem nenhuma cobertura (`grep` por `ArrowUp\|ArrowDown\|Reorder` em
`tests/BeeDay.E2E.Tests` = zero resultados) para uma capacidade de acessibilidade real e já
implementada (`beeday-sortable.js`, mesmo caminho `NotifyReorderAsync` do drag-and-drop por mouse).
**Corrigido**: novo `ArrowDown_ReordersHabitsAndPersistsAfterReload` em `HabitAndTaskTests.cs` — cria
2 Habits, confirma a ordem inicial via `[data-sortable-item]`, foca o corpo editável do primeiro
(`role="button"`, não é excluído pelo guard `isInteractive` de `beeday-sortable.js`, que
deliberadamente não trata `role="button"` como interativo para não quebrar o próprio drag), pressiona
`ArrowDown` (tecla real via `Page.Keyboard`, não um evento sintético), confirma a troca de ordem no
DOM e a persistência após um `GotoAsync` real. 3/3 execuções isoladas aprovadas antes da entrega.

### 31.7 `BD30-F094` — artefatos de falha do E2E nunca chegam a ser enviados pelo CI, corrigido (achado novo)

`E2ETestBase.cs` já captura screenshot + trace do Playwright em toda falha de teste E2E desde que foi
escrito — mas salvos em `e2e-artifacts/` sob o próprio diretório de build do projeto de teste, nunca
sob `${{ runner.temp }}\TestResults`. `release-quality-gate.yml` (o único workflow que roda
`BeeDay.E2E.Tests` — `ci.yml`'s Fast PR gate não) só faz upload de `TestResults` (`.trx`). Resultado:
um E2E falhando de verdade em CI — exatamente o cenário em que esses artefatos mais importam, quando
não é possível reproduzir localmente sem eles — tinha seu screenshot/trace descartados junto com o
runner efêmero. **Corrigido**: novo step `Upload E2E failure artifacts` em `release-quality-gate.yml`,
logo após o upload de `TestResults`, `if: always()`, `if-no-files-found: ignore` (testes passando não
produzem nenhum arquivo em `e2e-artifacts/`, por design do próprio `E2ETestBase.cs`).

### 31.8 `BD30-F071` — reverificado, inalterado, encaminhado

Cobertura E2E de troca de idioma ao vivo para `/experience-system` continua existindo só para a rota
raiz (1 de 21). Não corrigido nesta Sprint — expansão de cobertura de baixo risco, não defeito;
tempo desta Sprint priorizado para os achados de maior severidade/certeza de causa acima. Reatribuído
para Sprint 30.26.

### 31.9 Implementação

- `docs/testing/README.md`, `docs/testing/01-testing-strategy.md` — contagens reconciliadas
  (`BD30-F001`), mecanismo de `BD30-F042` documentado, seção de cobertura formal atualizada
  (`BD30-F007`).
- `Directory.Packages.props` — `coverlet.collector` (10.0.1).
- `tests/BeeDay.Domain.Tests/`, `BeeDay.Application.Tests/`, `BeeDay.Infrastructure.Tests/`,
  `BeeDay.Web.Tests/`, `BeeDay.E2E.Tests/` — `.csproj` com `PackageReference` para
  `coverlet.collector` (`BD30-F007`).
- `.github/workflows/release-quality-gate.yml` — novo step de upload de artefatos de falha E2E
  (`BD30-F094`).
- `tests/BeeDay.E2E.Tests/HabitAndTaskTests.cs` — novo teste `ArrowDown_ReordersHabitsAndPersistsAfterReload`
  (`BD30-F083`).
- `docs/epics/30-system-integrity/README.md` — nova Seção 31; `BD30-F001`/`BD30-F007`/`BD30-F042`/
  `BD30-F083`/`BD30-F094` corrigidos; `BD30-F059` reatribuído com evidência negativa; `BD30-F071`
  reverificado e reatribuído.

Nenhuma mudança de contrato público de Application, schema, migration. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária. O experimento revertido de `BD30-F059` (§31.5) não deixou
nenhum resíduo no diff final — confirmado por `git status`/`git diff --stat` antes do commit.

### 31.10 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `dotnet test tests/BeeDay.Domain.Tests/... --collect:"XPlat Code Coverage"` | PASS, 121/121, `coverage.cobertura.xml` produzido |
| `dotnet test tests/BeeDay.E2E.Tests/... --filter ArrowDown_ReordersHabitsAndPersistsAfterReload` (x3 isolado) | PASS, 3/3 |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 avisos, 0 erros |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.555/1.555 (121 Domain, 119 Application, 216 Infrastructure, 879 Web, 220 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.555/1.555 (121 Domain, 119 Application, 216 Infrastructure, 879 Web, 220 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |

### 31.11 Continuidade e entrega

O resultado mais valioso desta Sprint não é o número de achados corrigidos, mas a qualidade da
evidência por trás de cada disposição: `BD30-F042`, aberto e reobservado por 4 Sprints sem nunca ser
root-causado, foi fechado com uma causa mecânica concreta e verificável na própria saída de comandos
já executados nesta EPIC — não uma nova hipótese. `BD30-F059`, o achado de maior severidade revisado
nesta Sprint, teve uma hipótese real, estruturalmente plausível e testável genuinamente testada — e
corretamente revertida ao não se sustentar, em vez de enviada como uma correção não comprovada só
porque "parecia certa". `BD30-F094` é a prova de que auditar o sistema de testes não é só sobre os
testes em si, mas sobre toda a cadeia até um humano conseguir agir sobre uma falha real — um E2E
falhando em CI sem seus artefatos de diagnóstico é quase tão inútil quanto não ter o teste. Nenhuma
mutação de banco HMG/produção foi executada ou é necessária.

## 32. Sprint 30.25 — CI/CD, IIS, HMG & Production Readiness Audit

### 32.1 Escopo e método

Auditoria de CI/CD, IIS/HMG e prontidão de produção: os 6 workflows do GitHub Actions e os 12
scripts PowerShell já baselinados (`INV-011`/`INV-012`); reconciliação de `docs/deployment/
01-deployment.md`/`02-runtime-configuration.md` contra o estado real versionado de HMG (`BD30-F006`);
adição do workflow CodeQL deliberadamente adiado da Sprint 30.22 (`BD30-F008`); postura de rollback/
backup de HMG (`BD30-F016`/`BD30-F017`); e reverificação de uma race condition de `Position` já
identificada em Sprints anteriores (`BD30-F032`).

**Correção administrativa de Issue, antes do início desta Sprint**: a Issue #222 (título correto,
"Sprint 30.25 — CI/CD, IIS, HMG & Production Readiness Audit") continha, no corpo, o texto verbatim
da Sprint 30.30 (número de Sprint errado, "Depends on" errado, auto-referência de número de Issue
errada) — confirmado comparando contra a Issue #227 (a Issue real e corretamente populada da Sprint
30.30). Corrigido sob a autoridade de administração de Issues da EPIC (`CLAUDE.md` §7.4/§9.7) antes
de iniciar o trabalho, usando os achados já atribuídos a esta Sprint no Audit Ledger como fonte de
verdade para o escopo real.

Limite explícito, herdado do padrão já estabelecido nas 24 Sprints anteriores desta EPIC (o "EPIC 30
Remaining Sprint Global Execution Contract" referenciado pela Issue segue não encontrado em nenhum
lugar do repositório nem do GitHub): nenhuma ação destrutiva de Git/infraestrutura; nenhuma mutação
de ambiente HMG/produção real fora do consequência normal de um merge autorizado em `hmg`; nenhum
workflow de deploy/promoção disparado manualmente como parte da validação desta Sprint.

### 32.2 `BD30-F006` — documentação de runtime config de HMG invertida, corrigida

`docs/deployment/01-deployment.md` e `02-runtime-configuration.md` descreviam Homologation com
`Resend:Enabled=false`/`Development:Enabled=true` — o oposto do `appsettings.Homologation.json`
atualmente commitado (`Resend:Enabled=true`/`Development:Enabled=false`, confirmado por leitura
direta do arquivo). O runbook mais novo (`14-transactional-email-runbook.md`) e
`docs/infrastructure/06-transactional-email.md` já estavam corretos — só os dois documentos mais
antigos ficaram para trás quando o provider foi de fato invertido em uma Sprint anterior.

**Corrigido**: os dois documentos corrigidos — preservando deliberadamente a narrativa histórica de
incidentes já registrada (Sprint 26.9, Hotfix 26.9.1) exatamente como aconteceu, e corrigindo
apenas a afirmação de estado presente ("hoje"/"atual") embutida nessas seções, que havia se tornado
falsa com o tempo. `docs/deployment/README.md`'s "Estado real de HMG e PRD" (se aplicável) e
`06-transactional-email.md` não precisaram de correção — já estavam certos.

**Descoberta adicional durante a implementação** (não fazia parte do escopo original do achado): a
mesma afirmação invertida também existia como comentário de código, não só em documentação —
`deploy-hmg.yml` (linha do secret `BEEDAY_HMG_ALLOWED_RECIPIENTS`) e duas ocorrências em
`scripts/Deploy-BeeDay.ps1` (doc-comment do parâmetro `-ResendApiKey` e o comentário da lógica de
`Set-BeeDayEnvironmentVariables` que escreve as variáveis do Resend no App Pool). Corrigidos também
— sem nenhuma mudança de comportamento (a lógica de ambos já era condicionada aos valores reais dos
parâmetros/secrets recebidos em tempo de deploy, nunca ao texto do comentário).

### 32.3 `BD30-F017` — sem retenção de backups de deploy, corrigido

Cada deploy de HMG cria um par de backups completos (aplicação + dados) em `C:\Apps\BeeDay-Backups`,
mas nada jamais os expurga — confirmado por busca completa em todos os 12 scripts por lógica de
limpeza baseada em idade: só `Clear-BeeDayStdoutLogs.ps1` existe, e mira um diretório completamente
diferente (`C:\Apps\BeeDay-Data\Logs`).

**Corrigido**: novo `scripts/Clear-BeeDayBackups.ps1`, mesmo padrão autônomo/idempotente já
estabelecido por `Clear-BeeDayStdoutLogs.ps1` — deliberadamente não vinculado ao caminho crítico de
deploy/rollback de `Deploy-BeeDay.ps1`, para que uma falha aqui nunca possa afetar um deploy real.
Diferente do limpador de logs, cada backup aqui é uma árvore de diretório inteira (não um arquivo
único), e o timestamp usado para decidir idade é extraído do próprio nome do diretório (formato
`yyyyMMdd-HHmmss`, o mesmo que `Deploy-BeeDay.ps1` já usa para criá-los) em vez de confiado a
metadados do sistema de arquivos — que seriam ambíguos para uma árvore cujos arquivos internos foram
copiados em instantes ligeiramente diferentes durante o backup.

Piso de segurança adicional além do padrão do limpador de logs: `-MinimumToKeep` (default 3) nunca
expurga os N pares mais recentes de cada tipo, mesmo que todos estejam além de `-RetentionDays` —
justificado porque, até `BD30-F016` ser resolvido, o backup de aplicação/dados é o **único** material
de rollback que este processo de deploy possui; zerar os backups por completo após um período longo
sem deploys removeria essa rede de segurança silenciosamente.

Verificado por execução real, não só leitura de código: um diretório de teste temporário com pares
antigos/recentes provou remoção correta dos pares além do piso, preservação dos pares recentes e do
piso de segurança, idempotência (segunda execução sem nada a remover não lança erro), `-WhatIf` não
removendo nada, e um diretório com nome não-correspondente ao padrão nunca sendo tocado
independentemente da idade. Cobertura de regressão nova (`scripts/tests/Test-ClearBeeDayBackups.ps1`,
15 asserções, mesmo padrão sem framework/orientado a código de saída já usado pelos demais scripts de
teste desta pasta) — adicionada ao mesmo step `Validate deployment script regression suite` de
`deploy-hmg.yml` que já valida as outras 3 suítes antes de qualquer deploy real tocar IIS.

### 32.4 `BD30-F016`/`BD30-F032` — reverificados, confirmados, não corrigidos deliberadamente

**`BD30-F016`** (alta): confirmado por leitura direta do bloco de rollback completo de
`Deploy-BeeDay.ps1` — `Backup-BeeDayDatabase` existe e funciona (`Invoke-Sqlcmd ... BACKUP DATABASE`),
mas é genuinamente inalcançável de qualquer workflow hoje (`-BackupDatabase` nunca é passado nem por
`deploy-hmg.yml` nem por `deploy-prd.yml`); o rollback nunca reverte uma migration, só restaura
arquivos de aplicação a partir do backup. **Não corrigido deliberadamente**: habilitar
`-BackupDatabase` mudaria o comportamento do **próximo deploy real de HMG** contra o SQL Server real
— permissão de escrita da conta de serviço no diretório de destino não verificada, espaço em disco
não verificado, e dependeria de `BD30-F017` (agora corrigido) para não acumular backups SQL sem
limite. Mutação de comportamento de ambiente real fora da autoridade desta auditoria — decisão do
proprietário.

**`BD30-F032`** (baixa): confirmado ainda preciso por leitura direta das 4 configurações EF Core —
os 4 índices (`IX_Habits_User_Position`, `IX_Projects_User_Position`, `IX_RecurringTasks_User_Position`,
`IX_Todos_Project_Position`) existem, nenhum com `.IsUnique()`, ao contrário do idioma já estabelecido
no mesmo código-base para colunas genuinamente únicas (confirmado comparando com
`WalletTagConfiguration`/`UserConfiguration`/`UserTokenConfiguration`, que chamam `.IsUnique()`
explicitamente — a ausência aqui não é uma lacuna de leitura, é genuinamente diferente do padrão).
**Não corrigido deliberadamente**: adicionar a constraint é uma migration de schema que falharia (ou
teria efeito indefinido) se qualquer duplicata de `Position` já existir hoje em dados reais de HMG/
produção — verificar isso exige acesso de leitura a um banco de HMG/produção que esta auditoria não
tem nem está autorizada a usar. Decisão do proprietário: alguém com acesso de leitura autorizado
precisa verificar ausência de duplicatas antes que essa migration possa ser considerada segura.

### 32.5 `BD30-F008` — workflow CodeQL adicionado (portão informativo, não obrigatório)

Metade Dependabot já corrigida na Sprint 30.22; a metade CodeQL foi deliberadamente adiada para esta
Sprint, com o motivo já registrado no achado: um novo workflow do GitHub Actions tipicamente se torna
um check obrigatório uma vez configurado, mudança de maior impacto operacional mais adequada à Sprint
dedicada a CI/CD.

**Corrigido**: novo `.github/workflows/codeql.yml` — `github/codeql-action/init`+`analyze` para
`csharp`, `build-mode: autobuild`, disparado em PR para `hmg` (mesma fronteira de `ci.yml`) e
semanalmente (`schedule`, mesma cadência de `dependabot.yml`). Deliberadamente **não** obrigatório —
este repositório trata uma transição de check obrigatório como uma decisão de ritual real (ver o
próprio comentário de cabeçalho de `ci.yml` sobre como sua superfície de check obrigatório só mudou
após prova empírica); essa decisão pertence ao proprietário, não a esta Sprint. `permissions:` mínimo
e explícito (`contents: read`, `security-events: write` — o único novo escopo, necessário só para
`codeql-action/upload-sarif`), mesmo padrão já em uso em todo o resto de `.github/workflows/`.

**Validação real do primeiro run**: como o workflow dispara em `pull_request: [hmg]`, o próprio PR
desta Sprint contra `hmg` já constitui a primeira execução real — resultado registrado em §32.7 antes
da entrega, não assumido a partir da sintaxe YAML sozinha.

### 32.6 Implementação

- `docs/deployment/01-deployment.md`, `02-runtime-configuration.md` — `Resend`/`Development`
  corrigidos, narrativa histórica preservada (`BD30-F006`).
- `.github/workflows/deploy-hmg.yml` — comentário desatualizado corrigido (`BD30-F006`); novo step
  de teste de regressão adicionado ao preflight (`BD30-F017`).
- `scripts/Deploy-BeeDay.ps1` — 2 comentários desatualizados corrigidos (`BD30-F006`).
- `scripts/Clear-BeeDayBackups.ps1` (novo) — retenção de backups com piso de segurança (`BD30-F017`).
- `scripts/tests/Test-ClearBeeDayBackups.ps1` (novo) — 15 asserções (`BD30-F017`).
- `docs/deployment/04-operations.md` — §7/§9 atualizados para o novo estado de `BD30-F016`/`BD30-F017`.
- `.github/workflows/codeql.yml` (novo) — análise CodeQL informativa (`BD30-F008`).
- `docs/epics/30-system-integrity/README.md` — nova Seção 32; `BD30-F006`/`BD30-F017`/`BD30-F008`
  corrigidos; `BD30-F016`/`BD30-F032` reverificados e reatribuídos para decisão do proprietário;
  `INV-011`/`INV-012` reverificados.

Nenhuma mudança de contrato público de Application, schema, migration. Nenhuma mutação de banco
HMG/produção foi executada ou é necessária. O novo workflow `codeql.yml` é o único item desta Sprint
cujo primeiro efeito real só se observa em CI, não localmente — resultado real registrado abaixo.

### 32.7 Regressão e quality gates locais

| Comando | Resultado observado |
|---|---|
| `[System.Management.Automation.Language.Parser]::ParseFile` (todos os `.ps1` tocados) | PASS, 0 erros de sintaxe |
| `scripts/tests/Test-ClearBeeDayBackups.ps1` (execução real, isolado) | PASS, 15/15 asserções |
| `python3 -c "import yaml; yaml.safe_load(...)"` (`deploy-hmg.yml`, `codeql.yml`) | PASS, YAML válido |
| `dotnet format BeeDay.slnx --verify-no-changes` | PASS, exit 0 |
| `dotnet build BeeDay.slnx --configuration Release --warnaserror` | PASS, 0 avisos, 0 erros |
| `dotnet test BeeDay.slnx` (Debug, completo) | PASS, 1.555/1.555 (121 Domain, 119 Application, 216 Infrastructure, 879 Web, 220 E2E) — execução limpa, 0 falhas |
| `dotnet test BeeDay.slnx --configuration Release` | PASS, 1.555/1.555 (121 Domain, 119 Application, 216 Infrastructure, 879 Web, 220 E2E) — execução limpa, 0 falhas |
| `dotnet ef migrations has-pending-model-changes --project src/BeeDay.Infrastructure --startup-project src/BeeDay.Infrastructure` | PASS, nenhuma mudança pendente no modelo |
| `git diff --check` | PASS |
| `codeql.yml` — primeiro run real (PR #290 desta Sprint contra `hmg`) | PASS — jobs `CodeQL Analysis` (5m45s) e `CodeQL` (3s) verdes; `build-mode: autobuild` compilou `BeeDay.slnx` com sucesso |

### 32.8 Continuidade e entrega

Três achados de infraestrutura real desta Sprint (`BD30-F016`, `BD30-F032`) foram corretamente
identificados como **não seguros para corrigir sem acesso que esta auditoria não tem** — habilitar
backup SQL contra o HMG real, ou adicionar uma constraint única sem antes verificar duplicatas em
dados reais, são ambos mutações que exigem verificação prévia fora do alcance de uma auditoria
documental/de código. Corrigir apenas o que é seguro corrigir sem essa verificação (`BD30-F017`,
a ferramenta de retenção que se torna prerequisito quando `BD30-F016` for eventualmente resolvido) e
documentar precisamente o que falta, com evidência real, é o resultado correto — não uma correção
parcial insegura. A correção administrativa da Issue #222 no início desta Sprint (corpo duplicado da
Sprint 30.30) é um lembrete de que a própria infraestrutura de tracking da EPIC precisa do mesmo
padrão de verificação que o código. Nenhuma mutação de banco HMG/produção foi executada ou é
necessária.
