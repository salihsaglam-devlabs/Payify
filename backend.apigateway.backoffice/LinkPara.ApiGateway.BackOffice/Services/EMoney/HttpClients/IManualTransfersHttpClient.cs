using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IManualTransfersHttpClient
{
    public Task<PaginatedList<ManualTransferResponse>> GetAllManualTransfersAsync(GetAllManualTransfersRequest request);
    public Task CreateManualTransfersAsync(CreateManualTransferRequest request);
}