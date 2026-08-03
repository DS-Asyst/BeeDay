namespace LevelUp.Application.Features.Users.Requests;

public sealed record ChangeUserPasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
