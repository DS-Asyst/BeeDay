# HMG Continuous Deployment & Verification (EPIC 19, Sprint 19.6)

**Fonte da verdade:** verificado diretamente em `.github/workflows/ci.yml`, `deploy-hmg.yml`,
`verify-hmg.yml` (novo), `scripts/Deploy-BeeDay.ps1`, `src/BeeDay.Web/Program.cs`
(`MapHealthChecks`), `docs/deployment/03-observability.md`, `docs/deployment/05-privileged-iis-control.md`.
Descoberta de SERV3WEB nesta Sprint é **exclusivamente baseada no repositório** — nenhum acesso
direto ao servidor foi feito ou estava disponível nesta sessão; ver §7 para a distinção explícita
entre Repository State e Runtime State (`CLAUDE.md` §8.2).

**Última verificação:** 2026-08-11.

**Escopo:** eliminar a causa estrutural do deployment duplicado em HMG (comprovada na Sprint 19.1),
e introduzir `BeeDay — HMG Verification` como responsabilidade explícita e separada de
`BeeDay — HMG Deployment`. Nenhuma mudança em SERV3WEB, `deploy-prd.yml`, Release Quality Gate, ou
provenance final.

**Classificação de evidência:** `FACT`, `MEASUREMENT`, `INFERENCE`, `RECOMMENDATION`, `UNKNOWN`.

---

## 1. Before

`FACT`, reconfirmado nesta Sprint contra o estado atual do repositório (não só reaproveitado da
19.1):

```
push -> hmg  ─┐
              ├─> BeeDay CI ─> workflow_run (conclusion=success, head_branch=hmg) ─> BeeDay — HMG Deployment
PR hmg->main ─┘
```

`ci.yml` roda tanto em `push` (branch `hmg`) quanto em `pull_request` (branches `hmg`, `main`).
Quando uma PR de promoção `hmg→main` é aberta logo após um push em `hmg`, **ambas** as execuções
de `BeeDay CI` têm `head_branch == 'hmg'`, e `deploy-hmg.yml`'s guard original
(`workflow_run.conclusion == 'success' && workflow_run.head_branch == 'hmg'`) era satisfeito por
qualquer uma delas, independentemente do evento que a originou.

## 2. Root Cause Confirmation

`FACT` — reconfirmado, não apenas citado: os dois pares de deployments duplicados documentados na
19.1 continuam sendo evidência válida e não foram reescritos:

- `HMG Deploy #77`/`#78`, mesmo SHA `d5b9390` — `#77` disparado pela run de `push` (id
  `31378253170`), `#78` disparado pela run de `pull_request` da PR `hmg→main` (id `31378295921`).
- `HMG Deploy #57`/`#58`, mesmo SHA `9b860f7`, mesmo padrão.

Ambos permanecem preservados em
[`06-cicd-pipeline-discovery-baseline.md`](06-cicd-pipeline-discovery-baseline.md) §6, não
reescritos.

## 3. Target Architecture

```
push -> hmg (evento == 'push', head_branch == 'hmg')
        ↓
BeeDay CI
        ↓
workflow_run (conclusion=success AND event=='push' AND head_branch=='hmg')
        ↓
BeeDay — HMG Deployment  (job "Deploy HMG")
        ↓  artifact: beeday-hmg-deployment-info (SHA + resultado)
workflow_run (conclusion=success)
        ↓
BeeDay — HMG Verification  (job "Verify HMG")
        ├── Verify Readiness  (/health/ready, 6 tentativas, 5s)
        └── Run Smoke Tests   (GET /login, HTTP 200 + marcador de conteúdo)
```

A PR `hmg→main` continua rodando `BeeDay CI` (necessário — ver §6), mas essa execução **não
satisfaz mais** o guard de `deploy-hmg.yml`.

---

## 4. HMG Deployment Trigger Decision

| | Before | After | Reason |
|---|---|---|---|
| Trigger de `deploy-hmg.yml` | `workflow_run` em `BeeDay CI` (`conclusion=success && head_branch=='hmg'`) | `workflow_run` em `BeeDay CI` (`conclusion=success && head_branch=='hmg' && event=='push'`) | Fecha exatamente o mecanismo comprovado da 19.1, sem tocar `ci.yml` |

