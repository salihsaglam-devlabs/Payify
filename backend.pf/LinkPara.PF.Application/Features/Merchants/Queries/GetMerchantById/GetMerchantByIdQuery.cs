using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantById;

public class GetMerchantByIdQuery : IRequest<MerchantDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantByIdQueryHandler : IRequestHandler<GetMerchantByIdQuery, MerchantDto>
{
    private readonly IMerchantService _merchantService;

    public GetMerchantByIdQueryHandler(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<MerchantDto> Handle(GetMerchantByIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantService.GetByIdAsync(request.Id);
    }
}
