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
