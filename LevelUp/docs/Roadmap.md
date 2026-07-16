# Roadmap

## Fases concluídas

- Fase 1 — Personagem e hábitos
- Fase 2 — Treinamentos e UI com Spectre.Console
- Fase 3 — Projetos e missões
- Fase 4 — Capítulos, Chefes e consolidação de UX
- Fase 5 — Diário, Biblioteca, Mochila, Carteira e reconhecimento

## Fase 6 — Inteligência, confiabilidade e visão integrada

### Entrega atual

- [x] hygiene e baseline
- [x] `GameSession` e composição fora do `Program.cs`
- [x] schema versionado e migrações
- [x] validação de integridade
- [x] escrita atômica e snapshot anterior
- [x] dashboard inicial
- [x] estorno auditável na Carteira
- [x] cobertura configurada
- [x] documentação de persistência, testes e privacidade

### Próximos incrementos

- [ ] extrair flows das screens grandes
- [ ] contas, reservas e categorias da Carteira
- [ ] relatórios por período
- [ ] estatísticas de leitura e diário
- [ ] conquistas sistêmicas
- [ ] `TimeProvider` e `DateTimeOffset`
- [ ] códigos de erro de domínio localizáveis

## Fase 7 — Vida, energia e recuperação

A modelagem será definida somente após a camada de estatísticas oferecer dados suficientes para regras equilibradas.

## Fases futuras

- API em ASP.NET Core
- interface em Blazor
- banco de dados
- sincronização entre dispositivos
- cliente móvel

## Fase 7 — Progressão do personagem

- [x] Visualização da trilha de títulos
- [x] Faixas de nível consolidadas
- [x] Testes da progressão
- [ ] Histórico de evolução por data
- [ ] Especializações opcionais

## Fase 8 — Mundo, metas e desafios

- [x] Domínio de metas
- [x] Indicadores derivados do estado real
- [x] Persistência e migração
- [x] Tela Mundo
- [x] Testes de metas
- [ ] Desafios recorrentes
- [ ] Temporadas e eventos narrativos

## Fase 9 — Persistência relacional e plataforma

- [ ] SQLite e Entity Framework Core
- [ ] Importação do save JSON
- [ ] Repositórios por agregado
- [ ] Preparação para Blazor
