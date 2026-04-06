using LinkPara.PF.Application.Features.MerchantCategoryCodes;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.DeleteMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.SaveMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.UpdateMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetAllMcc;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMccService
{
    Task<PaginatedList<MccDto>> GetListAsync(GetAllMccQuery request);
    Task<MccDto> GetByIdAsync(Guid id);
    Task<MccDto> GetByCodeAsync(string mccCode);
    Task DeleteAsync(DeleteMccCommand command);
    Task SaveAsync(SaveMccCommand request);
    Task UpdateAsync(UpdateMccCommand request);
}
