using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Queries.GetMerchantIntegratorById;

public class GetMerchantIntegratorByIdQuery : IRequest<MerchantIntegratorDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantIntegratorByIdQueryHandler : IRequestHandler<GetMerchantIntegratorByIdQuery, MerchantIntegratorDto>
{
    private readonly IMerchantIntegratorService _merchantIntegratorService;

    public GetMerchantIntegratorByIdQueryHandler(IMerchantIntegratorService merchantIntegratorService)
    {
        _merchantIntegratorService = merchantIntegratorService;
    }
    public async Task<MerchantIntegratorDto> Handle(GetMerchantIntegratorByIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantIntegratorService.GetByIdAsync(request.Id);
    }
}
