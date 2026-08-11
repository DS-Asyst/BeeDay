# Fast Pull Request Validation — Decision Record (EPIC 19, Sprint 19.4)

**Fonte da verdade:** verificado diretamente em `.github/workflows/ci.yml`, `deploy-hmg.yml`,
`deploy-prd.yml`, `validate-promotion.yml`, Rulesets via `gh api repos/.../rules/branches/*`
(reconsultados em 2026-08-11), e `docs/deployment/07-validation-matrix.md` (Sprint 19.3).

**Última verificação:** 2026-08-11.

**Escopo:** decisão + 1 alteração estrutural mínima e segura. Este documento registra por que o
rename canônico `BeeDay CI → BeeDay — Pull Request Validation` **não foi aplicado** nesta Sprint,
o que **foi** implementado, e as condições exatas sob as quais o rename se torna seguro (para
quando as Sprints 19.6/19.7 existirem).

**Classificação de evidência:** `FACT`, `MEASUREMENT`, `INFERENCE`, `RECOMMENDATION`, `UNKNOWN`.

---

## 1. Contrato de entrada (Sprint 19.3)

`FACT` — `docs/deployment/07-validation-matrix.md` §18 recomendou como candidatos fortes a
`FAST/EVERY PR`: Format, Build, Domain Tests, Application Tests, Boundary/Architecture Tests
(embutidos), Infrastructure Tests, Web Tests, bundle EF + validação de bundle. §19 excluiu
`has-pending-model-changes` do Fast PR (GAP, destino 19.7) e marcou `E2E.Tests` como candidata a
`SELECTIVE`, mas **condicionada** a uma regra de seleção segura e determinística existir.

## 2. Fast PR Contract — já implementado, nenhuma mudança necessária

`FACT` — `ci.yml` já executa, para todo evento (`push` ou `pull_request`, sem distinção), exatamente
as 8 validações recomendadas pela matriz como `FAST/EVERY PR`: Format → Build (warnaserror) →
Domain/Application/Infrastructure/Web/E2E Tests (com os 2 boundary tests embutidos em
Domain.Tests/Application.Tests) → bundle EF + validação. Nenhuma validação recomendada estava
ausente; nenhuma validação não-recomendada (`has-pending-model-changes`) estava presente. **Nenhuma
alteração de conteúdo de validação foi necessária.**

## 3. Decisão sobre E2E — EVERY PR (não SELECTIVE)

`RECOMMENDATION` fundamentada:

Perguntas exigidas pela Sprint, respondidas com a evidência disponível:

- **O que muda exige E2E?** Qualquer mudança em `src/BeeDay.Web/**` (páginas/fluxos) ou nas próprias
  camadas inferiores que os 7 fluxos E2E exercitam (Domain/Application/Infrastructure, já que os
  fluxos passam por toda a pilha). Na prática, isso cobre a maioria das mudanças de produto.
- **Quais paths implicam risco de E2E?** Não existe hoje um mapeamento confiável e testado de
  "path → risco de E2E" no repositório. Construir esse mapeamento exigiria uma auditoria adicional
  fora do escopo desta Sprint (não é um "Discovery" já feito na 19.3, que classificou por *tipo* de
  validação, não por *path de arquivo*).
- **A regra pode produzir falso negativo?** Sim — qualquer regra baseada em path corre o risco real
  de classificar uma mudança como "segura para pular E2E" quando na verdade não é (ex.: mudança em
  `BeeDay.Infrastructure` que quebra um fluxo E2E sem tocar `BeeDay.Web`).
- **O que acontece em caso de incerteza?** Regra explícita da Sprint: *"quando houver dúvida,
  prefira executar o E2E."*

**Decisão:** manter `E2E.Tests` em `EVERY PR`, sem lógica de seleção por path. Duração medida
(64.3s) não é proibitiva o suficiente para justificar o risco de uma regra ainda não comprovada
como segura. **Nenhuma mudança de trigger/condição foi implementada para E2E.**
`RECOMMENDATION` registrada para Sprint futura (candidata a 19.5, não 19.4): investigar um
mapeamento path→risco comprovadamente seguro, se o custo do Fast PR crescer o suficiente para
justificar o esforço.

