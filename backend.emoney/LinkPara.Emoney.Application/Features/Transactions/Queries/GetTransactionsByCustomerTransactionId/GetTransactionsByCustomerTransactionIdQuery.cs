using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionsByCustomerTransactionId;

public class GetTransactionsByCustomerTransactionIdQuery : IRequest<List<CustomerTransactionDto>>
{
    public string CustomerTransactionId { get; set; }
}

public class GetTransactionsByCustomerTransactionIdQueryHandler : IRequestHandler<GetTransactionsByCustomerTransactionIdQuery, List<CustomerTransactionDto>>
{
    private readonly IGenericRepository<Transaction> _transactionRepository;

    public GetTransactionsByCustomerTransactionIdQueryHandler(
        IGenericRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<List<CustomerTransactionDto>> Handle(GetTransactionsByCustomerTransactionIdQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository
            .GetAll()
            .Where(x => x.CustomerTransactionId == request.CustomerTransactionId)
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
        {
            return new List<CustomerTransactionDto>();
        }

        return transactions.Select(x => new CustomerTransactionDto
        {
            Id = x.Id,
            TransactionType = x.TransactionType.ToString(),
            PaymentMethod = x.PaymentMethod.ToString(),
            TransactionStatus = x.TransactionStatus.ToString(),
            TransactionDirection = x.TransactionDirection.ToString(),
            CurrencyCode = x.CurrencyCode,
            Amount = x.Amount,
            PreBalance = x.PreBalance,
            CurrentBalance = x.CurrentBalance,
            Tag = x.Tag,
            TagTitle = x.TagTitle,
            TransactionDate = x.TransactionDate,
            Description = x.Description,
            PaymentType = x.PaymentType,
            ExternalReferenceId = x.ExternalReferenceId,
            RelatedTransactionId = x.RelatedTransactionId,
            WalletId = x.WalletId,
            IncomingTransactionId = x.IncomingTransactionId,
            WithdrawRequestId = x.WithdrawRequestId,
            CardTopupRequestId = x.CardTopupRequestId,
            ReceiverBankCode = x.ReceiverBankCode,
            ReceiverName = x.ReceiverName,
            SenderBankCode = x.SenderBankCode,
            SenderName = x.SenderName,
            SenderAccountNumber = x.SenderAccountNumber,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            CreatedBy = x.CreatedBy,
            LastModifiedBy = x.LastModifiedBy,
            RecordStatus = x.RecordStatus,
            ReturnedTransactionId = x.ReturnedTransactionId,
            ReceiptNumber = x.ReceiptNumber,
            IsReturned = x.IsReturned,
            IpAddress = x.IpAddress,
            Channel = x.Channel,
            IsCancelled = x.IsCancelled,
            CustomerTransactionId = x.CustomerTransactionId,
            PaymentChannel = x.PaymentChannel,
            IsSettlementReceived = x.IsSettlementReceived,
            UsedCreditAmount = x.UsedCreditAmount
        }).ToList();
    }
}
