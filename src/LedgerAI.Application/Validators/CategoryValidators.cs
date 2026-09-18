using FluentValidation;
using LedgerAI.Application.DTOs;

namespace LedgerAI.Application.Validators;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(60);
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Icon).MaximumLength(40);
    }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Icon).MaximumLength(40);
    }
}
