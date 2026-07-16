# Roadmap de banco de dados

## Decisão

Após a Fase 8, iniciar a preparação para banco de dados é recomendado. A migração completa deve ocorrer em uma fase própria, depois que o modelo de metas e progressão estiver estabilizado.

## Por que o momento é adequado

O LevelUp já possui múltiplos agregados, relacionamentos, histórico de leitura, ledger financeiro, conquistas, metas e necessidade crescente de consultas e relatórios.

O JSON continua útil para desenvolvimento local, mas passa a limitar consultas por período, integridade referencial, volume de histórico, migrações complexas, filtros e futura sincronização.

## Recomendação tecnológica

O primeiro banco recomendado é **SQLite com Entity Framework Core**:

- aplicação local e de usuário único;
- implantação simples;
- arquivo único;
- transações;
- migrações do EF Core;
- caminho natural para ASP.NET Core e Blazor.

Quando houver sincronização ou múltiplos dispositivos, considerar PostgreSQL no servidor e SQLite local para cache ou modo offline.

## Ordem proposta

1. interfaces de repositório por agregado;
2. DTOs de persistência separados das entidades;
3. projeto `LevelUp.Infrastructure`;
4. SQLite e EF Core;
5. importador do `save.json`;
6. testes de paridade;
7. exportação JSON como backup;
8. SQLite como armazenamento padrão.

## Evitar

- `DbContext` diretamente nas telas;
- migração sem testes de paridade;
- remoção do importador JSON;
- banco remoto antes de autenticação e sincronização.
