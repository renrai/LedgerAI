using LedgerAI.Domain.Exceptions;

namespace LedgerAI.Domain.ValueObjects;

/// <summary>Competência (ano/mês) de um orçamento. Persistida como inteiro YYYYMM.</summary>
public readonly record struct YearMonth : IComparable<YearMonth>
{
    public int Year { get; }
    public int Month { get; }

    public YearMonth(int year, int month)
    {
        if (year is < 2000 or > 2100)
            throw new DomainException("Ano fora do intervalo suportado (2000-2100).");
        if (month is < 1 or > 12)
            throw new DomainException("Mês deve estar entre 1 e 12.");

        Year = year;
        Month = month;
    }

    public int ToInt() => Year * 100 + Month;

    public static YearMonth FromInt(int value) => new(value / 100, value % 100);

    public static YearMonth FromDate(DateOnly date) => new(date.Year, date.Month);

    public static YearMonth Current => FromDate(DateOnly.FromDateTime(DateTime.UtcNow));

    public DateOnly FirstDay => new(Year, Month, 1);
    public DateOnly LastDay => new(Year, Month, DateTime.DaysInMonth(Year, Month));

    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    public int CompareTo(YearMonth other) => ToInt().CompareTo(other.ToInt());

    public override string ToString() => $"{Year:0000}-{Month:00}";
}
