namespace BeeDay.Application.Common.Identity;

public interface IUserTokenService
{
    public string GenerateToken();
    public string HashToken(string token);
}
