using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Transactions;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Features.CallCenterCustomerAccount;

public class CustomerAccountInfoResponse
{
    public AccountDto AccountInformation { get; set; }
    public List<WalletDto> Wallets { get; set; }
    public List<AccountKycChangeDto> KycChanges { get; set; }
    public PaginatedList<TransactionAdminDto> AccountTransactions { get; set; }
    public AccountLimitDto AccountLimits { get; set; }
    public string ErrorMessage { get; set; }
}
