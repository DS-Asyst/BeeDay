# Refatoração do LevelUp

## Estrutura aplicada

- Projetos de produção centralizados em `src/`.
- Projetos de teste centralizados em `tests/`.
- Apenas um projeto de apresentação: `src/LevelUp.Web`.
- Configuração compartilhada em `.editorconfig`, `Directory.Build.props` e `Directory.Packages.props`.
- Organização por feature nas camadas Application e Web.

## Domínio

- Entidades encapsuladas com métodos de domínio.
- Value objects para títulos, descrições, nome e apelido.
- Validação de enums e estado persistido.
- Exceções específicas de domínio.
- Regras de conclusão e status mantidas dentro das entidades e agregados.

## Application

- Casos de uso separados por feature.
- Interfaces focadas em vez de uma fachada única extensa.
- Requests e responses explícitos.
- Comportamento compartilhado concentrado em abstrações da camada.

## Infrastructure

- Persistência em `LevelUpBD.json`.
- Escrita atômica com arquivo temporário validado.
- Backups rotativos com retenção configurável.
- Recuperação automática pelo backup válido mais recente.
- Concorrência protegida no repositório.
- Logs estruturados sem exposição dos dados do usuário.
- Health check de leitura, escrita e validade.

## Web

- Componentes organizados por feature.
- Markup Razor separado de code-behind onde há comportamento relevante.
- Editores independentes para Habit, Task, Todo e Project.
- Tipagem explícita por `ActivityType`.
- Estado de dashboard, modais e perfil extraído das páginas.
- `LevelUpWebService` mantido como adaptador entre apresentação e Application.

## Qualidade

- Testes de Domain, Application e Infrastructure.
- Artefatos locais e gerados excluídos da distribuição.
- Nenhuma dependência da interface acessa diretamente a persistência JSON.
- Rotas e comportamento visual existentes preservados durante a reorganização.
