# Fase 6 — Inteligência, confiabilidade e visão integrada

## Objetivo

A Fase 6 transforma os dados acumulados pelo LevelUp em informação confiável e útil, sem iniciar ainda Vida, Energia, combate ou economia fictícia.

## Entregas implementadas

### 6.0 — Baseline e higiene

- branch dedicada `feature/phase-6-intelligence-reliability`;
- remoção de artefatos `.vs`, `bin` e `obj` do pacote de entrega;
- cobertura configurada com Coverlet;
- CI preparado para coletar cobertura.

### 6.1 — Composição e estado

- `GameSession` agrupa o estado e os serviços da sessão;
- `ApplicationBootstrap` centraliza a composição;
- `Program.cs` contém somente inicialização de encoding e execução;
- `GameStateService` passa a depender de `GameSession`.

### 6.2 — Persistência confiável

- `SchemaVersion` em `GameData`;
- migrações incrementais em `Services/Persistence/Migrations`;
- migração de projetos antigos sem Chefe final;
- `GameDataValidator` valida IDs e relacionamentos;
- escrita atômica com arquivo temporário;
- cópia `save.json.previous` do último snapshot válido.

### 6.3 — Carteira auditável

- lançamentos continuam preservados no histórico;
- correções passam a usar estorno;
- estornos referenciam a movimentação original;
- a interface deixa de oferecer edição e exclusão de lançamentos confirmados.

### 6.4 — Visão geral

- novo dashboard no menu principal;
- resumo de personagem, diário, biblioteca, carteira e reconhecimento;
- cálculos realizados por `DashboardService`, sem lógica duplicada na UI.

### 6.5 — Biblioteca

- livros novos começam na página zero;
- o primeiro registro contabiliza todas as páginas efetivamente lidas;
- o histórico permanece compatível com estatísticas futuras.

## Critérios de aceite

- restore, format, build e testes sem erros;
- saves antigos migrados automaticamente;
- falha durante escrita não destrói o último save válido;
- dashboard não altera estado;
- estorno mantém movimentação original e compensação;
- documentação e CI atualizados.

## Itens planejados para os próximos incrementos

- extração dos fluxos das telas com mais de 300 linhas;
- categorias e contas da Carteira;
- relatórios por período;
- conquistas sistêmicas;
- substituição gradual de mensagens de domínio por códigos de erro;
- adoção ampla de `TimeProvider` e `DateTimeOffset`.
