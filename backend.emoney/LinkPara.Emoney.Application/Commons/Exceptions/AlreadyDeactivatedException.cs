using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AlreadyDeactivatedException : ApiException
{
    public AlreadyDeactivatedException()
        : base(ApiErrorCode.AlreadyDeactivated, $"The entity's record status already passivated!") { }


    public AlreadyDeactivatedException(string recordName)
        : base(ApiErrorCode.AlreadyDeactivated, $"Record status of {recordName} already passivated!") { }
    
    protected AlreadyDeactivatedException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}