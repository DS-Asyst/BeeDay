# Arquitetura

## Camadas

### Domain
Entidades, estados, invariantes e cálculos sem dependência da interface.

### Services
Serviços por feature e workflows que coordenam múltiplos domínios.

### UI
Telas, componentes, prompts, temas e tradução da experiência para português.

### Persistence
`IGameDataStore`, `SaveService` e `GameStateService` persistem um snapshot completo em JSON.

## Módulos

- Character
- Habits / Training
- Projects / Quests / Milestones / Bosses
- Achievements
- Books
- Wallet

## Fluxo de conclusão de projeto

1. O usuário cria um Projeto com um Chefe final e um prefixo de conquista.
2. Capítulos e Missões representam o progresso da jornada.
3. Ao concluir todas as Missões e Capítulos, o Chefe final é desbloqueado.
4. Ao derrotar o Chefe, o Projeto é concluído.
5. `AchievementService` gera uma conquista profissional, como `Desenvolvedor ASP.NET Core`.
6. `GameStateService` persiste o novo estado.

## Compatibilidade

Algumas sobrecargas antigas permanecem temporariamente para leitura de testes e saves anteriores. Novos fluxos devem usar chefes por Projeto e conquistas persistidas.


## Fase 6

A composição da aplicação foi movida para `ApplicationBootstrap`. `GameSession` reúne o estado em memória e reduz o crescimento do construtor de `GameStateService`. A persistência possui migrações e validação antes de materializar a sessão.

```text
Program
  -> ApplicationBootstrap
      -> GameSession
      -> Workflows
      -> Screens

SaveService
  -> Normalize
  -> GameDataMigrator
  -> GameDataValidator
  -> GameSession
```
