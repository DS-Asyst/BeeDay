# Fase 8.5 — Consolidação e polimento

## Objetivo

Estabilizar a base antes de persistência relacional ou nova interface, reduzindo risco e eliminando experiências que não agregaram valor ao produto.

## Entregas

- Capítulos acessíveis somente pelo contexto de um Projeto;
- Carteira com termos Entrada e Saída e suporte a saldo negativo;
- Configurações reduzidas a idioma e versão do save;

- correção de testes incompatíveis com `GameSession`;
- extração de `QuestSelectionFlow`;
- redução do acoplamento da `QuestScreen`;
- testes adicionais de integridade;
- remoção de artefatos locais de build;
- remoção da tela **Visão geral** e dos serviços exclusivos do dashboard;
- remoção completa do módulo **Mundo/Metas**;
- remoção da tela separada **Progressão**;
- progressão por nível mantida como regra interna e removida da interface;
- schema do save elevado para a versão 4 para registrar a retirada do módulo de metas;
- documentação e roadmap consolidados.

## Princípios

1. Não manter módulos apenas porque já foram implementados.
2. A interface deve refletir necessidades reais do usuário.
3. A Ficha do personagem mostra apenas o estado atual do personagem; tabelas internas de progressão não são exibidas.
4. A remoção de módulos inclui domínio, serviços, persistência, testes e documentação.
5. Banco de dados permanece adiado até existir motivação mensurável.

## Dívidas conhecidas

- `QuestScreen`, `TrainingScreen`, `LibraryScreen` e `WalletScreen` ainda podem ser divididas em flows menores;
- o domínio usa `DateTime.Now` diretamente;
- mensagens de exceção do domínio ainda estão em português;
- `Character` e `Habit` ainda permitem mutabilidade excessiva;
- a Carteira precisa evoluir para contas, reservas e categorias antes da migração relacional.

## Critérios de conclusão

- build sem warnings;
- todos os testes passando;
- saves anteriores migrando para o schema 4;
- menu sem Visão geral ou Mundo;
- Personagem sem opção ou tabela visível de Progressão;
- documentação coerente com a implementação.
