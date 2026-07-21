# Etapa 6 — Logging e Observabilidade: preparação

Este documento define a preparação arquitetural. Nenhuma dependência de observabilidade foi adicionada na Sprint F.3.

## Objetivos da próxima etapa

1. Logging estruturado e consistente entre Web, Application e Infrastructure.
2. Correlação de uma operação desde o evento de UI até a persistência JSON.
3. Métricas de latência, taxa de erro e volume de operações.
4. Health checks úteis para aplicação, armazenamento e backup.
5. Diagnóstico sem exposição de títulos, descrições, apelidos ou conteúdo sensível.

## Pontos de instrumentação

- `Program.cs`: configuração dos providers, filtros e enriquecimento por ambiente.
- `DashboardState.ExecuteAsync`: início/fim/falha das operações acionadas pela interface.
- `LevelUpWebService`: duração e resultado dos casos de uso.
- Persistência em `LevelUp.Infrastructure`: leitura, escrita atômica, backup, recuperação e falhas de I/O.
- Health checks: disponibilidade, permissão de escrita e validade estrutural do arquivo JSON.

## Convenções propostas

- Categorias por namespace e componente.
- Event IDs estáveis por operação.
- `Information`: operações de negócio concluídas e inicialização.
- `Warning`: recuperação por backup, tentativa inválida e degradação recuperável.
- `Error`: falhas não recuperadas de persistência ou caso de uso.
- `Debug`: detalhes técnicos habilitados apenas em desenvolvimento.
- Nunca registrar conteúdo de descrição, nickname completo ou payload JSON.

## Ordem recomendada de implementação

1. Definir catálogo de eventos e política de dados.
2. Instrumentar persistência e casos de uso com `ILogger<T>`.
3. Adicionar correlation scope por operação/circuito.
4. Expandir health checks.
5. Adicionar métricas com OpenTelemetry.
6. Selecionar exporter/sink por ambiente após validar a telemetria local.
