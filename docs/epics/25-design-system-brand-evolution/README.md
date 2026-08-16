# EPIC 25 — beeday Design System & Brand System Evolution

**Fonte da verdade:** contexto oficial da EPIC 25 estabelecido pelo responsável pelo repositório
(matriz completa recebida em 2026-08-16); descobertas e decisões da Sprint 25.1 verificadas
diretamente em código durante a própria Sprint (branch
`sprint/25.1-design-system-governance-brand-contract`, criada a partir de `hmg` em `d5bc5d9`) —
leitura direta de `src/BeeDay.Web/`, `src/BeeDay.Domain/`, `src/BeeDay.Application/`,
`src/BeeDay.Infrastructure/`, `docs/design-system/`, `docs/ux/`, `docs/web/`,
`docs/epics/20-home-visual-experience/`, `docs/epics/21-lingo-product-experience/`, e execução real
de `dotnet format`/`dotnet build`/`dotnet test`. Nenhuma afirmação de "estado atual" abaixo vem de
memória — quando este documento evoluir em Sprints futuras, cada atualização deve reverificar
contra o código antes de alterar uma afirmação de estado atual.

**Última verificação:** 2026-08-16 (Sprint 25.1 — Design System Governance & Brand Contract,
COMPLETE — primeira Sprint de implementação da EPIC; ver "Sprint 25.1 — Results" ao final deste
documento).

**Escopo:** formalizar a governança normativa que sustenta toda a EPIC 25 (beeday Design System &
Brand System Evolution) e o contrato oficial de marca do beeday, evoluindo o Design System já
existente em `Components/DesignSystem/` — não uma reescrita paralela. A EPIC nasceu porque a EPIC 24
(refinamento visual da Home) foi deliberadamente pausada: continuar refinando páginas individuais
antes de formalizar as foundations globais arriscava produzir novos microssistemas visuais. Esta
EPIC fortalece o sistema global primeiro.

## Source of Truth

- Arquitetura atual: [`docs/architecture/`](../../architecture/README.md) — Clean Architecture,
  direção de dependência `Domain ← Application ← Infrastructure ← Web`.
- Design System atual: [`docs/design-system/`](../../design-system/README.md) — componentes
  reutilizáveis sob `src/BeeDay.Web/Components/DesignSystem/`, sem projeto/assembly separado (ver
  "Sprint 25.1 — Baseline Revalidation" abaixo para a contagem verificada nesta Sprint).
- UX/acessibilidade/responsividade atuais: [`docs/ux/`](../../ux/README.md).
- Implementação atual: `src/BeeDay.Web/` é a única fonte de verdade de comportamento — qualquer
  divergência entre este documento e o código deve ser resolvida a favor do código, reportada, e
  corrigida aqui.
- Contexto histórico direto: [`docs/epics/20-home-visual-experience/README.md`](../20-home-visual-experience/README.md)
  (Home, shell público, Design System evolution) e
  [`docs/epics/21-lingo-product-experience/README.md`](../21-lingo-product-experience/README.md)
  (mapeamento Lingo → BeeDay, cores, tipografia, componentes) — a EPIC 25 não repete essas
  descobertas, apenas as referencia quando relevante.
- Governança de Git/aprovação: [`CLAUDE.md`](../../../CLAUDE.md) — a seção 13 (Brand contract) foi
  adicionada nesta Sprint como ponteiro permanente para este documento.

---

## Brand Contract

Decisões oficiais, aprovadas antes do início da execução da EPIC 25, válidas para toda a EPIC sem
precisar ser rediscutidas por Sprint:

| Elemento | Valor oficial |
|---|---|
| Nome da marca | `beeday` |
| Casing visual oficial | lowercase (`BeeDay`/`BEEDAY`/`Beeday` não são a representação oficial visível) |
| Cor oficial da marca | `#5247F9` |

**Regra de uso da cor:** sempre que o nome visual da marca `beeday` for apresentado como elemento de
branding, sua cor oficial é `#5247F9`, salvo contexto técnico ou acessível em que representação
visual de cor não seja aplicável. Não substituir por aproximações.

