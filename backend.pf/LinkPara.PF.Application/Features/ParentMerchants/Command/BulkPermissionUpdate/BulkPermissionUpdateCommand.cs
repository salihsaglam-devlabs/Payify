using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPermissionUpdate;

public class BulkPermissionUpdateCommand : IRequest, IMapFrom<Merchant>
{
    public Guid[] ParentMerchantIdList { get; set; }
    public Guid? MainSubMerchantId { get; set; }
    public int? CityCode { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public MerchantType? MerchantType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public IntegrationMode? UpdateIntegrationMode { get; set; }
    public bool? FinancialTransactionAllowed { get; set; }
    public bool? PaymentAllowed { get; set; }
    public bool? InstallmentAllowed { get; set; }
    public bool? Is3dRequired { get; set; }
    public bool? PreAuthorizationAllowed { get; set; }
    public bool? IsPostAuthAmountHigherAllowed { get; set; }
    public bool? InternationalCardAllowed { get; set; }
    public bool? PaymentReturnAllowed { get; set; }
    public bool? IsReturnApproved { get; set; }
    public bool? IsExcessReturnAllowed { get; set; }
    public bool? PaymentReverseAllowed { get; set; }
    public bool? IsCvvPaymentAllowed { get; set; }
}
public class BulkPermissionUpdateCommandHandler : IRequestHandler<BulkPermissionUpdateCommand>
{
    private readonly IMerchantService _merchantService;

    public BulkPermissionUpdateCommandHandler(
        IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<Unit> Handle(BulkPermissionUpdateCommand request, CancellationToken cancellationToken)
    {
        await _merchantService.MerchantPermissionBatchUpdateAsync(request);

        return Unit.Value;
    }
}