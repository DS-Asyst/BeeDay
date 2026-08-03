using BeeDay.Domain.Exceptions;
using BeeDay.Domain.ValueObjects;
using Xunit;

namespace BeeDay.Domain.Tests;

public sealed class ValueObjectTests
{
    [Fact]
    public void ActivityTitle_NormalizesWhitespace()
    {
        var title = ActivityTitle.Create("  Read a book  ");
        Assert.Equal("Read a book", title.Value);
    }

    [Fact]
    public void ActivityDescription_RejectsTextAboveLimit()
    {
        var value = new string('x', ActivityDescription.MaximumLength + 1);
        Assert.Throws<DomainValidationException>(() => ActivityDescription.Create(value));
    }

    [Fact]
    public void Nickname_RemovesAtPrefix()
    {
        var nickname = Nickname.Create("  @tiago  ");
        Assert.Equal("tiago", nickname.Value);
    }
}
