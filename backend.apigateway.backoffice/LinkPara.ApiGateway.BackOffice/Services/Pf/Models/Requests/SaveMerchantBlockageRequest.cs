using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests; 
public class SaveMerchantBlockageRequest
{
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public MerchantBlockageStatus MerchantBlockageStatus { get; set; }

    public Guid MerchantId { get; set; }
}