## 4. Mudança implementada — remoção de `prd` do trigger `pull_request` de `ci.yml`

`FACT` + `RECOMMENDATION` executada (única alteração estrutural desta Sprint):

**Antes:**
```yaml
pull_request:
  branches:
    - hmg
    - main
    - prd
```

**Depois:**
```yaml
pull_request:
  branches:
    - hmg
    - main
```

**Evidência de segurança (verificada antes da mudança, não depois):**
- `gh api repos/tiagoarrigoni/BeeDay/rules/branches/prd` retorna `[]` — nenhum Ruleset, nenhum
  required check em `prd`, hoje ou historicamente nesta EPIC (confirmado em 19.1, 19.2, 19.3, e
  reconfirmado agora).
- Nenhum outro workflow consome uma execução de `ci.yml` disparada por uma PR `main→prd` —
  `deploy-prd.yml` resolve proveniência via a cadeia `prd←main←hmg` e busca a execução de `ci.yml`
  **em `hmg`**, nunca a execução (se existisse) da própria PR `main→prd` (ver `01-deployment.md`
  §4.2). `deploy-hmg.yml` só escuta `workflow_run` com `head_branch == 'hmg'`, irrelevante aqui.
- `validate-promotion.yml` tem seu próprio `on: pull_request: branches: [main, prd]`, inteiramente
  independente de `ci.yml` — continua rodando normalmente em PRs `main→prd`, sem qualquer alteração.

**Efeito:** uma PR `main→prd` deixa de disparar uma execução completa (~7 min) de `ci.yml` que
**nenhum mecanismo do repositório lia ou exigia**. Isso é trabalho eliminado com **risco zero de
proteção reduzida** — não existia proteção alguma dependendo dele.

**Isto NÃO é a mesma duplicação da Sprint 19.1** (aquela é `push:hmg` + PR `hmg→main` disparando
`ci.yml` duas vezes para o **mesmo commit**, e permanece para a Sprint 19.6). É uma segunda fonte de
trabalho descartável, encontrada nesta Sprint, distinta e agora eliminada.

---

## 5. Por que o rename `BeeDay CI → BeeDay — Pull Request Validation` NÃO foi aplicado

`FACT` — investigação completa de ambas as dependências restantes de `ci.yml`, feita antes de
qualquer decisão:

### 5.1 Dependência 1 — `push: branches: [hmg]` alimenta `deploy-hmg.yml`

Merges de PR para `hmg` (squash, merge commit ou rebase) resultam em um `push` real no servidor
Git — é esse `push` que hoje dispara `ci.yml`, cuja conclusão bem-sucedida (`workflow_run`,
`head_branch == 'hmg'`) é o único gatilho confiável e determinístico que mantém `deploy-hmg.yml`
implantando continuamente em SERV3WEB a cada merge.

**O que aconteceria se `push: hmg` fosse removido de `ci.yml`:** nenhuma execução de `ci.yml`
teria `head_branch == 'hmg'` proveniente de um merge normal. A única execução que ainda teria
`head_branch == 'hmg'` seria a PR de promoção `hmg→main` (porque `hmg` é literalmente a branch de
origem dessa PR) — ou seja, `deploy-hmg.yml` passaria a implantar **apenas quando alguém abrisse
uma PR `hmg→main`**, não a cada merge em `hmg`. Isso é uma mudança real e negativa de comportamento
de deployment, não uma simplificação segura — violaria diretamente a regra de preservar o
deployment atual.

**Alternativas consideradas e rejeitadas nesta Sprint:**

