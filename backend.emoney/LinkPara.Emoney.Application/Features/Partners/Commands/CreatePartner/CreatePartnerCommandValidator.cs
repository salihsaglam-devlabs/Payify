using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Partners.Commands.CreatePartner;

public class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator()
    {
        RuleFor(s => s.Email)
            .NotEmpty()
            .NotNull()
            .MaximumLength(300)
            .EmailAddress();

        RuleFor(s => s.Name)
            .NotEmpty()
            .NotNull()
            .MaximumLength(500);

        RuleFor(s => s.PhoneCode)
            .NotEmpty()
            .NotNull()
            .MaximumLength(20);

        RuleFor(s => s.PhoneNumber)
            .NotEmpty()
            .NotNull()
            .MaximumLength(50);
    }
}
