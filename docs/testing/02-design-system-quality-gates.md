# Design System Quality Gates

**Fonte da verdade:** `DesignSystemContrastTests`, `ResourceCatalogContractTests`,
`AccessibilityQualityTests`, a suíte responsiva Playwright existente e os workflows atuais.

**Última verificação:** 2026-08-16 (Sprint 25.15, EPIC 25).

## Cobertura automática

O gate combina camadas diferentes, porque nenhuma delas isoladamente representa a qualidade da
interface:

- bUnit/source contracts validam tokens, markup, roles, labels, estados e contratos dos componentes;
- `DesignSystemContrastTests` resolve aliases de `variables.css`, calcula luminância relativa e
  protege pares textuais críticos de brand, conteúdo, botões semânticos, informação e foco inverso;
- `ResourceCatalogContractTests` exige paridade entre neutro/`en-US`/`pt-BR`, fallback neutro igual
  ao inglês default, placeholders equivalentes, valores não vazios e `beeday` lowercase em recursos
  visíveis (exceto o contraexemplo didático da guideline);
- `AccessibilityQualityTests` executa axe no Chromium sobre Home, Typography, Login, Daily, Wallet e
  o diálogo canônico de transação, sem exclusões de regras ou seletores;
- a matriz Playwright existente cobre 390–1920 px, overflow horizontal, clipping, conteúdo longo em
  português, fontes carregadas, dimensões de controles e layouts públicos/autenticados.

O pacote test-only `Deque.AxeCore.Playwright` foi adicionado via Central Package Management porque
Playwright fornece o browser e as assertions, mas não um conjunto de regras de acessibilidade. O
pacote `Deque.AxeCore.Commons` tipa o resultado consumido pelo relatório do teste.

## O que um resultado verde não prova

Uma varredura automática limpa não equivale a conformidade WCAG, certificação legal ou validação
por tecnologia assistiva. Permanecem manuais: ordem de leitura compreensível, qualidade dos nomes,
fluxos completos por teclado/leitor de tela, zoom/reflow fora da matriz, percepção de movimento,
contraste dentro de ilustrações raster e qualidade subjetiva de tradução.

Os checks de contraste medem apenas pares que possuem contrato determinístico. Consumers renderizados
são complementados pelo axe; ilustrações não são tratadas como texto de UI.

## Estratégia visual e artefatos de falha

Não há baseline de screenshot versionado. A infraestrutura atual usa um único Chromium, mas depende
de fontes web e não possui ambiente de rasterização/baseline já qualificado; introduzir comparação de
pixels nesta Sprint adicionaria tolerâncias e manutenção sem evidência de determinismo. A alternativa
estável é a combinação de layout computado, bounding boxes, overflow, fonte computada, estrutura,
tokens e axe.

Em falhas E2E, `E2ETestBase` grava screenshot full-page e trace em
`tests/BeeDay.E2E.Tests/bin/<Configuration>/net10.0/e2e-artifacts/`. Como nenhum baseline foi
introduzido, não existe processo de atualização de imagens aprovado; screenshots continuam sendo
diagnóstico, não expectativa.

## Performance de imagens e fontes

As imagens públicas principais possuem dimensões intrínsecas. A imagem do Hero, provável candidata a
LCP por estar acima da dobra, usa `fetchpriority="high"`; composições abaixo da dobra usam
`loading="lazy"` e `decoding="async"`. Não existem variantes responsivas qualificadas das imagens
fonte, portanto nenhuma miniatura artificial ou pipeline novo foi criado.

Coiny/Nunito usam preconnect e `display=swap`, e Playwright confirma que as famílias são carregadas.
LCP/CLS reais continuam desconhecidos sem telemetria de campo ou um orçamento sintético calibrado;
esta Sprint não apresenta números de performance que o repositório não mede.

## Execução e CI

Execução local direcionada:

```powershell
dotnet test tests/BeeDay.Web.Tests/BeeDay.Web.Tests.csproj --configuration Release
dotnet test tests/BeeDay.E2E.Tests/BeeDay.E2E.Tests.csproj --configuration Release
```

Nenhum workflow duplicado foi criado. Alterações Web/E2E já selecionam a suíte Chromium no Fast PR;
o Release Quality Gate executa todos os projetos de teste e publica TRX. Os novos testes entram nesses
caminhos naturalmente.
