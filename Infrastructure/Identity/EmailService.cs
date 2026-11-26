using Application.Common.Interfaces;

namespace Infrastructure.Identity;

internal sealed class EmailService(AuthDbContext db) : IEmailService
{
    public async Task QueueAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        db.EmailOutbox.Add(new EmailOutboxMessage { Recipient = recipient, Subject = subject, Body = body });
        await db.SaveChangesAsync(cancellationToken);
    }
}
