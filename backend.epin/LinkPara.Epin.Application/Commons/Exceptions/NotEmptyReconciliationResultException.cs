using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class NotEmptyReconciliationResultException : ApiException
{
    public NotEmptyReconciliationResultException()
        : base(ApiErrorCode.NotEmptyReconciliationResult, "NotEmptyReconciliationResult")
    {
    }
}
