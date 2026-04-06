using System.Data;
using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.Verify3ds;

public class Verify3dsCommandValidator : AbstractValidator<Verify3dsCommand>
{
    public Verify3dsCommandValidator()
    {
        RuleFor(s => s.OrderId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.FormCollection)
            .NotNull()
            .NotEmpty();
    }
}