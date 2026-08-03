using LevelUp.Infrastructure.Persistence.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Single point where every <c>SaveChangesAsync</c> call in the SQL Server adapters (repositories and
/// <see cref="EfUnitOfWork"/>) runs, so EF Core's own exception types never leak past Infrastructure —
/// callers see the same <see cref="PersistenceException"/> hierarchy already used by the JSON provider.
/// </summary>
internal static class EfConcurrencySaveChanges
{
    public static async Task<int> ExecuteAsync(LevelUpDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "The record was modified or deleted by another operation since it was loaded.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new PersistenceException("The change could not be saved to SQL Server.", ex);
        }
    }
}
