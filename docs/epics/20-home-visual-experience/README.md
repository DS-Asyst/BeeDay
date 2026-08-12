# EPIC 20 — BeeDay Home & Visual Experience

**Fonte da verdade:** decisões aprovadas no Decision Checkpoint desta EPIC (conversa entre o
responsável pelo repositório e Claude Code, 2026-08-11); descobertas verificadas diretamente no
repositório durante a Sprint 20.1 (branch `sprint/20.1-reference-home-ui-discovery`) — leitura de
`src/BeeDay.Web/`, `docs/design-system/`, `docs/web/`, `docs/ux/`, `docs/testing/`, execução real de
`dotnet format`/`dotnet build`/`dotnet test` (752 testes, 0 falhas), e inspeção direta do diretório
de referência visual local. Nenhuma afirmação de "estado atual" abaixo vem de memória — quando este
documento evoluir em Sprints futuras, cada atualização deve reverificar contra o código antes de
alterar uma afirmação de estado atual.

**Última verificação:** 2026-08-11 (Sprint 20.5 — BeeDay Home Structure, COMPLETE).

**Escopo:** evolução da experiência visual do BeeDay — primeira Home oficial, evolução do Design
System existente, Application Shell/navegação, remoção do cursor personalizado, responsividade e
acessibilidade. Não é um redesenho de arquitetura nem do CI/CD.

---

## Objective

Evoluir a experiência visual do BeeDay — criar a primeira Home oficial do produto, evoluir a
linguagem visual e o Design System existentes, melhorar o Application Shell e a navegação, e
remover estruturalmente o cursor personalizado — preservando integralmente a arquitetura, os
contratos públicos e o Design System já estabelecidos. A EPIC usa uma página-modelo externa como
referência de intenção visual, não como implementação a copiar.

## Source of Truth

- Arquitetura atual: [`docs/architecture/`](../../architecture/README.md) — Clean Architecture,
  `DomainAssemblyBoundaryTests`/`PersistenceContractBoundaryTests` como guardas reais de fronteira.
- Design System atual: [`docs/design-system/`](../../design-system/README.md) — 26 componentes sob
  `src/BeeDay.Web/Components/DesignSystem/`, sem projeto/assembly separado.
- UX/acessibilidade/responsividade atuais: [`docs/ux/`](../../ux/README.md).
- Implementação atual: `src/BeeDay.Web/` é a única fonte de verdade de comportamento — qualquer
  divergência entre este documento e o código deve ser resolvida a favor do código, reportada, e
  corrigida aqui.
- **Página-modelo:** referência visual, não implementação — ver seção seguinte.

## Visual Reference

```text
C:\Users\tiago\Downloads\BeeDay.Home-Nav-Tagline\BeeDay.Home
```

Projeto ASP.NET Core Razor Pages autônomo, usado exclusivamente como referência local de
desenvolvimento durante o discovery da Sprint 20.1 (identidade visual, paleta, tipografia,
hierarquia, composição de seções, navegação, footer, hero, motion). **Não é uma dependência de
build ou runtime do BeeDay** — não é referenciado por `BeeDay.slnx`, não é restaurado, compilado ou
publicado como parte do produto, e não deve se tornar uma. O próprio pacote nunca foi compilado
pelo autor original (ambiente sem SDK `dotnet`, conforme seu `README.md`).

## Sprint 20.1 Discovery Summary

Resumo dos fatos confirmados — relatório completo da Sprint 20.1 preservado no histórico da
conversa da EPIC, não duplicado aqui.

- Arquitetura Clean Architecture íntegra, sem violação encontrada; Design System vive dentro de
  `BeeDay.Web.csproj`, sem fronteira de assembly própria.
- Design System atual: 26 componentes, foundations em `variables.css`/`polish.css`/
  `activity-design-system.css` (3 escalas de spacing paralelas, 2 de radius/sombra paralelas),
  **nenhum token de breakpoint compartilhado** (29 valores hardcoded distintos).
- Rota `/` (`Entry.razor`) hoje **não é uma Home** — é um resolvedor silencioso de destino
  pós-autenticação (ver Decisão 1).
- Não existe hoje navegação/shell público — `MainLayout`/`TopNavigation` são exclusivamente
  autenticados; `OnboardingLayout` é deliberadamente mínimo, sem navegação.
- Cursor personalizado: implementação pequena e isolada — `wwwroot/css/cursors.css` +
  `wwwroot/cursors/{cursor-normal,cursor-click}.png`, sem nenhum JavaScript de rastreamento de
  mouse associado. Baixo risco de remoção.
- Página-modelo: tecnologia não relacionada ao BeeDay (Razor Pages estático), paleta e tipografia
  próprias que conflitam com os tokens reais do BeeDay (`--beeday-color-primary` roxo vs. azul/
  amarelo da referência; Inter/Jersey 25 vs. Nunito).
- Capacidades reais disponíveis para a futura Home (via `DashboardResponse`/
  `IDashboardReadService`, já consumido por `/daily`): perfil, XP/nível, Habits, Tasks, Projects
  (com Todos aninhados), resumo de Wallet — sem gap de Application para uma primeira versão.
  "Streak"/dias consecutivos: não confirmado. "% de consistência" agregado: gap confirmado.
- 752 testes (Domain/Application/Infrastructure/Web/E2E), 0 falhas, confirmados por execução real
  na Sprint 20.1 e novamente neste checkpoint.
- Nenhuma infraestrutura de regressão visual ou de acessibilidade automatizada (axe-core/Pa11y)
  existe hoje.

## Approved Decisions

### Decisão 1 — `/` será a Home pública oficial

**CURRENT STATE (confirmado na Sprint 20.1, verificado diretamente em
`src/BeeDay.Web/Components/Features/ProfileCreation/Pages/Entry.razor`):** `/` (`AllowAnonymous`,
layout `OnboardingLayout`) não é uma Home hoje — renderiza um estado de loading e resolve
silenciosamente o destino real do usuário:

```text
anonymous
→ /login

authenticated + no profile
→ /profile/create

authenticated + profile + onboarding incomplete
→ /onboarding/tutorial

authenticated + profile + onboarding complete
→ /daily
```

A mesma árvore de decisão (perfil → onboarding → destino) está implementada de forma independente
em três lugares: `Entry.razor`, `LoginDestinationResolver.Resolve` (pós-login,
`src/BeeDay.Web/Services/Authentication/LoginDestinationResolver.cs`) e
`CreateProfile.razor.cs` (pós-criação de perfil).

