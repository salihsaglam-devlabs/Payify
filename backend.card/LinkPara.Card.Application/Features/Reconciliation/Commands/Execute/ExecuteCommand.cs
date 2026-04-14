using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Archive.Configuration;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Features.Archive.Commands.RunArchive;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Application.Features.Reconciliation.Commands.Execute
{
    public class ExecuteCommand : IRequest<ExecuteResponse>
    {
        public ExecuteRequest Request { get; set; } = new ExecuteRequest();
    }

    public class ExecuteCommandHandler : IRequestHandler<ExecuteCommand, ExecuteResponse>
    {
        private readonly IReconciliationService _service;
        private readonly IReconciliationErrorMapper _errorMapper;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ArchiveOptions _archiveOptions;
        private readonly ILogger<ExecuteCommandHandler> _logger;
        private readonly IStringLocalizer _localizer;

        public ExecuteCommandHandler(
            IReconciliationService service,
            IReconciliationErrorMapper errorMapper,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<ArchiveOptions> archiveOptions,
            ILogger<ExecuteCommandHandler> logger,
            Func<LocalizerResource, IStringLocalizer> localizerFactory)
        {
            _service = service;
            _errorMapper = errorMapper;
            _serviceScopeFactory = serviceScopeFactory;
            _archiveOptions = archiveOptions.Value;
            _logger = logger;
            _localizer = localizerFactory(LocalizerResource.Messages);
        }

        public async Task<ExecuteResponse> Handle(ExecuteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _service.ExecuteAsync(request.Request, cancellationToken);

                if (ShouldRunArchive(response))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                            await mediator.Send(
                                new RunArchiveCommand
                                {
                                    Request = new ArchiveRunRequest()
                                },
                                CancellationToken.None);

                            _logger.LogInformation(_localizer.Get("Reconciliation.AutoArchiveCompleted"));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, _localizer.Get("Reconciliation.AutoArchiveFailed"));
                        }
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("Handler.Reconciliation.ExecuteFailed"));
                var error = _errorMapper.MapException(ex, "HANDLER_RECONCILIATION_EXECUTE");
                return new ExecuteResponse
                {
                    Message = _localizer.Get("Handler.Reconciliation.ExecuteFailed"),
                    ErrorCount = 1,
                    Errors = new List<ReconciliationErrorDetail> { error }
                };
            }
        }

        private bool ShouldRunArchive(ExecuteResponse response)
        {
            return _archiveOptions.AutoArchiveAfterExecute == true
                   && response.TotalSucceeded > 0;
        }
    }
}
