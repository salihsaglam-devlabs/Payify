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
            RuleFor(x => x.Request.AggregateIds)
                .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
                .WithMessage("Archive preview aggregateIds cannot contain empty guid values.");

            RuleFor(x => x.Request.AggregateIds)
                .Must(ids => ids is null || ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.AggregateIds is { Length: > 0 })
                .WithMessage("Archive preview aggregateIds must be distinct.");

            RuleFor(x => x.Request.Limit)
                .InclusiveBetween(0, 1000)
                .When(x => x.Request.Limit.HasValue);
        });
    }
}
