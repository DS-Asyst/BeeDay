using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Profile.Models;
using LevelUp.Web.Services;
using Microsoft.AspNetCore.Components.Web;

namespace LevelUp.Web.Components.Features.Profile.State;

public sealed class ProfileState(LevelUpWebService store, ToastService toastService)
{
    public ProfileFormModel Model { get; } = new();
    public CharacterCreationStep Step { get; private set; } = CharacterCreationStep.Name;
    public CharacterClassOption? SelectedClass { get; private set; }
    public bool ShowConfirmation { get; private set; }
    public bool IsBusy { get; private set; }

    public static IReadOnlyList<CharacterClassOption> ClassOptions { get; } =
    [
        new(CharacterClass.Warrior, "Warrior", "/images/classes/classicon_warrior.jpg", "Strong and resilient in battle."),
        new(CharacterClass.Hunter, "Hunter", "/images/classes/classicon_hunter.jpg", "Precise and resourceful at a distance."),
        new(CharacterClass.Rogue, "Rogue", "/images/classes/classicon_rogue.jpg", "Fast, subtle and strategically lethal."),
        new(CharacterClass.Priest, "Priest", "/images/classes/classicon_priest.jpg", "A devoted protector and healer."),
        new(CharacterClass.Druid, "Druid", "/images/classes/classicon_druid.jpg", "Adapts through the strength of nature.")
    ];

    public bool CanContinue => HasValidName && HasValidNickname;
    public string NormalizedNickname => Model.Nickname.Trim().TrimStart('@');
    public string CurrentStepClass => Step == CharacterCreationStep.Name ? "name" : "classes";

    public async Task<bool> ProfileAlreadyExistsAsync()
    {
        var current = await store.LoadAsync();
        return current.Profile is not null;
    }

    public void ContinueToClasses()
    {
        if (!CanContinue)
        {
            return;
        }

        Model.Name = Model.Name.Trim();
        Model.Nickname = NormalizedNickname;
        Step = CharacterCreationStep.Class;
    }

    public void BackToName()
    {
        SelectedClass = null;
        ShowConfirmation = false;
        Step = CharacterCreationStep.Name;
    }

    public void RequestClassConfirmation(CharacterClassOption option)
    {
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
        if (SelectedClass is null)
        {
            return false;
        }

        if (IsBusy)
        {
            return false;
        }

        IsBusy = true;

        try
        {
            await store.CreateProfileAsync(Model.Name, NormalizedNickname, SelectedClass.Value);
            toastService.ShowSuccess("Character created successfully.");
            return true;
        }
        catch
        {
            toastService.ShowError("Your character could not be created.");
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

    private bool HasValidName => !string.IsNullOrWhiteSpace(Model.Name);

    private bool HasValidNickname =>
        !string.IsNullOrWhiteSpace(NormalizedNickname)
        && NormalizedNickname.Length >= 3
        && NormalizedNickname.All(character => char.IsLetterOrDigit(character) || character is '.' or '_' or '-');
}

public enum CharacterCreationStep
{
    Name,
    Class
}

public sealed record CharacterClassOption(
    CharacterClass Value,
    string DisplayName,
    string ImagePath,
    string Description);
