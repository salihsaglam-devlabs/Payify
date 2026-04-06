using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class CurrenciesFilterRequest
{
    public string Code { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

