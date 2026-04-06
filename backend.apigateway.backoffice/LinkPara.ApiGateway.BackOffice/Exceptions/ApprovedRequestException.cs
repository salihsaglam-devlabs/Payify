using LinkPara.SharedModels.Exceptions;

namespace LinkPara.ApiGateway.BackOffice.Exceptions;

public class ApprovedRequestException : CustomApiException
{
    public ApprovedRequestException(string code, string message) : base(code, message)
    {
    }
}
