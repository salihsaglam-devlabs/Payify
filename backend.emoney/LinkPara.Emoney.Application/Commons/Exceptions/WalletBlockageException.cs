using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class WalletBlockageException : ApiException
{
    public WalletBlockageException() : base(ApiErrorCode.WalletHasPendingApprovalRequest, "WalletHasPendingApprovalRequest")
    {
    }
    protected WalletBlockageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
