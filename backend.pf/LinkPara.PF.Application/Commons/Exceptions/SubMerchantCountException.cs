using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class SubMerchantCountException : CustomApiException
    {
        public SubMerchantCountException()
            : base(ApiErrorCode.SubMerchantCountError, "SubMerchantCountError")
        { }

        public SubMerchantCountException(string message)
            : base(ApiErrorCode.SubMerchantCountError, message) { }
    }
}
