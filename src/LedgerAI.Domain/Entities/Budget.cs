using LedgerAI.Domain.Common;
using LedgerAI.Domain.Enums;
using LedgerAI.Domain.Exceptions;
using LedgerAI.Domain.ValueObjects;

namespace LedgerAI.Domain.Entities;

/// <summary>Limite de gastos de uma categoria em uma competência (mês/ano).</summary>
public class Budget : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public YearMonth Period { get; private set; }
    public Money Limit { get; private set; }

    private Budget() { } // EF Core

    public static Budget Create(Guid userId, Category category, YearMonth period, Money limit)
    {
        if (userId == Guid.Empty) throw new DomainException("Orçamento precisa de um usuário.");
        ArgumentNullException.ThrowIfNull(category);
        if (category.UserId != userId) throw new DomainException("Categoria pertence a outro usuário.");
        if (category.Kind != TransactionType.Expense) throw new DomainException("Orçamentos só se aplicam a categorias de despesa.");
        if (limit.Amount == 0) throw new DomainException("Limite do orçamento deve ser maior que zero.");

        return new Budget { UserId = userId, CategoryId = category.Id, Period = period, Limit = limit };
    }

    public void ChangeLimit(Money limit)
    {
        if (limit.Amount == 0) throw new DomainException("Limite do orçamento deve ser maior que zero.");
        Limit = limit;
        Touch();
    }

    public BudgetStatus Evaluate(Money spent)
    {
        var percent = (double)(spent.Amount / Limit.Amount) * 100;
        return new BudgetStatus(spent, Limit, Math.Round(percent, 1), spent > Limit);
    }
}

public readonly record struct BudgetStatus(Money Spent, Money Limit, double PercentUsed, bool Exceeded);
