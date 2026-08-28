# Contrato de Workflow, Promoção e Reconciliação de Drift — EPIC 33, Sprint 33.19

Documento canônico produzido pela Sprint 33.19 (Issue [#380](https://github.com/DS-Asyst/BeeDay/issues/380)),
consolidando em um único lugar as regras de branching/PR do Lab, o fluxo de trabalho visual futuro
(pós-EPIC-33), o procedimento de reconciliação de drift, e o procedimento de integração
`beeday-frontend-lab:prd` → `DS-Asyst/BeeDay`. Nada aqui é novo em espírito — cada regra já estava em
vigor desde a Sprint em que foi estabelecida (citada inline); este documento apenas as reúne como
referência única, conforme exigido pela Issue #380 item 3.

## 1. Papéis dos dois repositórios (contrato permanente, não muda com o tempo)

```text
DS-Asyst/BeeDay              → única fonte de verdade de runtime/negócio. Nunca deixa de ser produção.
DS-Asyst/beeday-frontend-lab → fonte visual validada. NUNCA é implantado como produção.
```

- `beeday-frontend-lab:prd` marca uma baseline visual **aprovada pelo proprietário**, não uma
  implantação. Não existe pipeline de deploy a partir de `prd` do Lab.
- Nenhuma sincronização automática ocorre em qualquer direção entre os dois repositórios. Toda
  integração de mudança visual do Lab para `DS-Asyst/BeeDay` é manual, deliberada, e revisada como
  qualquer outra mudança de produto.
- `DS-Asyst/BeeDay` não referencia, importa, nem faz build contra `DS-Asyst/beeday-frontend-lab` em
  nenhum pipeline de CI/CD.

## 2. Branching e PR do Lab (`DS-Asyst/beeday-frontend-lab`)

Estabelecido pela Sprint 33.5 e seguido sem exceção desde então (Sprints 33.6-33.19):

```text
hmg (branch padrão)
 ↓
sprint/<numero>-<descricao>  (branch de trabalho, criada a partir do hmg local mais recente)
 ↓ Lab CI verde no head validado
PR → hmg  (obrigatório; check "Lab CI" obrigatório via Ruleset "Protect HMG", id 21652764)
 ↓ merge
hmg atualizado
 ↓ (quando aprovado pelo proprietário — ver Seção 4)
PR hmg → prd  (obrigatório; check "Lab CI" obrigatório via Ruleset "Protect PRD", id 21652766)
 ↓ merge
prd atualizado + tag de baseline aprovada
```

Regras dos Rulesets (idênticas para `hmg` e `prd`, confirmadas por leitura direta da API do GitHub
nesta Sprint): bloqueiam `deletion` e `non_fast_forward`; exigem Pull Request (`required_approving_review_count: 0`
— sem exigência de aprovação humana adicional além do check obrigatório, já que o proprietário é o
único committer autorizado no momento); exigem o check `Lab CI` (formatação + build `--warnaserror` +
testes, sem LocalDB por arquitetura — Sprint 33.5).

Documentação de commits/PR/docs do Lab segue o mesmo padrão de duas PRs por Sprint já estabelecido:
uma PR de código no repositório Lab (`sprint/<numero>-<descricao>`), seguida de uma PR de evidência/
Ledger no `DS-Asyst/BeeDay` (`sprint/<numero>-<descricao>-docs`) — nunca misturadas na mesma PR,
nunca a documentação adiantando-se ao código que ela descreve.

## 3. Extração vs. trabalho visual futuro (pós-EPIC-33)

A EPIC 33 extraiu/adaptou a superfície completa mapeada pelo Ledger (109 itens). Trabalho visual
futuro no Lab (novas explorações de UI, protótipos, iterações de Design System) segue o mesmo ciclo
branch → PR → `hmg` → (quando aprovado) PR → `prd` da Seção 2, mas não precisa mais referenciar um
`FE33-*` do Ledger original — esse Ledger é um artefato histórico da migração/extração, não um
registro vivo de todo trabalho futuro do Lab. Um novo Ledger ou mecanismo de rastreamento, se
necessário para uma iniciativa futura, é decisão de uma Epic/Sprint futura, não desta.

A regra COPY/ADAPT/MOCK/EXCLUDE (ADR-008, `04_MOCK_STATE_POLICY.md`) permanece válida para qualquer
extração futura de produção → Lab: nenhum reuso binário, cópia/adaptação de código-fonte apenas,
nunca lógica de negócio real, nunca banco de dados/autenticação/e-mail reais.

## 4. Aprovação visual do proprietário (obrigatória antes de qualquer promoção `hmg` → `prd`)

Conforme estabelecido pela Sprint 33.18 e confirmado pela Issue #380 ("Do not promote Lab baseline if
owner visual approval is pending"): nenhuma promoção `hmg` → `prd` ocorre sem aprovação visual
explícita e registrada do proprietário sobre uma superfície viva (não uma captura de tela). O
registro de aprovação inclui, no mínimo: o SHA candidato exato, a data, e a confirmação de que o
proprietário revisou (ou aceitou o gate automatizado de paridade como suficiente, quando
explicitamente declarado por ele) as superfícies vivas relevantes (`/preview`, `/design-system`,
`/emails`, e quaisquer páginas de produto específicas relevantes à mudança).

**Precedente registrado nesta Sprint:** aprovação do proprietário para o SHA `bdcbea9` concedida na
sessão de execução da EPIC 33 em 2026-08-28 (ver Seção 7 do relatório de paridade,
[`04-baseline-parity-report.md`](04-baseline-parity-report.md)), usada para autorizar a PR #13 do Lab
(`hmg` → `prd`).

## 5. Procedimento de integração `beeday-frontend-lab:prd` → `DS-Asyst/BeeDay`

Este procedimento aplica-se quando uma mudança visual desenvolvida/validada no Lab precisa ser
trazida de volta para produção — cenário distinto da extração original (produção → Lab) que dominou
a EPIC 33. Não há automação; cada etapa é manual e revisada:

1. **Identificar o alvo:** qual arquivo/componente real de `src/BeeDay.Web/` corresponde ao artefato
   do Lab a ser integrado.
2. **Reverter as adaptações Lab-específicas:** qualquer coisa que a Ledger tenha registrado como
   ADAPT/MOCK para aquele item (localização real, `ISender`/`BeeDayWebService`, autenticação real,
   URLs/tokens reais) precisa ser reintroduzida — o Lab nunca é copiado de volta literalmente, pois
   ele deliberadamente remove exatamente essas dependências.
3. **Abrir PR normal em `DS-Asyst/BeeDay`** seguindo o CLAUDE.md §7 (branch de trabalho → PR → `hmg`
   do BeeDay → merge autônomo permitido; `main`/`prd` do BeeDay permanecem fora da autoridade Git do
   Claude, CLAUDE.md §6.7/§6.11 — nenhuma mudança nesta Sprint alterou esse limite).
4. **Validação completa de produção:** toda a suíte de validação normal do BeeDay (`dotnet format`/
   `build`/testes por projeto, EF Core quando aplicável) — a validação do Lab (sem LocalDB) não
   substitui a validação de produção.
5. **Registrar a origem:** a PR de integração deve citar o commit/PR do Lab que originou a mudança,
   para rastreabilidade — mesma disciplina de citação já usada em toda a EPIC 33.

## 6. Reconciliação de drift

"Drift" = uma mudança acontece em um dos dois repositórios sem o mesmo momento de decisão explícita
no outro. Dado que não há sincronização automática (Seção 1), drift é esperado e não é, por si só, um
defeito — é a consequência natural de dois repositórios com propósitos diferentes evoluindo em
ritmos diferentes. Procedimento quando um drift é encontrado:

1. **Registrar o drift, não corrigi-lo silenciosamente** — mesmo padrão já usado repetidamente ao
   longo da EPIC 33 (ex.: fonte Inter/Jersey-25 → Coiny/Nunito, Sprint 33.6; `beeday-card-menu.js`
   ausente, Sprint 33.8; link do GitHub em `AppFooter.razor`, Sprint 33.9).
2. **Classificar:** o drift é uma mudança de produção que o Lab ainda não capturou (comum e
   esperado — o Lab não é atualizado automaticamente), ou uma mudança do Lab que diverge
   intencionalmente da produção (deveria estar registrada como `APPROVED_LAB_DIFFERENCE` no Ledger
   se ainda existir um Ledger vivo para o item, ou documentada de forma equivalente para trabalho
   pós-EPIC-33)?
3. **Decidir se vale a pena reconciliar agora** — nem todo drift precisa de ação imediata; muitos
   itens documentados durante a EPIC 33 permanecem como registro histórico deliberadamente não
   corrigido (CLAUDE.md §2.5: "ADRs são registros históricos imutáveis").
4. Se a decisão for reconciliar, seguir a Seção 5 (produção → Lab) ou uma extração pontual normal,
   conforme a direção do drift.

## 7. Coordenadas cruzadas (registro imutável desta Sprint)

| Coordenada | Valor |
|---|---|
| Baseline de produção fixa | `DS-Asyst/BeeDay@acce26a` (Sprint 33.1, imutável) |
| `DS-Asyst/BeeDay` — `hmg` no momento desta Sprint | `90eb52cdaa48a62b75937b9f4694465d9fae0488` (instantâneo informativo, não uma baseline fixada — `BeeDay:hmg` continua avançando normalmente fora do escopo da EPIC 33; consultar `git log origin/hmg -1` para o valor atual) |
| Registro de migração de organização | `docs/epics/33-ds-assyst-frontend-lab/README.md` §6 — `tiagoarrigoni/BeeDay` → `DS-Asyst/BeeDay`, confirmado operacionalmente (PR #382, HMG Deployment/Verification verdes, 2026-08-27) |
| `DS-Asyst/beeday-frontend-lab` — `hmg` (candidato aprovado) | `bdcbea9ee5381611f41201384c27e5be423edc47` |
| `DS-Asyst/beeday-frontend-lab` — `prd` (baseline promovida) | `923bee30ec1eb5246f1d86ffff2c336afacbf8d4` (merge commit da PR #13, mesmo conteúdo de `bdcbea9`) |
| Tag da baseline aprovada | `v1.0.0-lab-baseline` → `923bee3` |
| Relatório de paridade | [`04-baseline-parity-report.md`](04-baseline-parity-report.md) |
| Ledger canônico | [`03-frontend-inventory-ledger.md`](03-frontend-inventory-ledger.md) — 109/109 itens em estado terminal |
| Template de promoção futura | Seção 2 deste documento (ciclo branch → PR → `hmg` → PR → `prd`) — reutilizável sem alteração para qualquer promoção futura do Lab |

## 8. Disposição

Workflow, contrato de promoção e reconciliação de drift documentados e consolidados. `Lab hmg → prd`
promovido (PR #13, merge `923bee3`) sob aprovação visual explícita do proprietário para o SHA
`bdcbea9`. Nenhuma sincronização automática foi introduzida; nenhuma promoção de `DS-Asyst/BeeDay`
para `main`/`prd` foi realizada ou é autorizada por esta Sprint.

## 9. CORREÇÃO (Sprint 33.18-R) — status da promoção da Seção 7/8 revogado

**A promoção `923bee3`/tag `v1.0.0-lab-baseline` registrada acima não é mais tratada como baseline de
paridade visual confiável.** O proprietário rodou o Lab localmente após esta Sprint e observou
diferença visual substancial em relação à produção — detalhe completo em
[`README.md`](README.md) §24 e [`04-baseline-parity-report.md`](04-baseline-parity-report.md) §9. A
tag **não foi deletada nem reescrita**; permanece exatamente como estava, como registro histórico do
que foi promovido e quando — apenas não deve mais ser tratada como a baseline de paridade atual
enquanto o proprietário não decidir mantê-la, substituí-la ou removê-la.

A causa raiz (composição de CSS incompleta) foi corrigida na Sprint 33.18-R (Lab PR #14, merge
`5df4f24` em `hmg`) seguindo exatamente o ciclo branch → PR → `hmg` desta Seção 2 — **sem** nova
promoção `hmg → prd`, conforme instrução explícita do proprietário. O novo SHA candidato
(`5df4f24`) aguarda revisão visual real antes de qualquer promoção futura.
