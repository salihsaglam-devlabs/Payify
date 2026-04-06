using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions
{
    public class ProvisionPreviewException : ApiException
    {
        public ProvisionPreviewException() : base(ApiErrorCode.PreviewProvisionError, "ProvisionPreviewError") { }
    }
}