**Por que não `push: hmg` direto em `deploy-hmg.yml` (alternativa considerada e rejeitada):**
daria a `deploy-hmg.yml` seu próprio trigger de push, mas o deployment **continuaria** precisando
do artifact que só `ci.yml` produz (`beeday-publish`/`beeday-migrations`) — sem uma cadeia de
proveniência (reservada para a Sprint 19.8, "não invente Build Once/Deploy Many nesta Sprint"),
`deploy-hmg.yml` não teria como resolver de forma confiável qual execução de `ci.yml` validou
aquele push específico. A Fase 4 desta própria Sprint permite explicitamente manter a dependência
de `workflow_run` quando o deployment ainda depende de output exclusivo do CI — é exatamente o
caso aqui.

## 5. BeeDay CI Changes

`push: hmg` **NÃO foi removido** — ver justificativa completa no comentário adicionado a `ci.yml`
(reproduzido em §6). Isso é uma decisão deliberada, não um adiamento por omissão.

## 6. Por que `push: hmg` permanece em `BeeDay CI`

`FACT` + decisão explícita: `deploy-hmg.yml` continua consumindo `beeday-publish`/
`beeday-migrations`, artifacts que só `ci.yml` produz. Removê-lo exigiria uma de duas coisas fora
de escopo desta Sprint: (a) implementar resolução de proveniência independente (`push` direto em
`deploy-hmg.yml` + cadeia de PRs, como `deploy-prd.yml` já faz para `main→prd`) — isso é
"artifact provenance final", explicitamente reservado para a 19.8; ou (b) fazer `deploy-hmg.yml`
reconstruir o artifact sozinho — reintroduziria trabalho duplicado, o oposto do que a Epic quer.

## 7. workflow_run Changes

`deploy-hmg.yml`'s dependência de `workflow_run` em `BeeDay CI` foi **mantida, não eliminada** —
apenas com o guard adicional `event == 'push'`. `verify-hmg.yml` (novo) introduz uma **segunda**
dependência de `workflow_run`, desta vez em `BeeDay — HMG Deployment`.

**Achado técnico decisivo para o design de `verify-hmg.yml`:** a Sprint 19.1 já havia comprovado
(`06-...` §6.1) que, para runs disparadas por `workflow_run`, a API do GitHub reporta
`head_sha`/`head_branch` da run **triggerada** refletindo o estado da branch padrão (`main`) no
momento da criação — não o SHA real que disparou o evento. Isso significa que, se
`verify-hmg.yml` tentasse ler `github.event.workflow_run.head_sha` para saber qual SHA foi
implantado, obteria um valor **errado** (o tip de `main`, não o SHA de `hmg` implantado).

**Decisão:** `deploy-hmg.yml` agora expõe o SHA real via um artifact dedicado
(`beeday-hmg-deployment-info`, um JSON pequeno com `sha`/`workflowRun`/`result`/`environment`/
`timestampUtc`), baixado por `verify-hmg.yml` usando `run-id: ${{ github.event.workflow_run.id }}`
— o campo `id` do payload de `workflow_run` **é** confiável (já usado por `deploy-hmg.yml` desde
sempre para resolver qual execução de `ci.yml` baixar). Isso evita depender do único campo
(`head_sha`) já comprovado não-confiável nesse contexto.

## 8. Artifact Source Analysis

`CLEAR` — inalterado desde a 19.1: `deploy-hmg.yml` nunca reconstrói, sempre baixa
`beeday-publish`/`beeday-migrations` por `run-id` pinado à execução de `ci.yml` resolvida. SOURCE
SHA é identificável (`steps.resolve-sha.outputs.sha`, agora também exposto como job `output` e
gravado no artifact de deployment info).

## 9. Concurrency Analysis / Superseded Deployment Policy

`FACT` + decisão explícita — **mantido sem alteração**: `concurrency: group: beeday-homologation,
cancel-in-progress: false`. Analisado explicitamente nesta Sprint (não apenas herdado sem revisão):

| Cenário | Comportamento | Por que é seguro |
|---|---|---|
| SHA A em deploy, SHA B chega antes de A terminar | B enfileira, espera A terminar, depois deploya | Estado final correto (B, o mais novo, fica sendo o último aplicado) |
| Cancelar A no meio (`cancel-in-progress: true`, alternativa rejeitada) | Mataria o processo no meio de stop/backup/copy/migrate/start | `Deploy-BeeDay.ps1` tem lógica própria de rollback que só roda se o script **completar** seu próprio fluxo de erro — um cancelamento externo do job mata o processo abruptamente, sem chance de rollback, podendo deixar IIS parado ou o app pool inconsistente |

