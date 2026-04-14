using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.FileIngestion;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;
using LinkPara.Card.Domain.Enums.FileIngestion;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.FileIngestion.Commands.IngestFile;

public class FileIngestionCommand : IRequest<List<FileIngestionResponse>>
{
    public FileSourceType FileSourceType { get; set; }
    public FileType FileType { get; set; }
    public FileContentType FileContentType { get; set; }
    public string FilePath { get; set; }
}

public class FileIngestionCommandHandler : IRequestHandler<FileIngestionCommand, List<FileIngestionResponse>>
{
    private readonly IFileIngestionService _fileIngestionService;
    private readonly IIngestionErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<FileIngestionCommandHandler> _logger;

    public FileIngestionCommandHandler(
        IFileIngestionService fileIngestionService,
        IIngestionErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<FileIngestionCommandHandler> logger)
    {
        _fileIngestionService = fileIngestionService;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<List<FileIngestionResponse>> Handle(FileIngestionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _fileIngestionService.IngestAsync(new FileIngestionRequest
            {
                FileSourceType = request.FileSourceType,
                FileType = request.FileType,
                FileContentType = request.FileContentType,
                FilePath = request.FilePath
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.FileIngestion.Failed"));
            var error = _errorMapper.MapException(ex, "HANDLER_FILE_INGESTION");
            return new List<FileIngestionResponse>
            {
                new FileIngestionResponse
                {
                    FileName = request.FilePath ?? "unknown",
                    Status = FileStatus.Failed,
                    Message = _localizer.Get("Handler.FileIngestion.Failed"),
                    ErrorCount = 1,
                    Errors = new List<IngestionErrorDetail> { error }
                }
            };
        }
    }
}
