using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchants.Command.SaveSubMerchant;

public class SaveSubMerchantCommand : IRequest<Guid>, IMapFrom<SubMerchant>
{
    public string Name { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid MerchantId { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public bool IsManuelPaymentPageAllowed { get; set; }
    public bool IsLinkPaymentPageAllowed { get; set; }
    public bool IsOnUsPaymentPageAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool PaymentReverseAllowed { get; set; }
    public bool PaymentReturnAllowed { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsExcessReturnAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class SaveSubMerchantCommandHandler : IRequestHandler<SaveSubMerchantCommand, Guid>
{
    private readonly ISubMerchantService _subMerchantService;

    public SaveSubMerchantCommandHandler(ISubMerchantService subMerchantService)
    {
        _subMerchantService = subMerchantService;
    }
    public async Task<Guid> Handle(SaveSubMerchantCommand request, CancellationToken cancellationToken)
    {
        return await _subMerchantService.SaveAsync(request);
    }
}
