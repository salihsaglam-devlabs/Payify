using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Utility;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.HttpProviders.Fraud;

public interface ISearchService
{
    Task<SearchResponse> GetSearchByName(SearchByNameRequest request);
    Task<SearchResponse> GetSearchByIdentity(string id);
    Task<BaseResponse> RemoveOngoingMonitoringAsync(string referenceNumber);
}