**APPROVED TARGET STATE:** `/` deverá futuramente representar a Home pública oficial do BeeDay.

**Restrição registrada junto com esta decisão:** a transformação futura de `/` não pode eliminar a
jornada autenticada existente (pós-login, criação de perfil, onboarding, retorno local seguro,
`/daily` como destino do usuário autenticado apto), nem introduzir uma quarta cópia independente
dessa árvore de decisão. Quando a mudança for implementada, deverá ser avaliada a menor refatoração
necessária para preservar essas três implementações como estão ou consolidá-las deliberadamente —
essa análise pertence à Sprint que implementar a mudança, não a este checkpoint.

**Esta decisão não autoriza, por si só:** alterar `Entry.razor`, alterar rotas, alterar redirects,
alterar autenticação ou onboarding, mover a lógica de resolução de destino, ou criar a Home. A
implementação ocorre em Sprint futura (prevista: 20.5).

### Decisão 2 — Sprint 20.4 incluirá o shell público

**CURRENT STATE (confirmado na Sprint 20.1):** `MainLayout`/`TopNavigation` são exclusivamente a
experiência autenticada; `OnboardingLayout` é deliberadamente mínimo (`<main>@Body</main>` +
`BeeDayToastHost`, sem nenhuma navegação). Não existe hoje nenhuma navegação/shell público
equivalente ao que a futura Home pública vai precisar.

**APPROVED TARGET STATE:** a Sprint 20.4 — Application Shell & Navigation incluirá a
criação/evolução do shell necessário para a Home pública, além de qualquer refinamento do shell
autenticado existente.

**Restrição registrada junto com esta decisão:** isso não autoriza um segundo Design System nem
uma navegação copiada literalmente da página-modelo. A implementação futura deve primeiro
reutilizar contratos existentes onde apropriado — `BeeDayBrand`, `BeeDayButton`, `PixelIcon`,
foundations, containers, comportamento de foco, motion, e demais componentes compartilhados — antes
de criar qualquer átomo novo. A composição em si (o arranjo do shell público) pode ser nova, porque
essa responsabilidade ainda não existe no repositório; os átomos e foundations não devem ser
duplicados.

**Sub-decisão explicitamente NÃO tomada agora:** não está decidido que o header público e
`TopNavigation` devam ser o mesmo componente. São contextos potencialmente diferentes (experiência
pública: Home, entrada/login quando apropriado, apresentação institucional; experiência autenticada
de produto: Daily, Wallet, Account, Activities, Projects). A Sprint 20.4 deverá analisar a
composição correta — sem forçar artificialmente um único componente para responsabilidades
distintas, e sem duplicar Brand/Button/Icon/tokens/comportamento de foco/semântica de espaçamento e
interação. Ver "Deferred Decisions".

### Decisão 3 — Documentação transversal da EPIC 20

**CURRENT STATE (confirmado na Sprint 20.1):** a EPIC 20 atravessa múltiplas áreas documentais
existentes (`web/`, `design-system/`, `ux/`, potencialmente `testing/`/`application/`), e
`docs/CONVENTIONS.md` não define uma convenção para um relatório transversal desse tipo — o único
precedente real (`docs/deployment/13-epic19-final-architecture-report.md`) existe porque a EPIC 19
inteira pertencia a uma única área.

**APPROVED TARGET STATE:** `docs/epics/20-home-visual-experience/` é a área documental oficial da
EPIC 20. Este documento (`README.md`) é sua implementação inicial. Decisão aplicada de forma
mínima — nenhuma reorganização de documentação existente, nenhuma migração de EPICs anteriores para
esse padrão, nenhuma estrutura documental genérica além desta pasta.

## Architecture Constraints

Válidas para toda a EPIC 20, sem exceção implícita:

- Preservar Clean Architecture e a direção de dependência (`Domain ← Application ← Infrastructure ←
  Web`).
- Domain permanece independente de Infrastructure/Web/persistência/UI.
- Application não recebe preocupações de UI; novas necessidades de dados da Home passam por
  contratos da Application Layer, nunca por acesso direto da Web a Infrastructure/persistência.
- Infrastructure permanece substituível.
- Web permanece a única camada responsável pela apresentação.
- O Design System existente (`Components/DesignSystem/`) deve ser **evoluído**, não substituído nem
  duplicado por uma segunda árvore de componentes.
- Evitar duplicação de foundations, componentes e navegação (ver "Risks").
- Preservar contratos públicos (rotas, layouts, autenticação) salvo decisão explícita em contrário —
  ver Decisão 1 para a única exceção já aprovada (`/`), ainda não implementada.

## Visual Adoption Principles

```text
reference
→ visual intention
→ existing BeeDay contract
→ reuse/refine/extend/create
→ BeeDay implementation
```

Nunca:

```text
reference HTML/CSS
→ copy into BeeDay
```

A página-modelo define principalmente como o BeeDay deve *parecer*; o repositório BeeDay continua
definindo como o produto deve ser *construído*.

## Visual Adoption Map — Key Results

Resultado completo produzido na Sprint 20.1 (preservado no histórico da conversa da EPIC). Decisões
estruturais mantidas rastreáveis aqui:

**REUSE** — já existe contrato adequado, sem alteração estrutural:

- `BeeDayHero` (existe, não usado por nenhuma página de produto hoje — candidato direto ao hero da
  Home);
- `BeeDayButton` (o efeito visual "pressionável" da referência já tem equivalente nas variantes
  `--comic`/`--skew-press`);
- `BeeDayCard`, quando adequado;
- `BeeDayBrand` (contrato de marca preferencial — ver "Brand duplication" em Risks);
- `AppFooter` como base reutilizável, quando apropriado ao contexto público;
- containers existentes (`.beeday-container`, `--beeday-reading-width`);
- Pixel Icon System (`PixelIcon`/`PixelIconRegistry`, sprite único);
- foundations existentes (cor, tipografia, radius, elevação, motion, focus).

**REJECT** — conflita com identidade, acessibilidade ou Design System já estabelecidos:

- importar a paleta azul/amarelo da página-modelo como nova identidade do BeeDay;
- introduzir "Nunito" como terceira família tipográfica (Inter/Jersey 25 são as famílias vigentes);
- utilizar métricas fictícias da página-modelo (ex.: "84% consistência", "21 hábitos concluídos");
- copiar HTML/CSS da referência diretamente;
- importar as lacunas de acessibilidade da referência (sem skip-link, sem guarda de
  `prefers-reduced-motion` no scroll-reveal).

