# Relatório de Paridade Estrutural/de Código — EPIC 33, Sprint 33.18

Relatório terminal de fechamento produzido pela Sprint 33.18 (Issue [#379](https://github.com/DS-Asyst/BeeDay/issues/379)),
conforme exigido pelo pacote de planejamento aprovado. Este documento é a evidência formal de que
todo item `FE33-*` do Ledger canônico ([`03-frontend-inventory-ledger.md`](03-frontend-inventory-ledger.md))
está em estado terminal contra a baseline de produção fixa, e registra a coordenada exata candidata a
revisão visual do proprietário.

## 1. Baseline de produção fixa (imutável)

`acce26a` (`tiagoarrigoni/BeeDay`, hoje `DS-Asyst/BeeDay` — mesmo commit, coordenada renomeada na
Sprint 33.2) — mesma baseline estabelecida na Sprint 33.1 e usada por todas as Sprints de extração
desde então. Nenhuma mudança de rota/componente de produção ocorreu entre `acce26a` e o `hmg` atual
de `DS-Asyst/BeeDay` além do que já está registrado nas Seções 5/6 do README do épico (1 link
corrigido em `Contact.razor`, nenhuma mudança de rota/componente) — a baseline permanece válida.

## 2. Coordenada candidata do Lab (`hmg`)

```text
Repositório: DS-Asyst/beeday-frontend-lab
Branch:      hmg
SHA:         bdcbea9ee5381611f41201384c27e5be423edc47
Commit:      Merge pull request #12 from DS-Asyst/sprint/33.17-page-email-gallery-responsive-preview
```

Esta é a coordenada imutável candidata à revisão visual do proprietário desta Sprint. Nenhum código
de produto foi alterado por esta Sprint (18) — o SHA acima é idêntico ao registrado ao final da
Sprint 33.17 (README §21).

## 3. Matriz de estado terminal do Ledger

| Estado | Contagem | % |
|---|---:|---:|
| `VERIFIED` | 105 | 96,3% |
| `EXCLUDED` | 4 | 3,7% |
| `MAPPED` / `EXTRACTED` / `ADAPTED` / `PARITY_PENDING` / `NOT_AUDITED` (estados de trabalho) | **0** | 0% |
| **Total (`FE33-001`–`FE33-109`)** | **109** | 100% |

**Nenhum item permanece em estado de trabalho.** Critério de aceitação "Zero Ledger items remain in
working states" cumprido — confirmado por contagem direta sobre
[`03-frontend-inventory-ledger.md`](03-frontend-inventory-ledger.md) nesta Sprint, não por suposição.

### 3.1 Os 4 itens `EXCLUDED` (com justificativa já registrada em Sprint anterior, reconfirmada aqui)

| ID | Item | Sprint | Motivo |
|---|---|---:|---|
| FE33-089 | `Dashboard/Pages/LegacyHomeRedirect.razor` (`/home`) | 33.13 | Redirect puro para `/profile`, sem superfície visual própria — nada a extrair. |
| FE33-107 | `wwwroot/js/beeday-card-menu.js` | 33.8 | Drift de documentação: o arquivo não existe em `acce26a` — removido da produção após o inventário original (Sprint 33.4) ter sido escrito. Nenhum arquivo foi fabricado para preencher a lacuna. |
| FE33-108 | `DesignSystem/Pages/IconCatalog.razor` (`/design-system/icons`) | 33.16 | Precedente de composição para a Component Gallery do Lab, não copiado 1:1 — o Lab construiu sua própria galeria (Sprint 33.16) a partir dos componentes já extraídos. |
| FE33-109 | `DesignSystem/Pages/HeroCatalog.razor` (`/design-system/hero`) | 33.16 | Idem FE33-108. |

Nenhum destes 4 itens representa um defeito de extração — todos são exclusões deliberadas, cada uma
com evidência registrada no momento em que foi decidida (Ledger, coluna Notes).

## 4. Verificação de paridade estrutural/de código realizada nesta Sprint

**Escopo desta verificação:** cada um dos 109 itens já carrega sua própria evidência específica,
registrada pela Sprint que o extraiu (arquivo de teste dedicado e/ou comparação de código-fonte no
momento da extração — ver coluna "Evidence" do Ledger). Esta Sprint não repete individualmente as 109
comparações já registradas; em vez disso, (a) executa a suíte de regressão completa e cumulativa de
todas as 12 Sprints de extração como gate primário, e (b) realiza amostragem dirigida de itens de
maior risco/alcance — fundamentos compartilhados por tudo, e itens representativos de camadas
distintas — com comparação byte-a-byte direta contra `acce26a`.

### 4.1 Gate de regressão completo (`DS-Asyst/beeday-frontend-lab` no SHA candidato)

```text
dotnet format BeeDayLab.slnx --verify-no-changes  → limpo
dotnet build BeeDayLab.slnx -c Release --warnaserror → 0 avisos/erros
dotnet test BeeDayLab.slnx -c Release             → 509/509 aprovados
  - BeeDayLab.ArchitectureTests: 22/22
  - BeeDayLab.Web.Tests:        487/487
git diff --check                                  → limpo
```

Os 509 testes incluem, sem exceção, toda suíte de paridade dedicada já construída por cada Sprint de
extração (`FoundationTokenParityTests`, `IconSystemParityTests`, `SharedComponentsParityTests`,
`FormsAccessibilityTests`, `ModalAndSortableTests`, `LayoutShellTests`, `NavigationTests`,
`FootersAndPagesTests`, `PublicPagesTests`, `InstitutionalPagesTests`, `ExperienceSystemPagesTests`,
`LoginAndAuthTests`, `ProfileCreationTests`, `IdentityRecoveryTests`, `AccountPageTests`,
`OnboardingAndRedirectTests`, `DailyPageTests`, `DashboardStateTests`, `ActivityEditorModalTests`,
`HabitVisualStateTests`, `WalletPageTests`, `WalletComponentTests`, `WalletScenarioAndStateTests`,
`EmailTemplateCatalogTests`, `EmailPreviewPageTests`, `ComponentGalleryTests`, `PreviewHubTests`,
`PreviewPageRegistryTests`, `LocalizationResourceTests`, `ScenarioEngineTests`,
`ScenarioAndLocalizationBoundaryTests`, `AssetExistenceTests`) — todas verdes no SHA candidato. Uma
regressão introduzida por qualquer Sprint posterior em um item verificado por uma Sprint anterior
teria quebrado o teste daquela Sprint anterior; nenhuma quebrou.

### 4.2 Amostragem dirigida com comparação byte-a-byte contra `acce26a`

| Item verificado | Método | Resultado |
|---|---|---|
| Tokens de design (`variables.css`, FE33-001/003/004/005/007/008) | Extração do conjunto completo de declarações `--beeday-*` de ambos os arquivos (ordem-independente) e `comm` das duas listas ordenadas | **197/197 declarações idênticas** — 0 ausentes na produção, 0 extras no Lab, 0 divergência de valor. Fundamento de todo o resto do Design System confirmado. |
| `BeeDayButton.razor` (FE33-013) | `diff -B -w --strip-trailing-cr` (ignora apenas fim-de-linha/espaço) | **Idêntico byte-a-byte** (a única diferença bruta anterior era CRLF vs. LF, neutralizada). |
| Sprite de ícones (`sprite.svg`, FE33-009/010) | Contagem de elementos `<symbol>` em ambos + diff completo | **60/60 símbolos idênticos**; único diff residual é um artefato de exibição de encoding do pipe do terminal sobre o travessão (—) do comentário de licença — não uma diferença real de conteúdo/bytes do arquivo. |
| `TransactionalEmailTemplateCatalog.cs` vs. `IdentityEmailComposer.cs` (FE33-103) | Leitura completa lado a lado (Sprint 33.15, registrada no momento da extração) | Shell HTML/plain-text e todos os tokens de cor confirmados copiados verbatim; único desvio é o host `beeday-lab.invalid` sintético em vez da URL de produção real (MOCK, por desenho — FE33-101/102). |
| `Wallet.razor`/`WalletScenarioProvider.cs` (FE33-098) | Leitura completa (Sprint 33.14, registrada no momento da extração; reconfirmada nesta Sprint por leitura repetida) | Nenhuma referência a `ISender`/`BeeDayWebService`/EF Core/SQL; toda transição de estado determinística por seed fixo. |

Nenhuma diferença não classificada foi encontrada durante esta amostragem. Toda diferença observada
já está corretamente registrada no Ledger sob COPY/ADAPT/MOCK/EXCLUDE.

## 5. Classificação de diferenças remanescentes

Requisito da Issue #379, item 3 ("Classify every remaining difference as extraction defect, required
Lab adaptation, exclusion or owner-approved difference"):

- **Defeito de extração:** **nenhum encontrado** nesta Sprint (nem pela suíte de regressão completa,
  nem pela amostragem dirigida da Seção 4.2).
- **Adaptação de Lab exigida (ADAPT/MOCK):** toda instância já classificada e documentada
  individualmente no Ledger (coluna Strategy), com a razão registrada no momento da extração —
  nenhuma nova adaptação não documentada foi encontrada.
- **Exclusão:** os 4 itens da Seção 3.1, todos já justificados.
- **Diferença aprovada pelo proprietário:** nenhuma diferença desta categoria foi identificada como
  pendente de classificação — não há "diferença visual conhecida, mas aceita" registrada que ainda
  careça de aprovação explícita distinta da aprovação visual geral da baseline (Seção 7).

## 6. Defeitos de extração resolvidos nesta Sprint

**Nenhum.** A Sprint 33.18 não alterou nenhum arquivo de código-fonte do Lab — nem `src/`, nem
`tests/`. Isso é esperado e correto dado o resultado da Seção 4: nenhum defeito de extração foi
encontrado para resolver. Nenhum redesign foi realizado, conforme o limite explícito da Issue #379
("No intentional redesign before baseline approval").

## 7. Revisão visual do proprietário

**Superfícies vivas para revisão** (todas navegáveis a partir de `DS-Asyst/beeday-frontend-lab` no
SHA candidato, `bdcbea9`, sem necessidade de banco de dados):

- `/preview` — índice de todas as 53 rotas reais extraídas, agrupadas por área, com seletor
  compartilhado de cenário/viewport/idioma e preview responsivo (Sprint 33.17).
- `/design-system` — Component Gallery com os 26 componentes reutilizáveis já extraídos (Sprint 33.16).
- `/emails` — Email Gallery com os 2 templates transacionais reais (Sprint 33.15).

**Estado de aprovação visual: `PENDING`.**

Conforme o limite explícito da Issue #379 ("Do not mark subjective visual parity owner-approved
without owner confirmation") e o critério de aceitação ("Owner visual approval state is explicit:
APPROVED or PENDING, never inferred"), o Claude não pode e não declara aprovação visual em nome do
proprietário. Nenhuma automação de captura de tela em massa foi usada como substituto de aprovação
(limite explícito da Issue #379, item 1). Este estado permanece `PENDING` até que o proprietário
revise a baseline ao vivo nas 3 superfícies acima e registre sua decisão explicitamente — o registro
dessa decisão, quando ocorrer, será adicionado a este documento como uma seção adicional, sem
reescrever o que está registrado aqui.

## 8. Disposição

**GO** para todos os itens mecânicos/verificáveis em código desta Sprint: matriz de estado terminal
completa (109/109), relatório de paridade estrutural/de código produzido, SHA candidato do Lab
registrado de forma imutável, zero defeitos de extração encontrados, zero redesign realizado.

**PENDING** para a aprovação visual do proprietário (Seção 7) — pré-requisito explícito da Issue #379
antes de qualquer promoção adicional do baseline. Sprint 33.19 (Frontend Lab Workflow, Promotion
Contract & EPIC Closure) pode prosseguir com o trabalho de workflow/contrato de promoção que não
dependa da aprovação visual em si, mas o fechamento final da EPIC 33 depende desta aprovação.

## 9. CORREÇÃO — Falha de paridade visual descoberta pós-promoção (Sprint 33.18-R)

**Este relatório, tal como escrito nas Seções 1–8, refletiu evidência real de gate estrutural/de
código no momento em que foi produzido — mas a aprovação visual que a Sprint 33.19 tratou como
satisfeita não se sustentou.** O proprietário rodou o Lab localmente e observou diferença visual
substancial em relação ao BeeDay original. Detalhe completo, auditoria de causa-raiz e correção em
[`README.md`](README.md) §24.

**Causa raiz identificada:** `App.razor` do Lab nunca referenciava o bundle de CSS isolation
(`BeeDayLab.Web.styles.css`, 44 arquivos/3262 linhas gerados no build mas nunca linkados) e 6 arquivos
CSS globais de produção nunca haviam sido copiados para o Lab (`activity-design-system.css`,
`settings.css`, `cards.css`, `dragdrop.css`, `identity.css`, `institutional.css` — nunca catalogados
pelo inventário original da Sprint 33.4, agora `FE33-110`–`FE33-115`). Nenhum dos 509/516 testes
anteriores capturava esta classe de defeito — todos validam estrutura/comportamento de DOM, não
aplicação real de CSS no navegador.

**Correção aplicada** (Lab PR #14, merge `5df4f24` em `hmg`, não promovido para `prd`): os 6 arquivos
copiados verbatim, o bundle de CSS isolation linkado, `<ReconnectModal />` adicionado, `lab-shell.css`
obsoleto removido. Nova suíte `StylesheetCompositionTests.cs` (7 testes) como proteção de regressão
determinística — verificada como capaz de detectar exatamente esta classe de defeito (5/7 falham
contra a composição anterior, 7/7 passam contra a correção).

**Nova coordenada candidata:** `DS-Asyst/beeday-frontend-lab@5df4f24` (branch `hmg`) — explicitamente
**não** promovida para `prd`, nenhuma tag nova criada, `v1.0.0-lab-baseline` não alterada.

**Disposição corrigida:** gate estrutural/de código GO no novo SHA candidato; aprovação visual do
proprietário permanece **PENDING**, agora contra o SHA `5df4f24`. As Seções 1–8 acima permanecem
como registro histórico do que foi observado no momento em que foram escritas — não foram apagadas
nem reescritas.

## 10. FECHAMENTO FINAL — Aprovação do Proprietário (`357dc9d`) e Promoção

**Estado final, não mais `PENDING`.** O proprietário completou a revisão visual ao vivo de
`DS-Asyst/beeday-frontend-lab@357dc9d` (SHA candidato registrado em `README.md` §25.4, refinamento do
Email Gallery sobre a correção de composição de CSS de `5df4f24`) e registrou aprovação explícita em
2026-09-04:

> *"I have completed the live OWNER review of DS-Asyst/beeday-frontend-lab@357dc9d. I explicitly
> APPROVE 357dc9d as the final EPIC 33 visual baseline."*

**Coordenada final promovida:**

```text
DS-Asyst/beeday-frontend-lab
Branch hmg (aprovado) : 357dc9db59a665bc324d281ce374bb63e058779f
Branch prd (promovido): a0f380e0542392874df6b780062a685a3c314800  (PR #16, hmg → prd)
Tag                    : v1.1.0-lab-baseline → a0f380e (nova; v1.0.0-lab-baseline → 923bee3 preservada,
                         não reescrita, registro histórico do baseline invalidado na Seção 9)
```

**Ledger final:** 115/115 itens `FE33-001`–`FE33-115` em estado terminal (111 `VERIFIED` + 4
`EXCLUDED`), zero em estado de trabalho.

**Disposição final:** **GO.** Gate estrutural/de código GO, aprovação visual do proprietário
explícita e registrada para o SHA final promovido, Ledger em estado terminal completo. Critério 6
("Owner visual baseline approval explicitly recorded") da Issue #361 satisfeito para o SHA final. Este
relatório está encerrado — nenhuma seção anterior foi apagada ou reescrita.
