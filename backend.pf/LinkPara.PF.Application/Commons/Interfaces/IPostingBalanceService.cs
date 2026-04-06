using LinkPara.PF.Application.Features.PostingBalances.Queries.GetAllPostingBalances;
using LinkPara.PF.Application.Features.PostingBalances;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceStatistics;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPostingBalanceService
{
    Task<PostingBalanceResponse> GetAllAsync(GetAllPostingBalanceQuery request);
    Task<PostingBalanceStatisticsResponse> GetStatisticsAsync(GetPostingBalanceStatisticsQuery request);
    Task<PostingBalanceDto> GetByIdAsync(Guid id);
}
