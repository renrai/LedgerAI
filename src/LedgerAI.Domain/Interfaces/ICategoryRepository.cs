using LedgerAI.Domain.Entities;

namespace LedgerAI.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid userId, string name, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Category> categories, CancellationToken ct = default);
    void Remove(Category category);
}
