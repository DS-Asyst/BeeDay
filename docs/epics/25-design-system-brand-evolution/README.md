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

**Última verificação:** 2026-08-16 (Sprint 25.3 — Color System Consolidation, COMPLETE — ver
"Sprint 25.3 — Results" ao final deste documento). Sprint 25.2 — Brand Identity & Wordmark
Convergence e Sprint 25.1 — Design System Governance & Brand Contract permanecem preservadas como
registros das Sprints anteriores.

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

---

## Sprint 25.2 — Brand Identity & Wordmark Convergence (Results)

**Branch:** `sprint/25.2-brand-identity-wordmark`, criada a partir de `hmg` em `a090182` (merge da
Sprint 25.1, confirmado via `gh pr view 126` — `state: MERGED`, `mergedAt: 2026-08-16T12:24:20Z` —
antes de criar a branch, conforme pré-condição da Sprint).

### Estado inicial

`hmg` continha exatamente as mudanças da Sprint 25.1 (fast-forward `d5bc5d9 → a090182`, diff de
fast-forward = os mesmos 4 arquivos do commit da 25.1). Nenhuma outra Sprint foi integrada a `hmg`
entre a 25.1 e o início desta Sprint. Working tree limpo confirmado antes da criação da branch.

### Inventário revalidado da 25.1

Revalidado contra o HEAD atual por leitura direta (não presumido do relatório da 25.1). Como
nenhuma mudança de código ocorreu entre a 25.1 e o início desta Sprint, os fatos de código do
inventário anterior permaneciam válidos, mas a revalidação encontrou **três correções relevantes**
ao inventário original:

1. **`beeday-wordmark.png` é um terceiro asset real**, não apenas uma menção hipotética — existe em
   `src/BeeDay.Web/wwwroot/beeday-wordmark.png` (raiz do `wwwroot`, não em `assets/brand/`), 904×276,
   confirmado por `docs/epics/21-lingo-product-experience/color-audit-sprint-21.13.md` (auditoria de
   pixel da Sprint 21.13). Sem nenhum consumidor real em `.razor`/`.razor.cs`/`.css` (busca
   repo-wide). O inventário da Sprint 25.1 mencionava "nenhum arquivo `beeday-wordmark.png`" citando
   apenas o prompt da própria Sprint 25.2 — a busca direta desta Sprint corrigiu isso: o arquivo
   existe, está órfão. Ver "Classificação dos assets de wordmark" abaixo.
2. **`docs/design-system/02-components.md` §8 e `01-foundations.md`** descreviam `BeeDayBrand` como
   um wordmark em imagem (`/beeday-wordmark.png`) — desatualizados desde antes desta Sprint (última
   verificação de `01-foundations.md` foi 2026-08-15, Sprint 22.2, um dia antes do início da EPIC
   25). A implementação real já era texto CSS (dois `<span>`) nessa data. `02-components.md` foi
   corrigido nesta Sprint (ver "Documentação atualizada"); `01-foundations.md` foi apenas registrado,
   não corrigido (a afirmação stale é sobre tipografia da marca, fora do escopo desta Sprint — ver
   "Itens DEFER").
3. **`PublicHeader.razor`'s `alt=""` não é uma inconsistência acidental** — a imagem está dentro de
   um `<a>` com `aria-label` próprio (`PublicHeaderHomeAriaLabel`); `alt=""` é o tratamento
   correto (decorativo, evita anúncio duplicado). A Sprint 25.1 registrou isso como possível
   inconsistência com o `alt="BeeDay"` do `AppFooter` (mesma imagem, sem link ao redor) — mantido
   como achado, mas reclassificado: são dois contextos de acessibilidade genuinamente diferentes, não
   um bug a "corrigir para bater."

Categorias reafirmadas sem mudança: TECHNICAL IDENTITY e HISTORICAL permanecem intocados; nenhuma
ocorrência nova nessas categorias foi encontrada nem alterada.

### Representação canônica escolhida

**`BeeDayBrand` permanece a primitive canônica do wordmark vivo** (nome técnico preservado por
backward compatibility, conforme o Brand Contract). Seu output visual convergiu para `beeday`
inteiro em `--beeday-color-brand-primary` (`#5247f9`), eliminando o tratamento bicolor anterior.

**O raster `assets/brand/beeday-top-navigation.png` (Header/Footer público) foi preservado como
está — não convergido para `<BeeDayBrand />`.** Decisão registrada como `PRESERVE` com impedimento
técnico documentado (a Sprint pede exatamente isso quando a convergência não é seguramente possível
— §2 do prompt): a imagem é um lockup composto (ilustração da abelha + wordmark, 866×288, tratamento
monocromático já em `#5247F9`/lowercase `beeday` — confirmado por inspeção visual direta do PNG na
Sprint 25.1) desenhado para a escala de identidade pública de página inteira; `BeeDayBrand` é uma
primitive de texto inline, dimensionada para chrome compacto (sidebar/auth card, nunca usada sozinha
como identidade hero). Substituir a imagem pelo componente:

- removeria a ilustração da abelha das duas superfícies públicas mais visíveis do produto (Header e
  Footer de toda página pública) — uma mudança de conteúdo pertencente ao Character & Illustration
  System (Sprint 25.13), não a esta Sprint de wordmark;
