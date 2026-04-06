using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class WalletHasBalanceException : ApiException
{
    public WalletHasBalanceException()
        : base(ApiErrorCode.WalletHasBalanceException, "WalletHasBalanceException") { }
    
    protected WalletHasBalanceException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}