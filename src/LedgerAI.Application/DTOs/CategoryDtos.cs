using LedgerAI.Domain.Enums;

namespace LedgerAI.Application.DTOs;

public sealed record CategoryDto(Guid Id, string Name, TransactionType Kind, string? Icon, bool IsSystem);

public sealed record CreateCategoryRequest(string Name, TransactionType Kind, string? Icon = null);

public sealed record UpdateCategoryRequest(string Name, string? Icon = null);
