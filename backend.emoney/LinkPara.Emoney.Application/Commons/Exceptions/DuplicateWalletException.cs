using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class DuplicateWalletException : ApiException
{
    public DuplicateWalletException()
        : base(ApiErrorCode.DuplicateWallet, "DuplicateWallet")
    {
    }
    
    protected DuplicateWalletException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
