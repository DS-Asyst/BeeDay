# Functional Journey Matrix

Matriz reconstruída na Sprint 30.4 a partir das rotas, componentes, handlers, repositórios e testes
existentes no repositório. Ela não substitui os contratos de cada área: concentra a evidência que
prova cada jornada e torna explícito onde a prova ainda é apenas por camadas ou está ausente.

**Reconciliada na Sprint 30.29** (Full-System Regression & Realistic State Validation): três `GAP`
já fechados em Sprints anteriores (`BD30-F018`/`BD30-F019`/`BD30-F020`) nunca haviam sido atualizados
aqui — a matriz não recebeu nenhuma edição desde sua criação até esta Sprint. Corrigido; ver §5.
Também nesta Sprint: primeira jornada E2E cobrindo estado realista com múltiplas features
simultâneas (Habit + Task + Project/To-Do + Wallet) na mesma sessão de usuário, navegando entre
elas — `CrossFeatureRealisticStateTests`, ver `tests/BeeDay.E2E.Tests/`.

## 1. Como interpretar a matriz

| Estado | Significado |
|---|---|
| `E2E` | há prova em Chromium real atravessando a interface e o runtime Web |
| `LAYERED` | o comportamento está coberto em uma ou mais camadas adequadas, mas não no navegador |
| `GAP` | falta evidência material; o finding e a Sprint proprietária estão registrados |

Os responsáveis abaixo são Sprints da EPIC 30. A Sprint funcional decide o contrato da área; a
Sprint 30.24 é responsável pelo endurecimento transversal da suíte e dos gates. Um teste E2E não
substitui regras de Domain/Application nem testes de persistência, e um teste unitário não prova
navegação, cookies, circuitos Blazor ou comportamento do navegador.

## 2. Visão executiva

| Jornada suportada | Owner funcional / prova | Happy path | Validação e recuperação | Persistência | Autorização e navegação | Situação |
|---|---|---|---|---|---|---|
| visitante | 30.18 / 30.24 | Home pública, CTAs e páginas públicas em `HomeTests` | responsividade, a11y, foco e fallback de cultura | cookie de cultura em Integration | público permitido; rotas protegidas redirecionam | `E2E` |
| cadastro | 30.10 / 30.24 | navegador chega a confirmação pendente | validators, throttle e falha de e-mail em Application/Integration | criação atômica de usuário/perfil/token | links públicos e destino de confirmação | `E2E` + `LAYERED` |
| confirmação de e-mail | 30.10 / 30.24 | `CreateAccount_ConfirmsEmailThroughARealLink_ThenUnlocksLogin` atravessa um link real em Chromium | inválido, expirado, replay, reenvio e throttle | token consumido e usuário confirmado | login bloqueado antes e aceito depois | `E2E` + `LAYERED` |
| login | 30.10 / 30.24 | login real direciona a profile/onboarding | senha inválida, usuário ausente/inativo/não confirmado sem enumeração | cookie e session version | destinos e limites protegidos comprovados | `E2E` + `LAYERED` |
| onboarding | 30.11 / 30.24 | navegador completa o fluxo e chega ao Daily | componentes e validator de perfil | flag de conclusão e perfil em Application | login escolhe onboarding ou profile | `E2E` + `LAYERED` |
| Daily | 30.17 / 30.24 | shell autenticado, dashboard e deep links | skeleton/estados vazios/componentes | snapshot SQL em `EfDashboardReadServiceTests` | acesso protegido, sidebar e drawer | `E2E` + `LAYERED` |
| hábitos | 30.12 / 30.24 | criar e concluir atualiza saldo e XP | título inválido e regras no Domain/Application/Web | round-trip, update, reorder, delete e concorrência SQL | isolamento por usuário e retorno ao Daily | `E2E` + `LAYERED` |
| tarefas | 30.13 / 30.24 | criar e concluir, inclusive teclado | repeat inválido e estados do editor | round-trip, update, reorder, delete e concorrência SQL | isolamento por usuário e Daily | `E2E` + `LAYERED` |
| to-dos | 30.13 / 30.24 | criar, editar (com persistência), concluir e excluir um To-Do em `TodoLifecycleTests` | editor, data opcional, ownership e reward | add/update/remove/move/reorder SQL | workspace protegido e isolamento por usuário | `E2E` + `LAYERED` |
| projetos/workspace | 30.14 / 30.24 | criar projeto, abrir workspace, mutar to-do dentro dele e recarregar preservando progresso em `ProjectLifecycleTests` | descrição inválida e estados de componente | projeto e filhos persistidos com ordem/cascade | workspace parte do Daily autenticado | `E2E` + `LAYERED` |
| Wallet | 30.15 / 30.24 | criar tag/transação; editar/excluir mínimo em pt-BR | validators, ownership, duplicidade e recovery do modal | repositories e read service SQL | rota protegida e navegação pelo shell | `E2E` + `LAYERED` |
| configurações/conta | 30.11 / 30.24 | perfil e idioma salvos pelo navegador | erro localizado e regras de senha/perfil | idioma/tema/perfil em Application e integração | `/account`/`/settings` protegidos | `GAP` BD30-F021 |
| localização | 30.20 / 30.24 | alternância pública e autenticada en-US/pt-BR | cookie ausente, inválido e cultura não suportada | preferência persiste e converge com cookie | cultura preservada entre auth/logout | `E2E` + `LAYERED` |
| logout | 30.10 / 30.24 | logout real encerra sessão | idempotência e isolamento de sessões em Integration | cookie de autenticação removido; cultura preservada | Daily volta a exigir login | `E2E` + `LAYERED` |

