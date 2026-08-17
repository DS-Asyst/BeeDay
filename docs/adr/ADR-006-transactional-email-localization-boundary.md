# ADR-006 — Transactional Email Culture Transport & Localization Boundary

**Status:** Aceito e implementado (EPIC 28, Sprint 28.2)
**Data:** 2026-08-17

## Contexto

A EPIC 26 estabeleceu `IIdentityEmailComposer`/`IdentityEmailComposer` (Infrastructure) produzindo
e-mails de confirmação de conta e redefinição de senha inteiramente em inglês, hardcoded. O produto já
possui um sistema completo de localization en-US/pt-BR, mas exclusivamente Web-owned: 19 catálogos
`.resx` sob `src/BeeDay.Web/`, `IStringLocalizer<T>` e `RequestLocalizationOptions` (
`docs/web/07-localization.md`). A Sprint 26.6 já havia avaliado e explicitamente adiado essa decisão
(`docs/infrastructure/06-transactional-email.md` §13.5), documentando duas direções candidatas sem
escolher nenhuma: (a) um catálogo `.resx` estreito, Infrastructure-owned, exclusivo para strings de
e-mail; ou (b) mover a composição para Web (usando `IStringLocalizer` lá) e Infrastructure passar a
apenas enviar um `EmailMessage` já renderizado.

A EPIC 28 (pacote de planejamento fornecido pelo responsável do repositório) comissionou explicitamente
esta decisão para a Sprint 28.2, com as seguintes restrições permanentes já estabelecidas antes desta
Sprint: Application/Infrastructure não podem depender de Web; não inferir idioma por Environment,
`Accept-Language` pós-request, ou `CurrentUICulture` global; não criar um segundo Localization System
por conveniência; reaproveitar dado de culture já persistido em `User` se existir.

A Sprint 28.1 (auditoria) confirmou o dado necessário já existe no lugar certo: `User.Language`
(`src/BeeDay.Domain/Entities/User.cs`, enum `UserLanguage` — `English`/`Portuguese`) é uma propriedade
persistida do Domain, alcançável por Application via `IUserRepository` independentemente de qualquer
requisição HTTP. Todos os 3 pontos de composição de e-mail (`EmailConfirmationIssuer.Issue`,
`ResendEmailConfirmationCommandHandler.Handle`, `RequestPasswordResetCommandHandler.Handle`) já
carregam o `User` completo no momento em que chamam o composer.

## Decisão

Adotada a opção (a) já esboçada na Sprint 26.6, com o mecanismo de leitura mais conservador possível:

1. **Contrato:** `IIdentityEmailComposer.ComposeEmailConfirmation`/`ComposePasswordReset` ganham um
   parâmetro obrigatório `UserLanguage language` (Domain — camada que Application e Infrastructure já
   dependem, sem introduzir nova dependência). Os 3 call sites em Application passam `user.Language`
   diretamente — nenhum default, nenhuma inferência.
2. **Catálogo:** um catálogo `.resx` novo e estreito,
   `src/BeeDay.Infrastructure/Identity/EmailResources.resx`/`.en-US.resx`/`.pt-BR.resx` (9 chaves:
   saudação + título/introdução/rodapé/CTA de cada um dos 2 fluxos), owned por Infrastructure — não
   uma cópia dos 19 catálogos Web, um catálogo próprio e menor, com propósito único.
3. **Leitura:** `System.Resources.ResourceManager.GetString(name, explicitCultureInfo)`, chamado com
   uma `CultureInfo` explícita por chamada — nunca `Microsoft.Extensions.Localization`/
   `IStringLocalizer` (evita qualquer aparência de "reuso" do mecanismo Web-owned) e nunca mutação de
   `Thread.CurrentThread.CurrentUICulture`/`CultureInfo.CurrentUICulture` (que seria estado global
   compartilhado entre requisições concorrentes de usuários com idiomas diferentes).
4. **Mapeamento `UserLanguage` → `CultureInfo`:** uma função privada de duas linhas dentro de
   `IdentityEmailComposer`, deliberadamente não reaproveitando
   `BeeDay.Web.Localization.BeeDayCultures.FromUserLanguage` — Infrastructure não pode referenciar Web.
   Essa duplicação mínima e inevitável na fronteira é aceita explicitamente por esta ADR, não um
   descuido.
5. **Preheader** permanece um detalhe interno do compositor, não promovido ao contrato `EmailMessage`
   nesta Sprint — decisão de escopo, não uma limitação técnica; pode ser adicionado depois sem quebrar
   este contrato de novo.
6. **Conteúdo pt-BR** desta Sprint é uma tradução funcional de primeira passada das strings inglesas já
   existentes — não uma revisão de voice/tone. Ambos os idiomas serão revisados juntos na Sprint 28.4.

## Alternativas rejeitadas

- **Opção (b) da Sprint 26.6** (mover composição para Web, `IStringLocalizer` lá): rejeitada por ser
  uma mudança arquitetural maior (moveria o owner de `IIdentityEmailComposer` através de uma fronteira
  de camada, tocando todos os call sites) sem benefício adicional sobre a opção (a) para o problema
  real (9 strings, não 19 catálogos inteiros).
- **Reaproveitar os catálogos Web diretamente de Infrastructure**: rejeitada — inverteria a direção de
  dependência (`Infrastructure` → `Web`), proibido por `CLAUDE.md` §3 e pela própria EPIC 28.
