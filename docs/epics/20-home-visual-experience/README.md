# EPIC 20 — BeeDay Home & Visual Experience

**Fonte da verdade:** decisões aprovadas no Decision Checkpoint desta EPIC (conversa entre o
responsável pelo repositório e Claude Code, 2026-08-11); descobertas verificadas diretamente no
repositório durante a Sprint 20.1 (branch `sprint/20.1-reference-home-ui-discovery`) — leitura de
`src/BeeDay.Web/`, `docs/design-system/`, `docs/web/`, `docs/ux/`, `docs/testing/`, execução real de
`dotnet format`/`dotnet build`/`dotnet test` (752 testes, 0 falhas), e inspeção direta do diretório
de referência visual local. Nenhuma afirmação de "estado atual" abaixo vem de memória — quando este
documento evoluir em Sprints futuras, cada atualização deve reverificar contra o código antes de
alterar uma afirmação de estado atual.

**Última verificação:** 2026-08-11 (Decision Checkpoint pós-Sprint 20.1).

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
- **Brand duplication** — `BeeDayBrand` deve ser o contrato de marca preferencial; `TopNavigation`/
  `AccountSidePanel` ainda renderizam a marca como texto literal residual (`LEVEL`/`UP`) — achado
  pré-existente, correção potencialmente relevante à Sprint 20.4, não decidida aqui.
- **Breakpoint proliferation** — 29 valores de breakpoint hardcoded já existem; novos componentes
  não devem simplesmente somar mais um sem analisar os existentes.
- **Navigation duplication** — shell público e autenticado podem ter composições distintas (ver
  Decisão 2), mas devem reutilizar os mesmos contratos inferiores (Brand/Button/Icon/tokens/foco).
- **Route regression** — a transformação futura de `/` (Decisão 1) não pode quebrar login,
  criação de perfil, onboarding ou o destino `/daily`.
- **Application boundary** — dados futuros da Home (ex.: se "streak"/"% de consistência" forem
  aprovados) devem vir de um contrato apropriado da Application Layer, nunca de acesso direto da
  Web a Infrastructure/persistência.

## Sprint Roadmap

```text
20.1 Reference Home & Current UI Discovery — COMPLETE

20.2 Visual Foundations Adoption

20.3 Native Cursor & Global Visual Cleanup

20.4 Application Shell & Navigation

20.5 BeeDay Home Structure

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
- Correção da marca residual (`LEVEL`/`UP` em `TopNavigation`/`AccountSidePanel`) e do link antigo
  em `AppFooter` — pré-existentes, relevantes à Sprint 20.4 mas não decididos como parte do escopo
  dela neste checkpoint.

## Deferred Findings (pré-existentes, não corrigir nesta EPIC salvo decisão futura)

- Rota `/welcome` aparentemente morta (nenhum link de entrada encontrado no repositório).
- `wwwroot/css/feedback.css:20` — declaração `animation` sintaticamente inválida.
- Inversão de z-index: o token `--beeday-z-modal` (900) é menor que dois z-index literais de modal
  real (1200, 1400).
- Link para repositório antigo (`github.com/tiagoarrigoni/LevelUp`) em `AppFooter.razor`.
- Múltiplas escalas visuais paralelas (spacing, radius, sombra) já existentes antes da EPIC 20.
- Ausência de ferramenta automatizada de acessibilidade e de regressão visual automatizada.
