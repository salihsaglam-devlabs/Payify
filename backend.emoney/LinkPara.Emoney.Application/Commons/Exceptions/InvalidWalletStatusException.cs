using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidWalletStatusException : ApiException
{
    public InvalidWalletStatusException()
        : base(ApiErrorCode.InvalidWalletStatus, "InvalidWalletStatus")
    {
    }

    public InvalidWalletStatusException(object key)
        : base(ApiErrorCode.InvalidWalletStatus, "Wallet : {key} is in invalid status")
    {
    }
    
    protected InvalidWalletStatusException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}