- exigiria um contrato novo de dimensionamento hero-scale para `BeeDayBrand` (nenhum hook de altura
  grande o suficiente existe hoje) — criação de capability nova, não convergência;
- violaria a instrução explícita desta Sprint de não redesenhar Header/Footer (§4 do prompt).

Como o PNG já satisfaz o Brand Contract (lowercase, cor oficial) sem nenhuma mudança, a convergência
segura e mínima foi: manter o asset, corrigir apenas a semântica acessível ao redor dele (ver
"Accessible names revisados"). Este par de decisões (`BeeDayBrand` = primitive canônica de texto;
raster público = lockup preservado, não uma segunda "primitive" concorrente, e sim uma composição de
marca para um contexto que `BeeDayBrand` não cobre hoje) fica registrado para a Sprint 25.13
(Character & Illustration System) decidir formalmente se/quando a ilustração da abelha ganha um
contrato de componente próprio.

### Alterações em `BeeDayBrand`

- `BeeDayBrand.razor` — `aria-label="BeeDay"` → `aria-label="beeday"`.
- `BeeDayBrand.razor.css` — `.beeday-brand__day` deixou de usar `--beeday-color-brand-yellow`; ambos
  os segmentos (`__bee`, `__day`) agora usam `--beeday-color-brand-primary`. O modificador
  `--inverse` (`OnDarkSurface`) também foi unificado: ambos os segmentos migram para
  `--beeday-color-text-inverse` em vez de só `__bee` (o `__day` amarelo, que tinha contraste próprio
  em superfícies escuras, deixaria de fazer sentido isolado depois da convergência para cor única no
  estado padrão). `OnDarkSurface` não tem nenhum consumidor real de produto hoje (confirmado por
  busca repo-wide) — mudança de baixo risco, mantém o contrato (`[Parameter] public bool
  OnDarkSurface`) intacto.
- Nenhuma mudança de markup (os dois `<span>` permanecem, `role="img"` preservado), nenhuma mudança
  de tipografia (nenhum `font-family` tocado), nenhum novo token criado — reusou
  `--beeday-color-brand-primary`/`--beeday-color-text-inverse`, ambos já existentes.

### Alterações em Header/Footer

- `AppFooter.razor` — `alt="BeeDay"` (hardcoded, não localizado) → `alt="beeday"` (hardcoded,
  casing apenas — não promovido a resource key nesta Sprint; ver "Itens DEFER").
- `PublicHeader.razor` — nenhuma mudança de markup (o `alt=""` da imagem está correto — ver
  "Representação canônica escolhida"); o `aria-label` do link `<a>` é resolvido via
  `SharedResources["PublicHeaderHomeAriaLabel"]`, corrigido na resource (ver abaixo).
- Nenhuma estrutura, espaçamento, link ou responsive behavior alterado em nenhum dos dois.

### Casing migrado por superfície

Confirmado por leitura direta de cada arquivo antes de editar, todas as strings abaixo convergidas
de `BeeDay`/`BEEDAY` (Title Case ou all-caps) para `beeday` (lowercase), preservando o texto ao
redor:

| Superfície | Chave(s) | en-US | pt-BR |
|---|---|---|---|
| `BeeDayBrand` | `aria-label` (hardcoded no `.razor`) | `BeeDay` → `beeday` | — |
| Header/Footer (`SharedResources`) | `ContinueToBeeDay`, `FooterLinksAriaLabel`, `FooterCopyright`, `PublicHeaderHomeAriaLabel` | 4 valores | 4 valores |
| `AppFooter.razor` | `alt` (hardcoded) | `BeeDay` → `beeday` | — |
| Navegação autenticada (`LayoutResources`) | `NavLogoutAriaLabel`, `BrandHomeAriaLabel` | 2 valores | 2 valores |
| Wallet (`WalletResources`) | `PageTitle` | 1 valor | 1 valor |
| Account (`AccountResources`) | `PageTitle`, `ProfileDescription`, `PreferencesDescription` | 3 valores | 3 valores |
| Profile Creation (`ProfileCreationResources`) | `PageTitle`, `StartYourJourney`, `IdentifiedInBeeDay`, `WelcomeToast`, `RedirectingToLoginTitle` | 5 valores | 5 valores |
| Home (`HomeResources`) | `PageTitle` (`StepsHeading` já era lowercase, não alterado) | 1 valor | 1 valor |
| Dashboard (`DashboardResources`) | `DailyPageTitle`, `ProfilePageTitle`, `WeeklyHistoryUnavailableDescription` | 3 valores | 3 valores |
| Onboarding (`OnboardingResources`) | `PageTitle`, `EnterBeeDayButton` | 2 valores | 2 valores |
| Authentication (`AuthenticationResources`) | `PageTitle` | 1 valor | 1 valor |
| Identity (`IdentityResources`) | `ForgotPasswordPageTitle`, `ResetPasswordPageTitle`, `ConfirmEmailPageTitle`, `ResendConfirmationPageTitle`, `EmailConfirmationSentPageTitle` | 5 valores | 5 valores |
| `Welcome.razor` | `<span>` hardcoded (tela transitória de redirect) | `BEEDAY` → `beeday` | — |
| E-mails transacionais (`IdentityEmailComposer.cs`) | assunto/corpo de confirmação e reset | 4 strings | — (inglês apenas, sem localization — ver "Itens DEFER") |
| `FromName` do remetente (`appsettings.json`, `.Production.json`) | `Resend:FromName` | `BeeDay` → `beeday` | — |

