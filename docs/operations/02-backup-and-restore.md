# Backup, Restore e Continuidade

## 1. Banco SQL

Definir:

- backup full diário;
- diferencial ou log conforme edição e necessidade;
- retenção;
- cópia fora do servidor;
- criptografia;
- monitoramento da execução;
- teste periódico de restore.

## 2. Objetivos

Definir formalmente:

- RPO: perda máxima aceitável de dados;
- RTO: tempo máximo de recuperação.

Sem esses valores, não existe política de continuidade verificável.

## 3. Teste de restore

Processo trimestral mínimo:

1. selecionar backup;
2. restaurar em ambiente isolado;
3. aplicar verificação de integridade;
4. executar migrations pendentes em cópia;
5. iniciar aplicação;
6. executar smoke tests;
7. registrar duração e resultado;
8. corrigir desvios.

## 4. JSON legado

Após o corte:

- retirar JSON do caminho ativo;
- manter cópia final criptografada conforme retenção aprovada;
- restringir ACL;
- documentar data de descarte;
- não disponibilizar fallback na aplicação.

## 5. Rollback de deploy

Rollback de binário não pode assumir rollback automático de schema. Migrations devem ser backward-compatible durante a janela de implantação sempre que possível.
