namespace RentPakHaji.Common.Domain.Repositories;

/// <summary>
/// Unit of Work abstraction — wraps a single DB transaction.
/// Each service DbContext implements this.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
