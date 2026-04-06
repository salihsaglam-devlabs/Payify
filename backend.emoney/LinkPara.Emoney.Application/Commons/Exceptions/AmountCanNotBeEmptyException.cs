using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AmountCanNotBeEmptyException : ApiException
{
    public AmountCanNotBeEmptyException()
        : base(ApiErrorCode.AmountCanNotBeEmpty, "AmountCanNotBeEmpty") { }
    
    protected AmountCanNotBeEmptyException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