**Evidência confirmada nesta Sprint — a decisão já está parcialmente implementada, não é uma
introdução nova:** `src/BeeDay.Web/wwwroot/css/variables.css:5` já declara
`--beeday-color-brand-primary: #5247f9` (remigrado na EPIC 22, conforme comentário em
`variables.css:183,190`), consumido por `.beeday-brand__bee` em
`Components/DesignSystem/Text/BeeDayBrand.razor.css:15`. O componente `BeeDayBrand`
(`Components/DesignSystem/Text/BeeDayBrand.razor`) já renderiza o wordmark em lowercase — dois
`<span>` com texto literal `bee`/`day`, não `Bee`/`Day`. Esta Sprint **formaliza** a regra; não
altera nenhum CSS, token ou componente.

### Typography (direção registrada, não decidida nesta Sprint)

Nunito já é usada globalmente no repositório (`--beeday-font-body`) e é a hipótese de trabalho para
Product/UI type — tratar uma eventual formalização como consolidação do que já existe, não como
troca. Coiny é candidata aprovada para estudo como Brand/Display type — ainda não decidida; requer
validação de licença, distribuição, rendering, métricas, acentos pt-BR/en-US, clipping, line-height,
mobile, performance, fallback e acessibilidade antes de qualquer adoção. Essa validação pertence à
Sprint 25.4, não a esta.

---

## Brand Identity vs. Technical Identity

```text
Brand identity  ≠  Technical identity
```

A decisão de marca acima **não autoriza rename técnico**. Permanecem `BeeDay` (identidade técnica)
até que uma Sprint específica aprove explicitamente uma mudança técnica, com análise de consumers e
backward compatibility:

- solution (`BeeDay.slnx`) e todos os `.csproj` (`BeeDay.Domain`, `BeeDay.Application`,
  `BeeDay.Infrastructure`, `BeeDay.Web`, e os 5 projetos de teste);
- namespaces (`namespace BeeDay.*` em todo `src/`/`tests/`);
- assemblies e artefatos de build (ex.: `BeeDay.Web.styles.css`, o bundle de CSS isolation
  referenciado em `Components/App.razor:34`);
- classes e componentes prefixados `BeeDay*` (`BeeDayButton`, `BeeDayCard`, `BeeDayIcon`,
  `BeeDayBrand`, os demais componentes do Design System, `BeeDayDbContext` e correlatos);
- migrations e artefatos de banco de dados;
- configuration keys (ex.: `BeeDay:Persistence:SqlServer:ConnectionString`);
- o identificador interno `SetApplicationName("BeeDay")` do ASP.NET Core Data Protection
  (`src/BeeDay.Web/Program.cs:63`) — isolamento de chaves, nunca exibido a usuário;
- claims, cookies, variáveis de ambiente, caminhos IIS, workflows, scripts, contratos externos,
  test infrastructure.

**Confirmado nesta Sprint:** `src/BeeDay.Domain` (47 arquivos que contêm `BeeDay`) e
`src/BeeDay.Application` (95 arquivos) não têm **nenhuma** ocorrência literal de string `"BeeDay"` —
toda ocorrência é `namespace BeeDay.*` ou caminho de `.csproj`. A fronteira arquitetural entre
identidade técnica e copy visível está intacta nessas duas camadas: nenhuma string de marca vaza
para Domain/Application. Nenhuma ação necessária; registrado como confirmação positiva.

---

## Design System Governance

O Design System atual (`src/BeeDay.Web/Components/DesignSystem/`) é o ponto de partida canônico —
não deve ser substituído nem duplicado por uma segunda árvore de componentes.

**Hierarquia obrigatória antes de criar algo novo:**

```text
REUSE → EXTEND → CONSOLIDATE → REFACTOR → CREATE ONLY IF NECESSARY
```

Regras permanentes:

- não devem existir implementações paralelas sem necessidade comprovada;
- não criar componentes `V2` para evitar compreender ou evoluir o componente atual;
- shared behavior pertence a shared components;
- Product Patterns podem permanecer especializados quando a diferença for semanticamente legítima —
  nem toda divergência entre áreas (Login, Identity, Wallet, ProjectWorkspace) é dívida a eliminar;
- accessibility, responsive behavior e localization fazem parte do contrato do componente, não um
  extra opcional;
