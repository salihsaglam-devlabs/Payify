using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.Init3ds;

public class Init3dsCommandValidator : AbstractValidator<Init3dsCommand>
{
    public Init3dsCommandValidator()
    {
        RuleFor(s => s.ThreeDSessionId)
           .NotNull()
           .NotEmpty();

        RuleFor(s => s.MerchantId)
           .NotNull()
           .NotEmpty();

        RuleFor(s => s.CallbackUrl)
           .NotNull()
           .NotEmpty();
    }
}
