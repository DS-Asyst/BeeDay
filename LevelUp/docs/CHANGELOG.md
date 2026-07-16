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
