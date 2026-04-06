using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantReturnPools.Queries.GetMerchantReturnPools
{
    public class GetMerchantReturnPoolsQuery :SearchQueryParams, IRequest<PaginatedList<MerchantReturnPoolDto>>
    {
        public string ConversationId { get; set; }
        public Guid? MerchantId { get; set; }
        public DateTime? ActionDate { get; set; }
        public Guid? ActionUser { get; set; }
        public ReturnStatus? ReturnStatus { get; set; }
        public string OrderId { get; set; }
        public DateTime? CreateDateStart { get; set; }
        public DateTime? CreateDateEnd { get; set; }
        public string FirstCardNumber { get; set; }
        public string LastCardNumber { get; set; }
        public int? BankCode { get; set; }
        public string BankName { get; set; }
        public bool? BankStatus { get; set; }
    }

    public class GetMerchantReturnPoolsQueryHandler : IRequestHandler<GetMerchantReturnPoolsQuery, PaginatedList<MerchantReturnPoolDto>>
    {
        private readonly IMerchantReturnPoolService _merchantReturnPoolService;
        public GetMerchantReturnPoolsQueryHandler(IMerchantReturnPoolService merchantReturnPoolService)
        {
            _merchantReturnPoolService = merchantReturnPoolService;
        }
        public Task<PaginatedList<MerchantReturnPoolDto>> Handle(GetMerchantReturnPoolsQuery request, CancellationToken cancellationToken)
        {
            return _merchantReturnPoolService.GetPaginatedPendingPoolAsync(request);
        }
    }
}