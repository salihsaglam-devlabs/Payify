using FluentValidation;
using LinkPara.Card.Application.Features.FileIngestion.Commands.IngestFile;
using LinkPara.Card.Domain.Enums.FileIngestion;

namespace LinkPara.Card.Application.Features.FileIngestion.Validators;

public class FileIngestionCommandValidator : AbstractValidator<FileIngestionCommand>
{
    public FileIngestionCommandValidator()
    {
        RuleFor(x => x.FilePath)
            .Must(path => string.IsNullOrWhiteSpace(path))
            .When(x => x.FileSourceType == FileSourceType.Remote)
            .WithMessage("FilePath should not be provided when FileSourceType is Remote.");

        RuleFor(x => x.FilePath)
            .NotEmpty()
            .When(x => x.FileSourceType == FileSourceType.Local)
            .WithMessage("FilePath should be provided when FileSourceType is Local.");
    }
}
