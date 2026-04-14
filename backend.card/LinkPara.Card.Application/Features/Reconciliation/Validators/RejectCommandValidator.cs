using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Reject;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class RejectCommandValidator : AbstractValidator<RejectCommand>
{
    public RejectCommandValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Messages", "LinkPara.Card.API");

        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.OperationId)
                .NotEqual(Guid.Empty)
                .WithMessage(localizer.GetString("Validation.OperationIdNotEmpty").Value);

            RuleFor(x => x.Request.Comment)
                .NotEmpty()
                .WithMessage(localizer.GetString("Validation.RejectCommentRequired").Value);
        });
    }
}

