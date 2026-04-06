using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Command.SaveMerchantBlockage;

public class SaveMerchantBlockageCommand : IRequest, IMapFrom<MerchantBlockage>
{
    public decimal TotalAmount { get; set; }
    public decimal BlockageAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public MerchantBlockageStatus MerchantBlockageStatus { get; set; }
    public Guid MerchantId { get; set; }
}

public class SaveMerchantBlockageCommandHandler : IRequestHandler<SaveMerchantBlockageCommand>
{
    private readonly IMerchantBlockageService _merchantBlockageService;

    public SaveMerchantBlockageCommandHandler(IMerchantBlockageService merchantBlockageService)
    {
        _merchantBlockageService = merchantBlockageService;
    }

    public async Task<Unit> Handle(SaveMerchantBlockageCommand request, CancellationToken cancellationToken)
    {
        await _merchantBlockageService.SaveAsync(request);

        return Unit.Value;
    }
}