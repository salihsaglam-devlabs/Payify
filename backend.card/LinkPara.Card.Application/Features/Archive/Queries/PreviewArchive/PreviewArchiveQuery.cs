using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Archive.Queries.PreviewArchive;

public class PreviewArchiveQuery : IRequest<ArchivePreviewResponse>
{
    public ArchivePreviewRequest Request { get; set; } = new();
}

public class PreviewArchiveQueryHandler : IRequestHandler<PreviewArchiveQuery, ArchivePreviewResponse>
{
    private readonly IArchiveService _archiveService;
    private readonly IArchiveErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<PreviewArchiveQueryHandler> _logger;

    public PreviewArchiveQueryHandler(
        IArchiveService archiveService,
        IArchiveErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<PreviewArchiveQueryHandler> logger)
    {
        _archiveService = archiveService;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<ArchivePreviewResponse> Handle(PreviewArchiveQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _archiveService.PreviewAsync(request.Request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Archive.PreviewFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_ARCHIVE_PREVIEW");
            return new ArchivePreviewResponse
            {
                Message = _localizer.Get("Handler.Archive.PreviewFailed"),
                ErrorCount = 1,
                Errors = new List<ArchiveErrorDetail> { error }
            };
        }
    }
}
