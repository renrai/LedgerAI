namespace LedgerAI.Application.DTOs;

public sealed record BudgetDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Period,
    decimal Limit,
    decimal Spent,
    double PercentUsed,
    bool Exceeded);

/// <summary>Cria ou atualiza o orçamento de uma categoria em uma competência (YYYY-MM).</summary>
public sealed record UpsertBudgetRequest(Guid CategoryId, string Period, decimal Limit);
