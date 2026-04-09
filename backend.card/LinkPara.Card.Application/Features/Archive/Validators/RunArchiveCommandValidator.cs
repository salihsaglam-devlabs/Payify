using FluentValidation;
using LinkPara.Card.Application.Features.Archive.Commands.RunArchive;

namespace LinkPara.Card.Application.Features.Archive.Validators;

public class RunArchiveCommandValidator : AbstractValidator<RunArchiveCommand>
{
    public RunArchiveCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.AggregateIds)
                .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
                .WithMessage("Archive run aggregateIds cannot contain empty guid values.");

            RuleFor(x => x.Request.AggregateIds)
                .Must(ids => ids is null || ids.Distinct().Count() == ids.Length)
                .When(x => x.Request.AggregateIds is { Length: > 0 })
                .WithMessage("Archive run aggregateIds must be distinct.");

            RuleFor(x => x.Request.MaxFiles)
                .InclusiveBetween(0, 1000)
                .When(x => x.Request.MaxFiles.HasValue);
        });
    }
}
