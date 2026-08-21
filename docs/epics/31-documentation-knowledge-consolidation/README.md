# EPIC 31 — beeday Repository Documentation & Knowledge Consolidation

**GitHub Issue:** [#228](https://github.com/tiagoarrigoni/BeeDay/issues/228)
**Status:** Em andamento — Sprint 31.1 concluída

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
  - `CLAUDE.md`, `AGENTS.md`, `.claude/` (11 arquivos), `.agents/` (10 arquivos), `.codex/`
    (3 arquivos) — governança de IA, untracked desde o commit `869b57e`
    ("chore(governance): stop tracking CLAUDE.md in the product repository", 2026-08-20) — total
    26 arquivos.
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
