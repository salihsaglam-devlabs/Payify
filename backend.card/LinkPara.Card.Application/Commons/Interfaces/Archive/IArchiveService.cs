using LinkPara.Card.Application.Commons.Models.Archive;

namespace LinkPara.Card.Application.Commons.Interfaces.Archive;

public interface IArchiveService
{
    Task<ArchivePreviewResponse> PreviewIngestionFilesAsync(
        ArchivePreviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ArchiveRunResponse> RunIngestionFilesAsync(
        ArchiveRunRequest request,
        CancellationToken cancellationToken = default);
}
