# Segurança e privacidade

## Dados armazenados

O save pode conter:

- nome e progresso do personagem;
- hábitos, projetos, missões e leituras;
- movimentações financeiras reais;
- justificativas de retiradas;
- histórico de conquistas.

## Regras atuais

- todos os dados permanecem locais;
- `save.json`, backups e arquivos anteriores são ignorados pelo Git;
- o usuário é responsável por proteger o diretório local;
- valores da Carteira não são moeda fictícia.

## Riscos

O JSON não é criptografado. Qualquer pessoa com acesso ao computador pode ler os dados.

## Evolução planejada

- opção de ocultar valores no dashboard;
- criptografia local opcional;
- proteção por senha;
- exportação e exclusão controladas;
- política de retenção de backups.
