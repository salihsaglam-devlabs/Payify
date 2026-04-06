using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.MoneyTransfer;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Consumers;

public class ReturnTransferCompletedConsumer : IConsumer<ReturnTransferCompleted>
{
    private readonly ILogger<ReturnTransferCompletedConsumer> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<ReturnTransactionRequest> _repository;

    public ReturnTransferCompletedConsumer(ILogger<ReturnTransferCompletedConsumer> logger,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService,
        IGenericRepository<ReturnTransactionRequest> repository)
    {
        _logger = logger;
        _auditLogService = auditLogService;
        _applicationUserService = applicationUserService;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<ReturnTransferCompleted> context)
    {
        var request = await _repository.GetAll()
            .FirstOrDefaultAsync(s =>
                s.Id == context.Message.TransactionSourceReferenceId);

        var transferResponse = context.Message;

        if (request is not null)
        {
            try
            {
                request.Status = transferResponse.MoneyTransferStatus == MoneyTransferStatus.PaymentSucceeded
                ? ReturnTransactionStatus.Completed
                : ReturnTransactionStatus.Failed;

                request.MoneyTransferPaymentDate = transferResponse.TransferDate;
                request.MoneyTransferReferenceId = transferResponse.MoneyTransferPaymentId;
                request.MoneyTransferStatus = transferResponse.MoneyTransferStatus;
                request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();

                await _repository.UpdateAsync(request);

                var details = new Dictionary<string, string>
                {
                    {"ReturnTransactionRequestId", transferResponse.TransactionSourceReferenceId.ToString() },
                    {"BankTransactionResponse", context.Message.BankTransactionResponse }
                };

                await SendAuditLogAsync(
                    transferResponse.MoneyTransferStatus == MoneyTransferStatus.PaymentSucceeded,
                    request.CreatedBy,
                    details);
            }
            catch (Exception exception)
            {
                _logger.LogError($"ReturnTransferCompleted Consumer Error {exception}", exception);

                var details = new Dictionary<string, string>
                {
                    {"ReturnTransactionRequestId",transferResponse.TransactionSourceReferenceId.ToString() },
                    {"ExceptionMessage",exception.Message}
                };

                await SendAuditLogAsync(
                    transferResponse.MoneyTransferStatus == MoneyTransferStatus.PaymentSucceeded,
                    request.CreatedBy,
                    details);
            }
        }

        return;
    }

    private async Task SendAuditLogAsync(bool isSuccess, string userId, Dictionary<string, string> details)
    {
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            parsedUserId = Guid.Empty;
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "ReturnTransferCompletedConsumer",
                Resource = "ReturnIncomingTransaction",
                SourceApplication = "Emoney",
                UserId = parsedUserId
            }
        );
    }
}
