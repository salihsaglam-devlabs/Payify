using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.HttpClients
{
    public interface ISourceBankAccountHttpClient
    {
        Task<PaginatedList<SourceBankAccountDto>> GetListAsync(GetSourceBankAccountListRequest request);
    }
}
