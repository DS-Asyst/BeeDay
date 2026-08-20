# Testing

Estratégia e infraestrutura de testes: pirâmide de testes (Domain/Application/Infrastructure/Web/
E2E), banco de teste real (LocalDB), testes arquiteturais de fronteira, e fluxo de execução local/CI.

**Fonte da verdade:** reconstruído por completo na Sprint 16.9 a partir de `tests/*`, `BeeDay.slnx`
e execução real de `dotnet test BeeDay.slnx --configuration Release --no-build`.

**Última verificação:** 2026-08-16 (Sprint 25.16, EPIC 25 — gate final) — 1.116 testes, 0 falhas
reais (93 Domain, 73 Application, 129 Infrastructure, 741 Web, 80 E2E), reconfirmado por execução
completa em Debug e Release nesta Sprint. A primeira passada em Debug reportou uma falha isolada em
`ActivityFilterBarTests.SharedSearchInputPreservesTheDebouncedFilterContract` (bUnit
`WaitForAssertion` sob contenção da suíte completa); retry isolado da classe: 3/3 aprovados — mesmo
padrão de contenção já registrado neste documento (§7), não uma regressão. O inventário histórico
detalhado por arquivo em [`01-testing-strategy.md`](01-testing-strategy.md) está reconciliado.

> **Nota da EPIC 30:** essas contagens são um baseline histórico e já não representam a suíte
> atual. A reconciliação numérica permanece aberta como `BD30-F001` para a Sprint 30.24; a matriz
> funcional abaixo não antecipa esse trabalho.

## Documentos

| Documento | Status |
|---|---|
| [`01-testing-strategy.md`](01-testing-strategy.md) | Histórico — estratégia válida; inventário numérico pendente de reconciliação por `BD30-F001` na Sprint 30.24. |
| [`02-design-system-quality-gates.md`](02-design-system-quality-gates.md) | Atual — axe, contraste, localização, estratégia visual/responsiva, artefatos e integração CI da Sprint 25.15. |
| [`03-functional-journey-matrix.md`](03-functional-journey-matrix.md) | Atual — ownership, evidência por camada e gaps das jornadas suportadas, auditados na Sprint 30.4. |

## Ordem de leitura recomendada

1. `01-testing-strategy.md`
2. `02-design-system-quality-gates.md`
3. `03-functional-journey-matrix.md`
4. [`docs/web/06-testing.md`](../web/06-testing.md) — mapeamento componente→teste específico de
   `BeeDay.Web.Tests`/`BeeDay.E2E.Tests` (Sprint 16.7).
