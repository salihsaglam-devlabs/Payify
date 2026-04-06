using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class PriceDidNotMatchException : ApiException
{
    public PriceDidNotMatchException()
        : base(ApiErrorCode.PriceDidNotMatch, "PriceDidNotMatch")
    {
    }
}
