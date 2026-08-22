# Authentication

Cookies de autenticação, confirmação de e-mail, recuperação de senha, rate limiting de login e
invalidação de sessão.

**Disposição final (Sprint 31.10, EPIC 31):** esta pasta permanece intencionalmente sem documento
próprio — não é mais uma área reservada aguardando uma Sprint futura, é a decisão definitiva. O
conteúdo de autenticação cobre exatamente os mesmos mecanismos que `docs/security/` já documenta
(cookies, sessão, rate limiting, hashing de senha, tokens de e-mail/reset), e as tentativas de
"separar" os dois assuntos historicamente esbarraram em uma fronteira artificial — autenticação
é uma das superfícies de segurança da aplicação, não um domínio à parte com Aggregate, Command ou
camada própria. `docs/security/` é o owner canônico de todo esse conteúdo (ver
[`docs/security/README.md`](../security/README.md) "Ownership canônico"). Manter esta pasta com um
único arquivo de redirecionamento evita duplicar ou fragmentar a mesma explicação em dois lugares.

## Documentos

Nenhum documento vive aqui por design — ver `docs/security/`.

## Ordem de leitura recomendada

1. [`docs/security/01-security-baseline.md`](../security/01-security-baseline.md) — o que cada
   mecanismo de autenticação/sessão faz.
2. [`docs/security/02-operational-security.md`](../security/02-operational-security.md) — onde/como
   cada mecanismo é configurado e implantado.
