using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Execute;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class ExecuteCommandValidator : AbstractValidator<ExecuteCommand>
{
    public ExecuteCommandValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Messages", "LinkPara.Card.API");

        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleForEach(x => x.Request.GroupIds)
                .NotEqual(Guid.Empty);

            RuleForEach(x => x.Request.EvaluationIds)
                .NotEqual(Guid.Empty);

            RuleForEach(x => x.Request.OperationIds)
                .NotEqual(Guid.Empty);

            RuleFor(x => x.Request.GroupIds)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.GroupIds.Length > 0)
                .WithMessage(localizer.GetString("Validation.GroupIdsDistinct").Value);

            RuleFor(x => x.Request.EvaluationIds)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.EvaluationIds.Length > 0)
                .WithMessage(localizer.GetString("Validation.EvaluationIdsDistinct").Value);

            RuleFor(x => x.Request.OperationIds)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.OperationIds.Length > 0)
                .WithMessage(localizer.GetString("Validation.OperationIdsDistinct").Value);

            When(x => x.Request.Options is not null, () =>
            {
                RuleFor(x => x.Request.Options!.MaxEvaluations)
                    .InclusiveBetween(1, 100_000)
                    .When(x => x.Request.Options!.MaxEvaluations.HasValue)
                    .WithMessage(localizer.GetString("Validation.ExecuteMaxEvaluationsRange").Value);

                RuleFor(x => x.Request.Options!.LeaseSeconds)
                    .InclusiveBetween(1, 3600)
                    .When(x => x.Request.Options!.LeaseSeconds.HasValue)
                    .WithMessage(localizer.GetString("Validation.ExecuteLeaseSecondsRange").Value);
            });
        });
    }
}
