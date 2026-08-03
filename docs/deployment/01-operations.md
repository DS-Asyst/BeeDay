# HMG, Produção e Observabilidade

## 1. Ambientes

### Development

- provider JSON ou SQL local por configuração;
- e-mail capturado;
- secrets locais;
- logs legíveis.

### HMG

- provider SQL Server;
- banco vazio recriável;
- deploy automático a partir de `hmg`;
- integrações sandbox;
- dados sintéticos;
- smoke tests automáticos.

### Production

- deploy somente por merge aprovado em `prd`;
- ambiente protegido no GitHub;
- secrets separados;
- migration controlada;
- rollback documentado.

## 2. Branches

- `hmg`: integração e homologação;
- `prd`: produção;
- feature branches temporárias;
- push direto bloqueado em `prd`;
- status checks obrigatórios;
- ao menos uma aprovação;
- histórico linear ou política definida.

## 3. Observabilidade

### Logs

Campos mínimos:

- timestamp UTC;
- severity;
- correlationId;
- userId pseudonimizado quando necessário;
- operation;
- duration;
- result;
- errorCode.

### Métricas

- login success/failure/rate-limited;
- duração de handlers;
- requests por endpoint;
- erros por código;
- conexões e circuitos Blazor;
- filas pendentes;
- e-mails falhos;
- health status;
- tempo de query SQL;
- deadlocks e timeouts.

### Alertas

- readiness falhando;
- erro 5xx acima do limite;
- taxa elevada de login inválido;
- falha de backup;
- banco indisponível;
- espaço em disco baixo;
- migration falha;
- runner offline.

## 4. Hardening do runner

- usuário dedicado;
- mínimo privilégio;
- sem login interativo;
- runner atualizado;
- acesso somente aos diretórios necessários;
- workflows de forks não executam no runner;
- secrets limitados ao environment;
- auditoria de execução;
- rotação de credenciais.
