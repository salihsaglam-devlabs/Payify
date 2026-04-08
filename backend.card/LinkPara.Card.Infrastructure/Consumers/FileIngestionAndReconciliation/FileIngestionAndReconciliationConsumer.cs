using LinkPara.Card.Application.Commons.Models.FileIngestion.Shared;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Features.FileIngestion.Commands.IngestFile;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Evaluate;
using LinkPara.Card.Application.Features.Reconciliation.Commands.Execute;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Interfaces.Localization;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Domain.Enums.FileIngestion;
using MassTransit;
using MediatR;

namespace LinkPara.Card.Infrastructure.Consumers.FileIngestionAndReconciliation;

public class FileIngestionAndReconciliationConsumer : IConsumer<FileIngestionAndReconciliationJobRequest>
{
    private readonly ISender _mediator;
    private readonly IAuditUserContextAccessor _auditUserContextAccessor;
    private readonly ICardResourceLocalizer _localizer;

    public FileIngestionAndReconciliationConsumer(
        ISender mediator,
        IAuditUserContextAccessor auditUserContextAccessor,
        ICardResourceLocalizer localizer)
    {
        _mediator = mediator;
        _auditUserContextAccessor = auditUserContextAccessor;
        _localizer = localizer;
    }

    public async Task Consume(ConsumeContext<FileIngestionAndReconciliationJobRequest> context)
    {
        var message = context.Message;
        var previousUserId = _auditUserContextAccessor.CurrentUserId;
        _auditUserContextAccessor.CurrentUserId = ResolveInitiatedByUserId(context);

        try
        {
            switch (message.Type)
            {
                case FileIngestionAndReconciliationJobType.IngestFile:
                {
                    if (message.IngestionRequest is null)
                        throw new InvalidOperationException(_localizer.Get("FileIngestion.ConsumerRequestMissing"));

                    var ingestCmd = new FileIngestionCommand
                    {
                        FileSourceType = message.IngestionRequest.FileSourceType,
                        FileType = message.IngestionRequest.FileType,
                        FileContentType = message.IngestionRequest.FileContentType,
                        FilePath = message.IngestionRequest.FilePath
                    };

                    var result = await _mediator.Send(ingestCmd, context.CancellationToken);

                    await context.RespondAsync(new FileIngestionAndReconciliationJobResponse
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

                    await context.RespondAsync(new FileIngestionAndReconciliationJobResponse
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

                    await context.RespondAsync(new FileIngestionAndReconciliationJobResponse
                    {
                        Type = message.Type,
                        IsSuccess = IsSuccessful(result),
                        ExecuteResponse = result
                    });

                    break;
                }

                default:
                    throw new NotSupportedException(_localizer.Get("Reconciliation.ConsumerUnsupportedJobType", message.Type));
            }
        }
        catch (Exception ex)
        {
            await context.RespondAsync(new FileIngestionAndReconciliationJobResponse
            {
                Type = message.Type,
                IsSuccess = false,
                ErrorMessage = ex.Message
            });

            throw;
        }
        finally
        {
            _auditUserContextAccessor.CurrentUserId = previousUserId;
        }
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
