# Writing, Voice, Tone & Localization System

Sistema de escrita para superfícies de marca e produto do beeday. Ele torna avaliável a copy nova
sem exigir que o texto existente seja reescrito apenas por novidade.

**Fonte da verdade:** amostra representativa dos 17 catálogos `.resx` e seus consumers em Home,
onboarding/auth/Identity, Daily, Wallet, Account, Design System feedback, Experience e e-mail de
Identity, auditada em 2026-08-16. Os catálogos atuais possuem 650 chaves por cultura e 1.950 valores
entre neutro, `en-US` e `pt-BR`; o teste `ResourceCatalogContractTests` protege paridade, fallback e
placeholders. O pipeline técnico continua documentado em
[`docs/web/07-localization.md`](../web/07-localization.md).

## Narrativa de marca

O beeday ajuda a construir um dia melhor, um passo de cada vez. A narrativa aprovada conecta o que
importa a ações concretas: organizar Daily, hábitos, tarefas e projetos, acompanhar progresso e
reconhecer avanços consistentes. Ela não promete transformação instantânea, produtividade perfeita,
resultados garantidos ou métricas que o produto não calcula.

Mensagens novas devem poder responder:

1. qual ação ou estado real do produto está sendo comunicado;
2. como isso ajuda a pessoa a avançar com clareza;
3. qual próximo passo existe, quando houver um.

## Voice

A voz é estável em qualquer contexto:

- **clara:** usa palavras concretas, informa o objeto da ação e evita slogan no lugar de instrução;
- **encorajadora:** reconhece progresso sem infantilizar, pressionar ou atribuir culpa;
- **orientada à ação:** verbos indicam o que fazer e feedback confirma o que ocorreu;
- **honesta:** distingue sucesso, falha e estado parcial, sem esconder consequência ou inventar
  benefício;
- **próxima, não invasiva:** fala diretamente com "you/your" e "você/seu" quando isso ajuda, sem
  presumir emoção, rotina, capacidade financeira ou objetivo pessoal.

Copy off-brand é vaga, punitiva, excessivamente efusiva, faz piada em contexto sensível, promete o
que o produto não entrega ou usa gamificação fictícia para forçar engajamento.

## Tone por contexto

| Contexto | Tom | Estrutura preferida | Evitar |
|---|---|---|---|
| Operação neutra | direto e calmo | estado ou ação em poucas palavras | slogan, exclamação, adjetivo promocional |
| Onboarding e motivação | acolhedor e orientador | benefício real + próximo passo | urgência artificial, promessa absoluta |
| Sucesso e celebração | positivo e proporcional | resultado confirmado; XP/level se realmente calculado | confete verbal em operação rotineira, métrica inventada |
| Warning | preventivo e específico | risco + ação segura | alarmismo ou culpa |
| Erro/falha | factual, respeitoso e recuperável | o que falhou + impacto preservado + tentativa possível | humor, culpa, detalhe interno/exceção crua |
| Confirmação destrutiva | inequívoco e sóbrio | verbo + objeto; consequência permanente separada | eufemismo, CTA ambíguo como "OK" |
| Empty state | informativo e convidativo | estado + explicação curta + ação quando disponível | tratar vazio esperado como erro |
| Wallet/finance | preciso e contido | valor, tipo, data, efeito e reversibilidade | piada, comemoração de gasto/receita, julgamento financeiro |
| Segurança/Identity | direto e protetivo | condição, validade, próximo passo | revelar existência de conta, minimizar risco |

## Style

### Casing e hierarquia

- A marca visual é sempre `beeday`, inclusive dentro de frase, heading, CTA e e-mail. `BeeDay`
  permanece somente como technical identity em nomes de código, assembly, tipo ou catálogo de
  desenvolvimento.
- Copy nova de produto usa sentence case em headings, labels e botões: `Reset password`,
  `Redefinir senha`. Não usar Title Case palavra por palavra em português.
- Eyebrows são rótulos curtos. Aparência uppercase pertence ao CSS; não alterar o casing de
  `beeday` nem gravar uppercase no recurso apenas por apresentação.
- Strings uppercase herdadas de onboarding/Experience são exceções expressivas existentes, não
  precedente para copy nova. Uma migração requer validar o consumer e o efeito visual.

### Labels, frases e pontuação

- CTA começa com verbo específico: `Create transaction`, `Try again`, `Criar transação`, `Tentar
  novamente`. O label deve continuar correto fora do parágrafo que o cerca.
- Headings, labels, tabs, eyebrows e títulos de toast não recebem ponto final. Pergunta real mantém
  interrogação.
- Descrições, mensagens de erro e consequências destrutivas usam frases completas e pontuação.
- Preferir uma ideia por frase e duas frases curtas a uma cadeia de condições. Texto de botão e
  label não deve carregar explicação.
- Não usar reticências para loading. Estado assíncrono é comunicado pelo componente e por
  `aria-busy`; a copy nomeia a operação quando necessário.

### Números, XP, datas e moeda

- Usar algarismos para quantidades, percentuais, nível e tempo operacional. Pluralização continua
  em chaves singular/plural quando o catálogo já as separa.
- `XP` é sigla invariável e uppercase: `{0} XP`, `+{0} XP`. `Level` localiza para `Nível`; nomes
  técnicos de models continuam em inglês.
- Datas visíveis usam o short-date da cultura ativa; valores de `<input type="date">`, atributos
  `datetime` e persistência permanecem ISO.
