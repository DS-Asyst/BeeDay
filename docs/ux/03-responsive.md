# Responsiveness

**Fonte da verdade:** inventário direto dos 55 stylesheets de produção em `src/BeeDay.Web`,
revalidado em 2026-08-16 pela Sprint 25.7. Este documento registra contratos atuais; histórico de
implementação permanece nos READMEs das EPICs correspondentes.

## 1. Mapa atual

Existem 105 declarações `@media` em 44 arquivos. Destas, 70 são queries de largura de viewport em
33 arquivos; não existe `@container`. Depois de normalizar whitespace e converter `rem` para o
equivalente em 16px, há 26 cortes `max-width` e dois cortes `min-width` (27 valores físicos, pois
1200px aparece nos dois sentidos).

CSS custom properties não funcionam como valor de media feature, portanto o projeto não cria
`--beeday-breakpoint-*`. Um breakpoint compartilhado é um contrato literal documentado e protegido
por teste, não um token runtime artificial.

### Cortes de largura presentes

```text
max-width: 352, 380, 420, 448, 480, 520, 544, 560, 576, 600, 620, 640, 650,
           672, 700, 720, 736, 767.84, 832, 860, 900, 960, 1000, 1100, 1199, 1200px
min-width: 641, 1200px
```

Valores iguais escritos em px/rem ou com whitespace diferente são o mesmo corte físico; a Sprint
25.7 não reformatou CSS feature-local somente para reduzir a contagem textual.

## 2. Contratos estruturais compartilhados

### Shell autenticado — 1200px

O único breakpoint que troca o paradigma completo de navegação é **1200px**:

- `>= 1200px`: `DesktopSidebar` fixa e Workspace deslocado pela sidebar;
- `< 1200px`: `MobileHeader` + `MobileSidebar` overlay e Workspace com largura integral.

O literal `min-width: 1200px` é coordenado em `MainLayout.razor.css`,
`DesktopSidebar.razor.css`, `MobileHeader.razor.css` e `MobileSidebar.razor.css`.
`Dashboard/Pages/Home.razor.css` se alinha ao mesmo limite com o par complementar
`min-width: 1200px` / `max-width: 1199px` para controlar o overflow interno do board. Testes
source-level e E2E cobrem explicitamente 1199px e 1200px.

1024px é tablet/intermediário no shell atual. Qualquer documentação que o descreva como início da
sidebar desktop está obsoleta.

### Public/Design System — 42rem e 40rem

- `42rem` (672px) coordena header/language switcher público e a recomposição de actions nos
  primitives PageHeader/SectionHeader/Hero.
- `40rem` (640px) é o limite compacto recorrente de Auth/Onboarding/Footer/Brand Guidelines e dos
  catálogos públicos.

Esses cortes representam famílias estruturais reutilizadas, mas continuam literais junto ao owner
do componente. Nem todo consumer precisa dos dois; copiar um corte sem a mesma mudança de layout é
proibido.

## 3. Containers e gutters

| Responsabilidade | Owner | Contrato atual |
|---|---|---|
| Gutter público/global | `polish.css` | `--beeday-page-gutter: clamp(1rem, 2.5vw, 2rem)`, reduzido a `.75rem` em 30rem |
| Reading width público | `polish.css` | `--beeday-reading-width: 72rem`; vira `100%` abaixo de 60rem |
| Header/Footer público | `.beeday-container` | `min(100% - 2rem, 1440px)` |
| Shell autenticado | `MainLayout.razor.css` | sidebar 15.5rem; conteúdo usa largura integral restante |
| Home marketing | `Home.razor.css` | inner 72rem; seções full-bleed anulam/reaplicam somente o gutter público |
| Onboarding | `OnboardingLayout.razor.css` | main até 72rem; páginas internas escolhem sua largura focada |
| Brand Guidelines | `TypographyGuidelines.razor.css` | 72rem, cards em uma coluna a 40rem |
| Daily | feature Dashboard | workspace integral; 4 colunas/scroll interno, 2 colunas e 1 coluna por cortes próprios |
| Wallet | `wallet.css` | 1440px/76rem e grid próprios; convergência completa pertence à 25.11 |
| ProjectWorkspace | CSS da feature | 58rem e breakpoints 700/520px; convergência pertence à 25.12 |

`.beeday-main--authenticated` remove o gutter/reading-width global: páginas autenticadas são
responsáveis por padding e largura, evitando que Daily, Wallet e experiências focadas sejam
forçadas ao mesmo container. `--beeday-content-width` e os overrides scoped
`--beeday-reading-width: 48rem`/`--beeday-workspace-width: 100rem` não têm consumers efetivos no
estado atual; permanecem candidatos para o sweep final, não contratos para código novo.

## 4. Classificação por owner

- **SHARED STRUCTURAL:** shell 1200px; public primitives 42rem; família pública compacta 40rem.
- **LEGITIMATE FEATURE LOCAL:** Home marketing 60/52/46/30rem; Daily 1199/900/700/620px; Wallet;
  ProjectWorkspace; Account 720px; activity cards 650px.
- **REDUNDANT / CONSOLIDATE:** grafias duplicadas e blocos repetidos dentro de Wallet/Cards são
  candidatos quando o owner for migrado; não justificam rewrite transversal.
- **DEFER TO FEATURE SPRINT:** Wallet → 25.11; Daily/Project → 25.12; Character/illustration → 25.13.

Igualdade numérica não torna dois breakpoints compartilhados. A mudança de estrutura, o owner e os
consumers precisam coincidir.

## 5. Texto, ilustração e overflow

Headlines públicas usam `clamp()` e podem quebrar; containers flex/grid que recebem texto usam
`min-width: 0`/wrapping quando necessário. Tradução, zoom e fallback de fonte têm prioridade sobre
manter uma linha única. `/brand/typography` é verificada em mobile, tablet e desktop sem clipping.

Home preserva artwork e proporções existentes. Full-bleed só pode escapar do gutter no eixo
horizontal quando a própria seção controla `overflow`; não se transforma em padrão para Product
UI. O documento não pode produzir overflow horizontal. Daily pode usar scroll horizontal interno
na faixa intermediária, sem ampliar o `documentElement`.

## 6. Viewports de regressão

- mobile: 390/430px;
- tablet/intermediário: 768/900/1024/1199px;
- fronteira desktop: 1200px;
- desktop/wide: 1280/1440/1920px.

Os testes representativos verificam navigation mode, drawer, ausência de overflow, Home pública,
`/brand/typography`, shell autenticado e board Daily. Novos layouts devem escolher viewports pela
mudança estrutural real, não por nomes genéricos como “tablet”.