`EnterBeeDayButton` (`OnboardingResources`) — caso especial: valor era `ENTER BEEDAY`/`ENTRAR NO
BEEDAY`, all-caps, mesmo padrão de outras strings da mesma tela (`STEP {0} OF {1}`, `BACK`, também
all-caps, não tocadas — não representam a marca). Convergido para `ENTER beeday`/`ENTRAR NO beeday`
— a marca mantém lowercase mesmo dentro de um botão estilizado em maiúsculas (nenhum
`text-transform` CSS encontrado nesse botão — confirmado por leitura de `Tutorial.razor.css`; a
renderização muda de fato, não é só semântica).

**Preservado sem alteração (`TECHNICAL IDENTITY`):** todos os `namespace BeeDay.*`, `BeeDay.slnx`,
os 9 `.csproj`, nomes de componentes (`BeeDayButton`, `BeeDayCard`, `BeeDayIcon`, `BeeDayBrand`
propriamente dito, etc.), `BeeDayDbContext`/migrations, cookies (`BeeDay.Auth`, `BeeDay.Culture`),
configuration keys (`BeeDay:*`), `SetApplicationName("BeeDay")`, URLs reais do GitHub
(`github.com/tiagoarrigoni/BeeDay`), nomes de banco de dados de teste, `BEEDAY_DESIGNTIME_CONNECTION`
(variável de ambiente). **Preservado (`HISTORICAL`):** `docs/history/*`, `docs/adr/*`, seções de
Sprint já escritas em `docs/epics/20-*`/`docs/epics/21-*`. **Chaves de resource (nomes, não
valores)** preservadas em todos os casos — `ContinueToBeeDay`, `EnterBeeDayButton`,
`IdentifiedInBeeDay` etc. continuam com esses nomes (identificadores técnicos, convenção CONVENTIONS.md
§11: identificadores sempre em inglês, independente do idioma do texto).

### Resources/localization alterados

34 arquivos `.resx` (11 famílias × 3 culturas: `SharedResources`, `LayoutResources`,
`WalletResources`, `AccountResources`, `ProfileCreationResources`, `HomeResources`,
`DashboardResources`, `OnboardingResources`, `AuthenticationResources`, `IdentityResources` — 30
arquivos — mais o já contado acima). Paridade de chaves preservada em todos os arquivos (nenhuma
chave adicionada, removida ou renomeada — apenas valores). As outras 6 famílias de resources do
projeto (`ExperienceResources`, `ProjectResources`, `HabitResources`, `TaskResources`,
`TodoResources`, `DesignSystemResources`) foram verificadas e confirmadas **sem nenhuma ocorrência**
de `BeeDay` em valores — não precisaram de alteração (confirmado por busca `<value>...BeeDay...</value>`
em todos os `.resx` de `src/BeeDay.Web`, zero resultados após esta Sprint).

### Accessible names revisados

- `BeeDayBrand`: `aria-label` → `beeday` (lowercase, mesmo texto que os nós de texto visíveis
  "bee"+"day" concatenados).
- Header (`PublicHeader`): `aria-label` do link → "beeday home"/"página inicial do beeday"; `alt`
  da imagem permanece `""` (correto — decorativo dentro de link já nomeado).
- Footer (`AppFooter`): `alt` da imagem → `beeday` (único texto acessível para a marca nesse bloco,
  sem link ao redor).
- Navegação autenticada: `aria-label` de logout e do link de marca → `beeday` em ambas as culturas.
- Nenhum nome acessível foi duplicado ou removido; nenhuma mudança de landmark, foco ou ordem de
  tabulação.

### Classificação dos assets de wordmark

| Asset | Caminho | Status | Evidência |
|---|---|---|---|
| `beeday-top-navigation.png` | `wwwroot/assets/brand/` | **ACTIVE** | Consumido por `PublicHeader.razor` e `AppFooter.razor`; já lowercase/brand color, preservado sem alteração |
| `beeday-wordmark.png` | `wwwroot/` (raiz) | **LEGACY / UNCONSUMED** | 904×276, zero consumidores em `.razor`/`.razor.cs`/`.css` (busca repo-wide); documentado como "oficial" em `docs/design-system/{01-foundations,02-components}.md` até esta Sprint corrigir `02-components.md`. Registrado como candidato a remoção física — não removido aqui (Sprint 25.16 ou Sprint dedicada, conforme o padrão do repositório para exclusão de arquivo) |
| `favicon.png` | `wwwroot/` | **DEFER** | Ver "Decisão sobre favicon" abaixo |

### Decisão sobre favicon

`favicon.png` é um glifo genérico "@" (branco sobre gradiente azul/roxo) — não é a abelha, não é o
wordmark `beeday`, não usa nenhuma das duas cores de marca em tratamento reconhecível como tal.
Nenhum asset de favicon aprovado com a identidade `beeday`/abelha existe no repositório hoje.
Classificação: **`DEFER`** — não inventado um favicon novo (proibido pelo prompt desta Sprint), gap
registrado para decisão futura (candidato natural: Sprint 25.2 já está fechada; Sprint 25.13,
Character & Illustration System, ou uma Sprint de Brand Identity dedicada, quando houver asset
aprovado). Não bloqueou a convergência do wordmark.

