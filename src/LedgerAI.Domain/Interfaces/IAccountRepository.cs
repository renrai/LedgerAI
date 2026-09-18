using LedgerAI.Domain.Entities;

namespace LedgerAI.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetByUserAsync(Guid userId, bool includeArchived = false, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    void Remove(Account account);
}
