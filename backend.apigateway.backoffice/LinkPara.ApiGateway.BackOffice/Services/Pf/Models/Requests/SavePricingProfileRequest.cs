using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SavePricingProfileRequest
{
    public string Name { get; set; }
    public ProfileType ProfileType { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PerTransactionFee { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
}
