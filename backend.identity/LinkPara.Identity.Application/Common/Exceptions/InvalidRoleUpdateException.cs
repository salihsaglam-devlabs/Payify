using LinkPara.SharedModels.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class InvalidRoleUpdateException : ApiException
{
    public InvalidRoleUpdateException()
        : base(ApiErrorCode.InvalidRoleUpdate, $"Role type/scope cannot be changed.") { }
}