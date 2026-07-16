# Fase 8.5 — Consolidação e polimento

## Objetivo

Estabilizar a base construída até a Fase 8 antes de introduzir persistência relacional ou uma nova interface. Esta fase não adiciona novos módulos de produto; ela reduz risco, melhora a manutenção e prepara decisões futuras.

## Entregas desta versão

- correção do teste de metas que utilizava argumentos posicionais incompatíveis com `GameSession`;
- extração de `QuestSelectionFlow`, retirando seleção de Projetos, Capítulos e Missões da `QuestScreen`;
- redução do acoplamento direto de `QuestScreen` com `MilestoneService`;
- testes adicionais para IDs duplicados e Capítulos órfãos;
- limpeza de artefatos locais de build e saves temporários do pacote;
- consolidação do roadmap e remoção de fases duplicadas na documentação;
- definição de critérios objetivos para iniciar a Fase 9.

## Princípios adotados

1. Não reescrever a aplicação inteira durante uma fase de polimento.
2. Extrair fluxos com responsabilidade clara e alto volume de código.
3. Preservar regras de domínio e comportamento já validados.
4. Aumentar testes em torno de persistência e relacionamentos.
5. Adiar banco de dados até existir motivação mensurável.

## Dívidas ainda conhecidas

- `QuestScreen`, `TrainingScreen`, `LibraryScreen` e `WalletScreen` ainda podem ser divididas em flows menores;
- o domínio usa `DateTime.Now` diretamente;
- mensagens de exceção do domínio ainda estão em português;
- `Character` e `Habit` ainda permitem mutabilidade excessiva;
- a Carteira precisa evoluir para contas, reservas e categorias antes da migração relacional;
- o módulo de metas precisa ser validado como produto antes de ganhar desafios recorrentes e temporadas.

## Critérios de conclusão

- build sem warnings;
- todos os testes passando;
- saves antigos carregando após migrações;
- nenhum artefato `bin`, `obj`, `.vs` ou save local no pacote de código-fonte;
- documentação oficial coerente com a implementação;
- branch da Fase 9 criada apenas após a validação dos critérios de prontidão.
