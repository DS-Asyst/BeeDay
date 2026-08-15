using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Layout;

public sealed class PublicHomeLanguageState
{
    public UserLanguage Language { get; private set; } = UserLanguage.English;

    public event Action? Changed;

    public void SetLanguage(UserLanguage language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;
        Changed?.Invoke();
    }
}
