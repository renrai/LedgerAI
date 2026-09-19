using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Common;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LedgerAI.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventDispatcher dispatcher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("ledger");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Persiste as mudanças e, só depois do commit, despacha os eventos de domínio dos agregados
    /// rastreados. Os eventos são limpos antes do save para não serem reprocessados em cascata.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());

        var result = await base.SaveChangesAsync(ct);

        if (events.Count > 0)
            await dispatcher.DispatchAsync(events, ct);

        return result;
    }
}
