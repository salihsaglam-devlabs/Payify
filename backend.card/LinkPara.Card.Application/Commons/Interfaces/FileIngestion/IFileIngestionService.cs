using LinkPara.Card.Application.Commons.Models.FileIngestion;

namespace LinkPara.Card.Application.Commons.Interfaces.FileIngestion;

public interface IFileIngestionService
{
    Task<List<FileIngestionResponse>> IngestAsync(FileIngestionRequest request, CancellationToken cancellationToken = default);
}
