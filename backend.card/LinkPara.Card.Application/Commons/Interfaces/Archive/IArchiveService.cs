using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;

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
