using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Queries.GetParameterGroupById
{
    public class GetParameterGroupByIdQuery : IRequest<ParameterGroupDto>
    {
        public Guid Id { get; set; }
    }

    public class GetParameterGroupByIdQueryHandler : IRequestHandler<GetParameterGroupByIdQuery, ParameterGroupDto>
    {
        private readonly IParameterGroupService _parameterGroupService;

        public GetParameterGroupByIdQueryHandler(IParameterGroupService parameterGroupService)
        {
            _parameterGroupService = parameterGroupService;
        }
        public async Task<ParameterGroupDto> Handle(GetParameterGroupByIdQuery request, CancellationToken cancellationToken)
        {
            return await _parameterGroupService.GetByIdAsync(request);
        }
    }
}