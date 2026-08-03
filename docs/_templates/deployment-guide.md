<!--
Template: Deployment Guide
Uso: documentar um procedimento operacional (deploy, rollback, configuração de ambiente).
Nomenclatura do arquivo: NN-titulo-em-kebab-case.md (NN = número de ordem de leitura na pasta).
-->

# [Nome do procedimento]

**Fonte da verdade:** [obrigatório — ex.: "Verificado diretamente em
`.github/workflows/deploy-prd.yml` e `scripts/Deploy-BeeDay.ps1`"]

## Objetivo

[O que este procedimento realiza e quando ele deve ser executado.]

## Escopo

[Ambientes afetados (hmg/prd). O que este guia explicitamente não cobre.]

## Pré-requisitos

[Acessos, segredos, ferramentas necessárias antes de executar.]

## Estrutura

### Passo a passo

1. [...]

### Variáveis de ambiente/segredos envolvidos

| Nome | Onde é definido | Obrigatório |
|---|---|---|

### Rollback

[Como reverter, se o procedimento falhar.]

### Verificação pós-execução

[Como confirmar que o procedimento funcionou — health checks, logs, etc.]

## Exemplo

[Comando real ou trecho de workflow, com caminho de arquivo.]

## Convenções de nomenclatura

[Convenção de nome de site IIS/app pool/variável de ambiente usada neste projeto.]
