using FluentValidation;
using LinkPara.Card.Application.Features.Archive.Queries.PreviewArchive;

namespace LinkPara.Card.Application.Features.Archive.Validators;

public class PreviewArchiveQueryValidator : AbstractValidator<PreviewArchiveQuery>
{
    public PreviewArchiveQueryValidator()
    {
        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleForEach(x => x.Request.FileIds ?? Array.Empty<Guid>()).NotEqual(Guid.Empty);

            RuleFor(x => x.Request.FileIds ?? Array.Empty<Guid>())
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .When(x => (x.Request.FileIds?.Length ?? 0) > 0)
                .WithMessage("Archive preview fileIds must be distinct.");

            RuleFor(x => x.Request.Limit)
                .InclusiveBetween(1, 1000)
                .When(x => x.Request.Limit.HasValue);
        });
    }
}
