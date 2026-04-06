using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class CustomerAccountInfoResponse
{
    public CallCenterAccountDto AccountInformation { get; set; }
    public List<WalletDto> Wallets { get; set; }
    public List<AccountKycChangeDto> KycChanges { get; set; }
    public PaginatedList<TransactionAdminDto> AccountTransactions { get; set; }
    public AccountLimitDto AccountLimits { get; set; }
    public string ErrorMessage { get; set; }
}
