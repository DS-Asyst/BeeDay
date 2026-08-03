# Documentation Conventions

Este documento define como toda a documentação sob `docs/` deve ser escrita e mantida. É a
referência oficial para toda a EPIC 16 em diante.

**Fonte da verdade:** este documento é normativo por si mesmo — define a convenção, não descreve
código. Sua validade vem da aprovação explícita na Sprint 16.2, não de verificação contra `src/`.

## 1. Regra fundamental: fonte da verdade obrigatória

Todo documento técnico (fora de `adr/` e `history/`, que são registros congelados por natureza)
deve declarar explicitamente de onde vêm suas afirmações. Nenhuma sentença deve ser escrita a
partir de memória de sprints anteriores ou copiada de documentação antiga sem reverificação.

Formas aceitas de declaração, geralmente na abertura do documento:

- "Verificado diretamente em `src/BeeDay.Infrastructure/...`"
- "Baseado na implementação atual de `BeeDayDbContext`"
- "Extraído dos contratos em `Application/Common/Contracts`"
- "Derivado dos testes em `tests/BeeDay.Web.Tests`"

Se uma seção específica não pôde ser verificada (ex.: comportamento observado apenas
manualmente), isso deve ser dito explicitamente, não silenciado.

## 2. Nomenclatura de arquivos

- `kebab-case.md` para todo arquivo novo.
- Documentos que fazem parte de uma sequência de leitura numerada usam prefixo `NN-` (`01-`, `02-`,
  ...), sem lacunas — ao remover ou mover um documento numerado, renumerar os remanescentes.
- ADRs usam `ADR-NNN-titulo-em-kebab-case.md`, `NNN` com 3 dígitos, sequencial e nunca reaproveitado
  mesmo se um ADR for rejeitado.
- Cada pasta de área tem um `README.md` — nunca `index.md` ou `_index.md`.
- Templates vivem em `docs/_templates/` com o prefixo `_` na pasta (não no arquivo) para
  sinalizar "não é conteúdo, é modelo".

## 3. Organização

- Uma pasta por área, espelhando a arquitetura real do sistema (ver `docs/README.md`).
- Nenhum documento vive solto na raiz de `docs/` além de `README.md` e `CONVENTIONS.md`.
- Documentos que se tornam obsoletos ou puramente históricos são movidos para `docs/history/`, não
  apagados — preservando a árvore Git via `git mv`.

## 4. Headings

- Um único `#` (H1) por documento, com o título.
- Seções principais em `##` (H2), subseções em `###` (H3). Evitar passar de H4.
- Não pular níveis (H2 direto para H4).

## 5. Links internos

- Sempre relativos ao arquivo atual (`../persistence/README.md`, não caminho absoluto do
  repositório).
- Todo link deve apontar para um arquivo que existe — verificar antes de publicar.
- Ao referenciar uma decisão, linkar o ADR correspondente em vez de reexplicar a decisão.

## 6. Diagramas e Mermaid

- Preferir Mermaid (` ```mermaid `) a imagens estáticas quando o diagrama for texto/estrutura
  (fluxos, dependências, ER) — mantém o diagrama versionável e editável em texto.
- Usar imagem estática apenas para capturas de tela reais de UI.

## 7. Imagens e screenshots

- Vivem em `docs/<área>/assets/` (criar a subpasta apenas quando houver ao menos uma imagem).
- Nome de arquivo descritivo em kebab-case, não `image1.png`.
- Toda imagem tem texto alternativo (`![descrição](caminho)`).
- Screenshots de UI devem ser retiradas novamente sempre que o componente capturado mudar
  visualmente de forma relevante — uma screenshot desatualizada é pior que nenhuma.

## 8. Blocos de código

- Sempre com a linguagem declarada (` ```csharp `, ` ```razor `, ` ```bash `, ` ```yaml `).
- Trechos de código do projeto devem ser reais, copiados do arquivo fonte, com o caminho do
  arquivo indicado antes ou dentro do bloco — nunca pseudocódigo apresentado como se fosse real.
- Comandos de shell devem ter sido executados e confirmados nesta sessão antes de entrarem em um
  documento, quando praticável.

## 9. Tabelas

- Usadas para: parâmetros de componente, comparação de opções, listas de status por documento.
- Não usar tabela para texto corrido que ficaria mais claro em prosa.

## 10. Emojis

- Não usar em texto técnico corrido, headings ou nomes de arquivo.
- Permitido apenas em checklists de critérios de aceite quando o próprio autor do pedido já usa
  esse formato (ex.: ✓/✗ em relatórios de sprint), nunca como decoração.

## 11. Idioma

- Documentação de processo/sprint (relatórios, auditorias, ADRs) em português, seguindo o idioma
  usado pelo autor do pedido nesta EPIC.
- `README.md` da raiz do repositório em inglês, por ser o ponto de entrada para qualquer
  colaborador ou ferramenta externa (GitHub, npm/NuGet registries, etc.).
- Identificadores de código, nomes de arquivo e nomes de pasta sempre em inglês, independentemente
  do idioma do texto ao redor.

## 12. Referências cruzadas

- Preferir um único documento "dono" de cada assunto, com os demais linkando para ele, em vez de
  duplicar a mesma explicação em vários lugares.
- Quando um documento cobre parcialmente um assunto que pertence a outra área (ex.:
  `security/01-security-baseline.md` cobrindo autenticação), declarar isso explicitamente no
  `README.md` da área, como feito em `docs/authentication/README.md`.

## 13. Manter documentos sincronizados com o código

- Nenhum documento deve ser considerado "concluído para sempre" — cada um deve poder responder
  "quando isso foi verificado pela última vez?".
- Ao alterar código que um documento descreve, atualizar o documento na mesma mudança sempre que
  praticável (regra já existente em `CLAUDE.md`).
- ADRs são a exceção deliberada: nunca são atualizados após aceitos, exceto para adicionar uma nota
  de "superseded por ADR-XXX" no cabeçalho.
- Documentos em `docs/history/` nunca são atualizados — são registro congelado por definição.
