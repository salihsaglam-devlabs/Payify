using FluentValidation;
using LinkPara.Card.Application.Features.Archive.Commands.RunArchive;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.Archive.Validators;

public class RunArchiveCommandValidator : AbstractValidator<RunArchiveCommand>
{
    public RunArchiveCommandValidator(IStringLocalizerFactory factory)
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

            RuleFor(x => x.Request.MaxFiles)
                .InclusiveBetween(0, 1000)
                .When(x => x.Request.MaxFiles.HasValue)
                .WithMessage(localizer.GetString("Validation.ArchiveMaxFilesRange").Value);
        });
    }
}
