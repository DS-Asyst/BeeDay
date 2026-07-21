# Etapa 7 — FluentValidation

## Escopo entregue

- FluentValidation 12.1.1 centralizado pelo Central Package Management.
- Validadores na camada Application para Profile, Habit, Task, To-Do, Project e ordenação.
- Registro automático dos validadores no contêiner de DI.
- Validação obrigatória antes de qualquer mutação do repositório.
- `ApplicationValidationException` com erros agrupados por campo.
- Integração com `ProblemDetails` por meio de `ValidationProblemDetails` e HTTP 400.
- Testes unitários para regras válidas e inválidas.

## Decisão arquitetural

As invariantes continuam protegidas no Domain. FluentValidation atua na fronteira da Application, fornecendo mensagens por campo e rejeitando requests inválidos antes de acessar a persistência.

As anotações atuais dos modelos Blazor foram mantidas nesta etapa para preservar a validação imediata dos formulários. A fonte de verdade do backend passa a ser os validadores da Application. A futura Etapa 8 poderá executar os mesmos validadores em um `ValidationBehavior` do MediatR sem reescrever regras.

## Fora do escopo

- MediatR e pipeline behaviors.
- Validação remota ou assíncrona dependente de banco de dados.
- Alterações nas regras de negócio existentes.
- Configuração de IIS ou Docker.
