using BeeDay.Web.Components.Features.ProfileCreation.Models;
using BeeDay.Web.Services;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.ProfileCreation.State;

public sealed class ProfileCreationState(
    BeeDayWebService store,
    ToastService toastService,
    IStringLocalizer<ProfileCreationResources> localizer)
{
    public ProfileCreationFormModel Model { get; } = new();
    public ProfileCreationStep Step { get; private set; } = ProfileCreationStep.Account;
    public bool IsBusy { get; private set; }
    public string? ValidationError { get; private set; }
    public bool HasAuthenticatedSession { get; private set; }

    public Task<BeeDay.Application.Features.Users.Responses.CurrentUserResponse?> LoadDataAsync() => store.GetCurrentUserAsync();

    public string NormalizedName => Model.Name.Trim();
    public string NormalizedNickname => Model.Nickname.Trim().TrimStart('@');
    public string CurrentStepClass => Step.ToString().ToLowerInvariant();

    public bool IsPasswordValid =>
        Model.Password.Length >= 8 &&
        Model.Password.Any(char.IsLetter) &&
        Model.Password.Any(char.IsDigit);

    public bool ShouldShowConfirmPassword => IsPasswordValid;

    public bool CanContinueAccount =>
        !string.IsNullOrWhiteSpace(NormalizedName) &&
        !string.IsNullOrWhiteSpace(Model.Email) &&
        Model.Email.Contains('@', StringComparison.Ordinal) &&
        IsPasswordValid &&
        !string.IsNullOrEmpty(Model.ConfirmPassword) &&
        string.Equals(Model.Password, Model.ConfirmPassword, StringComparison.Ordinal);

    public bool CanCompleteProfile =>
        NormalizedNickname.Length >= 3 &&
        NormalizedNickname.All(character =>
            char.IsLetterOrDigit(character) || character is '.' or '_' or '-');

    public async Task<OnboardingStatus> InitializeAsync(bool hasAuthenticatedSession)
    {
        HasAuthenticatedSession = hasAuthenticatedSession;

        if (!hasAuthenticatedSession)
        {
            // Anonymous registration must not execute the authenticated Dashboard query.
            Step = ProfileCreationStep.Account;
            Model.Name = string.Empty;
            Model.Email = string.Empty;
            Model.Password = string.Empty;
            Model.ConfirmPassword = string.Empty;
            Model.Nickname = string.Empty;
            return new OnboardingStatus(false, false);
        }

        var user = await store.GetCurrentUserAsync();
        if (user is not null)
        {
            Model.Name = user.Name;
            Model.Email = user.Email;
            Step = ProfileCreationStep.Profile;
        }

        return new OnboardingStatus(user is not null, user?.HasProfile == true);
    }

    public async Task<OnboardingStatus> GetStatusAsync()
    {
        var user = await store.GetCurrentUserAsync();
        return new OnboardingStatus(user is not null, user?.HasProfile == true);
    }

    public bool ContinueToProfile()
    {
        ValidationError = null;

        if (string.IsNullOrWhiteSpace(NormalizedName))
        {
            ValidationError = localizer["FullNameRequired"];
            return false;
        }

        if (string.IsNullOrWhiteSpace(Model.Email) || !Model.Email.Contains('@', StringComparison.Ordinal))
        {
            ValidationError = localizer["ValidEmailRequired"];
            return false;
        }

        if (!IsPasswordValid)
        {
            ValidationError = localizer["PasswordRequirementsError"];
            return false;
        }

        if (!string.Equals(Model.Password, Model.ConfirmPassword, StringComparison.Ordinal))
        {
            ValidationError = localizer["PasswordsDoNotMatch"];
            return false;
        }

        Step = ProfileCreationStep.Profile;
        return true;
    }

    public void Back()
    {
        ValidationError = null;
        Step = ProfileCreationStep.Account;
    }

    public async Task<bool> CompleteProfileAsync()
    {
        if (IsBusy)
        {
            return false;
        }

        ValidationError = null;

        if (!CanCompleteProfile)
        {
            ValidationError = localizer["NicknameRequirementsError"];
            return false;
        }

        Model.Nickname = NormalizedNickname;
        IsBusy = true;

        try
        {
            if (!HasAuthenticatedSession)
            {
                await store.CreateAccountAsync(
                    NormalizedName,
                    Model.Email.Trim(),
                    Model.Password,
                    NormalizedNickname,
                    string.Empty);
            }
            else
            {
                await store.CompleteUserProfileAsync(
                    NormalizedName,
                    NormalizedNickname,
                    string.Empty);
            }

            toastService.ShowSuccess(localizer["WelcomeToast"]);
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
}

public enum ProfileCreationStep
{
    Account,
    Profile
}

public sealed record OnboardingStatus(bool HasUser, bool HasProfile);
