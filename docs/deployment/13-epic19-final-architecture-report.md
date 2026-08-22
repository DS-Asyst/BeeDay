# EPIC 19 — Final CI/CD Architecture & Observability Report

**Fonte da verdade:** leitura integral de todos os 6 workflows em `.github/workflows/` existentes à
época; `gh api` em Rulesets reais (`hmg`, `main`, `prd`); `gh run list`/`gh run view` em amostras
reais recentes de todas as categorias de workflow; `docs/deployment/06-cicd-pipeline-discovery-baseline.md`
(baseline histórico da Sprint 19.1, não reescrito); todos os documentos `docs/deployment/07-12`.

**Última verificação:** 2026-08-11 (Sprint 19.9, encerramento da EPIC 19).

**Natureza deste documento (nota da Sprint 31.11, EPIC 31):** este é o relatório de fechamento da
EPIC 19 — um retrato histórico do estado da arquitetura de CI/CD em 2026-08-11, não uma referência
viva. As tabelas de inventário abaixo (§5-11) já não cobrem `.github/workflows/codeql.yml`,
adicionado pela EPIC 30 (Sprint 30.25) — a EPIC 19 encerrou antes desse workflow existir. Para o
estado atual de qualquer workflow, consulte diretamente `.github/workflows/`,
[`06`](06-cicd-pipeline-discovery-baseline.md)–[`12`](12-artifact-provenance.md) (mantidos como
histórico com notas de atualização pontuais) ou [`docs/testing/01-testing-strategy.md`](../testing/01-testing-strategy.md)
§7. Este documento não é reescrito para acompanhar mudanças posteriores ao fechamento da EPIC 19,
exceto correções pontuais de erros factuais internos (ver histórico de commits).

**Escopo:** consolidação final — não redesenho. Nenhuma alteração de arquitetura, artifact
provenance, Ruleset, ou Release Quality Gate foi feita nesta Sprint.

**Classificação de evidência:** `FACT`, `MEASUREMENT`, `INFERENCE`, `RECOMMENDATION`, `UNKNOWN`.

---

## 1. Executive Summary

A EPIC 19 transformou um pipeline monolítico, com validação duplicada, deployment duplicado,
naming inconsistente e zero artifact provenance explícita, em uma arquitetura de 4 fronteiras
distintas — `Sprint→HMG`, `HMG Deployment`, `HMG Verification`, `HMG→MAIN` — cada uma com
responsabilidade única, nome coerente, Ruleset alinhado, e proveniência determinística e
rastreável ponta a ponta. `Pull Request Validation` (antes `BeeDay CI`) caiu de uma média medida
de 6m23s para ~2m07s-2m42s (2 amostras reais pós-redesenho, redução real de 61.8%-66.8% conforme
metodologia de medição). O CI duplicado pós-merge em `hmg` foi estruturalmente eliminado e
confirmado remotamente (zero ocorrências desde a Sprint 19.8). O deployment duplicado em HMG (causa
raiz da Sprint 19.1) foi eliminado e confirmado por evidência real de execução (`skipped` correto
em 2 casos legados). Nenhuma cobertura de validação foi removida — Format/Infrastructure/Web/E2E
foram realocados para `Release Quality Gate`, que já tem 5 execuções reais bem-sucedidas na
fronteira `hmg→main`. **EPIC 19 — COMPLETE.**

## 2. Original Problems

