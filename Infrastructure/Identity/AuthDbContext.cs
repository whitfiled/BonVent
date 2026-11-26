using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Identity;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<EmailOutboxMessage> EmailOutbox => Set<EmailOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<RefreshToken>().HasKey(token => token.Id);
        modelBuilder.Entity<RefreshToken>().Property(token => token.TokenHash).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<RefreshToken>().HasIndex(token => token.TokenHash).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
        modelBuilder.Entity<AuditEvent>().HasKey(item => item.Id);
        modelBuilder.Entity<AuditEvent>().Property(item => item.Action).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(item => item.ResourceType).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(item => item.ResourceId).HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(item => item.Metadata).HasMaxLength(4000);
        modelBuilder.Entity<AuditEvent>().HasIndex(item => item.CreatedAtUtc);
        modelBuilder.Entity<EmailOutboxMessage>().HasKey(item => item.Id);
        modelBuilder.Entity<EmailOutboxMessage>().Property(item => item.Recipient).IsRequired().HasMaxLength(320);
        modelBuilder.Entity<EmailOutboxMessage>().Property(item => item.Subject).IsRequired().HasMaxLength(250);
        modelBuilder.Entity<EmailOutboxMessage>().Property(item => item.Body).IsRequired().HasMaxLength(10000);
        modelBuilder.Entity<EmailOutboxMessage>().HasIndex(item => new { item.SentAtUtc, item.CreatedAtUtc });
    }

    public async Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }
}
