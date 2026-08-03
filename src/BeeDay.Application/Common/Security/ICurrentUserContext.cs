namespace BeeDay.Application.Common.Security;

public interface ICurrentUserContext
{
    public Guid? UserId { get; }
}
