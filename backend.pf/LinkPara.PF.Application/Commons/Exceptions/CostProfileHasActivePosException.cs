using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;
public class CostProfileHasActivePosException : ApiException
{
    public CostProfileHasActivePosException()
         : base(ApiErrorCode.CostProfileHasActivePos, "CostProfileHasActivePos")
    {
    }
}
