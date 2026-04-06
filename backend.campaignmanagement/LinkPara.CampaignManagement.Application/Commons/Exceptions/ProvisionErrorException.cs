using LinkPara.SharedModels.Exceptions;

namespace LinkPara.CampaignManagement.Application.Commons.Exceptions;

public class ProvisionErrorException : ApiException
{
    public ProvisionErrorException()
: base(ApiErrorCode.ProvisionError, $"ProvisionError")
    {
    }
}
