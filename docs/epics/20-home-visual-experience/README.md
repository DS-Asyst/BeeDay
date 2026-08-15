# EPIC 20 — BeeDay Home & Visual Experience

**Fonte da verdade:** decisões aprovadas no Decision Checkpoint desta EPIC (conversa entre o
responsável pelo repositório e Claude Code, 2026-08-11); descobertas verificadas diretamente no
repositório durante a Sprint 20.1 (branch `sprint/20.1-reference-home-ui-discovery`) — leitura de
`src/BeeDay.Web/`, `docs/design-system/`, `docs/web/`, `docs/ux/`, `docs/testing/`, execução real de
`dotnet format`/`dotnet build`/`dotnet test` (752 testes, 0 falhas), e inspeção direta do diretório
de referência visual local. Nenhuma afirmação de "estado atual" abaixo vem de memória — quando este
documento evoluir em Sprints futuras, cada atualização deve reverificar contra o código antes de
alterar uma afirmação de estado atual.

**Última verificação:** 2026-08-12 (Sprint 20.8 — Full Visual Convergence, Responsive, Accessibility
& Final Audit, PARTIAL — Sprint final da EPIC 20. EPIC 20 status: READY FOR HMG VISUAL VALIDATION,
não COMPLETE — ver "Sprint 20.8 — Results" e "Sprint Roadmap").

**Escopo:** evolução da experiência visual do BeeDay — primeira Home oficial, migração do Design
System existente para o target visual da página-modelo, Application Shell/navegação, remoção do
cursor personalizado, responsividade e acessibilidade. Não é um redesenho de arquitetura nem do
CI/CD.

---

## Direction Change (Sprint 20.6) — Binding Decision

**Este registro não apaga o histórico anterior** — as seções abaixo (Objective, Visual Adoption
Principles, Sprint 20.1/20.2 results) permanecem como estavam, preservadas como registro do que foi
decidido e verificado em cada momento. Esta seção documenta a **evolução** da decisão.

1. A Sprint 20.5 entregou a estrutura funcional correta da Home (`/` pública, routing, CTA
   autenticado reutilizando `LoginDestinationResolver`).
2. A revisão visual pós-20.5 identificou desalinhamento: a Home absorveu pouco da composição da
   página-modelo e ficou visualmente próxima demais do Design System legado do BeeDay.
3. A interpretação da EPIC até a Sprint 20.5 tratava o Design System atual como *target* visual a
   preservar, e a página-modelo como inspiração secundária — **essa interpretação está revogada.**
4. **Nova decisão vinculante:** o repositório BeeDay continua fonte de verdade para arquitetura,
   Domain, Application, Infrastructure, contratos, funcionalidades, routing, autenticação,
   comportamento e dependency direction. **A página-modelo passa a ser a fonte de verdade visual de
   destino** — paleta, tipografia, escala, buttons, cards, navigation, containers, spacing, borders,
   radius, shadows, surfaces, hierarchy, composition, interaction states, responsive behavior.
5. A conclusão da Sprint 20.2 ("as foundations atuais são suficientes") **deixa de ser vinculante
   como decisão visual** — permanece válida apenas como documentação do baseline encontrado naquele
   momento (ver seção "Sprint 20.2", preservada sem edição abaixo).
6. A Sprint 20.6 inicia a migração do Design System usando a página-modelo como target, provada
   através da Home — ver "Sprint 20.6 — Results" abaixo.
7. **Correção registrada, não silenciosa, dentro da mesma Sprint:** a primeira passagem de
   implementação da Sprint 20.6 leu o conflito de escopo (impacto em consumidores existentes) como
   motivo para **não adotar** a cor primária e a família tipográfica de corpo da referência,
   mantendo `--beeday-color-primary`/Inter como target. O responsável pelo repositório corrigiu essa
   leitura: a decisão vinculante do ponto 4 já determina que a página-modelo é a fonte de verdade
   visual, incluindo paleta e tipografia — impacto em consumidores é um problema de **migração**
   (tokens canônicos novos + compatibilidade temporária + consumidores migrados nesta Sprint +
   consumidores deferred à 20.7), não uma justificativa para preservar o alvo visual anterior.
   Corrigido na mesma Sprint, sem abrir uma Sprint nova — ver "Color Migration"/"Typography
   Migration" abaixo para a estratégia efetivamente implementada.
8. **O que permanece deliberadamente não migrado, e por quê — agora por bloqueio técnico concreto,
   não por impacto amplo:** `--beeday-font-ui` (Jersey 25) continua reservado ao chrome
   pixel-console/retro-game do BeeDay (responsabilidade de marca real, documentada antes desta
   Sprint em `typography-policy.css`); o fundo do `PublicHeader` continua claro (`BeeDayBrand` não
   tem hoje uma variante de cor para fundo escuro — introduzi-la é uma mudança de componente
   compartilhado com escopo próprio). Ambos registrados como candidatos explícitos para a Sprint
   20.7, não como decisões pendentes de confirmação.

---

## Objective (histórico — Sprints 20.1–20.5)

Evoluir a experiência visual do BeeDay — criar a primeira Home oficial do produto, evoluir a
linguagem visual e o Design System existentes, melhorar o Application Shell e a navegação, e
remover estruturalmente o cursor personalizado — preservando integralmente a arquitetura, os
contratos públicos e o Design System já estabelecidos. A EPIC usa uma página-modelo externa como
referência de intenção visual, não como implementação a copiar.

**Superseded pela Sprint 20.6:** a cláusula "preservando integralmente... o Design System já
estabelecidos" não é mais a leitura vinculante — ver "Direction Change" acima.

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

**REJECT** (Sprint 20.1, ver "Direction Change" — os dois primeiros itens foram **revogados como
regra permanente** na Sprint 20.6; o restante permanece válido):

- ~~importar a paleta azul/amarelo da página-modelo como nova identidade do BeeDay~~ — revogado.
  **Sprint 20.6:** implementado — `--beeday-color-brand-primary` (`#2538d2`, literal da referência)
  é agora a cor canônica, consumida por Home/`PublicHeader`; `--beeday-color-primary` (roxo) mantida
  só como compatibilidade para consumidores ainda não migrados — ver "Color Migration";
- ~~introduzir "Nunito" como terceira família tipográfica~~ — revogado como regra de princípio.
  **Sprint 20.6:** implementado, mas como evolução de `--beeday-font-body` (Inter → Nunito), não
  como terceira família — `--beeday-font-ui` (Jersey 25) permanece a segunda família, reservada ao
  chrome pixel-console por responsabilidade de marca real e pré-documentada — ver "Typography
  Migration";
- utilizar métricas fictícias da página-modelo (ex.: "84% consistência", "21 hábitos concluídos") —
  **permanece válido**;
- copiar HTML/CSS da referência diretamente — **permanece válido**;
- importar as lacunas de acessibilidade da referência (sem skip-link, sem guarda de
  `prefers-reduced-motion` no scroll-reveal) — **permanece válido**.

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
- **Typography duplication** — Nunito substituiu Inter como `--beeday-font-body` (troca de valor,
  não uma terceira família); Jersey 25 (`--beeday-font-ui`) permanece a segunda família, por
  responsabilidade de marca real e já documentada antes da Sprint 20.6, não risco de duplicação —
  ver "Typography Migration".
- **Button duplication** — o efeito visual "pressionável" da referência já tem equivalente no
  Design System (`BeeDayButton` + sombras "game").
- **Brand duplication** — `BeeDayBrand` deve ser o contrato de marca preferencial. **Correção
  (Sprint 20.4):** o achado original de texto literal residual `LEVEL`/`UP` em `TopNavigation`/
  `AccountSidePanel` estava desatualizado — verificado diretamente que ambos já renderizam `BEE`/
  `DAY` corretamente (corrigido em commits anteriores à EPIC 19). **Resolvido estruturalmente
  (Sprint 20.7):** o único ponto real remanescente — os dois componentes tinham markup próprio em vez
  de delegar a `BeeDayBrand` — foi corrigido: `BeeDayBrand.razor.css` ganhou um hook reutilizável
  (`--beeday-brand-color`, custom property CSS opt-in, sem novo parâmetro Razor) que qualquer
  superfície escura pode usar para recolorir "BEE"; `TopNavigation`/`AccountSidePanel` agora renderizam
  `<BeeDayBrand />` diretamente. Risco encerrado.
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

## Sprint 20.6 — Reference Design System Extraction & Home Migration (Results)

