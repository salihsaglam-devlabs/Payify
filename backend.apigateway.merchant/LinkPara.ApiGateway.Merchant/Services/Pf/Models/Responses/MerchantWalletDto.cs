using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantWalletDto
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
}