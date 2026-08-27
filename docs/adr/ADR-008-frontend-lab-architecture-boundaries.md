# ADR-008 — beeday Frontend Lab: Arquitetura, Fronteiras e Contrato de Promoção

**Status:** Aceito
**Data:** 2026-08-27

## Contexto

A EPIC 33 (Issue [#361](https://github.com/DS-Asyst/BeeDay/issues/361), pacote de planejamento
aprovado pelo proprietário) cria `DS-Asyst/beeday-frontend-lab`, um repositório separado e
independentemente executável para desenvolvimento visual sem banco de dados, cuja finalidade é
reproduzir fielmente a superfície de frontend atual do `BeeDay.Web` (Design System, layouts,
páginas, e-mails transacionais) como um workspace de experimentação visual controlado, nunca como
um segundo backend do produto.

Este ADR formaliza a decisão arquitetural exigida pela Sprint 33.3 (Issue
[#364](https://github.com/DS-Asyst/BeeDay/issues/364)) antes que qualquer código seja extraído
(Sprint 33.6 em diante), fixando os limites, o mecanismo de extração e o contrato de
promoção/drift que todas as Sprints seguintes da EPIC 33 devem obedecer.

**Fatos verificados sobre a arquitetura atual do `BeeDay.Web`** (fonte:
`src/BeeDay.Web/BeeDay.Web.csproj`, `docs/web/README.md`, `docs/design-system/README.md`,
2026-08-27):

- `BeeDay.Web` é uma aplicação Blazor Server única (.NET 10, `Microsoft.NET.Sdk.Web`), composition
  root do sistema, com `ProjectReference` para `BeeDay.Application`, `BeeDay.Domain` e
  `BeeDay.Infrastructure`. **Não existe** nenhum projeto/assembly separado que hospede apenas o
  Design System ou os componentes de Feature — `Components/DesignSystem/*` e
  `Components/Features/*` são pastas por convenção dentro do mesmo `.csproj`, sem fronteira de
  compilação.
- Não existe hoje nenhum teste arquitetural que impeça um componente de `Components/DesignSystem/`
  de referenciar `BeeDay.Infrastructure` diretamente (ao contrário de
  `PersistenceContractBoundaryTests`, que protege a fronteira `Application`/`Infrastructure`).
  Nenhum componente observado viola isso na prática, mas a fronteira não é imposta por código.
- A maioria dos componentes de Feature chama `BeeDay.Application` através da fachada
  `BeeDayWebService` (`ISender.Send(...)`); `Wallet.razor` e 5 páginas de `Features/Identity/Pages/`
  injetam `MediatR.ISender` diretamente — ambos os caminhos acabam em Application/Infrastructure
  reais.
- Já existem duas páginas de catálogo visual roteáveis dentro do próprio `BeeDay.Web`
  (`DesignSystem/Pages/IconCatalog.razor`, `HeroCatalog.razor`) — precedente direto e reutilizável
  como referência de composição para as Sprints 33.16/33.17 (Component Gallery / Page + Email
  Gallery), mas que não são, elas mesmas, extraídas por dependerem do mesmo assembly.

Consequência direta: como não existe um pacote/assembly de Design System publicável ou
referenciável isoladamente, **não há como o Lab consumir o Design System via `ProjectReference` ou
pacote NuGet sem trazer consigo `BeeDay.Application`/`BeeDay.Infrastructure`/`BeeDay.Domain`**
inteiros (violando a exigência de EPIC 33 de que o Lab seja livre de banco de dados/backend por
arquitetura). Extrair um projeto de biblioteca compartilhada a partir do `BeeDay.Web` atual seria,
por si só, uma mudança arquitetural estrutural do produto (nova fronteira de assembly, novo
contrato público) — fora do escopo autorizado pela EPIC 33 (`00_EPIC_33_MASTER_PLAN.md` §3.1,
"não em escopo sem aprovação separada: mudar arquitetura de produto porque o repositório moveu").
Portanto o único mecanismo compatível com o escopo autorizado é **cópia/adaptação de código-fonte
Razor/CSS/JS para o repositório Lab**, nunca reuso binário/de projeto.

## Decisão

### 1. Fronteira do repositório

`DS-Asyst/beeday-frontend-lab` é um repositório e solução .NET completamente separados de
`DS-Asyst/BeeDay`. Não existe `ProjectReference`, pacote NuGet interno, submodule Git ou qualquer
outro mecanismo de compartilhamento binário/de projeto entre os dois repositórios. Todo código
Lab-side é uma cópia ou adaptação de código-fonte, mantida deliberadamente, nunca sincronizada
automaticamente.

### 2. Direção de dependência dentro do Lab

```text
Lab host / gallery
      ↓
Lab pages e layouts
      ↓
Lab shared components
      ↓
Lab presentation models + deterministic scenarios
      ↓
static/local assets apenas
```

Nenhuma camada do Lab depende de `BeeDay.Domain`, `BeeDay.Application`, `BeeDay.Infrastructure`,
EF Core, SQL Server/LocalDB, autenticação real, ou entrega real de e-mail (Resend/SMTP). Ver lista
completa de proibições em `04_MOCK_STATE_POLICY.md` e `02_FRONTEND_LAB_ARCHITECTURE_AND_BOUNDARIES.md`
do pacote de planejamento — este ADR adota ambos os documentos como parte da decisão.

### 3. Stack técnica

Blazor/Razor (mesma stack do produto), confirmada pelos fatos acima — não há evidência de que
outra abordagem seja mais segura ou barata, e usar a mesma stack minimiza a distância entre o
código copiado/adaptado e sua origem, facilitando a comparação de paridade.

### 4. Regra COPY / ADAPT / MOCK / EXCLUDE

| Categoria | Quando aplicar | Exemplos concretos verificados nesta Sprint |
|---|---|---|
| **COPY** | Componente é puramente apresentacional — recebe apenas primitivos/modelos de apresentação como parâmetro, não injeta `BeeDayWebService`/`ISender`/serviço algum. | A maioria de `Components/DesignSystem/*` (`BeeDayButton`, `BeeDayCard`, `BeeDayIcon`, os 6 componentes de `Forms/`, etc.), CSS de `wwwroot/css/`, sprite de ícones. |
| **ADAPT** | Componente real, mas que hoje recebe dado de runtime (injeta `BeeDayWebService`/`ISender` ou parâmetro tipado em Domain/Application) — extrai o contrato de apresentação e substitui a fonte de dado pelo motor de cenários (Sprint 33.10). | `Wallet.razor` e as páginas de `Features/Identity/Pages/` (injeção direta de `ISender`); qualquer componente de `Components/Features/*` que hoje chama `BeeDayWebService`. |
| **MOCK** | O estado visual depende de cálculo de negócio (Domain/Application) — o Lab recebe o valor já resolvido via cenário, nunca recalcula a regra. | Cálculo de XP/nível, saldo agregado de Wallet, contadores de progresso do Dashboard — ver `04_MOCK_STATE_POLICY.md` "Business-rule boundary". |
| **EXCLUDE** | Infraestrutura de execução/host, não superfície visual. | `Program.cs`, `BeeDay.Infrastructure`, `BeeDay.Application`, `BeeDay.Domain`, EF Core, `Services/Authentication/*`, cookie de autenticação real, `BeeDayWebService` (a fachada em si, não o que ela expõe visualmente). |

### 5. Contrato de fonte da verdade

`DS-Asyst/BeeDay` permanece a única fonte da verdade de runtime/negócio/produção, durante e depois
da EPIC 33. `beeday-frontend-lab:hmg` é integração visual ativa; `beeday-frontend-lab:prd` é fonte
visual validada elegível para promoção controlada — nunca produção implantada. O Lab nunca
sobrescreve produção por sincronização automática, em nenhuma direção.

### 6. Contrato de promoção e drift

Adotado integralmente de `05_PROMOTION_AND_DRIFT_CONTRACT.md`: promoção Lab `hmg → prd` exige
aprovação visual explícita do proprietário; integração `beeday-frontend-lab:prd → DS-Asyst/BeeDay`
exige branch de integração a partir do `hmg` mais recente do BeeDay, porta/refatoração deliberada
do delta aprovado (nunca cópia cega arquivo-por-arquivo), preservação de Clean Architecture/
contratos públicos, e o ciclo normal PR → `hmg` → HMG deployment/verificação. Drift entre produção e
Lab é resolvido comparando sempre contra o alvo de produção **atual**, nunca apenas contra a
baseline original da EPIC 33.

### 7. Validação sem banco de dados

O Lab deve compilar, formatar e testar sem SQL Server/LocalDB/EF Core, usando um gate próprio
determinístico (`dotnet format`, `dotnet build --warnaserror`, `dotnet test`, contratos de
código-fonte). Chromium/Playwright é permitido apenas para verificações pequenas e limitadas onde
semântica de navegador realmente importa — nunca como gate autônomo de aprovação visual subjetiva,
que permanece exclusivamente do proprietário.

## Consequências positivas

- O proprietário pode revisar/experimentar visualmente sem subir SQL Server LocalDB, autenticação
  real ou qualquer segredo de produção.
- Nenhuma duplicação de regra de negócio é introduzida — o motor de cenários único (Sprint 33.10)
  impede que cada Sprint de superfície invente sua própria simulação de domínio.
- A ausência de acoplamento binário entre os dois repositórios elimina o risco de uma mudança no
  Lab quebrar o build de produção, ou vice-versa.
- A regra COPY/ADAPT/MOCK/EXCLUDE, ancorada nos fatos reais do `BeeDay.Web` (não em suposição),
  torna auditável qual categoria cada item do Ledger (Sprint 33.4) deveria assumir.

## Consequências negativas

- Manutenção dupla: uma mudança visual real em produção não se propaga automaticamente ao Lab (por
  desenho) — cada Sprint de promoção futura exige porte manual deliberado.
- Sem fronteira de assembly compartilhada, drift de token/CSS entre os dois repositórios só é
  detectável por comparação explícita (parte do contrato de drift), não pelo compilador.
- O Lab reproduz, mas não substitui, os 25 primitives + `BeeDaySortable` do Design System real —
  qualquer novo componente de produção precisa ser portado deliberadamente para o Lab continuar
  representativo.

## Restrições

- Não criar um projeto de biblioteca compartilhada entre `BeeDay.Web` e o Lab durante a EPIC 33 —
  isso seria uma mudança arquitetural estrutural fora do escopo autorizado (ver Contexto acima).
- Não copiar `BeeDay.Domain`, `BeeDay.Application`, `BeeDay.Infrastructure`, EF Core, SQL Server/
  LocalDB, autenticação real, segredos, ou entrega real de e-mail para o Lab, em nenhuma Sprint.
- Não recriar cálculo de regra de negócio (XP, saldo, agregações) dentro do Lab só para produzir um
  valor de exibição — o cenário fornece o valor já resolvido.
- Não introduzir um segundo sistema de tokens/componentes/ícones concorrente — o Lab representa o
  Experience System existente, não um novo.
- Não sincronizar `beeday-frontend-lab:prd → DS-Asyst/BeeDay` automaticamente, em nenhuma
  circunstância.
- Não redesenhar durante a extração (Sprints 33.6–33.17): copiar/representar a verdade atual
  primeiro; divergência intencional só é permitida depois que a baseline inicial for aprovada pelo
  proprietário.

## Referências

- Pacote de planejamento aprovado pelo proprietário, EPIC 33 (
  `EPIC_33_DS_ASSYST_FRONTEND_LAB_REWRITE`): `00_EPIC_33_MASTER_PLAN.md`,
  `02_FRONTEND_LAB_ARCHITECTURE_AND_BOUNDARIES.md`, `04_MOCK_STATE_POLICY.md`,
  `05_PROMOTION_AND_DRIFT_CONTRACT.md` — adotados integralmente por este ADR, com a correção de
  nome de organização `DS-Assyst` → `DS-Asyst` (ver `docs/epics/33-ds-assyst-frontend-lab/README.md`
  §5).
- [ADR-007](ADR-007-in-process-application-contracts.md) — mesma disciplina de "contrato antes de
  implementação", aplicada aqui à fronteira entre dois repositórios em vez de dois assemblies.
- [`docs/web/README.md`](../web/README.md), [`docs/design-system/README.md`](../design-system/README.md)
  — fatos de arquitetura atual usados para fundamentar a Decisão.
- `docs/epics/33-ds-assyst-frontend-lab/README.md` — Ledger operacional da EPIC 33, Sprint 33.3.
- EPIC 33 Issue [#361](https://github.com/DS-Asyst/BeeDay/issues/361), Sprint 33.3 Issue
  [#364](https://github.com/DS-Asyst/BeeDay/issues/364).
