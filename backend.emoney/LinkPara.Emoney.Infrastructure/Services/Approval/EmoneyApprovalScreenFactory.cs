using LinkPara.Approval;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class EmoneyApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmoneyApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "Limits":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(LimitScreenService));
                }
            case "EmoneyAccounts":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(CustodyAccountScreenService));
                }
            case "EmoneyPricingProfiles":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(PricingProfileScreenService));
                }
            case "CommercialPricing":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(CommercialPricingScreenService));
                }
            case "TierPermissions":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(TierPermissionScreenService));
                }
            case "Topups":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(TopupScreenService));
                }
            case "TierLevels":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(TierLevelScreenService));
                }
            case "Wallets":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(WalletScreenService));
                }
            case "Chargeback":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(ChargebackScreenService));
                }
            case "WalletBlockages":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(WalletBlockageScreenService));
                }
            default:
                return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(DefaultScreenService));
        }
    }
}