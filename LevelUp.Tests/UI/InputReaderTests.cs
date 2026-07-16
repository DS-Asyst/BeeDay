using LevelUp.UI;
using Xunit;

namespace LevelUp.Tests.UI;

public sealed class InputReaderTests
{
    [Theory]
    [InlineData("cancel")]
    [InlineData("CANCEL")]
    [InlineData("  cancel  ")]
    public void IsCancellationCommand_ShouldAcceptCaseAndWhitespace(
        string value
    )
    {
        Assert.True(InputReader.IsCancellationCommand(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("cancelar")]
    [InlineData("não")]
    public void IsCancellationCommand_ShouldRejectOtherValues(
        string value
    )
    {
        Assert.False(InputReader.IsCancellationCommand(value));
    }
}
