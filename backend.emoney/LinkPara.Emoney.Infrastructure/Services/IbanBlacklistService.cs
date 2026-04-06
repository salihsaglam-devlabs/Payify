using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.HttpProviders.BusinessParameter;

namespace LinkPara.Emoney.Infrastructure.Services;

public class IbanBlacklistService : IIbanBlacklistService
{
    private readonly string _groupCode = "BlacklistedIbans";

    private readonly IParameterService _parameterService;

    public IbanBlacklistService(IParameterService parameterService)
    {
        _parameterService = parameterService;
    }

    public async Task<bool> IsBlacklistedAsync(string iban)
    {
        try
        {
            if (string.IsNullOrEmpty(iban))
            {
                return false;
            }

            var parameters = await _parameterService.GetParametersAsync(_groupCode);

            if (parameters.Count == 0)
            {
                return false;
            }

            return parameters.Any(s => s.ParameterCode == iban);
        }
        catch
        {
            return false;
        }
    }
}
