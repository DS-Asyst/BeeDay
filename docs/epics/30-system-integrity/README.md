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
| INV-008 | Rotas e shell | 54 declarações `@page` encontradas diretamente em componentes Razor | `BASELINED` | 30.17 |
| INV-009 | Fluxos funcionais | Identity/Auth/User, Dashboard, Habits, Tasks, Todos, Projects, Wallets, Experience, Onboarding e páginas públicas identificados | `MAPPED` (Identity/Auth `VERIFIED` na 30.10) | 30.4, 30.10–30.18 |
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
| BD30-F009 | média | existem apenas dois guards automatizados de dependência, cobrindo Domain e Application; Infrastructure e Web não têm guard equivalente | `FIXED` | 30.9 |
| BD30-F010 | baixa | o índice de documentação classifica `authentication/` e `developer/` como reservados e `api/` como não reauditado | `OPEN` | 30.28 |
| BD30-F011 | baixa | `docs/infrastructure/README.md` registra 5 classes Options; o repositório possui 6 Options atuais, além de `EmailProvider` e `EmailProviderSelector` | `OPEN` | 30.7 |
| BD30-F012 | baixa | existe documentação versionada da EPIC 28, mas ela não aparece no índice `docs/README.md` | `OPEN` | 30.28 |
| BD30-F013 | alta | em HMG, validar `TransactionFormModel.Amount` sob `pt-BR` lançava `ArgumentException`/`FormatException` em `RangeAttribute.SetupConversion` ao interpretar o limite textual `"0.01"` pela cultura corrente; a falha ocorria no `EditForm`, antes de MediatR e antes de qualquer `INSERT` | `FIXED` | 30.2 |
| BD30-F014 | baixa | os logs do mesmo período contêm warnings do EF Core sobre MARS/savepoints, mas a cadeia causal confirmada do incidente termina na validação DataAnnotations antes de MediatR/persistência; não há evidência de participação desses warnings nesta falha | `OPEN` | 30.7 |
| BD30-F015 | média | `docs/deployment/04-operations.md` ainda afirmava que não existiam deploy automatizado de HMG nem aplicação de migrations, além de registrar caminhos e fluxo de release obsoletos; os workflows e a execução real provam o fluxo CI artifact -> HMG Deployment -> HMG Verification | `FIXED` | 30.3 |
| BD30-F016 | alta | o rollback de HMG restaura aplicação e configuração do App Pool, mas não desfaz migrations; embora `Deploy-BeeDay.ps1` implemente `-BackupDatabase`, `deploy-hmg.yml` não o habilita e não há evidência versionada de backup SQL externo correlacionado ao deploy | `OPEN` | 30.25 |
| BD30-F017 | média | cada deploy cria backups de aplicação e dados em `C:\Apps\BeeDay-Backups`, mas não existe política versionada de retenção, expurgo ou restore automatizado de uma execução histórica | `OPEN` | 30.25 |
| BD30-F018 | alta | a confirmação de e-mail tem cobertura robusta de Application/Integration para sucesso, token inválido/expirado/replay, reenvio e throttle, mas nenhuma jornada Chromium atravessa um link real até liberar o login | `FIXED` | 30.10 |
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
| BD30-F030 | alta | `UserExperience.Entries` participa da deduplicação em memória, porém era ignorada no mapping relacional; `ExperienceEntry` é top-level e nada jamais adicionava novas entries ao `DbSet` — confirmado por teste real contra LocalDB: nenhuma linha era persistida, e a mesma fonte podia ser recompensada indefinidamente (recompletar um Todo/Task/Project já concluído antes) | `FIXED` | 30.7 (revalidar impacto em 30.16) |
| BD30-F032 | baixa | `EfHabitRepository.AddAsync`/`EfProjectRepository.AddAsync`/`EfRecurringTaskRepository.AddAsync`/`EfProjectRepository.AddTodoAsync` calculam a próxima `Position` via `MaxAsync` seguido de um insert separado, sem índice/constraint único em `(UserId, Position)` (ou `(ProjectId, Position)` para Todo) — duas inserções concorrentes do mesmo usuário podem computar o mesmo `maxPosition` e persistir ordinais duplicados; não há perda de dado, apenas dessincronia cosmética de ordenação, autocorrigível no próximo reorder | `OPEN` | 30.25 |
| BD30-F033 | baixa | `EfWalletReadService.ApplyOrdering` ordena `Transaction` por `Description`/`Amount`/`CreatedAtUtc` sem índice cobrindo esses campos (apenas `IX_Transactions_Wallet_Date` existe) — SQL Server ordena em tempdb após o seek por `WalletId`; impacto real baixo dado o volume típico de transações por usuário em um app financeiro pessoal | `OPEN` | 30.21 |
| BD30-F034 | alta | histórico de `ExperienceEntry` não era persistido antes da correção da Sprint 30.7 (`BD30-F030`); alternar conclusão/reabertura repetida de Todo/Task/Project podia conceder XP duplicado sem limite antes da correção. Existência e magnitude de inflação histórica em HMG/produção **não quantificadas** por esta Sprint — nenhuma consulta ou mutação de banco de HMG/produção foi executada. As linhas de `ExperienceEntry` persistidas antes da correção podem ser insuficientes para reconstruir `TotalExperience` corretamente de forma determinística (o histórico anterior à correção nunca existiu). Nenhuma mutação de banco está autorizada por este achado; nenhum reset/recálculo arbitrário é permitido | `OPEN` | 30.16 |
| BD30-F035 | média | `BeeDayWebService` (20 métodos) e os call sites diretos de `ISender.Send` em `Wallet.razor` e nas páginas de Identity/Account/Onboarding nunca propagavam um `CancellationToken` real — toda chamada usava implicitamente `CancellationToken.None`, então navegar para longe ou fechar o circuito Blazor Server nunca cancelava uma mutação/query em andamento no servidor | `FIXED` | 30.8 |
| BD30-F036 | baixa | `EmailConfirmationSent.razor`/`ResendConfirmation.razor`: `StartCountdown()` reatribui `_timer`/`_cts` sem descartar a instância anterior se chamado uma segunda vez antes do `Dispose()` do componente — hoje inalcançável em uso normal (o botão fica desabilitado enquanto `_secondsRemaining > 0`), portanto latente, não explorável | `OPEN` | 30.10 |
| BD30-F037 | baixa | polimento de UX não-bloqueante: cards individuais do Dashboard (Habit/Task/Todo/Project) não têm `Disabled` vinculado a `State.IsBusy` (só o overlay global `BeeDayLoading` reflete ocupado — a proteção contra double-submit é real, aplicada em `DashboardState.ExecuteAsync`, mas o clique num segundo card fica sem feedback visual imediato); e `Wallet.razor.RefreshAfterMutationAsync` não chama `StateHasChanged()` uma segunda vez após zerar `_highlightBalance`, então o destaque visual do saldo pode não sumir até outro render não relacionado ocorrer | `OPEN` | 30.20 |
| BD30-F038 | média | `Login.razor` mantinha uma implementação própria de `IsLocalPath` para sanitizar `ReturnUrl`, distinta e mais fraca que a canônica de `LoginDestinationResolver` — só rejeitava `//`, não a variante `/\` que navegadores normalizam para URL absoluta; a decisão de redirecionamento real em `POST /auth/login` já usava a canônica completa, então não era explorável via o próprio fluxo de login, mas era uma fronteira de segurança duplicada e incompleta | `FIXED` | 30.10 |
| BD30-F039 | alta | `ConfirmEmail.razor` enviava `ConfirmEmailCommand` em `OnInitializedAsync`, que roda duas vezes sob o `@rendermode="InteractiveServer"` global (`<Routes>` em `App.razor`) — uma vez no prerender estático, outra na reconexão interativa. A primeira chamada confirmava o e-mail corretamente; a segunda, idêntica, era corretamente rejeitada pela proteção contra replay de token — mas essa segunda rejeição é o estado final que o navegador do usuário real exibe, então todo usuário real via "Link já utilizado" no primeiro clique legítimo em um link de confirmação real. Só detectável por um teste de navegador real (Chromium) — nenhum teste unitário/bUnit/integração via `HttpClient` exercita as duas passagens de render do Blazor Server | `FIXED` | 30.10 |
| BD30-F040 | baixa | `User.SetActive` (que corretamente invalida sessões ao desativar) não é chamado por nenhum Command/handler alcançável — só por testes. **Determinação da Sprint 30.11** (auditoria funcional completa de Profile/Onboarding/Account/Settings, incluindo inspeção direta de `Account.razor` e as três seções de Settings): classificado como **fluxo de produto ausente**, não código morto — o método de Domain está correto, testado e o guard de `OnValidatePrincipal` (`!user.IsActive`) funciona; simplesmente não existe hoje nenhuma entrada de produto (autoatendimento ou administrativa) que o alcance. Decidir se/como construir essa entrada é uma decisão de política de produto fora da autoridade desta auditoria — não inventada aqui | `OPEN` | decisão do proprietário |
| BD30-F041 | baixa | `MemoryIdentityRequestThrottle` e `LoginRateLimiterFactory` (`PartitionedRateLimiter`) são ambos em memória, por instância de processo — corretos para o único servidor IIS de HMG hoje, mas não compartilhados entre instâncias; uma futura implantação horizontalmente escalada (PRD em Azure, ainda não provisionada) contornaria o limite de taxa distribuindo requisições entre instâncias | `OPEN` | 30.22 |
| BD30-F042 | média | confiabilidade da suíte E2E em Debug: três execuções completas de `dotnet test BeeDay.slnx` durante a Sprint 30.10 produziram, respectivamente, 6/194, 1/194 e 1/194 falhas — nunca o mesmo teste duas vezes, sempre `TimeoutException` de navegação (`GotoAsync`)/screenshot, nunca um teste de Identity/Auth desta Sprint. A execução `--configuration Release` subsequente passou 194/194 sem qualquer falha. Evidência suporta `CHANGE-CAUSED = NO` para a Sprint 30.10, mas confirma um problema real de confiabilidade da suíte E2E em Debug, independente desta Sprint. **Não confirmado como causa raiz** — contenção LocalDB/Playwright é registrada aqui apenas como padrão observado/hipótese consistente com uma anotação de memória de sessão anterior a esta Sprint, não como causa provada por esta auditoria. A Sprint 30.24 deve investigar a causa real e definir o contrato de repetibilidade de teste | `OPEN` | 30.24 |
| BD30-F043 | baixa | `ProfileCreationState.cs` (state class `AddScoped`, mesmo padrão de ciclo de vida de `DashboardState`) nunca propagava `CancellationToken` em suas 4 chamadas a `BeeDayWebService` — a varredura da Sprint 30.8 (`BD30-F035`) só buscou `*.razor`/`*.razor.cs`, e esta é uma classe `.cs` simples, fora do glob | `FIXED` | 30.11 |
| BD30-F044 | alta | `UpdateCurrentUserAccountCommandHandler` permitia alterar o e-mail da conta sem reverificar a senha atual e sem resetar `IsEmailConfirmed` — uma sessão sequestrada (cookie roubado, XSS) bastava para trocar silenciosamente o e-mail para um endereço controlado pelo atacante, que `RequestPasswordResetCommandHandler` então tratava como já confirmado, habilitando um fluxo completo de esqueci-minha-senha e bloqueio do dono legítimo. Primitivo de account takeover real | `FIXED` | 30.11 |
| BD30-F045 | média | `Tutorial.razor.NextAsync`, no slide final, chamava `Store.CompleteOnboardingAsync()` sem `try/catch` e o componente não injetava `ToastService` — uma falha (rede, 5xx transitório, sessão expirada) se propagava como exceção não tratada no circuito Blazor, sem nenhum feedback ao usuário, ao contrário de todo outro caminho de salvamento do app | `FIXED` | 30.11 |
| BD30-F046 | baixa | `docs/web/04-feature-components.md` descrevia `Tutorial.razor` navegando para `/daily` ao concluir o onboarding; o código real (e `LoginDestinationResolver.Resolve`) sempre navegou para `/profile` | `FIXED` | 30.11 |
| BD30-F047 | média | `AuthenticatedCultureSynchronizer.SynchronizeAtLoginAsync`: um cookie `BeeDay.Culture` desatualizado em um segundo dispositivo/navegador, ao logar, silenciosamente sobrescreve uma alteração de idioma deliberada feita em Settings — comportamento documentado e intencional (cookie explícito sempre vence naquela sessão), mas conflita com o critério de aceite "preferência de idioma permanece consistente entre sessões autenticadas". Decisão de produto necessária: cookie deveria ceder à conta no login, ou cookies não deveriam sobreviver a uma troca de idioma na conta | `OPEN` | 30.20 |
| BD30-F048 | baixa | `Program.cs` grava `ClaimTypes.Name`/`ClaimTypes.Email` no cookie `BeeDay.Auth` (até 14 dias, "remember me") no login, mas nunca os atualiza após uma edição de Nome/E-mail em Account — hoje inofensivo (`grep` confirma que nada no código lê essas duas claims de volta), mas é PII potencialmente desatualizada sentada num cookie de longa duração | `OPEN` | 30.22 |
| BD30-F049 | média | nenhum teste baseado em viewport real (Playwright) cobria `/profile/create` Etapa 2 (apelido), `/account`/`/settings` (as 3 seções) ou `/onboarding/tutorial` — `LoginExperienceTests` só provava a Etapa 1 do cadastro; `AccountLifecycleTests`/`SettingsLocalizationTests` nunca chamavam `SetViewportSizeAsync` | `FIXED` | 30.11 |

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
