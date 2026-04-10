using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
using MediatR;

namespace LinkPara.Card.Application.Features.Archive.Commands.RunArchive;

public class RunArchiveCommand : IRequest<ArchiveRunResponse>
{
    public ArchiveRunRequest Request { get; set; } = new();
}

public class RunArchiveCommandHandler : IRequestHandler<RunArchiveCommand, ArchiveRunResponse>
{
    private readonly IArchiveService _archiveService;

    public RunArchiveCommandHandler(IArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    public Task<ArchiveRunResponse> Handle(RunArchiveCommand request, CancellationToken cancellationToken)
    {
        return _archiveService.RunAsync(request.Request, cancellationToken);
    }
}
