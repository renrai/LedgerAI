namespace LedgerAI.Application.DTOs;

public sealed record CategoryBreakdownDto(Guid? CategoryId, string CategoryName, decimal Total, int Count, double Percent);

public sealed record MonthlySummaryDto(
    string Period,
    decimal Income,
    decimal Expense,
    decimal Net,
    decimal TotalBalance,
    int UncategorizedCount,
    IReadOnlyList<CategoryBreakdownDto> ExpensesByCategory,
    IReadOnlyList<BudgetDto> Budgets);
