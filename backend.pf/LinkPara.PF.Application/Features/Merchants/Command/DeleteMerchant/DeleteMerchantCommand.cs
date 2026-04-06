using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;

public class DeleteMerchantCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteMerchantCommandHandler : IRequestHandler<DeleteMerchantCommand>
{
    private readonly IMerchantService _merchantService;

    public DeleteMerchantCommandHandler(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }
    public async Task<Unit> Handle(DeleteMerchantCommand request, CancellationToken cancellationToken)
    {
        await _merchantService.DeleteAsync(request);

        return Unit.Value;
    }
}