**Última verificação:** 2026-08-12 (Sprint 20.6, branch `sprint/20.6-reference-design-system-home`) —
inclui uma correção de leitura da estratégia de cor/tipografia feita dentro da própria Sprint (ver
"Direction Change" pontos 7-8): a paleta e a família de corpo canônicas da referência foram
efetivamente migradas via tokens novos + compatibilidade temporária, não mais deixadas como
"decisão pendente".
**Fonte da verdade:** reinspeção direta de `Index.cshtml`/`site.css`/`_Layout.cshtml`/`site.js` da
página-modelo (incluindo uma segunda reinspeção de `site.css` durante a correção, para confirmar os
valores literais `#2538d2`/`#4458dc`/`#ffd326`/`#ffb72e`/Nunito); leitura direta de `variables.css`,
`typography.css`, `typography-policy.css`, `design-system.css`, `App.razor`,
`BeeDayButton.razor(.cs)`, `BeeDayCard.razor(.cs)`, `BeeDayBrand.razor(.css)`, `PixelIcon.razor.css`,
`PublicHeader.razor(.css)`, `PublicLayout.razor.css`, `Home.razor(.css)`; busca repo-wide de
consumidores de `--beeday-color-primary`/`--beeday-font-body`/`--beeday-font-ui`/`BeeDayBrand` antes
de alterar qualquer token; execução real de `dotnet format --verify-no-changes`/`dotnet build`/
`dotnet test` após cada rodada de mudanças.

### Reference Design System Extraction

| Category | Reference | Previous BeeDay | New BeeDay | Action |
|---|---|---|---|---|
| Primary color | `#2538d2`/`#4458dc` (blue) | `--beeday-color-primary` `#673ab7` (purple) | **Novo canônico:** `--beeday-color-brand-primary` `#2538d2` (+ `-hover`/`-active`/`-light`); `--beeday-color-primary` mantida, inalterada, como compat para consumidores não migrados | EVOLVE (novo token canônico + alias de compatibilidade temporário — ver "Color Migration") |
| Accent color | `#ffd326`/`#ffb72e` (yellow) | `--beeday-game-yellow` `#ffc928` | **Inalterado** — já próximo do target | REUSE (quase idêntico, sem token novo) |
| Page background | `#fff` | `--beeday-color-surface` `#fff` | Inalterado | REUSE |
| Text/ink | `#253143` | `--beeday-color-text-primary` `#2f2737` | Inalterado | REUSE |
| Muted text | `#687386` | `--beeday-color-text-secondary` `#514858` | Inalterado | REUSE |
| Border | `#e7eaf1` | `--beeday-color-border` `#d7d2da` | Inalterado | REUSE |
| Body font | Nunito | Inter | **Migrado (canônico, todo o produto):** `--beeday-font-body` agora `"Nunito"` | EVOLVE (troca de valor de token existente, ver "Typography Migration") |
| Display font | Nunito 900 | Jersey 25 (`--beeday-font-ui`) | Inalterado — Jersey 25 | KEEP (responsabilidade de marca real e documentada — ver "Typography Migration") |
| Display weight | 900 | extrabold 700 (teto anterior da escala) | **Novo:** `--beeday-font-weight-black: 900` | EXTEND (novo token, sistêmico) |
| Display scale | wordmark 88–168px / h1 42–66px | `.beeday-hero__content h1` clamp 32–51px | **Novo:** `--beeday-font-size-hero: clamp(2.75rem, 7vw, 5.5rem)` (44–88px) | EXTEND (novo token, sistêmico) |
| Radius (surfaces) | 26–36px | topo da escala = `xl` 20px, sem degrau intermediário até `pill` | **Novo:** `--beeday-radius-2xl: 1.75rem` (28px) | EXTEND (preenche gap real na escala) |
| Radius (buttons) | 15px | `.beeday-button` base = 3px (pixel/8-bit) | **Novo modificador opt-in:** `.beeday-button--soft` (`border-radius: pill`) | EXTEND (modificador, não redesenho do padrão default) |
| Shadow | `0 18px 60px rgba(30,45,120,.12)` | `--beeday-shadow-lg` `0 1.5rem 4rem rgba(12,6,22,.42)` | Inalterado (reutilizado via `--soft`) | REUSE |
| Container | 1140px | `--beeday-reading-width` 1152px (72rem) | Inalterado — já quase idêntico | REUSE |
| Buttons (shape) | radius 15px, sem borda, sombra 0 6px 0 (offset), hover lift | radius 3px, borda pixel 2px, sombra offset pixel, hover -1px, active +4px (press) | **Novo modificador `.beeday-button--soft`:** sem borda, radius pill, `--beeday-shadow-md`→`-lg` no hover, sem "press" pixel | EXTEND `BeeDayButton` (opt-in, zero impacto em 40+ consumidores existentes) |
| Cards | radius 26–36px, sem borda, sombra suave | `.beeday-card` radius `xs` (3.2px), borda 1px, sombra `sm` | **Novo modificador `.beeday-card--soft`:** sem borda, `radius-2xl`, `shadow-lg` | EXTEND `BeeDayCard` (opt-in) |
| Navigation (header) | altura 78px, fundo escuro, brand + nav + CTA outline | `PublicHeader` altura 60px, fundo claro, brand + 1 CTA | Altura ajustada para 68px (`4.25rem`); CTA usa `--soft`; marca "BEE" recolorida para `--beeday-color-brand-primary`; fundo permanece claro (ver "PublicHeader Migration") | REFINE (altura/CTA/cor da marca), fundo claro mantido por decisão explícita escopada |
| Breakpoints | 900px/600px | 29 valores hardcoded, sem token | Reutilizado `56.25rem`/900px (precedente já existente em `feedback.css`/Dashboard `Home.razor.css`) para colapso dos splits | REUSE (precedente existente, não um 30º valor arbitrário) |
| Motion (hero) | fade/slide on scroll (`IntersectionObserver`) | nenhum | **Não implementado** — fora de escopo desta Sprint (não pedido, risco de regressão de acessibilidade sem tratamento de `prefers-reduced-motion` próprio da referência) | OUT OF SCOPE (deliberado) |

### Color Migration

**Correção registrada (mesma Sprint):** a primeira passagem desta Sprint rejeitou a paleta azul da
referência por impacto em consumidores existentes. O responsável pelo repositório corrigiu essa
leitura: a decisão vinculante da EPIC 20 já determina que a página-modelo é a fonte de verdade
visual de destino — impacto em consumidores é um problema de **migração** a resolver (tokens
canônicos + compatibilidade temporária + migração faseada), não um motivo para preservar a paleta
anterior como target. Esta seção documenta a estratégia corrigida, efetivamente implementada.

**Decisão:** `--beeday-color-brand-primary` (`#2538d2`, extraído diretamente de `site.css` da
página-modelo) é adicionado como a cor primária **canônica** do Design System — a direção de marca
vinculante daqui para frente, com `-hover`/`-active`/`-light` completando a família.
`--beeday-color-primary` (`#673ab7`, roxo) **é mantida, com o mesmo valor de sempre**, como alias de
compatibilidade para os consumidores ainda não migrados. `--beeday-game-yellow` (`#ffc928`) não
ganhou um token novo — já está próximo o bastante do amarelo da referência (`#ffd326`/`#ffb72e`)
para ser reutilizado como está (extend, não duplicate).

