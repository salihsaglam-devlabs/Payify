using LinkPara.Approval;

namespace LinkPara.Billing.Infrastructure.Services.Approval;

public class BillingApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BillingApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "Institutions":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(InstitutionScreenService));
                }
            default:
                return (IApprovalScreenService)
                    _serviceProvider.GetService(typeof(DefaultScreenService));
        }
    }
}