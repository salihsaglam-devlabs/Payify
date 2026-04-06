using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.SaveMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Commands.UpdateMerchantPreApplication;
using LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetAllMerchantPreApplication;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantPreApplicationService
{
    Task<MerchantPreApplicationCreateResponse> SaveAsync(SaveMerchantPreApplicationCommand request);
    Task<MerchantPreApplicationDto> GetPosApplicationByIdAsync(Guid id);
    Task<PaginatedList<MerchantPreApplicationDto>> GetFilterAsync(GetAllMerchantPreApplicationQuery request);
    Task UpdatePosApplicationAsync(UpdateMerchantPreApplicationCommand request);
    Task DeleteAsync(Guid id);
}