**EXTEND / EVALUATE** — decisão adiada para a Sprint correspondente, não resolvida aqui:

- eventual escala tipográfica de display/hero (Sprint 20.2, somente se genuinamente necessária);
- composição de card de feature/pilar (Sprint 20.5/20.6);
- shell/navigation público (Sprint 20.4 — ver Decisão 2);
- motion de entrada (scroll-reveal), somente se houver justificativa de produto e suporte
  obrigatório a `prefers-reduced-motion` (Sprint 20.5/20.7);
- integração de conteúdo, somente com dados reais via Application (Sprint 20.6).

## Risks

Riscos ativos da EPIC, a serem verificados a cada Sprint antes de introduzir algo novo:

- **Foundation duplication** — já existem 3 escalas paralelas de spacing e 2 de radius/sombra
  (`variables.css`, `polish.css`, `activity-design-system.css`). A Sprint 20.2 não deve criar mais
  uma inadvertidamente.
- **Typography duplication** — Inter/Jersey 25 são as famílias vigentes; "Nunito" da referência não
  deve ser introduzida.
- **Button duplication** — o efeito visual "pressionável" da referência já tem equivalente no
  Design System (`BeeDayButton` + sombras "game").
- **Brand duplication** — `BeeDayBrand` deve ser o contrato de marca preferencial. **Correção
  (Sprint 20.4):** o achado original de texto literal residual `LEVEL`/`UP` em `TopNavigation`/
  `AccountSidePanel` estava desatualizado — verificado diretamente que ambos já renderizam `BEE`/
  `DAY` corretamente (corrigido em commits anteriores à EPIC 19). O que permanece é apenas
  estrutural: os dois componentes têm markup próprio em vez de delegar a `BeeDayBrand` — decisão
  deliberada nesta Sprint de não migrar (contexto de header escuro exige tratamento de cor diferente
  do padrão claro do componente compartilhado; migrar exigiria estender `BeeDayBrand` sem
  necessidade direta comprovada por este Sprint).
- **Breakpoint proliferation** — 29 valores de breakpoint hardcoded já existem; novos componentes
  não devem simplesmente somar mais um sem analisar os existentes.
- **Navigation duplication** — shell público e autenticado podem ter composições distintas (ver
  Decisão 2), mas devem reutilizar os mesmos contratos inferiores (Brand/Button/Icon/tokens/foco).
- **Route regression** — a transformação futura de `/` (Decisão 1) não pode quebrar login,
  criação de perfil, onboarding ou o destino `/daily`.
- **Application boundary** — dados futuros da Home (ex.: se "streak"/"% de consistência" forem
  aprovados) devem vir de um contrato apropriado da Application Layer, nunca de acesso direto da
  Web a Infrastructure/persistência.

## Sprint 20.2 — Visual Foundations Adoption (Audit Results)

**Última verificação:** 2026-08-11 (Sprint 20.2, branch `sprint/20.2-visual-foundations-adoption`).
**Fonte da verdade:** leitura direta de `src/BeeDay.Web/wwwroot/css/{variables,typography,
typography-policy,utilities,polish,design-system,activity-design-system}.css`, `BeeDayHero.razor`/
`.razor.cs`, busca repo-wide por `clamp(` e por consumidores de `--beeday-type-display`.

**Resultado: NO CHANGE.** Cada categoria de foundation avaliada abaixo já é suficiente para a
futura Home — nenhum arquivo de CSS/token foi alterado nesta Sprint.

| Categoria | Decisão | Evidência |
|---|---|---|
| Color | REUSE | Paleta de marca/status/atividade/botão/comic (`variables.css`) já cobre as necessidades identificadas; nenhum gap concreto comprovado |
| Typography | REUSE | `--beeday-type-display` (2.2rem, Inter) já existe para display numérico; `.beeday-hero__content h1` (`design-system.css:515`) já é fluido via `clamp(2rem, 5vw, 3.2rem)` e já herda Jersey 25 pela regra genérica `h1` de `typography-policy.css`; precedente adicional de headline oversized já existe página a página (`Entry.razor.css` `clamp(2rem,6vw,4rem)`, `Welcome.razor.css` `clamp(2.5rem,8vw,6rem)`, `Tutorial.razor.css` `clamp(2.8rem,8vw,4.7rem)`). Nenhum consumidor real (a Home ainda não existe) comprova que o teto atual de `BeeDayHero` é insuficiente — decisão é REUSE agora, reavaliar com evidência real na Sprint 20.5 se necessário |
| Spacing | REUSE / DEFER | Escala canônica = `variables.css` (9 degraus); `polish.css` (`--beeday-grid`, `--beeday-control-height-*`, `--beeday-page-gutter`, `--beeday-section-gap`) já são os aliases de ritmo de página reutilizáveis pela Home; `activity-design-system.css` permanece uma escala paralela pré-existente, escopada a cards de atividade — não consolidada nesta Sprint (fora de escopo) |
| Radius | REUSE | 6 degraus (`xs`→`pill`) já cobrem superfícies de card/botão; `--beeday-radius-xl` (1.25rem) representa semanticamente a intenção "superfície bem arredondada" da referência sem copiar o valor físico literal (26px) |
| Elevation / Game Shadows | REUSE | 4 degraus de elevação com blur + `--beeday-game-shadow-{sm,md,lg}` (offset sólido, sem blur) já implementam tanto a sombra suave de card quanto o efeito "botão pressionável" da referência |
| Buttons | REUSE | `BeeDayButton` + modificadores `--comic`/`--comic-press`/`--skew-press` (`design-system.css`, confirmado nesta Sprint) já implementam a linguagem visual pressionável observada na referência |
| Focus | NO CHANGE | `focus-visible` global e `--beeday-focus-outline`/`--beeday-focus-ring` preservados intactos — nenhuma alteração, nenhum risco de redução de visibilidade/contraste |
| Motion | NO CHANGE | Tokens de duração/easing existentes (padrão + "pixel") são semanticamente suficientes como placeholder; nenhum novo token criado sem consumidor aprovado; scroll-reveal/`IntersectionObserver` permanecem fora de escopo (pertencem à composição, Sprints 20.5/20.7) |
| Controls | REUSE | `--beeday-control-height-{sm,md,lg}` já reutilizável |
| Containers | REUSE | `.beeday-container` (`min(100% - 2rem, 1440px)`) e `--beeday-reading-width` (72rem, 100% abaixo de 60rem) já comportam a largura de shell da referência (~1140px) sem necessidade de token novo |
| Breakpoints | DEFER | 29 valores hardcoded confirmados (herdado da Sprint 20.1); nenhum preprocessor/build tooling de CSS existe no repositório (confirmado: sem `package.json`, sem PostCSS/Sass em `src/BeeDay.Web`) — um token de breakpoint compartilhado não pode ser usado dentro de condições `@media` em CSS puro sem ferramenta nova, que esta Sprint está proibida de introduzir. Estratégia registrada para quando a Home for composta: reutilizar um valor já estabelecido de uma família existente (ex.: a família `760px` já compartilhada por 4 arquivos de `Components/Layout/` para o corte "shell mobile", ou a família `672px`/`42rem` já compartilhada por `BeeDayPageHeader`/`BeeDayHero`/`polish.css` para o corte "cabeçalho estreito") em vez de introduzir um 30º valor arbitrário. Migração completa dos 29 valores permanece fora de escopo, deferida à 20.7 |
| Z-index / Layers | DEFER | Inversão pré-existente do token `--beeday-z-modal` não afetada; nenhuma foundation da Home depende disso ainda |

