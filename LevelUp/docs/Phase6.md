# Fase 6 — Confiabilidade e composição

## Objetivo

Fortalecer a base do LevelUp sem introduzir Vida, Energia, combate ou economia fictícia.

## Entregas implementadas

### 6.0 — Baseline e higiene

- branch dedicada;
- remoção de artefatos `.vs`, `bin` e `obj` dos pacotes de entrega;
- cobertura configurada com Coverlet;
- CI preparado para coletar cobertura.

### 6.1 — Composição e estado

- `GameSession` agrupa o estado e os serviços da sessão;
- `ApplicationBootstrap` centraliza a composição;
- `Program.cs` contém somente inicialização e execução;
- `GameStateService` passa a depender de `GameSession`.

### 6.2 — Persistência confiável

- `SchemaVersion` em `GameData`;
- migrações incrementais em `Services/Persistence/Migrations`;
- migração de projetos antigos sem Chefe final;
- `GameDataValidator` valida IDs e relacionamentos;
- escrita atômica com arquivo temporário;
- cópia `save.json.previous` do último snapshot válido.

### 6.3 — Carteira auditável

- lançamentos permanecem no histórico;
- correções usam estorno;
- estornos referenciam a movimentação original;
- a interface não oferece edição ou exclusão de lançamentos confirmados.

### 6.4 — Biblioteca

- livros novos começam na página zero;
- o primeiro registro contabiliza todas as páginas efetivamente lidas;
- o histórico permanece compatível com estatísticas futuras.

## Critérios de aceite

- restore, format, build e testes sem erros;
- saves antigos migrados automaticamente;
- falha durante escrita não destrói o último save válido;
- estorno mantém movimentação original e compensação;
- documentação e CI atualizados.

## Itens planejados

- extração dos fluxos das telas com mais de 300 linhas;
- categorias e contas da Carteira;
- relatórios sob demanda, sem uma tela geral obrigatória;
- conquistas sistêmicas;
- substituição gradual de mensagens de domínio por códigos de erro;
- adoção ampla de `TimeProvider` e `DateTimeOffset`.
