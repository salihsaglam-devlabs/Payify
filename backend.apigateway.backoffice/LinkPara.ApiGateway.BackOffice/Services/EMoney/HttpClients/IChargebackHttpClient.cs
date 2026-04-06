using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients
{
    public interface IChargebackHttpClient
    {
        Task<PaginatedList<ChargebackDto>> GetChargebackAsync(GetChargebackRequest request);
        Task<ChargebackDto> InitializeChargebackAsync(InitChargebackRequest request);
        Task<ChargebackDto> ApproveChargebackAsync(ApproveChargebackRequest request);
        Task<ChargebackDocumentDto> AddChargebackDocumentAsync(AddChargebackDocumentRequest request);
        Task<bool> DeleteChargebackDocumentAsync(DeleteChargebackDocumentRequest request);
        Task<List<ChargebackDocumentDto>> GetChargebackDocumentsAsync(GetChargebackDocumentRequest request);
    }
}
