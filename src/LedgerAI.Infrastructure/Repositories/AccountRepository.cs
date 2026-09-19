using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LedgerAI.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext db) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task<IReadOnlyList<Account>> GetByUserAsync(Guid userId, bool includeArchived = false, CancellationToken ct = default) =>
        await db.Accounts
            .Where(a => a.UserId == userId && (includeArchived || !a.IsArchived))
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Account account, CancellationToken ct = default) =>
        await db.Accounts.AddAsync(account, ct);

    public void Remove(Account account) => db.Accounts.Remove(account);
}
