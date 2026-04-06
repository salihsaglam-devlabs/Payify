using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Fields.Queries;

public class GetByInstitutionIdQuery : SearchQueryParams, IRequest<PaginatedList<FieldDto>>
{
    public Guid InstitutionId { get; set; }
}

public class GetByInstitutionIdQueryHandler : IRequestHandler<GetByInstitutionIdQuery, PaginatedList<FieldDto>>
{
    private readonly IFieldService _fieldService;

    public GetByInstitutionIdQueryHandler(IFieldService fieldService)
    {
        _fieldService = fieldService;
    }

    public async Task<PaginatedList<FieldDto>> Handle(GetByInstitutionIdQuery request, CancellationToken cancellationToken)
    {
        return await _fieldService.GetByInstitutionIdAsync(request);
    }
}