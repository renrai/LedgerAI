using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LedgerAI.Infrastructure.Repositories;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public async Task<IReadOnlyList<Category>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Kind).ThenBy(c => c.Name)
            .ToListAsync(ct);

    public Task<bool> NameExistsAsync(Guid userId, string name, CancellationToken ct = default) =>
        db.Categories.AnyAsync(c => c.UserId == userId && EF.Functions.ILike(c.Name, name.Trim()), ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await db.Categories.AddAsync(category, ct);

    public async Task AddRangeAsync(IEnumerable<Category> categories, CancellationToken ct = default) =>
        await db.Categories.AddRangeAsync(categories, ct);

    public void Remove(Category category) => db.Categories.Remove(category);
}
