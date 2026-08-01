# Erros e Versionamento

## 1. Envelope de erro

Toda fronteira HTTP futura deve usar Problem Details com extensões padronizadas:

```json
{
  "type": "https://levelup/errors/activity-not-found",
  "title": "Activity not found",
  "status": 404,
  "detail": "The requested activity does not exist or is unavailable.",
  "instance": "/api/v1/habits/...",
  "code": "activity.not_found",
  "correlationId": "...",
  "errors": {}
}
```

## 2. Códigos estáveis

Categorias iniciais:

```text
auth.invalid_credentials
auth.account_inactive
auth.email_not_confirmed
auth.rate_limited
auth.session_invalidated
user.not_found
user.email_conflict
user.nickname_conflict
activity.not_found
activity.version_conflict
activity.invalid_order
project.not_found
wallet.not_found
wallet.transaction_not_found
wallet.tag_not_found
wallet.tag_in_use
validation.failed
persistence.unavailable
operation.conflict
```

Mensagens podem mudar. Códigos não devem mudar sem versionamento.

## 3. Mapeamento HTTP

| Situação | Status |
|---|---:|
| validação | 400 |
| não autenticado | 401 |
| autenticado sem autorização | 403 |
| recurso inexistente ou de outro usuário | 404 |
| conflito de versão ou unicidade | 409 |
| rate limit | 429 |
| dependência indisponível | 503 |

Para evitar enumeração, recursos de outro usuário devem parecer inexistentes.

## 4. Versionamento

Versão inicial:

```text
/api/v1
```

Mudanças compatíveis:

- adicionar campo opcional;
- adicionar endpoint;
- adicionar novo código de erro;
- adicionar enum somente quando consumidores tolerarem valores desconhecidos.

Mudanças incompatíveis:

- remover ou renomear campo;
- alterar tipo;
- transformar opcional em obrigatório;
- alterar significado;
- mudar status code esperado;
- reutilizar código de erro para outra situação.

## 5. Depreciação

Toda depreciação deve informar:

- contrato substituto;
- data de início;
- versão de remoção;
- prazo mínimo de suporte;
- impacto nos consumidores.
