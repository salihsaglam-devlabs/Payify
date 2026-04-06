using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.Currencies;

public class CurrencyDto : IMapFrom<Currency>
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public int Priority { get; set; }
    public CurrencyType CurrencyType { get; set; }
}
