using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class CanNotCloseMainWalletException : ApiException
{
    public CanNotCloseMainWalletException()
        : base(ApiErrorCode.CanNotCloseMainWallet, "MainWalletCanNotBeClosed") { }
    
    protected CanNotCloseMainWalletException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}