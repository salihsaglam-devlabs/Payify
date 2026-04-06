using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantResponse
{
    public Guid MerchantId { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
}
