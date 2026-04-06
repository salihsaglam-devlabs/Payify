using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class SubMerchantUserModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
