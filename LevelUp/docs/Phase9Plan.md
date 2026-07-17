# Fase 9 — Persistência relacional

## Objetivo

Migrar a persistência operacional de JSON para SQLite com Entity Framework Core somente depois que o domínio e os fluxos atuais estiverem estabilizados.

## Pré-requisitos

- build sem avisos e suíte de testes verde;
- serviços sem dependência de UI;
- regras de progressão centralizadas em workflows e serviços;
- carteira usando movimentações assinadas e tags;
- schema JSON versionado e validado;
- política de backup e importação definida.

## Etapas propostas

1. Criar o projeto `LevelUp.Infrastructure`.
2. Introduzir interfaces de repositório por agregado.
3. Adicionar EF Core e SQLite.
4. Criar `LevelUpDbContext` e configurações Fluent API.
5. Mapear Personagem, Hábitos, Projetos, Capítulos, Missões e Chefes.
6. Mapear Biblioteca, Carteira, Tags e Conquistas.
7. Criar constraints, índices e políticas de exclusão.
8. Criar importador idempotente do `save.json`.
9. Testar paridade JSON × SQLite.
10. Manter exportação JSON como backup.
11. Tornar SQLite o armazenamento padrão após validação real.
12. Preparar comandos e consultas para a futura interface Blazor.

## Banco recomendado

SQLite + Entity Framework Core enquanto o LevelUp for local e de usuário único. PostgreSQL passa a ser considerado apenas quando houver API, sincronização ou múltiplos usuários.