| Problema original (Sprint 19.1) | Status final |
|---|---|
| CI monolítico | Resolvido — `Pull Request Validation` (Fast Gate) + `Release Quality Gate` (suíte completa), fronteiras distintas |
| Validações repetidas | Resolvido — suíte completa roda 1x por PR (não mais duplicada em `push`+`pull_request` do mesmo commit) |
| Deployment HMG duplicado | Resolvido — confirmado estruturalmente (Sprint 19.6) e remotamente (nenhuma duplicação real desde então) |
| `BeeDay CI` executando novamente após merge | Resolvido (Sprint 19.8) — artifact provenance elimina a segunda execução |
| Responsibilities misturadas | Resolvido — 6 workflows, cada um com 1 responsabilidade |
| Workflow names inconsistentes | Resolvido — taxonomia final confirmada nesta Sprint (§5) |
| Ausência de Release Quality Gate | Resolvido (Sprint 19.7) — ativo e required em `main` desde a 19.8.4 |
| Ausência de HMG Verification | Resolvido (Sprint 19.6) |
| Ausência de artifact provenance explícita | Resolvido (Sprint 19.8) |
| Rulesets incoerentes | Resolvido — `hmg`/`main` alinhados às fronteiras reais |
| Documentação desatualizada | Resolvido nesta Sprint — `docs/architecture/08-deployment-architecture.md` reconciliado |
| Pouca observabilidade operacional | Parcialmente fechado nesta Sprint — gap de summary em `Pull Request Validation` corrigido |
| Fast PR feedback ~6+ min | Resolvido — ~2m07s-2m42s medido remotamente |
| Múltiplas execuções redundantes | Resolvido — auditado com evidência real nesta Sprint (§12) |

## 3. AS-IS Baseline

`FACT`, reaproveitado de `06-cicd-pipeline-discovery-baseline.md` (Sprint 19.1, não reescrito):

