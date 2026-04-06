using LinkPara.Emoney.Application.Features.Chargebacks;
using LinkPara.Emoney.Application.Features.Chargebacks.Commands;
using LinkPara.Emoney.Application.Features.Chargebacks.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IChargebackService
{
    Task<PaginatedList<ChargebackDto>> GetChargebackAsync(GetChargebackQuery request);
    Task<ChargebackDto> InitChargebackAsync(InitChargebackCommand request);
    Task<ChargebackDto> ApproveChargebackAsync(ApproveChargebackCommand request);
    Task<ChargebackDocumentDto> AddChargebackDocumentAsync(AddChargebackDocumentCommand request);
    Task<bool> DeleteChargebackDocumentAsync(DeleteChargebackDocumentCommand request);
    Task<List<ChargebackDocumentDto>> GetChargebackDocumentsAsync(GetChargebackDocumentQuery request);
}
