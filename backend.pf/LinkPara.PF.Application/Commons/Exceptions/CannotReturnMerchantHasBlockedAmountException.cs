using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class CannotReturnMerchantHasBlockedAmountException : ApiException
{
    public CannotReturnMerchantHasBlockedAmountException()
        : base(ApiErrorCode.CannotReturnMerchantHasBlockedAmount, "ReturnCannotBecauseMerchantHasBlockedAmountException")
    {
    }
}