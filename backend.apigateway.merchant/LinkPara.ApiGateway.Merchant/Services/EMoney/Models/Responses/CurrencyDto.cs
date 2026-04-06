using LinkPara.ApiGateway.Merchant.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Emoney.Models.Responses;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public CurrencyType CurrencyType { get; set; }
}