**Por que essa estratégia, e não uma troca de valor direta:** `--beeday-color-primary` é consumido
por dezenas de pontos em todo o produto autenticado (variantes de `BeeDayButton`, anéis de foco,
badges, links, `TopNavigation`, `AccountSidePanel`, `ProfileSidePanel`, `DashboardColumn`,
`cards.css`, `wallet.css`, `feedback.css`, `forms.css`, `identity.css`, `pixel-ui.css`, `theme.css`,
`editor-modal.css`, `dragdrop.css` — confirmados nesta Sprint via busca repo-wide, ver "Existing
Consumer Compatibility"). Trocar o *valor* desse token recolore instantaneamente todos esses pontos
de uma vez — inclusive páginas fora do escopo desta Sprint (Daily, Wallet, Login, Onboarding).
Introduzir a cor canônica sob um **nome de token novo** (em vez de sobrescrever o valor do token
existente) permite que Home/PublicHeader consumam a direção final imediatamente, com **zero risco de
regressão** para os consumidores restantes, que continuam lendo o valor antigo sob o nome antigo até
serem migrados deliberadamente.

**Consumidores já migrados nesta Sprint** (primeiros consumidores reais da família canônica):

- `Home.razor.css` — gradiente do hero/CTA (`brand-primary` → `brand-primary-light`, mesma direção
  de gradiente clara→mais-clara que `site.css`'s `.section-blue`), eyebrows, ícones
  (`PixelIconColor.Primary`, recolorido via `::deep` escopado só ao DOM da Home).
- `PublicHeader.razor.css` — a metade "BEE" da marca (`::deep .beeday-brand`, escopado só ao DOM do
  header; os outros 8 consumidores de `BeeDayBrand` continuam roxo).

**Consumidores deferred para a Sprint 20.7** (continuam lendo `--beeday-color-primary`, valor
inalterado, zero regressão): `TopNavigation`, `AccountSidePanel`, `ProfileSidePanel`,
`DashboardColumn`, `MainLayout`, `OnboardingLayout` (Login/Identity/Onboarding/`BeeDayBrand` nos
outros 8 pontos), `Account.razor`, `Wallet.razor`, Dashboard (`/daily`) e todos os cards de
atividade/hábito.

### Typography Migration

**Correção registrada (mesma Sprint):** mesma correção de leitura do Color Migration — ver acima.

**Decisão:** `--beeday-font-body` evolui de `"Inter"` para `"Nunito"` — mudança de **valor**, não de
nome de token, aplicada imediatamente a todo o produto (toda UI regular já renderiza Nunito). Isso é
seguro como troca direta (diferente da cor) porque é uma substituição puramente tipográfica: nenhuma
regra CSS do repositório depende de métricas específicas de Inter, e Nunito é extraída diretamente
de `site.css`/`_Layout.cshtml` da página-modelo, onde é literalmente a única família usada em toda a
página (corpo e display). `Google Fonts` em `App.razor` foi atualizado com
`family=Nunito:wght@400;600;700;800;900`.

**`--beeday-font-ui` (Jersey 25) permanece — não é compatibilidade histórica, é responsabilidade de
marca real e já formalmente documentada:** `typography-policy.css` (132 linhas, pré-existente a esta
Sprint) é uma política tipográfica executável que reserva Jersey 25 exclusivamente ao chrome
pixel-console/retro-game do BeeDay — reforçada com `!important` para `.beeday-button` especificamente
("nenhuma variante/modificador/classe legada consiga renderizar o botão fora dessa família"),
`BeeDayBrand`, títulos de página/card, e o restante do `pixel-ui.css`. Migrar essa família para
Nunito removeria a identidade "8-bit"/pixel do BeeDay, deliberadamente construída ao longo de
múltiplas Sprints anteriores (Pixel Icon System, `pixel-ui.css`, paletas "comic" de `BeeDayButton`,
NES adapter) — não é o mesmo tipo de "impacto amplo por falta de estratégia" do Color Migration; é
uma responsabilidade de marca distinta e intencional, que a própria correção desta Sprint autoriza
manter quando documentada (ver `docs/design-system/01-foundations.md` §3).

**O que migrou como consequência direta:** o peso/escala de display — `--beeday-font-weight-black:
900` (novo token, iguala o peso 900 consistente da referência) e `--beeday-font-size-hero`
(pré-existente desta Sprint) agora aplicados sobre `--beeday-font-body` (Nunito), não mais sobre
`--beeday-font-ui` (Jersey 25) — a headline/eyebrow/CTA final da Home usam Nunito 900 em escala
hero, igualando a família E a presença da referência, não apenas a escala como na primeira passagem
desta Sprint.

**Consumidores deferred para a Sprint 20.7:** nenhum, para o corpo — `--beeday-font-body` já é
canônico e migrado em todo o produto (troca de token global, sem consumidores pendentes). Para o
display: nenhuma migração de `--beeday-font-ui` está planejada — permanece a família definitiva do
chrome pixel-console, não uma migração pendente.

### Spacing / Radius / Shadow / Container Migration

- **Spacing:** nenhuma escala global alterada; ritmo generoso do tipo "landing page" (a referência
  usa ~110px de padding vertical por seção) implementado via composição local em `Home.razor.css`
  (`clamp(3.5rem, 8vw, 6rem)` para o ritmo entre seções, `clamp(4rem, 10vw, 7rem)` para o hero) — CSS
  local de composição, não uma foundation nova (§23 do prompt: "section arrangement" é exemplo
  explícito de composição local).
- **Radius:** `--beeday-radius-2xl` adicionado (preenche um gap real: a escala saltava de `xl`
  20px direto para `pill` 999px, sem um degrau de "superfície generosa"). Consumido pelo novo
  modificador `.beeday-card--soft`.
- **Shadow:** nenhum token novo — `--beeday-shadow-md`/`-lg` já existentes (e já na faixa "suave e
  grande" pretendida pela referência) reutilizados pelos novos modificadores `--soft`.
- **Container:** `--beeday-reading-width` (1152px) já é quase idêntico ao `shell` da referência
  (1140px) — reutilizado sem alteração.

### Component Impact Analysis

Antes de alterar `BeeDayButton`/`BeeDayCard`, os consumidores existentes foram buscados
repo-wide: `BeeDayButton` tem 40+ pontos de uso confirmados (Sprints 16.7/20.1); `BeeDayCard` é
usado por páginas de catálogo e composições diversas. **Nenhum desses consumidores foi alterado** —
os dois novos modificadores (`--soft`) são estritamente opt-in via o parâmetro `Class` já existente
em ambos os componentes (o mesmo mecanismo que já sustenta `--comic`/`--skew-press`/`--plain` em
`BeeDayButton` e `--padded`/`--muted`/`--interactive` em `BeeDayCard`) — a aparência default de
ambos os componentes é bit-a-bit idêntica à anterior para todo consumidor que não passar a nova
classe.

### BeeDayButton Migration

**EXTEND, não substituição.** Novo modificador `.beeday-button--soft` em `design-system.css`
(propriedade compartilhada do Design System, não CSS local da Home — §23): remove a borda pixel e o
radius de 3px, aplica `border-radius: pill`, substitui a sombra offset "pixel press" por
`--beeday-shadow-md`→`-lg` no hover (elevação, não "afundar"), preserva o mecanismo de cor
(`--beeday-button-background`/`-foreground`/etc.) para continuar compondo com qualquer `Variant`
(Primary/Secondary/...). Usado por `PublicHeader` e pelos 3 CTAs de `Home.razor` (hero, seção final).
API pública de `BeeDayButton` (`Variant`, `Compact`, `OnClick`, `Class`, ...) **inalterada** —
nenhum parâmetro novo, nenhuma quebra de contrato.

### BeeDayCard Migration

**EXTEND, não substituição.** Novo modificador `.beeday-card--soft` em `design-system.css`: remove a
borda de 1px, aplica `--beeday-radius-2xl`, `--beeday-shadow-lg`. Usado pela seção de capacidades
(`#capabilities`) e pela seção de progresso (`#progress`) de `Home.razor`. API pública de
`BeeDayCard` (`Padded`, `Muted`, `Interactive`, `Class`, ...) **inalterada**.

### BeeDayHero Migration

**Não migrado — decisão mantida da Sprint 20.5, reavaliada e confirmada.** `BeeDayHero` continua
representando corretamente o padrão "painel introdutório em caixa" (usado potencialmente por outras
páginas operacionais no futuro, como o catálogo já demonstra para Wallet/Account). O Hero da Home
precisa de uma banda full-bleed colorida — uma responsabilidade estrutural genuinamente diferente
que só a Home consome hoje. Forçar essa responsabilidade em `BeeDayHero` exigiria uma variante nova
usada por um único consumidor real (viola a regra de só estender com 2+ consumidores). A composição
do hero da Home é, portanto, local (`.home-hero` em `Home.razor.css`), reutilizando tokens
compartilhados (cor, tipografia, radius, shadow) mas não o componente `BeeDayHero` em si.
`BeeDayHero` permanece disponível e correto para seu papel original.

### PublicHeader Migration

Arquitetura preservada — `PublicHeader` continua o shell público, não foi transformado em
`TopNavigation` nem misturado com navegação autenticada. Migração visual: altura de `3.75rem` (60px)
para `4.25rem` (68px, mais próxima dos 78px da referência que dos 60px anteriores, mantendo
proporção com a densidade compacta do resto do produto); CTA passou a usar `.beeday-button--soft`
para consistência de forma com os CTAs da Home; a metade "BEE" de `BeeDayBrand` passou a usar
`--beeday-color-brand-primary` (canônico) via `::deep .beeday-brand` escopado só ao DOM do
`PublicHeader` — sem alterar o componente `BeeDayBrand` compartilhado (seus outros 8 consumidores
— Login, as 5 páginas de Identity, Tutorial, CreateProfile — continuam roxo, fora de escopo).

**Fundo permanece claro** (`--beeday-color-surface`), diferente do header escuro (`#17203b`) da
referência — decisão escopada, não uma continuação da rejeição de paleta já corrigida acima: inverter
o fundo exigiria uma variante de cor clara/inversa de `BeeDayBrand` (hoje o componente só define uma
cor fixa por metade da marca, sem variante para fundo escuro) e reavaliar contraste/foco de todos os
elementos do header — uma mudança de componente compartilhado com escopo próprio, não uma
consequência direta e de baixo risco da migração de paleta como a recoloração de "BEE". Registrada
como candidata explícita para a Sprint 20.7, junto com uma eventual variante inversa de `BeeDayBrand`.
`PublicLayout.razor.css`'s `--beeday-top-navigation-height` atualizado de `3.75rem` para `4.25rem`
para continuar compensando corretamente o header fixo (consumidor direto da mudança de altura,
identificado e corrigido).

### Home Migration

Reescrita completa de `Home.razor`/`Home.razor.css`, preservando 100% o routing, autenticação e
`AuthenticatedEntryDestinationResolver` da Sprint 20.5:

- **Hero:** banda full-bleed (`margin-inline: calc(-1 * var(--beeday-page-gutter))` para escapar do
  padding de `.beeday-main`), gradiente `--beeday-color-brand-primary`→`-brand-primary-light`
  (mesma direção clara→mais-clara de `site.css`'s `.section-blue`), headline em
  `--beeday-font-size-hero` + `--beeday-font-weight-black` sobre `--beeday-font-body` (Nunito, não
  mais Jersey 25), CTA duplo (`BeeDayButton --soft` + link secundário âncora para `#capabilities`,
  estilizado localmente como botão-fantasma).
- **Split "capabilities":** texto (eyebrow/h2/p) + `BeeDayCard --soft` com as 5 capacidades reais em
  linhas (não mais grid uniforme de 5 cards separados).
- **Split "progress" (invertido):** texto + `BeeDayCard --soft` com ícones institucionais
  (Experience/Level) — sem números, sem ring de progresso fictício.
- **CTA final:** nova seção full-bleed antes do footer, mesmo tratamento visual do hero, mesmo
  padrão de CTA duplo/reutilizado.
- **Footer:** `AppFooter` via `PublicLayout`, inalterado.

### Visual Showcase

A referência usa um "dashboard-card" com anel de progresso e barras com números fictícios — **não
reproduzido** (dado fictício proibido, §18 do prompt de execução). A **função visual** (elemento
visual ao lado do texto, na seção de progresso) foi traduzida usando capacidades reais do BeeDay:
ícones `PixelIconName.Experience`/`Level` dentro do novo `BeeDayCard --soft`, com o texto
institucional "Level up as you go" — sem simular um valor específico de XP/nível.

### Reference Fidelity

| Aspecto | Alinhamento | Evidência |
|---|---|---|
| Palette | **HIGH ALIGNMENT** | `--beeday-color-brand-primary` (`#2538d2`) é o valor literal da referência, consumido pelo hero/CTA/eyebrows/ícones da Home e pela marca do `PublicHeader`. Bloqueio técnico concreto restante: header não invertido para fundo escuro (ver "PublicHeader Migration") — único ponto não HIGH, tratado à parte abaixo |
| Typography | **HIGH ALIGNMENT** | `--beeday-font-body` é Nunito (valor literal da referência) em todo o produto; headline/eyebrow/CTA final da Home usam Nunito peso 900 (`--beeday-font-weight-black`), igualando família E presença. `--beeday-font-ui` (Jersey 25) mantido por responsabilidade de marca real e pré-documentada (chrome pixel-console/`BeeDayButton`) — não é uma família rejeitada, é uma segunda família com papel distinto do da referência, que não tem equivalente |
| Hero | **HIGH ALIGNMENT** | Banda full-bleed em azul canônico, headline Nunito 900 em escala hero, CTA duplo, whitespace generoso — cor, família e composição agora alinhadas |
| Header/Navigation | **PARTIAL ALIGNMENT** | Marca "BEE" recolorida para o azul canônico e altura/CTA aproximados; fundo permanece claro (bloqueio técnico concreto: `BeeDayBrand` não tem variante de cor para fundo escuro hoje — inverter exigiria essa variante nova, escopo de componente compartilhado, não uma consequência direta da migração de paleta) — candidato explícito para a Sprint 20.7 |
| Buttons | **HIGH ALIGNMENT** | `--soft` modifier reproduz forma (pill, sem borda, sombra suave); cor de fundo (`--beeday-game-yellow`, variante Primary padrão) já era próxima do amarelo da referência |
| Cards | **HIGH ALIGNMENT** | `--soft` modifier reproduz radius/sombra generosos da referência |
| Spacing/Section rhythm | **HIGH ALIGNMENT** | Ritmo vertical generoso via `clamp()` local, split layouts alternados |
| Composition (split layouts) | **HIGH ALIGNMENT** | Duas seções split (texto+visual), uma invertida, replicando diretamente o padrão `split-grid`/`split-grid.reverse` da referência |
| Responsive behavior | **HIGH ALIGNMENT** | Colapso de splits em 900px (mesmo breakpoint conceitual da referência), reutilizando precedente já existente no repositório |

**Avaliação qualitativa (pergunta obrigatória do prompt de execução):** "se alguém olhar a
referência e depois a Home BeeDay, é evidente que pertencem à mesma direção visual?" — sim, em cor,
tipografia e composição: o hero usa o mesmo azul, a mesma família tipográfica em peso 900, a mesma
estrutura full-bleed/split/CTA duplo da referência. O único desvio remanescente é escopado e técnico
(fundo do header ainda claro, por falta de uma variante de `BeeDayBrand` para fundo escuro — não uma
rejeição de paleta), registrado como candidato explícito para a Sprint 20.7.

## Sprint 20.7 — Design System Project-Wide Migration (Results)

**Última verificação:** 2026-08-12 (Sprint 20.7, branch `sprint/20.7-design-system-migration`, criada
a partir de `hmg` já sincronizado com o merge da Sprint 20.6, `0794ea4`).
**Fonte da verdade:** busca repo-wide direta de todo consumidor de `--beeday-color-primary` (`src/`,
excluindo `obj/` gerado) antes e depois de cada alteração; leitura direta de `TopNavigation.razor(.css)`,
`MainLayout.razor(.css)`, `AccountSidePanel.razor(.css)`, `ProfileSidePanel.razor.css`,
`BeeDayBrand.razor(.css)`, `PublicHeader.razor.css`, `DashboardColumn.razor.css`, `Account.razor.css`,
`app.css`, `polish.css`; execução real de `dotnet format --verify-no-changes`/`dotnet build`/
`dotnet test` após cada lote de mudanças.

**Resultado geral: PARTIAL.** A migração de **cor/tipografia/foco** (foundations) foi auditada e
concluída de ponta a ponta — não parcialmente. A migração da **linguagem "soft" de forma** (radius
generoso, sombra elevada, botões sem borda pixel) para as superfícies densas/funcionais do produto
(`/daily`, cards de atividade, `/wallet`, `/account`) foi deliberadamente **não** propagada nesta
Sprint — ver "Deferred" abaixo para o raciocínio.

### Consumer audit methodology

Busca repo-wide por `--beeday-color-primary\b` em `src/` (não em `obj/`, que é build output
regenerado, não fonte). Cada ocorrência foi lida em contexto e classificada antes de qualquer edição:
**brand/generic UI chrome** (focus rings, links, badges, hovers, seleção, drag handle) → migrada;
**status** (`success`/`warning`/`danger`/`info`) → nenhuma ocorrência real encontrada usando
`--beeday-color-primary` para status, nada a migrar; **activity semantics**
(`task`/`todo`/`project`/`attribute`/`habit`) → nenhuma ocorrência real usando `--beeday-color-primary`
para distinguir tipos de atividade lado a lado (`ProjectWorkspace.razor.css` foi avaliada
especificamente por usar a cor dentro de uma view de um único Project, não para categorização — ver
"Color Migration" abaixo). Nenhum search/replace cego foi executado sem essa classificação prévia.

### Color Migration — complete, not partial

Ao final da auditoria, **zero consumidores reais restantes** de `--beeday-color-primary` em todo o
`src/` — confirmado por nova busca repo-wide após cada lote de edições. Como consequência,
`--beeday-color-primary` (e `-hover`/`-active`/`-light`/`-soft`) foi **removida** de `variables.css`
em vez de mantida como alias permanente — não sobrou nenhum consumidor a compatibilizar.

**Arquivos migrados** (22 arquivos CSS globais + CSS isolado, todos `--beeday-color-primary*` →
`--beeday-color-brand-primary*`, mais os literais RGB duplicados `rgb(103 58 183 / ..%)`/
`rgb(101 64 159 / ..%)` → `rgb(37 56 210 / ..%)` que pareavam com eles):
`design-system.css`, `dragdrop.css`, `cards.css`, `wallet.css`, `feedback.css`, `forms.css`,
`identity.css`, `pixel-ui.css`, `theme.css`, `editor-modal.css`, `app.css` (inclui o
`:focus-visible` **global** de todo elemento focável do produto — a mudança de maior alcance desta
Sprint para consistência de teclado), `BeeDayBrand.razor.css`, `AppFooter.razor.css`,
`IconCatalog.razor.css`, `HeroCatalog.razor.css`, `ActivityAttributeSelect.razor.css`,
`PixelIcon.razor.css`, `Login.razor.css`, `HabitEditorModal.razor.css`,
`BeeDayFeedbackModal.razor.css`, `ReconnectModal.razor.css`, `Tutorial.razor.css`,
`ProjectContextFilter.razor.css`, `ProjectWorkspace.razor.css`, `CreateProfile.razor.css` (literal
RGB companion apenas).

**`--beeday-focus-color`/`--beeday-focus-ring`** (`variables.css`) também migrados diretamente
(sem alias) — diferente de `--beeday-color-primary`, este token tem um único papel bem definido (cor
do anel de foco), não o papel amplo/sobrecarregado de superfície/texto, então migrá-lo diretamente não
arrisca a mesma regressão de fundo/texto em rede: recolore consistentemente um indicador de interação
transitório, em todo o produto, de uma vez.

**`ProjectWorkspace.razor.css`** — avaliada e migrada para `--beeday-color-brand-primary` (não para
`--beeday-color-project`): os usos ali (eyebrow, barra de progresso, botão "add", toggle de lista) são
chrome genérico de uma view de edição de UM projeto já selecionado, não uma categorização lado a lado
de tipos de atividade (que é o papel real de `--beeday-color-project`, usado em `DashboardColumn`'s
badge de contagem e nos cards do board — ambos **não tocados**, confirmado semanticamente corretos).

**Nova foundation canônica** (`variables.css`): `--beeday-color-brand-primary-soft` adicionada (faltava
na família criada na Sprint 20.6), consumida por `design-system.css`/`feedback.css`.

### Typography — no further action needed

`--beeday-font-body` (Nunito) já era canônica e sem consumidores pendentes desde a Sprint 20.6.
`--beeday-font-ui` (Jersey 25) reavaliada: continua reservada ao chrome pixel-console/retro-game
(`BeeDayButton` via `!important` em `typography-policy.css`, `BeeDayBrand`, títulos de página/card,
`pixel-ui.css`) — responsabilidade de marca real, formalmente documentada antes desta Sprint, não
apenas legado. Nenhum consumidor usa Jersey 25 "só por legado" sem essa responsabilidade — auditado
diretamente, nenhuma migração necessária.

### BeeDayBrand — reusable color-variant hook (non-breaking)

`.beeday-brand { color: var(--beeday-brand-color, var(--beeday-color-brand-primary)); }` —
`--beeday-brand-color` é uma custom property CSS opcional (não um novo parâmetro Razor, API pública
100% inalterada) que qualquer container ancestral pode definir para recolorir "BEE" no seu próprio
contexto (ex.: `--beeday-brand-color: var(--beeday-color-text-inverse);` para superfícies escuras).
O fallback (nenhum override) é a marca canônica — usado por todo consumidor de superfície clara:
`PublicHeader` (que teve seu `::deep` override da Sprint 20.6 removido, pois o default já é o
canônico agora), Login, as 5 páginas de Identity, `Tutorial`, `CreateProfile` (nenhuma delas exigiu
edição própria — herdam a nova cor automaticamente via o componente compartilhado).

### TopNavigation Migration

- **Brand delegation:** substituído o markup literal `<span class="top-navigation__brand-bee">BEE</span>`/
  `day` por `<BeeDayBrand />`, resolvendo o achado histórico "Brand duplication" (ver "Risks", registrado
  desde a Sprint 20.1, deliberadamente não resolvido nas Sprints 20.4/20.5 por falta de necessidade
  comprovada — agora resolvido porque esta Sprint exige exatamente essa convergência). `--beeday-brand-color:
  var(--beeday-color-text-inverse)` aplicado localmente (superfície escura). Efeito colateral positivo:
  corrige uma inconsistência de matiz pré-existente entre "DAY" (`#ffc800` aqui vs. `#ffc928` no resto do
  produto) — agora ambos usam `--beeday-game-yellow`.
- **Color:** fundo migrado do literal `#5b1095` para `var(--beeday-color-brand-primary-active)`.
  Durante a migração, encontradas **3 declarações `.top-navigation { background: ... }` conflitantes**
  no mesmo arquivo (resultado de edições incrementais anteriores) — a última (`#5b1095` flat) sempre
  vencia a cascata, tornando um gradiente roxo intermediário código morto. Consolidadas em uma única
  declaração canônica.
- **Height:** `3.75rem` → `4.25rem`, igualando `PublicHeader` (Sprint 20.6) — "spacing language"
  compartilhada entre os dois shells.
- **Tokens:** `#ffc800`/`#ffc928`/`#171321` literais → `var(--beeday-game-yellow)`/`var(--beeday-game-ink)`.
- **Arquitetura preservada:** `TopNavigation` não foi transformado em `PublicHeader` — continua um
  componente próprio, com sua responsabilidade autenticada (painel de perfil, links Daily/Wallet, menu
  de suporte) intacta.

### MainLayout Migration

- Rails colapsados (`.beeday-side-slot`) migrados de `#5b1095` para `var(--beeday-color-brand-primary-active)`
  — mesmo token de `TopNavigation`/`AccountSidePanel`/`ProfileSidePanel` (um único "authenticated shell
  surface" canônico, não mais um hex repetido em 4 arquivos).
- **Bug pré-existente corrigido como parte da mesma consolidação:** havia 3 blocos de regra parcialmente
  conflitantes para `.beeday-side-slot`; um deles pretendia tornar o rail transparente quando seu painel
  está aberto (evitar uma barra escura dupla), mas um bloco posterior de mesma especificidade re-aplicava
  o fundo escuro por cima, tornando essa regra código morto. Consolidado em um único bloco que restaura o
  comportamento aparentemente pretendido.
- `--beeday-top-navigation-height` (`.beeday-app`) `3.75rem` → `4.25rem`; `background: #f4f3f5` (literal
  quase idêntico ao token existente) → `var(--beeday-color-background)`.
- `polish.css`'s fallback de `scroll-padding-top` atualizado para `4.25rem` (mesmo valor).
- **Lógica do layout não alterada** — apenas backgrounds/superfícies/spacing, conforme pedido.

### AccountSidePanel / ProfileSidePanel Migration

- Mesma migração de cor de fundo (`#5b1095` → `var(--beeday-color-brand-primary-active)`).
- `AccountSidePanel`: brand delegado a `<BeeDayBrand />` (mesmo mecanismo de `TopNavigation`), resolvendo
  a mesma duplicação de marca.
- `top: 3.75rem` (posicionamento mobile fixo, abaixo da barra fixa) → `4.25rem` em ambos, para continuar
  alinhado com a nova altura de `TopNavigation`.

### Component Impact Analysis (BeeDayButton / BeeDayCard)

Avaliado explicitamente se `--soft` (Sprint 20.6) deveria se tornar o default de `BeeDayButton`/
`BeeDayCard` ("current default → new canonical default", conforme solicitado). **Decisão: não
flipado nesta Sprint.** `BeeDayButton` tem 40+ consumidores confirmados; validação visual não está
disponível neste ambiente (mesma limitação de todas as Sprints anteriores — sem conectividade de
banco de dados). Trocar o default cegamente recoloriria/reformaria instantaneamente toda a interface
"pixel/comic" do produto (badges, botões de pontuação de hábito, confirmações destrutivas) sem
qualquer verificação visual possível — um risco de regressão desproporcional ao que pode ser
verificado nesta Sprint. Mantida a estratégia já validada na Sprint 20.6 (canônico como modificador
opt-in + migração deliberada por consumidor real) em vez de um flip global. **Não é uma decisão
final** — registrada como candidata explícita para avaliação futura, com validação visual real.

### Deferred — "soft" visual language propagation

**Não implementado nesta Sprint, com motivo explícito:** propagar radius generoso (`--radius-2xl`)/
sombra elevada/formato sem borda para as superfícies **densas e funcionais** do produto —
`DashboardColumn` (board Kanban compacto, já auditado: usa somente tokens compartilhados corretos,
nenhuma cor legada, radius pequeno deliberado para densidade), cards de Habits/Tasks/Todos/Projects em
`cards.css` (linguagem "pixel/comic" com paletas por tipo de atividade — `--beeday-color-task`/`-todo`/
`-project`/habit colors — preservadas intactas, conforme exigido), `/wallet` (`wallet.css`, já com cor
migrada, mas sem radius/shadow "soft" aplicado), `/account` (`Account.razor.css`, auditado — não usa
cor legada, mas também não usa `--soft`). **Motivo:** a intensidade de forma "generosa/marketing" do
`--soft` foi desenhada e validada para a Home (uma landing page); aplicá-la a um board de Kanban denso
ou a cards de atividade compactos mudaria sua função visual (menos itens visíveis, hierarquia diferente)
sem qualquer verificação visual disponível — risco desproporcional, igual ao do item anterior.
Cor/tipografia/foco (a base) convergem; a composição/densidade de cada superfície permanece a que já
existia, deliberadamente, seguindo a mesma lógica que a própria Sprint autoriza para Login/Onboarding
("a composição pode continuar específica de cada jornada").

### Legacy CSS

`--beeday-color-primary` e seus 4 modificadores foram **removidos** (não apenas marcados como
"a remover depois") de `variables.css`, já que a auditoria confirmou zero consumidores restantes.
Nenhum outro token/modificador ficou órfão nesta Sprint — `--soft` (Sprint 20.6) continua consumido
por `PublicHeader`/`Home.razor`; nenhum CSS duplicado foi introduzido (os dois casos de duplicação
pré-existente encontrados — `TopNavigation`'s 3 blocos `.top-navigation`, `MainLayout`'s 3 blocos
`.beeday-side-slot` — foram consolidados, não somados).

### Responsiveness

Verificado por leitura de código (mesma limitação de ambiente das Sprints anteriores — sem navegador
disponível): alturas de header/nav (`4.25rem`) e os pontos de corte mobile dependentes dela
(`top: 4.25rem` em `AccountSidePanel`/`ProfileSidePanel`) permanecem consistentes entre si após a
mudança; nenhum breakpoint novo introduzido; `TopNavigation`'s colapso mobile (680px, esconde
`__links`) não alterado, apenas a marca dentro dele.

### Accessibility

`:focus-visible` global (`app.css`) agora consistente em azul canônico em todo o produto — melhoria de
consistência de indicador de foco, não uma regressão (mesma opacidade/espessura de anel, só a cor
migrou). `BeeDayBrand`'s `aria-label="Bee Day"` preservado nos dois novos consumidores (`TopNavigation`,
`AccountSidePanel`) dentro de wrappers `aria-hidden="true"` já existentes (a ação real é comunicada
pelo `aria-label` do botão/link ancestral, como antes). Nenhuma landmark, hierarquia de heading ou
`prefers-reduced-motion` alterada — a transição de `font-size` do brand-hover em `TopNavigation` foi
adicionada à lista de seletores já neutralizados sob `prefers-reduced-motion: reduce`.

### Tests

`AccountSidePanelTests.RendersInstitutionalBrandingInsteadOfSocialMedia` atualizado: as asserções que
liam `.support-drawer__brand-bee`/`.support-drawer__brand-day` (classes removidas com a migração para
`<BeeDayBrand />`) agora leem `.beeday-brand`/`.beeday-brand__accent` dentro de `.support-drawer__brand-mark`.
Nenhum outro teste existente assumia as classes/markup legados de `TopNavigation` (não há
`TopNavigationTests.cs`/`MainLayoutTests.cs` dedicados no repositório). `BeeDayBrandTests`,
`PublicHeaderTests`, `PublicLayoutTests`, `HomeTests`, `EntryFlowVisualConsistencyTests`,
`DailyPageScrollArchitectureTests` executados como suíte focada após as mudanças estruturais — todos
aprovados sem alteração necessária.

### Visual Validation

```text
NOT EXECUTED
```

Mesma limitação de todas as Sprints anteriores desta EPIC — sem conectividade real de banco de dados
neste ambiente. Nenhuma aprovação visual simulada ou declarada.

### Documentation

Este documento (extração/migração de cor documentadas acima); `docs/design-system/01-foundations.md`
(ver seção "Migração de marca em andamento" — a atualizar para refletir a conclusão nesta Sprint);
"Deferred to Sprint 20.7" (abaixo) reescrita para refletir o que foi de fato concluído vs. o que
genuinamente permanece.

## Sprint 20.8 — Full Visual Convergence, Responsive, Accessibility & Final Audit (Results)

**Última verificação:** 2026-08-12 (Sprint 20.8, branch `sprint/20.8-final-visual-convergence`, criada
a partir de `hmg` já sincronizado com o merge da Sprint 20.7, `932f4be`). Sprint final da EPIC 20.
**Fonte da verdade:** busca repo-wide direta de todo consumidor de `<BeeDayButton`/`<BeeDayCard`
(40+ e 9 pontos respectivamente, cada um lido com contexto completo, não amostrado) antes de decidir
os defaults; leitura direta de `LoginBackground.razor(.css)`, `OnboardingLayout.razor(.css)`,
`HabitCard.razor`/`ActivityCard.razor`/`cards.css`, `wallet.css`, `settings.css`,
`pixel-nes.css`/`NES_ATTRIBUTION.md`; execução real de `dotnet format --verify-no-changes`/
`dotnet build`/`dotnet test` após cada lote de mudanças.

**Resultado geral: PARTIAL.** Ver §25 (critério de conclusão) — a EPIC não pode ser declarada
COMPLETE sem validação visual real em HMG, que não ocorreu nesta Sprint (mesma limitação de
ambiente de todas as Sprints anteriores). Trabalho real de auditoria + correção + implementação foi
executado (não apenas um audit), com escopo e evidência explícitos abaixo.

### Login Background Removal

Investigação repo-wide encontrou **dois sistemas de background distintos**, não um:

1. **`LoginBackground.razor`/`.razor.css`** (`Features/Authentication/Components/`) — um componente
   elaborado de "nuvens" animadas em parallax (4 PNGs em `wwwroot/images/authentication/clouds-1/` +
   `LICENSE.txt` de atribuição, `@keyframes` de drift/float, guarda de `prefers-reduced-motion`).
   Busca por `<LoginBackground` em todo `src/` confirmou **zero consumidores reais** — nunca foi
   montado por `Login.razor` nem por `OnboardingLayout.razor`. Código morto, órfão.
2. **`OnboardingLayout.razor.css`** — o fundo **realmente renderizado**: `background-image:
   url('/images/onboarding/auth-galaxy.png')` (starfield escuro + overlay gradiente) na própria
   `.onboarding-layout`, o layout compartilhado por Login, os 5 Identity pages, ProfileCreation
   (`CreateProfile`/`Welcome`) e Tutorial — 9 páginas ao todo.

**Ação:** removida a implementação de ambos.
`.onboarding-layout` passou a usar `background: var(--beeday-color-background)` (mesmo token
canônico de fundo já usado por `MainLayout`/`.beeday-app`) — sem gradiente novo, sem imagem
substituta, conforme instruído. `LoginBackground.razor`/`.razor.css` removidos (zero consumidores).
`wwwroot/images/authentication/` (pasta inteira, exclusiva do componente removido) e
`wwwroot/images/onboarding/` (continha apenas `auth-galaxy.png`, exclusiva do fundo removido)
removidas. `ResourcePreloader` (`App.razor`) verificado — não referencia esses caminhos
especificamente, framework-level, nada a ajustar.

**Teste de regressão estrutural adicionado:** `EntryFlowVisualConsistencyTests.OnboardingLayoutHasNoBackgroundImage`
— confirma ausência de `background-image`/`url(` em `OnboardingLayout.razor.css`, ausência do arquivo
`LoginBackground.razor` e ausência das duas pastas de imagem, para impedir reintrodução silenciosa.

**Documentação corrigida:** `docs/ux/02-accessibility.md` §6 (lista de blocos `reduced-motion`,
`LoginBackground.razor.css` removido da lista) e `docs/ux/03-responsive.md` §2.1 (linha 640px/40rem,
arquivo removido da célula; contagem de arquivos CSS isolados 30→29).

### BeeDayButton — Final Decision

**Decisão: `--soft` (Sprint 20.6) tornou-se o default canônico do `.beeday-button` base.** Não
mantido como "old default + new soft variant" permanente.

**Auditoria que fundamentou a decisão** (repo-wide, `<BeeDayButton`, todo consumidor lido com
contexto completo — não uma amostra): dos 40+ pontos de uso, a **grande maioria (30+) já usa
`--comic`/`--comic-press`/`--plain`/`--pixel-cta`**, modificadores que redeclaram border/radius/
shadow por completo e são, portanto, **totalmente não afetados** pela mudança do default — essa é a
linguagem de "ênfase operacional primária" já estabelecida (Save/Delete/Sign in/Create em Wallet,
editores de atividade via `EditorModalShell`, `BeeDayConfirmDialog`, Login, Identity, Account via
`BeeDaySettingsForm`) e **preservada intacta**, exatamente como pedido ("preserve variantes
funcionais reais"). Apenas **~9 consumidores sem modificador de forma** (Wallet: paginação de
`TransactionList`, "Try again"/"Clear filters"/"Create transaction" de `WalletEmptyState`;
`ForgotPassword`/`ResetPassword` submits; Tutorial's `BACK`/`NEXT`/`ENTER DAILY`) herdam a nova
aparência — a convergência pretendida para ações secundárias/utilitárias.

**Implementação:** geometria de `--soft` (sem borda, `border-radius: pill`, `box-shadow: shadow-md`,
hover `translateY(-2px)`+`shadow-lg`, active `translateY(0)`+`shadow-sm`, focus-visible `shadow-md`+
`--beeday-focus-ring`) movida para dentro de `.beeday-button` base em `design-system.css`; o
modificador `.beeday-button--soft` **removido** (redundante); os 6 consumidores (`PublicHeader`×2,
`Home.razor`×4) tiveram `Class="beeday-button--soft"` removido de seu markup.

**Efeito colateral identificado e corrigido:** `.beeday-pixel-cta` (`pixel-nes.css` — experiência
pixel única e deliberadamente restrita ao modal de celebração de Level Up, documentada como
"RESTRICTED USE") dependia do `border-width` da base (antes `2px solid`) para seu mecanismo
`border-image` renderizar. Com a base agora `border: 0`, essa dependência quebraria silenciosamente.
Corrigido: `.beeday-pixel-cta` ganhou `border-width: var(--beeday-pixel-unit); border-style: solid;`
explícitos, tornando-se autossuficiente independente da borda da base.

### BeeDayCard — Final Decision

**Decisão: `--soft` (Sprint 20.6) tornou-se o default canônico do `.beeday-card` base**, mesma lógica.

**Auditoria:** todo consumidor real de `<BeeDayCard` foi lido — Wallet
(`.wallet-summary__card`/`.wallet-main-panel`/`.wallet-tags-panel`/`.wallet-tag-item`/
`.wallet-transaction-card`, `wallet.css`), a página Account (`.beeday-settings-section`,
`settings.css`) — **todos já redeclaram border/border-radius/background/box-shadow por completo**
(um deles com comentário pré-existente explicando exatamente por quê, `wallet.css` linha ~60).
**Achado corrigido nesta Sprint:** a documentação anterior (`02-components.md` §3) afirmava que "a
maioria dos cards de produto (Activity/Habit) não usa `BeeDayCard`" — verificado diretamente que isso
está **incorreto**: `HabitCard.razor`/`ActivityCard.razor` renderizam `<BeeDayCard Class="@CardCssClass">`
como raiz; o que é verdade é que `cards.css` estiliza as classes computadas
(`.habit-card`/`.activity-card` + variantes) com border/radius/shadow própria e completa (`.habit-card`
inclusive com `!important`) — arquitetural e semanticamente são consumidores reais do componente
compartilhado, apenas visualmente autossuficientes. **Conclusão da auditoria: o novo default não tem
nenhum efeito visual sobre nenhum desses consumidores** — só a Home dependia de fato do default (já
usava `--soft` explicitamente). Modificador `.beeday-card--soft` removido; os 2 consumidores
(`Home.razor`) tiveram a classe removida do markup.

### Daily / Activity Cards / Wallet / Account — shape convergence status

**Resultado honesto: tokens/cor/tipografia/foco convergiram (Sprints 20.6-20.8); a linguagem de
forma "generosa" (radius grande, sombra elevada, sem borda) da Home **não** foi propagada à
composição desses cards** — e a auditoria acima explica exatamente por quê: `.activity-card`/
`.habit-card`/`.wallet-transaction-card`/`.wallet-tag-item`/`.beeday-settings-section` têm sua
própria forma completa e deliberadamente compacta (radius pequeno, borda fina, sombra sutil),
apropriada à densidade funcional dessas superfícies (grades de Kanban, listas de transação,
formulários de configuração) — mudar essa forma exigiria reescrever cada um desses sistemas CSS
individualmente (não uma mudança de um ponto central como o default de `BeeDayButton`/`BeeDayCard`),
sem nenhuma validação visual disponível para verificar o resultado em superfícies tão densas e
usadas constantemente. Isso está alinhado com a própria instrução da Sprint ("Isso NÃO significa
torná-los idênticos... Não transforme Daily em uma landing page") mas significa que a "sensação" de
Daily/Wallet/Account continua visualmente distinta (mais compacta, mais "pixel/funcional") da Home
(mais espaçosa, mais "soft/marketing") — convergência de identidade (cor, tipografia, foco,
linguagem de botão para ações secundárias), não de composição/densidade. Registrado explicitamente
como item que impede COMPLETE — ver §25.

### Brand — Final State

Confirmado (nenhuma ação nova necessária): `--beeday-brand-color` (Sprint 20.7) já resolve os 4
contextos pedidos — light surface (default do componente, usado por Login/Identity/Onboarding/
`PublicHeader`), dark/brand surface (`TopNavigation`/`AccountSidePanel` via
`--beeday-brand-color: var(--beeday-color-text-inverse)`), public navigation (`PublicHeader`),
authenticated navigation (`TopNavigation`). Nenhum override local frágil (`::deep` cru) restante —
confirmado por busca repo-wide, único consumo de `--beeday-brand-color` fora do próprio
`BeeDayBrand.razor.css` é `TopNavigation.razor.css`/`AccountSidePanel.razor.css`, ambos via a custom
property, não overrides diretos de seletor.

### Typography — Jersey 25 Final Role

Confirmado, sem mudança necessária: `--beeday-font-ui` (Jersey 25) reservado ao chrome
pixel-console/retro-game (`BeeDayButton` via `!important` em `typography-policy.css`, `BeeDayBrand`,
títulos de página/card, `pixel-ui.css`) — responsabilidade de marca real e formalmente documentada,
não uso legado. `--beeday-font-body` (Nunito) é a família de corpo canônica, sem consumidores
pendentes. Nenhuma família tipográfica concorrente sem semântica explícita encontrada.

### Colors — Final Sweep

Confirmado repo-wide: zero consumidores de `--beeday-color-primary` (removida na Sprint 20.7, apenas
menções em comentário histórico permanecem). **Removida nesta Sprint:** `--beeday-color-accent`/
`-hover` (`#f29b24`/`#d9820d`) — token órfão pré-existente (não introduzido pela EPIC 20), zero
consumidores confirmados por busca repo-wide, evidência clara antes da remoção. Nenhuma cor
semântica de status/atividade foi alterada — `--beeday-color-success`/`-warning`/`-danger`/`-info` e
a família `task`/`todo`/`project`/atributos/hábitos permanecem exatamente como estavam.

### Legacy CSS/Tokens Removed

- `--beeday-color-primary` + `-hover`/`-active`/`-light`/`-soft` (Sprint 20.7).
- `--beeday-color-accent`/`-hover` (esta Sprint — órfão pré-existente).
- `.beeday-button--soft` (modificador, incorporado ao default desta Sprint).
- `.beeday-card--soft` (modificador, incorporado ao default desta Sprint).
- `LoginBackground.razor`/`.razor.css` + `wwwroot/images/authentication/` +
  `wwwroot/images/onboarding/` (código morto + assets exclusivos).

### Responsive Audit

Revisão por leitura de código (mesma limitação de ambiente — sem navegador disponível para as
Sprints desta EPIC): nenhuma mudança estrutural de grid/breakpoint foi introduzida nesta Sprint
(apenas forma/cor/background de superfícies existentes); `.beeday-button--compact`/`--full-width`
verificados compatíveis com a nova geometria pill (não sobrescrevem radius); `OnboardingLayout`'s
breakpoint `40rem` preservado (só o `background-position` morto foi removido). **Não executado:**
verificação visual real em wide desktop/desktop/tablet/mobile/narrow mobile — requer HMG.

### Accessibility Audit

Revisão por leitura de código: `:focus-visible` do botão/card agora usa exclusivamente
`--beeday-focus-ring` (mesmo anel canônico azul já usado em todo o resto do produto desde a Sprint
20.7) — nenhuma perda de indicador de foco, opacidade/espessura inalteradas. `--comic`/`--comic-press`/
`--skew-press`/`--plain`/`--pixel-cta` mantêm seus próprios `focus-visible` já existentes, inalterados.
Estados `:disabled` inalterados (paleta cinza, `cursor: not-allowed`, opacidade). `aria-label`s de
`BeeDayBrand` preservados nos consumidores existentes. Nenhuma landmark, hierarquia de heading,
`prefers-reduced-motion` (novo teste de reduced-motion não necessário — remoção de background não
introduziu motion novo) alterada. **Não executado:** verificação de contraste/zoom/teclado real em
navegador — requer HMG.

### Interaction States Consistency

`--comic` (ênfase primária) e o novo default `--soft`-based (ações secundárias/utilitárias) agora
formam dois papéis semânticos claros e coerentes, aplicados de forma consistente em todo o produto —
não uma mistura arbitrária por página. `:disabled` idêntico em ambos. `:focus-visible` usa o mesmo
anel canônico em ambos (exceto onde os modificadores comic/pixel definem o próprio, por design,
desde antes desta Sprint).

### Tests

`AccountSidePanelTests.cs` (Sprint 20.7, inalterado nesta Sprint). Novo:
`EntryFlowVisualConsistencyTests.OnboardingLayoutHasNoBackgroundImage`. Nenhum teste frágil de
valor CSS literal foi criado. Suíte completa executada integralmente após cada lote de mudanças —
770/770 aprovados (suite cresceu de 769 para 770 com o novo teste), sem flakiness.

### Visual Validation

```text
NOT EXECUTED
```

Mesma limitação de todas as Sprints da EPIC 20 — sem conectividade real de banco de dados neste
ambiente. Nenhuma aprovação visual simulada ou declarada. **A revisão visual em HMG é obrigatória
antes de a EPIC 20 poder ser declarada COMPLETE** (§25 do prompt de execução desta Sprint).



A antiga Sprint "Home Content & Product Integration" (dados reais/pessoais na Home, streak,
"% de consistência", integração de Application) **não pertence mais ao caminho crítico da EPIC 20**
(ver roadmap abaixo) — registrada aqui como evolução futura de produto, não como uma Sprint numerada
da EPIC:

- Dados reais/pessoais na Home (ex.: um resumo do progresso do próprio visitante autenticado).
- Investigar se um campo de "streak"/dias consecutivos existe ou vale a pena expor via Application.
- Decidir um "% de consistência" agregado, se aprovado (gap de Application já registrado desde a
  Sprint 20.1).
- Reavaliar estratégia de navegação por âncora no `PublicHeader` com o conteúdo da Home consolidado.

## Deferred to Sprint 20.7 (Design System Component Migration) — histórico, ver Results acima

**Concluído na Sprint 20.7** (registrado aqui apenas para preservar o histórico do que este bloco
pedia originalmente, sem apagá-lo): a migração de cor (`--beeday-color-primary` → canônico, com
remoção do token legado por ausência de consumidores restantes) e a migração de `TopNavigation`/
`MainLayout`/`AccountSidePanel`/`ProfileSidePanel` — ver "Sprint 20.7 — Results" acima para o
detalhamento completo.

## Resolved in Sprint 20.8 (histórico, ver Results acima)

Itens antes listados como "deferred to 20.8" e agora resolvidos nesta Sprint — preservados aqui como
registro, não apagados: decisão final de default `BeeDayButton`/`BeeDayCard` (`--soft` incorporado
como default, auditado, implementado); remoção do background de imagem do `OnboardingLayout`/Login;
remoção do token órfão `--beeday-color-accent`; confirmação de que o contrato `--beeday-brand-color`
já resolve todos os contextos pedidos, sem overrides frágeis restantes.

## Deferred beyond EPIC 20 (genuinely remaining)

- **Propagação da linguagem "soft" de forma** (radius generoso, sombra elevada, sem borda) para a
  composição de superfícies densas/funcionais — `DashboardColumn`, cards de Habits/Tasks/Todos/
  Projects (`cards.css`), `.wallet-transaction-card`/`.wallet-tag-item`/`.wallet-summary__card`,
  `.beeday-settings-section` — cada uma dessas classes já se auto-declara border/radius/shadow por
  completo (não herdam mais nada do default de `BeeDayCard`), então propagar a forma exigiria
  reescrever cada sistema CSS individualmente, não mudar um ponto central; não feito sem validação
  visual real disponível (ver "Daily / Activity Cards / Wallet / Account — shape convergence status"
  na Sprint 20.8).
- Variante de cor inversa para `BeeDayBrand` (fundo escuro), candidata para permitir inverter o
  fundo do `PublicHeader` para a direção escura da referência (`#17203b`) — avaliado nas Sprints
  20.6/20.7/20.8, não implementado (mudança de componente compartilhado com escopo próprio).
- Auditoria transversal de responsividade/acessibilidade com navegador real (wide desktop → narrow
  mobile) — não executada em nenhuma Sprint desta EPIC, sem ambiente disponível; **obrigatória em
  HMG antes de a EPIC 20 poder ser declarada COMPLETE**.
- Decisão sobre ferramenta de a11y automatizada (axe-core/Pa11y) e regressão visual — nenhuma
  introduzida em nenhuma Sprint desta EPIC.

## Sprint Roadmap

```text
20.1 Reference Home & Current UI Discovery — COMPLETE

20.2 Current Visual Foundations Audit — COMPLETE (baseline audit only; not a visual-preservation decision — see Direction Change)

20.3 Native Cursor & Global Visual Cleanup — COMPLETE

20.4 Application Shell & Navigation — COMPLETE

20.5 BeeDay Home Functional Structure — COMPLETE

20.6 Reference Design System Extraction & Home Migration — COMPLETE

20.7 Design System Project-Wide Migration — COMPLETE (foundations/color/typography/focus/shell converged; shape-language propagation deferred — see 20.8)

20.8 Full Visual Convergence, Responsive, Accessibility & Final Audit — PARTIAL (Button/Card default decided, Login background removed, legacy tokens swept; shape-language propagation to dense surfaces and real HMG visual/responsive/accessibility validation remain — see Results)
```

EPIC 20 status: **READY FOR HMG VISUAL VALIDATION**, não COMPLETE — ver critério de conclusão na
seção "Sprint 20.8 — Results" acima.

Numeração não obriga artificialmente a implementação — se a análise real de uma Sprint revelar uma
fronteira tecnicamente inadequada, isso deve ser reportado antes de alterar o plano, não decidido
silenciosamente. A antiga "Home Content & Product Integration" saiu do caminho crítico numerado da
EPIC — ver "Deferred (product content)" acima.

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
- ~~Se `TopNavigation`/`AccountSidePanel` devem migrar para `BeeDayBrand`~~ — **resolvida na Sprint
  20.7:** ambos migraram para `<BeeDayBrand />` via o hook `--beeday-brand-color`, ver seção
  "TopNavigation Migration" da Sprint 20.7. Não permanece pendente.

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
## Sprint 20.14 — Home Pública BeeDay — Results

**Status:** COMPLETE — `/` consolida a entrada pública oficial com proposta de valor, preview honesto
do produto e caminhos explícitos para cadastro e Login.

- `PublicHeader` apresenta BeeDay, Login e cadastro para visitantes; usuários autenticados mantêm
  o destino resolvido por `AuthenticatedEntryDestinationResolver`.
- O Hero comunica evolução diária e direciona `Get started` para `/profile/create` e a ação
  secundária para `/login`. O X da Sprint 20.13 fecha o ciclo de volta para `/`.
- A demonstração usa somente Daily, Habits, Tasks e Projects existentes. As seções seguintes
  explicam Define, Practice e Evolve, os pilares Habits/Progress/Consistency e XP/Level reais.
  Streak, conquistas e métricas fabricadas não são apresentados.
- `AppFooter` foi reduzido a identidade, tagline, copyright e dois links externos reais; rotas
  vazias de Terms, Privacy, News, Contact e Community não foram inventadas.
- A composição reutiliza `PublicLayout`, `BeeDayBrand`, `BeeDayButton` e `BeeDayIcon`, sem novos
  componentes, imagens, JavaScript, dependências ou mudanças de autenticação.
## Sprint 20.15 — Refinamento Visual da Experiência Pública — Results

**Status:** COMPLETE — Home, Login e Create Account agora compartilham uma direção visual pública
coerente em azul, branco, amarelo e superfícies neutras.

- A faixa Header/Hero foi rastreada ao `padding-block` global de `.beeday-main` em `polish.css`.
  `PublicLayout` neutraliza esse padding na origem; border e shadow divisórios do Header também foram
  removidos, sem margem negativa ou workaround no Hero.
- CTAs em superfícies azuis usam variantes contextuais branca e amarela sem alterar o sistema global
  de botões. O amarelo permanece accent. O Footer público usa `#17203B` com texto, links, hover e
  focus de alto contraste.
- Login e Create Account compartilham `PublicAuthActions` para X → `/` e ação contextual à direita.
  Create Account perdeu o card e as cores/botões comic legados, reutiliza `beeday-field__control` e
  mantém integralmente seu state machine, validações, disabled/loading e fluxo de confirmação.
- Não existe infraestrutura de localização. A área pública permanece em inglês nesta Sprint; uma
  estratégia de i18n continua fora do escopo e deve ser decidida separadamente.
