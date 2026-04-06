using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantDeductions.Queries.GetFilterMerchantDeductionQuery
{
    public class GetFilterMerchantDeductionQuery : SearchQueryParams, IRequest<PaginatedList<MerchantDeductionDto>>
    {
        public Guid MerchantTransactionId { get; set; }
        public Guid MerchantId { get; set; }
        public int? Currency { get; set; }
        public decimal? TotalDeductionAmountBiggerThan { get; set; }
        public decimal? TotalDeductionAmountSmallerThan { get; set; }
        public decimal? RemainingDeductionAmountBiggerThan { get; set; }
        public decimal? RemainingDeductionAmountSmallerThan { get; set; }
        public DateTime? ExecutionDateStart { get; set; }
        public DateTime? ExecutionDateEnd { get; set; }
        public DeductionType? DeductionType { get; set; }
        public DeductionStatus? DeductionStatus { get; set; }
        public Guid MerchantDueId { get; set; }
        public RecordStatus? RecordStatus { get; set; }
        public string ConversationId { get; set; }
    }

    public class GetFilterMerchantDeductionQueryHandler : IRequestHandler<GetFilterMerchantDeductionQuery, PaginatedList<MerchantDeductionDto>>
    {
        private readonly IMerchantDeductionService _merchantDeductionService;
        
        public GetFilterMerchantDeductionQueryHandler(IMerchantDeductionService merchantDeductionService)
        {
            _merchantDeductionService = merchantDeductionService;
        }

        public async Task<PaginatedList<MerchantDeductionDto>> Handle(GetFilterMerchantDeductionQuery request, CancellationToken cancellationToken)
        {
            return await _merchantDeductionService.GetFilterMerchantDeductionsAsync(request);
        }
    }
}
