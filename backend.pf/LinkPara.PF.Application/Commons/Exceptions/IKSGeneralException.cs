using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class IKSGeneralException : CustomApiException
    {
        public IKSGeneralException() 
            : base(ApiErrorCode.IKSGeneralError, "IKSGeneralError")
        {}
        
        public IKSGeneralException(string message) 
            : base(ApiErrorCode.IKSGeneralError, message) {}
    }
}
