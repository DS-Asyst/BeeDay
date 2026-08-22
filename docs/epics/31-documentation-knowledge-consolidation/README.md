# EPIC 31 — beeday Repository Documentation & Knowledge Consolidation

**GitHub Issue:** [#228](https://github.com/tiagoarrigoni/BeeDay/issues/228)
**Status:** Em andamento — Sprints 31.1 e 31.2 concluídas

## Objetivo

Consolidar a documentação pós-EPIC-30 em um sistema de conhecimento verificado, atual e
não-duplicado, para que o repositório seja a fonte da verdade autoritativa sobre produto,
arquitetura, engenharia, testes, deployment, operações, segurança e o beeday Experience System.

Esta EPIC não reescreve documentação por estilo — ela prova qual documentação é atual, qual é
histórica, qual está superada, qual precisa de reconciliação e qual pode ser removida com segurança.
O contrato completo (evidence hierarchy, ownership canônico, Documentation Ledger, preservação
histórica, nomenclatura, governança de agentes, validação e critério de conclusão) está registrado
na Issue [#228](https://github.com/tiagoarrigoni/BeeDay/issues/228) e não é duplicado aqui.

## Pré-condição verificada

EPIC 30 — beeday System Integrity & Complete Engineering Audit está formalmente concluída:

- Todas as 30 Issues/Sub-issues da EPIC 30 (#196–#227) estão `CLOSED` no GitHub.
- `docs/epics/30-system-integrity/README.md` declara explicitamente **"EPIC 30 — COMPLETE"**
  (linha 4731) e release readiness **`READY`** (linha 4292).
- Nenhum achado Critical/High permanece não resolvido nessa auditoria.

Pré-condição da EPIC 31 (Issue #228, seção "Preconditions") satisfeita.

---

## Sprint 31.1 — Documentation Baseline & Knowledge Inventory

**GitHub Issue:** [#229](https://github.com/tiagoarrigoni/BeeDay/issues/229)
**Branch:** `sprint/31.1-documentation-baseline-inventory`
**Depende de:** EPIC 30 Sprint 30.30 concluída (verificado acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Construir um inventário completo, baseado em evidência, de toda a documentação e artefatos de
conhecimento durável do repositório, antes de qualquer mudança estrutural.

### Metodologia

1. Enumeração exaustiva via `git ls-files docs .github scripts README.md` (arquivos rastreados) e
   `find`/`ls` direto no disco para artefatos locais-apenas explicitamente em escopo (governança de
   IA, artefatos de ferramenta não rastreados).
2. Para áreas com status já autodeclarado em `docs/README.md` (tabela "Áreas", fixada na Sprint
   16.2 e atualizada por Sprints subsequentes), esse status — com a Sprint de verificação citada —
   foi adotado como evidência do estado atual. Reverificação profunda de cada assunto contra
   código/testes é o trabalho explícito das Sprints 31.4–31.14, não desta Sprint (ver "Sprint-specific
   boundary" em #229: "Inventory first. Do not perform broad reconstruction... in this Sprint.").
3. Onde a evidência se contradisse (ex.: `docs/README.md` vs. o autorrelato de um documento) ou o
   próprio documento já declarava uma ressalva (ex.: `docs/api/README.md`, status da EPIC 28), o
   estado registrado reflete essa divergência (`STALE` / `PARTIALLY_CURRENT` / `DUPLICATED`) em vez
   de herdar cegamente o status de área.
4. Buscas por marcadores legados conhecidos (`LevelUp`/`Level Up`, terminologia de persistência JSON
   legada, áreas reservadas/incompletas) executadas com `grep`/`Grep` sobre `docs/`.
5. Nenhum arquivo foi movido, mesclado, reescrito ou removido nesta Sprint.

### Cobertura

- **150 arquivos rastreados pelo Git** em `docs/`, `.github/`, `scripts/` e `README.md` da raiz
  (`git ls-files docs .github scripts README.md`, executado nesta sessão).
- **34 artefatos locais-apenas** (untracked por design ou por natureza) também inventariados:
  - `CLAUDE.md`, `AGENTS.md`, `.claude/` (12 arquivos), `.agents/` (10 arquivos), `.codex/`
    (3 arquivos) — governança de IA, untracked desde o commit `869b57e`
    ("chore(governance): stop tracking CLAUDE.md in the product repository", 2026-08-20) — total
    27 arquivos. (Corrigido na Sprint 31.2 — a contagem original da Sprint 31.1 dizia 11 arquivos em
    `.claude/`, mas a tabela do Ledger sempre teve as 12 linhas corretas; apenas este texto
    narrativo estava com a soma errada.)
  - `.github/upgrades/scenarios/dotnet-version-upgrade/` — 6 arquivos gerados por uma ferramenta de
    upgrade assessment do .NET (`net11.0`), datados de 2026-08-20, não rastreados.
  - `scripts/iis-control/Provision-BeeDayHmgIisControl.ps1.orig` — 1 arquivo órfão não rastreado,
    sem equivalente em nenhuma outra pasta de `scripts/`.
- **Total: 184 entradas** no Documentation Ledger, IDs `DOC-001` a `DOC-184`.
- **Excluídos conscientemente do Ledger:** `LICENSE`, `.editorconfig`, `.gitattributes`,
  `Directory.Build.props`, `Directory.Packages.props`, `BeeDay.slnx`. São contratos de
  repositório/ferramenta (leitura obrigatória pela seção 2.1 do `CLAUDE.md`), não documentação
  narrativa — fora do escopo textual explícito da Sprint 31.1 (Issue #229, seção "Scope").

### Distribuição por estado (`Working state`, Global Contract §4)

| Estado | Rastreados (Git) | Locais-apenas | Total |
|---|---|---|---|
| `CURRENT` | 85 | 0 | 85 |
| `NOT REVIEWED` | 45 | 27 | 72 |
| `UNKNOWN` | 0 | 7 | 7 |
| `HISTORICAL` | 15 | 0 | 15 |
| `PARTIALLY_CURRENT` | 3 | 0 | 3 |
| `DUPLICATED` | 1 | 0 | 1 |
| `STALE` | 1 | 0 | 1 |
| **Total** | **150** | **34** | **184** |

Todos os estados acima pertencem ao vocabulário de `Working states` definido no Global Execution
Contract (#228, §4). Nenhum artefato ficou sem classificação de estado — `NOT REVIEWED` e `UNKNOWN`
são estados de trabalho válidos nesta fase de inventário, não uma omissão.

### Achados principais (routing para Sprints subsequentes)

1. **STALE** — a tabela de áreas em `docs/README.md` ainda descreve a EPIC 30 como "Em andamento —
   baseline criado na Sprint 30.1" (`DOC-012`), enquanto `docs/epics/30-system-integrity/README.md`
   declara "EPIC 30 — COMPLETE" e todas as suas 30 Issues estão `CLOSED` (`DOC-097`). Contradição de
   uma única causa raiz — rotear para 31.13/31.14.
2. **PARTIALLY_CURRENT** — a árvore "Repository structure" do `README.md` raiz ainda lista
   `CLAUDE.md` como se fosse rastreado (`DOC-010`, linha 64), contradizendo a política vigente desde
   o commit `869b57e`. Rotear para 31.3/31.14.
3. **DUPLICATED** — `docs/domain/audit-inventory.md` (`DOC-077`) duplica o inventário da Sprint
   30.5 já incorporado em `docs/epics/30-system-integrity/README.md`. Rotear para 31.5/31.13 para
   decisão de merge/supersessão.
4. **PARTIALLY_CURRENT** — `docs/epics/28-transactional-email-experience/README.md` (`DOC-096`)
   permanece com status "IMPLEMENTATION READY — POST-MERGE HMG VALIDATION PENDING" (Sprint 28.10),
   anterior à auditoria completa de CI/CD e HMG da EPIC 30 — não está claro se já foi superado.
   Rotear para 31.11/31.13.
5. **Áreas reservadas/incompletas confirmadas sem conteúdo próprio:** `docs/authentication/README.md`
   (aponta para `docs/security/01-security-baseline.md`), `docs/developer/README.md` (aponta para o
   `README.md` raiz) e `docs/api/` parcialmente (`beeday.v1.yaml` "não reauditado quanto ao conteúdo"
   desde a Sprint 15.5).
6. **Divergência entre adaptadores de governança de IA** — `.claude/skills/` tem 9 arquivos de skill
   incluindo `beeday-ui-ux` (`DOC-164`); `.agents/skills/` tem apenas 8, sem nenhum equivalente a
   `beeday-ui-ux` (`DOC-174`); `.codex/` usa um único arquivo de regras consolidado em vez de
   arquivos por skill (`DOC-177`) — paridade de conteúdo entre os três adaptadores ainda não
   verificada. Rotear para 31.3.
7. **Artefato órfão não rastreado** — `scripts/iis-control/Provision-BeeDayHmgIisControl.ps1.orig`
   (`DOC-184`) tem extensão `.orig`, típica de backup/merge deixado para trás, sem padrão equivalente
   em nenhum outro script do repositório. Rotear para 31.3/31.12.
8. **Artefato de ferramenta com ownership desconhecido** — `.github/upgrades/scenarios/
   dotnet-version-upgrade/*` (`DOC-178`–`DOC-183`) é a saída de uma ferramenta de avaliação de
   upgrade do .NET (alvo `net11.0`), datada de 2026-08-20, não rastreada e sem relação com
   documentação de produto. Disposição final (manter como scratch local / `.gitignore` / remover)
   não está clara — sinalizado para esclarecimento do owner via 31.3; nenhuma ação tomada aqui.
9. **Classificação de validade de ADR intencionalmente adiada** — nenhum ADR (`docs/adr/ADR-00
   1`–`006`) foi pré-classificado como `VALID`/`SUPERSEDED`/`OBSOLETE`/`CONFLICTING`/`NEW ADR
   REQUIRED` nesta Sprint. Essa classificação é trabalho obrigatório explícito da Sprint 31.2
   (Global Contract §1 e Issue #230).
10. **Terminologia legada `LevelUp`/JSON** aparece em 30 arquivos do repositório (busca `grep`
    executada nesta sessão), incluindo documentos de estado atual como `docs/domain/business-
    rules.md` e `docs/domain/domain-events.md` (`DOC-078`, `DOC-079`) — sinalizado para verificação,
    por área, de que essas menções são apenas contexto histórico e não afirmações de estado atual
    (majoritariamente devido às Sprints 31.4–31.13, por assunto).

### Documentation Ledger

Colunas: `ID` · `Path` · `Category` · `Current state` · `Evidence` · `Notes / routing`.

`Owner topic`, `Owning Sprint` e `Final state` — os três campos restantes exigidos pelo schema do
Ledger (Global Contract §4) — são deliberadamente **`PENDING — Sprint 31.2`** para todas as 184
entradas nesta Sprint: o próprio Global Contract (§3) atribui essa decisão de ownership canônico à
Sprint 31.2 ("Sprint 31.2 must assign every Documentation Ledger item to: a canonical owner topic;
an owning Sprint; an intended final state"), e o required work da Sprint 31.1 pede apenas para
"record unknown ownership explicitly for resolution in 31.2" — não para antecipá-la. A coluna
"Notes / routing" abaixo já indica, a título informativo e não vinculante, a Sprint especialista
cujo escopo declarado (Issue #228, "Sprint Roadmap") mais provavelmente cobrirá cada assunto.

| ID | Path | Category | Current state | Evidence | Notes / routing |
|---|---|---|---|---|---|
| DOC-001 | `.github/dependabot.yml` | GitHub repository configuration | NOT REVIEWED | File exists in .github/ | Not durable narrative documentation; verify PR template content still matches current contribution/PR expectations in 31.11. |
| DOC-002 | `.github/pull_request_template.md` | GitHub repository configuration | NOT REVIEWED | File exists in .github/ | Not durable narrative documentation; verify PR template content still matches current contribution/PR expectations in 31.11. |
| DOC-003 | `.github/workflows/ci.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-004 | `.github/workflows/codeql.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-005 | `.github/workflows/deploy-hmg.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-006 | `.github/workflows/deploy-prd.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-007 | `.github/workflows/release-quality-gate.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-008 | `.github/workflows/validate-promotion.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-009 | `.github/workflows/verify-hmg.yml` | CI/CD workflow (source of truth, not doc) | NOT REVIEWED | File exists in .github/workflows/ | Workflow itself is implementation, referenced by docs/deployment/ and root README as evidence source; cross-check of workflow-name/trigger claims owed to 31.11. |
| DOC-010 | `README.md` | Root entry point | PARTIALLY_CURRENT | Root README self-content | Lists `CLAUDE.md` in the repository-structure tree (line 64) even though CLAUDE.md was untracked from Git by commit 869b57e (2026-08-20). Stale reference — route to 31.3/31.14. |
| DOC-011 | `docs/CONVENTIONS.md` | Documentation conventions | CURRENT | Self-normative, approved Sprint 16.2 | Authoritative per Global Contract §5; unchanged unless 31.2/31.14 finds evidenced need. |
| DOC-012 | `docs/README.md` | Documentation index | CURRENT | Self-declared area table, taxonomy fixed Sprint 16.2 | EPIC 30 row still says "Em andamento—baseline criado na Sprint 30.1" while EPIC 30 is formally CLOSED/COMPLETE (GitHub #201-#227, docs/epics/30-system-integrity/README.md L4731). STALE — route to 31.14. |
| DOC-013 | `docs/_templates/adr.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-014 | `docs/_templates/aggregate.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-015 | `docs/_templates/api.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-016 | `docs/_templates/architecture-document.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-017 | `docs/_templates/component.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-018 | `docs/_templates/deployment-guide.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-019 | `docs/_templates/design-system-component.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-020 | `docs/_templates/feature.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-021 | `docs/_templates/security-guide.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-022 | `docs/_templates/testing-guide.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-023 | `docs/_templates/use-case.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-024 | `docs/_templates/ux-flow.md` | Documentation template | NOT REVIEWED | docs/CONVENTIONS.md §2 (templates live in _templates/) | Scaffolding, not system documentation; verify still matches current headings/labels convention in 31.2. |
| DOC-025 | `docs/adr/ADR-001-contract-first.md` | ADR | NOT REVIEWED | docs/adr/README.md index | Immutable historical record by convention; current validity classification deferred to 31.2 per Global Contract §1/§5. |
| DOC-026 | `docs/adr/ADR-002-greenfield-database.md` | ADR | NOT REVIEWED | docs/adr/README.md index | Immutable historical record by convention; current validity classification deferred to 31.2 per Global Contract §1/§5. |
| DOC-027 | `docs/adr/ADR-003-aggregate-repositories.md` | ADR | NOT REVIEWED | docs/adr/README.md index | Immutable historical record by convention; current validity classification deferred to 31.2 per Global Contract §1/§5. |
| DOC-028 | `docs/adr/ADR-004-sql-server-runtime-cutover.md` | ADR | NOT REVIEWED | docs/adr/README.md index | Immutable historical record by convention; current validity classification deferred to 31.2 per Global Contract §1/§5. |
| DOC-029 | `docs/adr/ADR-005-json-legacy-removal.md` | ADR | NOT REVIEWED | docs/adr/README.md index | Immutable historical record by convention; current validity classification deferred to 31.2 per Global Contract §1/§5. |
| DOC-030 | `docs/adr/ADR-006-transactional-email-localization-boundary.md` | ADR | NOT REVIEWED | docs/adr/README.md index | Immutable historical record by convention; current validity classification deferred to 31.2 per Global Contract §1/§5. |
| DOC-031 | `docs/adr/README.md` | ADR index | NOT REVIEWED | docs/README.md: "Correto (histórico, imutável)" | Per-ADR validity (VALID/SUPERSEDED/OBSOLETE/CONFLICTING/NEW ADR REQUIRED) is explicit Sprint 31.2 required work; not pre-classified here. |
| DOC-032 | `docs/api/README.md` | API documentation | PARTIALLY_CURRENT | docs/api/README.md self-declaration | `beeday.v1.yaml` explicitly "não reauditado quanto ao conteúdo" — only renamed from levelup.v1.yaml (Sprint 15.5). Reserved/incomplete area — final disposition owed in 31.14. |
| DOC-033 | `docs/api/beeday.v1.yaml` | API spec (OpenAPI) | NOT REVIEWED | docs/api/README.md | Content not reaudited since Sprint 15.5 rename; verify against actual consumers in 31.14. |
| DOC-034 | `docs/application/01-cqrs.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-035 | `docs/application/02-use-cases.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-036 | `docs/application/03-pipeline.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-037 | `docs/application/04-contracts.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-038 | `docs/application/05-exceptions.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-039 | `docs/application/06-dependency-flow.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-040 | `docs/application/README.md` | Application docs | CURRENT | docs/README.md: reconstruído Sprint 16.5 | Deep reconciliation against current CQRS/handlers owed to 31.6. |
| DOC-041 | `docs/architecture/01-overview.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-042 | `docs/architecture/02-solution-structure.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-043 | `docs/architecture/03-clean-architecture.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-044 | `docs/architecture/04-dependency-rules.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-045 | `docs/architecture/05-runtime-flows.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-046 | `docs/architecture/06-persistence-architecture.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-047 | `docs/architecture/07-security-architecture.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-048 | `docs/architecture/08-deployment-architecture.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-049 | `docs/architecture/README.md` | Architecture docs | CURRENT | docs/README.md: reconstruído Sprint 16.3 | Deep reconciliation against current project references/ADR status owed to 31.4. |
| DOC-050 | `docs/authentication/README.md` | Authentication (reserved) | NOT REVIEWED | docs/authentication/README.md self-declaration | No own content yet — explicitly points to docs/security/01-security-baseline.md. Reserved/incomplete area listed in Sprint scope §5; final disposition owed to 31.10. |
| DOC-051 | `docs/brand/01-character-illustration.md` | Brand System docs | CURRENT | docs/README.md: formalizado EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-052 | `docs/brand/02-writing-voice-localization.md` | Brand System docs | CURRENT | docs/README.md: formalizado EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-053 | `docs/brand/03-color-palette.md` | Brand System docs | CURRENT | docs/README.md: formalizado EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-054 | `docs/brand/README.md` | Brand System docs | CURRENT | docs/README.md: formalizado EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-055 | `docs/deployment/01-deployment.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-056 | `docs/deployment/02-runtime-configuration.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-057 | `docs/deployment/03-observability.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-058 | `docs/deployment/04-operations.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-059 | `docs/deployment/05-privileged-iis-control.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-060 | `docs/deployment/06-cicd-pipeline-discovery-baseline.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-061 | `docs/deployment/07-validation-matrix.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-062 | `docs/deployment/08-fast-pr-validation-decision.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-063 | `docs/deployment/09-pipeline-performance.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-064 | `docs/deployment/10-hmg-deployment-verification.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-065 | `docs/deployment/11-release-quality-gate.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-066 | `docs/deployment/12-artifact-provenance.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-067 | `docs/deployment/13-epic19-final-architecture-report.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-068 | `docs/deployment/14-transactional-email-runbook.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-069 | `docs/deployment/README.md` | Deployment/Ops docs | CURRENT | docs/README.md: reconstruído Sprint 16.9, estendido EPIC 19/28 | Deep reconciliation against current workflows/runbooks owed to 31.11 (CI/CD subset) and 31.12 (IIS/HMG/ops subset). |
| DOC-070 | `docs/design-system/01-foundations.md` | Design System docs | CURRENT | docs/README.md: governança revalidada EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-071 | `docs/design-system/02-components.md` | Design System docs | CURRENT | docs/README.md: governança revalidada EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-072 | `docs/design-system/03-icons.md` | Design System docs | CURRENT | docs/README.md: governança revalidada EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-073 | `docs/design-system/04-forms.md` | Design System docs | CURRENT | docs/README.md: governança revalidada EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-074 | `docs/design-system/README.md` | Design System docs | CURRENT | docs/README.md: governança revalidada EPIC 25 | Deep reconciliation owed to 31.8. |
| DOC-075 | `docs/developer/README.md` | Developer guide (reserved) | NOT REVIEWED | docs/developer/README.md self-declaration | No own content yet — points to root README Requirements/Start locally. Reserved/incomplete area; final disposition owed to 31.14. |
| DOC-076 | `docs/domain/README.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-077 | `docs/domain/audit-inventory.md` | Domain docs | DUPLICATED | Header: "Sprint 30.5" Domain Complete Audit inventory | Subject (per-file Domain audit inventory) is already carried inside docs/epics/30-system-integrity/README.md (Sprint 30.5 section). Candidate duplicate canonical source — route to 31.5/31.13 for merge-or-supersede decision. |
| DOC-078 | `docs/domain/business-rules.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Contains a LevelUp/JSON-era reference (grep hit) — verify presented strictly as historical context, not current-state, in 31.5. |
| DOC-079 | `docs/domain/domain-events.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Contains a LevelUp/JSON-era reference (grep hit) — verify presented strictly as historical context, not current-state, in 31.5. |
| DOC-080 | `docs/domain/entities.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-081 | `docs/domain/habit.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-082 | `docs/domain/project.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-083 | `docs/domain/recurring-task.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-084 | `docs/domain/relationships.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-085 | `docs/domain/transaction.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-086 | `docs/domain/user-token.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-087 | `docs/domain/user.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-088 | `docs/domain/value-objects.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-089 | `docs/domain/wallet-tag.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-090 | `docs/domain/wallet.md` | Domain docs | CURRENT | docs/README.md: reconstruído Sprint 16.4 | Deep reconciliation against current Domain code/tests owed to 31.5. |
| DOC-091 | `docs/epics/20-home-visual-experience/README.md` | Epic history (EPIC 20) | HISTORICAL | docs/README.md: "Histórico concluído" | No action expected; preserve per Global Contract §5. |
| DOC-092 | `docs/epics/21-lingo-product-experience/README.md` | Epic history (EPIC 21) | HISTORICAL | docs/README.md: "Histórico concluído" | No action expected; preserve per Global Contract §5. |
| DOC-093 | `docs/epics/21-lingo-product-experience/color-audit-sprint-21.13.md` | Epic history (EPIC 21) | HISTORICAL | docs/README.md: "Histórico concluído" | No action expected; preserve per Global Contract §5. |
| DOC-094 | `docs/epics/21-lingo-product-experience/color-inventory-sprint-21.13.csv` | Epic history (EPIC 21) | HISTORICAL | docs/README.md: "Histórico concluído" | No action expected; preserve per Global Contract §5. |
| DOC-095 | `docs/epics/25-design-system-brand-evolution/README.md` | Epic history (EPIC 25) | HISTORICAL | docs/README.md: "Concluída na Sprint 25.16" | Also functions as brand/design governance reference reused by current docs (design-system/, brand/) — verify no unintended canonical-ownership overlap in 31.13. |
| DOC-096 | `docs/epics/28-transactional-email-experience/README.md` | Epic history (EPIC 28) | PARTIALLY_CURRENT | docs/README.md: "IMPLEMENTATION READY — POST-MERGE HMG VALIDATION PENDING" (Sprint 28.10) | Status predates EPIC 30's full CI/CD and HMG production-readiness audit; unclear whether still accurate or superseded by EPIC 30 evidence. Verify in 31.11/31.13. |
| DOC-097 | `docs/epics/30-system-integrity/README.md` | Epic history (EPIC 30) | STALE | File content itself declares "EPIC 30 — COMPLETE" (L4731) and release readiness READY (L4292), contradicting docs/README.md's own "Em andamento" row for this same area | Same contradiction as DOC row for docs/README.md; single root cause, two symptoms. Route to 31.13/31.14. |
| DOC-098 | `docs/history/README.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-099 | `docs/history/backup-restore-planning.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-100 | `docs/history/current-state-sprint-log.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-101 | `docs/history/domain-aggregate-map.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-102 | `docs/history/domain-persistence-map.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-103 | `docs/history/hmg-production-observability-planning.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-104 | `docs/history/json-to-sql-transition.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-105 | `docs/history/migration-status.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-106 | `docs/history/persistence-contracts.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-107 | `docs/history/target-architecture-sprint-log.md` | Historical documentation | HISTORICAL | docs/README.md: "Correto (histórico, congelado)" | Frozen by convention (docs/CONVENTIONS.md §13); no update expected. Verify none is silently relied upon as current-state evidence elsewhere (31.13). |
| DOC-108 | `docs/infrastructure/01-repositories.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-109 | `docs/infrastructure/02-sql-server.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-110 | `docs/infrastructure/03-concurrency.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-111 | `docs/infrastructure/04-services.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-112 | `docs/infrastructure/05-dependency-injection.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-113 | `docs/infrastructure/06-transactional-email.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-114 | `docs/infrastructure/README.md` | Infrastructure docs | CURRENT | docs/README.md: reconstruído Sprint 16.6; cache removido Sprint 18.6 | Deep reconciliation owed to 31.7. |
| DOC-115 | `docs/persistence/01-relational-model.md` | Persistence docs | CURRENT | docs/README.md: reconstruído Sprint 16.6 | Deep reconciliation owed to 31.7. |
| DOC-116 | `docs/persistence/02-ef-core-strategy.md` | Persistence docs | CURRENT | docs/README.md: reconstruído Sprint 16.6 | Deep reconciliation owed to 31.7. |
| DOC-117 | `docs/persistence/README.md` | Persistence docs | CURRENT | docs/README.md: reconstruído Sprint 16.6 | Deep reconciliation owed to 31.7. |
| DOC-118 | `docs/security/01-security-baseline.md` | Security docs | CURRENT | docs/README.md: baseline Sprint 16.9, nomenclatura corrigida Sprint 16.10 | Overlaps declared with docs/authentication/ (reserved); canonical ownership boundary owed to 31.2/31.10. |
| DOC-119 | `docs/security/02-operational-security.md` | Security docs | CURRENT | docs/README.md: baseline Sprint 16.9, nomenclatura corrigida Sprint 16.10 | Overlaps declared with docs/authentication/ (reserved); canonical ownership boundary owed to 31.2/31.10. |
| DOC-120 | `docs/security/README.md` | Security docs | CURRENT | docs/README.md: baseline Sprint 16.9, nomenclatura corrigida Sprint 16.10 | Overlaps declared with docs/authentication/ (reserved); canonical ownership boundary owed to 31.2/31.10. |
| DOC-121 | `docs/testing/01-testing-strategy.md` | Testing docs | CURRENT | docs/README.md: reconstruído Sprint 16.9 | Deep reconciliation (test counts, LocalDB/Playwright strategy) owed to 31.9. |
| DOC-122 | `docs/testing/02-design-system-quality-gates.md` | Testing docs | CURRENT | docs/README.md: reconstruído Sprint 16.9 | Deep reconciliation (test counts, LocalDB/Playwright strategy) owed to 31.9. |
| DOC-123 | `docs/testing/03-functional-journey-matrix.md` | Testing docs | CURRENT | docs/README.md: reconstruído Sprint 16.9 | Deep reconciliation (test counts, LocalDB/Playwright strategy) owed to 31.9. |
| DOC-124 | `docs/testing/README.md` | Testing docs | CURRENT | docs/README.md: reconstruído Sprint 16.9 | Deep reconciliation (test counts, LocalDB/Playwright strategy) owed to 31.9. |
| DOC-125 | `docs/ux/01-guidelines.md` | UX docs | CURRENT | docs/README.md: reconstruído Sprint 16.8 | Deep reconciliation owed to 31.8. |
| DOC-126 | `docs/ux/02-accessibility.md` | UX docs | CURRENT | docs/README.md: reconstruído Sprint 16.8 | Deep reconciliation owed to 31.8. |
| DOC-127 | `docs/ux/03-responsive.md` | UX docs | CURRENT | docs/README.md: reconstruído Sprint 16.8 | Deep reconciliation owed to 31.8. |
| DOC-128 | `docs/ux/README.md` | UX docs | CURRENT | docs/README.md: reconstruído Sprint 16.8 | Deep reconciliation owed to 31.8. |
| DOC-129 | `docs/web/01-composition-root.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-130 | `docs/web/02-routing-and-pages.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-131 | `docs/web/03-layouts.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-132 | `docs/web/04-feature-components.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-133 | `docs/web/05-design-system-integration.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-134 | `docs/web/06-testing.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-135 | `docs/web/07-localization.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-136 | `docs/web/README.md` | Web docs | CURRENT | docs/README.md: reconstruído Sprint 16.7; 07-localization adicionado Sprint 23.9 | Deep reconciliation owed to 31.8. |
| DOC-137 | `scripts/Clear-BeeDayBackups.ps1` | Operational script | NOT REVIEWED | File exists in scripts/ | Header comments (if any) function as durable documentation; owed to 31.12. |
| DOC-138 | `scripts/Clear-BeeDayStdoutLogs.ps1` | Operational script | NOT REVIEWED | File exists in scripts/ | Header comments (if any) function as durable documentation; owed to 31.12. |
| DOC-139 | `scripts/Deploy-BeeDay.ps1` | Operational script | NOT REVIEWED | File exists in scripts/ | Header comments (if any) function as durable documentation; owed to 31.12. |
| DOC-140 | `scripts/New-IconSprite.ps1` | Operational script | NOT REVIEWED | File exists in scripts/ | Header comments (if any) function as durable documentation; owed to 31.12. |
| DOC-141 | `scripts/Reset-TestData.ps1` | Operational script | NOT REVIEWED | File exists in scripts/ | Header comments (if any) function as durable documentation; owed to 31.12. |
| DOC-142 | `scripts/iis-control/Invoke-BeeDayIisControl.ps1` | Operational script (IIS control) | NOT REVIEWED | File exists in scripts/iis-control/ | Header comments (if any) function as durable documentation; cross-check against docs/deployment/05-privileged-iis-control.md owed to 31.12. |
| DOC-143 | `scripts/iis-control/Invoke-BeeDayIisControlUpdater.ps1` | Operational script (IIS control) | NOT REVIEWED | File exists in scripts/iis-control/ | Header comments (if any) function as durable documentation; cross-check against docs/deployment/05-privileged-iis-control.md owed to 31.12. |
| DOC-144 | `scripts/iis-control/Provision-BeeDayHmgIisControl.ps1` | Operational script (IIS control) | NOT REVIEWED | File exists in scripts/iis-control/ | Header comments (if any) function as durable documentation; cross-check against docs/deployment/05-privileged-iis-control.md owed to 31.12. |
| DOC-145 | `scripts/iis-control/Provision-BeeDayHmgIisControlUpdater.ps1` | Operational script (IIS control) | NOT REVIEWED | File exists in scripts/iis-control/ | Header comments (if any) function as durable documentation; cross-check against docs/deployment/05-privileged-iis-control.md owed to 31.12. |
| DOC-146 | `scripts/iis-control/Request-BeeDayIisControlPromotion.ps1` | Operational script (IIS control) | NOT REVIEWED | File exists in scripts/iis-control/ | Header comments (if any) function as durable documentation; cross-check against docs/deployment/05-privileged-iis-control.md owed to 31.12. |
| DOC-147 | `scripts/tests/Test-ClearBeeDayBackups.ps1` | Operational script test | NOT REVIEWED | File exists in scripts/tests/ | Header comments (if any) function as durable documentation per Sprint scope §5; not yet extracted. Owed to 31.12. |
| DOC-148 | `scripts/tests/Test-ClearBeeDayStdoutLogs.ps1` | Operational script test | NOT REVIEWED | File exists in scripts/tests/ | Header comments (if any) function as durable documentation per Sprint scope §5; not yet extracted. Owed to 31.12. |
| DOC-149 | `scripts/tests/Test-DeployBeeDayRecovery.ps1` | Operational script test | NOT REVIEWED | File exists in scripts/tests/ | Header comments (if any) function as durable documentation per Sprint scope §5; not yet extracted. Owed to 31.12. |
| DOC-150 | `scripts/tests/Test-InvokeBeeDayIisControlContract.ps1` | Operational script test | NOT REVIEWED | File exists in scripts/tests/ | Header comments (if any) function as durable documentation per Sprint scope §5; not yet extracted. Owed to 31.12. |
| DOC-151 | `CLAUDE.md` | AI/project governance (root, untracked by design) | NOT REVIEWED | commit 869b57e (2026-08-20): "stop tracking CLAUDE.md in the product repository" | Local-only per explicit owner policy; still readable on disk and used every session. Classification into `PROJECT_RULE`/`AGENT_RULE`/`DUPLICATED_RULE`/`OBSOLETE_RULE` is Sprint 31.3's required work. |
| DOC-152 | `AGENTS.md` | AI/project governance (root, untracked by design) | NOT REVIEWED | commit 869b57e: already untracked before this policy commit | Same disposition as `CLAUDE.md`. Content overlap between `AGENTS.md` and `CLAUDE.md` not yet diffed — candidate `DUPLICATED_RULE`, verify in 31.3. |
| DOC-153 | `.claude/README.md` | AI/project governance (Claude Code adapter, untracked) | NOT REVIEWED | Directory exists locally; untracked per commit 869b57e policy | Adapter-level readme; verify it documents only Claude-Code-specific execution behavior, not duplicated project rules, in 31.3. |
| DOC-154 | `.claude/settings.json` | AI/project governance (Claude Code adapter, untracked) | NOT REVIEWED | Directory exists locally | Tool configuration, not narrative documentation; included for completeness. |
| DOC-155 | `.claude/settings.local.json` | AI/project governance (Claude Code adapter, untracked) | NOT REVIEWED | Directory exists locally | Local-only tool configuration; verify no secret/machine-specific value is present (CLAUDE.md §10) in 31.3. |
| DOC-156 | `.claude/skills/beeday-architecture/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | One of 9 `.claude/skills/*` files; 31.3 must confirm each skill stays execution-behavior-only and does not restate `CLAUDE.md` governance verbatim. |
| DOC-157 | `.claude/skills/beeday-engineering/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-158 | `.claude/skills/beeday-epic-autonomy/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-159 | `.claude/skills/beeday-git-delivery/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-160 | `.claude/skills/beeday-infrastructure/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-161 | `.claude/skills/beeday-quality/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-162 | `.claude/skills/beeday-review/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-163 | `.claude/skills/beeday-sprint/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. |
| DOC-164 | `.claude/skills/beeday-ui-ux/SKILL.md` | AI/project governance (Claude Code skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-156 note. No equivalent file exists under `.agents/skills/` (see DOC-166..173) — cross-adapter drift, verify in 31.3. |
| DOC-165 | `.agents/README.md` | AI/project governance (generic agent adapter, untracked) | NOT REVIEWED | Directory exists locally | Adapter-level readme for non-Claude-Code agents; verify scope vs `.claude/README.md` in 31.3. |
| DOC-166 | `.agents/settings.local.json` | AI/project governance (generic agent adapter, untracked) | NOT REVIEWED | Directory exists locally | Local-only configuration; verify no secret/machine-specific value present in 31.3. |
| DOC-167 | `.agents/skills/beeday-architecture/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | One of 8 `.agents/skills/*` files (one fewer than `.claude/skills/`, see DOC-164). Verify content parity with the `.claude` counterpart in 31.3. |
| DOC-168 | `.agents/skills/beeday-engineering/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. |
| DOC-169 | `.agents/skills/beeday-epic-autonomy/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. |
| DOC-170 | `.agents/skills/beeday-git-delivery/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. |
| DOC-171 | `.agents/skills/beeday-infrastructure/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. |
| DOC-172 | `.agents/skills/beeday-quality/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. |
| DOC-173 | `.agents/skills/beeday-review/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. |
| DOC-174 | `.agents/skills/beeday-sprint/SKILL.md` | AI/project governance (generic agent skill, untracked) | NOT REVIEWED | Directory exists locally | See DOC-167 note. `.agents/skills/` has no `beeday-ui-ux` entry — confirmed missing, not just unlisted; route to 31.3 as a `DUPLICATED`/drifted subject between adapters. |
| DOC-175 | `.codex/README.md` | AI/project governance (Codex adapter, untracked) | NOT REVIEWED | Directory exists locally | Adapter-level readme; verify scope vs `.claude/README.md` and `.agents/README.md` in 31.3. |
| DOC-176 | `.codex/config.toml` | AI/project governance (Codex adapter, untracked) | NOT REVIEWED | Directory exists locally | Tool configuration, not narrative documentation. |
| DOC-177 | `.codex/rules/beeday.rules` | AI/project governance (Codex adapter, untracked) | NOT REVIEWED | Directory exists locally | Single consolidated rules file — structurally different from the per-skill layout of `.claude/skills/` and `.agents/skills/`. Verify in 31.3 whether the same 8-9 skill subjects are represented here in a different shape, or are missing. |
| DOC-178 | `.github/upgrades/scenarios/dotnet-version-upgrade/assessment.csv` | Tooling-generated scratch artifact (untracked) | UNKNOWN | Directory exists locally, dated 2026-08-20; not present in `git ls-files` | `.NET` upgrade-assistant scan output (target `net11.0`) unrelated to product documentation. Ownership and intended disposition (keep as local scratch / add to `.gitignore` / delete) is unclear — flag for owner clarification via 31.3, no action taken here. |
| DOC-179 | `.github/upgrades/scenarios/dotnet-version-upgrade/assessment.json` | Tooling-generated scratch artifact (untracked) | UNKNOWN | Same as DOC-178 | Same disposition as DOC-178. |
| DOC-180 | `.github/upgrades/scenarios/dotnet-version-upgrade/assessment.md` | Tooling-generated scratch artifact (untracked) | UNKNOWN | Same as DOC-178 | Same disposition as DOC-178. |
| DOC-181 | `.github/upgrades/scenarios/dotnet-version-upgrade/dependencies-health.json` | Tooling-generated scratch artifact (untracked) | UNKNOWN | Same as DOC-178 | Same disposition as DOC-178. |
| DOC-182 | `.github/upgrades/scenarios/dotnet-version-upgrade/scenario-instructions.md` | Tooling-generated scratch artifact (untracked) | UNKNOWN | Same as DOC-178 | Same disposition as DOC-178. |
| DOC-183 | `.github/upgrades/scenarios/dotnet-version-upgrade/scenario.json` | Tooling-generated scratch artifact (untracked) | UNKNOWN | Same as DOC-178 | Same disposition as DOC-178. |
| DOC-184 | `scripts/iis-control/Provision-BeeDayHmgIisControl.ps1.orig` | Operational script — stray artifact (untracked) | UNKNOWN | Not present in `git ls-files`; sibling `Provision-BeeDayHmgIisControl.ps1` (DOC tracked) exists without `.orig` | `.orig` extension strongly suggests a leftover backup/merge/patch artifact rather than an intentional file. Verify whether still needed and, if not, remove — route to 31.3 or 31.12. |

### Sprint-specific validation (Issue #229)

- **Cobertura do inventário vs. diretórios do repositório:** `git ls-files docs .github scripts
  README.md` retornou exatamente 150 arquivos; todos representados em `DOC-001`–`DOC-150`. Os 34
  artefatos locais-apenas foram enumerados via `find`/`ls` direto no disco e representados em
  `DOC-151`–`DOC-184`. Nenhum arquivo do escopo declarado (Issue #229, "Scope") ficou de fora.
- **Caminhos do Ledger resolvem para artefatos reais:** todo `path` da tabela veio diretamente de
  `git ls-files` (rastreados) ou de `find`/`ls` executados nesta sessão (não rastreados) — nenhum
  caminho foi inventado ou copiado de memória.
- **Busca por marcadores legados conhecidos:** `LevelUp`/`Level Up` (30 arquivos), terminologia de
  persistência JSON legada (8 arquivos) — ambas executadas via `Grep` sobre `docs/` nesta sessão,
  resultado registrado nos achados acima e nas notas de linha correspondentes do Ledger.

### Critérios de aceite (Issue #229)

- [x] Every in-scope documentation artifact is represented in the Documentation Ledger — 184/184.
- [x] Every Ledger entry has a stable ID and evidence source.
- [x] No artifact remains unclassified as to its current state — todo registro tem um valor válido
      do vocabulário `Working states`.
- [x] Every known duplicate subject is recorded — `DOC-077`; drift entre `.claude/skills/` e
      `.agents/skills/` (`DOC-164`, `DOC-174`).
- [x] Every incomplete/reserved documentation area is recorded — `authentication/`, `developer/`,
      `api/` (parcial).
- [x] No document is deleted merely because it appears stale — nenhuma exclusão, movimentação ou
      reescrita foi realizada nesta Sprint.

### Sprint-specific boundary respeitado

Nenhuma reconstrução ampla, exclusão ou migração de arquitetura de informação foi realizada. Nenhum
arquivo foi movido, mesclado ou reescrito. Atribuição de `owner topic` e `final state` foi
deliberadamente deixada pendente para a Sprint 31.2, conforme Global Contract §3.

### Riscos residuais

- Os 45 documentos rastreados marcados `NOT REVIEWED` herdam o status de área declarado em
  `docs/README.md`, mas não foram reverificados individualmente linha a linha contra código/testes
  atuais nesta Sprint — essa verificação profunda é o objeto das Sprints 31.4–31.14.
- Os 6 artefatos de `.github/upgrades/` e o arquivo `.orig` órfão têm ownership/disposição
  desconhecidos; nenhuma ação de limpeza foi tomada — decisão de manter, ignorar ou remover cabe ao
  owner via Sprint 31.3.
- A paridade de conteúdo entre `.claude/skills/`, `.agents/skills/` e `.codex/rules/beeday.rules`
  não foi verificada linha a linha nesta Sprint — apenas a diferença estrutural/de contagem de
  arquivos foi confirmada.

### Validação executada

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx
dotnet test BeeDay.slnx
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa (nenhum código-fonte foi alterado nesta Sprint — apenas documentação sob `docs/` foi
criada).

### Deliverable

Documentation Ledger canônico (acima, 184 entradas) e este relatório de inventário da Sprint 31.1.

---
## Sprint 31.2 — Canonical Documentation Architecture & ADR Validity Baseline

**GitHub Issue:** [#230](https://github.com/tiagoarrigoni/BeeDay/issues/230)
**Branch:** `sprint/31.2-canonical-architecture-adr-baseline`
**Depende de:** #229 (concluída, ver seção acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Definir a arquitetura de informação final da documentação, o modelo de ownership canônico e o
baseline de validade de ADR, antes que Sprints especialistas reconciliem cada assunto.

### Correção de evidência da Sprint 31.1

Ao revisar o Ledger para atribuir ownership, uma inconsistência de contagem foi encontrada na
própria narrativa da Sprint 31.1 (não na tabela do Ledger, que está correta): o texto de "Cobertura"
dizia `.claude/` (11 arquivos), mas a contagem real (`awk -F'`' '{print $2}' sobre as 34 linhas
locais-apenas) é **12 arquivos** (`README.md`, `settings.json`, `settings.local.json` e 9 arquivos
`skills/*/SKILL.md`, incluindo `beeday-ui-ux`). O total geral de 184 entradas permanece correto — o
erro estava apenas na subdivisão narrativa (11+10+3+2=26 vs. o correto 12+10+3+2=27, artefatos
locais-apenas). Corrigido nesta Sprint (ver diff em `docs/epics/31-documentation-knowledge-
consolidation/README.md`, seção "Cobertura" da Sprint 31.1) — outcome: `CORRECTED`, per Global
Contract §2.

### 1. Ownership canônico e Sprint responsável (Required work #1–#3)

Usando o Ledger da Sprint 31.1 (184 entradas, `DOC-001`–`DOC-184`) como inventário completo de
planejamento, toda entrada recebe nesta Sprint um `Owner topic`, uma `Owning Sprint` e um `Intended
final state` — nenhuma entrada permanece sem dono após esta Sprint, conforme Global Contract §3.

A atribuição segue diretamente o escopo já aprovado de cada Sprint especialista na Issue #228
("Sprint Roadmap") — nenhum escopo novo foi inventado; onde um assunto atravessa duas Sprints
declaradas (ex.: `docs/deployment/*` contém tanto conteúdo de IIS/HMG/operações quanto de pipeline
CI/CD), o arquivo foi atribuído à Sprint cujo escopo declarado corresponde ao conteúdo real do
arquivo, mesmo vivendo fisicamente na mesma pasta — nenhuma movimentação de arquivo é implicada por
essa atribuição de ownership; mover fisicamente arquivos entre pastas, se necessário, é decisão da
Sprint especialista, não desta.

Colunas: `ID` (referencia a mesma linha do Ledger da Sprint 31.1) · `Owner topic` · `Owning Sprint` ·
`Intended final state` · `Rationale`.

| ID | Owner topic | Owning Sprint | Intended final state | Rationale |
|---|---|---|---|---|
| DOC-001 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-002 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-003 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-004 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-005 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-006 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-007 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-008 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-009 | CI/CD & GitHub Engineering | 31.11 | CURRENT | In-scope evidence source for 31.11 ("Inspect actual workflow files and names"). |
| DOC-010 | Repository Entry Points | 31.14 | CURRENT | 31.14 owns root README as "concise entry point"; must correct the CLAUDE.md tracking-status line found in 31.1. |
| DOC-011 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: remains the normative convention; no evidenced change required. |
| DOC-012 | Documentation Governance | 31.14 | CURRENT | 31.14 rebuilds this as "the accurate index of the final documentation architecture"; must also correct the stale EPIC 30 status row found in 31.1. |
| DOC-013 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-014 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-015 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-016 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-017 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-018 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-019 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-020 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-021 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-022 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-023 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-024 | Documentation Governance | 31.2 (this Sprint) | CURRENT | Verified this Sprint: all 12 templates have exactly one H1 (docs/CONVENTIONS.md §4); no structural defect found. |
| DOC-025 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-026 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-027 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-028 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-029 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-030 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-031 | Architecture Decision Records | 31.13 | HISTORICAL | Validity classified this Sprint (see ADR Validity Baseline table); 31.13 applies supersession metadata/links without rewriting ADR bodies. |
| DOC-032 | API Contract | 31.14 | CURRENT | Reserved/incomplete-area disposition folded into 31.14's residual-documentation closure. |
| DOC-033 | API Contract | 31.14 | CURRENT | Reserved/incomplete-area disposition folded into 31.14's residual-documentation closure. |
| DOC-034 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-035 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-036 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-037 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-038 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-039 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-040 | Application / CQRS | 31.6 | CURRENT | Matches Sprint 31.6 declared scope (commands/queries/handlers/contracts). |
| DOC-041 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-042 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-043 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-044 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-045 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-046 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-047 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-048 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-049 | Architecture | 31.4 | CURRENT | Matches Sprint 31.4 declared scope (solution structure, dependency direction, composition root). |
| DOC-050 | Authentication & Identity | 31.10 | CURRENT | 31.10 must give this reserved area an explicit final disposition: dedicated content, or formal fold into docs/security/ — either outcome lands as CURRENT, not left reserved. |
| DOC-051 | Brand System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope (Brand System ownership alongside Web/Design System/UX). |
| DOC-052 | Brand System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope (Brand System ownership alongside Web/Design System/UX). |
| DOC-053 | Brand System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope (Brand System ownership alongside Web/Design System/UX). |
| DOC-054 | Brand System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope (Brand System ownership alongside Web/Design System/UX). |
| DOC-055 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-056 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-057 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-058 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-059 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-060 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-061 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-062 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-063 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-064 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-065 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-066 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-067 | CI/CD & GitHub Engineering | 31.11 | CURRENT | CI/CD-pipeline subset per Sprint 31.11 declared scope, despite living under docs/deployment/ physically — cross-reference only, no file move implied. |
| DOC-068 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-069 | Deployment & Operations | 31.12 | CURRENT | IIS/HMG/runtime/ops subset per Sprint 31.12 declared scope. |
| DOC-070 | Design System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-071 | Design System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-072 | Design System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-073 | Design System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-074 | Design System | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-075 | Developer Guide | 31.14 | CURRENT | 31.14 must give this reserved area an explicit final disposition (own content, or formal pointer to root README) — folded into residual-documentation closure. |
| DOC-076 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-077 | Domain Model | 31.5 | SUPERSEDED | Candidate merge/supersession into docs/epics/30-system-integrity/README.md (Sprint 30.5 section) — final decision (merge vs. formal supersession note) made in 31.5, not assumed here. |
| DOC-078 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-079 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-080 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-081 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-082 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-083 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-084 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-085 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-086 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-087 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-088 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-089 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-090 | Domain Model | 31.5 | CURRENT | Matches Sprint 31.5 declared scope. |
| DOC-091 | Epic History | 31.13 | HISTORICAL | Already self-declared closed/historical in docs/README.md; 31.13 confirms no current-state doc silently depends on it as live evidence. |
| DOC-092 | Epic History | 31.13 | HISTORICAL | Already self-declared closed/historical in docs/README.md; 31.13 confirms no current-state doc silently depends on it as live evidence. |
| DOC-093 | Epic History | 31.13 | HISTORICAL | Already self-declared closed/historical in docs/README.md; 31.13 confirms no current-state doc silently depends on it as live evidence. |
| DOC-094 | Epic History | 31.13 | HISTORICAL | Already self-declared closed/historical in docs/README.md; 31.13 confirms no current-state doc silently depends on it as live evidence. |
| DOC-095 | Epic History | 31.13 | HISTORICAL | Already self-declared closed/historical in docs/README.md; 31.13 confirms no current-state doc silently depends on it as live evidence. |
| DOC-096 | Epic History | 31.11 (status verify), then 31.13 (reclassify) | HISTORICAL | Status "POST-MERGE HMG VALIDATION PENDING" predates EPIC 30's CI/CD/HMG audit — 31.11 confirms current accuracy before 31.13 folds it in alongside the other closed Epics. |
| DOC-097 | Epic History | 31.13/31.14 | HISTORICAL | EPIC 30 is formally COMPLETE; 31.13/31.14 correct the stale docs/README.md "Em andamento" row and reclassify alongside sibling closed Epics. |
| DOC-098 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-099 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-100 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-101 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-102 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-103 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-104 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-105 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-106 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-107 | Historical Documentation | 31.13 | HISTORICAL | Already frozen per docs/CONVENTIONS.md §13; 31.13 confirms nothing current-state relies on it silently. |
| DOC-108 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-109 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-110 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-111 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-112 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-113 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-114 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-115 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-116 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-117 | Infrastructure & Persistence | 31.7 | CURRENT | Matches Sprint 31.7 declared scope. |
| DOC-118 | Security | 31.10 | CURRENT | Matches Sprint 31.10 declared scope; canonical boundary vs. docs/authentication/ resolved there. |
| DOC-119 | Security | 31.10 | CURRENT | Matches Sprint 31.10 declared scope; canonical boundary vs. docs/authentication/ resolved there. |
| DOC-120 | Security | 31.10 | CURRENT | Matches Sprint 31.10 declared scope; canonical boundary vs. docs/authentication/ resolved there. |
| DOC-121 | Testing & Quality Engineering | 31.9 | CURRENT | Matches Sprint 31.9 declared scope. |
| DOC-122 | Testing & Quality Engineering | 31.9 | CURRENT | Matches Sprint 31.9 declared scope. |
| DOC-123 | Testing & Quality Engineering | 31.9 | CURRENT | Matches Sprint 31.9 declared scope. |
| DOC-124 | Testing & Quality Engineering | 31.9 | CURRENT | Matches Sprint 31.9 declared scope. |
| DOC-125 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-126 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-127 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-128 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-129 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-130 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-131 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-132 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-133 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-134 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-135 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-136 | Web / UX / Presentation | 31.8 | CURRENT | Matches Sprint 31.8 declared scope. |
| DOC-137 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-138 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-139 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-140 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-141 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-142 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-143 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-144 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-145 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-146 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-147 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-148 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-149 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-150 | Deployment & Operations | 31.12 | CURRENT | Operational scripts cross-referenced by Sprint 31.12's IIS/deployment runbook reconciliation. |
| DOC-151 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-152 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-153 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-154 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-155 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-156 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-157 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-158 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-159 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-160 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-161 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-162 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-163 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-164 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-165 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-166 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-167 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-168 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-169 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-170 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-171 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-172 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-173 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-174 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-175 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-176 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-177 | AI/Project Governance | 31.3 | CURRENT | Classification into PROJECT_RULE/AGENT_RULE/DUPLICATED_RULE/OBSOLETE_RULE is 31.3's required work; disposition target is "kept and reconciled", not removal, absent owner approval. |
| DOC-178 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked .NET-upgrade-assistant scratch output, no product-documentation value; deletion requires owner confirmation in 31.3, not assumed here. |
| DOC-179 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked .NET-upgrade-assistant scratch output, no product-documentation value; deletion requires owner confirmation in 31.3, not assumed here. |
| DOC-180 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked .NET-upgrade-assistant scratch output, no product-documentation value; deletion requires owner confirmation in 31.3, not assumed here. |
| DOC-181 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked .NET-upgrade-assistant scratch output, no product-documentation value; deletion requires owner confirmation in 31.3, not assumed here. |
| DOC-182 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked .NET-upgrade-assistant scratch output, no product-documentation value; deletion requires owner confirmation in 31.3, not assumed here. |
| DOC-183 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked .NET-upgrade-assistant scratch output, no product-documentation value; deletion requires owner confirmation in 31.3, not assumed here. |
| DOC-184 | Repository Hygiene / Tooling Artifacts | 31.3 | REMOVED (candidate) | Untracked stray backup/merge artifact with no tracked counterpart pattern; deletion requires owner confirmation in 31.3, not assumed here. |

### 2. ADR Validity Baseline (Required work #6–#8)

Cada ADR foi lido integralmente nesta Sprint e comparado contra evidência atual (root `README.md`,
`docs/architecture/02-solution-structure.md`, `src/` real via `ls`, e o texto já autodeclarado em
`docs/adr/README.md` e nos próprios ADRs). Nenhum corpo de ADR foi reescrito — apenas classificado.

| ADR | Decisão | Classificação | Evidência / racional |
|---|---|---|---|
| [ADR-001](../../adr/ADR-001-contract-first.md) — Contract-First | Criar `LevelUp.Contracts` como projeto independente para todas as fronteiras. | **`CONFLICTING`** | `src/` real (via `ls`) tem exatamente 4 projetos de produção — `BeeDay.Domain`, `BeeDay.Application`, `BeeDay.Infrastructure`, `BeeDay.Web` — nenhum projeto `Contracts` separado jamais existiu ou existe hoje; `docs/architecture/02-solution-structure.md` confirma a mesma estrutura de 4 projetos. O objetivo arquitetural (Application depende de abstrações, nunca de Infrastructure diretamente) **foi** alcançado, mas por um meio diferente do decidido (interfaces dentro do próprio `BeeDay.Application`, não um projeto `Contracts` externo) — já reconhecido honestamente em `docs/adr/README.md` linha 21 ("adoção parcial em código — nenhum projeto Contracts separado foi criado"). Nenhum ADR posterior supera formalmente esta divergência. Candidata a `NEW ADR REQUIRED` (documentar formalmente a forma final adotada) ou a uma nota de supersessão — decisão final não tomada aqui, apenas classificada; roteada para 31.4 (Arquitetura) e 31.13 (aplicação da nota de supersessão). |
| [ADR-002](../../adr/ADR-002-greenfield-database.md) — Banco novo sem importar JSON | Não implementar importação/dual-write entre JSON e SQL Server. | **`VALID`** | Root `README.md`: "SQL Server is the only persistence provider... no JSON storage, no legacy import, and no compatibility layer — the database starts empty for every new environment." Coincide exatamente com a decisão. |
| [ADR-003](../../adr/ADR-003-aggregate-repositories.md) — Repositórios por Aggregate + read services | Repositórios orientados a Aggregate em vez de repositório genérico. | **`VALID`** | O próprio cabeçalho do ADR já foi atualizado (não reescrito — nota de status) para "totalmente implementado e adotado desde a Sprint 14.6", listando exatamente os 8 contratos + `IUnitOfWork` que o root `README.md` confirma hoje (`IUserRepository`, `IUserTokenRepository`, `IHabitRepository`, `IRecurringTaskRepository`, `IProjectRepository`, `IWalletRepository`, `ITransactionRepository`, `IWalletTagRepository`, `IUnitOfWork`). |
| [ADR-004](../../adr/ADR-004-sql-server-runtime-cutover.md) — SQL Server como único provider de runtime | Migrar todo handler de produção de JSON para os 8 contratos + `IUnitOfWork`, em corte único. | **`VALID`** (com nota de supersessão parcial já registrada no próprio ADR) | O corte de runtime em si permanece válido e é a realidade atual (root `README.md`). O próprio ADR já documenta, em seu cabeçalho, que um parágrafo específico (manter `LevelUpData`/pipeline JSON como código legado não registrado) foi revertido e formalmente superado por ADR-005 — exemplo correto de nota de supersessão sem reescrita de corpo, conforme `docs/CONVENTIONS.md` §13. |
| [ADR-005](../../adr/ADR-005-json-legacy-removal.md) — Remoção do pipeline JSON legado e `LevelUpData` | Remover fisicamente o código JSON legado e `LevelUpData`, não apenas desregistrar. | **`VALID`** | Root `README.md` confirma: "There is no JSON storage... no compatibility layer." Nenhuma referência a `JsonLevelUpDocumentStore`/`LevelUpData` em código de produção atual (fora do escopo desta Sprint verificar arquivo-por-arquivo, mas a ausência de qualquer menção nos documentos de arquitetura/persistência atuais — `CURRENT` no Ledger — corrobora). |
| [ADR-006](../../adr/ADR-006-transactional-email-localization-boundary.md) — Culture de e-mail transacional via `User.Language` | Contrato `IIdentityEmailComposer` recebe `UserLanguage`; catálogo `.resx` estreito Infrastructure-owned; sem `IStringLocalizer`/estado global. | **`VALID`** | `docs/infrastructure/06-transactional-email.md` está marcado `CURRENT` no Ledger (Sprint 16.6, estendido EPIC 26/28); a decisão é recente (EPIC 28, Sprint 28.2, 2026-08-17) e nenhuma evidência posterior a contradiz. |

Nenhum ADR foi classificado `SUPERSEDED`, `OBSOLETE` ou `NEW ADR REQUIRED` de forma definitiva nesta
Sprint isoladamente — `ADR-001` é a única exceção material, marcada `CONFLICTING` com uma
recomendação (não uma decisão) de `NEW ADR REQUIRED` a ser confirmada em 31.4/31.13. Isso cumpre o
Required work #8: nenhuma Sprint futura poderá depender de um ADR cuja validade atual seja
desconhecida — todos os 6 têm uma classificação explícita e evidenciada.

### 3. Responsabilidade: root `README.md` vs. `docs/README.md` (Required work #5)

Formalizando o que a estrutura atual já pratica implicitamente (nenhuma mudança de conteúdo feita
nesta Sprint — apenas a regra é tornada explícita para orientar a Sprint 31.14):

- **Root `README.md`** é o ponto de entrada conciso para qualquer colaborador ou ferramenta externa
  (GitHub, IDEs, humanos clonando o repositório pela primeira vez): o que o produto é, stack,
  capacidades atuais em alto nível, como rodar localmente, quality gate, estratégia de branch, e um
  único link de saída para `docs/README.md`. Não deve conter detalhe de nível de implementação que já
  vive em `docs/<área>/`.
- **`docs/README.md`** é o índice completo e navegável de toda a documentação técnica sob `docs/`:
  taxonomia por área, status por área, ordem de leitura recomendada, e a ponte explícita para o
  `beeday Experience System` público.
- Nenhum documento deve duplicar por cópia manual o que o outro já cobre — cada um linka para o
  outro exatamente uma vez no ponto de entrada/saída natural (root `README.md` linka para
  `docs/README.md` na seção "Documentation"; `docs/README.md` linka de volta para o root `README.md`
  como item 1 da "Ordem de leitura recomendada").

Esta responsabilidade já está satisfeita pela estrutura atual dos dois arquivos — nenhuma
reestruturação foi necessária. A única divergência encontrada (root `README.md` listando `CLAUDE.md`
como rastreado, achado da Sprint 31.1) é um erro factual pontual, não uma violação da separação de
responsabilidades, e permanece roteada para 31.3/31.14.

### 4. Hierarquia final de `docs/` (Required work #4)

Nenhuma mudança estrutural de pastas é justificada por evidência nesta Sprint. A taxonomia atual
(uma pasta por área, espelhando a arquitetura real do sistema, fixada na Sprint 16.2) permanece
canônica. A única exceção concreta encontrada é `docs/domain/audit-inventory.md`, cuja disposição
final (merge em `docs/epics/30-system-integrity/README.md` vs. nota de supersessão formal) é
decidida em 31.5 — não é uma mudança de hierarquia, é a resolução de um duplicado dentro da mesma
pasta.

### Sprint-specific validation (Issue #230)

- **Colisão de caminhos canônicos planejados:** nenhuma — nenhum novo caminho/pasta foi proposto
  nesta Sprint (seção 4 acima).
- **Referências e alvos de supersessão de ADR existem:** `ADR-004` → `ADR-005` (referência cruzada
  já existente em ambos os arquivos, verificada nesta Sprint); `ADR-001` candidata a uma futura nota
  de supersessão/novo ADR — alvo ainda não existe, registrado como pendência, não como link quebrado.
- **Revisão de duplicação entre ownership planejado:** nenhum assunto recebeu dois Owner topics
  conflitantes nesta atribuição (verificado por inspeção da tabela de 184 linhas — cada `DOC-ID`
  aparece exatamente uma vez).

### Critérios de aceite (Issue #230)

- [x] Every maintained subject has exactly one canonical owner — 184/184 entradas com `Owner topic`
      único.
- [x] Every Ledger item has an owning Sprint or justified terminal disposition — 184/184.
- [x] No ADR remains with unknown current validity — 6/6 classificados.
- [x] Root README and docs/README.md responsibilities are explicitly separated — seção 3 acima.
- [x] Proposed hierarchy contains no unnecessary folder or duplicate topic ownership — nenhuma
      hierarquia nova proposta; duplicado único conhecido (`audit-inventory.md`) já roteado.
- [x] Later Sprints can identify which ADRs are valid current evidence — tabela da seção 2.

### Sprint-specific boundary respeitado

Nenhuma reescrita especialista de documentação (Domain, Application, Infrastructure, etc.) foi
realizada nesta Sprint — apenas o modelo de ownership e o baseline de validade de ADR, conforme
`CLAUDE.md`. Nenhum corpo de ADR foi alterado.

### Riscos residuais

- A classificação `CONFLICTING` de ADR-001 é uma constatação factual (o projeto `Contracts` nunca
  existiu); a decisão sobre se isso exige um novo ADR formal, uma nota de supersessão, ou nenhuma
  ação além de já estar documentado em `docs/adr/README.md`, cabe à Sprint 31.4/31.13 ou ao owner.
- `docs/deployment/*` teve seu ownership dividido entre 31.11 (CI/CD) e 31.12 (IIS/HMG/operações)
  sem mover nenhum arquivo fisicamente — se uma Sprint especialista decidir que a divisão física por
  pasta é necessária, isso é uma decisão de arquitetura de informação nova, fora do escopo desta
  Sprint.

### Validação executada

Sprint estritamente documental (nenhum código-fonte, projeto, configuração de runtime, script,
workflow, migration ou teste alterado) — aplicada a política de validação proporcional ao risco da
EPIC 31 aprovada pelo owner em 2026-08-21: suíte completa de testes **não** executada nesta Sprint.

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa. Validação de link/caminho: todo link relativo inserido nesta Sprint (`../../adr/ADR-
00N-*.md`) foi verificado apontando para arquivo existente.

### Deliverable

Modelo de ownership canônico (184 entradas), ADR Validity Baseline (6 ADRs classificados),
separação formal de responsabilidade root `README.md`/`docs/README.md`, e decisão de hierarquia de
`docs/` (nenhuma mudança estrutural justificada).

---
## Sprint 31.3 — AI Governance Extraction & Repository Cleanup

**GitHub Issue:** [#231](https://github.com/tiagoarrigoni/BeeDay/issues/231)
**Branch:** `sprint/31.3-ai-governance-extraction-repository-cleanup`
**Depende de:** #230 (concluída, ver seção acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Separar governança de projeto durável de configuração específica de agente, sem perder nenhuma
regra de repositório, e fechar achados de higiene já identificados nas Sprints 31.1/31.2 sobre os
próprios artefatos de governança de IA.

### Validação aplicada nesta Sprint (política aprovada pelo owner)

Sprint estritamente documental/de configuração local — nenhum artefato executável de produção foi
alterado (todos os arquivos tocados são `.md` rastreados ou arquivos locais-apenas fora do Git).
Suíte completa de testes **não** executada, por política aprovada em 2026-08-21.

### 1. Método

Lidos integralmente nesta sessão: `CLAUDE.md` (já presente no contexto do sistema), `AGENTS.md`,
`.claude/README.md`, `.claude/settings.json`, `.claude/settings.local.json`,
`.claude/skills/beeday-quality/SKILL.md` e `beeday-ui-ux/SKILL.md` (amostra representativa),
`.agents/README.md`, `.agents/settings.local.json`, `.codex/README.md`, `.codex/config.toml`,
`.codex/rules/beeday.rules`. Comparados contra `docs/` rastreado via `Grep` para achar duplicação
real (não presumida).

### 2. Classificação de instruções (Required work #1)

| Artefato | Classificação predominante | Evidência / racional |
|---|---|---|
| `CLAUDE.md` §1 (Missão, 9 prioridades) | **`PROJECT_RULE`** — sem lar canônico rastreado até esta Sprint | Busca (`Grep`) por essa ordenação específica em `docs/` não encontrou nenhuma ocorrência antes desta Sprint. **Migrado nesta Sprint** para o `README.md` raiz, nova seção "Engineering priorities" (rastreada). Outcome: `CORRECTED`. |
| `CLAUDE.md` §2 (ordem de leitura, hierarquia de autoridade) | `AGENT_RULE` | Procedimento de como abordar uma tarefa — não um fato sobre o sistema; não precisa de cópia rastreada. |
| `CLAUDE.md` §3 (comportamento de engenharia: reuse-first, sem placeholder) | `AGENT_RULE` | Disciplina de processo, não um fato verificável sobre `src/`. |
| `CLAUDE.md` §4 (Contrato de arquitetura: Domain/Application/Infrastructure/Web) | `DUPLICATED_RULE` | Lar canônico rastreado já existe: `docs/architecture/03-clean-architecture.md`, `04-dependency-rules.md` (ambos `CURRENT` no Ledger). |
| `CLAUDE.md` §5 (beeday Experience System e marca: `beeday` minúsculo, `#5247F9`, marca≠identidade técnica) | `DUPLICATED_RULE` | `docs/brand/03-color-palette.md` já documenta `#5247F9`; `docs/brand/README.md` e `CLAUDE.md` do próprio repositório (linha "Unless a specific technical rename is explicitly approved") já expressam a regra marca≠identidade técnica em ambos os lugares. |
| `CLAUDE.md` §6 (roteamento de Skills) | `AGENT_RULE` | Específico de como um agente de IA seleciona procedimento — sem equivalente humano necessário. |
| `CLAUDE.md` §7–8 (Autorização: Level 0-3, Classes A-E) | `AGENT_RULE` | Modelo de autonomia de agente de IA — não uma regra de negócio ou arquitetura do produto. |
| `CLAUDE.md` §9 (Git workflow: `hmg`→`main`→`prd`, branches protegidas) | `DUPLICATED_RULE` (fato central) + `AGENT_RULE` (procedimento) | O fato central (fluxo de branches, `main`/`prd` protegidas) já está em `README.md` raiz, seção "Branch strategy" — `DUPLICATED_RULE`, correto. O detalhe procedural (Class C/D, quando comitar) é `AGENT_RULE`. |
| `CLAUDE.md` §10 (Segurança e segredos) | `AGENT_RULE`/prática geral | Nenhuma regra de segredo específica do produto (não é uma política de segurança do produto beeday, é uma disciplina universal de qualquer engenharia) — nenhuma duplicação necessária. |
| `CLAUDE.md` §11–12 (Disciplina de mudança, contrato de documentação) | `AGENT_RULE` | Refletido em espírito por `docs/CONVENTIONS.md`, mas como processo de IA, não como fato do produto. |
| `CLAUDE.md` §13 (Testes e validação obrigatória: comandos `dotnet format/build/test`) | `DUPLICATED_RULE` | `README.md` raiz, seção "Quality gate", tem exatamente os mesmos comandos. |
| `CLAUDE.md` §13.1 (classificação de falha: `CHANGE-CAUSED` etc.) | `DUPLICATED_RULE` | `docs/testing/01-testing-strategy.md` já usa e explica `CHANGE-CAUSED` no contexto real de flakiness conhecida. |
| `CLAUDE.md` §14 (revisão em duas passadas) | `AGENT_RULE` | Processo de revisão de IA — nenhum documento de produto rastreado precisa disso. |
| `CLAUDE.md` §15 (modelo de qualidade: `PASS`/`BLOCKER`/`MAJOR`/`MINOR`) | `AGENT_RULE` | Vocabulário de relatório de agente — `BLOCKER`/`MAJOR`/`MINOR` em `docs/design-system/02-components.md` e `docs/deployment/11-release-quality-gate.md` são usos textualmente coincidentes, não o mesmo modelo formal; não constitui duplicação real. |
| `CLAUDE.md` §16 (Repository State vs Environment State) | `AGENT_RULE`/conceitual | Modelo mental para relatório de agente; não é uma regra de negócio do produto. |
| `CLAUDE.md` §17 (condições de parada autônoma) | `AGENT_RULE` | Específico de execução autônoma de IA. |
| `CLAUDE.md` §18 (Definition of Done) | `AGENT_RULE` | Checklist de conclusão de tarefa de IA. |
| `CLAUDE.md` §19 (relatório de fim de tarefa) | `AGENT_RULE` | Formato de relatório de IA. |
| `CLAUDE.md` §20 (princípios operacionais) | `AGENT_RULE` | Heurísticas de decisão de IA. |
| `AGENTS.md` (documento inteiro, 21 seções paralelas) | `DUPLICATED_RULE` — **intencional e necessária** | `AGENTS.md` é o adaptador equivalente de `CLAUDE.md` para OpenAI Codex — cada ferramenta de IA só carrega automaticamente seu próprio arquivo (`CLAUDE.md` para Claude Code, `AGENTS.md` para Codex). `AGENTS.md` já reconhece isso explicitamente em sua própria Seção 2.1, item 2: "`CLAUDE.md` when it exists, because it contains shared BeeDay engineering governance". Reduzir `AGENTS.md` a um ponteiro fino para `CLAUDE.md` regrediria a autossuficiência de governança do Codex caso a leitura cruzada falhe — **não recomendado**, mantido como duplicação intencional. |
| `.claude/skills/*` (9 arquivos), `.agents/skills/*` (8→9 arquivos após esta Sprint) | `AGENT_RULE` | Procedimentos reutilizáveis de execução, por definição — nenhum contém fato de produto não já coberto acima. |
| `.claude/README.md`, `.agents/README.md`, `.codex/README.md` | `AGENT_RULE` | Documentação do próprio adaptador (por que Skills existem, filosofia de permissão) — não fatos do produto. |
| `.claude/settings.json`, `.claude/settings.local.json`, `.agents/settings.local.json` | `AGENT_RULE` | Configuração técnica de ferramenta (allowlist de comandos, variáveis de ambiente). Verificado nesta Sprint: nenhum segredo, token, senha ou credencial presente em nenhum dos três arquivos — apenas padrões de permissão de comando e caminhos temporários locais da máquina (esperado, ver `.claude/README.md`: "`settings.local.json` — local session behavior only"). |
| `.codex/config.toml`, `.codex/rules/beeday.rules` | `AGENT_RULE` | Postura de sandbox/aprovação e portões de escalonamento por comando — mecanismo técnico do Codex, sem fato de produto. |

Nenhum `OBSOLETE_RULE` foi encontrado — nenhuma instrução morta ou contraditória com o estado atual
do repositório foi identificada em nenhum dos artefatos de governança de IA.

### 3. Ação tomada: migração da regra genuína sem lar rastreado (Required work #2)

Único `PROJECT_RULE` sem cópia rastreada encontrado: a ordem de prioridades de engenharia
(Correctness → Repository integrity → Architectural integrity → Security → Backward compatibility
→ Experience/Design System consistency → Maintainability → Minimal scope → Delivery efficiency).

Adicionada nesta Sprint como nova seção **"Engineering priorities"** no `README.md` raiz (arquivo
rastreado), imediatamente após a introdução do produto — consistente com a responsabilidade de
`README.md` como ponto de entrada conciso, formalizada na Sprint 31.2. A seção linka de volta ao
fato de que o contrato de governança completo permanece local (não expõe conteúdo de IA no
repositório público, respeitando a política do commit `869b57e`).

Também corrigida, na mesma edição, a árvore "Repository structure" do `README.md` raiz — removida
a linha `├── CLAUDE.md`, que listava um arquivo não rastreado como se fizesse parte da árvore real
do repositório (achado `DOC-010`/`PARTIALLY_CURRENT`, Sprint 31.1). Outcome: `CORRECTED`.

### 4. Ação tomada: paridade entre adaptadores de agente (achado da Sprint 31.1, `DOC-164`/`DOC-174`)

Confirmado por `diff` byte-a-byte (ignorando final de linha) que `.agents/skills/*` é cópia
integral de `.claude/skills/*` (mesmo conteúdo, apenas CRLF em vez de LF) — os dois adaptadores
devem permanecer sincronizados por design. `.agents/skills/beeday-ui-ux/SKILL.md` estava
genuinamente ausente (8 de 9 arquivos espelhados). Criado nesta Sprint como cópia byte-idêntica
(CRLF, consistente com os demais arquivos de `.agents/skills/`) de
`.claude/skills/beeday-ui-ux/SKILL.md`. Arquivo local-apenas — não rastreado pelo Git, portanto
sem impacto no diff desta Sprint nem necessidade de aprovação de remoção/untracking. Outcome:
`CREATED`.

`.codex/rules/beeday.rules` foi verificado e é um arquivo de portões de escalonamento de comando
(technical gate), não um espelho de `.agents/skills/` — não tem lacuna equivalente a preencher;
`.codex/README.md` já documenta corretamente que `.agents/skills/` (não `.codex/`) é onde os
procedimentos do Codex vivem.

### 5. Artefatos rastreados a remover/untrackear (Required work #5)

Nenhum. `CLAUDE.md`, `AGENTS.md`, `.claude/`, `.agents/`, `.codex/` já estão totalmente
untracked desde o commit `869b57e` (2026-08-20) — não existe nenhum artefato de governança de IA
atualmente rastreado pelo Git para remover. A cláusula "Owner decision required" da Issue #231 não
se aplica nesta Sprint por ausência de artefato-alvo.

### 6. Achados não resolvidos, mantidos como estão (por decisão explícita de não agir sem confirmação do owner)

- `.github/upgrades/scenarios/dotnet-version-upgrade/*` (6 arquivos, `DOC-178`–`183`) e
  `scripts/iis-control/Provision-BeeDayHmgIisControl.ps1.orig` (`DOC-184`) permanecem no disco,
  não rastreados, classificados `REMOVED (candidate)` desde a Sprint 31.2. Nenhuma exclusão foi
  executada nesta Sprint: são arquivos fora do escopo desta tarefa cuja origem/necessidade futura
  não pode ser confirmada com certeza a partir de evidência de repositório — exclusão de arquivos
  de disco que não pertencem à tarefa atual, mesmo não rastreados, requer confirmação explícita do
  owner antes de ser executada, por prudência operacional (não uma regra formal do Global
  Contract, mas consistente com `CLAUDE.md` §9.1 "never silently discard... work"). Permanecem
  pendentes de decisão do owner.

### Documentation Ledger — atualização (Required work: manter Ledger sincronizado)

| ID | Path | Outcome desta Sprint |
|---|---|---|
| DOC-151–177 | `CLAUDE.md`, `AGENTS.md`, `.claude/*`, `.agents/*` (exceto o novo arquivo abaixo), `.codex/*` | Classificados (tabela seção 2); nenhum removido/untracked; nenhum segredo encontrado. |
| DOC-178–184 | `.github/upgrades/*`, `scripts/iis-control/*.orig` | Sem mudança — permanecem `REMOVED (candidate)`, pendente decisão explícita do owner (seção 6). |
| **DOC-185** (novo) | `.agents/skills/beeday-ui-ux/SKILL.md` | **`CREATED`** nesta Sprint — cópia byte-idêntica de `.claude/skills/beeday-ui-ux/SKILL.md`; local-apenas, não rastreado; categoria `AI/Project Governance`; Owner topic `AI/Project Governance`; Owning Sprint 31.3 (concluído); Final state `CURRENT`. |
| DOC-010 (`README.md` raiz) | — | Estado corrigido nesta Sprint: linha `CLAUDE.md` removida da árvore e seção "Engineering priorities" adicionada. Estado do Ledger atualizado de `PARTIALLY_CURRENT` para `CURRENT`. |

### Critérios de aceite (Issue #231)

- [x] No critical project rule exists only in an untracked/local agent file — o único item
      encontrado (ordem de prioridades) foi migrado para `README.md` raiz nesta Sprint.
- [x] Project rules and agent-specific behavior have clear ownership — tabela da seção 2.
- [x] No tracked governance artifact is removed without required owner approval — nenhum artefato
      rastreado existia para remover; nada foi removido.
- [x] Tooling dependencies are verified before file removal/untracking — não aplicável (nenhuma
      remoção/untracking de arquivo rastreado ocorreu); a criação de `.agents/skills/beeday-ui-
      ux/SKILL.md` foi verificada por `diff` contra o par existente antes da criação.
- [x] Duplicate governance text is reduced where safe — `AGENTS.md` mantido integralmente por ser
      duplicação intencional e necessária (seção 2); a única duplicação removível (ordem de
      prioridades sem lar rastreado) foi resolvida por migração, não por redução de texto duplicado
      entre `CLAUDE.md`/`AGENTS.md`.

### Sprint-specific boundary respeitado

"Agent-specific file" não foi tratado como sinônimo de "deve ser removido do Git" — nenhum arquivo
de governança de IA foi proposto para remoção; a única ação de arquivo foi uma criação (fechamento
de paridade) e duas edições em `README.md` (migração de regra + correção factual pontual).

### Riscos residuais

- `.github/upgrades/*` e o arquivo `.orig` órfão permanecem no disco local, sem decisão do owner.
- A duplicação intencional entre `CLAUDE.md`/`AGENTS.md` significa que uma futura mudança de
  governança precisa ser replicada manualmente nos dois arquivos — risco de deriva já mitigado pela
  leitura cruzada instruída em `AGENTS.md` §2.1, mas não eliminado.

### Validação executada

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa. Nenhum segredo introduzido (verificado por leitura direta dos 3 arquivos de
settings). Nenhum código-fonte, projeto, script, workflow, migration ou teste alterado.

### Deliverable

Relatório de classificação de governança (tabela da seção 2), ownership de regra de projeto vs.
regra de agente reconciliado, `README.md` raiz atualizado (nova seção + correção factual),
`.agents/skills/beeday-ui-ux/SKILL.md` criado, e Documentation Ledger sincronizado.

---
## Sprint 31.4 — Architecture Documentation Reconciliation

**GitHub Issue:** [#232](https://github.com/tiagoarrigoni/BeeDay/issues/232)
**Branch:** `sprint/31.4-architecture-documentation-reconciliation`
**Depende de:** #231 (concluída, ver seção acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Reconciliar os 9 documentos de `docs/architecture/` com o sistema real pós-EPIC-30, sem inventar
camadas, padrões ou responsabilidades.

### Validação aplicada nesta Sprint (política aprovada pelo owner)

Sprint documental — nenhum código-fonte foi alterado (apenas `docs/architecture/*.md`). Suíte
completa de testes **não** executada, por política aprovada em 2026-08-21.

### Método

Comparação linha a linha entre os 9 documentos e evidência real: os 4 `.csproj` de `src/*`
(`ProjectReference`/`PackageReference`), `Program.cs`, `BeeDayDbContext.cs` e `Configurations/*`,
os 8 repositórios + 2 read services + `IUnitOfWork`, `User.cs`/`Habit.cs`, e os 6 ADRs. Um
levantamento inicial abrangente foi delegado (agente de exploração, somente leitura, sem edição) e
todo achado de discrepância relatado foi **reverificado de forma independente nesta sessão** antes
de qualquer edição — `grep`/leitura direta confirmaram cada mismatch abaixo antes da correção.

### Achados corrigidos (Required work #3)

| Documento | Achado | Correção aplicada |
|---|---|---|
| `03-clean-architecture.md` (tabela §1, linha `BeeDay.Web`) | Claim "`IStringLocalizer`/`ResourceManager` fora de `BeeDay.Web` → 0 ocorrências" estava incorreta: `src/BeeDay.Infrastructure/Identity/IdentityEmailComposer.cs:21` usa `ResourceManager` puro (não `IStringLocalizer`) deliberadamente, para localizar e-mail transacional sem depender de Web (ADR-006). | Claim dividida: `IStringLocalizer` continua exclusivo de Web (confirmado, 0 ocorrências); `ResourceManager` em Infrastructure agora citado como exceção documentada, com link para ADR-006 e `docs/infrastructure/06-transactional-email.md`. |
| `04-dependency-rules.md` (linha 29) | `Habit.cs:18` citado para `Habit.Create(...)` — assinatura real está na linha 20. | Corrigido para `Habit.cs:20` (verificado via `grep -n`). |
| `05-runtime-flows.md` (linha 85) | `/auth/login` citado como `Program.cs:247-316` — endpoint real começa em 295 e termina em 381 (arquivo cresceu desde a última verificação). | Corrigido para `Program.cs:295-381`. |
| `06-persistence-architecture.md` §5 (Migrations) | Afirmava "exatamente uma migration"; hoje existem 2 — `20260821054442_AddTransactionAmountUpperBoundCheckConstraint.cs` foi adicionada em 2026-08-21 (mesmo dia desta Sprint, commit `06b5b49`). | Seção reescrita citando as 2 migrations e o que a segunda faz; cabeçalho "Fonte da verdade" do documento também atualizado. |
| `06-persistence-architecture.md` §6 (tabela `Transactions`) | Check constraint documentado como `Amount > 0`; a migration acima o substituiu por `Amount > 0 AND Amount <= 999999999999`. | Tabela atualizada com o teto superior e referência à migration. |
| `06-persistence-architecture.md` §7 | `IProjectRepository` citado com 13 métodos; contagem real (`grep -c "Task"` no arquivo de interface) é 12. | Corrigido para 12 métodos (continua sendo o maior repositório, conclusão não muda). |
| `06-persistence-architecture.md` §10 | Contagem de testes de `Persistence/SqlServer/` citada como 65; contagem real de `[Fact]`/`[Theory]` é 68. | Corrigido para 68 — este é exatamente o tipo de "stale test count" que a Sprint 31.1 pediu para localizar (Issue #229, item 5). |
| `07-security-architecture.md` (5 citações de linha) | `Program.cs:124-171` (cookie auth), `Program.cs:144-170` (`OnValidatePrincipal`), `User.cs:33` (`SessionVersion`), linha 131 (`InvalidateSessions()`), `Program.cs:302-316` (rate limiter), `Program.cs:219` (`UseAntiforgery()`) — todas desatualizadas por crescimento do arquivo desde a última verificação. | Todas as 6 corrigidas para os números de linha reais, verificados nesta sessão: `127-183`, `146-183`, `35`, `150`, `367-381`, `267`, respectivamente. Nenhuma mudança de conteúdo/lógica — apenas rastreabilidade. |

Nenhuma correção alterou uma conclusão arquitetural — todas preservam exatamente o que o documento
já afirmava sobre comportamento, apenas corrigindo onde encontrar a evidência no código atual.

### Lacuna de citação de ADR fechada (Required work #5)

Nenhum dos 9 documentos citava [ADR-003](../adr/ADR-003-aggregate-repositories.md) — a decisão
mais diretamente relacionada ao §7-8 de `06-persistence-architecture.md` (repositórios por
Aggregate + read services + `IUnitOfWork`), classificada `VALID` na Sprint 31.2. Adicionada uma
citação explícita no início do §7, evitando duplicar o conteúdo do ADR (apenas um link + uma frase
de contexto).

Confirmado: nenhum dos 9 documentos cita ADR-001 (`CONFLICTING`, Sprint 31.2) como autoridade —
eles simplesmente não o mencionam, então não há "confiança indevida em ADR inválido" para corrigir
aqui (Required work #8/acceptance criterion correspondente já satisfeito por omissão, não por
correção).

### Conteúdo já correto, confirmado sem reescrita (Required work #4)

A grande maioria das afirmações verificadas already estava correta e não foi tocada, incluindo:
estrutura de 4 projetos e grafo de `ProjectReference` (`02-solution-structure.md`), isolamento do
Domain (zero `using` EF Core/AspNetCore), os 8 repositórios + `IUnitOfWork` e seus registros DI
(`AddScoped`/`AddTransient`), o pipeline de behaviors do MediatR, a configuração de cookie/rate
limit/antiforgery em si (apenas os números de linha estavam desatualizados, não a lógica
descrita), os 5 segredos de `deploy-prd.yml`, e o `web.config` de HMG. Nenhum destes foi reescrito
por estilo — permanecem `CURRENT` no Ledger sem alteração de conteúdo.

### Confirmação: nenhuma camada/padrão inventado (acceptance criterion)

Nenhum documento descreve uma camada, projeto `Contracts` separado, ou padrão que não existe no
código — confirmado por busca (`find -iname "*Contracts*"`) que não há nenhum `.csproj` de nome
`Contracts` em lugar nenhum da solução; os "contratos" são pastas dentro de `BeeDay.Application`,
exatamente como os 9 documentos já descreviam.

### Documentation Ledger — atualização

| ID | Path | Atualização |
|---|---|---|
| DOC-041–049 | `docs/architecture/*` (9 arquivos) | `Current state` permanece `CURRENT`; achados de linha/contagem desatualizados corrigidos (tabela acima); evidência de reconciliação registrada. |

### Critérios de aceite (Issue #232)

- [x] Architecture docs match actual project references — confirmado e já correto antes desta
      Sprint.
- [x] Domain/Application/Infrastructure/Web responsibilities match repository truth — 1 correção
      aplicada (`ResourceManager` em Infrastructure).
- [x] No undocumented invented layer or pattern appears — confirmado, nenhum projeto `Contracts`
      inventado nem em código nem em documentação.
- [x] Current architecture docs do not rely on invalid/superseded ADRs as current authority —
      nenhum dos 9 documentos cita ADR-001 (`CONFLICTING`); ADR-002/003/006 citados são todos
      `VALID`.
- [x] Duplicate architecture explanations are consolidated or linked — nenhuma duplicação nova
      introduzida; lacuna de citação ADR-003 fechada por link, não por cópia de conteúdo.
- [x] Already-correct architecture content may be marked CURRENT without rewrite — aplicado (seção
      acima).

### Sprint-specific boundary respeitado

Nenhuma mudança de arquitetura de software foi feita para facilitar a documentação — todas as 8
correções são fatos (números de linha, contagens, uma migration nova, uma exceção documentada já
existente no código) sendo alinhados à documentação, nunca o inverso.

### Riscos residuais

- Itens marcados `UNVERIFIABLE` pelo levantamento inicial (ex.: comportamento interno de
  `Deploy-BeeDay.ps1`, conteúdo de `appsettings*.json`, histórico de execução de `deploy-prd.yml`)
  não foram reverificados nesta Sprint — pertencem ao escopo de 31.11/31.12, não de arquitetura.
- Novas migrations/mudanças de schema futuras podem voltar a desatualizar §5/§6 de
  `06-persistence-architecture.md` — sem mecanismo automático de detecção; próxima Sprint que tocar
  Infrastructure deve reverificar.

### Validação executada

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa. Todo número de linha citado nas correções acima foi verificado com `grep -n`/leitura
direta nesta sessão antes de ser escrito.

### Deliverable

5 arquivos de `docs/architecture/` corrigidos (`03`, `04`, `05`, `06`, `07`), lacuna de citação
ADR-003 fechada, e Documentation Ledger atualizado.

---
## Sprint 31.5 — Domain & Functional Model Documentation Reconciliation

**GitHub Issue:** [#233](https://github.com/tiagoarrigoni/BeeDay/issues/233)
**Branch:** `sprint/31.5-domain-functional-model-documentation-reconciliation`
**Depende de:** #232 (concluída, ver seção acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Reconciliar os 15 documentos de `docs/domain/` com o Domain atual, sem converter documentação em
cópia de assinaturas C#.

### Validação aplicada nesta Sprint (política aprovada pelo owner)

Sprint documental — nenhum código-fonte foi alterado. Suíte completa de testes **não** executada,
por política aprovada em 2026-08-21.

### Método

Comparação exaustiva entre os 15 documentos de `docs/domain/` e o código atual: os 47 arquivos de
`src/BeeDay.Domain/` (todas as 8 entidades Aggregate Root lidas por completo — construtor,
invariantes, mutações, eventos), os 12 enums, os 6 Value Objects + 2 tipos VO-like em `Experience/`,
os 5 arquivos de `Events/`, e o subsistema de XP/Level (`UserExperience`, `ExperienceEntry`,
`ExperienceCurve`, `ExperienceRewardPolicy`, `ExperienceRewardEventPublisher`). Levantamento inicial
delegado a um agente de exploração somente-leitura; toda conclusão relevante foi cruzada contra
citações de arquivo/linha antes de qualquer edição.

### Resultado: nenhuma divergência de conteúdo encontrada

Diferente das Sprints 31.4 (8 correções), a comparação desta Sprint não encontrou nenhum mismatch
de fato entre os 15 documentos e o código atual — nenhum conceito de negócio obsoleto, nenhuma
contagem incorreta, nenhum método/classe inexistente, nenhuma assinatura desatualizada. Verificado
especificamente e confirmado correto:

- os 8 Aggregate Roots (`User`, `Habit`, `RecurringTask`, `Project`, `Wallet`, `WalletTag`,
  `Transaction`, `UserToken`) e que `Todo` é corretamente descrito como entidade filha de `Project`
  (sem `ITodoRepository`), não um Aggregate Root;
- `Profile` como projeção sem identidade computada a cada leitura de `User.Profile`, sem estado
  próprio persistido — descrito de forma idêntica em `entities.md`/`user.md`;
- o subsistema de XP/Level: nenhum evento de domínio é construído dentro de `Domain` (apenas
  definidos lá) — `ExperienceGrantedDomainEvent`/`UserLeveledUpDomainEvent` são publicados por
  `Application/Common/Experience/ExperienceRewardEventPublisher.cs`, exatamente como
  `domain-events.md` descreve; valores de recompensa (Habit 1 XP, Task 5 XP, Todo 7 XP, Project 20
  XP) conferem com `ExperienceRewardPolicy.cs`;
- `relationships.md`: todo relacionamento (referência por Guid vs. composição real, único caso de
  navegação de coleção sendo `Project.Todos`) confere com os campos reais de cada entidade;
- os 47 arquivos de `docs/domain/audit-inventory.md` (Sprint 30.5) continuam existindo e
  correspondendo ao código — nenhuma adição/remoção desde então.

### Achados menores corrigidos (Required work #3)

| Documento | Achado | Correção |
|---|---|---|
| `wallet.md` | Frase de resumo dizia "(3 métodos)" para o Aggregate Root, mas a própria tabela de Operações públicas do mesmo documento lista 5 (`Create`, 3×`Calculate*`, `Touch`) — inconsistência interna de redação, não um erro contra o código. | Reescrita para "(3 métodos de cálculo além de `Create`/`Touch`)", removendo a ambiguidade sem mudar nenhuma afirmação factual. |

### Resolução do achado de duplicação (`DOC-077`, Sprints 31.1/31.2)

`docs/domain/audit-inventory.md` foi identificado na Sprint 31.1 como possível duplicata do
inventário de Sprint 30.5 embutido em `docs/epics/30-system-integrity/README.md`, com decisão final
explicitamente atribuída a esta Sprint (31.2, ownership map). Decisão: **manter ambas as cópias,
com ownership canônico declarado** — `docs/domain/audit-inventory.md` é a referência viva
(atualizada se o Domain mudar; reverificada e confirmada 100% atual nesta Sprint), a cópia dentro do
relatório da Sprint 30.5 é o registro histórico congelado de como a Sprint foi entregue (EPIC 30 já
está `HISTORICAL`, documentos de Epic concluída não são reescritos por convenção). Nenhum conteúdo
foi removido — decisão registrada como nota explícita no topo de `audit-inventory.md`. Outcome do
Ledger: `DOC-077` muda de `DUPLICATED` (estado de trabalho, Sprint 31.1) para `CURRENT`
(disposição final: mantido, não mesclado nem removido).

### Critérios de aceite (Issue #233)

- [x] Every documented Domain concept exists in current repository evidence — 100% confirmado, 0
      conceitos inventados.
- [x] Aggregate boundaries match implementation — 8 Aggregate Roots confirmados via interfaces de
      repositório reais; `Todo` corretamente descrito como filho de `Project`.
- [x] Important invariants are documented from verifiable evidence — todas as invariantes citadas
      nos 15 documentos foram verificadas linha a linha contra o código.
- [x] No obsolete business concept remains presented as current — nenhum encontrado.
- [x] Documentation does not duplicate raw implementation signatures unnecessarily — confirmado;
      os documentos descrevem comportamento/invariante em nível conceitual, não colam assinaturas
      C# completas.

### Sprint-specific boundary respeitado

Nenhuma preocupação de UI/Application foi movida para a documentação de Domain. Nenhum
comportamento de Domain foi alterado para casar com documentação antiga — nesta Sprint a
documentação já estava correta, então nenhuma mudança de código foi sequer cogitada.

### Riscos residuais

- Duas pequenas inconsistências de **código** (não de documentação) foram confirmadas durante a
  verificação, ambas já corretamente documentadas como tal pelos próprios arquivos e não corrigidas
  nesta Sprint (fora de escopo — reconciliação documental, não correção de código):
  - `UserToken.Create` valida seu enum via `Enum.IsDefined` bruto em vez do helper compartilhado
    `EnumValidation.Defined` usado por toda outra entidade (`user-token.md` já sinaliza isso como
    "inconsistência menor, não corrigida").
  - Nenhuma ação necessária — ambas já são conhecidas e documentadas; mencionadas aqui apenas para
    rastreabilidade caso uma Sprint de engenharia futura queira endereçá-las.

### Validação executada

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa.

### Deliverable

`docs/domain/wallet.md` (correção de redação) e `docs/domain/audit-inventory.md` (nota de
canonicidade) atualizados; Documentation Ledger sincronizado (`DOC-077` → `CURRENT`).

---
## Sprint 31.6 — Application & Use-Case Documentation Reconciliation

**GitHub Issue:** [#234](https://github.com/tiagoarrigoni/BeeDay/issues/234)
**Branch:** `sprint/31.6-application-use-case-documentation-reconciliation`
**Depende de:** #233 (concluída, ver seção acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Reconciliar os 7 documentos de `docs/application/` com a orquestração real de casos de uso, sem
inventar padrões não implementados.

### Validação aplicada nesta Sprint (política aprovada pelo owner)

Sprint documental — nenhum código-fonte foi alterado. Suíte completa de testes **não** executada,
por política aprovada em 2026-08-21.

### Método

Levantamento inicial delegado (agente de exploração, somente leitura) cobrindo os 10 diretórios de
`Features/`, os 4 Behaviors do pipeline MediatR, as 8 interfaces de repositório + `IUnitOfWork`, a
única exceção própria de Application, e os 37 Commands + 6 Queries reais. Todo achado de
discrepância foi reverificado nesta sessão com `grep`/leitura direta antes de qualquer edição.

### Achados corrigidos (Required work #2)

| Documento | Achado | Correção |
|---|---|---|
| `04-contracts.md` | Afirmava que apenas `UpdateTodoCommandHandler` usa `IUnitOfWork` completo, sendo o padrão comum injetar o repositório isolado. Na realidade, **13 Handlers em 6 Features** usam `IUnitOfWork`, a maioria com transação explícita coordenando 2+ Aggregates — é um padrão central, não uma exceção. | Seção reescrita listando os 13 Handlers reais por Feature, mantendo os Handlers de escrita única (sem transação) como contraste correto. |
| `04-contracts.md` | `IProjectRepository` citado com 13 métodos; contagem real é 12 (mesmo achado já corrigido em `docs/architecture/06-persistence-architecture.md` na Sprint 31.4 — agora consistente nos dois documentos). | Corrigido para 12. |
| `01-cqrs.md` | Tabela Command/Query dizia "24 comandos" sem retorno; contagem real (`grep` de `record.*Command`) é 31 (37 comandos totais − 6 com retorno). | Corrigido para 31. |
| `01-cqrs.md`, `02-use-cases.md` (3 ocorrências) | "9 Features" — contagem real é 10 (`Authentication, Dashboard, Habits, Identity, Ordering, Projects, Tasks, Todos, Users, Wallets`), consistente com a própria listagem de `README.md` e com o corpo dos dois documentos, que já têm 10 seções. | Corrigido para 10 nas 3 ocorrências. |
| `02-use-cases.md` | "5 casos de uso concedem XP" citando apenas 3 Commands na mesma frase — não fechava a conta mesmo contando `ToggleTodoCommand` como até duas concessões (máximo 4). | Reescrito para "3 Commands concedem XP", com a ressalva de que `ToggleTodoCommand` pode conceder duas vezes na mesma execução. |
| `02-use-cases.md` | Linha "Obter usuário atual" citava o Handler apenas como "(Handler correspondente em `UserHandlers.cs`)", em vez do nome de classe real, diferente de toda outra linha da tabela. | Nomeado explicitamente `GetCurrentUserQueryHandler`. |
| `README.md` | Listagem de `Common/Identity/` omitia `IEmailConfirmationIssuer.cs` (6 arquivos reais, 5 listados) — interface com implementação concreta registrada em DI e usada por 4 Handlers de Identity/Users. | Adicionado à listagem. |

### Padrão de orquestração documentado (Required work #3)

`03-pipeline.md` documentava apenas o `ApplicationActionDomainEvent` sintético publicado pelo
`DomainEventBehavior`. Adicionada uma nota distinguindo esse caminho do caminho **manual** de
publicação dos eventos de domínio reais (`ExperienceGrantedDomainEvent`/`UserLeveledUpDomainEvent`,
definidos em `Domain/Events/`), disparados diretamente pelos 3 Handlers que concedem XP via
`ExperienceRewardEventPublisher` — sem duplicar o detalhe já coberto por `docs/domain/domain-
events.md`, apenas um link cruzado.

### Conteúdo já correto, confirmado sem reescrita (Required work #4)

A descrição dos 4 Behaviors do pipeline (`03-pipeline.md`) conferiu linha a linha sem nenhum
mismatch — ordem de registro, lógica de cada um, incluindo o trecho de reflexão do
`DomainEventBehavior` reproduzido verbatim. A hierarquia de exceções (`05-exceptions.md`) também
não teve nenhum mismatch: `ApplicationValidationException` é a única exceção própria, exceções de
Domain propagam sem tradução, `ActivityNotFoundException` confirmada inexistente no repositório
inteiro. O catálogo completo de casos de uso por Feature em `02-use-cases.md` (Commands, Queries,
Handlers, Contracts, Aggregate, Repository, Resultado) conferiu 100% correto exceto os 2 achados
acima — todas as outras ~35 linhas de tabela permanecem inalteradas.

### Critérios de aceite (Issue #234)

- [x] Documented commands/queries/use-case patterns exist — confirmado; 3 contagens corrigidas.
- [x] Application documentation reflects actual dependency boundaries — `IUnitOfWork`/repositório
      isolado corrigido para refletir o padrão real.
- [x] Critical application flows have accurate orchestration descriptions — pipeline e concessão
      de XP verificados e (no segundo caso) complementados.
- [x] UI concerns are not presented as Application responsibilities — nenhuma menção de
      Razor/Blazor/HTTP encontrada nos 7 documentos.
- [x] Idempotency/cancellation/authorization claims are only made where verified — verificado:
      propagação de `CancellationToken` confirmada em handlers de 5 Features distintas; guards de
      ownership (`RequireExistsAsync`/`RequireOwnedTagAsync`/`EnsureOwned`) e checagens de
      duplicidade (`IsNameInUseAsync`/`IsEmailInUseAsync`) confirmados como reais, não apenas
      afirmados.

### Sprint-specific boundary respeitado

Nenhuma preocupação de apresentação (Razor/Blazor/HTTP) foi documentada como responsabilidade de
Application. Nenhum padrão foi inventado — todas as correções acima alinham a documentação ao que
o código já faz.

### Riscos residuais

Nenhum. Todos os achados desta Sprint foram corrigidos; nenhuma correção pendente foi identificada
que dependa de outra Sprint especialista.

### Validação executada

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa. Toda contagem/citação corrigida foi reverificada com `grep`/leitura direta nesta
sessão antes de ser escrita.

### Deliverable

5 arquivos de `docs/application/` corrigidos (`01-cqrs.md`, `02-use-cases.md`, `03-pipeline.md`,
`04-contracts.md`, `README.md`), e Documentation Ledger atualizado.

---
## Sprint 31.7 — Infrastructure & Persistence Documentation Reconciliation

**GitHub Issue:** [#235](https://github.com/tiagoarrigoni/BeeDay/issues/235)
**Branch:** `sprint/31.7-infrastructure-persistence-documentation-reconciliation`
**Depende de:** #234 (concluída, ver seção acima)

> Segue `CLAUDE.md`, o padrão de planejamento beeday e o Global Execution Contract da EPIC 31 em
> #228.

### Objetivo da Sprint

Reconciliar os 10 documentos de `docs/infrastructure/` e `docs/persistence/` com a implementação
real de EF Core/SQL Server e os adaptores externos.

### Validação aplicada nesta Sprint (política aprovada pelo owner)

Sprint documental — nenhum código-fonte foi alterado. Suíte completa de testes **não** executada,
por política aprovada em 2026-08-21.

### Método

Levantamento inicial delegado (agente de exploração, somente leitura), explicitamente informado dos
achados já confirmados nas Sprints 31.4/31.6 sobre esta mesma área de código (2 migrations, contagem
de métodos de `IProjectRepository`, padrão de uso de `IUnitOfWork`, contagem de testes) para
verificar se os mesmos fatos desatualizados se repetiam nestes 10 documentos. Todo achado foi
reverificado nesta sessão com `grep`/leitura direta antes de qualquer edição.

### Achados corrigidos (Required work #2)

| Documento | Achado | Correção |
|---|---|---|
| `02-sql-server.md`, `docs/persistence/01-relational-model.md`, `docs/persistence/02-ef-core-strategy.md` | Repetiam a mesma alegação já corrigida na Sprint 31.4/31.6 em outros documentos: "exatamente uma migration"/`CK_Transactions_Amount > 0`. Hoje existem 2 migrations e o check tem teto superior. | As 3 seções reescritas citando `20260821054442_AddTransactionAmountUpperBoundCheckConstraint.cs`; "Fontes de verdade" dos 2 documentos de persistence atualizadas para citar a segunda migration. |
| `01-repositories.md` | `IProjectRepository` citado com 13 métodos (mesmo achado já corrigido em `docs/architecture/`/`docs/application/`). | Corrigido para 12. |
| `01-repositories.md` | Afirmava que `EfDashboardReadService`/`EfWalletReadService` calculam resumos via os métodos de Domain `Wallet.Calculate*`. Na realidade ambos usam agregação SQL (`SumAsync`) diretamente, contornando o Domain deliberadamente para não carregar toda transação pela rede. | Reescrito para descrever o mecanismo real. |
| `README.md` (infrastructure) | `Diagnostics/` descrita como vazia; hoje contém `EmailEventIds.cs` (Sprint 28.7). | Corrigido. |
| `README.md` (infrastructure) | Listagem de `Identity/` (6 arquivos) omitia `HmgRecipientGuardedEmailSender`, `EmailAddressLogMasking` e os 3 catálogos `EmailResources.*.resx`. | Listagem completada (10 itens reais). |
| `README.md` (infrastructure) | "17 interfaces" — a própria enumeração da frase soma 19. | Corrigido para 19. |
| `04-services.md` | `ResendEmailSender` descrito como registrado diretamente como `IEmailSender`; desde a Sprint 26.4, `IEmailSender` real é `HmgRecipientGuardedEmailSender` (decorator), `ResendEmailSender` é registrado como tipo concreto. | Célula da tabela corrigida com a cadeia real. |
| `04-services.md` | `IdentityEmailComposer` descrito com tema escuro/`#7A4FCB`, sem parâmetro de idioma — desatualizado desde a Sprint 26.6 (cor) e 28.2/ADR-006 (localização). | Corrigido para tema claro/`#5247F9`/`UserLanguage`/`ResourceManager`, com link para `06-transactional-email.md`. |
| `04-services.md` | Guarda de path traversal de `DevelopmentEmailSender` descrita como uniforme; desde a Sprint 26.9, um `Directory` absoluto contorna a checagem deliberadamente (só o caso relativo continua protegido). | Célula reescrita distinguindo os dois casos. |
| `06-transactional-email.md` | §2 (mapa de arquitetura) citava "5 Options classes"; hoje são 6 (`HmgRecipientGuardOptions`, Sprint 26.4, nunca anotada como atualização nesta seção específica, embora o resto do documento a descreva corretamente em outras seções). | Corrigido para 6, com a peça do decorator de e-mail também adicionada ao mesmo diagrama. |

Nenhuma referência a persistência JSON legada (`LevelUpData`, `JsonLevelUpDocumentStore`) foi
encontrada apresentada como estado atual em nenhum dos 10 documentos — todas as menções
encontradas já são corretamente históricas ou são comentários de código (fora do escopo documental
desta Sprint, conforme já registrado no próprio `README.md` como achado aceito). Nenhum valor de
segredo (connection string, API key) aparece em nenhum dos 10 documentos — apenas nomes de
seção/chave de configuração.

### Conteúdo já correto, confirmado sem reescrita (Required work #4)

`docs/infrastructure/03-concurrency.md` e `05-dependency-injection.md` conferiram 100% sem nenhum
mismatch — o mecanismo de RowVersion/tradução de exceção de concorrência e as 29 registrações de DI
(incluindo o decorator de e-mail já corretamente descrito neste último) já estavam corretos.
`docs/persistence/01-relational-model.md`/`02-ef-core-strategy.md` também conferiram corretos em
praticamente todo o resto do conteúdo (11 tabelas, TPC, Owned Types, Complex Types, conversores de
enum, RowVersion shadow property) — apenas os 2 achados de migration acima precisaram de correção.

### Critérios de aceite (Issue #235)

- [x] SQL Server/EF Core documentation matches current runtime — 10 achados corrigidos.
- [x] Repository/UoW responsibilities match implementation — contagens e mecanismos de cálculo
      corrigidos.
- [x] Migration and transaction documentation is accurate — 3 documentos atualizados para as 2
      migrations reais.
- [x] No legacy JSON persistence path is presented as current — confirmado, nenhum encontrado.
- [x] Configuration contracts are documented without sensitive values — confirmado.
- [x] Historical persistence material is preserved appropriately — nenhum conteúdo histórico foi
      removido; apenas fatos de estado atual foram corrigidos.

### Sprint-specific boundary respeitado

Nenhuma mudança de arquitetura de persistência foi feita para simplificar a documentação — todas as
10 correções alinham a documentação ao código já existente.

### Riscos residuais

- Comentários de código (não documentação) em `EfConcurrencySaveChanges.cs` e
  `EventJournalOptions.cs` ainda mencionam "o provider JSON" como referência histórica — já
  registrado como achado aceito e fora de escopo em `docs/infrastructure/README.md`; nenhuma ação
  nova necessária.
- `docs/infrastructure/06-transactional-email.md` tem um volume grande de narrativa de
  deployment/HMG (§5.1, §6, §15-17) fora do escopo desta Sprint (pertence a 31.11/31.12) — não
  reverificado aqui.

### Validação executada

```bash
git status
dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx --configuration Release --warnaserror
git diff --check
```

Resultados registrados na seção "Quality/Validation" do relatório final da Sprint enviado ao owner
nesta conversa. Toda contagem/citação corrigida foi reverificada com `grep`/leitura direta nesta
sessão antes de ser escrita.

### Deliverable

7 arquivos corrigidos (`docs/infrastructure/README.md`, `01-repositories.md`, `02-sql-server.md`,
`04-services.md`, `06-transactional-email.md`, `docs/persistence/01-relational-model.md`,
`02-ef-core-strategy.md`), e Documentation Ledger atualizado.

---
