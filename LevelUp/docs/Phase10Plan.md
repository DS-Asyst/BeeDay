# Fase 10 — SQLite totalmente relacional

## Estado

Implementada na camada de persistência.

## Objetivo

Eliminar a persistência documental e qualquer serialização JSON em tempo de execução. Cada conceito do domínio passa a ser representado por colunas e relacionamentos relacionais nativos no SQLite.

## Estrutura

- `Characters`: dados básicos e progressão dos seis atributos.
- `CharacterTitles`: títulos do personagem em relação 1:N.
- `Projects`, `Milestones`, `Quests` e `Bosses`: progressão de projetos com chaves estrangeiras.
- `Books` e `BookProgressEntries`: livros e histórico de leitura em relação 1:N.
- `WalletTags` e `WalletTransactions`: carteira com tag e autorreferência para estorno.
- `Habits` e `Achievements`: tabelas relacionais próprias.
- `GameMetadata`: versão do schema e revisão do save.

Não existem `AggregateDocument`, `Payload`, repositórios documentais nem `JsonSerializer` no fluxo de persistência.

## Ciclo de vida do DbContext

Cada operação `Load` ou `Save` cria e descarta seu próprio `LevelUpDbContext`. Isso evita estado rastreado entre operações e elimina a necessidade de `ChangeTracker.Clear()`.

## Migração

A migration `Phase10RelationalSqlite` substitui as antigas tabelas documentais pelas tabelas relacionais. A mudança é destrutiva para bancos da Fase 9: os documentos antigos são removidos, pois não há mais desserialização JSON no runtime.

Antes de aplicar sobre um banco importante, faça uma cópia do arquivo `levelup.db`.

## Validação

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project LevelUp.Console
```

No DB Browser for SQLite, valide que `Projects` possui colunas como `Name`, `Description`, `PrimaryAttribute` e `Status`, e que nenhuma tabela possui a coluna `Payload`.
