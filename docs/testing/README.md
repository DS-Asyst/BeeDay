# Testing

Estratégia e infraestrutura de testes: pirâmide de testes (Domain/Application/Infrastructure/Web/
E2E), banco de teste real (LocalDB), testes arquiteturais de fronteira, e fluxo de execução local/CI.

**Fonte da verdade:** reconstruído por completo na Sprint 16.9 a partir de `tests/*`, `BeeDay.slnx`
e execução real de `dotnet test BeeDay.slnx --configuration Release --no-build`.

**Última verificação:** 2026-08-21 (Sprint 31.9, EPIC 31) — **1.557 testes, 0 falhas**
(121 Domain, 119 Application, 216 Infrastructure, 880 Web, 221 E2E), confirmado por execução real de
`dotnet test BeeDay.slnx` completa em Debug ao início da Sprint 31.1. O baseline de 1.554
(121/119/216/879/219), registrado ao final da Sprint 30.23, já estava 3 testes desatualizado
(Web +1, E2E +2) — a suíte continuou crescendo organicamente nos dias seguintes. O inventário
histórico detalhado por arquivo em [`01-testing-strategy.md`](01-testing-strategy.md) foi reconciliado
para o mesmo total.

## Documentos

| Documento | Status |
|---|---|
| [`01-testing-strategy.md`](01-testing-strategy.md) | Atual — estratégia e inventário numérico reconciliados na Sprint 30.24 (`BD30-F001`). |
| [`02-design-system-quality-gates.md`](02-design-system-quality-gates.md) | Atual — axe, contraste, localização, estratégia visual/responsiva, artefatos e integração CI da Sprint 25.15. |
| [`03-functional-journey-matrix.md`](03-functional-journey-matrix.md) | Atual — ownership, evidência por camada e gaps das jornadas suportadas, auditados na Sprint 30.4. |

## Ordem de leitura recomendada

1. `01-testing-strategy.md`
2. `02-design-system-quality-gates.md`
3. `03-functional-journey-matrix.md`
4. [`docs/web/06-testing.md`](../web/06-testing.md) — mapeamento componente→teste específico de
   `BeeDay.Web.Tests`/`BeeDay.E2E.Tests` (Sprint 16.7).
