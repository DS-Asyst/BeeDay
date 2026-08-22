# BeeDay Documentation

Índice principal da documentação do BeeDay. A documentação descreve o sistema como ele é hoje e as
decisões que o trouxeram até aqui — não a ordem histórica em que as funcionalidades foram
implementadas. A exceção é [`adr/`](adr/README.md): cada ADR é um registro imutável do momento em
que uma decisão foi tomada.

**Fonte da verdade:** esta taxonomia foi definida na Sprint 16.2 verificando diretamente
`BeeDay.slnx`, `src/*`, `tests/*` e o inventário de documentos existentes auditado na Sprint 16.1.

**`docs/` vs. o `beeday Experience System`:** este diretório é a documentação técnica para quem
desenvolve o repositório — não é publicado, exige acesso ao código-fonte para ser lido. Desde a
Sprint 25.17, `/experience-system` (`beeday Experience System`) é o equivalente público e navegável
dentro do próprio produto: uma representação real, localizada em `en-US`/`pt-BR` e sempre
sincronizada com o código, do que a EPIC 25 formalizou sobre Brand System, UI Design System e UX
System — acessível a qualquer visitante, sem autenticação, a partir do link no footer institucional.
As duas áreas não duplicam conteúdo por cópia manual: `/experience-system` resume e apresenta as
mesmas decisões documentadas em [`design-system/`](design-system/README.md),
[`brand/`](brand/README.md), [`ux/`](ux/README.md) e
[`epics/25-design-system-brand-evolution/`](epics/25-design-system-brand-evolution/README.md) para
um público diferente. Ver [`docs/web/02-routing-and-pages.md`](web/02-routing-and-pages.md) §9 para
a estrutura de rotas e composição.

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
| [`application/`](application/README.md) | CQRS, Use Cases, Pipeline, Contracts, Exceptions, Dependency Flow | Correto — reconstruído por completo na Sprint 16.5 a partir do código atual |
| [`infrastructure/`](infrastructure/README.md) | Repositories, Unit of Work, SQL Server, Concurrency, Event Journal, Identity/Email/Health/Background, Dependency Injection | Correto — reconstruído por completo na Sprint 16.6 a partir do código atual; cache de aplicação removido na Sprint 18.6 (código morto) |
| [`web/`](web/README.md) | Composition root, páginas Blazor Server, Feature components, layouts, integração com Design System, localização en-US/pt-BR | Correto — reconstruído por completo na Sprint 16.7 a partir do código atual; `07-localization.md` adicionado na Sprint 23.9 (EPIC 23) |
| [`persistence/`](persistence/README.md) | Modelo relacional, estratégia EF Core | Correto — reconstruído por completo na Sprint 16.6 a partir do código atual |
| [`authentication/`](authentication/README.md) | Cookies, confirmação de e-mail, rate limiting | Reservado — ver `security/` enquanto isso |
| [`security/`](security/README.md) | Baseline de segurança, segurança operacional | Correto — `02-operational-security.md` reconstruído na Sprint 16.9; nomenclatura residual de `01-security-baseline.md` corrigida na Sprint 16.10 |
| [`deployment/`](deployment/README.md) | Deploy, GitHub Actions, runtime configuration, observabilidade, operações | Correto — reconstruído por completo na Sprint 16.9 a partir do código atual |
| [`testing/`](testing/README.md) | Estratégia e infraestrutura de testes | Correto — reconstruído por completo na Sprint 16.9 a partir do código atual |
| [`design-system/`](design-system/README.md) | Design System Blazor, BeeDay Icon System, Foundations, Componentes, Forms | Atual — governança e contratos revalidados na EPIC 25 |
| [`brand/`](brand/README.md) | Brand System: identidade, personagens, ilustração, escrita e localização | Atual — Character/Illustration e Writing/Voice/Tone formalizados na EPIC 25 |
| [`ux/`](ux/README.md) | Diretrizes de UX, acessibilidade, responsividade | Correto — reconstruído por completo na Sprint 16.8 a partir do código atual |
| [`api/`](api/README.md) | Especificação OpenAPI | Não reauditado quanto ao conteúdo |
| [`adr/`](adr/README.md) | Registros de decisão arquitetural | Correto (histórico, imutável) |
| [`developer/`](developer/README.md) | Guia de contribuição, setup de ambiente | Reservado — ver `README.md` da raiz enquanto isso |
| [`history/`](history/README.md) | Diários de sprint e transições já concluídas | Correto (histórico, congelado) |
| [`epics/20-home-visual-experience/`](epics/20-home-visual-experience/README.md) | EPIC 20 — Home & Visual Experience: decisões aprovadas, discovery transversal, Visual Adoption Map, roadmap de Sprints | Histórico concluído |
| [`epics/21-lingo-product-experience/`](epics/21-lingo-product-experience/README.md) | EPIC 21 — Lingo-Based Product Experience & Design System: especificação de migração Lingo → BeeDay, component mapping, gamification capability matrix | Histórico concluído |
| [`epics/25-design-system-brand-evolution/`](epics/25-design-system-brand-evolution/README.md) | EPIC 25 — beeday Design System & Brand System Evolution: contrato de marca, governança, implementação e gate final | Concluída na Sprint 25.16 |
| [`epics/28-transactional-email-experience/`](epics/28-transactional-email-experience/README.md) | EPIC 28 — Transactional Email Experience, Deliverability & Observability: baseline, contrato de localização, composição, deliverability e fechamento consolidado | `IMPLEMENTATION READY — POST-MERGE HMG VALIDATION PENDING` (status próprio da Epic, na Sprint 28.10) |
| [`epics/30-system-integrity/`](epics/30-system-integrity/README.md) | EPIC 30 — System Integrity & Complete Engineering Audit: inventário versionado, Audit Ledger, findings e ownership por Sprint | Em andamento — baseline criado na Sprint 30.1 |
| [`epics/31-documentation-knowledge-consolidation/`](epics/31-documentation-knowledge-consolidation/README.md) | EPIC 31 — Repository Documentation & Knowledge Consolidation: Documentation Ledger, ADR validity baseline, reconciliação por área | Em andamento — reconciliação de `docs/architecture/` e `docs/domain/` concluída (Sprints 31.1–31.5) |

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
