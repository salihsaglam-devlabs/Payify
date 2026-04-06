using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class WalletBlockageAmountException : ApiException
{
    public WalletBlockageAmountException() : base(ApiErrorCode.WalletBlockageAmountIsGreaterThanAvailabeBalance, "WalletBlockageAmountIsGreaterThanAvailabeBalance")
    {
    }
    protected WalletBlockageAmountException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
