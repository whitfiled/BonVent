using System.Collections.Concurrent;

namespace Infrastructure.Identity;

internal sealed class RefreshTokenConcurrencyGuard
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public async ValueTask<IAsyncDisposable> AcquireAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var tokenLock = _locks.GetOrAdd(tokenHash, static _ => new SemaphoreSlim(1, 1));
        await tokenLock.WaitAsync(cancellationToken);
        return new Releaser(tokenLock);
    }

    private sealed class Releaser(SemaphoreSlim tokenLock) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            tokenLock.Release();
            return ValueTask.CompletedTask;
        }
    }
}
