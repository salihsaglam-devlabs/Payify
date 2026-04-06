using LinkPara.Approval;

namespace LinkPara.Accounting.Infrastructure.Services.Approval;

public class AccountingApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AccountingApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "Payments":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(CustomerScreenService));
                }
            case "Customers":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(CustomerScreenService));
                }
            default:
                return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(DefaultScreenService));
        }
    }
}
