using LinkPara.Fraud.Application.Commons.Models;
using LinkPara.Fraud.Application.Commons.Models.Searchs;
using LinkPara.Fraud.Application.Features.Searchs;
using LinkPara.Fraud.Application.Features.Searchs.SearchByNames;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllSearches;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Fraud.Application.Commons.Interfaces;

public interface ISearchService
{
    Task<SearchResponse> GetSearchByNameAsync(SearchByNameQuery request);
    Task<SearchResponse> GetSearchByIdentityAsync(string id);
    Task<PaginatedList<SearchLogDto>> GetListAsync(GetAllSearchesQuery query);
    Task<BaseResponse> RemoveOngoingAsync(string referenceNumber);
}
