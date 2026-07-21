# Etapa 6 — Logging e Observabilidade

## Escopo aplicado

Esta etapa estabelece uma base de diagnóstico sem acoplar o projeto ao IIS, Docker, Serilog, OpenTelemetry ou serviços externos.

### Logging estruturado

- Console em JSON, adequado para desenvolvimento, IIS e containers futuros.
- Escopo por requisição com `CorrelationId` e `RequestId`.
- `EventId` estáveis para persistência, backups e falhas HTTP.
- Propriedades estruturadas em vez de interpolação de texto.
- Tempo de gravação da persistência registrado como `DurationMs`.
- Logs rotineiros de criação e remoção de backup rebaixados para `Debug`, reduzindo ruído em produção.

### Tratamento global de erros

- `IExceptionHandler` centralizado.
- Respostas no padrão `ProblemDetails`.
- Mapeamentos atuais:
  - validação de domínio: HTTP 400;
  - atividade inexistente: HTTP 404;
  - estado de domínio inválido: HTTP 409;
  - indisponibilidade da persistência: HTTP 503;
  - erro inesperado: HTTP 500.
- Detalhes técnicos são expostos apenas em `Development`.
- Toda resposta de erro inclui o identificador de correlação.

### Health checks

- `/health/live`: confirma que o processo está ativo.
- `/health/ready`: confirma que a persistência JSON está pronta.
- `/health`: apresenta o relatório completo em JSON.

O health check de armazenamento valida diretórios, permissão de escrita e integridade estrutural do JSON.

### Configuração por ambiente

- `appsettings.Development.json`: maior detalhamento para diagnóstico local.
- `appsettings.Production.json`: reduz ruído de framework e mantém eventos relevantes.

## Fora do escopo

Serilog, arquivos de log, Windows Event Log, OpenTelemetry, dashboards, métricas de infraestrutura, Prometheus, Grafana e configuração específica de IIS permanecem para a Etapa 10, quando o modelo de hospedagem estiver definido.

## Validação recomendada

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Com a aplicação iniciada, validar:

```text
https://localhost:7245/health/live
https://localhost:7245/health/ready
https://localhost:7245/health
```
