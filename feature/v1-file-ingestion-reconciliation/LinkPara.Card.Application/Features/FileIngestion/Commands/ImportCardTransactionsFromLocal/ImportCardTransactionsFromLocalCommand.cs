using FluentValidation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Enums;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using MediatR;

namespace LinkPara.Card.Application.Features.FileIngestion.Commands.ImportCardTransactionsFromLocal;

public class ImportCardTransactionsFromLocalCommand : IRequest<FileIngestionOperationResponse>
{
    public string DirectoryPath { get; set; }
    public bool ResetExisting { get; set; }
    public FileIngestionResetScope ResetScope { get; set; }
}

public class ImportCardTransactionsFromLocalCommandValidator : AbstractValidator<ImportCardTransactionsFromLocalCommand>
{
    public ImportCardTransactionsFromLocalCommandValidator()
    {
        RuleFor(x => x.ResetScope).IsInEnum();
    }
}

public class ImportCardTransactionsFromLocalCommandHandler : IRequestHandler<ImportCardTransactionsFromLocalCommand, FileIngestionOperationResponse>
{
    private readonly IFileIngestionService _fileIngestionService;

    public ImportCardTransactionsFromLocalCommandHandler(IFileIngestionService fileIngestionService)
    {
        _fileIngestionService = fileIngestionService;
    }

    public async Task<FileIngestionOperationResponse> Handle(ImportCardTransactionsFromLocalCommand request, CancellationToken cancellationToken)
    {
        var result = await _fileIngestionService.ImportCardTransactionsFromLocalDirectoryAsync(
            request.DirectoryPath,
            request.ResetExisting,
            request.ResetScope,
            cancellationToken);

        return FileIngestionOperationResponse.FromResults(FileIngestionSourceTypes.LocalCardTransactions, result);
    }
}
