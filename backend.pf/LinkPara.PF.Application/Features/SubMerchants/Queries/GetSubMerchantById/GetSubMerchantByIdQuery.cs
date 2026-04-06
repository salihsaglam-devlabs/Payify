using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchants.Queries.GetSubMerchantById;

public class GetSubMerchantByIdQuery : IRequest<SubMerchantDto>
{
    public Guid Id { get; set; }
}
public class GetSubMerchantByIdQueryHandler : IRequestHandler<GetSubMerchantByIdQuery, SubMerchantDto>
{
    private readonly ISubMerchantService _subMerchantService;

    public GetSubMerchantByIdQueryHandler(ISubMerchantService subMerchantService)
    {
        _subMerchantService = subMerchantService;
    }

    public async Task<SubMerchantDto> Handle(GetSubMerchantByIdQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantService.GetByIdAsync(request.Id);
    }
}