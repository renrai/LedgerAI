using LedgerAI.Domain.Enums;

namespace LedgerAI.Application.DTOs;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    string Currency,
    bool IsArchived,
    decimal Balance,
    DateTimeOffset CreatedAt);

public sealed record CreateAccountRequest(string Name, AccountType Type, string Currency = "BRL");

public sealed record UpdateAccountRequest(string Name, AccountType Type);
