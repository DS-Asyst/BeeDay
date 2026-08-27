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
