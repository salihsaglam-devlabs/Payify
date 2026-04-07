using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Enums;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using MediatR;

namespace LinkPara.Card.Application.Features.FileIngestion.Commands.ImportCardTransactionsFromFtp;

public class ImportCardTransactionsFromFtpCommand : IRequest<FileIngestionOperationResponse>
{
    public bool ResetExisting { get; set; }
    public FileIngestionResetScope ResetScope { get; set; }
}

public class ImportCardTransactionsFromFtpCommandValidator : AbstractValidator<ImportCardTransactionsFromFtpCommand>
{
    public ImportCardTransactionsFromFtpCommandValidator()
    {
        RuleFor(x => x.ResetScope).IsInEnum();
    }
}

public class ImportCardTransactionsFromFtpCommandHandler : IRequestHandler<ImportCardTransactionsFromFtpCommand, FileIngestionOperationResponse>
{
    private readonly IFileIngestionService _fileIngestionService;

    public ImportCardTransactionsFromFtpCommandHandler(IFileIngestionService fileIngestionService)
    {
        _fileIngestionService = fileIngestionService;
    }

    public async Task<FileIngestionOperationResponse> Handle(ImportCardTransactionsFromFtpCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileIngestionService.ImportCardTransactionsFromFtpAsync(
            request.ResetExisting,
            request.ResetScope,
            cancellationToken);

        return FileIngestionOperationResponse.FromResults(FileIngestionSourceTypes.FtpCardTransactions, result);
    }
}
