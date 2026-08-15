namespace BeeDay.Web.Resources;

/// <summary>
/// Marker type for resolving the shared, cross-feature resource catalog via
/// <c>IStringLocalizer&lt;SharedResources&gt;</c>. Carries no members — its only purpose is to
/// anchor <c>SharedResources.{culture}.resx</c> resolution by namespace/type convention. Feature
/// areas that outgrow this shared catalog get their own marker type and resx pair the same way,
/// instead of everything piling into this one file.
/// </summary>
public sealed class SharedResources;
