using LinkPara.BusinessParameter.Application.Features.Parameters;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.DeleteParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.SaveParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.UpdateParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetAllParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetParameterById;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.BusinessParameter.Application.Commons.Interfaces;

public interface IParameterService
{
    Task<ParameterDto> GetParameterAsync(string groupCode, string parameterCode);
    Task<List<ParameterDto>> GetParametersAsync(string groupCode);
    Task SaveAsync(SaveParameterCommand request);
    Task<PaginatedList<ParameterDto>> GetAllParameterAsync(GetAllParameterQuery request);
    Task<ParameterDto> GetByIdAsync(GetParameterByIdQuery request);
    Task DeleteAsync(DeleteParameterCommand command);
    Task<ParameterDto> UpdateAsync(UpdateParameterCommand command);
}
