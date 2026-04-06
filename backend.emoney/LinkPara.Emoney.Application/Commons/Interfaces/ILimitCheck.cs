using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ILimitCheck
{
    Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel, 
        AccountCurrentLevel accountCurrentLevel);
}