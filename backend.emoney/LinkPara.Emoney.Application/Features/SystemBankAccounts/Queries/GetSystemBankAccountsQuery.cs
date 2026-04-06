using MediatR;
using LinkPara.HttpProviders.BusinessParameter;
using Microsoft.Extensions.Logging;
using LinkPara.Emoney.Application.Features.Banks;

namespace LinkPara.Emoney.Application.Features.SystemBankAccounts.Queries;

public class GetSystemBankAccountsQuery : IRequest<List<SystemBankAccountDto>>
{
}

public class GetSystemBankAccountsQueryHandler : IRequestHandler<GetSystemBankAccountsQuery, List<SystemBankAccountDto>>
{
    private readonly IParameterService _parameterService;
    private readonly ILogger<GetSystemBankAccountsQueryHandler> _logger;

    public GetSystemBankAccountsQueryHandler(IParameterService parameterService,
        ILogger<GetSystemBankAccountsQueryHandler> logger)
    {
        _parameterService = parameterService;
        _logger = logger;
    }

    public async Task<List<SystemBankAccountDto>> Handle(GetSystemBankAccountsQuery query, CancellationToken cancellationToken)
    {
        return await GetSystemBankAccountsAsync();
    }

    private async Task<List<SystemBankAccountDto>> GetSystemBankAccountsAsync()
    {
        var parameterTemplateValueList = await _parameterService
            .GetAllParameterTemplateValuesAsync("SystemBankAccounts", null);

        var systemBankAccountParameterCodes = parameterTemplateValueList.Select(p => p.ParameterCode).Distinct();

        var systemBankAccountList = new List<SystemBankAccountDto>();

        foreach (var parameterCode in systemBankAccountParameterCodes)
        {
            try
            {
                var systemBankAccountValues = parameterTemplateValueList
                    .Where(f => f.ParameterCode == parameterCode).ToList();

                if (string.IsNullOrWhiteSpace(systemBankAccountValues
                    .FirstOrDefault(x => x.TemplateCode == "Iban")?.TemplateValue) || string.IsNullOrWhiteSpace(systemBankAccountValues
                    .FirstOrDefault(x => x.TemplateCode == "Name")?.TemplateValue))
                {
                    _logger.LogError($"System Bank Account; Business Parameter System Bank Account " +
                                     $"Data should be checked! Parameter: {parameterCode}", parameterCode);
                    continue;
                }

                var bankCode = systemBankAccountValues.FirstOrDefault(x => x.TemplateCode == "BankCode")?.TemplateValue;

                var systemBankAccountDto = new SystemBankAccountDto
                {
                    Iban = systemBankAccountValues.FirstOrDefault(x => x.TemplateCode == "Iban")?.TemplateValue,
                    Name = systemBankAccountValues.FirstOrDefault(x => x.TemplateCode == "Name")?.TemplateValue,
                    Bank = new BankDto
                    {
                        Code = !string.IsNullOrEmpty(bankCode) ? Convert.ToInt32(bankCode) : 0,
                        Name = systemBankAccountValues.FirstOrDefault(x => x.TemplateCode == "BankName")?.TemplateValue,
                        LogoUrl = systemBankAccountValues.FirstOrDefault(x => x.TemplateCode == "BankLogoUrl")?.TemplateValue
                    }              
                };

                systemBankAccountList.Add(systemBankAccountDto);
            }
            catch (Exception exception)
            {
                _logger.LogError($"System Bank Account; Business Parameter System Bank Account " +
                                 $"Data should be checked! System Bank Account : {parameterCode}, Error : {exception}", parameterCode);
            }
        }

        return systemBankAccountList;
    }

}