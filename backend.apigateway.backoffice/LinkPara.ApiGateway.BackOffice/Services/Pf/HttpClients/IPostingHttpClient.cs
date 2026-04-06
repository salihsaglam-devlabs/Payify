using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IPostingHttpClient
{
    Task<PaginatedList<PostingTransferErrorDto>> GetAllTransferErrorAsync(GetAllPostingTransferErrorRequest request);
    Task<PaginatedList<PostingBillDto>> GetBillsAsync(GetPostingBillRequest request);
}