- Wallet representa USD. O símbolo `$` é fixo pela regra financeira atual, enquanto agrupamento,
  decimais e posição do símbolo seguem `CurrentCulture` (`$1,234.56` em `en-US`, `$ 1.234,56` em
  `pt-BR`). Copy nunca deve inferir outra moeda pelo idioma.

### Erro, validação e empty state

- Erro diz o que não foi concluído sem expor stack trace ou texto cru de Domain/Application. Se o
  estado anterior foi preservado, informar isso; oferecer `Try again`/`Tentar novamente` apenas
  quando a ação existe.
- Validação nomeia campo/requisito e como corrigir. Não usar "invalid" sozinho nem culpar a pessoa.
- Destruição usa title com objeto, pergunta de confirmação e warning de irreversibilidade/efeito
  em elementos relacionados.
- Empty state diferencia vazio inicial (`No tags yet`) de filtro sem resultado (`No transactions
  found`). A descrição explica o estado; a ação cria ou limpa filtro quando suportada.

### Emojis e símbolos

Emoji não faz parte da copy operacional atual e não deve substituir ícone, label ou estado. `+`/`−`,
`%`, `$` e `XP` só aparecem com significado real calculado. Ícones são fornecidos pelo BeeDay Icon
System e nunca carregam sozinhos informação necessária.

## Glossário canônico

| Conceito | `en-US` | `pt-BR` | Significado de produto |
|---|---|---|---|
| Activity | Activity | Atividade | termo guarda-chuva de Habit, Task e To-Do; não substitui o subtipo na UI |
| Habit | Habit | Hábito | comportamento recorrente com progresso/contador |
| Task | Task | Tarefa | atividade recorrente ou repetível, concluída manualmente |
| To-Do | To-Do | Pendência | ação pertencente a exatamente um Project; manter hífen/capitalização em inglês |
| Project | Project | Projeto | objetivo composto por To-Dos/Pendências |
| Daily | Daily | Diário | superfície que organiza as atividades do dia; technical route continua `/daily` |
| Wallet | Wallet | Carteira | superfície financeira em USD |
| Transaction | Transaction | Transação | registro de Income/Expense; Receita/Despesa |
| Tag | Tag | Tag | agrupador configurável da Wallet; não traduzir para "etiqueta" sem decisão de produto |
| Experience | Experience | Experiência | progresso acumulado do perfil |
| XP | XP | XP | unidade de Experience; sigla invariável |
| Level | Level | Nível | degrau derivado de XP |

O glossário governa texto visível, não renomeia classes, enums, rotas, tabelas ou contratos. O
nome de uma entity em C# não é justificativa para mostrar inglês na interface em `pt-BR`.

## Política `en-US` / `pt-BR`

- Traduzir significado, ação e impacto, não ordem das palavras. A promessa e o fato de produto
  devem permanecer equivalentes, mesmo quando o português precisa de estrutura diferente.
- `en-US` é cultura default: o `.resx` neutro deve ter as mesmas chaves e valores do `.en-US.resx`.
  `pt-BR` tem as mesmas chaves, com tradução própria.
- Projetar para expansão: componentes devem aceitar wrap e crescimento de português sem abreviar
  conceito, reduzir fonte ou inserir quebra manual no recurso.
- Preservar todos os placeholders (`{0}`, `{1}` e formatos) em cada cultura. A ordem pode mudar na
  frase, mas valor, contexto e encoding continuam no consumer.
- Não concatenar fragmentos localizados para montar frase. Criar uma chave completa quando ordem,
  gênero ou plural puder divergir.
- Conteúdo do usuário, IDs, rotas, valores ISO e identifiers técnicos não são traduzidos.
- `beeday`, `XP` e, pela decisão atual de produto, `Tag`, não são traduzidos. `Daily` e `Wallet`
  são traduzidos no texto visível, mas nomes técnicos continuam inalterados.
- Fixar `CurrentCulture` e `CurrentUICulture` nos testes que verificam texto, data, número ou moeda.

## E-mail transacional

`IdentityEmailComposer` produz confirmação de e-mail e reset de senha em inglês, com HTML seguro,
links one-time e casing correto de `beeday`. O contrato `IIdentityEmailComposer` recebe apenas
recipient, display name e token; handlers de Application não fornecem idioma, e Infrastructure não
participa do pipeline `IStringLocalizer` da Web.

Localizar corretamente exige decidir a cultura autoritativa do destinatário e transportá-la pelo
contrato sem introduzir dependência Web ou um segundo conjunto de recursos. Essa mudança
arquitetural fica `DEFER`; não selecionar idioma por ambiente, cultura global do worker ou
`Accept-Language` ausente. Enquanto isso, novos textos desses templates seguem o tone de Identity:
diretos, sem humor, com validade, uso único, ação e instrução para ignorar pedido não iniciado.

## Checklist de revisão

1. A copy descreve comportamento real e usa o termo canônico?
2. Voice permanece clara, encorajadora, acionável, honesta e não invasiva?
3. Tone corresponde ao risco do contexto, especialmente Wallet, erro, destruição e Identity?
4. Casing, pontuação, números, XP, data e moeda seguem as regras acima?
5. Neutro/en-US/pt-BR têm paridade e placeholders equivalentes?
6. O layout foi verificado com expansão, wrap e ambas as culturas quando a copy visível mudou?
