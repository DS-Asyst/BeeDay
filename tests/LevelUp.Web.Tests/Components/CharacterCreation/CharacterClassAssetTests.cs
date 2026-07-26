using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.CharacterCreation.State;

namespace LevelUp.Web.Tests.Components.CharacterCreation;

public sealed class CharacterClassAssetTests
{
    [Fact]
    public void EverySupportedClassHasOneAsset()
    {
        var options = CharacterCreationState.ClassOptions;

        Assert.Equal(Enum.GetValues<CharacterClass>().Length, options.Count);
        Assert.Equal(Enum.GetValues<CharacterClass>().Order(), options.Select(option => option.Value).Order());
    }

    [Fact]
    public void ClassAssetPathsAreUniqueAndUsePixelPreservingPngFiles()
    {
        var paths = CharacterCreationState.ClassOptions.Select(option => option.ImagePath).ToArray();

        Assert.Equal(paths.Length, paths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(paths, path =>
        {
            Assert.StartsWith("/images/classes/classicon_", path, StringComparison.Ordinal);
            Assert.EndsWith(".png", path, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void LegacyJpegClassAssetsAreNoLongerReferenced()
    {
        Assert.DoesNotContain(
            CharacterCreationState.ClassOptions,
            option => option.ImagePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                      option.ImagePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
    }
}
