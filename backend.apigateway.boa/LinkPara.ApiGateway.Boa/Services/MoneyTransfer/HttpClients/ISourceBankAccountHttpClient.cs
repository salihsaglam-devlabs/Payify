using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.MoneyTransfer.HttpClients;

public interface ISourceBankAccountHttpClient
{
    Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request);
}
