using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantPools;
using LinkPara.PF.Application.Features.MerchantPools.Command.ApproveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Command.SaveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Queries.GetFilterMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Queries.GetMerchantPoolById;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantPoolService
{
    Task SaveAsync(SaveMerchantPoolCommand request);
    Task<MerchantPoolDto> GetByIdAsync(GetMerchantPoolByIdQuery request);
    Task<ApproveMerchantPoolResponse> ApproveMerchantPool(ApproveMerchantPoolCommand request);
    Task<PaginatedList<MerchantPoolDto>> GetFilterListAsync(GetFilterMerchantPoolQuery request);
    Task<string> GenerateMerchantNumberAsync();
}
