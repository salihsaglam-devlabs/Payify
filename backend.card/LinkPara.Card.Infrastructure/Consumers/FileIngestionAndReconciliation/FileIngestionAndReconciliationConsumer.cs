using LinkPara.Card.Application.Commons.Models.FileIngestion.Shared;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Features.FileIngestion.Commands.IngestFile;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Execute;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Domain.Enums.FileIngestion;
using MassTransit;
using MediatR;

namespace LinkPara.Card.Infrastructure.Consumers.FileIngestionAndReconciliation;

public class FileIngestionAndReconciliationConsumer : IConsumer<FileIngestionAndReconciliationJobRequest>
{
    private readonly ISender _mediator;
    private readonly IAuditStampService _auditStampService;
    private readonly IStringLocalizer _localizer;
    private readonly bool _respondToContext;

    public FileIngestionAndReconciliationConsumer(
        ISender mediator,
        IAuditStampService auditStampService,
        IOptions<ReconciliationOptions> reconciliationOptions,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _mediator = mediator;
        _auditStampService = auditStampService;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
        _respondToContext = reconciliationOptions.Value.Consumer.RespondToContext.Value;
    }

    public async Task Consume(ConsumeContext<FileIngestionAndReconciliationJobRequest> context)
    {
        var message = context.Message;
        var previousUserId = _auditStampService.GetCurrentAuditUserId();
        _auditStampService.SetAuditUserId(ResolveInitiatedByUserId(context));

        try
        {
            switch (message.Type)
            {
                case FileIngestionAndReconciliationJobType.IngestFile:
                {
                    if (message.IngestionRequest is null)
                    {
                        await RespondIfEnabled(context, new FileIngestionAndReconciliationJobResponse
                        {
                            Type = message.Type,
                            IsSuccess = false
                        });

                        return;
                    }

                    var ingestCmd = new FileIngestionCommand
                    {
                        FileSourceType = message.IngestionRequest.FileSourceType,
                        FileType = message.IngestionRequest.FileType,
                        FileContentType = message.IngestionRequest.FileContentType,
                        FilePath = message.IngestionRequest.FilePath
                    };

                    var result = await _mediator.Send(ingestCmd, context.CancellationToken);

                    await RespondIfEnabled(context, new FileIngestionAndReconciliationJobResponse
                    {
                        Type = message.Type,
                        IsSuccess = IsSuccessful(result),
                        IngestionResponses = result
                    });

                    break;
                }

                case FileIngestionAndReconciliationJobType.Evaluate:
                {
                    var evalReq = message.EvaluateRequest ?? new EvaluateRequest();

                    var result = await _mediator.Send(
                        new EvaluateCommand { Request = evalReq },
                        context.CancellationToken);

                    await RespondIfEnabled(context, new FileIngestionAndReconciliationJobResponse
                    {
                        Type = message.Type,
                        IsSuccess = IsSuccessful(result),
                        EvaluateResponse = result
                    });

                    break;
                }

                case FileIngestionAndReconciliationJobType.Execute:
                {
                    var execReq = message.ExecuteRequest ?? new ExecuteRequest();

                    var result = await _mediator.Send(
                        new ExecuteCommand { Request = execReq },
                        context.CancellationToken);

                    await RespondIfEnabled(context, new FileIngestionAndReconciliationJobResponse
                    {
                        Type = message.Type,
                        IsSuccess = IsSuccessful(result),
                        ExecuteResponse = result
                    });

                    break;
                }

                default:
                {
                    await RespondIfEnabled(context, new FileIngestionAndReconciliationJobResponse
                    {
                        Type = message.Type,
                        IsSuccess = false
                    });

                    return;
                }
            }
        }
        catch (Exception)
        {
            await RespondIfEnabled(context, new FileIngestionAndReconciliationJobResponse
            {
                Type = message.Type,
                IsSuccess = false
            });
            return;
        }
        finally
        {
            _auditStampService.SetAuditUserId(previousUserId);
        }
    }

    private async Task RespondIfEnabled(
        ConsumeContext<FileIngestionAndReconciliationJobRequest> context,
        FileIngestionAndReconciliationJobResponse response)
    {
        if (!_respondToContext)
            return;

        await context.RespondAsync(response);
    }

    private static string? ResolveInitiatedByUserId(ConsumeContext<FileIngestionAndReconciliationJobRequest> context)
    {
        if (!string.IsNullOrWhiteSpace(context.Message.InitiatedByUserId))
        {
            return context.Message.InitiatedByUserId;
        }

        foreach (var key in new[] { "InitiatedByUserId", "UserId", "user-id", "x-user-id" })
        {
            if (context.Headers.TryGetHeader(key, out var value) &&
                value is string headerValue &&
                !string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        return null;
    }

    private static bool IsSuccessful(IReadOnlyCollection<FileIngestionResponse> responses)
    {
        return responses.Count > 0 &&
               responses.All(x => x.Status == FileStatus.Success && x.Errors.Count == 0);
    }

    private static bool IsSuccessful(EvaluateResponse response)
    {
        return response.ErrorCount == 0;
    }

    private static bool IsSuccessful(ExecuteResponse response)
    {
        return response.ErrorCount == 0 &&
               response.TotalAttempted > 0 &&
               response.TotalFailed == 0;
    }
}
