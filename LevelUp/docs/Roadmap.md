# Roadmap

## Fases concluídas

- Fase 1 — Personagem e hábitos
- Fase 2 — Treinamentos e interface Spectre.Console
- Fase 3 — Projetos e Missões
- Fase 4 — Capítulos, Chefes e consolidação de UX
- Fase 5 — Diário, Biblioteca, Mochila, Carteira e reconhecimento
- Fase 6 — Confiabilidade, composição e persistência versionada
- Fase 7 — Progressão narrativa do personagem na Ficha
- Fase 8.5 — Consolidação e polimento

## Decisões de produto consolidadas

- a progressão do personagem aparece exclusivamente na Ficha;
- não existe tela Visão geral no menu principal;
- o módulo Mundo/Metas foi removido;
- JSON versionado permanece como persistência oficial;
- banco de dados só será adotado com motivação concreta.

## Próximos incrementos de polimento

- [ ] extrair flows adicionais de Missões;
- [ ] modularizar Treinamentos, Biblioteca e Carteira;
- [ ] encapsular mutabilidade de `Character` e `Habit`;
- [ ] introduzir `TimeProvider` e avaliar `DateTimeOffset`;
- [ ] criar códigos de erro de domínio localizáveis;
- [ ] revisar política única de salvamento;
- [ ] ampliar cobertura de workflows críticos;
- [ ] adicionar contas, reservas e categorias à Carteira.

## Fase 9 — Persistência relacional

A Fase 9 depende dos critérios registrados em `Phase9Plan.md`.

### Etapas planejadas

- [ ] preparar fronteiras e DTOs de persistência;
- [ ] criar infraestrutura SQLite + EF Core;
- [ ] mapear agregados e constraints;
- [ ] criar importador do JSON;
- [ ] validar paridade e migração;
- [ ] manter exportação JSON;
- [ ] preparar a arquitetura para Blazor e API.

## Fases futuras

- vida, energia e recuperação com regras orientadas por dados;
- interface web com Blazor;
- API em ASP.NET Core;
- PostgreSQL para cenário multiusuário;
- sincronização entre dispositivos;
- cliente móvel.