### Documentação atualizada

- `docs/design-system/02-components.md` — §8 (`BeeDayBrand`) reescrito para descrever a
  implementação real (texto CSS, não imagem); cabeçalho "Última verificação" recebeu nova entrada
  registrando a correção e sua causa (drift pré-existente, não introduzido por esta Sprint).
- `docs/epics/25-design-system-brand-evolution/README.md` — este documento, seção "Sprint 25.2 —
  Results" (você está lendo).
- `docs/design-system/01-foundations.md` — **não alterado**, apenas registrado como stale (ver
  "Itens DEFER") — a afirmação incorreta é sobre origem da tipografia da marca, território da
  Sprint 25.4.
- `docs/epics/21-lingo-product-experience/README.md` e `color-audit-sprint-21.13.md` — **não
  alterados** (registro histórico congelado, CONVENTIONS.md — "não reescreva Epic antiga").

### Impacto arquitetural

Nenhum. Nenhuma camada, projeto, contrato público, dependência ou API de componente foi alterada —
`BeeDayBrand` manteve seu único parâmetro (`OnDarkSurface`) e sua estrutura de markup.

### Backward compatibility

Preservada. Nenhum nome de componente, namespace, resource key, cookie, configuration key ou
contrato de API mudou — apenas valores de string visíveis/acessíveis e dois arquivos de
configuração (`FromName`). Consumers de `BeeDayBrand` (11 localizações) continuam funcionando sem
nenhuma mudança de código — o componente é usado exatamente da mesma forma (`<BeeDayBrand />`,
mesmo parâmetro opcional).

### Ocorrências residuais de `BeeDay` classificadas

Busca final repo-wide (`src/`, `tests/`, quatro formas: `BeeDay`, `BEEDAY`, `Beeday`, `beeday`)
confirmou:

- **Zero** ocorrências de `<value>...BeeDay...</value>` remanescentes em qualquer `.resx` de
  `src/BeeDay.Web` (46 arquivos verificados).
- **Zero** ocorrências de `Beeday` (mixed-case) em `src/`.
- `BEEDAY` remanescente: apenas `BEEDAY_DESIGNTIME_CONNECTION` (variável de ambiente,
  `BeeDayDbContextFactory.cs`) — `TECHNICAL IDENTITY`, correto preservar.
- `BeeDay` remanescente em `src/`/`tests/`: exclusivamente `TECHNICAL IDENTITY` (namespaces,
  `.csproj`, `BeeDay.slnx`, nomes de componente, `BeeDayDbContext`, cookies, configuration keys,
  connection strings de teste, `ApplicationName` de teste) ou `HISTORICAL`/`DEVELOPMENT` já
  registrados na Sprint 25.1 e não tocados aqui (`IconCatalog.razor`/`HeroCatalog.razor` — DS
  catalog pages, DEFER mantido) — nenhuma ocorrência de marca visual ativa restante encontrada por
  acidente.

### Itens `DEFER`

- `docs/design-system/01-foundations.md` — afirmação stale sobre `beeday-wordmark.png` conter "a
  tipografia própria da marca" — não corrigida (território de tipografia, Sprint 25.4).
- `wwwroot/beeday-wordmark.png` — candidato a remoção física, não removido (Sprint 25.16 ou Sprint
  dedicada de limpeza de assets).
- `favicon.png` — sem decisão de substituição (nenhum asset aprovado existe).
- `IdentityEmailComposer.cs` — e-mails transacionais continuam apenas em inglês, sem passar pelo
  mecanismo de localization (`IStringLocalizer` não usado ali) — corrigido apenas o casing das
  strings existentes; completude de localization de e-mail é gap separado, não assumido como escopo
  desta Sprint.
- `AppFooter.razor`'s `alt="beeday"` continua hardcoded (não usa `@Localizer`, ao contrário do resto
  do mesmo arquivo) — oportunidade de promover a uma resource key própria, não feito aqui (mudança
  estritamente de casing, não de infraestrutura, conforme o escopo desta Sprint).
- `IconCatalog.razor`/`HeroCatalog.razor` — páginas de catálogo de desenvolvimento com `"...| BeeDay"`
  no título; categoria `DEVELOPMENT/DOCUMENTATION`, prioridade menor que superfícies de produto
  público — não convergidas nesta Sprint.
- Composição bee+wordmark do raster público (`beeday-top-navigation.png`) — decisão formal sobre se
  a ilustração da abelha ganha um contrato de componente próprio pertence à Sprint 25.13 (Character
  & Illustration System).
- Contagem desatualizada de componentes em `docs/design-system/README.md` (24 vs. 26 real) —
  registrado na Sprint 25.1, ainda não corrigido (Sprint 25.16 ou próxima Sprint que tocar o
  documento).

### Testes atualizados

17 arquivos de teste atualizados para refletir a nova regra de marca — nenhum teste removido,
nenhuma cobertura reduzida:

