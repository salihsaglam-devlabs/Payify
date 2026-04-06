using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.BillingModels.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Billing.Application.Features.Institutions.Queries;

public class GetAllInstitutionQuery : SearchQueryParams, IRequest<PaginatedList<InstitutionDto>>
{
    public Guid? VendorId { get; set; }
    public Guid? SectorId { get; set; }
    public OperationMode? OperationMode { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllInstitutionQueryHandler : IRequestHandler<GetAllInstitutionQuery, PaginatedList<InstitutionDto>>
{
    private readonly IInstitutionService _institutionService;

    public GetAllInstitutionQueryHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<PaginatedList<InstitutionDto>> Handle(GetAllInstitutionQuery request, CancellationToken cancellationToken)
    {
        return await _institutionService.GetListAsync(request);
    }
}