# Fase 9 — Persistência SQLite exclusiva

## Estado

Implementada. O SQLite é a única fonte de verdade do LevelUp.

## Regras operacionais

1. O banco padrão fica em `%LOCALAPPDATA%\LevelUp\levelup.db`.
2. A pasta e o banco são criados automaticamente.
3. As migrations do EF Core são aplicadas na inicialização.
4. Banco sem personagem inicia o fluxo de criação do primeiro personagem.
5. Não existe importação automática de `save.json`.
6. Não existe exportação automática de backup JSON.
7. Não existe fallback de persistência na camada de aplicação.
8. Evoluções de esquema usam migrations; correções controladas de dados podem usar scripts SQLite.

## Estrutura histórica

Na Fase 9, cada agregado era armazenado como documento dentro do SQLite. Essa implementação foi substituída na Fase 10 pelo modelo totalmente relacional descrito em `Phase10Plan.md`.

## Validação esperada

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project LevelUp.Console
```

Fluxo manual mínimo: criar personagem, criar e concluir missão, fechar o programa, abrir novamente e confirmar a persistência.
