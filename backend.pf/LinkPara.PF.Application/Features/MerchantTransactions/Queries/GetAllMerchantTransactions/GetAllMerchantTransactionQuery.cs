using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantTransactions;

public class GetAllMerchantTransactionQuery : SearchQueryParams, IRequest<PaginatedList<MerchantTransactionDto>>
{
    public Guid? MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public int? AcquireBankCode { get; set; }
    public int? IssuerBankCode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public TransactionType? TransactionType { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }
    public PfTransactionSource? PfTransactionSource { get; set; }
    public string ConversationId { get; set; }
    public string BankOrderId { get; set; }
    public string CardFirstNumbers { get; set; }
    public string CardLastNumbers { get; set; }
    public bool? IsChargeBack { get; set; }
    public bool? IsSuspecious { get; set; }
    public bool? IsManualReturn { get; set; }
    public bool? IsOnUsPayment { get; set; }
    public bool? IsInsurancePayment { get; set; }
    public bool? IsPerInstallment { get; set; }
    public string CreatedNameBy { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
}

public class GetAllMerchantTransactionQueryHandler : IRequestHandler<GetAllMerchantTransactionQuery, PaginatedList<MerchantTransactionDto>>
{
    private readonly IMerchantService _merchantService;

    public GetAllMerchantTransactionQueryHandler(IMerchantService merchantService)
    {

        _merchantService = merchantService;
    }

    public async Task<PaginatedList<MerchantTransactionDto>> Handle(GetAllMerchantTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _merchantService.GetMerchantTransactionList(request);
    }
}
