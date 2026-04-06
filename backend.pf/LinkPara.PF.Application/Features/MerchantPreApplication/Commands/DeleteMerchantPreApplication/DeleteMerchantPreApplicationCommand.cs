using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Commands.DeleteMerchantPreApplication;

public class DeleteMerchantPreApplicationCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePendingPosApplicationCommandHandler : IRequestHandler<DeleteMerchantPreApplicationCommand>
{
    private readonly IMerchantPreApplicationService _merchantPreApplicationService;

    public DeletePendingPosApplicationCommandHandler(IMerchantPreApplicationService merchantPreApplicationService)
    {
        _merchantPreApplicationService = merchantPreApplicationService;
    }

    public async Task<Unit> Handle(DeleteMerchantPreApplicationCommand request, CancellationToken cancellationToken)
    {
        await _merchantPreApplicationService.DeleteAsync(request.Id);

        return Unit.Value;
    }
}