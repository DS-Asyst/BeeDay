# Iconography

**Fonte da verdade:** verificado diretamente em `src/BeeDay.Web/Components/DesignSystem/Icons/`
(`PixelIcon.razor(.cs)`, `PixelIconRegistry.cs`, `PixelIconDefinition.cs`, `PixelIconName.cs`,
`PixelIconCategory.cs`, `PixelIconSize.cs`, `PixelIconColor.cs`) e `src/BeeDay.Web/wwwroot/icons/`.

**Última verificação:** 2026-08-07.

## 1. Objetivo

Documentar o Pixel Icon System: como um ícone vai do arquivo `.svg` fonte até a tela, o registry
central que faz essa ligação, e a estratégia de nomenclatura.

## 2. Estratégia: sprite único, não arquivos individuais importados

Todo ícone é renderizado como `<svg><use href="/icons/sprite.svg#{symbolId}" /></svg>` —
`PixelIcon.razor` nunca referencia um arquivo `.svg` individual diretamente em runtime; o
mapeamento `PixelIconName → symbolId` é feito por `PixelIconRegistry.Resolve`. Isso significa uma
única requisição de rede para `sprite.svg` cobre todo ícone da aplicação, em vez de uma por ícone —
mas também significa que os arquivos individuais sob `wwwroot/icons/**/*.svg` (listados em `AssetPath`
no registry) não são consumidos diretamente pelo componente; presume-se que `sprite.svg` é gerado a
partir deles por um processo não auditado nesta Sprint (nenhum script de build de sprite foi
encontrado em `src/BeeDay.Web/`).

## 3. `PixelIconRegistry` — o único ponto de mapeamento

```csharp
public sealed record PixelIconDefinition(
    string SymbolId, string AssetPath, PixelIconCategory Category,
    string SemanticName, string? DefaultLabel = null, PixelIconName? Fallback = null);
```

`PixelIconRegistry.Definitions` é um `Dictionary<PixelIconName, PixelIconDefinition>` com
**61 entradas** — uma para cada um dos 61 valores do enum `PixelIconName`, confirmado por contagem
direta (`grep` de declarações do enum vs. entradas `[PixelIconName.X] = Define(...)` do registry:
61 = 61, nenhum órfão em nenhuma das duas direções).

`Resolve(name)` nunca lança — se o nome não existir no dicionário (impossível hoje, já que todo
valor do enum tem entrada, mas o código está escrito para essa garantia poder ser quebrada no
futuro sem crashar), cai para `DefaultFallback = PixelIconName.Warning`. `TryGet` existe como
alternativa não-lançante explícita.

## 4. Categorias

`PixelIconCategory` tem 9 valores: `Actions`, `Activities`, `Attributes`, `Feedback`, `Forms`,
`Navigation`, `Social`, `Statistics`, `System`. Cada uma das 61 definições declara exatamente uma
categoria — usada apenas como metadado (`data-icon-category` no SVG renderizado); não há filtro por
categoria na UI de produto, só no catálogo (`IconCatalog.razor`, ver §7).

## 5. Bibliotecas de origem dos assets

| Pasta em `wwwroot/icons/` | Origem | Ícones |
|---|---|---|
| `material-symbols/` | Google Material Symbols (derivado) | 11 subpastas temáticas (`actions`, `books`, `feedback`, `forms`, `habits`, `navigation`, `profile`, `projects`, `statistics`, `system`, `tasks`) — a maioria das 61 definições |
| `devicon/social/` | Devicon | `facebook.svg`, `github.svg`, `linkedin.svg` |
| `official-brand/social/` | Marcas oficiais (Instagram, X, YouTube) | `instagram.svg`, `x.svg`, `youtube.svg` |

