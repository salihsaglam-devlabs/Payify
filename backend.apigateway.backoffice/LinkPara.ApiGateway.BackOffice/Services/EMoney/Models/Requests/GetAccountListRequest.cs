using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetAccountListRequest : SearchQueryParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string IdentityNumber { get; set; }
    public string WalletNumber { get; set; }
    public AccountType? AccountType { get; set; }
    public AccountKycLevel? AccountKycLevel { get; set; }
    public AccountStatus? AccountStatus { get; set; }
    public bool? IsCommercial { get; set; }
}
