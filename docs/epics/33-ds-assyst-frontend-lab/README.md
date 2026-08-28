# EPIC 33 — DS-Assyst Repository Migration & beeday Frontend Lab

Este documento é o **Ledger de Migração/Extração** canônico da EPIC 33, criado pela Sprint 33.1
conforme exigido pelo pacote de planejamento aprovado pelo proprietário
(`EPIC_33_DS_ASSYST_FRONTEND_LAB_REWRITE`, fornecido em
`C:\Users\tiago\Downloads\EPIC_33_DS_ASSYST_FRONTEND_LAB_REWRITE`). A Issue #361 (EPIC) e as Issues
de cada Sprint (#362–#380) são a fonte operacional de escopo, ordem e autorização; este documento
registra evidência versionada de estado de repositório/GitHub coletada em 2026-08-27.

**Fonte da verdade:** inspeção direta via `git`/`gh` do repositório `tiagoarrigoni/BeeDay` na branch
`sprint/33.1-baseline-organization-migration-readiness`, criada a partir de `origin/hmg`.

## 1. Baseline pós-EPIC-32

| Ref | SHA | Observação |
|---|---|---|
| `hmg` (`origin/hmg`) | `acce26a` | Merge de #360 (remediação do Release Quality Gate da EPIC 32). HMG Deployment + HMG Verification concluídos com sucesso em 2026-08-27T09:39–09:41Z. |
| `main` (`origin/main`) | `3fffc30` | Não inclui a EPIC 32 (última promoção registrada é da era EPIC 19/PR #66); `main→prd` está fora da autoridade autônoma do Claude e não é pré-requisito da EPIC 33. |
| `prd` (`origin/prd`) | `5da0001` | Desatualizada em relação a `hmg`/`main`; estado esperado — `prd` não é promovida pela EPIC 33. |

**Baseline aprovada de migração/extração:** `acce26a` (HEAD de `hmg` no momento da Sprint 33.1). É
o commit mais recente com entrega da EPIC 32 provada em HMG (deploy + verificação verdes), portanto
é a referência imutável usada por todas as Sprints de extração de frontend (Fase B em diante) e pela
transferência de repositório (Sprint 33.2).

## 2. Inventário de prontidão para transferência

| Item | Estado observado | Classificação |
|---|---|---|
| PRs abertos | Nenhum (`gh pr list --state open` vazio) | GO |
| Branches | `hmg`, `main`, `prd`, `chore/batch-nuget-dependency-updates`, `chore/dependabot-grouping-policy` (branches Dependabot não relacionadas à EPIC 33) | GO |
| Workflow runs recentes | Últimos runs em `hmg`/`main` (`HMG Deployment`, `HMG Verification`, `Release Quality Gate`, `Promotion Policy`) concluídos com sucesso; nenhum run ativo/pendente | GO |
| Branch protection (rulesets) | `Protect HMG` (id 20580759) e `Protect Main` (id 20608232), `enforcement: active`, escopo `Repository` (não organizacional) — exigem PR, bloqueiam force-push/deleção, e checks obrigatórios (`Pull Request Validation` em `hmg`; `Release Quality Gate` + `Validate Promotion` em `main`) | GO — reconciliar após transferência (Sprint 33.2 §3) |
| Default branch | `main` | GO — registrar para reconciliação pós-transferência |
| Actions policy | `enabled: true`, `allowed_actions: all` | GO |
| Environments | `homologation` (8 secrets nomeados: `BEEDAY_ALLOWED_HOSTS`, `BEEDAY_APP_CONNECTION`, `BEEDAY_HMG_ALLOWED_RECIPIENTS`, `BEEDAY_MIGRATOR_CONNECTION`, `BEEDAY_PUBLIC_BASE_URL`, `BEEDAY_RESEND_API_KEY`, `BEEDAY_RESEND_FROM_ADDRESS`, `BEEDAY_RESEND_FROM_NAME`; nenhuma variável), `copilot` (sem secrets/variáveis observados) | GO — nenhum valor foi lido ou exposto; apenas nomes/presença |
| Repository-level secrets/variables | Nenhum encontrado via `gh secret list` / `gh variable list` (config atual usa secrets por ambiente) | GO |
| Webhooks | Nenhum (`gh api .../hooks` retornou `[]`) | GO |
| Deploy keys | Nenhum (`gh api .../keys` retornou `[]`) | GO |
| Referências hard-coded a `tiagoarrigoni/BeeDay` | 13 arquivos contêm a string `tiagoarrigoni`; a maioria são context vars dinâmicas do GitHub Actions (`github.repository` em `deploy-hmg.yml:336-337`, `validate-promotion.yml:34` — não hard-coded, atualizam sozinhas) ou documentação histórica (`docs/epics/*`, `docs/deployment/*`). Único link literal voltado ao produto: `src/BeeDay.Web/Components/Features/Institutional/Pages/Contact.razor:15` (`https://github.com/tiagoarrigoni/BeeDay`) | **Transfer-sensitive** — corrigir na Sprint 33.2 (§4 do contrato: "update canonical owner/repository references"); GitHub mantém redirect da URL antiga, mas a referência canônica deve ser atualizada |
| Organização de destino `DS-Assyst` | `gh api orgs/DS-Assyst` → **404 Not Found** (autenticado e não autenticado); usuário atual (`tiagoarrigoni`) não pertence a nenhuma organização (`gh api user/orgs` vazio); token atual não possui escopo `admin:org` | **BLOCKED** |

## 3. Disposição

**GO** para toda a auditoria de prontidão de repositório (Seção 2): não há operação de
release/deployment ativa, working tree limpo, nenhum PR aberto, nenhuma proteção teria que ser
contornada, e a única referência transfer-sensitive encontrada (`Contact.razor`) é pequena e será
corrigida na própria Sprint 33.2 pós-transferência.

**BLOCKED** para a execução da Sprint 33.2 (transferência de `tiagoarrigoni/BeeDay` →
`DS-Assyst/BeeDay`): a organização `DS-Assyst` **não existe** no GitHub no momento desta Sprint —
`gh api orgs/DS-Assyst` retorna 404 tanto autenticado quanto publicamente, e a conta atual não é
membro de nenhuma organização. Criar uma organização GitHub é uma ação de conta que pertence ao
proprietário (não uma operação de repositório dentro da autoridade de Sprint/EPIC do Claude — CLAUDE.md
§6.6 Classe E / §6.9 "credencial/autoridade externa indisponível"). Transferir um repositório para uma
organização inexistente não é tecnicamente possível.

Este bloqueio segue exatamente a regra definida em `08_CLAUDE_INITIAL_PROMPT.txt` item 5: quando um
pré-requisito necessário não está disponível, a EPIC para e o bloqueio exato é reportado — nenhuma
baseline ou organização foi inventada para contornar a ausência.

**Ação necessária do proprietário antes da Sprint 33.2:** criar a organização GitHub `DS-Assyst` (via
github.com/account/organizations/new) e confirmar que a conta autenticada (`tiagoarrigoni`) tem
permissão de "Owner" ou equivalente nela para autorizar/realizar a transferência de repositório.

## 4. Itens fora do escopo desta Sprint

Conforme o limite da Sprint 33.1 (`github-issues/Sprint-33.1.md`): nenhum código de produto foi
alterado; nenhum valor de secret foi exposto; a transferência de repositório não foi iniciada; a
extração de frontend não foi iniciada.

## 5. Correção — nome da organização e estado real de `main`

Duas correções de evidência, registradas aqui em vez de reescrever as Seções 1–4 (que descrevem
fielmente o que foi observado e executado na Sprint 33.1 no momento em que foi executada):

1. **Nome da organização.** O pacote de planejamento da EPIC 33 (e portanto o texto acima e as
   Issues #361–#380 originalmente criadas) referenciava a organização de destino como `DS-Assyst`
   (com "ss" duplo). Essa grafia estava incorreta — um erro de digitação do pacote de planejamento,
   não um fato sobre o GitHub. A organização real, já existente desde 2026-08-18 e da qual
   `tiagoarrigoni` é membro `admin`, é **`DS-Asyst`** (um único "s"). O proprietário confirmou a
   grafia correta e todas as Issues #361–#380 (títulos e corpos) foram corrigidas via `gh issue
   edit` para usar `DS-Asyst`. A disposição **BLOCKED** registrada na Seção 3 permanece verdadeira
   como registro histórico do que a Sprint 33.1 observou ao consultar literalmente `DS-Assyst`
   (404) — o bloqueio foi real para aquele nome; não foi um bloqueio real do repositório.
2. **Estado de `main`.** A Seção 1 registra `main` (`origin/main`) como `3fffc30`, desatualizado
   desde a era da EPIC 19. Essa leitura usou a branch local `main` do workspace (que estava
   defasada), não `origin/main`. O estado real de `origin/main` no momento da Sprint 33.1 já era
   `a3c7dbe` ("Merge pull request #359 from tiagoarrigoni/hmg"), ou seja, `main` já incorporava a
   EPIC 32 completa (`acce26a`) através do pipeline de promoção automatizado do repositório
   (`BeeDay — Promotion Policy` + `BeeDay — Release Quality Gate`, ambos verdes) — nenhuma ação do
   Claude. Isso não invalida a disposição da Sprint 33.1 (o único item de risco real era a
   organização inexistente), mas corrige a leitura de estado registrada.

## 6. Sprint 33.2 — Transferência para DS-Asyst & Reconciliação de Governança

**Execução da transferência:** realizada manualmente pelo proprietário (ação de conta GitHub fora
da autoridade de repositório/Sprint do Claude — CLAUDE.md §6.6 Classe E). Confirmado por evidência
direta nesta Sprint.

**Estado pré-transferência (registrado na Sprint 33.1, reconfirmado aqui):** `hmg` = `acce26a`
(depois `fc8fd03` após o merge da Sprint 33.1), `main` = `a3c7dbe`, `prd` = `5da0001`. Nenhum PR
aberto, nenhum workflow run ativo no momento da verificação.

**Verificação pós-transferência (evidência via `gh api`, 2026-08-27):**

| Item | Resultado |
|---|---|
| Coordenada canônica | `gh api repos/DS-Asyst/BeeDay` → `full_name: DS-Asyst/BeeDay`, `default_branch: main`, `visibility: public`, `archived: false` |
| Redirecionamento da coordenada antiga | `gh api repos/tiagoarrigoni/BeeDay` resolve para `DS-Asyst/BeeDay` (redirect padrão do GitHub) |
| Branches/SHAs | `hmg=fc8fd03…`, `main=a3c7dbe…`, `prd=5da0001…` — idênticos aos SHAs pré-transferência; nenhuma perda de histórico |
| Tags | `v1.0.0-foundation` presente |
| Issues/PRs da EPIC 33 | #361 (EPIC), #362–#380 (Sprints), PR #381 (merged) — todos presentes e íntegros sob `DS-Asyst/BeeDay` |
| Rulesets | `Protect HMG` (20580759) e `Protect Main` (20608232), `enforcement: active`, `source: DS-Asyst/BeeDay`, mesmos required status checks (`Pull Request Validation`; `Release Quality Gate` + `Validate Promotion`) |
| Actions policy | `enabled: true`, `allowed_actions: all` — idêntico |
| Workflows registrados | `ci.yml`, `codeql.yml`, `deploy-hmg.yml`, `deploy-prd.yml`, `release-quality-gate.yml`, `validate-promotion.yml`, `verify-hmg.yml` — todos `active` |
| Environments | `homologation` (8 secrets pelo nome, idênticos: `BEEDAY_ALLOWED_HOSTS`, `BEEDAY_APP_CONNECTION`, `BEEDAY_HMG_ALLOWED_RECIPIENTS`, `BEEDAY_MIGRATOR_CONNECTION`, `BEEDAY_PUBLIC_BASE_URL`, `BEEDAY_RESEND_API_KEY`, `BEEDAY_RESEND_FROM_ADDRESS`, `BEEDAY_RESEND_FROM_NAME`), `copilot` — nenhum valor foi lido |
| Webhooks / Deploy keys | Nenhum em ambos (`[]`) |

Nenhum valor de secret foi lido ou exposto em nenhuma etapa desta verificação.

**Remote local:** `origin` atualizado de `https://github.com/tiagoarrigoni/BeeDay.git` para
`https://github.com/DS-Asyst/BeeDay.git`.

**Referências de coordenada corrigidas no código rastreado** (link público real, não documentação
histórica de Sprint/ADR — que permanece imutável por registrar evidência do estado no momento em
que foi coletada):

- `src/BeeDay.Web/Components/Features/Institutional/Pages/Contact.razor` — link de suporte do
  GitHub atualizado para `https://github.com/DS-Asyst/BeeDay`.
- `tests/BeeDay.Web.Tests/Components/Institutional/InstitutionalPagesTests.cs`,
  `tests/BeeDay.Web.Tests/Components/Layout/AppFooterTests.cs`,
  `tests/BeeDay.E2E.Tests/InstitutionalPagesTests.cs` — asserções de `href` atualizadas para
  acompanhar o link real da página (nenhuma asserção foi enfraquecida).

Documentação histórica de Sprints/ADRs anteriores (`docs/epics/20-*`, `docs/epics/25-*`,
`docs/epics/28-*`, `docs/deployment/06,08,11,12-*`) que referencia `tiagoarrigoni/BeeDay` em
comandos `gh api` executados no passado ou em links de PR históricos **não foi alterada** —
constitui evidência histórica imutável (o GitHub redireciona as URLs antigas) e reescrevê-la
apenas para refletir a nova coordenada violaria a regra de que registros históricos não são
reescritos para refletir estado posterior.

**Validação (Claude-safe, sem LocalDB):**

- `dotnet format BeeDay.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDay.slnx -c Release` — sucesso, 0 avisos/erros.
- `dotnet test tests/BeeDay.Domain.Tests` — 121 aprovados.
- `dotnet test tests/BeeDay.Application.Tests` — 119 aprovados.
- `dotnet test tests/BeeDay.Web.Tests --filter InstitutionalPagesTests|AppFooterTests` — 29
  aprovados (cobre diretamente as asserções alteradas).
- `git diff --check` — limpo.

**Disposição:** GO. `DS-Asyst/BeeDay` é a coordenada canônica operacionalmente comprovada após o
ciclo normal branch → PR → `hmg` → HMG Deployment → HMG Verification. PR #382 (`45a795d`), HMG
Deployment e HMG Verification confirmados verdes em 2026-08-27T12:11–12:13Z.

## 7. Sprint 33.3 — Frontend Lab Architecture & Contracts

**Decisão registrada em [`ADR-008`](../../adr/ADR-008-frontend-lab-architecture-boundaries.md)**,
fundamentada em inspeção direta de `src/BeeDay.Web/BeeDay.Web.csproj`, `docs/web/README.md` e
`docs/design-system/README.md` (2026-08-27), não em suposição:

- `BeeDay.Web` não tem fronteira de assembly entre `DesignSystem`/`Features`/composition root —
  todos compartilham um único `.csproj` com `ProjectReference` para `Application`/`Domain`/
  `Infrastructure`. Consequência: o Lab **não pode** reusar o Design System via
  `ProjectReference`/pacote sem trazer o backend inteiro — o único mecanismo compatível com o
  escopo autorizado da EPIC 33 é cópia/adaptação de código-fonte, nunca reuso binário. Isso confirma
  (com evidência concreta, não apenas por diretriz do pacote de planejamento) a regra
  COPY/ADAPT/MOCK/EXCLUDE já prevista em `04_MOCK_STATE_POLICY.md` /
  `02_FRONTEND_LAB_ARCHITECTURE_AND_BOUNDARIES.md`.
- Stack confirmada: Blazor/Razor, igual à produção — nenhuma evidência aponta outra abordagem mais
  segura.
- Precedente direto já existente em produção para as galerias futuras (Sprints 33.16/33.17):
  `DesignSystem/Pages/{IconCatalog,HeroCatalog}.razor`, páginas de catálogo roteáveis dentro do
  próprio `BeeDay.Web`.
- Contrato de fonte da verdade, drift e promoção adotado integralmente de
  `05_PROMOTION_AND_DRIFT_CONTRACT.md` — `DS-Asyst/BeeDay` nunca deixa de ser a verdade de
  runtime/negócio; `beeday-frontend-lab:prd` é fonte visual validada, nunca produção implantada;
  nenhuma sincronização automática em qualquer direção.
- Validação sem banco de dados confirmada como arquiteturalmente exigida (nenhuma dependência de
  SQL Server/LocalDB/EF Core/autenticação real em nenhuma camada do Lab).

**Sprint-Specific Boundary respeitado:** nenhum código de frontend foi copiado nesta Sprint; nenhum
redesenho de UI; o repositório Lab não foi inicializado (permanece vazio, `DS-Asyst/beeday-frontend-lab`,
criado em 2026-08-18 pelo proprietário, `size:0`, sem branches/conteúdo — confirmado por
`gh api repos/DS-Asyst/beeday-frontend-lab`) — bootstrap é escopo da Sprint 33.5.

**Validação (Claude-safe, sem LocalDB):** apenas análise de documentação/código-fonte, conforme
exigido pela Sprint 33.3; nenhum código de produto foi alterado; `git diff --check` limpo.

**Disposição:** GO. Arquitetura do Lab aprovada e registrada em ADR-008; Sprint 33.4 (Inventário) e
33.5 (Bootstrap) podem prosseguir sobre esta decisão.

## 8. Sprint 33.4 — Frontend Inventory & Extraction Map

**Ledger canônico:** [`03-frontend-inventory-ledger.md`](03-frontend-inventory-ledger.md) —
109 itens (`FE33-001`–`FE33-109`), cobrindo as 54 rotas `@page` verificadas em
`docs/web/02-routing-and-pages.md`, os 25 primitives + `BeeDaySortable` do Design System, 14 peças
de layout/shell, 8 categorias de foundation, 4 categorias de asset, 3 módulos JS de interação, os 2
templates de e-mail transacional reais (`ComposeEmailConfirmation`/`ComposePasswordReset`) mais o
shell HTML/plain-text compartilhado, e o contrato de localização — todos com estratégia
COPY/ADAPT/MOCK/EXCLUDE, caminho Lab (sob o placeholder `<LabWeb>`, já que o projeto exato só nasce
na Sprint 33.5), Sprint proprietária única, e evidência de origem.

**Achado relevante desta Sprint:** o produto tem hoje apenas **2 templates de e-mail transacional
reais** (confirmação de e-mail e redefinição de senha, `IIdentityEmailComposer`), não uma família
maior — corrigindo qualquer suposição de que a Sprint 33.15 lidaria com um catálogo grande. Ambos
compartilham um único shell HTML + alternativa plain-text.

**Sprint-Specific Boundary respeitado:** nenhuma extração em massa foi iniciada; nenhum item foi
tratado como `VERIFIED` (Sprint 33.4 é inventário, não extração); nenhum dado privado de usuário de
produção foi copiado para o Ledger (todos os exemplos usados são nomes de arquivo/rota, não dados).

**Validação (Claude-safe, sem LocalDB):** análise de documentação/código-fonte + listagem direta de
diretórios; nenhum código de produto foi alterado; `git diff --check` limpo.

**Disposição:** GO. Inventário completo (nenhuma rota/componente/template real deixado de fora);
Sprint 33.5 (Bootstrap do repositório Lab) pode prosseguir.

## 9. Sprint 33.5 — DS-Asyst Frontend Lab Repository Bootstrap

**Entregue:** `DS-Asyst/beeday-frontend-lab` deixou de estar vazio. Commit raiz `3f2e391` ("chore:
bootstrap beeday Frontend Lab (Sprint 33.5)") empurrado diretamente para `hmg` — não há Sprint
anterior nesse repositório para basear um PR normal; a partir de agora, todo trabalho segue o ciclo
branch → PR → `hmg` exigido pelo contrato da EPIC.

**Solução mínima e independente:** `BeeDayLab.slnx` (.NET 10), `src/BeeDayLab.Web` (Blazor Server,
`Microsoft.NET.Sdk.Web`, **zero** `ProjectReference`/`PackageReference` a qualquer projeto
`BeeDay.*`, EF Core ou driver SQL) + `tests/BeeDayLab.ArchitectureTests` (2 testes reais, não
placeholder, que leem os `.csproj`/`appsettings*.json` do próprio repositório e falham se qualquer
referência proibida ou `ConnectionStrings` aparecer) + `tests/BeeDayLab.Web.Tests` (bUnit). Validado
nesta Sprint, com evidência real, não suposição:

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 3/3 aprovados (2 guardas de arquitetura + 1 bUnit).
- `dotnet run --project src/BeeDayLab.Web` — aplicação real subiu e respondeu `HTTP 200` na Home
  (`http://localhost:5180/`), confirmado por `curl` antes de encerrar o processo.

**Governança do repositório:**

- Branch padrão: `hmg` (confirmado via `gh api repos/DS-Asyst/beeday-frontend-lab`).
- `prd` criada a partir de `hmg` (mesmo commit `3f2e391`).
- Rulesets `Protect HMG` (id `21652764`) e `Protect PRD` (id `21652766`), `enforcement: active`:
  bloqueiam deleção e non-fast-forward, exigem PR, exigem o check obrigatório `Lab CI`
  (`.github/workflows/ci.yml`, job `Lab CI` — format + build `--warnaserror` + testes, sem LocalDB
  por arquitetura). Regras efetivas confirmadas por leitura direta do GitHub após a criação, não
  apenas pela chamada de criação.
- `Lab CI` provado executável **antes** de se tornar check obrigatório: `workflow_dispatch` manual
  em `hmg` (run `33075335907`) passou 100% verde — mesma disciplina que `docs/deployment/08-fast-pr-validation-decision.md`
  documenta para `DS-Asyst/BeeDay` (nunca deixar um Ruleset exigir um contexto ainda não provado).

**Decisão operacional explícita da EPIC — visibilidade temporária do Lab:**

```text
TEMPORARY LAB VISIBILITY: PUBLIC
PLANNED FUTURE STATE AFTER LAB STABILIZATION / PRD: GitHub Team + PRIVATE repository
```

`DS-Asyst/beeday-frontend-lab` foi tornado **público** nesta Sprint por decisão explícita do
proprietário: a organização `DS-Asyst` está no plano GitHub Free, que não permite Rulesets/branch
protection em repositório privado (`403 "Upgrade to GitHub Pro or make this repository public to
enable this feature"`, confirmado tanto na API de Rulesets quanto na API legada de branch
protection antes de qualquer mudança). Sem proteção de branch, `hmg`/`prd` ficariam sem o requisito
"protections are configured" da Sprint 33.5 — o proprietário escolheu tornar o Lab público agora,
em vez de aceitar esse gap ou pagar por um plano superior imediatamente.

**Verificação de segurança de publicação (antes de confiar na mudança já aplicada — executada e
registrada aqui por completude, já que a instrução do proprietário e a mudança de visibilidade
chegaram na mesma janela de execução):** o repositório continha, no momento da publicação,
exatamente 1 commit (`3f2e391`), 24 arquivos, todos escritos nesta própria Sprint — nenhum arquivo
foi copiado de `DS-Asyst/BeeDay` ou de qualquer fonte externa. Busca (`git grep`) nos arquivos
efetivamente versionados por padrões de secret/credencial/connection string
(`password=`, `api[_-]?key=`, `secret=`, `connectionstring`, `bearer `, `-----BEGIN`,
`AKIA[0-9A-Z]{16}`, `Data Source=`, `Server=...Database=`) encontrou apenas o nome de um método de
teste e uma string literal `"ConnectionStrings"` dentro de uma asserção que verifica a **ausência**
dela — nenhum valor de secret real. `src/BeeDayLab.Web/appsettings.json` contém apenas `Logging` e
`AllowedHosts`. Nenhum asset de terceiros, dado de usuário real, ou documentação confidencial foi
incluído (nenhum asset foi copiado nesta Sprint — Sprint 33.7). **Nenhum bloqueador de publicação
encontrado.** `DS-Asyst/BeeDay` permanece privado — esta decisão não se aplica a ele.

**Sprint-Specific Boundary respeitado:** nenhuma página de Feature foi copiada (extração começa na
Sprint 33.6); nenhuma autenticação/banco de dados/e-mail real foi adicionado; `prd` não é tratada
como branch de experimentação.

**Disposição:** GO. `DS-Asyst/beeday-frontend-lab` operacional, protegido, e provado buildável/
testável/executável sem banco de dados. Sprint 33.6 (Foundations Extraction) pode prosseguir.

## 10. Sprint 33.6 — Foundations Extraction (FE33-001..008)

**Entregue no repositório Lab** (não em `DS-Asyst/BeeDay`): PR #1
(`DS-Asyst/beeday-frontend-lab`), branch `sprint/33.6-foundations-extraction`, merge `10ebc0e`.
Primeiro ciclo normal branch → PR → `hmg` do Lab (a Sprint 33.5 fez commit raiz direto, sem PR, por
não existir Sprint anterior para basear um). `Lab CI` verde antes do merge.

Os 7 arquivos CSS de foundation (`variables.css`, `typography.css`, `typography-policy.css`,
`theme.css`, `utilities.css`, `animations.css`, `polish.css`) mais o `wwwroot/app.css` raiz foram
copiados **verbatim** de `src/BeeDay.Web/wwwroot/` (baseline `acce26a`) — nenhum valor de token
mudou. `App.razor` do Lab replica a mesma ordem relativa de cascata da produção e o link real do
Google Fonts (`Coiny`+`Nunito`).

**Duas correções de evidência aplicadas ao Ledger nesta Sprint** (não à Seção 1 do documento, que
permanece registro histórico do que a Sprint 33.4 observou):

1. O caminho de produção real dos tokens é `variables.css`, não `design-system.css` (aproximação
   do momento do inventário) — `design-system.css` é component-scoped (Sprint 33.8).
2. A fonte de marca real é `Coiny`+`Nunito` (confirmado em `src/BeeDay.Web/Components/App.razor`),
   não `Inter`/`Jersey 25` — que `docs/web/02-routing-and-pages.md` §2 ainda cita de forma
   desatualizada (retirados nas Sprints 20.6/21.4/21.9, drift de documentação do próprio
   `DS-Asyst/BeeDay`, fora do escopo da EPIC 33 corrigir).

**Exclusão deliberada, registrada, não silenciosa:** a cauda de `app.css` que referencia classes de
componentes/features ainda não extraídos (`.card-action-menu__panel`, `.editor-modal__*`,
`.beeday-field__control`, `.activity-card__checkbox`, `.profile-panel__brand`) e o keyframe
`card-menu-enter` foram deixados de fora — sem markup correspondente no Lab ainda, pertencem às
Sprints 33.7-33.14.

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 46/46 aprovados (2 guardas de arquitetura + 44
  Web.Tests, incluindo as 43 novas asserções de `FoundationTokenParityTests` cobrindo as 4 famílias
  de cor de marca, 4 semantic feedback, os 9 degraus de spacing, 7 de radius, 4 de shadow, 5 tokens
  de motion e as 9 camadas de z-index).
- `dotnet run` verificado localmente: Home, `variables.css`, `typography.css`, `app.css`,
  `lab-shell.css` todos `HTTP 200`.

Todos os 8 itens `FE33-001`–`FE33-008` do Ledger movidos de `MAPPED` para `VERIFIED`.

**Disposição:** GO. Sprint 33.7 (Icon & Asset System Extraction) pode prosseguir.

## 11. Sprint 33.7 — Icon & Asset System Extraction (FE33-009..012)

**Entregue no repositório Lab:** PR #2 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.7-icon-asset-extraction`, merge `c4df0ad`. `Lab CI` verde antes do merge.

Copiados verbatim de `acce26a`: `wwwroot/icons/sprite.svg` (59 símbolos), o manifesto
`design/icons/catalog/icon-mapping.csv`, e os 6 tipos C# puros do sistema de ícones
(`BeeDayIconName`, `BeeDayIconCategory`, `BeeDayIconColor`, `BeeDayIconSize`,
`BeeDayIconDefinition`, `BeeDayIconRegistry`) — sem `BeeDayIcon.razor` em si, que permanece
FE33-028/Sprint 33.8, junto dos outros 25 primitives do Design System.

Também copiados, byte-a-byte (confirmado `Bin` no diff do Git — não corrompidos como texto), os 10
assets de ilustração/logo mapeados (`wwwroot/assets/{brand,dashboard,flags,footer,hero,home}/`,
4.9MB). **Auditoria de licença desta Sprint** (`docs/brand/01-character-illustration.md`): nenhuma
menção de licenciamento restritivo — são artes originais do próprio produto BeeDay (mascote "bee",
ilustrações Home/Dashboard/Hero, bandeiras, onda do footer), sem consumidor de terceiro a proteger.

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 122/122 aprovados (2 guardas de arquitetura + 120
  Web.Tests, incluindo as 67 novas asserções de `IconSystemParityTests` — uma definição de registry
  por valor de enum, todo `symbolId` resolvido verificado presente no sprite copiado, comportamento
  de fallback, e spot-checks de categoria — e as 10 de `AssetExistenceTests`).
- `dotnet run` verificado localmente: Home, `sprite.svg`, `bee.png`, `footer-wave.svg` todos
  `HTTP 200`.

`FE33-009`–`FE33-012` movidos de `MAPPED` para `VERIFIED`.

**Reconciliação de ambiente de trabalho:** a partir desta Sprint, o proprietário clonou
`DS-Asyst/beeday-frontend-lab` localmente em `C:\DevOps\MyHub\beeday-frontend-lab` — o scratchpad
temporário usado para bootstrap/Sprints 33.5–33.7 não é mais necessário; toda Sprint futura do Lab
usa esse clone real.

**Disposição:** GO. Sprint 33.8 (Shared Components Extraction) pode prosseguir.

## 12. Sprint 33.8 — Shared Components Extraction (FE33-013..038, FE33-105..107)

**Entregue no repositório Lab:** PR #3 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.8-shared-components-extraction`, merge `f87e498`. `Lab CI` verde antes do merge.

26 componentes de presentation do Design System + 3 itens de JS interop (menos um, ver drift
abaixo) copiados/adaptados de `acce26a`: `Buttons/BeeDayButton`, `Cards/BeeDayCard`,
`Feedback/{BeeDayConfirmDialog,BeeDayDashboardSkeleton,BeeDayEmptyState,BeeDayErrorBoundary,
BeeDayLoading,BeeDaySkeleton,BeeDayToastHost}`, `Forms/{BeeDayCheckbox,BeeDayDateInput,BeeDayInput,
BeeDaySelect,BeeDayTextArea,BeeDayValidationMessage}`, `Icons/BeeDayIcon` (wrapper Razor — os tipos
de registry já vieram da Sprint 33.7), `Layout/{BeeDayHero,BeeDayPageHeader,BeeDaySectionHeader,
BeeDaySettingsForm,BeeDaySettingsSection}`, `Modals/{DialogFocusScope,EditorModalShell}`,
`Progress/BeeDayProgressBar`, `Text/{BeeDayBrand,SearchHighlight}`,
`Behaviors/DragDrop/BeeDaySortable` (+ `SortableOrder`, `SortableReorderEvent`).

**Correções de classificação Strategy aplicadas ao Ledger nesta Sprint** (não à Seção 1 nem a
qualquer leitura histórica de Sprint anterior — apenas às células Strategy/State das linhas
FE33-013..038/105..107, que descrevem trabalho desta própria Sprint):

1. `BeeDayConfirmDialog`, `BeeDayErrorBoundary`, `BeeDayLoading`, `BeeDayProgressBar` (FE33-015,
   018, 019, 035) estavam classificados como COPY no inventário original (Sprint 33.4); a leitura
   completa do código-fonte nesta Sprint encontrou injeção de `IStringLocalizer<DesignSystemResources>`
   não prevista em nenhum dos quatro. Reclassificados para ADAPT — o Lab não tem pipeline de
   localização (`Program.cs` não chama `AddLocalization`, e `BeeDayIcon`, já copiado na Sprint 33.7,
   também não depende de localizer), então a dependência foi removida e substituída por strings
   padrão em inglês hardcoded, mesma abordagem já usada em FE33-021/027.
2. `BeeDaySortable` (FE33-038) estava classificado como ADAPT com a nota "a chamada real a
   `store.ReorderAsync` vira handler local no Lab" — a leitura completa mostrou que o componente já
   é presentation-only: expõe apenas `OnReorder` (`EventCallback<SortableReorderEvent>`) e não
   contém nenhuma chamada direta a `DashboardState`/`store.ReorderAsync`. Reclassificado para COPY
   (apenas rename de namespace); a nota original descrevia como um *consumidor* futuro do Lab deveria
   se ligar ao evento, não uma adaptação do componente em si.
3. `beeday-dialog-focus.js` (FE33-106) estava classificado como COPY — o arquivo `.js` em si
   permanece verbatim, mas o caminho de import hardcoded com sufixo de cache-busting (`?v=...`) usado
   pelos consumidores (`DialogFocusScope.cs`, `EditorModalShell.razor.cs`) foi removido nesta Sprint;
   reclassificado para ADAPT para refletir essa mudança no ponto de consumo.
4. A referência cruzada de FE33-034 ("`DialogFocusScope`, `beeday-dialog-focus.js` (ver FE33-109)")
   continha um erro de digitação do inventário original — FE33-109 é a página de galeria
   `HeroCatalog` (Sprint 33.16, sem relação); a dependência real é FE33-106. Corrigido na tabela.

**Dependência transitiva descoberta, não fabricada:** `BeeDayHero.Surface` é tipado pelo enum
`BeeDayPaletteToken` (`Components/DesignSystem/BeeDayPaletteToken.cs`), não inventariado
individualmente no FE33-001..109 original. Copiado junto como COPY simples — seus tokens CSS
(paleta/utility classes) já existiam no Lab desde a foundation extraction (Sprint 33.6), então não
há novo token introduzido, apenas o tipo C# que referencia os já existentes.

**Drift de documentação encontrado, não corrigido silenciosamente:** `beeday-card-menu.js`
(FE33-107) não existe em `src/BeeDay.Web/wwwroot/js/` no estado atual de `acce26a` — apenas
`beeday-sortable.js`, `beeday-culture-sync.js`, `beeday-editorial-footer.js` e
`beeday-dialog-focus.js` estão presentes. O arquivo foi removido da produção em algum ponto após o
inventário da Sprint 33.4 ter sido escrito. Nenhum arquivo foi fabricado para preencher a lacuna;
FE33-107 foi movido para `EXCLUDED` com a nota registrada na tabela — mesmo padrão do drift de fonte
(Inter/Jersey-25 → Coiny/Nunito) já documentado na Sprint 33.6.

**Exclusão deliberada, registrada, não silenciosa:** `ValidationMessageLocalizer.cs` (referenciado
por FE33-027) **não foi portado** — mapeia mensagens de validação de negócio reais do BeeDay (regras
de senha, tamanho de nome/título, etc.) que não existem no Lab; portar essa tabela seria exatamente
o "mock de lógica de negócio real" que a ADR-008 proíbe. `BeeDayValidationMessage` no Lab renderiza
`EditContext.GetValidationMessages(...)` diretamente, sem tradução.

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 171/171 aprovados (2 guardas de arquitetura + 169
  Web.Tests, incluindo as novas `SharedComponentsParityTests.cs`, `FormsAccessibilityTests.cs` e
  `ModalAndSortableTests.cs`).
- `dotnet run` verificado localmente: Home `HTTP 200`.

`FE33-013`–`FE33-038` e `FE33-105`–`FE33-106` movidos de `MAPPED` para `VERIFIED`. `FE33-107`
movido de `MAPPED` para `EXCLUDED` (drift, ver acima).

**Disposição:** GO. Sprint 33.9 (Layout Extraction) pode prosseguir.

## 13. Sprint 33.9 — Layout & Navigation Extraction (FE33-039..052)

**Entregue no repositório Lab:** PR #4 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.9-layout-navigation-extraction`, merge `01ad2f4`. `Lab CI` verde antes do merge.

14 componentes de layout/navegação + `SkipToContentLink` + `Components/Pages/{NotFound,Error}.razor`
copiados/adaptados de `acce26a`, seguindo a regra geral de remoção de `IStringLocalizer` já
estabelecida na Sprint 33.8 (o Lab não tem pipeline de localização) — aplicada de forma uniforme a
todo arquivo tocado nesta Sprint, independentemente de a classificação original do inventário dizer
COPY ou ADAPT.

**Correções de classificação Strategy aplicadas ao Ledger nesta Sprint** (apenas às linhas
FE33-039..052, que descrevem o trabalho desta própria Sprint):

1. **FE33-039 (`MainLayout.razor`)** — a dependência original listada ("`CascadingAuthenticationState`,
   `AuthorizeRouteView`") não corresponde ao arquivo real; essas duas pertencem a `Routes.razor`/
   `App.razor` (shell de roteamento), não a `MainLayout.razor`. A dependência real é
   `AuthenticatedUserInitializer` (serviço de backend real, removido por completo).
2. **FE33-046 (`NavigationItem(s).razor`)** — dividido na correção: `NavigationItem.razor` (item
   individual) é COPY puro; `NavigationItems.razor` (container com a lista real de nav) injeta
   `IStringLocalizer<LayoutResources>` e passa a ser ADAPT. Formulário de logout
   (`action="/auth/logout"` + `<AntiforgeryToken />`) mantido exatamente como está — nenhum endpoint
   real foi fabricado, apenas presentation parity.
3. **FE33-048 (`AppFooter.razor`)** — reclassificado de COPY para ADAPT (localizer). Mais
   importante: **drift de documentação encontrado**, não corrigido silenciosamente — a nota original
   ("link GitHub de suporte aponta para `DS-Asyst/BeeDay` real") não corresponde ao arquivo lido em
   `acce26a`; nenhum link do GitHub existe em `AppFooter.razor`. Provavelmente descreve um link que
   vive em uma página Institucional `Contact.razor`, fora do escopo desta Sprint. Nada foi
   inventado para "corrigir" a nota — o footer foi copiado fielmente.
4. **FE33-049 (`EditorialFooter.razor`)** — reclassificado de COPY para ADAPT: além do localizer,
   depende de `wwwroot/js/beeday-editorial-footer.js`, não listado originalmente. Verificado que o
   arquivo existe de fato em `acce26a` (ao contrário do drift de FE33-107/Sprint 33.8) — copiado sem
   o sufixo de cache-busting `?v=...`, mesma convenção já estabelecida.
5. **FE33-051 (`NotFound.razor`)** — reclassificado de COPY para ADAPT (localizer); `[AllowAnonymous]`
   também removido (sem pipeline de autorização no Lab).

**Dependência transitiva descoberta nesta Sprint, tratada com ADAPT (não apenas COPY) por tocar
código de produção fora do escopo permitido:** `PublicLanguageSwitcher.razor` (dependência de
FE33-047) referenciava `BeeDay.Web.Localization.BeeDayCultures`, que por sua vez referencia
`BeeDay.Domain.Enums.UserLanguage` — uma dependência de `BeeDay.Domain` proibida pela ADR-008.
Substituído por dois literais de código de cultura inline (`"pt-BR"`/`"en-US"`); o formulário
`/culture/set` e a leitura de `CultureInfo.CurrentUICulture` foram mantidos verbatim (nenhum pipeline
real de localização de requisição existe no Lab — mesmo padrão do formulário de logout).

**Novo, não fabricado, sem dependência de produção real:** `ReconnectDisplayState` (enum) dá a
`ReconnectModal.razor` (FE33-050, MOCK) um `[Parameter]` para definir diretamente qual dos 5 estados
visuais reais (espelhados das classes CSS `components-reconnect-show/retrying/failed/paused/
resume-failed`) mostrar, sem `ReconnectModal.razor.js` (JS interop de reconexão SignalR real) — não
copiado, nenhum circuit-handler real é acionado.

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 202/202 aprovados (2 guardas de arquitetura + 200
  Web.Tests, incluindo as novas `LayoutShellTests.cs`, `NavigationTests.cs` e
  `FootersAndPagesTests.cs`; asserções de gerenciamento de foco real via log de invocação JSInterop
  do bUnit, não apenas presença de markup).
- `dotnet run` verificado localmente: `/`, `/not-found`, `/Error` todos `HTTP 200`.

`FE33-039`–`FE33-052` movidos de `MAPPED` para `VERIFIED`.

**Correção nesta Sprint:** a disposição original registrada aqui nomeava a próxima Sprint como
"Sprint 33.10 (Localization Contract)" — título incorreto. O título canônico real, confirmado pela
Issue #371 (`DS-Asyst/BeeDay`), é **Sprint 33.10 — Mock Data & UI State Engine**. Isso não invalida a
atribuição de `FE33-104` (Localization) a 33.10 no Ledger — a Issue #371 lista explicitamente "Support
locale selection ... where useful" entre seu Required Work, então a extração de localização é parte
legítima do escopo mais amplo do motor de cenário/estado, não uma Sprint separada mal-numerada.

**Disposição:** GO. Sprint 33.10 (Mock Data & UI State Engine) pode prosseguir.

## 14. Sprint 33.10 — Mock Data & UI State Engine (FE33-104)

**Entregue no repositório Lab:** PR #5 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.10-mock-data-ui-state-engine`, merge `046e85e`. `Lab CI` verde antes do merge.

Diferente das Sprints 33.6–33.9, esta Sprint é majoritariamente **código novo, próprio do Lab** —
não extração de arquivo de produção. A Issue #371 (título canônico: "Mock Data & UI State Engine",
não "Localization Contract" como uma disposição anterior deste documento nomeou incorretamente —
ver correção na Seção 13) pede o motor único de cenário determinístico que toda Sprint de superfície
futura (Public/Identity/Daily/Wallet/Email — Sprints 33.11–33.15) deve consumir em vez de inventar
seu próprio mock local, conforme a ADR-008 já previa nas categorias ADAPT/MOCK.

**Motor de cenário** (`src/BeeDayLab.Web/Scenarios/`, sem equivalente em produção — infraestrutura
nova do Lab): `ScenarioState` (8 estados nomeados da Issue #371: `Empty`, `Populated`, `Loading`,
`Error`, `NoResults`, `Disabled`, `LargeContent`, `Selected`), `ViewportPreset` (`Desktop`/`Tablet`/
`Mobile`, opcional), `ScenarioContext` (record imutável: estado + cultura + viewport),
`IScenarioProvider<TData>` (o ponto de extensão único — doc XML com exemplo completo
`WalletScenarioProvider` para as Sprints 33.11–33.15 seguirem, mais o contrato de determinismo:
função pura de `ScenarioContext`, proibido `Random`/`Guid.NewGuid()`/relógio de parede),
`ScenarioSelection` (serviço `Scoped` por circuito, mesmo padrão de lifetime do `ToastService` da
Sprint 33.8, com evento `Changed` para uma futura página de galeria com seletor de cenário —
Sprint 33.16/33.17 — se ligar). `DemoCardListScenarioProvider` prova o mecanismo fim-a-fim contra os
primitives do Design System já existentes (`BeeDayEmptyState`, `BeeDaySkeleton`, `BeeDayCard`) —
ilustrativo, sem página real consumindo ainda.

**Localização (FE33-104)** — ver linha atualizada no Ledger acima para o detalhe completo. Resumo:
`LabCultures.cs` substitui `BeeDayCultures` (mantendo só as partes livres de `BeeDay.Domain`);
`AuthenticatedAccountCultureProvider` não foi portado (infraestrutura de backend real);
`Program.cs` ganhou um pipeline de localização real porém mínimo (`AddLocalization()`,
`UseRequestLocalization()` só com `CookieRequestCultureProvider`, sem sniffing de header/query
string) e um endpoint `POST /culture/set` — genérico, sem autenticação real, que faz o
`PublicLanguageSwitcher.razor` já entregue na Sprint 33.9 funcionar de fato (ele já postava para essa
rota, que retornava 404 até agora) **sem alterar aquele arquivo**. Catálogos `.resx` reais
(`SharedResources`, `LayoutResources`, `DesignSystemResources`, en-US+pt-BR) copiados verbatim.

**Decisão de escopo deliberada, registrada, não silenciosa:** nenhum componente das Sprints 33.8
(PR #3) ou 33.9 (PR #4) foi retroadaptado para consumir `IStringLocalizer` de novo — essas ~40
componentes hardcodaram strings em inglês deliberadamente pela regra estabelecida naquelas Sprints
já entregues/verificadas. Reescrever todas elas não foi pedido pela Issue #371 (que é sobre o motor,
com seleção de locale como uma capacidade de suporte) e constituiria retrabalho não limitado sobre
duas Sprints já encerradas. O mecanismo de localização criado aqui fica disponível para Sprints
futuras (33.11+) optarem por usá-lo em páginas novas.

**Prova de integração fim-a-fim:** verificado manualmente que `POST /culture/set` com token de
antiforgery real (obtido renderizando temporariamente o `PublicLanguageSwitcher.razor` já existente,
página removida antes do commit — não faz parte do diff) retorna `302` com
`Set-Cookie: BeeDayLab.Culture=c%3Dpt-BR%7Cuic%3Dpt-BR`, e uma requisição seguinte mostra o botão
Português do switcher como `aria-pressed="true"`.

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 232/232 aprovados (6 guardas de arquitetura + 226
  Web.Tests, incluindo as novas `ScenarioEngineTests.cs` — lookup determinístico, forma de dado por
  estado, `Changed` disparando só em mudança real —, `LocalizationResourceTests.cs` — resolução real
  de string en-US/pt-BR dos `.resx` copiados — e a nova guarda de arquitetura
  `ScenarioAndLocalizationBoundaryTests.cs`, provando em código que nada sob `Scenarios/` referencia
  `BeeDay.Domain`/`Application`/`Infrastructure`, EF Core, SQL, `ISender`/`BeeDayWebService`, ou usa
  primitivos não-determinísticos, e que `AuthenticatedAccountCultureProvider` não foi portado).
- `dotnet run` verificado localmente: Home `HTTP 200`; `POST /culture/set` sem token de antiforgery
  válido retorna `400` (mesmo comportamento da produção); fluxo completo com token real confirmado
  acima.

`FE33-104` movido de `MAPPED` para `VERIFIED`.

**Disposição:** GO. Sprint 33.11 (Public Pages Extraction) pode prosseguir.

## 15. Sprint 33.11 — Public Pages Extraction (FE33-053..076)

**Entregue no repositório Lab:** PR #6 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.11-public-pages-extraction`, merge `d86dfa9`. `Lab CI` verde antes do merge. A maior
Sprint de extração da EPIC até aqui — 24 itens do Ledger, ~40 arquivos reais (12 páginas
institucionais + 4 templates + shell/nav compartilhados, a Home pública, Typography Guidelines, e a
árvore de 20 rotas `/experience-system/*`).

**Reversão de política de localização, só para os arquivos novos desta Sprint:** as Sprints 33.8/33.9
estabeleceram "remover todo `IStringLocalizer`" porque o Lab não tinha pipeline de localização. A
Issue #372 desta Sprint pede explicitamente (item 3) "Represent current localized content" — o
pipeline já existe desde a Sprint 33.10. Portanto, todo arquivo novo desta Sprint **mantém**
`IStringLocalizer<T>` exatamente como a produção injeta, e 4 novas famílias de catálogo `.resx` foram
copiadas verbatim, espelhando a convenção da Sprint 33.10 (classe marcadora + `.resx`/`.en-US.resx`/
`.pt-BR.resx`, na mesma pasta relativa da produção): `HomeResources`, `BrandTypographyResources`
(ambas em `Components/Pages/Public/`), `InstitutionalResources` (`Components/Pages/Institutional/`),
`ExperienceSystemResources` (`Components/Pages/ExperienceSystem/`). Nenhum arquivo das Sprints
33.8/33.9/33.10 foi retroadaptado — a reversão vale só para o que esta Sprint entrega.

**Retirada do shell bootstrap da Sprint 33.5:** `Components/Pages/Home.razor` (e seu
`HomePageTests.cs`), o placeholder cujo próprio comentário dizia "exists to become" conteúdo real,
foi removido e substituído pela Home real (`Components/Pages/Public/Home.razor`) — mantê-lo teria
gerado conflito de rota `@page "/"` em tempo de compilação.

**Correções aplicadas ao Ledger nesta Sprint** (apenas às linhas FE33-053..076):

1. **FE33-057 (`Contact.razor`)** — reclassificado de COPY para ADAPT: confirma que o link GitHub
   real (`https://github.com/DS-Asyst/BeeDay`) vive de fato aqui — validando a suposição de drift já
   registrada em FE33-048 (Sprint 33.9), que apontava este arquivo (fora de escopo naquela Sprint)
   como a origem provável. Renderizado como `<span>` inerte, mesmo tratamento de FE33-048.
2. **FE33-076 (composição `ExperienceSystem`)** — reclassificado de ADAPT para COPY: a dependência
   `NavigationManager` listada originalmente não corresponde ao código real — nenhum componente sob
   `Components/Features/ExperienceSystem/` injeta `NavigationManager`; o destaque de navegação ativa
   usa parâmetros `Current`/`CurrentHref` explícitos fixados por cada página, não resolução em tempo
   real pelo router.
3. **FE33-067 (`InstitutionalPageShell` e afins)** — dependência `NavigationManager` confirmada como
   real e precisa (ao contrário de FE33-076) — `InstitutionalPageShell` de fato a injeta para computar
   o href ativo. Nenhuma correção necessária aqui, registrado por contraste com o item 2.

**Cenário engine:** não utilizado nesta Sprint. Nenhuma das ~40 páginas tem estados Empty/Loading/
Error/NoResults na produção — é conteúdo editorial/institucional fixo, conforme o próprio limite de
escopo da Issue #372 ("Use shared scenario engine only where state variation is needed").

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 259/259 aprovados (6 guardas de arquitetura + 253
  Web.Tests, incluindo as novas `PublicPagesTests.cs`, `InstitutionalPagesTests.cs`,
  `ExperienceSystemPagesTests.cs`, 4 novas teorias de resx em `LocalizationResourceTests.cs`, e a
  nova `TestCultureScope.cs`; `HomePageTests.cs` retirado junto com o shell bootstrap).
- `dotnet run` verificado localmente: `/`, `/mission`, `/contact`, `/experience-system`,
  `/experience-system/brand`, `/brand/typography` e amostragem das demais rotas institucionais/
  ExperienceSystem, todos `HTTP 200`; `/?authenticated=true` confirmado renderizando o CTA
  autenticado da Home; `/contact` confirmado sem emitir a URL real do GitHub.

`FE33-053`–`FE33-076` movidos de `MAPPED` para `VERIFIED`.

**Disposição:** GO. Sprint 33.12 (Identity & Account Visual States) pode prosseguir.

## 16. Sprint 33.12 — Identity & Account Visual States (FE33-077..087)

**Entregue no repositório Lab:** PR #7 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.12-identity-account-visual-states`, merge `4f1168d`. `Lab CI` verde antes do merge. A
primeira Sprint a efetivamente consumir o motor de cenário da Sprint 33.10 — 11 itens do Ledger, 71
arquivos, 6 novos `IScenarioProvider<TData>`.

**Interrupção e retomada:** a execução original desta Sprint foi interrompida no meio do trabalho
(reinício do processo do harness) — o agente já tinha criado o endpoint `/auth/login` em
`Program.cs`, os 6 pares de scenario provider, e os catálogos `.resx`/CSS, mas nenhum arquivo
`.razor` de página existia ainda e nada estava commitado. O trabalho não foi perdido nem reiniciado
do zero: o mesmo agente foi retomado com seu contexto original via mensagem direta, terminou as
páginas/testes restantes, e só então seguiu o fluxo normal de commit/push/PR. Nenhuma decisão de
arquitetura foi refeita nessa retomada.

**Padrão central desta Sprint:** toda chamada real a `MediatR.ISender.Send(...)`/`BeeDayWebService`
foi substituída por um provider de cenário próprio da página (`XxxScenarioData` + `XxxScenarioProvider
: IScenarioProvider<XxxScenarioData>`, registrado `Singleton`, mesmo padrão de
`DemoCardListScenarioProvider` da Sprint 33.10) — a chamada real vira `await Task.Delay(...)` (preserva
a UX do estado "loading" sem I/O real) seguido de `provider.GetScenario(scenarioSelection.Context)`.
6 providers novos: `ForgotPasswordScenarioProvider`, `ResendConfirmationScenarioProvider`,
`ResetPasswordScenarioProvider`, `ProfileCreationScenarioProvider` (todos: `ScenarioState.Error` →
falha sintética, demais → sucesso), `ConfirmEmailScenarioProvider` (mapeamento 8→6 estados
documentado em código, ver linha FE33-083 do Ledger acima), `AccountScenarioProvider` (perfil
sintético realista + flag de sucesso/falha independente por seção de salvamento).

**Exclusão deliberada, registrada, não silenciosa, com guarda de arquitetura dedicada:**
`BeeDay.Web.Localization.DomainErrorLocalizer.cs` (`using BeeDay.Application.Exceptions;`/
`using BeeDay.Domain.Exceptions;`) **não foi portado** — mesmo tratamento de
`ValidationMessageLocalizer.cs` na Sprint 33.8. Nenhum arquivo desta Sprint o chama; uma nova
verificação de arquitetura (`ScenarioAndLocalizationBoundaryTests.cs`, estendida) prova isso em
código para toda a pasta `Components/Pages/Identity/`, não apenas por convenção.

**Endpoint `POST /auth/login` novo (Program.cs):** mesmo padrão de `POST /culture/set` (Sprint
33.10) — determinístico, **não grava cookie nem cria sessão** (limite explícito da Issue #373).
Credencial sintética fixa e documentada em código: `demo@beeday.app` / `BeeDayLab!2026`. Sucesso
redireciona para `returnUrl` (guarda open-redirect `IsLocalPath` reaproveitada) ou `/profile/create`
como fallback; falha redireciona para `/login?error=invalid`, o mesmo branch de feedback dirigido
por query string que `Login.razor` já renderizava na produção. Testado com round-trips HTTP reais via
`Microsoft.AspNetCore.Mvc.Testing`/`WebApplicationFactory<Program>` — nova dependência de teste
(`Directory.Packages.props`), sem precedente anterior de testar uma rota mapeada diretamente em
`Program.cs`; justificada em vez de reimplementar a lógica do endpoint no teste.

**Outras correções/decisões registradas:**

1. **`PreferencesFormModel.cs` (FE33-086)** — reclassificado de COPY para ADAPT: `Language`/`Theme`
   eram tipados `BeeDay.Domain.Enums.UserLanguage`/`UserTheme`; retipados para os enums Lab-local
   `AccountLanguage`/`AccountTheme` (`Scenarios/AccountScenarioData.cs`), mesmo tratamento de
   `LabCultures.cs` (33.10) e `PublicLanguageSwitcher.razor` (33.9).
2. **Troca de idioma no Account** mantida funcional de verdade (não apenas visual): `Account.razor`
   copia o formulário oculto `culture-sync-form` + `beeday-culture-sync.js` (JS genérico,
   presentation-only, já reaproveitava `/culture/set` na produção) e reusa o mecanismo real que já
   existe no Lab desde a Sprint 33.10.
3. **`RedirectToLogin.razor` (FE33-087, MOCK)** — o `Navigation.NavigateTo(forceLoad:true)` real foi
   removido por completo (dispará-lo navegaria o revisor para longe da própria pré-visualização, não
   apenas "sem sessão real" como as demais páginas). Alcançável só via nova rota de demonstração
   dedicada `/identity/redirect-to-login-preview`, nunca ligada a um trigger real de autenticação.
4. **`@attribute [Authorize]` em `Account.razor`** removido — mesma razão de `[AllowAnonymous]` já
   removido em Sprints anteriores: nenhum pipeline de autorização existe no Lab (`Program.cs` não
   chama `AddAuthorization`/`UseAuthorization`) para qualquer um dos dois atributos significar algo.

**Validação (Lab, sem LocalDB):**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 317/317 aprovados (7 guardas de arquitetura + 310
  Web.Tests, incluindo `LoginAndAuthTests.cs`, `ProfileCreationTests.cs`, `IdentityRecoveryTests.cs`,
  `AccountPageTests.cs`, `OnboardingAndRedirectTests.cs`).
- `dotnet run` verificado localmente: `/login`, `/welcome`, `/profile/create` (+`?authenticated=true`),
  `/account/forgot-password`, `/account/resend-confirmation`, `/account/confirm-email`,
  `/account/reset-password`, `/onboarding/tutorial`, `/account`, `/settings` todos `HTTP 200`;
  `POST /auth/login` verificado para sucesso/falha/guarda open-redirect/ausência de cookie.

`FE33-077`–`FE33-087` movidos de `MAPPED` para `VERIFIED`.

**Disposição:** GO. Sprint 33.13 (Daily / Productivity Visual States) pode prosseguir.

## 17. Sprint 33.13 — Daily / Productivity Visual States (FE33-088..097)

**Entregue no repositório Lab:** PR #8 (`DS-Asyst/beeday-frontend-lab`), branch
`sprint/33.13-daily-productivity-visual-states`, head `01f7525`, merge `def365c`. `Lab CI` permaneceu
verde no head validado e o merge ocorreu pelo fluxo protegido normal, sem bypass. Foram entregues
78 arquivos e 8.639 linhas para `/profile`, `/daily`, quatro editores de atividade, Project
Workspace, Experience Bar e feedback visual de level-up.

**Contrato único de apresentação:** quase toda a superfície de produção era tipada contra responses
de `BeeDay.Application` e enums de `BeeDay.Domain`. A Sprint criou uma única tradução central em
`Scenarios/DailyDashboardScenarioData.cs`: 8 enums Lab-local e os records de Profile, Habit, Task,
Todo e Project. Nenhuma página ou componente redefiniu contratos próprios. A forma real do
`DashboardResponse` foi preservada: todos aninhados sob Project, sem lista top-level concorrente.

**Provider determinístico:** `DailyDashboardScenarioProvider` cobre Empty, Populated e LargeContent
com ids derivados de seed e instante de referência fixo. Loading/Error são tratados pelos callers;
NoResults é produzido pelos filtros reais sobre o dataset Populated. O cenário LargeContent coloca
as quatro coleções acima do threshold de virtualização (30) e o cenário Populated cobre todas as 7
faixas de `HabitVisualState`. Não há `Random`, `Guid.NewGuid()`, relógio de parede, rede ou
persistência.

**Estado e interação:** `LabDashboardState` preserva busca, filtro por atributo/projeto, contadores,
modais, workspace e reorder via `SortableOrder.Move`. `BeeDayWebService`/handlers/reload foram
substituídos por mutações locais após atraso sintético curto, mantendo os estados busy/toast. Os
valores de domínio já resolvidos — particularmente `ProgressPercentage` — não são recalculados após
interações locais.

**XP/level-up (MOCK):** a transição positiva anuncia o `XpGainPerAction` fixo do cenário (10);
desmarcar Task/Todo e registrar Habit negativo não anunciam ganho. `BeeDayFeedbackEventHandler`
(`MediatR` + Domain Event) foi excluído. Para tornar o estado visual diretamente exercitável, cada
3ª ação positiva adiciona ao `BeeDayFeedbackStore` um payload sintético com avanço de um nível —
comportamento determinístico de apresentação, sem curva, threshold ou regra de recompensa.

**Correções do Ledger confirmadas contra a fonte real:**

1. FE33-097 `DashboardColumn`: ADAPT → COPY — não depende de `DashboardState`; recebe fragments e
   contadores já resolvidos.
2. FE33-092 `TaskEditorModal`: COPY → ADAPT — renderiza o enum Domain `TaskRepeat`, substituído por
   `DailyTaskRepeat` no contrato central.
3. FE33-093 `TodoEditorModal`: COPY → ADAPT — recebe `ProjectSummary` de Application, substituído
   por `DailyProjectSummary`.
4. FE33-089 `LegacyHomeRedirect`: EXCLUDE confirmado — redirect puro `/home` → `/profile`, sem
   superfície visual.
5. `ExperienceViewModel.From(UserExperience)` não foi portado por depender de Domain; apenas o
   overload sobre o resumo de apresentação foi mantido.

**Validação (Lab, sem LocalDB), executada no head `01f7525` e preservada por estar inalterado:**

- `dotnet format BeeDayLab.slnx --verify-no-changes` — limpo.
- `dotnet build BeeDayLab.slnx -c Release --warnaserror` — 0 avisos/erros.
- `dotnet test BeeDayLab.slnx -c Release` — 437/437 aprovados (10 Architecture + 427 Web), 0 falhas,
  0 ignorados.
- Smoke local: `/profile` e `/daily` retornaram HTTP 200 com conteúdo real do cenário Populated.
- GitHub `Lab CI` — verde no mesmo head `01f7525` antes do merge.

`FE33-088`, `FE33-090`–`FE33-097` movidos de `MAPPED` para `VERIFIED`; `FE33-089` permanece
`EXCLUDED` por não possuir superfície visual.

**Disposição:** GO. Sprint 33.14 (Wallet Visual States) pode prosseguir a partir do `hmg` atualizado
do Lab (`def365c`).
