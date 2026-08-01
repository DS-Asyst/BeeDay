namespace LevelUp.Application.Common.Security;

public interface IPasswordService
{
    public string Hash(string password);
    public bool Verify(string password, string passwordHash);

    /// <summary>
    /// True when <paramref name="passwordHash"/> was produced with weaker parameters (e.g. a
    /// lower iteration count) than the service's current settings, and should be re-hashed with
    /// <see cref="Hash"/> next time the plaintext password is available (a successful login).
    /// </summary>
    public bool NeedsRehash(string passwordHash);
}