- backward compatibility deve ser preservada sempre que possível;
- mudanças devem ser incrementais, não big-bang — cada consumidor migra com confirmação visual
  própria, seguindo o padrão já usado pela EPIC 21 para a migração do `BeeDayButton`.

### Governança de hardcode vs. token

> Hardcode não é automaticamente dívida, e token não é automaticamente a abstração correta.

Antes de promover um valor local para foundation/token, verificar: significado, reutilização,
ownership, semântica, consumers reais, necessidade de states/variants, e o risco de criar um token
específico demais para um único consumidor. Quando um novo token for justificado, ele deve ser
classificável em uma destas categorias (taxonomia a refinar na Sprint 25.3, registrada aqui apenas
como vocabulário): `Brand`, `Surface`, `Content`, `Semantic`, `Feedback`, `Product`, `Illustration`,
`Component`. Nenhuma reorganização de tokens ocorre nesta Sprint.

## Brand System vs. UI Design System

Os dois sistemas compartilham foundations quando apropriado, mas não são a mesma responsabilidade.
Nenhuma nova camada arquitetural é criada por causa desta distinção — é conceitual, não física.

| Brand System | UI Design System |
|---|---|
| Identity, wordmark, logo, brand color | Foundations, semantic tokens |
| Expressive typography, imagery | Components, states, layouts |
| Illustration, characters | Forms, navigation, surfaces, feedback |
| Voice, tone, writing | Responsive behavior, accessibility contracts |
| Marketing/brand expression | Interaction |

`/design-system/*` (quando existir como rota) é catálogo técnico/de desenvolvimento;
`/brand/*` (planejado, primeira página `/brand/typography` na Sprint 25.4) é guideline pública da
marca. As duas responsabilidades não devem ser fundidas sem razão arquitetural verificada, e uma
página `/brand/*` só é publicada quando a guideline correspondente já está formalmente estabilizada
— sem páginas vazias ou placeholders. Nenhuma das duas existe como área de código nesta Sprint; esta
seção registra apenas a distinção conceitual para orientar as Sprints 25.2–25.16.

## Decision Taxonomy

Vocabulário oficial para classificar decisões ao longo de toda a EPIC 25:

| Termo | Significado |
|---|---|
| `PRESERVE` | a solução atual está correta e é intencional — não mexer |
| `FORMALIZE` | uma prática que já existe na implementação precisa virar regra documentada explícita |
| `CONSOLIDATE` | múltiplas soluções equivalentes devem convergir para uma só |
| `REFINE` | a solução é válida, mas precisa de melhoria incremental |
| `REPLACE` | a solução atual é inadequada e deve ser substituída de forma controlada, com plano de migração |
| `REMOVE` | o elemento é comprovadamente desnecessário ou obsoleto |
| `NEW` | é uma capacidade genuinamente ausente, sem equivalente a reaproveitar |
| `DEFER` | a decisão depende de evidência adicional ou pertence a uma Sprint posterior |

Esta Sprint usa `FORMALIZE` para o Brand Contract (a cor e o casing já existiam em código; a regra
governando-os é que é nova) e `DEFER` para todo item do inventário da Sprint 25.2 abaixo.

---

## Roadmap

| Sprint | Objetivo |
|---|---|
| 25.1 | Design System Governance & Brand Contract |
| 25.2 | Brand Identity & Wordmark Convergence |
| 25.3 | Color System Consolidation |
| 25.4 | Typography System & Public Typography Guidelines (`/brand/typography`) |
| 25.5 | Shape, Spacing, Borders & Depth |
| 25.6 | Motion, Interaction & Layer System |
| 25.7 | Responsive, Layout & Breakpoint System |
| 25.8 | Core Component Contracts & State Matrix |
| 25.9 | Forms, Authentication & Identity Convergence |
| 25.10 | Feedback, Dialogs & Accessibility Lifecycle |
| 25.11 | Wallet Design System Convergence |
| 25.12 | Daily & ProjectWorkspace Convergence |
| 25.13 | Character & Illustration System |
| 25.14 | Writing, Voice, Tone & Localization System |
| 25.15 | Design System Quality Engineering |
| 25.16 | Documentation, Migration Sweep & Final Quality Gate |

