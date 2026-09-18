using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Interfaces;

namespace LedgerAI.Application.Mapping;

/// <summary>
/// Mapeamentos entidade → DTO usando <b>extension members</b> do C# 14:
/// um bloco <c>extension(T x)</c> agrupa métodos e propriedades de extensão do mesmo receptor.
/// </summary>
public static class DtoMappings
{
    extension(User user)
    {
        public UserDto ToDto() => new(user.Id, user.Name, user.Email, user.CreatedAt);
    }

    extension(Account account)
    {
        public AccountDto ToDto(decimal balance) => new(
            account.Id,
            account.Name,
            account.Type,
            account.Currency,
            account.IsArchived,
            balance,
            account.CreatedAt);
    }

    extension(Category category)
    {
        public CategoryDto ToDto() => new(category.Id, category.Name, category.Kind, category.Icon, category.IsSystem);

        public CategorizationCandidate ToCandidate() => new(category.Id, category.Name, category.Kind);
    }

    extension(Transaction transaction)
    {
        public TransactionDto ToDto() => new(
            transaction.Id,
            transaction.AccountId,
            transaction.CategoryId,
            transaction.Type,
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Description,
            transaction.OccurredAt,
            transaction.Notes,
            transaction.CategorizationSource,
            transaction.AiConfidence,
            transaction.CreatedAt);

        public CategorizationRequest ToCategorizationRequest() => new(
            transaction.Id,
            transaction.Description,
            transaction.Amount.Amount,
            transaction.Type,
            transaction.OccurredAt);
    }

    extension(Budget budget)
    {
        public BudgetDto ToDto(string categoryName, BudgetStatus status) => new(
            budget.Id,
            budget.CategoryId,
            categoryName,
            budget.Period.ToString(),
            budget.Limit.Amount,
            status.Spent.Amount,
            status.PercentUsed,
            status.Exceeded);
    }

    extension<T>(PagedResult<T> page)
    {
        public PagedResponse<TOut> ToResponse<TOut>(Func<T, TOut> map) =>
            new([.. page.Items.Select(map)], page.Page, page.PageSize, page.TotalCount, page.TotalPages);
    }
}
