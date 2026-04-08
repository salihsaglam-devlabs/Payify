using LinkPara.Card.Application.Commons.Models.Archive;

namespace LinkPara.Card.Application.Commons.Interfaces.Archive;

public interface IArchiveService
{
    Task<ArchivePreviewResponse> PreviewAsync(
        ArchivePreviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ArchiveRunResponse> RunAsync(
        ArchiveRunRequest request,
        CancellationToken cancellationToken = default);
}
