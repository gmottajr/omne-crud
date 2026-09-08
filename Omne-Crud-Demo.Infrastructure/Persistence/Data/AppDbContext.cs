using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Omne_Crud_Demo.Abstractions;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Infrastructure.Persistence.Data;

public sealed class AppDbContext : DbContext
{
    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger): base(options)
    {
        _logger = logger;
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();

        try
        {
            _logger.LogDebug("Saving changes to database. Tracked entries: {TrackedEntries}", ChangeTracker.Entries().Count());

            var affectedRows = await base.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database changes saved successfully. Affected rows: {AffectedRows}", affectedRows);

            return affectedRows;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed while saving changes.");

            throw;
        }
    }

    private void ApplyAuditFields()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(entry =>
                entry.State == EntityState.Added ||
                entry.State == EntityState.Modified)
        .ToList();

        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {

            var entityType = entry.Entity.GetType().Name;

            var primaryKey = entry.Metadata.FindPrimaryKey();

            var entityId = primaryKey is not null
                ? string.Join(
                    ",",
                    primaryKey.Properties.Select(property =>
                        entry.Property(property.Name).CurrentValue?.ToString() ?? "null"))
                : "unknown";

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.MarkAsCreated(utcNow);
                    _logger.LogDebug(
                       "Audit metadata applied to newly created {EntityType}. " +
                       "CreatedAt: {CreatedAt}.",
                       entityType,
                       utcNow);
                    break;

                case EntityState.Modified:
                    entry.Entity.MarkAsUpdated(utcNow);
                    _logger.LogDebug(
                        "Audit metadata applied to modified {EntityType}. " +
                        "EntityId: {EntityId}, UpdatedAt: {UpdatedAt}.",
                        entityType,
                        entityId,
                        utcNow);

                    break;
            }
        }
    }
}