Nenhum arquivo de atribuição de licença foi encontrado para `material-symbols/`, `devicon/` ou
`official-brand/` (diferente de `wwwroot/css/vendor/NES_ATTRIBUTION.md`, que documenta a proveniência
do adapter NES.css usado por `pixel-nes.css` — ver [`02-components.md`](02-components.md) e
`docs/ux/02-accessibility.md`). Não confirmado se isso representa uma lacuna de atribuição ou se as
licenças de origem (Material Symbols é Apache 2.0, não exige atribuição por arquivo) simplesmente
não exigem o mesmo tratamento que o excerto de NES.css (MIT, com atribuição explícita).

## 6. Nomenclatura

- `PixelIconName` (C#, PascalCase) é o único identificador usado por componentes — nunca uma string
  solta.
- `SymbolId` (kebab-case, ex.: `"chevron-down"`, `"recurring-task"`) é o `id` do `<symbol>` dentro
  do sprite — geralmente uma versão kebab-case do nome C#, mas não sempre 1:1: `PixelIconName.Account`
  mapeia para `SymbolId "user"` (não `"account"`) e `AssetPath "material-symbols/profile/user.svg"`
  — o nome semântico do enum (`Account`, papel de navegação) diverge do nome do arquivo fonte
  (`user`, o que o ícone visualmente representa).
- `SemanticName`/`DefaultLabel` (frase em inglês, ex. "Expand", "Previous", "Add") — usados como
  `aria-label` quando `PixelIcon.Decorative="false"` e nenhum `Label` explícito é passado.

## 7. Uso do componente `PixelIcon`

Ver contrato completo de parâmetros em
[`02-components.md`](02-components.md#5-layout) — resumo aqui: `Name` (`EditorRequired`), `Size`
(5 valores: `ExtraSmall` 12px, `Small` 16px, `Medium` 20px — padrão, `Large` 24px, `ExtraLarge`
32px), `Color` (8 valores semânticos: `Current`, `Primary`, `Secondary`, `Muted`, `Success`,
`Warning`, `Danger`, `Information` — resolvidos via classe CSS `pixel-icon--color-{nome}`, não
`style` inline), `Decorative` (padrão `true` → `aria-hidden="true"`), `Label` (obrigatório quando
`Decorative="false"`, senão `PixelIcon.OnParametersSet` lança `InvalidOperationException` —
único componente do Design System que valida acessibilidade em tempo de renderização, não apenas
por convenção documental).

## 8. Catálogo visual — `/design-system/icons`

`DesignSystem/Pages/IconCatalog.razor` (rota `/design-system/icons`, `[Authorize]`, sem restrição
adicional por ambiente) renderiza `PixelIconRegistry.All` — todos os 61 ícones, por categoria,
como referência visual para desenvolvimento. Ver
[`docs/web/02-routing-and-pages.md`](../web/02-routing-and-pages.md) §6.

## 9. Achado

- `docs/web/05-design-system-integration.md` (Sprint 16.7) registra "60 valores de `PixelIconName`"
  — a contagem direta feita nesta Sprint confirma **61**. Discrepância pequena (um a menos),
  provavelmente um erro de contagem manual na Sprint anterior, não uma mudança de código entre as
  duas Sprints (nenhum commit de código-fonte ocorreu entre 16.7 e 16.8). Não corrigido no arquivo
  da Sprint 16.7 — fora do escopo desta Sprint, que documenta exclusivamente Design System/UX.

## 10. Fontes consultadas

- `src/BeeDay.Web/Components/DesignSystem/Icons/PixelIcon.razor`, `PixelIcon.razor.cs`,
  `PixelIconRegistry.cs`, `PixelIconDefinition.cs`, `PixelIconName.cs`, `PixelIconCategory.cs`,
  `PixelIconSize.cs`, `PixelIconColor.cs`.
- `src/BeeDay.Web/wwwroot/icons/` (listagem completa de arquivos, 3 bibliotecas de origem).
- `src/BeeDay.Web/Components/DesignSystem/Pages/IconCatalog.razor`.
- Contagem cruzada: declarações do enum `PixelIconName` vs. entradas `[PixelIconName.X]` no
  registry (61 = 61).
