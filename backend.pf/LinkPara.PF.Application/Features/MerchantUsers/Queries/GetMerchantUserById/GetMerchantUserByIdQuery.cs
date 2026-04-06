using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantUsers.Queries.GetMerchantUserById;

public class GetMerchantUserByIdQuery : IRequest<MerchantUserDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantUserByIdQueryHandler : IRequestHandler<GetMerchantUserByIdQuery, MerchantUserDto>
{
    private readonly IMerchantUserService _merchantUserService;

    public GetMerchantUserByIdQueryHandler(IMerchantUserService merchantUserService)
    {
        _merchantUserService = merchantUserService;
    }
    public async Task<MerchantUserDto> Handle(GetMerchantUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantUserService.GetByIdAsync(request.Id);
    }
}
