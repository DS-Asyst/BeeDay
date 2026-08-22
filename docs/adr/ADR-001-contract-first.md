# ADR-001 — Adotar Contract-First

**Status:** Aceito — princípio adotado a partir da Sprint 13.1; adoção em código parcial (contratos
por Aggregate definidos, dois read services adotados, nenhum handler de escrita migrado). Ver
`docs/history/migration-status.md` (caminho atualizado na Sprint 16.10 — o arquivo era
`docs/architecture/08-migration-status.md` até a reorganização de taxonomia da Sprint 16.2; ver
[`docs/history/README.md`](../history/README.md)). **Superseded quanto ao mecanismo por
[ADR-007](ADR-007-in-process-application-contracts.md) (2026-08-22)** — o projeto `LevelUp.Contracts`
separado decidido abaixo nunca foi criado; o objetivo de Contract-First foi alcançado por interfaces
internas de `BeeDay.Application`, forma que o ADR-007 documenta formalmente. O princípio de
Contract-First em si (não o mecanismo de projeto separado) permanece válido.  
**Data:** 2026-07-31

## Contexto

A Application depende atualmente de um contrato de repositório que retorna o documento global `LevelUpData`. A futura migração para SQL Server pode propagar detalhes de persistência para handlers e UI.

## Decisão

Adotar Contract-First em todas as fronteiras e criar `LevelUp.Contracts` como projeto independente.

## Consequências positivas

- contratos estáveis;
- troca de adapter com menor impacto;
- testes de conformidade;
- preparação para API futura;
- redução de vazamento de entidades;
- versionamento explícito.

## Consequências negativas

- mais tipos e mappers;
- custo inicial de refatoração;
- necessidade de governança;
- risco de contratos excessivamente genéricos se mal definidos.

## Restrições

- não criar contratos que espelhem tabelas;
- não expor entidades;
- não criar repositório genérico como solução universal;
- não introduzir microserviços.
