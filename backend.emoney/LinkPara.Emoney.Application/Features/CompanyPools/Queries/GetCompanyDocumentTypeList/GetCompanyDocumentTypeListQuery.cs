using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyDocumentTypeList;

public class GetCompanyDocumentTypeListQuery : IRequest<List<CompanyDocumentTypeDto>>
{
    public CompanyType CompanyType { get; set; }
}

public class GetCompanyDocumentTypeListQueryHandler : IRequestHandler<GetCompanyDocumentTypeListQuery, List<CompanyDocumentTypeDto>>
{
    private readonly IParameterService _parameterService;

    public GetCompanyDocumentTypeListQueryHandler(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }

    public async Task<List<CompanyDocumentTypeDto>> Handle(GetCompanyDocumentTypeListQuery request, CancellationToken cancellationToken)
    {
        var parameters = await _parameterService.GetParametersAsync("CorporateWalletDocumentType");

        var result = new List<CompanyDocumentTypeDto>();
        foreach (var parameter in parameters)
        {
            var templates = await _parameterService.GetAllParameterTemplateValuesAsync(parameter.GroupCode, parameter.ParameterCode);

            var companyDocumentType = new CompanyDocumentTypeDto
            {
                Name = parameter.ParameterCode,
                DocumentTypeId = Guid.Parse(parameter.ParameterValue)
            };

            if (request.CompanyType == CompanyType.Individual)
            {
                if (templates.Any(x => x.TemplateCode == "IsIndividualRequired"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "IsIndividualRequired");

                    if (bool.TryParse(template.TemplateValue, out bool isRequired) && isRequired)
                    {
                        companyDocumentType.IsRequired = true;
                    }
                }

                if (templates.Any(x => x.TemplateCode == "HasIndividualDocument"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "HasIndividualDocument");

                    if (bool.TryParse(template.TemplateValue, out bool hasIndividualDocument) && hasIndividualDocument)
                    {
                        result.Add(companyDocumentType);
                    }
                }
            }

            if (request.CompanyType == CompanyType.Corporate)
            {
                if (templates.Any(x => x.TemplateCode == "IsCorporateRequired"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "IsCorporateRequired");

                    if (bool.TryParse(template.TemplateValue, out bool isRequired) && isRequired)
                    {
                        companyDocumentType.IsRequired = true;
                    }
                }

                if (templates.Any(x => x.TemplateCode == "HasCorporateDocument"))
                {
                    var template = templates.FirstOrDefault(x => x.TemplateCode == "HasCorporateDocument");

                    if (bool.TryParse(template.TemplateValue, out bool hasCorporateDocument) && hasCorporateDocument)
                    {
                        result.Add(companyDocumentType);
                    }
                }
            }
        }

        return result;
    }
}
