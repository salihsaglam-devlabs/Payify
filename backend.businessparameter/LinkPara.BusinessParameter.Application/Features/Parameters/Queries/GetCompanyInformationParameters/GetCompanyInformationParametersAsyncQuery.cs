using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.BusinessParameter.Application.Features.Parameters.Queries.GetCompanyInformationParameters
{
    public class GetCompanyInformationParametersAsyncQuery : IRequest<List<ParameterDto>>
    {
    }
    public class GetCompanyInformationParametersAsyncQueryHandler : IRequestHandler<GetCompanyInformationParametersAsyncQuery, List<ParameterDto>>
    {
        private readonly IParameterService _parameterService;
        private const string CompanyInformation = "PublicCompanyInformation";

        public GetCompanyInformationParametersAsyncQueryHandler(IParameterService parameterService)
        {
            _parameterService = parameterService;
        }

        public async Task<List<ParameterDto>> Handle(GetCompanyInformationParametersAsyncQuery request, CancellationToken cancellationToken)
        {
            return await _parameterService.GetParametersAsync(CompanyInformation);
        }
    }
}