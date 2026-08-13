# Icon System

**Fonte da verdade:** `Components/DesignSystem/Icons`, `design/icons/catalog/icon-mapping.csv` e
`wwwroot/icons/sprite.svg`. **Última verificação:** 2026-08-12 (Sprint 21.8).

## Contrato oficial

`BeeDayIcon` é a única primitive de ícones do produto. Consumidores escolhem um nome semântico
tipado (`BeeDayIconName`), tamanho, cor e comportamento acessível; não importam SVGs nem conhecem
a biblioteca de origem. `BeeDayIconRegistry` resolve cada nome para um `symbolId`, asset gerado,
categoria e label padrão. Um nome inválido usa `Warning` como fallback.

```razor
<BeeDayIcon Name="BeeDayIconName.Search" Size="BeeDayIconSize.Medium" />
<BeeDayIcon Name="BeeDayIconName.Warning"
            Decorative="false"
            Label="Warning status" />
```

O nome anterior `PixelIcon` foi removido, sem alias: todos os consumidores internos foram migrados
e manter duas APIs oficiais criaria dívida sem benefício de compatibilidade externa.

## Renderização e estilo

O componente renderiza `<svg><use href="/icons/sprite.svg#{symbolId}"></use></svg>`. O sprite é
estático e local: não há pacote JavaScript, fonte de ícones ou requisição a CDN em runtime.
Ícones funcionais usam Lucide com traço de 2px, extremidades e junções arredondadas, `fill="none"`
e `stroke="currentColor"`. Marcas sociais mantêm seus vetores próprios e usam `fill: currentColor`.
Assim, cor e estados continuam controlados pelos tokens/classes do BeeDay.

A escala permanece deliberadamente curta e baseada nos usos reais:

| Token | Tamanho |
|---|---:|
| `ExtraSmall` | 12px |
| `Small` | 16px |
| `Medium` | 20px (default) |
| `Large` | 24px |
| `ExtraLarge` | 32px |

As cores oficiais são `Current`, `Primary`, `Secondary`, `Muted`, `Success`, `Warning`, `Danger`
e `Information`. Prefira `Current` quando o ícone deve acompanhar texto ou estado do controle.

## Acessibilidade

- Ícones decorativos são o default: `aria-hidden="true"` e `focusable="false"`.
- Ícones informativos exigem `Decorative="false"` e um `Label` não vazio; o componente valida isso
  durante a renderização e expõe `role="img"`/`aria-label`.
- Botões icon-only continuam responsáveis por um nome acessível no próprio controle.
- Não use somente um ícone ou somente cor para comunicar um estado crítico.

## Fontes, geração e manutenção

`design/icons/catalog/icon-mapping.csv` é o manifesto auditável. Os SVGs Lucide selecionados ficam
em `design/icons/source/lucide/`, com licença e atribuição locais. Devicon e assets oficiais são
reservados às seis marcas sociais. Execute `scripts/New-IconSprite.ps1` após alterar o catálogo:
ele valida o mapeamento, limpa os outputs gerados e recria os SVGs publicados e o sprite.

Para adicionar um ícone:

1. confirme uma necessidade semântica que não esteja coberta;
2. adicione o valor ao enum e a definição ao registry;
3. versione o SVG fonte e registre origem/licença no CSV;
4. regenere o sprite e rode os testes de contrato;
5. confira `/design-system/icons` em tema claro/escuro e nos tamanhos usados.

Não crie nomes por página (`HomeBlueIcon`), variantes puramente visuais ou imports diretos. Use
nomes de intenção como `Search`, `Wallet` e `ValidationError`.

## Decisões da Sprint 21.8

A arquitetura tipada e o sprite existentes eram sólidos e foram preservados. O problema era a
linguagem visual pixel/filled e o nome público. Os 54 ícones funcionais foram migrados para um
conjunto Lucide outline coerente; os seis ícones de marca foram preservados. `Streak` foi removido
porque não existe capacidade de domínio correspondente; usos ilustrativos passaram a `Habit`.
Material Symbols e o catálogo Streamline Pixel sem consumidores foram removidos após auditoria.

Nenhuma mudança foi feita em Domain, Application, Infrastructure, regras de produto ou layout.
