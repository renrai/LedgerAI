using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Interfaces;

namespace LedgerAI.Application.Services;

public sealed class DashboardService(
    ITransactionRepository transactions,
    ICategoryRepository categories,
    BudgetService budgetService,
    ICurrentUser currentUser)
{
    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(string? period, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var yearMonth = BudgetService.ParsePeriod(period);
        var (from, to) = (yearMonth.FirstDay, yearMonth.LastDay);

        var incomeTotals = await transactions.SumByCategoryAsync(userId, from, to, TransactionType.Income, ct);
        var expenseTotals = await transactions.SumByCategoryAsync(userId, from, to, TransactionType.Expense, ct);
        var balance = await transactions.GetBalanceAsync(userId, null, ct);
        var budgets = await budgetService.BuildStatusAsync(userId, yearMonth, ct);

        var categoryNames = (await categories.GetByUserAsync(userId, ct)).ToDictionary(c => c.Id, c => c.Name);

        var income = incomeTotals.Sum(t => t.Total);
        var expense = expenseTotals.Sum(t => t.Total);

        var breakdown = expenseTotals
            .OrderByDescending(t => t.Total)
            .Select(t => new CategoryBreakdownDto(
                t.CategoryId,
                t.CategoryId is { } id ? categoryNames.GetValueOrDefault(id, "?") : "Sem categoria",
                t.Total,
                t.Count,
                expense == 0 ? 0 : Math.Round((double)(t.Total / expense) * 100, 1)))
            .ToList();

        var uncategorized = expenseTotals.Concat(incomeTotals).Where(t => t.CategoryId is null).Sum(t => t.Count);

        return new MonthlySummaryDto(
            yearMonth.ToString(),
            income,
            expense,
            income - expense,
            balance,
            uncategorized,
            breakdown,
            budgets);
    }
}
