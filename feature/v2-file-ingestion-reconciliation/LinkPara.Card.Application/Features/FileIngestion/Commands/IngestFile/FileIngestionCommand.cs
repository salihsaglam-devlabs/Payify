using LinkPara.Card.Application.Commons.Interfaces.FileIngestion;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Domain.Enums.FileIngestion;
using MediatR;

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

    public FileIngestionCommandHandler(IFileIngestionService fileIngestionService)
    {
        _fileIngestionService = fileIngestionService;
    }

    public Task<List<FileIngestionResponse>> Handle(FileIngestionCommand request, CancellationToken cancellationToken)
    {
        return _fileIngestionService.IngestAsync(new FileIngestionRequest
        {
            FileSourceType = request.FileSourceType,
            FileType = request.FileType,
            FileContentType = request.FileContentType,
            FilePath = request.FilePath
        }, cancellationToken);
    }
}
