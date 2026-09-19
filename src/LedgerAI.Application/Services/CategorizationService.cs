using LedgerAI.Application.DTOs;
using LedgerAI.Application.Interfaces;
using LedgerAI.Application.Mapping;
using LedgerAI.Domain.Entities;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LedgerAI.Application.Services;

/// <summary>
/// Orquestra a categorização automática: busca transações sem categoria, pede sugestões ao
/// <see cref="ITransactionCategorizer"/> e aplica as que superam o limiar mínimo de confiança.
/// </summary>
public sealed class CategorizationService(
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ITransactionCategorizer categorizer,
    IUnitOfWork uow,
    ILogger<CategorizationService> logger)
{
    /// <summary>Sugestões abaixo deste valor são descartadas e a transação permanece sem categoria.</summary>
    public const double MinConfidence = 0.6;

    public async Task<CategorizationResult> CategorizePendingAsync(Guid userId, int limit = 50, CancellationToken ct = default)
    {
        var pending = await transactions.GetUncategorizedAsync(userId, limit, ct);
        return await CategorizeAsync(userId, pending, ct);
    }

    public async Task<CategorizationResult> CategorizeAsync(Guid userId, IReadOnlyList<Transaction> targets, CancellationToken ct = default)
    {
        if (targets.Count == 0)
            return new CategorizationResult(0, 0, []);

        var userCategories = await categories.GetByUserAsync(userId, ct);
        var byId = userCategories.ToDictionary(c => c.Id);
        var candidates = userCategories.Select(c => c.ToCandidate()).ToList();

        var suggestions = await categorizer.SuggestAsync(
            [.. targets.Select(t => t.ToCategorizationRequest())],
            candidates,
            ct);

        var suggestionByTx = suggestions.ToDictionary(s => s.TransactionId);
        var items = new List<CategorizedItem>(targets.Count);
        var categorized = 0;

        foreach (var transaction in targets)
        {
            if (!suggestionByTx.TryGetValue(transaction.Id, out var suggestion)
                || suggestion.CategoryId is not { } categoryId
                || !byId.TryGetValue(categoryId, out var category)
                || category.Kind != transaction.Type
                || suggestion.Confidence < MinConfidence)
            {
                items.Add(new CategorizedItem(transaction.Id, transaction.Description, null, null, suggestion?.Confidence ?? 0, suggestion?.Reason));
                continue;
            }

            transaction.Categorize(category, suggestion.Source, suggestion.Confidence);
            categorized++;
            items.Add(new CategorizedItem(transaction.Id, transaction.Description, category.Id, category.Name, suggestion.Confidence, suggestion.Reason));
        }

        if (categorized > 0)
            await uow.SaveChangesAsync(ct);

        logger.LogInformation("Categorização: {Categorized}/{Total} transações do usuário {UserId}", categorized, targets.Count, userId);

        return new CategorizationResult(targets.Count, categorized, items);
    }
}