**Política escolhida: SERIALIZE (manter `cancel-in-progress: false`).** Custo aceito: trabalho de A
não é aproveitado se B chega logo depois (A ainda roda até o fim antes de B começar) — troca
deliberada de eficiência por segurança de não interromper uma operação de estado (stop/start
IIS + migração de banco) no meio.

## 10. Manual Retry Policy

`FACT` — `workflow_dispatch` permanece o mecanismo de retry manual em ambos os workflows
(`deploy-hmg.yml` e `verify-hmg.yml`, novo). Diferenciado estruturalmente de deployment automático
pelo `github.event_name`, não por heurística. Um retry manual do mesmo SHA é uma ação humana
deliberada — não é reclassificado como "duplicação".

---

## 11. SERV3WEB Discovery

`FACT`, **exclusivamente derivado do repositório** — nenhuma inspeção ao vivo do servidor foi
realizada ou possível nesta sessão (sem acesso direto a SERV3WEB). Ver `CLAUDE.md` §8.2 —
isto é Repository State, não Runtime State verificado.

| Item | Valor conhecido | Fonte |
|---|---|---|
| Site IIS | `BeeDay-HMG` | `deploy-hmg.yml` (`-SiteName`) |
| App Pool | `BeeDay-Web-AppPool` | `deploy-hmg.yml` (`-AppPoolName`) |
| Diretório de deploy | `C:\Apps\BeeDay.Web` | `deploy-hmg.yml` (`-DestinationPath`) |
| URL pública | `https://h-beeday.com.br` | `deploy-hmg.yml` (`-HealthCheckUrl`) |
| Endpoint de readiness | `/health/ready` (`SqlServerHealthCheck`, tag `ready`, `CanConnectAsync`) | `src/BeeDay.Web/Program.cs`, `docs/deployment/03-observability.md` §3 |
| Runner de deploy | self-hosted `[Windows, X64, hmg]`, conta `svc_beeday_runner` (baixo privilégio) | `deploy-hmg.yml`, `05-privileged-iis-control.md` |
| Controle de IIS (Stop/Start/Configure) | delegado a Scheduled Tasks SYSTEM (`\BeeDay\HMG-IisControl`), runner nunca tem permissão direta | `05-privileged-iis-control.md` §2-3 |
| Logs de aplicação | `C:\Apps\BeeDay-Data\Logs\stdout` (via `web.config`/`AspNetCoreModuleV2`) | `docs/deployment/03-observability.md` §2 |

## 12. SERV3WEB Changes Required?

**`NO SERVER CHANGE REQUIRED`.** Justificativa: toda a mudança desta Sprint acontece na camada de
orquestração do GitHub Actions (triggers, um novo workflow, um artifact) — nenhum step novo ou
alterado toca IIS, AppPool, bindings, runtime, permissões de filesystem, ou variáveis de ambiente
do servidor. `Verify Readiness`/`Run Smoke Tests` fazem apenas requisições HTTP `GET` contra
endpoints públicos já existentes e não-autenticados (`/health/ready`, `/login` — confirmado em
`docs/security/02-operational-security.md` §10 que nenhum dos endpoints de health exige
autenticação). Nenhuma mudança de servidor foi executada.

## 13. Server Changes Executed

Nenhuma. Não aplicável.

---

## 14. HMG Verification Workflow

`verify-hmg.yml` (novo arquivo, workflow `BeeDay — HMG Verification`, job `Verify HMG`).
Responsabilidade única: dado um deployment HMG bem-sucedido, provar que o estado implantado está
disponível (Readiness) e funcionalmente utilizável no mínimo necessário (Smoke). Não builda, não
testa, não reconstrói nada — só consome o resultado do deployment.

## 15. Readiness Contract

`FACT` — classificação da Fase 8: **`TRUE READINESS CHECK`**, não apenas "startup wait". `/health/ready`
executa `SqlServerHealthCheck.CanConnectAsync` — verifica conectividade real com o SQL Server, não
apenas "o processo está de pé". `Verify Readiness` (novo step em `verify-hmg.yml`) reimplementa
esse mesmo check como uma etapa explícita e nomeada, com timeout limitado por tentativa (10s),
6 tentativas, 5s de espera entre elas, e falha clara (`throw`) se nenhuma tentativa retornar HTTP
200 — sem `sleep` fixo disfarçado de verificação.

