using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.Billing.Application.Features.Billing.Commands.CancelBillAccounting;

public class CancelBillAccountingCommand : IRequest<BillCancelAccountingResponseDto>
{
    public Guid TransactionId { get; set; }
}

public class CancelBillAccountingCommandHandler : IRequestHandler<CancelBillAccountingCommand, BillCancelAccountingResponseDto>
{
    private readonly IGenericRepository<Transaction> _transactionService;
    private readonly IEmoneyService _eMoneyService;
    private readonly IAccountingService _accountingService;
    private readonly ILogger<CancelBillAccountingCommandHandler> _logger;
    private readonly IReconciliationService _reconciliationService;

    public CancelBillAccountingCommandHandler(IGenericRepository<Transaction> transactionService,
        IEmoneyService eMoneyService,
        IAccountingService accountingService,
        ILogger<CancelBillAccountingCommandHandler> logger,
        IReconciliationService reconciliationService)
    {
        _transactionService = transactionService;
        _eMoneyService = eMoneyService;
        _accountingService = accountingService;
        _logger = logger;
        _reconciliationService = reconciliationService;
    }

    public async Task<BillCancelAccountingResponseDto> Handle(CancelBillAccountingCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionService.GetByIdAsync(request.TransactionId);

        transaction.Description = "AccountingCancelled";
        transaction.TransactionStatus = TransactionStatus.AccountingCancelled;
        transaction.UpdateDate = DateTime.Now;

        var provisionResponse = await _eMoneyService.CancelProvisionAsync(transaction.ProvisionReferenceId);

        if (!provisionResponse.IsSucceed)
        {
            var errorMessage = $"ProvisionCancelFailedWhenTryingToCancelAccounting" +
                $"ProvisionReference: {transaction.ProvisionReferenceId}," +
                $"Error: {provisionResponse.ErrorMessage}";

            _logger.LogError("An error occurred: {ErrorMessage}", errorMessage);


            throw new BillAccountingCancelException(errorMessage);
        }

        await _accountingService.CancelAccountingPaymentAsync(transaction);
        await _transactionService.UpdateAsync(transaction);
        await _reconciliationService.CancelInstitutionPaymentByTransactionIdAsync(transaction.Id);

        return new BillCancelAccountingResponseDto
        {
            IsSuccess = true,
            ReferenceNumber = provisionResponse.ReferenceNumber
        };
    }
}
