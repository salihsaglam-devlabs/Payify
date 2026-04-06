using LinkPara.PF.Application.Features.MerchantBlockages;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.SaveMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdatePaymentDate;
using LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetAllMerchantBlockages;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantBlockageService
{
    Task<PaginatedList<MerchantBlockageDto>> GetAllAsync(GetAllMerchantBlockageQuery query);
    Task<MerchantBlockageDto> GetByMerchantIdAsync(Guid merchantId);
    Task SaveAsync(SaveMerchantBlockageCommand request);
    Task UpdateAsync(UpdateMerchantBlockageCommand request);
    Task UpdatePaymentDateAsync(UpdatePaymentDateCommand request);
}