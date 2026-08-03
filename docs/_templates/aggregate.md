<!--
Template: Aggregate (Domain)
Uso: documentar um Aggregate Root do Domain — suas entidades filhas, invariantes e o repositório
que o serve.
Nomenclatura do arquivo: nome-do-aggregate-em-kebab-case.md (ex.: Habit -> habit.md).
-->

# [NomeDoAggregate]

**Fonte da verdade:** [obrigatório — ex.: "Verificado diretamente em
`src/BeeDay.Domain/Entities/NomeDoAggregate.cs` e `IContratoRepository` em
`src/BeeDay.Application/Common/Contracts/`"]

## Objetivo

[Que conceito de negócio este Aggregate representa.]

## Escopo

[Quais entidades filhas e Value Objects pertencem a este Aggregate. O que explicitamente não
pertence a ele (e a qual Aggregate pertence em vez disso).]

## Pré-requisitos

[Conceitos de DDD assumidos (Aggregate Root, invariante, etc.) — linkar para
`docs/domain/README.md` se houver um glossário.]

## Estrutura

### Invariantes

[Regras de negócio que o Aggregate garante — com o método/arquivo que as impõe.]

### Repositório

[Interface de repositório correspondente e onde ela vive.]

### Eventos de domínio emitidos

| Evento | Quando é emitido | Consumidores |
|---|---|---|

### Transações cross-Aggregate

[Se este Aggregate participa de alguma transação via `IUnitOfWork` com outro Aggregate, listar
aqui.]

## Exemplo

[Trecho real de criação/mutação do Aggregate, com caminho de arquivo.]

## Convenções de nomenclatura

[Convenção de nome de Aggregate Root vs. entidade filha vs. Value Object neste projeto.]

## Testes relacionados

[Caminho dos testes de Domain e de Infrastructure relevantes.]
