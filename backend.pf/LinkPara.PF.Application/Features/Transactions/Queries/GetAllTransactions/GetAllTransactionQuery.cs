using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.Transactions.Queries.GetAllTransactions;

public class GetAllTransactionQuery : SearchQueryParams, IRequest<PaginatedList<TransactionDto>>
{
    public Guid? PostingBalanceId { get; set; }
    public Guid? MerchantId { get; set; }
    public DateTime? PostingDate { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
    public TransactionType?[] TransactionType { get; set; }
}

public class GetAllTransactionQueryHandler : IRequestHandler<GetAllTransactionQuery, PaginatedList<TransactionDto>>
{
    private readonly IGenericRepository<PostingTransaction> _transactionRepository;
    private readonly IGenericRepository<PostingAdditionalTransaction> _additionalTransactionRepository;

    public GetAllTransactionQueryHandler(IGenericRepository<PostingTransaction> transactionRepository,
        IGenericRepository<PostingAdditionalTransaction> additionalTransactionsRepository)
    {
        _transactionRepository = transactionRepository;
        _additionalTransactionRepository = additionalTransactionsRepository;
    }
    public async Task<PaginatedList<TransactionDto>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
    {
        var transactions = _transactionRepository.GetAll();
        var additionalTransactions = _additionalTransactionRepository.GetAll();
        
        var combinedTransactions = transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                MerchantId = t.MerchantId,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate,
                PostingDate = t.PostingDate,
                PaymentDate = t.PaymentDate,
                OldPaymentDate = t.OldPaymentDate,
                CardNumber = t.CardNumber,
                OrderId = t.OrderId,
                InstallmentCount = t.InstallmentCount,
                Currency = t.Currency,
                Amount = t.Amount,
                PointAmount = t.PointAmount,
                BankCommissionRate = t.BankCommissionRate,
                BankCommissionAmount = t.BankCommissionAmount,
                AmountWithoutBankCommission = t.AmountWithoutBankCommission,
                PfCommissionRate = t.PfCommissionRate,
                PfPerTransactionFee = t.PfPerTransactionFee,
                ParentMerchantCommissionAmount = t.ParentMerchantCommissionAmount,
                ParentMerchantCommissionRate = t.ParentMerchantCommissionRate,
                AmountWithoutParentMerchantCommission = t.AmountWithoutParentMerchantCommission,
                PfCommissionAmount = t.PfCommissionAmount,
                PfNetCommissionAmount = t.PfNetCommissionAmount,
                AmountWithoutCommissions = t.AmountWithoutCommissions,
                BlockageStatus = t.BlockageStatus,
                PostingBankBalanceId = t.PostingBankBalanceId,
                PostingBalanceId = t.PostingBalanceId,
                TransactionStartDate = t.TransactionStartDate,
                ConversationId = t.ConversationId,
                MerchantDeductionId = t.MerchantDeductionId,
                RelatedPostingBalanceId = t.RelatedPostingBalanceId,
                SubMerchantId = t.SubMerchantId,
                SubMerchantName = t.SubMerchantName,
                SubMerchantNumber = t.SubMerchantNumber,
                EasySubMerchantName = t.EasySubMerchantName,
                EasySubMerchantNumber = t.EasySubMerchantNumber,
                PfTransactionSource = t.PfTransactionSource,
                MerchantPhysicalPosId = t.MerchantPhysicalPosId,
                InstallmentSequence = t.InstallmentSequence,
                IsPerInstallment = t.IsPerInstallment,
                MerchantInstallmentTransactionId = t.MerchantInstallmentTransactionId
            })
            .Concat(additionalTransactions.Select(at => new TransactionDto
            {
                Id = at.Id,
                MerchantId = at.MerchantId,
                TransactionType = at.TransactionType,
                TransactionDate = at.TransactionDate,
                PostingDate = at.PostingDate,
                PaymentDate = at.PaymentDate,
                OldPaymentDate = at.OldPaymentDate,
                CardNumber = at.CardNumber,
                OrderId = at.OrderId,
                InstallmentCount = at.InstallmentCount,
                Currency = at.Currency,
                Amount = at.Amount,
                PointAmount = at.PointAmount,
                BankCommissionRate = at.BankCommissionRate,
                BankCommissionAmount = at.BankCommissionAmount,
                AmountWithoutBankCommission = at.AmountWithoutBankCommission,
                PfCommissionRate = at.PfCommissionRate,
                PfPerTransactionFee = at.PfPerTransactionFee,
                ParentMerchantCommissionAmount = at.ParentMerchantCommissionAmount,
                ParentMerchantCommissionRate = at.ParentMerchantCommissionRate,
                AmountWithoutParentMerchantCommission = at.AmountWithoutParentMerchantCommission,
                PfCommissionAmount = at.PfCommissionAmount,
                PfNetCommissionAmount = at.PfNetCommissionAmount,
                AmountWithoutCommissions = at.AmountWithoutCommissions,
                BlockageStatus = at.BlockageStatus,
                PostingBankBalanceId = at.PostingBankBalanceId,
                PostingBalanceId = at.PostingBalanceId,
                TransactionStartDate = at.TransactionStartDate,
                ConversationId = at.ConversationId,
                MerchantDeductionId = at.MerchantDeductionId,
                RelatedPostingBalanceId = at.RelatedPostingBalanceId,
                SubMerchantId = at.SubMerchantId,
                SubMerchantName = at.SubMerchantName,
                SubMerchantNumber = at.SubMerchantNumber,
                EasySubMerchantName = at.EasySubMerchantName,
                EasySubMerchantNumber = at.EasySubMerchantNumber,
                PfTransactionSource = at.PfTransactionSource,
                MerchantPhysicalPosId = at.MerchantPhysicalPosId,
                InstallmentSequence = at.InstallmentSequence,
                IsPerInstallment = at.IsPerInstallment,
                MerchantInstallmentTransactionId = at.MerchantInstallmentTransactionId
            }));

        if (request.PostingBalanceId is not null)
        {
            combinedTransactions = combinedTransactions.Where(b => b.PostingBalanceId == request.PostingBalanceId);
        }

        if (request.MerchantId is not null)
        {
            combinedTransactions = combinedTransactions.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.BlockageStatus is not null)
        {
            combinedTransactions = combinedTransactions.Where(b => b.BlockageStatus == request.BlockageStatus);
        }

        if (request.PostingDate is not null)
        {
            combinedTransactions = combinedTransactions.Where(b => b.PostingDate == request.PostingDate);
        }
        
        if (request.TransactionType is not null)
        {
            combinedTransactions = combinedTransactions.Where(b => request.TransactionType.Contains(b.TransactionType));
        }

        return await combinedTransactions
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
