# Official Brand Color Palette

**Fonte da verdade:** decisão explícita de Brand do responsável pelo repositório, registrada na
Sprint 29.2 (2026-08-17) porque a EPIC 27 nunca a documentou apesar de já tê-la implementado em
código — `src/BeeDay.Web/wwwroot/css/variables.css` (`--beeday-palette-cor0` a `-cor9`) e
`src/BeeDay.Web/Components/DesignSystem/BeeDayPaletteToken.cs` (o enum `BeeDayPaletteToken`), ambos
introduzidos na EPIC 27 Sprint 27.1. Comentários de código anteriores a esta Sprint referenciam um
`03_DESIGN_DECISIONS.md` que nunca existiu neste repositório; esta foi a primeira documentação real e
committed da paleta. Sprint 29.3 (2026-08-17) acrescentou a seção sobre a referência cromática do CTA
branco importante, abaixo.

## A paleta

Dez cores físicas/base, cada uma com um identificador `CORx` fixo. Os valores HEX abaixo são a
decisão de marca — não devem ser alterados, aproximados ou substituídos, e nenhuma décima primeira
cor deve ser adicionada sem nova decisão explícita do responsável.

| Token | HEX | Token CSS técnico | Enum C# |
|---|---|---|---|
| `COR0` | `#5247F9` | `--beeday-palette-cor0` | `BeeDayPaletteToken.Cor0` |
| `COR1` | `#CE82FF` | `--beeday-palette-cor1` | `BeeDayPaletteToken.Cor1` |
| `COR2` | `#58CC02` | `--beeday-palette-cor2` | `BeeDayPaletteToken.Cor2` |
| `COR3` | `#1CB0F6` | `--beeday-palette-cor3` | `BeeDayPaletteToken.Cor3` |
| `COR4` | `#FFB100` | `--beeday-palette-cor4` | `BeeDayPaletteToken.Cor4` |
| `COR5` | `#FF7878` | `--beeday-palette-cor5` | `BeeDayPaletteToken.Cor5` |
| `COR6` | `#FFFFFF` | `--beeday-palette-cor6` | `BeeDayPaletteToken.Cor6` |
| `COR7` | `#ECECED` | `--beeday-palette-cor7` | `BeeDayPaletteToken.Cor7` |
| `COR8` | `#100F3E` | `--beeday-palette-cor8` | `BeeDayPaletteToken.Cor8` |
| `COR9` | `#DEFFF7` | `--beeday-palette-cor9` | `BeeDayPaletteToken.Cor9` |

`COR0` é o mesmo físico que `--beeday-color-brand-primary` (a Brand Color histórica e única antes da
EPIC 27) — `variables.css` já expressa isso como um alias (`--beeday-palette-cor0: var(--beeday-color-
brand-primary)`) em vez de repetir o literal, para que as duas nunca divirjam. As demais nove são
valores físicos novos introduzidos pela EPIC 27, sem equivalente anterior.

`COR7` no código (`#ECECED`) difere em um dígito do valor informado em alguns registros informais
(`#ECEDED`); `#ECECED` é o valor efetivamente implementado em `variables.css` desde a Sprint 27.1 e é
o que este documento formaliza como correto — não uma nova decisão desta Sprint.

## Para que serve

Esta é a **Official Brand Palette** — as dez cores físicas disponíveis para composição visual do
beeday. Ela não é, por si só, uma camada de tokens semânticos de UI. A arquitetura de consumo é:

```text
Official Brand Palette (este documento)
COR0..COR9
        │
        ▼
Semantic Design Tokens (docs/design-system/01-foundations.md §2)
--beeday-hero-surface-*, --beeday-color-accent-secondary(-on-light), Tag colors etc.
        │
        ▼
Components / Layouts / Patterns
BeeDayHero, BeeDayButton (important-white), Wallet Tag picker, etc.
        │
        ▼
Pages
Institutional editorial pages, Wallet, ExperienceSystemHome
```

Consumidores não devem espalhar os dez HEX pelo CSS/Razor além do `variables.css`/`BeeDayPaletteToken.cs`
que já os centralizam. Novo uso de uma cor da paleta deve passar por um token semântico ou pelo
próprio enum — nunca por um literal HEX repetido.

Usos atuais autorizados: superfícies de Hero/page-header (`BeeDayHero.Surface`) e cores de Tag da
Wallet (`WalletTagFormModel`). Não é uma paleta de ilustração — valores artísticos de composição
(ex. a paleta usada pelos personagens/wave da Home) permanecem fora desta lista, ver
[`01-character-illustration.md`](01-character-illustration.md).

## Contraste de foreground

Cada `CORx` já é pareada com um foreground WCAG-checked em `variables.css`
(`--beeday-palette-corN-foreground`): `COR0` e `COR8` pareiam com branco; as demais oito pareiam com
o `COR8` escuro. Essa é a regra **geral** de contraste da paleta, usada por qualquer superfície
`beeday-surface-corN` (utilities.css) — por exemplo Tags, que podem legitimamente ter texto escuro
sobre fundo claro.

## Regra de page header (Sprint 29.2)

Página headers do beeday Experience System/Institutional são superfícies coloridas sólidas — nunca
gradiente. Diferente da regra geral de contraste acima, o contrato visual de **page header**
especificamente é mais restrito:

- todo texto dentro de um page header colorido usa branco, sempre — não há lógica página a página
  escolhendo preto ou branco;
- portanto, só pode ser page-header-background um `CORx` cujo contraste com texto branco atinja pelo
  menos 4.5:1 (WCAG AA, texto normal).

Contraste calculado (fórmula WCAG, `#FFFFFF` como texto) para as dez cores:

