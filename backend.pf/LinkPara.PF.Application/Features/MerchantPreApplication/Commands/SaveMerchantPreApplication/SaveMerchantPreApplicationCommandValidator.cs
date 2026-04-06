using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Commands.SaveMerchantPreApplication;

public class SaveMerchantPreApplicationCommandValidator : AbstractValidator<SaveMerchantPreApplicationCommand>
{
    public SaveMerchantPreApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("Name is required.");
        RuleFor(x => x.Surname)
            .NotNull()
            .NotEmpty()
            .WithMessage("Surname is required.");
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .WithMessage("Email is required.");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^[0-9]{10,15}$")
            .WithMessage("Phone number is required and must be between 10 and 15 digits.");
        RuleFor(x => x.ProductTypes)
            .NotNull()
            .NotEmpty()
            .WithMessage("ProductType is required.");
        RuleFor(x => x.MonthlyTurnover)
            .NotNull()
            .NotEmpty()
            .WithMessage("MonthlyTurnover is required.")
            .IsInEnum();
        RuleFor(x => x.Website)
            .NotNull()
            .WithMessage("Website is required.")
            .NotEmpty();
        RuleFor(x => x.ConsentConfirmation)
            .NotNull()
            .NotEmpty()
            .WithMessage("ConsentConfirmation is required.")
            .Equal(true);
        RuleFor(x => x.KvkkConfirmation)
            .NotNull()
            .NotEmpty()
            .WithMessage("KvkkConfirmation is required.")
            .Equal(true);
    }
}