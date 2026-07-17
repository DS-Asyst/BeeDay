# Architecture

## Camadas

- **Domain**: entidades e regras locais. `Character.ApplyReward()` é o único ponto autorizado a alterar progressão.
- **Services**: gerenciamento de coleções, validações e transições de estado.
- **Workflows**: coordenação de casos de uso que atravessam entidades, aplicação de recompensas e persistência.
- **UI**: entrada e apresentação; não concede XP nem ativa entidades diretamente.
- **Persistence**: serialização, validação e migração do estado.

## Recompensas

Toda atividade retorna `Reward`. O objeto pode transportar XP geral, atributo associado, XP de atributo e títulos. Workflows aplicam o resultado ao personagem e persistem a sessão.

## Progressão de projeto

Projeto → Capítulos → Quests → Boss final. O projeto define `PrimaryAttribute`; quests vinculadas herdam esse valor. A ativação ocorre por `ProjectWorkflowService`, `MilestoneWorkflowService`, `QuestWorkflowService` e `BossWorkflowService`.

## Limite arquitetural da Fase 8.5

O armazenamento permanece em JSON. As regras de negócio não dependem do mecanismo de persistência, permitindo substituir o `IGameDataStore` por EF Core na Fase 9.
