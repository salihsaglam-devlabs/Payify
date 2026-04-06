using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPools.Queries.GetMerchantPoolById;

public class GetMerchantPoolByIdQuery : IRequest<MerchantPoolDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantPoolByIdQueryHandler : IRequestHandler<GetMerchantPoolByIdQuery, MerchantPoolDto>
{
    private readonly IMerchantPoolService _merchantPoolService;

    public GetMerchantPoolByIdQueryHandler(IMerchantPoolService merchantPoolService)
    {
        _merchantPoolService = merchantPoolService;
    }
    public async Task<MerchantPoolDto> Handle(GetMerchantPoolByIdQuery request, CancellationToken cancellationToken)
    {
       return await _merchantPoolService.GetByIdAsync(request);
    }
}
