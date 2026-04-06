using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantPreApplication;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetMerchantPreApplicationById;

public class GetMerchantPreApplicationByIdQuery : IRequest<MerchantPreApplicationDto>
{
    public Guid Id { get; set; }
}
public class GetPosApplicationByIdQueryHandler : IRequestHandler<GetMerchantPreApplicationByIdQuery, MerchantPreApplicationDto>
{
    private readonly IMerchantPreApplicationService _merchantPreApplicationService;
    
    public GetPosApplicationByIdQueryHandler(IMerchantPreApplicationService merchantPreApplicationService)
    {
        _merchantPreApplicationService = merchantPreApplicationService;
    }
    
    public async Task<MerchantPreApplicationDto> Handle(GetMerchantPreApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantPreApplicationService.GetPosApplicationByIdAsync(request.Id);
    }
}