| Alternativa | Por que rejeitada |
|---|---|
| Dar a `deploy-hmg.yml` seu próprio trigger `push: branches: [hmg]`, resolvendo a proveniência do artefato via cadeia de PRs (como `deploy-prd.yml` já faz para `main→prd`) | Constitui "artifact provenance implementation", explicitamente reservado para a Sprint 19.8. Não implementado. |
| Criar um novo workflow separado só para PR validation, mantendo `ci.yml` como está | Duplicaria a suíte inteira de testes para o mesmo evento de PR — mais trabalho redundante, não menos. Contradiz o princípio central da Epic. Não implementado. |
| Restringir o guard de `deploy-hmg.yml` para aceitar só `workflow_run` de eventos `push` (não `pull_request`) | Corrigiria a duplicação da 19.1 diretamente, mas essa correção já está formalmente atribuída à Sprint 19.6 desde a própria Sprint 19.1 (e reafirmada na 19.3). Implementá-la aqui seria antecipar escopo de outra Sprint. Não implementado. |

### 5.2 Dependência 2 — `pull_request: branches: [main]` alimenta o required check `BeeDay CI` do Ruleset de `main`

O Ruleset de `main` (id 20608232) exige literalmente o check `"BeeDay CI"` (nome do **job**, não do
workflow) para qualquer PR `hmg→main`. Esse check só é produzido hoje pela execução de `ci.yml` na
própria PR de promoção. Não existe, nesta Sprint, nenhum workflow `BeeDay — Release Quality Gate`
(reservado para a Sprint 19.7) para assumir essa responsabilidade.

**O que aconteceria se `main` fosse removido do trigger `pull_request` de `ci.yml`:** toda PR
`hmg→main` ficaria permanentemente bloqueada, esperando um check `"BeeDay CI"` que nunca mais
seria produzido — exatamente o cenário de falha que a Sprint descreve explicitamente
("Ruleset exige check antigo + workflow para de produzi-lo = PR permanentemente bloqueada").

**Conclusão:** `ci.yml` continua, por necessidade comprovada (não por inércia), sendo responsável
por **validação de push pós-merge** (alimenta HMG) **e** por **um gate de promoção temporário**
(satisfaz o Ruleset de `main` até a 19.7 existir), além de validação de PR propriamente dita. O
nome `BeeDay — Pull Request Validation` continuaria **materialmente falso** se aplicado agora —
mesma conclusão e mesmo princípio já usados na Sprint 19.2.1 para a mesma decisão, agora com a
cadeia de causalidade completamente rastreada e comprovada (não apenas inferida).

---

## 6. Temporary Architecture (rastreabilidade obrigatória)

`FACT`

| Responsabilidade emprestada | Razão | Dono atual | Dono futuro | Sprint de remoção |
|---|---|---|---|---|
| `push: hmg` em `ci.yml` | Único gatilho confiável de `deploy-hmg.yml` até a 19.8 | ~~`ci.yml`~~ **REMOVIDO na Sprint 19.8** | Proveniência final (Build Once/Deploy Many) | **RESOLVIDO na 19.8** (ver nota abaixo) |
| `pull_request: main` em `ci.yml` | Satisfaz o required check `"BeeDay CI"` do Ruleset de `main` | ~~`ci.yml`~~ **REMOVIDO na Sprint 19.8.4** | `BeeDay — Release Quality Gate` | **RESOLVIDO na 19.8.4** (ver nota abaixo) |

**Atualização da Sprint 19.7:** `BeeDay — Release Quality Gate` (`release-quality-gate.yml`) foi
criado e validado localmente, mas **não foi ativado** — o Ruleset de `main` ainda exige
`"BeeDay CI"`, e `pull_request: main` ainda não foi removido de `ci.yml`. Trocar isso agora, antes
de o novo check ter sido reportado ao GitHub pelo menos uma vez, criaria exatamente o risco que a
própria Sprint 19.7 mandou evitar: `main` permanentemente bloqueada esperando um check que nunca
apareceu, ou um Ruleset exigindo um check que `ci.yml` parou de produzir antes do substituto existir
de fato. Sequência de ativação completa (merge → 1 execução real bem-sucedida → mutação de
Ruleset autorizada → remoção de `pull_request: main`) documentada em
[`11-release-quality-gate.md`](11-release-quality-gate.md) §13-14. Status: `NOT RESOLVED` (ver
§19 daquele documento) — implementação pronta, ativação pendente de validação remota.

