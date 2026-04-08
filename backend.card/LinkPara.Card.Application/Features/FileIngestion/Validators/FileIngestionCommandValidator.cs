using FluentValidation;
using LinkPara.Card.Application.Features.FileIngestion.Commands.IngestFile;
using LinkPara.Card.Domain.Enums.FileIngestion;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Features.FileIngestion.Validators;

public class FileIngestionCommandValidator : AbstractValidator<FileIngestionCommand>
{
    public FileIngestionCommandValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Exceptions", "LinkPara.Card.API");

        RuleFor(x => x.FilePath)
            .Must(path => string.IsNullOrWhiteSpace(path))
            .When(x => x.FileSourceType == FileSourceType.Remote)
            .WithMessage(localizer.GetString("Validation.FilePathMustBeEmptyForRemote").Value);

        RuleFor(x => x.FilePath)
            .NotEmpty()
            .When(x => x.FileSourceType == FileSourceType.Local)
            .WithMessage(localizer.GetString("Validation.FilePathRequiredForLocal").Value);
    }
}
