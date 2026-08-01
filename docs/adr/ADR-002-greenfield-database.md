# ADR-002 — Criar Banco Novo sem Importar JSON

**Status:** Aceito pelo proprietário do projeto  
**Data:** 2026-07-31

## Contexto

O projeto possui dados em JSON e migrations internas de schema. O proprietário decidiu que o banco relacional começará com dados novos.

## Decisão

Não implementar importação, dual-write ou compatibilidade de dados entre JSON e SQL Server.

## Consequências

- contas e dados recomeçam do zero;
- menor complexidade e risco de corrupção na migração;
- IDs antigos não são preservados;
- cookies e tokens antigos devem ser invalidados;
- comunicação clara aos usuários será obrigatória;
- JSON final será tratado como backup histórico, não como fonte ativa.
