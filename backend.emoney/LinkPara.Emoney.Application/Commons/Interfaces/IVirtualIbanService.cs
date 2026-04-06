using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IVirtualIbanService
{
    Task AssignToAccountAsync(Guid accountId);
    Task CheckAvailableCount();
    Task<bool> CheckKycLevelToIbanAssignmentAsync(AccountKycLevel kycLevel);
    Task AssignToAccountsAsync();
}
