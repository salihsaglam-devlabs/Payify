using LinkPara.Approval;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class PfApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PfApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "AcquireBanks":
            {
                return (IApprovalScreenService)
                    _serviceProvider.GetService(typeof(AcquireBankScreenService));
            }
            case "CardBins":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(CardBinScreenService));
                }
            case "MerchantCategoryCodes":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantCategoryCodeScreenService));
                }
            case "Vpos":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(VPosScreenService));
                }
            case "PricingProfiles":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(PricingProfileScreenService));
                }
            case "MerchantPools":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantPoolScreenService));
                }
            case "Merchants":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantScreenService));
                }
            case "MerchantTransactions":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantTransactionScreenService));
                }
            case "BankHealthChecks":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(BankHealthCheckScreenService));
                }
            case "BankLimits":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(BankLimitScreenService));
                }
            case "PostingBalances":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(PostingBalanceScreenService));
                }
            case "DueProfile":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(DueProfileScreenService));
                }
            case "MerchantBlockages":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantBlockagesScreenService));
                }
            case "MerchantBusinessPartner":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantBusinessPartnerScreenService));
                }
            case "MerchantDue":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantDueScreenService));
                }
            case "MerchantIntegrators":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantIntegratorsScreenService));
                }
            case "MerchantLimits":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantLimitScreenService));
                }
            case "MerchantReturnPools":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantReturnPoolScreenService));
                }
            case "MerchantUsers":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MerchantUserScreenService));
                }
            case "CostProfiles":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(CostProfileScreenService));
                }
            default:
                throw new ArgumentOutOfRangeException(resource);
        }
    }
}