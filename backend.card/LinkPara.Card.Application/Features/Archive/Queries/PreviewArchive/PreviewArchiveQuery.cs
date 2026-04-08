using LinkPara.Card.Application.Commons.Models.Archive;
using MediatR;

namespace LinkPara.Card.Application.Features.Archive.Queries.PreviewArchive;

public class PreviewArchiveQuery : IRequest<ArchivePreviewResponse>
{
    public ArchivePreviewRequest Request { get; set; } = new();
}

public class PreviewArchiveQueryHandler : IRequestHandler<PreviewArchiveQuery, ArchivePreviewResponse>
{
    private readonly LinkPara.Card.Application.Commons.Interfaces.Archive.IArchiveService _archiveService;

    public PreviewArchiveQueryHandler(LinkPara.Card.Application.Commons.Interfaces.Archive.IArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    public Task<ArchivePreviewResponse> Handle(PreviewArchiveQuery request, CancellationToken cancellationToken)
    {
        return _archiveService.PreviewIngestionFilesAsync(request.Request, cancellationToken);
    }
}
