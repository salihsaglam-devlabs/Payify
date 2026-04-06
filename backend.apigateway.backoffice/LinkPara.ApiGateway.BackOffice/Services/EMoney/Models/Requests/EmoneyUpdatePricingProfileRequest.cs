using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class EmoneyUpdatePricingProfileRequest
{
    public Guid Id { get; set; }
    public DateTime ActivationDateStart { get; set; }
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TransferType TransferType { get; set; }
    public List<PricingProfileItemUpdateModel> ProfileItems { get; set; }
}

public class PricingProfileItemUpdateModel : PricingProfileItemModel
{
    public Guid Id { get; set; }
}
