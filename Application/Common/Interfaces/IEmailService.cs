namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task QueueAsync(string recipient, string subject, string body, CancellationToken cancellationToken);
}
