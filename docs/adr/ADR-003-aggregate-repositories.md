# ADR-003 — Repositórios por Agregado e Read Services

**Status:** Proposto para aprovação  
**Data:** 2026-07-31

## Contexto

`ILevelUpRepository` trabalha com todo o documento. Um repositório genérico também não expressaria adequadamente ownership, consultas e invariantes.

## Decisão

Criar repositórios orientados a agregados para escrita e read services orientados a projeções para consulta.

## Exemplos

- `IUserRepository`
- `IActivityRepository`
- `IProjectRepository`
- `IWalletRepository`
- `IExperienceRepository`
- `IDashboardReadService`
- `IWalletReadService`
- `IUnitOfWork`

## Consequências

- contratos mais expressivos;
- melhor teste;
- queries eficientes;
- menor dependência do ORM;
- maior número de interfaces, controlado por feature.
