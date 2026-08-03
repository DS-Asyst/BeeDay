<!--
Template: Feature
Uso: documentar uma Feature de Application (ex.: Habits, Wallet, Identity) como um todo — os casos
de uso que ela agrupa. Para um caso de uso individual, use o template Use Case.
Nomenclatura do arquivo: nome-da-feature-em-kebab-case.md (ex.: Habits -> habits.md).
-->

# [NomeDaFeature]

**Fonte da verdade:** [obrigatório — ex.: "Verificado diretamente em
`src/BeeDay.Application/Features/NomeDaFeature/`"]

## Objetivo

[Que capacidade do produto esta Feature entrega.]

## Escopo

[Quais casos de uso pertencem a esta Feature. Quais Aggregates ela orquestra.]

## Pré-requisitos

[Aggregates envolvidos — linkar para os documentos correspondentes em `docs/domain/`.]

## Estrutura

### Casos de uso

| Caso de uso | Command/Query | Handler |
|---|---|---|

### Contratos (Requests/Responses)

[Listar os principais, com caminho de arquivo.]

### Regras de autorização/ownership

[Como esta Feature garante que um usuário só acessa seus próprios dados.]

## Exemplo de fluxo ponta a ponta

[UI -> Command/Query -> Handler -> Repositório, com caminhos de arquivo reais.]

## Convenções de nomenclatura

[Convenção de nome de Command/Query/Handler/Request/Response usada nesta Feature.]

## Testes relacionados

[Caminho dos testes de Application e Web relevantes.]
