using System.Collections.Concurrent;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence;

internal sealed class BookingConcurrencyGuard : IBookingConcurrencyGuard
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _roomLocks = new();

    public async ValueTask<IAsyncDisposable> AcquireAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var roomLock = _roomLocks.GetOrAdd(roomId, static _ => new SemaphoreSlim(1, 1));
        await roomLock.WaitAsync(cancellationToken);
        return new Releaser(roomLock);
    }

    private sealed class Releaser(SemaphoreSlim roomLock) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            roomLock.Release();
            return ValueTask.CompletedTask;
        }
    }
}
