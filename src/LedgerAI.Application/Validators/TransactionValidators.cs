using FluentValidation;
using LedgerAI.Application.DTOs;

namespace LedgerAI.Application.Validators;

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Valor deve ser maior que zero.")
            .LessThanOrEqualTo(1_000_000_000);
        RuleFor(x => x.Description).NotEmpty().WithMessage("Descrição é obrigatória.").MaximumLength(200);
        RuleFor(x => x.OccurredAt).NotEmpty()
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Data não pode estar no futuro.");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(1_000_000_000);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class CategorizeTransactionRequestValidator : AbstractValidator<CategorizeTransactionRequest>
{
    public CategorizeTransactionRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