`BeeDayBrandTests.cs` (aria-label), `VisualFoundationTests.cs` (Web.Tests — conteúdo CSS de
`BeeDayBrand.razor.css`; E2E — aria-label e "Log out of beeday" via Playwright real),
`PublicHeaderTests.cs`, `AppFooterTests.cs`, `MobileHeaderTests.cs`, `DesktopSidebarTests.cs`,
`NavigationItemsTests.cs`, `TutorialTests.cs`, `HomeTests.cs` (Web.Tests),
`AuthenticatedCultureIntegrationTests.cs`, `DashboardLocalizationIntegrationTests.cs`,
`WalletLocalizationIntegrationTests.cs`, `IdentityFlowLocalizationIntegrationTests.cs`,
`IdentityInfrastructureTests.cs`, `ShellResponsiveLayoutTests.cs` (E2E), `AccountLifecycleTests.cs`
(E2E), `NavigationTests.cs` (E2E).

**Duas falhas reais encontradas e corrigidas dentro desta Sprint, antes do relatório final** (não
escondidas — ver "Resultado de `dotnet test`" abaixo para a execução que as expôs):

1. `AppFooterTests.UnderPortugueseUiCulture_RendersPortugueseLinksAriaLabelAndCopyright` — erro de
   cópia desta própria Sprint: o bloco pt-BR foi editado com o valor em inglês (`"beeday links"`) em
   vez do valor pt-BR (`"Links do beeday"`). Corrigido.
2. `VisualFoundationTests.SharedBrandPreservesLightAndInverseColorContracts` (Web.Tests) — teste não
   capturado pela busca inicial por `"BeeDay"` porque suas asserções já usavam `beeday` lowercase nos
   seletores CSS (`.beeday-brand__bee`/`.beeday-brand__day`) — o teste checava o CSS antigo
   (`--beeday-color-brand-yellow` em `__day`, regras separadas). Reescrito para verificar a nova
   estrutura (seletor combinado, cor única) e adicionada uma asserção `DoesNotContain` explícita para
   o token de cor removido.

### Resultado de `dotnet test` (execução final)

1.063 testes, **1.063 aprovados, 0 falhas** (Domain 93, Application 73, Infrastructure 129, E2E 65,
Web 703) — nenhuma flakiness de contenção reapareceu nesta execução (contraste com a Sprint 25.1,
onde `RateLimitingIntegrationTests` falhou uma vez sob contenção da suíte completa e passou isolado;
aqui a suíte completa passou 100% de primeira, na segunda tentativa desta Sprint).

### Validação final da Sprint 25.2

```bash
git diff --check                                    # sem saída — sem problemas de whitespace/EOL
dotnet format BeeDay.slnx --verify-no-changes        # aprovado
dotnet build BeeDay.slnx                             # aprovado, 0 aviso(s), 0 erro(s)
dotnet test BeeDay.slnx                              # 1.063 testes, 1.063 aprovados, 0 falhas
git status                                           # 57 arquivos modificados, ver relatório da Sprint
```

Todos os 48 arquivos `.resx` do projeto verificados como XML bem-formado após as edições
(`xml.etree.ElementTree`, zero erros de parsing).

### Confirmação de escopo — Sprint 25.2

Nenhuma Sprint 25.3+ foi antecipada. Color System, Coiny, Typography System, Shape Language,
spacing/radius, Motion, breakpoints, redesign de componentes, Auth/Identity forms convergence,
Wallet, Daily, ProjectWorkspace, Character System, Illustration System, Writing System,
`/brand/typography`, visual regression, axe, e limpeza ampla de CSS/documentação — nenhum desses foi
tocado. As únicas correções de documentação feitas (`02-components.md` §8) foram estritamente sobre
o componente que esta própria Sprint alterou, não uma varredura documental.

---

## Sprint 25.3 — Color System Consolidation (Results)

**Branch:** `sprint/25.3-color-system-consolidation`, iniciada em `eab4369`, mesmo commit de `hmg` e
`origin/hmg` após `git fetch origin --prune` (`0/0` de divergência). Nenhuma troca de branch, reset,
stash, commit, push ou PR ocorreu.

### Estado encontrado no handoff

O working tree continha cinco arquivos unstaged, nenhum staged e nenhum untracked. As 18 alterações
herdadas eram substituições sem mudança física de cor e foram classificadas `COHERENT PARTIAL
IMPLEMENTATION`:

- `BeeDayCardMenu.razor.css`: 3 aliases de Surface/Text Inverse em estados Danger;
- `ProjectContextFilter.razor.css`: 3 usos dos aliases Chrome/Filter já existentes;
- `ProjectWorkspace.razor.css`: 9 equivalências de branco para Surface/Text Inverse, sem
  convergência estrutural da Feature;
- `cards.css`: 2 usos de Text Inverse no status de Project;
- `design-system.css`: 1 uso de Text Inverse no icon toggle ativo.

Todas foram preservadas. Nenhum trabalho válido anterior foi revertido ou refeito.

### Baseline cromático revalidado

Metodologia reproduzível: custom properties com `color` no nome em `variables.css`; literals HEX/
RGB/HSL em todos os CSS runtime fora de `variables.css` e do excerpt vendor, normalizando casing e
HEX de três dígitos.

