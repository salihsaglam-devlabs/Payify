using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Common.Exceptions
{
    public class SessionExpiredException : ApiException
    {
        public SessionExpiredException()
            : base(ApiErrorCode.SessionExpired, "User session expired.") { }
    }
}
