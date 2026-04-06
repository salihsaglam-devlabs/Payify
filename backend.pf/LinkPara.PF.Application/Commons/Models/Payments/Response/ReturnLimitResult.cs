using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class ReturnLimitResult
{
    public bool IsSuccess { get; set; } 
    public LimitType LimitType { get; set; }
    public Period Period { get; set; }
}