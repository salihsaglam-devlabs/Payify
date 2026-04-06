using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Application.Features.MerchantDeductions.Queries.GetFilterMerchantDeductionQuery;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantDeductionService
{
    Task<PaginatedList<MerchantDeductionDto>> GetFilterMerchantDeductionsAsync(GetFilterMerchantDeductionQuery request);
}