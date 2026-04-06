using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceStatistics;

public class GetPostingBalanceStatisticsQuery : IRequest<PostingBalanceStatisticsResponse>
{
    public Guid? MerchantId { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
    public DateTime? PaymentDateStart { get; set; }
    public DateTime? PaymentDateEnd { get; set; }
    public PostingMoneyTransferStatus?[] MoneyTransferStatus { get; set; }
}

public class GetPostingBalanceStatisticsQueryHandler : IRequestHandler<GetPostingBalanceStatisticsQuery, PostingBalanceStatisticsResponse>
{
    private readonly IPostingBalanceService _postingBalanceService;

    public GetPostingBalanceStatisticsQueryHandler(IPostingBalanceService postingBalanceService)
    {
        _postingBalanceService = postingBalanceService;
    }
    public async Task<PostingBalanceStatisticsResponse> Handle(GetPostingBalanceStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _postingBalanceService.GetStatisticsAsync(request);
    }
}