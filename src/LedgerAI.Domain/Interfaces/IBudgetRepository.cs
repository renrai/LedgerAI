using LedgerAI.Domain.Entities;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<Budget?> GetByCategoryAndPeriodAsync(Guid userId, Guid categoryId, YearMonth period, CancellationToken ct = default);
    Task<IReadOnlyList<Budget>> GetByPeriodAsync(Guid userId, YearMonth period, CancellationToken ct = default);
    Task AddAsync(Budget budget, CancellationToken ct = default);
    void Remove(Budget budget);
}