## 3. Evidência por jornada

### 3.1 Visitante, identidade e conta

- `HomeTests` prova Home, CTAs, responsividade, acessibilidade, foco e troca pública de idioma.
- `AccountLifecycleTests` prova cadastro até confirmação pendente, login, onboarding, edição de
  perfil e logout bloqueando o retorno ao Daily.
- `LoginIntegrationTests`, `EmailConfirmationIntegrationTests`, `LogoutIntegrationTests`,
  `AuthorizationIntegrationTests` e `ProblemDetailsIntegrationTests` cobrem os contratos HTTP,
  falhas seguras, redirects e limites de autorização.
- `AccountRegistrationTests`, `AuthenticationHandlersTests`, `IdentityHandlersTests` e
  `UserAccountHandlersTests` cobrem atomicidade, regras de identidade e recuperação abaixo da UI.
- `AccountTests`, `WelcomeTests`, `TutorialTests`, `LoginTests` e
  `IdentityFormConvergenceTests` exercitam estados e mensagens dos componentes.
- Falta atravessar em Chromium um link real de confirmação até o login liberado
  (`BD30-F018`). O contrato de token já está coberto em Integration; a Sprint 30.10 deve adicionar
  a prova de navegador sem duplicar essa matriz de casos.

### 3.2 Daily, hábitos, tarefas, to-dos e projetos

- `AuthenticatedHomeTests`, `NavigationTests` e `HabitAndTaskTests` provam shell, Daily,
  navegação, criação/conclusão de hábitos e tarefas e abertura do workspace de projeto.
- Os component tests de Dashboard, editores e workspace cobrem estados visuais, localização e
  interação isolada; `RequestValidatorTests` cobre os requests compartilhados.
- `EfDashboardReadServiceTests`, `EfHabitRepositoryTests`,
  `EfRecurringTaskRepositoryTests` e `EfProjectRepositoryTests` provam a persistência SQL,
  ordenação, mutações, concorrência e filhos do projeto.
- `MultiUserIsolationIntegrationTests` impede acesso cruzado aos cinco agregados.
- **Fechado nas Sprints 30.13/30.14** (`BD30-F019`/`BD30-F020`, reverificado na Sprint 30.29):
  `TodoLifecycleTests` prova criar um To-Do dentro de um Project, alternar conclusão, editar (com
  persistência após reload) e excluir; `ProjectLifecycleTests` prova o workspace sobrevivendo a um
  reload real de página com progresso e lista de To-Dos intactos, além de editar/excluir o Project.
  Esta matriz ficou desatualizada por várias Sprints depois que o gap foi fechado — nenhuma edição
  havia sido feita desde a criação original (Sprint 30.4); corrigida na Sprint 30.29.

