using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class GetCorporateWalletUsersRequest : SearchQueryParams
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetCorporateWalletUsersServiceRequest : GetCorporateWalletUsersRequest
{
    public Guid AccountId { get; set; }
}