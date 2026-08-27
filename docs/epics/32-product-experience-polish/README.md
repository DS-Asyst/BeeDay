# EPIC 32 — beeday Code Refinement & Product Experience Polish

Este documento é o **Experience Ledger** canônico da EPIC 32, criado pela Sprint 32.1 conforme
exigido pela Issue #244 §5. A Issue #244 (EPIC) e as Issues de cada Sprint continuam sendo a fonte
operacional de escopo, ordem e autorização. O Ledger registra evidência versionada de comportamento
de interação/experiência real do produto, encaminhando cada achado para exatamente uma Sprint
proprietária dentro da EPIC 32 (32.2–32.19).

**Fonte da verdade:** inventário construído a partir de `docs/web/02-routing-and-pages.md`,
`docs/web/03-layouts.md`, `docs/web/04-feature-components.md`, `docs/design-system/02-components.md`,
`docs/ux/01-guidelines.md`, `docs/ux/02-accessibility.md`, `docs/ux/03-responsive.md`,
`docs/testing/03-functional-journey-matrix.md`, leitura direta de código sob
`src/BeeDay.Web/Components/`, e execução real da aplicação (`dotnet run`, SQL Server LocalDB local,
`BeeDay_Sprint32_Baseline`) navegada via automação de browser Chromium real (não simulação) em
2026-08-22. O snapshot começou em `a68d0f135f1b537433397d1e55503ea1f28e4c75` (`hmg` = `origin/hmg`),
na branch `sprint/32.1-experience-baseline-interaction-inventory`, criada a partir de `hmg`
sincronizada com `origin/hmg`.

## 1. Finalidade e regras do Ledger

O Ledger é a fonte operacional de achados de experiência de produto durante toda a EPIC 32. Ele não
substitui a auditoria profunda que pertence a cada Sprint 32.2–32.19 — a Sprint 32.1 inventaria e
estabelece a baseline, não corrige.

