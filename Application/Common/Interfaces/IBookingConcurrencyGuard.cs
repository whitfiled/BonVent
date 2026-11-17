namespace Application.Common.Interfaces;

internal interface IBookingConcurrencyGuard
{
    ValueTask<IAsyncDisposable> AcquireAsync(Guid roomId, CancellationToken cancellationToken);
}
