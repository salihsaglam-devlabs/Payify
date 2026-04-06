using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.UpdateMultipleSubMerchant;

public class UpdateMultipleSubMerchantCommand : IRequest
{
    public List<Guid> SubMerchants { get; set; }
    public List<SubMerchantLimitDto> Limits { get; set; }
    public bool? IsManuelPaymentPageAllowed { get; set; }
    public bool? IsLinkPaymentPageAllowed { get; set; }
    public bool? IsOnUsPaymentPageAllowed { get; set; }
    public bool? PreAuthorizationAllowed { get; set; }
    public bool? PaymentReverseAllowed { get; set; }
    public bool? PaymentReturnAllowed { get; set; }
    public bool? InstallmentAllowed { get; set; }
    public bool? Is3dRequired { get; set; }
    public bool? IsExcessReturnAllowed { get; set; }
    public bool? InternationalCardAllowed { get; set; }
}
public class UpdateMultipleSubMerchantCommandHandler : IRequestHandler<UpdateMultipleSubMerchantCommand>
{
    private readonly ISubMerchantService _subMerchantService;

    public UpdateMultipleSubMerchantCommandHandler(ISubMerchantService subMerchantService)
    {
        _subMerchantService = subMerchantService;
    }

    public async Task<Unit> Handle(UpdateMultipleSubMerchantCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantService.UpdateMultipleAsync(request);

        return Unit.Value;
    }
}