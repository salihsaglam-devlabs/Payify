using FluentValidation;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Reconciliation.Validators;

public class EvaluateCommandValidator : AbstractValidator<EvaluateCommand>
{
    public EvaluateCommandValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Exceptions", "LinkPara.Card.API");

        RuleFor(x => x.Request)
            .NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleForEach(x => x.Request.IngestionFileIds)
                .NotEqual(Guid.Empty);

            RuleFor(x => x.Request.IngestionFileIds)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.IngestionFileIds.Length > 0)
                .WithMessage(localizer.GetString("Validation.IngestionFileIdsDistinct").Value);

            When(x => x.Request.Options is not null, () =>
            {
                RuleFor(x => x.Request.Options!.ChunkSize)
                    .InclusiveBetween(100, 10_000);

                RuleFor(x => x.Request.Options!.ClaimTimeoutSeconds)
                    .InclusiveBetween(30, 3600);

                RuleFor(x => x.Request.Options!.ClaimRetryCount)
                    .InclusiveBetween(1, 10);
            });
        });
    }
}
