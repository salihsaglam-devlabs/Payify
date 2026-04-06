using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantLimits.Commands.UpdateSubMerchantLimit;

public class UpdateSubMerchantLimitCommand : IRequest
{
    public SubMerchantLimitDto SubMerchantLimit { get; set; }
}

public class UpdateSubmerchantLimitCommandHandler : IRequestHandler<UpdateSubMerchantLimitCommand>
{
    private readonly ISubMerchantLimitService _subMerchantLimitService;

    public UpdateSubmerchantLimitCommandHandler(ISubMerchantLimitService subMerchantLimitService)
    {
        _subMerchantLimitService = subMerchantLimitService;
    }

    public async Task<Unit> Handle(UpdateSubMerchantLimitCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantLimitService.UpdateAsync(request);
        return Unit.Value;
    }
}