# Changelog

## Unreleased — Fase 5.2

### Added

- classes de personagem escolhidas na criação;
- títulos automáticos por nível;
- conquistas persistidas;
- Chefe final obrigatório por Projeto;
- conquista profissional ao concluir Projeto;
- tela de conquistas no módulo Personagem;
- menu Configurações e comando Salvar jogo.

### Changed

- Chefes deixam de pertencer a Capítulos e passam a encerrar Projetos;
- o campo legado de título do Projeto foi substituído por prefixo de conquista do Chefe;
- cartões de Projeto e Capítulo refletem a nova progressão;
- documentação consolidada e relatórios redundantes removidos.

## Fase 6 — Inteligência e confiabilidade

- adicionada versão do schema e infraestrutura de migração;
- adicionada validação de integridade de saves;
- salvamento alterado para escrita atômica com snapshot anterior;
- introduzidos `GameSession` e `ApplicationBootstrap`;
- adicionado dashboard de visão geral;
- Carteira passa a oferecer estorno auditável na interface;
- livros novos iniciam na página zero;
- cobertura e documentação de persistência, testes e privacidade adicionadas.

## Fases 7 e 8

- adicionada visualização da progressão completa do personagem;
- adicionado módulo Mundo;
- adicionadas metas persistentes baseadas em indicadores reais;
- schema do save elevado para a versão 3;
- adicionada migração da versão 2 para a versão 3;
- corrigida persistência de `Milestone.ProjectId`;
- adicionada avaliação sobre migração para SQLite após a Fase 8.

## Fase 8.5 — Consolidação e polimento

### Fixed

- corrigida a criação de `GameSession` no teste de metas usando argumento nomeado para `GoalService`;
- preservada a validação de relacionamentos de Capítulos e Projetos.

### Changed

- seleção de Projetos, Capítulos e Missões extraída para `QuestSelectionFlow`;
- `QuestScreen` deixa de depender diretamente de `MilestoneService` para seleções;
- roadmap consolidado, removendo fases duplicadas e a migração prematura para banco de dados;
- artefatos locais removidos do pacote de código-fonte.

### Added

- testes para IDs duplicados e Capítulos sem Projeto válido;
- documentação da Fase 8.5;
- plano de prontidão e implementação para a Fase 9.
