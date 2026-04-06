using LinkPara.PF.Application.Commons.Interfaces.Boa;
using MediatR;

namespace LinkPara.PF.Application.Features.Boa.Merchants.Queries.GetBoaMerchantByMerchantNumber;

public class GetBoaMerchantByMerchantNumberQuery : IRequest<BoaMerchantDto>
{
    public string MerchantNumber { get; set; }
}

public class GetBoaMerchantByMerchantNumberQueryHandler : IRequestHandler<GetBoaMerchantByMerchantNumberQuery, BoaMerchantDto>
{
    private readonly IBoaMerchantService _boaMerchantService;

    public GetBoaMerchantByMerchantNumberQueryHandler(IBoaMerchantService boaMerchantService)
    {
        _boaMerchantService = boaMerchantService;
    }

    public async Task<BoaMerchantDto> Handle(GetBoaMerchantByMerchantNumberQuery request, CancellationToken cancellationToken)
    {
        return await _boaMerchantService.GetMerchantByNumberAsync(request.MerchantNumber);
    }
}