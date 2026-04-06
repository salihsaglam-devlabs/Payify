using LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetAllMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners;
using LinkPara.SharedModels.Pagination;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.SaveMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.UpdateMerchantBusinessPartner;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IMerchantBusinessPartnerService
    {
        Task<PaginatedList<MerchantBusinessPartnerDto>> GetAllAsync(GetAllMerchantBusinessPartnerQuery request);
        Task<MerchantBusinessPartnerDto> GetByIdAsync(Guid id);
        Task SaveAsync(SaveMerchantBusinessPartnerCommand request);
        Task UpdateAsync(UpdateMerchantBusinessPartnerCommand request);
    }
}
