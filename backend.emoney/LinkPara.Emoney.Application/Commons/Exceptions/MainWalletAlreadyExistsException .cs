using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class MainWalletAlreadyExistsException: ApiException
{
    public MainWalletAlreadyExistsException()
       : base(ApiErrorCode.MainWalletAlreadyExistError, "MainWalletAlreadyExist")
    {
    }
    
    protected MainWalletAlreadyExistsException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