**Dependency gates:** Foundation gate `25.1 → 25.2 → … → 25.7` é sequencial. Component gate
`25.8 → 25.9 → 25.10` depende do Foundation gate concluído. Product convergence (`25.11`, `25.12`)
depende do Component gate. Brand language (`25.13`, `25.14`) segue. Quality/closure
(`25.15 → 25.16`) é a última etapa. Nenhuma Sprint avança automaticamente — cada uma aguarda
autorização explícita do usuário.

---

## Sprint 25.1 — Design System Governance & Brand Contract (Results)

**Branch:** `sprint/25.1-design-system-governance-brand-contract`, criada a partir de `hmg` em
`d5bc5d9` (working tree limpo confirmado antes da criação — ver relatório da Sprint para o `git
status` completo).

Esta Sprint não alterou nenhum arquivo em `src/` ou `tests/` — apenas documentação
(`CLAUDE.md`, `docs/README.md`, `docs/design-system/README.md`, e este documento). Nenhuma mudança
funcional ou visual foi implementada.

### Baseline Revalidation

Revalidação executada nesta Sprint, não assumida do baseline anterior (auditoria pré-EPIC de
2026-08-16, que registrava 1.054/1.063 aprovados e 9 falhas, causa não definitivamente comprovada):

| Comando | Resultado |
|---|---|
| `dotnet format BeeDay.slnx --verify-no-changes` | Aprovado — sem alterações necessárias |
| `dotnet build BeeDay.slnx` | Aprovado — 0 Aviso(s), 0 Erro(s) |
| `dotnet test BeeDay.slnx` (execução completa) | 1.063 testes, **1.062 aprovados, 1 com falha** |
| Retry isolado de `RateLimitingIntegrationTests` | 6/6 aprovados, 0 falhas |

A falha única na execução completa foi
`BeeDay.Web.Tests.Integration.RateLimitingIntegrationTests.IpLimit_BlocksFurtherAttemptsAcrossDifferentEmails`
(`Assert.Equal` esperava `TooManyRequests`, obteve `Found`,
`tests/BeeDay.Web.Tests/Integration/RateLimitingIntegrationTests.cs:90`). É um teste de rate
limiting sensível a timing (janela de tempo real), executado sob contenção de uma suíte completa de
1.063 testes. O retry isolado da classe inteira passou 100%. **Classificação:** confirmed
transient/flaky, consistente com o padrão já registrado para contenção de LocalDB/Playwright sob
suíte completa — não é uma regressão introduzida por esta Sprint (nenhum arquivo de `src`/`tests` foi
alterado) e não pertence ao escopo desta Sprint corrigir (rate limiting de Identity, não Design
System/Brand).

**Reconciliação com o baseline pré-EPIC:** o número "1.054/1.063, 9 falhas" registrado antes do
início da EPIC 25 não se sustentou nesta reexecução — o estado real, verificado agora, é
substancialmente melhor (1.062/1.063 na primeira passada; 1.063/1.063-equivalente após confirmar a
única falha como transiente). O número antigo nunca foi confirmado como reproduzível pela própria
auditoria que o gerou. Esta Sprint adota 1.063 testes / 1 falha transiente confirmada como a
referência corrente para a EPIC 25 daqui em diante, substituindo o número antigo.

### Design System — Contagem Verificada

Confirmado por leitura direta (`Glob` de `Components/DesignSystem/**/*.razor`), não por documentação
citada de memória: **25 componentes reutilizáveis** fisicamente em `Components/DesignSystem/`
(excluindo as 2 páginas de catálogo roteáveis, `HeroCatalog.razor`/`IconCatalog.razor`, que não são
componentes reutilizáveis) + `BeeDaySortable` (fisicamente fora da pasta, em
`Components/Behaviors/DragDrop/`, documentado como Design System interop) = **26 no total**. Este
número reconcilia com a matriz da EPIC 25 ("~25 primitives canônicos, 26 incluindo
`BeeDaySortable`"). A própria tabela de contagem em `docs/design-system/README.md` (24, última
verificação Sprint 20.3, antes da EPIC 21 começar) está desatualizada — falta
`Components/DesignSystem/Progress/BeeDayProgressBar.razor` (+ `BeeDayProgressTone.cs`), adicionado
depois (Sprint 21.6, Progress Right Rail). Reportado e registrado em
[`docs/design-system/README.md`](../../design-system/README.md) ("Achados relevantes"), não
corrigido na tabela em si nesta Sprint (limpeza documental transversal pertence à Sprint 25.16 ou à
próxima Sprint que tocar aquele documento).

