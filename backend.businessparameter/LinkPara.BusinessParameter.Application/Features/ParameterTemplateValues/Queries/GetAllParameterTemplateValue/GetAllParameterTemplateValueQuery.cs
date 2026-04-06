using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.ParameterTemplateValues.Queries.GetAllParameterTemplateValue;
    public class GetAllParameterTemplateValueQuery : SearchQueryParams, IRequest<PaginatedList<ParameterTemplateValueDto>>
{
    }

    public class GetAllParameterTemplateValueQueryHandler : IRequestHandler<GetAllParameterTemplateValueQuery, PaginatedList<ParameterTemplateValueDto>>
    {
        private readonly IParameterTemplateValueService _parameterTemplateValue;

        public GetAllParameterTemplateValueQueryHandler(IParameterTemplateValueService parameterTemplateValue)
        {
            _parameterTemplateValue = parameterTemplateValue;
        }
        public async Task<PaginatedList<ParameterTemplateValueDto>> Handle(GetAllParameterTemplateValueQuery request, CancellationToken cancellationToken)
        {
            return await _parameterTemplateValue.GetAllParameterTemplateValuesAsync(request);
        }
    }