**Redundância reconhecida e aceita:** `Deploy-BeeDay.ps1` já faz esse mesmo check internamente como
parte da sua própria decisão de sucesso/rollback — por definição, se o job `deploy` terminou com
sucesso, a aplicação já estava pronta há poucos segundos. A repetição em `verify-hmg.yml` tem valor
arquitetural, não apenas redundância: torna Readiness uma responsabilidade **observável e nomeada**
no GitHub Actions (hoje invisível, presa dentro da lógica interna de um script PowerShell), e ainda
cobre a janela rara em que o app fique indisponível nos segundos entre o fim do deploy e o início
da verificação.

## 16. Smoke Contract

`FACT` — a Sprint 19.3 já havia comprovado que `BeeDay.E2E.Tests` roda **antes** do deploy, contra
um Kestrel local, não contra HMG — não são smoke de HMG (`07-validation-matrix.md` §21). `Run Smoke
Tests` (novo step) faz `GET https://h-beeday.com.br/login` e confirma HTTP 200 + a string `"BeeDay"`
no corpo da resposta — prova que a página pública realmente renderiza através de todo o pipeline
real (IIS → `AspNetCoreModuleV2` → Kestrel → Blazor Server), algo que `/health/ready` (um endpoint
JSON) não exercita. Read-only, sem criação de estado, sem dependência de credencial/conta de teste.

## 17. Verification Trigger

`workflow_run` em `BeeDay — HMG Deployment`, `types: [completed]`, guardado por
`conclusion == 'success'` (sem checar `head_branch`/`event` novamente — já filtrado uma vez por
`deploy-hmg.yml`, e esses campos teriam o mesmo problema de confiabilidade descrito em §7).
`workflow_dispatch` como caminho manual, resolvendo a última execução bem-sucedida de
`deploy-hmg.yml` via REST API (mesmo padrão já usado em `deploy-hmg.yml` para `ci.yml`).

## 18. Failure Semantics

`FACT`, confirmado por leitura direta do YAML — nenhum `continue-on-error` introduzido:

| Falha | Efeito |
|---|---|
| Deploy falha (`Deploy-BeeDay.ps1` lança exceção) | Job `deploy` falha → `BeeDay — HMG Deployment` falha → `verify-hmg.yml` nunca dispara (guard `conclusion == 'success'`) |
| Readiness falha (6 tentativas sem HTTP 200) | `throw` → job `verify` falha → `BeeDay — HMG Verification` falha |
| Smoke falha (não-200 ou conteúdo inesperado) | `throw` → job `verify` falha → `BeeDay — HMG Verification` falha |

## 19. SHA Correlation

`FACT` — `deployed-sha` (output do job `deploy`) → gravado em `beeday-hmg-deployment-info` →
baixado por `verify-hmg.yml` via `run-id` → exposto como `steps.deployed.outputs.sha` → impresso
no resumo (`$GITHUB_STEP_SUMMARY`) de ambos os workflows. Permite responder "qual SHA foi
implantado" e "qual SHA foi verificado" a partir do resumo de cada execução, sem precisar ler logs
brutos — cadeia de proveniência completa (hash de artifact, auditoria formal) permanece para a
19.8, mas a correlação básica SHA-a-SHA já existe.

## 20. Deployment / Verification Summary

Implementado via `$GITHUB_STEP_SUMMARY` (recurso nativo do GitHub Actions, sem infraestrutura
nova) em ambos os workflows — SHA, run, resultado, ambiente, timestamp. Mínimo necessário para
operação segura (Fase 14/15), não a observabilidade completa da 19.9.

---

## 21. Duplicate Deployment Analysis

| | Estado |
|---|---|
| **Before** | Push em `hmg` + PR `hmg→main` do mesmo commit → 2 execuções de `BeeDay CI` com `head_branch=='hmg'` → 2 deployments reais (comprovado, `#77`/`#78`, `#57`/`#58`) |
| **After (estrutural)** | Apenas a execução de `BeeDay CI` cujo evento seja `push` satisfaz o guard de `deploy-hmg.yml` — a execução da PR `hmg→main` (evento `pull_request`) não satisfaz mais, mesmo com `head_branch=='hmg'` | `STRUCTURALLY ELIMINATED` |
| **After (remoto)** | `NOT YET VALIDATED REMOTELY` — nenhum push/PR com esta mudança foi executado no GitHub Actions ainda |