| Token | Contraste com branco | Elegível para page header? |
|---|---:|---|
| `COR0` `#5247F9` | ~5.96:1 | ✅ sim |
| `COR1` `#CE82FF` | ~2.54:1 | ❌ não |
| `COR2` `#58CC02` | ~2.09:1 | ❌ não |
| `COR3` `#1CB0F6` | ~2.45:1 | ❌ não |
| `COR4` `#FFB100` | ~1.82:1 | ❌ não |
| `COR5` `#FF7878` | ~2.56:1 | ❌ não |
| `COR6` `#FFFFFF` | 1.00:1 | ❌ não |
| `COR7` `#ECECED` | ~1.15:1 | ❌ não |
| `COR8` `#100F3E` | ~18.10:1 | ✅ sim |
| `COR9` `#DEFFF7` | ~1.11:1 | ❌ não |

Apenas **`COR0`** e **`COR8`** são elegíveis. Isso já era parcialmente refletido em código antes desta
Sprint — `BeeDayPaletteTokenExtensions.IsWhiteForeground()` (`BeeDayPaletteToken.cs`) já identifica
exatamente esses dois tokens como os únicos cujo foreground aprovado é branco — mas nunca havia sido
usado para **restringir** quais cores um page header pode escolher; as outras oito continuavam
disponíveis para heros/headers com foreground escuro (contrato de contraste diferente, não incorreto
em si, apenas não é mais o contrato de page header).

O Design System implementa essa restrição na camada de consumo, não em `BeeDayHero` (um primitive
genérico usado também fora do contexto de page header, ex. o hero compacto da Wallet, que já usava
`COR0`): os quatro templates institucionais (`EditorialPageTemplate`, `HelpPageTemplate`,
`ProductPageTemplate`, `LegalDocumentPageTemplate` — `src/BeeDay.Web/Components/Features/
Institutional/Components/`) definem `Surface` como `Cor0` (Editorial/Help/Product) ou `Cor8` (Legal)
por padrão, e nenhuma das 11 páginas institucionais reais sobrescreve esse valor. `ExperienceSystemHome`
já usava `Cor8` para seu hero antes desta Sprint — já estava em conformidade.

### Mapeamento efetivo (Sprint 29.2, atualizado na Sprint 29.4)

**Sprint 29.4:** `/brand-guidelines` deixou de compartilhar componente/superfície com
`/experience-system` (`Cor8`) e virou sua própria página institucional, na família Editorial ("About
us") — agora `Cor0`, como Mission/Efficacy/Contact. `/experience-system` continua `Cor8`, sozinho na
linha `ExperienceSystemPage`.

| Página | Rota | Template | `CORx` | HEX |
|---|---|---|---|---|
| Mission | `/mission` | Editorial | `Cor0` | `#5247F9` |
| Efficacy | `/efficacy` | Editorial | `Cor0` | `#5247F9` |
| Contact | `/contact` | Editorial | `Cor0` | `#5247F9` |
| beeday | `/beeday` | Product | `Cor0` | `#5247F9` |
| beeday Plus | `/beeday-plus` | Product | `Cor0` | `#5247F9` |
| Android | `/android` | Product | `Cor0` | `#5247F9` |
| iOS | `/ios` | Product | `Cor0` | `#5247F9` |
| FAQs | `/faqs` | Help | `Cor0` | `#5247F9` |
| Brand guidelines | `/brand-guidelines` | Editorial | `Cor0` | `#5247F9` |
| Community guidelines | `/community-guidelines` | Legal | `Cor8` | `#100F3E` |
| Terms | `/terms` | Legal | `Cor8` | `#100F3E` |
| Privacy | `/privacy` | Legal | `Cor8` | `#100F3E` |
| Experience System home | `/experience-system` | `ExperienceSystemPage` | `Cor8` | `#100F3E` |

Antes desta Sprint, 8 das 11 rotas institucionais usavam um dos oito tokens não elegíveis (Efficacy
`Cor3`, Contact `Cor2`, Faqs `Cor4`, ProductPlus `Cor1`, Android `Cor2`, Ios `Cor3`,
CommunityGuidelines `Cor9`, Terms `Cor7`) — uma escolha arbitrária por página, sem regra de
associação documentada. Esta tabela é o mapeamento documentado e determinístico a partir de agora:
Editorial/Help/Product usam por padrão o brand-primary `Cor0`; páginas Legal/document usam o `Cor8`
escuro, mais formal. Uma página futura em um dos quatro templates existentes herda o padrão da sua
família automaticamente; uma família de página genuinamente nova precisaria de seu próprio padrão
explícito e documentado, escolhido entre `{Cor0, Cor8}`.

## Referência cromática do CTA branco importante (Sprint 29.3)

`#2CBAFF` é a referência de design solicitada para o texto dos CTAs brancos importantes (ex. "I
ALREADY HAVE AN ACCOUNT"/"JÁ TENHO UMA CONTA", "Create account"). **Não é uma décima primeira cor da
paleta** — `COR0`-`COR9` permanece fechada. `#2CBAFF` mede ~2.19:1 de contraste contra branco,
abaixo do mínimo WCAG AA de 4.5:1 para texto — a mesma situação de `COR3`/`#1CB0F6` (~2.44:1), a
referência anterior para esse mesmo papel. Ambas pertencem à mesma família de matiz (~200° em HSL);
o token técnico já existente `--beeday-color-accent-secondary-on-light` (`#0B72A6`, ~5.29:1) continua sendo o representante
acessível correto dessa família para uso em texto — nenhum novo literal ou token foi criado. Ambos os
tokens (`--beeday-color-accent-secondary` e sua variante `-on-light`) estão declarados e comentados
em `src/BeeDay.Web/wwwroot/css/variables.css`, próximos à declaração da paleta COR0-COR9.
