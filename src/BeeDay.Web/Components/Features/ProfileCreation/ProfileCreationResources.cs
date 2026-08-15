namespace BeeDay.Web.Components.Features.ProfileCreation;

/// <summary>
/// Marker type for resolving the Create Account flow's resource catalog via
/// <c>IStringLocalizer&lt;ProfileCreationResources&gt;</c>. Covers both <c>CreateProfile.razor</c>
/// and <c>ProfileCreationState</c> (the account/nickname client-side validation messages and the
/// completion toast), since they are two halves of the same page's UI text.
/// </summary>
public sealed class ProfileCreationResources;
