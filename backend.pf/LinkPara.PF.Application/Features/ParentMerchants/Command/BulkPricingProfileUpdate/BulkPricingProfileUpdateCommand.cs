using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPricingProfileUpdate;

public class BulkPricingProfileUpdateCommand : IRequest, IMapFrom<Merchant>
{
    public Guid[] ParentMerchantIdList { get; set; }
    public Guid? MainSubMerchantId { get; set; }
    public int? CityCode { get; set; }
    public IntegrationMode? IntegrationMode { get; set; }
    public MerchantType? MerchantType { get; set; }
    public MerchantStatus? MerchantStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string PricingProfileNumber { get; set; }
}
public class BulkPricingProfileUpdateCommandHandler : IRequestHandler<BulkPricingProfileUpdateCommand>
{
    private readonly IMerchantService _merchantService;

    public BulkPricingProfileUpdateCommandHandler(
        IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<Unit> Handle(BulkPricingProfileUpdateCommand request, CancellationToken cancellationToken)
    {
        await _merchantService.PricingProfileBatchUpdateAsync(request);

        return Unit.Value;
    }
}