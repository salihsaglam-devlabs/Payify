using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdatePricingProfileRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PerTransactionFee { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}
