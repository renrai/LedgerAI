using System.Globalization;
using LedgerAI.Domain.Exceptions;

namespace LedgerAI.Domain.ValueObjects;

/// <summary>
/// Valor monetário imutável. Sempre não negativo — o sinal é dado pelo <c>TransactionType</c>.
/// Mapeado no EF Core 10 como <i>complex type</i> (colunas amount + currency na mesma tabela).
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    public const string DefaultCurrency = "BRL";

    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new DomainException("Valor monetário não pode ser negativo.");
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Moeda deve ser um código ISO 4217 de 3 letras.");

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0, currency);

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount, EnsureSameCurrency(a, b));

    public static Money operator -(Money a, Money b)
    {
        var currency = EnsureSameCurrency(a, b);
        return a.Amount < b.Amount
            ? throw new DomainException("Resultado da subtração seria negativo.")
            : new Money(a.Amount - b.Amount, currency);
    }

    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    public static bool operator <(Money a, Money b) => a.CompareTo(b) < 0;
    public static bool operator >(Money a, Money b) => a.CompareTo(b) > 0;
    public static bool operator <=(Money a, Money b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Money a, Money b) => a.CompareTo(b) >= 0;

    private static string EnsureSameCurrency(Money a, Money b) =>
        a.Currency == b.Currency
            ? a.Currency
            : throw new DomainException($"Moedas diferentes: {a.Currency} e {b.Currency}.");

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");
}
