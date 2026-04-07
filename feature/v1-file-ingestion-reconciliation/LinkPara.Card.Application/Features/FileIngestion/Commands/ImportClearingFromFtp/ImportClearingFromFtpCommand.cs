using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Enums;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using MediatR;

namespace LinkPara.Card.Application.Features.FileIngestion.Commands.ImportClearingFromFtp;

public class ImportClearingFromFtpCommand : IRequest<FileIngestionOperationResponse>
{
    public bool ResetExisting { get; set; }
    public FileIngestionResetScope ResetScope { get; set; }
}

public class ImportClearingFromFtpCommandValidator : AbstractValidator<ImportClearingFromFtpCommand>
{
    public ImportClearingFromFtpCommandValidator()
    {
        RuleFor(x => x.ResetScope).IsInEnum();
    }
}

public class ImportClearingFromFtpCommandHandler : IRequestHandler<ImportClearingFromFtpCommand, FileIngestionOperationResponse>
{
    private readonly IFileIngestionService _fileIngestionService;

    public ImportClearingFromFtpCommandHandler(IFileIngestionService fileIngestionService)
    {
        _fileIngestionService = fileIngestionService;
    }

    public async Task<FileIngestionOperationResponse> Handle(ImportClearingFromFtpCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileIngestionService.ImportClearingFromFtpAsync(
            request.ResetExisting,
            request.ResetScope,
            cancellationToken);

        return FileIngestionOperationResponse.FromResults(FileIngestionSourceTypes.FtpClearing, result);
    }
}
