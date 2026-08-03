<!--
Template: UX Flow
Uso: documentar uma jornada de usuário ponta a ponta (ex.: criação de conta, onboarding, criação de
hábito). Complementa architecture/*-runtime-flows.md, mas sob a ótica do usuário, não da pilha
técnica.
Nomenclatura do arquivo: nome-da-jornada-em-kebab-case.md.
-->

# [Nome da jornada]

**Fonte da verdade:** [obrigatório — ex.: "Verificado percorrendo o fluxo real em
`src/BeeDay.Web/Components/Features/.../Pages/` e o teste E2E correspondente em
`tests/BeeDay.E2E.Tests/`"]

## Objetivo

[Que necessidade do usuário esta jornada atende.]

## Escopo

[Onde a jornada começa e termina. O que não faz parte dela.]

## Pré-requisitos

[Estado prévio necessário — ex.: conta já criada, e-mail confirmado.]

## Estrutura

### Passos da jornada

1. [Tela/ação do usuário] -> [o que o sistema faz] -> [próxima tela]

### Estados de erro/borda

[O que acontece quando algo dá errado em cada passo.]

### Pontos de decisão

[Onde a jornada se ramifica dependendo de uma escolha do usuário ou estado do sistema.]

## Exemplo

[Referência ao teste E2E que cobre esta jornada, com caminho de arquivo.]

## Convenções de nomenclatura

[Se aplicável — convenção de nome de página/rota.]
