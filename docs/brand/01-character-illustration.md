# Character & Illustration System

Regras reproduzíveis extraídas exclusivamente dos ativos aprovados e de seus consumers atuais. O
documento formaliza o que já é observável; não cria personagens, nomes, lore, anatomia ou variantes.

**Fonte da verdade:** os sete PNGs sob `src/BeeDay.Web/wwwroot/assets/`, seus consumers em
`Components/Features/Home/Pages/Home.razor`, `Components/Layout/` e o responsive de
`Home.razor.css`, inspecionados em 2026-08-16. Sem arquivos-fonte vetoriais, character sheets ou
metadados autorais no repositório, qualquer característica além dessa evidência é desconhecida.

## Inventário atual

Todos os ativos encontrados são PNG RGBA (`Format32bppArgb`). Dimensões declaradas no markup
coincidem com as dimensões intrínsecas.

| Ativo | Dimensão / peso | Consumer e tratamento | Semântica | Status |
|---|---:|---|---|---|
| `assets/brand/beeday-top-navigation.png` | 866×288 / 117.690 B | `PublicHeader` e `AppFooter`; largura responsiva por CSS, sem lazy-loading | Identidade: link do header recebe nome acessível externo; footer usa `alt="beeday"` | Wordmark canônico atual; fora do sistema de personagens |
| `assets/flags/brazil.png` | 612×408 / 170.991 B | `PublicLanguageSwitcher`; reduzida por CSS dentro de botão rotulado | Decorativa; o botão informa idioma e estado | Utilitário atual, não ilustração de marca |
| `assets/flags/united-states.png` | 612×408 / 140.253 B | `PublicLanguageSwitcher`; reduzida por CSS dentro de botão rotulado | Decorativa; o botão informa idioma e estado | Utilitário atual, não ilustração de marca |
| `assets/hero/home-team.png` | 1536×1024 / 1.770.361 B | Hero da Home; `contain`, caixa quadrada, 32rem desktop, 24rem até 52rem e 19rem até 30rem; `fetchpriority="high"` | Decorativa, em container `aria-hidden`; texto adjacente entrega a mensagem | Composição canônica aprovada da Home, não character sheet |
| `assets/home/how-beeday-works-bee.png` | 1254×1254 / 533.600 B | Seção de passos; `contain`, absoluta no desktop e em fluxo até 60rem; `loading="lazy"`, `decoding="async"` | Decorativa, em container `aria-hidden`; passos carregam o conteúdo | Composição individual canônica atual da abelha |
| `assets/home/home-team-fall.png` | 1536×1024 / 1.793.100 B | Fechamento da Home; largura `clamp` por viewport, sobrepõe a wave sem crop; lazy/async | Decorativa, `alt=""` e `aria-hidden="true"` | Variante de grupo canônica para esta composição |
| `assets/home/wave-site.png` | 1672×941 / 476.914 B | Fechamento da Home; centralizada, largura mínima 48rem, recorte vertical controlado pelo container; lazy/async | Decorativa, em container `aria-hidden` | Fundo composition-specific; não é foundation de UI |

Não existem ativos individuais dos seis personagens coadjuvantes, SVGs de personagem, variantes
alternativas ou poses nomeadas. O PNG raiz `beeday-wordmark.png`, sem consumer e separado do lockup
canônico de Header/Footer, foi removido no sweep final. Também não há evidência de que
recortes extraídos das imagens de grupo sejam permitidos; portanto, não devem ser produzidos.

## Abelha central

A abelha é o personagem central confirmado pela combinação de evidências: é o único personagem
com arquivo individual nomeado, ocupa o maior primeiro plano do hero e reaparece no grupo de
fechamento. O repositório não estabelece nome próprio, gênero, idade, personalidade ou biografia.

Regras observáveis:

- silhueta arredondada e compacta, corpo aproximadamente circular, duas antenas com terminais
  esféricos, duas faixas escuras, asas laterais translúcidas e membros curtos;
- cabeça e corpo formam um volume contínuo, com olhos e boca grandes em relação à silhueta;
- a evidência aprovada cobre alegria de boca aberta, alegria de olhos fechados e pose dinâmica;
  outras emoções não estão aprovadas;
- no hero, pode ser o maior elemento e ficar em primeiro plano; no grupo de fechamento, aparece
  menor e afastada, sem perder antenas, asas ou faixas identificadoras;
- a versão individual é clara/quase monocromática e pertence ao fundo azul-claro da seção de
  passos; ela não autoriza recolorir as versões de grupo;
- nenhuma composição aprovada corta a silhueta da abelha. Antenas e asas devem permanecer dentro
  do quadro, com respiro suficiente para não parecerem acidentalmente truncadas.

## Personagens coadjuvantes confirmados

Os dois grupos mostram os mesmos seis coadjuvantes. Os rótulos abaixo são descrições visuais,
não nomes oficiais.

