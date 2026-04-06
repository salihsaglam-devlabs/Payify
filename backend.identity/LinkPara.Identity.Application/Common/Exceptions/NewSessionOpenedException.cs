using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Common.Exceptions
{
    public class NewSessionOpenedException : ApiException
    {
        public NewSessionOpenedException()
            : base(ApiErrorCode.NewSessionOpened, "Logged in from somewhere else.") { }
    }
}