| Métrica | HEAD antes da 25.3 | Final | Interpretação |
|---|---:|---:|---|
| Color tokens nomeados | 117 | 121 | +4 Product/Reward aliases; nenhuma cor física nova |
| Literals fora da foundation | 123 | 105 | -18 equivalências herdadas, sem alteração visual |
| Valores únicos fora da foundation | 75 | 73 | duas duplicações físicas deixaram de ser hardcode |
| Literals em declarations da foundation | 100 | 96 | component aliases passaram a apontar para Surface/Content |
| Valores físicos únicos na foundation | 91 | 91 | paleta física inalterada |

### Taxonomia e ownership final

- **Brand:** `#5247F9` é a única Brand Color aprovada; hover/active/light/soft são states da mesma
  família.
- **Surface:** Background, Surface, Muted, Subtle e Overlay; aliases iguais continuam separados por
  responsabilidade.
- **Content:** Text Primary/Secondary/Muted/Inverse e Border/Strong/Interactive.
- **Semantic:** Success, Warning, Danger e Information; nenhum state foi criado só por simetria.
- **Product:** Reward/XP, Task, To-Do, Project, Attributes, Habits e Wallet tag default.
- **Illustration:** valores artísticos permanecem locais e não viram UI tokens automaticamente.
- **Component:** Button, Card e Dashboard chrome podem aliasar foundations quando isso explicita o
  contrato real.

`#335F71` continua com tokens separados para Information e Task: igualdade física não elimina
diferença semântica.

### Brand Yellow — decisão

`#FFD326` **não** foi promovido a segunda Brand Color. Os consumers reais são Reward/XP
(`ExperienceBar` e `BeeDayProgressBar` tone Reward), portanto a classificação pedida é `SEMANTIC /
COMPONENT`, com ownership de Product/Reward. Foram criados `--beeday-color-reward`, `-hover`,
`-active` e `-foreground` e os consumers migraram sem mudança visual. `--beeday-color-brand-yellow*` permanece como
`LEGACY / COMPATIBILITY`, aliasando Reward para não quebrar consumers externos ou indiretos.

### Component aliases e Buttons

Foregrounds Success, Danger e Reference Blue agora aliasam Text Inverse; Confirmation Cancel
background aliasa Surface. As oito variants públicas, enum, classes, values, hover/press/focus,
sizing e typography permanecem iguais. Danger e ConfirmationDanger continuam compartilhando a
mesma família. Reference Blue permanece modifier legado fora do enum, candidato a revisão futura.

### `BeeDayBrand.OnDarkSurface`

A busca repo-wide confirmou zero consumers reais; apenas componente e teste exercem o parâmetro.
O handoff exigia `beeday → #5247F9` em todos os modos, enquanto o CSS herdado da 25.2 ainda usava
Text Inverse no modo inverse. O parâmetro, a classe e o markup foram preservados por backward
compatibility, mas o modo agora mantém Brand Primary. Eventual contraste sobre uma surface escura
real deve ser avaliado quando existir consumer; sem background runtime associado hoje, a medição de
contraste é `N/A`. Não foi inventada `brand-white`/`brand-inverse`.

### Focus

O ring default continua derivado de Brand Primary (`rgb(82 71 249 / 32%)`). Focus inverse mantém o
valor físico `#FFD326` e seu ring de 45%, mas com ownership independente de Reward. Ambos permanecem
`RESERVED`, sem consumer runtime confirmado. Auditoria WCAG completa fica em `DEFER 25.15`.

### Classificação de Features e hardcodes preservados

- **Home:** `#464AFA`/`#4048F9`, `#D5EEFD` e brancos da composição são `ILLUSTRATION / DEFER
  25.13`; não representam o wordmark e nenhum redesign ocorreu.
- **Identity/Login:** feedbacks locais são Success/Danger por significado, mas usam valores físicos
  diferentes da foundation: `LEGACY / DEFER 25.9`, sem Forms/Auth convergence.
- **Wallet:** tag default, cores persistidas pelo usuário e constantes do contrast calculator são
  `PRODUCT-SPECIFIC`; nenhuma migração estrutural ocorreu (`DEFER 25.11`).
- **ProjectWorkspace:** 15 literals locais restantes são Feature/Product values ou `REQUIRES
  REVIEW`; somente os nove brancos exatamente equivalentes foram normalizados (`DEFER 25.12`).
- **Daily:** neutrals/shadows locais permanecem Feature/Component values até a convergência da
  25.12.
- **Overlays, shadows, alpha markers e provider/algorithmic values:** `LEGITIMATE LOCAL VALUE` ou
  `REQUIRES REVIEW`, preservados.

Os 105 literals restantes não foram tratados como falha da Sprint. Redução de hardcodes não foi
usada como métrica isolada.

### Tokens sem consumer estático

Overlay e semantic soft states foram preservados como `RESERVED`; Attributes como
`PRODUCT-SPECIFIC / RESERVED`; focus inverse como `RESERVED`; Brand Yellow como
`COMPATIBILITY`; tokens com possível uso dinâmico/indireto como `UNKNOWN / DEFER`. Nenhum token foi
removido por grep negativo; o sweep amplo pertence à 25.16.

### Arquivos e testes

13 arquivos modificados: `variables.css`; 8 CSS consumers (`BeeDayBrand`, `BeeDayProgressBar`,
`BeeDayCardMenu`, `ExperienceBar`, `ProjectContextFilter`, `ProjectWorkspace`, `cards.css`,
`design-system.css`); `VisualFoundationTests.cs`; `docs/design-system/01-foundations.md`;
`docs/design-system/02-components.md`; e este documento.

