using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IWalletBlockageHttpClient
{
    Task<PaginatedList<WalletBlockageDto>> GetWalletBlockageAsync(GetWalletBlockageRequest request);
    Task AddWalletBlockageAsync(AddWalletBlockageRequest request);
    Task<WalletBlockageDocumentDto> AddWalletBlockageDocumentAsync(AddWalletBlockageDocumentRequest request);
    Task RemoveWalletBlockageDocumentAsync(RemoveWalletBlockageDocumentRequest request);
    Task<List<WalletBlockageDocumentDto>> GetWalletBlockageDocumentsAsync(GetWalletBlockageDocumentRequest request);    
}