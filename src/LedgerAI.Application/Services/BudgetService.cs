using System.Globalization;
using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Mapping;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Application.Services;

public sealed class BudgetService(
    IBudgetRepository budgets,
    ICategoryRepository categories,
    ITransactionRepository transactions,
    ICurrentUser currentUser,
    IUnitOfWork uow)
{
    public async Task<IReadOnlyList<BudgetDto>> ListAsync(string? period, CancellationToken ct = default)
    {
        var yearMonth = ParsePeriod(period);
        return await BuildStatusAsync(currentUser.UserId, yearMonth, ct);
    }

    public async Task<BudgetDto> UpsertAsync(UpsertBudgetRequest request, CancellationToken ct = default)
    {
        var period = ParsePeriod(request.Period);
        var category = await categories.GetByIdAsync(request.CategoryId, currentUser.UserId, ct)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var budget = await budgets.GetByCategoryAndPeriodAsync(currentUser.UserId, category.Id, period, ct);

        if (budget is null)
        {
            budget = Budget.Create(currentUser.UserId, category, period, new Money(request.Limit));
            await budgets.AddAsync(budget, ct);
        }
        else
        {
            budget.ChangeLimit(new Money(request.Limit));
        }

        await uow.SaveChangesAsync(ct);

        var spent = await SpentAsync(currentUser.UserId, category.Id, period, ct);
        return budget.ToDto(category.Name, budget.Evaluate(spent));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var budget = await budgets.GetByIdAsync(id, currentUser.UserId, ct) ?? throw new NotFoundException(nameof(Budget), id);
        budgets.Remove(budget);
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Monta o status de todos os orçamentos de uma competência (usado também pelo dashboard).</summary>
    public async Task<IReadOnlyList<BudgetDto>> BuildStatusAsync(Guid userId, YearMonth period, CancellationToken ct = default)
    {
        var list = await budgets.GetByPeriodAsync(userId, period, ct);
        if (list.Count == 0) return [];

        var categoryNames = (await categories.GetByUserAsync(userId, ct)).ToDictionary(c => c.Id, c => c.Name);
        var totals = await transactions.SumByCategoryAsync(userId, period.FirstDay, period.LastDay, TransactionType.Expense, ct);
        var spentByCategory = totals.Where(t => t.CategoryId is not null).ToDictionary(t => t.CategoryId!.Value, t => t.Total);

        return
        [
            .. list.Select(b =>
            {
                var spent = new Money(spentByCategory.GetValueOrDefault(b.CategoryId), b.Limit.Currency);
                return b.ToDto(categoryNames.GetValueOrDefault(b.CategoryId, "?"), b.Evaluate(spent));
            })
        ];
    }

    private async Task<Money> SpentAsync(Guid userId, Guid categoryId, YearMonth period, CancellationToken ct)
    {
        var totals = await transactions.SumByCategoryAsync(userId, period.FirstDay, period.LastDay, TransactionType.Expense, ct);
        var total = totals.FirstOrDefault(t => t.CategoryId == categoryId)?.Total ?? 0;
        return new Money(total);
    }

    public static YearMonth ParsePeriod(string? period)
    {
        if (string.IsNullOrWhiteSpace(period))
            return YearMonth.Current;

        if (DateOnly.TryParseExact(period + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return YearMonth.FromDate(date);

        throw new DomainException("Período inválido. Use o formato YYYY-MM.");
    }
}
