using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions
{
    [Serializable]
    public class WalletBlockedException : ApiException
    {
        public WalletBlockedException()
            : base(ApiErrorCode.WalletBlocked, "WalletBlockedException")
        {
        }
        public WalletBlockedException(object key)
            : base(ApiErrorCode.WalletBlocked, "Wallet : {key} is in invalid blocked")
        {

        }
        
        protected WalletBlockedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
