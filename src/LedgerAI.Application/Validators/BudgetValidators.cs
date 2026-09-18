using System.Text.RegularExpressions;
using FluentValidation;
using LedgerAI.Application.DTOs;

namespace LedgerAI.Application.Validators;

public sealed partial class UpsertBudgetRequestValidator : AbstractValidator<UpsertBudgetRequest>
{
    public UpsertBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Period).NotEmpty().Matches(PeriodRegex()).WithMessage("Período deve estar no formato YYYY-MM.");
        RuleFor(x => x.Limit).GreaterThan(0).WithMessage("Limite deve ser maior que zero.");
    }

    // Source generator de Regex (.NET 7+): compila em tempo de build, sem custo de runtime.
    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex PeriodRegex();
}
