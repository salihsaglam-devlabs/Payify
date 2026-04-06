

namespace LinkPara.Approval.Infrastructure.Services.Approval;

public class ApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "Cases":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(MakerCheckersScreenService));
                }
            default:
                throw new Exception();
        }
    }
}