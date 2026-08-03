using BeeDay.Domain.Entities;
using Xunit;

namespace BeeDay.Domain.Tests;

public sealed class UserSessionHardeningTests
{
    [Fact]
    public void NewUser_StartsAtSessionVersionOne()
    {
        var user = User.Create("Tiago", "tiago@beeday.invalid");

        Assert.Equal(1, user.SessionVersion);
    }

    [Fact]
    public void InvalidateSessions_AdvancesSessionVersion()
    {
        var user = User.Create("Tiago", "tiago@beeday.invalid");

        user.InvalidateSessions();

        Assert.Equal(2, user.SessionVersion);
    }

    [Fact]
    public void InvalidateSessions_CalledTwice_AdvancesTwice()
    {
        var user = User.Create("Tiago", "tiago@beeday.invalid");

        user.InvalidateSessions();
        user.InvalidateSessions();

        Assert.Equal(3, user.SessionVersion);
    }

    [Fact]
    public void SetActive_False_InvalidatesSessions()
    {
        var user = User.Create("Tiago", "tiago@beeday.invalid");

        user.SetActive(false);

        Assert.False(user.IsActive);
        Assert.Equal(2, user.SessionVersion);
    }

    [Fact]
    public void SetActive_True_DoesNotInvalidateSessions()
    {
        var user = User.Create("Tiago", "tiago@beeday.invalid");
        user.SetActive(false);

        user.SetActive(true);

        Assert.True(user.IsActive);
        Assert.Equal(2, user.SessionVersion);
    }
}
