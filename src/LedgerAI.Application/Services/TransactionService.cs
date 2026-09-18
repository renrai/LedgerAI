using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Mapping;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Application.Services;

public sealed class TransactionService(
    ITransactionRepository transactions,
    IAccountRepository accounts,
    ICategoryRepository categories,
    ICurrentUser currentUser,
    IUnitOfWork uow)
{
    public async Task<PagedResponse<TransactionDto>> SearchAsync(TransactionQuery query, CancellationToken ct = default)
    {
        var filter = new TransactionFilter(
            currentUser.UserId,
            query.From,
            query.To,
            query.AccountId,
            query.CategoryId,
            query.Type,
            query.Uncategorized,
            query.Search,
            Math.Max(1, query.Page),
            Math.Clamp(query.PageSize, 1, 200));

        var page = await transactions.SearchAsync(filter, ct);
        return page.ToResponse(t => t.ToDto());
    }

    public async Task<TransactionDto> GetAsync(Guid id, CancellationToken ct = default) =>
        (await FindAsync(id, ct)).ToDto();

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken ct = default)
    {
        var account = await accounts.GetByIdAsync(request.AccountId, currentUser.UserId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        if (account.IsArchived)
            throw new DomainException("Não é possível lançar em uma conta arquivada.");

        var transaction = Transaction.Create(
            currentUser.UserId,
            account.Id,
            request.Type,
            new Money(request.Amount, account.Currency),
            request.Description,
            request.OccurredAt,
            request.Notes);

        if (request.CategoryId is { } categoryId)
        {
            var category = await categories.GetByIdAsync(categoryId, currentUser.UserId, ct)
                ?? throw new NotFoundException(nameof(Category), categoryId);
            transaction.Categorize(category, CategorizationSource.Manual);
        }

        // Se o usuário já categorizou (ou pediu para não categorizar), o evento de domínio
        // não deve disparar a IA. Limpar os eventos aqui evita trabalho desnecessário.
        if (!request.AutoCategorize || transaction.IsCategorized)
            transaction.ClearDomainEvents();

        await transactions.AddAsync(transaction, ct);
        await uow.SaveChangesAsync(ct);

        return transaction.ToDto();
    }

    public async Task<TransactionDto> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken ct = default)
    {
        var transaction = await FindAsync(id, ct);

        var account = await accounts.GetByIdAsync(request.AccountId, currentUser.UserId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        transaction.Update(
            account.Id,
            request.Type,
            new Money(request.Amount, account.Currency),
            request.Description,
            request.OccurredAt,
            request.Notes);

        await uow.SaveChangesAsync(ct);
        return transaction.ToDto();
    }

    public async Task<TransactionDto> CategorizeAsync(Guid id, CategorizeTransactionRequest request, CancellationToken ct = default)
    {
        var transaction = await FindAsync(id, ct);
        var category = await categories.GetByIdAsync(request.CategoryId, currentUser.UserId, ct)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        transaction.Categorize(category, CategorizationSource.Manual);
        await uow.SaveChangesAsync(ct);
        return transaction.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var transaction = await FindAsync(id, ct);
        transactions.Remove(transaction);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Transaction> FindAsync(Guid id, CancellationToken ct) =>
        await transactions.GetByIdAsync(id, currentUser.UserId, ct) ?? throw new NotFoundException(nameof(Transaction), id);
}
