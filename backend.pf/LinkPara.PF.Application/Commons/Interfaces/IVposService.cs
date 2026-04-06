using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Application.Features.VirtualPos.Command.DeleteVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;
using LinkPara.PF.Application.Features.VirtualPos.Queries.GetFilterVpos;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IVposService
{
    Task<PaginatedList<VposDto>> GetFilterListAsync(GetFilterVposQuery request);
    Task<VposDto> GetByIdAsync(Guid id);
    Task DeleteAsync(DeleteVposCommand command);
    Task SaveAsync(SaveVposCommand command);
    Task UpdateAsync(UpdateVposCommand command);
}
