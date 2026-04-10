using FluentValidation;
using LinkPara.Card.Application.Features.Archive.Queries.PreviewArchive;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Archive.Validators;

public class PreviewArchiveQueryValidator : AbstractValidator<PreviewArchiveQuery>
{
    public PreviewArchiveQueryValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Messages", "LinkPara.Card.API");

        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.IngestionFileIds)
                .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
                .WithMessage(localizer.GetString("Validation.ArchiveIngestionFileIdsNoEmpty").Value);

            RuleFor(x => x.Request.IngestionFileIds)
                .Must(ids => ids is null || ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.IngestionFileIds is { Length: > 0 })
                .WithMessage(localizer.GetString("Validation.ArchiveIngestionFileIdsDistinct").Value);

            RuleFor(x => x.Request.Limit)
                .InclusiveBetween(0, 10_000)
                .When(x => x.Request.Limit.HasValue)
                .WithMessage(localizer.GetString("Validation.ArchivePreviewLimitRange").Value);
        });
    }
}
