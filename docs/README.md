# LevelUp — Documentação Técnica Contract-First

**Status:** arquitetura-alvo aprovada para planejamento  
**Escopo:** aplicação LevelUp completa  
**Plataforma atual:** .NET 10, Blazor Server, Clean Architecture pragmática, MediatR, persistência JSON  
**Plataforma-alvo:** contratos estáveis, EF Core, SQL Server e banco iniciado sem dados legados

## 1. Objetivo

Esta documentação redefine a base técnica do LevelUp a partir do código existente. O objetivo é preparar toda a aplicação para substituir a persistência JSON por banco relacional sem acoplar os casos de uso, a interface Blazor ou o domínio ao Entity Framework Core.

A estratégia adotada é **Contract-First Development**:

1. o contrato é definido antes da implementação;
2. consumidores dependem de contratos, não de adapters;
3. domínio, UI, persistência e transporte não compartilham modelos por conveniência;
4. alterações incompatíveis exigem versão ou migração explícita;
5. testes verificam o contrato independentemente da implementação;
6. EF Core e JSON são adapters substituíveis durante a transição;
7. o novo banco será criado vazio, sem importação de `LevelUpBD.json` ou backups JSON.

## 2. Decisões principais

- Criar o projeto `LevelUp.Contracts`.
- Remover `LevelUpData` como contrato de aplicação.
- Substituir `ILevelUpRepository` por portas específicas por agregado e por consulta.
- Separar contratos públicos, commands, domain entities e persistence models.
- Adotar respostas tipadas e um catálogo único de erros.
- Manter JSON somente como adapter temporário de compatibilidade funcional.
- Não criar rotina de migração de dados JSON → SQL.
- Iniciar SQL Server com migrations novas e base vazia.
- Introduzir EF Core apenas após os contratos e testes de contrato estarem estabilizados.
- Remover o conceito persistido de `CurrentUserId`.
- Adicionar rate limiting de login, invalidação de sessões e testes E2E antes da exposição pública.

## 3. Índice

### Arquitetura

- [Estado atual](architecture/01-current-state.md)
- [Arquitetura-alvo](architecture/02-target-architecture.md)
- [Regras de dependência](architecture/03-dependency-rules.md)
- [Fluxos de execução](architecture/04-runtime-flows.md)

### Contract-First

- [Padrão Contract-First](contracts/01-contract-first-standard.md)
- [Catálogo de contratos](contracts/02-contract-catalog.md)
- [Erros e versionamento](contracts/03-errors-and-versioning.md)
- [Estrutura proposta em C#](contracts/04-csharp-structure.md)

### Dados

- [Modelo relacional](data/01-relational-model.md)
- [Estratégia EF Core](data/02-ef-core-strategy.md)
- [Transição JSON → SQL sem dados](data/03-json-to-sql-transition.md)

### Segurança e qualidade

- [Baseline de segurança](security/01-security-baseline.md)
- [Estratégia de testes](testing/01-testing-strategy.md)

### Operação

- [HMG, produção e observabilidade](operations/01-operations.md)
- [Backup, restore e continuidade](operations/02-backup-and-restore.md)

### Execução

- Roadmap técnico, backlog por sprint e Definition of Done: ainda não redigidos (`docs/roadmap/` não existe nesta branch). Criar antes de linkar aqui.

### Decisões arquiteturais

- [ADR-001 — Contract-First](adr/ADR-001-contract-first.md)
- [ADR-002 — Banco novo sem dados JSON](adr/ADR-002-greenfield-database.md)
- [ADR-003 — Repositórios por agregado](adr/ADR-003-aggregate-repositories.md)

### Contrato HTTP futuro

- [OpenAPI inicial](openapi/levelup.v1.yaml)

## 4. Ordem obrigatória de implementação

1. corrigir riscos atuais;
2. criar contratos e testes de contrato;
3. adaptar a implementação JSON aos novos contratos;
4. introduzir EF Core e SQL Server;
5. executar testes de conformidade nos dois adapters;
6. trocar o adapter ativo por configuração;
7. iniciar o banco vazio;
8. remover JSON após estabilização;
9. automatizar HMG e fortalecer operação.

## 5. Fora do escopo

- importar usuários ou dados do JSON;
- manter identificadores antigos;
- realizar dual-write entre JSON e SQL;
- expor todas as operações como API pública imediatamente;
- introduzir microserviços;
- substituir Blazor Server nesta etapa;
- implementar Event Sourcing.

## 6. Resultado esperado

Ao final, a aplicação deverá permitir trocar:

```text
Json adapters → EF Core adapters
```

sem alterar:

- contratos de entrada e saída;
- componentes Blazor;
- handlers dos casos de uso, salvo composição de dependências;
- regras de domínio;
- catálogo de erros;
- testes de comportamento.
