using LinkPara.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Infrastructure.Services.Approval;

public class BusinessParameterApprovalScreenFactory : IApprovalScreenFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BusinessParameterApprovalScreenFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public IApprovalScreenService GetApprovalScreenService(string resource)
    {
        switch (resource)
        {
            case "ParameterGroups":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(ParameterGroupScreenService));
                }
            case "ParameterTemplate":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(ParameterTemplateScreenService));
                }
            case "Parameters":
                {
                    return (IApprovalScreenService)
                        _serviceProvider.GetService(typeof(ParameterScreenService));
                }
            default:
                throw new Exception();
        }
    }
}
