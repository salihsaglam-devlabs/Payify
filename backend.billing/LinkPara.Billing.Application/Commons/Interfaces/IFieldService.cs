using LinkPara.Billing.Application.Features.Fields;
using LinkPara.Billing.Application.Features.Fields.Queries;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IFieldService
{
    Task<PaginatedList<FieldDto>> GetByInstitutionIdAsync(GetByInstitutionIdQuery request);
}