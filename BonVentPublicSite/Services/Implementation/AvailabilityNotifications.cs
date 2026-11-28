using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace BonVentPublicSite.Services.Implementation;

// Notifications cover reservations made through this MVC instance and contain no booking details.
public sealed class AvailabilityNotifications
{
    private readonly ConcurrentDictionary<Guid, Client> clients = new();
    private static readonly byte[] Message = Encoding.UTF8.GetBytes("availability-changed");

    public async Task AcceptAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest || !IsSameOrigin(context) || clients.Count >= 256)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var client = new Client(socket);
        var id = Guid.NewGuid();
        clients.TryAdd(id, client);
        try
        {
            var buffer = new byte[256];
            while (socket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
            {
                await socket.ReceiveAsync(buffer.AsMemory(), context.RequestAborted);
                break; // The endpoint only sends messages; clients cannot submit data.
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally { clients.TryRemove(id, out _); }
    }

    public async Task NotifyAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await Task.WhenAll(clients.Values.Select(client => client.SendAsync(timeout.Token)));
    }

    private static bool IsSameOrigin(HttpContext context)
    {
        if (!Uri.TryCreate(context.Request.Headers.Origin.ToString(), UriKind.Absolute, out var origin)
            || !Uri.TryCreate($"{context.Request.Scheme}://{context.Request.Host}", UriKind.Absolute, out var host))
            return false;
        return origin.Scheme == host.Scheme && origin.Host.Equals(host.Host, StringComparison.OrdinalIgnoreCase) && origin.Port == host.Port;
    }

    private sealed class Client(WebSocket socket)
    {
        private readonly SemaphoreSlim sendLock = new(1, 1);
        public async Task SendAsync(CancellationToken cancellationToken)
        {
            var acquired = false;
            try
            {
                await sendLock.WaitAsync(cancellationToken);
                acquired = true;
                if (socket.State == WebSocketState.Open)
                    await socket.SendAsync(Message.AsMemory(), WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            finally { if (acquired) sendLock.Release(); }
        }
    }
}
