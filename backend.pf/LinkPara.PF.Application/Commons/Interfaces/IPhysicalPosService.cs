using LinkPara.PF.Application.Features.PhysicalPoses;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.DeletePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.SavePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Command.UpdatePhysicalPos;
using LinkPara.PF.Application.Features.PhysicalPoses.Queries.GetAllPhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPhysicalPosService
{
    Task<PaginatedList<PhysicalPosDto>> GetAllAsync(GetAllPhysicalPosQuery request);
    Task<PhysicalPosDto> GetByIdAsync(Guid id);
    Task DeleteAsync(DeletePhysicalPosCommand command);
    Task SaveAsync(SavePhysicalPosCommand command);
    Task UpdateAsync(UpdatePhysicalPosCommand command);
}
