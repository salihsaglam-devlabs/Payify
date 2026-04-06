using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantLimits.Queries.GetSubMerchantLimitById;

public class GetSubMerchantLimitByIdQuery : SearchQueryParams, IRequest<SubMerchantLimitDto>
{
    public Guid SubMerchantLimitId { get; set; }
}

public class GetSubMerchantLimitByIdHandler : IRequestHandler<GetSubMerchantLimitByIdQuery, SubMerchantLimitDto>
{
    private readonly ISubMerchantLimitService _subMerchantLimitService;

    public GetSubMerchantLimitByIdHandler(ISubMerchantLimitService subMerchantLimitService)
    {
        _subMerchantLimitService = subMerchantLimitService;
    }

    public async Task<SubMerchantLimitDto> Handle(GetSubMerchantLimitByIdQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantLimitService.GetByIdAsync(request.SubMerchantLimitId);
    }
}