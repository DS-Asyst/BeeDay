# Etapa 1 — Reorganização da solução

## Alterações aplicadas

1. Projetos de produção movidos para `src/`.
2. Projetos agrupados logicamente no `LevelUp.slnx` em Core, Infrastructure, Presentation e Tests.
3. Referências entre projetos atualizadas para a nova estrutura.
4. Configurações comuns de build centralizadas em `Directory.Build.props`.
5. Versões dos pacotes de teste centralizadas em `Directory.Packages.props`.
6. `.editorconfig` ampliado com regras para C#, Razor, XML, JSON e Markdown.
7. Métodos de DI formatados, validados contra argumentos nulos e mantidos por camada.
8. Projetos de testes padronizados e isolados por camada.
9. README atualizado com a arquitetura e novos comandos.

## Escopo preservado

Esta etapa não altera entidades, regras de domínio, contratos de aplicação, persistência JSON ou componentes visuais. Essas mudanças pertencem às etapas seguintes.

## Stage 1 final cleanup

- Enabled compiler documentation generation so IDE0005 can run during builds.
- Suppressed CS1591 because public XML documentation is not mandatory at this stage.
- Applied explicit braces to control-flow statements.
- Applied explicit public accessibility to interface members according to `.editorconfig`.
- Corrected nullable access on the Blazor error page.
- Aligned HTTPS redirection with non-development environments and made HTTPS the primary launch profile.
