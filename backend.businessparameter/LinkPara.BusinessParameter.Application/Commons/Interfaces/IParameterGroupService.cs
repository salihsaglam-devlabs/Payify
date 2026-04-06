using LinkPara.BusinessParameter.Application.Features.ParameterGroups;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.DeleteParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.SaveParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetAllParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetParameterGroupById;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.BusinessParameter.Application.Commons.Interfaces;

public interface IParameterGroupService
{
    Task SaveAsync(SaveParameterGroupCommand request);
    Task<PaginatedList<ParameterGroupDto>> GetAllParameterGroupAsync(GetAllParameterGroupQuery request);
    Task<ParameterGroupDto> GetByIdAsync(GetParameterGroupByIdQuery request);
    Task DeleteAsync(DeleteParameterGroupCommand command);
    Task<ParameterGroupDto> UpdateAsync(UpdateParameterGroupCommand command);
}
