using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Enums;

namespace LedgerAI.Domain.Interfaces;

public sealed record TransactionFilter(
    Guid UserId,
    DateOnly? From = null,
    DateOnly? To = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    bool? Uncategorized = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record CategoryTotal(Guid? CategoryId, decimal Total, int Count);

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<PagedResult<Transaction>> SearchAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetUncategorizedAsync(Guid userId, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryTotal>> SumByCategoryAsync(Guid userId, DateOnly from, DateOnly to, TransactionType type, CancellationToken ct = default);
    Task<decimal> GetBalanceAsync(Guid userId, Guid? accountId = null, CancellationToken ct = default);
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default);
    void Remove(Transaction transaction);
}
