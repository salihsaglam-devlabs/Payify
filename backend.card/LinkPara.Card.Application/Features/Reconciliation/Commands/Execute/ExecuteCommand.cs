using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Archive.Configuration;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
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
        private readonly LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService _service;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ArchiveOptions _archiveOptions;
        private readonly ILogger<ExecuteCommandHandler> _logger;
        private readonly IStringLocalizer _localizer;

        public ExecuteCommandHandler(
            LinkPara.Card.Application.Commons.Interfaces.Reconciliation.IReconciliationService service,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<ArchiveOptions> archiveOptions,
            ILogger<ExecuteCommandHandler> logger,
            Func<LocalizerResource, IStringLocalizer> localizerFactory)
        {
            _service = service;
            _serviceScopeFactory = serviceScopeFactory;
            _archiveOptions = archiveOptions.Value;
            _logger = logger;
            _localizer = localizerFactory(LocalizerResource.Messages);
        }

        public async Task<ExecuteResponse> Handle(ExecuteCommand request, CancellationToken cancellationToken)
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

        private bool ShouldRunArchive(ExecuteResponse response)
        {
            return _archiveOptions.AutoArchiveAfterExecute == true
                   && response.TotalSucceeded > 0;
        }
    }
}
