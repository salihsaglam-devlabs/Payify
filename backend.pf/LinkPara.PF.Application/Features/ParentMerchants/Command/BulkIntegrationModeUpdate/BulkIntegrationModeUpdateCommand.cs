using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPermissionUpdate;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.ParentMerchants.Command.BulkIntegrationModeUpdate;

public class BulkIntegrationModeUpdateCommand : IRequest, IMapFrom<Merchant>
{
    public Guid[] ParentMerchantIdList { get; set; }
    public Guid? MainSubMerchantId { get; set; }
    public int? CityCode { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public MerchantType? MerchantType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public bool? IsApiMode { get; set; }
    public bool? IsHppMode { get; set; }
    public bool? IsManuelPaymentPageMode { get; set; }
    public bool? IsLinkPaymentPageMode { get; set; }
    public bool? IsOnUsMode { get; set; }
}
public class BulkIntegrationModeUpdateCommandHandler : IRequestHandler<BulkIntegrationModeUpdateCommand>
{
    private readonly IMerchantService _merchantService;

    public BulkIntegrationModeUpdateCommandHandler(
        IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<Unit> Handle(BulkIntegrationModeUpdateCommand request, CancellationToken cancellationToken)
    {
        await _merchantService.MerchantIntegrationModeBatchUpdateAsync(request);

        return Unit.Value;
    }
}