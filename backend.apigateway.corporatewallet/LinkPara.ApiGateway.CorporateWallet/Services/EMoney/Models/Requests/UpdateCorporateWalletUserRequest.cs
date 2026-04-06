using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class UpdateCorporateWalletUserRequest
{
    public Guid AccountUserId { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public List<Guid> Roles { get; set; }
}
public class UpdateCorporateWalletUserServiceRequest : UpdateCorporateWalletUserRequest
{
    public Guid AccountId { get; set; }
}
