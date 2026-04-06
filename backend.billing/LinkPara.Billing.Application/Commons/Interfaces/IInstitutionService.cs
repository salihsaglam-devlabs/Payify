using LinkPara.Billing.Application.Features.Institutions;
using LinkPara.Billing.Application.Features.Institutions.Commands;
using LinkPara.Billing.Application.Features.Institutions.Queries;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IInstitutionService
{
    Task<InstitutionDto> GetByIdAsync(Guid institutionId);
    Task<PaginatedList<InstitutionDto>> GetListAsync(GetAllInstitutionQuery request);
    Task<Vendor> GetActiveVendorIdByIdAsync(Guid institutionId);
    Task UpdateAsync(UpdateInstitutionCommand request);
}