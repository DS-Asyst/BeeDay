# Brand System

Documentação viva da identidade e da expressão da marca beeday. Esta área é proprietária de
wordmark, personagens, ilustração e linguagem verbal; componentes, states e foundations de
interface continuam pertencendo ao [`design-system/`](../design-system/README.md).

**Fonte da verdade:** ativos e consumers atuais em `src/BeeDay.Web`, contratos documentados pela
EPIC 25 e testes que protegem o markup correspondente. Última verificação: 2026-08-16, Sprint
25.16.

## Documentos

| Documento | Conteúdo |
|---|---|
| [`01-character-illustration.md`](01-character-illustration.md) | Inventário, personagens confirmados, shape language, composição, acessibilidade e performance |
| [`02-writing-voice-localization.md`](02-writing-voice-localization.md) | Narrativa, Voice, Tone, Style, glossário e política bilíngue |

## Limites

- O Brand System pode reutilizar cores e tipografia das foundations, mas valores artísticos de uma
  composição não viram tokens semânticos de UI automaticamente.
- Uma imagem aprovada em uma composição não é, por isso, um modelo anatômico reutilizável nem
  autoriza derivações.
- Rotas públicas `/brand/*` exigem guideline estabilizada e autorização própria. Esta área de
  documentação não implica publicação de uma página.
- Nomes, personalidade, história, função narrativa e anatomia não observável permanecem
  desconhecidos até existir fonte aprovada no repositório.

## Relação com a EPIC 25

A governança e o contrato de marca continuam registrados em
[`docs/epics/25-design-system-brand-evolution/README.md`](../epics/25-design-system-brand-evolution/README.md).
Este diretório é o owner das regras vivas; o documento da EPIC registra decisões e resultados de
execução.
