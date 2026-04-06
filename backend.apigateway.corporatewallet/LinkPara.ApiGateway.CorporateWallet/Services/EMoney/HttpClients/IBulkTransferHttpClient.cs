using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface IBulkTransferHttpClient
{
    Task ActionBulkTransferAsync(ActionBulkTransferRequest request);
    Task<BulkTransferDto> GetBulkTransferByIdAsync(Guid id);
    Task<PaginatedList<BulkTransferDto>> GetBulkTransfersAsync(GetBulkTransfersRequest request);
    Task<BulkTransferDto> GetReportBulkTransferByIdAsync(Guid id);
    Task<PaginatedList<BulkTransferDto>> GetReportBulkTransfersAsync(GetReportBulkTransferRequest request);
    Task SaveBulkTransferAsync(SaveBulkTransferRequest request);
}