Estados usados durante a EPIC (definidos pela Issue #244 §5):

```text
DISCOVERED
IN_REVIEW
FIXED
VERIFIED
ACCEPTED
```

Estados terminais: `FIXED`, `VERIFIED`, `ACCEPTED`. `ACCEPTED` exige evidência e justificativa
explícitas e nunca é usado para aceitar risco material de produto/segurança/usuário em nome do
proprietário sem aprovação dele.

Todos os achados registrados nesta Sprint estão em `DISCOVERED` — nenhuma correção foi aplicada
nesta Sprint além do que o próprio §5.1 permite (nenhum bloqueio foi encontrado que exigisse
correção para permitir a análise da baseline, então nenhuma exceção foi usada).

### 1.1 Por que nenhuma correção foi aplicada nesta Sprint

A Issue #245 (Sprint-Specific Boundary) autoriza corrigir um blocker apenas quando ele impede a
análise confiável da baseline, e exige registrar essa exceção explicitamente. Nenhum achado listado
abaixo impediu a inventariação — todos foram observados e documentados normalmente. Por isso nenhum
código de produto foi alterado nesta Sprint; a única mudança desta entrega é este documento (mais a
reconciliação de Issue/Project).

### 1.2 Sprint 32.6.1 — Autonomous Validation & E2E Gate Separation (Issue #342, governança, sem achado de produto)

Sprint de emenda de governança inserida antes da 32.7 (roadmap atualizado da Issue #244), motivada
pelo hang determinístico de `AccountLifecycleTests.Login_CompletesOnboarding_ReachesDashboard`
registrado como `ACCEPTED RISK` em `docs/testing/01-testing-strategy.md` §7 (Sprint 32.5) — o
proprietário decidiu que Claude nunca deve depender de LocalDB/E2E como gate de validação autônomo.
Nenhum achado `EXP32-Fxxx` foi consumido ou alterado; nenhum código de produto mudou.

**Reconciliação verificada (evidência, não suposição):**

- A Issue #244 já continha, no início desta Sprint, o **EPIC 32 Global Execution Contract**
  atualizado (§4 execução autônoma sem parada entre Sprints; §5 política de validação code-first —
  proibição de LocalDB, `BeeDay.E2E.Tests` preservado mas não executado pelo Claude, proibição de
  `dotnet test BeeDay.slnx`; §6 preservação do split de CI) — nada precisou ser escrito de novo ali.
- As 14 Issues de Sprint restantes (`#251`–`#264`, Sprints 32.7–32.20) já referenciam "The updated
  EPIC 32 Global Execution Contract in #244 applies in full" e já contêm, cada uma, sua própria
  seção "Sprint-Specific Validation" proibindo LocalDB/`BeeDay.E2E.Tests`/`dotnet test BeeDay.slnx`
  e nenhuma exige `CONTEXT RESET REQUIRED` — confirmado por leitura direta via `gh issue view` em
  `#251`, `#252`, `#253`, `#258` e `#264` (amostra cobrindo início, meio e fim do intervalo restante).
  Único ajuste necessário: `#251` referenciava esta Sprint só pelo título (`Depends on: Sprint
  32.6.1 — Autonomous Validation & E2E Gate Separation`), sem número — corrigido para `(#342)` após
  esta Issue ser criada.
- O split de CI já é exatamente o exigido: `ci.yml` (Fast Gate — `pull_request: branches: [hmg]`,
  sem filtro de branch de origem, portanto todo PR destinado a `hmg`, não só `sprint/*→hmg`) roda
  somente `Domain.Tests`+`Application.Tests`; `release-quality-gate.yml` (fronteira `hmg→main`) roda
  os 5 projetos completos, incluindo `BeeDay.E2E.Tests` e Format — confirmado por leitura direta dos dois
  workflows e por `docs/testing/01-testing-strategy.md` §7 (reconciliado na Sprint 31.11). Nenhuma
  mudança de workflow foi necessária ou aplicada.
- `CLAUDE.md` é um ativo de governança **local ao ambiente**, não rastreado neste repositório desde
  `869b57e` ("chore(governance): stop tracking CLAUDE.md in the product repository", Sprint anterior
  a esta) — está fora do escopo de qualquer commit/PR desta Sprint. O proprietário atualizou esse
  arquivo local separadamente durante esta própria Sprint para remover o mandato de
  `dotnet test BeeDay.slnx` do gate obrigatório do Claude (§9.1), substituindo-o pelas invocações
  explícitas de `Domain.Tests`/`Application.Tests` e por uma proibição explícita de LocalDB —
  consistente com a política já registrada na Issue #244.
- Nenhum teste `BeeDay.E2E.Tests` foi removido, pulado ou enfraquecido nesta Sprint.

**Conclusão:** o contrato de validação/execução já estava reconciliado na cadeia de Issues antes
desta Sprint existir formalmente; o trabalho desta Sprint foi verificar essa reconciliação com
evidência, criar o registro formal (Issue #342, sub-issue de #244), corrigir a única referência
desatualizada (`#251`) e documentar o estado aqui. Nenhuma correção de código/workflow foi
necessária — consistente com `Required Work #5` da Issue #342 ("preserve... unless a real defect is
found").

## 2. Ambiente do baseline

| Item | Evidência |
|---|---|
| Branch base | `hmg` = `origin/hmg` = `a68d0f135f1b537433397d1e55503ea1f28e4c75` (inclui a remoção do Dependabot, `chore: remove Dependabot automation per owner decision`) |
| Branch da Sprint | `sprint/32.1-experience-baseline-interaction-inventory` |
| SDK | .NET SDK (mesma toolchain fixada em `dotnet-tools.json`/`global.json` do repositório) |
| Banco local | SQL Server LocalDB (`MSSQLLocalDB`), database `BeeDay_Sprint32_Baseline`, migrada via `dotnet ef database update` com `BEEDAY_DESIGNTIME_CONNECTION` (ver `src/BeeDay.Infrastructure/Persistence/SqlServer/BeeDayDbContextFactory.cs`) |
| Execução da aplicação | `dotnet run --project src/BeeDay.Web/BeeDay.Web.csproj`, `ASPNETCORE_ENVIRONMENT=Development`, `http://localhost:5059`, conexão via `BeeDay__Persistence__SqlServer__ConnectionString` apontando para o LocalDB acima |
| E-mail de desenvolvimento | `DevelopmentEmailSender` (`src/BeeDay.Web/Data/Emails/`, `.gitignore` linha 55) — usado para obter o link real de confirmação de e-mail durante o cadastro do usuário de teste |
| Usuário de teste | conta real criada via fluxo de cadastro completo (`sprint32-baseline@example.com` / nickname `sprint32baseline`), e-mail confirmado por link real, onboarding completo até `/daily` |
| Evidência de navegador | Chromium real controlado via automação de browser (não simulação/snapshot estático) — navegação, cliques, digitação, leitura de DOM/acessibilidade e inspeção de `document.activeElement`/estilos computados |
| Limitação desta sessão | redimensionamento de viewport para larguras móveis (`resize_window`) não teve efeito no navegador desta sessão (`window.innerWidth` permaneceu no tamanho físico do monitor, 2552px, mesmo após a chamada reportar sucesso) — nenhuma evidência de navegador em viewport móvel real foi coletada nesta Sprint; ver §9 |

## 3. Metodologia e evidência

Cada achado abaixo é uma de duas classes:

- **Evidência de navegador nesta Sprint** — observado ao vivo nesta sessão: clique, digitação,
  inspeção de `document.activeElement`, `getComputedStyle`, atributos ARIA reais via
  `document.querySelectorAll` sobre o DOM renderizado. Marcado como tal no campo Evidência de cada
  achado.
- **Evidência de código/documentação já verificada** — extraída de documentação mantida
  (`docs/ux/`, `docs/web/`, `docs/design-system/`) cuja própria fonte de verdade já declara ter sido
  verificada diretamente em código/CSS/testes em Sprints anteriores (16.7, 16.8, 25.6, 25.10,
  25.15/25.16). A Sprint 32.1 não reexecutou essas varreduras — ela reencaminha o achado, ainda
  aberto, para dentro do vocabulário e do fluxo de Sprint da EPIC 32, porque ele é escopo de
  interação/experiência que a EPIC 32 existe para resolver e ainda não foi corrigido por nenhuma
  Sprint anterior.

Fluxos exercitados ao vivo nesta Sprint (usuário real, dados reais, sem mocks):

1. Home pública (`/`) → cadastro (`/profile/create`, 3 passos: dados, senha, nickname) → confirmação
   de e-mail por link real → login → onboarding (`/onboarding/tutorial`, 5 slides) → `/profile`.
2. `/daily`: menu de criação (Habit/Task/To-Do/Project), criação de 1 Habit, 1 Task e 1 Project;
   abertura do `ProjectWorkspace`, criação de 1 To-Do dentro do projeto; alternância de conclusão via
   checkbox da coluna To-Dos do board; verificação do filtro "Show completed projects"; busca por
   texto no board (`water`) com `SearchHighlight` e recontagem das 4 colunas.
   validação de campo obrigatório (`Title is required.`) no editor de Habit; teste de fechamento via
   `Escape`.
3. `/wallet`: criação de 1 transação (Expense, `$4.50`), edição, abertura do diálogo de exclusão
   (`BeeDayConfirmDialog`) e cancelamento (transação preservada); inspeção de formatação de valor e
   data.
4. `/settings` (`/account`): seções Profile e Security (Password) renderizadas e inspecionadas.
5. `/login`: validação nativa do formulário HTML (campo vazio).
6. Páginas públicas: `/faqs` (acordeão nativo `<details>/<summary>`), `/experience-system`
   (overview + grid de pilares).

Áreas do escopo da Issue #245 cobertas apenas por evidência de código nesta Sprint (sem interação de
navegador própria desta sessão), com razão explícita — ver §9 para o detalhamento completo:

- responsivo/mobile em qualquer rota (limitação de ferramenta, §2);
- estados de carregamento com atraso visível (`BeeDayLoading`, atraso de 350ms) — todas as mutações
  desta sessão completaram rápido demais localmente para observar o overlay renderizado;
- seção Preferences (tema/idioma) de `/settings` — Profile e Security foram alcançadas; Preferences
  não foi revisitada nesta sessão após uma instabilidade de renderização do navegador de automação
  (não do produto — ver nota abaixo) ter tornado a rolagem da página pouco confiável;
- filtros/tags do Wallet (busca, tipo, tag, intervalo de data) — apenas o formulário de criação foi
  testado ao vivo; os filtros têm cobertura de teste automatizado existente (`WalletTests`,
  `WalletValidatorTests`) citada pela Matriz de Jornadas Funcionais (`docs/testing/03-functional-journey-matrix.md`).

**Nota sobre instabilidade de ferramenta descartada.** Durante a inspeção de `/settings`, duas
tentativas de rolar a página via roda do mouse produziram capturas de tela com zoom/deslocamento
inconsistentes e dois timeouts de `Page.captureScreenshot` ("renderer may be frozen"); uma terceira
tentativa, em uma aba nova, com o mesmo fluxo, renderizou corretamente sem nenhuma anomalia. Como o
comportamento não foi reproduzível de forma consistente e a única evidência de JavaScript coletada
(`window.scrollY`, ausência de `transform`/`filter` em toda a cadeia de ancestrais) contradisse o que
as capturas de tela mostravam, esta Sprint trata isso como artefato da ferramenta de automação de
browser, não como achado de produto — nenhum `EXP32-Fxxx` foi aberto para isso. Registrado aqui por
transparência, não como achado.

## 4. Cobertura por área do produto

| Área (Issue #245) | Rotas/componentes relevantes | Evidência coletada nesta Sprint | Novos achados |
|---|---|---|---|
| Rotas autenticadas e públicas | 54 rotas, `docs/web/02-routing-and-pages.md` | Documento revalidado (Sprint 30.17); navegação real por 9 rotas representativas | — |
| Shell de aplicação e navegação | `MainLayout`, `DesktopSidebar`, `MobileHeader`, `MobileSidebar`, `docs/web/03-layouts.md` | Shell desktop (`/profile`, `/daily`, `/wallet`, `/settings`) navegado ao vivo; shell mobile não verificado ao vivo (§9) | — |
| Layouts de página | `PublicLayout`, `EditorialLayout`, `OnboardingLayout`, `MainLayout` | Todos os 4 layouts visitados ao vivo (Home, FAQs, cadastro/login/onboarding, shell autenticado) | — |
| Botões e hierarquia de ação | `BeeDayButton`, botões especializados (`habit-editor__direction-button`) | `BeeDayButton` observado em uso extensivo; botão de toggle especializado inspecionado via DOM | EXP32-F001 |
| Formulários e inputs | `BeeDayInput`/`Date`/`Select`, `InputNumber` monetário, formulário nativo de Login | Formulários de Habit/Task/Project/Transaction preenchidos e submetidos ao vivo; Login testado com campo vazio | EXP32-F005, EXP32-F007, EXP32-F011 |
| Diálogos/modais | `EditorModalShell`, `BeeDayConfirmDialog` | Criação e exclusão (cancelada) testadas ao vivo com `Escape` | EXP32-F002 |
| Listas, cards e coleções | `DashboardColumn`, `ActivityCard`/`HabitCard`, `ProjectWorkspace` To-Do list | Cards de Habit/Task/Project e lista de To-Do do workspace inspecionados ao vivo | EXP32-F003, EXP32-F004 |
| Busca, filtros e ordenação | Busca do Daily (`filter-bar__input`), filtros do Wallet | Busca do Daily testada ao vivo (filtragem + `SearchHighlight` + recontagem); filtros do Wallet não exercitados nesta sessão (cobertura de teste existente citada) | EXP32-F013 |
| Carregamento e performance percebida | `BeeDayLoading` (atraso de 350ms), `BeeDayDashboardSkeleton` | Apenas evidência de código (`docs/ux/01-guidelines.md` §3/§8) — mutações desta sessão completaram antes do atraso de 350ms | — |
| Estados vazios/primeiro uso | `BeeDayEmptyState`, colunas do Daily, Wallet, Tags | 4 colunas vazias do Daily e Wallet sem transações observados ao vivo; ambiguidade vazio-vs-sem-resultados descoberta ao vivo | EXP32-F013 |
| Erros e recuperação | Banner inline, `BeeDayValidationMessage`, validação nativa do Login | Validação de campo obrigatório (Habit) e validação nativa (Login) testadas ao vivo | EXP32-F007 |
| Toasts/notificações/confirmações | `BeeDayToastHost`/`ToastService`, `BeeDayConfirmDialog` | 4 toasts de sucesso observados ao vivo (Habit/Task/Project/Transaction); diálogo de exclusão observado ao vivo | — |
| Teclado/foco/acessibilidade | `DialogFocusScope`, `FocusOnNavigate`, ARIA | `Escape` testado em 2 modais; `aria-pressed` ausente confirmado via DOM; foco pós-fechamento confirmado via `document.activeElement` | EXP32-F001, EXP32-F002, EXP32-F008, EXP32-F009, EXP32-F010 |
| Responsivo/mobile | Breakpoint 1200px do shell; breakpoints do Daily (900/620px) | Apenas evidência de código/documentação (`docs/ux/03-responsive.md`) — sem evidência de navegador nesta sessão (§9) | — |
| Daily, Habits, Tasks, To-Dos, Projects | `Features/Dashboard`, `Habits`, `Tasks`, `Todos`, `Projects` | Um de cada entidade criado, editado (Project) e um To-Do concluído ao vivo | EXP32-F001–F004, F013 |
| Wallet | `Features/Wallets` | Transação criada, editada, exclusão cancelada; formatação de moeda/data inspecionada | EXP32-F005, EXP32-F006 |
| Settings/Profile/Account | `Features/Account` (Profile, Security, Preferences) | Profile e Security verificados ao vivo; Preferences apenas por código (§9) | — |
| Superfícies públicas e de marca | Home, `/faqs`, `/experience-system`, `docs/web/02-routing-and-pages.md` §9/§10 | 3 rotas públicas representativas verificadas ao vivo; as 9 restantes das 12 editoriais cobertas apenas por documentação já verificada (Sprint 29.4) | — |

Nenhuma área do escopo da Issue #245 ficou sem revisão — as marcadas "apenas por código/documentação"
têm razão explícita registrada em §3/§9, conforme exigido pelo critério de aceite "No major product
area remains unreviewed without an explicit evidence-backed reason."

## 5. Inventário de rotas

O inventário completo e verificado das 54 rotas vive em
[`docs/web/02-routing-and-pages.md`](../../web/02-routing-and-pages.md) (última verificação: Sprint
30.17) e não é duplicado aqui. Esta Sprint confirmou por navegação real: `/`, `/login`,
`/profile/create`, `/account/email-confirmation-sent`, `/account/confirm-email`,
`/onboarding/tutorial`, `/profile`, `/daily`, `/wallet`, `/settings`, `/faqs`,
`/experience-system`. Nenhuma rota indocumentada ou quebrada foi encontrada nessa amostra; todos os
links internos usados nesses fluxos (cadastro → confirmação → login → onboarding → Daily; navegação
lateral Profile/Daily/Wallet/Account) resolveram corretamente.

## 6. Experience Ledger — achados

Todos os achados abaixo estão em estado `DISCOVERED`. Resolução e proteção de regressão pertencem à
Sprint proprietária listada; nenhuma foi aplicada nesta Sprint (§1.1).

### EXP32-F001 — Botões de direção do Habit não expõem estado via ARIA

| Campo | Valor |
|---|---|
| Área | Botões e hierarquia de ação / Daily |
| Rota/Página | `/daily` — editor "Create Habit" / "Edit Habit" |
| Componente | `HabitEditorModal` (`.habit-editor__direction-button`, botões "Positive"/"Negative") |
| Severidade | HIGH |
| Device | Shared (afeta qualquer input method, mas o impacto prático é maior para leitor de tela) |
| Accessibility Impact | Yes |
| Owning Sprint | 32.4 (Buttons & Action Hierarchy) — cross-ref 32.13 (Focus, Keyboard & Interaction Accessibility) |
| State | **FIXED** — Sprint 32.4 |

**Interação:** alternar qual direção (`Positive`/`Negative`/`Both`) um Habit registra, via dois
botões dedicados no editor.

**Comportamento anterior:** os dois botões (`+ Positive`, `− Negative`) alternavam a classe CSS
`active` para comunicar qual(is) direção(ões) está(ão) habilitada(s) — por padrão, um novo Habit tem
ambos `active` (`HabitDirection.Both`, confirmado via `docs/web/04-feature-components.md` §5). Nenhum
dos dois botões possuía `aria-pressed` ou qualquer outro atributo ARIA de estado.

**Problema:** um usuário de leitor de tela não tinha como saber, a partir do próprio botão, se
"Positive" e/ou "Negative" estava atualmente habilitado — a única fonte de verdade era uma classe CSS
visual (`.habit-editor__direction-button.active`). Isso era inconsistente com o restante do
inventário de estado do Design System (`docs/design-system/02-components.md` §2, que lista
`aria-pressed`/`aria-expanded` como o padrão para toggles como `.beeday-icon-toggle` e `CardMenu`).

**Evidência (navegador, Sprint 32.1):** `document.querySelectorAll('button')` dentro do dialog do
editor retornou os dois botões com `ariaPressed: null` em ambos; `outerHTML` confirmou
`class="habit-editor__direction-button active"` em ambos por padrão, sem nenhum atributo `aria-*`
de estado.

**Resolution:** `HabitEditorModal.razor` agora declara `aria-pressed="@PositiveAriaPressed"` /
`aria-pressed="@NegativeAriaPressed"` nos dois botões, com `PositiveAriaPressed`/`NegativeAriaPressed`
(`HabitEditorModal.razor.cs`) derivados diretamente de `AllowsPositive`/`AllowsNegative` — a mesma
fonte que já controlava a classe CSS `active` — seguindo o padrão canônico já estabelecido em
`DashboardColumn.AriaPressed` (`beeday-icon-toggle`, string `"true"`/`"false"`). Nenhuma mudança
visual: apenas o atributo ARIA foi adicionado, a classe CSS `active` permanece a única fonte de
estilo.

**Evidência (E2E, Sprint 32.4):** `HabitAndTaskTests.HabitDirectionButtons_ExposeAriaPressedAndStayInSyncAfterToggling`
(Chromium real via Playwright) abre o editor "Create Habit", confirma `aria-pressed="true"` em ambos
os botões no estado inicial (`HabitDirection.Both`), clica Positive e confirma
`aria-pressed="false"`/`"true"`, clica Positive novamente e confirma o retorno a `"true"`/`"true"`, e
finalmente clica Negative a partir de `Both` confirmando `"true"`/`"false"` — prova end-to-end de que
o atributo permanece sincronizado com `HabitDirection` através de toda transição, não apenas na
renderização inicial.

**Regression Protection:**
- `HabitEditorModalTests.DirectionButtons_AriaPressed_MatchesHabitDirectionOnInitialRender` (bUnit,
  `[Theory]` cobrindo `Both`/`Positive`/`Negative`).
- `HabitEditorModalTests.TogglePositive_OnABothDirectionHabit_UpdatesAriaPressedOnBothButtons` /
  `ToggleNegative_OnABothDirectionHabit_UpdatesAriaPressedOnBothButtons` (bUnit).
- `HabitAndTaskTests.HabitDirectionButtons_ExposeAriaPressedAndStayInSyncAfterToggling` (E2E, Chromium
  real).

---

### EXP32-F002 — Foco não é restaurado após fechar um editor aberto pelo menu de criação

| Campo | Valor |
|---|---|
| Área | Teclado/foco/acessibilidade / Daily |
| Rota/Página | `/daily` |
| Componente | `HabitEditorModal`/`TaskEditorModal`/etc. sobre `EditorModalShell` + `DialogFocusScope`, acionados pelo menu "+ Activity" |
| Severidade | MEDIUM |
| Device | Shared (teclado) |
| Accessibility Impact | Yes |
| Owning Sprint | 32.13 (Focus, Keyboard & Interaction Accessibility) — cross-ref 32.6 (Modal & Dialog Experience) |
| State | DISCOVERED |

**Interação:** abrir "+ Activity" → escolher "Habit" no menu (que fecha o menu e abre o editor) →
fechar o editor com `Escape`.

**Comportamento atual:** `document.activeElement` após o `Escape` é `<body>` — nenhum elemento da
página recebe foco.

**Problema:** `docs/ux/02-accessibility.md` §5 documenta que o ciclo de vida canônico dos diálogos
(`DialogFocusScope`) restaura o foco no elemento que abriu o diálogo ("trigger"), e degrada
silenciosamente (sem erro) quando esse trigger não existe mais no DOM. Aqui o "trigger" real do
editor é o item de menu "Habit" — que já foi desmontado no instante em que o menu fechou, antes
mesmo do editor abrir. Como não há nenhum trigger persistente registrado (ex.: o próprio botão
"+ Activity"), o usuário de teclado perde completamente sua posição de navegação e precisa recomeçar
a tabulação do topo da página a cada vez que cria uma atividade pelo menu — isso é consistente com o
comportamento documentado (degradação sem erro), mas é uma lacuna real de experiência de teclado que
a documentação já antecipava sem propor destino.

**Evidência (navegador, nesta Sprint):** `document.activeElement.outerHTML` imediatamente após
`Escape` retornou `<body>...` (a raiz do documento), confirmado duas vezes (criação de Habit e
sequência completa de criação de Task/Project).

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.13 (candidato: `DialogFocusScope` aceitar/registrar
o botão "+ Activity" como trigger persistente do menu de criação, já que ele sobrevive ao fechamento
do menu).

**Nota da Sprint 32.6 (cross-ref, sem fix):** confirmado por leitura de código que o contrato atual não
tem como o chamador comunicar "não há trigger persistente" a `DialogFocusScope` — `deactivate(id)` em
`beeday-dialog-focus.js` só sabe degradar para o escopo pai na pilha (`stack`) quando
`previouslyFocused` deixa de estar `.isConnected`; não existe um segundo parâmetro para um trigger de
fallback explícito. Isso é consistente com o candidato de correção já registrado acima e não foi
alterado nesta Sprint — a mudança de contrato pertence à 32.13, não a esta.

---

### EXP32-F003 — Anel de foco em cascata do botão até a coluna inteira do board

| Campo | Valor |
|---|---|
| Área | Listas, cards e coleções / Daily |
| Rota/Página | `/daily` |
| Componente | `DashboardColumn` + `HabitCard`/`ActivityCard` (`.habit-card__score-button`) |
| Severidade | MEDIUM |
| Device | Shared |
| Accessibility Impact | Partial (não bloqueia, mas confunde qual elemento está de fato focado) |
| Owning Sprint | 32.7 (Lists, Cards & Collection Patterns) |
| State | FIXED |

**Interação:** clicar/focar o botão "+" de um Habit card para registrar comportamento positivo.

**Comportamento atual:** ao focar o botão (`habit-card__score-button`, com `aria-label` correto,
"Register positive for Drink water"), o navegador desenha visualmente **dois** contornos de foco
sobrepostos: um ao redor do card individual do Habit e outro, mais fino, ao redor de toda a coluna
"Habits" (do cabeçalho até a base da coluna).

**Problema:** o segundo contorno (coluna inteira) não corresponde a nenhum conceito real de "coluna
focada" em nenhum outro lugar do produto — é uma cascata de estilo `:focus-within` que se propagou
do botão, para o card, para o container da coluna. Isso é redundante e pode confundir um usuário de
baixa visão sobre qual elemento específico está focado.

**Evidência (navegador, nesta Sprint):** captura de tela ampliada (`zoom`) da região do card após
focar o botão mostra os dois contornos distintos e aninhados; `document.activeElement` confirmado
como o próprio `<button class="habit-card__score-button">`, não o card nem a coluna.

**Resolution (Sprint 32.7):** removidas as duas declarações `.dashboard-column:focus-within`
(`DashboardColumn.razor.css`) responsáveis pelo contorno duplicado. Nenhum elemento interativo da
coluna dependia desse contorno para indicação de foco: os cards já têm o seu próprio
(`.habit-card:focus-within`/`.activity-card:focus-within`) e os botões do cabeçalho da coluna
(`.beeday-icon-toggle:focus-visible`) também. Verificado ao vivo em `/daily` — após a correção,
focar `.habit-card__score-button` deixa `document.activeElement` no próprio botão, o card exibe seu
contorno normal, e `getComputedStyle` da coluna confirma borda/box-shadow neutros (sem o anel de
foco secundário).

**Regression Protection:** `DashboardColumnTests.DoesNotDeclareFocusWithinOnTheColumnItself`
(`tests/BeeDay.Web.Tests/Components/Dashboard/DashboardColumnTests.cs`) — asserção de que
`.dashboard-column:focus-within` não reaparece em `DashboardColumn.razor.css`.

---

### EXP32-F004 — To-Do dentro do ProjectWorkspace não tem controle de conclusão inline

| Campo | Valor |
|---|---|
| Área | Listas, cards e coleções / Daily / Projects |
| Rota/Página | `/daily` — painel `ProjectWorkspace` |
| Componente | `ProjectWorkspace.razor` (`.project-workspace__todo`) vs. coluna "To-Dos" do board (`DashboardColumn`) |
| Severidade | HIGH |
| Device | Shared |
| Accessibility Impact | Partial (a ação existe em outro lugar da UI, mas não é descobrível a partir do workspace) |
| Owning Sprint | 32.7 (Lists, Cards & Collection Patterns) — cross-ref 32.15 (Daily Experience Polish) |
| State | FIXED |

**Interação:** dentro de um projeto aberto (`ProjectWorkspace`), tentar marcar um To-Do como
concluído a partir da própria linha do To-Do no workspace.

**Comportamento atual:** a linha do To-Do dentro do `ProjectWorkspace`
(`<article class="project-workspace__todo">`) contém apenas o título — nenhum checkbox, botão ou
controle de conclusão. A mesma entidade, quando exibida na coluna "To-Dos" do board principal do
Daily, tem um checkbox visível e funcional (confirmado: cliclar nele conclui o To-Do, atualiza o
progresso do Project para 100% e o filtra da lista de projetos ativos).

**Problema:** um usuário que abre o workspace de um projeto para trabalhar nos seus To-Dos não
consegue concluir nenhum a partir dali — precisa fechar o workspace e usar a coluna "To-Dos" do board
principal, ou abrir o editor completo do To-Do. Isso é uma inconsistência de affordance para a mesma
ação sobre a mesma entidade, dependendo de onde ela é vista.

**Evidência (navegador, nesta Sprint):** `outerHTML` do `<article class="project-workspace__todo">`
não contém nenhum `<button>`/`<input>`; captura de tela ampliada confirma apenas o título e uma barra
de destaque colorida à esquerda. Em contraste, captura de tela ampliada da coluna "To-Dos" do board
mostra um checkbox quadrado explícito ao lado do mesmo título; clicá-lo concluiu o To-Do e atualizou
o Project para "Completed 100%" (confirmado revelando-o via o toggle "Show completed projects").

**Resolution (Sprint 32.7):** `ProjectWorkspace` agora expõe um novo parâmetro
`EventCallback<TodoSummary> OnToggleTodo`, wired em `Home.razor` para o mesmo
`State.ToggleTodoAsync(todo.Id)` já usado pela coluna "To-Dos" do board. Cada linha de To-Do no
workspace renderiza (quando o parâmetro tem delegate) o mesmo botão de conclusão reutilizado da
coluna do board — classe compartilhada `activity-card__checkbox`/`activity-card__checkbox-glyph`
(`cards.css`), não uma implementação paralela — posicionado de forma absoluta dentro da linha
(`.project-workspace__todo-toggle`) porque uma regra de CSS pré-existente e incondicional mais
abaixo no mesmo arquivo (`display: block` sobre `.project-workspace__todo`) já neutralizava o
`display: grid`/`grid-template-columns` original; posicionamento absoluto contorna essa divergência
sem tocar nela, já que está fora do escopo desta Sprint. Rótulos acessíveis localizados adicionados
(`TodoCompleteAriaLabel`/`TodoMarkIncompleteAriaLabel`, pt-BR/en-US/neutro). Verificado ao vivo:
criado um Project com dois To-Dos em `/daily`, aberto o workspace, clicado o novo checkbox — o
título ganhou risco, o glyph ficou verde/preenchido, `aria-label` mudou para "Mark … as incomplete",
e o cabeçalho do workspace atualizou Status/Progress em tempo real (1/2 · 50% → depois 2/2 · 100%
quando ambos concluídos), idêntico ao comportamento já existente da coluna To-Dos do board.

**Regression Protection:** três testes novos em `ProjectWorkspaceTests`
(`WhenOnToggleTodoHasNoDelegate_RendersNoCompletionControl`,
`TodoToggle_InvokesOnToggleTodoWithTheTappedTodo`,
`TodoToggle_ExposesCompletionStateThroughItsAccessibleName`).

---

### EXP32-F005 — Inputs nativos de data/número seguem o locale do SO/navegador, não o idioma escolhido no beeday

| Campo | Valor |
|---|---|
| Área | Formulários e inputs (transversal) |
| Rota/Página | `/daily` (Project "Expected date", To-Do "Due date"), `/wallet` (campo "Amount" e "Date" do formulário de transação) |
| Componente | `BeeDayDateInput<T>` (nativo `type="date"`), `InputNumber` monetário do Wallet |
| Severidade | HIGH |
| Device | Desktop (confirmado); comportamento depende do locale do SO, não do dispositivo |
| Accessibility Impact | No (não é falha WCAG técnica), mas risco de erro de entrada de dados financeiros |
| Owning Sprint | 32.5 (Forms & Input Experience) — cross-ref 32.16 (Wallet), 32.15 (Daily/Projects) |
| State | FIXED |

**Interação:** abrir o editor "Create Project" (campo "Expected date") e o editor "Edit Transaction"
do Wallet (campo "Amount").

**Comportamento atual:** o placeholder nativo do campo de data renderiza como `dd/mm/aaaa`
(formato/idioma português), mesmo com a interface do beeday inteiramente em inglês ("Create Project",
"Title", etc.) — o navegador/SO desta máquina está configurado em `pt-BR`, e o `<input type="date">`
nativo herda esse locale independentemente da cultura selecionada dentro do beeday. O mesmo ocorre
com o valor monetário: uma transação criada digitando `4.50` (ponto decimal, exibida corretamente
como `$4.50` em toda a UI do Wallet) é **redisplayed** no campo "Amount" do formulário de edição como
`4,50` (vírgula decimal) — o `<input type="number">` nativo formata o valor gravado usando o locale
do navegador ao renderizá-lo de volta.

**Problema:** dois formatos de decimal diferentes para o mesmo valor monetário na mesma tela
(`$4.50` no resumo/lista vs. `4,50` no campo editável) podem levar a erro de leitura ou de digitação
por parte do usuário — um risco real em um campo financeiro. O mesmo padrão afeta qualquer consumidor
de `BeeDayDateInput` (Project, To-Do, Wallet).

**Evidência (navegador, nesta Sprint):** captura de tela do formulário "Create Project" mostrando
`dd/mm/aaaa`; captura de tela do formulário "Create Transaction" (valor digitado `4.50`, salvo e
exibido como `$4.50` na lista); captura de tela do mesmo registro reaberto em "Edit Transaction"
mostrando `4,50` no campo Amount.

**Resolution:** `FIXED` nesta Sprint. `BeeDayDateInput` (`<InputDate>`) e o `InputNumber` monetário do
Wallet agora recebem `lang="@CultureInfo.CurrentCulture.Name"` — Chromium usa o atributo `lang` do
próprio elemento (não o locale do SO/navegador) para formatar `<input type="date">`/
`<input type="number">` quando presente. Verificado ao vivo no navegador (pt-BR): o placeholder do
campo de data passou a exibir `dd/mm/aaaa` de forma consistente com a cultura selecionada, e uma
transação de `$4,50` reaberta em "Edit Transaction" agora mostra `4,50` no campo Amount — mesmo
formato de decimal usado pelo `WalletCurrencyFormatter` na lista, eliminando a divergência.

**Regression Protection:** `BeeDayFormTests.DateInput_LangAttributeFollowsCurrentCulture_NotTheMachineDefault`
(en-US/pt-BR) e `TransactionFormModalTests.AmountInput_LangAttributeFollowsCurrentCulture_NotTheMachineDefault`
(en-US/pt-BR) assert `lang` on the rendered `<input>` matches the pinned culture, not the machine
default.

---

### EXP32-F006 — Data da lista de transações do Wallet não usa o mesmo formato do próprio input de data

| Campo | Valor |
|---|---|
| Área | Wallet |
| Rota/Página | `/wallet` |
| Componente | Linha da lista `Transactions` vs. campo "Date" do editor de transação |
| Severidade | MEDIUM |
| Device | Desktop (confirmado) |
| Accessibility Impact | No |
| Owning Sprint | 32.16 (Wallet Experience Polish) — cross-ref 32.5 (Forms & Input Experience, EXP32-F005) |
| State | DISCOVERED |

**Interação:** criar uma transação hoje e observar sua data tanto no formulário quanto na lista.

**Comportamento atual:** o campo "Date" do formulário mostra `22/08/2026` (dia/mês/ano); a mesma
transação, na lista de "Transactions", mostra `8/22/2026` (mês/dia/ano).

**Problema:** dentro da mesma feature, para o mesmo registro, dois formatos de data diferentes
coexistem — um herdado do locale do navegador (EXP32-F005) e outro aparentemente formatado
explicitamente pelo código em `en-US`. Isso é inconsistente mesmo sem considerar o locale do
navegador.

**Evidência (navegador, nesta Sprint):** captura de tela do card "Coffee" na lista de transações
mostrando `8/22/2026`, lado a lado com a mesma transação reaberta em edição mostrando `22/08/2026`
no campo Date.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.16 (candidato: teste de componente fixando o
formato de data usado na lista e sua paridade com o formato do input, coordenado com a correção de
EXP32-F005).

---

### EXP32-F007 — Login depende de validação nativa do navegador, não do padrão localizado do Design System

| Campo | Valor |
|---|---|
| Área | Formulários e inputs / Errors, Recovery & User Feedback |
| Rota/Página | `/login` |
| Componente | `Login.razor` (`<form method="post" action="/auth/login">`, não `EditForm`) |
| Severidade | LOW |
| Device | Shared |
| Accessibility Impact | No (validação nativa do navegador é, por si, acessível) |
| Owning Sprint | 32.5 (Forms & Input Experience) |
| State | FIXED |

**Interação:** submeter o formulário de login com o campo Email vazio.

**Comportamento atual:** o navegador foca o campo Email e mostra seu balão de validação nativo (não
localizado pelo beeday, sem o estilo de `BeeDayValidationMessage`/`.identity-feedback--error` usado
no resto do produto) — confirmado em `docs/web/04-feature-components.md` §7: `Login.razor` é
formulário HTML puro (não `EditForm`) porque o POST vai direto para o endpoint minimal API, não para
um handler Blazor.

**Problema:** a mensagem de validação do navegador não segue o idioma/estilo do beeday (ela usa o
idioma do navegador/SO, não a cultura `pt-BR`/`en-US` selecionada no app), diferente de todo outro
formulário do produto, que usa `BeeDayValidationMessage` localizado.

**Evidência (navegador, nesta Sprint):** captura de tela mostrando o campo Email focado com o anel de
validação nativo do Chromium após clicar "Sign in" com o formulário vazio.

**Resolution:** `FIXED` nesta Sprint, mantendo a validação nativa (decisão explícita — ver abaixo) mas
corrigindo o problema real relatado: o idioma da mensagem. `Login.razor` não pode migrar para
`EditForm`/`BeeDayValidationMessage` sem reintroduzir a discussão arquitetural já resolvida em
`docs/web/04-feature-components.md` §7 (o POST precisa ir direto ao endpoint minimal API, não a um
handler Blazor) — isso seria redesenhar o fluxo de login, fora do escopo de "Polish, Not Redesign"
(Issue #244 §2.2) para um achado `LOW`. Em vez disso, os campos `login-email`/`login-password` agora
localizam a própria mensagem de validação nativa via `oninvalid`/`setCustomValidity(...)`, usando as
chaves já existentes `DesignSystemResources.ValidationEmailRequired`/`ValidationEmailFormat` (reuso
exato — mesmo texto já usado por todo outro formulário do produto) e a nova
`ValidationLoginPasswordRequired`. Verificado ao vivo no navegador sob cultura pt-BR:
`email.validationMessage` retorna "O e-mail é obrigatório."/"Informe um endereço de e-mail válido." e
`password.validationMessage` retorna "A senha é obrigatória." — mesmo balão nativo do Chromium,
agora no idioma selecionado do beeday em vez do idioma do navegador/SO.

**Regression Protection:** `LoginTests.EmailAndPasswordFields_HaveLocalizedNativeValidationMessages`
(en-US) e `LoginTests.UnderPortugueseUiCulture_NativeValidationMessagesAreLocalized` (pt-BR) assert
the `oninvalid` handler's localized message text for both fields.

---

### EXP32-F008 — Nenhum mecanismo de "pular para o conteúdo" dentro de uma página já carregada

| Campo | Valor |
|---|---|
| Área | Teclado/foco/acessibilidade (transversal) |
| Rota/Página | Todas |
| Componente | `Routes.razor` (`FocusOnNavigate Selector="h1"`) |
| Severidade | MEDIUM |
| Device | Shared (teclado/leitor de tela) |
| Accessibility Impact | Yes |
| Owning Sprint | 32.13 (Focus, Keyboard & Interaction Accessibility) |
| State | DISCOVERED |

**Interação:** um usuário de teclado, já em uma página carregada com navegação lateral (shell
autenticado) ou cabeçalho público, tenta pular diretamente para o conteúdo principal sem tabular por
toda a navegação.

**Comportamento atual:** `FocusOnNavigate` move o foco para o `<h1>` apenas na navegação entre
páginas (cobrindo parcialmente o problema); não existe um "skip to content"/`skip-link` equivalente
para pular painéis laterais/navegação *dentro* de uma página já carregada.

**Problema:** confirmado como achado ainda aberto em `docs/ux/02-accessibility.md` §4 (busca
repo-wide por "skip to content"/"skip-link" sem nenhuma ocorrência). Roteado para dentro da EPIC 32
porque é exatamente o tipo de defeito de interação que o Interaction Quality Contract da Issue #244
§6 e o Accessibility Contract §9 exigem cobrir.

**Evidência:** achado de código/documentação já verificado — `docs/ux/02-accessibility.md` §4,
verificação original por busca direta em `src/BeeDay.Web/`.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.13.

---

### EXP32-F009 — `text-muted` sobre superfície branca abaixo do limiar AA para texto normal

| Campo | Valor |
|---|---|
| Área | Teclado/foco/acessibilidade (transversal — contraste de cor) |
| Rota/Página | Onde `--beeday-color-text-muted` for usado em texto normal sobre `--beeday-color-surface` |
| Componente | Token `--beeday-color-text-muted` (`#817789`) |
| Severidade | MEDIUM |
| Device | Shared |
| Accessibility Impact | Yes |
| Owning Sprint | 32.13 (Focus, Keyboard & Interaction Accessibility) |
| State | DISCOVERED |

**Interação:** leitura de texto auxiliar/meta-informação/estado vazio que usa o token `text-muted`.

**Comportamento atual:** `DesignSystemContrastTests` calcula `text-muted` (`#817789`) sobre
`--beeday-color-surface` (`#fff`) em **≈4.26:1** — abaixo do limiar AA de 4.5:1 para texto normal
(passa apenas o limiar de 3:1 para texto grande/UI).

**Problema:** confirmado já aberto em `docs/ux/02-accessibility.md` §9: a varredura axe já migrou os
consumers pequenos identificados (EmptyState, Footer, Login, Wallet, editor modal) para
`text-secondary`, mas o token `text-muted` em si permanece abaixo do limiar e "outros usos precisam
ser avaliados no contexto renderizado" — ou seja, o inventário completo de onde `text-muted` ainda é
usado em texto normal (não grande/UI) não está fechado.

**Evidência:** achado de código/documentação já verificado —
`docs/ux/02-accessibility.md` §9, `DesignSystemContrastTests`.

**Evidência adicional (navegador, Sprint 32.3):** varredura axe ao vivo contra `/profile` (rota nunca
antes varrida por nenhum teste automatizado — `AccessibilityQualityTests` cobre `/daily` e `/wallet`,
não `/profile`) confirmou `color-contrast (serious)` em múltiplos elementos que usam `text-muted`
nesta página: a descrição do cabeçalho ("Choose one next step and keep your day moving."), o label
"Level" e o valor "X of 100 experience points" do `ExperienceBar`, e a descrição do card "Weekly
history is not available yet". Confirmado pré-existente e não causado pela Sprint 32.3: o parágrafo
do cabeçalho usava o mesmo token `text-muted` antes da refatoração para `BeeDayPageHeader` (via
`.product-home__header p`) e continua usando o mesmo token depois (via `.beeday-page-header p`
compartilhado) — nenhuma mudança de cor foi introduzida, apenas a página passou a ser varrida pela
primeira vez.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.13 (candidato: inventário completo de usos
restantes de `text-muted` em texto normal e migração ou promoção documentada do token; incluir
`/profile` no inventário e, se prático, na cobertura axe existente).

---

### EXP32-F010 — Cobertura incompleta de fallback local para `prefers-reduced-motion`

| Campo | Valor |
|---|---|
| Área | Teclado/foco/acessibilidade (transversal — movimento) |
| Rota/Página | Auth/ProfileCreation, Habit, ProjectWorkspace, DashboardColumn, activity cards |
| Componente | 8 dos 31 stylesheets com `animation`/`transition`/`@keyframes` |
| Severidade | LOW |
| Device | Shared |
| Accessibility Impact | Yes |
| Owning Sprint | 32.13 (Focus, Keyboard & Interaction Accessibility) — cross-ref 32.15 (Daily), 32.5 (Auth/ProfileCreation) |
| State | DISCOVERED |

**Interação:** usuário com `prefers-reduced-motion: reduce` habilitado no sistema operacional
interagindo com Habit cards, o workspace de projeto, colunas do Dashboard ou telas de
Auth/ProfileCreation.

**Comportamento atual:** o safety net global (`animations.css`, `*, *::before, *::after`) reduz a
duração de toda animação para `.01ms`, mas 8 dos 31 stylesheets de produção com `animation`/
`transition`/`@keyframes` não têm fallback local — o que significa que delay/opacity/transform ainda
podem ocultar feedback nesses 8 casos mesmo com o safety net ativo.

**Problema:** confirmado já aberto em `docs/ux/02-accessibility.md` §6 (inventário direto da Sprint
25.6: 23/31 com fallback após a Sprint; os 8 restantes — Auth/ProfileCreation, Habit,
ProjectWorkspace, DashboardColumn, motion interno dos activity cards — ficaram atribuídos aos seus
owners de convergência, ainda sem Sprint que os feche).

**Evidência:** achado de código/documentação já verificado — `docs/ux/02-accessibility.md` §6.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.13 (candidato: fallback local nos 8 stylesheets
restantes, com teste de regressão por stylesheet).

---

### EXP32-F011 — Quatro implementações CSS independentes do mesmo padrão visual de campo de formulário

| Campo | Valor |
|---|---|
| Área | Formulários e inputs (transversal) |
| Rota/Página | Editor de atividade, Identity, Wallet |
| Componente | `.beeday-field__control`, `.editor-modal__field input`, `.identity-field input`, `.wallet-filters input` |
| Severidade | MEDIUM |
| Device | Shared |
| Accessibility Impact | No |
| Owning Sprint | 32.19 (Front-End Code Refinement & Component Consolidation) — cross-ref 32.5 (Forms & Input Experience) |
| State | DISCOVERED |

**Interação:** N/A (achado estrutural de CSS, não uma interação isolada — afeta a consistência visual
percebida entre formulários de áreas diferentes).

**Comportamento atual:** quatro seletores CSS distintos implementam separadamente a mesma aparência
visual de campo de input, em vez de compartilhar uma única declaração.

**Problema:** confirmado já aberto em `docs/ux/01-guidelines.md` §4 ("Não existe (achados, não
corrigidos)"). Risco de drift visual entre áreas quando um dos quatro for ajustado sem replicar a
mudança nos outros três.

**Evidência:** achado de código/documentação já verificado — `docs/ux/01-guidelines.md` §4,
referenciando `docs/design-system/04-forms.md` §5.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.19 (candidato: consolidar em um único seletor/
classe compartilhada, com teste de regressão visual ou snapshot de CSS).

---

### EXP32-F012 — Cooldown de reenvio de confirmação duplicado em vez de compartilhado

| Campo | Valor |
|---|---|
| Área | Formulários e inputs / Identity (transversal) |
| Rota/Página | `/account/resend-confirmation`, `/account/email-confirmation-sent` |
| Componente | `ResendConfirmation.razor`, `EmailConfirmationSent.razor` (cada um com seu próprio `PeriodicTimer` de 60s) |
| Severidade | LOW |
| Device | Shared |
| Accessibility Impact | No |
| Owning Sprint | 32.19 (Front-End Code Refinement & Component Consolidation) — cross-ref 32.5 (Forms & Input Experience) |
| State | DISCOVERED |

**Interação:** N/A (achado estrutural de código, não uma interação isolada).

**Comportamento atual:** as duas páginas implementam de forma independente o mesmo cooldown de 60
segundos via `PeriodicTimer`, sem compartilhar um componente/serviço comum.

**Problema:** confirmado já aberto em `docs/web/04-feature-components.md` §7, explicitamente marcado
como "candidato a duplicação, fora do escopo de apresentação" da Sprint que o descreveu — cabe à EPIC
32 (que possui explicitamente uma Sprint de consolidação de front-end, 32.19) decidir se consolida.

**Evidência:** achado de código/documentação já verificado — `docs/web/04-feature-components.md` §7.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.19 (candidato: extrair um componente/hook
`CooldownTimer` compartilhado, com teste cobrindo os dois consumidores).

---

### EXP32-F013 — Busca do Daily não distingue "sem resultados" de "nunca teve itens"

| Campo | Valor |
|---|---|
| Área | Busca, filtros e ordenação / Estados vazios |
| Rota/Página | `/daily` |
| Componente | `DashboardColumn` (`BeeDayEmptyState`/`EmptyLabel` por coluna) + busca (`filter-bar__input`) |
| Severidade | MEDIUM |
| Device | Shared |
| Accessibility Impact | Partial (a mesma mensagem de "vazio" é lida pelo leitor de tela em dois contextos semanticamente diferentes) |
| Owning Sprint | 32.8 (Search, Filters & Sorting) — cross-ref 32.10 (Empty States & First-Use Experience) |
| State | **FIXED** — Sprint 32.8 |

**Interação:** digitar um termo de busca no campo de busca do Daily que corresponda a itens em apenas
uma das 4 colunas.

**Comportamento atual:** buscando por `water` (correspondendo apenas ao Habit "Drink water"), as
colunas Tasks/To-Dos/Projects (que tinham itens antes da busca, agora filtrados a zero) mostram
exatamente o mesmo texto de estado vazio ("No tasks yet" / "Create a recurring task to organize work
that repeats over time.") que mostrariam se o usuário nunca tivesse criado nenhum item nelas.

**Problema:** não há nenhuma indicação textual de que uma busca está ativa e filtrando a lista para
zero resultados — o usuário pode concluir erroneamente que não tem nenhuma Task/To-Do/Project
cadastrada, quando na verdade tem, apenas não correspondem ao termo buscado. `docs/ux/01-guidelines.md`
§7 confirma que cada coluna gera seu próprio texto de estado vazio a partir de um único `EmptyLabel`
por coluna, sem um segundo texto para o caso "zero resultados de busca/filtro".

**Evidência (navegador, nesta Sprint):** captura de tela com a busca "water" ativa (campo com "×" de
limpar, contagem "1" em Habits) mostrando as colunas Tasks ("0"), To-Dos ("0") e Projects ("0") com o
texto de estado vazio idêntico ao observado antes de qualquer item existir.

**Resolution (Sprint 32.8):** `DashboardState` ganhou quatro propriedades computadas
(`HabitsFilteredToZero`, `ActiveTasksFilteredToZero`, `ActiveTodosFilteredToZero`,
`ActiveProjectsFilteredToZero`) que distinguem "a coleção ativa tem itens, mas o
filtro/busca atual não corresponde a nenhum" de "a coleção nunca teve nenhum item ativo" —
comparando a contagem pós-filtro (`FilteredHabits`/`FilteredTasks`/etc.) contra a contagem
não-filtrada equivalente (`data.Habits`/`data.Tasks`/etc.). `ActiveTodosFilteredToZero` cobre tanto
a busca textual quanto o `ProjectContextFilter` (`selectedProjectId`), já que ambos narrowam a
mesma coluna To-Dos. `Home.razor` consome essas propriedades para escolher entre o par
`EmptyTitle`/`EmptyDescription` original da coluna e o novo par compartilhado
`NoFilterResultsTitle`/`NoFilterResultsDescription` (localizado en-US/pt-BR/neutro) — nenhum novo
parâmetro foi adicionado a `DashboardColumn`, que continua agnóstico a filtro/busca.

**Regression Protection:** `DashboardStateTests` — `HabitsFilteredToZero_IsTrueOnlyWhenSearchHidesAnExistingHabit`,
`HabitsFilteredToZero_IsFalseWhenThereWasNeverAnyHabitAtAll`,
`ActiveTasksFilteredToZero_IsTrueOnlyWhenSearchHidesAnExistingActiveTask`,
`ActiveTodosFilteredToZero_IsTrueWhenTheProjectContextFilterHidesEveryActiveTodo`,
`ActiveProjectsFilteredToZero_IsTrueOnlyWhenSearchHidesAnExistingActiveProject` — todos
database-free, exercitando `DashboardState` diretamente via um `ISender` fake.

---

### EXP32-F014 — Logout falha com 400 para qualquer usuário autenticado que caia em uma rota quebrada

| Campo | Valor |
|---|---|
| Área | Shell de aplicação e navegação / Redirects / Dead ends |
| Rota/Página | Qualquer rota inexistente (ex.: `/route-that-does-not-exist`) enquanto autenticado |
| Componente | `Program.cs` (`UseStatusCodePagesWithReExecute`), `NotFound.razor`, `NavigationItems.razor` (formulário de Logout, `<AntiforgeryToken />`) |
| Severidade | HIGH |
| Device | Shared |
| Accessibility Impact | Yes (usuário fica sem rota de recuperação clara de teclado/leitor de tela: uma página JSON crua, fora do chrome do produto) |
| Owning Sprint | 32.2 (Application Shell & Navigation Fluidity) |
| State | **FIXED** — Sprint 32.2 |

**Interação:** estar autenticado, navegar para uma URL que não corresponde a nenhuma rota (link
quebrado, favorito obsoleto, erro de digitação), depois clicar em "Logout" na barra lateral
renderizada sobre o `NotFound` resultante.

**Comportamento anterior:** `POST /auth/logout` retornava HTTP 400 com um `ProblemDetails` JSON cru
("Unexpected request without body, failed to bind parameter \"string returnUrl\"..."), fora do chrome
visual do produto, sem forma de recuperação a não ser o botão Voltar do navegador.

**Causa raiz confirmada:** `Program.cs` usava `UseStatusCodePagesWithReExecute("/not-found")`, que
mantém a URL quebrada original na barra de endereço enquanto o servidor renderiza o markup de
`/not-found` para aquela resposta (confirmado com `curl`: o HTML bruto contém o
`__RequestVerificationToken` corretamente). Assim que o circuito interativo conecta, o Router
client-side do Blazor resolve **de novo, independentemente**, essa mesma URL (ainda sem
correspondência) e invoca seu próprio fallback `NotFoundPage` — desta vez sem nenhum `HttpContext`
para obter um token de antiforgery, então `<AntiforgeryToken />` renderiza vazio e o Router
silenciosamente sobrescreve o formulário de Logout corretamente pré-renderizado por um sem token.
Confirmado via `document.querySelectorAll('form[action="/auth/logout"] input')` retornando `[]` após
o circuito conectar, apesar do HTML inicial (via `curl`) conter o campo.

**Resolution:** `Program.cs` agora usa `app.UseStatusCodePages(...)`, redirecionando explicitamente
**somente** quando `Response.StatusCode == 404` para `/not-found` — nunca reexecutando. Um redirect
real muda a URL da barra de endereço para `/not-found` antes do circuito conectar, então tanto o
render do servidor quanto o Router client-side resolvem a mesma rota real via o caminho normal
`RouteView`, sem fallback duplicado. Uma tentativa inicial de usar o método de conveniência
`UseStatusCodePagesWithRedirects("/not-found")` (que reage a toda a faixa 400–599 com corpo vazio, não
só 404) quebrou `CultureCookieIntegrationTests.SetCulture_WithUnsupportedCulture_IsRejected` (400
genuíno virou 302) — corrigido restringindo o redirect explicitamente a 404 via a sobrecarga baseada
em delegate. `NotFound.razor` também recebeu `<PageTitle>`, `@rendermode InteractiveServer` (explícito,
consistente com Profile/Daily/Wallet/Account) e `<h3>` → `<h1>` (a página não tinha heading nível 1,
quebrando o próprio contrato de `FocusOnNavigate Selector="h1"` do Router).

**Regression Protection:**
- `NavigationTests.NonexistentRoute_WhileAuthenticated_LogoutStillWorks` (E2E, Chromium real) —
  reproduz o cenário exato (rota inexistente → circuito conecta → clicar Logout) e prova o fim a fim,
  já que o bug só se manifesta com um circuito interativo real, irreproduzível via
  `WebApplicationFactory`.
- `NotFoundRedirectIntegrationTests` (2 testes) — guarda determinística de que a rota inexistente
  redireciona (302) para `/not-found`, e que essa rota carrega um token de antiforgery válido quando
  autenticado.
- `CultureCookieIntegrationTests.SetCulture_WithUnsupportedCulture_IsRejected` (pré-existente,
  revalidado) — prova que a correção não regrediu respostas 4xx genuínas de outros endpoints.
- `NotFoundTests` (bUnit, pré-existente) atualizado para `h1`.

---

### EXP32-F015 — Item de navegação "Account" nunca mostra current-route no alias `/account`

| Campo | Valor |
|---|---|
| Área | Shell de aplicação e navegação / Active-route state |
| Rota/Página | `/account` (alias de compatibilidade de `/settings`, `docs/web/02-routing-and-pages.md` §3) |
| Componente | `NavigationItem.razor(.cs)`, `NavigationItems.razor` |
| Severidade | MEDIUM |
| Device | Shared |
| Accessibility Impact | Yes (`aria-current="page"` nunca aparece nesse alias, então um leitor de tela não confirma em qual item da navegação o usuário está) |
| Owning Sprint | 32.2 (Application Shell & Navigation Fluidity) |
| State | **FIXED** — Sprint 32.2 |

**Interação:** navegar para `/account` (em vez de `/settings`) e observar o item "Account" da barra
lateral.

**Comportamento anterior:** nem a classe CSS `is-active` nem `aria-current="page"` eram aplicados —
`NavigationItem.IsCurrentRouteActive()` (e o próprio `NavLink` interno do Blazor) só compara a rota
atual contra o único `Href` recebido (`/settings`); `/account` não começa com `/settings`, então a
comparação por prefixo falha. Confirmado ao vivo: `document.querySelectorAll('a[href="/settings"]')`
em `/account` retornava `isActive: false, ariaCurrent: null` nos dois (desktop e mobile).

**Resolution:** novo parâmetro opcional `AlternateHref` em `NavigationItem` — o chamador (que conhece
o alias, não o componente genérico) passa `AlternateHref="/account"` no item Account de
`NavigationItems.razor`. `IsCurrentRouteActive()` agora verifica `Href` OU `AlternateHref`; a classe
`is-active` do `<NavLink>` (que só conhece `Href`) deixou de ser a única fonte da classe visual — o
markup agora também aplica a classe manualmente a partir do mesmo método que já decide
`aria-current`, então os dois nunca mais podem divergir entre si.

**Regression Protection:**
- `NavigationItemTests.RouteMode_AlternateHrefAlsoCountsAsCurrent` /
  `RouteMode_AlternateHrefDoesNotMatchUnrelatedRoutes` (bUnit).
- `NavigationTests.Desktop_VisitingTheAccountAliasStillMarksAccountAsCurrent` (E2E, Chromium real).

### EXP32-F016 — `/profile` reimplementava à mão o padrão eyebrow-título-descrição-ações em vez de reusar `BeeDayPageHeader`

| Campo | Valor |
|---|---|
| Área | Layouts de página / headings / header-action rows |
| Rota/Página | `/profile` |
| Componente | `DashboardHome.razor` (`.product-home__header`) vs. `BeeDayPageHeader` |
| Severidade | MEDIUM |
| Device | Shared |
| Accessibility Impact | Partial (ver evidência abaixo — `aria-labelledby` apontava para um `id` que só rotulava o título, não o cabeçalho inteiro; corrigido para `aria-label` equivalente) |
| Owning Sprint | 32.3 (Page Layout Consistency) |
| State | **FIXED** — Sprint 32.3 |

**Interação:** N/A (achado estrutural de layout/CSS, não uma interação isolada).

**Comportamento anterior:** `DashboardHome.razor` (`/profile`) renderizava um `<header class="product-home__header">` com um `<span>` de data como pseudo-eyebrow, `<h1>` e `<p>` de descrição, e o botão "Open Daily" — reimplementando à mão exatamente o shape "eyebrow → título → descrição → ações" que `docs/design-system/02-components.md` §5 e `docs/ux/01-guidelines.md` §2 já documentam como o contrato compartilhado de `BeeDayPageHeader`/`BeeDaySectionHeader`/`BeeDayHero`. `/settings` (`Account.razor`) já consumia `BeeDayPageHeader` para o mesmo shape; `/profile` era a única página do arquétipo "página de conteúdo autenticada" que não reusava nenhum dos três primitives — uma duplicação de CSS/markup feature-local (`.product-home__header`, `.product-home__header h1`, `.product-home__date`) que o Required Work #6 da Issue #247 pede para evitar.

**Evidência (navegador, nesta Sprint):** captura de tela em 1440px confirmou que o `<h1>` de `/profile` renderizava em `clamp(1.75rem, 4vw, 2.35rem)` (medido: 37.6px) enquanto o `<h1>` de `/settings`, usando `BeeDayPageHeader`, renderiza em `clamp(2rem, 5vw, 3.2rem)` (medido: 51.2px) — a mesma família de conteúdo (página autenticada de card/seção, não o board operacional do Daily nem o hero financeiro do Wallet) com duas escalas tipográficas de título diferentes, sem nenhuma justificativa documentada para a diferença.

**Resolution:** `DashboardHome.razor` agora usa `<BeeDayPageHeader Eyebrow="@DateTime.Today...\" Title="..." Description="...">` com o botão "Open Daily" no slot `Actions`, eliminando o `<header>` bespoke. O `aria-labelledby="home-heading"` da `<section>` externa (que dependia de um `id` no `<h1>`, inacessível através do componente compartilhado) foi substituído por `aria-label` com o mesmo texto do título — preserva o nome acessível da seção sem depender de um `id` que `BeeDayPageHeader` não expõe no `<h1>`. CSS feature-local morta (`.product-home__header`, `.product-home__header h1`, `.product-home__date`, e os overrides de mobile equivalentes no media query de 42rem) foi removida de `DashboardHome.razor.css`; o colapso para coluna em mobile e a largura total do botão de ação em telas estreitas agora vêm inteiramente do contrato compartilhado (`design-system.css`/`polish.css`), já testado por `Account`/`ExperienceSystem`.

**Regression Protection:**
- `AuthenticatedHomeContractTests` (bUnit, já existente) — continua verde sem alteração: ainda confirma a rota, o estado compartilhado, os primitives oficiais (`BeeDayCard`/`BeeDayButton`/`BeeDayIcon`/`BeeDayProgressBar`/`ExperienceBar`) e exatamente uma ocorrência do CTA "Open Daily".
- `AuthenticatedHomeTests.MobileAndTabletPrioritizeEssentialProgressWithoutLegacyRegionsOrOverflow` (E2E, Chromium real, 7 larguras reais incluindo 390px/430px móvel) — confirma heading acessível visível, ausência de overflow horizontal e navegação mobile funcionando após a troca de componente.
- `AuthenticatedHomeTests.LoginEntersProfileWithDesktopShellAndHonestWeeklyState` e `OnlyOneOpenDailyControlRemainsOnProfile` (E2E) — confirmam o heading "Welcome back" e o único CTA "Open Daily" continuam expostos com o papel/nome corretos.
- `Epic21ConsolidationTests.FinalExperienceMatrixHasNoDocumentOverflow` (E2E, 7 larguras) e `ShellResponsiveLayoutTests` — revalidados sem alteração de comportamento.

---

### EXP32-F017 — `wallet.css` tinha duas declarações conflitantes de `.wallet-page`, com a centralização dependente de cascata implícita

| Campo | Valor |
|---|---|
| Área | Layouts de página / max-widths e gutters |
| Rota/Página | `/wallet` |
| Componente | `wwwroot/css/wallet.css` |
| Severidade | LOW |
| Device | Shared |
| Accessibility Impact | No |
| Owning Sprint | 32.3 (Page Layout Consistency) |
| State | **FIXED** — Sprint 32.3 |

**Interação:** N/A (achado estrutural de CSS).

**Comportamento anterior:** `wallet.css` continha duas regras `.wallet-page` separadas: uma legada, minificada, no topo do arquivo (`width:min(1440px,100%);margin:0 auto;padding:2rem 1.5rem 3rem`, sem comentário, misturada com outras regras minificadas antigas) e outra, formatada e comentada ("Wallet visual alignment with My Account"), mais abaixo (`width: min(76rem, 100%); padding: 2rem clamp(1rem, 3vw, 2.5rem) 4rem;`). Pela ordem de cascata, a segunda regra sobrescrevia `width`/`padding` da primeira, mas **não declarava `margin`** — a centralização (`margin: 0 auto`) da página inteira dependia silenciosamente da regra legada, que parecia morta (largura/padding totalmente sobrescritos) mas na verdade ainda era a única fonte da centralização.

**Problema:** duas fontes de verdade para o mesmo seletor, uma delas parecendo mais morta do que estava — exatamente o tipo de "CSS feature-local quando uma fundação já existe" que o Required Work #6 da Issue #247 pede para evitar, e um risco real para qualquer edição futura que removesse a regra legada (aparentemente segura de remover) sem perceber que ela ainda fornecia `margin: 0 auto`.

**Evidência (navegador, nesta Sprint):** captura de tela de `/wallet` em 1440px antes e depois da consolidação é pixel-idêntica (mesma posição/largura do container, `getBoundingClientRect` idêntico); `WalletTests.Hero_IsRoughlyHalfTheHeightOfTheBrandGuidelinesHeroAndWorksWithoutAnIllustration` (E2E) permanece verde sem alteração, confirmando que a consolidação não mudou nenhum pixel renderizado.

**Resolution:** consolidada em uma única regra `.wallet-page` (mantendo `width: min(76rem, 100%)`, `padding: 2rem clamp(1rem, 3vw, 2.5rem) 4rem`, cor de texto), agora declarando `margin: 0 auto` explicitamente. A regra legada de 1440px foi removida.

**Regression Protection:** `WalletTests` (E2E, suíte completa) e `AccessibilityQualityTests.DailyWalletAndCanonicalDialog_HaveNoAutomaticallyDetectableViolations` revalidados sem alteração de comportamento.

---

### EXP32-F018 — Daily (`/daily`) não tem cabeçalho de página visível

| Campo | Valor |
|---|---|
| Área | Layouts de página / headings |
| Rota/Página | `/daily` |
| Componente | `Home.razor` (`<h1 class="beeday-visually-hidden">`) |
| Severidade | N/A (exceção documentada, não um defeito) |
| Device | Shared |
| Accessibility Impact | No (o `<h1>` existe e é alcançável por leitor de tela; apenas não é visível) |
| Owning Sprint | 32.3 (Page Layout Consistency) |
| State | **ACCEPTED** — Sprint 32.3 |

**Interação:** N/A.

**Comportamento atual:** ao contrário de `/profile`, `/settings` (`BeeDayPageHeader`) e `/wallet` (`BeeDayHero`), o Daily não renderiza nenhum cabeçalho visível — apenas um `<h1 class="beeday-visually-hidden">` para leitores de tela, com a `ActivityFilterBar` (busca + botão "+ Activity") liderando visualmente a hierarquia da página.

**Por que isso não é um achado a corrigir:** Daily é o board operacional denso do produto (4 colunas, scroll interno, densidade de informação alta) — um arquétipo de página distinto de "página de conteúdo" (Profile/Account) ou "hero de produto" (Wallet). Adicionar um cabeçalho visível redistribuiria espaço vertical de um board já otimizado para densidade, o que seria redesenho de fluxo de produto (`docs/epics/32-product-experience-polish/README.md` §2.2 proíbe isso) e não polimento de consistência de layout. A ausência de rationale documentado para essa exceção — ao contrário de `Home.razor` (marketing, já documentado em `docs/ux/01-guidelines.md` §4) — era a lacuna real; corrigida nesta Sprint apenas documentando a exceção (`docs/ux/01-guidelines.md` §2), sem alterar comportamento.

**Evidência (navegador, nesta Sprint):** captura de tela de `/daily` em 1440px confirma o board começando no topo absoluto da workspace, sem nenhum título/eyebrow visível, com a barra de busca/criação como primeira linha visual.

**Resolution:** `docs/ux/01-guidelines.md` §2 atualizado para documentar explicitamente a exceção do Daily (ver também correção da referência incorreta a `BeeDayPageHeader` para Wallet, que na verdade usa `BeeDayHero` — texto anterior estava desatualizado).

**Regression Protection:** N/A — nenhuma mudança de comportamento; documentação apenas.

---

### EXP32-F019 — Larguras de container diferem entre Profile (64rem) e Account/Wallet (76rem)

| Campo | Valor |
|---|---|
| Área | Layouts de página / max-widths |
| Rota/Página | `/profile` vs. `/settings`, `/wallet` |
| Componente | `.product-home` (`DashboardHome.razor.css`) vs. `.account-page` (`Account.razor.css`) e `.wallet-page` (`wallet.css`) |
| Severidade | N/A (diferença intencional, guiada por conteúdo) |
| Device | Desktop (onde a diferença de largura máxima é perceptível) |
| Accessibility Impact | No |
| Owning Sprint | 32.3 (Page Layout Consistency) |
| State | **ACCEPTED** — Sprint 32.3 |

**Interação:** N/A.

**Comportamento atual:** `.product-home` usa `max-width: 64rem`; `.account-page` e `.wallet-page` usam `width: min(76rem, 100%)`.

**Por que isso não é um achado a corrigir:** `settings.css` confirma que os formulários de `/settings` usam `grid-template-columns: repeat(2, minmax(0, 1fr))` (campos Name/Email lado a lado) e `wallet.css` confirma que `/wallet` usa um grid de duas colunas (`minmax(0,1fr) 330px`, painel de transações + painel de tags) — ambos precisam de mais largura horizontal para conteúdo de duas colunas. `/profile` é uma pilha de cards de coluna única (progresso, atividade semanal, projeto em andamento); 64rem é suficiente para esse conteúdo sem linhas de texto excessivamente longas. A diferença é, portanto, guiada por conteúdo, não uma inconsistência não explicada — forçar os três para a mesma largura arriscaria linhas de card desnecessariamente longas em `/profile` sem nenhum benefício de consistência real.

**Evidência:** `src/BeeDay.Web/wwwroot/css/settings.css` linha 36 (`grid-template-columns: repeat(2, minmax(0, 1fr))`); `src/BeeDay.Web/wwwroot/css/wallet.css` (`.wallet-workspace{display:grid;grid-template-columns:minmax(0,1fr) 330px...}`); `DashboardHome.razor` confirmado como pilha vertical de `BeeDayCard` de coluna única.

**Resolution:** nenhuma mudança de código. Documentado aqui para que Sprints futuras (32.15 Daily, 32.16 Wallet, 32.17 Settings) não reabram esta diferença como achado não explicado.

**Regression Protection:** N/A — nenhuma mudança de comportamento.

---

### EXP32-F020 — `role="status"` em `<article>` do card "Weekly activity unavailable" viola aria-allowed-role

| Campo | Valor |
|---|---|
| Área | Teclado/foco/acessibilidade (transversal — ARIA) |
| Rota/Página | `/profile` |
| Componente | `BeeDayCard` consumido por `DashboardHome.razor` (`.product-home__weekly-unavailable`, `.product-home__status`) |
| Severidade | LOW |
| Device | Shared |
| Accessibility Impact | Yes |
| Owning Sprint | 32.13 (Focus, Keyboard & Interaction Accessibility) |
| State | DISCOVERED |

**Interação:** leitor de tela navegando pelo card "Weekly activity" quando o histórico semanal está indisponível (e, pelo mesmo padrão, o card de status de "Profile unavailable").

**Comportamento atual:** `DashboardHome.razor` passa `role="status"` para `BeeDayCard`, que renderiza um `<article>` (`<article class="beeday-card beeday-card--padded product-home__weekly-unavailable" role="status">`). Axe (varredura ao vivo desta Sprint, primeira vez que `/profile` foi varrida — `AccessibilityQualityTests` não cobria esta rota antes) reporta `aria-allowed-role (minor)`: `role="status"` não é um papel ARIA permitido para o elemento `<article>` pela especificação ARIA-in-HTML.

**Problema:** o `role="status"` é semanticamente correto para anunciar a região como uma live region de status a leitores de tela, mas o elemento hospedeiro (`<article>`, que já carrega semântica própria de "conteúdo autocontido") não é um alvo permitido para esse papel — um `<div>` seria o hospedeiro correto. Não causado nem tocado pela Sprint 32.3; descoberto porque esta foi a primeira varredura automatizada de `/profile`.

**Evidência (navegador, nesta Sprint):** resultado do axe-core contra `/profile` (Chromium real, `Deque.AxeCore.Playwright`) listando a violação `aria-allowed-role` para o node exato acima.

**Resolution:** não aplicável — `DISCOVERED`.

**Regression Protection:** a definir pela Sprint 32.13 (candidato: `BeeDayCard` oferecer uma variante que renderiza `<div>` em vez de `<article>` quando `role="status"`/`role="alert"` é passado, ou os consumidores pararem de usar `role="status"` sobre `<article>`; adicionar `/profile` à cobertura axe de `AccessibilityQualityTests` como parte da correção).

---

### EXP32-F021 — Falha de validação em `EditorModalShell` não move o foco para o campo inválido

| Campo | Valor |
|---|---|
| Área | Formulários e inputs (transversal — todo consumidor de `EditorModalShell`) |
| Rota/Página | Qualquer editor: Habit, Task, To-Do, Project, Transaction, Tag |
| Componente | `EditorModalShell` (`<EditForm>` só tratava `OnValidSubmit`) |
| Severidade | MEDIUM |
| Device | Shared (teclado/leitor de tela) |
| Accessibility Impact | Yes |
| Owning Sprint | 32.5 (Forms & Input Experience) |
| State | FIXED |

**Interação:** abrir "Create Habit", deixar "Título" vazio e clicar em "Criar".

**Comportamento atual (antes da correção):** o `BeeDayValidationMessage` inline aparecia corretamente
abaixo do campo, mas o foco do teclado permanecia onde já estava — no botão de submit — em vez de ir
para o campo `#habit-title` agora inválido. Confirmado via `document.activeElement` no navegador
(retornava o `BUTTON`, não o `INPUT`).

**Problema:** um usuário de teclado/leitor de tela que submete um formulário inválido não é levado ao
campo que precisa corrigir — precisa navegar manualmente até encontrá-lo. Isso viola o "predictable
recovery" exigido pelo Interaction Quality Contract da Issue #244 §6, e afeta sistemicamente todo
formulário do produto que usa `EditorModalShell` (Habit, Task, To-Do, Project, Transaction, Tag).

**Evidência (navegador, nesta Sprint):** `document.activeElement.tagName`/`.id` confirmados via
DevTools antes e depois da correção, em `/daily` → "Create Habit" com "Título" vazio.

**Resolution:** `FIXED` nesta Sprint. `EditorModalShell`'s `<EditForm>` agora trata `OnInvalidSubmit`
além de `OnValidSubmit`: após um `Task.Yield()` (deixa os re-renders que `EditContext.Validate()` já
disparou para os campos agora inválidos chegarem ao cliente primeiro), reutiliza o módulo JS que
`DialogFocusScope` já carrega para este mesmo diálogo (`beeday-dialog-focus.js`, bumped to
`?v=20260822-1`) e chama a nova função exportada `focusFirstInvalid(dialogId)`, que foca o primeiro
elemento `.invalid`/`[aria-invalid="true"]` dentro do diálogo — `.invalid` é a classe que os
componentes `Input*` do Blazor já aplicam automaticamente ao campo malsucedido, então nenhum
consumidor de `EditorModalShell` precisou de qualquer mudança. Verificado ao vivo no navegador:
`document.activeElement.id` passou a retornar `"habit-title"` após o submit inválido; submit válido
subsequente não regrediu (foco/toast normais).

**Regression Protection:** `WalletTests.InvalidSubmit_MovesFocusToFirstInvalidField` (E2E, prova o
efeito real de foco no DOM — Notes é focado antes do submit para que a asserção não possa ser
confundida com o `autofocus` inicial de Description); `EditorModalShellTests.
InvalidSubmit_NeverInvokesOnSubmit_AndDoesNotThrow`/`ValidSubmit_StillInvokesOnSubmit` (bUnit, prova
o contrato ao redor — um modelo inválido nunca chega a `OnSubmit`, e o novo interop não lança sem um
runtime JS real).

---

### EXP32-F022 — `BeeDayConfirmDialog` sem affordance de scroll em nenhuma largura de viewport

| Campo | Valor |
|---|---|
| Área | Modais e diálogos (transversal — todo consumidor de `BeeDayConfirmDialog`) |
| Rota/Página | Qualquer fluxo de exclusão com `Warning`/`WarningDetails` (ex.: `/daily` → excluir Habit) |
| Componente | `BeeDayConfirmDialog` / `.delete-confirmation` (`feedback.css`) |
| Severidade | MEDIUM |
| Device | Shared, mais provável em viewport curto (mobile/landscape) |
| Accessibility Impact | Yes (ações inalcançáveis por teclado/mouse) |
| Owning Sprint | 32.6 (Modal & Dialog Experience) |
| State | FIXED |

**Interação:** abrir "Edit Habit" de um Habit existente, clicar "Delete" (o único fluxo de exclusão do
produto que popula `Warning` + `WarningDetails`, via `HabitEditorModal.razor`), em um viewport baixo.

**Comportamento atual (antes da correção):** `.delete-confirmation` tinha `overflow: hidden` e nenhum
`max-height`, em nenhum breakpoint — apenas a largura era ajustada em `@media (max-width: 520px)`. Um
diálogo cujo conteúdo (ícone + título + mensagem + item + aviso + ações) excedesse a altura do
viewport era cortado sem nenhuma forma de rolar até os botões "Cancel"/"Delete".

**Problema:** viola o contrato de "mobile height/scroll behavior... predictable" desta Sprint (Issue
#250) e o Interaction Quality Contract da Issue #244 §6 — uma confirmação destrutiva cujas ações não
podem ser alcançadas é pior do que apenas inconsistente, é uma ação bloqueada.

**Evidência (navegador, nesta Sprint):** `HabitAndTaskTests.
DeleteHabitConfirmation_RemainsScrollableAndActionableAtConstrainedViewportHeight` (Playwright) força
um viewport de 390×320 (menor que o conteúdo do diálogo com Warning), confirma via
`BoundingBoxAsync()` que o botão "Delete Habit" permanece dentro dos limites verticais do viewport, e
clica nele com sucesso — provando que a rolagem interna funciona de ponta a ponta, não apenas por
inspeção do CSS.

**Resolution:** `FIXED` nesta Sprint. `.delete-confirmation` recebeu `max-height: calc(100vh -
1.25rem); overflow: hidden auto;` — o mesmo idioma já usado por `.editor-modal`
(`editor-modal.css`), ativo em todas as larguras (não apenas mobile), consistente com o padrão que
`EditorModalShell` já seguia.

**Regression Protection:** `HabitAndTaskTests.
DeleteHabitConfirmation_RemainsScrollableAndActionableAtConstrainedViewportHeight` (acima).

---

### EXP32-F023 — `BeeDayFeedbackModal` sem affordance de scroll no desktop

| Campo | Valor |
|---|---|
| Área | Modais e diálogos / Feedback de experiência (level-up) |
| Rota/Página | Qualquer conclusão de Habit/Task/To-Do/Project que dispare o modal de level-up |
| Componente | `BeeDayFeedbackModal` / `.beeday-feedback` (`BeeDayFeedbackModal.razor.css`) |
| Severidade | LOW |
| Device | Desktop (o breakpoint `max-width: 560px` já cobria mobile) |
| Accessibility Impact | No (risco teórico — `History` está limitado a `.Take(3)` na prática) |
| Owning Sprint | 32.6 (Modal & Dialog Experience) |
| State | FIXED |

**Comportamento atual (antes da correção):** `.beeday-feedback` só ganhava `max-height`/`overflow-y:
auto` dentro de `@media (max-width: 560px)` — em telas maiores, `overflow: hidden` sem `max-height`
algum. Como `History.Take(3)` limita o conteúdo hoje, isso nunca reproduziu na prática, mas é o mesmo
padrão estrutural do EXP32-F022 (achado por evidência de código, não por reprodução observada — ver
§9 sobre o limite entre achado codificado e achado verificado ao vivo).

**Resolution:** `FIXED` nesta Sprint. `.beeday-feedback` recebeu o mesmo `max-height: calc(100vh -
1.25rem); overflow: hidden auto;` na regra base (não mais restrito ao breakpoint mobile), alinhando
com `.editor-modal` e o `.delete-confirmation` corrigido acima.

**Regression Protection:** nenhum teste novo — `History.Take(3)` já garante que o conteúdo atual nunca
excede o `max-height` corrigido; nenhuma regressão de comportamento observável a proteger sem um
cenário reprodutível. Se um `History` maior for exposto no futuro, o comportamento de scroll já está
correto por construção (mesmo CSS que `EXP32-F022` prova em browser).

---

### EXP32-F024 — Camadas de z-index dos diálogos não seguem um único nível

| Campo | Valor |
|---|---|
| Área | Modais e diálogos (transversal) |
| Componente | `BeeDayFeedbackModal`/`ProjectWorkspace` (`--beeday-z-modal`, 900) vs. `EditorModalShell` (`--beeday-z-modal-raised`, 1200) vs. `BeeDayConfirmDialog` (`--beeday-z-confirmation`, 1400) |
| Severidade | LOW |
| Device | Shared |
| Accessibility Impact | No |
| Owning Sprint | não roteada — nenhum empilhamento incorreto foi observado na prática |
| State | DISCOVERED |

**Comportamento atual:** os quatro diálogos do produto usam três níveis diferentes da escala de
z-index (`variables.css`), sem uma regra explícita de "qual camada é a camada modal". Na prática, a
única combinação de empilhamento realmente exercitada hoje é editor → confirmação de exclusão
(`EditorModalShell` 1200 sob `BeeDayConfirmDialog` 1400, ordem correta) e não existe hoje nenhum fluxo
que abra `BeeDayFeedbackModal`/`ProjectWorkspace` (900) sobre um editor aberto.

**Problema:** é uma inconsistência latente, não um defeito reproduzido — se um fluxo futuro precisar
abrir o feedback de level-up por cima de um editor aberto, ele ficaria atrás (900 < 1200), não na
frente. Registrado como achado de código (não de comportamento observado em navegador) para que uma
Sprint futura que introduza esse fluxo não precise redescobrir a causa.

**Resolution:** não aplicável — `DISCOVERED`, sem correção especulativa (nenhuma evidência de
empilhamento incorreto observado nesta Sprint; ver Issue #244 sobre não introduzir "otimizações
especulativas").

**Regression Protection:** nenhuma — nada foi alterado.

---

### EXP32-F025 — `ReconnectModal` fora do contrato compartilhado de diálogo (exceção aceita)

| Campo | Valor |
|---|---|
| Área | Modais e diálogos / conectividade |
| Componente | `ReconnectModal` (`Components/Layout/`) |
| Severidade | N/A — não é um defeito |
| Owning Sprint | não roteada — exceção documentada, não um achado a corrigir |
| State | ACCEPTED |

**Comportamento atual:** `ReconnectModal` é o template padrão de Blazor Web App para o modal de
reconexão do SignalR (`<dialog>` HTML nativo, `showModal()`/`.close()`), não uma implementação do
Design System — não usa `EditorModalShell`, `BeeDayConfirmDialog`, `DialogFocusScope` nem
`beeday-dialog-focus.js`. Escape/backdrop/foco vêm nativamente do elemento `<dialog>` do navegador, não
de código do beeday. Não tem breakpoint mobile próprio (`width: 20rem; margin: 20vh auto` fixo), mas
reutiliza os tokens de cor do Design System.

**Por que é uma exceção aceita e não um achado a corrigir:** é UI de infraestrutura (perda de conexão
SignalR do próprio framework Blazor Server), não uma superfície de produto — dobrá-la no contrato de
diálogo do Design System exigiria reimplementar comportamento nativo do navegador (Escape/foco/trap)
que já funciona corretamente, por um ganho de consistência puramente cosmético em um componente que o
usuário só vê durante uma falha de conectividade. Consistente com "Não redesenhar o beeday. Refinar o
beeday" (Issue #244 §2.2).

**Resolution:** `ACCEPTED` — documentado nesta Sprint como exceção intencional ao contrato
`DialogFocusScope`/`beeday-dialog-focus.js` que os outros quatro diálogos do produto seguem.

**Regression Protection:** não aplicável.

---

### Reconciliação dos shells de diálogo (Sprint 32.6, Required Work #2 da Issue #250)

A Issue #250 pede explicitamente para "encontrar e consolidar shells de modal duplicados onde a
evidência mostrar responsabilidades equivalentes". Os quatro diálogos do produto
(`EditorModalShell`, `BeeDayConfirmDialog`, `BeeDayFeedbackModal`, `ProjectWorkspace`) foram lidos
por completo nesta Sprint para essa avaliação.

**Conclusão: já estão consolidados no nível correto — o primitivo compartilhado
(`DialogFocusScope` + `beeday-dialog-focus.js`) — e não devem ser fundidos em um único componente
visual.** Os quatro já compartilham exatamente o mesmo ciclo de vida (`docs/design-system/
02-components.md` §2: OPEN → foco inicial → contenção de Tab/Shift+Tab → Escape/close com busy
guard → restore), confirmado por leitura de código e pelos testes E2E existentes
(`InteractiveComponentsTests.NestedDialogsTrapKeyboardAndRestoreFocusAcrossEscapeClosures`,
`HabitAndTaskTests.ProjectWorkspace_UsesSharedProgressFocusAndResponsiveContracts`). O que
diverge entre eles não é o ciclo de vida — é o **propósito de cada um**: `EditorModalShell` é um
formulário (`Model`+`EditForm`, `role="dialog"`), `BeeDayConfirmDialog` é uma confirmação
destrutiva de duas ações (`role="alertdialog"`, ícone + aviso), `BeeDayFeedbackModal` é um resumo
celebratório de leitura única (`InitialFocusSelector="self"`, sem ações de formulário) e
`ProjectWorkspace` é um painel de navegação/leitura (sem `Model`/submit, com sua própria barra de
progresso e lista de To-Dos). Forçar essas quatro formas visuais em um único componente exigiria uma
árvore de parâmetros condicionais maior do que o problema real (duplicação de ~10 linhas de
`@onclick`/`@onkeydown` por componente) justifica — inconsistente com "Não redesenhar o beeday.
Refinar o beeday" (Issue #244 §2.2) e com o limite explícito da Issue #250: "Do not introduce a new
modal framework if existing foundations can be reconciled".

O único candidato real a shell duplicado fora do padrão (`ProjectWorkspace` reimplementando sua
própria estrutura de cabeçalho/backdrop em vez de estender `EditorModalShell`) foi avaliado e
descartado como duplicação indevida pelo mesmo motivo: `ProjectWorkspace` não tem `Model`/`EditForm`
nem botão de submit — não é um editor, é um painel de leitura, então não é uma instância de
"responsabilidade equivalente" a `EditorModalShell` apesar da semelhança de chrome.

O trabalho de consolidação real e seguro encontrado nesta Sprint foi a inconsistência de affordance
de scroll móvel entre os quatro (EXP32-F022/F023 acima, ambos `FIXED`) — não uma fusão de shells.

---

### Classificação de arquétipos de coleção (Sprint 32.7, Required Work #2 da Issue #251)

Os quatro tipos de coleção recorrentes do produto (Habits, Tasks/To-Dos, Projects e o próprio
`ProjectWorkspace`) foram lidos por completo para confirmar se compartilham um contrato de
apresentação coerente sem forçar um único componente genérico sobre eles.

**Conclusão: dois arquétipos reais, já compartilhados corretamente — não três/quatro implementações
paralelas.** `HabitCard` e `ActivityCard` (Tasks/To-Dos/Projects) compartilham toda a semântica
estrutural de card — grid de 3 colunas, corpo abrível (`role="button"`, `tabindex="0"`,
`@onkeydown`), foco (`:focus-within`/`:focus-visible`), touch target de 44px e tipografia — via
seletores agrupados em `cards.css` (`.activity-card, .habit-card { ... }`), divergindo apenas onde a
entidade realmente diverge: Habits têm dois botões de pontuação (`+`/`−`) em vez de um único
checkbox de conclusão, porque "pontuar um hábito" e "concluir uma tarefa" não são a mesma ação.
Forçar os dois em um único componente exigiria condicionais para a divergência mais visível do
produto (dois botões vs. um checkbox) sem eliminar duplicação real. `BeeDaySortable` já fornece o
contrato de coleção compartilhado (`role="list"`/`role="listitem"`, reordenação, virtualização
opcional) consumido identicamente pelas quatro colunas do board (`Home.razor`).

O `ProjectWorkspace` é um arquétipo à parte, não uma variação de card: é uma lista compacta de
linhas (`.project-workspace__todo`) dentro de um painel de navegação, não uma grade de cards. Antes
desta Sprint, essa lista não compartilhava a affordance de conclusão do arquétipo de card
(EXP32-F004, corrigido acima) — a correção reutiliza deliberadamente o mesmo par de classes de
checkbox do arquétipo de card (`activity-card__checkbox`/`activity-card__checkbox-glyph`) em vez de
criar um terceiro estilo de checkbox, sem forçar a linha inteira a virar um `ActivityCard`,
respeitando a divergência genuína de layout (linha compacta vs. card).

Nenhum shell de coleção órfão ou duplicado foi encontrado além do já corrigido em EXP32-F003/F004.

**Transactions e tags (Wallet), lidos por completo para fechar o restante do Scope da Issue #251:**
`TransactionCard.razor` e `WalletTagManager.razor` já usam `BeeDayCard` com o mesmo contrato de
interação (`role="button"`, `tabindex="0"`, `@onkeydown` com Enter/Space, foco visível herdado do
componente compartilhado) — nenhuma implementação paralela de card. Divergem legitimamente do
arquétipo Habit/Activity: a ação primária é abrir um editor (não concluir/pontuar), então nenhum dos
dois expõe checkbox/botão de pontuação — um terceiro arquétipo de card genuíno ("card que abre
editor"), não uma inconsistência a corrigir. Ambos já reusam foundations existentes
(`BeeDayEmptyState` para a lista vazia de tags, `BeeDayButton` para a ação de criar). Nenhuma
normalização adicional de metadata/status/action placement foi necessária nessas duas coleções.

---

### Auditoria de loading & perceived performance (Sprint 32.9, Issue #253)

Nenhum achado `EXP32-Fxxx` foi atribuído a esta Sprint — apenas o gap de evidência já registrado em
§9 ("Estados de carregamento com atraso visível não observados ao vivo"). Por isso o trabalho desta
Sprint foi auditar, com evidência de código direta, se mutações relevantes do produto expõem um
estado de processamento proporcional antes de assumir que existe um defeito a corrigir — conforme a
própria Issue #253 exige ("differentiate real latency from missing feedback before changing
behavior"; "do not add skeletons/spinners merely for visual uniformity").

**Superfícies auditadas (leitura direta do código, cinco contextos arquiteturais distintos):**

- **Dashboard (`/daily`)** — toda mutação de `DashboardState` (save/delete/toggle/register/reorder
  dos quatro tipos de coleção) já converge para um único wrapper privado, `ExecuteAsync`, que seta
  `IsBusy=true` antes de chamar a operação e `IsBusy=false` no `finally`, ignorando reentrância
  (`if (IsBusy) return`). `IsBusy` alimenta o overlay global `BeeDayLoading`, cujo CSS
  (`feedback.css`) já atrasa deliberadamente a revelação em 350ms (`animation: beeday-loading-reveal
  .16s ease .35s forwards`) especificamente para evitar flicker em operações rápidas — o padrão
  "delayed overlay" já existe e já é a fonte única, não uma implementação paralela.
- **Wallet (`/wallet`)** — `WalletInteractionState` (`TryBegin`/`End`) cobre as 4 mutações
  (salvar/excluir transação, salvar/excluir tag), desabilitando os botões relevantes via `IsBusy`
  enquanto em voo. Busca/filtro/ordenação/paginação (`RefreshAsync`) usam um flag distinto,
  `_isRefreshing`, propagado como `aria-busy` até `TransactionList` — `wallet.css`
  (`.wallet-transaction-list[aria-busy="true"]{opacity:.62;pointer-events:none}`) já traduz isso em
  feedback visual real (esmaecimento + bloqueio de interação), não apenas um atributo de
  acessibilidade sem efeito visual. Nenhuma mutação encontrada sem o par begin/end.
- **Account/Settings (`/account`)** — `Account.razor` já expõe um `IsBusy` por seção
  (Profile/Security/Preferences) mais o overlay global compartilhado
  (`<BeeDayLoading IsVisible="@IsAnyBusy" .../>`) — mesmo padrão do Dashboard, reutilizado, não
  duplicado.
- **`EditorModalShell`** (fundação compartilhada por todos os editores de Habit/Task/Todo/Project) —
  `IsBusy` já desabilita Save/Cancel/Delete e aciona o estado `IsLoading` do próprio botão de salvar;
  `Submit`/`Cancel` já são no-op enquanto `IsBusy`, prevenindo duplo-submit no nível do handler, não
  só visualmente.
- **Login (`/login`)** — página não-interativa (POST HTML tradicional para `/auth/login`, sem
  circuito Blazor), portanto não pode reusar `IsBusy`/`BeeDayLoading`. O padrão equivalente já existe
  via `onsubmit` inline: desabilita o botão de submit e troca seu texto para "Signing in..." no
  instante do POST — feedback imediato e verídico, adaptado à natureza estática desta página
  específica.

**Conclusão:** nenhum defeito de "feedback ausente" foi encontrado nas cinco superfícies auditadas —
o contrato de estado de processamento já está normalizado (um padrão compartilhado por contexto
arquitetural: `IsBusy`/`ExecuteAsync` + `BeeDayLoading` para circuitos interativos, `aria-busy` +
CSS de esmaecimento para refresh não-bloqueante, JS inline mínimo para a página estática de login).
Consistente com o limite explícito da Sprint, nenhum spinner/skeleton foi adicionado por uniformidade
visual e nenhuma otimização especulativa de rerender foi introduzida — não havia claim de redução a
provar (Required Work #4/#6 ficam vacuamente satisfeitos: nada foi reduzido porque nada precisava
ser).

**Risco residual não fechável pelo Claude:** a observação ao vivo do overlay `BeeDayLoading` sob
latência de rede real (throttling), registrada em §9, permanece como evidência de runtime/HMG —
consistente com o Required Work #7 desta própria Issue ("record runtime-only observations as
requiring HMG/release evidence rather than forcing Claude browser execution"). Não promovida a
`VERIFIED`/`FIXED` por inspeção de código apenas, conforme a Issue #244 §3.1 exige.

---

### Empty states & first-use experience (Sprint 32.10, Issue #254)

Nenhum achado novo foi atribuído a esta Sprint além do cross-ref já registrado a `EXP32-F013`
(`FIXED` na Sprint 32.8). O Required Work #6 da Issue #254 exige manter "filtered no-results
recovery connected to the 32.8 filter contract" — a 32.8 já distinguia corretamente
vazio-genuíno de sem-resultados-de-filtro, mas só com texto descritivo ("tente outro termo, ou
limpe-o"), sem uma ação real. Esta Sprint fecha essa lacuna reusando o padrão canônico já
estabelecido pelo Wallet (`WalletEmptyState`: `BeeDayEmptyState` + `BeeDayButton` de "Clear
Filters" logo abaixo, variante `ConfirmationCancel`) em vez de inventar um sistema de empty-state
paralelo para o Dashboard:

- `DashboardState.ClearFilters()` (novo) — reset único e determinístico de `Search` e
  `SelectedProjectId` juntos, já que qualquer um dos dois pode ter causado o resultado zerado na
  coluna To-Dos; nenhuma das quatro colunas precisa saber qual filtro foi a causa real.
- `DashboardColumn` ganha dois parâmetros novos, `ShowClearFilterAction`/`OnClearFilter` — quando
  verdadeiro, renderiza um `BeeDayButton` (`Variant="ConfirmationCancel"`, `Compact="true"`) logo
  abaixo do `BeeDayEmptyState` existente. Nenhuma mudança na fundação `BeeDayEmptyState` em si
  (permanece agnóstica a ação, reusada por Wallet/Dashboard/`BeeDayErrorBoundary` sem
  divergência).
- `Home.razor` passa `ShowClearFilterAction="@State.XxxFilteredToZero"` e
  `OnClearFilter="State.ClearFilters"` nas quatro colunas — a mesma condição já usada para a
  escolha de texto na Sprint 32.8, agora também decidindo a presença do CTA.

Classificação dos quatro estados exigida pelo Required Work #2 já está completa desde a 32.8/32.9:
`DISCOVERED`→vazio genuíno (texto/CTA de criação já existente por coluna), sem-resultados-de-filtro
(`NoFilterResultsTitle`/`Description` + o novo CTA "Clear search"), e erro (`_errorMessage`/`BeeDayEmptyState`
não é usado para mascarar falhas — confirmado por leitura de `Home.razor`/`DashboardState`, que não
têm um caminho de erro renderizado como estado vazio).

**Regression Protection:** `DashboardColumnTests.ShowClearFilterAction_RendersAClearButtonThatInvokesOnClearFilter`,
`WhenShowClearFilterActionIsFalse_RendersNoClearButton`, e `DashboardStateTests.ClearFilters_ResetsSearchAndSelectedProjectContext`
— todos database-free.

---

## 7. Achados pré-existentes roteados para dentro da EPIC 32

| ID original | Ledger de origem | Achado | Sprint EPIC 32 |
|---|---|---|---|
| `BD30-F021` | `docs/epics/30-system-integrity/README.md` | Nenhuma prova de navegador para tema, alteração de senha e recuperação visível dos demais saves de `/settings` (`docs/testing/03-functional-journey-matrix.md` §2/§3.4) — ainda `OPEN` | 32.17 (Settings, Profile & Account Experience Polish) |

Este item não recebeu um novo ID `EXP32-Fxxx` — ele permanece com sua identidade original na EPIC 30,
e é roteado aqui porque fechar a lacuna de evidência de navegador para as seções de Preferences/
Security de `/settings` é exatamente o tipo de trabalho que a Sprint 32.17 existe para fazer.

## 8. Jornadas críticas que exigirão evidência de navegador no gate final

Conforme exigido pela Issue #245, item 7 ("Identify critical journeys that will require browser/E2E
coverage at the final gate"):

1. Cadastro → confirmação de e-mail por link real → login → onboarding → `/daily` (já E2E, ver
   `docs/testing/03-functional-journey-matrix.md` §2 — manter).
2. Criar → concluir → editar → excluir um Habit, uma Task, um To-Do (dentro e fora do workspace) e um
   Project, incluindo o ciclo de conclusão de To-Do a partir de **ambos** os pontos de entrada
   identificados em EXP32-F004, uma vez corrigido.
3. Ciclo de vida completo de uma transação do Wallet (criar, editar, excluir com confirmação real,
   não cancelada como nesta Sprint) em `pt-BR` e `en-US`, incluindo os dois formatos de data/número
   observados em EXP32-F005/EXP32-F006, uma vez corrigidos.
4. Salvar Profile, alterar senha e alterar Preferences (tema/idioma) em `/settings`, fechando
   `BD30-F021` (§7) dentro do escopo da Sprint 32.17.
5. Navegação completa por teclado do menu "+ Activity" (Daily) até salvar e fechar um editor,
   incluindo a correção de EXP32-F001 e EXP32-F002.
6. As 12 páginas editoriais do footer institucional e o `beeday Experience System` completo, em
   viewport mobile real (bloqueado nesta Sprint por limitação de ferramenta — ver §9).
7. Shell autenticado (`DesktopSidebar`/`MobileHeader`/`MobileSidebar`) e o board do Daily nos
   breakpoints 620/900/1199/1200px, em viewport mobile real (mesmo bloqueio).

## 9. Riscos residuais e limitações desta sessão

- **Viewport móvel não testado ao vivo.** A ferramenta `resize_window` reportou sucesso, mas
  `window.innerWidth` permaneceu no tamanho físico do monitor (2552px) em todas as tentativas
  (inclusive isolando a chamada, sem nenhuma outra ação no mesmo lote, e tentando novamente em uma
  aba nova). Uma tentativa de simular a largura via `document.documentElement.style.zoom` confirmou
  que o `zoom` do Chromium escala apenas a renderização visual, sem alterar `window.innerWidth` nem
  o resultado de `matchMedia`, portanto não é um substituto válido. Nenhum achado de responsividade
  novo foi criado nesta Sprint — o contrato de breakpoints documentado (`docs/ux/03-responsive.md`) e
  a cobertura de teste E2E existente (citada em `docs/web/03-layouts.md` e no Ledger da EPIC 30) são a
  evidência disponível. **Isto é um risco residual explícito para a Sprint 32.14** (Responsive &
  Mobile Interaction Polish): ela deve resolver ou contornar esta limitação de ferramenta antes de
  poder declarar qualquer achado de responsividade como `VERIFIED` — o Evidence Model da Issue #244
  §7.3 proíbe declarar um achado visual/responsivo como `FIXED`/`VERIFIED` apenas por inspeção de
  código.
- **Seção Preferences de `/settings` não revisitada ao vivo** nesta sessão, após a instabilidade de
  ferramenta descrita em §3 ter tornado a rolagem da página pouco confiável para captura de tela
  fiel. As seções Profile e Security foram verificadas normalmente. Sem achado associado — apenas
  gap de cobertura explícito para quem herdar 32.17.
- **Filtros do Wallet (busca, tipo, tag, intervalo de data) não exercitados ao vivo** nesta Sprint —
  apenas o formulário de criação/edição de transação foi testado. Cobertura de teste automatizado
  existente (`WalletTests`, `WalletValidatorTests`) foi usada como evidência de que os filtros têm
  contrato coberto, sem substituir a inspeção de interação da Sprint 32.16.
- **Estados de carregamento com atraso visível não observados ao vivo** — todas as mutações desta
  sessão (ambiente local, LocalDB, sem latência de rede real) completaram antes do atraso documentado
  de 350ms do `BeeDayLoading`. Nenhum achado foi criado; a Sprint 32.9 (Loading & Perceived
  Performance) deve reproduzir latência real (throttling de rede) para validar o overlay ao vivo.

Nenhum destes riscos bloqueou a criação do Ledger — cada um está registrado com a Sprint futura que
deve fechá-lo, conforme a Issue #245 exige ("map each finding to exactly one owning later Sprint").

## 10. Roteamento por Sprint futura (resumo)

| Sprint | Achados herdados desta Sprint |
|---|---|
| 32.2 — Application Shell & Navigation Fluidity | EXP32-F014, EXP32-F015 — ambos `FIXED` nesta Sprint |
| 32.3 — Page Layout Consistency | EXP32-F016, EXP32-F017 — ambos `FIXED` nesta Sprint; EXP32-F018, EXP32-F019 — `ACCEPTED` (exceções documentadas); EXP32-F020 — novo, roteado para 32.13; EXP32-F009 — evidência adicional (`/profile`) |
| 32.4 — Buttons & Action Hierarchy | EXP32-F001 — `FIXED` nesta Sprint |
| 32.5 — Forms & Input Experience | EXP32-F005 — `FIXED` nesta Sprint; EXP32-F007 — `FIXED` nesta Sprint; EXP32-F021 — novo, `FIXED` nesta Sprint; EXP32-F006 (cross-ref), EXP32-F011 (cross-ref), EXP32-F012 (cross-ref) |
| 32.6 — Modal & Dialog Experience | EXP32-F022, EXP32-F023 — ambos novos, `FIXED` nesta Sprint; EXP32-F024 — novo, `DISCOVERED` (não roteada — latente, sem Sprint dona); EXP32-F025 — novo, `ACCEPTED` (exceção documentada); EXP32-F002 (cross-ref, não corrigido — pertence à 32.13) |
| 32.7 — Lists, Cards & Collection Patterns | EXP32-F003, EXP32-F004 — ambos `FIXED` nesta Sprint |
| 32.8 — Search, Filters & Sorting | EXP32-F013 — `FIXED` nesta Sprint |
| 32.9 — Loading & Perceived Performance | gap de evidência registrado em §9 (sem achado novo); auditoria de código confirmou nenhum defeito de feedback ausente — ver §"Auditoria de loading & perceived performance" |
| 32.10 — Empty States & First-Use Experience | EXP32-F013 (cross-ref) — CTA "Clear search" adicionado ao estado sem-resultados |
| 32.11 — Errors, Recovery & User Feedback | EXP32-F007 (cross-ref) |
| 32.12 — Toasts, Notifications & Confirmation Feedback | nenhum achado novo |
| 32.13 — Focus, Keyboard & Interaction Accessibility | EXP32-F001 (cross-ref), EXP32-F002, EXP32-F008, EXP32-F009, EXP32-F010, EXP32-F020 |
| 32.14 — Responsive & Mobile Interaction Polish | risco residual de §9 (evidência de navegador móvel pendente) |
| 32.15 — Daily Experience Polish | EXP32-F004 (cross-ref), EXP32-F010 (cross-ref) |
| 32.16 — Wallet Experience Polish | EXP32-F005 (cross-ref), EXP32-F006 |
| 32.17 — Settings, Profile & Account Experience Polish | `BD30-F021` (§7), gap de Preferences (§9) |
| 32.18 — Public Pages & Brand Experience Polish | nenhum achado novo; 3 rotas públicas revalidadas sem defeito |
| 32.19 — Front-End Code Refinement & Component Consolidation | EXP32-F011, EXP32-F012 |
| 32.20 — Full Product Polish & Final Experience Gate | consome o estado final de todo o Ledger |

## 11. Validação desta Sprint

Esta Sprint não alterou nenhum arquivo de produto — apenas criou este documento e atualizou
`docs/README.md` (linha de índice) — portanto a validação obrigatória confirma que nenhuma mudança
de código foi introduzida:

```text
dotnet format BeeDay.slnx --verify-no-changes   -> PASS (sem alterações a formatar)
dotnet build BeeDay.slnx --configuration Release --warnaserror -> PASS (0 avisos, 0 erros)
git diff --check                                -> PASS (sem problemas de whitespace)
git status                                       -> apenas os arquivos desta entrega staged
```

`dotnet test` não foi executado com foco nesta Sprint porque nenhum comportamento executável mudou
(apenas documentação) — consistente com a política de validação proporcional já usada em entregas
anteriores de documentação pura (ver, por exemplo, o histórico de PRs da EPIC 31). Os achados
`EXP32-Fxxx` acima são, eles próprios, a evidência de comportamento desta Sprint — obtida executando
a aplicação real, não simulada.

## 12. Fontes consultadas

- `docs/web/02-routing-and-pages.md`, `03-layouts.md`, `04-feature-components.md`.
- `docs/design-system/02-components.md`.
- `docs/ux/01-guidelines.md`, `02-accessibility.md`, `03-responsive.md`.
- `docs/testing/03-functional-journey-matrix.md`.
- `src/BeeDay.Web/Components/Features/Dashboard/`, `Wallets/`, `Account/`, `Authentication/`,
  `Institutional/`, `ExperienceSystem/`.
- `src/BeeDay.Infrastructure/Persistence/SqlServer/BeeDayDbContextFactory.cs`,
  `src/BeeDay.Infrastructure/Identity/DevelopmentEmailSender.cs`.
- Execução real de `dotnet run --project src/BeeDay.Web/BeeDay.Web.csproj` contra SQL Server LocalDB
  local, navegada via automação de browser Chromium real em 2026-08-22.
