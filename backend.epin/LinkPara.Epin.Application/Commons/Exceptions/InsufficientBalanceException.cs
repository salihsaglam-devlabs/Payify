using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class InsufficientBalanceException : ApiException
{
    public InsufficientBalanceException(string message)
        : base(ApiErrorCode.InsufficientBalance, message)
    {
    }
}