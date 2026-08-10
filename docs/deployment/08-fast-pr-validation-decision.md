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
| `push: hmg` em `ci.yml` | Único gatilho confiável de `deploy-hmg.yml` hoje | `ci.yml` | `BeeDay — HMG Deployment` (redesenhado) ou mecanismo de proveniência dedicado | 19.6 |
| `pull_request: main` em `ci.yml` | Satisfaz o required check `"BeeDay CI"` do Ruleset de `main` | `ci.yml` | `BeeDay — Release Quality Gate` | 19.7 |

**Condição de desbloqueio do rename:** o rename `BeeDay CI → BeeDay — Pull Request Validation`
(workflow e, separadamente, o job/check — sujeito a mutação de Ruleset com plano de transição
próprio) só deve ser reavaliado depois que **ambas** as linhas da tabela acima tiverem um dono
futuro implementado e funcional.

---

## 7. Ruleset Analysis (reconfirmado, nenhuma mutação necessária)

`FACT`

| Branch | Ruleset | Required Check Atual | Required Check Alvo | Migração necessária agora? |
|---|---|---|---|---|
| `hmg` | 20580759 | `BeeDay CI` | inalterado | Não |
| `main` | 20608232 | `BeeDay CI`, `Validate Promotion` | inalterado | Não |
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

## 12. Fontes consultadas

- `.github/workflows/ci.yml`, `deploy-hmg.yml`, `deploy-prd.yml`, `validate-promotion.yml`.
- `gh api repos/tiagoarrigoni/BeeDay/rules/branches/{hmg,main,prd}` (reconsultado nesta Sprint).
- `docs/deployment/06-cicd-pipeline-discovery-baseline.md`, `07-validation-matrix.md`.
- `CLAUDE.md` (governança de mutação remota, seção 5.11).
