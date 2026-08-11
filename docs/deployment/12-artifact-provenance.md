# Validated Artifact Provenance & HMG CI Deduplication (EPIC 19, Sprint 19.8)

**Fonte da verdade:** verificado diretamente em `.github/workflows/ci.yml`, `deploy-hmg.yml`,
`verify-hmg.yml`, `deploy-prd.yml`, `validate-promotion.yml`; `gh api repos/.../rulesets/*`
(reconsultado em 2026-08-11); `gh run list`/`gh pr view` (evidência real, PR #60); `git log`
(topologia real de commits/merge).

**Última verificação:** 2026-08-11.

**Escopo:** eliminar a segunda execução completa de `BeeDay CI` que hoje roda após todo merge em
`hmg`, substituindo-a por resolução determinística de proveniência do artefato já validado pela PR.
Nenhuma mudança em SERV3WEB, Release Quality Gate, Ruleset, ou promoção final a PRD.

**Classificação de evidência:** `FACT`, `MEASUREMENT`, `INFERENCE`, `RECOMMENDATION`, `UNKNOWN`.

---

## 1. Previous Sprint Integration Check

`FACT` — confirmado via `git fetch --prune` + `git log --oneline --decorate --graph origin/hmg`:
PR #60 (Sprint 19.7 + 19.7.1) mergeada em `hmg` no commit `b396026` antes do início desta Sprint.
`MSYS_NO_PATHCONV=1 git show "origin/hmg:.github/workflows/release-quality-gate.yml"` confirma o
fix da 19.7.1 (`--no-build` removido do step Publish) presente no conteúdo real de `hmg`, não
apenas no branch local. Pré-condição da Seção 42 do prompt satisfeita.

## 2. AS-IS Execution Graph (real, não suposto)

```text
sprint/* ──PR──> hmg
                  │
                  ├── pull_request: BeeDay CI  (valida PR HEAD SHA)
                  │
                  └── MERGE (commit novo, 2 pais: hmg anterior + PR HEAD)
                          │
                          ├── push: BeeDay CI  (revalida o MESMO conteúdo, SHA diferente)
                          │        │
                          │        └── workflow_run (event=='push') ──> BeeDay — HMG Deployment
                          │                                                    │
                          │                                                    └──> BeeDay — HMG Verification
                          └── (PR HEAD SHA artifact nunca reaproveitado)
```

## 3. Real Duplicate Execution Evidence

Caso real usado (Sprint 19.7 → `hmg`, PR #60, `gh run list --workflow=ci.yml`):

| Campo | Valor |
|---|---|
| PR | #60 (`sprint/19.7-release-quality-gate` → `hmg`) |
| PR HEAD SHA | `2b2d82e` |
| PR validation run | `31454686441` — `pull_request`, `success`, 03:11:59–03:17:57 UTC (**~6.0 min**) |
| Merge SHA | `b396026` (mergedAt `2026-08-11T03:18:14Z`) |
| Post-merge CI run | `31454995477` — `push`, `hmg`, `success`, 03:18:16–03:24:54 UTC (**~6.6 min**) |
| Deploy run | `31455318044` — `workflow_run`, `success`, 03:24:56–03:26:35 UTC |
| Verification run | não localizável via `gh run list --workflow` (`verify-hmg.yml` não registrado — ver §4) |

`MEASUREMENT`: **~6.6 minutos de wall-clock duplicados** neste merge específico — o run `push`
repete integralmente Format/Build/5 suítes de teste/E2E/Publish/EF bundle já validados pelo run
`pull_request` 7 minutos antes, para o mesmo conteúdo de árvore.

`FACT`: os dois runs validam commits **diferentes** (`2b2d82e` vs `b396026`), mas com a mesma árvore
resultante (merge sem conflitos) — exatamente a duplicação que esta Sprint elimina.

`INFERENCE`: dois runs `pull_request` adicionais aparecem no log para o mesmo PR HEAD SHA
(`31454651495` às 03:11:18 e `31454686441` às 03:11:59) — provável re-disparo por push
subsequente à mesma branch durante o desenvolvimento local desta sessão, não uma anomalia do
mecanismo em si; não investigado further pois é irrelevante à arquitetura de proveniência.

## 4. Descoberta relevante: `verify-hmg.yml` não está registrado no catálogo de workflows

`FACT` — `gh api repos/tiagoarrigoni/BeeDay/actions/workflows` **não lista** `verify-hmg.yml`, e
`gh run list --workflow=verify-hmg.yml` retorna 404 ("not found on the default branch"). A branch
default do repositório (`main`) nunca recebeu nenhuma das mudanças da EPIC 19 até hoje — nenhuma PR
`hmg→main` foi mergeada ainda (consistente com `docs/deployment/11-release-quality-gate.md`:
Release Quality Gate "nunca executado remotamente"). Os nomes retornados por essa API para
`ci.yml`/`deploy-hmg.yml`/`deploy-prd.yml` (`"BeeDay CI"`, `"BeeDay Homologation Deploy"`,
`"BeeDay Production Deploy"`) também estão desatualizados em relação ao `name:` atual desses
arquivos em `hmg` — não afeta esta Sprint (nenhum rename planejado), mas é relevante para §25.

## 5. Merge Strategy Analysis — restrição crítica confirmada

`FACT`, via `gh api repos/tiagoarrigoni/BeeDay/rulesets/20580759` (Protect HMG):

```json
"allowed_merge_methods": ["merge", "squash", "rebase"]
```

**Os três métodos são permitidos** — não apenas merge commit. PR #60 usou merge commit (confirmado
por `git log -1 --format="%P" b396026` → dois pais: `e249bce` (hmg anterior) e `2b2d82e` (PR HEAD),
e `gh pr view 60 --json headRefOid` → `2b2d82e`, idêntico ao segundo pai). **Mas isso é uma
propriedade desta PR específica, não uma garantia do repositório.**

**Implicação decisiva para o design:** uma estratégia de proveniência baseada em topologia Git
(`git log --format=%P <merge-sha>` para achar o PR HEAD SHA como "segundo pai") **funcionaria hoje
mas quebraria silenciosamente** na primeira PR squash ou rebase para `hmg` — squash produz um
commit novo sem nenhuma relação de ancestralidade Git com o PR HEAD SHA original; rebase produz
commits novos com SHAs diferentes também. **Por isso a estratégia escolhida (§8) usa a API de Pull
Requests do GitHub (`listPullRequestsAssociatedWithCommit` + `pr.head.sha`), não `git log
--format=%P`** — o mesmo princípio que `CLAUDE.md` §5.7.2 já formaliza: *"A pull request's recorded
head commit always identifies the actual source-branch commit regardless of whether the target
branch merge used a merge commit, squash, or rebase"* — e que `deploy-prd.yml` já usa há pelo menos
uma Sprint para a cadeia `main→prd`/`hmg→main`.

## 6. Commit Identity / Parent Analysis

| | Valor |
|---|---|
| Merge SHA | `b396026` |
| Pai 1 | `e249bce` (hmg antes do merge) |
| Pai 2 | `2b2d82e` (PR #60 HEAD SHA, confirmado via `gh pr view --json headRefOid`) |
| PR HEAD SHA == merge SHA? | **Não** — `2b2d82e ≠ b396026` |
| Relação provável via Git ancestry | Válida apenas para merge commit (este caso); não generalizável (§5) |
| Relação usada pela implementação | API de PR do GitHub (`pr.head.sha`), independente do método de merge |

## 7. Artifact Inventory

| Artifact | Produzido por | Consumido por |
|---|---|---|
| `beeday-publish` | `ci.yml` (todo evento) | `deploy-hmg.yml`, `deploy-prd.yml` (via cadeia) |
| `beeday-migrations` | `ci.yml` (todo evento) | `deploy-hmg.yml` apenas |
| `beeday-test-results` | `ci.yml` | Nenhum consumidor automatizado (inspeção manual) |
| `beeday-e2e-artifacts` | `ci.yml` (`if: always()`) | Nenhum consumidor automatizado |
| `beeday-hmg-deployment-info` | `deploy-hmg.yml` | `verify-hmg.yml` |

## 8. Existing Artifact Retention

`FACT` — `beeday-publish`/`beeday-migrations`: `retention-days: 7`. Tempo real observado entre
validação PR e merge no caso de evidência (§3): **~7 minutos**. Retenção de 7 dias é
>1400x o intervalo real observado — não há risco de expiração entre validação e merge em operação
normal. Não alterado (nenhuma justificativa para aumentar, conforme princípio da Seção 29 do
prompt).

## 9. Existing Artifact Consumers

`FACT`: apenas `deploy-hmg.yml` e `deploy-prd.yml` baixam `beeday-publish`; apenas `deploy-hmg.yml`
baixa `beeday-migrations`. Nenhum outro consumidor identificado em `.github/workflows/`,
`scripts/`, ou `docs/`.

## 10. Provenance Requirements

Toda pergunta da Seção 4 do prompt, respondida pela arquitetura implementada (§16):

| Pergunta | Resposta |
|---|---|
| WHAT source was validated? | `sourcePr.head.sha` (PR HEAD SHA) |
| WHICH CI run validated it? | O run `pull_request` de `ci.yml` cujo `head_sha` bate exatamente |
| WHICH artifact was produced? | `beeday-publish`/`beeday-migrations` daquele `run-id` exato |
| WHICH PR introduced it? | `sourcePr.number` |
| WHICH merge produced the HMG state? | `context.sha` (merge SHA, gravado como `mergeSha`) |
| WHICH artifact was deployed? | O baixado por `run-id` pinado — nunca "latest" |
| WHICH SHA is currently running? | `sourceSha` no `deployment-info.json`, lido por `verify-hmg.yml` |

## 11. Strategy Alternatives

### Strategy A — PR Artifact Reuse (via cadeia de PRs do GitHub)

Resolve o PR associado ao commit de merge, lê seu `head.sha` (confiável independente do método de
merge — §5), busca o run `ci.yml` `pull_request` bem-sucedido para esse SHA exato, baixa o artifact
por `run-id`. **Extensão direta do padrão já comprovado em `deploy-prd.yml`, um hop mais cedo.**

### Strategy B — Merge Artifact Rebuild

Após merge, gerar artifact novamente (build/publish apenas, sem revalidar testes). **Rejeitada**:
viola "deploy exactly what was validated" — o artifact seria produzido de uma árvore que, embora
tipicamente idêntica à da PR, nunca foi ela mesma submetida aos testes/gate que autorizaram o merge;
qualquer diferença de ambiente/toolchain entre o build da PR e o rebuild pós-merge ficaria
indetectável.

### Strategy C — Commit/Artifact Manifest dedicado

Manifest JSON separado, publicado como artifact próprio junto ao publish/migrations. **Avaliada e
parcialmente adotada, mas sem arquivo dedicado** — os campos de proveniência (§17) foram anexados ao
`deployment-info.json` já existente (Sprint 19.6), que já tem exatamente um consumidor
(`verify-hmg.yml`) e já é o artifact correto para essa informação. Um manifest **separado**
adicionaria um artifact novo sem consumidor além do que já existe — rejeitado por violar a
instrução explícita da Seção 14 do prompt ("não introduza metadata sem consumidor claro").

### Strategy D — Outro mecanismo suportado pelo repositório

Nenhum mecanismo melhor identificado além da API de Pull Requests do GitHub já em uso por
`deploy-prd.yml` — não há infraestrutura externa (registry de artefatos, etc.) no repositório.

## 12. Decision Matrix

| Strategy | Correctness | Provenance | Performance | Complexity | Race Safety | Decision |
|---|---|---|---|---|---|---|
| A — PR Artifact Reuse | Alta (SHA exato via API, independente de merge method) | Completa (PR ↔ head SHA ↔ CI run ↔ artifact) | Elimina ~6.6min/merge | Baixa (extensão do padrão já existente) | Alta (match exato de `head_sha`, sem "latest") | **SELECTED** |
| B — Merge Artifact Rebuild | Baixa (rebuild ≠ o que foi testado) | Fraca (SHA do rebuild, não da validação) | Parcial (só builda, não retesta) | Média | Média (ainda pode divergir do testado) | Rejected |
| C — Manifest dedicado novo | Alta | Completa | Neutro | Média (artifact extra sem consumidor novo) | Alta | Rejected (redundante — ver §11) |
| D — Outro mecanismo | N/A | N/A | N/A | N/A | N/A | Not applicable |

## 13. Selected Architecture

**Strategy A**, implementada em `deploy-hmg.yml`. Ver `.github/workflows/deploy-hmg.yml`, step
"Resolve validated source commit and BeeDay CI run", para a implementação completa.

```text
push -> hmg (merge commit/squash/rebase, qualquer método)
   │
   ▼
deploy-hmg.yml (trigger direto: push, sem workflow_run)
   │
   ├── listPullRequestsAssociatedWithCommit(mergeSha)
   │        → PR cujo base.ref == 'hmg'
   ├── valida PR.head.repo == este repositório (fork check, §19)
   ├── lê PR.head.sha (validatedSha)
   ├── busca run ci.yml (event=pull_request, status=success, head_sha==validatedSha)
   │        → falha fechado se não encontrado
   ├── download beeday-publish/beeday-migrations por run-id exato
   └── deploy (inalterado) → deployment-info.json (sourceSha + mergeSha + PR + run) → verify-hmg.yml
```

## 14. Artifact Identity Contract

Inalterado — `beeday-publish`/`beeday-migrations` continuam identificados por **nome de artifact +
`run-id` exato** (não por SHA no nome). Renomear os artifacts (`beeday-publish-<sha>`) foi
considerado e rejeitado: `run-id` já é uma chave determinística suficiente, e um rename exigiria
mudanças coordenadas em `deploy-hmg.yml` e `deploy-prd.yml` sem nenhum ganho de correção — apenas
risco de quebra, contrário à Seção 13 do prompt ("não renomeie sem necessidade comprovada").

## 15. Provenance Manifest Contract

`deployment-info.json` (artifact `beeday-hmg-deployment-info`), estendido nesta Sprint:

```json
{
  "sourceSha": "2b2d82e...",
  "mergeSha": "b396026...",
  "pullRequest": "60",
  "validationRunId": "31454686441",
  "workflowRun": "<deploy-hmg.yml run id>",
  "result": "success",
  "environment": "homologation",
  "timestampUtc": "2026-08-11T..."
}
```

**Por que não um manifest separado por artifact** (pergunta explícita da Seção 17 do prompt): o
único consumidor de qualquer informação de proveniência hoje é `verify-hmg.yml`, que já baixa
`deployment-info.json`. Embutir os mesmos campos num segundo arquivo (ex.: dentro do próprio
`beeday-publish.zip`) duplicaria a informação sem nenhum consumidor adicional — violaria a
instrução da Seção 14 ("não introduza metadata sem consumidor claro"). Se um futuro consumidor
precisar de proveniência sem baixar `deployment-info.json` (ex.: um dashboard externo), esse é o
gatilho correto para reavaliar um manifest dedicado — não esta Sprint.

## 16. Race Condition Analysis

| Cenário | Comportamento | Por que é seguro |
|---|---|---|
| PR A validado, PR B validado, A mergeado, B mergeado | Cada push resolve seu próprio merge SHA → seu próprio PR → seu próprio `head_sha` exato | Nenhuma ambiguidade: cada resolução é isolada por commit, não por "última execução" |
| PR A validado, novos commits pushados em A (novo `head_sha`), artifact antigo ainda existe, A mergeado | O merge SHA resultante está associado ao PR A; `pr.head.sha` retornado pela API é o **HEAD atual registrado no GitHub** (o commit que efetivamente foi mergeado, dado que Protect HMG exige que o check `BeeDay CI` esteja verde para o HEAD atual antes de permitir merge) | A busca usa esse `head.sha` exato — nunca o artifact antigo do push anterior, que teria `head_sha` diferente e simplesmente não seria encontrado nesse caminho |
| Múltiplos merges em sequência rápida | Cada push dispara sua própria execução de `deploy-hmg.yml`; `concurrency: cancel-in-progress: false` serializa deploys (inalterado desde a 19.6) — cada um resolve seu próprio `context.sha` independentemente | Sem interferência cruzada — resolução é por commit, não por estado global mutável |
| Nenhum PR encontrado para o commit (push direto, hipotético) | `core.setFailed` — nenhum artifact baixado, nenhum deploy | Fail closed (§18) |
| PR encontrado mas nenhum run `ci.yml` bem-sucedido para o `head_sha` | `core.setFailed` após paginar até 2000 runs | Fail closed — nunca cai para "latest" |

**Rejeitado explicitamente:** qualquer resolução por "latest successful run" — a implementação
sempre casa por `head_sha` exato, nunca por recência.

## 17. Security Boundary Analysis

`FACT`: a implementação adiciona uma checagem de origem (`pr.head.repo.full_name ==
<owner>/<repo>`) que **não existia antes** para o caminho `sprint/*→hmg` — `validate-promotion.yml`
só cobre `pull_request: branches: [main, prd]`, nunca cobriu PRs para `hmg`. Sem essa checagem, um
PR de um fork nomeando sua branch de forma a coincidir por acidente (`base.ref == 'hmg'` ainda
exigiria que o PR fosse aberto contra este repositório, mas `head.repo` poderia ser um fork) poderia
teoricamente ser encontrado pela resolução — a checagem de `head.repo` fecha essa lacuna
estruturalmente, falhando fechado se a origem não bater.

`INFERENCE` (achado, não corrigido — fora de escopo desta Sprint): `deploy-prd.yml`'s cadeia
`main→prd`/`hmg→main` **não tem** a mesma checagem explícita de `head.repo`. Isso não é introduzido
por esta Sprint e não foi corrigido aqui (tocar `deploy-prd.yml` está fora do escopo "Sprint →
HMG" desta Sprint) — registrado como débito técnico (§35).

## 18. Failure Semantics

| Condição | Comportamento |
|---|---|
| Nenhum PR associado ao commit de merge | `core.setFailed`, artifact não baixado, deploy não ocorre |
| PR encontrado mas `head.repo` não é este repositório | `core.setFailed` — recusa confiar no artifact |
| Nenhum run `ci.yml` bem-sucedido para o `head_sha` validado | `core.setFailed` após varrer até 2000 runs |
| Múltiplos PRs associados ao commit (raro) | Usa o primeiro com `base.ref == 'hmg'` — mesma tolerância que `deploy-prd.yml` já aceita para seu próprio matching |
| Artifact expirado/corrompido | `download-artifact` falha nativamente (erro do runner) — job falha, nenhum fallback |
| `run-id` ambíguo | Não aplicável — `run-id` é sempre um valor único resolvido deterministicamente |

Nenhum fallback silencioso para rebuild, "deploy latest", ou "deploy previous" foi implementado —
consistente com a Seção 28 do prompt ("DEPLOYMENT MUST FAIL CLOSED").

## 19. BeeDay CI Before × After

| | Before | After |
|---|---|---|
| Triggers | `push: hmg`, `pull_request: [hmg, main]`, `workflow_dispatch` | `pull_request: [hmg, main]`, `workflow_dispatch` |
| Execuções por merge em `hmg` | 2 (`pull_request` da PR + `push` pós-merge) | 1 (`pull_request` da PR) |
| Produz artifact para `deploy-hmg.yml`? | Sim (run `push`) | Sim (run `pull_request`, reaproveitado) |
| Nome do workflow | `BeeDay CI` | **Inalterado** (ver §25) |

## 20. HMG Deployment Before × After

| | Before | After |
|---|---|---|
| Trigger | `workflow_run` (BeeDay CI, `event=='push'`) + `workflow_dispatch` | `push: hmg` direto + `workflow_dispatch` |
| Resolução do artifact | `run-id` do `workflow_run` (ou "latest successful on hmg" no dispatch) | Cadeia de proveniência via PR (uniforme para push e dispatch) |
| Checkout ref | `workflow_run.head_sha \|\| github.sha` (ternário) | `github.sha` (checkout padrão, sem ternário) |
| Dependência de `workflow_run.head_sha` (metadado já provado não-confiável, `06-...md` §6.1) | Sim | **Eliminada** |
| `deployment-info.json` | `sha`, `workflowRun`, `result`, `environment`, `timestampUtc` | + `sourceSha`, `mergeSha`, `pullRequest`, `validationRunId` |

## 21. HMG Verification Impact

`FACT`: nenhuma mudança de responsabilidade — `verify-hmg.yml` continua fazendo Readiness + Smoke
contra o ambiente real, disparado por `workflow_run` em `BeeDay — HMG Deployment`. Única mudança: o
campo lido do manifest (`sha` → `sourceSha`, §20). Nenhum check removido.

## 22. Ruleset Impact Matrix

| Branch | Required checks | Impact 19.8 |
|---|---|---|
| `hmg` (20580759) | `BeeDay CI` | Nenhum — `ci.yml` continua produzindo o check `"BeeDay CI"` via `pull_request`, inalterado |
| `main` (20608232) | `BeeDay CI`, `Validate Promotion` | Nenhum — `pull_request: main` não tocado (débito separado, `08-fast-pr-validation-decision.md`) |
| `prd` | Nenhum Ruleset | Nenhum |

`allowed_merge_methods` de ambos: `["merge", "squash", "rebase"]` — confirmado, motivou a escolha
de estratégia (§5). Nenhuma mutação de Ruleset foi executada ou solicitada nesta Sprint.

## 23. Workflow Rename Decision

**Não renomeado.** `"BeeDay CI"` é o `context` literal exigido pelo `required_status_checks` de
**ambos** os Rulesets (`hmg` e `main`, §22) — confirmado via `gh api .../rulesets/*` antes de
qualquer decisão, conforme exigido pela Seção 18 do prompt. Mesmo com `push: hmg` removido,
`ci.yml` continua responsável por `pull_request: main` (débito separado, não resolvido por esta
Sprint — ver `08-fast-pr-validation-decision.md` §6, atualizado nesta Sprint no §35 abaixo), então
o nome `BeeDay — Pull Request Validation` ainda seria materialmente impreciso mesmo que o rename
em si não quebrasse nenhum Ruleset. Rename permanece bloqueado até a ativação do Release Quality
Gate (Sprint 19.7's sequência de ativação, não desta Sprint).

## 24. Implementation Summary

1. `deploy-hmg.yml`: trigger `workflow_run` (BeeDay CI) substituído por `push: branches: [hmg]`
   direto; nova step de resolução de proveniência via API de PRs do GitHub (substitui a resolução
   por `workflow_run`/`"latest successful run"`); checkout simplificado (sem ternário); manifest
   estendido; permissão `pull-requests: read` adicionada (mínima, já usada por `deploy-prd.yml`).
2. `verify-hmg.yml`: campo `sha` → `sourceSha` no consumo do manifest.
3. `ci.yml`: trigger `push: branches: [hmg]` removido.
4. `scripts/iis-control/Request-BeeDayIisControlPromotion.ps1`: comentário corrigido para refletir
   que o checkout agora reflete a ponta de `hmg`, não literalmente "o commit que BeeDay CI validou"
   (nunca foi uma checagem de segurança — apenas metadado de auditoria).

## 25. Files Modified / Created

- `.github/workflows/deploy-hmg.yml` (modificado)
- `.github/workflows/verify-hmg.yml` (modificado)
- `.github/workflows/ci.yml` (modificado)
- `scripts/iis-control/Request-BeeDayIisControlPromotion.ps1` (comentário apenas)
- `docs/deployment/12-artifact-provenance.md` (novo — este documento)
- `docs/deployment/README.md` (atualizado — novo documento indexado)
- `docs/deployment/01-deployment.md` (atualizado — §2/§4.1 refletem o novo trigger/resolução)
- `docs/deployment/08-fast-pr-validation-decision.md` (atualizado — nota de resolução no §6)
- `docs/deployment/10-hmg-deployment-verification.md` (atualizado — nota de resolução no §22/§26, histórico preservado)

Nenhum workflow novo criado (§26 do prompt) — a responsabilidade coube inteiramente a
`deploy-hmg.yml`, já dono da resolução de artifact. Nenhum script novo criado — a lógica de
resolução é inline (`actions/github-script`), consistente com o padrão já usado por
`deploy-prd.yml` (nenhum módulo compartilhado existe para esse padrão no repositório).

## 26. Documentation Updated

Ver §25. Nenhum baseline histórico (`06-cicd-pipeline-discovery-baseline.md`) reescrito — apenas
documentos que descrevem o estado *atual* do pipeline foram atualizados, com notas de resolução
adicionadas (não substituição) aos documentos que registraram a dívida originalmente
(`08-fast-pr-validation-decision.md`, `10-hmg-deployment-verification.md`), seguindo o padrão já
estabelecido pela própria 19.6/19.7 para o mesmo tipo de atualização.

## 27. Commands Executed

```text
git status / git branch --show-current / git rev-parse HEAD / git fetch --prune
git log --oneline --decorate --graph -20
gh pr view 60 --json ...
git log -1 --format="%P" b396026
gh run list --workflow=ci.yml/deploy-hmg.yml --json ...
gh api repos/tiagoarrigoni/BeeDay/actions/workflows
gh api repos/tiagoarrigoni/BeeDay/rulesets
gh api repos/tiagoarrigoni/BeeDay/rulesets/20580759
gh api repos/tiagoarrigoni/BeeDay/rulesets/20608232
python -c "import yaml; yaml.safe_load(...)"  (deploy-hmg.yml, verify-hmg.yml, ci.yml)
PowerShell Parser.ParseFile (Request-BeeDayIisControlPromotion.ps1)
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx -c Release --warnaserror
dotnet test BeeDay.slnx -c Release --no-build
git diff --check / git status
```

## 28. Local Validation Results

Ver §29 do relatório da Sprint (seção correspondente na resposta final) — nenhuma alteração de
código C#/Domain/Application/Infrastructure/Web ocorreu nesta Sprint (apenas YAML/PowerShell
comentário/Markdown), então a suíte .NET não é afetada por esta mudança, mas é executada mesmo
assim por disciplina de validação obrigatória (`CLAUDE.md` §8).

## 29. Negative Validation Results

`FACT`, raciocinado e documentado (mutação remota destrutiva não foi executada, conforme Seção 38
do prompt: "não faça mutações remotas destrutivas apenas para testar"):

| Cenário negativo | Mecanismo que rejeita | Verificado como |
|---|---|---|
| SHA errado (commit sem PR associado) | `listPullRequestsAssociatedWithCommit` retorna vazio → `core.setFailed` | Leitura de código (lógica determinística, sem branch não testada) |
| Artifact ausente (run sem sucesso para o `head_sha`) | Paginação exaustiva sem match → `core.setFailed` | Leitura de código |
| PR de fork | Checagem `head.repo.full_name` → `core.setFailed` | Leitura de código |
| Run ambíguo (múltiplos matches) | `.find()` usa o primeiro — mesmo comportamento tolerado por `deploy-prd.yml` já em produção | Comparação direta com padrão existente |

`UNKNOWN`: nenhum teste negativo foi executado **remotamente** (não há como simular com segurança
um "PR de fork" real contra o repositório sem criar um fork de fato — fora de escopo e
potencialmente uma mutação não solicitada). A verificação remota real (§30) cobrirá apenas o
caminho positivo.

## 30. Remote Validation Status

**`NOT YET VALIDATED REMOTELY`.** Depende de commit/push autorizados (aguardando aprovação, ver
relatório final) e da observação de uma PR real `sprint/19.8-artifact-provenance → hmg` e seu
merge. Critério de sucesso: exatamente 1 execução de `BeeDay CI` por merge (não 2), seguida de
`BeeDay — HMG Deployment` bem-sucedido resolvendo o artifact da execução `pull_request`.

## 31. Before × After Timing

Baseado no caso real de evidência (§3) — não uma promessa, uma medição de um merge real:

| | Before (medido) | After (esperado, não validado remotamente) |
|---|---|---|
| `BeeDay CI` (PR validation) | ~6.0 min | ~6.0 min (inalterado) |
| `BeeDay CI` (post-merge, `push:hmg`) | ~6.6 min | **0 min — eliminado** |
| Total de CI por merge | ~12.6 min | ~6.0 min |
| Deploy trigger delay | Aguarda o run `push` completar (~6.6 min) antes de sequer começar a resolver o artifact | Dispara imediatamente no `push` — resolução de proveniência é rápida (chamadas de API, segundos) |

## 32. Duplicate Work Eliminated

`FACT` (medido, §3, um merge real) + `RECOMMENDATION` (generalização): a cada merge `sprint/*→hmg`,
~6.6 minutos de Format+Build+5 suítes de teste+E2E+Publish+EF bundle deixam de ser reexecutados
para conteúdo já validado. Nenhuma validação foi removida — a suíte completa continua rodando
exatamente uma vez por mudança, na PR.

## 33. Remaining Technical Debt

- **`BeeDay.slnx` Release configuration behavior** — dívida pré-existente (`09-...md` §27.10,
  `11-...md` §23.9), reconfirmada não corrigida, não relacionada a este Sprint.
- **Release Quality Gate Ruleset transition** — sequência de ativação (`11-...md` §23.11) segue
  pendente, não tocada por esta Sprint.
- **Production artifact promotion** — `deploy-prd.yml` já implementa Build Once/Deploy Many
  completo (dois hops); esta Sprint resolve o hop equivalente para `hmg`, fechando a lacuna que
  impedia dizer que a cadeia inteira `hmg→main→prd` (mais agora `sprint→hmg`) segue o mesmo
  princípio. Nenhuma mudança em `deploy-prd.yml` foi necessária ou feita.
- **`deploy-prd.yml` não verifica `head.repo` da cadeia `main→prd`/`hmg→main`** (§17) — achado,
  não corrigido, fora do escopo "Sprint → HMG" desta Sprint.
- **`pull_request: main` em `ci.yml`** — permanece como responsabilidade emprestada até a ativação
  do Release Quality Gate (inalterado por esta Sprint, débito documentado desde a 19.4/19.6/19.7).

## 34. Fontes consultadas

- `.github/workflows/ci.yml`, `deploy-hmg.yml`, `verify-hmg.yml`, `deploy-prd.yml`,
  `validate-promotion.yml` (lidos/escritos integralmente).
- `scripts/iis-control/Request-BeeDayIisControlPromotion.ps1`.
- `gh pr view 60`, `gh run list --workflow=ci.yml/deploy-hmg.yml`, `gh api
  repos/.../actions/workflows`, `gh api repos/.../rulesets/*` (evidência real, 2026-08-11).
- `git log`/`git show` (topologia de commits real, PR #60).
- `CLAUDE.md` §5.7.2 (Build Once, Deploy Many — princípio já formalizado, generalizado aqui).
- `docs/deployment/06-cicd-pipeline-discovery-baseline.md` §6.1 (achado sobre `workflow_run.head_sha`
  não confiável, reaplicado para justificar a remoção do ternário em `deploy-hmg.yml`).
- `docs/deployment/08-fast-pr-validation-decision.md`, `10-hmg-deployment-verification.md`
  (dívida original documentada, agora resolvida).
- `docs/deployment/11-release-quality-gate.md` (Ruleset de `main`, sequência de ativação — não
  tocada).

---

## 35. Sprint 19.8.1 — Remote Deployment Summary Correction

**Fonte da verdade:** `gh run view 31456637128` (jobs, steps, log bruto real); reprodução local
com `powershell.exe` (Windows PowerShell 5.1, não `pwsh`); leitura integral de `deploy-hmg.yml`,
`verify-hmg.yml`.

**Escopo:** corrigir exclusivamente a falha pós-deployment em `Record deployment info`. Nenhuma
mudança na arquitetura de proveniência validada nesta mesma Sprint 19.8 (§13 acima).

### 35.1 Run remoto analisado

| Campo | Valor |
|---|---|
| Workflow | `BeeDay — HMG Deployment` (run `31456637128`) |
| Trigger | `push`, merge SHA `80546a7` (PR #61, branch `sprint/19.8-artifact-provenance`) |
| Conclusion | `failure` |
| Source SHA validado | `3a9d8e8` |
| BeeDay CI run resolvido | `31456327054` |

### 35.2 O que passou (deployment real)

`FACT`, confirmado via `gh run view --json jobs`: steps 1-8 (`Set up job` até `Deploy to IIS with
rollback`) todos `success`. Log bruto confirma: migrations aplicadas (`No migrations were applied.
The database is already up to date.` seguido de `Migrations applied successfully.`), IIS
STOP/CONFIGURE/START todos `exitCode=0`, readiness OK, `Deployment completed successfully.`, tempo
total `00:00:36.0350676`.

**`DEPLOYMENT = SUCCESS`** — o mecanismo de proveniência da Sprint 19.8 (resolução de PR, SHA
validado, run de `BeeDay CI` verde, download de artifacts por `run-id`) funcionou integralmente em
produção real. Isto não é reavaliado ou redesenhado nesta Sprint.

### 35.3 O que falhou

Step 9 (`Record deployment info`): `failure`. Step 10 (`Upload deployment info`): `skipped` (efeito
direto — nunca executa após falha do step anterior sem `if: always()`).

### 35.4 Root Cause

`SYMPTOM`: workflow termina com `conclusion: failure`, mesmo com deployment real bem-sucedido.

`IMMEDIATE FAILURE`: `gh run view --log` mostra o script gerado pelo runner contendo:

```text
+ "## BeeDay â€” HMG Deployment" >> $env:GITHUB_STEP_SUMMARY
+                ~~~
Unexpected token 'HMG' in expression or statement.
```

seguido de erros em cascata (`Expressions are only allowed as the first element of a pipeline.`,
`Missing expression after unary operator '-'.`) em todas as linhas subsequentes do bloco.

`TECHNICAL CAUSE`: o caractere em dash (`—`, U+2014) presente no literal PowerShell
`"## BeeDay — HMG Deployment"` (fonte YAML) foi escrito pelo runner do GitHub Actions no arquivo
`.ps1` temporário como UTF-8. O shell real deste job — confirmado no próprio log
(`shell: C:\Windows\System32\WindowsPowerShell\v1.0\powershell.EXE`) — é **Windows PowerShell
5.1**, não `pwsh`. Ao ler um arquivo `.ps1` sem BOM, o PowerShell 5.1 usa o codepage legado do
sistema (não UTF-8) para decodificar o conteúdo — diferente do `pwsh` (PowerShell 7+), que assume
UTF-8 por padrão. Os 3 bytes UTF-8 do em dash (`0xE2 0x80 0x94`) decodificados byte-a-byte nesse
codepage produzem `â€”` — três caracteres distintos, o último dos quais (`0x94` → U+201D, ASPAS
CURVAS DUPLAS DE FECHAMENTO) é tratado pelo tokenizer do PowerShell como delimitador de string
válido (PowerShell aceita aspas curvas como equivalentes às aspas retas `"` por compatibilidade com
texto colado de editores rich-text). Isso fecha a string prematuramente logo após `â€`, deixando
`HMG Deployment" >> $env:GITHUB_STEP_SUMMARY` como tokens soltos fora de qualquer string — a causa
exata de `Unexpected token 'HMG'` e de toda a cascata de erros nas linhas seguintes (o desbalanço
de aspas corrompe o parsing do restante do script).

`ROOT CAUSE`: um caractere Unicode não-ASCII (em dash) dentro de um literal de string PowerShell
executado via `shell: powershell` (Windows PowerShell 5.1) em um runner que grava o script como
UTF-8 sem BOM — combinação que não existe em `shell: pwsh` (PowerShell 7+, que lê UTF-8
corretamente por padrão) nem em texto fora de literais de string (comentários `#` são ignorados
independentemente do encoding, pois não abrem nem fecham nada).

**Reproduzido localmente, não apenas inferido:** o mesmo conteúdo (em dash, escrito como UTF-8 sem
BOM, executado via `powershell.exe` real) produziu exatamente `Token 'HMG' inesperado na expressão
ou instrução` — a mesma classe de erro do log remoto, na mesma posição. Ver §35.7.

### 35.5 Windows PowerShell 5.1 Analysis

`FACT`: confirmado que o runner real usa `powershell.exe` (Windows PowerShell 5.1), não `pwsh`,
para todo step com `shell: powershell` neste workflow — evidência direta do log
(`shell: C:\Windows\System32\WindowsPowerShell\v1.0\powershell.EXE -command ". '{0}'"`). A
correção não pode assumir comportamento de PowerShell 7. `ConvertTo-Json`/`Set-Content` (§35.6) e a
sintaxe `>>` para `$env:GITHUB_STEP_SUMMARY` já são compatíveis com PS 5.1 (usadas há múltiplas
Sprints sem erro relacionado a elas mesmas) — o defeito é exclusivamente do caractere não-ASCII, não
desses mecanismos.

### 35.6 Deployment Info Contract

`FACT`, confirmado por leitura de código antes de qualquer alteração: `deployment-info.json`
carrega `sourceSha`, `mergeSha`, `pullRequest`, `validationRunId`, `workflowRun`, `result`,
`environment`, `timestampUtc` — todos os 8 campos definidos na Sprint 19.8, nenhum removido ou
renomeado nesta Sprint corretiva. `verify-hmg.yml`'s step `Read deployed SHA` consome
`$info.sourceSha` (Sprint 19.8) — inalterado por esta Sprint.

### 35.7 JSON Encoding Analysis

`FACT`, verificado empiricamente com `powershell.exe` real (não apenas lido): `$info | ConvertTo-Json
| Set-Content -LiteralPath $infoPath` já produzia (e continua produzindo, sem alteração) JSON
válido — reproduzido localmente, com o `deployment-info.json` resultante relido com sucesso via
`ConvertFrom-Json`, todos os 8 campos preservados corretamente. Este mecanismo **não fazia parte do
defeito** e não foi alterado.

### 35.8 Step Summary Analysis

Reproduzido localmente com `powershell.exe` (Windows PowerShell 5.1 real, arquivo `.ps1` escrito
como UTF-8 sem BOM, mesma condição do runner real):

| Cenário | Comando | Resultado |
|---|---|---|
| Conteúdo ANTES da correção (em dash) | `powershell.exe -File old.ps1` | **FALHA**: `Token 'HMG' inesperado na expressão ou instrução` — reproduz exatamente o erro remoto |
| Conteúdo DEPOIS da correção (hífen ASCII) | `powershell.exe -File fixed.ps1` | **Sucesso** (exit 0) — Markdown gerado corretamente, todas as 8 linhas da tabela presentes, backticks/links preservados |

### 35.9 Fix Implemented

`deploy-hmg.yml` (step `Record deployment info`) e `verify-hmg.yml` (step `Record verification
summary`): `"## BeeDay — HMG Deployment"`/`"## BeeDay — HMG Verification"` → `"## BeeDay - HMG
Deployment"`/`"## BeeDay - HMG Verification"` (em dash → hífen ASCII). Comentário explicativo
adicionado a cada step, citando a evidência real (run `31456637128`) e proibindo reintrodução do
caractere. Auditoria completa de ambos os blocos (script Python, varredura de código de ponto >127
em cada linha do step) confirma **nenhum outro caractere não-ASCII** restante em nenhum dos dois
blocos corrigidos.

**Por que `verify-hmg.yml` foi incluído**, mesmo o prompt desta Sprint focando em
`deploy-hmg.yml`: o mesmo padrão exato (`"## BeeDay — HMG Verification" >> $env:GITHUB_STEP_SUMMARY`)
existe em `verify-hmg.yml`, ainda não exercido remotamente porque `deploy-hmg.yml` falhou antes de
`verify-hmg.yml` sequer disparar. Corrigir apenas `deploy-hmg.yml` garantiria que a **próxima**
execução bem-sucedida (produzida exatamente por esta correção) travasse imediatamente no mesmo
defeito em `verify-hmg.yml` — o que violaria diretamente o critério de aceite "`verify-hmg.yml`
continua compatível". Reportado explicitamente aqui, não corrigido silenciosamente.

**Achado relacionado, não corrigido (fora do escopo desta Sprint):** `release-quality-gate.yml`
(step `Record gate summary`, linha 237) tem o **mesmo defeito exato**
(`"## BeeDay — Release Quality Gate" >> $env:GITHUB_STEP_SUMMARY`, também `shell: powershell`).
Esse workflow nunca executou remotamente (nenhuma PR `hmg→main` existiu até hoje — ver
`11-release-quality-gate.md`), então o defeito é latente, não confirmado em produção, e pertence a
uma fronteira (`hmg→main`) fora do escopo desta Sprint corretiva (que trata exclusivamente de
`Sprint → HMG`). Registrado como débito técnico (§35.13) para correção na próxima vez que aquele
workflow for tocado — deliberadamente não corrigido aqui para não expandir o escopo desta Sprint
sem necessidade comprovada por uma falha real observada naquele workflow especificamente.

### 35.10 Why This Fix Is Minimal

Nenhuma mudança de arquitetura, mecanismo de resolução de proveniência, contrato de artifact, ou
lógica de negócio. Duas linhas de string literal alteradas (caractere único cada), mais comentários
explicativos. Nenhuma migração para `shell: pwsh` (rejeitada — sem evidência de necessidade além
deste caractere específico, e mudaria o shell de todos os outros steps do mesmo job
desnecessariamente).

### 35.11 Provenance Preservation

`FACT`: nenhuma linha do step `Resolve validated source commit and BeeDay CI run` foi tocada —
busca de PR, validação de mesmo-repositório, paginação de `listWorkflowRuns`, match de `head_sha`,
`run-id`, download de artifacts, política fail-closed — todos idênticos ao estado pós-19.8.
`push: hmg` **não foi reintroduzido** em `ci.yml` (confirmado: arquivo não tocado nesta Sprint).

### 35.12 Artifact Upload Contract

`FACT`, verificado por leitura de código: `Upload deployment info` (nome `beeday-hmg-deployment-info`,
path `${{ runner.temp }}\DeploymentInfo`, `retention-days: 14`) não foi alterado — aponta para o
mesmo caminho que `Record deployment info` continua produzindo (`$infoPath` inalterado).

### 35.13 Remaining Debt (desta Sprint)

- `release-quality-gate.yml` tem o mesmo defeito latente (§35.9) — não corrigido, recomendado para
  a próxima Sprint que tocar aquele workflow ou uma corretiva dedicada antes de sua primeira
  execução remota real.
- Dívidas pré-existentes não relacionadas (inalteradas): `BeeDay.slnx` Release configuration
  behavior; ativação do Ruleset do Release Quality Gate; promoção final a PRD.

### 35.14 Remote Validation Status

**`NOT YET VALIDATED REMOTELY`.** Depende de commit/push autorizados e de um novo push real em
`hmg` (merge desta correção) produzindo uma execução completa de `deploy-hmg.yml` até `Upload
deployment info`, seguida da primeira execução real de `verify-hmg.yml`.

**Atualização (validado posteriormente):** `gh run list --workflow=deploy-hmg.yml` confirma runs
`push`/`hmg` bem-sucedidos após o merge desta correção (ex.: `31480023629`, `31481910386`), ambos
completando `Record deployment info`/`Upload deployment info` sem erro — a correção da 19.8.1 está
**`REMOTE VALIDATED`**.

---

## 36. Sprint 19.8.3 — HMG Verification Provenance Trigger Hardening

**Fonte da verdade:** `gh run view`/`gh run list` (evidência real de múltiplos runs),
`git show`/`git log` (conteúdo real de `deploy-hmg.yml` em `main` antes da PR #64), leitura
integral de `deploy-hmg.yml`/`verify-hmg.yml` no estado atual.

**Escopo:** endurecer a fronteira `HMG Deployment → HMG Verification` para que `verify-hmg.yml`
nunca tente validar um deployment cujo próprio evento de disparo seja incompatível com o contrato
atual — mantendo comportamento fail-closed. Nenhuma mudança em artifact provenance, Release
Quality Gate, ou Rulesets.

### 36.1 Incidente real

`FACT`: `BeeDay — HMG Verification` (run `31482750014`, disparado logo após a PR #64 `hmg→main`)
falhou no step `Download deployment info` — `Artifact not found for name:
beeday-hmg-deployment-info`. Steps seguintes (`Read deployed SHA`, `Verify Readiness`, `Run Smoke
Tests`) `skipped`. `Record verification summary` (`if: always()`) executou normalmente, reportando
`Job result: failure`.

### 36.2 Cadeia causal reconstruída (comprovada, não presumida)

`EVIDENCE`: `gh run view 31482750014 --log` confirma o resolver usou
`context.payload.workflow_run.id` cegamente: `Using triggering workflow_run id 31482602788`.

`EVIDENCE`: `gh run view 31482602788` mostra `event: "workflow_run"`, `headBranch: "main"`,
`headSha: edc41c8` (tip **anterior** de `main`, antes da PR #64), job `"Deploy to SERV3WEB"`, step
`"Resolve BeeDay CI run to deploy"` — nomes que **não existem** na versão atual do arquivo.

`FACT`: `MSYS_NO_PATHCONV=1 git show "edc41c8:.github/workflows/deploy-hmg.yml"` confirma que essa
era a versão **legada** (`name: BeeDay Homologation Deploy`, gatilho `workflow_run: [BeeDay CI]`
com guard `head_branch == 'hmg'`, sem nenhum step `Record deployment info`/`Upload deployment
info` — o contrato de `beeday-hmg-deployment-info` não existia nessa versão, introduzido só na
Sprint 19.6 e estendido na 19.8).

`FACT`: `git log --oneline edc41c8` mostra que o commit anterior de `main` era `d5b9390` (PR #48),
**anterior a toda a EPIC 19** (Sprint 19.1 em diante) — `main` nunca recebeu nenhuma mudança da
EPIC 19 até a PR #64, a primeira promoção `hmg→main` real desta EPIC.

`EVIDENCE`: `gh run list --workflow=ci.yml` mostra a run `31482098907` (`pull_request`,
`head_branch=hmg`, `head_sha=9889ebd`, `success`, `10:24:59Z`) — a validação `BeeDay CI` da própria
PR #64. `deploy-hmg.yml` legado em `main` (`workflow_run: [BeeDay CI]` + `head_branch=='hmg'`)
disparou ao completar essa run (`10:31:44Z`, antes do merge em `10:32:56Z`), porque o GitHub
Actions resolve listeners de `workflow_run` usando a cópia do workflow registrada na branch
**default** (`main`) no momento do evento — não a cópia de `hmg` ou de qualquer commit específico.
`main` ainda tinha a versão legada registrada, pois o merge da PR #64 só ocorreu **depois**.

**Cadeia completa:**

```text
PR #64 (hmg->main) aberta
  -> ci.yml pull_request:main dispara "BeeDay CI" (head_branch=hmg, run 31482098907)
  -> "BeeDay CI" completa (10:31:xx, ANTES do merge)
  -> GitHub resolve listeners de workflow_run:[BeeDay CI] usando main (default branch) NAQUELE
     MOMENTO -> main ainda tem deploy-hmg.yml LEGADO (name: BeeDay Homologation Deploy)
  -> legado dispara, guard head_branch=='hmg' satisfeito -> deploy real executado com sucesso
     (run 31482602788) mas SEM produzir beeday-hmg-deployment-info (contrato não existe nessa
     versão)
  -> PR #64 mergeada (10:32:56Z) -> main agora tem o deploy-hmg.yml ATUAL
  -> verify-hmg.yml (já registrado via hmg, escuta por NOME "BeeDay — HMG Deployment", nome
     idêntico em toda versão do arquivo) casa com a conclusão do run legado -> tenta baixar
     beeday-hmg-deployment-info do run 31482602788 -> FALHA (artifact nunca existiu)
```

### 36.3 Achado adicional: duplicação de deployment não relatada no prompt

`FACT`, descoberto durante a investigação, não hipótese: `gh run list --workflow=deploy-hmg.yml`
mostra que esse mesmo mecanismo legado disparou **repetidamente** ao longo de toda a sessão
sempre que uma execução de `BeeDay CI` teve `head_branch=='hmg'` (a maioria `skipped`, pois PRs de
sprint têm `head_branch` diferente de `hmg` — mas **duas** vezes resultou em deployment real e
bem-sucedido, redundante ao deployment oficial: runs `31455318044` (03:24:56Z, coincidindo com o
push:hmg da Sprint 19.7) e `31482602788` (o próprio incidente desta Sprint). Ambos deployaram o
mesmo conteúdo (mesmos artifacts `beeday-publish`/`beeday-migrations`, produzidos por `ci.yml`
independente de qual `deploy-hmg.yml` os consome) via automação duplicada — não uma inconsistência
de conteúdo, mas execução redundante e, agora, a causa direta da falha de `verify-hmg.yml`.

**Confirmado eliminado após a PR #64:** nenhuma execução `workflow_run`/`headBranch=main` ocorreu
desde `10:32:56Z` (quando `main` passou a ter o `deploy-hmg.yml` atual, que não escuta mais
`workflow_run` nenhum). `main` sincronizada com `hmg` remove esse listener legado por completo.

### 36.4 Historical Trigger Analysis / Incident Classification

| Cenário | Classificação |
|---|---|
| A. Continua reproduzível no estado atual | **NÃO** — `main` e `hmg` compartilham hoje o mesmo `deploy-hmg.yml` (`push: hmg` direto, sem `workflow_run` algum); o listener legado que causou o incidente não existe mais em nenhuma branch |
| B. Efeito transitório da promoção que atualizou os workflows | **SIM** — a condição habilitadora (main sem NENHUMA mudança da EPIC 19 desde antes da Sprint 19.1) é um evento único, não recorrente na mesma escala |
| C. Fragilidade estrutural | **SIM, residual** — `verify-hmg.yml` confiava incondicionalmente em `context.payload.workflow_run.id` sem checar se o evento que disparou o deployment observado era um dos eventos que `deploy-hmg.yml` realmente suporta hoje; isso permanece uma fraqueza genérica mesmo que ESTE caminho específico de recorrência esteja fechado |
| D. Combinação B + C | **`SELECIONADO`** — o incidente foi majoritariamente transitório (não pode recorrer via este exato mecanismo), mas a fragilidade de confiança irrestrita em `workflow_run.id` é real e vale a pena fechar com uma correção barata e comprovadamente segura |

### 36.5 Deployment Info Contract (confirmado inalterado)

`FACT`: `deployment-info.json` continua com os 8 campos da Sprint 19.8 (`sourceSha`, `mergeSha`,
`pullRequest`, `validationRunId`, `workflowRun`, `result`, `environment`, `timestampUtc`), produzido
apenas por `deploy-hmg.yml` no step `Record deployment info`, artifact `beeday-hmg-deployment-info`,
`retention-days: 14`. `verify-hmg.yml` usa `run-id: ${{ steps.resolve-run.outputs.result }}`
corretamente — o mecanismo de download em si nunca esteve errado; o problema era **qual** `run-id`
chegava até ele.

### 36.6 Estratégias avaliadas

| Estratégia | Classificação | Motivo |
|---|---|---|
| A — nenhuma alteração funcional | `REJECTED` | Embora o caminho exato não possa recorrer, a fragilidade de confiança irrestrita em `workflow_run.id` (§36.4-C) é real, barata de fechar, e diretamente relacionada ao incidente — documentação sozinha deixaria a mesma classe de fragilidade sem tratamento |
| B — guard no HMG Verification | `SUPPORTED` — **selecionada** | Usa uma propriedade confiável e já comprovada pela evidência (`workflow_run.event`, campo padrão do payload, não uma heurística) para recusar runs cujo evento de origem não é um dos que `deploy-hmg.yml` produz hoje (`push`/`workflow_dispatch`) |
| C — contract marker adicional | `UNNECESSARY` | `workflow_run.event` já é uma propriedade suficiente e confiável — introduzir um marcador novo no artifact duplicaria informação sem consumidor adicional |
| D — trigger hardening no `on:` | `REJECTED` (tecnicamente inviável) | GitHub Actions não suporta filtrar `workflow_run` pelo evento que disparou o run observado diretamente na chave `on:` — esse filtro só é possível na expressão `if:` do job, que é exatamente a Estratégia B |

### 36.7 Selected Strategy / Why This Is The Minimum Safe Fix

Estratégia B. Uma alteração: o `if:` do job `verify` em `verify-hmg.yml` passa a exigir
`github.event_name == 'workflow_run'` explicitamente, `conclusion == 'success'`, e
`github.event.workflow_run.event == 'push' || github.event.workflow_run.event ==
'workflow_dispatch'`. A checagem explícita de `github.event_name` foi adicionada após revisão do
usuário — funcionalmente equivalente à forma anterior (que dependia do acesso null-safe de
expressões do GitHub Actions, já usado em outros pontos deste repositório, ex.:
`deploy-prd.yml`'s `github.event.pull_request.head.sha || github.sha`), mas mais explícita e
autodocumentada. Mínima porque:

- usa um campo já presente no payload padrão do GitHub Actions, sem lógica nova de resolução;
- não introduz artifact, manifest, ou consulta à API adicional;
- não altera o mecanismo de download nem a leitura do JSON;
- não afeta o caminho `workflow_dispatch` (primeira cláusula do `if:`, inalterada);
- não enfraquece o fail-closed de um deployment `push`-legítimo com artifact realmente ausente —
  esse caso continua falhando no step `Download deployment info` exatamente como antes.

### 36.8 Changes Implemented

`verify-hmg.yml`, job `verify`, bloco `if:` — condição adicional sobre `workflow_run.event`.
Comentário substituído (o anterior descrevia o guard antigo de `deploy-hmg.yml`, pré-19.8, e
estava desatualizado) por um novo que documenta o incidente, a cadeia causal, e a justificativa da
correção.

### 36.9 Fail-Closed Behavior (verificado, não apenas assumido)

| Cenário | Resultado do job `verify` |
|---|---|
| `workflow_dispatch` manual | Roda (inalterado) |
| Deployment `push`-triggered, `success`, artifact presente | Roda, valida normalmente |
| Deployment `push`-triggered, `success`, artifact ausente (defeito real futuro) | Roda, falha em `Download deployment info` — **inalterado, ainda fail-closed** |
| Deployment `workflow_run`-triggered (legado/indireto, como o incidente) | **`skipped`** — job nunca inicia, nunca produz um `Download` falho nem um falso positivo |
| Deployment com `conclusion != 'success'` | `skipped` (comportamento pré-existente, inalterado) |

Em nenhum cenário o verification passa a validar um deployment diferente do que foi observado, nem
mascara ausência de provenance como sucesso.

### 36.10 Artifact Provenance Preservation

`FACT`: `deploy-hmg.yml` não foi tocado nesta Sprint (apenas lido/consultado). Resolução de PR,
validação de mesmo-repositório, paginação, match de `head_sha`, `run-id`, download de artifacts —
todos inalterados. `push: hmg` continua ausente de `ci.yml`.

### 36.11 Local Validation

YAML validado (`python -c yaml.safe_load`); lógica do `if:` verificada manualmente contra os 4
cenários da tabela §36.9; sintaxe de expressão idêntica a um padrão já usado e executado com
sucesso em versões anteriores de `deploy-hmg.yml` (parênteses aninhados com `&&`/`||`).

### 36.12 Remote Validation Status

**`NOT YET VALIDATED REMOTELY`.** A validação real ocorrerá na próxima vez que um deployment
`workflow_run`-triggered incompatível surgir (cenário raro agora que `main`/`hmg` estão
sincronizadas) — não há forma de testar remotamente o caminho negativo sem recriar
artificialmente a condição legada, o que não foi feito. O caminho positivo (deployment `push`
normal) será revalidado organicamente no próximo merge em `hmg`.

### 36.13 Remaining Debt

Nenhuma dívida nova. Débitos pré-existentes inalterados: `BeeDay.slnx` Release configuration
behavior; promoção final a PRD; achado não corrigido do Sprint 19.8 sobre `deploy-prd.yml` não ter
checagem de `head.repo` equivalente à de `deploy-hmg.yml`.
