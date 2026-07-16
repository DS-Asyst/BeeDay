# Planejamento da Fase 9 — Persistência relacional

## Objetivo

Introduzir persistência relacional somente quando o JSON deixar de atender às necessidades reais do LevelUp. A decisão deve ser orientada por requisitos, não apenas por preferência tecnológica.

## Banco recomendado

Para a aplicação local e de usuário único, a primeira opção é:

- SQLite;
- Entity Framework Core;
- arquivo local sem servidor;
- migrações controladas;
- exportação JSON mantida para backup e portabilidade.

PostgreSQL deve ser considerado apenas quando surgirem API, múltiplos usuários, sincronização entre dispositivos ou hospedagem centralizada.

## Critérios de prontidão

A Fase 9 deve começar quando pelo menos dois destes sinais forem verdadeiros:

- o histórico cresce a ponto de prejudicar o carregamento do JSON;
- relatórios exigem consultas frequentes por período, categoria ou relacionamento;
- a Carteira possui contas, categorias, transferências e muitos lançamentos;
- o aplicativo precisa de paginação ou filtros avançados;
- existe uma interface Blazor ou API em planejamento imediato;
- existe necessidade de concorrência ou sincronização;
- a manutenção das migrações JSON passou a ter custo elevado.

## Etapa 9.0 — Preparação

1. congelar e versionar o schema JSON atual;
2. completar testes de round-trip e migração;
3. remover APIs legadas de persistência;
4. separar DTOs de persistência das entidades, quando necessário;
5. definir chaves, constraints e política de exclusão;
6. documentar dados financeiros e privacidade.

## Etapa 9.1 — Infraestrutura

1. criar `LevelUp.Infrastructure`;
2. adicionar EF Core e provider SQLite;
3. criar `LevelUpDbContext`;
4. mapear entidades por configuração fluente;
5. representar valores monetários em centavos (`long`) ou aplicar conversão explícita;
6. criar a primeira migration do EF Core.

## Etapa 9.2 — Fronteiras de aplicação

1. definir repositórios apenas para agregados que realmente precisem deles;
2. impedir acesso ao `DbContext` pelas telas;
3. manter workflows e serviços independentes do provider;
4. criar uma unidade de trabalho ou política transacional simples;
5. substituir gradualmente o snapshot global por consultas e comandos.

## Etapa 9.3 — Importação e paridade

1. criar importador de `save.json`;
2. validar integridade antes de importar;
3. preservar IDs e relacionamentos;
4. registrar arquivo importado e checksum;
5. testar paridade JSON versus SQLite;
6. impedir importação duplicada.

## Etapa 9.4 — Transição

1. executar SQLite como armazenamento principal em uma branch experimental;
2. manter exportação JSON;
3. validar desempenho e integridade com dados reais;
4. publicar uma versão candidata;
5. somente depois remover a escrita operacional em JSON.

## Riscos

- acoplar domínio e UI ao EF Core;
- mapear entidades complexas sem testes suficientes;
- perder precisão de valores monetários;
- quebrar saves históricos;
- introduzir banco antes de estabilizar regras de domínio;
- aumentar a complexidade sem benefício perceptível ao usuário.

## Resultado esperado

Uma persistência local robusta, consultável e transacional, mantendo o domínio reutilizável para Console, Blazor e futuras APIs.
