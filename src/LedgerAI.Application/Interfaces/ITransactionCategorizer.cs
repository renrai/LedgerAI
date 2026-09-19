using LedgerAI.Domain.Enums;

namespace LedgerAI.Application.Interfaces;

/// <summary>Categoria disponível para o classificador escolher.</summary>
public sealed record CategorizationCandidate(Guid Id, string Name, TransactionType Kind);

/// <summary>Transação a ser classificada.</summary>
public sealed record CategorizationRequest(
    Guid TransactionId,
    string Description,
    decimal Amount,
    TransactionType Type,
    DateOnly OccurredAt);

/// <summary>Sugestão devolvida pelo classificador. <c>CategoryId</c> nulo significa "não sei".</summary>
public sealed record CategorizationSuggestion(
    Guid TransactionId,
    Guid? CategoryId,
    double Confidence,
    string? Reason = null,
    CategorizationSource Source = CategorizationSource.Ai);

/// <summary>
/// Abstração do classificador de transações. A implementação padrão usa um LLM via
/// <c>Microsoft.Extensions.AI</c>; a alternativa é um classificador por palavras-chave, usado
/// quando nenhum provedor de IA está configurado.
/// </summary>
public interface ITransactionCategorizer
{
    Task<IReadOnlyList<CategorizationSuggestion>> SuggestAsync(
        IReadOnlyList<CategorizationRequest> transactions,
        IReadOnlyList<CategorizationCandidate> categories,
        CancellationToken ct = default);
}
