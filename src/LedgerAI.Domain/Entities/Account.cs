using LedgerAI.Domain.Common;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Domain.Entities;

public class Account : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public string Currency { get; private set; } = Money.DefaultCurrency;
    public bool IsArchived { get; private set; }

    private Account() { } // EF Core

    public static Account Create(Guid userId, string name, AccountType type, string currency = Money.DefaultCurrency)
    {
        if (userId == Guid.Empty) throw new DomainException("Conta precisa de um usuário.");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Account
        {
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Currency = Money.Zero(currency).Currency // valida o código ISO 4217
        };
    }

    public void Update(string name, AccountType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Type = type;
        Touch();
    }

    public void Archive()
    {
        if (IsArchived) throw new DomainException("Conta já está arquivada.");
        IsArchived = true;
        Touch();
    }

    public void Restore()
    {
        IsArchived = false;
        Touch();
    }
}