`VisualFoundationTests` foi atualizado para o Brand Primary em todos os modos e ganhou um novo test
guardando Reward ownership, compatibility aliases e aliases de Button. O total da solução passou de
1.063 para 1.064 testes.

### Impacto arquitetural e backward compatibility

Impacto restrito a Web foundations, CSS consumers, testes source-level e documentação. Domain,
Application, Infrastructure, persistence e contratos públicos não mudaram. `BeeDayBrand` preservou
parâmetro/markup; variants de Button foram preservadas; tokens Brand Yellow antigos continuam
resolvendo para os mesmos valores; nenhuma cor computada foi alterada fora da correção explícita de
`OnDarkSurface`, que não possui consumer runtime.

### Validação final

```bash
git diff --check
# aprovado, sem saída após normalização CRLF

dotnet format BeeDay.slnx --verify-no-changes
# aprovado

dotnet build BeeDay.slnx
# aprovado, 0 aviso(s), 0 erro(s)

dotnet test tests/BeeDay.Web.Tests/BeeDay.Web.Tests.csproj \
  --filter "FullyQualifiedName~VisualFoundationTests|FullyQualifiedName~BeeDayBrandTests"
# 9/9 aprovados

dotnet test BeeDay.slnx --no-build
# 1.064/1.064 aprovados: Domain 93, Application 73, Infrastructure 129, Web 704, E2E 65

git status
# 13 arquivos modificados, nenhum staged e nenhum untracked
```

Não houve falha, retry ou resultado flaky nesta Sprint; classificação de falhas: **nenhuma**. A
suíte completa incluiu todos os 65 E2E Chromium, cobrindo as superfícies críticas afetadas pelo CSS
compartilhado.

### Itens `DEFER` e confirmação de escopo

Identity/Login → 25.9; Wallet → 25.11; Daily/ProjectWorkspace → 25.12; Home/Illustration → 25.13;
WCAG/visual regression/axe → 25.15; remoção de tokens/assets/aliases legados → 25.16. Coiny,
typography, spacing, radius, borders estruturais, shadows/depth, motion, z-index, breakpoints,
responsive architecture, component API redesign, Writing System e qualquer rota `/brand/color`
não foram antecipados. A Sprint para aqui; 25.4 não foi iniciada.

## Sprint 25.4 — Typography System & Public Typography Guidelines (Results)

**Fonte da verdade:** inventário direto de `App.razor`, 54 arquivos CSS de produção,
`typography.css`, `typography-policy.css`, `BeeDayBrand`, Home/Footer, resources, testes Web/E2E e
documentação oficial do Google Fonts para Coiny. Executado em 2026-08-16 sobre o commit da Sprint
25.3 `b1a35b4665db1110c34545427b85e3ae56f595df`.

### Baseline e classificação

O baseline pré-alteração continha 400 declarations de `font-family`, `font-size`, `font-weight`,
`line-height`, `letter-spacing` e `text-transform` em 54 arquivos CSS, mais 31 `font` shorthands.
Os sete papéis compostos existentes já tinham consumers reais e foram `PRESERVE`; duplicações
locais em Cards, Wallet, Home, ProjectWorkspace e Dashboard foram `DEFER` para as Sprints de
convergência correspondentes. Nenhuma migração massiva foi feita.

### Decisão Coiny

Coiny foi `FORMALIZE` como Brand/Display oficial após os gates objetivos passarem:

- Google Fonts registra categoria Display, peso 400, licença SIL OFL 1.1 e subsets latin/latin-ext;
- a licença permite uso/embedding e nenhum binário foi adicionado ao repositório;
- a entrega reutiliza o mesmo Google Fonts já usado por Nunito, com `font-display: swap`;
- o subset latino WOFF2 observado em Chrome tinha 15.576 bytes;
- Chromium confirmou carregamento, glyphs acentuados pt-BR/en-US, fallback declarado, ausência de
  clipping, wrapping mobile, legibilidade nas escalas aprovadas e ausência de overflow.

Coiny ficou restrita a `--beeday-font-display`, `--beeday-type-brand-display`, `BeeDayBrand` e
composições com opt-in `.brand-display`. Nunito continua sendo a única família Product/UI.

### Papéis e página pública

Além dos sete contratos legados preservados, foram formalizados Brand Display, Hero, Page Title,
Section Title, Card Title, Caption e Eyebrow com aliases quando a expressão existente já era a
mesma. A nova `/brand/typography` é pública, anônima, responsiva, localizada para `en-US`/`pt-BR`,
usa `PublicLayout`, demonstra Coiny/Nunito ao vivo e documenta papéis, casing, uso correto e mau
uso sem expor detalhes internos desnecessários.

`Typography`/`Tipografia` aponta para a rota no Footer institucional e na coluna About/Sobre nós
da Home. A estrutura e a responsividade existentes foram preservadas.

### Impacto e compatibilidade

Impacto restrito à camada Web/presentation: fontes/tokens CSS, `BeeDayBrand`, nova página e
resources, Footer/Home, testes e documentação. Domain, Application, Infrastructure, persistência,
rotas existentes, parâmetros públicos de componentes e tokens compostos legados não mudaram.
Nenhuma página `/brand/color`, `/brand/characters` ou `/brand/writing` foi criada.

