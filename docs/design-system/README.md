# Design System

Documentação do Design System Blazor do BeeDay — reconstruída por completo na Sprint 16.8 a partir
exclusivamente do código atual (`src/BeeDay.Web/Components/DesignSystem/`,
`src/BeeDay.Web/wwwroot/css/`, `src/BeeDay.Web/wwwroot/js/`, `tests/BeeDay.Web.Tests/`). Nenhuma
afirmação vem de `docs/history/` ou de sprints anteriores sem reverificação direta no código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes consultadas".

**Última verificação:** 2026-08-11 (Sprint 20.3) — contagem de folhas de CSS globais corrigida de
20 para 19 (`css/cursors.css` removido — remoção estrutural do cursor gráfico personalizado, EPIC
20). Verificação anterior: 2026-08-10 (Sprint 18.7) — contagem corrigida de 19 para 20.

## Objetivo

Dar a qualquer pessoa que for construir uma tela nova em `BeeDay.Web` uma resposta direta a três
perguntas: quais tokens visuais existem, quais componentes reutilizáveis já resolvem o problema, e
como esses componentes se conectam a JS interop/CSS quando precisam medir o DOM real. O Design
System não é uma biblioteca separada publicada — é a pasta `Components/DesignSystem/` dentro do
próprio `BeeDay.Web`, consumida diretamente pelos componentes de Feature (ver
[`docs/web/README.md`](../web/README.md)).

## Escopo

Dentro: os 26 componentes reutilizáveis sob `Components/DesignSystem/` (Buttons, Cards, Forms,
Feedback, Icons, Layout, Modals, Attributes, Text), as 19 folhas de CSS sob `wwwroot/css/`, os 3
módulos de interop JS que servem componentes do Design System, e o Pixel Icon System
(`PixelIconRegistry`, sprite único). Fora: componentes de Feature (`Components/Features/*` — ver
[`docs/web/04-feature-components.md`](../web/04-feature-components.md)), layouts de página
(`Components/Layout/*` — ver [`docs/web/03-layouts.md`](../web/03-layouts.md)), e
`BeeDaySortable` (`Components/Behaviors/DragDrop/`, comportamento de reordenação usado pelo
Dashboard — documentado como interop em [`02-components.md`](02-components.md) §9 mas fisicamente
fora desta pasta).

## Estrutura

```text
docs/design-system/
├── README.md                    este documento
├── 01-foundations.md            cores, tokens, tipografia, espaçamento, raio, elevação, breakpoints, grid, z-index
├── 02-components.md             os 26 componentes: objetivo, parâmetros, estados, eventos, JS interop, consumidores
├── 03-icons.md                  Pixel Icon System: sprite, registry, bibliotecas de origem, nomenclatura
└── 04-forms.md                  os 6 componentes de formulário: inputs, validação, estados, botões de ação
```

`docs/ux/` (fluxos, acessibilidade, responsividade) é uma pasta irmã, não uma subpasta desta —
ver [`docs/ux/README.md`](../ux/README.md).

## Relação com `BeeDay.Web`

O Design System não é um projeto ou pacote separado: é uma árvore de componentes dentro do próprio
`BeeDay.Web.csproj`, sem nenhuma fronteira de assembly. Toda regra de import é por convenção
(namespace `BeeDay.Web.Components.DesignSystem.*`, `@using` em `_Imports.razor`), não por
compilação separada. Os componentes de Feature (`Dashboard`, `Wallets`, `Habits`, etc.) importam e
compõem os componentes daqui livremente — não existe hoje nenhum teste arquitetural que impeça uma
Feature de referenciar Infrastructure diretamente de dentro de um componente Design System (o
inverso do que `PersistenceContractBoundaryTests` faz para `Application`/`Infrastructure`, ver
[`docs/testing/01-testing-strategy.md`](../testing/01-testing-strategy.md) §8) — nenhum componente
observado nesta auditoria viola isso na prática, mas a fronteira não é imposta por código.

## Contagem de componentes (corrige contagem anterior)

26 componentes reutilizáveis (`.razor` com `@code`/`.razor.cs` próprio, excluindo páginas de
catálogo e enums/modelos de suporte):

| Pasta | Componentes |
|---|---|
| `Attributes/` | `ActivityAttributeBadge`, `ActivityAttributeSelect` |
| `Buttons/` | `BeeDayButton` |
| `Cards/` | `BeeDayCard`, `BeeDayCardMenu` |
| `Feedback/` | `BeeDayConfirmDialog`, `BeeDayDashboardSkeleton`, `BeeDayEmptyState`, `BeeDayLoading`, `BeeDaySkeleton`, `BeeDayToastHost` |
| `Forms/` | `BeeDayCheckbox`, `BeeDayDateInput`, `BeeDayInput`, `BeeDaySelect`, `BeeDayTextArea`, `BeeDayValidationMessage` |
| `Icons/` | `PixelIcon` |
| `Layout/` | `BeeDayHero`, `BeeDayPageHeader`, `BeeDaySectionHeader`, `BeeDaySettingsForm`, `BeeDaySettingsSection` |
| `Modals/` | `EditorModalShell` |
| `Text/` | `BeeDayBrand`, `SearchHighlight` |