- `ci.yml` (então `BeeDay CI`) disparava em `push:hmg` **e** `pull_request:[hmg,main,prd]` — o
  mesmo commit produzia 2 execuções completas da suíte quando uma PR `hmg→main` seguia um push
  recente em `hmg` (runs #164/#165 documentados, ~6-8min cada).
- `deploy-hmg.yml` disparava via `workflow_run` em qualquer conclusão de `BeeDay CI` com
  `head_branch=='hmg'` — produzindo 2 deployments reais para o mesmo estado (HMG Deploy #57/#58,
  #77/#78, evidência de log direta).
- Nenhum artifact provenance explícito — deployment confiava implicitamente no `run-id` mais
  recente, sem checagem de mesmo-repositório.
- Nenhum Release Quality Gate — `hmg→main` era protegida apenas por `BeeDay CI` (a mesma suíte de
  PR, duplicada) + nenhum `has-pending-model-changes`.
- Nenhum HMG Verification — readiness embutido dentro de `Deploy-BeeDay.ps1`, invisível como check
  distinto; nenhum smoke real.

## 4. Final Architecture

```text
sprint/*
   |
   v
PR -> hmg
   |
   v
BeeDay -- Pull Request Validation  (Fast Gate: Restore, Build, Domain+Application Tests,
   |                                 Publish, EF bundle)
   v
artifacts: beeday-publish, beeday-migrations
   |
   v
merge hmg
   |
   v
BeeDay -- HMG Deployment  (push:hmg direto, resolve PR/source-SHA/validation-run por
   |                        proveniencia, download por run-id pinado, deploy IIS)
   v
BeeDay -- HMG Verification  (workflow_run, guard event==push, readiness + smoke reais)
   |
   v
HMG approved state
   |
   v
PR -> main
   |
   v
BeeDay -- Promotion Policy (fork/path check) + BeeDay -- Release Quality Gate (suite completa)
   |
   v
main
```

## 5. Workflow Inventory

| Path | Workflow name | Job id | Job/check name | Trigger | Runner | Concurrency |
|---|---|---|---|---|---|---|
| `ci.yml` | `BeeDay — Pull Request Validation` | `ci` | `Pull Request Validation` | `pull_request:[hmg]`, `workflow_dispatch` | `windows-latest` | `beeday-ci-*`, cancel=true |
| `deploy-hmg.yml` | `BeeDay — HMG Deployment` | `deploy` | `Deploy HMG` | `push:[hmg]`, `workflow_dispatch` | self-hosted `[Windows,X64,hmg]` | `beeday-homologation`, cancel=false |
| `verify-hmg.yml` | `BeeDay — HMG Verification` | `verify` | `Verify HMG` | `workflow_run:[BeeDay — HMG Deployment]`, `workflow_dispatch` | self-hosted `[Windows,X64,hmg]` | `beeday-hmg-verification`, cancel=true |
| `release-quality-gate.yml` | `BeeDay — Release Quality Gate` | `quality-gate` | `Release Quality Gate` | `pull_request:[main]`, `workflow_dispatch` | `windows-latest` | `beeday-release-quality-gate-*`, cancel=true |
| `validate-promotion.yml` | `BeeDay — Promotion Policy` | `validate-promotion` | `Validate Promotion` | `pull_request:[main,prd]` | `ubuntu-latest` | `beeday-validate-promotion-*`, cancel=true |
| `deploy-prd.yml` | `BeeDay — Production Deployment` | `deploy` | `Deploy Production` | `push:[prd]`, `workflow_dispatch` | self-hosted `[Windows,X64]` | `beeday-production`, cancel=false |

**Artifacts produzidos/consumidos:**

| Artifact | Produtor | Consumidor | Retenção |
|---|---|---|---|
| `beeday-publish` | `ci.yml` | `deploy-hmg.yml`, `deploy-prd.yml` | 7 dias |
| `beeday-migrations` | `ci.yml` | `deploy-hmg.yml` | 7 dias |
| `beeday-test-results` | `ci.yml` | nenhum (inspeção manual) | 14 dias |
| `beeday-release-gate-test-results` | `release-quality-gate.yml` | nenhum (inspeção manual) | 14 dias |
| `beeday-e2e-failure-artifacts` | `release-quality-gate.yml` | nenhum (inspeção manual) | 14 dias |
| `beeday-hmg-deployment-info` | `deploy-hmg.yml` | `verify-hmg.yml` | 14 dias |

## 6. Ruleset Matrix

`FACT`, `gh api` real, reconsultado nesta Sprint:

| Branch | Ruleset ID | Required checks | Strict policy | Review count | Merge methods | Bypass | Enforcement |
|---|---|---|---|---|---|---|---|
| `hmg` | `20580759` | `Pull Request Validation` | `true` | 0 | merge/squash/rebase | `[]` | `active` |
| `main` | `20608232` | `Release Quality Gate`, `Validate Promotion` | `false` | 0 | merge/squash/rebase | `[]` | `active` |
| `prd` | nenhum | — | — | — | — | — | — |

Confirmado exatamente conforme esperado. Nenhuma mutação nesta Sprint.

## 7. Validation Stage Matrix

| Validação | Fast Gate (`hmg`) | Release Gate (`main`) |
|---|---|---|
| Format | Não | Sim |
| Build (`--warnaserror`) | Sim | Sim |
| Domain.Tests + boundary | Sim | Sim |
| Application.Tests + boundary | Sim | Sim |
| Infrastructure.Tests | Não | Sim |
| Web.Tests | Não | Sim |
| E2E.Tests + Playwright | Não | Sim |
| Publish + validação | Sim | Sim |
| EF bundle + validação | Sim | Sim |
| `has-pending-model-changes` | Não | Sim |
| Promotion path (fork/branch) | Não | Sim (`Validate Promotion`) |

**Coverage moved, não coverage deleted** — confirmado por leitura direta de `release-quality-gate.yml`, inalterado desde a Sprint 19.7 (exceto fix de PS5.1 na 19.8.2).

## 8. Artifact Provenance

Cadeia auditada (Sprint 19.8, endurecida na 19.8.3): PR → `listPullRequestsAssociatedWithCommit` →
`head.sha` (nunca topologia Git — Ruleset de `hmg` permite squash/rebase) → checagem de
mesmo-repositório (fail-closed) → busca paginada de `ci.yml` `pull_request` `success` para o
`head_sha` exato → download por `run-id` pinado. `deploy-hmg.yml`'s guard (19.8.3) garante que
`verify-hmg.yml` só confia em deployments cujo evento de origem seja `push`/`workflow_dispatch`
— nunca `workflow_run` (defeito de transição já corrigido). `deploy-prd.yml` replica o mesmo
padrão em 2 hops (`prd←main←hmg`). Nenhum redesenho nesta Sprint — auditado, correto.

## 9. HMG Deployment Contract

`push:hmg` direto (Sprint 19.8) → resolve PR/source-SHA/validation-run → download
`beeday-publish`/`beeday-migrations` por `run-id` → `Verify .NET SDK` → promove script IIS
privilegiado → `Deploy-BeeDay.ps1` (backup, stop, migrations, copy, start, readiness, rollback em
falha) → `Record deployment info` (JSON com `sourceSha`/`mergeSha`/`pullRequest`/
`validationRunId`) → upload `beeday-hmg-deployment-info`.

## 10. HMG Verification Contract

`workflow_run` em `BeeDay — HMG Deployment`, guardado por `event in [push, workflow_dispatch]`
(19.8.3) → download `deployment-info` → `Verify Readiness` (`/health/ready`, 6 tentativas) →
`Run Smoke Tests` (`GET /login`, HTTP 200 + marcador) → summary.

## 11. Promotion Contract

`Validate Promotion` (fork/path check, ubuntu, ~7s, roda em paralelo) + `Release Quality Gate`
(suíte completa, windows, ~7min, critical path real da fronteira). `Pull Request Validation`
**confirmado não disparar** nesta fronteira (evidência real, §12).

## 12. Performance Before × After

| | Amostra | Média | Mediana | Min | Max | Classificação |
|---|---|---|---|---|---|---|
| `BeeDay CI` (pré-19.8.5) | n=5 | 6m23s (383s) | 6m22s (382s) | 5m00s | 7m39s | `MEASURED REMOTELY` |
| `Pull Request Validation` (pós-19.8.5/6) | n=2 | 2m26s (146.5s) | — | 2m11s | 2m42s | `MEASURED REMOTELY` (amostra pequena — ver nota) |

**Economia absoluta:** 236.5s (3m56.5s). **Redução:** 61.8% (medição por janela total de run,
`created_at`→`updated_at`; medições anteriores citando 66.8% usaram duração do job reportada por
`gh pr checks`, que exclui alguns segundos de overhead de fila/setup — ambas as metodologias são
válidas, reportadas aqui lado a lado para transparência).

**Nota honesta sobre tamanho de amostra:** apenas 2 execuções reais existem para a arquitetura
atual (`Pull Request Validation`), porque o redesenho (Sprint 19.8.5) e o rename (19.8.6)
ocorreram há poucas horas no momento desta auditoria. O alvo de "pelo menos 5 amostras" do escopo
desta Sprint não foi atingido para esta categoria especificamente — não por omissão, mas porque a
mudança é recente demais. Recomendado como item de acompanhamento (não bloqueante — ver §21).

## 13. Runner Work Before × After

| | Antes | Depois |
|---|---|---|
| PR → hmg | 1x suíte completa (~6-8min) | 1x Fast Gate (~2-3min) |
| Pós-merge hmg | +1x suíte completa (~6-8min, duplicada) | 0 (eliminado) |
| Deployment HMG | ocasionalmente 2x (duplicado) | 1x sempre |
| hmg → main | suíte completa (mesma da PR, duplicada) | `Validate Promotion` (~7s) + `Release Quality Gate` (~7min, suíte completa) |

**Total runner-minutes por ciclo Sprint→HMG:** antes ≈ 12-16min (2x full suite + deploy);
depois ≈ 2-3min (Fast Gate) + deploy (~2min) — full suite não paga novamente até `hmg→main`.

## 14. Cache Analysis

`FACT`, evidência real de log (`gh run view --log`):

| Cache | Evidência | Classificação |
|---|---|---|
| NuGet (`setup-dotnet` nativo) | `"Cache hit for..."` confirmado em run real (`31535910823`) | `EFFECTIVE` |
| Playwright (`release-quality-gate.yml`) | `"Cache not found..."` em **4/4** amostras reais recentes | `INEFFECTIVE` — root cause: `release-quality-gate.yml` só dispara via `pull_request` (nunca `push`), então nunca popula um cache persistente numa branch estável; cada PR de promoção é um escopo novo sem cache herdável. **Recomendação (não implementada nesta Sprint):** avaliar se vale a pena manter — o custo do miss (~20-25s de `Install Playwright Chromium`) é modesto frente ao total do gate (~7min), então não é `COUNTERPRODUCTIVE`, apenas nunca traz o benefício pretendido. |

## 15. Failure Localization

Incidentes reais da EPIC, e se a arquitetura atual localiza a falha corretamente:

| Incidente | Fronteira correta identificada hoje? |
|---|---|
| `publish --no-build` (19.5.2) | Sim — falha isolada ao step `Publish BeeDay`, não ao job inteiro |
| PowerShell em dash (19.8.1/19.8.2) | Sim — falha isolada ao step de summary, deployment real já confirmado `SUCCESS` separadamente |
| HMG Verification artifact mismatch (19.8.3) | Sim — falha isolada a `Download deployment info`, deployment real já confirmado `SUCCESS` |
| Legacy `workflow_run` deployment (19.8.3) | Sim — guard evita a execução (`skipped`, não falha vermelha), confirmado remotamente (2 casos reais pós-fix) |

Comparado ao AS-IS monolítico (um único job de ~6-8min onde qualquer falha aparecia como "o CI
quebrou", sem diferenciar validação/artifact/deploy/verificação), a arquitetura atual localiza a
causa por fronteira — confirmado com 4 incidentes reais, não apenas hipoteticamente.

## 16. Observability Contract

Confirmado, workflow a workflow: cada um permite descobrir trigger, evento, source SHA, PR,
run ID, duração, conclusão, artifact relacionado, e workflow downstream — via dados nativos do
GitHub Actions (`gh run view --json/--log`, `check-runs` API). Nenhuma plataforma externa
introduzida (Grafana/Prometheus/Datadog/etc.) — conforme decisão da EPIC.

## 17. Concurrency Model

| Workflow | Group | Cancel-in-progress | Racional |
|---|---|---|---|
| `Pull Request Validation` | por ref | `true` | Push adicional na mesma PR não precisa dos resultados do anterior |
| `HMG Deployment` | fixo (`beeday-homologation`) | `false` | Nunca cancelar um deploy em andamento — enfileira |
| `HMG Verification` | fixo | `true` | Somente leitura, resultado mais novo é o que importa |
| `Release Quality Gate` | por PR/ref | `true` | Mesmo racional da PR Validation |
| `Validate Promotion` | por PR | `true` | Barato, sem risco |
| `Production Deployment` | fixo | `false` | Mesmo racional do HMG Deployment |

Nenhuma alteração nesta Sprint — decisões já corretas e reconfirmadas.

## 18. Build Once / Deploy Many Status

| Fronteira | Status |
|---|---|
| Sprint → HMG | `IMPLEMENTED` — artifact validado pela PR é reaproveitado, nunca reconstruído |
| HMG → MAIN | `NOT IMPLEMENTED` (deliberado) — `Release Quality Gate` revalida a suíte completa, por design (fronteira de release exige rigor máximo, não reaproveitamento) |
| MAIN → PRD | `IMPLEMENTED` (arquitetura), `NOT VALIDATED` (nunca rodou com sucesso — PRD não provisionado) |

## 19. Documentation Reconciliation

`docs/architecture/08-deployment-architecture.md` — §1/§2 reescritos nesta Sprint (job `validate`
inexistente, triggers obsoletos, runner "SERV3-WEB1" não confirmado no YAML atual). §3-9
preservados (ainda precisos). `docs/deployment/06-...` (baseline histórico) — não reescrito.

## 20. Epic Acceptance Criteria

| # | Critério | Veredito | Evidência |
|---|---|---|---|
| 1 | PR de Sprint dispara validação única intencional | `PASS` | 1 run de `Pull Request Validation` por push na PR, confirmado |
| 2 | Merge em HMG evita repetir CI aprovado | `PASS` | Zero runs de `Pull Request Validation` pós-merge, confirmado em 6 merges reais |
| 3 | Cada estado HMG gera no máximo 1 deployment normal | `PASS` | 6 deployments `push` reais, todos únicos por commit |
| 4 | Deploys superseded tratados deterministicamente | `PASS` | `cancel-in-progress:false` — enfileira, nunca corrompe |
| 5 | HMG possui readiness/smoke | `PASS` | `Verify Readiness` + `Run Smoke Tests`, 4 execuções reais bem-sucedidas |
| 6 | `hmg→main` executa Full Quality Gate | `PASS` | `Release Quality Gate`, 5 execuções reais, suíte completa |
| 7 | Cada teste/check possui estágio definido | `PASS` | Matriz §7 |
| 8 | Pipeline possui métricas before/after | `PASS` (com ressalva) | §12 — amostra pós-mudança pequena (n=2), não um defeito |
| 9 | Cache/performance investigados | `PASS` | §14 |
| 10 | Workflow/job/check names profissionais | `PASS` | §5, taxonomia final confirmada |
| 11 | Rulesets correspondem às fronteiras | `PASS` | §6 |
| 12 | Artifact provenance rastreável | `PASS` | §8 |
| 13 | Build Once/Deploy Many implementado ou limitado explicitamente | `PASS` | §18, limitações declaradas explicitamente |
| 14 | Merge strategy formalizada | `PASS` | `allowed_merge_methods` documentado, resolução via API de PR (não topologia Git) |
| 15 | Observabilidade operacional | `PASS` | §16 |
| 16 | Nenhuma infra PRD/Azure antecipada | `PASS` | Confirmado — nenhuma mudança em `deploy-prd.yml`/PRD nesta EPIC além do já existente |
| 17 | Documentação corresponde à implementação | `PASS` | §19, gap fechado nesta Sprint |

**17/17 PASS.**

## 21. Remaining Debt

| Item | Severidade | Bloqueia encerramento? |
|---|---|---|
| A. `BeeDay.slnx` não propaga Release para `src/` | `MEDIUM` | `NO` — mitigado pela ordem correta dos steps em todos os workflows afetados |
| B. `deploy-prd.yml` sem checagem `head.repo` equivalente à de `deploy-hmg.yml` | `LOW` | `NO` — PRD nunca executou com sucesso, sem exposição real ainda |
| C. Ruleset de `prd` ausente | `LOW` | `NO` — decisão arquitetural deliberada (PRD não provisionado) |
| D. Produção não ativada / Azure não antecipado | `N/A` | `NO` — fronteira preservada deliberadamente |
| E. Amostra de performance pós-19.8.5/6 pequena (n=2) | `LOW` | `NO` — crescerá organicamente |
| F. Playwright cache em `Release Quality Gate` nunca hita | `LOW` | `NO` — custo modesto, não afeta correção |
| G. Rename final de `BeeDay CI`... | `RESOLVED` | — já concluído na Sprint 19.8.6 |

## 22. Recommended Follow-up Work

- Reavaliar item E após mais execuções reais de `Pull Request Validation` (não requer ação agora).
- Avaliar item F (Playwright cache) numa manutenção futura de performance, se o custo agregado do
  `Release Quality Gate` crescer.
- Item A (`BeeDay.slnx`) continua candidato a uma Sprint dedicada fora desta EPIC.
- Item B (`deploy-prd.yml` provenance hardening) é candidato natural para quando PRD for
  efetivamente provisionado.

## 23. Final Verdict

**EPIC 19 — COMPLETE.**

Todos os 17 critérios de aceite originais atingidos com evidência real. Nenhum blocker
identificado. As dívidas remanescentes (§21) são todas classificadas como não-bloqueantes,
pré-existentes ou deliberadamente fora do escopo desta EPIC (PRD).
