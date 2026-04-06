using LinkPara.Emoney.Domain.Enums;
using MediatR;
using LinkPara.HttpProviders.BusinessParameter;
using Microsoft.Extensions.Localization;
using LinkPara.HttpProviders.BusinessParameter.Models;
using LinkPara.SharedModels.Exceptions;
using Microsoft.Extensions.Logging;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.Currencies.Queries.Currencies;

public class CurrenciesFilterQuery : IRequest<List<CurrencyDto>>
{
    public string Code { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class CurrenciesFilterQueryHandler : IRequestHandler<CurrenciesFilterQuery, List<CurrencyDto>>
{
    private readonly IStringLocalizer _localizer;
    private readonly IParameterService _parameterService;
    private readonly ILogger<CurrenciesFilterQueryHandler> _logger;

    public CurrenciesFilterQueryHandler(IStringLocalizerFactory factory,
        IParameterService parameterService, ILogger<CurrenciesFilterQueryHandler> logger)
    {
        _localizer = factory.Create("Currencies", "LinkPara.Emoney.API");
        _parameterService = parameterService;
        _logger = logger;
    }

    public async Task<List<CurrencyDto>> Handle(CurrenciesFilterQuery request, CancellationToken cancellationToken)
    {
        return !string.IsNullOrWhiteSpace(request.Code)
            ? await GetSingleCurrencyAsync(request)
            : await GetCurrenciesAsync(request.RecordStatus);
    }

    private async Task<List<CurrencyDto>> GetSingleCurrencyAsync(CurrenciesFilterQuery request)
    {
        var parameterTemplateValueList = await _parameterService
            .GetAllParameterTemplateValuesAsync("Currencies", request.Code);

        var code =
            parameterTemplateValueList.FirstOrDefault(x => x.TemplateCode == "Code");

        var symbol = parameterTemplateValueList.FirstOrDefault
            (x => x.TemplateCode == "Symbol")?.TemplateValue.ToString();

        var priority =
            Convert.ToInt32(parameterTemplateValueList.FirstOrDefault
            (x => x.TemplateCode == "Priority")?.TemplateValue);


        if (code == null)
        {
            throw new NotFoundException(nameof(ParameterTemplateValueDto), request.Code);
        }

        return new List<CurrencyDto>
        {
            new()
            {
                CurrencyType = CurrencyType.Money,
                Code = code.TemplateValue,
                Name = _localizer[$"{code.ParameterCode}"].Value,
                Symbol = symbol,
                Priority = priority
            }
        };
    }

    private async Task<List<CurrencyDto>> GetCurrenciesAsync(RecordStatus? recordStatus)
    {
        var parameterTemplateValueList = await _parameterService
            .GetAllParameterTemplateValuesAsync("Currencies", null);

        if(recordStatus is not null)
        {
            parameterTemplateValueList = parameterTemplateValueList.Where(x => x.RecordStatus == recordStatus).ToList();
        }     

        var currencyCodes = parameterTemplateValueList.Select(p => p.ParameterCode).Distinct();

        var currencyList = new List<CurrencyDto>();
        foreach (var currencyCode in currencyCodes)
        {
            try
            {
                var currencyValues = parameterTemplateValueList
                    .Where(f => f.ParameterCode == currencyCode).ToList();

                var code = currencyValues.FirstOrDefault(x => x.TemplateCode == "Code")?.TemplateValue;
                var name = currencyValues.FirstOrDefault(x => x.TemplateCode == "Name")?.ParameterCode;
                var symbol = currencyValues.FirstOrDefault(x => x.TemplateCode == "Symbol")?.TemplateValue;
                var priority = currencyValues.FirstOrDefault(x => x.TemplateCode == "Priority")?.TemplateValue;

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogError($"Currency Business Parameter Currencies " +
                                     $"Data should be checked! Parameter: {currencyCode}", currencyCode);
                    continue;
                }

                var currencyDto = new CurrencyDto
                {
                    CurrencyType = CurrencyType.Money,
                    Code = code,
                    Name = _localizer[$"{name}"].Value,
                    Priority = Convert.ToInt32(priority),
                    Symbol = symbol
                };

                currencyList.Add(currencyDto);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Currency Business Parameter Currencies " +
                                 $"Data should be checked! Currency : {currencyCode}, Error : {exception}");
            }
        }

        var orderByList = currencyList.Where(x => x.Priority != 0).OrderBy(x => x.Priority).ToList();
        orderByList.AddRange(currencyList.Where(x => x.Priority == 0).ToList());

        return orderByList;
    }
}