**Atualização da Sprint 19.6:** a Sprint 19.6 investigou remover `push: hmg` de `ci.yml`
completamente e concluiu que isso exigiria implementar resolução de proveniência independente para
`deploy-hmg.yml` (equivalente ao que `deploy-prd.yml` já faz para `main→prd`) — explicitamente fora
do escopo da 19.6 ("não invente Build Once/Deploy Many nesta Sprint"). Em vez disso, a 19.6
**eliminou o dano concreto** (deployment duplicado) restringindo o guard de `deploy-hmg.yml` a
`workflow_run.event == 'push'`, mantendo a dependência de `push: hmg` como uma responsabilidade
deliberada e documentada, não mais uma dívida esquecida. Status: `PARTIALLY RESOLVED` — ver
[`10-hmg-deployment-verification.md`](10-hmg-deployment-verification.md) §23 para a análise
completa. O "Sprint de remoção" desta linha foi corrigido de 19.6 para **19.8**, que é quando a
proveniência final poderá de fato substituir essa dependência.

**Atualização da Sprint 19.8:** `push: hmg` foi **removido** de `ci.yml`. `deploy-hmg.yml` passou a
disparar diretamente em `push: branches: [hmg]`, resolvendo o artefato já validado pela PR via uma
cadeia de proveniência baseada na API de Pull Requests do GitHub (o mesmo padrão que
`deploy-prd.yml` já usava para `main→prd`, aplicado um hop antes) — não uma reconstrução, não um
novo `workflow_run`. Esta linha da tabela está **RESOLVIDA**. Ver
[`12-artifact-provenance.md`](12-artifact-provenance.md) para a investigação completa, incluindo
por que a resolução não pode se basear em topologia de commits Git (o Ruleset de `hmg` permite
merge commit, squash, **e** rebase — apenas merge commit preserva uma relação de ancestralidade
Git verificável com o PR HEAD SHA).

