using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PricingProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileType ProfileType { get; set; }
    public string CurrencyCode { get; set; }
    public decimal PerTransactionFee { get; set; }
    public string PricingProfileNumber { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
}
