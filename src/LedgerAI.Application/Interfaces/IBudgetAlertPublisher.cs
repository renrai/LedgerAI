namespace LedgerAI.Application.Interfaces;

public enum BudgetAlertLevel
{
    Warning = 1,  // >= 80% do limite
    Exceeded = 2  // > 100% do limite
}

public sealed record BudgetAlert(
    Guid UserId,
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    string Period,
    decimal Spent,
    decimal Limit,
    double PercentUsed,
    BudgetAlertLevel Level,
    DateTimeOffset RaisedAt);

/// <summary>Publica alertas de orçamento para consumidores em tempo real (Server-Sent Events na API).</summary>
public interface IBudgetAlertPublisher
{
    ValueTask PublishAsync(BudgetAlert alert, CancellationToken ct = default);
}
