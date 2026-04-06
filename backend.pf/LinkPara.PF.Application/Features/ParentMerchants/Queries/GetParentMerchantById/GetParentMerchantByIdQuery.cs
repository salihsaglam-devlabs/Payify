using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Merchants;
using MediatR;

namespace LinkPara.PF.Application.Features.ParentMerchants.Queries.GetParentMerchantById;

public class GetParentMerchantByIdQuery : IRequest<MerchantDto>
{
    public Guid Id { get; set; }
}
public class GetParentMerchantByIdQueryHandler : IRequestHandler<GetParentMerchantByIdQuery, MerchantDto>
{
    private readonly IMerchantService _merchantService;

    public GetParentMerchantByIdQueryHandler(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<MerchantDto> Handle(GetParentMerchantByIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantService.GetByIdAsync(request.Id);
    }
}