Nenhuma paleta paralela, terceira família tipográfica, escala paralela de spacing/radius/shadow ou
breakpoint arbitrário foi introduzido. Nenhum componente específico da Home foi criado. Nenhum
código da página-modelo foi copiado.

## Sprint 20.3 — Native Cursor & Global Visual Cleanup (Results)

**Última verificação:** 2026-08-11 (Sprint 20.3, branch `sprint/20.3-native-cursor-cleanup`).
**Fonte da verdade:** reauditoria repo-wide direta (`cursor`, `pointer*`, `mouse*`, `clientX/Y`,
`pageX/Y`) em `src/`, `tests/`, `docs/`, excluindo `bin`/`obj`; leitura direta de `cursors.css`,
`dragdrop.css`, `cards.css`, `App.razor`, `beeday-sortable.js`, `beeday-card-menu.js` antes e depois
da remoção; busca residual final confirmando zero ocorrências ativas.

**Resultado:** o cursor gráfico personalizado foi removido estruturalmente. O BeeDay usa cursores
nativos do navegador/sistema operacional em toda a aplicação.

- **Removido:** `src/BeeDay.Web/wwwroot/css/cursors.css`;
  `src/BeeDay.Web/wwwroot/cursors/cursor-normal.png`;
  `src/BeeDay.Web/wwwroot/cursors/cursor-click.png` (pasta ficou vazia e foi removida junto);
  a linha `<link rel="stylesheet" href="@Assets["css/cursors.css"]" />` em `Components/App.razor`
  (ordem e carregamento dos demais 18 stylesheets preservados, sem reorganização).
- **Semântica nativa preservada, movida para os stylesheets já proprietários dos seletores** (sem
  novo stylesheet global de cursor, sem override artificial):
  - `cursor: grab`/`grabbing` para `.beeday-sortable__item` (e o corpo aberto do card dentro dele)
    — movido para `dragdrop.css`, que já possuía os comentários apontando essa
    responsabilidade e as demais regras não-cursor do sortable;
  - `cursor: pointer` para `.activity-card__body--openable`/`.habit-card__body--openable`
    (`role="button"`, corpo clicável de card fora do sortable) — adicionado em `cards.css`, que já
    possui esses seletores.
  - `cursor: not-allowed` em `BeeDayButton:disabled` (`design-system.css`) e
    `.beeday-field__control:disabled`/`[readonly]` (`forms.css`) já existiam de forma independente
    de `cursors.css` — confirmado, não precisaram de nenhuma alteração.
- **Lógica de pointer/mouse funcional preservada, intocada:** `beeday-sortable.js`
  (`pointerdown`/`pointermove`/`clientX`/`clientY` — mecanismo de reordenação em si) e
  `beeday-card-menu.js` (`pointerdown` — detecção de clique fora do menu) — confirmados como não
  relacionados ao cursor gráfico, nenhuma linha alterada.
- **Nenhum override global artificial criado** (ex.: `cursor: auto !important` em `body *`) — o
  objetivo foi deixar o navegador voltar ao comportamento padrão, não escondê-lo.
- Documentação sincronizada: `docs/ux/02-accessibility.md` §8 (achado atualizado de "tensão ativa"
  para "removido, histórico"), `docs/web/05-design-system-integration.md` (ordem de CSS),
  `docs/design-system/{README,01-foundations}.md` e `docs/ux/03-responsive.md` (contagem de folhas
  globais corrigida de 20 para 19 — `cursors.css` não tinha `@media` próprio, os 29 breakpoints
  permanecem inalterados), `css/vendor/NES_ATTRIBUTION.md`/`nes-core.beeday-excerpt.css` (comentário
  de proveniência que citava `cursors.css` como dono da estilização de cursor).
- 752 testes continuam passando; nenhum teste existente asserta sobre `cursors.css`/`<link>`, nenhum
  teste artificial foi criado.

## Sprint 20.4 — Application Shell & Navigation (Results)

**Última verificação:** 2026-08-11 (Sprint 20.4, branch `sprint/20.4-application-shell-navigation`).
**Fonte da verdade:** leitura direta de `MainLayout.razor(.css)`, `OnboardingLayout.razor(.css)`,
`TopNavigation.razor(.css)`, `AccountSidePanel.razor`, `AppFooter.razor(.css)`,
`BeeDayBrand.razor(.css)`, `BeeDayButton.razor(.cs)`, `Routes.razor`, `_Imports.razor`, todas as
rotas `@page`/`@layout` atuais (`grep` direto, não a documentação), `polish.css`; execução real de
`dotnet build`/`dotnet test`.

**Arquitetura escolhida:** um novo layout separado, `PublicLayout` (opção C do §12 do prompt da
Sprint), **não aplicado a nenhuma rota nesta Sprint** — `OnboardingLayout` permanece
byte-a-byte preservado, sem nenhuma alteração.

- **Por quê:** os 9 consumidores atuais de `OnboardingLayout` (`/`, `/welcome`, `/login`,
  `/profile/create`, as 5 rotas de `Identity`, `/onboarding/tutorial`) são, com exceção de `/`, fluxos
  autocontidos de card centralizado que já injetam seu próprio `BeeDayBrand` individualmente —
  confirmado por leitura direta de todos os 9 arquivos. Aplicar um header/nav público persistente a
  eles introduziria navegação onde hoje não existe por design (foco no formulário, sem distração),
  contrariando o comportamento atual sem necessidade comprovada. Apenas `/` se tornará a Home
  pública (Decisão 1 do checkpoint) — e essa mudança é explicitamente da Sprint 20.5, não desta.
