using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Archive.Commands.RunArchive;

public class RunArchiveCommand : IRequest<ArchiveRunResponse>
{
    public ArchiveRunRequest Request { get; set; } = new();
}

public class RunArchiveCommandHandler : IRequestHandler<RunArchiveCommand, ArchiveRunResponse>
{
    private readonly IArchiveService _archiveService;
    private readonly IArchiveErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<RunArchiveCommandHandler> _logger;

    public RunArchiveCommandHandler(
        IArchiveService archiveService,
        IArchiveErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<RunArchiveCommandHandler> logger)
    {
        _archiveService = archiveService;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<ArchiveRunResponse> Handle(RunArchiveCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _archiveService.RunAsync(request.Request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Archive.RunFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_ARCHIVE_RUN");
            return new ArchiveRunResponse
            {
                Message = _localizer.Get("Handler.Archive.RunFailed"),
                ErrorCount = 1,
                Errors = new List<ArchiveErrorDetail> { error }
            };
        }
    }
}
