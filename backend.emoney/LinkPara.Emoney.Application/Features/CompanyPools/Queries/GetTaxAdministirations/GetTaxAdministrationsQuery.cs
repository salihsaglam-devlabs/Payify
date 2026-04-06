using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.Exceptions;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetTaxAdministirations;

public class GetTaxAdministrationsQuery : IRequest<List<TaxAdministrationsResponse>>
{
}

public class GetTaxAdministrationsQueryHandler : IRequestHandler<GetTaxAdministrationsQuery, List<TaxAdministrationsResponse>>
{
    private readonly IParameterService _parameterService;

    public GetTaxAdministrationsQueryHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }

    public async Task<List<TaxAdministrationsResponse>> Handle(GetTaxAdministrationsQuery request, CancellationToken cancellationToken)
    {
        var taxAdministrations = await _parameterService.GetParametersAsync("TaxAdministrations");

        if (taxAdministrations is null)
        {
            throw new NotFoundException("TaxAdministrations");
        }
        var result = new List<TaxAdministrationsResponse>();
        taxAdministrations.ForEach(taxAdministration =>
        {
            result.Add(new TaxAdministrationsResponse { Code = taxAdministration.ParameterCode, Name = taxAdministration.ParameterValue });
        });
        return result;
    }
}