### Testes focados executados durante a implementação

```text
BeeDay.Web.Tests — TypographyGuidelines/AppFooter/Home/VisualFoundation: 21/21
BeeDay.E2E.Tests — BrandTypographyTests, 390 px e 1.280 px: 3/3
```

### Validação final

```text
git diff --check
# aprovado

dotnet format BeeDay.slnx --verify-no-changes
# aprovado

dotnet build BeeDay.slnx --configuration Release --warnaserror
# aprovado — 0 avisos, 0 erros

dotnet test BeeDay.slnx --configuration Release --no-build
# 1.070/1.070: Domain 93, Application 73, Infrastructure 129, Web 707, E2E 68
```

Não houve falha ou retry de teste. Os 68 E2E incluem os 65 cenários anteriores e 3 novos cenários
de tipografia pública em Chromium.

### Itens `DEFER` e confirmação de escopo

Shape/spacing/borders/depth → 25.5; motion → 25.6; breakpoints → 25.7; component APIs → 25.8;
Auth/Identity → 25.9; feedback/accessibility transversal → 25.10/25.15; Wallet → 25.11;
Daily/ProjectWorkspace → 25.12; Characters → 25.13; Writing → 25.14; sweep amplo → 25.16.
Nenhuma dessas Sprints foi antecipada.

## Sprint 25.5 — Shape, Spacing, Borders & Depth (Results)

**Fonte da verdade:** inventário direto dos 55 arquivos CSS de produção, `variables.css`,
`activity-design-system.css`, `polish.css`, shared components/layouts, Feature CSS, testes visuais e
documentação viva. Executado sobre o commit da Sprint 25.4
`79646f9a01a3387e8a8df63af2767dc639349285`.

### Baseline e classificação

O baseline pré-alteração tinha 628 declarations de spacing (167 tokenizadas), 99 radii (71
tokenizados), 159 borders (103 com algum token) e 92 declarations de shadow/filter (56
tokenizadas). Os valores foram classificados como Shared Scale, Legitimate Micro-Spacing,
Feature-Specific, Illustration/Composition ou Legacy/Candidate antes de qualquer migração.

### Consolidação sem mudança visual

- `--beeday-grid` passou a aliasar `--beeday-spacing-sm`;
- `--activity-space-{xs,sm,md,lg}` preservou a API feature-scoped e passou a aliasar a escala
  canônica equivalente;
- 21 expressões exatas de spacing em Button, Feedback, EditorModal, skeletons e layouts passaram a
  consumir tokens existentes; o total continuou 628, com 188 tokenizadas e 440 literais;
- `--beeday-border-width-subtle: 1px` formalizou boundaries estruturais comprovados por múltiplos
  consumers e migrou 20 usos compartilhados sem alteração computada;
- radii, physical depth e shadows já possuíam foundation suficiente; nenhum alias sem consumer ou
  nível por simetria foi criado.

### Product Shape Language

Controls/inputs e cards default usam radius `lg`; panels escolhem `md`/`lg` pela densidade; dialogs
usam `lg`; pills e círculos são reservados às silhuetas correspondentes; marketing/showcase pode
usar `2xl`, mantendo curvas artísticas locais. Borders de 1px delimitam estrutura; o contrato de
2px permanece para interação/content cards; Strong é diferença de cor; focus combina outline e
ring visível.

Button é o único shared component com bottom depth físico: 4px em repouso, colapso + deslocamento
de 4px no pressed. Shadows sutis separam menu/prominent surface; `md`/`lg` pertencem a feedback e
modal/overlay; Button/Card default permanecem sem shadow.

### Impacto, compatibilidade e escopo

Impacto restrito a CSS/Web, teste estrutural e documentação. Todos os valores computados foram
preservados; aliases feature-scoped, `--beeday-border-width`, radii e shadows existentes continuam
compatíveis. Micro-spacing, activity radii/shadows, Wallet, ProjectWorkspace, Home/illustration e
shadows locais foram preservados sob seus owners. Nenhum componente/API, motion, z-index,
breakpoint, typography ou Color System foi redesenhado.

### Validação

```text
git diff --check
# aprovado

dotnet format BeeDay.slnx --verify-no-changes
# aprovado após normalização CRLF restrita aos 18 arquivos alterados; a primeira execução detectou
# ENDOFLINE em VisualFoundationTests.cs introduzido pela edição local

dotnet build BeeDay.slnx --configuration Release --warnaserror
# aprovado, 0 avisos, 0 erros

dotnet test BeeDay.slnx --no-build --configuration Release
# 1.071/1.071 aprovados: Domain 93, Application 73, Infrastructure 129, Web 708, E2E 68

BeeDay.Web.Tests — VisualFoundation/DesignSystem/Progress/Card/Layout: 50/50
BeeDay.E2E.Tests — InteractiveComponentsTests/BrandTypographyTests: 5/5
```

### Itens `DEFER` e confirmação de escopo

Motion/z-index → 25.6; responsive/breakpoints → 25.7; component APIs → 25.8; Auth/Identity → 25.9;
feedback/a11y → 25.10; Wallet → 25.11; Daily/Project → 25.12; illustration → 25.13; Writing →
25.14; quality engineering → 25.15; final sweep → 25.16. Nenhuma foi antecipada.
