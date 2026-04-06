using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.PostingBalances.Queries.GetAllPostingBalances;

public class GetAllPostingBalanceQuery : SearchQueryParams, IRequest<PostingBalanceResponse>
{
    public Guid? MerchantId { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
    public DateTime? PaymentDateStart { get; set; }
    public DateTime? PaymentDateEnd { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public PostingMoneyTransferStatus?[] MoneyTransferStatus { get; set; }
    public PostingBalanceType?[] PostingBalanceType { get; set; }
    public PostingPaymentChannel? PostingPaymentChannel { get; set; }
}

public class GetAllPostingBalanceQueryHandler : IRequestHandler<GetAllPostingBalanceQuery, PostingBalanceResponse>
{
    private readonly IPostingBalanceService _postingBalanceService;

    public GetAllPostingBalanceQueryHandler(IPostingBalanceService postingBalanceService)
    {
        _postingBalanceService = postingBalanceService;
    }
    public async Task<PostingBalanceResponse> Handle(GetAllPostingBalanceQuery request, CancellationToken cancellationToken)
    {
        return await _postingBalanceService.GetAllAsync(request);
    }
}
