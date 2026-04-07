using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Enums;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using MediatR;

namespace LinkPara.Card.Application.Features.FileIngestion.Commands.ImportClearingFromLocal;

public class ImportClearingFromLocalCommand : IRequest<FileIngestionOperationResponse>
{
    public string DirectoryPath { get; set; }
    public bool ResetExisting { get; set; }
    public FileIngestionResetScope ResetScope { get; set; }
}

public class ImportClearingFromLocalCommandValidator : AbstractValidator<ImportClearingFromLocalCommand>
{
    public ImportClearingFromLocalCommandValidator()
    {
        RuleFor(x => x.ResetScope).IsInEnum();
    }
}

public class ImportClearingFromLocalCommandHandler : IRequestHandler<ImportClearingFromLocalCommand, FileIngestionOperationResponse>
{
    private readonly IFileIngestionService _fileIngestionService;

    public ImportClearingFromLocalCommandHandler(IFileIngestionService fileIngestionService)
    {
        _fileIngestionService = fileIngestionService;
    }

    public async Task<FileIngestionOperationResponse> Handle(ImportClearingFromLocalCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileIngestionService.ImportClearingFromLocalDirectoryAsync(
            request.DirectoryPath,
            request.ResetExisting,
            request.ResetScope,
            cancellationToken);

        return FileIngestionOperationResponse.FromResults(FileIngestionSourceTypes.LocalClearing, result);
    }
}