- **`Microsoft.Extensions.Localization`/`IStringLocalizer<T>` dentro de Infrastructure**: tecnicamente
  disponível (o projeto já referencia `Microsoft.AspNetCore.App` via `FrameworkReference`), mas
  rejeitada por ser exatamente o mecanismo que `02_INITIAL_USER_PROMPT.txt` da EPIC 28 nomeou
  explicitamente como não-permitido ("Infrastructure não deve simplesmente começar a usar
  `IStringLocalizer` da Web") — `ResourceManager` puro remove qualquer ambiguidade sobre isso.
- **Culture via `Thread.CurrentThread.CurrentUICulture`/`CultureInfo.CurrentUICulture`**: rejeitada
  explicitamente pelas regras compartilhadas da EPIC 28 ("não usar CurrentUICulture global como estado
  durável") — um `IEmailSender`/composer pode, em teoria, ser chamado fora do ciclo de vida de uma
  única requisição (ex.: um worker futuro), e mesmo dentro de uma requisição, mutar estado global por
  chamada é uma fonte clássica de corrida em cenários concorrentes.

## Direção de dependência

```text
BeeDay.Domain          — UserLanguage (já existia), User.Language (já existia).
BeeDay.Application     — IIdentityEmailComposer agora expõe UserLanguage nos dois métodos (Domain,
                          já uma dependência existente — nenhuma nova).
BeeDay.Infrastructure   — IdentityEmailComposer lê User.Language recebido, resolve para CultureInfo
                          via mapeamento privado, lê EmailResources.resx via ResourceManager. Nenhuma
                          referência nova a Web, IStringLocalizer, ou Microsoft.Extensions.Localization.
BeeDay.Web              — inalterado; continua sem conhecer nenhum tipo concreto de Infrastructure.
```

Nenhuma direção de dependência mudou. `PersistenceContractBoundaryTests.ApplicationAssembly_DoesNotReferenceInfrastructure`
continua válido e não foi alterado por esta Sprint.

## Compatibilidade

`IIdentityEmailComposer.ComposeEmailConfirmation`/`ComposePasswordReset` são um contrato interno deste
repositório, sem consumidor externo (nenhum pacote NuGet publicado o expõe). A extensão de assinatura
(4º parâmetro obrigatório) foi aplicada, com todos os 3 call sites reais atualizados na mesma Sprint —
nenhum overload de compatibilidade foi necessário porque não há consumidor fora deste código-fonte.

## Consequências positivas

- Confirmação de conta e redefinição de senha agora podem ser compostas em `en-US` ou `pt-BR`,
  respeitando a preferência já salva do usuário, sem qualquer nova dependência de camada.
- O padrão (`ResourceManager` + `CultureInfo` explícita por chamada) é seguro sob concorrência — duas
  composições simultâneas para usuários com idiomas diferentes não podem interferir uma na outra.
- A fronteira "Infrastructure nunca depende de Web" permanece intacta e agora tem um exemplo concreto
  documentado de como uma feature aparentemente Web-shaped (localization) pode ser servida sem violar
  essa fronteira.

## Consequências negativas

- Uma segunda, pequena fonte de strings de e-mail existe agora além dos 19 catálogos Web — aceito
  deliberadamente (não é o mesmo problema que "duplicar um catálogo inteiro por conveniência"; é o
  próprio owner que a Sprint 26.6 já havia esboçado).
- O mapeamento `UserLanguage` → `CultureInfo` existe duplicado (uma vez em
  `BeeDay.Web.Localization.BeeDayCultures`, outra vez, menor, em `IdentityEmailComposer`) — inevitável
  sem inverter a direção de dependência; qualquer terceiro idioma futuro exige atualizar os dois
  pontos.
- O conteúdo pt-BR desta Sprint não passou por revisão de voice/tone completa — carregado como
  tradução funcional da Sprint 26.6, a ser revisado na Sprint 28.4.

## Restrições

Esta ADR proíbe explicitamente, para qualquer trabalho futuro nesta fronteira:

- Adicionar `Microsoft.Extensions.Localization`/`IStringLocalizer<T>` a `BeeDay.Infrastructure` para
  fins de e-mail transacional sem uma nova ADR que reavalie esta decisão.
- Inferir idioma por `ASPNETCORE_ENVIRONMENT`/`Accept-Language` pós-request/`CurrentUICulture` global
  em qualquer composer de e-mail.
- Duplicar o catálogo Web inteiro dentro de Infrastructure "só para simplificar" — o catálogo de e-mail
  deve permanecer estreito, com apenas as chaves que o compositor de e-mail realmente usa.
- Reverter para strings hardcoded para adicionar um fluxo de e-mail novo — todo fluxo novo usa o mesmo
  catálogo `EmailResources`/mecanismo de culture explícita.

## Referências

- `docs/infrastructure/06-transactional-email.md` §13.5 — decisão originalmente adiada (Sprint 26.6),
  atualizada nesta Sprint com ponteiro para esta ADR.
- `docs/epics/28-transactional-email-experience/README.md` — Sprint 28.1 (achados que fundamentam esta
  decisão) e Sprint 28.2 (implementação).
- `docs/web/07-localization.md` §8-9 — sistema de localization Web-owned que esta ADR deliberadamente
  não estende nem duplica.
- `docs/brand/02-writing-voice-localization.md` — seção "E-mail transacional", que já nomeava esta
  lacuna.
