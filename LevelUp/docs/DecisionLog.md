# Registro de Decisões

## Código em inglês e interface em português

Classes, métodos, namespaces e propriedades permanecem em inglês. Todo texto apresentado ao usuário deve estar em português do Brasil.

## Cancelamento explícito

Fluxos de criação e edição aceitam o comando `cancel`. Confirmações usam opções explícitas em vez de abreviações.

## Organização do menu por domínio

O menu principal é composto por Personagem, Diário, Biblioteca, Mochila, Configurações, Salvar jogo e Sair.

## Carteira representa dinheiro real

A Carteira registra reservas e saídas reais. Ela não é moeda fictícia, não é recompensa e não deve ser misturada com XP ou conquistas.

## Biblioteca independente

Livros e leituras não pertencem a Projetos. A Biblioteca limita o foco a dois livros simultaneamente em andamento e mantém histórico por data.

## Capítulos ordenam Projetos

`Milestone` é apresentado como Capítulo. Missões podem pertencer opcionalmente a um Capítulo do mesmo Projeto. Apenas um Capítulo fica ativo por Projeto.

## Um Chefe final por Projeto

Capítulos não possuem Chefes. Cada novo Projeto exige um Chefe final. Quando todas as Missões e Capítulos válidos terminam, o Chefe é desbloqueado. Derrotá-lo conclui o Projeto.

## Separação entre classe, título e conquista

- Classe: identidade escolhida ao criar o Personagem e sem efeito mecânico nesta fase.
- Título: progressão automática baseada no nível.
- Conquista: feito histórico persistido, como a conclusão de um Projeto.

## Conquista profissional composta

Ao criar um Projeto, o usuário informa o nome do Chefe e um prefixo. A conquista é formada pela combinação, como `Desenvolvedor ASP.NET Core`.

## Compatibilidade temporária

Sobrecargas antigas permanecem apenas para facilitar testes e leitura de saves anteriores. Novas funcionalidades devem usar a arquitetura atual.

## Fase 6

- A Fase 6 prioriza inteligência, confiabilidade e relatórios antes de Vida e Energia.
- Saves são versionados e migrados fora do bootstrap.
- A Carteira é tratada como ledger: correções usam estorno.
- `ApplicationBootstrap` e `GameSession` organizam composição e estado.

## Simplificação da Fase 8.5

- A tela Visão geral foi removida do menu principal.
- A progressão fica exclusivamente na Ficha do personagem.
- O módulo Mundo/Metas foi removido integralmente.
- O schema 4 registra a retirada desse módulo da persistência oficial.
## Capítulos pertencem ao contexto do Projeto

Capítulos não aparecem como seção independente do Diário. O gerenciamento de capítulos é iniciado ao abrir um Projeto, reforçando a associação obrigatória entre os dois conceitos.

## Carteira aceita saldo negativo

A interface usa os termos Entrada e Saída. O saldo pode ficar negativo para representar dívidas, empréstimos e compromissos financeiros reais. Saídas continuam exigindo justificativa.

## Progressão por nível não é exibida

As faixas de título permanecem como regra interna do domínio. A Ficha do personagem apresenta somente o título e o nível atuais, sem revelar a tabela completa de progressão.
