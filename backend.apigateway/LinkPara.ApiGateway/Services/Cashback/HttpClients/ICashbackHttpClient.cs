using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkPara.ApiGateway.Services.Cashback.Models.Requests;
using LinkPara.ApiGateway.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Cashback.HttpClients;

public interface ICashbackHttpClient
{
    Task<PaginatedList<CashbackRuleSummaryDto>> GetFilteredRulesAsync(GetFilteredRulesRequest request);
}
