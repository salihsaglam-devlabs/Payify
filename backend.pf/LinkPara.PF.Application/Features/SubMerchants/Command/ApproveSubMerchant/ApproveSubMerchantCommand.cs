using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.ApproveSubMerchant;

public class ApproveSubMerchantCommand : IRequest
{
        public Guid SubMerchantId { get; set; }
        public bool IsApprove { get; set; }
        public string RejectReason { get; set; }
        public string ParameterValue { get; set; }
}
public class ApproveSubMerchantCommandHandler : IRequestHandler<ApproveSubMerchantCommand>
{
    private readonly ISubMerchantService _subMerchantService;

    public ApproveSubMerchantCommandHandler(ISubMerchantService subMerchantService)
    {
        _subMerchantService = subMerchantService;
    }

    public async Task<Unit> Handle(ApproveSubMerchantCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantService.ApproveAsync(request);

        return Unit.Value;
    }
}