| Identificação observável | Características comprovadas nos grupos | Desconhecido / não autorizado |
|---|---|---|
| Raposa alaranjada | Orelhas pontudas, focinho/cauda claros, laço roxo, camisa clara e jardineira azul | Nome, idade, papel, proporção fora do grupo |
| Urso marrom | Orelhas redondas, focinho claro, camisa azul, calça/bermuda roxa e cinto | Nome, idade, ocupação, acessórios alternativos |
| Felino laranja listrado | Listras escuras, faixa vermelha e bermuda vermelha | Espécie exata, nome, idade, papel |
| Primata marrom | Face/orelhas claras, cauda curva, camiseta amarela com acabamento azul | Espécie exata, nome, idade, papel |
| Felino branco | Orelhas triangulares, laço rosa e roupa rosa com parte superior clara | Espécie exata, nome, idade, papel |
| Flor antropomórfica | Pétalas amarelas, face marrom, folhas/caule verdes e vaso verde | Espécie botânica, nome, idade, papel |

Todos usam olhos grandes, volumes arredondados, boca expressiva e poses de corpo inteiro. Os grupos
comprovam alegria, surpresa/medo cômico e movimento; não comprovam estados negativos intensos,
violência, poses estáticas de referência nem relações narrativas permanentes.

## Illustration Shape Language

- **Geometria:** massas simples, arredondadas e infladas; extremidades, focinhos, orelhas e
  acessórios evitam cantos agudos dominantes.
- **Contorno:** não há outline externo uniforme. A separação vem de mudanças de cor, oclusão,
  highlights e sombras suaves.
- **Volume e perspectiva:** render 3D estilizado, perspectiva frontal/três-quartos e sobreposição
  de corpos para sugerir profundidade. Não misturar com line art plano dentro do mesmo grupo.
- **Luz:** highlights macios, sombras de contato e glow difuso colorido; sem sombras duras de UI,
  hachura ou textura realista.
- **Detalhe:** alto contraste facial e detalhes suficientes para leitura do personagem, mas sem
  anatomia realista, pelagem detalhada ou ruído de superfície.
- **Cor:** amarelo da abelha e violeta/azul de marca convivem com laranja, marrom, rosa, verde e
  cyan específicos da arte. Essas cores artísticas permanecem `Illustration`; não representam
  automaticamente Brand, Semantic ou Product tokens.
- **Fundo:** os grupos usam transparência e halos difusos; a wave usa violeta próprio da
  composição. Um fundo novo deve preservar contraste e silhueta sem transformar a cor da arte em
  regra de interface.

Essas regras descrevem coerência interna; não permitem copiar anatomia, proporção ou styling de
outra marca para preencher especificações ausentes.

## Composição e responsive

- Usar os grupos completos nos dois contextos aprovados. A arte individual da abelha pertence à
  composição de passos; não substituir uma pela outra sem nova aprovação.
- Preservar a proporção intrínseca com `height: auto` ou `object-fit: contain`; nunca esticar para
  preencher uma caixa.
- Não cortar personagens, antenas, asas, orelhas ou a flor. O único recorte aprovado é o recorte
  vertical da wave, calculado pelo container do fechamento.
- Manter texto e ações fora da arte. No hero desktop, imagem e conteúdo ocupam colunas distintas;
  até 52rem, a imagem empilha acima do conteúdo. A arte nunca deve interceptar ponteiro nem
  encobrir foco, labels ou controles.
- O grupo de fechamento pode sobrepor a wave porque ambos são decorativos e o texto começa na base
  opaca abaixo. A sobreposição não autoriza posicionar personagens sobre links.
- Em viewports estreitos, reduzir o conjunto, não extrair personagens para compensar espaço. O
  teste E2E atual protege proporção, ausência de crop e overlap com a wave.

## Acessibilidade e performance

As quatro artes da Home são decorativas: o heading, os cinco passos e os grupos de links comunicam
todo o conteúdo sem elas. Devem manter `alt=""`; `aria-hidden="true"` pode estar na imagem ou em seu
container. Se uma arte futura comunicar informação ausente do texto, ela deixa de ser decorativa e
precisa de alternativa localizada que comunique a função, não uma lista de detalhes visuais.

O hero é acima da dobra, permanece no carregamento inicial e usa prioridade alta. As três imagens
posteriores usam lazy-loading; seus bitmaps são decodificados de forma assíncrona. `width`/`height` intrínsecos
devem continuar declarados para reservar proporção e reduzir layout shift.

Os quatro PNGs artísticos somam 4.573.975 bytes no repositório e não possuem variantes responsivas.
O peso é dívida conhecida, não permissão para recompressão destrutiva: otimização futura deve
comparar fidelidade, transparência, tamanho transferido e suporte do browser, manter fallback quando
necessário e ser validada visualmente nos breakpoints existentes.

## Checklist para novo uso

1. Confirmar que o ativo já é aprovado para o contexto; não derivar pose, roupa, cor ou recorte.
2. Classificar como decorativo ou informativo antes de escrever `alt`/ARIA.
3. Declarar dimensões intrínsecas e preservar aspect ratio.
4. Definir prioridade de carregamento pela posição real na página; abaixo da dobra usa lazy/async.
5. Validar desktop, 960px, 736px, 480px e 390px sem crop, overflow ou obstrução de conteúdo.
6. Registrar qualquer novo ativo, consumer, semântica e status neste inventário na mesma mudança.
