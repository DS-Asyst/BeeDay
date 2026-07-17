# Decision Log

## ADR-001 — Reward como resultado universal

**Decisão:** atividades retornam `Reward`; somente `Character.ApplyReward()` modifica XP, atributos e títulos.

**Motivo:** eliminar concessões duplicadas ou esquecidas e corrigir definitivamente quests concluídas sem XP.

## ADR-002 — XP fixo por atividade

- Hábito: 0,5 XP.
- Quest: 1 XP.
- Capítulo: bônus igual à soma do XP das quests do capítulo.

O tempo do hábito deixa de influenciar recompensa.

## ADR-003 — Atributo principal do projeto

Projetos possuem `PrimaryAttribute`. Quests de projeto ou capítulo herdam esse atributo; seleção manual é permitida apenas para quests independentes.

## ADR-004 — Workflows controlam progressão

Ativação e transições de Quest, Capítulo, Boss e Projeto pertencem à camada de serviços/workflows. A UI apenas solicita operações.

## ADR-005 — Persistência relacional adiada

A Fase 8.5 estabiliza regras e contratos. SQLite/EF Core será introduzido na Fase 9 sem redesenhar o domínio.

## 2026-07-16 — Progressão de leitura e navegação de inventário

- Registrar progresso de leitura exige somente a página atual; data e hora são capturadas automaticamente.
- Progresso parcial de livros não concede XP.
- A conclusão concede uma única recompensa: 1 XP para livros com menos de 100 páginas; para livros com 100 páginas ou mais, `floor(totalPages * 0,10)` XP.
- Marcos de 1, 5, 10, 25 e 50 livros concluídos desbloqueiam conquistas temáticas de leitura.
- A navegação principal passa a expor Inventário, contendo Biblioteca e Carteira. Mochila fica fora da navegação até existir um sistema de itens.
- "Ficha do personagem" passa a ser apresentada como "Perfil".

## ADR — SQLite como persistência operacional da Fase 9

**Decisão:** usar SQLite com Entity Framework Core e migrations como única persistência oficial. Não há importação, exportação ou fallback automático em JSON.

**Motivos:** aplicação local e monousuário, implantação simples, transações ACID, inspeção fácil e caminho natural para EF Core.

**Consequências:** o banco fica fora do repositório, migrations passam a ser obrigatórias e o executável depende do projeto Infrastructure.

## ADR — Tabelas por agregado com payload serializado

**Decisão:** persistir cada agregado em tabela própria, mantendo seu payload completo serializado na primeira iteração relacional.

**Motivos:** preservar invariantes e encapsulamento do domínio, reduzir risco de regressão e permitir migração gradual para um modelo relacional mais granular.

**Consequências:** consultas analíticas dentro do payload não são o foco desta fase. Colunas e read models normalizados deverão ser adicionados quando houver requisitos concretos da interface Blazor.

## ADR — Composition root separado

**Decisão:** transformar o projeto central em biblioteca e criar `LevelUp.Console` como executável.

**Motivos:** impedir dependência circular entre núcleo e infraestrutura e permitir outros clientes no futuro.
