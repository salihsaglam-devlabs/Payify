using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;

public class UpdateMerchantBlockageCommand : IRequest
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
}

public class UpdateMerchantBlockageCommandHandler : IRequestHandler<UpdateMerchantBlockageCommand>
{
    private readonly IMerchantBlockageService _merchantBlockageService;

    public UpdateMerchantBlockageCommandHandler(IMerchantBlockageService merchantBlockageService)
    {
        _merchantBlockageService = merchantBlockageService;
    }
    public async Task<Unit> Handle(UpdateMerchantBlockageCommand request, CancellationToken cancellationToken)
    {
        await _merchantBlockageService.UpdateAsync(request);

        return Unit.Value;
    }
}