**Atualização da Sprint 19.8.4:** com `BeeDay — Release Quality Gate` já possuindo duas execuções
reais bem-sucedidas em `hmg→main` (PRs #64 e #66, Sprint 19.8.3), o Ruleset de `main` foi mutado
(`"BeeDay CI"` → `"Release Quality Gate"` como required check, `"Validate Promotion"` preservado —
read-back confirmado, ver [`11-release-quality-gate.md`](11-release-quality-gate.md) §25) e
**somente depois** `pull_request: main` foi removido de `ci.yml`. Ambas as linhas da tabela acima
estão agora **RESOLVIDAS**.

**Condição de desbloqueio do rename:** com ambas as linhas resolvidas, o rename `BeeDay CI →
BeeDay — Pull Request Validation` está tecnicamente desbloqueado quanto a essas duas dependências
— mas o Ruleset de `hmg` ainda exige literalmente o contexto `"BeeDay CI"`, então o rename em si
exigiria uma **segunda e separada** mutação de Ruleset (desta vez em `hmg`), deliberadamente não
executada na Sprint 19.8.4 para não misturar duas mudanças remotas de governança na mesma rodada —
ver [`11-release-quality-gate.md`](11-release-quality-gate.md) §25.4 para a análise completa
(`REQUIRES HMG RULESET TRANSITION`, `DEFERRED`).

---

## 7. Ruleset Analysis (reconfirmado, nenhuma mutação necessária)

`FACT`

| Branch | Ruleset | Required Check Atual | Required Check Alvo | Migração necessária agora? |
|---|---|---|---|---|
| `hmg` | 20580759 | `BeeDay CI` | inalterado | Não |
| `main` | 20608232 | ~~`BeeDay CI`~~ **`Release Quality Gate`**, `Validate Promotion` | — | Mutado na Sprint 19.8.4 (ver `11-release-quality-gate.md` §25) |
| `prd` | nenhum | — | — | Não |

## 8. Remote Mutations

| Requested | Authorized | Executed | Result |
|---|---|---|---|
| Nenhuma | — | **Não** | A remoção de `prd` do trigger de `ci.yml` não afeta nenhum Ruleset (confirmado antes da mudança, §4) |

---

## 9. Duplicate Execution Impact

`FACT` — avaliação explícita, sem afirmação sem evidência:

- **Duplicação `push:hmg` + PR `hmg→main` disparando `ci.yml` duas vezes para o mesmo commit
  (Sprint 19.1 §12):** **PRESERVED** (não tocada) — depende da correção reservada para a Sprint
  19.6, cujas alternativas de correção antecipada foram avaliadas e rejeitadas nesta Sprint (§5.1).
- **Execução vestigial de `ci.yml` em PRs `main→prd` sem nenhum consumidor:** **ELIMINATED** — via
  a remoção de `prd` do trigger `pull_request` (§4). Esta não é a mesma duplicação da 19.1, mas é
  trabalho redundante real que existia e agora não existe mais.

---

## 10. Performance / HMG / Release Gate — explicitamente adiados

`RECOMMENDATION`, sem implementação:

- **19.5 (Performance):** oportunidade de mapear path→risco de E2E para uma futura regra
  `SELECTIVE` segura (§3); nenhuma outra oportunidade de performance foi avaliada nesta Sprint
  (fora de escopo).
- **19.6 (HMG Deployment & Verification):** corrigir a duplicação `push`+`PR hmg→main` (§5.1, §9);
  redesenhar como `deploy-hmg.yml` resolve proveniência sem depender de `ci.yml` rodar em `push`.
- **19.7 (Release Quality Gate):** criar `BeeDay — Release Quality Gate` para assumir a
  responsabilidade de required check em `main`, liberando `ci.yml` para finalmente se tornar
  `BeeDay — Pull Request Validation` de fato; incluir `has-pending-model-changes` (GAP da 19.3).

---

## 11. Repository-wide Reference Audit

`FACT` — como nenhum rename de workflow/job foi aplicado, não há novas referências a auditar além
da mudança de trigger. Busca por `prd` em referências a `ci.yml`/`BeeDay CI` no repositório:

| Referência | Classificação | Ação |
|---|---|---|
| `docs/deployment/01-deployment.md` §2, linha "PR para hmg/main/prd" | `STALE — UPDATE` | Corrigida nesta Sprint (§ acima) |
| `docs/deployment/06-cicd-pipeline-discovery-baseline.md` (menciona `pull_request` `hmg/main/prd` como observado na 19.1) | `HISTORICAL — KEEP` | Não alterada — era verdade no momento da observação |
| `docs/deployment/07-validation-matrix.md` | `HISTORICAL — KEEP` (classificação de validação não muda) | Não alterada — a classificação por tipo de validação continua válida independente do trigger de `prd` |

---

## 12. Sprint 19.8.5 — Fast HMG Developer Feedback (revisão desta decisão)

**Fonte da verdade:** `gh run view --json jobs`/`--log` de 5 execuções reais recentes de `BeeDay
CI` (timing por step e, extraído do log bruto, por projeto de teste); leitura integral de
`ci.yml`/`release-quality-gate.yml`; `docs/deployment/07-validation-matrix.md` §14/§18/§19
(candidatos já identificados na Sprint 19.3, nunca implementados por falta, na época, de uma
fronteira de release independente).

### 12.1 Por que esta Sprint pode revisar a decisão da 19.4

A decisão original (§5 acima) manteve toda a suíte em `ci.yml` para PRs `sprint/*→hmg` porque, na
Sprint 19.4, `ci.yml` era a **única** validação real que existia — não havia `BeeDay — Release
Quality Gate` (criado só na 19.7) nem a fronteira `hmg→main` protegida (ativada só na 19.8.4).
Manter tudo era a escolha correta *para aquele estado*. Hoje, com `Release Quality Gate` validado
remotamente duas vezes (PRs #64, #66) e sendo o required check real de `main`, a mesma suíte
completa roda de qualquer forma antes de qualquer promoção — o que muda é **quando** cada
validação é mais valiosa, exatamente a pergunta que a Sprint 19.8.5 formaliza: `Sprint→HMG`
responde "qualidade mínima para integrar e testar em homologação?"; `HMG→MAIN` responde "qualidade
suficiente para a linha de release?".

### 12.2 Baseline remoto real (5 execuções, `pull_request`→`hmg`, `windows-latest`)

| Run | Branch origem | Duração total |
|---|---|---|
| `31487328668` | `sprint/19.8.4-main-ruleset-transition` | 6m11s |
| `31485737898` | `hmg` (promoção `hmg→main` pré-19.8.4) | 6m22s |
| `31484479264` | `sprint/19.8.3-hmg-verification-provenance-hardening` | 5m00s |
| `31482098907` | `hmg` (promoção `hmg→main` pré-19.8.4) | 6m43s |
| `31481327596` | `sprint/19.8.2-release-gate-powershell-compatibility` | 7m39s |

**Before: média 6m23s, mediana 6m22s, min 5m00s, max 7m39s.**

Nota de honestidade metodológica: 2 das 5 amostras (`898`, `907`) são execuções de `ci.yml`
disparadas pela PR de promoção `hmg→main` (antes da Sprint 19.8.4 remover esse trigger), não por
uma PR `sprint/*→hmg`. O job em si não distingue a origem — mesmo comando, mesmo custo — então a
medição permanece representativa do "critical path real de `BeeDay CI`", não distorcida pela
origem da PR.

### 12.3 Critical path real (por projeto de teste, extraído do log bruto de 3 runs)

| Projeto | Amostra 1 | Amostra 2 | Amostra 3 | Média |
|---|---|---|---|---|
| Application.Tests | 3.8s | 2.9s | 3.2s | **3.3s** |
| Domain.Tests | 2.8s | 2.1s | 2.0s | **2.3s** |
| E2E.Tests | 54.3s | 46.3s | 61.1s | **53.9s** |
| Infrastructure.Tests | 67.6s | 51.4s | 124.8s | **81.3s** |
| Web.Tests | 39.0s | 30.4s | 65.4s | **44.9s** |

Infrastructure.Tests confirma empiricamente, com dado remoto real (não só a variância local já
registrada em `07-validation-matrix.md` §5/§11), a maior variância absoluta (51.4s–124.8s,
+143%) — consistente com a contenção de `CREATE`/`DROP DATABASE` já documentada, agora também
observável em produção do pipeline, não só em amostra local.

### 12.4 Decisão: Fast HMG Gate (novo `ci.yml`)

| Validação | Decisão | Justificativa |
|---|---|---|
| Restore, Build (`--warnaserror`) | `KEEP` | `CRITICAL` — sem build, nada mais é válido; necessário de qualquer forma para produzir `beeday-publish`/`beeday-migrations` |
| Domain.Tests + `DomainAssemblyBoundaryTests` | `KEEP` | ~2.3s, `SELF-CONTAINED`, `CRITICAL` (único guard de arquitetura do Domain) |
| Application.Tests + `PersistenceContractBoundaryTests` | `KEEP` | ~3.3s, `SELF-CONTAINED`, `CRITICAL` (único guard de arquitetura da Application) |
| Publish, validação do publish | `KEEP` | Produz `beeday-publish` — contrato de artifact, não pode sair (ver §12.6) |
| EF tool restore, EF bundle, validação do bundle | `KEEP` | Produz `beeday-migrations` — contrato de artifact, não pode sair |
| Format (`dotnet format --verify-no-changes`) | `MOVE TO RELEASE GATE ONLY` | ~42s; zero risco funcional (não afeta comportamento em runtime); já roda em `release-quality-gate.yml`; risco de bloquear a promoção de `main` para *todos* que compartilham o estado de `hmg` até ser corrigido é aceito — ver §12.7 |
| Infrastructure.Tests | `MOVE TO RELEASE GATE ONLY` | ~81.3s média, alta variância confirmada; `HIGH` risco, mas coberto integralmente por `Release Quality Gate` antes de `main`; `hmg` é o próprio ambiente de homologação onde isso seria exercitado manualmente |
| Web.Tests | `MOVE TO RELEASE GATE ONLY` | ~44.9s; mesma justificativa — `HIGH` risco coberto pela fronteira de release, não pela fronteira de integração |
| E2E.Tests + cache/install Playwright | `MOVE TO RELEASE GATE ONLY` | ~54s (teste) + ~15s (Playwright) ≈ 69s; `CI DEPENDENCY` pesada (browser+TCP); já `REQUIRED` em `Release Quality Gate`; cobertura na fronteira `hmg→main` **não reduzida** |

**Nenhuma seleção por path implementada** — decisão binária e determinística (Domain+Application
sempre; Infrastructure+Web+E2E+Format nunca, neste gate), evitando a complexidade que a própria
Sprint 19.4 já havia rejeitado para E2E por falta de uma regra comprovadamente segura.

### 12.5 Before × Expected After

| | Before (medido remotamente) | After (simulação local) |
|---|---|---|
| Total | 6m23s (média) | ver `09-pipeline-performance.md` §Sprint 19.8.5 |

Classificação de evidência obrigatória por tipo de medição no relatório final desta Sprint —
`MEASURED REMOTELY` (before) vs `MEASURED LOCALLY` (simulação do novo gate) vs `ESTIMATED`
(projeção remota do novo gate, ainda não observada).

### 12.6 Artifact Contract (reconfirmado, não alterado)

`deploy-hmg.yml` consome exatamente `beeday-publish` e `beeday-migrations` (confirmado via
`grep -n "name: beeday" deploy-hmg.yml`). Nenhuma das duas deixou de ser produzida — os steps
`Publish BeeDay`, `Generate EF Core migration bundle`, e os dois uploads correspondentes não foram
tocados. `beeday-e2e-artifacts` (sem consumidor downstream confirmado) foi removida junto com a
remoção de E2E deste workflow — `beeday-test-results` continua sendo produzida, agora só com os
resultados de Domain+Application.

### 12.7 Format — decisão explícita (não automática)

Avaliado individualmente conforme exigido: Format não protege nenhum defeito funcional — código
mal formatado compila e roda de forma idêntica. O risco de removê-lo do Fast Gate é puramente
operacional (uma PR com formatação incorreta some despercebida até a promoção `hmg→main`, onde
`Release Quality Gate` bloqueia — potencialmente represando também outras mudanças já integradas
no mesmo estado de `hmg`). Decisão: `MOVE`, aceitando esse risco operacional porque (a) é
inteiramente evitável (`dotnet format` local antes do push), (b) não ameaça a testabilidade
funcional em HMG — que é exatamente a pergunta que este gate precisa responder — e (c) o custo
(~42s, o segundo maior item individual depois da suíte de testes) é desproporcional a um problema
sem nenhum impacto em runtime.

### 12.8 Release Quality Gate — preservação confirmada

`release-quality-gate.yml` **não foi tocado nesta Sprint** (`git diff --stat` confirma). Continua
executando: Format, Build (`--warnaserror`), os 5 projetos de teste completos (incluindo os 2
boundary tests embutidos), Publish + validação, `has-pending-model-changes`, EF bundle +
validação. Nenhuma cobertura desaparece de nenhuma das duas fronteiras — apenas muda de qual
fronteira a exige.

---

## 13. Fontes consultadas

- `.github/workflows/ci.yml`, `deploy-hmg.yml`, `deploy-prd.yml`, `validate-promotion.yml`.
- `gh api repos/tiagoarrigoni/BeeDay/rules/branches/{hmg,main,prd}` (reconsultado nesta Sprint).
- `docs/deployment/06-cicd-pipeline-discovery-baseline.md`, `07-validation-matrix.md`.
- `CLAUDE.md` (governança de mutação remota, seção 5.11).
