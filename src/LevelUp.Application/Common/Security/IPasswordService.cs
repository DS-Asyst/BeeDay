namespace LevelUp.Application.Common.Security;

public interface IPasswordService
{
    public string Hash(string password);
    public bool Verify(string password, string passwordHash);
}
