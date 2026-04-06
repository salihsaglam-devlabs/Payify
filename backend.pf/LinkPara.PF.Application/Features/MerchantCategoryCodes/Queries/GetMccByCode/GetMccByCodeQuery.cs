using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccByCode;

public class GetMccByCodeQuery : IRequest<MccDto>
{
    public string Code { get; set; }
}

public class GetMccByCodeQueryHandler : IRequestHandler<GetMccByCodeQuery, MccDto>
{
    private readonly IMccService _mccService;

    public GetMccByCodeQueryHandler(IMccService mccService)
    {
        _mccService = mccService;
    }
    public async Task<MccDto> Handle(GetMccByCodeQuery request, CancellationToken cancellationToken)
    {
        return await _mccService.GetByCodeAsync(request.Code);
    }
}