Não declaro a duplicação eliminada empiricamente até observar uma execução real.

## 22. Temporary Architecture Remaining

| Responsabilidade emprestada | Razão | Dono futuro | Sprint de remoção |
|---|---|---|---|
| `push: hmg` em `BeeDay CI` (produz artifact para `deploy-hmg.yml`) | Sem proveniência independente ainda | Cadeia de proveniência completa (Build Once/Deploy Many final) | **19.8 — RESOLVIDO, ver nota abaixo** |
| `pull_request: main` em `BeeDay CI` (satisfaz required check de `main`) | Sem `BeeDay — Release Quality Gate` ainda | Release Quality Gate | 19.7 |

**Atualização da Sprint 19.8:** a alternativa considerada e rejeitada em §4 acima ("Por que não
`push: hmg` direto em `deploy-hmg.yml`") foi implementada nesta Sprint, agora que a cadeia de
proveniência que faltava existe. `deploy-hmg.yml` dispara em `push: hmg` diretamente e resolve o
artefato já validado pela PR via API de Pull Requests do GitHub — `push: hmg` foi removido de
`ci.yml`. A decisão original registrada em §4/§6 permanece válida como registro histórico do que
era verdade na 19.6 (não reescrita); ver
[`12-artifact-provenance.md`](12-artifact-provenance.md) para a implementação completa.

## 23. Resolução da dívida da Sprint 19.4

A 19.4 registrou (`08-fast-pr-validation-decision.md` §6): *"push: hmg em ci.yml — Future owner:
BeeDay — HMG Deployment (redesenhado) ou mecanismo de proveniência dedicado — Sprint de remoção:
19.6."*

**Status: `PARTIALLY RESOLVED`, não `RESOLVED`** — reportado com honestidade, não simplificado para
fechar um checklist. O **dano concreto** (deployment duplicado) está estruturalmente eliminado
nesta Sprint. O **acoplamento em si** (`ci.yml` ainda precisa rodar em `push` porque é quem produz
o artifact) permanece, agora por uma razão **documentada e deliberada** (§6), não por dívida
esquecida — sua remoção completa depende da proveniência final da Sprint 19.8, conforme já havia
sido antecipado desde a 19.1. `08-fast-pr-validation-decision.md` foi atualizado (ver §26) para
refletir esse status preciso, substituindo o campo de "Sprint de remoção: 19.6" por uma referência
a esta análise.

---

## 24. BeeDay CI Remaining Responsibilities

`pull_request → hmg` (validação de PR real) e `pull_request → main` (gate temporário de promoção,
até a 19.7) e `push → hmg` (produção do artifact para deployment, até a 19.8). **Não renomeado**
para `BeeDay — Pull Request Validation` nesta Sprint — continua materialmente impreciso enquanto
essas duas responsabilidades emprestadas persistirem. Decisão final do rename cabe à 19.7/19.8,
quando ambas puderem ter sido resolvidas.

## 25. 19.7 Deferrals

`BeeDay — Release Quality Gate`, `has-pending-model-changes`, redesenho final de `hmg→main`, e a
decisão de rename de `BeeDay CI`.

## 26. 19.8 Deferrals

Proveniência final (SOURCE SHA → BUILD RUN → ARTIFACT HASH → HMG → MAIN → PRD), remoção completa da
dependência de `push: hmg` em `BeeDay CI`, e Build Once/Deploy Many final.

---

## 27. Fontes consultadas

- `.github/workflows/ci.yml`, `deploy-hmg.yml`, `verify-hmg.yml` (lidos/escritos integralmente).
- `scripts/Deploy-BeeDay.ps1`, `scripts/iis-control/Request-BeeDayIisControlPromotion.ps1`.
- `src/BeeDay.Web/Program.cs` (`MapHealthChecks`), `docs/deployment/03-observability.md` §3.
- `docs/deployment/05-privileged-iis-control.md`.
- `docs/deployment/06-cicd-pipeline-discovery-baseline.md` §6, §6.1, §12 (evidência histórica,
  não reescrita).
- `docs/deployment/07-validation-matrix.md` §21 (E2E não é smoke de HMG).
- `docs/deployment/08-fast-pr-validation-decision.md` (dívida da 19.4, atualizada).
- `docs/security/02-operational-security.md` §10 (endpoints de health sem autenticação).
- `CLAUDE.md` §8.2 (Repository State vs. Runtime State).
