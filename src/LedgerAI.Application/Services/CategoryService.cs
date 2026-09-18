using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Mapping;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.Interfaces;

namespace LedgerAI.Application.Services;

public sealed class CategoryService(ICategoryRepository categories, ICurrentUser currentUser, IUnitOfWork uow)
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await categories.GetByUserAsync(currentUser.UserId, ct);
        return [.. list.Select(c => c.ToDto())];
    }

    public async Task<CategoryDto> GetAsync(Guid id, CancellationToken ct = default) =>
        (await FindAsync(id, ct)).ToDto();

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (await categories.NameExistsAsync(currentUser.UserId, request.Name, ct))
            throw new DomainException($"Já existe uma categoria chamada '{request.Name}'.");

        var category = Category.Create(currentUser.UserId, request.Name, request.Kind, request.Icon);
        await categories.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);
        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await FindAsync(id, ct);

        if (!string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase)
            && await categories.NameExistsAsync(currentUser.UserId, request.Name, ct))
            throw new DomainException($"Já existe uma categoria chamada '{request.Name}'.");

        category.Update(request.Name, request.Icon);
        await uow.SaveChangesAsync(ct);
        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await FindAsync(id, ct);
        category.EnsureDeletable();
        categories.Remove(category);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Category> FindAsync(Guid id, CancellationToken ct) =>
        await categories.GetByIdAsync(id, currentUser.UserId, ct) ?? throw new NotFoundException(nameof(Category), id);
}
