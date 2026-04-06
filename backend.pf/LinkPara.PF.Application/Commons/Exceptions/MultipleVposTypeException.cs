using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class MultipleVposTypeException : ApiException
{
    public MultipleVposTypeException()
        : base(ApiErrorCode.MultipleVposTypeNotAllowed, "MultipleVposTypeNotAllowed")
    {
    }
}