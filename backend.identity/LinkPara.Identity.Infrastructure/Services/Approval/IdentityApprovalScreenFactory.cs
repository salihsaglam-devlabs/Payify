
using LinkPara.Approval;

namespace LinkPara.Identity.Infrastructure.Services.Approval;

public class IdentityApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public IdentityApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "Users":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(UserScreenService));
                }
            case "Roles":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(RoleScreenService));
                }
            case "Questions":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(QuestionScreenService));
                }
            case "AgreementDocuments":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(AgreementDocumentScreenService));
                }
            default:
                throw new Exception();
        }
    }
}
