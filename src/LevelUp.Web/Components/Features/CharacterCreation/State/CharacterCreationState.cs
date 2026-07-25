using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.CharacterCreation.Models;
using LevelUp.Web.Services;
using Microsoft.AspNetCore.Components.Web;

namespace LevelUp.Web.Components.Features.CharacterCreation.State;

public sealed class CharacterCreationState(LevelUpWebService store, ToastService toastService)
{
    public CharacterCreationFormModel Model { get; } = new();
    public CharacterCreationStep Step { get; private set; } = CharacterCreationStep.Account;
    public CharacterClassOption? SelectedClass { get; private set; }
    public bool ShowConfirmation { get; private set; }
    public bool IsBusy { get; private set; }
    public string? ValidationError { get; private set; }
    public bool HasAuthenticatedSession { get; private set; }

    public Task<LevelUp.Domain.Entities.LevelUpData> LoadDataAsync() => store.LoadAsync();

    public static IReadOnlyList<CharacterClassOption> ClassOptions { get; } =
    [
        new(CharacterClass.Warrior, "Warrior", "/images/classes/classicon_warrior.jpg", "Strong and resilient in battle."),
        new(CharacterClass.Hunter, "Hunter", "/images/classes/classicon_hunter.jpg", "Precise and resourceful at a distance."),
        new(CharacterClass.Rogue, "Rogue", "/images/classes/classicon_rogue.jpg", "Fast, subtle and strategically lethal."),
        new(CharacterClass.Priest, "Priest", "/images/classes/classicon_priest.jpg", "A devoted protector and healer."),
        new(CharacterClass.Druid, "Druid", "/images/classes/classicon_druid.jpg", "Adapts through the strength of nature.")
    ];

    public string NormalizedNickname => Model.Nickname.Trim().TrimStart('@');
    public string CurrentStepClass => Step.ToString().ToLowerInvariant();

    public bool IsPasswordValid =>
        Model.Password.Length >= 8 &&
        Model.Password.Any(char.IsLetter) &&
        Model.Password.Any(char.IsDigit);

    public bool ShouldShowConfirmPassword => IsPasswordValid;

    public bool CanContinueAccount =>
        !string.IsNullOrWhiteSpace(Model.Email) &&
        Model.Email.Contains('@', StringComparison.Ordinal) &&
        IsPasswordValid &&
        !string.IsNullOrEmpty(Model.ConfirmPassword) &&
        string.Equals(Model.Password, Model.ConfirmPassword, StringComparison.Ordinal);

    public bool CanContinueCharacter =>
        NormalizedNickname.Length >= 3 &&
        NormalizedNickname.All(character =>
            char.IsLetterOrDigit(character) || character is '.' or '_' or '-');

    public async Task<OnboardingStatus> InitializeAsync(bool hasAuthenticatedSession)
    {
        HasAuthenticatedSession = hasAuthenticatedSession;

        if (!hasAuthenticatedSession)
        {
            // Anonymous registration must not execute the authenticated Dashboard query.
            Step = CharacterCreationStep.Account;
            Model.Name = string.Empty;
            Model.Email = string.Empty;
            Model.Password = string.Empty;
            Model.ConfirmPassword = string.Empty;
            Model.Nickname = string.Empty;
            return new OnboardingStatus(false, false);
        }

        var data = await store.LoadAsync();
        if (data.CurrentUser is not null)
        {
            Model.Name = data.CurrentUser.Name;
            Model.Email = data.CurrentUser.Email;
            Step = CharacterCreationStep.Character;
        }

        return new OnboardingStatus(
            data.CurrentUser is not null,
            data.CurrentCharacter is not null);
    }

    public async Task<OnboardingStatus> GetStatusAsync()
    {
        var data = await store.LoadAsync();
        return new OnboardingStatus(
            data.CurrentUser is not null,
            data.CurrentCharacter is not null);
    }

    public bool ContinueToCharacter()
    {
        ValidationError = null;

        if (string.IsNullOrWhiteSpace(Model.Email) || !Model.Email.Contains('@', StringComparison.Ordinal))
        {
            ValidationError = "Enter a valid email address.";
            return false;
        }

        if (!IsPasswordValid)
        {
            ValidationError = "Password must contain at least 8 characters, one letter and one number.";
            return false;
        }

        if (!string.Equals(Model.Password, Model.ConfirmPassword, StringComparison.Ordinal))
        {
            ValidationError = "Passwords do not match.";
            return false;
        }

        Step = CharacterCreationStep.Character;
        return true;
    }

    public bool ContinueToClasses()
    {
        ValidationError = null;

        if (!CanContinueCharacter)
        {
            ValidationError = "Nickname must have 3 to 24 characters and may contain letters, numbers, dots, underscores or hyphens.";
            return false;
        }

        Model.Nickname = NormalizedNickname;
        Step = CharacterCreationStep.Class;
        return true;
    }

    public void Back()
    {
        ValidationError = null;
        ShowConfirmation = false;
        SelectedClass = null;
        Step = Step == CharacterCreationStep.Class
            ? CharacterCreationStep.Character
            : CharacterCreationStep.Account;
    }

    public void RequestClassConfirmation(CharacterClassOption option)
    {
        ValidationError = null;
        SelectedClass = option;
        ShowConfirmation = true;
    }

    public void CloseConfirmation()
    {
        ShowConfirmation = false;
        SelectedClass = null;
    }

    public async Task<bool> ConfirmClassAsync()
    {
        if (SelectedClass is null || IsBusy)
        {
            return false;
        }

        IsBusy = true;
        ValidationError = null;

        try
        {
            if (!HasAuthenticatedSession)
            {
                await store.CreateAccountAsync(
                    NormalizedNickname,
                    Model.Email.Trim(),
                    Model.Password,
                    NormalizedNickname,
                    SelectedClass.Value,
                    string.Empty);
            }
            else
            {
                await store.CreateCharacterAsync(
                    NormalizedNickname,
                    NormalizedNickname,
                    SelectedClass.Value,
                    string.Empty);
            }

            toastService.ShowSuccess("Welcome to LevelUp.");
            return true;
        }
        catch (Exception exception)
        {
            ValidationError = exception.Message;
            toastService.ShowError(exception.Message);
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task HandleConfirmationKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            CloseConfirmation();
        }

        return Task.CompletedTask;
    }


}

public enum CharacterCreationStep
{
    Account,
    Character,
    Class
}

public sealed record CharacterClassOption(
    CharacterClass Value,
    string DisplayName,
    string ImagePath,
    string Description);

public sealed record OnboardingStatus(bool HasUser, bool HasCharacter);
