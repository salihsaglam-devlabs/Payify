using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Accounting.Application.Commons.Exceptions;

public class CanNotCancelNotSuccessPaymentException : ApiException
{
    public CanNotCancelNotSuccessPaymentException()
        : base(ApiErrorCode.CanNotCancelNotSuccessPayment, "CanNotCancelNotSuccessPayment")
    {
    }
}