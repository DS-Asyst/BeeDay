# UX

Fluxos de experiência do usuário, diretrizes de UX, acessibilidade e responsividade do BeeDay —
reconstruída por completo na Sprint 16.8 a partir exclusivamente do código atual
(`src/BeeDay.Web/Components/`, `src/BeeDay.Web/wwwroot/`, `tests/BeeDay.Web.Tests/`,
`tests/BeeDay.E2E.Tests/`). Nenhuma afirmação vem de `docs/history/` ou de sprints anteriores sem
reverificação direta no código.

**Fonte da verdade:** cada documento abaixo declara individualmente as fontes exatas usadas para
validá-lo, na seção final "Fontes consultadas".

## Objetivo

Documentar como o produto deve se comportar do ponto de vista de quem usa — não apenas o que cada
componente faz (isso é [`docs/design-system/`](../design-system/README.md)), mas os padrões
observados de feedback, hierarquia visual, fluxo, estados vazios/carregando/erro, e as garantias
(ou lacunas) de acessibilidade e responsividade que o código atual realmente entrega.

## Escopo

Dentro: convenções de UX observadas no código (não prescrições novas — toda afirmação é "o código
faz X", não "o código deveria fazer X"), acessibilidade (ARIA, teclado, foco, contraste, semântica),
responsividade (breakpoints reais, comportamento por componente). Fora: fluxos de arquitetura
camada-a-camada (isso é [`docs/architecture/05-runtime-flows.md`](../architecture/05-runtime-flows.md),
que continua sendo a fonte para "o que acontece no servidor quando X acontece"); definição de
componente-a-componente (isso é [`docs/design-system/`](../design-system/README.md)).

## Estrutura

```text
docs/ux/
├── README.md               este documento
├── 01-guidelines.md        hierarquia visual, feedback, consistência, fluxo, microinterações,
│                            estados vazios/loading/erro, confirmações
├── 02-accessibility.md     ARIA, teclado, foco, contraste, semântica, leitores de tela
└── 03-responsive.md        breakpoints reais, comportamento por componente, mobile/tablet/desktop
```

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-guidelines.md`](01-guidelines.md) | Padrões de UX observados: hierarquia, feedback, fluxo, microinterações, estados |
| [`02-accessibility.md`](02-accessibility.md) | O que o código garante e o que não garante em acessibilidade |
| [`03-responsive.md`](03-responsive.md) | Todos os breakpoints reais e o comportamento adaptativo por componente |

## Ordem de leitura recomendada

1. [`docs/design-system/README.md`](../design-system/README.md) — os componentes que esses padrões
   consomem.
2. `01-guidelines.md` — os padrões em si.
3. `02-accessibility.md` e `03-responsive.md` — as duas dimensões de qualidade transversal.

## Relação com `docs/architecture/05-runtime-flows.md`

Esse documento continua sendo a referência para o fluxo técnico completo (Web → Application →
Infrastructure → SQL Server) de ações como "criar um Hábito" ou "login". `docs/ux/` não duplica
esses diagramas — descreve o que o usuário vê e quando, não a cadeia de chamadas por trás.
