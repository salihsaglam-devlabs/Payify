using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Institutions.Queries.GetById;

public class GetByIdQuery : SearchQueryParams, IRequest<InstitutionDto>
{
    public Guid InstitutionId { get; set; }
}

public class GetByIdQueryHandler : IRequestHandler<GetByIdQuery, InstitutionDto>
{
    private readonly IInstitutionService _institutionService;

    public GetByIdQueryHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionDto> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        return await _institutionService.GetByIdAsync(request.InstitutionId);
    }
}