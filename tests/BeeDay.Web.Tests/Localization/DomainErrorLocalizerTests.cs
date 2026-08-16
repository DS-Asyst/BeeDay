using BeeDay.Application.Exceptions;
using BeeDay.Domain.Exceptions;
using BeeDay.Web.Localization;
using BeeDay.Web.Resources;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Localization;

public sealed class DomainErrorLocalizerTests
{
    [Theory]
    [InlineData("en-US", "Email 'a@b.com' is already registered.", "This email is already registered.")]
    [InlineData("pt-BR", "Email 'a@b.com' is already registered.", "Este e-mail já está cadastrado.")]
    [InlineData("en-US", "Nickname '@joe' is already in use.", "This nickname is already in use.")]
    [InlineData("pt-BR", "Nickname '@joe' is already in use.", "Este apelido já está em uso.")]
    [InlineData("en-US", "The current password is incorrect.", "The current password is incorrect.")]
    [InlineData("pt-BR", "The current password is incorrect.", "A senha atual está incorreta.")]
    [InlineData("en-US", "The new password must be different from the current password.", "The new password must be different from the current password.")]
    [InlineData("pt-BR", "The new password must be different from the current password.", "A nova senha deve ser diferente da senha atual.")]
    [InlineData("en-US", "The password reset token is invalid or expired.", "This password reset link is invalid or has expired.")]
    [InlineData("pt-BR", "The password reset token is invalid or expired.", "Este link de redefinição de senha é inválido ou expirou.")]
    [InlineData("en-US", "A User can only complete their profile once.", "Your profile has already been completed.")]
    [InlineData("pt-BR", "A User can only complete their profile once.", "Seu perfil já foi concluído.")]
    public void InvalidDomainStateException_TranslatesKnownMessages(string culture, string rawMessage, string expected)
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture(culture, () =>
            DomainErrorLocalizer.Translate(new InvalidDomainStateException(rawMessage), localizer));

        Assert.Equal(expected, translated);
    }

    [Theory]
    [InlineData("en-US", "Please wait 42 seconds before requesting another email.", "Please wait 42 seconds before requesting another email.")]
    [InlineData("pt-BR", "Please wait 42 seconds before requesting another email.", "Aguarde 42 segundos antes de solicitar outro e-mail.")]
    public void InvalidDomainStateException_PreservesTheDynamicWaitSeconds(string culture, string rawMessage, string expected)
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture(culture, () =>
            DomainErrorLocalizer.Translate(new InvalidDomainStateException(rawMessage), localizer));

        Assert.Equal(expected, translated);
    }

    [Fact]
    public void InvalidDomainStateException_UnknownMessage_FallsBackToTheGenericLocalizedMessage()
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
            DomainErrorLocalizer.Translate(new InvalidDomainStateException("Some future message nobody mapped yet."), localizer));

        Assert.Equal("Algo deu errado. Tente novamente em instantes.", translated);
    }

    [Theory]
    [InlineData("email", "en-US", "Please enter a valid email address.")]
    [InlineData("email", "pt-BR", "Informe um endereço de e-mail válido.")]
    [InlineData("nickname", "en-US", "Please choose a different nickname.")]
    [InlineData("nickname", "pt-BR", "Escolha um apelido diferente.")]
    [InlineData("name", "en-US", "Please enter a valid name.")]
    [InlineData("name", "pt-BR", "Informe um nome válido.")]
    [InlineData("color", "pt-BR", "Verifique as informações destacadas e tente novamente.")]
    public void DomainValidationException_TranslatesByField(string field, string culture, string expected)
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture(culture, () =>
            DomainErrorLocalizer.Translate(new DomainValidationException(field, "irrelevant raw text"), localizer));

        Assert.Equal(expected, translated);
    }

    [Fact]
    public void ApplicationValidationException_TranslatesToTheGenericValidationMessage()
    {
        var localizer = CreateLocalizer();
        var exception = new ApplicationValidationException([new ValidationFailure("Title", "irrelevant")]);

        var translated = BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
            DomainErrorLocalizer.Translate(exception, localizer));

        Assert.Equal("Verifique as informações destacadas e tente novamente.", translated);
    }

    [Fact]
    public void UnrelatedException_TranslatesToTheGenericMessage()
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture("en-US", () =>
            DomainErrorLocalizer.Translate(new InvalidOperationException("some infrastructure failure"), localizer));

        Assert.Equal("Something went wrong. Try again in a moment.", translated);
    }

    private static IStringLocalizer<SharedResources> CreateLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        return services.BuildServiceProvider().GetRequiredService<IStringLocalizer<SharedResources>>();
    }
}