- **Alternativas rejeitadas:** (A) substituir `OnboardingLayout` inteiro pelo shell público — rejeitada,
  quebraria as 8 páginas de auth/onboarding sem necessidade; (B) `OnboardingLayout` incorporar o
  shell opcionalmente via parâmetro — rejeitada, adicionaria uma ramificação condicional a um layout
  hoje deliberadamente mínimo, sem um segundo consumidor real ainda (viola §18, "só extraia quando
  houver pelo menos dois consumidores reais").

## Public Shell Implementation

Dois componentes novos em `src/BeeDay.Web/Components/Layout/` (mesma pasta/convenção dos layouts
existentes, CSS isolado via `.razor.css` como todo o resto da pasta):

- **`PublicLayout.razor`** — `<PublicHeader />` + `<main class="beeday-main">@Body</main>` +
  `<AppFooter />` + `<BeeDayToastHost />`. Define `--beeday-top-navigation-height: 3.75rem` e
  `padding-top` no wrapper — mesmo contrato de nome/valor que `MainLayout.razor.css` já usa para
  compensar seu header fixo (reaproveitado, não duplicado; `polish.css`'s
  `scroll-padding-top: calc(var(--beeday-top-navigation-height, 3.75rem) + ...)` também se beneficia
  automaticamente).
- **`PublicHeader.razor`** — `<header class="public-header">` fixo (`position: fixed; z-index: 100`,
  mesmo padrão de `TopNavigation.razor.css`) contendo: link de marca (`<BeeDayBrand />` sem
  modificação, envolto em `<a href="/" aria-label="BeeDay home">`) e um único CTA reativo ao estado
  de autenticação via `<AuthorizeView>` nativo do Blazor (`Log in` → `/login` para anônimo; `Go to
  Daily` → `/daily` para autenticado).

**Nenhuma rota foi alterada para usar esses componentes** — validação apenas via testes de
componente (bUnit), conforme §25 do prompt da Sprint ("catálogo existente; teste de componente...
não uma nova route pública artificial"). `/` continua exatamente como a Sprint 20.1 a documentou.

## Reused Design System Contracts

| Contrato | Uso | Alteração necessária |
|---|---|---|
| `BeeDayBrand` | Marca do `PublicHeader` | Nenhuma — API já suficiente (componente sem parâmetros, cor `--beeday-color-primary` já legível sobre o fundo claro escolhido para o header público) |
| `BeeDayButton` | CTA do `PublicHeader` (`Compact`, `OnClick`) | Nenhuma — mesmo padrão já usado por `Tutorial.razor` para navegar via `NavigationManager.NavigateTo` a partir de um clique |
| `AppFooter` | Footer do `PublicLayout` | Nenhuma — conteúdo já genérico o suficiente (nenhum link/ação exclusivo de usuário autenticado) |
| `BeeDayToastHost` | Toast global do `PublicLayout` | Nenhuma — mesmo uso de `MainLayout`/`OnboardingLayout` |
| `.beeday-container` | Largura do conteúdo interno do header | Nenhuma |
| `.beeday-main` (utilitário global de `polish.css`) | `<main>` do `PublicLayout` | Nenhuma |
| `--beeday-top-navigation-height`, `--beeday-shadow-sm`, `--beeday-color-surface`/`-border`, foco global (`theme.css`/`polish.css`) | Estrutura/elevação/foco do `PublicHeader` | Nenhuma |
| `<AuthorizeView>` (Blazor nativo, já importado globalmente via `_Imports.razor`) | CTA reativo a auth | Nenhuma — primeiro uso deste componente nativo no repositório, mas não é um contrato do BeeDay a preservar/estender, é infraestrutura do próprio framework |

**Nenhum componente/token novo foi criado no Design System.** `CREATE` só foi usado para a
*composição* (`PublicHeader`/`PublicLayout` em si, responsabilidade que genuinamente não existia).

## Public Navigation Behavior

- **Decisão deliberada:** o `PublicHeader` **não tem lista de links de navegação nesta Sprint** —
  apenas marca + 1 CTA. Não existem destinos reais para linkar ainda (a Home não tem seções — Sprint
  20.5/20.6) e o prompt da Sprint proíbe explicitamente inventar âncoras para conteúdo inexistente
  (§10). Consequência: **o gap de mobile nav que a Sprint 20.1 encontrou em `TopNavigation`
  (`.top-navigation__links { display: none }` em 680px, sem alternativa) não pode se repetir aqui —
  não há lista de links para esconder.**
- Comportamento responsivo real: `flex` com `justify-content: space-between` mantém marca (esquerda)
  e CTA (direita) sempre visíveis e alcançáveis em qualquer largura testada — nenhum elemento essencial
  recebe `display: none`. Único ajuste em viewport estreita: `min-height` do header reduz de 3.75rem
  para 3.25rem no breakpoint `42rem` (ver §Breakpoints abaixo).
- Quando a Sprint 20.5/20.6 introduzir seções reais da Home com âncoras, a estratégia mobile
  (disclosure/drawer vs. links reorganizados) deverá ser decidida então, com conteúdo real — não
  antecipada aqui.

## Authentication-Aware Behavior

CTA usa `<AuthorizeView>` (cascata de `Routes.razor`'s `<CascadingAuthenticationState>`, já
disponível em toda a árvore) — **dois estados apenas, sem replicar a árvore
perfil→onboarding→destino**:

- Anônimo → `Log in` → `Navigation.NavigateTo("/login")`.
- Autenticado → `Go to Daily` → `Navigation.NavigateTo("/daily")`.

**Gap registrado, não resolvido (deliberado):** ao contrário de `LoginDestinationResolver.Resolve`
(que decide entre `/profile/create`, `/onboarding/tutorial` ou `/daily` conforme perfil/onboarding),
o CTA autenticado do `PublicHeader` sempre aponta para `/daily`, mesmo que o usuário não tenha perfil
ou onboarding completo. Isso **não é a quarta cópia da árvore de decisão** — é uma regra
deliberadamente mais simples ("autenticado → oferecer entrada no produto"), não uma tentativa de
replicar a mesma lógica. Sem impacto em runtime nesta Sprint (`PublicHeader` não está montado em
nenhuma rota real ainda). Antes da Sprint 20.5 montar `PublicLayout` em `/`, esta decisão deve ser
revisitada: usar `LoginDestinationResolver.Resolve(...)` (reutilizável, é `static`) ou aceitar o
comportamento simplificado. Ver "Deferred Decisions".

## Onboarding Compatibility

Impacto: **zero.** `OnboardingLayout.razor` e as 9 páginas que o usam (Login, as 5 de Identity,
Tutorial, CreateProfile, Entry) não foram tocadas — nenhuma linha alterada. Confirmado por
`git diff` vazio para esses arquivos.

## Authenticated Shell Impact

Impacto: **zero.** `MainLayout.razor`/`TopNavigation.razor`/`AccountSidePanel.razor` não foram
alterados. Avaliado deliberadamente (§14 do prompt) se a marca residual justificava uma correção
direta — não justificava, porque **o achado estava desatualizado**: `TopNavigation`/
`AccountSidePanel` já renderizam `BEE`/`DAY` corretamente (ver correção em
[`docs/web/README.md`](../../web/README.md#achados-relevantes-reportados-não-corrigidos) e
[`docs/web/03-layouts.md`](../../web/03-layouts.md)). O único ponto real remanescente — não
delegarem a `BeeDayBrand` — é estrutural, não um bug, e não foi alterado por não haver necessidade
direta desta Sprint.

## Accessibility (Sprint 20.4)

- `<header>` como landmark único por página (nunca coexiste com o `<header>` de `TopNavigation`,
  pois pertencem a layouts mutuamente exclusivos) — sem `aria-label` redundante, mesmo padrão de
  `TopNavigation.razor`.
- `<nav>` **deliberadamente omitido** — não existe lista de navegação real ainda; um `<nav>` vazio
  seria pior prática que a ausência do landmark.
- Marca: link real (`<a href="/">`) com `aria-label="BeeDay home"` (padrão comum de "logo linka para
  home"), conteúdo via `BeeDayBrand` (que já tem seu próprio `aria-label="Bee Day"`).
- CTA: `<button>` real (via `BeeDayButton`), não `div role="button"` — navegação por clique, mesmo
  padrão já usado pelo botão "ENTER DAILY" de `Tutorial.razor` (`BeeDayButton` + `OnClick` →
  `Navigation.NavigateTo`).
- `focus-visible`, `scroll-margin` e o anel de foco (`--beeday-focus-ring`) aplicam-se automaticamente
  aos dois elementos focáveis (link e botão) via as regras globais já existentes em
  `theme.css`/`polish.css` — nenhum CSS de foco novo foi necessário.
- `prefers-reduced-motion`: nenhuma transição/animação nova foi introduzida no CSS do
  `PublicHeader`/`PublicLayout` — nada para guardar.
- Nenhum `div role="button"`/`onclick` sem semântica foi usado em lugar nenhum do novo código.

## Responsive Behavior (Sprint 20.4)

Breakpoint reutilizado: **`42rem` (672px)** — a mesma família já compartilhada por
`BeeDayPageHeader`/`BeeDayHero`/`polish.css` para o corte "cabeçalho estreito" (`polish.css:88`),
não um valor novo. Nenhuma migração de breakpoints existentes foi feita; nenhum 30º valor foi
introduzido.

## Deferred Decisions (atualização Sprint 20.4)

- Se `TopNavigation`/`AccountSidePanel` devem migrar para `BeeDayBrand` com uma variante de cor para
  header escuro — avaliado, não decidido (baixa prioridade, não é mais um bug).

**Resolvida na Sprint 20.5:** o CTA autenticado do `PublicHeader` passou a reutilizar
`LoginDestinationResolver.Resolve(...)` via `AuthenticatedEntryDestinationResolver` — ver abaixo.

## Sprint 20.5 — BeeDay Home Structure (Results)

**Última verificação:** 2026-08-11 (Sprint 20.5, branch `sprint/20.5-beeday-home-structure`).
**Fonte da verdade:** leitura direta de `Entry.razor` (antes de removido), `LoginDestinationResolver`,
`CreateProfile.razor.cs`, `BeeDayHero`/`BeeDayCard`/`PixelIcon`/`PixelIconRegistry`,
`Tutorial.razor` (copy institucional estabelecida), `AuthorizationIntegrationTests.cs`,
`EntryFlowVisualConsistencyTests.cs`; execução real de `dotnet build`/`dotnet test`.

**Resultado:** `/` é agora a Home pública oficial do BeeDay — sem redirecionamento automático, para
visitante anônimo ou autenticado.

### `/` routing change

| | Antes (até Sprint 20.4) | Depois (Sprint 20.5) |
|---|---|---|
| Arquivo | `Features/ProfileCreation/Pages/Entry.razor` | `Features/Home/Pages/Home.razor` (nova área `Home`, 13ª de `Components/Features/`) |
| Layout | `OnboardingLayout` | `PublicLayout` (Sprint 20.4) |
| Comportamento | Loading state + redirect silencioso (`NavigateTo(..., forceLoad: true, replace: true)`) para `/login`, `/profile/create`, `/onboarding/tutorial` ou `/daily` conforme estado | Renderiza a Home diretamente para qualquer visitante — nenhum redirect |
| `@rendermode` | `InteractiveServer` explícito | Implícito (herdado de `App.razor`) |

`Entry.razor`/`Entry.razor.css` foram removidos — busca repo-wide confirmou zero outros
consumidores (nenhum teste, nenhuma outra página o referenciava por nome/tipo).

### Entry resolver refactoring — a política de destino foi preservada, não apagada

A árvore de decisão perfil → onboarding → `/daily` continua ativa exatamente onde já vivia:

- `LoginDestinationResolver.Resolve` (`Program.cs`, endpoint `/auth/login`) — inalterado.
- `CreateProfile.razor.cs` (pós-conclusão de perfil) — inalterado.

O terceiro consumidor histórico (a cópia inline dentro do próprio `Entry.razor`) foi **substituído,
não duplicado**: `Services/Authentication/AuthenticatedEntryDestinationResolver.cs` (novo, `Scoped`)
envolve `BeeDayWebService.GetCurrentUserAsync()` + `LoginDestinationResolver.Resolve(...)` — reusa a
regra existente em vez de reimplementá-la. Usado pelo CTA autenticado de `PublicHeader` e de
`Home.razor` (ver "Authentication-Aware Behavior" abaixo). **Nenhuma quarta implementação da árvore
foi criada.**

### Home architecture

- **Owner:** `Features/Home/Pages/Home.razor` (+ `Home.razor.css`, composição local — grid dos
  cards, ritmo vertical, `max-width` reaproveitando `--beeday-reading-width`).
- **Layout:** `PublicLayout` (Sprint 20.4) — `PublicHeader` + `@Body` + `AppFooter` +
  `BeeDayToastHost`, sem alteração.
- **Landmark:** um único `<article class="home-page">` como raiz (não um segundo `<main>` — ver
  "Deferred Findings"; `PublicLayout` já fornece o único `<main class="beeday-main">` da página).

### Visual reference translation

**Absorvido da referência:** composição hero→capabilities→progress (ritmo vertical de seções),
hierarquia eyebrow→título→subtítulo→CTA (via `BeeDayHero`, já existente), grid de cards de feature,
CTA como ação principal visível sem rolar.

**Rejeitado deliberadamente:** paleta azul/amarela e fonte Nunito da referência (usa
`--beeday-color-primary`/Inter/Jersey 25 do BeeDay); métricas fictícias da referência ("84%
consistência", "21 hábitos concluídos") — nenhum número aparece na Home; ilustração/imagem de hero
(nenhum asset apropriado existe — `BeeDayHero` sem `Illustration` é um padrão de primeira classe já
documentado no catálogo, não uma degradação); HTML/CSS/JS copiados — zero.

### Home sections

| Seção | Conteúdo | Justificativa de produto |
|---|---|---|
| Hero (`BeeDayHero`, variante `Default`) | Eyebrow "BEEDAY", título "Be better every day", subtítulo institucional, CTA | Apresentação imediata da marca e proposta, sem depender de dados |
| `#capabilities` | 5 `BeeDayCard`: Daily, Habits, Tasks, Projects, Wallet | As 5 capacidades reais confirmadas no discovery (README + `DashboardResponse`); texto de Daily/Habits/Tasks/Projects copiado quase literalmente de `Tutorial.razor` (terminologia já estabelecida); Wallet baseado em `README.md`/`HeroCatalog.razor` |
| `#progress` | Parágrafo institucional sobre XP/Level, sem números | Capacidade real confirmada (`UserProfileSummary.TotalExperience`/`CurrentLevel`), mas nenhum dado do visitante é buscado — texto puramente institucional |

**Character/Inventory não aparecem** — reconfirmado nesta Sprint que não existem como features reais
(nenhuma pasta `Components/Features/Character` ou `Inventory`; README explicita que os atributos de
atividade não são stats de personagem).

### Design System reuse

| Contrato | Uso | Alteração |
|---|---|---|
| `BeeDayHero` | Hero da Home, variante `Default`, sem `Illustration` | Nenhuma — primeiro consumidor de produto real (antes só usado no catálogo) |
| `BeeDayBrand` | Via `PublicHeader` (Sprint 20.4) | Nenhuma |
| `BeeDayButton` | CTA do Hero e do `PublicHeader` | Nenhuma |
| `BeeDayCard` (`Padded`) | 5 cards de capability | Nenhuma |
| `PixelIcon` | `Daily`, `Streak`, `RecurringTask`, `Project`, `Wallet` (todos já existentes no registry, `Decorative` padrão — cada card já tem `<h3>` como nome acessível) | Nenhuma |
| Foundations (`--beeday-reading-width`, `--beeday-spacing-*`, cores de texto) | `Home.razor.css` | Nenhuma — nenhum token novo |

**Nenhum `HomeHero`/`HomeFeatureCard` foi criado.**

### PublicHeader final behavior

`AuthorizeView` com dois ramos:

- **Anônimo:** "Log in" → `/login`.
- **Autenticado:** "Continue to BeeDay" → `AuthenticatedEntryDestinationResolver.ResolveAsync()` →
  `/profile/create` (sem perfil) | `/onboarding/tutorial` (onboarding incompleto) | `/daily`
  (pronto) — os 3 estados testados individualmente (ver Tests).

O mesmo padrão é usado pelo CTA principal da própria `Home.razor` (Hero `PrimaryAction`).

### Product capability accuracy

Toda capacidade apresentada foi confirmada como real antes de ser escrita:

- Daily/Habits/Tasks/Projects: texto copiado de `Tutorial.razor` (copy institucional já aprovada
  pelo produto, reutilizada verbatim/quase-verbatim).
- Wallet: confirmado em `README.md` ("Wallet: transactions, tags, filters...") e
  `HeroCatalog.razor`.
- XP/Level (seção Progress): confirmado em `UserProfileSummary`
  (`TotalExperience`/`CurrentLevel`/...) e no README ("experience curve... level-up feedback").
- Nenhum número/porcentagem/streak-count aparece — confirmado por teste
  (`HomeTests.DoesNotPresentFabricatedMetrics`).

### Navigation / anchors — reavaliado, mantido sem links

A Sprint 20.1 já havia decidido não adicionar links por falta de destinos reais; agora existem
(`#capabilities`, `#progress`). Reavaliado explicitamente e **mantida a decisão de não adicionar
links de âncora ao `PublicHeader` nesta Sprint**, por dois motivos: (1) para uma página de rolagem
única com apenas 2 seções, o valor de wayfinding de uma âncora é marginal — tudo está a um scroll de
distância; (2) adicionar 2+ links de texto ao `PublicHeader` exigiria uma estratégia de colapso
mobile real (o `PublicHeader` hoje só tem marca+1 CTA, que cabe em qualquer largura sem esconder
nada — adicionar links agora arriscaria reproduzir, apressadamente, o mesmo gap que a Sprint 20.1
encontrou em `TopNavigation`). As duas seções já têm `id` estável (`#capabilities`, `#progress`),
prontas para uma futura âncora sem exigir nova alteração da Home. Decisão registrada para
reavaliação na Sprint 20.6/20.7, quando o conteúdo da Home estiver mais consolidado.

### Responsive behavior

Grid de cards (`repeat(auto-fit, minmax(15rem, 1fr))`) reflui sem nenhum breakpoint — técnica CSS
fluida que dispensa `@media`. Único breakpoint usado: **`42rem` (672px)**, mesma família já
reutilizada pelo `PublicHeader` (Sprint 20.4)/`BeeDayPageHeader`/`BeeDayHero`. Nenhum valor novo.

### Accessibility

Um único `<h1>` (dentro de `BeeDayHero`), `<h2>` por seção, `<h3>` por card — hierarquia contínua.
`<article>` como raiz da página (não um segundo `<main>` — ver Deferred Findings sobre
`Account.razor`/`Wallet.razor`). Ícones decorativos (`Decorative` padrão do `PixelIcon`, cada card
já nomeado pelo `<h3>` adjacente). CTA como `<button>` real via `BeeDayButton`. `focus-visible`
herdado globalmente, nenhum CSS de foco novo. Nenhuma informação comunicada só por cor. Nenhum
motion novo introduzido (scroll reveal explicitamente fora de escopo).

### Tests

- `PublicHeaderTests.cs` atualizado: CTA autenticado agora testado nos 3 estados reais (sem perfil →
  `/profile/create`; onboarding incompleto → `/onboarding/tutorial`; pronto → `/daily`) via um
  `ISender` stub para `GetCurrentUserQuery`, não mais um destino fixo `/daily`.
- `PublicLayoutTests.cs` atualizado (precisa do resolver registrado, já que `PublicLayout` renderiza
  `PublicHeader`).
- `HomeTests.cs` (novo, 6 testes): `h1` único com a mensagem de marca; as 5 capacidades reais e
  somente elas; ausência de métricas fabricadas (regex `\d+\s*(%|day|days)`); CTA para anônimo;
  CTA para autenticado; IDs de âncora estáveis.
- `AuthorizationIntegrationTests.cs` — **sem alteração necessária**: `Anonymous_CanAccessPublicPage("/")`
  já validava 200 OK sem redirect HTTP (o antigo redirect de `Entry.razor` acontecia via SignalR
  após a resposta HTTP inicial, nunca como 3xx) — continua passando exatamente como antes.
- `EntryFlowVisualConsistencyTests.cs` — revisado, sem alteração necessária (escaneia apenas
  `Features/{Authentication,Identity,ProfileCreation,Onboarding}`; `Entry.razor` nunca usava
  `BeeDayBrand` e não fazia parte do conjunto de arquivos relevante para a asserção).

### E2E

Novo `HomeTests.cs` (`BeeDay.E2E.Tests`, 2 fluxos): visitante anônimo acessa `/` sem redirect e vê o
`h1` "Be better every day"; clique em "Get started" alcança `/login`. `AccountLifecycleTests.cs`
preservado sem alteração (todos os fluxos partem de `/login` diretamente, não de `/`).

## Deferred to Sprint 20.6 (Home Content & Product Integration)

- Dados reais/pessoais na Home (ex.: um resumo do progresso do próprio visitante autenticado) —
  deliberadamente não implementado; a Home atual é 100% institucional/estática.
- Investigar se um campo de "streak"/dias consecutivos existe ou vale a pena expor via Application.
- Decidir um "% de consistência" agregado, se aprovado (gap de Application já registrado desde a
  Sprint 20.1).
- Reavaliar estratégia de navegação por âncora no `PublicHeader` com o conteúdo da Home consolidado.

## Deferred to Sprint 20.7 (Responsive & Accessibility Pass)

- Auditoria transversal de responsividade/acessibilidade da Home (validação manual em navegador
  real — não executada nesta Sprint, sem ambiente disponível).
- Decisão sobre ferramenta de a11y automatizada (axe-core/Pa11y) e regressão visual — nenhuma
  introduzida nesta Sprint.

## Sprint Roadmap

```text
20.1 Reference Home & Current UI Discovery — COMPLETE

20.2 Visual Foundations Adoption — COMPLETE (NO CHANGE — existing foundations judged sufficient)

20.3 Native Cursor & Global Visual Cleanup — COMPLETE

20.4 Application Shell & Navigation — COMPLETE (PublicLayout/PublicHeader created, not yet wired to any route)

20.5 BeeDay Home Structure — COMPLETE (/ is now the public Home; Entry.razor removed; destination policy preserved and reused, not duplicated)

20.6 Home Content & Product Integration

20.7 Responsive & Accessibility Pass

20.8 Visual Consistency & Final Audit
```

Numeração não obriga artificialmente a implementação — se a análise real de uma Sprint revelar uma
fronteira tecnicamente inadequada, isso deve ser reportado antes de alterar o plano, não decidido
silenciosamente.

## Deferred Decisions

Apenas decisões que continuam genuinamente pendentes (as três decisões deste checkpoint **não**
permanecem aqui):

- Se o header/shell público (Decisão 2) será o mesmo componente de `TopNavigation` ou uma
  composição distinta que reutiliza os mesmos átomos — análise pertence à Sprint 20.4.
- Se existe (ou vale a pena adicionar via Application) um campo de "streak"/dias consecutivos por
  Habit, e se um "% de consistência" agregado será exposto — análise pertence à Sprint 20.6.
- Se a EPIC 20 introduz o primeiro token de breakpoint compartilhado just-in-time para os
  componentes novos, sem migrar os 29 valores existentes na mesma Sprint — análise pertence à
  Sprint 20.2/20.7.
- Se vale introduzir uma ferramenta de acessibilidade automatizada (axe-core/Pa11y) e/ou de
  regressão visual — nenhuma decisão tomada; nenhuma delas deve ser assumida como aprovada.
- Se `TopNavigation`/`AccountSidePanel` devem migrar para `BeeDayBrand` (com uma variante de cor
  para header escuro) em vez de manter markup próprio — avaliado na Sprint 20.4 e deliberadamente
  não decidido/implementado por não ser diretamente necessário ao shell público; permanece uma
  melhoria de reuso de baixa prioridade, não um bug (o texto de marca já está correto).

## Deferred Findings (pré-existentes, não corrigir nesta EPIC salvo decisão futura)

- Rota `/welcome` aparentemente morta (nenhum link de entrada encontrado no repositório).
- `wwwroot/css/feedback.css:20` — declaração `animation` sintaticamente inválida.
- Inversão de z-index: o token `--beeday-z-modal` (900) é menor que dois z-index literais de modal
  real (1200, 1400).
- Múltiplas escalas visuais paralelas (spacing, radius, sombra) já existentes antes da EPIC 20.
- Ausência de ferramenta automatizada de acessibilidade e de regressão visual automatizada.

**Resolvido (não era mais verdade, corrigido na Sprint 20.4):** o link para o repositório antigo
`github.com/tiagoarrigoni/LevelUp` em `AppFooter.razor` — verificado que já aponta para
`github.com/tiagoarrigoni/BeeDay`. Documentado aqui só para registrar que o achado original da
Sprint 20.1 estava desatualizado, não porque algo foi alterado nesta Sprint.
