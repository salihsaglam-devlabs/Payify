using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class SendDataException : ApiException
{
    public SendDataException(string message)
        : base(ApiErrorCode.SendData, message)
    {
    }
}
