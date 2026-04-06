using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.Merchants.Command.ApproveMerchant;

public class ApproveMerchantCommand : IRequest
{
    public Guid MerchantId { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
}

public class ApproveMerchantCommandHandler : IRequestHandler<ApproveMerchantCommand>
{
    private readonly IMerchantService _merchantService;

    public ApproveMerchantCommandHandler(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<Unit> Handle(ApproveMerchantCommand request, CancellationToken cancellationToken)
    {
        await _merchantService.ApproveMerchant(request);

        return Unit.Value;
    }
}
