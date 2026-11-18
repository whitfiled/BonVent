namespace Application.Common.Interfaces;

internal interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
