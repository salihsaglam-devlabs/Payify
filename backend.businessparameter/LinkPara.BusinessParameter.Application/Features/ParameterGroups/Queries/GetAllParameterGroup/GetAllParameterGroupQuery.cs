using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetAllParameterGroup
{
    public class GetAllParameterGroupQuery : SearchQueryParams, IRequest<PaginatedList<ParameterGroupDto>>
    {
        public string GroupCode { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }

    public class GetAllParameterGroupQueryHandler : IRequestHandler<GetAllParameterGroupQuery, PaginatedList<ParameterGroupDto>>
    {
        private readonly IParameterGroupService _parameterGroupService;

        public GetAllParameterGroupQueryHandler(IParameterGroupService parameterGroupService)
        {
            _parameterGroupService = parameterGroupService;
        }
        public async Task<PaginatedList<ParameterGroupDto>> Handle(GetAllParameterGroupQuery request, CancellationToken cancellationToken)
        {
            return await _parameterGroupService.GetAllParameterGroupAsync(request);
        }
    }
}
