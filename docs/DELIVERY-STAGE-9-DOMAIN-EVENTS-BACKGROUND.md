# Etapa 9 — Domain Events, Background Processing e Cache

## Entregas

- Abstrações de eventos de domínio em `LevelUp.Domain`.
- `DomainEventBehavior` no pipeline MediatR para publicar eventos após Commands concluídos.
- Notification handlers desacoplados para auditoria e invalidação de cache.
- Fila assíncrona baseada em `Channel<T>` com capacidade limitada.
- `BackgroundService` para executar tarefas fora da requisição.
- Journal append-only em `Data/LevelUpEvents.ndjson`.
- Cache em memória de 30 segundos para `GetLevelUpQuery`.
- Invalidação automática do cache após qualquer Command bem-sucedido.

## Fluxo

Command -> Handler -> persistência -> DomainEventBehavior -> MediatR Notification

A notificação dispara:

1. invalidação do cache do dashboard;
2. enfileiramento de auditoria;
3. gravação assíncrona do evento no journal NDJSON.

## Decisões

- O journal é separado do `LevelUpBD.json`, evitando aumentar o documento principal.
- A fila é limitada a 256 itens para aplicar backpressure.
- Falhas no journal são registradas em log e não desfazem a operação de negócio já concluída.
- O cache é local ao processo e está preparado para futura troca por Redis.
