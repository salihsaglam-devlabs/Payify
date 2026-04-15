using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Approve;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class ApproveCommandValidator : AbstractValidator<ApproveCommand>
{
    public ApproveCommandValidator(IStringLocalizerFactory factory)
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
                .MaximumLength(2000)
                .When(x => x.Request.Comment is not null)
                .WithMessage(localizer.GetString("Validation.CommentMaxLength").Value);
        });
    }
}

