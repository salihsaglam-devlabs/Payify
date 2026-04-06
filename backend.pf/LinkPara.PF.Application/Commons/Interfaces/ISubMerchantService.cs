using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchants.Command.ApproveSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.DeleteSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.SaveSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateMultipleSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Queries.GetAllSubMerchant;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ISubMerchantService
{
    Task<PaginatedList<SubMerchantDto>> GetListAsync(GetAllSubMerchantQuery request);
    Task<SubMerchantDto> GetByIdAsync(Guid id);
    Task<Guid> SaveAsync(SaveSubMerchantCommand request);
    Task DeleteAsync(DeleteSubMerchantCommand request);
    Task UpdateAsync(UpdateSubMerchantCommand request);
    Task UpdateMultipleAsync(UpdateMultipleSubMerchantCommand request);
    Task ApproveAsync(ApproveSubMerchantCommand request);
}
