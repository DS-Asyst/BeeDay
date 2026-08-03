# BeeDay Documentation

Índice principal da documentação do BeeDay. A documentação descreve o sistema como ele é hoje e as
decisões que o trouxeram até aqui — não a ordem histórica em que as funcionalidades foram
implementadas. A exceção é [`adr/`](adr/README.md): cada ADR é um registro imutável do momento em
que uma decisão foi tomada.

**Fonte da verdade:** esta taxonomia foi definida na Sprint 16.2 verificando diretamente
`BeeDay.slnx`, `src/*`, `tests/*` e o inventário de documentos existentes auditado na Sprint 16.1.

## Regra permanente desta documentação

Todo documento deve declarar explicitamente sua fonte de verdade — por exemplo:
"Verificado diretamente em `src/BeeDay.Infrastructure/...`", "Baseado na implementação atual de
`BeeDayDbContext`", "Extraído dos contratos em `Application/Common/Contracts`", "Derivado dos
testes em `tests/BeeDay.Web.Tests`". Ver [`CONVENTIONS.md`](CONVENTIONS.md).

## Áreas

| Área | Conteúdo | Status |
|---|---|---|
| [`architecture/`](architecture/README.md) | Visão geral, estrutura da solução, camadas, dependências, runtime, persistência, segurança, deployment | Correto — reconstruído por completo na Sprint 16.3 a partir do código atual |
| [`domain/`](domain/README.md) | Aggregates, Entities, Value Objects, Domain Events, Business Rules | Correto — reconstruído por completo na Sprint 16.4 a partir do código atual |
| [`application/`](application/README.md) | Casos de uso, Requests/Responses | Parcialmente correto/obsoleto — reescrita pendente |
| [`infrastructure/`](infrastructure/README.md) | Caching, background jobs, health checks, Identity técnica | Reservado — sem conteúdo próprio ainda |
| [`web/`](web/README.md) | Composition root, páginas Blazor Server | Reservado — sem conteúdo próprio ainda |
| [`persistence/`](persistence/README.md) | Modelo relacional, estratégia EF Core | Correto |
| [`authentication/`](authentication/README.md) | Cookies, confirmação de e-mail, rate limiting | Reservado — ver `security/` enquanto isso |
| [`security/`](security/README.md) | Baseline de segurança | Correto |
| [`deployment/`](deployment/README.md) | Ambiente, backup/restore, CI/CD | Correto/parcialmente correto |
| [`testing/`](testing/README.md) | Estratégia e infraestrutura de testes | Parcialmente correto |
| [`design-system/`](design-system/README.md) | Design System Blazor, Pixel Icon System | Reservado — sem conteúdo próprio ainda |
| [`ux/`](ux/README.md) | Fluxos de usuário e jornadas | Reservado — ver `architecture/05-runtime-flows.md` enquanto isso |
| [`api/`](api/README.md) | Especificação OpenAPI | Não reauditado quanto ao conteúdo |
| [`adr/`](adr/README.md) | Registros de decisão arquitetural | Correto (histórico, imutável) |
| [`developer/`](developer/README.md) | Guia de contribuição, setup de ambiente | Reservado — ver `README.md` da raiz enquanto isso |
| [`history/`](history/README.md) | Diários de sprint e transições já concluídas | Correto (histórico, congelado) |

## Templates

Modelos oficiais para criar novos documentos de cada tipo vivem em [`_templates/`](_templates/).

## Ordem de leitura recomendada para quem chega agora ao projeto

1. [`README.md`](../README.md) da raiz — visão geral, stack, como rodar.
2. [`architecture/`](architecture/README.md) — como as camadas se relacionam.
3. [`persistence/`](persistence/README.md) e [`security/`](security/README.md) — como os dados e a
   autenticação funcionam hoje.
4. [`testing/`](testing/README.md) — como validar mudanças.
5. [`adr/`](adr/README.md) — por que as decisões estruturais foram tomadas.
6. [`deployment/`](deployment/README.md) — como o sistema chega a produção.
7. [`history/`](history/README.md) — apenas se precisar entender a jornada até aqui.