### 3.3 Wallet

- `WalletTests` atravessa login, shell, tag, transação, saldo, teclado, responsividade e filtros.
- O caso pt-BR cria `0.01`, edita para `0.02`, exclui e verifica que o circuito continua
  interativo, preservando a regressão confirmada na Sprint 30.2.
- `WalletHandlersTests`, `WalletValidatorTests`, os component tests de Wallet e os repositories
  SQL cobrem validação, ownership, filtros, concorrência e round-trip.
- Não há gap crítico de fundação E2E aberto nesta matriz; a auditoria funcional profunda continua
  pertencendo à Sprint 30.15.

### 3.4 Configurações, localização e logout

- `SettingsLocalizationTests` prova o POST real de cultura, reload e persistência; `HomeTests`
  prova a alternância pública.
- `AuthenticatedCultureIntegrationTests`, `CultureCookieIntegrationTests`,
  `RequestLocalizationIntegrationTests`, `PublicFlowLocalizationIntegrationTests` e
  `IdentityFlowLocalizationIntegrationTests` cobrem precedência, fallback e convergência.
- `AccountLifecycleTests` e `LogoutIntegrationTests` provam o encerramento da sessão e a
  preservação independente da cultura.
- O navegador não cobre ainda tema, alteração de senha e recuperação visível dos demais saves de
  conta (`BD30-F021`). A Sprint 30.11 deve escolher casos representativos, mantendo as regras de
  senha e perfil em Application/Integration.

## 4. Fundação E2E compartilhada

`E2ETestBase` mantém um `BrowserContext` isolado por teste, captura screenshot/trace somente em
falha e oferece duas operações mecânicas:

- `GotoAsync(path)` navega e espera o circuito Blazor estabilizar;
- `SubmitLoginAsync(email, password)` abre o formulário compartilhado, preenche os campos e o
  submete.

`SubmitLoginAsync` deliberadamente não cria usuário, não decide o destino e não contém asserts.
Cada jornada continua mostrando seu arranjo de usuário e verificando explicitamente URL, página e
resultado. `AccountLifecycleTests` mantém o login inline porque ali o próprio formulário e seus
destinos são o comportamento sob teste.

## 5. Plano de fechamento dos gaps

**Reverificado na Sprint 30.29** — este plano descrevia o estado no momento da criação da matriz
(Sprint 30.4); `BD30-F018`/`BD30-F019`/`BD30-F020` já estão fechados há várias Sprints (30.10/30.13/
30.14 respectivamente), só a tabela abaixo e as linhas correspondentes em §2 nunca haviam sido
atualizadas para refletir isso.

| Finding | Prova mínima esperada | Sprint | Situação |
|---|---|---|---|
| BD30-F018 | confirmação por link real no navegador, login bloqueado antes e permitido depois | 30.10 | Fechado — `AccountLifecycleTests.CreateAccount_ConfirmsEmailThroughARealLink_ThenUnlocksLogin` |
| BD30-F019 | criar, editar, concluir, recarregar e excluir um to-do no workspace | 30.13 | Fechado — `TodoLifecycleTests` |
| BD30-F020 | projeto + mutação de to-do + reload preservando workspace e progresso | 30.14 | Fechado — `ProjectLifecycleTests` |
| BD30-F021 | casos representativos de tema/senha e recovery visível de save de conta | 30.11 | Ainda `OPEN` no Ledger — ver `docs/epics/30-system-integrity/README.md` |
| BD30-F001/BD30-F007 | reconciliar inventário de testes e instituir cobertura formal | 30.24 | Fechado — ver Ledger Sprint 30.24 |

As Sprints funcionais devem manter os asserts de negócio próximos da jornada. A Sprint 30.24 deve
reconciliar inventário, execução, cobertura e publicação de artifacts, sem transformar helpers em
uma DSL que esconda seletores ou falhas.
