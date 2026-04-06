using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class PostingUpdateBankNotPaymentWaitingException : ApiException
{
    public PostingUpdateBankNotPaymentWaitingException()
         : base(ApiErrorCode.PostingUpdateBankNotPaymentWaiting, "PostingUpdateBankNotPaymentWaiting")
    {
    }
}