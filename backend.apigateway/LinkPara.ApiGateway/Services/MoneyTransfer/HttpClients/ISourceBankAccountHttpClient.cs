using LinkPara.ApiGateway.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.MoneyTransfer.HttpClients
{
    public interface ISourceBankAccountHttpClient
    {
        Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request);
    }
}