### Documentação canônica

Nenhum documento existente era o owner adequado para Brand/Governance da EPIC 25.
`docs/design-system/README.md` é o owner do Design System (componentes/tokens verificados a partir
do código); `docs/ux/README.md` é o owner de UX/acessibilidade/responsividade; nenhum dos dois é
apropriado para decisões de marca aprovadas pelo responsável pelo produto (não verificáveis a partir
do código da mesma forma). Seguindo o precedente já estabelecido pela EPIC 20 (Decisão 3, "EPIC
transversal a múltiplas áreas documentais") e pela EPIC 21, este documento
(`docs/epics/25-design-system-brand-evolution/README.md`) foi criado como a área documental oficial
da EPIC 25 — decisão aplicada de forma mínima, sem reorganizar `docs/design-system/`/`docs/ux/`
existentes. `docs/README.md` recebeu uma linha na tabela "Áreas" apontando para este documento
(mesmo padrão das linhas de `epics/20-*` e `epics/21-*`). `docs/design-system/README.md` recebeu um
ponteiro de volta (item 5 em "Ordem de leitura recomendada"). `CLAUDE.md` recebeu a seção 13 (Brand
contract), a menor adição necessária para persistir a regra permanente "brand identity ≠ technical
identity" no arquivo que rege todas as tarefas futuras no repositório, com ponteiro para este
documento em vez de duplicar o contrato completo.

Uma área `docs/brand/` dedicada (documentação técnica verificável a partir do código, no mesmo
espírito de `docs/design-system/`) foi avaliada e **não criada nesta Sprint** — classificação
`DEFER`: ainda não há conteúdo formalizado o suficiente para justificar uma nova área própria além
deste registro de decisão de Epic; reavaliar quando a Sprint 25.2 (Wordmark) ou 25.4
(`/brand/typography`) produzirem conteúdo verificável a partir do código que mereça um owner
técnico próprio, distinto do registro de decisões desta Sprint.

### Inventário de ocorrências `BeeDay` / `beeday` / `BEEDAY` — input para a Sprint 25.2

Classificação por padrão/localização (não por ocorrência individual — os buckets técnicos somam
centenas de arquivos por design arquitetural, listar cada um não agregaria valor). Todas as citações
abaixo foram verificadas por leitura direta nesta Sprint.

**VISUAL BRAND** — representações visíveis da marca:

- `Components/DesignSystem/Text/BeeDayBrand.razor` (+ `.razor.css`) — wordmark vivo em texto,
  lowercase (`bee`/`day` como nós de texto literais), duas cores: `bee` em
  `--beeday-color-brand-primary` (`#5247f9`), `day` em `--beeday-color-brand-yellow` (`#ffd326`).
  Usado em `DesktopSidebar`, `MobileSidebar`, `MobileHeader` (shell autenticado).
- `wwwroot/assets/brand/beeday-top-navigation.png` — wordmark em imagem estática, lowercase
  "beeday", cor única (azul/roxo de marca) + ilustração de uma abelha. Usado em `PublicHeader` e
  `AppFooter` (shell público). **Duas representações de wordmark confirmadas nesta Sprint**,
  visualmente diferentes uma da outra (texto bicolor vs. imagem monocromática + mascote) — evidência
  concreta do gap "três representações concorrentes de wordmark" do baseline; a terceira é o
  favicon abaixo. Não convergido aqui — pertence à Sprint 25.2.
- `wwwroot/favicon.png` — glifo genérico "@", sem relação com a abelha ou com o wordmark "beeday".
- `Components/Features/Home/HomeResources*.resx` — "BeeDay — Be better every day" / "Seja melhor a
  cada dia" (Title Case) e "How beeday works" / "Como o beeday funciona" (lowercase) **no mesmo
  arquivo**, nas duas culturas — a única inconsistência de casing confirmada dentro de um único
  documento de recursos.

**TECHNICAL IDENTITY** — não renomear sem aprovação explícita (ver seção acima):

- `namespace BeeDay.*`: Domain (47 arquivos), Application (95 arquivos), Infrastructure
  (49 arquivos), Web — confirmado sem string de marca literal vazando em Domain/Application.
- `BeeDay.slnx`, os 9 `.csproj`, `BeeDayDbContext`/`BeeDayDbContextFactory`/migrations, todos os
  componentes `BeeDay*`, `BeeDay.Web.styles.css`.
- `SetApplicationName("BeeDay")` (`Program.cs:63`) — Data Protection, nunca exibido.
- 6 arquivos em `.github/workflows/` — nomes técnicos de CI/artefato, não inventariados linha a
  linha (fora do raio de impacto de um wordmark/casing).

**ACCESSIBLE NAME** — texto exposto a tecnologia assistiva, tratamento hoje inconsistente:

- `BeeDayBrand.razor:3` — `aria-label="BeeDay"` (Title Case), pinado por
  `tests/BeeDay.Web.Tests/Components/Text/BeeDayBrandTests.cs:13`.
- `AppFooter.razor:6` — `alt="BeeDay"` na imagem do wordmark.
- `PublicHeader.razor:6` — **mesma imagem**, `alt=""` (tratada como decorativa) — inconsistente com
  o Footer para o mesmo asset.
- `SharedResources*.resx` — "BeeDay home" / "Página inicial do BeeDay" (contexto de aria-label).

**RESOURCE/COPY** — texto de produto localizado (en-US/pt-BR):

- Padrão de título de página `"<Página> | BeeDay"` repetido em ~10 arquivos de recursos (Wallet,
  Account, ProfileCreation, Dashboard, Onboarding, Authentication, 5× Identity).
- Corpo de texto citando o produto: "Start your journey in BeeDay.", "Welcome to BeeDay.", "Manage
  the personal information shown across BeeDay.", "Choose how BeeDay looks and communicates with
  you.", e equivalentes pt-BR.
- Compartilhado: "BeeDay links"/"Links do BeeDay", copyright do footer
  ("© 2026 BeeDay. All rights reserved." / "© 2026 BeeDay. Todos os direitos reservados."), "Log out
  of BeeDay"/"Sair do BeeDay", "BeeDay — go to Profile"/"BeeDay — ir para o Perfil".
- Consistentemente Title Case "BeeDay" nas duas culturas, com a única exceção já registrada em
  VISUAL BRAND (`HomeResources`).

**TEST EXPECTATION** — pinam texto/casing hoje; migram junto de qualquer mudança na Sprint 25.2:

- `BeeDayBrandTests.cs` — `aria-label`, texto dos nós `bee`/`day`.
- `tests/BeeDay.Infrastructure.Tests/IdentityInfrastructureTests.cs` — assunto/remetente exatos dos
  e-mails transacionais (ver REQUIRES REVIEW).
- `tests/BeeDay.Web.Tests/Components/Visual/VisualFoundationTests.cs`,
  `tests/BeeDay.E2E.Tests/VisualFoundationTests.cs` — cobrem os tokens de fundação, não
  inventariados linha a linha nesta Sprint.

**HISTORICAL** — nunca editado por casing/rename:

- `docs/history/*` (10 documentos), `docs/adr/*` (5 ADRs + README) — registro congelado.
- `docs/history/current-state-sprint-log.md` documenta que o próprio nome técnico do produto já
  mudou uma vez no passado (`LevelUp.Domain`/`LevelUp.Application`/... → `BeeDay.*`, era Sprint
  ~14.x) — precedente direto de que renames técnicos neste repositório são deliberados, cobrem o
  corpus inteiro de uma vez, e ficam documentados; nunca casuais. Reforça, não contradiz, a regra
  desta Sprint de não fazer rename técnico indiscriminado.
- Seções de resultado já escritas dentro de `docs/epics/20-*/README.md` e `docs/epics/21-*/README.md`.

**DEVELOPMENT/DOCUMENTATION** — superfícies internas/dev-facing, prioridade menor que produto público:

- `Components/DesignSystem/Pages/IconCatalog.razor:5` — `<PageTitle>BeeDay Icon Catalog |
  BeeDay</PageTitle>` (hardcoded, não localizado — consistente com ser catálogo de
  desenvolvimento, não página de produto).
- `Components/DesignSystem/Pages/HeroCatalog.razor:5` — mesmo padrão ("Hero Catalog | BeeDay").
- A maioria dos 111 documentos sob `docs/**/*.md`, `CLAUDE.md`, `README.md` — prosa técnica citando
  o nome do produto.
- `docs/epics/21-lingo-product-experience/color-inventory-sprint-21.13.csv` e documento
  companheiro — auditoria de cores já existente, input direto para a Sprint 25.3, não reauditada
  aqui.

**REQUIRES REVIEW** — não decidível apenas com evidência de repositório, fica para a Sprint 25.2:

- Qual das duas representações de wordmark (`BeeDayBrand` texto bicolor vs. PNG monocromático +
  mascote) é canônica — ou se ambas são intencionalmente diferentes por contexto (shell autenticado
  vs. shell público) e devem continuar assim.
- `alt=""` vs. `alt="BeeDay"` para a mesma imagem (`PublicHeader` vs. `AppFooter`) — decidir o
  contrato de nome acessível correto antes de tocar em qualquer um dos dois.
- `IdentityEmailComposer.cs` (`src/BeeDay.Infrastructure/Identity/`) hardcoda assunto/corpo dos
  e-mails transacionais em inglês, sem passar pelo mecanismo de localization (`IStringLocalizer` não
  é usado ali) — gap real, mas é tanto uma questão de completude de localization (EPIC 23) quanto de
  casing de marca; registrado para quem for dono da decisão, não presumido como escopo automático da
  Sprint 25.2.
- `HomeResources`: "How beeday works"/"Como o beeday funciona" (lowercase) vs. "BeeDay — Be better
  every day"/"...Seja melhor a cada dia" (Title Case) no mesmo arquivo — qual reflete a direção
  correta, ou se ambas convergem.
- Favicon (glifo "@" genérico) — dentro do escopo de convergência de identidade/wordmark, ou
  deliberadamente um "app icon" à parte.

### Achados confirmados adicionais

- `README.md` (raiz) declara "768 tests currently pass across five projects" — desatualizado (total
  real verificado nesta Sprint: 1.063). Documentação claramente stale encontrada durante a inspeção
  (Fase 1); não corrigida aqui — fora do escopo de Design System/Brand Contract, candidata para
  quem próximo tocar `README.md`.
- Diagnósticos de lint do editor (`MD029`/`MD060`) foram sinalizados nas edições de `CLAUDE.md` e
  `docs/README.md`/`docs/design-system/README.md` nesta Sprint — são o mesmo padrão estilístico já
  usado em todo o restante desses documentos (seções numeradas sem `#`, tabelas sem espaçamento
  "compact"), não uma regressão introduzida agora. Preservado para manter consistência com o
  restante do arquivo, conforme CLAUDE.md §4 ("respect existing... coding style").

### Validação final desta Sprint

```bash
git diff --check                                    # sem saída — sem problemas de whitespace/EOL
dotnet format BeeDay.slnx --verify-no-changes        # aprovado
dotnet build BeeDay.slnx                             # aprovado, 0 aviso(s), 0 erro(s)
dotnet test BeeDay.slnx                               # 1.063 testes, 1.062 aprovados, 1 falha (ver acima)
git status                                            # ver relatório da Sprint
```

Nenhuma mudança de código foi feita — apenas documentação. `dotnet ef migrations
has-pending-model-changes` não se aplica (nenhuma mudança de modelo).

### Confirmação de escopo

Nenhuma mudança visual ou funcional foi implementada. Nenhuma Sprint 25.2+ foi antecipada — o
inventário acima é apenas input registrado, nenhuma convergência de wordmark, cor, tipografia,
shape, motion, breakpoint, componente, Auth/Identity, Wallet, ProjectWorkspace, Character,
Illustration, Writing, visual regression, axe, ou limpeza ampla de documentação/CSS foi executada.
