using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class GreaterThanProvisionAmountException : ApiException
{
    public GreaterThanProvisionAmountException()
    : base(ApiErrorCode.GreaterThanProvisionAmount, "GreaterThanProvisionAmount")
    {
    }
    
    protected GreaterThanProvisionAmountException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
