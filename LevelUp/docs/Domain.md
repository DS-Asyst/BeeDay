# Domínio

## Personagem

- `CharacterClass`: identidade escolhida na criação e sem efeito mecânico nesta fase.
- `CharacterRank`: título automático derivado do nível.
- faixas: Aprendiz, Aventureiro, Discípulo, Adepto, Especialista, Mestre e Lenda.

## Reconhecimento

- `Achievement`: conquista persistida e histórica.
- conquistas de Projeto são únicas pelo identificador do Projeto.
- o nome é composto por `AchievementPrefix + Boss.Name`.

## Projetos

- todo novo Projeto possui um Chefe final obrigatório;
- Capítulos não possuem Chefes;
- Missões podem pertencer opcionalmente a um Projeto e a um Capítulo compatível;
- o Chefe é desbloqueado somente depois de todos os requisitos do Projeto;
- derrotar o Chefe conclui o Projeto.

## Biblioteca

- máximo de dois livros em andamento;
- progresso por página e histórico por data;
- experiência concedida somente por avanço positivo.

## Carteira

- entradas e saídas representam dinheiro real;
- saídas exigem justificativa;
- o saldo pode ficar negativo para representar dívidas ou valores emprestados;
- a Carteira não concede moeda fictícia.


## Confiabilidade do estado

`GameData` é um snapshot versionado. Relacionamentos entre Projeto, Capítulo, Missão e Chefe são validados no carregamento. A Carteira adota estorno como operação de correção, preservando a movimentação original. Livros novos começam sem páginas registradas (`CurrentPage = 0`).
