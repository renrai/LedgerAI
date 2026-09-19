using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Domain.ValueObjects;
using LedgerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LedgerAI.Infrastructure.Repositories;

public sealed class BudgetRepository(AppDbContext db) : IBudgetRepository
{
    public Task<Budget?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct);

    public Task<Budget?> GetByCategoryAndPeriodAsync(Guid userId, Guid categoryId, YearMonth period, CancellationToken ct = default) =>
        db.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == categoryId && b.Period == period, ct);

    public async Task<IReadOnlyList<Budget>> GetByPeriodAsync(Guid userId, YearMonth period, CancellationToken ct = default) =>
        await db.Budgets.Where(b => b.UserId == userId && b.Period == period).ToListAsync(ct);

    public async Task AddAsync(Budget budget, CancellationToken ct = default) =>
        await db.Budgets.AddAsync(budget, ct);

    public void Remove(Budget budget) => db.Budgets.Remove(budget);
}
