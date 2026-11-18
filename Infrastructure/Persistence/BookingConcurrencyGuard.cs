using System.Collections.Concurrent;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

internal sealed class BookingConcurrencyGuard : IBookingConcurrencyGuard
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _roomLocks = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _idempotencyLocks = new();

    public async ValueTask<IAsyncDisposable> AcquireAsync(
        Guid roomId,
        Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var roomLock = _roomLocks.GetOrAdd(roomId, static _ => new SemaphoreSlim(1, 1));
        var idempotencyLock = _idempotencyLocks.GetOrAdd(idempotencyKey, static _ => new SemaphoreSlim(1, 1));

        await roomLock.WaitAsync(cancellationToken);
        try
        {
            await idempotencyLock.WaitAsync(cancellationToken);
            return new Releaser(roomLock, idempotencyLock);
        }
        catch
        {
            roomLock.Release();
            throw;
        }
    }

    private sealed class Releaser(SemaphoreSlim roomLock, SemaphoreSlim idempotencyLock) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            idempotencyLock.Release();
            roomLock.Release();
            return ValueTask.CompletedTask;
        }
    }
}
