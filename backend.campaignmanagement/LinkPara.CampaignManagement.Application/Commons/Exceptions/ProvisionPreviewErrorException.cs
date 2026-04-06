using LinkPara.SharedModels.Exceptions;

namespace LinkPara.CampaignManagement.Application.Commons.Exceptions;

public class ProvisionPreviewErrorException : ApiException
{
    public ProvisionPreviewErrorException()
        : base(ApiErrorCode.ProvisionPreviewError, $"ProvisionPreviewError")
    {
    }
}
