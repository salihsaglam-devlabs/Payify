using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPools.Command.ApproveMerchantPool;

public class ApproveMerchantPoolCommand : IRequest<ApproveMerchantPoolResponse>
{
    public Guid MerchantPoolId { get; set; }
    public bool IsApprove { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
    public Guid UserId { get; set; }
}

public class ApproveMerchantPoolCommandHandler : IRequestHandler<ApproveMerchantPoolCommand, ApproveMerchantPoolResponse>
{
    private readonly IMerchantPoolService _merchantPoolService;

    public ApproveMerchantPoolCommandHandler(IMerchantPoolService merchantPoolService)
    {
        _merchantPoolService = merchantPoolService;
    }
    public async Task<ApproveMerchantPoolResponse> Handle(ApproveMerchantPoolCommand request, CancellationToken cancellationToken)
    {
        return await _merchantPoolService.ApproveMerchantPool(request);
    }
}