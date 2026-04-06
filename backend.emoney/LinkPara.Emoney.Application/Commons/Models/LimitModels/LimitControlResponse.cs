using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.LimitModels;

public class LimitControlResponse
{
    public bool IsLimitExceeded { get; set; }
    public LimitOperationType LimitOperationType { get; set; }
}