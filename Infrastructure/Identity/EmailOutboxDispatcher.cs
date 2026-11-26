using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity;

internal sealed class EmailOutboxDispatcher(
    IServiceScopeFactory scopes,
    IOptions<SmtpOptions> options,
    ILogger<EmailOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await DispatchAsync(stoppingToken); }
            catch (Exception exception) { logger.LogError(exception, "Email outbox dispatch failed."); }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task DispatchAsync(CancellationToken cancellationToken)
    {
        var smtp = options.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.From)) return;

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var messages = await db.EmailOutbox.Where(item => item.SentAtUtc == null).OrderBy(item => item.CreatedAtUtc).Take(20).ToListAsync(cancellationToken);
        if (messages.Count == 0) return;

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(smtp.UserName) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(smtp.UserName, smtp.Password)
        };

        foreach (var message in messages)
        {
            try
            {
                await client.SendMailAsync(new MailMessage(smtp.From, message.Recipient, message.Subject, message.Body), cancellationToken);
                message.SentAtUtc = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                message.Attempts++;
                logger.LogWarning(exception, "Could not send email outbox message {MessageId}.", message.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
