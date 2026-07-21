# Etapa 4 — Persistência JSON

## Entregue

- Gravações atômicas por arquivo temporário validado.
- Backups com timestamp e retenção configurável.
- Recuperação automática pelo backup válido mais recente.
- Validação do estado de domínio após desserialização.
- Controle assíncrono de concorrência.
- Logs estruturados.
- Configuração em `appsettings.json`.
- Health check disponível em `/health`.
- Exceções específicas de persistência.
- Testes de leitura, gravação, retenção, recuperação e health check.

O arquivo principal permanece `LevelUpBD.json`; não é necessária migração de dados.
