using LedgerAI.Domain.Common;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Events;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Domain.Entities;

public class Transaction : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateOnly OccurredAt { get; private set; }
    public string? Notes { get; private set; }
    public CategorizationSource CategorizationSource { get; private set; } = CategorizationSource.None;

    /// <summary>Confiança (0..1) informada pela IA quando a categoria foi sugerida automaticamente.</summary>
    public double? AiConfidence { get; private set; }

    public bool IsCategorized => CategoryId.HasValue;

    /// <summary>Valor com sinal: receitas positivas, despesas negativas.</summary>
    public decimal SignedAmount => Type == TransactionType.Income ? Amount.Amount : -Amount.Amount;

    private Transaction() { } // EF Core

    public static Transaction Create(
        Guid userId,
        Guid accountId,
        TransactionType type,
        Money amount,
        string description,
        DateOnly occurredAt,
        string? notes = null)
    {
        if (userId == Guid.Empty) throw new DomainException("Transação precisa de um usuário.");
        if (accountId == Guid.Empty) throw new DomainException("Transação precisa de uma conta.");
        if (amount.Amount == 0) throw new DomainException("Valor da transação deve ser maior que zero.");
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            Type = type,
            Amount = amount,
            Description = description.Trim(),
            OccurredAt = occurredAt,
            Notes = notes
        };

        transaction.Raise(new TransactionCreatedEvent(transaction.Id, userId));
        return transaction;
    }

    public void Update(Guid accountId, TransactionType type, Money amount, string description, DateOnly occurredAt, string? notes)
    {
        if (accountId == Guid.Empty) throw new DomainException("Transação precisa de uma conta.");
        if (amount.Amount == 0) throw new DomainException("Valor da transação deve ser maior que zero.");
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        // Se o tipo mudou, a categoria atual deixa de fazer sentido.
        if (type != Type) ClearCategory();

        AccountId = accountId;
        Type = type;
        Amount = amount;
        Description = description.Trim();
        OccurredAt = occurredAt;
        Notes = notes;
        Touch();
    }

    public void Categorize(Category category, CategorizationSource source, double? confidence = null)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (category.UserId != UserId) throw new DomainException("Categoria pertence a outro usuário.");
        if (category.Kind != Type) throw new DomainException($"Categoria '{category.Name}' é de {category.Kind}, mas a transação é de {Type}.");
        if (source == CategorizationSource.Ai && confidence is null or < 0 or > 1)
            throw new DomainException("Categorização por IA exige confiança entre 0 e 1.");

        CategoryId = category.Id;
        CategorizationSource = source;
        AiConfidence = source == CategorizationSource.Ai ? confidence : null;
        Touch();
    }

    public void ClearCategory()
    {
        CategoryId = null;
        CategorizationSource = CategorizationSource.None;
        AiConfidence = null;
        Touch();
    }
}
