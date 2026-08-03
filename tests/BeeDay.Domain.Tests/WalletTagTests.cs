using BeeDay.Domain.Entities;
using BeeDay.Domain.Exceptions;
using Xunit;

namespace BeeDay.Domain.Tests;

public sealed class WalletTagTests
{
    [Fact]
    public void Create_normalizes_name_and_color()
    {
        var tag = WalletTag.Create(Guid.NewGuid(), "  Food   and drinks  ", "#7a4fcb");

        Assert.Equal("Food and drinks", tag.Name);
        Assert.Equal("#7A4FCB", tag.Color);
    }

    [Fact]
    public void Create_uses_default_color()
    {
        var tag = WalletTag.Create(Guid.NewGuid(), "Food");

        Assert.Equal(WalletTag.DefaultColor, tag.Color);
    }

    [Theory]
    [InlineData("")]
    [InlineData("7A4FCB")]
    [InlineData("#XYZXYZ")]
    [InlineData("#FFF")]
    public void Create_rejects_invalid_colors(string color)
    {
        if (color.Length == 0)
        {
            var tag = WalletTag.Create(Guid.NewGuid(), "Food", color);
            Assert.Equal(WalletTag.DefaultColor, tag.Color);
            return;
        }

        Assert.Throws<DomainValidationException>(() => WalletTag.Create(Guid.NewGuid(), "Food", color));
    }

    [Fact]
    public void Rename_and_change_color_update_tag()
    {
        var tag = WalletTag.Create(Guid.NewGuid(), "Food");

        tag.Rename("Transport");
        tag.ChangeColor("#112233");

        Assert.Equal("Transport", tag.Name);
        Assert.Equal("#112233", tag.Color);
    }
}