O `README.md` anterior desta pasta (placeholder, nunca publicado) citava "24 componentes
confirmados" — número não verificado nesta auditoria; a contagem atual é 26.
`DesignSystem/Pages/{IconCatalog,HeroCatalog}.razor` são páginas roteáveis de catálogo visual, não
componentes reutilizáveis — documentadas em
[`docs/web/02-routing-and-pages.md`](../web/02-routing-and-pages.md) §6, não contadas aqui.

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-foundations.md`](01-foundations.md) | Cores, tokens, tipografia, espaçamento, border-radius, elevação/sombra, breakpoints, grid, z-index, movimento |
| [`02-components.md`](02-components.md) | Os 26 componentes reutilizáveis — parâmetros, estados, eventos, dependências, JS interop, consumidores |
| [`03-icons.md`](03-icons.md) | Pixel Icon System — sprite, `PixelIconRegistry`, bibliotecas de origem, estratégia, nomenclatura |
| [`04-forms.md`](04-forms.md) | Os 6 componentes de formulário — contrato comum, validação, estados, botões |

## Ordem de leitura recomendada

1. `01-foundations.md` — os tokens que todo o resto consome.
2. `02-components.md` — o catálogo completo.
3. `03-icons.md` e `04-forms.md` — os dois subsistemas mais usados, em detalhe.
4. [`docs/ux/README.md`](../ux/README.md) — como esses componentes devem ser usados, não apenas o que fazem.

## Achados relevantes (reportados, não corrigidos)

- Nenhum token de breakpoint compartilhado existe (`variables.css` não define nenhuma
  `--beeday-breakpoint-*`) — 16 valores de corte distintos, hardcoded por arquivo, alguns
  inconsistentes para o mesmo propósito visual. Ver [`01-foundations.md`](01-foundations.md) §9 e
  [`docs/ux/03-responsive.md`](../ux/03-responsive.md).
- `wwwroot/css/feedback.css:20` — `animation: delete-confirmation-enter var(--beeday-transition-normal)-out;`
  concatena `-out` diretamente após um `var()`, o que é sintaxe CSS inválida (não é possível
  interpolar texto após uma função `var()` em um valor de propriedade). O navegador descarta a
  declaração `animation` inteira nessa regra — `.delete-confirmation` provavelmente não recebe a
  animação de entrada `delete-confirmation-enter` declarada via `@keyframes` mais abaixo no mesmo
  arquivo. Não corrigido (CSS fora do escopo desta Sprint).
- `wwwroot/css/cards.css` (576 linhas) e `wwwroot/css/wallet.css` (699 linhas) acumulam os mesmos
  seletores (`.activity-card`, `.habit-card__body`, `.wallet-transaction-card`, etc.) redeclarados
  3 a 5 vezes em blocos sucessivos marcados por comentário `/* Sprint N ... */`, em vez de
  consolidados em uma única declaração — inclui uso de `!important` para uma declaração posterior
  vencer uma anterior (`cards.css` linha ~251). Funciona (o CSS em cascata resolve corretamente),
  mas dificulta encontrar o valor "vigente" de qualquer propriedade sem ler o arquivo inteiro.
- Comentários-fonte em `wwwroot/css/pixel-nes.css` e `wwwroot/css/design-system.css` referenciam
  `docs/design-system/foundations.md` e `docs/design-system/components.md` (sem prefixo numérico) —
  os arquivos desta Sprint usam `01-`/`02-` por convenção (`docs/CONVENTIONS.md` §2). Os comentários
  no CSS não foram alterados (fora do escopo desta Sprint); os nomes de arquivo reais são
  `01-foundations.md`/`02-components.md`.
- A Sprint 16.7 (`docs/web/05-design-system-integration.md`) registrou a existência do bundle
  `BeeDay.Web.styles.css` (CSS isolation do Blazor) mas não enumerou seus arquivos-fonte. Esta
  Sprint encontrou **30 arquivos `*.razor.css`** (3.886 linhas — quase o mesmo volume que as 19
  folhas globais de `wwwroot/css/`, 3.939 linhas) e os incorporou à auditoria — ver
  [`01-foundations.md`](01-foundations.md) §9. Um deles (`Layout/TopNavigation.razor.css`) declara
  uma cor de marca (`#5b1095`) fora do sistema de tokens `--beeday-color-*`.
