using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LedgerAI.Infrastructure.Repositories;

public sealed class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    public async Task<PagedResult<Transaction>> SearchAsync(TransactionFilter filter, CancellationToken ct = default)
    {
        var query = db.Transactions.AsNoTracking().Where(t => t.UserId == filter.UserId);

        if (filter.From is { } from) query = query.Where(t => t.OccurredAt >= from);
        if (filter.To is { } to) query = query.Where(t => t.OccurredAt <= to);
        if (filter.AccountId is { } accountId) query = query.Where(t => t.AccountId == accountId);
        if (filter.CategoryId is { } categoryId) query = query.Where(t => t.CategoryId == categoryId);
        if (filter.Type is { } type) query = query.Where(t => t.Type == type);
        if (filter.Uncategorized is true) query = query.Where(t => t.CategoryId == null);
        if (filter.Uncategorized is false) query = query.Where(t => t.CategoryId != null);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Description, pattern));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.OccurredAt)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Transaction>(items, filter.Page, filter.PageSize, total);
    }

    public async Task<IReadOnlyList<Transaction>> GetUncategorizedAsync(Guid userId, int limit = 50, CancellationToken ct = default) =>
        await db.Transactions
            .Where(t => t.UserId == userId && t.CategoryId == null)
            .OrderByDescending(t => t.OccurredAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CategoryTotal>> SumByCategoryAsync(Guid userId, DateOnly from, DateOnly to, TransactionType type, CancellationToken ct = default) =>
        await db.Transactions.AsNoTracking()
            .Where(t => t.UserId == userId && t.Type == type && t.OccurredAt >= from && t.OccurredAt <= to)
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategoryTotal(g.Key, g.Sum(t => t.Amount.Amount), g.Count()))
            .ToListAsync(ct);

    public async Task<decimal> GetBalanceAsync(Guid userId, Guid? accountId = null, CancellationToken ct = default)
    {
        var query = db.Transactions.AsNoTracking().Where(t => t.UserId == userId);
        if (accountId is { } id) query = query.Where(t => t.AccountId == id);

        return await query.SumAsync(
            t => t.Type == TransactionType.Income ? t.Amount.Amount : -t.Amount.Amount,
            ct);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken ct = default) =>
        await db.Transactions.AddAsync(transaction, ct);

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default) =>
        await db.Transactions.AddRangeAsync(transactions, ct);

    public void Remove(Transaction transaction) => db.Transactions.Remove(transaction);
}
