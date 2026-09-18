using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Mapping;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.Interfaces;

namespace LedgerAI.Application.Services;

public sealed class AccountService(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    ICurrentUser currentUser,
    IUnitOfWork uow)
{
    public async Task<IReadOnlyList<AccountDto>> ListAsync(bool includeArchived = false, CancellationToken ct = default)
    {
        var list = await accounts.GetByUserAsync(currentUser.UserId, includeArchived, ct);
        var result = new List<AccountDto>(list.Count);

        foreach (var account in list)
        {
            var balance = await transactions.GetBalanceAsync(currentUser.UserId, account.Id, ct);
            result.Add(account.ToDto(balance));
        }

        return result;
    }

    public async Task<AccountDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var account = await FindAsync(id, ct);
        var balance = await transactions.GetBalanceAsync(currentUser.UserId, id, ct);
        return account.ToDto(balance);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        var account = Account.Create(currentUser.UserId, request.Name, request.Type, request.Currency);
        await accounts.AddAsync(account, ct);
        await uow.SaveChangesAsync(ct);
        return account.ToDto(0);
    }

    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default)
    {
        var account = await FindAsync(id, ct);
        account.Update(request.Name, request.Type);
        await uow.SaveChangesAsync(ct);

        var balance = await transactions.GetBalanceAsync(currentUser.UserId, id, ct);
        return account.ToDto(balance);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var account = await FindAsync(id, ct);
        account.Archive();
        await uow.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var account = await FindAsync(id, ct);
        account.Restore();
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Account> FindAsync(Guid id, CancellationToken ct) =>
        await accounts.GetByIdAsync(id, currentUser.UserId, ct) ?? throw new NotFoundException(nameof(Account), id);
}
