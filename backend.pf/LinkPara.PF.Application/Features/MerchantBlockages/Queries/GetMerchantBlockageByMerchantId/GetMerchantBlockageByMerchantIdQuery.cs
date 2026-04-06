using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetMerchantBlockageByMerchantId;

public class GetMerchantBlockageByMerchantIdQuery : IRequest<MerchantBlockageDto>
{
    public Guid MerchantId { get; set; }
}

public class GetMerchantBlockageByMerchantIdQueryHandler : IRequestHandler<GetMerchantBlockageByMerchantIdQuery, MerchantBlockageDto>
{
    private readonly IMerchantBlockageService _merchantBlockageService;

    public GetMerchantBlockageByMerchantIdQueryHandler(IMerchantBlockageService merchantBlockageService)
    { 
        _merchantBlockageService = merchantBlockageService;
    }

    public async Task<MerchantBlockageDto> Handle(GetMerchantBlockageByMerchantIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantBlockageService.GetByMerchantIdAsync(request.MerchantId);
    }
}