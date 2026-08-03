# Testing

Estratégia e infraestrutura de testes: pirâmide de testes, testes de contrato, testes de
integração Web e testes E2E (Playwright).

**Fonte da verdade:** confirmado nesta sessão executando `dotnet test BeeDay.slnx --configuration
Release --no-build`: 742 testes aprovados (93 Domain, 72 Application, 120 Infrastructure, 450 Web,
7 E2E).

## Documentos

| Documento | Status |
|---|---|
| [`01-testing-strategy.md`](01-testing-strategy.md) | Parcialmente correto — infraestrutura de teste descrita é real e precisa; comandos (`dotnet test LevelUp.slnx`) e nomenclatura residual "LevelUp" pendentes de atualização. |

## Ordem de leitura recomendada

1. `01-testing-strategy.md`
