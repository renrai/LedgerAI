using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Services;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Events;
using LedgerAI.Domain.Interfaces;
using LedgerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LedgerAI.Application.EventHandlers;

/// <summary>
/// Reage à criação de uma transação: categoriza via IA e, se a categoria tiver orçamento
/// na competência, avalia e publica alertas de 80% / 100%.
/// </summary>
public sealed class TransactionCreatedHandler(
    ITransactionRepository transactions,
    IBudgetRepository budgets,
    ICategoryRepository categories,
    CategorizationService categorization,
    BudgetService budgetService,
    IBudgetAlertPublisher alerts,
    ILogger<TransactionCreatedHandler> logger) : IDomainEventHandler<TransactionCreatedEvent>
{
    public async Task HandleAsync(TransactionCreatedEvent domainEvent, CancellationToken ct = default)
    {
        var transaction = await transactions.GetByIdAsync(domainEvent.TransactionId, domainEvent.UserId, ct);
        if (transaction is null) return;

        if (!transaction.IsCategorized)
        {
            try
            {
                await categorization.CategorizeAsync(domainEvent.UserId, [transaction], ct);
            }
            catch (Exception ex)
            {
                // Falha na IA nunca deve impedir o lançamento; a transação fica pendente.
                logger.LogWarning(ex, "Falha ao categorizar transação {TransactionId}", transaction.Id);
            }
        }

        if (transaction.CategoryId is { } categoryId && transaction.Type == Domain.Enums.TransactionType.Expense)
            await CheckBudgetAsync(transaction, categoryId, ct);
    }

    private async Task CheckBudgetAsync(Transaction transaction, Guid categoryId, CancellationToken ct)
    {
        var period = YearMonth.FromDate(transaction.OccurredAt);
        var budget = await budgets.GetByCategoryAndPeriodAsync(transaction.UserId, categoryId, period, ct);
        if (budget is null) return;

        var status = (await budgetService.BuildStatusAsync(transaction.UserId, period, ct))
            .FirstOrDefault(b => b.Id == budget.Id);
        if (status is null) return;

        var level = status.Exceeded ? BudgetAlertLevel.Exceeded
            : status.PercentUsed >= 80 ? BudgetAlertLevel.Warning
            : (BudgetAlertLevel?)null;

        if (level is null) return;

        var category = await categories.GetByIdAsync(categoryId, transaction.UserId, ct);

        await alerts.PublishAsync(new BudgetAlert(
            transaction.UserId,
            budget.Id,
            categoryId,
            category?.Name ?? "?",
            period.ToString(),
            status.Spent,
            status.Limit,
            status.PercentUsed,
            level.Value,
            DateTimeOffset.UtcNow), ct);
    }
}
