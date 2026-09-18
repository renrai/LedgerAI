using LedgerAI.Domain.Enums;

namespace LedgerAI.Application.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    string Description,
    DateOnly OccurredAt,
    string? Notes,
    CategorizationSource CategorizationSource,
    double? AiConfidence,
    DateTimeOffset CreatedAt);

public sealed record CreateTransactionRequest(
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateOnly OccurredAt,
    Guid? CategoryId = null,
    string? Notes = null,
    bool AutoCategorize = true);

public sealed record UpdateTransactionRequest(
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateOnly OccurredAt,
    string? Notes = null);

public sealed record CategorizeTransactionRequest(Guid CategoryId);

public sealed record TransactionQuery(
    DateOnly? From = null,
    DateOnly? To = null,
    Guid? AccountId = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    bool? Uncategorized = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

/// <summary>Resultado da categorização automática em lote.</summary>
public sealed record CategorizationResult(int Processed, int Categorized, IReadOnlyList<CategorizedItem> Items);

public sealed record CategorizedItem(Guid TransactionId, string Description, Guid? CategoryId, string? CategoryName, double Confidence, string? Reason);
