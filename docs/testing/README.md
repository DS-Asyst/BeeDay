# Testing

Estratégia e infraestrutura de testes: pirâmide de testes (Domain/Application/Infrastructure/Web/
E2E), banco de teste real (LocalDB), testes arquiteturais de fronteira, e fluxo de execução local/CI.

**Fonte da verdade:** reconstruído por completo na Sprint 16.9 a partir de `tests/*`, `BeeDay.slnx`
e execução real de `dotnet test BeeDay.slnx --configuration Release --no-build`.

**Última verificação:** 2026-08-11 (Sprint 20.5, EPIC 20) — contagem atualizada para 768 testes
aprovados (93 Domain, 73 Application, 129 Infrastructure, 464 Web, 9 E2E), refletindo a cobertura
nova de `PublicHeader`/`Home` (EPIC 20). Ver [`01-testing-strategy.md`](01-testing-strategy.md) §1
para o detalhamento por projeto.

## Documentos

| Documento | Status |
|---|---|
| [`01-testing-strategy.md`](01-testing-strategy.md) | Correto — reconstruído na Sprint 16.9; nomenclatura `LevelUp` residual e comandos desatualizados do documento anterior foram corrigidos. |

## Ordem de leitura recomendada

1. `01-testing-strategy.md`
2. [`docs/web/06-testing.md`](../web/06-testing.md) — mapeamento componente→teste específico de
   `BeeDay.Web.Tests`/`BeeDay.E2E.Tests` (Sprint 16.7).
